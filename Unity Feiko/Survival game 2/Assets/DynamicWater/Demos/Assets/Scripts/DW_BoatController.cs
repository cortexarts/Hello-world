using UnityEngine;

/// <summary>
/// Basic boat controller.
/// </summary>
[RequireComponent(typeof (Rigidbody))]
public class DW_BoatController : MonoBehaviour {
    public float MovementSpeed = 1400f;
    public float RotationSpeed = 20f;

    private void Update() {
        // Receiving the input
        Vector3 dir = Vector3.zero;
        #if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
            #if UNITY_3_5 && UNITY_ANDROID
                dir.x = Mathf.Clamp(-Input.acceleration.y * 2f, -1f, 1f);
                dir.z = 1f;
            #else
                dir.x = Mathf.Clamp(Input.acceleration.x * 2f, -1f, 1f);
                dir.z = 1f;
            #endif
        #else
            dir.x = Input.GetAxisRaw("Horizontal");
            dir.z = Input.GetAxisRaw("Vertical");
        #endif

        // Move backwards at half speed
        float speed = dir.z > 0f ? dir.z : dir.z * 0.5f;

        // Apply movement
        Vector3 force = new Vector3(transform.forward.x, 0f, transform.forward.z) * speed * MovementSpeed;
        GetComponent<Rigidbody>().AddForce(force * Time.deltaTime, ForceMode.VelocityChange);

        // Apply rotation
        GetComponent<Rigidbody>().AddTorque(0f, dir.x * RotationSpeed * Time.deltaTime, 0f, ForceMode.VelocityChange);
    }
}