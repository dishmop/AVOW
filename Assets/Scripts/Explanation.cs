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
	
	List<GameObject> annotations = new List<GameObject>();
	

	public enum AnnotationState{
		kNone,
		kIndividual,
		kBattery,
		kWholeCircuit
	};

	public AnnotationState annotationState = AnnotationState.kNone;
	
	public enum State{
		kError,
		kNormal,
		kCircuitOnly,
		kCircuitAndMetalOnly
	}
	
	public State state = State.kNormal;
	
	State lastState = State.kError;


	
	// Update is called once per frame
	void FixedUpdate () {
		// On change
		if (state != lastState){
			switch (state){
				case State.kNormal:{
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
				case State.kCircuitOnly:{
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
				case State.kCircuitAndMetalOnly:{
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
			lastState = state;
		}
		
		
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
								thisAnnotation.ampState = Annotation.State.kRightBottom;
							}
							else if (top){
								thisAnnotation.ampState = Annotation.State.kLeftTop;
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
				if (annotations.Count() != 1){
					foreach (GameObject go in annotations){
						GameObject.Destroy(go);
					}
					annotations.Clear();
					GameObject newAnnotation = GameObject.Instantiate(annotationPrefab) as GameObject;
					newAnnotation.transform.SetParent(transform);
					annotations.Add(newAnnotation);
				}
				Annotation thisAnnotation = annotations[0].GetComponent<Annotation>();
				if (thisAnnotation.componentGOs.Count() != AVOWGraph.singleton.allComponents.Count() -1){
					thisAnnotation.componentGOs.Clear();
					foreach (GameObject go in  AVOWGraph.singleton.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kLoad){
							thisAnnotation.componentGOs.Add (go);
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
					}		
				}
				thisAnnotation.voltState = Annotation.State.kLeftTop;
				thisAnnotation.ampState = Annotation.State.kRightBottom;
				break;
			}
		}


	
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
