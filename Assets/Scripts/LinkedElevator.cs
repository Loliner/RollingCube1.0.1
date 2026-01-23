using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LinkedElevator : Elevator
{
    [SerializeField] GameObject linkedGameObject;
    [SerializeField] Vector3 linkedOffset;
    public override void OnStartAnimation()
    {
        Debug.Log("LinkedElevator OnStartAnimation");
        linkedGameObject.transform.DOMove(linkedGameObject.transform.position + linkedOffset, 2).SetEase(Ease.InOutSine);
    }
}
