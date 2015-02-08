#pragma strict

var Health = 100;

function Update ()
{
	if(Health <= 0)
	{
		Dead();
	}
}

function ApplyDammage (TheDammage : int)
{
	Health = Health - TheDammage;
}
function Dead ()
{
	Destroy (gameObject);
}