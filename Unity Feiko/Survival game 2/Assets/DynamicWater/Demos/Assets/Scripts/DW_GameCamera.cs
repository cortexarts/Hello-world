// WowCamera.cs
// by Synthetique - [url]http://forum.unity3d.com/viewtopic.php?p=278801#278801[/url]

using UnityEngine;

public class DW_GameCamera : MonoBehaviour {
    public Transform Target;

    public float TargetHeight = 1.7f;
    public float Distance = 5.0f;
    public float OffsetFromWall = 0.1f;

    public float MaxDistance = 20;
    public float MinDistance = .6f;
    public float SpeedDistance = 5;

    public float AngleX = 0f;
    public float AngleY = 30f;

    public float XSpeed = 200.0f;
    public float YSpeed = 200.0f;

    public int YMinLimit = -40;
    public int YMaxLimit = 80;

    public int ZoomRate = 40;

    public float PositionDampening = 3.0f;
    public float RotationDampening = 3.0f;
    public float ZoomDampening = 5.0f;

    public LayerMask CollisionLayers = -1;

    private float _xDeg;
    private float _yDeg;
    private float _currentDistance;
    private float _desiredDistance;
    private float _correctedDistance;

    private Vector3 _targetPos;
    public bool RotateToTarget = true;

    private void Start() {
        _yDeg = AngleY;
        _xDeg = AngleX;

        _currentDistance = Distance;
        _desiredDistance = Distance;
        _correctedDistance = Distance;

        // Make the rigid body not change rotation 
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        Reposition();
    }

    public void Reposition() {
        _yDeg = AngleY;
        _xDeg = AngleX;
        _targetPos = transform.position;
    }

    private void LateUpdate() {
        if (!Target) {
            return;
        }

        UpdatePlayerFollow();
    }

    /** 
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */

    private void UpdatePlayerFollow() {
        // If either mouse buttons are down, let the mouse govern camera position 
        if (GUIUtility.hotControl == 0) {
            if (Application.platform != RuntimePlatform.Android && Input.GetMouseButton(1)) {
                _xDeg += Input.GetAxis("Mouse X") * XSpeed * 0.02f;
                _yDeg -= Input.GetAxis("Mouse Y") * YSpeed * 0.02f;
            }
                // otherwise, ease behind the target if any of the directional keys are pressed 
            else if (RotateToTarget) {
                float targetRotationAngle = Target.eulerAngles.y;
                float currentRotationAngle = transform.eulerAngles.y;
                _xDeg = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, RotationDampening * Time.deltaTime);
            }
        }

        //_targetPos = Vector3.Lerp(_targetPos, Target.position, Time.deltaTime * PositionDampening);
        _targetPos = Target.position;

        // calculate the desired distance 
        _desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * ZoomRate * Mathf.Abs(_desiredDistance) * SpeedDistance;
        _desiredDistance = Mathf.Clamp(_desiredDistance, MinDistance, MaxDistance);

        _yDeg = ClampAngle(_yDeg, YMinLimit, YMaxLimit);

        // set camera rotation 
        Quaternion rotation = Quaternion.Euler(_yDeg, _xDeg, 0);
        _correctedDistance = _desiredDistance;

        // calculate desired camera position
        Vector3 vTargetOffset = new Vector3(0, -TargetHeight, 0);
        Vector3 position = _targetPos - (rotation * Vector3.forward * _desiredDistance + vTargetOffset);

        // check for collision using the true target's desired registration point as set by user using height 
        RaycastHit collisionHit;
        Vector3 trueTargetPosition = new Vector3(_targetPos.x, _targetPos.y, _targetPos.z) - vTargetOffset;

        // if there was a collision, correct the camera position and calculate the corrected distance 
        bool isCorrected = false;
        if (Physics.Linecast(trueTargetPosition, position, out collisionHit, CollisionLayers.value)) {
            // calculate the distance from the original estimated position to the collision location,
            // subtracting out a safety "offset" distance from the object we hit.  The offset will help
            // keep the camera from being right on top of the surface we hit, which usually shows up as
            // the surface geometry getting partially clipped by the camera's front clipping plane.
            _correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - OffsetFromWall;
            isCorrected = true;
        }

        // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance 
        _currentDistance = !isCorrected || _correctedDistance > _currentDistance ? Mathf.Lerp(_currentDistance, _correctedDistance, Time.deltaTime * ZoomDampening) : _correctedDistance;

        // keep within legal limits
        _currentDistance = Mathf.Clamp(_currentDistance, MinDistance, MaxDistance);

        // recalculate position based on the new currentDistance 
        position = _targetPos - (rotation * Vector3.forward * _currentDistance + vTargetOffset);
        //position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * PositionDampening) - (rotation * Vector3.forward * _currentDistance + vTargetOffset);
        //Debug.Log(Vector3.Lerp(transform.position, Target.position, Time.deltaTime * PositionDampening));

        transform.rotation = rotation;
        transform.position = position;
        //transform.position = isCorrected ? position : position;
        // Vector3.Lerp(transform.position, position, Time.deltaTime * PositionDampening);
    }

    private static float ClampAngle(float angle, float min, float max) {
        if (angle < -360f) {
            angle += 360f;
        }

        if (angle > 360f) {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, min, max);
    }
}