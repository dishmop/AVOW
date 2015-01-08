using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWGraph : MonoBehaviour {

	public static AVOWGraph singleton = null;

	// the nodes in the graph are simply numbered
	
	public class Node{
		public List<GameObject> components = new List<GameObject>();
		public bool visited;
		public int id;
		
		// simulation data
		public float voltage;
		
		// Visualisation data
		public float h0;
		public float hWidth;
		
		public string GetID(){
			return id.ToString ();
		}
		
	}
	
	public List<Node> allNodes = new List<Node>();
	public List<GameObject> allComponents = new List<GameObject>();
	
	

	// Place an new component between two existing nodes
	public void PlaceComponent(GameObject newGO, Node node0, Node node1){

		newGO.transform.parent = transform;
		AVOWComponent newComponent = newGO.GetComponent<AVOWComponent>();
		newComponent.node0 = node0;
		newComponent.node1 = node1;
		newComponent.SetHOrder(allComponents.Count);

		
		node0.components.Add (newGO );
		node1.components.Add (newGO );
		allComponents.Add (newGO );
	}
	
	public GameObject FindVoltageSource(){
		return allComponents.Find(item => item.GetComponent<AVOWComponent>().type == AVOWComponent.Type.kVoltageSource);
	}
	


	
	public Node AddNode(){
		Node newNode = new Node();
		newNode.id = allNodes.Count;
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
