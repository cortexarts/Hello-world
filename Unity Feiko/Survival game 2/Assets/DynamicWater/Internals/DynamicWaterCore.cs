using UnityEngine;

namespace LostPolygon.DynamicWaterSystem {
    /// <summary>
    /// Interface representing the state of fluid simulation in simulation grid space.
    /// </summary>
    public interface IDynamicWaterSolverFieldState {
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
        /// </returns>
        float GetFieldValue(float x, float z);
    }

    /// <summary>
    /// Interface representing the state of fluid simulation.
    /// </summary>
    public interface IDynamicWaterFieldState {
        /// <summary>
        /// Returns water level at the given position in simulation grid space.
        /// </summary>
        /// <param name="position">
        /// The position at which to query the water level.
        /// </param>
        /// <returns>
        /// The water level at the given position in world space.
        /// </returns>
        float GetWaterLevel(Vector3 position);

        /// <summary>
        /// Returns water level at the given position in simulation grid space.
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
        float GetWaterLevel(float x, float y, float z);

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
        /// The water level at the given position in world space.
        /// </returns>
        float GetWaterLevel(float x, float z);
    }

    /// <summary>
    /// Interface representing generic fluid volume physical properties.
    /// </summary>
    public interface IDynamicWaterFluidVolume : IDynamicWaterFieldState {
        /// <summary>
        /// Gets the Transform.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// Gets or sets the size of simulation field in world units.
        /// </summary>
        Vector2 Size { get; }

        /// <summary>
        /// Gets the fluid density in kg/m^3.
        /// </summary>
        float Density { get; }

        /// <summary>
        /// Gets the fluid volume BoxCollider
        /// </summary>
        BoxCollider Collider { get; }

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
        void CreateSplash(Vector3 center, float radius, float force);
    }

    /// <summary>
    /// Interface representing the public properties of <see cref="DynamicWater"/> class.
    /// </summary>
    public interface IDynamicWaterSettings : IDynamicWaterFluidVolume {
        /// <summary>
        /// Gets the actual simulation grid resolution.
        /// </summary>
        Vector2Int GridSize { get; }

        /// <summary>
        /// Gets the size of single simulation grid node in world space.
        /// </summary>
        float NodeSize { get; }

        /// <summary>
        /// Gets the current DynamicWaterSolver component instance.
        /// </summary>
        DynamicWaterSolver Solver { get; }

        /// <summary>
        /// Gets a value indicating whether the water mesh normals should be calculated.
        /// </summary>
        bool CalculateNormals { get; }

        /// <summary>
        /// Gets a value indicating whether the fast approximate method of calculating the
        /// water mesh normals should be used instead of <c>Mesh.RecalculateNormals()</c>.
        /// </summary>
        /// <remarks>
        /// Enabling this could provide a huge performance boost with the cost of degraded quality.
        /// Especially useful on mobile devices.
        /// </remarks>
        bool UseFastNormals { get; }

        /// <summary>
        /// Gets a value indicating whether the tangents must be set (usually for bump-mapped shaders).
        /// </summary>
        /// <remarks>
        /// Enabling this may sometimes result in performance drop on high Quality levels. It is better to
        /// turn it off if your shader doesn't uses normals.
        /// </remarks>
        bool SetTangents { get; }

        /// <summary>
        /// Gets a value indicating whether to calculate where the simulation field is obstructed with
        /// GameObjects with tag.
        /// </summary>
        /// <remarks>
        /// As the simulation field can only be of rectangular shape for now, 
        /// this can be used to simulate complex shapes such as pond banks.
        /// </remarks>
        bool UseObstructions { get; }

        /// <summary>
        /// Gets a value indicating how much the obstruction field data will eroded (expanded or shrinked).
        /// </summary>
        /// <remarks>
        /// This can be used for dealing with edge artifacts that can occur when using obstruction data.
        /// </remarks>
        int ObstructionDataErosion { get; }

        /// <summary>
        /// Gets a value indicating whether the obstruction field data will be baked into
        /// water mesh vertex colors.
        /// </summary>
        /// <remarks>
        /// In the vertex color, red channel corresponds to the additinal dampening in that point,
        /// where value of 255 means zero dampening and value of 1 means maximum dampening.
        /// 0 is a special value that corresponds to the situation when the vertex 
        /// is fully obstructed by obstruction geometry. In this case, a value of 255
        /// is additionaly written to the blue channel.
        /// You can use this data for more advanced shading, for example, discard fragments that are
        /// fully obstructed.
        /// </remarks>
        bool MeshBakeObstructionData { get; }

        /// <summary>
        /// Returns maximum resolution (along largest side) possible for this water plane dimensions.
        /// </summary>
        /// <returns>
        /// Maximum resolution (along largest side) possible for this water plane dimensions.
        /// </returns>
        int MaxResolution();

