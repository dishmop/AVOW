using UnityEngine;
using System.Collections;

public class AVOWTutorialManager : MonoBehaviour {

	public static AVOWTutorialManager singleton = null;

	public float ctrlDistMovedThreshold;
	public float holdDistMovedThreshold;
	public float timeoutTime;
	public int thresholdInOutGapCount;
	

	enum State{
	
		kOff,
		kDebugPostFirstSquare,
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
//		kPressMouseHoldAndMove0,
//		kPressMouseHoldAndMove1,
//		kPressMouseMovedFar,
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
		kConnectonBarsSetup,
		kConnectionBarsWait,
		kConnectionBars1,
		kConnectionBars2,
		kBarsChangeShape,
		kCreateSecondSquare,
		kSecondSquareCreated,
		kTimeoutOnSecondSquare,
		kCreateSeriesSquare0,
		kCreateSeriesSquare1,
		kCreateSeriesSquare2,
		kCreateSeriesSquare3,
		kConstructedSeries,
		kCreateSeriesSquareLostGap,
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
//	bool hasDonePressMouseMovedFar;
	bool hasDoneIntro1;
//	bool hasDonePressMouseHoldAndMove0;
	bool hasDonePressMouseToOtherConnection;
	bool hasDoneInOutGapExplain;
	bool hasDoneReleasedOutside;
	bool hasDoneCreatedSeriesSquare0;
	bool hasDoneCreatedSeriesSquare1;
	
	public void Trigger(){
		textTriggerExtern = true;
	}
	
	public void StartTutorial(){
		state = State.kIntro0;
		//state = State.kDebugPostFirstSquare;
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


		AVOWConfig.singleton.tutDisableMouseButtton = true;
		AVOWConfig.singleton.tutDisableConnections = true;
		AVOWConfig.singleton.tutDisableUIButtons = true;
		AVOWConfig.singleton.tutDisable2ndBarConnections = true;
		AVOWConfig.singleton.tutDisable2ndComponentConnections = true;
		AVOWConfig.singleton.tutDisableBarConstruction = true;
		AVOWConfig.singleton.tutDisableComponentConstruction = true;
		
		hasDoneFindTheConnectionSetup0 = false;
//		hasDonePressMouseMovedFar = false;
		hasDoneIntro1 = false;
//		hasDonePressMouseHoldAndMove0 = false;
		hasDonePressMouseToOtherConnection = false;
		hasDoneInOutGapExplain = false;
		hasDoneReleasedOutside = false;
		hasDoneCreatedSeriesSquare0 = false;
		hasDoneCreatedSeriesSquare1 = false;

		inOutGapCount = thresholdInOutGapCount;
	}
	
