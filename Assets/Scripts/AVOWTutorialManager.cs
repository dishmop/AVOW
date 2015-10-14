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
		kPressMousePrematureRelease,
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
		kCreateSeriesSquare3_NodeNode,
		kConstructedSeries,
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
		kDestroySuccessful,
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
	public void GameUpdate () {
	
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
					AVOWTutorialText.singleton.AddText("I can attach to special connectors.");
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
					AVOWTutorialText.singleton.AddText("Go near a connector till you see the spark - then press and hold the mouse button.");
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
					AVOWConfig.singleton.tutDisableBarConstruction = false;
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

			case State.kPressMouseToOtherConnection:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("Go near the other connector and then release the button.");
					AVOWConfig.singleton.tutDisable2ndBarConnections = false;
				
				}	
				if (!AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kPressMousePrematureRelease;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
					state = State.kConstructed;
				}	

				break;
			}

			case State.kPressMousePrematureRelease:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You let go of the button without being near the other connector.");
					state = State.kPressMouseSetup;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
					state = State.kConstructed;
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
				state = State.kCreateSecondSquare;
				}
				break;
			}
			case State.kCreateSecondSquare:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableBarConstruction = false;
					AVOWTutorialText.singleton.AddText("Try making a second resistance square between the two connectors.");
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
				AVOWTutorialText.singleton.AddText("Go near a connector at the top or bottom of your existing square, press and hold the mouse button,move near the other connector and release.");
				state = State.kCreateSecondSquare;

				break;
			}
			case State.kCreateSeriesSquare0:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("As well as placing squares between two connectors, you can also place them between a connector and the small sparky rectangle in the centre of an existing square.");
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
					AVOWTutorialText.singleton.AddText("Go near a connector, press and hold the mouse button - then go near to the centre of an existing square and release.");
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
					state = State.kConstructedSeries;
				}	

				break;
			}	
			case State.kCreateSeriesSquare2:{
				if (onEnterState){
				}
				if (AVOWUI.singleton.GetUITool().GetNumConnections() == 2){
					if (!AVOWUI.singleton.GetUITool().GetConnection(1).GetComponent<AVOWComponent>()){
						state = State.kCreateSeriesSquare3_NodeNode;
					}
				}
				// We may make the squre while in this state - wwant to make sure that they only create one
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kConstructedSeries;
				}	
	
				break;
			}	
			case State.kCreateSeriesSquare3_NodeNode:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You tried to placed a square between two connectors - For the tutorial, you need to create one between a connector and a sparky rectangle in the centre of a resistance square.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kCreateSeriesSquare1;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kConstructedSeries;
				}
				break;
			}
				
			case State.kConstructedSeries:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("You have made your third resistance square.");
					AVOWTutorialText.singleton.AddText("In doing so, a new connector has been created.");
					AVOWConfig.singleton.tutDisable2ndComponentConnections = true;
					AVOWConfig.singleton.tutDisable2ndBarConnections = true;
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kMakeFourthSquare;					
				}
				break;
			}	
			
			case State.kMakeFourthSquare:{
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText("Find the new connector, which is in between the two smaller squares, and create another resistance square attached to it.");
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
					AVOWTutorialText.singleton.InterruptText("You've attached to the connector at the bottom, this is not the NEW connector.");
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
				AVOWTutorialText.singleton.InterruptText("You've attached to the connector at the top, this is not the NEW connector.");
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
				
					AVOWTutorialText.singleton.InterruptText("Good, that's the new connector - make a resistance square by attaching to something else.");
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
					AVOWTutorialText.singleton.AddText ("Now click the mouse button to destory the square.");
				}	
				if (AVOWGraph.singleton.GetNumConfirmedLoads() < 5 && !AVOWUI.singleton.GetUITool().IsHolding()){
					state = State.kDestroySuccessful;
				}	
				break;
			}

			case State.kDestroySuccessful:{	
				if (onEnterState){
					AVOWTutorialText.singleton.InterruptText ("You have succesfully destroyed a resistance square.");
					AVOWTutorialText.singleton.AddText ("You can continue to destroy, or if you want to make more squares, click the blue Create button on the left.");
					AVOWTutorialText.singleton.AddText ("This is the end of the tutorial. When you are finished click the Main Menu button. You can always play around more by selecting the Free Play option from the main menu.");
				}	
				if (AVOWUI.singleton.GetUITool().IsHolding()){
//					state = State.kDestroyPressAndHold;
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
	
// 		GUI.Box (new Rect(50, 50, 500, 30), "Tutorial: " + state.ToString() + " - " + AVOWGraph.singleton.GetNumConfirmedLoads().ToString());
	}
	
}
