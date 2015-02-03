using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	public GameObject cursorCubePrefab;
	public GameObject lighteningPrefab;
	
	
	// New attempt at encoding the state of the UI
	public GameObject 	connection0;
	public GameObject 	connection1;
	public Vector3 		connection0Pos;
	public Vector3 		connection1Pos;	
	public bool 		heldConnection;
	public AVOWCommand 	heldGapCommand;
	public GameObject 	heldGapConnection1;
	public float		newHOrder;
	
	float hysteresisFactor = 0.7f;
	
	int debugCount = 0;
			
	
	public float maxLighteningDist;
	
	
	
	float uiZPos;
	
	GameObject cursorCube;
	GameObject lightening0GO;
	GameObject lightening1GO;
	Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();
	
	Vector3 mouseWorldPos;
	
	// Set to true if we should not allow anything else to be created just yet
	// do this when killing a component
	public bool lockCreation;
	

	
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
	


	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	float FindClosestComponent(Vector3 pos, GameObject nodeGO, GameObject currentSelection, float minDist, ref GameObject closestComponent, ref Vector3 closestPos){
		AVOWNode node = nodeGO.GetComponent<AVOWNode>();

		// Make a copy of the list of compoents
		List<GameObject> components = node.components.GetRange(0, node.components.Count);

		if (currentSelection != null && currentSelection.GetComponent<AVOWComponent>() != null){
			components.Add(currentSelection);
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
			if (go == currentSelection){
				thisDist *= hysteresisFactor;
			}
			
			if (thisDist < minDist){
				minDist = thisDist;
				closestComponent = go;
				closestPos = thisPos;

			}
			
		}	
		return closestComponent ? minDist : maxLighteningDist;
	}
	
	void FindClosestPointOnNode(Vector3 pos, GameObject nodeGO, ref Vector3 closestPos){
	
		AVOWNode node = nodeGO.GetComponent<AVOWNode>();

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
	
	void FindMinMaxHBounds(out float minBounds, out float maxBounds){	
		minBounds = 99;
		maxBounds = -99;
		
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (heldGapCommand != null && heldGapCommand.GetNewComponent() == go) continue;
			minBounds = Mathf.Min (minBounds, component.hOrder);
			maxBounds = Mathf.Max (minBounds, component.hOrder);
		}
		minBounds += -2;
		maxBounds += 2;
	}
	
	void CalcNewHOrder(){

	
		// Ony bother doing this if we have a connection1 and that conneciton is to another node
		if (connection1 == null) return;
		
		AVOWNode node1 = connection1.GetComponent<AVOWNode>();
		
		if (node1 == null){
			newHOrder = connection1.GetComponent<AVOWComponent>().hOrder;
			return;
		}
		
		AVOWNode node0 = connection0.GetComponent<AVOWNode>();
		
		// Constrain our test position ot be inside both thenodes
		Vector3 testPos = mouseWorldPos;
		
		if (testPos.x < node0.h0) testPos.x = node0.h0;
		if (testPos.x > node0.h0 + node0.hWidth) testPos.x = node0.h0 + node0.hWidth;
		if (testPos.x < node1.h0) testPos.x = node1.h0;
		if (testPos.x > node1.h0 + node1.hWidth) testPos.x = node1.h0 + node1.hWidth;
		
		
		AVOWNode nodeHi = null;
		AVOWNode nodeLo = null;
		
		if (node0.voltage > node1.voltage){
			nodeHi = node0;
			nodeLo = node1;
		}
		else{
			nodeHi = node1;
			nodeLo = node0;
		}

		// Construct a list of all components flowing out and in to the nodes in the right direction of flow
		// there maybe repeasts
		List<GameObject> components = new List<GameObject>();
		foreach (GameObject go in nodeHi.components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.outNodeGO == nodeHi.gameObject &&
			    //(heldGapCommand == null || heldGapCommand.GetNewComponent() == null || go != heldGapCommand.GetNewComponent()) &&
			    component.isInteractive && 
			    component.type == AVOWComponent.Type.kLoad){
				components.Add(go);
			}
		}
		foreach (GameObject go in nodeLo.components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.inNodeGO == nodeLo.gameObject && 
			    //(heldGapCommand == null || heldGapCommand.GetNewComponent() == null || go != heldGapCommand.GetNewComponent()) &&
			    component.isInteractive && 
				component.type == AVOWComponent.Type.kLoad){
				components.Add(go);
			}
		}
		
		
		AVOWComponent compBefore = null;
		float minDistBefore = 100;
		AVOWComponent compAfter = null;
		float minDistAfter = 100;
		
	
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			float xMid = component.h0 + 0.5f * component.hWidth;
			
			float dist = testPos.x - xMid;
			
			// check if it is the same first
			if (MathUtils.FP.Feq(dist, minDistBefore)){
				if (component.hOrder > compBefore.hOrder){
					minDistBefore = dist;
					compBefore = component;
				}
			}
			if (MathUtils.FP.Feq(dist, minDistAfter)){
				if (component.hOrder < compAfter.hOrder){
					minDistAfter = dist;
					compAfter = component;
				}
			}
			
			
			// if the mouse is on the left of the comonent
			if (dist > 0){
				if (dist < minDistBefore){
					minDistBefore = dist;
					compBefore = component;
				}
			}
			else{
				if (-dist < minDistAfter){
					minDistAfter = -dist;
					compAfter = component;
				}
			}
		}
		
		string debugText = "";
		if (compBefore == null && compAfter == null){
			newHOrder = 0;
			debugText = "BeforeID = null, beforeHOrder = null, AfterID = null, AfterHOrder = null";
		}
		else if (compBefore == null)
		{
			newHOrder = compAfter.hOrder - 1;
			debugText = "BeforeID = null, beforeHOrder = null, AfterID = " + compAfter.GetID() + " AfterHOrder = " + compAfter.hOrder;
		}
		else if (compAfter == null)
		{
			newHOrder = compBefore.hOrder + 1;
			debugText = "BeforeID = " + compBefore.GetID() + ", beforeHOrder = " + compBefore.hOrder + ", AfterID = null, AfterHOrder = null";
		}
		else{		
			newHOrder = (compBefore.hOrder + compAfter.hOrder) * 0.5f;
			debugText = "BeforeID = " + compBefore.GetID() + ", beforeHOrder = " + compBefore.hOrder + ", AfterID = " + compAfter.GetID() + " AfterHOrder = " + compAfter.hOrder;
		}
		
		//Debug.Log (debugText + " - NewHOrder = " + newHOrder);
		
		
		
		
	}
		
	
	float FindClosestNode(Vector3 pos, GameObject ignoreNode, float minDist, GameObject currentSelection, ref GameObject closestNode, ref Vector3 closestPos){
		
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
			if (go == currentSelection){
				thisDist *= hysteresisFactor;
			}
			if (thisDist < minDist){
				minDist = thisDist;
				closestNode = go;
				closestPos = thisPos;
			}
		}	
		return closestNode ? minDist : maxLighteningDist;
	}
	
	void StateUpdate(){
		// Calc the mouse posiiton on world spave
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		// Get the mouse buttons
		bool  buttonPressed = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonReleased = (Input.GetMouseButtonUp(0) && !Input.GetKey (KeyCode.LeftControl));
//		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		// Set the cursor cubes position
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;


//		
//		// Playback bug
//		if (debugCount == 0){
//			mouseWorldPos = new Vector3(0.6f, 0.9f, -0.1f);
//			buttonPressed = true;
//		
//		}
//		else if (debugCount < 300){
//			mouseWorldPos = new Vector3(0.6f, 0.75f, -0.1f);
//		}
//		else {
//			mouseWorldPos = new Vector3(0.6f, 0.9f, -0.1f);
//		}	
//		++debugCount;	
//		
		
	//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		
		// If we don't have a held connection, then we find the closest node and that's all
		if (!heldConnection){
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
			FindClosestNode(mouseWorldPos, null, maxLighteningDist, connection0, ref closestObj, ref closestPos);
			connection0 = closestObj;
			connection0Pos = closestPos;
			connection1 = null;
			connection1Pos = Vector3.zero;
			
			if (buttonPressed && connection0 != null){
				heldConnection = true;
			}
		}
		else{
			
			// Update our connection posiiton on our node we are connected to
			FindClosestPointOnNode(mouseWorldPos, connection0, ref connection0Pos);

			// Now find the next closest thing - favouring whatever we have connected already
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
			float minDist = FindClosestComponent(mouseWorldPos, connection0, connection1, maxLighteningDist, ref closestObj, ref closestPos);
			minDist = FindClosestNode(mouseWorldPos, connection0, minDist, connection1, ref closestObj, ref closestPos);			
			connection1 = closestObj;
			connection1Pos = closestPos;		
		
			if (buttonReleased){
				heldConnection = false;
			}
			
		}
			
	}
	
	
	
	void VizUpdate(){
	
		// if we are holding a node then select that node
		AVOWGraph.singleton.UnselectAllNodes();
		if (heldConnection) connection0.GetComponent<AVOWNode>().SetSelected(true);
		
	
		// Lightening to connection 0 - which is always a node
		if (connection0 != null){
			lightening0GO.SetActive(true);
			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
			
			lightening0.startPoint = mouseWorldPos;
			lightening0.endPoint = connection0Pos;
			lightening0.size =  heldConnection ? 0.4f : 0.1f;
			lightening0.ConstructMesh();
		}
		else{
			lightening0GO.SetActive(false);
		}
		
		// Lightening to connection 1 - which may be a component or a node
		// don't do this in free mode
		AVOWGraph.singleton.EnableAllLightening();
		if (connection1 != null){
			lightening1GO.SetActive(true);
			Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
			lightening1.startPoint = mouseWorldPos;
			lightening1.endPoint = connection1Pos;
			lightening1.size = 0.1f;
			lightening1.ConstructMesh();
			
			// Also need to hide the lightening from the compoment to the node
			if (connection1.GetComponent<AVOWComponent>() != null){
				Debug.Log("connection1.GetComponent<AVOWComponent>().ID = " + connection1.GetComponent<AVOWComponent>().GetID());
				connection1.GetComponent<AVOWComponent>().EnableLightening(connection0, false);
			}
			
			// Tell the sim (this shouild probably be the camera)
			AVOWSim.singleton.mouseOverComponentForce = connection1;
		}
		else{
			lightening1GO.SetActive(false);
		}	
		
		// If we are connected to something then rotate the cube a bit
		if (connection0 != null){
			cursorCube.transform.Rotate (new Vector3(1, 2, 4));
		}
	}
	
	
	bool OrderHasChanged(){
		// Consider the two nodes we are putting a component between
		// Consider all the Interactive components on each node and see at what position our hValue would place a new component
		// We also look at where our old Hvalue would place the component if it is in the same position, then the order has not changed.
		AVOWComponent component = heldGapCommand.GetNewComponent().GetComponent<AVOWComponent>();
		AVOWNode inNode = component.inNodeGO.GetComponent<AVOWNode>();
		AVOWNode outNode = component.outNodeGO.GetComponent<AVOWNode>();
		
		// These should be ordered by hOrder
		List<GameObject> inComponents = inNode.inComponents;
		List<GameObject> outComponents = outNode.outComponents;
		
		float oldHOrder = component.hOrder;
		
		int inOrdinalOld = -1;
		int inOrdinalNew = -1;
		int outOrdinalOld = -1;
		int outOrdinalNew = -1;
		
		foreach (GameObject go in inComponents){
			// This list might be out of date
			if (go == null) continue;
	
			AVOWComponent inComponent = go.GetComponent<AVOWComponent>();
			
			if (inComponent.hOrder <= oldHOrder){
				inOrdinalOld++;
			}
			if (inComponent.hOrder <= newHOrder){
				inOrdinalNew++;
			}
		}
		foreach (GameObject go in outComponents){
			// This list might be out of date
			if (go == null) continue;

			AVOWComponent outComponent = go.GetComponent<AVOWComponent>();
			
			if (outComponent.hOrder < oldHOrder){
				outOrdinalOld++;
			}
			if (outComponent.hOrder < newHOrder){
				outOrdinalNew++;
			}
		}
	//	Debug.Log("inOrdinalOld " + inOrdinalOld + ", inOrdinalNew = " + inOrdinalNew + ", outOrdinalOld = " + outOrdinalOld + ", outOrdinalNew = " + outOrdinalNew);
		return (inOrdinalOld != inOrdinalNew || outOrdinalOld != outOrdinalNew);
	}
	
	void CommandsUpdate(){
		// if we have a command already check if we need to undo it
		if (heldGapCommand != null){
			// if the connection1 has changed (which includes us no longer holding anything) then undo our current command
			if (heldGapConnection1 != connection1 || OrderHasChanged()){
				heldGapCommand.UndoStep();
				heldGapCommand = null;
				
				string newID = null;
				string oldID = null;
				if (connection1)
					newID = connection1.GetComponent<AVOWComponent>() != null ? connection1.GetComponent<AVOWComponent>().GetID() : connection1.GetComponent<AVOWNode>().GetID();
				if (heldGapConnection1)
					oldID = heldGapConnection1.GetComponent<AVOWComponent>() != null ? heldGapConnection1.GetComponent<AVOWComponent>().GetID() : heldGapConnection1.GetComponent<AVOWNode>().GetID();
				Debug.Log("Undo command. OldID = " + oldID + ", newID = " + newID + ", Time = " + Time.time);
			}
		}
		
		// If we still have a command, then this command is still valid and nothing more to do - however, if we don't have one, 
		// then perhaps we should make one?
		if (heldGapCommand == null && connection1 != null){
			heldGapConnection1 = connection1;
			if (connection1.GetComponent<AVOWComponent>()){
				heldGapCommand = new AVOWCommandSplitAddComponent(connection0, connection1, resistorPrefab);
				Debug.Log("new AVOWCommandSplitAddComponent " + Time.time);
			}
			else{
				heldGapCommand = new AVOWCommandAddComponent(connection0, connection1, resistorPrefab);
				Debug.Log("new AVOWCommandAddComponent " + Time.time);
				
			}
			heldGapCommand.ExecuteStep();
			heldGapCommand.GetNewComponent().GetComponent<AVOWComponent>().hOrder = newHOrder;
			
			// Ned to force the sim to do an update (this would be better if all this logic was in a fixed update and it 
			// was more tightly controlled
			AVOWSim.singleton.FixedUpdate();
		}
	}
	
