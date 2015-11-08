#pragma strict

class AutoQualityChooser extends MonoBehaviour{
	//minimal framerate (if current FPS is lower, quality should decrease immediately)
	public var minAcceptableFramerate:float = 30;
	//current quality (as text, visible in inspector)
	public var currentQuality:String;
	//current framerate (calculated while component is running)
	public var currentFramerate:float;
	//disable component if user changed quality manually (for example in menu)
	public var forceBestQualityOnStart:boolean=false;
	public var disableAfterManualQualityChange:boolean=true;
	public var disabled:boolean=false;
	public var verbose:boolean=false;
	//how many times per second framerate should be checked
	public var updateRate:float = 1.0;  // how much updates per second.
	//Guard avoiding changing quality backwards and forwards
	//If threshold is set to X, it means that quality won't increase until framerate
	//will be higher than minAcceptableFramerate+X
	public var threshold:float = 5;
	//current quality number
	private var currQuality:int;
	private var minThreshold:float;
	private var maxThreshold:float;
	private var previousQuality:int=-1;
	private var frameCount:int = 0;
	private var nextUpdate:float = 0.0;
	private var ignoreOneIteration:boolean=true;
	private var testIteration:boolean=false;
	
	function afterQualityChange(){
		//Does nothing by default. 
		//If you have menu allowing user to choose quality, you can set it's active value value here.
	}
	
	function Start () {
		minThreshold=threshold;
		if(forceBestQualityOnStart){
			QualitySettings.SetQualityLevel(QualitySettings.names.Length-1);
			currQuality = QualitySettings.GetQualityLevel();
			currentQuality=""+currQuality+" ("+QualitySettings.names[currQuality]+")";
			if(verbose)Debug.Log("Quality on start: "+currentQuality);
		}else{
			aproxQuality();
		}
		restartComponent();
		nextUpdate = Time.realtimeSinceStartup + 1.0/updateRate;
	}
	
	private function aproxQuality(){ // simplified function from Bootcamp demo
		var fillrate = SystemInfo.graphicsPixelFillrate;
		var shaderLevel = SystemInfo.graphicsShaderLevel;
		var videoMemory = SystemInfo.graphicsMemorySize;
		var processors = SystemInfo.processorCount;
		if (fillrate < 0){
			if (shaderLevel < 10) fillrate = 1000;
			else if (shaderLevel < 20) fillrate = 1300;
			else if (shaderLevel < 30) fillrate = 2000;
			else fillrate = 3000;
			if (processors >= 6) 	fillrate *= 3;
			else if (processors >= 3) fillrate *= 2;
			if (videoMemory >= 512) 	fillrate *= 2;
			else if (videoMemory <= 128) fillrate /= 2;
		}
		var fillneed : float = (Screen.width*Screen.height + 400*300) * (minAcceptableFramerate / 1000000.0);
		var levelmult : float[] = [5.0, 30.0, 80.0, 130.0, 200.0, 320.0];
		var level:int = 0;
		while ((level < QualitySettings.names.Length-1) && fillrate > fillneed * levelmult[level+1]) ++level;
		QualitySettings.SetQualityLevel(level);
		currQuality = QualitySettings.GetQualityLevel();
		currentQuality=""+currQuality+" ("+QualitySettings.names[currQuality]+")";
		if(verbose)Debug.Log("Quality on start: "+currentQuality);
	}
	
	public function restartComponent():void{
		threshold=minThreshold;
		maxThreshold=minThreshold;
		currQuality = QualitySettings.GetQualityLevel();
	}
	
	function Update () {
		frameCount++;
	    if (Time.realtimeSinceStartup > nextUpdate){
	    	nextUpdate = Time.realtimeSinceStartup + 1.0/updateRate;
	        currentFramerate = frameCount * updateRate;
	        frameCount = 0;
	    	if(threshold>minThreshold)threshold--;
	    	if(currQuality != QualitySettings.GetQualityLevel()){
	    		currQuality = QualitySettings.GetQualityLevel();
	    		currentQuality=""+currQuality+" ("+QualitySettings.names[currQuality]+")";
	    		if(disableAfterManualQualityChange){
	    			disabled=true;
	    			return;
	    		}
	    	}
	    	if(disabled){
				ignoreOneIteration=true;
				return;
			}
	    	currQuality = QualitySettings.GetQualityLevel();
	        if(ignoreOneIteration){
	        	ignoreOneIteration=false;
	        	return;
	        }
	        if(testIteration){
	        	testIteration=false;
	        	if(currentFramerate<minAcceptableFramerate){ //failed
	        		decreaseQuality();
	        		return;
	        	}else{
	        		//...
	        	}
	        }
	        
	        if(currentFramerate<minAcceptableFramerate){
	        	decreaseQuality();
	        }else if(currentFramerate-threshold>minAcceptableFramerate){
	        	increaseQuality();
	      	}
	    }
	}
	
	public function increaseQuality():void{
		changeQuality(1);   
	}
	
	public function decreaseQuality():void{
		changeQuality(-1);
	}
	
	private function changeQuality(amount:int):void{
		if(amount>0){
			if(currQuality+amount>=QualitySettings.names.Length)return;
		}else{
			if(currQuality+amount<0)return;
		}
		if(currQuality+amount==previousQuality){
			maxThreshold*=2;
	    	threshold=maxThreshold;
	    	minThreshold+=0.5;
		}
		previousQuality=currQuality;
		QualitySettings.SetQualityLevel(currQuality+amount);
		currQuality = QualitySettings.GetQualityLevel();
		currentQuality=""+currQuality+" ("+QualitySettings.names[currQuality]+")";
	    ignoreOneIteration=true;
	    if(amount>0){
	    	testIteration=true;
	    	if(verbose)Debug.Log("Quality increased to "+currQuality+", framerate: "+currentFramerate);
	    }else{
	    	if(verbose)Debug.Log("Quality decreased to "+currQuality+", framerate: "+currentFramerate);
	    }
	    afterQualityChange();
	}
	
}