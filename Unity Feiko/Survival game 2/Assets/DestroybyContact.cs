using UnityEngine;
using System.Collections;

public class DestroybyContact : MonoBehaviour {
	
	public GameObject explosion;
	
	
	void OnTriggerEnter(Collider other) 
	{
		Destroy(other.gameObject);
		Destroy(gameObject);
	}
}