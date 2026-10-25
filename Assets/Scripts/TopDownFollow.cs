using UnityEngine;

public class TopDownFollow : MonoBehaviour
{
    public Transform target;
    public float height = 10f;
    void LateUpdate()
    {
        Vector3 newPosition = target.position;
        newPosition.y = height; // ตรึงความสูงกล้องไว้คงที่เสมอ
        transform.position = newPosition;
    }
}