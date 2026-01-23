using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Elevators : MonoBehaviour
{
    [SerializeField] GameObject[] elevators;
    [SerializeField] Vector3 offset;
    // [SerializeField] float startDelay = 1f;
    [SerializeField] bool reset;
    [SerializeField] float resetDelay = 3f;
    GameObject[] recordElevators;
    bool isTriggered = false;

    void Start()
    {
        for (int i = 0; i < elevators.Length; i++)
        {
            GameObject el = elevators[i];
            GameObject record = new GameObject();
            record.transform.position = el.transform.position;
            record.transform.rotation = el.transform.rotation;
            record.transform.localScale = el.transform.localScale;
            recordElevators[i] = record;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);
        if (isTriggered) return;
        if (other.gameObject.name == "Player")
        {
            StartCoroutine(StartAnimation());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isTriggered) return;
        if (other.gameObject.name == "Player")
        {
            StartCoroutine(ResetAnimation());
        }
    }

    IEnumerator StartAnimation()
    {
        isTriggered = true;
        // yield return new WaitForSeconds(startDelay);
        for (int i = 0; i < elevators.Length; i++)
        {
            elevators[i].transform.DOMove(elevators[i].transform.position + offset, 2).SetEase(Ease.InOutSine);
        }
        Debug.Log("Elevator StartAnimation");
        yield return null;
    }
    IEnumerator ResetAnimation()
    {
        if (reset)
        {
            yield return new WaitForSeconds(resetDelay);
            for (int i = 0; i < elevators.Length; i++)
            {
                elevators[i].transform.DOMove(recordElevators[i].transform.position, 2).SetEase(Ease.InOutSine).OnComplete(() => { isTriggered = false; });
            }
        }
    }
}