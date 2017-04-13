/*

D4RK3 54B3R's automated SAM Site

This script uses the Guided Missiles Mod.

This script will automatically launch missiles when it determines that a target has been found.
If it sees a target and fires, it will send a signal via laser antenna to home to indicate that it has found enemies.
This script is meant to be run off of a timer whose actions do not include trigger now.


Setup:
1. This will automate either cruise missiles or torpedos, so long as you do not change their names. Don't do it.
2. Place a single target painter turret on the grid. This is what will trigger the missiles and what automates target painting.
3. Place a programmable block with this script in it on the grid
4. Place a timer block, with the name "Timer Block (SAM)"
	The actions of this timer block, from left to right, should be
	Programmable Block (of the script) -> Run
	This timer block -> Start
5. Run the programmable block. It will manage the timer block for you.

There seems to be some issues for when you manually control the target painter turret;
The missiles will fire at whatever you paint, which is good, but the target painter turret's AI seems to be disabled permanently afterwards.
So don't manually control target painter turret unless you're ready to replace it.

If you add or remove missiles to the grid, you must first recompile the script before running.
*/

public const double fireDelay = 1; //Minimum # of seconds between missile shots.
public const double idleDelay = 3; //

public const double deactivateCooldown = 10; //If the turret idles for 10 seconds, then return to idle state
public static double deactivateTimer = 0;

public SAMState state = SAMState.Idle;


public static double scriptTime = 0;
public int counter = 0;
public int numMissiles = 0;
public bool isSetup = false;
public string nameTag = "SAM";

public List<IMySmallMissileLauncher> missileList = new List<IMySmallMissileLauncher>();
public IMyLargeConveyorTurretBase targeter;
public static IMyTimerBlock myTimerBlock;

void Main(string arg){
	if(!isSetup){
		setup();
		
		myTimerBlock.SetValue("TriggerDelay", (float)idleDelay);
		myTimerBlock.ApplyAction("OnOff_On");
		myTimerBlock.ApplyAction("Start");
		
		return;
	}
	
	
	double deltaTime = Runtime.TimeSinceLastRun.TotalSeconds; 
	scriptTime += deltaTime;
	
	Echo("State: " + state);
	
	//check turret status.
	if(targeter.IsShooting){
		//if the turret is shooting, then missile launch and set state to active
		IMySmallMissileLauncher missile = missileList[counter];
		if(missile != null){
			missile.ApplyAction("ShootOnce");
		}
		
		counter++;
		if(counter >= numMissiles) counter = 0;
		
		if(state == SAMState.Idle){
			myTimerBlock.SetValue("TriggerDelay", (float)fireDelay);
			myTimerBlock.ApplyAction("OnOff_On");
			myTimerBlock.ApplyAction("Start");
		}
		
		deactivateTimer = scriptTime + deactivateCooldown;
		
		state = SAMState.Active;
		
	}
	
	if(state == SAMState.Active){
		//in active state, make sure timer is operating at fireDelay.
		//also 
		if(scriptTime > deactivateTimer){
			state = SAMState.Idle;
				
			myTimerBlock.SetValue("TriggerDelay", (float)idleDelay);
			myTimerBlock.ApplyAction("OnOff_On");
			myTimerBlock.ApplyAction("Start");
			
		}
	}
}

public void setup(){
	/*
	Clear lists
	*/
	missileList.Clear();
	myTimerBlock = null;
	targeter = null;
	isSetup = false;
	
	
	/*
	Locate references for...
	*/
	List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();	
	
	//Missiles
	GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(allBlocks); 
	foreach(IMySmallMissileLauncher missile in allBlocks){
		if(missile.CustomName.IndexOf("Cruise") >= 0 ){ 
			missileList.Add(missile);
			continue;
		}
		if(missile.CustomName.IndexOf("Torpedo") >= 0 ){ 
			missileList.Add(missile);
			continue;
		}
	}
	
	
	//Target Painter
	GridTerminalSystem.GetBlocksOfType<IMyLargeConveyorTurretBase>(allBlocks); 
	foreach(IMyLargeConveyorTurretBase turret in allBlocks){
		if(turret.CustomName.IndexOf("Targeter") >= 0 ){ 
			targeter = turret;
			break;
		}
	}
	
	//Timer
	GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(allBlocks); 
	foreach(IMyTimerBlock timer in allBlocks){
		if(timer.CustomName.IndexOf(nameTag) >= 0 ){ 
			myTimerBlock = timer;
			break;
		}
	}
	
	//Laser Antenna
	nameTag = "SAM";
	
	/*
	Confirm references
	*/
	if(missileList.Count == 0){
		Echo("::ERROR:: no missile launchers found");
		return;
	}
	
	if(myTimerBlock == null){
		Echo("::ERROR:: no timer block found");
		return;
	}
	
	if(targeter == null){
		Echo("::ERROR:: no target painter found");
	}
	
	missileList.Sort((gun1, gun2) => gun1.CustomName.CompareTo(gun2.CustomName));
	
	Echo("Setup complete");
	isSetup = true;
	counter = 0;
	numMissiles = missileList.Count;
}

public enum SAMState{
	Idle=0,
	Active
}