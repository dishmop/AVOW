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

	public float hWidth;
	
	// These lists are filled with the components with current flowing in and out of the node (those which have been 
	// ordered will be first)
	public List<GameObject> inComponents;
	public List<GameObject> outComponents;
	
	
	public string GetID(){
		return id.ToString ();
	}
	
	static int staticCount = 0;
	
	void Awake(){
		id = staticCount++;
	}
	
	void Update(){
	
		transform.FindChild("Sphere").position = new Vector3(h0 + 0.5f * hWidth, voltage, 0);
		transform.FindChild("Sphere").localScale = new Vector3( 0.2f * hWidth,  0.2f * hWidth,  0.2f * hWidth);
		
		transform.FindChild("LineNode").position = new Vector3(h0 + 0.5f * hWidth, voltage, 0);
		transform.FindChild("LineNode").localScale = new Vector3( hWidth,  0.5f * hWidth,  1);		
	
	}
}
