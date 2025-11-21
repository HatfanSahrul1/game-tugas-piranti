using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToScene : MonoBehaviour
{
    public void MoveToActiveScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
