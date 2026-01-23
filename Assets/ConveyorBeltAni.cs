using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltAni : MonoBehaviour
{
    public float speed = 0.5f;
    public bool isPlaying = true;
    public float offsetY = 0.0f;
    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().materials[1];
    }

    void Update()
    {
        if (isPlaying)
        {
            // 随着时间推移改变贴图偏移
            float offset = Time.time * speed;
            float o = offset % 1.0f;
            mat.SetTextureOffset("_BaseMap", new Vector2(-o, 0));
        }
    }
}
