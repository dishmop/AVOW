using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class AVOWNode : MonoBehaviour {

	const int		kLoadSaveVersion = 1;	
	
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
	public float addConnPos = -1;	// Set this so sometjhing positive if we want to pretend there is something else connected to this node (like a cursor)
	
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
		RenderUpdate ();
	}
		
	
	
	public void SetSelected(bool enable){
		isSelected = enable;
	}
	
	public void SerialiseData(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bw.Write (visited);
		bw.Write (id);
		
		// simulation data
		bw.Write (voltage);
		
		// Visualisation data
		bw.Write ( h0);	// =-1 if not known yet	
		bw.Write (h0LowerBound);
		bw.Write (h0UpperBound);
		bw.Write (inOrdinalledWidth);
		bw.Write (outOrdinalledWidth);
		bw.Write (hasBeenLayedOut);	
		bw.Write (addConnPos);
		
		bw.Write (hWidth);
		bw.Write (isInteractive);
		bw.Write (isSelected);
	}
	
	public void DeserialiseData(BinaryReader br){
		int version = br.ReadInt32 ();
		switch (version){
			case (kLoadSaveVersion):{
				visited = br.ReadBoolean();	
				id = br.ReadInt32 ();	
				
				voltage = br.ReadSingle();
				h0 = br.ReadSingle();
				h0LowerBound = br.ReadSingle();
				h0UpperBound = br.ReadSingle();
				inOrdinalledWidth = br.ReadSingle();
				outOrdinalledWidth = br.ReadSingle();
				hasBeenLayedOut = br.ReadBoolean();
				addConnPos = br.ReadSingle();
				hWidth = br.ReadSingle();
				isInteractive = br.ReadBoolean();
				isSelected =  br.ReadBoolean();
				break;
			}
		}
	}
	
	
	public void SerialiseConnections(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		
		bw.Write (components.Count);
		for (int i = 0; i < components.Count; ++i){
			AVOWGraph.singleton.SerialiseRef(bw, components[i]);
		}
		
		bw.Write (inComponents.Count);
		for (int i = 0; i < inComponents.Count; ++i){
			AVOWGraph.singleton.SerialiseRef(bw, inComponents[i]);
		}
		
		bw.Write (outComponents.Count);
		for (int i = 0; i < outComponents.Count; ++i){
			AVOWGraph.singleton.SerialiseRef(bw, outComponents[i]);
		}
		
		AVOWGraph.singleton.SerialiseRef(bw, splitFromNode);
			
		
	}
	
	public void DeserialiseConnections(BinaryReader br){
		int version = br.ReadInt32 ();
		switch (version){
			case (kLoadSaveVersion):{
			
				
				int numComponents = br.ReadInt32 ();
				components = new List<GameObject>();
				for (int i = 0; i < numComponents; ++i){
					components.Add(AVOWGraph.singleton.DeseraliseRef(br));
				}
				
				int numInComponents = br.ReadInt32 ();
				inComponents = new  List<GameObject>();
				for (int i = 0; i < numInComponents; ++i){
					inComponents.Add(AVOWGraph.singleton.DeseraliseRef(br));
				}
	
				int numOutComponents = br.ReadInt32 ();
				outComponents = new List<GameObject>();
				for (int i = 0; i < numOutComponents; ++i){
					outComponents.Add(AVOWGraph.singleton.DeseraliseRef(br));
				}
				
				splitFromNode = AVOWGraph.singleton.DeseraliseRef(br);
				
				break;
			}
		}
	}
	
	public void GameUpdate (){
		
	}

	public void RenderUpdate(){
	
		// Hmm this seems to happen when ghe batery runs out
		if (float.IsNaN(h0)){
			Debug.Log ("Error in node h0");
			return;
		}
		
	
		Material material = transform.FindChild("LineNode").FindChild("LineNodeRender").GetComponent<Renderer>().material;
		float currentSelectVal = material.GetFloat("_Intensity");
		float newVal = Mathf.Lerp (currentSelectVal, isSelected ? 1 : 0, 0.4f);
		material.SetFloat("_Intensity", newVal);
		
		// Calc the prop value (use any of the components to get the gap value
		float useLength = 0;
		
		if (components.Count > 0){
			useLength = hWidth - 2 * components[0].GetComponent<AVOWComponent>().squareGap;
		}
		material.SetFloat("_GapProp", useLength / hWidth);
				
		transform.position = new Vector3(h0 + 0.5f * hWidth, voltage, 0);
		transform.localScale = new Vector3( hWidth,  0.5f * hWidth,  1);
		transform.gameObject.SetActive(isInteractive);	
		
		// Modify the scale and position so we don't extend outside of the connections we have
		if (AVOWConfig.singleton.modifiedNodeLengths) ApplyModifiedLength();

	
	}
	
	void ApplyModifiedLength(){
		// examine list of all conneciton points along this node
		float minPos = h0 + hWidth;
		float maxPos = h0;
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.isInteractive){
				float thisPos = component.h0 + 0.5f * component.hWidth;
				minPos = Mathf.Min (minPos, thisPos);
				maxPos = Mathf.Max (maxPos, thisPos);
			}
		}	
		if (addConnPos >= 0){
			minPos = Mathf.Min (minPos, addConnPos);
			maxPos = Mathf.Max (maxPos, addConnPos);
			
		}
		float nodeLength = maxPos - minPos;
		float centrePos = 0.5f * ( minPos + maxPos);
		
		transform.localScale = new Vector3( nodeLength,  0.5f * hWidth,  1);
		transform.position = new Vector3(centrePos, voltage, 0);
		
		// Recalc the prop value (use any of the components to get the gap value
		Material material = transform.FindChild("LineNode").FindChild("LineNodeRender").GetComponent<Renderer>().material;
		material.SetFloat("_GapProp", 1);
	}
}
