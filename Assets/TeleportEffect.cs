using System.Collections.Generic;
using System.Linq;
using DG.Tweening; // 必须导入 DOTween: https://dotween.demigiant.com/
using UnityEngine;

public class CubeTeleporter : MonoBehaviour
{
    [Header("对象引用")]
    [SerializeField]
    private GameObject cubeModel; // 正常状态的方块模型

    [SerializeField]
    private GameObject debrisPrefab; // 27个碎块组成的预制体

    [SerializeField]
    private Transform targetPoint; // 传送目标点

    [Header("上浮动画参数")]
    [SerializeField]
    private float floatHeight = 2.0f; // 上浮的高度

    [SerializeField]
    private float layerDelay = 0.2f; // 每一层之间的启动延迟

    [SerializeField]
    private float pieceRandomDelay = 0.15f; // 同一层内碎片的随机延迟范围

    [SerializeField]
    private float disassemblePieceDuration = 0.8f; // 散开时每个碎片时长

    [SerializeField]
    private float reassemblePieceDuration = 0.8f; // 组装时每个碎片时长

    private bool isTeleporting = false;

    [SerializeField]
    private bool isTeleported = true;

    void Update()
    {
        if (isTeleported == false)
        {
            isTeleported = true;
            StartTeleport();
        }
    }

    /// <summary>
    /// 调用此方法开始传送
    /// </summary>
    public void StartTeleport()
    {
        if (isTeleporting || targetPoint == null)
            return;
        isTeleporting = true;
        cubeModel.SetActive(false);

        PlayFloatingShatter();

        // 计算总等待时间：(2层延迟) + 单个动画时长
        float totalWaitTime = (layerDelay * 2) + disassemblePieceDuration;

        DOVirtual.DelayedCall(
            totalWaitTime * 0.8f,
            () =>
            {
                transform.position = targetPoint.position;
                PlayRisingReassemble();
            }
        );
    }

    private void PlayFloatingShatter()
    {
        // 1. 生成碎块预制体
        GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);

        // 2. 将所有子碎块提取到 List 中
        List<Transform> pieces = new List<Transform>();
        foreach (Transform child in debris.transform)
        {
            pieces.Add(child);
        }

        // 3. 核心分层逻辑：按世界 Y 坐标分组 (精度 0.1)
        // OrderByDescending 确保 Y 值最大的（最顶层）排在 List 前面，先开始动画
        var layers = pieces
            .GroupBy(p => Mathf.Round(p.position.y * 10f) / 10f)
            .OrderByDescending(g => g.Key)
            .ToList();

        Debug.Log($"[Teleport] 成功识别到 {layers.Count} 层碎块。");

        // 4. 遍历每一层执行动画
        for (int i = 0; i < layers.Count; i++)
        {
            // 每一层的基础延迟时间
            float baseLayerDelay = i * layerDelay;

            // 1. 将当前层的碎片转为列表并随机打乱顺序
            // 这样可以保证空间位置上的碎片起飞顺序是随机的
            List<Transform> currentLayerPieces = layers[i].ToList();
            ShuffleList(currentLayerPieces);

            int pieceCount = currentLayerPieces.Count;
            // 计算每个碎片的平均时间间隔
            float interval = pieceRandomDelay / pieceCount;

            for (int j = 0; j < pieceCount; j++)
            {
                Transform child = currentLayerPieces[j];

                // 2. 核心算法：均匀分布 + 微小抖动 (Jitter)
                // j * interval 保证了它们在时间上是排队开来的
                // Random.Range 则在小范围内让这种排队不那么死板
                float jitter = Random.Range(0f, interval * 0.5f);
                float finalDelay = baseLayerDelay + (j * interval) + jitter;

                // --- A. 垂直上浮 ---
                // 使用 DOMove (世界坐标) 配合 Vector3.up 确保绝对垂直向上
                Vector3 targetWorldPos = child.position + Vector3.up * floatHeight;
                child
                    .DOMove(targetWorldPos, disassemblePieceDuration)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutBack);

                // --- B. 随机倾斜旋转 ---
                // 产生前后或左右的轻微倾斜，而不是大幅度自旋
                // X 和 Z 轴控制倾斜方向，Y 轴给极少量的偏移
                Vector3 tiltRotation = new Vector3(
                    Random.Range(-35f, 35f), // 前后倾斜角度
                    Random.Range(-10f, 10f), // 极轻微自旋
                    Random.Range(-35f, 35f) // 左右倾斜角度
                );

                child
                    .DORotate(tiltRotation, disassemblePieceDuration, RotateMode.LocalAxisAdd)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutSine);

