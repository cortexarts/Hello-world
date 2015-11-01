using UnityEngine;
using System.Collections;

public class DestroybyContact : MonoBehaviour {
	
	public GameObject explosion;
	
	
	void OnTriggerEnter(Collider other) 
	{
		Instantiate (explosion, other.gameObject.transform.position, other.gameObject.transform.rotation);
		Destroy(gameObject);
	}
}