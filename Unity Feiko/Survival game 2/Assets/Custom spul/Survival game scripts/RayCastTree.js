#pragma strict

var rayLength = 10;

private var treeScript : TreeController;

private var playerAnim : PlayerControl;

function Update()
{
	var hit : RaycastHit;
	var fwd = transform.TransformDirection(Vector3.forward);
	
	if(Physics.Raycast(transform.position, fwd, hit, rayLength))
	{
		if(hit.collider.gameObject.tag == "Tree")
		{
			treeScript = GameObject.Find(hit.collider.gameObject.name).GetComponent(TreeController);
			playerAnim = GameObject.Find("FPSArms_Axe@Idle").GetComponent(PlayerControl);
			
			if(Input.GetButtonDown("Fire1") && playerAnim.canSwing == true)
			{
				treeScript.treeHealth -= 1;
			}
		}
	}
}