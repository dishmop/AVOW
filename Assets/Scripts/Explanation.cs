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
	
	void DisableUI(bool disable){
		AVOWConfig.singleton.tutDisableCreateUIButton = disable;
		AVOWConfig.singleton.tutDisableDestroyUIButton = disable;
		AVOWConfig.singleton.tutDisableConnections = disable;		
	}
	
	void SetupOffState(){
		vizState = VizState.kNormal;
		annotationState = AnnotationState.kNone;
		showAmps = false;
		transform.FindChild("Ohms law").gameObject.SetActive(false);
		transform.FindChild("Kirchoffs law").gameObject.SetActive(false);
		transform.FindChild("Ohms law boxes").gameObject.SetActive(false);
		transform.FindChild("Kirchoffs laws boxes").gameObject.SetActive(false);
		
		
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
					DisableUI(false);
					
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("What is this game really about?");
					AVOWTutorialText.singleton.AddPause(3);
					AVOWTutorialText.singleton.AddText("It is about voltages and currents in electrical circuits.");
					AVOWTutorialText.singleton.AddText("It is best to watch this explanation after playing the game a bit.");
					AVOWTutorialText.singleton.AddText("Add a single resistance cube between the connection points to move on.");
				}	
				if (AVOWGraph.singleton.GetNumConfirmedLoads() == 1){
					state = State.kRemovingTheWorld;	
				}
				break;
			}
			case State.kRemovingTheWorld:{
				if (onEnterState){
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Try adding another two resistance cubes - making a total of three.");
					
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Good! You may recognise this as a traditional circuit diagram.");
					AVOWTutorialText.singleton.AddText("There is a cell (battery) on the right and three resistors on the left.");
					AVOWTutorialText.singleton.AddText("Suppose the cell was 1 volt and the resistors were all 1 ohm.");
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kQuantities1;
				}				
				break;
			}
			case State.kQuantities1:{
				if (onEnterState){	
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("What are the currents and voltages in the circuit?");
					AVOWTutorialText.singleton.AddText("This game visualises them to help you understand how they behave.");
					AVOWTutorialText.singleton.AddText("The voltage difference across a component is represented by its height.");
					annotationState = AnnotationState.kIndividual;
					showOhms = true;
					showArrowsOnBattery = false;
					showLoadVoltages = false;	
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kQuantities2;
				}				
				break;
			}	
			case State.kQuantities2:{
				if (onEnterState){	
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
					MakeUnitCircuit();
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("You have a circuit with one resistor.");
					AVOWTutorialText.singleton.AddText("We can use Ohm's Law to calculate the current flowing through it.");
					AVOWTutorialText.singleton.AddText("Ohm's law states: Voltage = Resistance × Current");
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("Since all our resistors are 1 ohm, we can see from Ohm's Law, that the current flowing through a resistor will always be equal to the voltage accross it (Voltage = 1 × Current)");
					AVOWTutorialText.singleton.AddPause(2);
					AVOWTutorialText.singleton.AddText("This is why, even though our metal boxes may change size, they are ALWAYS square (Height = 1 × Width)");
					
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
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
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("In addition to Ohm's law there are also Kirchoff's laws.");
					AVOWTutorialText.singleton.AddText("Kichoff's Voltage Law:");
					AVOWTutorialText.singleton.AddText(" - The sum of all the voltage differences around any closed loop in a circuit is alwaus zero.");
					AVOWTutorialText.singleton.AddText("Kichoff's Current Law:");
					AVOWTutorialText.singleton.AddText(" - At a junction of resistors, the total current flowing in equals the total current flowing out");

					
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kKirchoffsLawsBoxes;
				}				
				break;
			}	
			case State.kKirchoffsLawsBoxes:{
				if (onEnterState){	
					transform.FindChild("Kirchoffs law").gameObject.SetActive(true);
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("These are perhaps better unstood in terms of how they affect our metal boxes.");
					AVOWTutorialText.singleton.AddText(" - The boxes in a circuit must fit together to form a rectangle with no gaps inside it.");
					AVOWTutorialText.singleton.AddText(" - The height of this rectangle must be the same as the voltage of the battery driving it.");
					
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kBoxRewrite;
				}				
				break;
			}	
			case State.kBoxRewrite:{
				if (onEnterState){	
					transform.FindChild("Kirchoffs law").gameObject.SetActive(false);
					transform.FindChild("Ohms law").gameObject.SetActive(false);
					transform.FindChild("Kirchoffs law boxes").gameObject.SetActive(true);
					transform.FindChild("Ohms law boxes").gameObject.SetActive(true);
					AVOWConfig.singleton.DisplayBottomPanel(true);
					AVOWTutorialText.singleton.AddText("");
					AVOWTutorialText.singleton.AddText("I've rewriten the electrical laws above in terms of our metal boxes.");
					
					vizState = VizState.kCircuitAndBatteryAndMetalOnly;
					annotationState = AnnotationState.kIndividual;
					SetButtonTrigger();
				}
				if (onButtonTrigger){
					state = State.kLotsCurrent;
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
