using UnityEngine;

public class CameraModeController : MonoBehaviour
{
    public enum CameraMode { ThirdPerson, TopDown, FirstPerson }
    public CameraMode currentMode = CameraMode.ThirdPerson;
    [Header("References")]
    public Transform player;
    public Transform cameraPivot;
    public MouseOrbitCamera orbitScript;
    [Header("Third-Person Settings")]
    public Vector3 thirdPersonLocalOffset = new Vector3(0, 1f, -5f);
    [Header("First-Person Settings")]
    public Vector3 firstPersonLocalOffset = new Vector3(0, 1.6f, 0f);
    [Header("Top-Down Settings")]
    public float topDownHeight = 15f;
    public float topDownOrthoSize = 10f;
    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
        SetMode(currentMode);
    }
void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentMode = (CameraMode)(((int)currentMode + 1) % 3);
            SetMode(currentMode);
        }
        if (currentMode == CameraMode.TopDown)
        {
            Vector3 pos = player.position;
            pos.y = topDownHeight;
            transform.position = pos;
        }
    }
    void SetMode(CameraMode mode)
    {
        switch (mode)
        {
            case CameraMode.ThirdPerson:
                transform.SetParent(cameraPivot);
                transform.localPosition = thirdPersonLocalOffset;
                transform.localRotation = Quaternion.identity;
                cam.orthographic = false;
                orbitScript.enabled = true;
                break;
            case CameraMode.FirstPerson:
                transform.SetParent(cameraPivot);
                transform.localPosition = firstPersonLocalOffset;
                transform.localRotation = Quaternion.identity;
                cam.orthographic = false;
                orbitScript.enabled = true;
                break;
            case CameraMode.TopDown:
                transform.SetParent(null);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                cam.orthographic = true;
                cam.orthographicSize = topDownOrthoSize;
                orbitScript.enabled = false;
        break;
        }
    }
}