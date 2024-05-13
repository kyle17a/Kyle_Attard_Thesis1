using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByIndex()
    {
        SceneManager.LoadScene(1);
    }
    public void LoadPuzzleScene()
    {
        SceneManager.LoadScene(3);
    }
    public void LoadWelcomeScene()
    {
        SceneManager.LoadScene(0);
    }
}
