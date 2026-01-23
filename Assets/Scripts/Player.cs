using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    private bool _isMoving = false;

    [SerializeField]
    private float _rollSpeed = 300;
    float rotateTotal;
    public bool isControlLocked = false;
    public bool isBeingTransported = false;
    Vector3 anchor;
    Vector3 axis;

    // Update is called once per frame
    void Update()
    {
        if (isControlLocked)
            return;
        if (Input.GetKey(KeyCode.A))
            PrepareRotate(Vector3.left);
        if (Input.GetKey(KeyCode.D))
            PrepareRotate(Vector3.right);
        if (Input.GetKey(KeyCode.W))
            PrepareRotate(Vector3.forward);
        if (Input.GetKey(KeyCode.S))
            PrepareRotate(Vector3.back);

        Rotate();
    }

    void PrepareRotate(Vector3 dir)
    {
        if (this._isMoving)
            return;
        // stop rotate if encounter barrier
        if (DetectCollision(dir))
            return;
        this._isMoving = true;
        GetComponent<Rigidbody>().useGravity = false;
        this.rotateTotal = 0;
        anchor = transform.position + (Vector3.down + dir) * 0.5f;
        axis = Vector3.Cross(Vector3.up, dir);
    }

    bool DetectCollision(Vector3 dir)
    {
        Ray ray = new Ray(transform.position + new Vector3(0, -0.4f, 0), dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.5f))
        {
            Debug.Log("Hit: " + hit.collider.name);
            return true;
        }
        return false;
    }

    void Rotate()
    {
        if (!this._isMoving)
            return;
        float currRotate = this._rollSpeed * Time.deltaTime;
        if (this.rotateTotal + currRotate <= 90)
        {
            transform.RotateAround(anchor, axis, currRotate);
            this.rotateTotal += currRotate;
        }
        else
        {
            currRotate = currRotate - (this.rotateTotal + currRotate - 90);
            transform.RotateAround(anchor, axis, currRotate);
            this.rotateTotal += currRotate;
        }

        if (this.rotateTotal >= 90)
        {
            ResetPosition();
            ResetRotate();
            this._isMoving = false;
            GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void Reset()
    {
        ResetPosition();
        ResetRotate();
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

    void ResetRotate()
    {
        Vector3 angles = gameObject.transform.rotation.eulerAngles;
        float x = Mathf.Round(angles.x / 90) * 90;
        float y = Mathf.Round(angles.x / 90) * 90;
        float z = Mathf.Round(angles.x / 90) * 90;
        Vector3 newAngles = new Vector3(x, y, z);
        gameObject.transform.eulerAngles = newAngles;
        // Debug.Log("ResetRotate: " + gameObject.transform.eulerAngles);
    }
}
