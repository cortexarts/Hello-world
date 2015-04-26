using UnityEngine;

/// <summary>
/// Sets the center of mass relative to the center of the collider.
/// </summary>
[RequireComponent(typeof (Rigidbody))]
public class DW_SetCenterOfMass : MonoBehaviour {
    public Vector3 CenterOfMassShift;

    private void Start() {
        GetComponent<Rigidbody>().centerOfMass = CenterOfMassShift;
    }

}