using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Analytics;

using Vectrosity;

public class Explanation : MonoBehaviour {
	public static Explanation singleton = null;

	public GameObject annotationPrefab;
	public GameObject sceneConstructor;
	public GameObject quarterColumn;
	public GameObject pusher;
	public GameObject objectiveBoard;
	
	public Material arrowMaterial;
	public Material dottedArrowMaterial;
	public Texture2D arrowFrontTex;
	public Texture2D arrowFrontBackTex;
	public Texture2D arrowBackTex;
	
	bool externalTextTrigger;
	bool hasTextTriggered;
	
	bool externalButtonTrigger;
	bool hasButtonTriggered;
	
	List<GameObject> annotations = new List<GameObject>();
	
	float timerStart;
	
	bool challengeContinueDone = false;
	GameObject challengeNode0GO;
	GameObject challengeNode1GO;
	GameObject challengeCell;
	GameObject[] challengeResistors = new GameObject[5];
	
	
	enum ChallengeMode{
		kNoMode,
		kClearGraph,
		kMakeGraph,
	}
	
	ChallengeMode challengeMode = ChallengeMode.kNoMode;

	public enum AnnotationState{
		kNone,
		kIndividual,
		kBattery,
		kWholeCircuit,
		kObjectiveGrid,
		kObjectiveFrames,
	};

	public AnnotationState annotationState = AnnotationState.kNone;
	AnnotationState lastAnnotationState = AnnotationState.kNone;
	
	public enum VizState{
		kError,
		kNormal,
		kCircuitOnly,
		kCircuitAndBatteryOnly,
		kCircuitAndBatteryAndMetalOnly,
		kCircuitAndBatteryAndMetalAndObjectivesOnly,
	}
	
	public VizState vizState = VizState.kNormal;
	
	public bool showAmps = false;
	public bool showOhms = false;
	public bool showArrowsOnBattery = false;
	public bool showArrowsOnLoads = false;
	public bool showLoadVoltages = true;
	
	VizState lastVizState = VizState.kError;
	
	public enum State{
		kOff,
		kIntro,
		kRemovingTheWorld,
		kMakeThree,
		kTradCircuit,
		kQuantities1,
		kQuantities2,
		kBoxes1,
		kBoxes2,
		kBoxesTotal,
		kBoxesSetupOne1,
		kBoxesSetupOne2,
		kOneCurrent,
		kLotsCurrent,
		kBoxesAreSquare,
		kShowWholeCircuit1,
		kShowWholeCircuit2,
		kKirchoffsLaws,
		kKirchoffsLawsBoxes,
		kBoxRewrite,
		kChallenge1,
		kChallenge1Solution,
		kChallenge2,
		kChallenge2Solution,		
		kChallengesComplete,
		kChallengesExplained,
		kObjectiveBoard,
		kObjectiveHeight,
		kObjectiveSquares,
		kObjectiveSquaresExplanation,
		kChallenge3,
		kChallenge3Solution,
		kFinish0,
		kFinish1,
	}
	
	public State state = State.kOff;
	State lastState = State.kOff;
	
	
	enum GridState{
		kNone,
		kDoGridCycle,
		kWait,
	}
	float gridStartTime = 0;
	GridState gridState = GridState.kNone;
	
	public Bounds GetBounds(){
		Bounds retBounds = new Bounds();
		foreach (Transform child in transform){
			if (child.gameObject.activeSelf && child.GetComponent<Renderer>() != null){
				retBounds.Encapsulate(child.GetComponent<Renderer>().bounds);
			}
		}
		return retBounds;
	}


