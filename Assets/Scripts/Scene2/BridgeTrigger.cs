using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeTrigger : MonoBehaviour
{
    [SerializeField]
    GameObject firstHinge;
    [SerializeField]
    GameObject secondHinge;
    bool isTriggered;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "Player" && !this.isTriggered)
        {
            isTriggered = true;
            StartCoroutine(ExampleCoroutine());
        }
    }

    IEnumerator ExampleCoroutine()
    {
        Debug.Log("remove first hinge joint");
        HingeJoint[] firstJoints = firstHinge.GetComponents<HingeJoint>();
        Destroy(firstJoints[1]);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);

        //After we have waited 5 seconds print the time again.
        Debug.Log("remove second hinge joint");
        HingeJoint[] secondJoints = secondHinge.GetComponents<HingeJoint>();
        Destroy(secondJoints[0]);
    }
}
