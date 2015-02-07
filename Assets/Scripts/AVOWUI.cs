using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUITool{
	
	protected float hysteresisFactor = 0.5f;
	protected float maxLighteningDist = 0.3f;
	protected float 		uiZPos;
	protected GameObject insideCube;
	
	
	
	protected class OrderBlock{
		
		public float minOrder = 99;
		public float maxOrder = -99;
		public float minPos = 99;
		public float maxPos = -99;
		
		public void AddComponent(AVOWComponent component){
			minOrder = Mathf.Min(minOrder, component.hOrder);
			maxOrder = Mathf.Max(maxOrder, component.hOrder);
			minPos = Mathf.Min(minPos, component.h0);
			maxPos = Mathf.Max(maxPos, component.h0 + component.hWidth);
		}
	}
	
	public virtual void Start(){}
	public virtual void Update(){}
	
	
	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	protected float FindClosestComponent(Vector3 pos, GameObject nodeGO, GameObject currentSelection, float minDist, ref GameObject closestComponent, ref Vector3 closestPos){
		AVOWNode node = nodeGO.GetComponent<AVOWNode>();
		
		// Make a copy of the list of compoents
		List<GameObject> components = node.components.GetRange(0, node.components.Count);
		
		if (currentSelection != null && currentSelection.GetComponent<AVOWComponent>() != null){
			components.Add(currentSelection);
		}
		
		
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			

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
	
	
	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	protected float FindClosestComponentCentre(Vector3 pos, GameObject currentSelection, float minDist, ref GameObject closestComponent, ref Vector3 closestPos){
		
		List<GameObject> components = AVOWGraph.singleton.allComponents;
		
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (!component.isInteractive) continue;
			
			float thisDist = 0;
			Vector3 thisPos = 0.5f * (component.GetConnectionPos0() + component.GetConnectionPos1());


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
	
	protected void FindClosestPointOnNode(Vector3 pos, GameObject nodeGO, ref Vector3 closestPos){
		
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
	
	protected float FindClosestNode(Vector3 pos, GameObject ignoreNode, float minDist, GameObject currentSelection, ref GameObject closestNode, ref Vector3 closestPos){
		
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
	
	
	protected void ActiveCubeAtCursor(GameObject cursor){
		// Create a new cube which will travel to the gap
		insideCube = AVOWUI.singleton.InstantiateCursorCube();
		insideCube.transform.position = cursor.transform.position;
		insideCube.transform.rotation = cursor.transform.rotation;
		
		Material[] metalMaterials = new Material[1];
		metalMaterials[0] = cursor.renderer.materials[0];
		insideCube.renderer.materials = metalMaterials;
	}
	
	
	protected void ActiveCubeAtComponent(GameObject component){
		Transform resistanceTransform = component.transform.FindChild("Resistance");
		
		// Create a new cube which will travel to the cursor
		insideCube = AVOWUI.singleton.InstantiateCursorCube();
		Vector3 targetScale = new Vector3(resistanceTransform.localScale.x, resistanceTransform.localScale.x, resistanceTransform.localScale.x);
		
		insideCube.transform.localScale = targetScale;
		insideCube.transform.position = resistanceTransform.position +  + 0.5f * targetScale;
		insideCube.transform.rotation = resistanceTransform.rotation;
		
		Material[] metalMaterials = new Material[1];
		metalMaterials[0] = insideCube.renderer.materials[0];
		insideCube.renderer.materials = metalMaterials;
		
		component.GetComponent<AVOWComponent>().isInteractive = false;
	}
	
	protected void RemoveMetal(GameObject cursor){
		Material[] highlightMaterials = new Material[1];
		highlightMaterials[0] = cursor.renderer.materials[1];
		cursor.renderer.materials = highlightMaterials;
		
	}
	
	// return dist remaining position of cube
	protected float LerpToComponent(AVOWComponent component, float lerpSpeed){
		Transform resistanceTransform = component.gameObject.transform.FindChild("Resistance");
		
		//Orentation
		Quaternion targetOrient = resistanceTransform.rotation;
		Quaternion currentOrient = insideCube.transform.rotation;
		currentOrient = Quaternion.Slerp(currentOrient, targetOrient, lerpSpeed);
		insideCube.transform.rotation = currentOrient;
		
		// Scale
		Vector3 targetScale = new Vector3(resistanceTransform.localScale.x, resistanceTransform.localScale.x, resistanceTransform.localScale.x);
		Vector3 currentScale = insideCube.transform.localScale;
		currentScale = Vector3.Lerp(currentScale, targetScale, lerpSpeed);
		insideCube.transform.localScale = new Vector3(currentScale.x, currentScale.x, currentScale.x);
		
		// Position
		Vector3 targetPos = resistanceTransform.position + 0.5f * targetScale;
		Vector3 currentPos = insideCube.transform.position;
		currentPos = Vector3.Lerp(currentPos, targetPos, lerpSpeed);
		insideCube.transform.position = currentPos;
		
		return (currentPos - targetPos).magnitude;
	}
	
	// return dist remaining position of cube
	protected float LerpToCursor(GameObject cursor, float lerpSpeed){
		
		Transform cursorTransform = cursor.transform;
		
		//Orentation
		Quaternion targetOrient = cursorTransform.rotation;
		Quaternion currentOrient = insideCube.transform.rotation;
		currentOrient = Quaternion.Slerp(currentOrient, targetOrient, lerpSpeed);
		insideCube.transform.rotation = currentOrient;
		
		// Scale
		Vector3 targetScale = cursorTransform.localScale;
		Vector3 currentScale = insideCube.transform.localScale;
		currentScale = Vector3.Lerp(currentScale, targetScale, lerpSpeed);
		insideCube.transform.localScale = new Vector3(currentScale.x, currentScale.x, currentScale.x);
		
		// Position
		Vector3 targetPos = cursorTransform.position;
		Vector3 currentPos = insideCube.transform.position;
		currentPos = Vector3.Lerp(currentPos, targetPos, lerpSpeed);
		insideCube.transform.position = currentPos;	
		
		return (currentPos - targetPos).magnitude;
	}
	
	
	protected GameObject RejoinToCursor(GameObject cursor){
		GameObject newCursorCube = AVOWUI.singleton.InstantiateCursorCube();
		newCursorCube.transform.position = cursor.transform.position;
		newCursorCube.transform.rotation = cursor.transform.rotation;
		newCursorCube.transform.localScale = cursor.transform.localScale;
		
		GameObject.Destroy(insideCube);
		insideCube = null;
		
		
		
		GameObject.Destroy (cursor);
		return newCursorCube;
	}
	
	

}


public class AVOWUICreateTool :  AVOWUITool{

	
	// New attempt at encoding the state of the UI
	public GameObject 	connection0;
	public GameObject 	connection1;
	public Vector3 		connection0Pos;
	public Vector3 		connection1Pos;	
	public bool 		heldConnection;
	public AVOWCommand 	heldGapCommand;
	public GameObject 	heldGapConnection1;
	
	
	float		newHOrder;

	
	public enum InsideGapState{	
		kOutside,
		kOutsideAndExiting,
		kEntering,
		kFullyInside,
		kOnNewComponent
	};
	
	public bool			  	isInside = false;
	public InsideGapState 	insideState = InsideGapState.kOutside;
	float					maxLerpSpeed = 0.5f;
	float					minLerpSpeed = 0f;
	float 					insideLerpSpeed;
	
	Vector3 				mouseWorldPos;
	
	GameObject 				cursorCube;
	GameObject 				lightening0GO;
	GameObject 				lightening1GO;
	
	
	public override void Start(){
		cursorCube = AVOWUI.singleton.InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		lightening0GO = AVOWUI.singleton.InstantiateLightening();
		lightening0GO.transform.parent = AVOWUI.singleton.transform;
		lightening1GO = AVOWUI.singleton.InstantiateLightening();
		lightening1GO.transform.parent = AVOWUI.singleton.transform;
		
		uiZPos = AVOWUI.singleton.transform.position.z;
		
		insideLerpSpeed = minLerpSpeed;
	}

	
	public override void Update () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		StateUpdate();
		CalcNewHOrder();
		CommandsUpdate();
		VizUpdate();
		
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
		
		
		
		//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		
		// If we don't have a held connection, then we find the closest node and that's all
		if (!heldConnection){
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
//			float minDist = FindClosestComponent(mouseWorldPos, connection0, maxLighteningDist, ref closestObj, ref closestPos);
			float minDist = maxLighteningDist;
			FindClosestNode(mouseWorldPos, null, minDist, connection0, ref closestObj, ref closestPos);
			connection0 = closestObj;
			connection0Pos = closestPos;
			connection1 = null;
			connection1Pos = Vector3.zero;
			
			if (buttonPressed && connection0 != null){
				heldConnection = true;
			}
		}
		else{
		
			if (connection0.GetComponent<AVOWNode>() == null){
				Debug.LogError ("Should never have a connection 0 held as a component");
			}
			
			// Update our connection posiiton on our node we are connected to
			FindClosestPointOnNode(mouseWorldPos, connection0, ref connection0Pos);
			
			// If not inside the held gap, find the next closest thing - favouring whatever we have connected already
			if (!isInside){
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
			else{
				if (buttonReleased){
					heldConnection = false;
					heldGapCommand.ExecuteStep();
					AVOWUI.singleton.commands.Push(heldGapCommand);
					heldGapCommand = null;
					heldGapConnection1 = null;
					connection1 = null;
					connection0 = null;
					insideState = InsideGapState.kOnNewComponent;
				}
			}
			
			
		}
		
		// If we have a gap which we are holding open, check if our mous is inside the gap
		isInside = false;
		AVOWComponent component = GetHeldCommandComponent();
		if (component != null){
			isInside = component.IsPointInsideGap(mouseWorldPos);
		}
		
		HandleCubeInsideGap();
		
	}
	
	AVOWComponent GetHeldCommandComponent(){
		if (heldGapCommand == null) return null;
		
		GameObject go = heldGapCommand.GetNewComponent();
		if (go == null) return null;
		
		AVOWComponent component = go.GetComponent<AVOWComponent>();
		return component;
	}
	
	void HandleCubeInsideGap(){
		
		// Manage the inside state machine
		switch (insideState){
			case InsideGapState.kOutside:{
				if (isInside) {
					// Create a new cube which will travel to the gap
					ActiveCubeAtCursor(cursorCube);
					
					
					// Remove the metal material from the cube that we have
					RemoveMetal(cursorCube);
					
					insideState = InsideGapState.kEntering;
					insideLerpSpeed = minLerpSpeed;
				}
				break;
			}
			case InsideGapState.kEntering:{
				
				// Move our inside cube to where it needs to be
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null && isInside){
					float distRemaining = LerpToComponent(component, insideLerpSpeed);
	
					if (MathUtils.FP.Feq(distRemaining, 0, 0.001f)){
						insideState = InsideGapState.kFullyInside;
						insideLerpSpeed = minLerpSpeed;
					}
				}
				else{
					insideState = InsideGapState.kOutsideAndExiting;
					insideLerpSpeed = minLerpSpeed;
					
				}
				
				insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
				
				break;
			}
			case InsideGapState.kFullyInside:{
				AVOWComponent component = GetHeldCommandComponent();
				if (component == null || !isInside){
					insideState = InsideGapState.kOutsideAndExiting;
					insideLerpSpeed = minLerpSpeed;
					
				}
				break;
			}
			case InsideGapState.kOutsideAndExiting:{
	
				float distRemaining = LerpToCursor(cursorCube, insideLerpSpeed);
				
				if (MathUtils.FP.Feq(distRemaining, 0, 0.001f)){
					insideState = InsideGapState.kOutside;
					
					// Remake our cursor
					cursorCube = RejoinToCursor(cursorCube);
					
				}
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null && isInside){
					insideState = InsideGapState.kEntering;	
					insideLerpSpeed = minLerpSpeed;
				}
				insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
				break;
			}			
			case InsideGapState.kOnNewComponent:{
				insideState = InsideGapState.kOutside;
				
				// Remake our cube
				cursorCube = RejoinToCursor(cursorCube);
				
				GameObject.Destroy(insideCube);
				insideCube = null;
				break;
			}
		}	
		
	}
	
	
	
	void VizUpdate(){
		
		// if we are holding a node then select that node
		AVOWGraph.singleton.UnselectAllNodes();
		if (heldConnection){
			if (connection0.GetComponent<AVOWNode>() != null) 
				connection0.GetComponent<AVOWNode>().SetSelected(true);
		}
		
		Vector3 lighteningConductorPos = (isInside) ? insideCube.transform.position : mouseWorldPos;
		Vector3 connection0PosUse = (isInside) ? new Vector3(insideCube.transform.position.x, connection0Pos.y, connection0Pos.z) : connection0Pos;
		Vector3 connection1PosUse = (isInside && connection1 != null && connection1.GetComponent<AVOWComponent>() == null) ? new Vector3(insideCube.transform.position.x, connection1Pos.y, connection1Pos.z) : connection1Pos;
		
		
		lighteningConductorPos.z = uiZPos;
		
		// Lightening to connection 0 - which is always a node
		if (connection0 != null){
			lightening0GO.SetActive(true);
			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
			lightening0.startPoint = lighteningConductorPos;
			lightening0.endPoint = connection0PosUse;
			
			float len = (lightening0.startPoint  - lightening0.endPoint).magnitude;
			lightening0.numStages = Mathf.Max ((int)(len * 10), 2);
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
			lightening1.startPoint = lighteningConductorPos;
			lightening1.endPoint = connection1PosUse;
			
			float len = (lightening1.startPoint  - lightening1.endPoint).magnitude;
			lightening1.numStages = Mathf.Max ((int)(len * 10), 2);
			lightening1.size = 0.1f;
			lightening1.ConstructMesh();
			
			// Also need to hide the lightening from the compoment to the node
			if (connection1.GetComponent<AVOWComponent>() != null){
				//				Debug.Log("connection1.GetComponent<AVOWComponent>().ID = " + connection1.GetComponent<AVOWComponent>().GetID());
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
			
			if (!inComponent.isInteractive) continue;
			if (inComponent.type == AVOWComponent.Type.kVoltageSource) continue;
			
			
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
			
			if (!outComponent.isInteractive) continue;
			if (outComponent.type == AVOWComponent.Type.kVoltageSource) continue;
			
			
			if (outComponent.hOrder < oldHOrder){
				outOrdinalOld++;
			}
			if (outComponent.hOrder < newHOrder){
				outOrdinalNew++;
			}
		}
		//		Debug.Log("oldHOrder = " + oldHOrder + ", newHOrder = " + newHOrder + ", inOrdinalOld " + inOrdinalOld + ", inOrdinalNew = " + inOrdinalNew + ", outOrdinalOld = " + outOrdinalOld + ", outOrdinalNew = " + outOrdinalNew);
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
				heldGapCommand = new AVOWCommandSplitAddComponent(connection0, connection1, AVOWUI.singleton.resistorPrefab);
				Debug.Log("new AVOWCommandSplitAddComponent " + Time.time);
			}
			else{
				heldGapCommand = new AVOWCommandAddComponent(connection0, connection1, AVOWUI.singleton.resistorPrefab);
				Debug.Log("new AVOWCommandAddComponent from " + connection0.GetComponent<AVOWNode>().GetID() + " to " + connection1.GetComponent<AVOWNode>().GetID() + " time = " + Time.time);
				
			}
			heldGapCommand.ExecuteStep();
			heldGapCommand.GetNewComponent().GetComponent<AVOWComponent>().hOrder = newHOrder;
			
			// Ned to force the sim to do an update (this would be better if all this logic was in a fixed update and it 
			// was more tightly controlled
			AVOWSim.singleton.FixedUpdate();
		}

	}
	
	
	
	protected void CalcNewHOrder(){
		// Ony bother doing this if we have a connection1 and that conneciton is to another node
		if (connection1 == null) return;
		
		AVOWNode node1 = connection1.GetComponent<AVOWNode>();
		
		if (node1 == null){
			newHOrder = connection1.GetComponent<AVOWComponent>().hOrder;
			//			Debug.Log("CalcNewHOrder = " + newHOrder);
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
		
		
		
		// Creare a disjoint set of OrderBlocks - each ORderblock contains a number of components
		// Construct a block by starting at a component on anodeHI or nodeLo and following its connections
		// along every way we can until we git NdoeHi or NodeLo again our new component must fir on the left or the righ of this block.
		AVOWGraph graph = AVOWGraph.singleton;
		graph.ClearUIOrderedVisitedFlags();
		List<OrderBlock> blocks = new List<OrderBlock>();
		
		// Run though all the components with current flowing out of the high component
		int uniqueIndex = 0;
		int useIndex = 0;
		foreach (GameObject go in nodeHi.outComponents){
			AVOWComponent component = GetValidOrderingComponent(go);
			if (component == null) continue;
			
			
			// if we haven't been visited yet
			Queue<AVOWComponent> componentQueue = new Queue<AVOWComponent>();
			
			componentQueue.Enqueue(component);
			
			useIndex = uniqueIndex;
			List<AVOWComponent> visitedComponents = new List<AVOWComponent>();
			
			bool connected = false;
			while (componentQueue.Count > 0){
				AVOWComponent thisComponent = componentQueue.Dequeue();
				
				// is this component has not been visited yet
				if (thisComponent.uiVisitedIndex == -1){
					thisComponent.uiVisitedIndex = useIndex;
					visitedComponents.Add(thisComponent);
					
					if (thisComponent.inNodeGO == null) continue;
					
					AVOWNode inNode = thisComponent.inNodeGO.GetComponent<AVOWNode>();
					
					if (inNode == nodeLo){
						connected = true;
						continue;
					}
					
					// Get list of components flowing out of this node and add them to the queue
					foreach (GameObject outGO in inNode.outComponents){
						AVOWComponent outComponent = GetValidOrderingComponent(outGO);
						if (outComponent == null) continue;
						
						componentQueue.Enqueue(outComponent);
					}
				}
				// If we have been visited - then we are simply part of the same block and must reconfigure all our 
				// components we have used so far to the new ID
				else{
					useIndex = thisComponent.uiVisitedIndex ;
					foreach (AVOWComponent visitedComponent in visitedComponents){
						visitedComponent.uiVisitedIndex = useIndex;
					}
				}
			}
			// If this is a genuinely unique block
			if (uniqueIndex == useIndex){
				// And if this block connects
				if (connected){
					uniqueIndex++;
					blocks.Add (new OrderBlock());
				}
				// If it is a unique bloc kthat does not connect, then we want to reuse the index so set all the ones we
				// have set this round to -1
				else{
					foreach (AVOWComponent visitedComponent in visitedComponents){
						visitedComponent.uiVisitedIndex = -1;
					}
				}
			}
			
		}
		
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = GetValidOrderingComponent(go);
			if (component == null) continue;
			
			// Ignore any that we've set to -1 as these are not connected
			if (component.uiVisitedIndex >= 0){
				blocks[component.uiVisitedIndex].AddComponent(component);
			}
			
		}
		
		
		
		
		//		Debug.Log("CalcNewHOrderL nodeHi =  " + nodeHi.GetID() + " , nodeLo = " + nodeLo.GetID() + ", numBlocks = " + blocks.Count);
		//		
		//		// print the contents fo the blocks
		//		foreach (OrderBlock block in blocks){
		//			Debug.Log ("block: minOrder = " + block.minOrder + ", maxOrder = " + block.maxOrder + ", minPos = " + block.minPos + ", maxPos = " + block.maxPos);
		//		}
		
		OrderBlock blockBefore = null;
		float minDistBefore = 100;
		OrderBlock blockAfter = null;
		float minDistAfter = 100;
		
		
		foreach (OrderBlock block in blocks){
			float xMid = 0.5f * (block.minPos + block.maxPos);
			
			float dist = testPos.x - xMid;
			
			// if the mouse is on the right of the block
			if (dist > 0){
				if  (dist < minDistBefore){
					minDistBefore = dist;
					blockBefore = block;
				}
			}
			else{
				if (-dist < minDistAfter){
					minDistAfter = -dist;
					blockAfter = block;
				}
			}
		}
		
		// If we haven't got any blocks then we are connecting between nodes that have no
		// connections between them yet - so do some different logic
		if (blocks.Count == 0){
			OrderBlock blockHi = new OrderBlock();
			OrderBlock blockLo = new OrderBlock();
			foreach (GameObject go in nodeHi.components){
				if (go == null) continue;
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (heldGapCommand != null && component.gameObject == heldGapCommand.GetNewComponent()) continue;
				if (component.type == AVOWComponent.Type.kVoltageSource) continue;
				
				blockHi.AddComponent(component);
			}
			foreach (GameObject go in nodeLo.components){
				if (go == null) continue;
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (heldGapCommand != null && component.gameObject == heldGapCommand.GetNewComponent()) continue;
				if (component.type == AVOWComponent.Type.kVoltageSource) continue;
				
				blockLo.AddComponent(component);
			}
			
			
			if (nodeHi.h0 + 0.5f * nodeHi.hWidth > nodeLo.h0 + 0.5f * nodeLo.hWidth){
				blockAfter = blockHi;
				blockBefore = blockLo;
			}
			else{
				blockAfter = blockLo;
				blockBefore = blockHi;
			}
			
		}
		
		string debugText = "";
		if (blockBefore == null && blockAfter == null){
			// I'm not sure that this can ever happen
			newHOrder = 0;
			debugText = "BeforeMinH = null, BeforeMaxH = null, AfterMinH = null, AfterMaxH = null";
		}
		else if (blockBefore == null)
		{
			newHOrder = blockAfter.minOrder - 1;
			debugText = "BeforeMinH = null, BeforeMaxH = null, AfterMinH = " + blockAfter.minOrder + " , AfterMaxH = " + blockAfter.maxOrder;
		}
		else if (blockAfter == null)
		{
			newHOrder = blockBefore.maxOrder + 1;
			debugText = "BeforeMinH = " + blockBefore.minOrder + ", BeforeMaxH = " + blockBefore.maxOrder + ", AfterMinH = null , AfterMaxH = null";
		}
		else{	
			newHOrder = (blockBefore.maxOrder + blockAfter.minOrder) * 0.5f;
			debugText = "BeforeMinH = " + blockBefore.minOrder + ", BeforeMaxH = " + blockBefore.maxOrder + ", AfterMinH = " + blockAfter.minOrder + " , AfterMaxH = " + blockAfter.maxOrder;
		}
		
		//		Debug.Log ("CalcNewHOrder: " + debugText + " - NewHOrder = " + newHOrder);
		
	}
	
	
	AVOWComponent GetValidOrderingComponent(GameObject go){
		if (go == null) return null;
		
		AVOWComponent component = go.GetComponent<AVOWComponent>();
		if (heldGapCommand != null && component.gameObject == heldGapCommand.GetNewComponent()) return null;
		//if (!component.isInteractive) return null;
		if (component.type == AVOWComponent.Type.kVoltageSource) return null;
		return component;
		
	}
	
	
}




public class AVOWUIDeleteTool :  AVOWUITool{
	
	
	// New attempt at encoding the state of the UI
	public GameObject 	connectionGO;
	public Vector3 		connectionPos;	
	public bool 		heldConnection;
	public AVOWCommand 	heldGapCommand;
	public GameObject	heldGapConnection;
	
	Vector3 				mouseWorldPos;
	
	GameObject 				cursorCube;
	GameObject 				lightening0GO;
	GameObject 				lightening1GO;
	
	public enum InsideGapState{	
		kUncreated,
		kCreateInside,
		kCreaeOutside,
		kOutside,
		kOutsideAndEntering,
		kInsideAndExiting,
		kFullyInside,
		kOnRemove,
		kOnCancel
	};
	
	public bool			  	isInside = true;
	public InsideGapState 	insideState = InsideGapState.kUncreated;
	float					maxLerpSpeed = 0.5f;
	float					minLerpSpeed = 0f;
	float 					insideLerpSpeed;
	
	
	
	
	public override void Start(){
		cursorCube = AVOWUI.singleton.InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		
		// Remove the metal material from the cube that we have
		RemoveMetal(cursorCube);
		
		
		lightening0GO = AVOWUI.singleton.InstantiateLightening();
		lightening0GO.transform.parent = AVOWUI.singleton.transform;
		
		lightening1GO = AVOWUI.singleton.InstantiateLightening();
		lightening1GO.transform.parent = AVOWUI.singleton.transform;
		
		uiZPos = AVOWUI.singleton.transform.position.z;
		
	}
	
	
	public override void Update () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		StateUpdate();
		CommandsUpdate();
		VizUpdate();
		
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
		
		
		
		//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		
		// If we don't have a held connection, then we find the closest node and that's all
		if (!heldConnection){
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
			//			float minDist = FindClosestComponent(mouseWorldPos, connection0, maxLighteningDist, ref closestObj, ref closestPos);
			float minDist = maxLighteningDist;
			FindClosestComponentCentre(mouseWorldPos, connectionGO, maxLighteningDist, ref closestObj, ref closestPos);
			connectionGO = closestObj;
			connectionPos = closestPos;
			
			if (buttonPressed && connectionGO != null){
				heldConnection = true;
			}
		}
		else{
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			
			connectionPos = 0.5f * (component.GetConnectionPos0() + component.GetConnectionPos1());
			// If not inside the held gap, find the next closest thing - favouring whatever we have connected already
			if (!isInside){

				if (buttonReleased){
					heldConnection = false;
					insideState = InsideGapState.kOnCancel;
					
				}
			}
			else{
				if (buttonReleased){
					heldConnection = false;
					heldGapCommand.ExecuteStep();
					AVOWUI.singleton.commands.Push(heldGapCommand);
					heldGapCommand = null;
					connectionGO = null;
					insideState = InsideGapState.kOnRemove;
				}
			}
			
			
		}
		
		// If we have a gap which we are holding open, check if our mous is inside the gap
		isInside = false;
		if (connectionGO != null){
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			isInside = component.IsPointInsideGap(mouseWorldPos);
		}
		
		if (heldConnection || insideState == InsideGapState.kOnCancel || insideState == InsideGapState.kOnRemove) HandleCubeInsideGap();
	}
	
	AVOWComponent GetHeldCommandComponent(){
		if (heldGapCommand == null) return null;
		
		GameObject go = heldGapCommand.GetNewComponent();
		if (go == null) return null;
		
		AVOWComponent component = go.GetComponent<AVOWComponent>();
		return component;
	}
	
	
	void HandleCubeInsideGap(){
		
		// Manage the inside state machine
		switch (insideState){
			case InsideGapState.kUncreated:{
				insideState = (isInside) ? InsideGapState.kCreateInside : InsideGapState.kCreaeOutside;
				break;
			}
			case InsideGapState.kCreateInside:{
							
				// Create a new cube which will travel to the gap
				ActiveCubeAtComponent(connectionGO);
				connectionGO.GetComponent<AVOWComponent>().isInteractive = false;
			
				

				
				insideState = InsideGapState.kInsideAndExiting;
				

				break;
			}
			
			case InsideGapState.kCreaeOutside:{
				insideState = InsideGapState.kOutside;

				
				break;
			}
			
			case InsideGapState.kOutside:{
				if (isInside) {
					// Create a new cube which will travel to the gap
					ActiveCubeAtComponent(connectionGO);
					
					

					
					insideState = InsideGapState.kInsideAndExiting;
					insideLerpSpeed = minLerpSpeed;
				}
				break;
			}
			case InsideGapState.kInsideAndExiting:{
				
				// Move our inside cube to where it needs to be
				AVOWComponent component = GetHeldCommandComponent();
				
				if (component != null && isInside){
					float distRemaining = LerpToCursor(cursorCube, insideLerpSpeed);
					
					if (MathUtils.FP.Feq(distRemaining, 0, 0.1f)){
						cursorCube = RejoinToCursor(cursorCube);
						GameObject.Destroy(insideCube);
						insideCube = null;
						insideState = InsideGapState.kFullyInside;
						insideLerpSpeed = minLerpSpeed;
						//RemoveMetal(cursorCube)
					}
				}
				else{
					insideState = InsideGapState.kOutsideAndEntering;
					insideLerpSpeed = minLerpSpeed;
					
				}
				
				insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
				
				break;
			}
			case InsideGapState.kFullyInside:{
				AVOWComponent component = GetHeldCommandComponent();
				if (component == null || !isInside){
					insideState = InsideGapState.kOutsideAndEntering;
					insideLerpSpeed = minLerpSpeed;
					// Create a new cube which will travel to the gap
					ActiveCubeAtCursor(cursorCube);
					RemoveMetal(cursorCube);
					
				}
				break;
			}
			case InsideGapState.kOutsideAndEntering:{
				
				float distRemaining = LerpToComponent(connectionGO.GetComponent<AVOWComponent>(), insideLerpSpeed);
				
				if (MathUtils.FP.Feq(distRemaining, 0, 0.01f)){
					insideState = InsideGapState.kOutside;
					
					// Remove our insidecube when it gets there
					GameObject.Destroy(insideCube);
					insideCube = null;
					connectionGO.GetComponent<AVOWComponent>().isInteractive = true;
					//cursorCube = RejoinToCursor(cursorCube);
				}
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null && isInside){
					insideState = InsideGapState.kInsideAndExiting;	
					insideLerpSpeed = minLerpSpeed;
				}
				insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
				break;
			}			
			case InsideGapState.kOnRemove:{
				insideState = InsideGapState.kUncreated;

			
				GameObject.Destroy(insideCube);
				insideCube = null;
				break;
			}
			case InsideGapState.kOnCancel:{
				insideState = InsideGapState.kUncreated;
				
			
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null){
					component.isInteractive = true;
				}				
			
				GameObject.Destroy(insideCube);
				insideCube = null;
				break;
			}
		}	
		
	}
	
	
	void VizUpdate(){
		
		Vector3 lighteningConductorPos = mouseWorldPos;//(isInside) ? insideCube.transform.position : mouseWorldPos;
		AVOWGraph.singleton.EnableAllLightening();
		
		lightening1GO.SetActive(false);
			
			
		// Lightening to connection 0 - which is always a node
		if (connectionGO != null){
			lightening0GO.SetActive(true);
			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
			if (!isInside || !heldConnection){
				lightening0.startPoint = lighteningConductorPos;
				lightening0.endPoint = connectionPos;
			}
			else{
				lightening1GO.SetActive(true);
				Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
				
				// Need to fin the two points on the nodes to connect to
				Vector3 node0Pos = Vector3.zero;
				Vector3 node1Pos = Vector3.zero;
				AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();

				FindClosestPointOnNode(lighteningConductorPos, component.node0GO, ref node0Pos);
				lightening0.startPoint = lighteningConductorPos;
				lightening0.endPoint = node0Pos;
				
				FindClosestPointOnNode(lighteningConductorPos, component.node1GO, ref node1Pos);
				lightening1.startPoint = lighteningConductorPos;
				lightening1.endPoint = node1Pos;

				float len1 = (lightening0.startPoint  - lightening1.endPoint).magnitude;
				lightening1.numStages = Mathf.Max ((int)(len1 * 10), 2);
				lightening1.size =  heldConnection ? 0.4f : 0.1f;
				lightening1.ConstructMesh();
			}
			float len0 = (lightening0.startPoint  - lightening0.endPoint).magnitude;
			lightening0.numStages = Mathf.Max ((int)(len0 * 10), 2);
			lightening0.size =  heldConnection ? 0.4f : 0.1f;
			lightening0.ConstructMesh();
		}
		else{
			lightening0GO.SetActive(false);
				
		}
		
		
		// If we are connected to something then rotate the cube a bit
		if (connectionGO != null){
			cursorCube.transform.Rotate (new Vector3(1, 2, 4));
		}
		
	}
	
	
	void CommandsUpdate(){

		// if we have a command already check if we need to undo it
		if (heldGapCommand != null && !heldConnection){
			heldGapCommand.UndoStep();
			heldGapCommand = null;
		}
		
		// If we still have a command, then this command is still valid and nothing more to do - however, if we don't have one, 
		// then perhaps we should make one?
		if (heldGapCommand == null && connectionGO != null && heldConnection){
			heldGapConnection = connectionGO;
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			
			heldGapCommand = new AVOWCommandRemove(heldGapConnection, mouseWorldPos);
			Debug.Log("new AVOWCommandRemove " + Time.time);

			heldGapCommand.ExecuteStep();
			
			// Ned to force the sim to do an update (this would be better if all this logic was in a fixed update and it 
			// was more tightly controlled
			AVOWSim.singleton.FixedUpdate();
		}

	}
	
		
}

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	
	public GameObject cursorCubePrefab;
	public GameObject lighteningPrefab;
	
	AVOWUITool	uiTool;
			
	

	public Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();

	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	

	void Update(){
		uiTool.Update();
	}
	
	public GameObject InstantiateCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	public GameObject InstantiateLightening(){
		GameObject obj = GameObject.Instantiate(lighteningPrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	


	

	

	
	void Start(){
		
		AVOWGraph graph = AVOWGraph.singleton;

		// Simple start
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();

				
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node0GO);
		
		
		/*
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		GameObject node2GO = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2GO, node0GO);	
		*/
		
		AVOWSim.singleton.Recalc();
		uiTool = new AVOWUICreateTool();
		
		uiTool.Start();
		
	}
	
	public void SetCreateTool(){
		uiTool = new AVOWUICreateTool();
		uiTool.Start();
	}
	
	public void SetDeleteTool(){
		uiTool = new AVOWUIDeleteTool();
		uiTool.Start();
	}
	
}

