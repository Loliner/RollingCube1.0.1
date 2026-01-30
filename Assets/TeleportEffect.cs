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

    [Header("动画参数")]
    [SerializeField]
    private float duration = 0.5f; // 动画阶段耗时

    [SerializeField]
    private float reassembleDuration = 0.5f;

    [SerializeField]
    private float explosionScale = 2.5f; // 碎块炸开的距离倍数

    [SerializeField]
    private float rotateAmount = 180f; // 碎块旋转的角度

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
        Debug.Log("StartTeleport");
        isTeleporting = true;

        // 1. 隐藏本体
        cubeModel.SetActive(false);

        // 2. 播散开动画
        PlayShatterEffect();

        // 3. 移动逻辑中心并延迟播汇聚动画
        DOVirtual.DelayedCall(
            duration,
            () =>
            {
                // 瞬间移动父物体到目标点
                transform.position = targetPoint.position;

                // 4. 执行汇聚动画
                PlayReassembleEffect();
            }
        );
    }

    private void PlayShatterEffect()
    {
        // 在当前位置生成碎块
        GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);

        foreach (Transform child in debris.transform)
        {
            // 计算炸开方向：基于碎块在方块中的相对位置，使其向外扩散
            // 如果碎块在中心，则给一个随机方向
            Vector3 direction = child.localPosition.normalized;
            if (direction == Vector3.zero)
                direction = Random.insideUnitSphere.normalized;

            Vector3 targetPos = child.localPosition + direction * explosionScale;

            // 动画：位移 + 随机旋转 + 缩放归零
            child.DOLocalMove(targetPos, duration).SetEase(Ease.OutCubic);
            child.DOLocalRotate(
                new Vector3(Random.value, Random.value, Random.value) * rotateAmount,
                duration
            );
            child.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad);
        }

        // 0.6秒后销毁这组碎块（留一点余量确保动画播完）
        Destroy(debris, duration + 0.1f);
    }

    private void PlayReassembleEffect()
    {
        // 在目标点生成另一组碎块
        GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);

        foreach (Transform child in debris.transform)
        {
            // 记录原始的合体位置
            Vector3 finalLocalPos = child.localPosition;

            // 初始状态：拉远、缩放为0、随机角度
            child.localPosition = finalLocalPos * 3f;
            child.localScale = Vector3.zero;
            child.localRotation = Quaternion.Euler(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360)
            );

            // 动画：向原始位置汇聚 + 放大 + 旋转归零
            child.DOLocalMove(finalLocalPos, reassembleDuration).SetEase(Ease.Unset);
            child.DOScale(Vector3.one, reassembleDuration).SetEase(Ease.Unset);
            child.DOLocalRotate(Vector3.zero, reassembleDuration).SetEase(Ease.Unset);
        }

        // 汇聚完成后显示本体
        DOVirtual.DelayedCall(
            reassembleDuration,
            () =>
            {
                cubeModel.transform.position = targetPoint.transform.position;
                cubeModel.SetActive(true);
                Destroy(debris);
                isTeleporting = false;
            }
        );
    }
}
