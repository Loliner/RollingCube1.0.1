using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    GameObject elevator;

    [SerializeField]
    Vector3 offset;

    // [SerializeField] float startDelay = 1f;
    [SerializeField]
    bool reset;

    [SerializeField]
    float resetDelay = 3f;

    [SerializeField]
    bool switcherFollow;
    GameObject recordElevator;
    GameObject recordSwitcher;
    bool isTriggered = false;

    void Start()
    {
        recordElevator = new GameObject();
        recordSwitcher = new GameObject();
        RecordTransform(gameObject, recordSwitcher);
        RecordTransform(elevator, recordElevator);
    }

    void RecordTransform(GameObject source, GameObject target)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
        target.transform.localScale = source.transform.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);
        if (isTriggered)
            return;
        if (other.gameObject.name == "Player")
        {
            StartCoroutine(StartAnimation());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isTriggered)
            return;
        if (other.gameObject.name == "Player")
        {
            StartCoroutine(ResetAnimation());
        }
    }

    IEnumerator StartAnimation()
    {
        isTriggered = true;
        // yield return new WaitForSeconds(startDelay);
        this.elevator.transform.DOMove(this.recordElevator.transform.position + offset, 2)
            .SetEase(Ease.InOutSine);
        if (switcherFollow)
        {
            this.gameObject.transform.DOMove(this.recordSwitcher.transform.position + offset, 2)
                .SetEase(Ease.InOutSine);
        }
        Debug.Log("Elevator StartAnimation");
        OnStartAnimation();
        yield return null;
    }

    public virtual void OnStartAnimation()
    {
        Debug.Log("Elevator OnStartAnimation");
    }

    IEnumerator ResetAnimation()
    {
        if (reset)
        {
            yield return new WaitForSeconds(resetDelay);
            elevator
                .transform.DOMove(recordElevator.transform.position, 2)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    isTriggered = false;
                });
            if (switcherFollow)
            {
                gameObject
                    .transform.DOMove(recordSwitcher.transform.position, 2)
                    .SetEase(Ease.InOutSine);
            }
            OnResetAnimation();
        }
    }

    public virtual void OnResetAnimation() { }
}
