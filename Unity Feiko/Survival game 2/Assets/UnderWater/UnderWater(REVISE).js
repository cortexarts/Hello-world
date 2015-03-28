#pragma strict

var waterLevel : float;
var myParticles : ParticleSystem;
private var isUnderwater : boolean;
private var normalColor : Color;
private var underwaterColor : Color;
private var chMotor : CharacterMotor;

//NEW VARIABLES
private var canSwim : boolean = true;
private var underGround : boolean = false;
var groundLevel : float;

function Start () 
	{
		normalColor = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        underwaterColor = new Color (0.22f, 0.65f, 0.77f, 0.5f);
        chMotor = GetComponent(CharacterMotor);
        myParticles.Stop();
    }

function Update () 
	{
            if ((transform.position.y < waterLevel) != isUnderwater) 
			{
                isUnderwater = transform.position.y < waterLevel;
                if (isUnderwater) SetUnderwater ();
                if (!isUnderwater) SetNormal ();
            }
            
            //NEW
            if(transform.position.y < groundLevel)
            {
            	canSwim = true;
            	underGround = true;
            }
            
            else
            {
            	underGround = false;
            }
            
            if(isUnderwater && canSwim == true && underGround == false && Input.GetKey(KeyCode.E))
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0,-200, 0);
            }
            else
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0, 0, 0);
            }
            
            if(isUnderwater && canSwim == true && Input.GetKey(KeyCode.Q))
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0, 200, 0);
            }
     }

function SetNormal () 
		{
            RenderSettings.fogColor = normalColor;
            RenderSettings.fogDensity = 0.002f;
			chMotor.movement.gravity = 20;
            chMotor.movement.maxFallSpeed = 20;
            chMotor.movement.maxForwardSpeed = 6;
            chMotor.movement.maxSidewaysSpeed = 6;
            myParticles.Stop();
            canSwim = false;
        }
     
function SetUnderwater () 
		{
            RenderSettings.fogColor = underwaterColor;
            RenderSettings.fogDensity = 0.03f;
            chMotor.movement.gravity = 2;
            chMotor.movement.maxFallSpeed = 5;
            chMotor.movement.maxForwardSpeed = 4;
            chMotor.movement.maxSidewaysSpeed = 4;
            myParticles.Play();
        }