	void HandleVizState(){
		
		// On change
		if (vizState != lastVizState){
			switch (vizState){
				case VizState.kNormal:{
					sceneConstructor.SetActive(true);
					quarterColumn.SetActive(true);
					pusher.transform.FindChild("Wall").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Charge").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Cylinder").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Sphere1").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Sphere2").gameObject.SetActive(true);
					objectiveBoard.SetActive(true);	
					AVOWConfig.singleton.showMetal = true;
					break;
				}
				case VizState.kCircuitOnly:{
					sceneConstructor.SetActive(false);
					quarterColumn.SetActive(false);
					pusher.transform.FindChild("Wall").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Charge").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Cylinder").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Sphere1").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Sphere2").gameObject.SetActive(false);
					objectiveBoard.SetActive(false);	
					AVOWConfig.singleton.showMetal = false;
					break;
				}
				case VizState.kCircuitAndBatteryOnly:{
					sceneConstructor.SetActive(false);
					quarterColumn.SetActive(false);
					pusher.transform.FindChild("Wall").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Charge").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Cylinder").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Sphere1").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Sphere2").gameObject.SetActive(false);
					objectiveBoard.SetActive(false);	
					AVOWConfig.singleton.showMetal = false;
					break;
				}				
				case VizState.kCircuitAndBatteryAndMetalOnly:{
					sceneConstructor.SetActive(false);
					quarterColumn.SetActive(false);
					pusher.transform.FindChild("Wall").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Charge").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Cylinder").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Sphere1").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Sphere2").gameObject.SetActive(false);
					objectiveBoard.SetActive(false);	
					AVOWConfig.singleton.showMetal = true;
					break;
				}		
				case VizState.kCircuitAndBatteryAndMetalAndObjectivesOnly:{
					sceneConstructor.SetActive(false);
					quarterColumn.SetActive(false);
					pusher.transform.FindChild("Wall").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Charge").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Cylinder").gameObject.SetActive(true);
					pusher.transform.FindChild("Battery").FindChild("Sphere1").gameObject.SetActive(false);
					pusher.transform.FindChild("Battery").FindChild("Sphere2").gameObject.SetActive(false);
					objectiveBoard.SetActive(true);	
					AVOWConfig.singleton.showMetal = true;
					break;
				}		
			}
			lastVizState = vizState;
		}
		
	}
	
	
	void HandleAnnotationState(){
		switch (annotationState){
			case AnnotationState.kNone:{
				foreach(GameObject go in annotations){
					GameObject.Destroy(go);
				}
				annotations.Clear();
				break;
			}
			case AnnotationState.kIndividual:{
				int count = 0;
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					bool top;
					bool right;
					bool bottom;
					bool left;
					
					if (IsOnEdge(component, out top, out right, out bottom, out left)){
						// If there are not enough entries in the annotation list
						if (annotations.Count() == count){
							GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
							newAnnotation.transform.SetParent(transform);
							annotations.Add(newAnnotation);
							
						}
						
						// Set the component
						Annotation thisAnnotation = annotations[count].GetComponent<Annotation>();
						if (thisAnnotation.componentGOs.Count() != 1){
							thisAnnotation.componentGOs.Clear();
							thisAnnotation.componentGOs.Add(go);
						}
						else{
							thisAnnotation.componentGOs[0] = go;
						}
						
						if (component.type == AVOWComponent.Type.kLoad){
						
							// Set up the volt annotation settings
							if (left && showLoadVoltages){
								thisAnnotation.voltState = Annotation.State.kLeftTop;
							}
							else if (right && showLoadVoltages){
								thisAnnotation.voltState = Annotation.State.kRightBottom;
							}
							else{
								thisAnnotation.voltState = Annotation.State.kDisabled;
							}
							
							// Set up the volt annotation settings
							if (bottom){
								thisAnnotation.ampState = showAmps ? Annotation.State.kRightBottom : Annotation.State.kDisabled;
							}
							else if (top){
								thisAnnotation.ampState = showAmps ? Annotation.State.kLeftTop : Annotation.State.kDisabled;
							}
							else{
								thisAnnotation.ampState = Annotation.State.kDisabled;
							}	
							
							// Set up the ohm annotation settings
							thisAnnotation.ohmState = showOhms ? Annotation.State.kLeftTop : Annotation.State.kDisabled;
							thisAnnotation.showArrows = showArrowsOnLoads;
						}
						else{
							thisAnnotation.voltState = Annotation.State.kRightBottom;
							thisAnnotation.ampState = Annotation.State.kDisabled;
							thisAnnotation.ohmState = Annotation.State.kDisabled;
							thisAnnotation.showArrows = showArrowsOnBattery;
						}
						++count;				
					}
					
				}		
				// If we have got more annotations that we haven't got to yet, then remove them.
				for (int i = count; i < annotations.Count(); ++i){
					GameObject.Destroy(annotations[i]);
				}
				annotations.RemoveRange(count, annotations.Count() - count);	
				break;
			}
			case AnnotationState.kBattery:{
				if (annotations.Count() > 1){
					foreach (GameObject go in annotations){
						GameObject.Destroy(go);
					}
					annotations.Clear();
				}
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					if (component.type == AVOWComponent.Type.kVoltageSource){
						if (annotations.Count() == 0){
							GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
							newAnnotation.transform.SetParent(transform);
							annotations.Add(newAnnotation);
							
						}
						
						// Set the component
						Annotation thisAnnotation = annotations[0].GetComponent<Annotation>();
						if (thisAnnotation.componentGOs.Count() != 1){
							thisAnnotation.componentGOs.Clear();
							thisAnnotation.componentGOs.Add(go);
						}
						else{
							thisAnnotation.componentGOs[0] = go;
						}
						
						thisAnnotation.voltState = Annotation.State.kRightBottom;
						thisAnnotation.ampState = Annotation.State.kDisabled;
					}
				}
				
				break;
			}
			case AnnotationState.kWholeCircuit:{
				if (annotations.Count() != 2){
					foreach (GameObject go in annotations){
						GameObject.Destroy(go);
					}
					annotations.Clear();
					for (int i = 0; i < 2; ++i){
						GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
						newAnnotation.transform.SetParent(transform);
						annotations.Add(newAnnotation);
					}
				}
				Annotation thisAnnotation = annotations[0].GetComponent<Annotation>();
				GameObject cell = null;
				if (thisAnnotation.componentGOs.Count() != AVOWGraph.singleton.allComponents.Count() -1){
					thisAnnotation.componentGOs.Clear();
					foreach (GameObject go in  AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							thisAnnotation.componentGOs.Add (go);
						}
						else{
							cell = go;
						}
					}
				}
				else{
					int count = 0;
					foreach (GameObject go in AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							thisAnnotation.componentGOs[count++] = go;
						}
						else{
							cell = go;
						}
					}		
				}
				thisAnnotation.voltState = Annotation.State.kLeftTop;
				thisAnnotation.ampState = showAmps ? Annotation.State.kRightBottom : Annotation.State.kDisabled;
				thisAnnotation.ohmState = showOhms ? Annotation.State.kLeftTop : Annotation.State.kDisabled;
			
			
			Annotation cellAnnotation = annotations[1].GetComponent<Annotation>();
				if (cellAnnotation.componentGOs.Count() != 1){
					cellAnnotation.componentGOs.Clear ();
					cellAnnotation.componentGOs.Add (cell);
				}
				else{
					cellAnnotation.componentGOs[0] = cell;
				}
				cellAnnotation.voltState = Annotation.State.kRightBottom;
				cellAnnotation.ampState = Annotation.State.kDisabled;
				break;
			}
			case AnnotationState.kObjectiveGrid:{
				int divNumber = 1;
				AVOWCircuitTarget target = AVOWObjectiveManager.singleton.GetCurrentTarget();
				if (target != null){
					divNumber = target.lcm;
				}
				int numAnnotation = (target == null) ? 1 : 2 * divNumber ;
				if (AVOWObjectiveManager.singleton.IsWaitingOnManualTrigger()){
					if (annotations.Count() != numAnnotation || lastAnnotationState != annotationState){
						foreach (GameObject go in annotations){
							GameObject.Destroy(go);
						}
						annotations.Clear();
						for (int i = 0; i < divNumber; ++i){
							GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
							newAnnotation.transform.SetParent(transform);
							float heightPos = 0.5f * ((float)i/(float)divNumber + (float)(i+1)/(float)divNumber);
							newAnnotation.transform.position = new Vector3(-0.1f, heightPos, transform.position.z);
							newAnnotation.GetComponent<Annotation>().SetVoltage(1f / divNumber);
							newAnnotation.GetComponent<Annotation>().voltState = Annotation.State.kLeftTop;
							newAnnotation.GetComponent<Annotation>().showArrows = true;
							newAnnotation.GetComponent<Annotation>().ampState = Annotation.State.kDisabled;
							newAnnotation.GetComponent<Annotation>().ohmState = Annotation.State.kDisabled;
							annotations.Add(newAnnotation);
						}
						for (int i = 0; i < divNumber && target != null; ++i){
							GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
							newAnnotation.transform.SetParent(transform);
							float horizontalPos = 0.5f * ((float)i/(float)divNumber + (float)(i+1)/(float)divNumber);
							//float horizontalPos = (float)i/(float)divNumber;
							newAnnotation.transform.position = new Vector3(-0.25f - horizontalPos, -0.15f, transform.position.z);
							newAnnotation.GetComponent<Annotation>().SetCurrent(1f / divNumber);
							newAnnotation.GetComponent<Annotation>().ampState = Annotation.State.kLeftTop;
							newAnnotation.GetComponent<Annotation>().showArrows = true;
							newAnnotation.GetComponent<Annotation>().voltState = Annotation.State.kDisabled;
							newAnnotation.GetComponent<Annotation>().ohmState = Annotation.State.kDisabled;
							annotations.Add(newAnnotation);
						}						
					}
				}
				//Debug.Log ("divNumber = " + divNumber);
				break;
			}
			case AnnotationState.kObjectiveFrames:{
				AVOWCircuitTarget target = AVOWObjectiveManager.singleton.GetCurrentTarget();
				int numAnnotation = target.componentDesc.Count();
				
				if (AVOWObjectiveManager.singleton.IsWaitingOnManualTrigger()){
					if (annotations.Count() != numAnnotation || lastAnnotationState != annotationState){
						foreach (GameObject go in annotations){
							GameObject.Destroy(go);
						}
						annotations.Clear();
						for (int i = 0; i < numAnnotation && target != null; ++i){
							GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
							newAnnotation.transform.SetParent(transform);
							float value = target.componentDesc[i].x;
							float posStart = target.componentDesc[i].y;
							float posEnd = posStart + value;
							float horizontalPos = 0.5f * (posStart + posEnd);
							
							//float horizontalPos = (float)i/(float)divNumber;
							newAnnotation.transform.position = new Vector3(-0.25f - horizontalPos, -0.15f, transform.position.z);
							newAnnotation.GetComponent<Annotation>().SetCurrent(value);
							newAnnotation.GetComponent<Annotation>().ampState = Annotation.State.kLeftTop;
							newAnnotation.GetComponent<Annotation>().showArrows = true;
							newAnnotation.GetComponent<Annotation>().voltState = Annotation.State.kDisabled;
							newAnnotation.GetComponent<Annotation>().ohmState = Annotation.State.kDisabled;
							annotations.Add(newAnnotation);
						}						
					}
				}
				//Debug.Log ("divNumber = " + divNumber);
				break;
			}			
		}
		lastAnnotationState = annotationState;
		
	}
	
	void DisableUI(bool disable){
		AVOWConfig.singleton.tutDisableCreateUIButton = disable;
		AVOWConfig.singleton.tutDisableDestroyUIButton = disable;
		AVOWConfig.singleton.tutDisableConnections = disable;		
	}
	
	public void OnLeave(){
		SetupOffState();
		state = State.kOff;
	}
	
	public void SetupOffState(){
		vizState = VizState.kNormal;
		annotationState = AnnotationState.kNone;
		showAmps = false;
		gridState = GridState.kNone;
		transform.FindChild("Ohms law").gameObject.SetActive(false);
		transform.FindChild("Kirchoffs laws").gameObject.SetActive(false);
		transform.FindChild("Ohms law boxes").gameObject.SetActive(false);
		transform.FindChild("Kirchoffs laws boxes").gameObject.SetActive(false);
		transform.FindChild("ObjectiveArrows").gameObject.SetActive(false);
		AVOWTutorialText.singleton.ClearContinueButton();
		AVOWTutorialText.singleton.ResetClearButtonCounter();
		DisableUI(false);
		
		
	}
	
	void SetButtonTrigger(){
		hasButtonTriggered = false;
		externalButtonTrigger = false;
		AVOWTutorialText.singleton.AddButton();
	}
	
	// Set bypass to true if we don't actually want to wait for the text
	void SetTextTrigger(){
		hasTextTriggered = false;
		externalTextTrigger = false;
		AVOWTutorialText.singleton.AddTrigger();
	}
	
	public void Trigger(){
		externalTextTrigger = true;
	}
	public void ButtonTrigger(){
		externalButtonTrigger = true;
	}	
	
	public void Initialise(){
		SetupOffState();
		state = State.kIntro;
	}
	
	public void Finish(){
		SetupOffState();
		DisableUI(false);
		state = State.kOff;
	}
	
	// Call to ensure we have a circuit with a single resistor
	void MakeUnitCircuit(){
		AVOWGraph graph = AVOWGraph.singleton;
		graph.GetComponent<AVOWGraph>().ClearCircuit();
		
		// Simple start
		GameObject node0GO = graph.GetComponent<AVOWGraph>().AddNode ();
		GameObject node1GO = graph.GetComponent<AVOWGraph>().AddNode ();
		
		
		graph.GetComponent<AVOWGraph>().PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.cellPrefab) as GameObject, node0GO, node1GO);
		graph.GetComponent<AVOWGraph>().PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.resistorPrefab) as GameObject, node1GO, node0GO);
		AVOWSim.singleton.GameUpdate();
	}
	
	void HandleExplanationState(){
		// on entering state
		bool onEnterState = (state != lastState);
		lastState = state;
		
		// Text trigger
		bool onTextTrigger = (!hasTextTriggered && externalTextTrigger);
		hasTextTriggered = hasTextTriggered || onTextTrigger;
		
		bool onButtonTrigger = (!hasButtonTriggered && externalButtonTrigger);
		hasButtonTriggered = hasButtonTriggered || onButtonTrigger;
		
		
		switch (state){
			case State.kOff:{
				
				break;
			}
			case State.kIntro:{
				if (onEnterState){
//					Debug.Log ("expl01kIntro -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl01kIntro", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });	
				
					DisableUI(true);
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("What is this game really about?");
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("It is about voltages and currents in electrical circuits.");
					AVOWTutorialText.singleton.AddText("It is best to watch this explanation after playing the game a bit.");
					AVOWTutorialText.singleton.AddText("Add a single resistance square between the connection points to move on.");
					SetTextTrigger();
				}	
				if (onTextTrigger){
					DisableUI(false);
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
					state = State.kRemovingTheWorld;	
				}
				break;
			}
			case State.kRemovingTheWorld:{
				if (onEnterState){
//					Debug.Log ("expl02kRemovingTheWorld -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl02kRemovingTheWorld", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });	
										
					DisableUI(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good - now, I will remove everything from the game apart from the electrical sparks.");
					AVOWTutorialText.singleton.AddText("Press the CONTINUE button in the bottom right corner to do this.");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kMakeThree;	
				}
				break;
			}
			case State.kMakeThree:{
				if (onEnterState){
					DisableUI(false);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Try adding another two resistance squares - making a total of three.");
					
					vizState = VizState.kCircuitOnly;
					annotationState = AnnotationState.kNone;
					showAmps = false;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kTradCircuit;	
				}
				break;
			}			
			case State.kTradCircuit:{
				if (onEnterState){	
//					Debug.Log ("expl03kTradCircuit -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl03kTradCircuit", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
						
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good! You may recognise this as a traditional circuit diagram.");
					AVOWTutorialText.singleton.AddText("There is a cell (battery) on the right and three resistors on the left.");
					AVOWTutorialText.singleton.AddText("Suppose the cell was 1 volt and the resistors were all 1 ohm. What are the voltages and currents in the circuit?");
					SetButtonTrigger();
					SetTextTrigger();
				}
				if (onTextTrigger){
					annotationState = AnnotationState.kIndividual;
					showOhms = true;
					showArrowsOnBattery = false;
					showLoadVoltages = false;	
					
				}
				if (onButtonTrigger){
					state = State.kQuantities1;
				}				
				break;
			}
			case State.kQuantities1:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("This game visualises them to help you understand how they behave.");
					AVOWTutorialText.singleton.AddText("The voltage difference across a component is represented by its height.");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kQuantities2;
				}				
				break;
			}	
			case State.kQuantities2:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("I've displayed the battery - this is a 1-volt battery.");
					vizState = VizState.kCircuitAndBatteryOnly;
					annotationState = AnnotationState.kIndividual;
					showArrowsOnBattery = true;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kBoxes1;
				}				
				break;
			}	
			case State.kBoxes1:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("The metal boxes visualise the voltage across each 1-ohm resistor. ");
					AVOWTutorialText.singleton.AddText("Try adding and removing resistors to see these voltages change.");
					AVOWTutorialText.singleton.AddPause(10);
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					showArrowsOnLoads = true;
					showLoadVoltages = true;	
					showOhms = false;
					SetTextTrigger();
				}
				if (onTextTrigger){
					state = State.kBoxes2;
				}				
				break;
			}	
			case State.kBoxes2:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Press CONTINUE when you are ready to move on.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kBoxesTotal;
				}				
				break;
			}	
			case State.kBoxesTotal:{
				if (onEnterState){	
//					Debug.Log ("expl04kBoxesTotal -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl04kBoxesTotal", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Of course, the total voltage across the circuit is always 1 volt because the cell driving it is 1 volt.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					showAmps = false;
					showOhms = false;
					showArrowsOnBattery = true;
					showArrowsOnLoads = true;
					showLoadVoltages = true;
					SetTextTrigger();
				}
				if (onTextTrigger){
					if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
						state = State.kBoxesSetupOne2;
					}
					else{
						state = State.kBoxesSetupOne1;
					}
				}				
				break;
			}	
			case State.kBoxesSetupOne1:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Set up the circuit with just one resistor to move on.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1 && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					state = State.kBoxesSetupOne2;
				}				
				break;
			}
			case State.kBoxesSetupOne2:{
				if (onEnterState){	
//					Debug.Log ("expl05kBoxesSetupOne2 -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl05kBoxesSetupOne2", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					
					MakeUnitCircuit();
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("You have a circuit with one resistor.");
					AVOWTutorialText.singleton.AddText("We can use Ohm's Law to calculate the current flowing through it.");
					AVOWTutorialText.singleton.AddText("Ohm's law states: Voltage = Resistance * Current");
					AVOWTutorialText.singleton.AddText("Our resistor is 1 ohm, the voltage across it is 1 volt, so the current must be 1 amp.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					DisableUI(true);
					annotationState = AnnotationState.kIndividual;
					showOhms = true;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kOneCurrent;
				}				
					break;
			}
			case State.kOneCurrent:{
				if (onEnterState){	
					transform.FindChild("Ohms law").gameObject.SetActive(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("The width of the metal boxes visualise the current flowing through the resistors in amps.");
					AVOWTutorialText.singleton.AddText("Try adding and removing resistors and observe the currents flowing through them.");
					AVOWTutorialText.singleton.AddPause(10);
				
					DisableUI(false);
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					showAmps = true;
					SetTextTrigger();
				}
				if (AVOWGraph.singleton.HasHalfFinishedComponents()){
					showOhms = false;
				}
				if (onTextTrigger){
					state = State.kLotsCurrent;
				}				
				break;
			}			
			case State.kLotsCurrent:{
				if (onEnterState){	
//					Debug.Log ("expl06kLotsCurrent -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl06kLotsCurrent", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Press CONTINUE when you are ready to move on.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kBoxesAreSquare;
				}				
				break;
			}	
			case State.kBoxesAreSquare:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Since all our resistors are 1 ohm, we can see from Ohm's Law, that the current through a resistor will always equal the voltage across it (Voltage = 1 * Current).");
					AVOWTutorialText.singleton.AddPause(2);
					AVOWTutorialText.singleton.AddText("This is why, even though our metal boxes may change size, they are ALWAYS square.");
					
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kShowWholeCircuit1;
				}				
				break;
			}
			case State.kShowWholeCircuit1:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("As well as considering each resistor individually, we can consider the network of all resistors in the circuit as if it were a single resistor.");
				
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kShowWholeCircuit2;
				}				
				break;
			}
			case State.kShowWholeCircuit2:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("The white arrows and labels now refer to the network of resistors as a whole instead of individual ones.");
					AVOWTutorialText.singleton.AddText("We can use the total width and height of our pattern of boxes to determine the voltage across the network and the current flowing through it.");
					AVOWTutorialText.singleton.AddText("We can then apply Ohm's Law to calculate the resistance of our network of resistors - note that it is not necessarily square so may not be 1 ohm.");
					showOhms = true;
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kWholeCircuit;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kKirchoffsLaws;
				}				
				break;
			}			
			case State.kKirchoffsLaws:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Also, note that the pattern of boxes always forms a rectangle, with no gaps, whose hieght is the voltage of the cell.");
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("This is due to something called \"Kirchoff's Laws\".");
					
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kChallenge1;
				}				
				break;
			}	
