using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class AVOWGraph : MonoBehaviour {

	public static AVOWGraph singleton = null;

	// the nodes in the graph are simply numbered
	
	public GameObject nodePrefab;
	public GameObject lighteningPoints;
	
		
	public static int kMinLowerBound = 0;
	public static int kMaxUpperBound = 9999;
	
	public List<GameObject> allNodes = new List<GameObject>();
	public List<GameObject> allComponents = new List<GameObject>();
	
	public float xMin;
	public float xMax;
	public float yMin;
	public float yMax;
	
	const int		kLoadSaveVersion = 1;	
	

	// Optmisation flags
	public bool topologyHasChanged;
	public bool valuesHaveChanged;
	
	List<float>	optValues = new List<float>();
	
	
	List<GameObject> componentsToRemove = new List<GameObject>();
	
	
	
	int GetTypeCode(GameObject go){
		if (go.GetComponent<AVOWNode>()) return 1;
		else if (go.GetComponent<AVOWComponent>()) return 2;
		else return -1;
	}
	
	public void ConstructFromTarget(AVOWCircuitTarget target){
		ClearCircuit();
		
		float multiplier = 1*2*3*4*5*6*7*8;
		
		
		// Simple start
		GameObject node0GO = AddNode ();
		GameObject node1GO = AddNode ();
		
		
		PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.cellPrefab), node0GO, node1GO);
		
		
		PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.resistorPrefab) as GameObject, node1GO, node0GO);
		allComponents[1].GetComponent<AVOWComponent>().Kill(45);


		AVOWSim.singleton.Recalc();
		
		// Make master list of components
		List<Vector3> desc = new List<Vector3>();
		foreach (Vector3 vals in target.componentDesc){
			desc.Add(vals);
		}
		foreach (Vector3 vals in target.hiddenComponents){
			desc.Add(vals);
		}
		
		
		// Make a dictionary lookup for voltages and new nodes
		Dictionary<int, GameObject> voltagToNodeLookup = new Dictionary<int, GameObject>();
		voltagToNodeLookup.Add (0, node0GO);
		voltagToNodeLookup.Add (Mathf.RoundToInt(multiplier), node1GO);
		
		// Create nodes and components
		foreach (Vector3 vals in desc){
			int minVoltage = Mathf.RoundToInt(vals[2] * multiplier);
			if (!voltagToNodeLookup.ContainsKey(minVoltage)){
				voltagToNodeLookup.Add (minVoltage, AddNode());
			}
			
			int maxVoltage = Mathf.RoundToInt((vals[2] + vals[0])  * multiplier);
			if (!voltagToNodeLookup.ContainsKey(maxVoltage)){
				voltagToNodeLookup.Add (maxVoltage, AddNode());
			}
			
			PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.resistorPrefab), voltagToNodeLookup[minVoltage], voltagToNodeLookup[maxVoltage]);
			
		}
		
	}
	
	// This must be reset at the beginning of each GameUpdate frame
	public void ResetOptFlags(){
		topologyHasChanged = false;
		valuesHaveChanged = false;
	}
	
	void ReassignOptValues(){
		optValues = new List<float>();
		for (int i = 0; i < allComponents.Count; ++i){
			AVOWComponent component = allComponents[i].GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kLoad){
				optValues.Add (component.resistanceAngle.GetValue());
			}
			else{
				optValues.Add (component.voltage);
			}
			optValues.Add (Convert.ToSingle(component.isInteractive));
			optValues.Add (Convert.ToSingle(component.showResistance));
			
		}
		optValues.Add (xMax);
	}
	
	void TestIfValuesHaveChanged(){
		if (topologyHasChanged || optValues.Count != 3 * allComponents.Count+1){
			valuesHaveChanged = true;
			ReassignOptValues();
			return;
			
		}

		int optIndex = 0;
		for (int i = 0; i < allComponents.Count; ++i){
			AVOWComponent component = allComponents[i].GetComponent<AVOWComponent>();
			bool diff = false;
			if (component.type == AVOWComponent.Type.kLoad){
				if (!MathUtils.FP.Feq (optValues[optIndex++], component.resistanceAngle.GetValue())){
					diff = true;
				}

			}
			else{
				if (!MathUtils.FP.Feq (optValues[optIndex++], component.voltage)){
					diff = true;
				}
			}
			if (optValues[optIndex++] != Convert.ToSingle(component.isInteractive)){
				diff = true;
			}
			if (optValues[optIndex++] != Convert.ToSingle(component.showResistance)){
				diff = true;
			}
			if (diff){
				valuesHaveChanged = true;
				ReassignOptValues();
				return;
			
			}
		}
		if (!MathUtils.FP.Feq (optValues[optIndex++], xMax)){
			valuesHaveChanged = true;
			ReassignOptValues();
			return;
		}
		
	}
	
	
	// Serialise a pointer to either a component or a node
	public void SerialiseRef(BinaryWriter bw, GameObject go){
		bw.Write (go != null);
		if (go != null){
			if (go.GetComponent<AVOWNode>() != null){
				bw.Write ((int)1);
				bw.Write (go.GetComponent<AVOWNode>().id);
			}
			else if (go.GetComponent<AVOWComponent>() != null){
				bw.Write ((int)2);
				bw.Write (go.GetComponent<AVOWComponent>().id);
			}
			else{
				bw.Write ((int)-1);
				bw.Write ((int)-1);
			}
		}
	}
	
	// Deserialise into to return a pointer to a node or component in the graph
	public GameObject DeseraliseRef(BinaryReader br){
		bool isObj = br.ReadBoolean();
		GameObject retObj = null;
		if (isObj){
			int code = br.ReadInt32 ();
			int id = br.ReadInt32 ();
			switch (code){
				case 1:{
					retObj = allNodes.Find (obj => obj.GetComponent<AVOWNode>().id == id);
					break;
				}
				case 2:{
					retObj = allComponents.Find (obj => obj.GetComponent<AVOWComponent>().id == id);
					break;
				}
				default:{
					Debug.LogError ("Error serialising reference");
					break;
				}
			}
		}
		return retObj;
	}
	
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bw.Write (valuesHaveChanged);
		if (!valuesHaveChanged) return;
		
		bw.Write (topologyHasChanged);
		if (topologyHasChanged){
			bw.Write (allComponents.Count);
			for (int i = 0; i < allComponents.Count; ++i){
				bw.Write ((int)allComponents[i].GetComponent<AVOWComponent>().type);
			}
		}
		for (int i = 0; i < allComponents.Count; ++i){
			allComponents[i].GetComponent<AVOWComponent>().SerialiseData(bw);
		}
		bw.Write (allNodes.Count);
		for (int i = 0; i < allNodes.Count; ++i){
			allNodes[i].GetComponent<AVOWNode>().SerialiseData(bw);
		}
		if (topologyHasChanged){
			for (int i = 0; i < allComponents.Count; ++i){
				allComponents[i].GetComponent<AVOWComponent>().SerialiseConnections(bw);
			}
			for (int i = 0; i < allNodes.Count; ++i){
				allNodes[i].GetComponent<AVOWNode>().SerialiseConnections(bw);
			}
		}
	
		bw.Write (xMin);
		bw.Write (xMax);
		bw.Write (yMin);
		bw.Write (yMax);
		
	}
	
	
	public void Deserialise(BinaryReader br){
	
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				valuesHaveChanged = br.ReadBoolean();
				if (!valuesHaveChanged){
					break;
				}
				topologyHasChanged = br.ReadBoolean();
			
				if (topologyHasChanged){
					int numComponents = br.ReadInt32 ();
					foreach (GameObject go in allComponents){
						GameObject.Destroy (go);
					}
					allComponents = new List<GameObject>();
			
					for (int i = 0; i < numComponents; ++i){
						GameObject newComponenent = null;
						AVOWComponent.Type thisType = (AVOWComponent.Type)br.ReadInt32 ();
						switch (thisType){
							case AVOWComponent.Type.kLoad:{
								newComponenent = GameObject.Instantiate(AVOWUI.singleton.resistorPrefab);
								break;
							}
							case AVOWComponent.Type.kVoltageSource:{
								newComponenent = GameObject.Instantiate(AVOWUI.singleton.cellPrefab);
								break;
							}
						}
						newComponenent.transform.parent = transform;
						allComponents.Add (newComponenent);
					}
				}
				for (int i = 0; i < allComponents.Count; ++i){
					allComponents[i].GetComponent<AVOWComponent>().DeserialiseData(br);
				}
				
			
				int numNodes = br.ReadInt32 ();
				if (topologyHasChanged){
					foreach (GameObject go in allNodes){
						GameObject.Destroy (go);
					}
					allNodes = new List<GameObject>();
					
					
					for (int i = 0; i < numNodes; ++i){
						GameObject newNode = GameObject.Instantiate(nodePrefab);
						allNodes.Add (newNode);
					}
				}
				for (int i = 0; i < numNodes; ++i){
					allNodes[i].GetComponent<AVOWNode>().DeserialiseData(br);
				}
				
				if (topologyHasChanged){
					for (int i = 0; i < allComponents.Count; ++i){
						allComponents[i].GetComponent<AVOWComponent>().DeserialiseConnections(br);
					}
					for (int i = 0; i < allNodes.Count; ++i){
						allNodes[i].GetComponent<AVOWNode>().DeserialiseConnections(br);
					}
				}
				
				xMin = br.ReadSingle();
				xMax = br.ReadSingle();
				yMin = br.ReadSingle();
				yMax = br.ReadSingle();
				
				
				break;
			}
			
		}

	}
	
	public void Initialise(){
		topologyHasChanged = false;
	}
	
	
	// Place an new component between two existing nodes
	public void PlaceComponent(GameObject newGO, GameObject node0, GameObject node1){
		topologyHasChanged = true;

		newGO.transform.parent = transform;
		AVOWComponent newComponent = newGO.GetComponent<AVOWComponent>();
		newComponent.SetNode0(node0);
		newComponent.SetNode1(node1);
		newComponent.SetID(allComponents.Count);


		
		node0.GetComponent<AVOWNode>().components.Add (newGO );
		node1.GetComponent<AVOWNode>().components.Add (newGO );
		allComponents.Add (newGO );
	}
	
	public void FillAllResistors(){
		foreach(GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.resistanceAngle.Set(45);
		}
	}
	
	public void EnableAllLightening(){
		foreach(GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.EnableLightening(component.node0GO, true);
			component.EnableLightening(component.node1GO, true);
		}
	}
	
	
	public void RemoveComponent(GameObject obj){
		componentsToRemove.Add (obj);
	}
	
	// Really remove it
	void PrivateRemoveQueuedComponent(){
		foreach (GameObject obj in componentsToRemove){
			AVOWComponent component = obj.GetComponent<AVOWComponent>();
			GameObject node0 = component.node0GO;
			GameObject node1 = component.node1GO;
			
			// Remove the component
			allComponents.Remove(obj);
			node0.GetComponent<AVOWNode>().components.Remove (obj);
			node1.GetComponent<AVOWNode>().components.Remove (obj);
			GameObject.Destroy(obj);
			topologyHasChanged = true;
		}
		
		componentsToRemove.Clear();
		
	}
	
	public void ClearCircuit(){
		topologyHasChanged = true;
		foreach (GameObject go in allComponents){
			GameObject.Destroy(go);
		}
		foreach (GameObject go in allNodes){
			GameObject.Destroy(go);
		}
		allComponents.Clear ();
		allNodes.Clear();
	}
	
	public void ForceComponentsToSize(){
		List<GameObject> listCopy = allComponents.GetRange(0, allComponents.Count);
		foreach (GameObject go in listCopy){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			float desVal = component.resistanceAngle.GetDesValue();
			component.resistanceAngle.Force(desVal);
			component.CheckForKillResistance();
			
		}
	}
	
	
	
	public bool HasHalfFinishedComponents(){
		foreach (GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (!MathUtils.FP.Feq (component.resistanceAngle.GetValue(), 45) && component.type == AVOWComponent.Type.kLoad){
				return true;
			}
		}
		return false; 
	}
	
	public GameObject FindVoltageSource(){
		return allComponents.Find(item => item.GetComponent<AVOWComponent>().type == AVOWComponent.Type.kVoltageSource);
	}
	
	public float GetTotalWidth(){
		if (allComponents.Count == 0) return 0;
		AVOWComponent component = allComponents[0].GetComponent<AVOWComponent>();
		if (component.type != AVOWComponent.Type.kVoltageSource){
			Debug.LogError ("GetTotalWidth - first component is not the voltage source");
		}
		return Mathf.Abs (component.fwCurrent);
	}
	
	// Merges all connections to node0
	public void MergeNodes(GameObject node0GO, GameObject node1GO){
		
		AVOWNode node0 = node0GO.GetComponent<AVOWNode>();
		AVOWNode node1 = node1GO.GetComponent<AVOWNode>();
		
		// Replace all the instances of node1 with node 0 in the components attached to node1
		// Add add the components to node0's list of components
		foreach(GameObject go in node1.components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.ReplaceNode(node1GO, node0GO);
			node0.components.Add (go);
		}
		
		// Delete node 1 - since it is no longet used
		allNodes.Remove (node1GO);
		GameObject.Destroy(node1GO);
		
		
	}
	


	// SPlit the nodeToSplit into two new nodes
	// we attach all apart from the anchored component to the new node
	public GameObject SplitNode(GameObject nodeToSplitGO, AVOWComponent movedComponent){

//		Debug.Log ("Split Node: " + nodeToSplitGO.GetComponent<AVOWNode>().GetID() + " - movedComponent = " + movedComponent.GetID ());		
		GameObject newNodeGO = AddNode();
		
		nodeToSplitGO.GetComponent<AVOWNode>().components.Remove(movedComponent.gameObject);
		movedComponent.ReplaceNode(nodeToSplitGO, newNodeGO);
		
		newNodeGO.GetComponent<AVOWNode>().components.Add (movedComponent.gameObject);
		
		newNodeGO.GetComponent<AVOWNode>().splitFromNode = nodeToSplitGO;
		nodeToSplitGO.GetComponent<AVOWNode>().splitFromNode = newNodeGO;
//		Debug.Log ("New node " + newNodeGO.GetComponent<AVOWNode>().GetID () + " has splitFromNoe = " + nodeToSplitGO.GetComponent<AVOWNode>().GetID ());
		return newNodeGO;
	}
	
	public void UnselectAllNodes(){
		foreach (GameObject go in allNodes){
			AVOWNode node = go.GetComponent<AVOWNode>();
			node.SetSelected(false);
		}
	}
	
	
	public GameObject AddNode(){
		topologyHasChanged = true;
		GameObject newNodeGO = GameObject.Instantiate(nodePrefab);
		newNodeGO.transform.parent = transform;
		
		allNodes.Add (newNodeGO);
		return newNodeGO;
	}
	
	// There are a number of tests we should perform to ensure we have a valid graph
	public bool ValidateGraph(){
		int numVoltageSources = 0;
		int numLoads = 0;
		foreach (GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource){
				numVoltageSources++;
			}
			else{
				numLoads++;
			}
		}
		// We should have exactly one voltage source and at least one load
		if (numVoltageSources != 1) return false;
		if (numLoads == 0 ) return false;
		return true;
		
	}
	
	
	// Clear any additional conneciton points on the ndoes due to cursors attaching to them
	public void ClearAdditionalConnectionPoints(){
		foreach (GameObject go in allNodes){
			AVOWNode node = go.GetComponent<AVOWNode>();
			node.addConnPos = -1;
		}
	}
	
	
	
	public void ClearLayoutFlags(){
		foreach (GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.h0 = -1;
			component.h0LowerBound = kMinLowerBound;
			component.h0UpperBound = kMaxUpperBound;
			component.hWidth = -1;
			component.inNodeOrdinal = AVOWComponent.kOrdinalUnordered;
			component.outNodeOrdinal = AVOWComponent.kOrdinalUnordered;	
			component.inLocalH0 = -1;
			component.outLocalH0 = -1;
			component.inNodeGO = null;
			component.outNodeGO = null;
		}
		foreach (GameObject nodeGO in allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			node.h0 = -1;
			node.h0LowerBound = kMinLowerBound;
			node.h0UpperBound = kMaxUpperBound;
			node.inOrdinalledWidth = 0;
			node.outOrdinalledWidth = 0;
			
			node.hWidth = -1;			
			// These lists are filled with the components with current flowing in and out of the node (those which have been 
			// ordered will be first)
			node.inComponents = new List<GameObject>();
			node.outComponents = new List<GameObject>();
		}

	}

	
	public bool IsAllLayedOut(){
		int numComponentsLayedOut = 0;
		int numNodesLayedOut = 0;
		
//		foreach (GameObject go in allComponents){
//			AVOWComponent component = go.GetComponent<AVOWComponent>();
//			component.h0 = -1;
//			component.hWidth = -1;
//			component.node0Ordinal = AVOWComponent.kOrdinaUnordered;
//			component.node1Ordinal = AVOWComponent.kOrdinaUnordered;	
//		}
//		foreach (Node node in allNodes){
//			node.h0 = -1;
//			node.hWidth = -1;			
//			// These lists are filled with the components with current flowing in and out of the node (those which have been 
//			// ordered will be first)
//			node.inComponents = new List<GameObject>();
//			node.outComponents = new List<GameObject>();
//		}
//		
		foreach (GameObject go in allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.h0 >= 0 &&
			    component.hWidth >= 0 &&
			    component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered &&
			    component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered) numComponentsLayedOut++;
		}
		// Note that here, we don't check the lists of in and out components that their ordinal numbers have been set
		// as that would be covered int he loop above
		foreach (GameObject nodeGO in allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			
			if (node.h0 >= 0 &&
				node.hWidth >= 0) numNodesLayedOut++;
		}
		return numComponentsLayedOut == allComponents.Count && numNodesLayedOut == allNodes.Count;
	}
	
	public void ClearUIOrderedVisitedFlags(){
		foreach (GameObject componentGO in allComponents){
			componentGO.GetComponent<AVOWComponent>().uiVisitedIndex = -1;
		}		
	}
	
	// Return the number of Loads which are either 
	// completly there or on their way to being there
	public int GetNumConfirmedLoads(){
		int count = 0;
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (MathUtils.FP.Feq (component.resistanceAngle.GetDesValue(), 45)){
				count++;
			}
		}
		return count;
	}
	
	
	public bool HasNonInteractiveComponents(){
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (!component.isInteractive || component.IsDying())
				return true;
		}
		return false;
	}
	
	public void ClearVisitedFlags(){
		foreach (GameObject componentGO in allComponents){
			componentGO.GetComponent<AVOWComponent>().visited = false;
		}
		foreach (GameObject nodeGO in allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			
			node.visited = false;
		}
	}
	
	public void ClearDisabledFlags(){
		foreach (GameObject componentGO in allComponents){
			componentGO.GetComponent<AVOWComponent>().disable = false;
		}
	}	
	
	public void ClearLoopRecords(){
		foreach (GameObject componentGO in allComponents){
			componentGO.GetComponent<AVOWComponent>().ClearLoopRecords();
		}
	}
		
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	
	public bool HasUnconfirmedComponents(){
		return (GetNumConfirmedLoads() != allComponents.Count-1);
	
	}
	
	public void RenderUpdate(){
		foreach (GameObject go in allNodes){
			go.GetComponent<AVOWNode>().RenderUpdate();
		}
		foreach (GameObject go in allComponents){
			go.GetComponent<AVOWComponent>().RenderUpdate();
		}
	}
	
	public void GameUpdate(){
	
		// If all our components are "confirmed" then we get each component to save the positions of their
		// connectoin points - as these will be used by the UI when attaching to avoid oscillations
		if (!HasUnconfirmedComponents()){
			foreach (GameObject go in allComponents){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				component.BakeConfirmedConnectionPositions();
			}
		}
		
		lighteningPoints.transform.FindChild("LighteningPoint1").gameObject.SetActive(allComponents.Count == 1);
		lighteningPoints.transform.FindChild("LighteningPoint2").gameObject.SetActive(allComponents.Count == 1);
		
		float intensity = Mathf.Sin (0.9f + 0.1f * Mathf.Sin(3 * Time.time));
		lighteningPoints.transform.FindChild("LighteningPoint1").gameObject.GetComponent<Renderer>().material.SetFloat("_Intensity", intensity);
		lighteningPoints.transform.FindChild("LighteningPoint2").gameObject.GetComponent<Renderer>().material.SetFloat("_Intensity", intensity);
		
		foreach (GameObject go in allNodes){
			go.GetComponent<AVOWNode>().GameUpdate();
		}
		foreach (GameObject go in allComponents){
			go.GetComponent<AVOWComponent>().GameUpdate();
		}
		PrivateRemoveQueuedComponent();
		
		// Record the width of the graph (which is always the same as the current running through the cell
		yMin = 0;
		yMax = 1;
		if (allComponents.Count == 0){
			xMin = 0;
			xMax = 0;
		}
		else{
			xMin = 0;
			xMax = Mathf.Abs (allComponents[0].GetComponent<AVOWComponent>().fwCurrent);
		}
		
		TestIfValuesHaveChanged();
	}
	
	
	
}
