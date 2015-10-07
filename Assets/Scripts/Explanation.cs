using UnityEngine;
using System.Collections;
using System.Linq;

using Vectrosity;

public class Explanation : MonoBehaviour {
	public static Explanation singleton = null;

	public GameObject sceneConstructor;
	public GameObject quarterColumn;
	public GameObject pusher;
	public GameObject objectiveBoard;
	
	public Material arrowMaterial;
	public Material dottedArrowMaterial;
	public Texture2D arrowFrontTex;
	public Texture2D arrowFrontBackTex;
	public Texture2D arrowBackTex;
	
	

	public enum State{
		kError,
		kNormal,
		kCircuitOnly
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
			}
			lastState = state;
		}
		
		transform.GetChild(0).GetComponent<Annotation>().componentGOs.Clear();
		if (AVOWGraph.singleton.allComponents.Count() > 1){
			transform.GetChild(0).GetComponent<Annotation>().componentGOs.Add (AVOWGraph.singleton.allComponents[1]);
		}
	
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
