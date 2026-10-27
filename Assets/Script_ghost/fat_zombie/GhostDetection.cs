using UnityEngine;

public class GhostDetection : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 8f;

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Debug.Log("Ghost found Player!");
        }
    }
}