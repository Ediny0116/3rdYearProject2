using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Renderer renderer;
    private Material mat;
    private PlayerMove playerMove;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        mat = renderer.material;
    }
    public void PlayerIsDead()
    {
        playerMove.EnableMovement();
        StartCoroutine(FadeInAndOut());
    }

    // Coroutine to fade the player in and out with alternating alpha values
    private IEnumerator FadeInAndOut()
    {
        float duration = 0.5f; // Total duration for each alpha value change
        float timeElapsed = 0f; // Time elapsed since starting the coroutine

        while (timeElapsed < 3f) // Repeat for 3 seconds
        {
            // Change alpha value to 0.5 (half-transparent)
            Color color = mat.color;
            color.a = 0.5f;
            mat.color = color;
            yield return new WaitForSeconds(duration); // Wait for half of the duration

            // Change alpha value to 1 (fully opaque)
            color.a = 1f;
            mat.color = color;
            yield return new WaitForSeconds(duration); // Wait for the other half of the duration

            timeElapsed += duration * 2f; // Increment the time elapsed by the duration of one complete cycle
        }
        // Ensure the final alpha value is set to 1
        Color finalColor = mat.color;
        finalColor.a = 1f;
        mat.color = finalColor;
    }
}
