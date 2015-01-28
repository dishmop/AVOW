using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	static List<AVOWTab>	tabs = new List<AVOWTab>();
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	public GameObject cursorCubePrefab;
	public GameObject lighteningPrefab;
	
	GameObject cursorCube;
	GameObject lightening;
	Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();
	
	// Set to true if we should not allow anything else to be created just yet
	// do this when killing a component
	public bool lockCreation;
	
	
	AVOWTab selectedTab = null;
	AVOWTab overTab = null;
	
	AVOWNode secondarySelectedNode = null;
	AVOWComponent secondarySelectedComponent = null;
	public AVOWNode previousSecondarySelectedNode = null;
		

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
	
	
	void NewStart(){
		cursorCube = GameObject.Instantiate(cursorCubePrefab) as GameObject;
		lightening = GameObject.Instantiate(lighteningPrefab) as GameObject;
	
	}
	
	void Start(){
	
		NewStart();
	/*
		// Simple 3 resistors
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node2);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node0);
	*/
	
		/*
		// 4 at the top, one at the bottom
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node1);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
*/	
	/*
		// Misc
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		AVOWNode node3 = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node2);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node0);
		*/
		
		
		// 4 down the side, one next to them - then join them up
		// THIS ONE CAUSES AN ERROR!
		/*
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		AVOWNode node3 = graph.AddNode ();
		AVOWNode node4 = graph.AddNode ();
		
		// The cell
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node4, node0);
		
		// The 4 up the side
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node4);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node2);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node1);

		// the big one
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node4);
		
		// the joiner
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
		
		// Another joiner
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node4);
		*/
		
		/*
		//		
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node2);
		*/
	
		// Simple start
		AVOWGraph graph = AVOWGraph.singleton;

		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();

				
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node0GO);
		
		
		/*
		// Sneeky crossover
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		AVOWNode node3 = graph.AddNode ();
		AVOWNode node4 = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node1, node0);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node1);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node4, node1);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node4);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node4, node3);
		*/
		/*
		// Four in a block
		AVOWGraph graph = AVOWGraph.singleton;
		
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		AVOWNode node3 = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node2);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node3);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node0);
		*/
		/*
		AVOWNode node0 = graph.AddNode ();
		AVOWNode node1 = graph.AddNode ();
		AVOWNode node2 = graph.AddNode ();
		AVOWNode node3 = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node0);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node2);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);

		*/
		
		
		
