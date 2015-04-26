using UnityEngine;
using LostPolygon.DynamicWaterSystem;

#if !UNITY_3_5
namespace LostPolygon.DynamicWaterSystem {
#endif
    /// <summary>
    /// The base class represents fluid volume. This class doesn't implements any kind of fluid surface simulation, 
    /// but is used for buoyancy force simulation. 
    /// </summary>
    /// <example>
    /// You can use this class to simulate only the buoyancy in case you don't need any visualization, for example,
    /// for simulating air balloons in the air.
    /// </example>
    /// \copydetails LostPolygon::DynamicWater::IDynamicWaterFluidVolumeProperties
    [AddComponentMenu("Lost Polygon/Dynamic Water System/Fluid Volume")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class FluidVolume : MonoBehaviour, IDynamicWaterFluidVolume {
        /// <summary>
        /// The tag name used by <see cref="DynamicWater"/> and <see cref="FluidVolume"/>.
        /// </summary>
        public const string DynamicWaterTagName = "DynamicWater";

        /// <summary>
        /// Gets or sets the size of simulation field in world units.
        /// </summary>
        public virtual Vector2 Size {
            get {
                return _size;
            }
            set {
                if (_size != value) {
                    _size.x = Mathf.Clamp(value.x, 0f, float.PositiveInfinity);
                    _size.y = Mathf.Clamp(value.y, 0f, float.PositiveInfinity);

                    PropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fluid density in kg/m^3.
        /// </summary>
        public float Density {
            get {
                return _density;
            }
            set {
                _density = Mathf.Clamp(value, 0f, 10000f);
            }
        }

        /// <summary>
        /// Gets or sets the depth of fluid volume and updates the collider accordingly.
        /// </summary>
        public virtual float Depth {
            get {
                return _depth;
            }
            set {
                _depth = Mathf.Clamp(value, 0f, 10000f);

                CreateCollider();
                UpdateCollider();
            }
        }

        /// <summary>
        /// Gets the fluid volume BoxCollider
        /// </summary>
        public virtual BoxCollider Collider {
            get {
                return gameObject.GetComponent<BoxCollider>();
            }
        }

        /* Property fields */

        [SerializeField]
        protected Vector2 _size = new Vector2(10f, 10f);

        [SerializeField]
        protected float _density = 1000;

        [SerializeField]
        protected float _depth = 10f;

        protected Transform _transform;
        protected BoxCollider _collider;

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
        public virtual float GetWaterLevel(float x, float z) {
            return float.PositiveInfinity;
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
        public virtual float GetWaterLevel(float x, float y, float z) {
            return float.PositiveInfinity;
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
        public virtual float GetWaterLevel(Vector3 position) {
            return float.PositiveInfinity;
        }

        /// <summary>
        /// Creates a circular drop splash on the fluid surface (if available).
        /// </summary>
        /// <param name="center">
        /// The center of the splash in local space.
        /// </param>
        /// <param name="radius">
        /// The radius of the splash.
        /// </param>
        /// <param name="force">
        /// The amount of force applied to create the splash.
        /// </param>
        public virtual void CreateSplash(Vector3 center, float radius, float force) {
            // Do nothing
        }

        /// <summary>
        /// Initializes the Collider and updates its bounds.
        /// </summary>
        /// <see cref="UpdateCollider"/>
        protected virtual void Initialize() {
            _transform = gameObject.GetComponent<Transform>();

            tag = DynamicWaterTagName;

            CreateCollider();
            UpdateCollider();
        }

        /// <summary>
        /// Updates the collider bounds according to the Size and Depth.
        /// </summary>
        protected virtual void UpdateCollider() {
            _collider.center = new Vector3(_size.x / 2f, _depth / 2f, _size.y / 2f);
            _collider.size = new Vector3(_size.x, _depth, _size.y);
        }

        /// <summary>
        /// Creates the Collider and updates its bounds.
        /// </summary>
        protected void CreateCollider() {
            _collider = gameObject.GetComponent<BoxCollider>();
            if (_collider == null) {
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            _collider.isTrigger = true;
            UpdateCollider();
        }

        protected virtual void PropertyChanged() {
            UpdateCollider();
        }

        private void Start() {
            Initialize();
        }

        private void OnDrawGizmos() {
            if (!Application.isEditor) {
                return;
            }

            Gizmos.DrawIcon(new Vector3(GetComponent<Collider>().bounds.center.x, transform.position.y + 0.1f, GetComponent<Collider>().bounds.center.z), "DynamicWater/FluidVolume.png");
            Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
            if (_collider != null) {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(new Vector3(Size.x / 2f, Depth / 2f, Size.y / 2f), new Vector3(Size.x, Depth, Size.y));
            }
        }
    }
#if !UNITY_3_5
}
#endif