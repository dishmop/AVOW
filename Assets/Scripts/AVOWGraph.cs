using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWGraph : MonoBehaviour {

	public static AVOWGraph singleton = null;

	// the nodes in the graph are simply numbered
	
	public class Node{
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
		
		public float hWidth;
		
		// These lists are filled with the components with current flowing in and out of the node (those which have been 
		// ordered will be first)
		public List<GameObject> inComponents;
		public List<GameObject> outComponents;
		

		public string GetID(){
			return id.ToString ();
		}
		
		static int staticCount = 0;
		
		public Node(){
			id = staticCount++;
		}
	}
	
	public int maxNodeId = -1;
	
	public static int kMinLowerBound = 0;
	public static int kMaxUpperBound = 9999;
	
	public List<Node> allNodes = new List<Node>();
	public List<GameObject> allComponents = new List<GameObject>();
	
	

	// Place an new component between two existing nodes
	public void PlaceComponent(GameObject newGO, Node node0, Node node1){

		newGO.transform.parent = transform;
		AVOWComponent newComponent = newGO.GetComponent<AVOWComponent>();
		newComponent.SetNode0(node0);
		newComponent.SetNode1(node1);
		newComponent.SetID(allComponents.Count);


		
		node0.components.Add (newGO );
		node1.components.Add (newGO );
		allComponents.Add (newGO );
	}
	
	public void RemoveComponent(GameObject obj){
		AVOWComponent component = obj.GetComponent<AVOWComponent>();
		AVOWGraph.Node node0 = component.node0;
		AVOWGraph.Node node1 = component.node1;

		// Remove the component
		allComponents.Remove(obj);
		node0.components.Remove (obj);
		node1.components.Remove (obj);
		GameObject.Destroy(obj);
		
	}
	
	
	
	
	
	public GameObject FindVoltageSource(){
		return allComponents.Find(item => item.GetComponent<AVOWComponent>().type == AVOWComponent.Type.kVoltageSource);
	}
	
	// Merges all connections to node0
	public void MergeNodes(Node node0, Node node1){
		
		// Replace all the instances of node1 with node 0 in the components attached to node1
		// Add add the components to node0's list of components
		foreach(GameObject go in node1.components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.ReplaceNode(node1, node0);
			node0.components.Add (go);
		}
		
		// Delete node 1 - since it is no longet used
		allNodes.Remove (node1);
		
		
	}
	


	// SPlit the nodeToSplit into two new nodes
	// we attach all apart from the anchored component to the new node
	public Node SplitNode(Node nodeToSplit, AVOWComponent movedComponent){
//		Node newNode = AddNode();
//		
//		foreach (GameObject go in nodeToSplit.components){
//			AVOWComponent component = go.GetComponent<AVOWComponent>();
//			if (component != anchoredComponent){
//				component.ReplaceNode(nodeToSplit, newNode);
//				
//				newNode.components.Add (go);
//			}
//		}
//		nodeToSplit.components.Clear();
//		nodeToSplit.components.Add (anchoredComponent.gameObject);
//		
//		
//		return newNode;
		
		Node newNode = AddNode();
		
		nodeToSplit.components.Remove(movedComponent.gameObject);
		movedComponent.ReplaceNode(nodeToSplit, newNode);
		
		newNode.components.Add (movedComponent.gameObject);
		
		return newNode;
	}
	
	
	public Node AddNode(){
		Node newNode = new Node();
		maxNodeId = Mathf.Max (maxNodeId, newNode.id);
		allNodes.Add (newNode);
		return newNode;
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
			component.inNode = null;
			component.outNode = null;
		}
		foreach (Node node in allNodes){
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
		foreach (Node node in allNodes){
			if (node.h0 >= 0 &&
				node.hWidth >= 0) numNodesLayedOut++;
		}
		return numComponentsLayedOut == allComponents.Count && numNodesLayedOut == allNodes.Count;
	}
	
	
	public void ClearVisitedFlags(){
		foreach (GameObject componentGO in allComponents){
			componentGO.GetComponent<AVOWComponent>().visited = false;
		}
		foreach (Node node in allNodes){
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
	
	
	
}
