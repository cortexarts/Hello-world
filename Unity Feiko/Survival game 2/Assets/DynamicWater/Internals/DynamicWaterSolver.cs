using UnityEngine;

namespace LostPolygon.DynamicWaterSystem {
    /// <summary>
    /// An abstract class representing the fluid wave function.
    /// </summary>
    public abstract class DynamicWaterSolver : MonoBehaviour, IDynamicWaterSolverFieldState {
        /// <summary>
        /// Represents the current simulation state.
        /// </summary>
        public float[] Field {
            get {
                return _field;
            }
            set {
                _field = value;
            }
        }

        /// <summary>
        /// The array of <c>byte</c> indicating whether the water is obstructed by an object. \n
        /// <c>0</c> means the grid node is obstructed by an object, so the simulation is not updated;
        /// <c>255</c> means the grid node is not obstructed, and the simulation can proceed freely.
        /// Intermediate values represent the additional dampening value in that node.
        /// </summary>
        public byte[] FieldObstruction {
            get {
                return _fieldObstruction;
            }
            set {
                _fieldObstruction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the solver currently
        /// allows interaction (like creating splashes dynamically).
        /// </summary>
        public bool CanInteract {
            get {
                return _canInteract;
            }
            protected set {
                _canInteract = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the simulation is dirty and has to be updated.
        /// </summary>
        /// <remarks>
        /// For optimization, <see cref="StepSimulation"/> must no be called if <c>IsDirty == false</c>, as
        /// nothing will visible change anyway.
        /// </remarks>
        public bool IsDirty {
            get {
                return _isDirty;
            }
            protected set {
                _isDirty = value;
            }
        }

        /// <summary>
        /// Constant value representing the maximal wave height when
        /// the simulation can be considered as not dirty.
        /// </summary>
        /// <remarks>
        /// You may adjust the value to a lower one is you notice
        /// the sudden "jumps" when the fluid is almost still.
        /// </remarks>
        /// <seealso cref="IsDirty"/>
        public const float DirtyThreshold = 0.0001f;

        protected bool _isDirty;
        protected bool _canInteract = true;
        protected Vector2Int _grid;
        protected bool _isInitialized = false;

        protected float[] _field;
        protected byte[] _fieldObstruction;
        

        /// <summary>
        /// Ensures that DynamicWaterSolver is attached to the object having a DynamicWater component.
        /// </summary>
        private void Awake() {
            if (GetComponent<DynamicWater>() == null) {
                Debug.LogError("DynamicWaterSolver attached to a GameObject without a DynamicWater component. Destroying the Solver");
                Destroy(this);
            }
        }

        /// <summary>
        /// The initialization method. Automatically called by <see cref="DynamicWater"/>.
        /// </summary>
        /// <param name="gridSize">
        /// The size in nodes of simulation grid.
        /// </param>
        /// <seealso cref="DynamicWater"/>
        public virtual void Initialize(Vector2Int gridSize) {
            _isInitialized = true;

            _grid = gridSize;
        }

        /// <summary>
        /// Creates a circular drop splash on the fluid surface.
        /// </summary>
        /// <param name="center">
        /// The center of the splash in simulation grid space.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        public abstract void CreateSplashNormalized(Vector2 center, float radius, float force);

        /// <summary>
        /// Returns water level at the given position in simulation grid space.
        /// </summary>
        /// <param name="x">
        /// The x coordinate.
        /// </param>
        /// <param name="z">
        /// The z coordinate.
        /// </param>
        /// <returns>
        /// The water level at the given position in simulation grid space.
        /// Returns negative infinity when the given position is outside the fluid.
        /// </returns>
        public abstract float GetFieldValue(float x, float z);

        /// <summary>
        /// Executes the simulation step. This is the method where the wave simulation has to be written.
        /// </summary>
        /// <param name="speed">
        /// The overall simulation speed factor.
        /// </param>
        /// <param name="damping">
        /// The damping value (range 0-1).
        /// </param>
        public abstract void StepSimulation(float speed, float damping);

        /// <summary>
        /// Creates a circular drop splash on the fluid surface.
        /// </summary>
        /// <param name="center">
        /// The center of the splash in simulation grid space.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash in simulation grid nodes.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        /// <param name="field">
        /// The simulation state field to create splash on.
        /// </param>
        protected void CreateSplashNormalized(Vector2 center, float radius, float force, ref float[] field) {
            if (!_isInitialized || !_canInteract) {
                return;
            }

            const float threshold = 0.02f;
            bool isFieldObstructionNull = _fieldObstruction == null;
            float invSqrRadius = 1f / (radius * radius);

            if (radius > 1f) {
                // Do not calculate anything outside splash radius
                int minX = Mathf.Clamp(Mathf.RoundToInt(center.x - radius), 1, _grid.x - 1);
                int maxX = Mathf.Clamp(Mathf.RoundToInt(center.x + radius), 1, _grid.x - 1);
                int minY = Mathf.Clamp(Mathf.RoundToInt(center.y - radius), 1, _grid.y - 1);
                int maxY = Mathf.Clamp(Mathf.RoundToInt(center.y + radius), 1, _grid.y - 1);
                for (int j = minY; j < maxY; j++) {
                    for (int i = minX; i < maxX; i++) {
                        int index = j * _grid.x + i;

                        if (!isFieldObstructionNull && _fieldObstruction[index] == byte.MinValue) {
                            continue;
                        }
                        float obstructionValue = isFieldObstructionNull ? 1f : _fieldObstruction[index] * FastFunctions.InvertedByteMaxValue;

                        // 1 - distance^2 / radius^2
                        float drop = 1f - ((center.x - i) * (center.x - i) + (center.y - j) * (center.y - j)) * invSqrRadius * obstructionValue;
                        if (drop < threshold) {
                            continue;
                        }

                        drop = drop * drop;
                        drop = drop * drop * 0.0416666666f - drop * 0.5f;
                        field[index] += drop * force;
                    }
                }
            } else {
                int x = Mathf.Clamp((int) center.x, 1, _grid.x - 2);
                int y = Mathf.Clamp((int) center.y, 1, _grid.y - 2);

                int index = y * _grid.x + x;

                if (!isFieldObstructionNull && _fieldObstruction[index] == byte.MinValue) {
                    return;
                }
                float obstructionValue = isFieldObstructionNull ? 1f : _fieldObstruction[index] * FastFunctions.InvertedByteMaxValue;

                field[index] += -force * obstructionValue;
            }

            _isDirty = true;
        }
    }
}