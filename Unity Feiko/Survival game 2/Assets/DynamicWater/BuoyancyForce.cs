using UnityEngine;
using System.Collections.Generic;
using LostPolygon.DynamicWaterSystem;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// When attached to a GameObject, makes it float on water.
    /// </summary>
    [AddComponentMenu("Lost Polygon/Dynamic Water System/Buoyancy Force")]
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyancyForce : MonoBehaviour {
        /// <summary>
        /// Gets or sets the number of subdivisions to approximate 
        /// the object volume with voxels.
        /// </summary>
        /// <remarks>
        /// Value of 1 is usually enough for cube-shaped object.
        /// Value of 2-3 is good for most objects with regular shape.
        /// You may want to set this value high enough if your object has
        /// an irregular shape (i.e. a boat).
        /// </remarks>
        /// <value>Range is 1-10.</value>
        public int Resolution {
            get {
                return _resolution;
            }
            set {
                int clamped = Mathf.Clamp(value, 1, 15);
                if (_resolution != clamped) {
                    _resolution = clamped;

                    if (Application.isPlaying) {
                        RecalculateVoxels();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the object density in kg/m^3.
        /// </summary>
        public float Density {
            get {
                return _density;
            }
            set {
                value = Mathf.Clamp(value, 0.1f, float.PositiveInfinity);

                if (_density != value) {
                    _density = value;

                    if (_calculateMassFromDensity && _volume > 0.01f) {
                        GetComponent<Rigidbody>().mass = _volume * _density;
                    }

                    RecalculateCache();
                }
            }
        }

        /// <summary>
        /// Gets or sets the additional drag 
        /// for when the object is in contact with the fluid.
        /// </summary>
        public float DragInFluid {
            get {
                return _dragInFluid;
            }
            set {
                _dragInFluid = Mathf.Clamp(value, 0f, float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Gets or sets the additional angular drag for when the object is in contact with the fluid.
        /// </summary>
        public float AngularDragInFluid {
            get {
                return _angularDragInFluid;
            }
            set {
                _angularDragInFluid = Mathf.Clamp(value, 0f, float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Gets or sets the force multiplie factor that will be 
        /// attached to the waves produced by the floating object.
        /// </summary>
        /// <remarks>
        /// For an object of relatively small size, do not set this value high,
        /// as the object will bounce on his own waves endlessly.
        /// </remarks>
        public float SplashForceFactor {
            get {
                return _splashForceFactor;
            }
            set {
                _splashForceFactor = Mathf.Clamp(value, 0f, 50f);
            }
        }

        /// <summary>
        /// Gets or sets the absolute maximum value of force applied to the water to create splashes.
        /// </summary>
        public float MaxSplashForce {
            get {
                return _maxSplashForce;
            }
            set {
                _maxSplashForce = Mathf.Clamp(value, 0f, 50f);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether children colliders 
        /// will be included in calculations.
        /// </summary>
        public bool ProcessChildren {
            get {
                return _processChildren;
            }
            set {
                _processChildren = value;

                if (_processChildren != value) {
                    _processChildren = value;

                    if (Application.isPlaying) {
                        RecalculateVoxels();
                    }

                    RecalculateCache();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Density"/> value will be used,
        /// or the density will be approximated from the object volume and mass.
        /// </summary>
        public bool CalculateMassFromDensity {
            get {
                return _calculateMassFromDensity;
            }
            set {
                if (_calculateMassFromDensity != value) {
                    _calculateMassFromDensity = value;
                    RecalculateCache();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating how much the object is submerged.
        /// </summary>
        /// <remarks>
        /// Value of 1 means the object is fully submerged.
        /// Value of 0 means the object if fully outside the fluid volume.
        /// </remarks>
        /// <value>Range is 0-1.</value>
        public float SubmergedVolume {
            get {
                return _subMergedVolume;
            }
        }

        [SerializeField]
        private int _resolution = 2;

        [SerializeField]
        private float _density = 750f;

        [SerializeField]
        private float _dragInFluid = 1f;

        [SerializeField]
        private float _angularDragInFluid = 1f;

        [SerializeField]
        private float _splashForceFactor = 2.5f;

        [SerializeField]
        private float _maxSplashForce = 2f;

        [SerializeField]
        private bool _calculateMassFromDensity = false;

        [SerializeField]
        private bool _processChildren;

        private Transform _transform;
        private Collider[] _colliders;
        private Rigidbody _rigidbody;
        private float _voxelSize;
        private BuoyancyVoxel[] _buoyancyVoxels;
        private Bounds _bounds;
        private Vector3Int _voxelResolution;
        private Vector3 _voxelArchimedesForce;
        private IDynamicWaterFluidVolume _water;
        private float _archimedesForceFactor;
        private float _volume;
        private float _dragNonFluid;
        private float _angularDragNonFluid;
        private float _subMergedVolume;
        private float _subMergedVolumePrev;
        private bool _isReady;

        private struct BuoyancyVoxel {
            public Vector3 Position;
            public bool HadPassedWater;
            public bool IsOnColliderEdge;
        }

        // Cached values
        private RecompiledMarker _recompileMarker;
        private int _voxelsLength;
        private float _maxSplashForceNormalized;
        private float _splashForceFactorNormalized;
        private float _forceNormalizeFactor;
        private float _archimedesForceMagnitude;
        private readonly Vector3 _upDirection = Vector3.up; // Force direction is towards the XZ plane up

        private void Start() {
            _isReady = false;

            _recompileMarker = new RecompiledMarker();

            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody>();

            RecalculateVoxels();
            RecalculateCache();
        }

        private void RecalculateCache() {
            if (_water == null || !_isReady) {
                return;
            }

            // Calculating the actual force applied to a voxel
            _archimedesForceFactor = 1f / _density;

            // Adjust the next line if you are not using Physics.gravity
            Vector3 gravityDirection = Physics.gravity;

            _archimedesForceMagnitude = -gravityDirection.magnitude * _archimedesForceFactor;
            _forceNormalizeFactor = _water.Density * _rigidbody.mass * Time.deltaTime;

            _voxelArchimedesForce = -_upDirection * (_archimedesForceMagnitude * _forceNormalizeFactor / _buoyancyVoxels.Length);

            // Normalizing the values to their actual range
            _splashForceFactorNormalized = _splashForceFactor / 100f;
            _maxSplashForceNormalized = _maxSplashForce / 100f;

            // Caching the array length
            _voxelsLength = _buoyancyVoxels.Length;
        }

        /// <summary>
        /// Recalculates the buoyancy approximation volume.
        /// </summary>
        private void RecalculateVoxels() {
            _isReady = false;

            // Extracting the array of required colliders
            if (_processChildren) {
                _colliders = GetComponentsInChildren<Collider>();
            } else {
                _colliders = GetComponent<Collider>() != null ? new[] {GetComponent<Collider>()} : new Collider[] {};
            }

            if (_colliders.Length == 0) {
                Debug.LogError(
                    _processChildren ? "No colliders are attached to the object and to the children, buoyancy disabled"
                        : "No colliders are attached to the object, buoyancy disabled." +
                          " Enabled \"Process children\" if you have colliders attached to the children",
                    this);

                Destroy(this);
                return;
            }

            // Saving the initial drag values
            _angularDragNonFluid = _rigidbody.angularDrag;
            _dragNonFluid = _rigidbody.drag;

            // Preserving original position and rotation
            Quaternion originalRotation = _transform.rotation;
            Vector3 originalPosition = _transform.position;

            _transform.rotation = Quaternion.identity;
            _transform.position = Vector3.zero;

            // Calculating global collider bounds containing all the children colliders
            _bounds = new Bounds();
            foreach (Collider col in _colliders)
                _bounds.Encapsulate(col.bounds);

            if (_bounds.extents.magnitude < 0.001f) {
                Debug.LogError("Collider bounds are zero-sized, buoyancy disabled", this);

                Destroy(this);
                return;
            }

            // Calculating the size of volume approximation voxel
            _voxelSize = _bounds.size.magnitude / _resolution / 2f;

            _voxelResolution.x = Mathf.RoundToInt(_bounds.size.x / _voxelSize) + 1;
            _voxelResolution.y = Mathf.RoundToInt(_bounds.size.y / _voxelSize) + 1;
            _voxelResolution.z = Mathf.RoundToInt(_bounds.size.z / _voxelSize) + 1;

            _buoyancyVoxels = SliceIntoVoxels().ToArray();
            if (_buoyancyVoxels.Length == 0) {
                Debug.LogWarning("No buoyancy voxels were generated. Try to increase the Resolution parameter", this);
            }

            // Restoring original position and rotation
            _transform.rotation = originalRotation;
            _transform.position = originalPosition;

            _voxelSize = Mathf.Pow(
                _bounds.size.x * _bounds.size.y * _bounds.size.z / (_voxelResolution.x * _voxelResolution.y * _voxelResolution.z),
                1f / 3f
                );

            // Calculating the approximate volume
            _volume = _buoyancyVoxels.Length * _voxelSize * _voxelSize * _voxelSize;

            // Updating the mass, if required
            if (_calculateMassFromDensity) {
                GetComponent<Rigidbody>().mass = _volume * _density;
            }

            _isReady = true;
        }

        /// <summary>
        /// Slices the collider into voxels.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/> of voxels.
        /// </returns>
        private List<BuoyancyVoxel> SliceIntoVoxels() {
            List<BuoyancyVoxel> voxelList = new List<BuoyancyVoxel>(_voxelResolution.x * _voxelResolution.y * _voxelResolution.z);

            for (int ix = 0; ix < _voxelResolution.x; ix++) {
                for (int iy = 0; iy < _voxelResolution.y; iy++) {
                    for (int iz = 0; iz < _voxelResolution.z; iz++) {
                        float x = _bounds.min.x + _bounds.size.x / _voxelResolution.x * (0.5f + ix);
                        float y = _bounds.min.y + _bounds.size.y / _voxelResolution.y * (0.5f + iy);
                        float z = _bounds.min.z + _bounds.size.z / _voxelResolution.z * (0.5f + iz);

                        Vector3 point = new Vector3(x, y, z);
                        for (int k = 0; k < _colliders.Length; k++) {
                            if (ColliderTools.IsPointInsideCollider(_colliders[k], point)) {
                                BuoyancyVoxel buoyancyVoxel;
                                buoyancyVoxel.Position = _transform.InverseTransformPoint(point);
                                buoyancyVoxel.IsOnColliderEdge = ColliderTools.IsPointAtColliderEdge(_colliders[k], point, _voxelSize);
                                buoyancyVoxel.HadPassedWater = true;

                                voxelList.Add(buoyancyVoxel);

                                break;
                            }
                        }
                    }
                }
            }

            return voxelList;
        }

        /// <summary>
        /// Checking if we have entered the fluid volume.
        /// </summary>
        private void OnTriggerEnter(Collider otherCollider) {
            if (!_isReady) {
                return;
            }

            if (otherCollider.CompareTag(FluidVolume.DynamicWaterTagName)) {
                IDynamicWaterFluidVolume water = otherCollider.gameObject.GetComponent<FluidVolume>() ??
                                                 otherCollider.gameObject.GetComponent<DynamicWater>();
                if (water != null) {
                    _water = water;

                    RecalculateCache();
                }
            }
        }

        /// <summary>
        /// Checking if we are staying in the fluid volume.
        /// </summary>
        private void OnTriggerStay(Collider otherCollider) {
            if (!_isReady) {
                return;
            }

            if (_water == null || (otherCollider.CompareTag(FluidVolume.DynamicWaterTagName) && otherCollider != _water.Collider)) {
                IDynamicWaterFluidVolume water = otherCollider.gameObject.GetComponent<FluidVolume>() ??
                                                 otherCollider.gameObject.GetComponent<DynamicWater>();
                if (water != null) {
                    _water = water;

                    RecalculateCache();
                }
            }
        }

        /// <summary>
        /// Checking if we have left the fluid volume.
        /// </summary>
        private void OnTriggerExit(Collider otherCollider) {
            if (!_isReady) {
                return;
            }

            if (_water != null && otherCollider.CompareTag(FluidVolume.DynamicWaterTagName) && otherCollider == _water.Collider) {
                _water = null;
            }
        }

        /// <summary>
        /// Buoyancy calculation step.
        /// </summary>
        private void FixedUpdate() {
            // Happens on assembly reload
            if (_recompileMarker == null) {
                RecalculateVoxels();
                _water = null;
                _recompileMarker = new RecompiledMarker();
            }

            // Failsafe
            if (_water == null || _density < 0.01f || !_isReady) {
                _rigidbody.drag = _dragNonFluid;
                _rigidbody.angularDrag = _angularDragNonFluid;
                return;
            }

            float invDoubleVoxelSize = 1f / (2f * _voxelSize);
            DynamicWater dynamicWater = _water as DynamicWater;
            bool solverCanInteract = dynamicWater != null && dynamicWater.Solver.CanInteract;

            for (int i = 0; i < _voxelsLength; i++) {
                Vector3 wp = _transform.TransformPoint(_buoyancyVoxels[i].Position);
                float waterLevel = _water.GetWaterLevel(wp.x, wp.y, wp.z);

                // No force is applied to the points outside the fluid
                if (waterLevel != float.NegativeInfinity && (wp.y - _voxelSize / 1f < waterLevel)) {
                    Vector3 velocity = _rigidbody.GetPointVelocity(wp);

                    // k == 1 when the voxel is fully submerged
                    // k == 0 when the voxel is fully outside
                    float k = (waterLevel - wp.y) * invDoubleVoxelSize + 0.5f;

                    // Create the splash when the point has passed the water surface.
                    if (_buoyancyVoxels[i].IsOnColliderEdge) {
                        if (!_buoyancyVoxels[i].HadPassedWater && (k < 1f && k > 0f)) {
                            // Scaling and limiting the splash force
                            if (solverCanInteract) {
                                float force = FastFunctions.FastVector3Magnitude(velocity) * _splashForceFactorNormalized;
                                if (force > _maxSplashForceNormalized) {
                                    force = _maxSplashForceNormalized;
                                }

                                if (force > 0.0075f) {
                                    _water.CreateSplash(wp, _voxelSize, force);
                                }
                            }

                            _buoyancyVoxels[i].HadPassedWater = true;
                        } else {
                            _buoyancyVoxels[i].HadPassedWater = false;
                        }
                    }

                    k = (k > 1f) ? 1f : (k < 0f) ? 0f : k;
                    _subMergedVolume += k;

                    // Calculating the actual force for this point depending oh how much
                    // the point is submerged into the fluid
                    Vector3 archimedesForce;
                    archimedesForce.x = k * _voxelArchimedesForce.x;
                    archimedesForce.y = k * _voxelArchimedesForce.y;
                    archimedesForce.z = k * _voxelArchimedesForce.z;

                    // Applying the local force
                    _rigidbody.AddForceAtPosition(archimedesForce, wp, ForceMode.Impulse);
                }
            }

            // Normalizing the submerged volume
            // 0 - object is fully outside the water
            // 1 - object is fully submerged
            _subMergedVolume /= _voxelsLength;

            const float threshold = 0.01f;

            // Sending the message to other components
            if (_subMergedVolumePrev < threshold && _subMergedVolume >= threshold) {
                SendMessage("OnFluidVolumeEnter", _water, SendMessageOptions.DontRequireReceiver);
            } else if (_subMergedVolumePrev >= threshold && _subMergedVolume < threshold) {
                SendMessage("OnFluidVolumeExit", _water, SendMessageOptions.DontRequireReceiver);
            }

            _subMergedVolumePrev = _subMergedVolume;

            // Calculating the drag
            _rigidbody.drag = Mathf.Lerp(_rigidbody.drag, _subMergedVolume > 0.0001f ? _dragNonFluid + _dragInFluid : _dragNonFluid, 8f * Time.deltaTime);
            _rigidbody.angularDrag = Mathf.Lerp(_rigidbody.angularDrag, _subMergedVolume > 0.0001f ? _angularDragNonFluid + _angularDragInFluid : _angularDragNonFluid, 8f * Time.deltaTime);
        }

        /// <summary>
        /// Draws the volume approximation voxels and the center of mass.
        /// </summary>
        private void OnDrawGizmos() {
            Gizmos.DrawIcon(transform.position, "DynamicWater/BuoyancyForce.png");
            if (!Application.isEditor || _buoyancyVoxels == null) {
                return;
            }

            Vector3 gizmoSize = Vector3.one * _voxelSize;
            Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);

            foreach (var p in _buoyancyVoxels) {
                Gizmos.DrawCube(transform.TransformPoint(p.Position), gizmoSize);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, _voxelSize / 2f);
        }
    }
#if !UNITY_3_5
}
#endif