using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            StartCoroutine(RespawnAfterDelay());
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
    }
}
