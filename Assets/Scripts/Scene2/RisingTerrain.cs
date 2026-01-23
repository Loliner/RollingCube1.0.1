using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RisingTerrain : MonoBehaviour
{
    [SerializeField] GameObject terrain1, terrain2, terrain3;
    bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        StartCoroutine(ExampleCoroutine());
    }

    IEnumerator ExampleCoroutine()
    {
        terrain1.transform.DOMove(terrain1.transform.position + new Vector3(0, 1.5f, 0), 2).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.5f);

        terrain2.transform.DOMove(terrain2.transform.position + new Vector3(0, 1.5f, 0), 2).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.5f);

        terrain3.transform.DOMove(terrain3.transform.position + new Vector3(0, 1.5f, 0), 2).SetEase(Ease.InOutSine);
    }
}
