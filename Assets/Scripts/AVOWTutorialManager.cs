using UnityEngine;
using System.Collections;

public class AVOWTutorialManager : MonoBehaviour {

	public static AVOWTutorialManager singleton = null;

	public float ctrlDistMovedThreshold;
	public float holdDistMovedThreshold;
	public float timeoutTime;
	public float thresholdInOutGapCount;
	

	enum State{
	
		kOff,
		kIntro0,
		kIntro1,
		kIntro2,
		kFindTheConnectionSetup0,
		kFindTheConnectionSetup1,
		kFindTheConnectionWait,
		kFindTheConnection1,
		kFindTheConnection2,
		kPressMouseSetup,
		kPressMouseWait,
		kPressMouseWaitTooLong,
		kPressMouseHoldAndMove0,
		kPressMouseHoldAndMove1,
		kPressMouseMovedFar,
		kPressMouseToOtherConnection,
		kPressMouseToOtherConnectionWait,
		kPressMousePrematureRelease,
		kOpenGap,
		kLostGap,
		kInOutGap,
		kInOutGapExplain,
		kWaitForConstruction,
		kReleasedOutside,
		kConstructed,
		kStop
	}
	
	
	State state = State.kOff;
	
	// Entering a new state
	State lastState = State.kOff;
	
	// Timer trigger
	bool hasTimeTriggered = true;
	float timeTrigger = 0;
		

	
	// Text trigger
	bool textTriggerExtern = false;
	bool hasTextTriggered = true;	
	
	
	// Bespoke trigger data
	
	// motion trigger
	Vector3 lastCursorPos;
	float cumDistMoved;
	
	
	// Connection triggers
	GameObject foundConnection1;
	GameObject foundConnection2;
	
	// GapIn out
	int inOutGapCount;
	bool lastIsInside;
	
	
	// The "Has done" flags
	bool hasDoneFindTheConnectionSetup0;
	bool hasDonePressMouseMovedFar;
	bool hasDoneIntro1;
	bool hasDonePressMouseHoldAndMove0;
//	bool hasDonePressMouseHoldAndMove1;
	bool hasDonePressMouseToOtherConnection;
	bool hasDoneInOutGapExplain;
	bool hasDoneReleasedOutside;
	
	public void Trigger(){
		textTriggerExtern = true;
	}
	
	public void StartTutorial(){
		state = State.kIntro0;
	}
	
	public void StopTutorial(){
		state = State.kStop;
	}
	
	
	
	public bool IsRunning(){
		return state != State.kOff;
	}
	
	
	void ForceTextCompletion(){
		AVOWTutorialText.singleton.ForceTextCompletion();
	}
	
	
	
	void SetTimerTrigger(float durationSeconds){
		timeTrigger = Time.fixedTime + durationSeconds;
		hasTimeTriggered = false;
	}
	
	// Set bypass to true if we don't actually want to wait for the text
	void SetTextTrigger(bool byPass){
		hasTextTriggered = false;
		textTriggerExtern = false;
		if (!byPass){		
			AVOWTutorialText.singleton.AddTrigger();
		}
		else{
			Trigger ();
		}
	}
	
