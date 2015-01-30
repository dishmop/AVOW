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
	
	int debugCounter = 0;
	
	public float maxLighteningDist;
	
	class Action{
		public Action(GameObject conn0Node, GameObject conn1Node, GameObject conn1Component, bool isNodeGap){
			this.conn0Node = conn0Node;
			this.conn1Node = conn1Node;
			this.conn1Component = conn1Component;
			this.isNodeGap = isNodeGap;
		}
		
		public  bool IsEqual(System.Object obj)
		{
			// If parameter is null return false.
			if (obj == null)
			{
				return false;
			}
			
			// If parameter cannot be cast to Action return false.
			Action p = obj as Action;
			if ((System.Object)p == null)
			{
				return false;
			}
			
			// Return true if the fields match:
			return (conn0Node == p.conn0Node) && (conn1Node == p.conn1Node) && (conn1Component == p.conn1Component) && (isNodeGap == p.isNodeGap);
		}
				
		public GameObject conn0Node;
		public GameObject conn1Node;
		public GameObject conn1Component;

		// Otherwise it is a component gap
		public bool	isNodeGap;
	};
	
	Action	currentAction;
	Action  lastAction;
	
	float uiZPos;
	
	GameObject cursorCube;
	GameObject lightening0GO;
	GameObject lightening1GO;
	Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();
	
	Vector3 mouseWorldPos;
	
	//UI state stuff
	GameObject connection0Node;
	GameObject connection1Node;
	bool connectionIsNodeGap;
	GameObject connection1Component;
	Vector3 connection0Pos;
	Vector3 connection1Pos;
	
	enum State {
		kFree,
		kHeldNode,
		kHeldOpen,
		kHeldInside
		
	};
	
	
	State state = State.kFree;
	
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
		lightening0GO = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening0GO.transform.parent = transform;
		lightening1GO = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening1GO.transform.parent = transform;
		
		uiZPos = transform.position.z;
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
		
		// Complex start
		AVOWGraph graph = AVOWGraph.singleton;
		
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		GameObject node2GO = graph.AddNode ();
		GameObject node3GO = graph.AddNode ();		
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2GO, node0GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node3GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node3GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node3GO, node0GO);
		
		
	/*
		// Simple start
		AVOWGraph graph = AVOWGraph.singleton;

		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();

				
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node0GO);
		*/
		
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
	
	void ClearConnection0Data(){
		connection0Pos = Vector3.zero;
		connection0Node = null;
	}
	
	void ClearConnection1Data(){
		connection1Pos = Vector3.zero;
		connection1Node = null;
		connection1Component = null;
	}
	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	float FindClosestComponent(Vector3 pos, AVOWNode node, GameObject currentComponent, out GameObject closestComponent, out Vector3 closestPos){
		// initialise outputs
		closestComponent = null;
		closestPos = Vector3.zero;
		float minDist = maxLighteningDist;
		
		// Make a copy of the list of compoents
		List<GameObject> components = node.components.GetRange(0, node.components.Count);
		
		// Check if any of the components are non-interactive, and, if they are, take the node
		// at their other end and add any of the compoments attached to that node
//		foreach(GameObject go in node.components){
//			AVOWComponent component = go.GetComponent<AVOWComponent>();
//			if (!component.isInteractive){
//				GameObject otherNode = component.GetOtherNode(node.gameObject);
//				foreach (GameObject otherComponentGO in otherNode.GetComponent<AVOWNode>().components){
//					if (otherComponentGO != component){
//						components.Add(otherComponentGO);
//					}
//				}
//			}
//		}
		if (currentComponent != null){
			components.Add(currentComponent);
		}
		
		
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			
			if (!component.isInteractive) continue;
			
			float thisDist = 0;
			Vector3 thisPos = Vector3.zero;
			// Check which of the two connectors to use
			if (node == component.node0GO.GetComponent<AVOWNode>()){
				thisPos = component.GetConnectionPos0();
			}
			else if (node == component.node1GO.GetComponent<AVOWNode>()){
				thisPos = component.GetConnectionPos1();
			}
			// If it is neither - then check if etierh of the nodes are non-interactive, if so, then it is that one
			else if (!component.node0GO.GetComponent<AVOWNode>().isInteractive){
				thisPos = component.GetConnectionPos0();
			}
			else if (!component.node1GO.GetComponent<AVOWNode>().isInteractive){
				thisPos = component.GetConnectionPos1();
			}
			else{
				continue;
			}
			thisPos.z = uiZPos;
			thisDist = (thisPos - pos).magnitude;
			
			// If this is the current Component, reduce the distance (for the purposes of hyserisis)
			if (go == currentComponent){
				thisDist *= 0.8f;
			}
			
			if (thisDist < minDist){
				minDist = thisDist;
				closestComponent = go;
				closestPos = thisPos;

			}
			
		}	
		return closestComponent ? minDist : maxLighteningDist;
	}
	
	void FindClosestPointOnNode(Vector3 pos, AVOWNode node, out Vector3 closestPos){

		closestPos = Vector3.zero;

		// If inside the h-range of the node
		if (pos.x > node.h0 && pos.x < node.h0 + node.hWidth){
			closestPos = new Vector3(pos.x, node.voltage, uiZPos);
			
		}
		else{
			if (pos.x <= node.h0){
				closestPos = new Vector3(node.h0, node.voltage, uiZPos);
			}
			else{
				closestPos = new Vector3(node.h0 + node.hWidth, node.voltage, uiZPos);
			}
		}
	}

	
	float FindClosestNode(Vector3 pos, GameObject ignoreNode, out GameObject closestNode, out Vector3 closestPos){
		// Initialise return values
		closestNode = null;
		closestPos = Vector3.zero;
		float minDist = maxLighteningDist;
		
		foreach (GameObject go in AVOWGraph.singleton.allNodes){
		
			if (go == ignoreNode) continue;
			
			AVOWNode node = go.GetComponent<AVOWNode>();
			
			if (!node.isInteractive) continue;
			
			// If inside the h-range of the node
			float thisDist = -1;
			Vector3 thisPos = Vector3.zero;
			if (pos.x > node.h0 && pos.x < node.h0 + node.hWidth){
				thisDist = Mathf.Abs(pos.y - node.voltage);
				thisPos = new Vector3(pos.x, node.voltage, uiZPos);
				
			}
			else{
				if (pos.x <= node.h0){
					thisPos = new Vector3(node.h0, node.voltage, uiZPos);
				}
				else{
					thisPos = new Vector3(node.h0 + node.hWidth, node.voltage, uiZPos);
				}
				thisDist = (pos - thisPos).magnitude;
			}
			if (thisDist < minDist){
				minDist = thisDist;
				closestNode = go;
				closestPos = thisPos;
			}
		}	
		return closestNode ? minDist : maxLighteningDist;
	}
	
	
	void NewUpdate(){
		// Calc the mouse posiiton on world spave
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		// Get the mouse buttons
		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		// Set the cursor cubes position
		Vector3 oldCubePos = cursorCube.transform.position;
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;
		
		// Do connection logic
		switch (state){
			// We are not holding on to anything
			case State.kFree:
			{
				//ClearConnection0Data();
				//ClearConnection1Data();
				FindClosestNode(mouseWorldPos, null, out connection0Node, out connection0Pos);
				
				// Only if we are connected to a node go we then test if we can connect to something else
				if (connection0Node != null){
					// Test if we are near a component connector
					float compDist = FindClosestComponent(mouseWorldPos, connection0Node.GetComponent<AVOWNode>(), connection1Component, out connection1Component, out connection1Pos);
					
					// Test if we are near a node
					GameObject nodeGO = null;
					Vector3 nodePos = Vector3.zero;
					float nodeDist = FindClosestNode(mouseWorldPos, connection0Node, out nodeGO, out nodePos);
					
					// Connect us to the closest of the two
					if (connection0Node != null && nodeDist < compDist){
						connection1Component = null;
						connection1Node = nodeGO;
						connection1Pos = nodePos;
						connectionIsNodeGap = true;
					}
					else{
						connectionIsNodeGap = false;
					}
					
					// If the button is pressed and we have a node, we need to change state
					if (buttonPressed){
						state = State.kHeldNode;
					}
				}
				
				break;
			}
			case State.kHeldNode:
			{
				//ClearConnection1Data();
				FindClosestPointOnNode(mouseWorldPos, connection0Node.GetComponent<AVOWNode>(), out connection0Pos);
				
				// Test if we are near a component connector
				Vector3 compPos = Vector3.zero;
				GameObject compGO = null;
				float compDist = FindClosestComponent(mouseWorldPos, connection0Node.GetComponent<AVOWNode>(), connection1Component,  out compGO, out compPos);
				
				// Test if we are near a node
				GameObject nodeGO = null;
				Vector3 nodePos = Vector3.zero;
				float nodeDist = FindClosestNode(mouseWorldPos, connection0Node, out nodeGO, out nodePos);
				
				// Connect us to the closest of the two
				if (nodeDist < compDist){
					connection1Component = null;
					connection1Node = nodeGO;
					connection1Pos = nodePos;
					connectionIsNodeGap = true;
				}	
				else{
					connection1Node = null;
					connection1Component = compGO;
					connection1Pos = compPos;
					connectionIsNodeGap = false;
				}

								
				// If the button is released, we need to change state
				if (!buttonDown){
					state = State.kFree;
				}
				break;
			}
			case State.kHeldOpen:
			{
				Vector3 node0Pos = Vector3.zero;
				FindClosestPointOnNode(mouseWorldPos, connection0Node.GetComponent<AVOWNode>(), out node0Pos);
				
				// Check how close we are to the thing we are already attached to
				float minDist = (connection1Pos - mouseWorldPos).magnitude;
				
				// Test if we are near a component connector (we store the position in a different variable so we can lerp
				// to the correction position.
				GameObject conn1GO = null;
				Vector3 conn1Pos = Vector3.zero;
				float compDist = FindClosestComponent(mouseWorldPos, connection0Node.GetComponent<AVOWNode>(), connection1Component, out conn1GO, out conn1Pos);
				
				// Test if we are near a node
				GameObject node1GO = null;
				Vector3 node1Pos = Vector3.zero;
				float nodeDist = FindClosestNode(mouseWorldPos, connection0Node, out node1GO, out node1Pos);
				
				bool newConnection1 = false;
				if (conn1GO != null && compDist < minDist){
					if (connection1Component != conn1GO){
						connection1Component = conn1GO;
						connection1Node = null;
						newConnection1 = true;
						Debug.Log ("kHeldOpen: - new Component - " + connection1Component.GetComponent<AVOWComponent>().GetID());
					}
					connectionIsNodeGap = false;
					connection1Pos = conn1Pos;
					minDist = compDist;
				}
				
				if (node1GO != null && nodeDist < minDist){
					if (connection1Node != node1GO){
						connection1Component = null;
						connection1Node = node1GO;
						newConnection1 = true;
						connectionIsNodeGap = true;
						Debug.Log ("kHeldOpen: - new Node - " + connection1Node.GetComponent<AVOWNode>().GetID());
					}
					connection1Pos = node1Pos;
					minDist = nodeDist;
				}	
				
				// Test if we are no longer attached to the node we were originally
				if (newConnection1){
					state = State.kHeldNode;

					++debugCounter;
				}
				// If we are still attached, then set the connection points to the connection
				// points of the new compment we made
				else{
					// If we are holding a component
					if (currentAction.isNodeGap){
						connection0Pos = node0Pos;
						connection1Pos = node1Pos;
					}
					else{
						AVOWComponent newComp = currentAction.conn1Component.GetComponent<AVOWComponent>();
						
						connection0Pos = node0Pos;
						//connection1Pos = Vector3.Lerp(connection1Pos, new Vector3( newComp.h0 + 0.5f * newComp.hWidth, node1Pos.y, node1Pos.z), 1f);
					}	
				}
				
				if (currentAction.isNodeGap){
				}
				
				
				
				// If the button is released, then we need to finish off the component
				if (!buttonDown){
//					commands.Peek().ExecuteStep();
					state = State.kFree;
				}
				
				break;
			}
			case State.kHeldInside:{
				break;
			}
			
		}
		// Record what we need to define the action associated with this state
		lastAction = currentAction;
		currentAction = DefineAction();
		
	}
	
	Action DefineAction(){
		switch(state){
			case (State.kFree):{
				return null;
			}
			case (State.kHeldNode):{
				if (connection1Node == null && connection1Component == null) 
					return null;
				else{
					return new Action (connection0Node, connection1Node, connection1Component, connectionIsNodeGap);
				}
			}
			// Just keep the action as it is!
			case (State.kHeldOpen):{
				return currentAction;
			}
		}
		return null;
	}
	
	bool ActionHasChange(){
		if (currentAction == null && lastAction == null) return false;
		
		if (currentAction == null && lastAction != null) return true;
		
		if (currentAction != null && lastAction == null) return true;
		
		// So neither are null
		return !currentAction.IsEqual(lastAction);
		
	}
	
	void NewUpdateVisuals(){
	
		// If we have changed the things we are pointing at
		if (ActionHasChange()){
			if (lastAction != null){
				Debug.Log ("UndoLastUnfinishedCommand");
				UndoLastUnfinishedCommand();
			}
			if (currentAction != null){
				if (currentAction.isNodeGap){
					AVOWCommandAddComponent command = new AVOWCommandAddComponent(currentAction.conn0Node , currentAction.conn1Node, resistorPrefab);
					IssueCommand(command);

					currentAction.conn1Component = command.GetNewComponent();
					currentAction.conn1Node = command.GetNewNode();	// null
					lastAction = currentAction;
					
					state = State.kHeldOpen;
				}
				// Otherwise it is a component
				else{
					// check is we can legitimately do this (sometimes we can't because we are connected to a node which is disappearing)
					AVOWComponent testComponent = currentAction.conn1Component.GetComponent<AVOWComponent>();
					if (testComponent.node0GO == currentAction.conn0Node || testComponent.node1GO == currentAction.conn0Node){
						AVOWCommandSplitAddComponent command = new AVOWCommandSplitAddComponent(currentAction.conn0Node, currentAction.conn1Component, resistorPrefab);
						IssueCommand(command);
						// Our current "action" is now meaninless as the comoment is not connected to the node anymore
						// So adjust our "last action" to make sense in this new context
						currentAction.conn1Component = command.GetNewComponent();
						currentAction.conn1Node = command.GetNewNode();
						
						lastAction = currentAction;
						
						state = State.kHeldOpen;
					}
					
				}
			}
		}
	
		// Lightening to connection 0 - which is always a node
		if (connection0Node != null){
			lightening0GO.SetActive(true);
			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
			
			lightening0.startPoint = mouseWorldPos;
			lightening0.endPoint = connection0Pos;
			lightening0.size =  (state == State.kFree) ? 0.1f : 0.4f;;
			lightening0.ConstructMesh();
		}
		else{
			lightening0GO.SetActive(false);
		}
		
		// Lightening to connection 1 - which may be a component or a node
		AVOWGraph.singleton.EnableAllLightening();
		if (connection1Component != null || connection1Node != null){
			lightening1GO.SetActive(true);
			Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
			lightening1.startPoint = mouseWorldPos;
			lightening1.endPoint = connection1Pos;
			lightening1.size = 0.1f;
			lightening1.ConstructMesh();
			
			// Also need to hide the lightening from the compoment to the node
			if (connection1Component != null){
				connection1Component.GetComponent<AVOWComponent>().EnableLightening(connection0Node, false);
			}
		}
		else{
			lightening1GO.SetActive(false);
		}		
		
		// If we are connected to something then rotate the cube a bit
		if (connection0Node != null || connection1Node != null){
			cursorCube.transform.Rotate (new Vector3(1, 2, 4));
		}
		
	}

	void OldNewUpdate()		{
		
		
		
//
//
//		if (state == State.kNodeHeld){
//			minNode = fixedNodeGO.GetComponent<AVOWNode>();
//			
//			// If inside the cruz of the node
//			if (testPos.x > minNode.h0 && testPos.x < minNode.h0 + minNode.hWidth){
//				minDist = Mathf.Abs(testPos.y - minNode.voltage);
//				minNodePos = new Vector3(testPos.x, minNode.voltage, testPos.z);
//				
//			}
//			else{
//				if (testPos.x <= minNode.h0){
//					minNodePos = new Vector3(minNode.h0, minNode.voltage, testPos.z);
//				}
//				else{
//					minNodePos = new Vector3(minNode.h0 + minNode.hWidth, minNode.voltage, testPos.z);
//				}
//				minDist = (testPos - minNodePos).magnitude;
//			}
//		}

//		// If we are connected to a node, see if we can also  connect to one of the 
//		// Resistors connected to this node
//		Vector3 minComponentPos = Vector3.zero;
//		minDist = maxLighteningDist;
//		AVOWComponent minComponent = null;
//		int minWhichNode = -1;
//		if (state == State.kNodeOnly || state == State.kNodeHeld){
//			
//			foreach (GameObject go in minNode.components){
//				AVOWComponent component = go.GetComponent<AVOWComponent>();
//				
//				if (component.type == AVOWComponent.Type.kVoltageSource) continue;
//				
//				float thisDist = 0;
//				int thisWhichNode = -1;
//				Vector3 connectionPos = Vector3.zero;
//				if (minNode == component.node0GO.GetComponent<AVOWNode>()){
//					connectionPos = component.GetConnectionPos0();
//					connectionPos.z = testPos.z;
//				}
//				else if (minNode == component.node1GO.GetComponent<AVOWNode>()){
//					connectionPos = component.GetConnectionPos1();
//					connectionPos.z = testPos.z;
//				}
//				else{
//					Debug.LogError ("Error UI");
//				}
//				thisDist = (connectionPos - testPos).magnitude;
//				float thisMaxDist = component.hWidth * 0.35f;
//				
//				if (thisDist < minDist && thisDist < thisMaxDist){
//					minComponent = component;
//					minDist = thisDist;
//					minWhichNode = thisWhichNode;
//					minComponentPos = connectionPos;
//					state = State.kNodeAndResistor;
//				}
//				
//			}
//		}
//		
//		
//		
//		if (buttonPressed){
//			if (state == State.kNodeOnly || state == State.kNodeAndResistor){
//				fixedNodeGO = minNode.gameObject;
//				state = State.kNodeHeld;
//			}
//		}
//		
//		// If we have such a node then make some lightening to it
//		if (state == State.kNodeOnly || state == State.kNodeAndResistor){
//			//cursorCube.transform.position = minPos;
//			lightening0.SetActive(true);
//			lightening0.GetComponent<Lightening>().startPoint = cursorCube.transform.position;
//			lightening0.GetComponent<Lightening>().endPoint = minNodePos;
//			lightening0.GetComponent<Lightening>().size = 0.2f;
//			lightening0.GetComponent<Lightening>().ConstructMesh();
//			
//			// Rotate the cube a bit too
//			cursorCube.transform.Rotate (new Vector3(1, 2, 4));
//			
//		}
//		else{
//			lightening0.SetActive(false);
//		}
//		
//
//		
//		AVOWGraph.singleton.EnableAllLightening();
//		
//		if (state == State.kNodeAndResistor){
//			
//			lightening1.SetActive(true);
//			lightening1.GetComponent<Lightening>().startPoint = cursorCube.transform.position;
//			lightening1.GetComponent<Lightening>().endPoint = minComponentPos;
//			lightening1.GetComponent<Lightening>().size = 0.2f;
//			lightening1.GetComponent<Lightening>().ConstructMesh();
//			
//			// Also need to hide the lightening from the compoment to the node
//			minComponent.GetComponent<AVOWComponent>().EnableLightening(minNode.gameObject, false);
//		}
//		else{
//			lightening1.SetActive(false);
//		}
		
	}
	

	
	

	// Update is called once per frame
	void Update () {
//		// Get the mouse position in world space
//		Vector3 mousePos = Input.mousePosition;
//		mousePos.z = 0;
//		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		NewUpdate();
		NewUpdateVisuals();
		return;
//		
//		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
//		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
//		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
//		
//		if (buttonReleased){
//			secondarySelectedNode = null;
//			secondarySelectedComponent = null;
//			previousSecondarySelectedNode = null;
//			selectedTab = null;
//			AVOWGraph.singleton.FillAllResistors();
//		}
//		if (buttonPressed || !buttonDown){
//			selectedTab = null;
//			secondarySelectedNode = null;
//		}
//		// If we do not have anything selected
//		overTab = null;
//		if (selectedTab == null){
//			foreach (AVOWTab tab in tabs){
//				bool isInside = tab.IsContaining(mouseWorldPos);
//				tab.SetMouseInside(isInside);
//				if (isInside){
//					if (buttonPressed){
//						
//						selectedTab = tab;
//					}
//					overTab = tab;
//				}
//				tab.SetSelected(tab == selectedTab);
//			}
//		}
//		// If we do have something selected, then we have different logic
//		else{
//			foreach (AVOWTab tab in tabs){
//				tab.SetMouseInside(false);
//			}		
//			secondarySelectedNode = null;	
//			secondarySelectedComponent = null;
//			foreach (AVOWTab tab in tabs){
//				// if we are in our select tab, then do nothing
//				if (tab == selectedTab) continue;
//				
//				bool isInside = tab.IsContaining(mouseWorldPos);
//				// If we are inside this tab, find all the other tabs which are
//				// part of this node - as we are "inside" all of them now too
//				if (isInside){
//					secondarySelectedNode = tab.GetNode();
//					secondarySelectedComponent = tab.GetAVOWComponent();
//					foreach (GameObject go in secondarySelectedNode.components){
//						AVOWComponent component = go.GetComponent<AVOWComponent>();
//						AVOWTab otherTab = null;
//						if (component.node0GO == secondarySelectedNode){
//							otherTab = go.transform.FindChild("LowerTab").GetComponent<AVOWTab>();
//						}
//						else{
//							otherTab = go.transform.FindChild("UpperTab").GetComponent<AVOWTab>();
//						}
//						otherTab.SetMouseInside(true);
//					}
//				}
//
//			}
//		}
//		
//		
//		
//		
////		Debug.Log ("secondarySelectedNode = " + 
////			(secondarySelectedNode!=null ? secondarySelectedNode.GetID() : "NULL") + " , previousSecondarySelectedNode = " + 
////			(previousSecondarySelectedNode!=null ? previousSecondarySelectedNode.GetID() : "NULL"));
//
//
//		// if we have a new secondarySelectNode then need to make (or destroy) one of the components
////		Debug.Log ("secondarySelectedNode = " + ((secondarySelectedNode != null) ? secondarySelectedNode.GetID():"NULL") + "...previousSecondarySelectedNode = " + ((previousSecondarySelectedNode != null) ? 
////			previousSecondarySelectedNode.GetID() : "NULL") + "....selectedComponent = " + ((selectedTab != null) ? selectedTab.GetAVOWComponent ().GetID() : "NULL") + 
////		           "...secondarySelectedComponent = " + ((secondarySelectedComponent != null) ? secondarySelectedComponent.GetID() : "NULL"));
//		
////		if (secondarySelectedNode != null && previousSecondarySelectedNode != null && secondarySelectedNode.GetComponent<AVOWNode>().splitFromNode != null && secondarySelectedNode.GetComponent<AVOWNode>().splitFromNode.GetComponent<AVOWNode>() == previousSecondarySelectedNode){
////			previousSecondarySelectedNode = secondarySelectedNode;
////		}
//		
//		if (secondarySelectedNode != previousSecondarySelectedNode){
//			// If we were previously on a node, then we need to remove the last component we added
//			if (previousSecondarySelectedNode != null){
//				Debug.Log ("Undo last command");
//				UndoLastCommand();
//				previousSecondarySelectedNode  = secondarySelectedNode;
//			}
//			
//			// If our currently selected one is a node, then we need to create a new component
//			if (secondarySelectedNode != null && !lockCreation){
//				// Are we trying to split a node
//				if (selectedTab.GetNode() == secondarySelectedNode){
//					AVOWCommand command = new AVOWCommandSplitAddComponent(secondarySelectedNode.gameObject, selectedTab.GetAVOWComponent ().gameObject, resistorPrefab);
//					IssueCommand(command);
//
//					
//				}
//				// or simple put a new component accross existing nodes
//				else{
//					AVOWCommand command = new AVOWCommandAddComponent(selectedTab.GetNode().gameObject, secondarySelectedNode.gameObject, resistorPrefab);
//					IssueCommand(command);
//					
//				}
//				
//			}
//			if (!lockCreation)
//				previousSecondarySelectedNode  = secondarySelectedNode;
//		}		
//		
//		// If we have a selected tab, then figure out if any tabs need to be disabled
//		// TO DO		
	}
	
	void IssueCommand(AVOWCommand command){
		command.ExecuteStep();
		commands.Push(command);
		
	}
	
	void UndoLastCommand(){
		AVOWCommand command = commands.Pop ();
		command.UndoStep ();
	}
	
	void UndoLastUnfinishedCommand(){
		if (commands.Count == 0) return;
		
		AVOWCommand command = commands.Peek ();
		if (!command.IsFinished()){
			commands.Pop ();
			command.UndoStep ();
		}
	}
	
	void OnGUI(){
		if (overTab){
			GUI.Label (new Rect(10, 10, Screen.width, 30), "overTab Tab: node = " + overTab.GetNode().GetID() + ", compoonent = " + overTab.GetAVOWComponent().GetID());
		}
	}
}
