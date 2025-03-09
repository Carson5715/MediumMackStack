using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DelayedSceneLoader : MonoBehaviour
{
    // Public variable to set the delay (in seconds) before the scene loads.
    public float delayTime = 5f;
    
    // Public variable to specify the name of the scene to load.
    public string sceneToLoad = "NextScene";

    void Start()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(sceneToLoad);
    }
}