        /// <summary>
        /// Returns maximum wave propagation speed possible for this quality level.
        /// </summary>
        /// <returns>
        /// Maximum wave propagation speed possible for this quality level.
        /// </returns>
        float MaxSpeed();

        /// <summary>
        /// Gets the water plane BoxCollider
        /// </summary>
        BoxCollider PlaneCollider { get; }
    }

    /// <summary>
    /// Interface representing the public settings properties of <see cref="DynamicWater"/> class,
    /// required by <see cref="DynamicWaterMesh"/>.
    /// </summary>
    public interface IDynamicWaterMeshSettings {
        /// <summary>
        /// Gets a value indicating whether the water mesh normals should be calculated.
        /// </summary>
        bool CalculateNormals { get; }

        /// <summary>
        /// Gets a value indicating whether the fast approximate method of calculating the
        /// water mesh normals should be used instead of <c>Mesh.RecalculateNormals()</c>.
        /// </summary>
        /// <remarks>
        /// Enabling this could provide a huge performance boost with the cost of a bit degraded quality.
        /// Especially useful on mobile devices.
        /// </remarks>
        bool UseFastNormals { get; }

        /// <summary>
        /// Gets a value indicating whether the tangents must be set (usually for bump-mapped shaders).
        /// </summary>
        /// <remarks>
        /// Enabling this may sometimes result in performance drop on high Quality levels. It is better to
        /// turn it off if your shader doesn't uses normals.
        /// </remarks>
        bool SetTangents { get; }
    }

    /// <summary>
    /// Linear wave equation solver. This is the core of interactive simulation.
    /// </summary>
    public static class LinearWaveEqueationSolver {
        /// <summary>
        /// Represents the maximal time delta for wave simulation to not diverge.
        /// </summary>
        /// <remarks>
        /// The <c>timeDelta</c> parameter of <see cref="Solve"/> method must always be clamped to this value, otherwise undesired behaviour will occur, producing massive artifacts.
        /// </remarks>
        /// <seealso cref="Solve"/>
        public const float MaxDt = 1.412f;

        /// <summary>
        /// Performs the wave simulation step.
        /// </summary>
        /// <param name="field">
        /// Represents the current simulation state.
        /// </param>
        /// <param name="fieldNew">
        /// Represents the updated simulation state.
        /// </param>
        /// <param name="fieldSpeed">
        /// Represents the simulation state difference.
        /// </param>
        /// <param name="fieldObstruction">
        /// Array of <c>bool</c> indicating whether the water is obstructed by an object. \n
        /// <c>true</c> means the grid node is obstructed by an object, so the simulation is not updated;
        /// <c>false</c> means the grid node is not obstructed, and the simulation can proceed freely.
        /// </param>
        /// <param name="gridSize">
        /// Actual simulation grid resolution.
        /// </param>
        /// <param name="timeDelta">
        /// Time delta in seconds.
        /// </param>
        /// <param name="damping">
        /// Damping value. Must be clamped to the 0-1 range. 
        /// </param>
        /// <param name="maxValue">
        /// Value representing maximal absolute wave height.
        /// </param>
        public static void Solve(float[] field, float[] fieldNew, float[] fieldSpeed,
                                 byte[] fieldObstruction, Vector2Int gridSize, float timeDelta, float damping,
                                 out float maxValue) {
            maxValue = float.NegativeInfinity;

            bool isFieldObstructionNull = fieldObstruction == null;
            float obstructionValue = 1f;
            for (int j = 0; j < gridSize.y; j++) {
                int index = j * gridSize.x;
                for (int i = 0; i < gridSize.x; i++) {
                    // Not updating borders and obstructions
                    if (!(i <= 0 || j <= 0 || i >= gridSize.x - 1 || j >= gridSize.y - 1 || (!isFieldObstructionNull && fieldObstruction[index] == byte.MinValue))) {
                        // Obstruction value (0-1) determined by obstruction geometry and obstruction mask
                        if (!isFieldObstructionNull) {
                            obstructionValue = fieldObstruction[index] * FastFunctions.InvertedByteMaxValue;
                        }

                        float laplasian = (field[index - 1] +
                                           field[index + 1] +
                                           field[index + gridSize.x] +
                                           field[index - gridSize.x]) * 0.25f -
                                           field[index];

                        fieldSpeed[index] += laplasian * timeDelta;
                        fieldNew[index] = (field[index] + fieldSpeed[index]) * damping * obstructionValue;

                        float valueAbs;
                        if (fieldNew[index] > 0f) {
                            valueAbs = fieldNew[index];
                        } else {
                            valueAbs = -fieldNew[index];
                        }
                        if (valueAbs > maxValue) {
                            maxValue = valueAbs;
                        }
                    }

                    index++;
                }
            }
        }
    }
}