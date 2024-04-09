using UnityEngine;
using Unity.Netcode;

public class PlayerMoveNetworkVer : NetworkBehaviour
{
    public float moveSpeed=3f ;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) Destroy(this);
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
