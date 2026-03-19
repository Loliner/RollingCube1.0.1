# RollingCube 1.0.1 — Unity 项目说明

## 项目概述

基于网格的滚方块解谜游戏，使用 Unity 2022.3.16f1 + URP 开发。玩家控制一个 1×1×1 的方块，向四个方向翻滚，通过路径规划、触发机关、利用空间层级（包括主动掉落）来解开谜题。

**设计文档：**
- `Design.md` — 完整机关规格与关卡设计规则
- `LevelDesign.md` — 第 1–6 关教学递进规划

---

## 技术栈

- **Unity：** 2022.3.16f1
- **渲染管线：** URP 14.0.9
- **动画库：** DOTween（位于 `Assets/Plugins/`）
- **文字渲染：** TextMeshPro 3.0.6
- **输入方式：** 直接轮询 `KeyCode`（未使用 Input Manager 或新版 Input System）

---

## 项目结构

```
Assets/
├── Scripts/
│   ├── Player.cs               # 玩家核心控制器
│   ├── Elevator.cs             # 机关 a — 升降平台触发器
│   ├── LinkedElevator.cs       # Elevator 变体，额外控制一个联动对象
│   ├── FragileGround.cs        # 机关 c — 脆弱地板
│   ├── SceneSwitcher.cs        # 自动跳转至下一编号场景
│   ├── Scene2/
│   │   ├── BridgeTrigger.cs    # 桥梁坍塌（销毁铰链关节）
│   │   └── RisingTerrain.cs    # 分段平台依次上升
│   └── Scene4/
│       └── Elevators.cs        # 多升降台同步控制器
├── ConveyorLogic.cs            # 机关 b — 传送带自动运输
├── ConveyorBeltAni.cs          # 传送带贴图滚动动画
├── PushableBlock.cs            # 机关 f — 可推动/会坠落的方块
├── TeleportEffect.cs           # 机关 d — 27 段分裂传送特效
├── Prefabs/
│   ├── Player.prefab / PlayerCube.prefab / PlayerCubeSegs.prefab
│   └── Terrain/                # SoilTerrain、GlassTerrain、ConveyorBeltTerrain 等
├── Scenes/
│   ├── Scene1.unity            # 教学关 — 基础移动
│   ├── Scene2.unity            # 多层空间感知
│   ├── Scene3.unity            # 机关 a 入门
│   ├── Scene4.unity / Scene4x.unity
│   └── Scene5.unity            # 最大关卡
└── Material/ Texture/ Shaders/ Resources/ Settings/
```

---

## 核心脚本说明

### Player.cs — 玩家控制器

挂载在玩家方块 GameObject 上，负责全部移动逻辑。

**移动流程：**
1. `Update()` 轮询 WASD 输入 → 调用 `PrepareRotate(dir)`
2. `PrepareRotate()` 调用 `DetectCollision()` → 路径通畅则开始翻滚
3. `Rotate()` 协程逐帧旋转，速度为 `_rollSpeed * Time.deltaTime`
4. `Reset()` 结束移动，吸附网格，处理可推方块

**关键字段：**
```csharp
float _rollSpeed = 300f        // 旋转速度（度/秒）
bool _isMoving                 // 正在翻滚时为 true
bool isControlLocked           // 传送带/传送门期间禁用输入
bool isBeingTransported        // 由 ConveyorLogic 设置
PushableBlock pushingBlock     // 正在推动方块时非空
```

**网格吸附：** `ResetPosition()` 将 X/Z 坐标取整到最近的 0.25m 倍数。

**碰撞检测：** `DetectCollision(dir)` 在移动方向发射射线，返回碰撞体（墙壁或可推方块）。

**抖动反馈：** 被阻挡时触发 `ShakeRandom()` 协程。

---

### Elevator.cs — 升降平台（机关 a）

触发器区域位于平台上方，玩家踩入时激活。

**可配置字段：**
```csharp
GameObject elevator            // 需要动画的对象
Vector3 offset                 // 移动量（如 (0, 2, 0)）
bool reset                     // 玩家离开后是否归位
float resetDelay               // 归位前的等待秒数
bool switcherFollow            // 触发器是否随平台一起移动
```

**流程：** `OnTriggerEnter` → `StartAnimation()`（DOTween MoveBy，InOutSine，2 秒）→ 可重写的 `OnStartAnimation()` 钩子。离开触发器时执行归位。

**LinkedElevator.cs** 继承自 Elevator，额外同步动画 `linkedGameObject`（偏移量为 `linkedOffset`）。

**Elevators.cs**（Scene4）一次性触发数组中所有升降台，共用同一个 `offset`。

---

### FragileGround.cs — 脆弱地板（机关 c）

挂载在带触发碰撞体的平台上，持有各分段子对象的引用。

**流程：** 玩家进入触发器 → 标记已触发。玩家离开 → `FadeOutWithDelay()` 启动（1 秒延迟）→ 关闭碰撞体、开启各分段刚体物理 → `FadeOutCoroutine()` 在 `fadeDuration = 1.0f` 秒内将 `_BaseColor` alpha 渐变至 0 → 禁用父对象。

**相关预制体：** `GlassTerrain.prefab`（透明玻璃地板）、`GlassTerrainBroken.prefab`（破碎后状态）。

---

### ConveyorLogic.cs — 传送带（机关 b）

每块传送带地砖挂载此脚本，子对象 `forwardPoint` 的 Transform 指向传输方向。

