﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	static List<AVOWTab>	tabs = new List<AVOWTab>();
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();
	
	
	AVOWTab selectedTab = null;
	AVOWTab overTab = null;
	
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
//		AVOWGraph graph = AVOWGraph.singleton;
//		
//		AVOWGraph.Node node0 = graph.AddNode ();
//		AVOWGraph.Node node1 = graph.AddNode ();
//		AVOWGraph.Node node2 = graph.AddNode ();
//		AVOWGraph.Node node3 = graph.AddNode ();
//		AVOWGraph.Node node4 = graph.AddNode ();
//		
//		
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node2);
//		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node1, node0);
//		
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node1);
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);
//		
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node4, node1);
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node4);
//		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node4, node3);
		
		AVOWGraph graph = AVOWGraph.singleton;

		AVOWGraph.Node node0 = graph.AddNode ();
		AVOWGraph.Node node1 = graph.AddNode ();

				
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node0, node1);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node1);
		
		
		/*
		AVOWGraph.Node node0 = graph.AddNode ();
		AVOWGraph.Node node1 = graph.AddNode ();
		AVOWGraph.Node node2 = graph.AddNode ();
		AVOWGraph.Node node3 = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1, node0);
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0, node2);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node1);
		
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node0);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2, node3);

		*/
		
		
		
//		
//		AVOWGraph.Node node0 = graph.AddNode ();
//		AVOWGraph.Node node1 = graph.AddNode ();
//		AVOWGraph.Node node2 = graph.AddNode ();
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
	
	

	// Update is called once per frame
	void Update () {
		// Get the mouse position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		if (buttonReleased){
			secondarySelectedNode = null;
			previousSecondarySelectedNode = null;
			selectedTab = null;
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
			foreach (AVOWTab tab in tabs){
				// if we are in our select tab, then do nothing
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
		
		
		
		
//		Debug.Log ("secondarySelectedNode = " + 
//			(secondarySelectedNode!=null ? secondarySelectedNode.GetID() : "NULL") + " , previousSecondarySelectedNode = " + 
//			(previousSecondarySelectedNode!=null ? previousSecondarySelectedNode.GetID() : "NULL"));


		// if we have a new secondarySelectNode then need to make (or destroy) one of the components
		if (secondarySelectedNode != previousSecondarySelectedNode){
			// If we were previously on a node, then we need to remove the last component we added
			if (previousSecondarySelectedNode != null){
				Debug.Log ("Undo last command");
				UndoLastCommand();
			}
			
			// If our currently selected one is a node, then we need to create a new component
			if (secondarySelectedNode != null){
				// Are we trying to split a node
				if (selectedTab.GetNode() == secondarySelectedNode){
					AVOWCommand command = new AVOWCommandSplitAddComponent(secondarySelectedNode, selectedTab.GetAVOWComponent ().gameObject, resistorPrefab);
					IssueCommand(command);

					
				}
				// or simple put a new component accross existing nodes
				else{
					AVOWCommand command = new AVOWCommandAddComponent(selectedTab.GetNode(), secondarySelectedNode, resistorPrefab);
					IssueCommand(command);
					
				}
				
			}
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
