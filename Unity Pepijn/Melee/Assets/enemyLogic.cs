using UnityEngine;
using System.Collections;

public class enemyLogic : MonoBehaviour {
	

	int Health = 100;

	void  Update (){
		if(Health <= 0)
		{
			Dead();
		}
	}
	
	void  ApplyDamage ( int TheDamage  ){
		Debug.Log("in ApplyDamage");
		Health = Health - TheDamage;
	}
	void  Dead (){
		Destroy (gameObject);
	}
}