//		
//		AVOWNode node0 = graph.AddNode ();
//		AVOWNode node1 = graph.AddNode ();
//		AVOWNode node2 = graph.AddNode ();
//		
//		GameObject[] resistors = new GameObject[3];
//		resistors[0] = GameObject.Instantiate(resistorPrefab) as GameObject;
//		resistors[1] = GameObject.Instantiate(resistorPrefab) as GameObject;
//		resistors[2] = GameObject.Instantiate(resistorPrefab) as GameObject;
//		
//		GameObject cell = GameObject.Instantiate(cellPrefab) as GameObject;
//
//		graph.PlaceComponent(resistors[0], node0, node1);
//		graph.PlaceComponent(resistors[1], node1, node2);
//		graph.PlaceComponent(resistors[2], node2, node1);
//		graph.PlaceComponent(cell, node0, node2);
//		
//		bool ok = graph.ValidateGraph();
//		if (!ok){
//			Debug.LogError ("built an invalid graph");
//		}
		AVOWSim.singleton.Recalc();
		
		
	}
	
	
	void NewUpdate(){
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		Vector3 oldCubePos = cursorCube.transform.position;
		mouseWorldPos.z = oldCubePos.z;
		cursorCube.transform.position = mouseWorldPos;
		
		// Rotate the cube a bit too
		cursorCube.transform.Rotate (new Vector3(1, 2, 4));
		
		// Find the node we are closest to
		float minDist = 100;
		AVOWNode minNode = null;
		Vector3 testPos = mouseWorldPos;
		Vector3 minPos = Vector3.zero;
		foreach (GameObject go in AVOWGraph.singleton.allNodes){
			AVOWNode node = go.GetComponent<AVOWNode>();
			
			// If inside the cruz of the node
			float thisDist = -1;
			Vector3 thisPos = Vector3.zero;
			if (testPos.x > node.h0 && testPos.x < node.h0 + node.hWidth){
				thisDist = Mathf.Abs(testPos.y - node.voltage);
				thisPos = new Vector3(testPos.x, node.voltage, testPos.z);
				
			}
			else{
				if (testPos.x <= node.h0){
					thisPos = new Vector3(node.h0, node.voltage, testPos.z);
				}
				else{
					thisPos = new Vector3(node.h0 + node.hWidth, node.voltage, testPos.z);
				}
				thisDist = (testPos - thisPos).magnitude;
			}
			if (thisDist < minDist){
				minNode = node;
				minDist = thisDist;
				minPos = thisPos;
				
			}
		}
		
		// If we have such a node then make som lightening to it
		if (minNode != null){
			//cursorCube.transform.position = minPos;
			lightening.GetComponent<Lightening>().startPoint = cursorCube.transform.position;
			lightening.GetComponent<Lightening>().endPoint = minPos;
			lightening.GetComponent<Lightening>().size = 0.2f;
			lightening.GetComponent<Lightening>().ConstructMesh();
			
			
		}
		
	}
	

	
	

	// Update is called once per frame
	void Update () {
		// Get the mouse position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		NewUpdate();
		
		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		if (buttonReleased){
			secondarySelectedNode = null;
			secondarySelectedComponent = null;
			previousSecondarySelectedNode = null;
			selectedTab = null;
			AVOWGraph.singleton.FillAllResistors();
		}
		if (buttonPressed || !buttonDown){
			selectedTab = null;
			secondarySelectedNode = null;
		}
		// If we do not have anything selected
		overTab = null;
		if (selectedTab == null){
			foreach (AVOWTab tab in tabs){
				bool isInside = tab.IsContaining(mouseWorldPos);
				tab.SetMouseInside(isInside);
				if (isInside){
					if (buttonPressed){
						
						selectedTab = tab;
					}
					overTab = tab;
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
			secondarySelectedComponent = null;
			foreach (AVOWTab tab in tabs){
				// if we are in our select tab, then do nothing
				if (tab == selectedTab) continue;
				
				bool isInside = tab.IsContaining(mouseWorldPos);
				// If we are inside this tab, find all the other tabs which are
				// part of this node - as we are "inside" all of them now too
				if (isInside){
					secondarySelectedNode = tab.GetNode();
					secondarySelectedComponent = tab.GetAVOWComponent();
					foreach (GameObject go in secondarySelectedNode.components){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						AVOWTab otherTab = null;
						if (component.node0GO == secondarySelectedNode){
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
		
		
		
		
//		Debug.Log ("secondarySelectedNode = " + 
//			(secondarySelectedNode!=null ? secondarySelectedNode.GetID() : "NULL") + " , previousSecondarySelectedNode = " + 
//			(previousSecondarySelectedNode!=null ? previousSecondarySelectedNode.GetID() : "NULL"));


		// if we have a new secondarySelectNode then need to make (or destroy) one of the components
//		Debug.Log ("secondarySelectedNode = " + ((secondarySelectedNode != null) ? secondarySelectedNode.GetID():"NULL") + "...previousSecondarySelectedNode = " + ((previousSecondarySelectedNode != null) ? 
//			previousSecondarySelectedNode.GetID() : "NULL") + "....selectedComponent = " + ((selectedTab != null) ? selectedTab.GetAVOWComponent ().GetID() : "NULL") + 
//		           "...secondarySelectedComponent = " + ((secondarySelectedComponent != null) ? secondarySelectedComponent.GetID() : "NULL"));
		
//		if (secondarySelectedNode != null && previousSecondarySelectedNode != null && secondarySelectedNode.GetComponent<AVOWNode>().splitFromNode != null && secondarySelectedNode.GetComponent<AVOWNode>().splitFromNode.GetComponent<AVOWNode>() == previousSecondarySelectedNode){
//			previousSecondarySelectedNode = secondarySelectedNode;
//		}
		
		if (secondarySelectedNode != previousSecondarySelectedNode){
			// If we were previously on a node, then we need to remove the last component we added
			if (previousSecondarySelectedNode != null){
				Debug.Log ("Undo last command");
				UndoLastCommand();
				previousSecondarySelectedNode  = secondarySelectedNode;
			}
			
			// If our currently selected one is a node, then we need to create a new component
			if (secondarySelectedNode != null && !lockCreation){
				// Are we trying to split a node
				if (selectedTab.GetNode() == secondarySelectedNode){
					AVOWCommand command = new AVOWCommandSplitAddComponent(secondarySelectedNode.gameObject, selectedTab.GetAVOWComponent ().gameObject, resistorPrefab);
					IssueCommand(command);

					
				}
				// or simple put a new component accross existing nodes
				else{
					AVOWCommand command = new AVOWCommandAddComponent(selectedTab.GetNode().gameObject, secondarySelectedNode.gameObject, resistorPrefab);
					IssueCommand(command);
					
				}
				
			}
			if (!lockCreation)
				previousSecondarySelectedNode  = secondarySelectedNode;
		}		
		
		// If we have a selected tab, then figure out if any tabs need to be disabled
		// TO DO		
	}
	
	void IssueCommand(AVOWCommand command){
		command.Execute();
		commands.Push(command);
		
	}
	
	void UndoLastCommand(){
		AVOWCommand command = commands.Pop ();
		command.UndoStep ();
	}
	
	void OnGUI(){
		if (overTab){
			GUI.Label (new Rect(10, 10, Screen.width, 30), "overTab Tab: node = " + overTab.GetNode().GetID() + ", compoonent = " + overTab.GetAVOWComponent().GetID());
		}
	}
}
