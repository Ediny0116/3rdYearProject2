using UnityEngine;
using Unity.Netcode;
using UnityEngine.PlayerLoop;

public class PlayerMoveNetworkVer : NetworkBehaviour
{
    public float moveSpeed=3f ;

    public override void OnNetworkSpawn()
    {
        if (IsServer && !IsLocalPlayer && NetworkObject.IsPlayerObject)
        {
            transform.position += new Vector3(-2, 0, 0);
        }
        base.OnNetworkSpawn();
    }
    void Update()
    {
        if(!Application.isFocused)
            return;
        // Get Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // calculate movement
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput) * moveSpeed * Time.deltaTime;

        // Convert movement direction to world space
        movement = transform.TransformDirection(movement);

        // Move Player
        transform.position += movement;
    }
}