                // --- C. 逐步缩小 ---
                child
                    .DOScale(Vector3.zero, disassemblePieceDuration)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutSine);
            }
        }

        // 5. 自动清理：在所有层动画结束后销毁碎片对象
        float maxAnimTime = (layers.Count * layerDelay) + disassemblePieceDuration;
        Destroy(debris, maxAnimTime + 1f);
    }

    // 一个简单的洗牌算法 (Fisher-Yates Shuffle)
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    private void PlayRisingReassemble()
    {
        // 1. 在目标位置生成碎块预制体
        GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);

        // 2. 将所有子碎块提取到 List 中
        List<Transform> pieces = new List<Transform>();
        foreach (Transform child in debris.transform)
        {
            pieces.Add(child);
        }

        // 3. 核心分层逻辑：按世界 Y 坐标分组
        // 使用 OrderBy(g => g.Key) 确保 Y 值最小的（最底层）排在前面，先开始动画
        var layers = pieces
            .GroupBy(p => Mathf.Round(p.position.y * 10f) / 10f)
            .OrderBy(g => g.Key)
            .ToList();

        Debug.Log($"[Teleport] 汇聚动画：成功识别到 {layers.Count} 层碎块。");

        // 4. 遍历每一层执行“反向”动画
        for (int i = 0; i < layers.Count; i++)
        {
            // 每一层的基础延迟时间
            float baseLayerDelay = i * layerDelay;

            foreach (Transform child in layers[i])
            {
                // 记录该碎块最终应该达到的完美位置和旋转
                Vector3 finalWorldPos = child.position;
                Quaternion finalRotation = child.rotation;

                // --- A. 设置初始状态 (动画开始前) ---
                // 碎片从上方 floatHeight 处“降落”回来
                child.position = finalWorldPos + Vector3.up * floatHeight;
                // 初始大小为 0
                child.localScale = Vector3.zero;
                // 初始随机倾斜角度
                child.localRotation = Quaternion.Euler(
                    Random.Range(-35f, 35f),
                    Random.Range(-10f, 10f),
                    Random.Range(-35f, 35f)
                );

                // 同一层内的碎片随机错开
                float finalDelay = baseLayerDelay + Random.Range(0f, pieceRandomDelay);

                // --- B. 执行汇聚动画 ---

                // 1. 垂直降落回原位
                child
                    .DOMove(finalWorldPos, reassemblePieceDuration)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutBack); // 使用 OutBack 产生轻微的回弹感，增加弹性

                // 2. 旋转归零 (回到完美方块的角度)
                child
                    .DORotateQuaternion(finalRotation, reassemblePieceDuration)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutSine);

                // 3. 逐步放大
                child
                    .DOScale(Vector3.one, reassemblePieceDuration)
                    .SetDelay(finalDelay)
                    .SetEase(Ease.OutSine);
            }
        }

        // 5. 动画结束后恢复主体，并清理碎块
        float maxAnimTime = (layers.Count * layerDelay) + reassemblePieceDuration + 1f;
        DOVirtual.DelayedCall(
            maxAnimTime,
            () =>
            {
                cubeModel.transform.position = targetPoint.transform.position;
                cubeModel.SetActive(true);
                Destroy(debris);
                isTeleporting = false; // 重置状态锁
            }
        );
    }
}
