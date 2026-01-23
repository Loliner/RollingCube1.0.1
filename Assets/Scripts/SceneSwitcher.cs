using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    bool isTriggered = false;
    long triggerTime;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "Player")
        {
            isTriggered = true;
            triggerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            long currTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (isTriggered && currTime - this.triggerTime > 2000)
            {
                isTriggered = false;
                string currentSceneName = SceneManager.GetActiveScene().name;
                Match match = Regex.Match(currentSceneName, @"Scene(\d+)");
                if (match.Success)
                {
                    int number = int.Parse(match.Groups[1].Value);
                    number++;
                    string newScene = "Scene" + number;
                    SceneManager.LoadScene(newScene);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            isTriggered = false;
        }

    }
}