//	
//	void NewUpdateVisuals2(){
//	
//		// If we have changed the things we are pointing at
//		if (ActionHasChange()){
//			if (lastAction != null){
//				Debug.Log ("UndoLastUnfinishedCommand");
//				UndoLastUnfinishedCommand();
//			}
//			if (currentAction != null){
//				if (currentAction.isNodeGap){
//					AVOWCommandAddComponent command = new AVOWCommandAddComponent(currentAction.conn0Node , currentAction.conn1Node, resistorPrefab);
//					IssueCommand(command);
//
//					currentAction.conn1Component = command.GetNewComponent();
//					currentAction.conn1Node = command.GetNewNode();	// null
//					lastAction = currentAction;
//					
//					state = State.kHeldOpen;
//				}
//				// Otherwise it is a component
//				else{
//					// check is we can legitimately do this (sometimes we can't because we are connected to a node which is disappearing)
//					AVOWComponent testComponent = currentAction.conn1Component.GetComponent<AVOWComponent>();
//					if (testComponent.node0GO == currentAction.conn0Node || testComponent.node1GO == currentAction.conn0Node){
//						AVOWCommandSplitAddComponent command = new AVOWCommandSplitAddComponent(currentAction.conn0Node, currentAction.conn1Component, resistorPrefab);
//						IssueCommand(command);
//						// Our current "action" is now meaninless as the comoment is not connected to the node anymore
//						// So adjust our "last action" to make sense in this new context
//						currentAction.conn1Component = command.GetNewComponent();
//						currentAction.conn1Node = command.GetNewNode();
//						
//						lastAction = currentAction;
//						
//						state = State.kHeldOpen;
//					}
//					
//				}
//			}
//		}
//	
//		// Lightening to connection 0 - which is always a node
//		if (connection0Node != null){
//			lightening0GO.SetActive(true);
//			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
//			
//			lightening0.startPoint = mouseWorldPos;
//			lightening0.endPoint = connection0Pos2;
//			lightening0.size =  (state == State.kFree) ? 0.1f : 0.4f;;
//			lightening0.ConstructMesh();
//		}
//		else{
//			lightening0GO.SetActive(false);
//		}
//		
//		// Lightening to connection 1 - which may be a component or a node
//		// don't do this in free mode
//		AVOWGraph.singleton.EnableAllLightening();
//		if (state != State.kFree && (connection1Component != null || connection1Node != null)){
//			lightening1GO.SetActive(true);
//			Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
//			lightening1.startPoint = mouseWorldPos;
//			lightening1.endPoint = connection1Pos2;
//			lightening1.size = 0.1f;
//			lightening1.ConstructMesh();
//			
//			// Also need to hide the lightening from the compoment to the node
//			if (connection1Component != null){
//				connection1Component.GetComponent<AVOWComponent>().EnableLightening(connection0Node, false);
//			}
//		}
//		else{
//			lightening1GO.SetActive(false);
//		}		
//		
//		// If we are connected to something then rotate the cube a bit
//		if (connection0Node != null){
//			cursorCube.transform.Rotate (new Vector3(1, 2, 4));
//		}
//		
//	}

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
//		Debug.Log(Time.time + ": UI Update");
		StateUpdate();
		CalcNewHOrder();
		CommandsUpdate();
		VizUpdate();

	}
	
	

	
	void OnGUI(){
		float lineNum = 1;
		float lineSize = 20;
		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "newHOrder = " + newHOrder);
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "connection1 = " + (connection1Node != null ? connection1Node.GetComponent<AVOWNode>().GetID() : "NULL"));
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "connection1Component = " + (connection1Component != null ? connection1Component .GetComponent<AVOWComponent>().GetID() : "NULL"));
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "currentAction.conn0Node = " + ((currentAction!= null && currentAction.conn0Node != null) ? currentAction.conn0Node.GetComponent<AVOWNode>().GetID() : "NULL"));
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "currentAction.conn1Node = " + ((currentAction!= null && currentAction.conn1Node != null) ? currentAction.conn1Node.GetComponent<AVOWNode>().GetID() : "NULL"));
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "currentAction.conn1Component= " + ((currentAction!= null && currentAction.conn1Component != null) ? currentAction.conn1Component.GetComponent<AVOWComponent>().GetID() : "NULL"));
//		GUI.Label (new Rect(10, lineSize * lineNum++ , Screen.width, lineSize), "currentAction.isNodeGap= " + ((currentAction!= null && currentAction.isNodeGap != null) ? (currentAction.isNodeGap ? "true" : "false") : "NULL"));
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
}

