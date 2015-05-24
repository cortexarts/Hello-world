//#define USE_TRIANGLE_STRIP

using UnityEngine;

namespace LostPolygon.DynamicWaterSystem {
    public class DynamicWaterMesh {
        /// <summary>
        /// Gets a value indicating whether the water mesh is dirty and must be updated.
        /// </summary>
        public bool IsDirty {
            get {
                return _isDirty;
            }
            set {
                _isDirty = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the water mesh is initilialized.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets the water Mesh instance.
        /// </summary>
        public Mesh Mesh {
            get {
                return _mesh;
            }
        }

        private readonly Vector2Int _grid;
        private readonly float _nodeSize;
        private readonly Vector2 _size;
        private readonly IDynamicWaterSettings _settings;
        private readonly Mesh _mesh;
        private bool _isDirty;
        private MeshStruct _meshStruct;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicWaterMesh"/> class.
        /// </summary>
        /// <param name="resolution">
        /// The number of grid nodes along the bigger side of the mesh.
        /// </param>
        /// <param name="size">The width and length of the mesh</param>
        /// <param name="settings">
        /// DynamicWaterSettings instance representing the public settings properties of <see cref="DynamicWater"/> class.
        /// </param>
        /// <param name="fieldObstruction">
        /// The array of <c>byte</c> indicating whether the water is obstructed by an object. \n
        /// <c>0</c> means the grid node is obstructed by an object, so the simulation is not updated;
        /// <c>255</c> means the grid node is not obstructed, and the simulation can proceed freely.
        /// Intermediate values represent the additional dampening value in that node.
        /// </param>
        public DynamicWaterMesh(int resolution,
                                Vector2 size,
                                IDynamicWaterSettings settings,
                                byte[] fieldObstruction = null) {
            IsReady = false;

            _settings = settings;

            _size = size;
            GridMath.CalculateGrid(resolution, size, out _grid, out _nodeSize);

            // Some failsafe
            if (_size.x < Vector3.kEpsilon || _size.y < Vector3.kEpsilon) {
                _grid = new Vector2Int(1, 1);
            }

            _mesh = new Mesh();
            _mesh.name = "DynamicWaterMesh";
            #if !UNITY_3_5
            _mesh.MarkDynamic();
            #endif
            AllocateMeshArrays();
            CreateMeshGrid(fieldObstruction);
            AssignMesh();
            _mesh.RecalculateBounds();

            IsReady = true;
        }

        /// <summary>
        /// Updates the mesh state using the simulation state field <paramref name="field"/> 
        /// and (optional) the obstruction field.
        /// </summary>
        /// <param name="field">
        /// The simulation state field.
        /// </param>
        /// <param name="fieldObstruction">
        /// The array of <c>bool</c> indicating whether the water is obstructed by an object.
        /// </param>
        public void UpdateMesh(float[] field, byte[] fieldObstruction)
        {
            if (!_isDirty || !IsReady)
            {
                return;
            }

            #if !UNITY_FLASH
            bool useNative = DynamicWaterNativeLibrary.CanUseNative;
            #else
            const bool useNative = false;
            #endif

            // Caching some values to avoid expensive calls
            bool isFieldObstructionNull = fieldObstruction == null;
            bool calculateNormals = _settings.CalculateNormals;
            bool useFastNormals = _settings.UseFastNormals;

            if (useNative)
            {
                #if !UNITY_FLASH
                DynamicWaterNativeLibrary.UpdateMesh(calculateNormals, useFastNormals, _nodeSize, _grid.x, _grid.y, field, fieldObstruction, _meshStruct.Vertices, _meshStruct.Normals);
                #endif
            }
            else
            {
                // We can't run native, run C# then
                Vector3 up = Vector3.up;
                Vector3 normal = up;
                float doubleNodeSize = 2f * _nodeSize;

                // Minimal node value to calculate normals for (in case of using approximate normals).
                const float normalThreshold = 0.0001f;

                // If not using fast normals, then all we need is to update the vertices
                if (!useFastNormals)
                {
                    for (int j = 0; j < _grid.y; j++)
                    {
                        int index = j * _grid.x;
                        for (int i = 0; i < _grid.x; i++)
                        {
                            _meshStruct.Vertices[index].y = field[index];

                            index++;
                        }
                    }
                }
                else
                {
                    // Otherwise, calculate the normals too
                    for (int j = 0; j < _grid.y; j++)
                    {
                        int index = j * _grid.x;
                        for (int i = 0; i < _grid.x; i++)
                        {
                            if ((i == 0 || j == 0 || i >= _grid.x - 1 || j >= _grid.y - 1) || (!isFieldObstructionNull && fieldObstruction[index] == byte.MinValue))
                            {
                                normal = up;
                            }
                            else
                            {
                                float valAbs;
                                if (field[index] > 0f)
                                {
                                    valAbs = field[index];
                                }
                                else
                                {
                                    valAbs = -field[index];
                                }

                                if (valAbs > normalThreshold)
                                {
                                    normal.x = field[index - 1] - field[index + 1];
                                    normal.z = field[index - _grid.x] - field[index + _grid.x];
                                    normal.y = doubleNodeSize;

                                    // Fast approximate Vector3 normalization
                                    // See also: LostPolygon.DynamicWaterSystem.FastFunctions.FastInvSqrt()
                                    float invLength = normal.x * normal.x + normal.y * normal.y + normal.z * normal.z;
                                    FastFunctions.FloatIntUnion u;
                                    u.i = 0;
                                    u.f = invLength;
                                    float xhalf = 0.5f * invLength;
                                    u.i = 0x5f3759df - (u.i >> 1);
                                    invLength = u.f * (1.5f - xhalf * u.f * u.f);

                                    normal.x *= invLength;
                                    normal.y *= invLength;
                                    normal.z *= invLength;
                                }
                                else
                                {
                                    normal = up;
                                }
                            }

                            _meshStruct.Normals[index] = normal;

                            _meshStruct.Vertices[index].y = field[index];
                            index++;
                        }
                    }
                }
            }


            // Actually updating the mesh
            _mesh.vertices = _meshStruct.Vertices;
            if (calculateNormals)
            {
                if (useFastNormals)
                {
                    _mesh.normals = _meshStruct.Normals;
                }
                else
                {
                    _mesh.RecalculateNormals();
                }
            }

            _isDirty = false;
        }

        /// <summary>
        /// Frees the mesh object.
        /// </summary>
        public void FreeMesh() {
            if (_mesh != null) {
                UnityEngine.Object.DestroyImmediate(_mesh);
            }
        }

        /// <summary>
        /// Assigns the initial mesh values generated in <see cref="CreateMeshGrid"/> to the Mesh object.
        /// </summary>
        private void AssignMesh() {
            _mesh.vertices = _meshStruct.Vertices;
            _mesh.normals = _meshStruct.Normals;
            if (_settings.SetTangents) {
                _mesh.tangents = _meshStruct.Tangents;
            }

            _mesh.uv = _meshStruct.UV;
            _mesh.colors32 = _meshStruct.Colors32;
            _mesh.triangles = _meshStruct.Triangles;

            _mesh.RecalculateBounds();

            // Freeing the memory
            _meshStruct.Tangents = null;
            _meshStruct.Triangles = null;
            _meshStruct.UV = null;
            _meshStruct.Colors32 = null;
        }

        /// <summary>
        /// Generates the initial values for the Mesh object.
        /// </summary>
        private void CreateMeshGrid(byte[] fieldObstruction = null) {
            float uvStepXInit = 1f / (_size.x / _nodeSize);
            float uvStepYInit = 1f / (_size.y / _nodeSize);
            Vector3 up = Vector3.up;

            // Tangents are not really recalculated, as that'd be horribly slow
            // for anything realtime. But in case of water it's hard to notice anyway.
            Vector4 tangent = new Vector4(1f, 0f, 0f, 1f);
            Color32 obstructionNone = new Color32(255, 0, 0, 0);
            Color32 obstructionData = new Color32(0, 0, 0, 0);
            Color32 obstructionDataSolid = new Color32(0, 0, 255, 0);

            bool setTangents = _settings.SetTangents;
            bool useObstructionData = _settings.MeshBakeObstructionData && fieldObstruction != null;

            int k = 0;

            for (int j = 0; j < _grid.y; j++) {
                for (int i = 0; i < _grid.x; i++) {
                    int index = j * _grid.x + i;

                    // Set vertices
                    _meshStruct.Vertices[index].x = i * _nodeSize;
                    _meshStruct.Vertices[index].y = 0f;
                    _meshStruct.Vertices[index].z = j * _nodeSize;

                    // Set triangles
                    if (j < _grid.y - 1 && i < _grid.x - 1) {
                        _meshStruct.Triangles[k + 0] = (j * _grid.x) + i;
                        _meshStruct.Triangles[k + 1] = ((j + 1) * _grid.x) + i;
                        _meshStruct.Triangles[k + 2] = (j * _grid.x) + i + 1;

                        _meshStruct.Triangles[k + 3] = ((j + 1) * _grid.x) + i;
                        _meshStruct.Triangles[k + 4] = ((j + 1) * _grid.x) + i + 1;
                        _meshStruct.Triangles[k + 5] = (j * _grid.x) + i + 1;

                        k += 6;
                    }

                    // Set UV
                    float uvStepX = uvStepXInit;
                    float uvStepY = uvStepYInit;

                    _meshStruct.UV[index].x = i * uvStepX;
                    _meshStruct.UV[index].y = j * uvStepY;

                    // Set colors
                    if (useObstructionData) {
                        obstructionData.r = fieldObstruction[index];
                        _meshStruct.Colors32[index] = fieldObstruction[index] == byte.MinValue ? obstructionDataSolid : obstructionData;
                    } else {
                        _meshStruct.Colors32[index] = obstructionNone;
                    }

                    // Set normals
                    _meshStruct.Normals[index] = up;

                    // Set tangents
                    if (setTangents) {
                        _meshStruct.Tangents[index] = tangent;
                    }

                    // Fix stretching
                    float UVDelta;

                    if (i == _grid.x - 1) {
                        if (_meshStruct.Vertices[index].x > _size.x) {
                            UVDelta = (_size.x - _meshStruct.Vertices[index].x) / _nodeSize;
                            _meshStruct.UV[index].x -= uvStepX * UVDelta;

                            _meshStruct.Vertices[index].x = _size.x;
                        }

                        if (_size.x - _meshStruct.Vertices[index].x < _nodeSize) {
                            UVDelta = (_size.x - _meshStruct.Vertices[index].x) / _nodeSize;
                            _meshStruct.UV[index].x += uvStepX * UVDelta;

                            _meshStruct.Vertices[index].x = _size.x;
                        }
                    }

                    if (j == _grid.y - 1) {
                        if (_meshStruct.Vertices[index].z > _size.y) {
                            UVDelta = (_size.y - _meshStruct.Vertices[index].z) / _nodeSize;
                            _meshStruct.UV[index].y -= uvStepY * UVDelta;

                            _meshStruct.Vertices[index].z = _size.y;
                        }

                        if (_size.y - _meshStruct.Vertices[index].z < _nodeSize) {
                            UVDelta = (_size.y - _meshStruct.Vertices[index].z) / _nodeSize;
                            _meshStruct.UV[index].y += uvStepY * UVDelta;

                            _meshStruct.Vertices[index].z = _size.y;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allocates memory for Mesh arrays.
        /// </summary>
        private void AllocateMeshArrays() {
            int numVertices = _grid.x * _grid.y;
            _meshStruct.Vertices = new Vector3[numVertices];
            _meshStruct.Normals = new Vector3[numVertices];
            if (_settings.SetTangents) {
                _meshStruct.Tangents = new Vector4[numVertices];
            }

            _meshStruct.Colors32 = new Color32[numVertices];
            _meshStruct.UV = new Vector2[numVertices];
            _meshStruct.Triangles = new int[((_grid.x - 1) * (_grid.y - 1)) * 2 * 3];

            _isDirty = true;
        }
    }
}