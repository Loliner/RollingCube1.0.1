using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorLogic : MonoBehaviour
{
    public Vector2 moveDirection = new Vector2(0, 1); // 对应 (X, Z) 逻辑方向
    public float moveSpeed = 2.0f; // 建议与玩家滚动速度一致
    public bool isActive = true; // 机关开关状态

    public float alignmentThreshold = 0.4f; // 中心点触发距离

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            // 关键：只有当玩家还没被其他传送带“接管”时，才启动新的传送链
            if (player != null && !player.isBeingTransported)
            {
                StartCoroutine(ContinuousTransport(player));
            }
        }
    }

    private (Vector3 pos1, Vector3 pos2, Vector3 pos3) PrepareTransport(Player player)
    {
        // 预处理逻辑（如有需要）
        // 1. 锁定玩家操作
        player.isBeingTransported = true; // 新增状态位，防止重复触发协程
        player.isControlLocked = true;

        // 2. 坐标校准：将玩家平滑拉到传送带中心线上
        Vector3 entryPos = player.transform.position;
        Vector3 centerLinePos = new Vector3(transform.position.x, entryPos.y, transform.position.z);

        // 3. 计算目标格子中心位置
        Vector3 nextGroundPos =
            transform.position + new Vector3(moveDirection.x, 0, moveDirection.y);
        Vector3 targetPos = new Vector3(nextGroundPos.x, entryPos.y, nextGroundPos.z);

        return (centerLinePos, targetPos, nextGroundPos);
    }

    private IEnumerator ContinuousTransport(Player player)
    {
        player.isBeingTransported = true; // 新增状态位，防止重复触发协程
        player.isControlLocked = true;

        GameObject currentConveyor = this.gameObject;

        while (currentConveyor != null)
        {
            ConveyorLogic logic = currentConveyor.GetComponent<ConveyorLogic>();
            if (logic == null || !logic.isActive)
                break;

            // 1. 坐标校准
            Vector3 entryPos = player.transform.position; // 玩家进入传送带时的位置
            Vector3 centerLinePos = new Vector3(
                transform.position.x,
                entryPos.y,
                transform.position.z
            );

            // 2. 计算目标格子中心位置
            Vector3 nextGroundPos =
                transform.position + new Vector3(moveDirection.x, 0, moveDirection.y); // 下一个格子位置
            Vector3 targetPos = new Vector3(nextGroundPos.x, entryPos.y, nextGroundPos.z); // 玩家目标位置

            // 3. 平滑移动到当前目标点
            while (Vector3.Distance(player.transform.position, targetPos) > 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(
                    player.transform.position,
                    targetPos,
                    logic.moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            player.transform.position = targetPos;

            // 4. 寻找下一个接力的传送带
            currentConveyor = GetNextConveyor(nextGroundPos);
        }

        // 4. 传送链结束，释放控制权
        player.isControlLocked = false;
        player.isBeingTransported = false;

        // 5. 最终位置检测
        CheckForFall(player);
    }

    private GameObject GetNextConveyor(Vector3 pos)
    {
        // 这里的检测范围要小，只看目标点中心位置
        Collider[] colliders = Physics.OverlapBox(pos, new Vector3(0.1f, 0.1f, 0.1f));
        foreach (var col in colliders)
        {
            if (col.GetComponent<ConveyorLogic>())
            {
                return col.gameObject;
            }
        }
        return null;
    }

    private void CheckForFall(Player player)
    {
        if (!Physics.Raycast(player.transform.position, Vector3.down, 1f))
        {
            Debug.Log("传送结束，触发掉落流程");
        }
    }
}
