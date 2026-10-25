using UnityEngine;

public class NavClickToMove : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent agent;
    public LayerMask groundMask;
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    void Update()
    {
        if (Input.GetMouseButton(0)) // คลิกซ้าย หรือคลิกซ้ายค้าง
        {
            
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                agent.SetDestination(hit.point);
            }
        }
    }
    // ใช้เช็คว่ากาลังเดินอยู่ไหม สาหรับส่งให้ Animator
    public bool IsMoving => agent.velocity.magnitude > 0.1f;
}