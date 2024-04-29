using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerMove playerMove;

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerMove = GetComponent<PlayerMove>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            playerMove.DisableMovement();

            // Start the coroutine to respawn after 2 seconds
            StartCoroutine(RespawnAfterDelay());

            // Call PlayerIsDead function from PlayerAnimation script
            playerAnimation.PlayerIsDead();
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Move the player to the specified position (0, 8, 0)
        transform.position = new Vector3(0f, 8f, 0f);

        // Reset player velocity
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.velocity = Vector3.zero;

        // Enable player movement after respawn
        playerMove.EnableMovement();
    }
}
