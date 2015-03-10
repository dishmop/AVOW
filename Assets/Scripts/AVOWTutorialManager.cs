using UnityEngine;
using System.Collections;

public class AVOWTutorialManager : MonoBehaviour {

	public static AVOWTutorialManager singleton = null;

	public float distMovedThreshold;

	enum State{
	
		kOff,
		kIntro0,
		kIntro1,
		kIntro2,
		kFindTheConnection1,
		kFindTheConnection2,
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
	
	public void Trigger(){
		textTriggerExtern = true;
	}
	
	public void StartTutorial(){
		state = State.kIntro0;
	}
	
	
	
	void SetTimerTrigger(float durationSeconds){
		timeTrigger = Time.fixedTime + durationSeconds;
		hasTimeTriggered = false;
	}
	
	void SetTextTrigger(){
		hasTextTriggered = false;
		AVOWTutorialText.singleton.AddTrigger();
		
	}

	// Use this for initialization
	void Start () {
	
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
		
	
		switch (state){
			case State.kIntro0:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableConnections = true;
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("I am cube. Move me around with your mouse.");
					SetTextTrigger();
				}
				if (onTextTrigger){
				state = State.kIntro1;
				}
				break;
			}
			case State.kIntro1:{
				if (onEnterState){
					SetupMotionTrigger();
				}
				if (OnTestMotionTrigger()){
					state = State.kIntro2;
				}
				break;
			}
			case State.kIntro2:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("That feels pleasent. I can attach to special connection points. There are two hidden in the wall somewhere. See if you can find them.");
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kFindTheConnection1;
				}
				break;
			}			
			case State.kFindTheConnection1:{
				if (onEnterState){
					AVOWConfig.singleton.tutDisableConnections = false;
				}
				if (false){
					state = State.kFindTheConnection2;
				}
				break;
				}
			}
	
	}
	
	void SetupMotionTrigger(){
		lastCursorPos = AVOWUI.singleton.GetCursorCube().transform.position;
		cumDistMoved = 0;
		
	}
	
	bool OnTestMotionTrigger(){
		Vector3 newCursorPos = AVOWUI.singleton.GetCursorCube().transform.position;
		cumDistMoved += (lastCursorPos - newCursorPos).magnitude;
		lastCursorPos = newCursorPos;
		return (cumDistMoved > distMovedThreshold);
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void OnGUI(){
	
		GUI.Box (new Rect(50, 50, 500, 30), "Tutorial: " + state.ToString());
	}
	
}
