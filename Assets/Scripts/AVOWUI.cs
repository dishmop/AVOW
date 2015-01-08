using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	static List<AVOWTab>	tabs = new List<AVOWTab>();
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	
	AVOWTab selectedTab = null;
	
	AVOWGraph.Node secondarySelectedNode = null;
	AVOWGraph.Node previousSecondarySelectedNode = null;
		

	public void RegisterTab(AVOWTab tab){
		tabs.Add(tab);
	}
	
	public void UnregisterTab(AVOWTab tab){
		tabs.Remove(tab);
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	
	void Start(){
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWGraph.Node node0 = graph.AddNode ();
		AVOWGraph.Node node1 = graph.AddNode ();
		AVOWGraph.Node node2 = graph.AddNode ();
		
		GameObject[] resistors = new GameObject[3];
		resistors[0] = GameObject.Instantiate(resistorPrefab) as GameObject;
		resistors[1] = GameObject.Instantiate(resistorPrefab) as GameObject;
		resistors[2] = GameObject.Instantiate(resistorPrefab) as GameObject;
		
		GameObject cell = GameObject.Instantiate(cellPrefab) as GameObject;

		graph.PlaceComponent(resistors[0], node0, node1);
		graph.PlaceComponent(resistors[1], node1, node2);
		graph.PlaceComponent(resistors[2], node2, node1);
		graph.PlaceComponent(cell, node0, node2);
		
		bool ok = graph.ValidateGraph();
		if (!ok){
			Debug.LogError ("built an invalid graph");
		}
		
		
	}
	
	

	// Update is called once per frame
	void Update () {
		// Get the mouse position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		if (buttonPressed || !buttonDown){
			selectedTab = null;
			secondarySelectedNode = null;
		}
		// If we do not have anything selected
		if (selectedTab == null){
			foreach (AVOWTab tab in tabs){
				bool isInside = tab.IsContaining(mouseWorldPos);
				tab.SetMouseInside(isInside);
				if (isInside){
					if (buttonPressed){
						
						selectedTab = tab;
					}
				}
				tab.SetSelected(tab == selectedTab);
			}
		}
		// If we do have something selected, then we have different logic
		else{
			foreach (AVOWTab tab in tabs){
				tab.SetMouseInside(false);
			}		
			secondarySelectedNode = null;	
			foreach (AVOWTab tab in tabs){
				// if we are in our select tabm then do nothing
				if (tab == selectedTab) continue;
				
				bool isInside = tab.IsContaining(mouseWorldPos);
				// If we are inside this tab, find all the other tabs which are
				// part of this node - as we are "inside" all of them now too
				if (isInside){
					secondarySelectedNode = tab.GetNode();
					foreach (GameObject go in secondarySelectedNode.components){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						AVOWTab otherTab = null;
						if (component.node0 == secondarySelectedNode){
							otherTab = go.transform.FindChild("LowerTab").GetComponent<AVOWTab>();
						}
						else{
							otherTab = go.transform.FindChild("UpperTab").GetComponent<AVOWTab>();
						}
						otherTab.SetMouseInside(true);
					}
				}

			}
		}
		// if we have a new secondarySelectNode then need to make (or destroy) one of the components
		if (secondarySelectedNode != previousSecondarySelectedNode){
			AVOWGraph graph = AVOWGraph.singleton;
			// If we were previously on a node, then we need to remove the last component we added
			if (previousSecondarySelectedNode != null){
				graph.RemoveLastComponent();
			}
			
			// If our currently selected one is a node, then we need to create a new component
			if (secondarySelectedNode != null){
				// Are we trying to split a node
				if (selectedTab.GetNode() == secondarySelectedNode){
				}
				// or simple put a new component accross existing nodes
				else{
					GameObject newComponent = GameObject.Instantiate(resistorPrefab) as GameObject;
					newComponent.GetComponent<AVOWComponent>().resistance.Force(100);
					newComponent.GetComponent<AVOWComponent>().resistance.Set(1);
					
					graph.PlaceComponent(newComponent, selectedTab.GetNode(), secondarySelectedNode);
				}
				
			}
			previousSecondarySelectedNode  = secondarySelectedNode;
		}		
		
		// If we have a selected tab, then figure out if any tabs need to be disabled
		

	
	}
}