//			case State.kKirchoffsLawsBoxes:{
//				if (onEnterState){	
//					transform.FindChild("Kirchoffs laws").gameObject.SetActive(true);
//					AVOWTutorialText.singleton.AddText("");
//					AVOWTutorialText.singleton.AddText("These are perhaps better unstood in terms of how they affect our metal boxes.");
//					AVOWTutorialText.singleton.AddText(" - The boxes in a circuit must fit together to form a rectangle with no gaps inside it.");
//					AVOWTutorialText.singleton.AddText(" - The height of this rectangle must be the same as the voltage of the battery driving it.");
//					
//					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
//					annotationState = AnnotationState.kIndividual;
//					SetButtonTrigger();
//				}
//				if (onButtonTrigger){
//					state = State.kBoxRewrite;
//				}				
//				break;
//			}	
//			case State.kBoxRewrite:{
//				if (onEnterState){	
//					transform.FindChild("Kirchoffs laws").gameObject.SetActive(false);
//					transform.FindChild("Ohms law").gameObject.SetActive(false);
//					transform.FindChild("Kirchoffs laws boxes").gameObject.SetActive(true);
//					transform.FindChild("Ohms law boxes").gameObject.SetActive(true);
//					AVOWTutorialText.singleton.AddText("");
//					AVOWTutorialText.singleton.AddText("I've rewriten the electrical laws above in terms of our metal boxes.");
//					
//					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
//					annotationState = AnnotationState.kIndividual;
//					SetButtonTrigger();
//				}
//				if (onButtonTrigger){
//					state = State.kChallenge1;
//				}				
//				break;
//			}
			case State.kChallenge1:{
				if (onEnterState){	
//					Debug.Log ("expl07kChallenge1 -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl07kChallenge1", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					
					transform.FindChild("Kirchoffs laws").gameObject.SetActive(false);
					transform.FindChild("Ohms law").gameObject.SetActive(true);
					transform.FindChild("Kirchoffs laws boxes").gameObject.SetActive(false);
					transform.FindChild("Ohms law boxes").gameObject.SetActive(false);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Try to construct a circuit using four resistors where each has a current of # of an amp flowing through it.");
					showAmps = true;
					showOhms = false;
					showArrowsOnBattery = true;
					showArrowsOnLoads = true;
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					challengeContinueDone = false;
					timerStart = Time.fixedTime;
				}
				if (MathUtils.FP.Feq(AVOWSim.singleton.cellCurrent, 1f/4f) && AVOWGraph.singleton.GetNumConfirmedLoads() == 4 && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good!");
					if (challengeContinueDone){
						AVOWTutorialText.singleton.ClearContinueButton();
					}
				
					state = State.kChallenge2;
				}
				// If time out
				if (!challengeContinueDone && Time.fixedTime > timerStart + 25){
					AVOWTutorialText.singleton.AddText("Press CONTINUE if you want to see how to do it.");
					challengeContinueDone = true;
					SetButtonTrigger();
				
				}	
				if (onButtonTrigger){
					state = State.kChallenge1Solution;
				}			
				break;
			}	
			case State.kChallenge1Solution:{
				if (onEnterState){
					DisableUI(true);
//					AVOWTutorialText.singleton.AddText("Showing the solution.");
					challengeMode = ChallengeMode.kNoMode;
					SetTextTrigger();
				}
				if (onTextTrigger){
					challengeMode = ChallengeMode.kClearGraph;
				}
				
				if (challengeMode == ChallengeMode.kClearGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// Find a resistor
					GameObject resistor = null;
					foreach (GameObject go in AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							resistor = go;
						}
					}
					if (resistor != null){
						AVOWCommandRemove removeCommand = new AVOWCommandRemove(resistor);
						removeCommand.ExecuteStep();
						removeCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kMakeGraph;
						challengeNode0GO = AVOWGraph.singleton.allNodes[0];
						challengeNode1GO = AVOWGraph.singleton.allNodes[1];
						challengeCell = AVOWGraph.singleton.allComponents[0];
					}
				}
				else if (challengeMode == ChallengeMode.kMakeGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// If we only have a cell, then construct a resistor
					if (AVOWGraph.singleton.allComponents.Count() == 1){
						AVOWCommandAddComponent addCommand = new AVOWCommandAddComponent(challengeNode0GO, challengeNode1GO, AVOWUI.singleton.resistorPrefab);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
						challengeResistors[0] = addCommand.GetNewComponent();
					}
					else if (AVOWGraph.singleton.allComponents.Count() < 5){
						AVOWCommandSplitAddComponent addCommand = new AVOWCommandSplitAddComponent(challengeCell.GetComponent<AVOWComponent>().node0GO, challengeCell, AVOWUI.singleton.resistorPrefab, true);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kNoMode;
						state = State.kChallenge2;
					}
				}
					
				break;
			}
		
			case State.kChallenge2:{
				if (onEnterState){	
//					Debug.Log ("expl08kChallenge2 -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl08kChallenge2", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					
					DisableUI(false);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Now try and make a circuit with $ of an amp flowing through one resistor and % flowing through another one. You may need more than two resistors to accomplish this.");
					showAmps = true;
					showOhms = false;
					showArrowsOnBattery = true;
					showArrowsOnLoads = true;
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					challengeContinueDone = false;
					timerStart = Time.fixedTime;
					
				}
				bool hasOneThird = false;
				bool hasTwoThirds = false;
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					if (go == null) continue;
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					if (component.type == AVOWComponent.Type.kLoad){
						if (MathUtils.FP.Feq(component.hWidth, 1f/3f)){
							hasOneThird = true;
						}
						if (MathUtils.FP.Feq(component.hWidth, 2f/3f)){
							hasTwoThirds = true;
						}
					}
				}
				if (hasOneThird && hasTwoThirds){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good!");
					if (challengeContinueDone){
						AVOWTutorialText.singleton.ClearContinueButton();
					}
					state = State.kChallengesComplete;
				}
				// If time out
				if (!challengeContinueDone && Time.fixedTime > timerStart + 25){
					AVOWTutorialText.singleton.AddText("Press CONTINUE if you want to see how to do it.");
					challengeContinueDone = true;
					SetButtonTrigger();
					
				}	
				if (onButtonTrigger){
					state = State.kChallenge2Solution;
				}					
				break;
			}		
			case State.kChallenge2Solution:{
				if (onEnterState){
					DisableUI(true);
//					AVOWTutorialText.singleton.AddText("Showing the solution.");
					challengeMode = ChallengeMode.kNoMode;
					SetTextTrigger();
				}
				if (onTextTrigger){
					challengeMode = ChallengeMode.kClearGraph;
				}
				
				if (challengeMode == ChallengeMode.kClearGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// Find a resistor
					GameObject resistor = null;
					foreach (GameObject go in AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							resistor = go;
						}
					}
					if (resistor != null ){
						AVOWCommandRemove removeCommand = new AVOWCommandRemove(resistor);
						removeCommand.ExecuteStep();
						removeCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kMakeGraph;
						challengeNode0GO = AVOWGraph.singleton.allNodes[0];
						challengeNode1GO = AVOWGraph.singleton.allNodes[1];
						challengeCell = AVOWGraph.singleton.allComponents[0];
					}
				}
				else if (challengeMode == ChallengeMode.kMakeGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// If we only 0 or 1 resistor then make a resistor
					if (AVOWGraph.singleton.allComponents.Count() < 3){
						AVOWCommandAddComponent addCommand = new AVOWCommandAddComponent(challengeNode0GO, challengeNode1GO, AVOWUI.singleton.resistorPrefab);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
						challengeResistors[0] = addCommand.GetNewComponent();
					}
					else if (AVOWGraph.singleton.allComponents.Count() < 4){
						AVOWCommandSplitAddComponent addCommand = new AVOWCommandSplitAddComponent(challengeCell.GetComponent<AVOWComponent>().node0GO, challengeCell, AVOWUI.singleton.resistorPrefab, true);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kNoMode;
						state = State.kChallengesComplete;
					}
				}
				
				break;
			}
				
			case State.kChallengesComplete:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("The \"resistors\" we are using could easily be replaced with motors, lamps, heaters etc. which behave much like our resistors.");
					AVOWTutorialText.singleton.AddText("Usually lamps and motors need to have a specific current flowing through them to work efficiently and this must be achieved using additional resistors in the circuit.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kChallengesExplained;
				}				
				break;
			}	
			case State.kChallengesExplained:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("So, the challenges you just did have real electrical meaning.");
					AVOWTutorialText.singleton.AddText("These are also exactly the same challenges that are presented in the game - but instead of asking for currents using numbers (say, $ Amp), we present the challenges visually.");
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kObjectiveBoard;
				}				
				break;
			}
			case State.kObjectiveBoard:{
				if (onEnterState){	
					transform.FindChild("Kirchoffs laws").gameObject.SetActive(false);
					transform.FindChild("Ohms law").gameObject.SetActive(false);
					transform.FindChild("Kirchoffs laws boxes").gameObject.SetActive(false);
					transform.FindChild("Ohms law boxes").gameObject.SetActive(false);
					AVOWTutorialText.singleton.AddPause(2);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("This is the challenge board.");
					DisableUI(true);
				
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kNone;
					SetTextTrigger();
					SetButtonTrigger();
					challengeMode = ChallengeMode.kClearGraph;
				}
				if (challengeMode == ChallengeMode.kClearGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// Find a resistor
					GameObject resistor = null;
					foreach (GameObject go in AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							resistor = go;
						}
					}
					if (resistor != null){
						AVOWCommandRemove removeCommand = new AVOWCommandRemove(resistor);
						removeCommand.ExecuteStep();
						removeCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kNoMode;
						challengeNode0GO = null;
						challengeNode1GO = null;
						challengeCell = null;
					}
				}		
				if (onTextTrigger){
					transform.FindChild("ObjectiveArrows").gameObject.SetActive(true);
					vizState = VizState.kCircuitAndBatteryAndMetalAndObjectivesOnly;
				
				}		
				if (onButtonTrigger){
				state = State.kObjectiveHeight;
				}				
				break;
			}
			case State.kObjectiveHeight:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("It is 1 volt high.");
					SetTextTrigger();
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kObjectiveSquares;
				}
				if (onTextTrigger){
					transform.FindChild("ObjectiveArrows").gameObject.SetActive(false);
					annotationState = AnnotationState.kObjectiveGrid;
				}
				break;
			}
			case State.kObjectiveSquares:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("It can be divide into smaller squares, each representing a smaller voltage and current.");
					SetTextTrigger();
				}
				if (onButtonTrigger){
					state = State.kObjectiveSquares;
				}
				if (onTextTrigger){
					annotationState = AnnotationState.kObjectiveGrid;
					gridState = GridState.kDoGridCycle;
				}
				if (gridState == GridState.kDoGridCycle && AVOWObjectiveManager.singleton.IsWaitingOnManualTrigger()){
					AVOWObjectiveManager.singleton.ManualTrigger();
					gridStartTime = Time.fixedTime;
					gridState = GridState.kWait;
				}
				if (gridState == GridState.kWait && Time.fixedTime > gridStartTime + 3){
					if (AVOWObjectiveManager.singleton.currentGoalIndex < 3){
						gridState = GridState.kDoGridCycle;
					}
					else{
						gridState = GridState.kNone;
						AVOWTutorialText.singleton.AddText("Click CONTINUE to move on.");
					
						SetButtonTrigger();
					}
				}
				if (onButtonTrigger){
					state = State.kObjectiveSquaresExplanation;
				}
			    break;
			}		
			case State.kObjectiveSquaresExplanation:{
//				Debug.Log ("expl09kObjectiveSquaresExplanation -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
				Analytics.CustomEvent("expl09kObjectiveSquaresExplanation", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
			
				
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("On top of this grid, we place metal frames.");
					AVOWTutorialText.singleton.AddText("We can infer the size of the frames from the grid-squares they occupy.");
				
					annotationState = AnnotationState.kNone;
					AVOWObjectiveManager.singleton.ManualTrigger();
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kChallenge3;
				}
				break;
			}					
			case State.kChallenge3:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("");
				AVOWTutorialText.singleton.AddText("These frames are asking you to construct a circuit with # Amp through one resistor and & Amp through another.");
					AVOWTutorialText.singleton.AddText("Try and do it. Imagine stacking the frames up into a rectangle - then fill in the gaps.");
					DisableUI(false);
				
					annotationState = AnnotationState.kObjectiveFrames;
					AVOWObjectiveManager.singleton.ManualTrigger();
					challengeContinueDone = false;
					timerStart = Time.fixedTime;
	
					//SetButtonTrigger();
				}
				
				// If no longer waiting, then it must have been completed
				if (AVOWObjectiveManager.singleton.IsCompletingGoal()){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good!");
					state = State.kFinish0;
				}
				// If time out
				if (!challengeContinueDone && Time.fixedTime > timerStart + 25){
					AVOWTutorialText.singleton.AddText("Press CONTINUE if you want to see how to do it.");
					challengeContinueDone = true;
					SetButtonTrigger();
					
				}	
				if (onButtonTrigger){
					state = State.kChallenge3Solution;
				}					
				AVOWObjectiveManager.singleton.ManualTrigger();
				break;
			}	
			case State.kChallenge3Solution:{
				if (onEnterState){
					DisableUI(true);
					//					AVOWTutorialText.singleton.AddText("Showing the solution.");
					challengeMode = ChallengeMode.kNoMode;
					SetTextTrigger();
				}
				if (onTextTrigger){
					challengeMode = ChallengeMode.kClearGraph;
				}
				
				if (challengeMode == ChallengeMode.kClearGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// Find a resistor
					GameObject resistor = null;
					foreach (GameObject go in AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							resistor = go;
						}
					}
					if (resistor != null ){
						AVOWCommandRemove removeCommand = new AVOWCommandRemove(resistor);
						removeCommand.ExecuteStep();
						removeCommand.ExecuteStep();
					}
					else{
						challengeMode = ChallengeMode.kMakeGraph;
						challengeNode0GO = AVOWGraph.singleton.allNodes[0];
						challengeNode1GO = AVOWGraph.singleton.allNodes[1];
						challengeCell = AVOWGraph.singleton.allComponents[0];
					}
				}
				else if (challengeMode == ChallengeMode.kMakeGraph && !AVOWGraph.singleton.HasHalfFinishedComponents()){
					// If we only 0 or 1 resistor then make a resistor
					if (AVOWGraph.singleton.allComponents.Count() < 4){
						AVOWCommandAddComponent addCommand = new AVOWCommandAddComponent(challengeNode0GO, challengeNode1GO, AVOWUI.singleton.resistorPrefab);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
						challengeResistors[0] = addCommand.GetNewComponent();
					}
					else if (AVOWGraph.singleton.allComponents.Count() < 5){
						AVOWCommandSplitAddComponent addCommand = new AVOWCommandSplitAddComponent(challengeCell.GetComponent<AVOWComponent>().node0GO, challengeCell, AVOWUI.singleton.resistorPrefab, true);
						addCommand.ExecuteStep();
						addCommand.ExecuteStep();
					}
					else{
						AVOWObjectiveManager.singleton.ManualTrigger();
						// If no longer waiting, then it must have been completed
						if (AVOWObjectiveManager.singleton.IsCompletingGoal()){
							state = State.kFinish0;
							challengeMode = ChallengeMode.kNoMode;
						}
					}
				}
				
				break;
			}
				
			case State.kFinish0:{
				if (onEnterState){
					annotationState = AnnotationState.kNone;
				}
				if (AVOWObjectiveManager.singleton.IsWaitingOnManualTrigger()){
					state = State.kFinish1;
				}
				break;
			}				
			case State.kFinish1:{
				if (onEnterState){
//					Debug.Log ("expl10kFinish1 -  gameTime: " + AVOWUpdateManager.singleton.GetGameTime());
					Analytics.CustomEvent("expl10kFinish1", new Dictionary<string, object>{ { "levelTime", AVOWUpdateManager.singleton.GetGameTime()} });
				
					DisableUI(false);
					SetupOffState();
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("This is the end of the explanation - Press CONTINUE to go back to the main menu.");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					AVOWGameModes.singleton.GoToMain();
				}
				break;
			}			
	
		}
	}
	
	
	// Update is called once per frame
	void FixedUpdate () {
	
		HandleVizState();
		HandleAnnotationState();
		HandleExplanationState();


	
	}
	
	bool IsOnEdge(AVOWComponent component, out bool top, out bool right, out bool bottom, out bool left){
		float minVolt = Mathf.Min(component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
		float maxVolt = Mathf.Max(component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
		float minAmp = component.h0;
		float maxAmp = component.h0 + component.hWidth;
	
		top =  MathUtils.FP.Feq(maxVolt, 1);
		right =  MathUtils.FP.Feq(maxAmp, AVOWSim.singleton.cellCurrent);
		bottom = MathUtils.FP.Feq(minVolt, 0);
		left = MathUtils.FP.Feq(minAmp, 0);
		
		return top || right || bottom || left;
		
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
		VectorLine.SetEndCap ("rounded_arrow", EndCap.Both, arrowMaterial, arrowFrontTex, arrowBackTex);
		VectorLine.SetEndCap ("rounded_DottedArrow", EndCap.Both, dottedArrowMaterial, arrowFrontTex, arrowBackTex);
		VectorLine.SetEndCap ("rounded_2Xarrow", EndCap.Both, arrowMaterial, arrowFrontTex, arrowFrontBackTex);
		VectorLine.SetEndCap ("rounded_2XDottedArrow", EndCap.Both, arrowMaterial, arrowFrontTex, arrowFrontBackTex);
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
}
