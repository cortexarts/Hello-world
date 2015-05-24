#define ARBITRARY_ROTATIONS

using System;
using System.Collections.Generic;
using LostPolygon.DynamicWaterSystem;
using UnityEngine;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// Main dynamic water class.
    /// </summary>
    /// <remarks>
    /// Most interaction is done with this class. 
    /// It differs from <see cref="FluidVolume"/> by having the wave simulation 
    /// and by having the visual representation.
    /// </remarks>
    [AddComponentMenu("Lost Polygon/Dynamic Water System/Dynamic Water")]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    public class DynamicWater : FluidVolume, IDynamicWaterSettings, IDynamicWaterMeshSettings {
        /// <summary>
        /// The tag and layer name used by clickable plane collider.
        /// </summary>
        public const string PlaneColliderLayerName = "DynamicWaterPlaneCollider";

        /// <summary>
        /// The tag name used to define obstruction geometry.
        /// </summary>
        public const string DynamicWaterObstructionTag = "DynamicWaterObstruction";

        /// <summary>
        /// The tag name used to define inverted obstruction geometry.
        /// </summary>
        public const string DynamicWaterObstructionInvertedTag = "DynamicWaterObstructionInverted";

        /// <summary>
        /// Value indicating whether to generate the static plane collider on initialization.
        /// </summary>
        public bool UsePlaneCollider = true;

        /// <summary>
        /// Value indicating whether to update the simulation when the mesh is not visible.
        /// </summary>
        public bool UpdateWhenNotVisible = false;

        /// <summary>
        /// Gets or sets the simulation grid resolution.
        /// </summary>
        /// <remarks>
        /// This value is normalized, so that it doesn't depends on the size of water plane.
        /// To get the actual grid size, see <see cref="GridSize"/>.
        /// </remarks>
        /// <value>This value to the range 4-256</value>
        public int Quality {
            get {
                return _quality;
            }
            set {
                if (_quality == value) {
                    return;
                }

                _quality = Mathf.Clamp(value, 4, 256);
                _resolution = Mathf.Clamp(Mathf.RoundToInt(_quality * MaxResolution() / 256f), 4, MaxResolution());

                // Making sure resolution is even
                if (_resolution % 2 == 1) {
                    _resolution++;
                }

                PropertyChanged();
            }
        }

        /// <summary>
        /// Gets the actual simulation grid resolution.
        /// </summary>
        public Vector2Int GridSize {
            get {
                return _grid;
            }
        }

        /// <summary>
        /// Gets the size of single simulation grid node in world space.
        /// </summary>
        public float NodeSize {
            get {
                return _nodeSize;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the water mesh normals should be calculated.
        /// </summary>
        public bool CalculateNormals {
            get {
                return _calculateNormals;
            }
            set {
                _calculateNormals = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the fast approximate method of calculating the
        /// water mesh normals should be used instead of <c>Mesh.RecalculateNormals()</c>.
        /// </summary>
        /// <remarks>
        /// Enabling this could provide a huge performance boost with the cost of a bit degraded quality.
        /// Especially useful on mobile devices.
        /// </remarks>
        public bool UseFastNormals {
            get {
                return _useFastNormals;
            }
            set {
                _useFastNormals = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tangents must be set (usually for bump-mapped shaders).
        /// </summary>
        /// <remarks>
        /// Enabling this may sometimes result in performance drop on high Quality levels. It is better to
        /// turn it off if your shader doesn't uses normals.
        /// </remarks>
        public bool SetTangents {
            get {
                return _setTangents;
            }
            set {
                _setTangents = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to calculate where the simulation field is obstructed with
        /// GameObjects with tag <c>DynamicWaterObstruction</c>
        /// </summary>
        /// <remarks>
        /// As the simulation field can only be of rectangular shape for now, 
        /// this can be used to simulate complex shapes such as pond banks.
        /// </remarks>
        public bool UseObstructions {
            get {
                return _useObstructions;
            }
            set {
                _useObstructions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the obstruction field data will be baked into
        /// water mesh vertex colors <c>DynamicWaterObstruction</c>
        /// </summary>
        /// <remarks>
        /// In the vertex color, red channel corresponds to the additinal dampening in that point,
        /// where 255 means zero dampening and 1 means maximum dampening.
        /// 0 is a special value that corresponds to the situation when the vertex 
        /// is fully obstructed by obstruction geometry. In this case, a value of 255
        /// is additionaly written to the blue channel.
        /// You can use this data for more advanced shading, for example, discard fragments that are
        /// fully obstructed.
        /// </remarks>
        public bool MeshBakeObstructionData {
            get {
                return _meshBakeObstructionData;
            }
            set {
                _meshBakeObstructionData = value;
                if (_meshBakeObstructionData == value) {
                    return;
                }

                _meshBakeObstructionData = value;
                PropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the obstruction field data will eroded (expanded or shrinked).
        /// </summary>
        /// <remarks>
        /// This can be used for dealing with edge artifacts that can occur when using obstruction data.
        /// </remarks>
        public int ObstructionDataErosion {
            get {
                return _obstructionDataErosion;
            }
            set {
                if (_obstructionDataErosion == value) {
                    return;
                }

                _obstructionDataErosion = value;
                PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the wave propagation speed.
        /// </summary>
        public float Speed {
            get {
                return _speed;
            }
            set {
                _speed = Mathf.Clamp(value, 0f, MaxSpeed());
            }
        }

        /// <summary>
        /// Gets or sets the wave damping value.
        /// </summary>
        /// <value>
        /// The value is clamped to the 0-1 range. The higher the value, the higher the damping. 
        /// Value of 0 corresponds to absence of any damping, which could lead to simulation instability
        /// when too much force was applied to the system.
        /// </value>
        /// <example>
        /// Optimal value for water is around 0.03.
        /// </example>
        public float Damping {
            get {
                return 1f - _damping;
            }
            set {
                _damping = 1f - Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// Gets the current DynamicWaterSolver component instance.
        /// </summary>
        public DynamicWaterSolver Solver {
            get {
                return _waterSolver;
            }
        }

        /// <summary>
        /// Gets the water plane BoxCollider
        /// </summary>
        public virtual BoxCollider PlaneCollider {
            get {
                return _planeCollider;
            }
        }

        /// <summary>
        /// Gets or sets the obstruction mask. The mask will be added to the obstruction geometry as-is.
        /// </summary>
        public Texture2D ObstructionMask {
            get {
                return _obstructionMask;
            }
            set {
                if (_obstructionMask != value) {
                    _obstructionMask = value;
                    PropertyChanged();
                }
            }
        }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private DynamicWaterSolver _waterSolver;
        private bool _prevIsDirty = true;
        private DynamicWaterMesh _waterMesh;
        private DynamicWaterMesh _waterMeshSimple;
        private Vector2 _nodeSizeNormalized;
        private BoxCollider _planeCollider;
        private RecompiledMarker _recompiledMarker;

#if !ARBITRARY_ROTATIONS
    private Vector3 _cachedPosition;
    private Vector3 _cachedLossyScale;
    #endif

        /* Property fields */

        [SerializeField]
        private float _damping = 0.97f;

        [SerializeField]
        private float _speed = 20f;

        [SerializeField]
        private bool _calculateNormals = true;

        [SerializeField]
        private bool _useFastNormals = true;

        [SerializeField]
        private bool _useObstructions;

        [SerializeField]
        private Texture2D _obstructionMask;

        [SerializeField]
        private bool _setTangents;

        [SerializeField]
        private int _quality = 96;

        [SerializeField]
        private bool _meshBakeObstructionData;

        [SerializeField]
        private int _obstructionDataErosion;

        private int _resolution = 64;
        private Vector2Int _grid;
        private float _nodeSize;

        public struct ObstructionInfo {
            public GameObject GameObject;
            public Collider Collider;
            public ColliderTools.ColliderSpecificTestEnum SpecificTest;

            public ObstructionInfo(GameObject go) {
                GameObject = go;
                Collider = go.GetComponent<Collider>();
                SpecificTest = ColliderTools.ColliderSpecificTestEnum.None;
                if (go.GetComponent<MarkObstructionAsTerrain>() != null) {
                    SpecificTest = ColliderTools.ColliderSpecificTestEnum.Terrain;
                }
            }
        }

        /// <summary>
        /// Creates a circular drop splash on the fluid surface (if available).
        /// </summary>
        /// <param name="center">
        /// The center of the splash in world space coordinates.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash in world space units.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        public override void CreateSplash(Vector3 center, float radius, float force) {
            if (_waterSolver == null || !_waterSolver.CanInteract || !_collider.bounds.Contains(center)) {
                return;
            }

            center = _transform.InverseTransformPoint(center);
            center = new Vector2(center.x * _nodeSizeNormalized.x, center.z * _nodeSizeNormalized.y);
            CreateSplashNormalized(center, radius, force);
        }

        /// <summary>
        /// Creates a circular drop splashes on the fluid surface across the line.
        /// </summary>
        /// <param name="start">
        /// The start point in world space coordinates.
        /// </param>
        /// <param name="end">
        /// The end point in world space coordinates.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash in world space units.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        public void CreateSplash(Vector3 start, Vector3 end, float radius, float force) {
            if (_waterSolver == null || !_waterSolver.CanInteract || !(_collider.bounds.Contains(start) || _collider.bounds.Contains(end))) {
                return;
            }

            start = _transform.InverseTransformPoint(start);
            end = _transform.InverseTransformPoint(end);

            start = new Vector2(start.x * _nodeSizeNormalized.x, start.z * _nodeSizeNormalized.y);
            end = new Vector2(end.x * _nodeSizeNormalized.x, end.z * _nodeSizeNormalized.y);

            CreateSplashLine(new Vector2Int(start), new Vector2Int(end), radius, force);
        }

        /// <summary>
        /// Returns water level at the given position in world space.
        /// </summary>
        /// <param name="x">
        /// The x coordinate.
        /// </param>
        /// <param name="z">
        /// The z coordinate.
        /// </param>
        /// <returns>
        /// The water level at the given position in world space.
        /// </returns>
        public override float GetWaterLevel(float x, float z) {
            return GetWaterLevel(x, 0f, z);
        }

        /// <summary>
        /// Returns water level at the given position in world space.
        /// </summary>
        /// <param name="x">
        /// The x coordinate.
        /// </param>
        /// <param name="y">
        /// The y coordinate.
        /// </param>
        /// <param name="z">
        /// The z coordinate.
        /// </param>
        /// <returns>
        /// The water level at the given position in world space.
        /// </returns>
        public override float GetWaterLevel(float x, float y, float z) {
            if (_waterSolver == null) {
                return 0f;
            }

#        if ARBITRARY_ROTATIONS
            Vector3 projected;
            Vector3 projectedNormalized;
            projected.x = x;
            projected.y = y;
            projected.z = z;
            projected = _transform.InverseTransformPoint(projected);

            projectedNormalized.x = projected.x * _nodeSizeNormalized.x;
            projectedNormalized.z = projected.z * _nodeSizeNormalized.y;
            projectedNormalized.y = _waterSolver.GetFieldValue(projectedNormalized.x, projectedNormalized.z);

            projected.y = projectedNormalized.y;
            projected = transform.TransformPoint(projected);

            return projected.y;
#        else
            Vector3 projected;
            projected.x = (x - _cachedPosition.x * _cachedLossyScale.x) * _nodeSizeNormalized.x;
            projected.z = (z - _cachedPosition.z * _cachedLossyScale.z) * _nodeSizeNormalized.y;
            projected.y = _waterSolver.GetFieldValue(projected.x, projected.z);

            if (projected.y != float.NegativeInfinity) {
                projected.y = _cachedPosition.y + _cachedLossyScale.y * projected.y;
            }

            return projected.y;
#        endif
        }

        /// <summary>
        /// Returns water level at the given position in world space.
        /// </summary>
        /// <param name="position">
        /// The position at which to query the water level.
        /// </param>
        /// <returns>
        /// The water level at the given position in world space.
        /// </returns>
        public override float GetWaterLevel(Vector3 position) {
            return GetWaterLevel(position.x, position.y, position.z);
        }

        /// <summary>
        /// Returns maximum resolution (along largest side) possible for this water plane dimensions.
        /// </summary>
        /// <returns>
        /// Maximum resolution (along largest side) possible for this water plane dimensions.
        /// </returns>
        public int MaxResolution() {
            const int maxSide = 65535;

            float maxResolution = Mathf.Sqrt(maxSide * Mathf.Max(_size.x, _size.y) / Mathf.Min(_size.x, _size.y));
            int maxResolutionInt = Mathf.FloorToInt(maxResolution) - 1;

            return maxResolutionInt;
        }

        /// <summary>
        /// Returns maximum wave propagation speed possible for this quality level.
        /// </summary>
        /// <returns>
        /// Maximum wave propagation speed possible for this quality level.
        /// </returns>
        public float MaxSpeed() {
            Vector2Int grid = SizeToGridResolution(Size, _resolution);
            float speedCoeff = (grid.x + grid.y) / 128f;
            float maxSpeed = LinearWaveEqueationSolver.MaxDt / (Time.fixedDeltaTime * speedCoeff);

            return maxSpeed;
        }

        /// <summary>
        /// Recalculate the static obstructions.
        /// </summary>
        public void RecalculateObstructions() {
            // Initializing the obstruction array
            byte[] fieldObstruction = new byte[_grid.x * _grid.y];
            for (int i = 0; i < fieldObstruction.Length; i++) {
                fieldObstruction[i] = 255;
            }

            bool obstructionTextureReadable = true;

            Vector2 invGrid = new Vector2(1f / _grid.x * _collider.size.x, 1f / _grid.y * _collider.size.z);
            Vector2 invSize = new Vector2(1f / _size.x, 1f / _size.y);
            GameObject[] obstructionsGO = GetGameObjectsWithTagInBounds(DynamicWaterObstructionTag, _collider.bounds);
            ObstructionInfo[] obstructions = GetObstructionInfoArray(obstructionsGO);
            GameObject[] obstructionsInvertedGO = GetGameObjectsWithTagInBounds(DynamicWaterObstructionInvertedTag, _collider.bounds);
            ObstructionInfo[] obstructionsInverted = GetObstructionInfoArray(obstructionsInvertedGO);

            for (int j = 0; j < _grid.y; j++) {
                for (int i = 0; i < _grid.x; i++) {
                    int index = j * _grid.x + i;

                    float normX = i * invGrid.x;
                    float normZ = j * invGrid.y;

                    float normX1 = normX * invSize.x;
                    float normZ1 = normZ * invSize.y;

                    // Getting the obstruction value from obstruction mask
                    byte obstructionMaskValue = byte.MaxValue;
                    if (obstructionTextureReadable) {
                        try {
                            if (_obstructionMask != null) {
                                obstructionMaskValue =
                                    (byte) Mathf.RoundToInt(_obstructionMask.GetPixelBilinear(normX1, normZ1).r * 255f);
                                fieldObstruction[index] = obstructionMaskValue;
                            }
                            // No need to calculate obstruction further if we've got a min value from obstruction mask
                            if (obstructionMaskValue == byte.MinValue) {
                                continue;
                            }
                        } catch (Exception e) {
                            obstructionTextureReadable = false;

                            Debug.LogError(e, this);
                        }
                    }

                    // Usual obstructions
                    for (int k = 0; k < obstructions.Length; k++) {
                        ObstructionInfo obstruction = obstructions[k];

                        if (obstruction.Collider != null) {
                            Vector3 point = _transform.TransformPoint(normX, 0f, normZ);
                            if (obstruction.Collider) {
                                if (obstruction.Collider.bounds.Contains(point) &&
                                    ColliderTools.IsPointInsideCollider(obstruction.Collider, point,
                                                                        obstruction.SpecificTest)) {
                                    fieldObstruction[index] = byte.MinValue;
                                    goto REPEAT;
                                }
                            }
                        }
                    }

                    // Inverted obstructions
                    for (int k = 0; k < obstructionsInverted.Length; k++) {
                        ObstructionInfo obstruction = obstructionsInverted[k];

                        if (obstruction.Collider != null) {
                            Vector3 point = _transform.TransformPoint(normX, 0f, normZ);
                            if (obstruction.Collider) {
                                if (
                                    !(obstruction.Collider.bounds.Contains(point) &&
                                      ColliderTools.IsPointInsideCollider(obstruction.Collider, point,
                                                                          obstruction.SpecificTest))) {
                                    fieldObstruction[index] = byte.MinValue;
                                    goto REPEAT;
                                }
                            }
                        }
                    }

                    // das evil
                    REPEAT:
                    ;
                }
            }

            fieldObstruction = ErodeObstructionField(_obstructionDataErosion, fieldObstruction);

            _waterSolver.FieldObstruction = fieldObstruction;
        }

        public byte[] ErodeObstructionField(int iterations, byte[] field) {
            if (iterations == 0) {
                return field;
            }
            bool erodeInwards = iterations > 0;
            iterations = iterations < 0 ? -iterations : iterations;

            byte[] field2 = new byte[field.Length];
            Array.Copy(field, field2, field.Length);

            byte[] fieldRead = field;
            byte[] fieldWrite = field2;

            for (int n = 0; n < iterations; n++) {
                for (int j = 0; j < _grid.y; j++) {
                    for (int i = 0; i < _grid.x; i++) {
                        if (i <= 0 || j <= 0 || i >= _grid.x - 1 || j >= _grid.y - 1) {
                            continue;
                        }

                        int index = j * _grid.x + i;
                        bool isSolid = fieldRead[index] == byte.MinValue;

                        if (erodeInwards) {
                            // Erode inwards
                            if (!isSolid) {
                                continue;
                            }

                            int solidNeighbourCount = (fieldRead[index - 1] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index + 1] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index + _grid.x] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index - _grid.x] == byte.MinValue ? 1 : 0);

                            int nonSolidNeighboursCount = (fieldRead[index - 1] != byte.MinValue ? 1 : 0) +
                                                          (fieldRead[index + 1] != byte.MinValue ? 1 : 0) +
                                                          (fieldRead[index + _grid.x] != byte.MinValue ? 1 : 0) +
                                                          (fieldRead[index - _grid.x] != byte.MinValue ? 1 : 0);

                            int nonSolidNeighboursSum = ((int) fieldRead[index - 1] != (int) byte.MinValue ? (int) fieldRead[index - 1] : 0) +
                                                        ((int) fieldRead[index + 1] != (int) byte.MinValue ? (int) fieldRead[index + 1] : 0) +
                                                        ((int) fieldRead[index + _grid.x] != (int) byte.MinValue ? (int) fieldRead[index + _grid.x] : 0) +
                                                        ((int) fieldRead[index - _grid.x] != (int) byte.MinValue ? (int) fieldRead[index - _grid.x] : 0);

                            if (solidNeighbourCount != 4) {
                                fieldWrite[index] = (byte) (nonSolidNeighboursSum / nonSolidNeighboursCount);
                            }
                        } else {
                            // Erode outwards
                            if (isSolid) {
                                continue;
                            }

                            int solidNeighbourCount = (fieldRead[index - 1] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index + 1] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index + _grid.x] == byte.MinValue ? 1 : 0) +
                                                      (fieldRead[index - _grid.x] == byte.MinValue ? 1 : 0);

                            if (solidNeighbourCount >= 1 && solidNeighbourCount != 4) {
                                fieldWrite[index] = byte.MinValue;

                                byte val = (byte) (fieldRead[index] / (4 - solidNeighbourCount) * 3);
                                // byte val = 127;

                                if (fieldRead[index - 1] != byte.MinValue) {
                                    fieldRead[index - 1] += val;
                                }
                                if (fieldRead[index + 1] != byte.MinValue) {
                                    fieldRead[index + 1] += val;
                                }
                                if (fieldRead[index - _grid.x] != byte.MinValue) {
                                    fieldRead[index - _grid.x] += val;
                                }
                                if (fieldRead[index + _grid.x] != byte.MinValue) {
                                    fieldRead[index + _grid.x] += val;
                                }
                            }
                        }
                    }
                }

                Array.Copy(fieldWrite, fieldRead, field.Length);

                if (n >= iterations - 1) {
                    continue;
                }

                fieldRead = fieldRead == field2 ? field : field2;
                fieldWrite = fieldWrite == field2 ? field : field2;
            }

            return fieldWrite;
        }

        /// <summary>
        /// Gets the info required for obstruction mask generation for an array of <typeparamref name="GameObject"/>.
        /// </summary>
        /// <param name="gameObjects">
        /// The array of <typeparam name="GameObject"></typeparam>.
        /// </param>
        /// <returns>
        /// The <see cref="ObstructionInfo[]"/>.
        /// </returns>
        public ObstructionInfo[] GetObstructionInfoArray(GameObject[] gameObjects) {
            ObstructionInfo[] result = new ObstructionInfo[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; i++) {
                result[i] = new ObstructionInfo(gameObjects[i]);
            }

            return result;
        }

        /// <summary>
        /// Returns GameObjects with tag <param name="searchTag"/> within Bounds <param name="bounds"></param>.
        /// </summary>
        /// <param name="searchTag">
        /// The tag to find GameObjects with.
        /// </param>
        /// <param name="bounds">
        /// The bounds to search within.
        /// </param>
        /// <returns>
        /// The GameObject array <see cref="GameObject"/>.
        /// </returns>
        protected GameObject[] GetGameObjectsWithTagInBounds(string searchTag, Bounds bounds) {
            GameObject[] obstructionsTemp = GameObject.FindGameObjectsWithTag(searchTag);
            List<GameObject> obstructionList = new List<GameObject>(obstructionsTemp.Length);
            foreach (GameObject obstruction in obstructionsTemp) {
                if (obstruction.GetComponent<Collider>() != null && bounds.Intersects(obstruction.GetComponent<Collider>().bounds)) {
                    obstructionList.Add(obstruction);
                }
            }

            return obstructionList.ToArray();
        }

        /// <summary>
        /// Initializes the required components and creates initial Mesh.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            UpdateComponents();
            UpdateProperties();
        }

        /// <summary>
        /// Called when any property connected with the mesh is changed.
        /// </summary>
        protected override void PropertyChanged() {
            UpdateProperties();
        }

        /// <summary>
        /// Update the collider boundaries.
        /// </summary>
        protected override void UpdateCollider() {
            if (_collider == null) {
                return;
            }

            // We don't want the collider to obstruct view in Editor mode
            if (Application.isPlaying) {
                _collider.center = new Vector3(_size.x / 2f, 0f, _size.y / 2f);
                _collider.size = new Vector3(_size.x, _depth * 2f, _size.y);
            } else {
                _collider.center = new Vector3(_size.x / 2f, -_depth / 2f, _size.y / 2f);
                _collider.size = new Vector3(_size.x, _depth - 0.1f, _size.y);
            }

            _collider.isTrigger = true;
        }

        /// <summary>
        /// Create the child gameObject for interacting with water plane.
        /// </summary>
        protected void CreatePlaneCollider() {
            GameObject planeColliderObject = null;

            // Trying to find an existing collider
            foreach (Transform child in transform) {
                if (child.CompareTag(PlaneColliderLayerName)) {
                    planeColliderObject = child.gameObject;
                    break;
                }
            }

            // If we haven't found any - creating a new one
            if (planeColliderObject == null) {
                planeColliderObject = new GameObject(PlaneColliderLayerName);
                planeColliderObject.tag = PlaneColliderLayerName;
                planeColliderObject.layer = LayerMask.NameToLayer(PlaneColliderLayerName);
                planeColliderObject.transform.parent = _transform;
                planeColliderObject.transform.rotation = transform.rotation;
            }

            // Attaching and setting up the BoxCollider
            BoxCollider bc = planeColliderObject.GetComponent<BoxCollider>();
            if (bc == null) {
                bc = planeColliderObject.AddComponent<BoxCollider>();
            }

            bc.size = new Vector3(_collider.size.x, 0f, _collider.size.z);
            bc.center = _collider.center;
            bc.isTrigger = true;

            _planeCollider = bc;

            planeColliderObject.transform.localPosition = Vector3.zero;
        }

        private void Start() {
            _recompiledMarker = new RecompiledMarker();
            Initialize();
        }

        private void FixedUpdate() {
            if (!Application.isPlaying) {
                return;
            }

            if (_recompiledMarker == null) {
                Initialize();
                _recompiledMarker = new RecompiledMarker();
            }

            if (Application.isEditor) {
                UpdateComponents();
            }

#if !ARBITRARY_ROTATIONS
        _cachedPosition = _transform.position;
        _cachedLossyScale = _transform.lossyScale;
        #endif

            StepSimulation();
        }

        private void OnDestroy() {
            ClearMeshes();
        }

        private void StepSimulation() {
            if (_waterSolver == null || (!_meshRenderer.isVisible && !UpdateWhenNotVisible)) {
                return;
            }

            // Update the field
            _waterSolver.StepSimulation(_speed, _damping);

            // Update the Mesh, if needed
            if (_waterSolver.IsDirty) {
                _waterMesh.IsDirty = _waterSolver.IsDirty;
                if (_meshRenderer.isVisible) {
                    float[] field = _waterSolver.Field;
                    byte[] fieldObstruction = _waterSolver.FieldObstruction;
                    _waterMesh.UpdateMesh(field, fieldObstruction);
                }
            }

            // Switch to simple mesh, if we can
            if (!_meshBakeObstructionData && (_prevIsDirty != _waterSolver.IsDirty)) {
                _meshFilter.mesh = _waterSolver.IsDirty ? _waterMesh.Mesh : _waterMeshSimple.Mesh;
            }

            _prevIsDirty = _waterSolver.IsDirty;
        }

        private void UpdateComponents() {
            _meshFilter = gameObject.GetComponent<MeshFilter>();
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Called when any property that defines the water simulation, changes.
        /// </summary>
        private void UpdateProperties() {
            // Calculating the grid parameters
            _resolution = Mathf.RoundToInt(_quality * MaxResolution() / 256f);
            GridMath.CalculateGrid(_resolution, Size, out _grid, out _nodeSize);

            // Determining what Solver instance do we have to use
            // and destroy other instances
            if (Application.isPlaying) {
                _waterSolver = null;

                DynamicWaterSolver[] solverComponents = GetComponents<DynamicWaterSolver>();
                foreach (DynamicWaterSolver solverComponent in solverComponents) {
                    if (_waterSolver == null) {
                        _waterSolver = solverComponent;
                    } else {
                        Destroy(solverComponent);
                    }
                }

                if (solverComponents.Length > 1) {
                    Debug.LogWarning(
                        "More than one DynamicWaterSolver component present. Using the first one, others are destroyed",
                        this);
                }

                // Creating the default solver, if no one is found
                if (_waterSolver == null) {
                    _waterSolver = gameObject.AddComponent<DynamicWaterSolverSimulation>();
                }

                _waterSolver.Initialize(_grid);

                // Recalculate the static obstruction objects
                if (_useObstructions) {
                    RecalculateObstructions();
                }
            }

            // Clearing the old meshes, if any
            ClearMeshes();

            // Creating the simple mesh substitute for when Solver is not in dirty state
            const int simpleMeshResolution = 6;
            _waterMeshSimple = new DynamicWaterMesh(simpleMeshResolution, Size, this);

            // Creating the actual water mesh
            _waterMesh = new DynamicWaterMesh(_resolution, Size, this, _waterSolver != null ? _waterSolver.FieldObstruction : null);

            // Assigning the mesh
            _meshFilter = gameObject.GetComponent<MeshFilter>();
            _meshFilter.mesh = _waterMesh.Mesh;

            // Precache for faster conversion
            _nodeSizeNormalized.x = 1f / Size.x * _grid.x;
            _nodeSizeNormalized.y = 1f / Size.y * _grid.y;

            // Update the collider bound
            UpdateCollider();

            // Create the child gameObject for interacting with water plane
            if (Application.isPlaying && UsePlaneCollider) {
                CreatePlaneCollider();
            }

            _prevIsDirty = true;
        }

        /// <summary>
        /// The size to grid resolution.
        /// </summary>
        /// <param name="size">
        /// The water plane dimensions.
        /// </param>
        /// <param name="resolution">
        /// The resolution.
        /// </param>
        /// <returns>
        /// The <see cref="Vector2Int"/>.
        /// </returns>
        private Vector2Int SizeToGridResolution(Vector2 size, int resolution) {
            float nodeSize = Mathf.Max(size.x, size.y) / resolution;
            Vector2Int grid;
            grid.x = Mathf.RoundToInt(size.x / nodeSize) + 1;
            grid.y = Mathf.RoundToInt(size.y / nodeSize) + 1;

            return grid;
        }

        /// <summary>
        /// Removes the meshes from the memory.
        /// </summary>
        private void ClearMeshes() {
            if (_meshFilter != null) {
                DestroyImmediate(_meshFilter.sharedMesh);
            }

            if (_waterMeshSimple != null) {
                _waterMeshSimple.FreeMesh();
                _waterMeshSimple = null;
            }

            if (_waterMesh != null) {
                _waterMesh.FreeMesh();
                _waterMesh = null;
            }
        }

        /// <summary>
        /// Creates a circular drop splash on the fluid surface (if available).
        /// </summary>
        /// <param name="center">
        /// The center of the splash in simulation field space coordinates.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash in simulation field space space units.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        private void CreateSplashNormalized(Vector2 center, float radius, float force) {
            radius = _size.x < _size.y ? radius / _size.x * _grid.x : radius / _size.y * _grid.y;

            _waterSolver.CreateSplashNormalized(center, radius, force);
        }

        private void CreateSplashLine(Vector2Int start, Vector2Int end, float radius, float force) {
            int dx = Math.Abs(end.x - start.x);
            int dy = Math.Abs(end.y - start.y);

            int sx, sy;

            if (start.x < end.x) {
                sx = 1;
            } else {
                sx = -1;
            }
            if (start.y < end.y) {
                sy = 1;
            } else {
                sy = -1;
            }

            int err = dx - dy;
            bool splashMade = false;
            while (true) {
                if (start.x == end.x && start.y == end.y) {
                    break;
                }

                Vector2 startf;
                startf.x = start.x;
                startf.y = start.y;
                CreateSplashNormalized(startf, radius, force);

                splashMade = true;

                int e2 = 2 * err;

                if (e2 > -dy) {
                    err = err - dy;
                    start.x = start.x + sx;
                }

                if (e2 < dx) {
                    err = err + dx;
                    start.y = start.y + sy;
                }
            }

            // Make sure we have made at least one splash
            if (!splashMade) {
                CreateSplashNormalized(new Vector2(start.x, start.y), radius, force);
            }
        }

        private void OnDrawGizmos() {
            if (!Application.isEditor) {
                return;
            }

            Gizmos.DrawIcon(new Vector3(GetComponent<Collider>().bounds.center.x, transform.position.y + 0.1f, GetComponent<Collider>().bounds.center.z), "DynamicWater/FluidVolume.png");
            Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
            if (_collider != null) {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(new Vector3(Size.x / 2f, -Depth / 2f, Size.y / 2f), new Vector3(Size.x, Depth, Size.y));
            }
        }
    }
#if !UNITY_3_5
}
#endif