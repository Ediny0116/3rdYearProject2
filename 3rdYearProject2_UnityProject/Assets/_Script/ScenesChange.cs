using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesChange : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine to switch scenes after 3 seconds
        StartCoroutine(SwitchSceneAfterDelay(3f));
    }

    private IEnumerator SwitchSceneAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Load the SampleScene
        SceneManager.LoadScene("SampleScene");
    }
}
