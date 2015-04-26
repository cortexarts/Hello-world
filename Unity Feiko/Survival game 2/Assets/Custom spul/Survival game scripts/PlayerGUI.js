#pragma strict

//Size of Textures

var size : Vector2 = new Vector2(240, 40);

//Health Variables
var healthPos : Vector2 = new Vector2(20, 20);
var healthBarDisplay : float = 1;
var healthBarEmpty : Texture2D;
var healthBarFull : Texture2D;

//Hunger Variables
var hungerPos : Vector2 = new Vector2(20, 60);
var hungerBarDisplay : float = 1;
var hungerBarEmpty : Texture2D;
var hungerBarFull : Texture2D;

//Thirst Variables
var thirstPos : Vector2 = new Vector2(20, 100);
var thirstBarDisplay : float = 1;
var thirstBarEmpty : Texture2D;
var thirstBarFull : Texture2D;

//Stamina Variables
var staminaPos : Vector2 = new Vector2(20, 140);
var staminaBarDisplay : float = 1;
var staminaBarEmpty : Texture2D;
var staminaBarFull : Texture2D;

//Fall Rate
var healthFallRate : int = 150;
var hungerFallRate : int = 150;
var thirstFallRate : int = 100;
var staminaFallRate : int = 35;

private var chMotor : CharacterMotor;
private var controller : CharacterController;

var canJump : boolean = false;

var jumpTimer : float = 0.7;

function Start()
{
	chMotor = GetComponent(CharacterMotor);
	controller = GetComponent(CharacterController);
}

function OnGUI()
{
	//Health GUI
	GUI.BeginGroup(new Rect (healthPos.x, healthPos.y, size.x, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), healthBarEmpty);
	
	GUI.BeginGroup(new Rect (0, 0, size.x * healthBarDisplay, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), healthBarFull);
	
	GUI.EndGroup();
	GUI.EndGroup();
	
	//Hunger GUI
	GUI.BeginGroup(new Rect (hungerPos.x, hungerPos.y, size.x, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), hungerBarEmpty);
	
	GUI.BeginGroup(new Rect (0, 0, size.x * hungerBarDisplay, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), hungerBarFull);
	
	GUI.EndGroup();
	GUI.EndGroup();
	
	//Thirst GUI
	GUI.BeginGroup(new Rect (thirstPos.x, thirstPos.y, size.x, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), thirstBarEmpty);
	
	GUI.BeginGroup(new Rect (0, 0, size.x * thirstBarDisplay, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), thirstBarFull);
	
	GUI.EndGroup();
	GUI.EndGroup();
	
	//Stamina GUI
	GUI.BeginGroup(new Rect (staminaPos.x, staminaPos.y, size.x, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), staminaBarEmpty);
	
	GUI.BeginGroup(new Rect (0, 0, size.x * staminaBarDisplay, size.y));
	GUI.Box(Rect(0, 0, size.x, size.y), staminaBarFull);
	
	GUI.EndGroup();
	GUI.EndGroup();
}

function Update()
{
	//HEALTH CONTROL SECTION
	if(hungerBarDisplay <= 0 && (thirstBarDisplay <= 0))
	{
		healthBarDisplay -= Time.deltaTime / healthFallRate * 2;
	}
	
	else
	{
		if(hungerBarDisplay <= 0 || thirstBarDisplay <= 0)
		{
			healthBarDisplay -= Time.deltaTime / healthFallRate;
		}
	}
	
	if(healthBarDisplay <= 0)
	{
		CharacterDeath();
	}
	
	//HUNGER CONTROL SECTION
	if(hungerBarDisplay >= 0)
	{
		hungerBarDisplay -= Time.deltaTime / hungerFallRate;
	}
	
	if(hungerBarDisplay <= 0)
	{
		hungerBarDisplay = 0;
	}
	
	if(hungerBarDisplay >= 1)
	{
		hungerBarDisplay = 1;
	}
	
	//THIRST CONTROL SECTION
	if(thirstBarDisplay >= 0)
	{
		thirstBarDisplay -= Time.deltaTime / thirstFallRate;
	}
	
	if(thirstBarDisplay <= 0)
	{
		thirstBarDisplay = 0;
	}
	
	if(thirstBarDisplay >= 1)
	{
		thirstBarDisplay = 1;
	}
	
	//STAMINA CONTROL SECTION
	if(controller.velocity.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
	{
		chMotor.movement.maxForwardSpeed = 12;
		chMotor.movement.maxSidewaysSpeed = 10;
		staminaBarDisplay -= Time.deltaTime / staminaFallRate;
	}
	
	else
	{
		chMotor.movement.maxForwardSpeed = 6;
		chMotor.movement.maxSidewaysSpeed = 6;
		staminaBarDisplay += Time.deltaTime / staminaFallRate;
	}
	
	//JUMPING SECTION
	if(Input.GetKeyDown(KeyCode.Space) && canJump == true)
	{
		staminaBarDisplay -= 0.2;
		Wait();
	}
	
	if(canJump == false)
	{
		jumpTimer -= Time.deltaTime;
		chMotor.jumping.enabled = false;
	}
	
	if(jumpTimer <= 0)
	{
		canJump = true;
		chMotor.jumping.enabled = true;
		jumpTimer = 0.7;
	}
	
	//COMMENTED THESE SECTIONS OUT - UPDATED 16/07/14
	//if(staminaBarDisplay <= 0.05)
	//{
		//canJump = false;
		//chMotor.jumping.enabled = false;
	//}
	
	//else
	//{
		//canJump = true;
		//chMotor.jumping.enabled = true;
	//}
	
	if(staminaBarDisplay >= 1)
	{
		staminaBarDisplay = 1;
	}
	
	if(staminaBarDisplay <= 0)
	{
		//ADDED line 181 here!
		staminaBarDisplay = 0;
		canJump = false;
		chMotor.jumping.enabled = false;
		chMotor.movement.maxForwardSpeed = 6;
		chMotor.movement.maxSidewaysSpeed = 6;
	}
}

function CharacterDeath()
{
	Application.LoadLevel("SIMPLELEVEL");
}

function Wait()
{
	yield WaitForSeconds(0.1);
	canJump = false;
}





















































































