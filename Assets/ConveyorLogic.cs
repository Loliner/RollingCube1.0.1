using System.Collections;
using UnityEngine;

public class ConveyorLogic : MonoBehaviour
{
    // public Vector2 moveDirection = new Vector2(0, 1); // 对应 (X, Z) 逻辑方向
    public float moveSpeed = 2.0f; // 建议与玩家滚动速度一致
    public bool isActive = true; // 机关开关状态
    public Transform forwardPoint; // 前方检测点

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
                Debug.Log("玩家进入传送带: " + gameObject.name);
                StartCoroutine(ContinuousTransport(player));
            }
        }
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

            Debug.Log("当前传送带: " + currentConveyor.name);

            // 1. 坐标校准
            Vector3 entryPos = player.transform.position; // 玩家进入传送带时的位置
            Vector3 centerLinePos = new Vector3(
                currentConveyor.transform.position.x,
                entryPos.y,
                currentConveyor.transform.position.z
            );

            // 2. 计算目标格子中心位置
            // Vector3 forwardMoveDir = currentConveyor.transform.forward;
            Vector3 forwardMoveDir = forwardPoint.position - currentConveyor.transform.position;
            Debug.Log("1: " + forwardPoint.position + " 2: " + currentConveyor.transform.position);
            forwardMoveDir.y = 0;
            forwardMoveDir.Normalize();
            // Vector2 currentDir = logic.moveDirection;
            Vector2 currentDir = forwardMoveDir;

            Debug.Log("传送方向: " + currentDir + ", 前方: " + forwardMoveDir);
            Vector3 targetPos =
                currentConveyor.transform.position
                + new Vector3(currentDir.x, entryPos.y, currentDir.y); // 玩家目标位置
            Vector3 nextGroundPos =
                currentConveyor.transform.position + new Vector3(currentDir.x, 0, currentDir.y); // 下一个格子位置

            Debug.Log("目标位置: " + targetPos);

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
