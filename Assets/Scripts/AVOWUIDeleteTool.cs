using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
		kCreateOutside,
		kCreateInside,
		kInside,
		kInsideAndTransitioning,
		kOutsideAndTransitioning,
		kOutside,
		kOnRemove,
		kOnCancel
	};
	
	public bool			  	isOutside = true;
	public InsideGapState 	insideState = InsideGapState.kUncreated;
	float					maxLerpSpeed = 0.5f;
	float					minLerpSpeed = 0f;
	float 					insideLerpSpeed;
	
	
	public override GameObject GetCursorCube(){
		return cursorCube;
	}
	
	
	public override void Start(){
		cursorCube = InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		
		// Remove the metal material from the cube that we have
		RemoveMetal(cursorCube);
		
		
		lightening0GO = AVOWUI.singleton.InstantiateGreenLightening();
		lightening0GO.transform.parent = AVOWUI.singleton.transform;
		
		lightening1GO = AVOWUI.singleton.InstantiateGreenLightening();
		lightening1GO.transform.parent = AVOWUI.singleton.transform;
		
		uiZPos = AVOWUI.singleton.transform.position.z;
		
	}
	
	public override void OnDestroy(){
		GameObject.Destroy(cursorCube);
		GameObject.Destroy(lightening0GO);
		GameObject.Destroy(lightening1GO);
		GameObject.Destroy(insideCube);
	}
	
	
	
	
	public override void Update () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		StateUpdate();
		CommandsUpdate();
		VizUpdate();
		
	}
	
	public override bool IsInsideGap(){
		return !isOutside;
	}
	
	
	public override int GetNumConnections(){
		return (connectionGO == null) ? 0 : 1;
	}
	
	
	public override bool IsHolding(){
		return heldConnection;
	}
	
	protected override GameObject InstantiateCursorCube(){
		return AVOWUI.singleton.InstantiateGreenCursorCube();
	}
	
	void StateUpdate(){
	
		// Calc the mouse posiiton on world space
		Vector3 screenCentre = new Vector3(Screen.width * 0.75f, Screen.height * 0.5f, 0);
		Vector3 inputScreenPos = Vector3.Lerp(screenCentre, Input.mousePosition, AVOWConfig.singleton.cubeToCursor.GetValue());
		
		Vector3 mousePos = inputScreenPos;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		// Get the mouse buttons
//		bool  buttonPressed = (!AVOWConfig.singleton.tutDisableMouseButtton && Input.GetMouseButtonDown(0));
		bool  buttonIsDown =   (!AVOWConfig.singleton.tutDisableMouseButtton && Input.GetMouseButton(0));
		bool  buttonReleased = (!AVOWConfig.singleton.tutDisableMouseButtton && Input.GetMouseButtonUp(0));
		//		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		// Set the cursor cubes position
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;
		
		
		//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		
		// If we don't have a held connection, then we find the closest node and that's all
		if (!heldConnection){
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
			FindClosestComponentCentre(mouseWorldPos, connectionGO, maxLighteningDist, ref closestObj, ref closestPos);
			connectionGO = closestObj;
			connectionPos = closestPos;
			
			if (buttonIsDown && connectionGO != null){
				heldConnection = true;
			}
		}
		else{
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			
			connectionPos = 0.5f * (component.GetConnectionPos0() + component.GetConnectionPos1());
			connectionPos.z = uiZPos;
			// If not inside the held gap, find the next closest thing - favouring whatever we have connected already
			if (isOutside){
				
				if (buttonReleased){
					heldConnection = false;
					heldGapCommand.ExecuteStep();
					AVOWUI.singleton.commands.Push(heldGapCommand);
					heldGapCommand = null;
					connectionGO = null;
					insideState = InsideGapState.kOnRemove;
				}
			}
			else{
				if (buttonReleased){
					heldConnection = false;
					insideState = InsideGapState.kOnCancel;
					
				}
			}
			
			
		}
		
		// If we have a gap which we are holding open, check if our mous is inside the gap
		isOutside = true;
		if (connectionGO != null){
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			isOutside = !component.IsPointInsideGap(mouseWorldPos);
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
			insideState = (isOutside) ? InsideGapState.kCreateOutside : InsideGapState.kCreateInside;
			break;
		}
		case InsideGapState.kCreateOutside:{
			
			// Create a new cube which will travel to the gap
			ActiveCubeAtComponent(connectionGO);
			connectionGO.GetComponent<AVOWComponent>().isInteractive = false;
			
			
			
			
			insideState = InsideGapState.kOutsideAndTransitioning;
			
			
			break;
		}
			
		case InsideGapState.kCreateInside:{
			insideState = InsideGapState.kInside;
			
			
			break;
		}
			
		case InsideGapState.kInside:{
			if (isOutside) {
				// Create a new cube which will travel to the gap
				ActiveCubeAtComponent(connectionGO);
				
				
				
				
				insideState = InsideGapState.kOutsideAndTransitioning;
				insideLerpSpeed = minLerpSpeed;
			}
			break;
		}
		case InsideGapState.kOutsideAndTransitioning:{
			
			// Move our inside cube to where it needs to be
			AVOWComponent component = GetHeldCommandComponent();
			
			if (component != null && isOutside){
				float distRemaining = LerpToCursor(cursorCube, insideLerpSpeed);
				
				if (MathUtils.FP.Feq(distRemaining, 0, 0.1f)){
					cursorCube = RejoinToCursor(cursorCube);
					GameObject.Destroy(insideCube);
					insideCube = null;
					insideState = InsideGapState.kOutside;
					insideLerpSpeed = minLerpSpeed;
					//RemoveMetal(cursorCube)
				}
			}
			else{
				insideState = InsideGapState.kInsideAndTransitioning;
				insideLerpSpeed = minLerpSpeed;
				
			}
			
			insideLerpSpeed = Mathf.Lerp (insideLerpSpeed, maxLerpSpeed, 0.1f);
			
			break;
		}
		case InsideGapState.kOutside:{
			AVOWComponent component = GetHeldCommandComponent();
			if (component == null || !isOutside){
				insideState = InsideGapState.kInsideAndTransitioning;
				insideLerpSpeed = minLerpSpeed;
				// Create a new cube which will travel to the gap
				ActiveCubeAtCursor(cursorCube);
				RemoveMetal(cursorCube);
				
			}
			break;
		}
		case InsideGapState.kInsideAndTransitioning:{
			
			float distRemaining = LerpToComponent(connectionGO.GetComponent<AVOWComponent>(), insideLerpSpeed);
			
			if (MathUtils.FP.Feq(distRemaining, 0, 0.01f)){
				insideState = InsideGapState.kInside;
				
				// Remove our insidecube when it gets there
				GameObject.Destroy(insideCube);
				insideCube = null;
				connectionGO.GetComponent<AVOWComponent>().isInteractive = true;
				//cursorCube = RejoinToCursor(cursorCube);
			}
			AVOWComponent component = GetHeldCommandComponent();
			if (component != null && isOutside){
				insideState = InsideGapState.kOutsideAndTransitioning;	
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
		AVOWGraph.singleton.ClearAdditionalConnectionPoints();
		if (connectionGO != null){
			
			lightening0GO.SetActive(true);
			Lightening lightening0 = lightening0GO.GetComponent<Lightening>();
			if (!isOutside || !heldConnection){
				lightening0.startPoint = lighteningConductorPos;
				lightening0.endPoint = connectionPos;
			}
			else{
				AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
				
				lightening1GO.SetActive(true);
				Lightening lightening1 = lightening1GO.GetComponent<Lightening>();
				
				// Need to fin the two points on the nodes to connect to
				Vector3 node0Pos = Vector3.zero;
				Vector3 node1Pos = Vector3.zero;
				
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
				
				
				// Extend the light bar on this node to touch the conneciton point
				if (component.node0GO != null){
					component.node0GO.GetComponent<AVOWNode>().addConnPos = node0Pos.x;
				}
				
				if (component.node1GO != null && component.node1GO.GetComponent<AVOWNode>() != null){
					component.node1GO.GetComponent<AVOWNode>().addConnPos = node1Pos.x;
				}	
			}
			float len0 = (lightening0.startPoint  - lightening0.endPoint).magnitude;
			lightening0.numStages = Mathf.Max ((int)(len0 * 10), 2);
			lightening0.size =  heldConnection ? 0.4f : 0.1f;
			lightening0.ConstructMesh();
			if (len0 < 0.01f) lightening1GO.SetActive(false);
	
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
			
			heldGapCommand = new AVOWCommandRemove(heldGapConnection, mouseWorldPos);
			Debug.Log("new AVOWCommandRemove " + Time.time);
			
			heldGapCommand.ExecuteStep();
			
			// Ned to force the sim to do an update (this would be better if all this logic was in a fixed update and it 
			// was more tightly controlled
			AVOWSim.singleton.FixedUpdate();
		}
		
	}
	
	
}