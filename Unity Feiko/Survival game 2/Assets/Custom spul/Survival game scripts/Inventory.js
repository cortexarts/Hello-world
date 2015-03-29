#pragma strict

var menuSkin : GUISkin;

var wood : int = 0;
var stone : int = 0;
var clay : int = 0;

var fish : int = 0;
var cookedFish : int = 0;

var bottle : int = 0;
var bottledWater : int = 0;

var bandage : int = 0;

var minimumVal : int = 0;

private var showGUI : boolean = false;

private var playerGUI : PlayerGUI;

function Start()
{
	playerGUI = GameObject.Find("First Person Controller").GetComponent(PlayerGUI);	
}

function Update()
{
	if(wood <= 0)
	{
		wood = minimumVal;
	}
	
	if(stone <= 0)
	{
		stone = minimumVal;
	}
	
	if(clay <= 0)
	{
		clay = minimumVal;
	}
	
	if(fish <= 0)
	{
		fish = minimumVal;
	}
	
	if(cookedFish <= 0)
	{
		cookedFish = minimumVal;
	}
	
	if(bottle <= 0)
	{
		bottle = minimumVal;
	}
	
	if(bottledWater <= 0)
	{
		bottledWater = minimumVal;
	}
	
	if(bandage <= 0)
	{
		bandage = minimumVal;
	}
	
	if(Input.GetKeyDown("i"))
	{
		showGUI = !showGUI;
	}
	
	if(showGUI == true)
	{
		Time.timeScale = 0;
		GameObject.Find("First Person Controller").GetComponent(FPSInputController).enabled = false;
		GameObject.Find("First Person Controller").GetComponent(MouseLook).enabled = false;
		GameObject.Find("Main Camera").GetComponent(MouseLook).enabled = false;
		GameObject.Find("FPSArms_Axe@Idle").GetComponent(PlayerControl).enabled = false;
		GameObject.Find("Main Camera").GetComponent(RayCastCollect).enabled = false;
	}
	
	if(showGUI == false)
	{
		Time.timeScale = 1;
		GameObject.Find("First Person Controller").GetComponent(FPSInputController).enabled = true;
		GameObject.Find("First Person Controller").GetComponent(MouseLook).enabled = true;
		GameObject.Find("Main Camera").GetComponent(MouseLook).enabled = true;
		GameObject.Find("FPSArms_Axe@Idle").GetComponent(PlayerControl).enabled = true;
		GameObject.Find("Main Camera").GetComponent(RayCastCollect).enabled = true;
	}
}

function OnGUI()
{
	if(showGUI == true)
	{
		GUI.skin = menuSkin;
			GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300));
				GUI.Box(Rect(0, 0, 300, 300), "Basic Inventory");
				
				//Resources collected
				GUI.Label(Rect(10, 50, 50, 50), "Wood");
				GUI.Box(Rect(60, 50, 20, 20), "" + wood);
				
				GUI.Label(Rect(90, 50, 50, 50), "Stone");
				GUI.Box(Rect(130, 50, 20, 20), "" + stone);
				
				GUI.Label(Rect(170, 50, 50, 50), "Clay");
				GUI.Box(Rect(200, 50, 20, 20), "" + clay);
				
				
				//Empty holders
				GUI.Label(Rect(10, 130, 50, 50), "Fish");
				GUI.Box(Rect(60, 130, 20, 20), "" + fish);
				
				GUI.Label(Rect(10, 150, 50, 50), "Bottle");
				GUI.Box(Rect(60, 150, 20, 20), "" + bottle);
				
				//Edable items
				GUI.Label(Rect(10, 190, 50, 50), "CFish");
				GUI.Box(Rect(60, 190, 20, 20), "" + cookedFish);
				if(GUI.Button(Rect(100, 190, 100, 20), "Eat Fish?"))
				{
					if(cookedFish >= 1)
					{
						cookedFish--;
						Eat();
					}
				}
				
				GUI.Label(Rect(10, 210, 50, 50), "BWater");
				GUI.Box(Rect(60, 210, 20, 20), "" + bottledWater);
				if(GUI.Button(Rect(100, 210, 100, 20), "Drink Water?"))
				{
					if(bottledWater >= 1)
					{
						bottledWater--;
						Drink();
					}
				}
				
				GUI.Label(Rect(10, 240, 50, 50), "Heal");
				GUI.Box(Rect(60, 240, 20, 20), "" + bandage);
				if(GUI.Button(Rect(100, 240, 100, 20), "Use Bandage?"))
				{
					if(bandage >= 1)
					{
						bandage--;
						Heal();
					}
				}
				GUI.EndGroup();
	}
}

function Eat()
{
	playerGUI.hungerBarDisplay += 0.1;
}

function Drink()
{
	playerGUI.thirstBarDisplay += 0.1;
}

function Heal()
{
	playerGUI.healthBarDisplay += 0.1;
}























































