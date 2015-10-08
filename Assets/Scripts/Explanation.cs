using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	

	public enum AnnotationState{
		kNone,
		kIndividual,
		kBattery,
		kWholeCircuit
	};

	public AnnotationState annotationState = AnnotationState.kNone;
	
	public enum VizState{
		kError,
		kNormal,
		kCircuitOnly,
		kCircuitAndBatteryOnly,
		kCircuitAndBatteryAndMetalOnly
	}
	
	public VizState vizState = VizState.kNormal;
	
	public bool showAmps = false;
	
	VizState lastVizState = VizState.kError;
	
	public enum State{
		kOff,
		kIntro,
		kRemovingTheWorld,
		kTradCircuit,
		kVisualiseQuantities1,
		kVisualiseQuantities2,
		kVisualiseBoxes1,
	}
	
	public State state = State.kOff;
	State lastState = State.kOff;


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
							// Set up the amp annotation position
							if (left){
								thisAnnotation.voltState = Annotation.State.kLeftTop;
							}
							else if (right){
								thisAnnotation.voltState = Annotation.State.kRightBottom;
							}
							else{
								thisAnnotation.voltState = Annotation.State.kDisabled;
							}
							
							// Set up the volt annotation position
							if (bottom){
								thisAnnotation.ampState = showAmps ? Annotation.State.kRightBottom : Annotation.State.kDisabled;
							}
							else if (top){
								thisAnnotation.ampState = showAmps ? Annotation.State.kLeftTop : Annotation.State.kDisabled;
							}
							else{
								thisAnnotation.ampState = Annotation.State.kDisabled;
							}	
						}
						else{
							thisAnnotation.voltState = Annotation.State.kRightBottom;
							thisAnnotation.ampState = Annotation.State.kDisabled;
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
				thisAnnotation.ampState = showAmps ? Annotation.State.kRightBottom : Annotation.State.kDisabled;;
				
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
		}
		
	}
	
	void SetupOffState(){
		vizState = VizState.kNormal;
		annotationState = AnnotationState.kNone;
		showAmps = false;
		
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
				SetupOffState();
				break;
			}
			case State.kIntro:{
				if (onEnterState){
					SetupOffState();
					AVOWConfig.singleton.tutDisableCreateUIButton = false;
					AVOWConfig.singleton.tutDisableDestroyUIButton = false;
					
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("What is this game really about?");
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("It is about how voltages and currents are distributed around electrical circuits.");
					AVOWTutorialText.singleton.AddText("It is best to watch this explanation after playing the game a bit.");
					AVOWTutorialText.singleton.AddText("Press the CONTINUE button in the bottom right corner.");
					SetButtonTrigger();
				}	
				if (onButtonTrigger){
					state = State.kRemovingTheWorld;
				}
				break;
			}
			case State.kRemovingTheWorld:{
				if (onEnterState){
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("For this explanation, I've removed everything from the game apart from the electrical sparks.");
					AVOWTutorialText.singleton.AddText("Try constructing a circuit with three resistors in it.");
				
					vizState = VizState.kCircuitOnly;
					annotationState = AnnotationState.kNone;
					showAmps = false;
					AVOWConfig.singleton.tutDisableCreateUIButton = true;
					AVOWConfig.singleton.tutDisableDestroyUIButton = true;
				}
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 3){
					state = State.kTradCircuit;	
				}
				break;
			}
			case State.kTradCircuit:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good! You may recognise this as a traditional circuit diagram");
					AVOWTutorialText.singleton.AddText("There is a cell (battery) on the right and three resistors on the left");
					AVOWTutorialText.singleton.AddText("Suppose the cell was 1 volt and the resistors were all 1 ohm. What would be the currents and voltages in the circuit?");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kVisualiseQuantities1;
				}				
				break;
			}
			case State.kVisualiseQuantities1:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("This is a hard problem to solve.");
					AVOWTutorialText.singleton.AddText("This game visualises the currents and voltages to help you understand how they behave.");
					AVOWTutorialText.singleton.AddText("Voltage is represented by height.");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kVisualiseQuantities2;
				}				
				break;
			}	
			case State.kVisualiseQuantities2:{
				if (onEnterState){	
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("I've displayed the battery now - this is 1 volt");
					vizState = VizState.kCircuitAndBatteryOnly;
					annotationState = AnnotationState.kBattery;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kVisualiseBoxes1;
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
