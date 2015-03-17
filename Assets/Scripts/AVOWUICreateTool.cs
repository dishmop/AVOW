using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AVOWUICreateTool :  AVOWUITool{
	
	
	// New attempt at encoding the state of the UI
	public GameObject 	connection0;
	public GameObject 	connection1;
	public Vector3 		connection0Pos;
	public Vector3 		connection1Pos;	
	public int			connection1WhichPoint = -1;	// WHich node we are attached to (if connection1 is a component)
	public bool 		heldConnection;
	public AVOWCommand 	heldGapCommand;
	public GameObject 	heldGapConnection1;
	
	
	float		newHOrder;
	
	
	public enum InsideGapState{	
		kOutside,
		kOutsideAndTransitioning,
		kInsideAndTransitioning,
		kInside,
		kOnNewComponent
	};
	
	public bool			  	isInside = false;
	public InsideGapState 	insideState = InsideGapState.kOutside;
	float					maxLerpSpeed = 0.5f;
	float					minLerpSpeed = 0f;
	float 					insideLerpSpeed;
	
	Vector3 				mouseWorldPos;
	Vector3					ghostMousePos;
	
	GameObject 				cursorCube;
	GameObject 				lightening0GO;
	GameObject 				lightening1GO;
	
	// We store a length of the current 0 connection and it only updates
	// when we have no "temporary" compoents in the graph
	// This stops oscilation when in the middle of making one
	float					confirmedConnectionHeight;
	
	
	public override void Start(){
		cursorCube = InstantiateCursorCube();
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
	
	public override GameObject GetCursorCube(){
		return cursorCube;
	}
	
	
	protected override GameObject InstantiateCursorCube(){
		return AVOWUI.singleton.InstantiateBlueCursorCube();		
	}
		
	
	public override void OnDestroy(){
		GameObject.Destroy(cursorCube);
		GameObject.Destroy(lightening0GO);
		GameObject.Destroy(lightening1GO);
		GameObject.Destroy(insideCube);
	}
	
	public override bool IsHolding(){
		return heldConnection;
	}
	
	public override int GetNumConnections(){
		int count = 0;
		if (connection0 != null) count++;
		if (connection1 != null) count++;
		return count; 
	}
	
	
	
	public override GameObject GetConnection(int index){
		if (index == 0) return connection0;
		if (index == 1) return connection1;
		return null;
	}
	
	public override bool IsInsideGap(){
		return isInside;
	}
	

	
	void StateUpdate(){
	
		// Calc the mouse posiiton on world space
		Vector3 screenCentre = AVOWConfig.singleton.GetViewCentre();
		Vector3 inputScreenPos = Vector3.Lerp(screenCentre, Input.mousePosition, AVOWConfig.singleton.cubeToCursor.GetValue());
		
		Vector3 mousePos = inputScreenPos;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		// Get the mouse buttons
		bool  buttonPressed = (!AVOWConfig.singleton.tutDisableMouseButtton && Input.GetMouseButtonDown(0));
		bool  buttonReleased = (!AVOWConfig.singleton.tutDisableMouseButtton && Input.GetMouseButtonUp(0));
		//		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		// Set the cursor cubes position
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;
		
		if (AVOWConfig.singleton.tutDisableConnections) 
		{
			connection0 = null;
			connection1 = null;
			return;
		}
		
		
		//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		ghostMousePos = Vector3.zero;
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
			
			if (!AVOWGraph.singleton.HasUnconfirmedComponents()){
				confirmedConnectionHeight = connection0Pos.y ;
			}
			
			// If not inside the held gap, find the next closest thing - favouring whatever we have connected already
			if (!isInside){
				GameObject closestObj = null;
				Vector3 closestPos = Vector3.zero;
				
				float minDist = maxLighteningDist;
				// Only search for components if there is more than just a battery
				ghostMousePos = mouseWorldPos + new Vector3(0, (mouseWorldPos.y - confirmedConnectionHeight) * 0.85f, 0);
				if (AVOWGraph.singleton.allComponents.Count > 1 && !AVOWConfig.singleton.tutDisable2ndComponentConnections){
					minDist = FindClosestComponent(ghostMousePos, connection0, connection1, maxLighteningDist, ref closestObj, ref closestPos, ref connection1WhichPoint);
				}
				if (!AVOWConfig.singleton.tutDisable2ndBarConnections){
					minDist = FindClosestNode(ghostMousePos, connection0, minDist, connection1, ref closestObj, ref closestPos);			
				}
				connection1 = closestObj;
				connection1Pos = closestPos;	
				if (buttonReleased){
					heldConnection = false;
					connection1 = null;
				}
			}
			else{
				if (buttonReleased){
					// do logic to see if the thing should be creared
					// If a node to node connection
					bool okToCreate = true;
					if (connection1.GetComponent<AVOWNode>() != null && AVOWConfig.singleton.tutDisableBarConstruction){
						okToCreate = false;
					}
					
					// If  anode to component connection
					if (connection1.GetComponent<AVOWComponent>() != null && AVOWConfig.singleton.tutDisableComponentConstruction){
						okToCreate = false;
					}
					if (okToCreate){
						heldConnection = false;
						heldGapCommand.ExecuteStep();
						AVOWUI.singleton.commands.Push(heldGapCommand);
						heldGapCommand = null;
						heldGapConnection1 = null;
						connection1 = null;
						connection0 = null;
						insideState = InsideGapState.kOnNewComponent;
					}
					else{
						heldConnection = false;
						connection1 = null;
					}
				}
			}

			
			
		}
//		Debug.DrawLine(mouseWorldPos, ghostMousePos, Color.red);
			
		// Extend the light bar on this node to touch the conneciton point
		if (connection0 != null){
			connection0.GetComponent<AVOWNode>().addConnPos = connection0Pos.x;
		}
		
		if (connection1 != null && connection1.GetComponent<AVOWNode>() != null){
			connection1.GetComponent<AVOWNode>().addConnPos = connection1Pos.x;
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
					
					insideState = InsideGapState.kInsideAndTransitioning;
					insideLerpSpeed = minLerpSpeed;
				}
				break;
			}
			case InsideGapState.kInsideAndTransitioning:{
				
				// Move our inside cube to where it needs to be
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null && isInside){
					float distRemaining = LerpToComponent(component, insideLerpSpeed);
					
					if (MathUtils.FP.Feq(distRemaining, 0, 0.001f)){
						insideState = InsideGapState.kInside;
						insideLerpSpeed = minLerpSpeed;
					}
				}
				else{
					insideState = InsideGapState.kOutsideAndTransitioning;
					insideLerpSpeed = minLerpSpeed;
					
				}
				
				insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
				
				break;
			}
			case InsideGapState.kInside:{
				AVOWComponent component = GetHeldCommandComponent();
				if (component == null || !isInside){
					insideState = InsideGapState.kOutsideAndTransitioning;
					insideLerpSpeed = minLerpSpeed;
					
				}
				break;
			}
			case InsideGapState.kOutsideAndTransitioning:{
				
				float distRemaining = LerpToCursor(cursorCube, insideLerpSpeed);
				
				if (MathUtils.FP.Feq(distRemaining, 0, 0.001f)){
					insideState = InsideGapState.kOutside;
					
					// Remake our cursor
					cursorCube = RejoinToCursor(cursorCube);
					
				}
				AVOWComponent component = GetHeldCommandComponent();
				if (component != null && isInside){
					insideState = InsideGapState.kInsideAndTransitioning;	
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
	
	
	void HandleVizConnectors(){
		if (connection0 == null || !heldConnection){
			foreach(GameObject go in AVOWGraph.singleton.allComponents){
				go.transform.FindChild("ConnectionSphere0").GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 214.0f/255.0f, 19.0f/255.0f));
				go.transform.FindChild("ConnectionSphere1").GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 214.0f/255.0f, 19.0f/255.0f));
				go.transform.FindChild("ConnectionSphere0").FindChild ("Blob Shadow Projector").gameObject.SetActive(true);
				go.transform.FindChild("ConnectionSphere1").FindChild ("Blob Shadow Projector").gameObject.SetActive(true);
			}
		}
		else{
			List<GameObject> spheres = FindConnectionSpheres(connection0, connection1);
			foreach(GameObject go in AVOWGraph.singleton.allComponents){
				GameObject sphere0 = go.transform.FindChild("ConnectionSphere0").gameObject;
				GameObject sphere1 = go.transform.FindChild("ConnectionSphere1").gameObject;
				if (spheres.Exists (obj => (obj == sphere0))){
					sphere0.GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 214.0f/255.0f, 19.0f/255.0f));
					go.transform.FindChild("ConnectionSphere0").FindChild ("Blob Shadow Projector").gameObject.SetActive(true);
				}
				else{
					sphere0.GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 0.125f, 0));
					go.transform.FindChild("ConnectionSphere0").FindChild ("Blob Shadow Projector").gameObject.SetActive(false);
				}
				if (spheres.Exists (obj => (obj == sphere1))){
					sphere1.GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 214.0f/255.0f, 19.0f/255.0f));
					go.transform.FindChild("ConnectionSphere1").FindChild ("Blob Shadow Projector").gameObject.SetActive(true);
					
				}
				else{
					sphere1.GetComponent<Renderer>().materials[1].SetColor("_Color", new Color(0, 0.125f, 0));
					go.transform.FindChild("ConnectionSphere1").FindChild ("Blob Shadow Projector").gameObject.SetActive(false);
				}
			}
		}
		//FindConnectionSpheres
	}
	
	
	
	void VizUpdate(){
	
		// if we are holding a node then select that node
		AVOWGraph.singleton.UnselectAllNodes();
		if (heldConnection){
			if (connection0.GetComponent<AVOWNode>() != null) 
				connection0.GetComponent<AVOWNode>().SetSelected(true);
		}
		
		// For some reason, sometimes the inside cube doesn't exist
		if (isInside && insideCube == null){
			Debug.Log("Error - inside cube doesn't exist");
			return;
		}
		
		Vector3 lighteningConductorPos = (isInside) ? insideCube.transform.position : mouseWorldPos;
		Vector3 connection0PosUse = (isInside) ? new Vector3(insideCube.transform.position.x, connection0Pos.y, connection0Pos.z) : connection0Pos;
		Vector3 connection1PosUse = connection1Pos;
		
		// Is inside a gap
		if (isInside && connection1 != null){
			// and that gap is between two nodes
			if (connection1.GetComponent<AVOWComponent>() == null){
				connection1PosUse = new Vector3(insideCube.transform.position.x, connection1Pos.y, connection1Pos.z);
			}else{
				connection1PosUse = (connection1WhichPoint == 0) ? connection1.GetComponent<AVOWComponent>().GetConnectionPos0() : connection1.GetComponent<AVOWComponent>().GetConnectionPos1() ;
				
			}
		}
		
		
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
		
		// Set the connector spheres on the resitors to be dark if we have a connection 0 and we are not
		// something we can connect to
		HandleVizConnectors();
		
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
			if (!isInside && connection0 != null && heldConnection){
				lightening1GO.SetActive(true);
				
				Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
				lightening1.startPoint = lighteningConductorPos;
				float dist = (ghostMousePos - lighteningConductorPos).magnitude;
				lightening1.endPoint = ghostMousePos + new Vector3(Random.Range (-0.25f * dist, 0.25f * dist), Random.Range (-0.25f * dist, 0.25f * dist), 0);
				
				float len = (lightening1.startPoint  - lightening1.endPoint).magnitude;
				lightening1.numStages = Mathf.Max ((int)(len * 10), 2);
				lightening1.size = 0.1f;
				lightening1.ConstructMesh();
			}
			else{
				lightening1GO.SetActive(false);
			}
		}	
		AVOWGraph.singleton.ClearAdditionalConnectionPoints();
		
		// Extend the light bar on this node to touch the conneciton point
		if (connection0 != null){
			connection0.GetComponent<AVOWNode>().addConnPos = connection0PosUse.x;
		}
		
		if (connection1 != null && connection1.GetComponent<AVOWNode>() != null){
			connection1.GetComponent<AVOWNode>().addConnPos = connection1PosUse.x;
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
			// if the connection1 has changed or we are no longer holding anything- but we were
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
		
	//	string debugText = "";
		if (blockBefore == null && blockAfter == null){
			// I'm not sure that this can ever happen
			newHOrder = 0;
		//	debugText = "BeforeMinH = null, BeforeMaxH = null, AfterMinH = null, AfterMaxH = null";
		}
		else if (blockBefore == null)
		{
			newHOrder = blockAfter.minOrder - 1;
		//	debugText = "BeforeMinH = null, BeforeMaxH = null, AfterMinH = " + blockAfter.minOrder + " , AfterMaxH = " + blockAfter.maxOrder;
		}
		else if (blockAfter == null)
		{
			newHOrder = blockBefore.maxOrder + 1;
		//	debugText = "BeforeMinH = " + blockBefore.minOrder + ", BeforeMaxH = " + blockBefore.maxOrder + ", AfterMinH = null , AfterMaxH = null";
		}
		else{	
			newHOrder = (blockBefore.maxOrder + blockAfter.minOrder) * 0.5f;
		//	debugText = "BeforeMinH = " + blockBefore.minOrder + ", BeforeMaxH = " + blockBefore.maxOrder + ", AfterMinH = " + blockAfter.minOrder + " , AfterMaxH = " + blockAfter.maxOrder;
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


