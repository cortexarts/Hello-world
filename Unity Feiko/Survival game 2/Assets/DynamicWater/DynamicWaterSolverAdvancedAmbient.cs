using System;
using UnityEngine;
using LostPolygon.DynamicWaterSystem;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// Ancestor of DynamicWaterSolverSimulation capable of doing some very crude ocean-like waves.
    /// </summary>
    /// <remarks>
    /// This Solver is not very efficient performance-wise, so it's probably better to avoid using it on mobile.
    /// </remarks>
    [AddComponentMenu("Lost Polygon/Dynamic Water System/Advanced Ambient Wave Solver")]
    public class DynamicWaterSolverAdvancedAmbient : DynamicWaterSolverSimulation {
        [Serializable]
        public class Wave {
            public float Amplitude = 2;
            public float Angle;
            public float Frequency = 20;
            public float Velocity = 5;
            public int Steepness = 1;
            public bool Circular;
            public Vector2 Position = new Vector2(0.5f, 0.5f);
            public bool Excluded = false;

            [NonSerialized]
            public Vector2 Direction;

            [NonSerialized]
            public Vector2 CircleShift;
        }

        public Wave[] Waves;

        /// <summary>
        /// Value indicating whether only ambient waves have to be simulated.
        /// </summary>
        public bool OnlyAmbient = true;

        /// <summary>
        /// The resulting simulation state. Calculated as the sum of ambient waves and dynamic simulated waves.
        /// </summary>
        private float[] _fieldSum;

        private float _time;

        /// <summary>
        /// The initialization method. Called by <see cref="DynamicWater"/>.
        /// </summary>
        /// <param name="gridSize">
        /// The simulation grid resolution.
        /// </param>
        /// <seealso cref="DynamicWater"/>
        public override void Initialize(Vector2Int gridSize) {
            base.Initialize(gridSize);

            _fieldSum = new float[_grid.x * _grid.y];

            // Initial state
            _field = _fieldSum;

            _canInteract = !OnlyAmbient;
        }

        public override void CreateSplashNormalized(Vector2 center, float radius, float force) {
            if (OnlyAmbient) {
                return;
            }

            CreateSplashNormalized(center, radius, force, ref FieldSim);
        }

        public override float GetFieldValue(float x, float z) {
            if (x <= 0 || z <= 0 || x >= _grid.x || z >= _grid.y) {
                return float.NegativeInfinity;
            }

            return _fieldSum[(int) z * _grid.x + (int) x];
        }

        /// <summary>
        /// Executes the simulation step.
        /// </summary>
        /// <param name="speed">
        /// The overall simulation speed factor.
        /// </param>
        /// <param name="damping">
        /// The damping value (range 0-1).
        /// </param>
        public override void StepSimulation(float speed, float damping) {
            if (!OnlyAmbient) {
                base.StepSimulation(speed, damping);
            }

            _canInteract = !OnlyAmbient;
            bool isFieldObstructionNull = _fieldObstruction == null;

            // Ambient wave time
            _time += Time.deltaTime;

            // Recalculating direction
            foreach (Wave wave in Waves) {
                wave.Direction = new Vector2(FastFunctions.FastCos(wave.Angle * FastFunctions.Deg2Rad), FastFunctions.FastSin(wave.Angle * FastFunctions.Deg2Rad));
                wave.CircleShift = new Vector2(-Mathf.Clamp01(wave.Position.x) * FastFunctions.DoublePi, -Mathf.Clamp01(wave.Position.y) * FastFunctions.DoublePi);
            }

            // Caching to avoid many divisions
            Vector2 invGrid = new Vector2(1f / _grid.x, 1f / _grid.y);
            for (int i = 0; i < _grid.x; i++) {
                for (int j = 0; j < _grid.y; j++) {
                    int index = j * _grid.x + i;

                    if (!isFieldObstructionNull && _fieldObstruction[index] == byte.MinValue) {
                        continue;
                    }
                    // Obstruction value (0-1) determined by obstruction geometry and obstruction mask
                    float obstructionValue = isFieldObstructionNull ? 1f : _fieldObstruction[index] * FastFunctions.InvertedByteMaxValue;

                    // Normalizing the coordinates
                    float normX = i * invGrid.x * FastFunctions.DoublePi;
                    float normY = j * invGrid.y * FastFunctions.DoublePi;
                    _fieldSum[index] = 0f;
                    for (int k = 0; k < Waves.Length; k++) {
                        Wave wave = Waves[k];
                        // Calculating new node value
                        if (wave.Excluded) {
                            continue;
                        }

                        float val;
                        if (wave.Circular) {
                            normX += wave.CircleShift.x;
                            normY += wave.CircleShift.y;
                            /* Non-optimized version. Use it if you want */
                            //val = FastFunctions.FastSqrt(normX * normX + normY * normY) * wave.Frequency + _time * wave.Velocity;

                            val = normX * normX + normY * normY;

                            FastFunctions.FloatIntUnion u;
                            u.i = 0;
                            u.f = val;
                            float xhalf = 0.5f * val;
                            u.i = 0x5f375a86 - (u.i >> 1);
                            u.f = u.f * (1.5f - xhalf * u.f * u.f);

                            val = u.f * val * wave.Frequency + _time * wave.Velocity;
                        } else {
                            val = (wave.Direction.x * normX + wave.Direction.y * normY) * wave.Frequency + _time * wave.Velocity;
                        }

                        /* Non-optimized version. Use it if you want */
                        //_fieldSum[index] += FastFunctions.FastPow((FastFunctions.FastSin(val) + 1f) * 0.5f, wave.Steepness) * wave.Amplitude * obstructionValue;

                        /* Optimized version */
                        // FastFunctions.FastSin(val)
                        float tmpVal = (val + Mathf.PI) * FastFunctions.InvDoublePi;
                        int floor = tmpVal >= 0 ? (int) (tmpVal) : (int) ((tmpVal) - 1);
                        val = val - FastFunctions.DoublePi * floor;

                        if (val < 0) {
                            val = 1.27323954f * val + 0.405284735f * val * val;
                        } else {
                            val = 1.27323954f * val - 0.405284735f * val * val;
                        }

                        // FastFunctions.FastPowInt
                        tmpVal = (val + 1f) * 0.5f;
                        int steepness = wave.Steepness;

                        while (steepness > 0) {
                            tmpVal *= val;
                            steepness--;
                        }

                        // Final calculations
                        val = tmpVal * wave.Amplitude * obstructionValue;
                        _fieldSum[index] += val;

                        /* End of optimized version */
                    }

                    if (!OnlyAmbient) {
                        _fieldSum[index] += FieldSimNew[index];
                    }
                }
            }

            // Updating the field
            _field = _fieldSum;

            _isDirty = true;
        }
    }
#if !UNITY_3_5
}
#endif