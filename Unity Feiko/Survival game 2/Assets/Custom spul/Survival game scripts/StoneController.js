#pragma strict

var stoneHealth : int = 5;

var rock : Transform;
var stone : GameObject;

function Start()
{
	stone = this.gameObject;
}

function Update()
{
	if(stoneHealth <= 0)
	{
		DestroyStone();
	}
}

function DestroyStone()
{
	Destroy(stone);
	
	var position : Vector3 = Vector3(Random.Range(-1.0, 1.0), 0, Random.Range(-1.0, 1.0));
	Instantiate(rock, stone.transform.position + Vector3(0, 0, 0) + position, Quaternion.identity);
	Instantiate(rock, stone.transform.position + Vector3(2, 2, 0) + position, Quaternion.identity);
	Instantiate(rock, stone.transform.position + Vector3(5, 5, 0) + position, Quaternion.identity);
}








































