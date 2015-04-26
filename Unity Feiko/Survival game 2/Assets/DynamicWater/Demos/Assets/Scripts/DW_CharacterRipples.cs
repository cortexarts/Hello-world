using System.Collections;
using LostPolygon.DynamicWaterSystem;
using UnityEngine;

/// <summary>
/// Creates ripples when Character walks in the water.
/// </summary>
[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (WaterDetector))]
public class DW_CharacterRipples : MonoBehaviour {
    // Ripple parameters
    public float SplashRadius = 0.2f;
    public float SplashForce = 10f;
    public float JumpSplashRadius = 0.5f;
    public float JumpSplashForce = 2.5f;

    /// <summary>
    /// The ParticleSystem prefab that will be instantiated on impact
    /// </summary>
    public ParticleSystem JumpSplashPrefab;

    /// <summary>
    /// Minimum rigibody.velocity.y value to instantiate splash.
    /// </summary>
    public float SplashThreshold = 3f;

    /// <summary>
    /// The <c>AudioClip[]</c> array, from which one random sound will be played upon impact.
    /// </summary>
    public AudioClip[] SplashSounds;

    private WaterDetector _waterDetector;
    private CharacterController _controller;

    private bool _isSubmergedPrev = true;
    private bool _isSubmerged = true;
    private bool _splashAllowed = true;

    protected void FixedUpdate() {
        // Checking for WaterDetector
        if (_waterDetector == null) {
            _waterDetector = GetComponent<WaterDetector>() ?? gameObject.AddComponent<WaterDetector>();
        }

        // If we are in the water
        if (_waterDetector.Water != null) {
            IDynamicWaterSettings water = _waterDetector.Water as IDynamicWaterSettings;
            if (water == null) {
                return;
            }

            _controller = GetComponent<CharacterController>();

            // If we are actually submerged to a some extent
            float waterLevel = water.GetWaterLevel(transform.position);

            float min = _controller.bounds.center.y - _controller.height / 2f;
            float max = _controller.bounds.center.y + _controller.height / 2f;
            _isSubmerged = min < waterLevel && max > waterLevel;

            // Do not make ripples while standing
            if (_controller.velocity.sqrMagnitude > 0.5f && _isSubmerged) {
                water.CreateSplash(transform.position, SplashRadius, SplashForce * Time.deltaTime);
            }

            // Checking if we must make a splash 
            if (_splashAllowed && _isSubmerged && _isSubmerged != _isSubmergedPrev && !_controller.isGrounded && _controller.velocity.y < -SplashThreshold) {
                SpawnSplash(JumpSplashPrefab, water.PlaneCollider.ClosestPointOnBounds(transform.position));
                water.CreateSplash(transform.position, JumpSplashRadius, JumpSplashForce);

                // To make sure we are not making splashes too often
                _splashAllowed = false;
                StartCoroutine(AllowSplash());
            }

            _isSubmergedPrev = _isSubmerged;
        }
    }

    protected void SpawnSplash(ParticleSystem splashPrefab, Vector3 position) {
        if (splashPrefab == null) {
            return;
        }

        ParticleSystem splash = Instantiate(splashPrefab, position, Quaternion.Euler(-90f, 0f, 0f)) as ParticleSystem;
        if (splash != null) {
            Destroy(splash.gameObject, splash.duration);
        }

        // Playing the splash sound
        if (SplashSounds.Length > 0) {
            AudioSource.PlayClipAtPoint(SplashSounds[Random.Range(0, SplashSounds.Length)], position);
        }
    }

    protected IEnumerator AllowSplash() {
        // We can only make splashes after 0.1 second interbal
        yield return new WaitForSeconds(0.1f);
        _splashAllowed = true;
    }
}