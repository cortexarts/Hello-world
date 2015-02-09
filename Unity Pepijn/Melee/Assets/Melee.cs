using UnityEngine;
using System.Collections;

public class Melee : MonoBehaviour {
	
	
	int TheDamage = 50;
	float Distance;
	float MaxDistance = 1.5f;
	int hit = RaycastHit;
	
	void  Update (){
		if (Input.GetButtonDown("Fire1"))
		{
		//ERROR
			RaycastHit hit;
			//END
			if (Physics.Raycast (transform.position, transform.TransformDirection(Vector3.forward), hit))
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