**关键字段：**
```csharp
float moveSpeed = 2.0f
Transform forwardPoint          // 指向传输方向
float alignmentThreshold = 0.4f
bool isActive
```

**传输协程（ContinuousTransport）流程：**
1. 锁定玩家输入（`isControlLocked = true`，`isBeingTransported = true`）
2. 将玩家平滑移向下一个格子中心
3. `GetNextConveyor(targetPos)` 用 `Physics.OverlapBox` 检测出口处是否有相连传送带
4. 有则继续传输；无则释放控制权

`ConveyorBeltAni.cs` 持续滚动 `_BaseMap` 贴图偏移，呈现传送带视觉效果。

---

### PushableBlock.cs — 可推方块（机关 f）

挂载在可推方块上，由 `Player.cs` 调用 `PreparePush()`。

**关键方法：**
```csharp
bool CanBePushed(Vector3 dir)         // 检查推动方向是否畅通
void PreparePush(Vector3 dir, Player) // 将方块附着到玩家移动上
void PushFinished()                   // 脱离玩家；调用 ShouldFall()
bool ShouldFall()                     // 向下射线检测；空洞则开启刚体物理
void ResetPosition()                  // 吸附到 0.25m 网格
```

碰撞体尺寸为 0.95f，避免与地形产生摩擦。

---

### TeleportEffect.cs — 传送特效（机关 d）

挂载在 `PlayerCubeSegs.prefab`（27 段分割方块）上，由传送门逻辑调用。

**关键方法：**
```csharp
void StartTeleport()            // 入口
void PlayFloatingShatter()      // 分解动画：向上漂浮 + 缩小 + 旋转
void PlayRisingReassemble()     // 重组动画：下落 + 放大 + 旋转还原
```

**调节参数：**
```csharp
float floatHeight = 2.0f
float layerDelay = 0.2f         // 各 Y 层之间的错开延迟
float disassemblePieceDuration = 0.8f
float reassemblePieceDuration = 0.8f
```

分段按 Y 坐标分层，`ShuffleList()` 对每层内部的触发时序随机化（Fisher-Yates 洗牌）。

---

### SceneSwitcher.cs — 场景推进

挂载在终点触发区。玩家需在触发器内停留 **2 秒**才会跳转，自动用正则解析当前场景名并加 1（Scene1 → Scene2）。

---

## 网格与坐标系

| 属性 | 数值 |
|------|------|
| 地砖尺寸 | 1×1×1 Unity 单位 |
| 玩家尺寸 | 1×1×1 |
| 网格精度 | 0.25m（坐标取整到 0.25 的倍数） |
| 水平轴 | X（列）和 Z（行） |
| 垂直轴 | Y |
| 相邻可行走层最小高度差 | 2 个单位（容纳玩家高度） |

**掉落机制：** 玩家滚入空洞后向下坠落，检查正下方同 X/Z 的层级是否有承接地面，有则落下继续，无则重置到起点。

---

## 机关实现状态

| 符号 | 机关 | 状态 | 脚本 |
|------|------|------|------|
| `a` | 升降平台 | 已实现 | `Elevator.cs`、`LinkedElevator.cs`、`Elevators.cs` |
| `b` | 传送带 | 已实现 | `ConveyorLogic.cs` |
| `c` | 脆弱地板 | 已实现 | `FragileGround.cs` |
| `d` | 传送门（仅特效） | 部分实现 | `TeleportEffect.cs`（传送门逻辑未接入） |
| `e` | 重力反转 | 暂不考虑 | — |
| `f` | 可推方块 | 已实现 | `PushableBlock.cs` |
| `g` | 计时开关 | 已设计未编码 | — |
| `h` | 序列锁 | 已设计未编码 | — |
| `i/j` | 颜色钥匙/门 | 已设计未编码 | — |

---

## 场景规划

| 场景 | 教学目标 | 关键机关 |
|------|----------|----------|
| Scene1 | 规则存在 — 基础移动 | 无 |
| Scene2 | 空间伏笔 — 多层感知 | BridgeTrigger、RisingTerrain |
| Scene3 | 机关入门 — 机关 a | Elevator |
| Scene4 / 4x | 反直觉路径设计 | Elevators（多台同步） |
| Scene5 | 顺序与时机（最大关卡） | 综合 |
| Scene6（规划中） | 掉落作为解法策略 | — |

---

## 已知问题

1. **Elevators.cs** — `recordElevators` 数组在使用前未初始化，存在空引用风险。
2. **旋转重置 bug** — `ResetRotate()` 对 x/y/z 三轴都错误地使用了 `angles.x`。
3. **传送门未接入** — `TeleportEffect.cs` 特效可用，但尚未与关卡中的传送门触发逻辑集成。
4. **机关 g/h/i/j** — `Design.md` 中已设计，但无对应 C# 实现。

---

## 开发规范

- 所有移动与触发逻辑使用 `OnTriggerEnter` / `OnTriggerExit`，不使用 `OnCollisionEnter`。
- 时序动画统一使用 DOTween，不要引入其他动画库。
- 多脚本协作的状态标志（`isMoving`、`isControlLocked`、`isBeingTransported`）在协程结束时必须复位。
- 每次移动结束后必须调用 `ResetPosition()` 将坐标吸附到 0.25m 网格。
- 场景文件命名必须保持 `Scene1`、`Scene2`... 格式，`SceneSwitcher.cs` 依赖正则解析名称自动跳转。