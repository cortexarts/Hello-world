using LostPolygon.DynamicWaterSystem;
using UnityEngine;

/// <summary>
/// Creates a ParticleSystem and plays a sound when enters or exits the water.
/// </summary>
[RequireComponent(typeof (BuoyancyForce))]
public class DW_WaterSplash : MonoBehaviour {
    /// <summary>
    /// The ParticleSystem prefab that will be instantiated on impact.
    /// </summary>
    public ParticleSystem SplashPrefab;

    /// <summary>
    /// Minimum rigibody.velocity.y value to instantiate splash.
    /// </summary>
    public float SplashThreshold = 3.1f;

    /// <summary>
    /// The <c>AudioClip[]</c> array, from which one random sound will be played upon impact.
    /// </summary>
    public AudioClip[] SplashSounds;

    private IDynamicWaterSettings _water;

    /// <summary>
    /// Called when BuoyantObject enters the water.
    /// </summary>
    /// <param name="eventWater">
    /// The FluidVolume which the object has entered.
    /// </param>
    public void OnFluidVolumeEnter(IDynamicWaterFluidVolume eventWater) {
        _water = eventWater as IDynamicWaterSettings;
        if (_water == null) {
            return;
        }

        if (_water.PlaneCollider != null) {
            SpawnSplash(SplashPrefab, _water.PlaneCollider.ClosestPointOnBounds(transform.position));
        }
    }

    /// <summary>
    /// Called when BuoyantObject exits the water.
    /// </summary>
    /// <param name="eventWater">
    /// The FluidVolume which the object has left.
    /// </param>
    public void OnFluidVolumeExit(IDynamicWaterFluidVolume eventWater) {
        if (_water == null)
        {
            return;
        }

        if (_water.PlaneCollider != null)
        {
            SpawnSplash(SplashPrefab, _water.PlaneCollider.ClosestPointOnBounds(transform.position));
        }

        _water = null;
    }

    private void SpawnSplash(ParticleSystem splashPrefab, Vector3 position) {
        if (splashPrefab == null || Mathf.Abs(GetComponent<Rigidbody>().velocity.y) < SplashThreshold) {
            return;
        }

        // Spawning the splash particle system
        Quaternion rotation = Quaternion.Euler(_water.transform.rotation.eulerAngles.x - 90f,
                                               _water.transform.rotation.eulerAngles.y,
                                               _water.transform.rotation.eulerAngles.z);
        ParticleSystem splash = Instantiate(splashPrefab, position, rotation) as ParticleSystem;
        if (splash != null) {
            Destroy(splash.gameObject, splash.duration);
        }

        // Playing the splash sound
        if (SplashSounds.Length > 0) {
            AudioSource.PlayClipAtPoint(SplashSounds[Random.Range(0, SplashSounds.Length)], position);
        }
    }
}