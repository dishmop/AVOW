using UnityEngine;
using System.Collections;

public class AVOWTutorialManager : MonoBehaviour {

	public static AVOWTutorialManager singleton = null;

	public float ctrlDistMovedThreshold;
	public float holdDistMovedThreshold;
	public float timeoutTime;
	public int thresholdInOutGapCount;
	public float largeCubeMul;
	
	Vector3 cubeSize;
	

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
		kMakeFourthSquare,
		kConnectedToOldBarBottom,
		kConnectedToOldBarTop,
		kConnectedToNewBar,
		kFourthLetGo,
		kFourthDone,
		kCreateButton,
		kFreeformCreate,
		kLastSquareCreated,
		kSelectDestroy,
		kDestroySquarePrelude,
		kDestroySquare,
		kDestroySparkActive,
		kDestroyPressAndHold,
		kDestroySuccessful,
		kDestroyLostGap,
		kDestroyLostConnection,
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
	bool hasDoneIntro1;
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

		AVOWConfig.singleton.tutDisableMouseMove = true;
		AVOWConfig.singleton.tutDisableMouseButtton = true;
		AVOWConfig.singleton.tutDisableConnections = true;
		AVOWConfig.singleton.tutDisableCreateUIButton = true;
		AVOWConfig.singleton.tutDisableDestroyUIButton = true;
		AVOWConfig.singleton.tutDisable2ndBarConnections = true;
		AVOWConfig.singleton.tutDisable2ndComponentConnections = true;
		AVOWConfig.singleton.tutDisableBarConstruction = true;
		AVOWConfig.singleton.tutDisableComponentConstruction = true;
		
		hasDoneFindTheConnectionSetup0 = false;
		hasDoneIntro1 = false;
		hasDonePressMouseToOtherConnection = false;
		hasDoneInOutGapExplain = false;
		hasDoneReleasedOutside = false;
		hasDoneCreatedSeriesSquare0 = false;
		hasDoneCreatedSeriesSquare1 = false;

