using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Respawn : NetworkBehaviour
{
    public Transform RespawnPoint;

    private void Start()
    {
        RespawnPoint = GameObject.FindGameObjectWithTag("Respawn").transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            Debug.Log("Player entered water");
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Move the player to the specified position (0, 8, 0)
        transform.position = RespawnPoint.position;

        // Reset player velocity
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.velocity = Vector3.zero;
    }
}