	void SetTextTrigger(){
		SetTextTrigger(false);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	void SetupInitialTutFlags(){
		AVOWConfig.singleton.tutDisableButtton = true;
		AVOWConfig.singleton.tutDisableConnections = true;
		AVOWConfig.singleton.tutDisableUIButtons = true;
		AVOWConfig.singleton.tutDisableSecondConnections = true;
		AVOWConfig.singleton.tutDisableConstruction = true;
		
		hasDoneFindTheConnectionSetup0 = false;
		hasDonePressMouseMovedFar = false;
		hasDoneIntro1 = false;
		hasDonePressMouseHoldAndMove0 = false;
//		hasDonePressMouseHoldAndMove1 = false;
		hasDonePressMouseToOtherConnection = false;
		hasDoneInOutGapExplain = false;
		hasDoneReleasedOutside = false;

		inOutGapCount = 0;
		lastIsInside = false;
	}
	
	void SetupExitTutFlags(){
		AVOWConfig.singleton.tutDisableButtton = false;
		AVOWConfig.singleton.tutDisableConnections = false;
		AVOWConfig.singleton.tutDisableUIButtons = false;
		AVOWConfig.singleton.tutDisableSecondConnections = false;
		AVOWConfig.singleton.tutDisableConstruction = false;
		

	}	

	
	// Update is called once per frame
	void FixedUpdate () {
		// on entering state
		bool onEnterState = (state != lastState);
		lastState = state;

		// Time trigger
		bool onTimeTrigger = (!hasTimeTriggered && Time.fixedTime > timeTrigger);
		hasTimeTriggered = hasTimeTriggered || onTimeTrigger;
		
		// Text trigger
		bool onTextTrigger = (!hasTextTriggered && textTriggerExtern);
		hasTextTriggered = hasTextTriggered || onTextTrigger;
		
		// Skip over text
		if (Input.GetKeyDown (KeyCode.Space)){
			ForceTextCompletion();
		}
		
	
		switch (state){
			case State.kIntro0:{	
				SetupInitialTutFlags();
				AVOWTutorialText.singleton.AddPause(3);
				state = State.kIntro1;
				break;
			}
			case State.kIntro1:{	
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("I am cube. Move me around with your mouse.");
					SetTextTrigger(hasDoneIntro1);
				}
				if (onTextTrigger){
					hasDoneIntro1 = true;
					state = State.kIntro2;
				}
				break;
			}
			case State.kIntro2:{
				if (onEnterState){
					SetupMotionTrigger();
					SetTimerTrigger(timeoutTime);
				}
				if (OnTestMotionTrigger()){
					AVOWTutorialText.singleton.InturruptText("Good.");
					state = State.kFindTheConnectionSetup0;
				}
				if (onTimeTrigger){
					state = State.kIntro1;
				}
				break;
			}
			case State.kFindTheConnectionSetup0:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("I can attach to special connection points.");
					AVOWTutorialText.singleton.AddTextNoLine("There are two hidden in the wall.");
					SetTextTrigger(hasDoneFindTheConnectionSetup0);
				}
				if (onTextTrigger){
					state = State.kFindTheConnectionSetup1;
				}
				break;
			}			
			case State.kFindTheConnectionSetup1:{
				if (onEnterState){
					hasDoneFindTheConnectionSetup0 = true;
					AVOWConfig.singleton.tutDisableConnections = false;
					AVOWTutorialText.singleton.AddText(" See if you can find them.");
					SetupConnectionTrigger();
					SetTextTrigger(hasDoneFindTheConnectionSetup0);
				}
				if (onTextTrigger){
					state = State.kFindTheConnectionWait;
				}
				break;
			}
			case State.kFindTheConnectionWait:{
				if (onEnterState){
					SetTimerTrigger(timeoutTime);
				}
				if (CountConnectionTrigger() == 1){
					state = State.kFindTheConnection1;
				}
				if (CountConnectionTrigger () == 2){
					state = State.kFindTheConnection2;
				}
				if (onTimeTrigger){
					state = State.kFindTheConnectionSetup0;
				}
				break;
			}			
			case State.kFindTheConnection1:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You found one, see if you can find the other.");
				}
				
				if (CountConnectionTrigger () == 2){
					state = State.kFindTheConnection2;
				}
				break;
			}	
			case State.kFindTheConnection2:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You found them both.");
					AVOWTutorialText.singleton.AddPause(2);
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kPressMouseSetup;
				}
				break;
			}
			case State.kPressMouseSetup:{
				if (HasCreatedSquare()){
					state = State.kConstructed;
				}
				else if (onEnterState){
					AVOWTutorialText.singleton.AddText("Go near a connection point till you see the spark - then press and hold the mouse button.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kPressMouseWait;
				}
				if (AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMouseHoldAndMove0;
				}
				break;
			}
			case State.kPressMouseWait:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableButtton = false;
					SetTimerTrigger(10);
				}
				if (onTimeTrigger){
					state = State.kPressMouseWaitTooLong;
				}
				if (AVOWUI.singleton.GetUITool().IsHolding()){
			    	state = State.kPressMouseHoldAndMove0;
				}
				break;
			}	
			case State.kPressMouseWaitTooLong:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("Remember, the connection points are hidden in the wall.");
					state = State.kPressMouseSetup;
				}
				break;
			}
			case State.kPressMouseHoldAndMove0:{	
				if (hasDonePressMouseMovedFar){
					state = State.kPressMouseToOtherConnection;
					break;
				}
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("Now, keep the mouse button held down and move far away from the connection point.");
					SetTextTrigger(hasDonePressMouseHoldAndMove0);
				}	
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (onTextTrigger){
					hasDonePressMouseHoldAndMove0 = true;
					state = State.kPressMouseHoldAndMove1;
				}
			    break;
			}
			case State.kPressMouseHoldAndMove1:{	
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				float sparkLen = (AVOWUI.singleton.GetUITool().GetConnection(0).transform.position - AVOWUI.singleton.GetUITool().GetCursorCube().transform.position).magnitude;
				if (sparkLen > holdDistMovedThreshold){
					state = State.kPressMouseMovedFar;
				}
				break;
			}			
			case State.kPressMouseMovedFar:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("Good.");
					AVOWTutorialText.singleton.AddText("While the button is held, you will stay attached to your connection point even when far away.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					hasDonePressMouseMovedFar = true;
					state = State.kPressMouseToOtherConnection;
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				break;
			}

			case State.kPressMouseToOtherConnection:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("There is another spark coming from me which is not attached to anything. This is called the loose spark.");
					AVOWTutorialText.singleton.AddTextNoLine("Try to connect the loose spark to the other connection point");
					SetTextTrigger(hasDonePressMouseToOtherConnection);
				}	
				if (onTextTrigger){
					state = State.kPressMouseToOtherConnectionWait;	
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					state = State.kOpenGap;
				}
				break;
			}
			case State.kPressMouseToOtherConnectionWait:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText(" that's hidden in the wall.");
					AVOWConfig.singleton.tutDisableSecondConnections = false;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					state = State.kOpenGap;
				}
				
		
			
				break;
			}
			case State.kPressMousePrematureRelease:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You let go of the button.");
					state = State.kPressMouseSetup;
				}
				break;
			}
			case State.kOpenGap:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("Good. You have opened a gap in the wall.");
					SetTextTrigger();
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
				
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2){
					state = State.kLostGap;
				}
				if (onTextTrigger){
					state = State.kInOutGap;
				}
				break;
			}	
			case State.kLostGap:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You've lost the gap you made. Try to connect the loose spark to the other connection point.");
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					state = State.kOpenGap;
				}
				break;
			}
			case State.kInOutGap:{
				if (onEnterState && !OnInOutGapTrigger()){
					AVOWTutorialText.singleton.AddText("Keep the gap open and move me inside the gap then outside. Do this a few times.");
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2){
					state = State.kLostGap;
				}
				if (OnInOutGapTrigger()){
					state = State.kInOutGapExplain;
				}
				break;
			}	
			case State.kInOutGapExplain:{
				if (onEnterState){
					if (!hasDoneInOutGapExplain) AVOWTutorialText.singleton.AddText("When I'm inside the gap, a resistance square appears.");
					SetTextTrigger(hasDoneInOutGapExplain);
				}	
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2){
					state = State.kLostGap;
				}
				if (onTextTrigger){
					hasDoneInOutGapExplain = true;
					state = State.kWaitForConstruction;
				}
				break;	
			}
			case State.kWaitForConstruction:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("Go inside the gap and release the mouse button to fix the resistance square there.");
					AVOWConfig.singleton.tutDisableConstruction = false;
				}
				if (HasCreatedSquare()){
					state = State.kConstructed;
				}	
				else if (!AVOWUI.singleton.GetUITool().IsHolding()){
			    	state = State.kReleasedOutside;
				}
				else if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2){
					state = State.kLostGap;
				}
				break;
			}
			case State.kReleasedOutside:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You released the button when outside the gap. This cancels what you were doing.");
					SetTextTrigger(hasDoneReleasedOutside);
				}
				if (onTextTrigger){
					hasDoneReleasedOutside = true;
					state = State.kPressMouseSetup;
				}
				break;
			}
			case State.kConstructed:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You have made your first resistance square.");
				}
				break;
			}
			
		case State.kStop:{
				SetupExitTutFlags();
				AVOWTutorialText.singleton.ClearText();
				state = State.kOff;
			
				break;
			}		
			
		}
			
	}
	
	bool HasCreatedSquare(){
		return AVOWGraph.singleton.GetNumConfirmedLoads() > 0;
	}
	
	
	bool OnInOutGapTrigger(){
		if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2) return false;
		
		bool thisIsInside = AVOWUI.singleton.GetUITool().IsInsideGap();
		if (thisIsInside != lastIsInside){
			inOutGapCount++;
		}
		lastIsInside = thisIsInside;
		return (inOutGapCount > thresholdInOutGapCount);
	}
	

	
	void SetupConnectionTrigger(){
		foundConnection1 = null;
		foundConnection2 = null;
	}
	
	int CountConnectionTrigger(){
		if (foundConnection2 != null) return 2;
		
		if (AVOWUI.singleton.GetUITool().GetNumConnections() == 0){
			if (foundConnection1 != null) return 1;
			else return 0;
		}
		else{
			GameObject currentConnection = AVOWUI.singleton.GetUITool().GetConnection(0);
			if (foundConnection1 == null){
				foundConnection1 = currentConnection;
				return 1;
			}
			else{
				if (currentConnection == foundConnection1){
					return 1;
				}
				else{
					foundConnection2 = currentConnection;
					return 2;
				}
			}
		}
		
	}
		
	void SetupMotionTrigger(){
		lastCursorPos = AVOWUI.singleton.GetUITool().GetCursorCube().transform.position;
		cumDistMoved = 0;
		
	}
	
	bool OnTestMotionTrigger(){
		Vector3 newCursorPos = AVOWUI.singleton.GetUITool().GetCursorCube().transform.position;
		cumDistMoved += (lastCursorPos - newCursorPos).magnitude;
		lastCursorPos = newCursorPos;
		return (cumDistMoved > ctrlDistMovedThreshold);
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void OnGUI(){
	
		GUI.Box (new Rect(50, 50, 500, 30), "Tutorial: " + state.ToString() + " - " + AVOWGraph.singleton.GetNumConfirmedLoads().ToString());
	}
	
}
