using UnityEngine;
using System.Collections;

			//if (Physics.Raycast (transform.position, transform.TransformDirection(Vector3.forward), hit))


public class Melee : MonoBehaviour {

	int TheDamage = 50;
	float Distance;
	float MaxDistance = 1.5f;

	void Update() {
		RaycastHit hit;

		if (Input.GetButtonDown("Fire1")){

			if (Physics.Raycast (transform.position, -Vector3.up, out hit))
			{
				Distance = hit.distance;
				if (Distance < MaxDistance) 
			{
			hit.transform.SendMessage("ApplyDamage", TheDamage, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}