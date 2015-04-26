using LostPolygon.DynamicWaterSystem;
using UnityEngine;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// Simplifies getting the instance of DynamicWater object is currently in.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WaterDetector : MonoBehaviour, IDynamicWaterFieldState {
        private IDynamicWaterFluidVolume _water;
    
        /// <summary>
        /// The instance of DynamicWater object is currently in.
        /// </summary>
        public IDynamicWaterFluidVolume Water {
            get {
                return _water;
            }
        }
    
        /// <summary>
        /// Returns water level at the given position in simulation grid space.
        /// </summary>
        /// <param name="position">
        /// The position at which to query the water level.
        /// </param>
        /// <returns>
        /// The water level at the given position in world space.
        /// </returns>
        public float GetWaterLevel(Vector3 position) {
            return _water != null ? _water.GetWaterLevel(position) : float.NegativeInfinity;
        }
    
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
        public float GetWaterLevel(float x, float y, float z) {
            return _water != null ? _water.GetWaterLevel(x, y, z) : float.NegativeInfinity;
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
        /// The water level at the given position in world space.
        /// </returns>
        public float GetWaterLevel(float x, float z) {
            return _water != null ? _water.GetWaterLevel(x, z) : float.NegativeInfinity;
        }
    
        private void OnTriggerEnter(Collider otherCollider) {
            // Making sure the object we have entered is DynamicWater
            if (otherCollider.CompareTag(FluidVolume.DynamicWaterTagName)) {
                _water = otherCollider.gameObject.GetComponent<DynamicWater>();
            }
        }
    
        private void OnTriggerExit(Collider otherCollider) {
            // Making sure the object we have left is DynamicWater
            if (_water != null && otherCollider.CompareTag(FluidVolume.DynamicWaterTagName) &&
                otherCollider == _water.Collider) {
                _water = null;
            }
        }
    }
#if !UNITY_3_5
}
#endif