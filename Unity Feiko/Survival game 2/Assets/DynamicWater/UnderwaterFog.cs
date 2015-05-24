using UnityEngine;
using LostPolygon.DynamicWaterSystem;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// A simple underwater fog effect. Attach this component to the Camera.
    /// </summary>
    [RequireComponent(typeof(WaterDetector))]
    public class UnderwaterFog : MonoBehaviour {
        // Settings that will be used when underwater
        public FogMode FogMode = FogMode.Linear;
        public Color FogColor = new Color32(112, 183, 255, 255);
        public float FogDensity = 0.1f;
        public float FogStartDistance = 1f;
        public float FogEndDistance = 15f;
        public Material Skybox = null;

        // Original settings
        private bool _defaultFog;
        private FogMode _defaultFogMode;
        private Color _defaultFogColor;
        private float _defaultFogDensity;
        private float _defaultFogStartDistance;
        private float _defaultFogEndDistance;
        private Material _defaultSkybox;

        private WaterDetector _waterDetector;
        private Camera _camera;

        private void Start() {
            // Check if the script is attached to a Camera
            _camera = gameObject.GetComponent<Camera>();
            if (_camera == null) {
                Debug.LogError(string.Format("The {0} script can only be attached to the Camera!", GetType()), transform);
                Destroy(this);
                return;
            }

            // Adding Rigidbody and Collider required for receiving trigger events
            Rigidbody tempRigidbody = gameObject.GetComponent<Rigidbody>();
            if (tempRigidbody == null) {
                tempRigidbody = gameObject.AddComponent<Rigidbody>();
                tempRigidbody.isKinematic = true;
                tempRigidbody.useGravity = false;
            } else {
                Debug.LogWarning("Rigidbody component is already attached to this camera, unexpected behaviour may occur",
                                 transform);
            }

            Collider tempCollider = gameObject.GetComponent<Collider>();
            if (tempCollider == null) {
                tempCollider = gameObject.AddComponent<BoxCollider>();
                tempCollider.isTrigger = true;
                ((BoxCollider) tempCollider).size = Vector3.zero;
            } else {
                Debug.LogWarning("Collider already attached to this camera, unexpected behaviour may occur", transform);
            }

            _waterDetector = GetComponent<WaterDetector>() ?? gameObject.AddComponent<WaterDetector>();

            // Reading the initial fog state
            _defaultFog = RenderSettings.fog;
            _defaultFogMode = RenderSettings.fogMode;
            _defaultFogColor = RenderSettings.fogColor;
            _defaultFogDensity = RenderSettings.fogDensity;
            _defaultFogStartDistance = RenderSettings.fogStartDistance;
            _defaultFogEndDistance = RenderSettings.fogEndDistance;
            _defaultSkybox = RenderSettings.skybox;
        }

        private void Update() {
            // If we are in the water
            if (_waterDetector != null && _waterDetector.Water != null) {
                // Retrieving the water level
                float waterLevel = _waterDetector.GetWaterLevel(transform.position.x, transform.position.y, transform.position.z);

                // Switch the fog if the camera is under the water
                bool fogState = _waterDetector.Water.Collider.bounds.Contains(_camera.transform.position) &&
                                _camera.transform.position.y < waterLevel;
                SetFog(fogState);
            } else {
                SetFog(false);
            }
        }

        private void SetFog(bool enableFog) {
            if (enableFog) {
                // Setting the desired fog settings
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode;
                RenderSettings.fogColor = FogColor;
                RenderSettings.fogDensity = FogDensity;
                RenderSettings.fogStartDistance = FogStartDistance;
                RenderSettings.fogEndDistance = FogEndDistance;
                if (Skybox != null) {
                    RenderSettings.skybox = Skybox;
                }
            } else {
                // Restoring the settings we saved at start
                RenderSettings.fog = _defaultFog;
                RenderSettings.fogMode = _defaultFogMode;
                RenderSettings.fogColor = _defaultFogColor;
                RenderSettings.fogDensity = _defaultFogDensity;
                RenderSettings.fogStartDistance = _defaultFogStartDistance;
                RenderSettings.fogEndDistance = _defaultFogEndDistance;
                RenderSettings.skybox = _defaultSkybox;
            }
        }
    }
#if !UNITY_3_5
}
#endif