using UnityEngine;
using LostPolygon.DynamicWaterSystem;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// Base class for dynamic wave simulation.
    /// </summary>
    [AddComponentMenu("Lost Polygon/Dynamic Water System/Wave Simulation Solver")]
    public class DynamicWaterSolverSimulation : DynamicWaterSolver {
        protected float[] FieldSim;
        protected float[] FieldSimNew;
        protected float[] FieldSimSpeed;
        protected float _simulationSpeedNormalizationFactor;
        private bool _switchField;

        /// <summary>
        /// The initialization method. Automatically called by <see cref="DynamicWater"/>.
        /// </summary>
        /// <param name="gridSize">
        /// Simulation grid resolution.
        /// </param>
        /// <seealso cref="DynamicWater"/>
        public override void Initialize(Vector2Int gridSize) {
            base.Initialize(gridSize);

            FieldSim = new float[_grid.x * _grid.y];
            FieldSimNew = new float[_grid.x * _grid.y];
            FieldSimSpeed = new float[_grid.x * _grid.y];

            // Initial state
            Field = FieldSim;
        }

        /// <summary>
        /// Executes the wave simulation step.
        /// </summary>
        /// <param name="speed">
        /// Overall simulation speed factor.
        /// </param>
        /// <param name="damping">
        /// The damping value (range 0-1).
        /// </param>
        public override void StepSimulation(float speed, float damping) {
            if (!_isDirty || !_isInitialized) {
                return;
            }

            _simulationSpeedNormalizationFactor = (_grid.x + _grid.y) / 128f; // assume 64 grid as normal

            float dt = Mathf.Clamp(Time.deltaTime * speed * _simulationSpeedNormalizationFactor, 0f, LinearWaveEqueationSolver.MaxDt);
            float max;
#if !UNITY_FLASH
            bool useNative = DynamicWaterNativeLibrary.CanUseNative;
#else
        const bool useNative = false;
        #endif

            if (useNative) {
#if !UNITY_FLASH
                if (_switchField) {
                    DynamicWaterNativeLibrary.SolveWaveEquationNative(_grid.x, _grid.y, dt, damping, out max, FieldSim, FieldSimNew, FieldSimSpeed, _fieldObstruction);
                    _field = FieldSimNew;
                } else {
                    DynamicWaterNativeLibrary.SolveWaveEquationNative(_grid.x, _grid.y, dt, damping, out max, FieldSimNew, FieldSim, FieldSimSpeed, _fieldObstruction);
                    _field = FieldSim;
                }
#endif
            } else {
                if (_switchField) {
                    LinearWaveEqueationSolver.Solve(FieldSim, FieldSimNew, FieldSimSpeed, _fieldObstruction, _grid, dt, damping, out max);
                    _field = FieldSimNew;
                } else {
                    LinearWaveEqueationSolver.Solve(FieldSimNew, FieldSim, FieldSimSpeed, _fieldObstruction, _grid, dt, damping, out max);
                    _field = FieldSim;
                }
            }

            _switchField = !_switchField;

            _isDirty = max > DirtyThreshold;
        }

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
        public override float GetFieldValue(float x, float z) {
            if (x <= 0 || z <= 0 || x >= _grid.x || z >= _grid.y) {
                return float.NegativeInfinity;
            }

            return FieldSim[(int) z * _grid.x + (int) x];
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
        public override void CreateSplashNormalized(Vector2 center, float radius, float force) {
            if (!_switchField) {
                CreateSplashNormalized(center, radius, force, ref FieldSimNew);
            } else {
                CreateSplashNormalized(center, radius, force, ref FieldSim);
            }
        }
    }
#if !UNITY_3_5
}
#endif