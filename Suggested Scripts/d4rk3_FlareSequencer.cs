
/*
D4RK3 54B3R's flare sequencer, to be used with guided missiles mod.
This script controls the firing of flares to try to fool or distract incoming guided missiles.

How to setup for use:
1. Programmable block with this script
2. Flares! Do not touch the name of these flares
3. Grid needs a timer block, with name that contains the tag "Flare"
	This timer block's actions must run the programmable block with the arg "run"
	Additionally, this timer block must start itself and trigger itself.
	
	
To run this script, put it on the toolbar of your cockpit and select "run with argument".
This script can take one of two arguments:
- "single" makes it cycle once through all flares, and then stops itself afterwards
- "cont" is short for continuous. It will continually cycle through all flares.

To stop the script, just run it again with the same argument you ran it with the first time.
So if I pressed the button for run, with the arg "cont"... hitting that same button again will stop it.

If the grid is modified, like if additional flares are added, then the script must be recompiled in order to use the new flares.
*/

//Set this to however many flares you want to fire per second
public const double flaresPerSecond = 6; 


/* Don't touch anything below this */

public double scriptTime = 0;
public double flareTimer;
public const double flareDelay = 1 / flaresPerSecond;

public FlareState state = FlareState.Idle;

public string nameTag = "Flare";

public bool isSetup = false;

public int counter = 0;
public int length = 0;

public List<IMySmallMissileLauncher> flaresList = new List<IMySmallMissileLauncher>();
public static IMyTimerBlock myTimerBlock;

void Main(string arg){
	if(!isSetup){
		setup();
		state = FlareState.Idle;
		
		if(!isSetup){
			stopTimer();
			return;
		}
		Echo("Flares Ready");
	}
	
	/* time keeping */
	
	double deltaTime = Runtime.TimeSinceLastRun.TotalSeconds; 
	scriptTime += deltaTime;
	
	
	/* arg parsing and running actions */
	arg = arg.ToLower();
	
		
	if(arg == "run" && scriptTime > flareTimer){
		//Locate and fire the flare
		
		IMySmallMissileLauncher flare = flaresList[counter] as IMySmallMissileLauncher;
		
		if(flare != null){ //No delay for the next run IF no flare was fired! This can happen when flares are damaged/destroyed!
			flareTimer = scriptTime + flareDelay;
			flare.ApplyAction("ShootOnce");
		}
		
		Echo("Firing Flare #" + counter);
		
		counter++;
		if(state == FlareState.SingleSequence){
			if(counter >= length){
				state = FlareState.Idle;
				stopTimer();
			}
			
		}else if(state == FlareState.Continuous){
			if(counter >= length)
				counter = 0;
		}
		
		
	}else if(arg == "single"){
		if(state != FlareState.SingleSequence){
			state = FlareState.SingleSequence;
			startTimer();
			counter = 0;
			scriptTime = 0;
			flareTimer = 0;
			Echo("Flares Active");
			
		}else{
			state = FlareState.Idle;
			Echo("Flares Ready");
			stopTimer();
		}
		
		
	}else if(arg == "cont"){
		if(state != FlareState.Continuous){
			state = FlareState.Continuous;
			startTimer();
			scriptTime = 0;
			flareTimer = 0;
			Echo("Flares Active");
			
		}else{
			state = FlareState.Idle;
			Echo("Flares Ready");
			stopTimer();
		}
	}
	
	
}

public void setup(){
	flaresList.Clear();
	myTimerBlock = null;
	isSetup = false;
		
	List<IMyTerminalBlock> blocksList = new List<IMyTerminalBlock>();
	GridTerminalSystem.SearchBlocksOfName(nameTag, blocksList); 
	
	for (int j = 0; j < blocksList.Count; j++) { 
		IMyTerminalBlock block = blocksList[j] as IMyTerminalBlock; 
		
		if(block is IMyTimerBlock && myTimerBlock == null){
			myTimerBlock = block as IMyTimerBlock;
		}else if(block is IMySmallMissileLauncher){
			flaresList.Add(block as IMySmallMissileLauncher);
		}
	}
	

	
	if(myTimerBlock == null){
		Echo(":ERROR: COULDN'T FIND TIMER BLOCK");
		return;
	}
	
	
	if(flaresList.Count == 0){
		Echo(":ERROR: COULDN'T FIND FLARES");
		return;
	}
	
	flaresList.Sort((gun1, gun2) => gun1.CustomName.CompareTo(gun2.CustomName));
	
	Echo("Setup complete");
	isSetup = true;
	counter = 0;
	length = flaresList.Count;
	
}

public void startTimer(){
	if(myTimerBlock != null){
		myTimerBlock.ApplyAction("OnOff_On");
		myTimerBlock.ApplyAction("TriggerNow");
	}
}

public void stopTimer(){
	if(myTimerBlock != null){
		myTimerBlock.ApplyAction("Stop");
		myTimerBlock.ApplyAction("OnOff_Off");
	}
}

public enum FlareState{
	Idle=0,
	SingleSequence,
	Continuous
};