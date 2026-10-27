using UnityEngine;
using UnityEngine.Rendering.Universal;

// Draws the on-screen blood with its own overlay camera, after post-processing, so the injury colour drain and
// darkening don't turn it grey. Sets itself up at runtime: put it on BloodScreen (under the player's camera).
// Needs a "ScreenBlood" layer in the project.
public class ScreenBloodCamera : MonoBehaviour
{
    public string layerName = "ScreenBlood";

    private Camera baseCam;
    private Camera bloodCam;

    void Awake()
    {
        int layer = LayerMask.NameToLayer(layerName);
        baseCam = GetComponentInParent<Camera>();
        if (layer < 0 || baseCam == null)
        {
            Debug.LogWarning("ScreenBloodCamera: needs a '" + layerName + "' layer and a camera above it.", this);
            enabled = false;
            return;
        }

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;

        GameObject go = new GameObject("ScreenBloodCamera");
        go.transform.SetParent(baseCam.transform, false);
        bloodCam = go.AddComponent<Camera>();
        bloodCam.cullingMask = 1 << layer;
        CopyLens();

        UniversalAdditionalCameraData bloodData = bloodCam.GetUniversalAdditionalCameraData();
        bloodData.renderType = CameraRenderType.Overlay;
        bloodData.renderPostProcessing = false;
        bloodData.renderShadows = false;

        baseCam.cullingMask &= ~(1 << layer);
        baseCam.GetUniversalAdditionalCameraData().cameraStack.Add(bloodCam);
    }

    void LateUpdate()
    {
        if (bloodCam != null)
            CopyLens();
    }

    void CopyLens()
    {
        bloodCam.fieldOfView = baseCam.fieldOfView;
        bloodCam.nearClipPlane = baseCam.nearClipPlane;
        bloodCam.farClipPlane = 5f;
    }

    void OnDestroy()
    {
        if (baseCam != null && bloodCam != null)
            baseCam.GetUniversalAdditionalCameraData().cameraStack.Remove(bloodCam);
    }
}
