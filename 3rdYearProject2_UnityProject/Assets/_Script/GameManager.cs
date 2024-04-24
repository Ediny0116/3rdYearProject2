using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public Transform respawnPoint;
    public bool touchWater=false;

    // When "Player" Object touch "Water" Object
    private void OnCollisionEnter(Collision collision)
    {
        // 檢查碰撞是否是Player和Water之間的碰撞
        if (collision.gameObject == player && collision.gameObject.CompareTag("Water") && !touchWater)
        {
            // 開始等待3秒後傳送玩家
            StartCoroutine(RespawnAfterDelay());
        }
    }
    private IEnumerator RespawnAfterDelay()
    {
        touchWater = true;
        // 等待1秒
        yield return new WaitForSeconds(1f);
        // 傳送Player至Respawn位置
        player.transform.position = respawnPoint.position;
        touchWater = false;
    }
}
