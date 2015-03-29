#pragma strict

var MenuSkin : GUISkin;

//References
var player : GameObject;
var mainCamera : GameObject;
var arms : GameObject;

//Icons
var campfireIcon : Texture;
var tentIcon : Texture;
var spareIcon : Texture;
var spareIcon2 : Texture;
var spareIcon3 : Texture;
var spareIcon4 : Texture;

//Player prefabs
var campFire : GameObject;
var tent : GameObject;
var spare : GameObject;
var spare2 : GameObject;
var spare3 : GameObject;
var spare4 : GameObject;


private var showGUI : boolean = false;

private var invScript : Inventory;

function Start()
{
	invScript = GetComponent(Inventory);
}

function Update()
{
	if(Input.GetKeyDown("c"))
	{
		showGUI = !showGUI;
	}
	
	if(showGUI == true)
	{
		Time.timeScale = 0;
		player.GetComponent(FPSInputController).enabled = false;
		player.GetComponent(MouseLook).enabled = false;
		mainCamera.GetComponent(MouseLook).enabled = false;
		arms.GetComponent(PlayerControl).enabled = false;
	}
	
	if(showGUI == false)
	{
		Time.timeScale = 1;
		player.GetComponent(FPSInputController).enabled = true;
		player.GetComponent(MouseLook).enabled = true;
		mainCamera.GetComponent(MouseLook).enabled = true;
		arms.GetComponent(PlayerControl).enabled = true;
	}
}

function OnGUI()
{
	if(showGUI == true)
	{
		GUI.skin = MenuSkin;
			GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300));
				GUI.Box(Rect(0, 0, 300, 300), "Crafting System");
				
				if(GUI.Button(Rect(10, 50, 50, 50), GUIContent (campfireIcon, "Build a campfire")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						campFire.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
				if(GUI.Button(Rect(10, 120, 50, 50), GUIContent (tentIcon, "Build a tent")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						tent.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
						
				if(GUI.Button(Rect(10, 190, 50, 50), GUIContent (spareIcon, "Build a spare")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						spare.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
				//SECOND COLUMN!
				if(GUI.Button(Rect(100, 50, 50, 50), GUIContent (spareIcon2, "Build a spare!")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						spare.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
				if(GUI.Button(Rect(100, 120, 50, 50), GUIContent (spareIcon3, "Build spare 3!")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						spare.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
				if(GUI.Button(Rect(100, 190, 50, 50), GUIContent (spareIcon4, "Build spare 4!")))
				{
					if(invScript.wood >= 6 && invScript.stone >= 3)
					{
						spare.SetActive(true);
						invScript.wood -= 6;
						invScript.stone -= 3;
					}
				}
				
				GUI.Label(Rect(100, 250, 100, 40), GUI.tooltip);
				GUI.EndGroup();
	}
}




































