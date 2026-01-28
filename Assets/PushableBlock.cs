using UnityEngine;

public class PushableBlock : MonoBehaviour
{
    private Transform playerTransform;
    private Vector3 lastPlayerPosition;
    private bool isAttached = false;
    private Vector3 pushDirection;

    public void PreparePush(Vector3 dir, Player player)
    {
        if (player.CompareTag("Player") && !isAttached)
        {
            playerTransform = player.transform;
            lastPlayerPosition = playerTransform.position;

            // 计算推动方向（取整后的单位向量，如 1,0,0）
            Vector3 diff = transform.position - playerTransform.position;
            pushDirection = new Vector3(Mathf.Round(diff.x), 0, Mathf.Round(diff.z)).normalized;

            // 预检前方是否有坑
            if (CanBePushed(pushDirection))
            {
                isAttached = true;
            }
        }
    }

    public bool CanBePushed(Vector3 dir)
    {
        // 射线检测前方
        // return !Physics.Raycast(transform.position, dir, 1.5f);
        return true;
    }

    public void PushFinished()
    {
        isAttached = false;
        playerTransform = null;
        ResetPosition();
    }

    private void Update()
    {
        if (isAttached && playerTransform != null)
        {
            // 1. 计算玩家这一帧移动了多少
            Vector3 currentPlayerPos = playerTransform.position;
            Vector3 moveDelta = currentPlayerPos - lastPlayerPosition;

            // 2. 将位移应用到自己身上（只取推动方向上的分量，防止侧滑）
            float projectedDelta = Vector3.Dot(moveDelta, pushDirection);
            if (projectedDelta > 0)
            {
                transform.position += pushDirection * projectedDelta;
            }

            // 3. 更新上一帧位置
            lastPlayerPosition = currentPlayerPos;

            // 4. 检查是否到达目标格中心
            // CheckSnapAndFix();
        }
    }

    private void CheckSnapAndFix()
    {
        // 如果玩家已经完成了 90 度滚动（通常表现为进入了新格子）
        // 或者方块中心点已经接近目标格中心
        // 执行对齐并断开连接 isAttached = false
    }

    void ResetPosition()
    {
        Vector3 pos = gameObject.transform.position;
        float x = Mathf.Round(pos.x * 100 / 25) * 25 / 100;
        float y = Mathf.Round(pos.y * 100 / 25) * 25 / 100;
        float z = Mathf.Round(pos.z * 100 / 25) * 25 / 100;
        Vector3 newPos = new Vector3(x, y, z);
        gameObject.transform.position = newPos;
        // Debug.Log("ResetPosition: " + gameObject.transform.position);
    }
}