	void SetupExitTutFlags(){
		AVOWConfig.singleton.tutDisableMouseButtton = false;
		AVOWConfig.singleton.tutDisableConnections = false;
		AVOWConfig.singleton.tutDisableUIButtons = false;
		AVOWConfig.singleton.tutDisable2ndBarConnections = false;
		AVOWConfig.singleton.tutDisable2ndComponentConnections = false;
		AVOWConfig.singleton.tutDisableBarConstruction = false;
		AVOWConfig.singleton.tutDisableComponentConstruction = false;
		

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
			case State.kDebugPostFirstSquare:{
				SetupInitialTutFlags();
				AVOWConfig.singleton.tutDisableMouseButtton = false;
				AVOWConfig.singleton.tutDisableConnections = false;
				AVOWConfig.singleton.tutDisable2ndBarConnections = false;
				
				AVOWUI.singleton.PlaceResistor(AVOWGraph.singleton.allNodes[0], AVOWGraph.singleton.allNodes[1]);
				state = State.kConnectonBarsSetup;
			
				break;
			}
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
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
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
					//state = State.kPressMouseHoldAndMove0;
					state = State.kPressMouseToOtherConnection;
				}
				break;
			}
			case State.kPressMouseWait:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableMouseButtton = false;
					SetTimerTrigger(10);
				}
				if (onTimeTrigger){
					state = State.kPressMouseWaitTooLong;
				}
				if (AVOWUI.singleton.GetUITool().IsHolding()){
			    	//state = State.kPressMouseHoldAndMove0;
					state = State.kPressMouseToOtherConnection;
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
//			case State.kPressMouseHoldAndMove0:{	
//				if (hasDonePressMouseMovedFar){
//					state = State.kPressMouseToOtherConnection;
//					break;
//				}
//				if (onEnterState){
//					AVOWTutorialText.singleton.InturruptText("Now, keep the mouse button held down and move far away from the connection point.");
//					SetTextTrigger(hasDonePressMouseHoldAndMove0);
//				}	
//				if (!AVOWUI.singleton.GetUITool().IsHolding()){
//					state = State.kPressMousePrematureRelease;
//				}
//				if (onTextTrigger){
//					hasDonePressMouseHoldAndMove0 = true;
//					state = State.kPressMouseHoldAndMove1;
//				}
//			    break;
//			}
//			case State.kPressMouseHoldAndMove1:{	
//				if (!AVOWUI.singleton.GetUITool().IsHolding()){
//					state = State.kPressMousePrematureRelease;
//				}
//				float sparkLen = (AVOWUI.singleton.GetUITool().GetConnection(0).transform.position - AVOWUI.singleton.GetUITool().GetCursorCube().transform.position).magnitude;
//				if (sparkLen > holdDistMovedThreshold){
//					state = State.kPressMouseMovedFar;
//				}
//				break;
//			}			
//			case State.kPressMouseMovedFar:{	
//				if (onEnterState){
//					AVOWTutorialText.singleton.InturruptText("Good.");
//					AVOWTutorialText.singleton.AddText("While the button is held, you will stay attached to your connection point even when far away.");
//					SetTextTrigger();
//				}
//				if (onTextTrigger){
//					hasDonePressMouseMovedFar = true;
//					state = State.kPressMouseToOtherConnection;
//				}
//				if (!AVOWUI.singleton.GetUITool().IsHolding()){
//					state = State.kPressMousePrematureRelease;
//				}
//				break;
//			}

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
					AVOWConfig.singleton.tutDisable2ndBarConnections = false;
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
					AVOWConfig.singleton.tutDisableBarConstruction = false;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
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
					AVOWConfig.singleton.tutDisableBarConstruction = true;
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kConnectonBarsSetup;
				}
				break;
			}
			case State.kConnectonBarsSetup:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("Find the two connection points again - see they have changed shape.");
					SetupConnectionTrigger();
					SetTextTrigger();
				}

				if (onTextTrigger){
					state = State.kConnectionBarsWait;
				}
				// Keep track even when not triggering
				CountConnectionTrigger();
				break;
			}	
			case State.kConnectionBarsWait:{
				if (onEnterState){
					SetTimerTrigger(timeoutTime);
				}
				if (CountConnectionTrigger() == 1){
					state = State.kConnectionBars1;
				}
				if (CountConnectionTrigger () == 2){
					state = State.kConnectionBars2;
				}
				if (onTimeTrigger){
					state = State.kConnectonBarsSetup;
				}
				break;
			}			
			case State.kConnectionBars1:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You found one, see if you can find the other.");
				}
				
				if (CountConnectionTrigger () == 2){
					state = State.kConnectionBars2;
				}
				break;
			}	
			case State.kConnectionBars2:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You found them both.");
					AVOWTutorialText.singleton.AddPause(2);
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kBarsChangeShape;
				}
				break;
			}
			case State.kBarsChangeShape:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("The connection points have become horizontal 'connection bars'.");
					AVOWTutorialText.singleton.AddPause(2);
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kCreateSecondSquare;
				}
				break;
			}
			case State.kCreateSecondSquare:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableBarConstruction = false;
					AVOWTutorialText.singleton.AddText("Try making a second resistance square between the two connection bars.");
					SetTimerTrigger(timeoutTime);
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 2){
					state = State.kSecondSquareCreated;
				}
				if (onTimeTrigger){
					state = State.kTimeoutOnSecondSquare;
				}
				break;
			}
			case State.kSecondSquareCreated:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableBarConstruction = true;
					AVOWTutorialText.singleton.InturruptText("Good - you've created your second resistance square.");
					AVOWTutorialText.singleton.AddPause (2);
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kCreateSeriesSquare0;
				}
				break;
			}
			case State.kTimeoutOnSecondSquare:{
				AVOWTutorialText.singleton.AddText("Go near a connection bar, press and hold the mouse button and place the loose spark over the other connection bar. Then move me inside the gap that has been created and release the mouse button.");
				state = State.kCreateSecondSquare;

				break;
			}
			case State.kCreateSeriesSquare0:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("As well as placing squares between two connection bars, you can also place them between a bar and a square that is connected to it.");
					SetTextTrigger(hasDoneCreatedSeriesSquare0);
				}
				if (onTextTrigger){
					hasDoneCreatedSeriesSquare0 = true;
					state = State.kCreateSeriesSquare1;
				}
				break;
			}
			case State.kCreateSeriesSquare1:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("Go near a connection bar, press and hold the mouse button - notice that some of the green spheres inside the resistance squares remain green whlie the others go black.");
					AVOWTutorialText.singleton.AddText("Place the loose spark over a lit green sphere.");
					AVOWConfig.singleton.tutDisable2ndComponentConnections = false;
					AVOWConfig.singleton.tutDisableComponentConstruction = false;
					SetTextTrigger(hasDoneCreatedSeriesSquare1);
				}
				if (onTextTrigger){
					hasDoneCreatedSeriesSquare1 = true;
					state = State.kCreateSeriesSquare2;
				}
				break;
			}	
			case State.kCreateSeriesSquare2:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableBarConstruction = false;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					state = State.kCreateSeriesSquare3;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kConstructedSeries;
				}
	
				break;
			}	
			case State.kCreateSeriesSquare3:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("Good, now move me inside the gap and release the mouse button to fix the square in place.");
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kConstructedSeries;
				}
				else if (AVOWUI.singleton.GetUITool().GetNumConnections() != 2){
					state = State.kCreateSeriesSquareLostGap;
				}

				break;
			}
			case State.kConstructedSeries:{
				if (onEnterState){
					AVOWTutorialText.singleton.InturruptText("You have made your third resistance square.");
				}
				break;
			}	
			case State.kCreateSeriesSquareLostGap:{
				AVOWTutorialText.singleton.InturruptText("You have lost the gap you made.");
				state = State.kCreateSeriesSquare1;		
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
		else if (AVOWUI.singleton.GetUITool().GetNumConnections() == 1){
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
		else{
			foundConnection1 = AVOWUI.singleton.GetUITool().GetConnection(0);
			foundConnection2 = AVOWUI.singleton.GetUITool().GetConnection(1);
			return 2;
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
