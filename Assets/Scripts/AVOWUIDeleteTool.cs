using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

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
	
	float 	heldTime;
	bool wasOutside;
	
	const int		kLoadSaveVersion = 1;	
	
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
	
	bool valuesHaveChanged;
	float[] optValues = new float[100];
	
	void ResetOptValues(){
		
		int i = 0;

		optValues[i++] = Convert.ToSingle (connectionGO != null ? connectionGO.GetInstanceID() : 0);
		optValues[i++] = Convert.ToSingle (connectionPos[0]);
		optValues[i++] = Convert.ToSingle (connectionPos[1]);
		optValues[i++] = Convert.ToSingle (connectionPos[2]);
		optValues[i++] = Convert.ToSingle (heldConnection);
		
		optValues[i++] = Convert.ToSingle (heldGapCommand != null);
		optValues[i++] = Convert.ToSingle (heldGapConnection != null ? heldGapConnection.GetInstanceID() : 0);
		optValues[i++] = Convert.ToSingle (mouseWorldPos[0]);
		optValues[i++] = Convert.ToSingle (mouseWorldPos[1]);
		optValues[i++] = Convert.ToSingle (mouseWorldPos[2]);
		
		optValues[i++] = Convert.ToSingle (cursorCube != null);
		if (cursorCube != null){
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[0]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[1]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localPosition[2]);
			
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[0]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[1]);
			optValues[i++] = Convert.ToSingle (cursorCube.transform.localScale[2]);
			
		}
		
		// We only look at a few bits of info about the lightening
		optValues[i++] = Convert.ToSingle (lightening0GO != null);
		if (lightening0GO != null){
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[0]);
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[1]);
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[2]);
			
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[0]);
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[1]);
			optValues[i++] = Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[2]);
		}
		
		// We only look at a few bits of info about the lightening
		optValues[i++] = Convert.ToSingle (lightening1GO != null);
		if (lightening1GO != null){
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[0]);
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[1]);
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[2]);
			
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[0]);
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[1]);
			optValues[i++] = Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[2]);
		}
		
		optValues[i++] = Convert.ToSingle (isOutside);
		optValues[i++] = Convert.ToSingle (insideState);
		optValues[i++] = Convert.ToSingle (maxLerpSpeed);
		optValues[i++] = Convert.ToSingle (minLerpSpeed);
		optValues[i++] = Convert.ToSingle (insideLerpSpeed);
				
		
	}
	
	
	void TestIfValuesHaveChanged(){
		int i = 0;
		bool diff = false;
		
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (connectionGO != null ? connectionGO.GetInstanceID() : 0));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (connectionPos[0]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (connectionPos[1]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (connectionPos[2]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (heldConnection));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (heldGapCommand != null));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (heldGapConnection != null ? heldGapConnection.GetInstanceID() : 0));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[0]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[1]));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (mouseWorldPos[2]));
		
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube != null));
		if (cursorCube != null){
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localPosition[2]));
			
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (cursorCube.transform.localScale[2]));
		}

		// We only look at a few bits of info about the lightening
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO != null));
		if (lightening0GO != null){
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().startPoint[2]));
			
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening0GO.GetComponent<Lightening>().endPoint[2]));
		}
		
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO != null));
		if (lightening1GO != null){
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().startPoint[2]));
			
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[0]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[1]));
			diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (lightening1GO.GetComponent<Lightening>().endPoint[2]));
		}
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (isOutside));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (insideState));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (maxLerpSpeed));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (minLerpSpeed));
		diff = diff || !MathUtils.FP.Feq (optValues[i++], Convert.ToSingle (insideLerpSpeed));

		if (diff){
			valuesHaveChanged = true;
			ResetOptValues();
		}
		
	}
	
	
	public override void ResetOptFlags(){
		valuesHaveChanged = false;
	}
	
	
	
	public override GameObject GetCursorCube(){
		return cursorCube;
	}
	
	
	
	void ModifyGapCommand(int code){
		int currentCode = SerialisationFactory.GetCommandCode (heldGapCommand);
		if (code != currentCode){
			heldGapCommand = SerialisationFactory.ConstructCommandFromCode(code);
		}
	}
	
	public override void Serialise(BinaryWriter bw){
		base.Serialise(bw);
		bw.Write (kLoadSaveVersion);
		bw.Write (valuesHaveChanged);
		if (!valuesHaveChanged) return;
		
		
		AVOWGraph.singleton.SerialiseRef(bw, connectionGO);
		
		bw.Write (connectionPos);
		bw.Write (heldConnection);
		bw.Write (SerialisationFactory.GetCommandCode(heldGapCommand));
		if (heldGapCommand != null){
			heldGapCommand.Serialise(bw);
		}
		AVOWGraph.singleton.SerialiseRef(bw, heldGapConnection);

		bw.Write (mouseWorldPos);
		
		bw.Write (cursorCube != null);
		if (cursorCube != null){
			bw.Write (cursorCube.transform.localPosition);
//			bw.Write (cursorCube.transform.localRotation);
			bw.Write (cursorCube.transform.localScale);
		}
		bw.Write (lightening0GO != null);
		if (lightening0GO != null){
			lightening0GO.GetComponent<Lightening>().Serialise(bw);
		}
		
		bw.Write (lightening1GO != null);
		if (lightening1GO != null){
			lightening1GO.GetComponent<Lightening>().Serialise(bw);
		}


		bw.Write (isOutside);
		bw.Write ((int)insideState);
		
		bw.Write (maxLerpSpeed);
		bw.Write (minLerpSpeed);
		bw.Write (insideLerpSpeed);

	}
	
	public override void Deserialise(BinaryReader br){
		base.Deserialise(br);
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				valuesHaveChanged = br.ReadBoolean();
				if (!valuesHaveChanged) break;
				
				connectionGO = AVOWGraph.singleton.DeseraliseRef(br);
				connectionPos = br.ReadVector3 ();
				heldConnection = br.ReadBoolean();
			
				int commandCode = br.ReadInt32 ();
				ModifyGapCommand(commandCode);
				if (heldGapCommand != null){
					heldGapCommand.Deserialise(br);
				}
	
				heldGapConnection = AVOWGraph.singleton.DeseraliseRef(br);
				mouseWorldPos = br.ReadVector3();
				
				bool hasCursorCube = br.ReadBoolean();
				if (hasCursorCube && cursorCube == null){
					cursorCube = InstantiateCursorCube();
				}
				else if (!hasCursorCube && cursorCube != null){
					GameObject.Destroy(cursorCube);
					cursorCube = null;
				}
				
				if (cursorCube != null){
					cursorCube.transform.localPosition = br.ReadVector3 ();
//					cursorCube.transform.localRotation = br.ReadQuaternion ();
					cursorCube.transform.localScale = br.ReadVector3 ();
				}
				
				
				bool hasLightening0 = br.ReadBoolean();
				if (hasLightening0 && lightening0GO == null){
					lightening0GO = AVOWUI.singleton.InstantiateLightening();
					lightening0GO.transform.parent = AVOWUI.singleton.transform;
				}
				else if (!hasLightening0 && lightening0GO != null){
					GameObject.Destroy ( lightening0GO);
					lightening0GO = null;
				}
				if (lightening0GO){
					lightening0GO.GetComponent<Lightening>().Deserialise(br);
				}
				
				bool hasLightening1 = br.ReadBoolean();
				if (hasLightening1 && lightening1GO == null){
					lightening1GO = AVOWUI.singleton.InstantiateLightening();
					lightening1GO.transform.parent = AVOWUI.singleton.transform;
				}
				else if (!hasLightening1 && lightening1GO != null){
					GameObject.Destroy ( lightening1GO);
					lightening1GO = null;
				}
				if (lightening1GO){
					lightening1GO.GetComponent<Lightening>().Deserialise(br);
				}
				
				isOutside = br.ReadBoolean();
				insideState = (InsideGapState)br.ReadInt32();
				maxLerpSpeed = br.ReadSingle();
				minLerpSpeed = br.ReadSingle();
				insideLerpSpeed = br.ReadSingle();
			
			
			
				break;
			}
		}
	}
	
	public override void Startup(){
		cursorCube = InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		
		// Remove the metal material from the cube that we have
		RemoveMetal(cursorCube);
		
		
		lightening0GO = AVOWUI.singleton.InstantiateGreenLightening();
		lightening0GO.transform.parent = AVOWUI.singleton.transform;
		lightening0GO.SetActive(false);
		
		lightening1GO = AVOWUI.singleton.InstantiateGreenLightening();
		lightening1GO.transform.parent = AVOWUI.singleton.transform;
		lightening1GO.SetActive(false);
		
		uiZPos = AVOWUI.singleton.transform.position.z;
		ResetButtonFlags();
		
		RenderUpdate();
			
		
	}
	
	public override void OnDestroy(){
		GameObject.Destroy(cursorCube);
		GameObject.Destroy(lightening0GO);
		GameObject.Destroy(lightening1GO);
		GameObject.Destroy(insideCube);
		AVOWSim.singleton.anchorObj = null;
	}
	
	
	public override void RenderUpdate () {
		
		// Calc the mouse posiiton on world space
		Vector3 screenCentre = new Vector3(Screen.width * 0.75f, Screen.height * 0.5f, 0);
		Vector3 inputScreenPos = Vector3.Lerp(screenCentre, Input.mousePosition, AVOWConfig.singleton.cubeToCursor.GetValue());
		
		Vector3 mousePos = inputScreenPos;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		
		// Get the mouse buttons
		HandleMouseButtonInput();
		
		
		// Set the cursor cubes position
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;
		
		VizUpdate();
	}
		
	
	public override void GameUpdate () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		HandleMouseButtonInput();
		StateUpdate();
		CommandsUpdate();
		ResetButtonFlags();
		TestIfValuesHaveChanged();
		
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

		//	Debug.Log("Mouse world pos = " + mouseWorldPos.ToString());
		
		
		// If we don't have a held connection, then we find the closest node and that's all
		if (!heldConnection){
			GameObject closestObj = null;
			Vector3 closestPos = Vector3.zero;
			FindClosestComponentCentre(mouseWorldPos, connectionGO, maxLighteningDistLarge, ref closestObj, ref closestPos);
			if (closestObj != null && closestObj.GetComponent<AVOWComponent>().IsDying()){
				connectionGO = null;
			}
			else{
				connectionGO = closestObj;
				connectionPos = closestPos;
			}
			
			if (IsButtonDown() && connectionGO != null){
				heldConnection = true;
			}
		}
		else{
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			
			connectionPos = 0.5f * (component.GetConnectionPos0() + component.GetConnectionPos1());
			connectionPos.z = uiZPos;
			// If not inside the held gap, find the next closest thing - favouring whatever we have connected already
			if (IsButtonReleased()){
				heldConnection = false;
				heldGapCommand.ExecuteStep();
				AVOWUI.singleton.commands.Push(heldGapCommand);
				heldGapCommand = null;
				connectionGO = null;
				insideState = InsideGapState.kOnRemove;
			}

		}
		
		AVOWCamControl.singleton.mode = IsButtonDown() ? AVOWCamControl.singleton.mode = AVOWCamControl.Mode.kFixVector : AVOWCamControl.singleton.mode = AVOWCamControl.Mode.kFrameGame;
		
		
		// If we have a gap which we are holding open, check if our mous is inside the gap
		isOutside = true;
		if (connectionGO != null){
			AVOWComponent component = connectionGO.GetComponent<AVOWComponent>();
			isOutside = !component.IsPointInsideGap(mouseWorldPos);
		}
		isOutside = false;
		if ((connectionPos - mouseWorldPos).magnitude >  maxLighteningDistLarge){
			heldConnection = false;
			insideState = InsideGapState.kOnCancel;
		}
		
	}
	
	AVOWComponent GetHeldCommandComponent(){
		if (heldGapCommand == null) return null;
		
		GameObject go = heldGapCommand.GetNewComponent();
		if (go == null) return null;
		
		AVOWComponent component = go.GetComponent<AVOWComponent>();
		return component;
	}
	
	
	
	
	
	void HandleCubeInsideGap(){
	
		if (!heldConnection){
			heldTime = Time.fixedTime ;
			
		}
		
		if (wasOutside != isOutside && heldConnection && Time.fixedTime > heldTime + 0.2f){
			AVOWUI.singleton.PlaySpin();
			wasOutside = isOutside;
		}
		
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
				
				// No components should be non interactive in this case
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					component.isInteractive = true;
				}
				
				GameObject.Destroy(insideCube);
				insideCube = null;
				break;
			}
		}	
		
	}
	
	
	void VizUpdate(){
		if (heldConnection || insideState == InsideGapState.kOnCancel || insideState == InsideGapState.kOnRemove) HandleCubeInsideGap();
		
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
		
		if (connectionGO != null){
			if (heldConnection){
				AVOWUI.singleton.SetElectricAudioVolume(1f);
			}
			else{
				AVOWUI.singleton.SetElectricAudioVolume(0.5f);
			}
			
		}
		else{
			AVOWUI.singleton.SetElectricAudioVolume(0);
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
//			Debug.Log("new AVOWCommandRemove " + Time.time);
			
			heldGapCommand.ExecuteStep();
			
			// Ned to force the sim to do an update (this would be better if all this logic was in a fixed update and it 
			// was more tightly controlled
			AVOWSim.singleton.GameUpdate();
		}
		
	}
	
	
}