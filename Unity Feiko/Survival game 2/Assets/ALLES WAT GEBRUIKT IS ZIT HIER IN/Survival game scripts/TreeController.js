#pragma strict

var treeHealth : int = 5;

var logs : Transform;
var coconut : Transform;
var tree : GameObject;

var speed : int = 8;

function Start()
{
	tree = this.gameObject;
	GetComponent.<Rigidbody>().isKinematic = true;
}

function Update()
{
	if(treeHealth <= 0)
	{
		GetComponent.<Rigidbody>().isKinematic = false;
		GetComponent.<Rigidbody>().AddForce(transform.forward * speed);
		DestroyTree();
	}
}

function DestroyTree()
{
	yield WaitForSeconds(7);
	Destroy(tree);
	
	var position : Vector3 = Vector3(Random.Range(-1.0, 1.0), 0, Random.Range(-1.0, 1.0));
	Instantiate(logs, tree.transform.position + Vector3(0,0,0) + position, Quaternion.identity);
	Instantiate(logs, tree.transform.position + Vector3(2,2,0) + position, Quaternion.identity);
	Instantiate(logs, tree.transform.position + Vector3(5,5,0) + position, Quaternion.identity);
	
	Instantiate(coconut, tree.transform.position + Vector3(0,0,0) + position, Quaternion.identity);
	Instantiate(coconut, tree.transform.position + Vector3(2,2,0) + position, Quaternion.identity);
	Instantiate(coconut, tree.transform.position + Vector3(5,5,0) + position, Quaternion.identity);
	
}


