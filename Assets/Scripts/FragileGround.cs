using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class FragileGround : MonoBehaviour
{
    bool isTriggered = false;
    float fadeDuration = 1.0f;

    [SerializeField]
    Transform[] firstDropSegments;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);
        if (isTriggered)
        {
            return;
        }
        if (other.gameObject.name == "Player")
        {
            isTriggered = true;

            foreach (Transform segment in firstDropSegments)
            {
                Rigidbody rb = segment.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                // MeshCollider（只作用于子物体）
                MeshCollider mc = segment.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    mc.enabled = true;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isTriggered)
            return;
        if (other.gameObject.name != "Player")
            return;

        BoxCollider[] boxColliders = GetComponents<BoxCollider>();
        foreach (var box in boxColliders)
        {
            Debug.Log('2');
            box.enabled = false;
        }

        foreach (Transform child in transform)
        {
            Debug.Log(child.name);
            // Rigidbody（只作用于子物体）
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log('4');
                rb.isKinematic = false;
                // rb.velocity = Vector3.zero;
                // rb.angularVelocity = Vector3.zero;
            }

            // MeshCollider（只作用于子物体）
            MeshCollider mc = child.GetComponent<MeshCollider>();
            if (mc != null)
            {
                Debug.Log('5');
                mc.enabled = true;
            }
        }

        StartCoroutine(FadeOutWithDelay());
    }

    IEnumerator FadeOutWithDelay()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        // 只获取子节点，不包含自己
        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        float time = 0f;

        // 记录每个 Renderer 的材质实例
        Material[][] materials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject == gameObject)
                continue;

            materials[i] = renderers[i].materials; // ⚠️ 自动实例化
        }

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);

            foreach (var mats in materials)
            {
                if (mats == null)
                    continue;

                foreach (var mat in mats)
                {
                    if (mat == null)
                        continue;

                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.GetColor("_BaseColor");
                        c.a = alpha;
                        mat.SetColor("_BaseColor", c);
                    }
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 最终强制设为 0
        foreach (var mats in materials)
        {
            if (mats == null)
                continue;

            foreach (var mat in mats)
            {
                if (mat == null)
                    continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = 0f;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }

        // 可选：彻底关闭
        gameObject.SetActive(false);
    }
}
