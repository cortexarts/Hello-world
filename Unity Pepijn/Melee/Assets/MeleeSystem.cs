using UnityEngine;
using System.Collections;

public class MeleeSystem : MonoBehaviour {
	
	
	int Damage = 50;
	float Distance;
	
	void  Update (){
		if (Input.GetButtonDown("Fire1"))
		{
			RaycastHit hit;
			if (Physics.Raycast (transform.position, -Vector3.up, out hit))
			{
				Distance = hit.distance;
				hit.transform.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}