		inOutGapCount = thresholdInOutGapCount;
	}
	
	void SetupExitTutFlags(){
		AVOWConfig.singleton.tutDisableMouseMove = false;
		
		AVOWConfig.singleton.tutDisableMouseButtton = false;
		AVOWConfig.singleton.tutDisableConnections = false;
		AVOWConfig.singleton.tutDisableCreateUIButton = false;
		AVOWConfig.singleton.tutDisableDestroyUIButton = false;
		AVOWConfig.singleton.tutDisable2ndBarConnections = false;
		AVOWConfig.singleton.tutDisable2ndComponentConnections = false;
		AVOWConfig.singleton.tutDisableBarConstruction = false;
		AVOWConfig.singleton.tutDisableComponentConstruction = false;
		

	}	

	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (state == State.kOff){
			return;
		}
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
				AVOWConfig.singleton.tutDisableCreateUIButton = false;
				AVOWConfig.singleton.tutDisableMouseMove = false;
				cubeSize = AVOWUI.singleton.GetUITool().GetCursorCube().transform.localScale;
			
				AVOWUI.singleton.PlaceResistor(AVOWGraph.singleton.allNodes[0], AVOWGraph.singleton.allNodes[1]);
				state = State.kConnectonBarsSetup;
			
				break;
			}
			case State.kIntro0:{
				if (onEnterState){
					SetupInitialTutFlags();
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("I am cube.");
					AVOWTutorialText.singleton.AddPause(3);
					cubeSize = AVOWUI.singleton.GetUITool().GetCursorCube().transform.localScale;
					AVOWUI.singleton.GetUITool().GetCursorCube().transform.localScale = cubeSize * largeCubeMul;
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kIntro1;
				}

				break;
			}
			case State.kIntro1:{	
				if (onEnterState){
					AVOWConfig.singleton.tutDisableMouseMove = false;

					AVOWTutorialText.singleton.AddText("Move me around with your mouse.");
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
					AVOWTutorialText.singleton.InterruptText("Good.");
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
					AVOWTutorialText.singleton.InterruptText("You found one, see if you can find the other.");
				}
				
				if (CountConnectionTrigger () == 2){
					state = State.kFindTheConnection2;
				}
				break;
			}	
			case State.kFindTheConnection2:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You found them both.");
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
					AVOWConfig.singleton.tutDisableCreateUIButton = false;
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
					AVOWTutorialText.singleton.InterruptText("There is another spark coming from me which is not attached to anything. This is called the loose spark.");
					AVOWTutorialText.singleton.AddText("Try to connect the loose spark to the other connection point");
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
					AVOWConfig.singleton.tutDisable2ndBarConnections = false;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					state = State.kOpenGap;
				}
				break;
			}
			case State.kPressMousePrematureRelease:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You let go of the button.");
					state = State.kPressMouseSetup;
				}
				break;
			}
			case State.kOpenGap:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("Good. You have opened a gap in the wall.");
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
					AVOWTutorialText.singleton.InterruptText("You've lost the gap you made. Try to connect the loose spark to the other connection point.");
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
					AVOWTutorialText.singleton.InterruptText("You released the button when outside the gap. This cancels what you were doing.");
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
					AVOWTutorialText.singleton.InterruptText("You have made your first resistance square.");
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
					AVOWTutorialText.singleton.InterruptText("Find the two connection points again - see they have changed shape.");
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
					AVOWTutorialText.singleton.InterruptText("You found one, see if you can find the other.");
				}
				
				if (CountConnectionTrigger () == 2){
					state = State.kConnectionBars2;
				}
				break;
			}	
			case State.kConnectionBars2:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You found them both.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kBarsChangeShape;
				}
				break;
			}
			case State.kBarsChangeShape:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("The connection points have become 'connection bars'.");
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
					AVOWTutorialText.singleton.InterruptText("Good - you've created your second resistance square.");
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
					AVOWTutorialText.singleton.AddText("As well as placing squares between two connection bars, you can also place them between a bar and a resistance square that is attached to it.");
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
					AVOWTutorialText.singleton.AddText("Go near a connection bar, press and hold the mouse button - then connect the loose spark to a green sphere in the centre of a connected square. ");
					AVOWConfig.singleton.tutDisable2ndComponentConnections = false;
					AVOWConfig.singleton.tutDisableComponentConstruction = false;
					SetTextTrigger(hasDoneCreatedSeriesSquare1);
				}
				if (onTextTrigger){
					hasDoneCreatedSeriesSquare1 = true;
					state = State.kCreateSeriesSquare2;
				}
				// We may make the squre while in this state - wwant to make sure that they only create one
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					AVOWConfig.singleton.tutDisable2ndComponentConnections = true;
					AVOWConfig.singleton.tutDisable2ndBarConnections = true;
				}
				break;
			}	
			case State.kCreateSeriesSquare2:{
				if (onEnterState){
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
					AVOWTutorialText.singleton.InterruptText("Good, now move me inside the gap and release the mouse button to fix the square in place.");
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
					AVOWTutorialText.singleton.InterruptText("You have made your third resistance square.");
					AVOWTutorialText.singleton.AddText("In doing so, a new connection bar has been created.");
					AVOWConfig.singleton.tutDisable2ndComponentConnections = true;
					AVOWConfig.singleton.tutDisable2ndBarConnections = true;
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kMakeFourthSquare;					
				}
				break;
			}	
			case State.kCreateSeriesSquareLostGap:{
				AVOWTutorialText.singleton.InterruptText("You have lost the gap you made.");
				state = State.kCreateSeriesSquare1;		
				break;
			}
			
			case State.kMakeFourthSquare:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("Find the new connection bar and create a new resistance square attached to it.");
				}
				// if we have connected to something
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 1 && AVOWUI.singleton.GetUITool().IsHolding()){
					float voltage = AVOWUI.singleton.GetUITool().GetConnection(0).GetComponent<AVOWNode>().voltage;
					if (voltage < 0.001f){
						state = State.kConnectedToOldBarBottom;
					}
					else if (voltage > 0.999f){
						state = State.kConnectedToOldBarTop;
					}
					else{
						state = State.kConnectedToNewBar;
					}
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 4){
					state = State.kFourthDone;
				}
				break;
			}
			case State.kConnectedToOldBarBottom:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You've connected to the bar at the bottom, this is one of the old connection bars.");
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kMakeFourthSquare;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 4){
					state = State.kFourthDone;
				}
				break;
			}
			case State.kConnectedToOldBarTop:{
				if (onEnterState){
				AVOWTutorialText.singleton.InterruptText("You've connected to the bar at the top, this is one of the old connection bars.");
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kMakeFourthSquare;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 4){
					state = State.kFourthDone;
				}
				break;
			}
			case State.kConnectedToNewBar:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisable2ndComponentConnections = false;
					AVOWConfig.singleton.tutDisable2ndBarConnections = false;
					AVOWConfig.singleton.tutDisableBarConstruction = false;
					AVOWConfig.singleton.tutDisableComponentConstruction = false;
				
					AVOWTutorialText.singleton.InterruptText("Good, that's the new bar - make a resistance square by attaching the loose spark to something else.");
				}
				if (!AVOWUI.singleton.GetUITool().IsHolding() && AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kFourthLetGo;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 4){
					state = State.kFourthDone;
				}
				break;
			}
			case State.kFourthLetGo:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("Oops you let go.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kMakeFourthSquare;
				}

				break;
			}		
			case State.kFourthDone:{	
				if (onEnterState){
					AVOWConfig.singleton.tutDisableBarConstruction = true;
					AVOWConfig.singleton.tutDisableComponentConstruction = true;
					AVOWTutorialText.singleton.InterruptText("You have made your fourth resistance square.");
					AVOWTutorialText.singleton.AddText("Notice how the connection bar had to move and the other squares resized to accomodate it.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kCreateButton;
					
				}
				break;
			}
			case State.kCreateButton:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("On the left of the screen is number showing how many more squares you can create.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kFreeformCreate;
				}
				break;
			}
			case State.kFreeformCreate:{	
				if (onEnterState){
					AVOWTutorialText.singleton.AddText ("You can make one more - make it anywhere you like.");
					AVOWConfig.singleton.tutDisableBarConstruction = false;
					AVOWConfig.singleton.tutDisableComponentConstruction = false;
				}	
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 5){
					state = State.kLastSquareCreated;
				}
				break;
			}	
			case State.kLastSquareCreated:{	
				if (onEnterState){
				AVOWTutorialText.singleton.AddText ("OK - you've run out of squares to create.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kSelectDestroy;
				
				}
				
				break;
			}		
			case State.kSelectDestroy:{	
				if (onEnterState){
					AVOWConfig.singleton.tutDisableDestroyUIButton = false;
					AVOWTutorialText.singleton.AddText ("On the left, there is a green button labelled 'Destroy'. Click this to activate the Destruction tool.");
				}	
				if (AVOWUI.singleton.GetUIMode() == AVOWUI.ToolMode.kDelete){
				state = State.kDestroySquare;
				}
				break;
			}	
			case State.kDestroySquare:{	
				if (onEnterState){
					AVOWTutorialText.singleton.AddText ("Go near the middle of a resistance square until you see the green spark.");
				}	
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 1){
					state = State.kDestroySparkActive;
				}
				break;
			}
			case State.kDestroySparkActive:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("Now press and hold the mouse button.");
				}	
				if (AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kDestroyPressAndHold;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 0){
					state = State.kDestroyLostConnection;
				}
				break;
			}
			case State.kDestroyPressAndHold:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("You've started to destroy the square. Now move outside the gap that has been left and release the mouse button to complete its destruction.");
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() < 5 && !AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kDestroySuccessful;
				}	
				else if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kDestroyLostGap;
				}
				else if (AVOWUI.singleton.GetUITool().GetNumConnections() == 0){
					state = State.kDestroyLostConnection;
				}
				break;
			}
			case State.kDestroySuccessful:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("You have succesfully destroyed a resistance square.");
					AVOWTutorialText.singleton.AddText ("If you want to make more squares, click the blue Create button on the left or alternatvely stick with the Destruction tool.");
					AVOWTutorialText.singleton.AddText ("Just play around with what you have learnt. When you are finished click the Main Menu button. You can always play around more be selecting the Free Play option from the main menu.");
				}	
				if (AVOWUI.singleton.GetUITool().IsHolding()){
//					state = State.kDestroyPressAndHold;
				}
				break;
			}
			case State.kDestroyLostGap:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("You let go of the mouse button whilst inside the gap - this cancels the destruction.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kDestroySquare;
				}
				if (AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kDestroyPressAndHold;
				}
				break;
			}
			case State.kDestroyLostConnection:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("You've moved too far away and lost the green spark.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					state = State.kDestroySquare;
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 1){
					state = State.kDestroySparkActive;
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
		
		float useSize = Mathf.Lerp (largeCubeMul, 1, AVOWConfig.singleton.cubeToCursor.GetValue());

		if (AVOWUI.singleton.GetUITool() != null && AVOWUI.singleton.GetUITool().GetCursorCube() != null){
			AVOWUI.singleton.GetUITool().GetCursorCube().transform.localScale = useSize * cubeSize;
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
	
 	//	GUI.Box (new Rect(50, 50, 500, 30), "Tutorial: " + state.ToString() + " - " + AVOWGraph.singleton.GetNumConfirmedLoads().ToString());
	}
	
}
