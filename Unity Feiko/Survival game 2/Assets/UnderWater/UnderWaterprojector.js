#pragma strict

var waterLevel : float;
var myParticles : ParticleSystem;
private var isUnderwater : boolean;
private var normalColor : Color;
private var underwaterColor : Color;
private var chMotor : CharacterMotor;

function Start () 
	{
		normalColor = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        underwaterColor = new Color (0.22f, 0.65f, 0.77f, 0.5f);
        chMotor = GetComponent(CharacterMotor);
        myParticles.Stop();
        GameObject.Find("Blob Light Projector").GetComponent(Projector).enabled = false;
    }

function Update () 
	{
            if ((transform.position.y < waterLevel) != isUnderwater) 
			{
                isUnderwater = transform.position.y < waterLevel;
                if (isUnderwater) SetUnderwater ();
                if (!isUnderwater) SetNormal ();
            }
            
            if(isUnderwater && Input.GetKey(KeyCode.E))
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0,-200, 0);
            }
            else
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0, 0, 0);
            }
            
            if(isUnderwater && Input.GetKey(KeyCode.Q))
            {
            	GetComponent.<ConstantForce>().relativeForce = Vector3(0, 200, 0);
            }
     }

function SetNormal () 
		{
            RenderSettings.fogColor = normalColor;
            RenderSettings.fogDensity = 0.05f;
            GameObject.Find("Blob Light Projector").GetComponent(Projector).enabled = false;
        }
     
function SetUnderwater () 
		{
            RenderSettings.fogColor = underwaterColor;
            RenderSettings.fogDensity = 0.08f;
            chMotor.movement.gravity = 2;
            chMotor.movement.maxFallSpeed = 5;
            chMotor.movement.maxForwardSpeed = 4;
            chMotor.movement.maxSidewaysSpeed = 4;
            myParticles.Play();
            GameObject.Find("Blob Light Projector").GetComponent(Projector).enabled = true;
        }