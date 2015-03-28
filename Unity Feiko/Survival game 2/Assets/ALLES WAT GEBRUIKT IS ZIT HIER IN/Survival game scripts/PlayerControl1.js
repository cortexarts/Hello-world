#pragma strict

private var hasAxe : boolean = false;

private var canSwing : boolean = true;
private var isSwinging : boolean = false;
var swingTimer : float = 0.7;

private var controller : CharacterController;
private var playerGUI : PlayerGUI;

function Start()
{
	hasAxe = true;
	controller = GameObject.Find("First Person Controller").GetComponent(CharacterController);
	playerGUI = GameObject.Find("First Person Controller").GetComponent(PlayerGUI);
}

function Update()
{
	//If we aren't moving and if we aren't swinging, then we idle!
	
	if(controller.velocity.magnitude <= 0 && isSwinging == false)
	{
		GetComponent.<Animation>().Play("Idle");
		GetComponent.<Animation>()["Idle"].wrapMode = WrapMode.Loop;
		GetComponent.<Animation>()["Idle"].speed = 0.2;
	}
	
	//If we're holding shift and moving, then sprint!
	
	if(controller.velocity.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
	{
		GetComponent.<Animation>().Play("Sprint");
		GetComponent.<Animation>()["Sprint"].wrapMode = WrapMode.Loop;
	}
	
	//WOODCUTTING SECTION
	if(hasAxe == true && canSwing == true)
	{
		if(Input.GetMouseButtonDown(0))
		{
			//Stamina reduction applied to the PlayerGUI script
			playerGUI.staminaBarDisplay -= 0.1;
			
			//Swinging animation
			GetComponent.<Animation>().Play("Swing");
			GetComponent.<Animation>()["Swing"].speed = 2;
			isSwinging = true;
			canSwing = false;
		}
	}
	
	if(canSwing == false)
	{
		swingTimer -= Time.deltaTime;
	}
	
	if(swingTimer <= 0)
	{
		swingTimer = 1;
		canSwing = true;
		isSwinging = false;
	}
}

















