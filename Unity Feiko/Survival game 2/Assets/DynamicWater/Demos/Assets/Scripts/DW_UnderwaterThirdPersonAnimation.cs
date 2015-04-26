using UnityEngine;
using LostPolygon.DynamicWaterSystem;

/// <summary>
/// Sets the animation speed relative to the amount of volume submerged.
/// </summary>
[RequireComponent(typeof(DW_ThirdPersonController))]
public class DW_UnderwaterThirdPersonAnimation : MonoBehaviour {
    public float UnderwaterSpeedFactor = 0.4f;

    private CharacterController _controller;
    private DW_ThirdPersonController _thirdPersonController;
    private WaterDetector _waterDetector;

    private bool _isSubmerged = true;
    private float _initGravity;
    private float _initRunSpeed;
    private float _initWalkSpeed;
    private float _initTrotSpeed;
    private float _initRunMaxAnimationSpeed;
    private float _initWalkAnimationSpeed;
    private float _initJumpAnimationSpeed;
    private float _initLandAnimationSpeed;
    private float _initTrotAnimationSpeed;

    private float _lerpedSlowdownFactor;

    private void Start() {
        _thirdPersonController = GetComponent<DW_ThirdPersonController>();
        _initGravity = _thirdPersonController.Gravity;
        _initWalkSpeed = _thirdPersonController.WalkSpeed;
        _initRunSpeed = _thirdPersonController.RunSpeed;
        _initTrotSpeed = _thirdPersonController.TrotSpeed;
        _initWalkAnimationSpeed = _thirdPersonController.walkMaxAnimationSpeed;
        _initRunMaxAnimationSpeed = _thirdPersonController.runMaxAnimationSpeed;
        _initJumpAnimationSpeed = _thirdPersonController.jumpAnimationSpeed;
        _initLandAnimationSpeed = _thirdPersonController.landAnimationSpeed;
        _initTrotAnimationSpeed = _thirdPersonController.trotMaxAnimationSpeed;
    }

    private void Update() {
        // Checking for WaterDetector
        if (_waterDetector == null) {
            _waterDetector = GetComponent<WaterDetector>() ?? gameObject.AddComponent<WaterDetector>();
        }

        // If we are in the water
        if (_waterDetector != null) {
            _controller = GetComponent<CharacterController>();
            _thirdPersonController = GetComponent<DW_ThirdPersonController>();

            // If we are actually submerged to a some extent
            float waterLevel = _waterDetector.GetWaterLevel(transform.position);

            float min = _controller.bounds.center.y;
            float max = _controller.bounds.center.y + _controller.height / 2f;
            // Assume we are submerged when the character is at least half in the water.
            _isSubmerged = (min < waterLevel && max > waterLevel) || (min < waterLevel && max < waterLevel);
            // 1 when fully submerged, 0 when half submerged
            float submergedCoeff = Mathf.Clamp01((waterLevel - min) / (max - min));

            // Setting the speeds
            if (_isSubmerged) {
                float smoothSlowdownFactor = Mathf.Lerp(1f, UnderwaterSpeedFactor, submergedCoeff);

                _thirdPersonController.Gravity = _initGravity * smoothSlowdownFactor;
                _thirdPersonController.WalkSpeed = _initWalkSpeed * smoothSlowdownFactor;
                _thirdPersonController.RunSpeed = _initRunSpeed * smoothSlowdownFactor;
                _thirdPersonController.TrotSpeed = _initTrotSpeed * smoothSlowdownFactor;
                _thirdPersonController.walkMaxAnimationSpeed = _initWalkAnimationSpeed * smoothSlowdownFactor;
                _thirdPersonController.runMaxAnimationSpeed = _initRunMaxAnimationSpeed * smoothSlowdownFactor;
                _thirdPersonController.jumpAnimationSpeed = _initJumpAnimationSpeed * smoothSlowdownFactor;
                _thirdPersonController.landAnimationSpeed = _initLandAnimationSpeed * smoothSlowdownFactor;
                _thirdPersonController.trotMaxAnimationSpeed = _initTrotAnimationSpeed * smoothSlowdownFactor;
            } else {
                _thirdPersonController.Gravity = _initGravity;
                _thirdPersonController.WalkSpeed = _initWalkSpeed;
                _thirdPersonController.RunSpeed = _initRunSpeed;
                _thirdPersonController.TrotSpeed = _initTrotSpeed;
                _thirdPersonController.walkMaxAnimationSpeed = _initWalkAnimationSpeed;
                _thirdPersonController.runMaxAnimationSpeed = _initRunMaxAnimationSpeed;
                _thirdPersonController.jumpAnimationSpeed = _initJumpAnimationSpeed;
                _thirdPersonController.landAnimationSpeed = _initLandAnimationSpeed;
                _thirdPersonController.trotMaxAnimationSpeed = _initTrotAnimationSpeed;
            }
        }
    }
}