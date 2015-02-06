using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWNode : MonoBehaviour {
	// This list may not be needed anymore
	public List<GameObject> components = new List<GameObject>();
	
	public bool visited;
	public int id;
	
	// simulation data
	public float voltage;
	
	// Visualisation data
	public float h0;	// =-1 if not known yet	
	public float h0LowerBound;		// Set to -1 if unknown
	public float h0UpperBound;		// Set to -1 if unknown
	public float inOrdinalledWidth;	// width which has been used up by ordinalled componens
	public float outOrdinalledWidth;// width which has been used up by ordinalled componens
	public GameObject splitFromNode;	// If we were splitout from another node - this points to that node
	public bool hasBeenLayedOut;	// false at first and then true after we have been layed out at least once. 
	
	public float hWidth;
	
	// These lists are filled with the components with current flowing in and out of the node
	// we oreder these by their h order
	public List<GameObject> inComponents;		// Components that have current flowing into this node
	public List<GameObject> outComponents;		// Componets that hae current flowing out of this node
	
	public bool isInteractive = true;
	
	bool isSelected = false;
	
	

	public string GetID(){
		return id.ToString ();
	}
	
	static int staticCount = 0;
	
	void Awake(){
		id = staticCount++;
	}
	
  	public void HasBeenLayedOut(){
		hasBeenLayedOut = true;
		
		// A bit naughty, but the only way we can get all the things in the right place
		Update ();
	}
		
	
	
	public void SetSelected(bool enable){
		isSelected = enable;
	}

	void Update(){
	
		Material material = transform.FindChild("LineNode").FindChild("LineNodeRender").renderer.material;
		float currentSelectVal = material.GetFloat("_Intensity");
		float newVal = Mathf.Lerp (currentSelectVal, isSelected ? 1 : 0, 0.4f);
		material.SetFloat("_Intensity", newVal);
		
		// Calc the prop value (use any of the components to get the gap value
		float useLength = hWidth - 2 * components[0].GetComponent<AVOWComponent>().squareGap;
		material.SetFloat("_GapProp", useLength / hWidth);
				
		transform.position = new Vector3(h0 + 0.5f * hWidth, voltage, 0);
		transform.localScale = new Vector3( hWidth,  0.5f * hWidth,  1);
		transform.gameObject.SetActive(isInteractive);		
	
	}
}
