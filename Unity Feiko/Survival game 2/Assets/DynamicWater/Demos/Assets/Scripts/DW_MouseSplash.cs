using UnityEngine;
using LostPolygon.DynamicWaterSystem;

/// <summary>
/// Automates drawing ripples with mouse or touch.
/// </summary>
public class DW_MouseSplash : MonoBehaviour {
    public DynamicWater Water = null;
    public float SplashForce = 10f;
    public float SplashRadius = 0.25f;
    public Camera Camera;

    private Vector3 prevPoint;
    private RaycastHit hitInfo;

    // Updating the splash generation
    private void FixedUpdate() {
        if (Water == null) {
            return;
        }

        if (Camera == null) {
            try {
                Camera = Camera.main ?? GetComponent<Camera>();
            } catch {}

            if (Camera == null) {
                Debug.LogError("No Camera attached and no active Camera was found, please set the Camera property for DW_MouseSplash to work", this);
                #if UNITY_3_5
                gameObject.active = false;
                #else
                gameObject.SetActive(false);
                #endif
                
                return;
            }
        }

        // Creating a ray from camera to world
        Ray ray;
        if (DW_GUILayout.IsRuntimePlatformMobile()) {
            if (Input.touchCount == 0)
                return;

            ray = Camera.ScreenPointToRay(Input.touches[0].position);
        } else {
            ray = Camera.ScreenPointToRay(Input.mousePosition);
        }

        // Checking for collision
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity,
                        1 << LayerMask.NameToLayer(DynamicWater.PlaneColliderLayerName));

        // Creating a splash line between previous position and current
        if (GUIUtility.hotControl == 0 && (Input.GetMouseButton(0) || Input.touchCount > 0)) {
            if (hitInfo.transform != null && Water != null && prevPoint != Vector3.zero) {
                Water.GetWaterLevel(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                Water.CreateSplash(prevPoint, hitInfo.point, SplashRadius, -SplashForce * Time.deltaTime);
            }
        }

        prevPoint = hitInfo.transform != null ? hitInfo.point : Vector3.zero;
    }
}