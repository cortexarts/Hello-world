using System.Runtime.InteropServices;
using UnityEngine;

namespace LostPolygon.DynamicWaterSystem {
    /// <summary>
    /// Simple 2-dimensional integer vector
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Int {
        public int x;
        public int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Int"/> struct.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        public Vector2Int(int x, int y) {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Int"/> struct.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public Vector2Int(Vector2 value) {
            x = (int) value.x;
            y = (int) value.y;
        }

        public static implicit operator Vector2Int(Vector2 value) {
            return new Vector2Int((int) value.x, (int) value.y);
        }

        public static implicit operator Vector2(Vector2Int value) {
            return new Vector2(value.x, value.y);
        }

        public override string ToString() {
            return "{" + x + ", " + y + "}";
        }
    }

    /// <summary>
    /// Simple 3-dimensional integer vector
    /// </summary>
    public struct Vector3Int {
        public int x;
        public int y;
        public int z;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
        /// </summary>
        public Vector3Int(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3Int(Vector3 value) {
            return new Vector3Int(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString() {
            return "{" + x + ", " + y + ", " + z + "}";
        }
    }

    /// <summary>
    /// A structure representing the properties of Mesh object.
    /// </summary>
    internal struct MeshStruct {
        public int[] Triangles;
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector4[] Tangents;
        public Vector2[] UV;
        public Color32[] Colors32;
    }

    /// <summary>
    /// A collection of functions that work with colliders.
    /// </summary>
    public static class ColliderTools {
        public enum ColliderSpecificTestEnum {
            None,
            Terrain
        }

        private static readonly Vector3[] meshColliderCheckDirections = {
                Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
            };

        /// <summary>
        /// Checks whether the point is inside a collider.
        /// </summary>
        /// <remarks>
        /// This method can check against all types of colliders, 
        /// including <c>TerrainCollider</c> and concave <c>MeshCollider</c>.
        /// </remarks>
        /// <param name="collider">
        /// The collider to check against.
        /// </param>
        /// <param name="point">
        /// The point being checked.
        /// </param>
        /// <param name="specificTest">
        /// Defines a kind of specific collision test that must be done against <paramref name="collider"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="point"/> is inside the <paramref name="collider"/>, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsPointInsideCollider(Collider collider, Vector3 point, ColliderSpecificTestEnum specificTest = ColliderSpecificTestEnum.None) {
            RaycastHit hit;
            if (specificTest == ColliderSpecificTestEnum.None) {
#if !UNITY_FLASH
                if (collider is TerrainCollider) {
                    if (!collider.Raycast(new Ray(point, Vector3.up), out hit, collider.bounds.size.y)) {
                        return false;
                    }
                } else
#endif
                    if (collider is MeshCollider && !((MeshCollider) collider).convex) {
                        if (!IsPointInsideMeshCollider(collider, point)) {
                            return false;
                        }
                    } else {
                        Vector3 direction = collider.bounds.center - point;
                        float directionMagnitude = direction.sqrMagnitude;
                        if (directionMagnitude > 0.0001f &&
                            collider.Raycast(new Ray(point, direction.normalized), out hit, FastFunctions.FastSqrt(directionMagnitude))) {
                            return false;
                        }
                    }
            } else {
                if (specificTest == ColliderSpecificTestEnum.Terrain) {
                    if (!collider.Raycast(new Ray(point + Vector3.up * collider.bounds.size.y, Vector3.down), out hit, collider.bounds.size.y)) {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether the point is inside a MeshCollider.
        /// </summary>
        /// <param name="collider">
        /// Collider to check against.
        /// </param>
        /// <param name="point">
        /// Point being checked.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="point"/> is inside the <paramref name="collider"/>, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsPointInsideMeshCollider(Collider collider, Vector3 point) {
            Ray rayCast = new Ray();
            for (int i = 0; i < meshColliderCheckDirections.Length; i++) {
                Vector3 dir = meshColliderCheckDirections[i];
                rayCast.origin = point - dir * 1000f;
                rayCast.direction = dir;
                RaycastHit hit;
                if (collider.Raycast(rayCast, out hit, 1000f) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if point is at most <paramref name="tolerance"/> away from the collider edge.
        /// </summary>
        /// <param name="collider">
        /// The collider to check against.
        /// </param>
        /// <param name="point">
        /// Point being checked.
        /// </param>
        /// <param name="tolerance">
        /// Maximal distance
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="point"/> is inside the <paramref name="collider"/> 
        /// and at most <paramref name="tolerance"/> away from its edge, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsPointAtColliderEdge(Collider collider, Vector3 point, float tolerance) {
            RaycastHit hit;

            tolerance *= 0.71f; // Approximately 1/sqrt(2)
            Vector3 direction = collider.bounds.center - point;
            Vector3 directionNormalized = direction.normalized;

            bool result = direction != Vector3.zero &&
                          collider.Raycast(
                              new Ray(point - directionNormalized * tolerance, directionNormalized),
                              out hit, tolerance
                              );

            return result;
        }
    }

    public static class GridMath {
        public static void CalculateGrid(int resolution, Vector2 size, out Vector2Int grid, out float nodeSize) {
            nodeSize = Mathf.Max(size.x, size.y) / resolution;
            grid.x = Mathf.FloorToInt(size.x / nodeSize + 1);
            grid.y = Mathf.FloorToInt(size.y / nodeSize + 1);
        }
    }

    /// <summary>
    /// A collection of fast implementations of mathematical functions.
    /// </summary>
    public static class FastFunctions {
        public const float DoublePi = Mathf.PI * 2f;
        public const float HalfPi = Mathf.PI / 2f;
        public const float InvDoublePi = 1f / DoublePi;
        public const float Deg2Rad = 1f / 180f * Mathf.PI;
        public const float InvertedByteMaxValue = 1f / byte.MaxValue;

        /// <summary>
        /// The union of float and int sharing the same location in memory.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct FloatIntUnion {
            [FieldOffset(0)]
            public float f;
            [FieldOffset(0)]
            public int i;
        }

        /// <summary>
        /// Calculates the approximate value of sine function at a given angle.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Sin()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// The angle in radians.
        /// </param>
        /// <returns>
        /// The approximate value of sine function at angle <paramref name="x"/>.
        /// </returns>
        /// <seealso href="http://lab.polygonal.de/?p=205"/>
        public static float FastSin(float x) {
            float floorVal = (x + Mathf.PI) * InvDoublePi;
            int floor = floorVal >= 0 ? (int) floorVal : (int) (floorVal - 1);
            x = x - DoublePi * floor;

            if (x < 0) {
                x = 1.27323954f * x + 0.405284735f * x * x;
            } else {
                x = 1.27323954f * x - 0.405284735f * x * x;
            }

            return x;
        }

        /// <summary>
        /// Calculates the approximate value of cosine function at a given angle.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Cos()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// The angle in radians.
        /// </param>
        /// <returns>
        /// The approximate value of cosie function at angle <paramref name="x"/>.
        /// </returns>
        /// <seealso href="http://lab.polygonal.de/?p=205"/>
        public static float FastCos(float x) {
            x = HalfPi - x;
            float floorVal = (x + Mathf.PI) * InvDoublePi;
            int floor = floorVal >= 0 ? (int) floorVal : (int) (floorVal - 1);
            x = x - DoublePi * floor;

            if (x < 0) {
                x = 1.27323954f * x + 0.405284735f * x * x;
            } else {
                x = 1.27323954f * x - 0.405284735f * x * x;
            }

            return x;
        }

        /// <summary>
        /// Calculates the approximate inverse square root of a given value.
        /// </summary>
        /// <remarks>
        /// This function is much faster than calling <c>1/Mathf.Sqrt(x)</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// The input value.
        /// </param>
        /// <returns>
        /// The approximate value of inverse square root of <paramref name="x"/>.
        /// </returns>
        /// <seealso href="http://en.wikipedia.org/wiki/Fast_inverse_square_root"/>
        public static float FastInvSqrt(float x) {
            FloatIntUnion u;
            u.i = 0;
            u.f = x;
            float xhalf = 0.5f * x;
            u.i = 0x5f375a86 - (u.i >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f;
        }

        /// <summary>
        /// Calculates the approximate square root of a given value.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Sqrt()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// Input value.
        /// </param>
        /// <returns>
        /// The approximate value of square root of <paramref name="x"/>.
        /// </returns>
        public static float FastSqrt(float x) {
            FloatIntUnion u;
            u.i = 0;
            u.f = x;
            float xhalf = 0.5f * x;
            u.i = 0x5f375a86 - (u.i >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * x;
        }

        /// <summary>
        /// Calculates the approximate log2 of a given value.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Log()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// Input value.
        /// </param>
        /// <returns>
        /// The approximate value of log2 of <paramref name="x"/>.
        /// </returns>
        /// <seealso cref="http://code.google.com/p/fastapprox"/>
        public static float FastLog2(float x) {
            FloatIntUnion union;
            union.i = 0;
            union.f = x;

            float y = union.i;
            y *= 1.1920928955078125e-7f;
            return y - 126.94269504f;
        }

        /// <summary>
        /// Calculates the approximate value of 2^<paramref name="x"/>.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Pow()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// Input value.
        /// </param>
        /// <returns>
        /// The approximate value of 2^<paramref name="x"/>.
        /// </returns>
        /// <seealso cref="http://code.google.com/p/fastapprox"/>
        public static float FastPow2(float x) {
            float clip = (x < -126f) ? -126.0f : x;

            FloatIntUnion union;
            union.f = 0;
            union.i = (int) ((1 << 23) * (clip + 126.94269504f));

            return union.f;
        }

        /// <summary>
        /// Calculates the approximate value of <paramref name="a"/>^<paramref name="b"/>.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Pow()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="a">
        /// The base value.
        /// </param>
        /// <param name="b">
        /// The power value.
        /// </param>
        /// <returns>
        /// The approximate value of <paramref name="a"/>^<paramref name="b"/>.
        /// </returns>
        /// <seealso cref="http://code.google.com/p/fastapprox"/>
        public static float FastPow(float a, float b) {
            // FastPow2(b * FastLog2(a));
            FloatIntUnion union;
            union.i = 0;
            union.f = a;

            float y = union.i;
            y *= 1.1920928955078125e-7f;
            union.f = b * (y - 126.94269504f);

            float clip = (union.f < -126f) ? -126.0f : union.f;
            union.i = (int) ((1 << 23) * (clip + 126.94269504f));

            return union.f;
        }

        /// <summary>
        /// Calculates the approximate value of <paramref name="a"/>^<paramref name="b"/>.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Mathf.Pow()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="a">
        /// The base value.
        /// </param>
        /// <param name="b">
        /// The power value.
        /// </param>
        /// <returns>
        /// The approximate value of <paramref name="a"/>^<paramref name="b"/>.
        /// </returns>
        public static float FastPowInt(float a, int b) {
            float result = a;

            while (b > 0) {
                result *= a;
                b--;
            }

            return result;
        }

        /// <summary>
        /// Calculates the approximate magnitude of <c>Vector3</c>.
        /// </summary>
        /// <remarks>
        /// This function is much faster than <c>Vector3.magnitude</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="vector">
        /// Input vector.
        /// </param>
        /// <returns>
        /// The approximate magnitude of <see cref="vector"/>.
        /// </returns>
        public static float FastVector3Magnitude(Vector3 vector) {
            float magnitude = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;

            FloatIntUnion u;
            u.i = 0;
            u.f = magnitude;
            float xhalf = 0.5f * magnitude;
            u.i = 0x5f375a86 - (u.i >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * magnitude;
        }

        /// <summary>
        /// Checks whether the float value is NaN.
        /// </summary>
        /// <remarks>
        /// This function is much faster than calling <c>Math.IsNaN()</c>, especially on mobile devices.
        /// </remarks>
        /// <param name="x">
        /// The input value to be checked.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="x"/> is NaN, <c>false</c> otherwise.
        /// </returns>
        /// <seealso href="http://stackoverflow.com/questions/639010/how-can-i-compare-a-float-to-nan-if-comparisons-to-nan-always-return-false"/>
        public static bool FastIsNaN(float x) {
            FloatIntUnion union;
            union.i = 0;
            union.f = x;

            return ((union.i & 0x7F800000) == 0x7F800000) && ((union.i & 0x007FFFFF) != 0);
        }
    }

    /// <summary>
    /// Does nothing. Used for detecting the assembly recompile event.
    /// </summary>
    public class RecompiledMarker {
    }
}