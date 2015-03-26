﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWComponent : MonoBehaviour {

	// Intrinsic data
	public GameObject node0GO;
	public GameObject node1GO;
	public SpringValue resistanceAngle = new SpringValue(45, SpringValue.Mode.kLinear, 40);
	public float connectorProp = 0.1f;
	public float squareGap = 0.02f;
	public float lighteningSize = 0.5f;
	public bool isInteractive = true;	
	public bool showResistance = true;
	// top bottom, left right 
	float useH0;
	float useH1;
	float useV0;
	float useV1;
	
	bool enableLightening0;
	bool enableLightening1;
	bool enableLightening2;
	
	float lighteningZDepth;
	
	// Used to keep a track of where our last stable (confirmed) connecton positions were
	Vector3 confirmedPos0 = Vector3.zero;
	Vector3 confirmedPos1 = Vector3.zero;
	
	
//	Vector3 oldNode0Pos = new Vector3(0, 0, 0);
//	Vector3 oldNode1Pos = new Vector3(0, 0, 0);
//	
			
	public float voltage;
	public enum Type{
		kVoltageSource,
		kLoad
	}
	public Type type;
	
	public enum FlowDirection{
		kIn,
		kOut
	}

	// Simulation data
	public class LoopRecord{
		// index of the loop traversing this component
		public int loopId;
		// True if this loops traverses from node0 to node1
		public bool isForward;
	}
	public List<LoopRecord> loopRecords;
	public bool visited;
	public int uiVisitedIndex;	// used by the UI to traverse
	public bool disable;
	public float fwCurrent;
	
	// Visulation data
	public Color col0;
	public Color col1;
	public float border;
	
	// Layout
	public float hOrder;				// components are sorted by this value when placed from left to right in diagram
	public float hWidth;			// Always known by the time we get to layout
	public float h0;				// Set to -1 if unknown
	public float h0LowerBound;		// Set to -1 if unknown
	public float h0UpperBound;		// Set to -1 if unknown
	public static int kOrdinalUnordered = 9999;
	public int inNodeOrdinal;		// the nth component on node0 flowing in this direction (=kOrdinaUnordered if unknown)
	public int outNodeOrdinal;		// the nth component on node1 flowing in this direction (=kOrdinaUnordered if unknown)
	public float inLocalH0;			// The starting position measured from the inNodes h0 (which we may not know)
	public float outLocalH0;		// The starting position measured from the outNodes h0 (which we may not know)
	public GameObject inNodeGO;		// references to node0 and node1. Current flows in to this node from the componet
	public GameObject outNodeGO;	// references to node0 and node1. Current flows out of this node into the compoent
	public bool hasBeenLayedOut;	// false at first and then true after we have been layed out at least once. 
	
	// Debug data
	static int staticCount = 0;
	int id;
	
	// Killing
	bool removeOnTarget = false;
	public AVOWCommand onDeadCommand = null;
	public bool onDeadCommandDoExecutate;	// whether to rint he wexecute or undo commands
	

	
	public GameObject GetOtherNode(GameObject node){
		if (node == node0GO) return node1GO;
		if (node == node1GO) return node0GO;
		return null;
	}
	
	public bool IsDying(){
		return removeOnTarget;
	}
	
	void Start(){

		lighteningZDepth = transform.position.z - 0.01f;
		transform.FindChild("Lightening0").gameObject.SetActive(false);
		transform.FindChild("Lightening1").gameObject.SetActive(false);
	}
	
	public float GetResistance(){
		if (type != Type.kLoad){
			Debug.Log("Attempting to read resistance from a non-Load type");
		}
		return Mathf.Tan (Mathf.Deg2Rad * resistanceAngle.GetValue());
	}
	
	// Return the voltage from fromNode to the other node
	public float GetVoltage(AVOWNode fromNode){
		if (type != Type.kVoltageSource){
			Debug.LogError ("Attempting to read voltage from a non-VoltageSource type");
		}
		// Ensure we are being asked about a node we are connected to
		if (fromNode.gameObject != node0GO && fromNode.gameObject != node1GO){
			Debug.LogError("Being asked about a voltgsge frop accross a node we are not connected to");
		}
		// Need to ensure we are returning the correct sign
		return (fromNode.gameObject == node0GO) ? voltage : -voltage;
		
	}
	
	public void Kill(float targetRes){
		resistanceAngle.Set (targetRes);
		removeOnTarget = true;
	}
	
	public void CheckForKillResistance(){
		if (!removeOnTarget) return;
		
		if (resistanceAngle.IsAtTarget()){
			if (onDeadCommand != null){
				if (onDeadCommandDoExecutate){
					onDeadCommand.ExecuteStep();
				}
				else{
					onDeadCommand.UndoStep();
				}
			}
			AVOWGraph.singleton.RemoveComponent(gameObject);
			// DEBUG
			
		}
	}
	
	public FlowDirection GetDirection(GameObject nodeGO){
		if (GetCurrent(nodeGO) > 0){
			return FlowDirection.kOut;
		}
		else{
			return FlowDirection.kIn;
		}
		
	}
	
	public Vector3 GetConnectionPos0(){
		float hMid = (h0 + 0.5f * hWidth);
		float node0VPos = node0GO.GetComponent<AVOWNode>().voltage;
		float node1VPos = node1GO.GetComponent<AVOWNode>().voltage;
		if (type == Type.kLoad){
			return new Vector3(hMid, node0VPos + connectorProp * (node1VPos - node0VPos), transform.position.z);
		}
		else{
			return new Vector3(hMid, node0VPos - connectorProp * (node1VPos - node0VPos), transform.position.z);
		}
	}
	
	public Vector3 GetConfirmedConnectionPos0(){
		return confirmedPos0;
	}
	
	public Vector3 GetConfirmedConnectionPos1(){
		return confirmedPos1;
	}
		
	public Vector3 GetConnectionPos1(){
		float hMid = (h0 + 0.5f * hWidth);
		float node0VPos = node0GO.GetComponent<AVOWNode>().voltage;
		float node1VPos = node1GO.GetComponent<AVOWNode>().voltage;
		if (type == Type.kLoad){
			return new Vector3(hMid, node1VPos + connectorProp * (node0VPos - node1VPos), transform.position.z);
		}
		else{
			return new Vector3(hMid, node1VPos - connectorProp * (node0VPos - node1VPos), transform.position.z);
		}
	}

	public Vector3 GetConnectionPosBottom(){
		Vector3 top = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f - 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
		Vector3 bottom = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f + 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
		return bottom + connectorProp * (top - bottom);
	}
		
	public Vector3 GetConnectionPosTop(){
		Vector3 top = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f - 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
		Vector3 bottom = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f + 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
		
		return top + connectorProp * (bottom - top);
	}
	
	public void BakeConfirmedConnectionPositions(){
		confirmedPos0 = GetConnectionPos0();
		confirmedPos1 = GetConnectionPos1();
	}
	
	
	public void EnableLightening(GameObject nodeGO, bool enable){
		
		if (nodeGO == node0GO){
			enableLightening0 = enable;
		}
		else if (nodeGO == node1GO){
			enableLightening1 = enable;
		}
		else if (!node0GO.GetComponent<AVOWNode>().isInteractive){
			enableLightening0 = enable;
		}
		else if (!node1GO.GetComponent<AVOWNode>().isInteractive){
			enableLightening1 = enable;
		}
		else{
			Debug.LogError ("Error enable Lighting");
		}
		
	}
	
	public void SetupInOutNodes(){
		if (GetDirection(node0GO) == FlowDirection.kOut){
			outNodeGO = node0GO;
			inNodeGO = node1GO;
		}
		else{
			inNodeGO = node0GO;
			outNodeGO = node1GO;
		}
	}
	
	public bool IsBetweenNodes(AVOWNode nodeA, AVOWNode nodeB){
		if (node0GO == nodeA.gameObject && node1GO == nodeB.gameObject) return true;
		if (node0GO == nodeB.gameObject && node1GO == nodeA.gameObject) return true;
		return false;
	}
	
	public void ReplaceNode(GameObject existingNodeGO, GameObject newNodeGO){
//		Debug.Log ("Request replace node :" + existingNodeGO.GetComponent<AVOWNode>().GetID() + " with node " + newNodeGO.GetComponent<AVOWNode>().GetID());
		if (node0GO == existingNodeGO){
			SetNode0(newNodeGO);
		} 
		else if (node1GO == existingNodeGO){
			SetNode1(newNodeGO);
		}
		else{
//			AVOWNode existingNode = existingNodeGO.GetComponent<AVOWNode>();
//			AVOWNode newNode = newNodeGO.GetComponent<AVOWNode>();
//			AVOWNode node0 = node0GO.GetComponent<AVOWNode>();
//			AVOWNode node1 = node1GO.GetComponent<AVOWNode>();
//			Debug.LogError ("Error replacing node " + existingNodeGO.GetComponent<AVOWNode>().GetID() + " with node " + newNodeGO.GetComponent<AVOWNode>().GetID() + " on component " + GetID());
		}
	}
	
	public float GetCurrent(GameObject fromNodeGO){
		if (fromNodeGO != node0GO && fromNodeGO != node1GO){
			Debug.LogError("Being asked about a voltage drop accross a node we are not connected to");
		}
		
		return fwCurrent * (IsForward(fromNodeGO) ? 1 : -1);
	}	
	
	public void ClearLoopRecords(){
		loopRecords = new List<LoopRecord>();
	}
	
	public void AddLoopRecord(int loopId, GameObject fromNodeGO){
		// Ensure we are being asked about a node we are connected to
		if (fromNodeGO != node0GO && fromNodeGO != node1GO){
			Debug.LogError("Being asked about a voltage drop accross a node we are not connected to");
		}
		
		LoopRecord thisRecord = new LoopRecord();
		thisRecord.loopId = loopId;
		thisRecord.isForward = IsForward(fromNodeGO);
		loopRecords.Add (thisRecord);
	}
	
	// Return true if we are traversing this component forwards (from node0 to node1) given that we
	// are coming from fromNode
	public bool IsForward(GameObject fromNode){
		return (fromNode == node0GO);
	}
	
	public string GetID(){
		string idVal = ((char)(id+65)).ToString ();
		if (type == Type.kLoad){
			return "(" + idVal + ")";
		}
		else{
			return "[" + idVal + "]";
		}
	}
	
	public void SetID(int thisID){
		hOrder = thisID;
	}
	
	public void SetNode0(GameObject nodeGO){
		node0GO = nodeGO;
	}

	public void SetNode1(GameObject nodeGO){
		node1GO = nodeGO;
	}

	
	// Use this for initialization
	void Awake () {
		id = staticCount++;
		
		if (type == Type.kLoad){
			resistanceAngle.Set (45);
			voltage = 0;
		}
		else{
			resistanceAngle.Force (0);
			voltage = 1;
		}
	}
	
	
	public void HasBeenLayedOut(){
		hasBeenLayedOut = true;
		// A bit naughty, but the only way we can get all the things in the right place
		RenderUpdate ();
		
	}
	
	public void GameUpdate (){
		CheckForKillResistance();
	}
	

	// Update is called once per frame
	public void RenderUpdate () {
	
		resistanceAngle.Update();
		
		// NOt sure why this should eveqr happen but...
		if (float.IsNaN(h0)){
			return;
		}
		
		float v0 = node0GO.GetComponent<AVOWNode>().voltage;
		float v1 = node1GO.GetComponent<AVOWNode>().voltage;
		
		
		border = 0;//0.2f * (h1-h0);
		
		float h1 = h0 + hWidth;

		// Ensure the shape is always square
		float midX = (h0 + h1)/2.0f;
		float midY = (v0 + v1)/2.0f;
		float halfSize = 0.5f * Mathf.Min (Mathf.Abs (v1-v0), Mathf.Abs (h1-h0));
		
		useH0 = midX - halfSize;
		useH1 = midX + halfSize;
		useV0 = midY - halfSize;
		useV1 = midY + halfSize;
		
		
		// NOt sure why I need to do this but....
		if (v0 > v1){
			float temp = useV0;
			useV0 = useV1;
			useV1 = temp;
		}
	
		Vector3 connector0Pos = GetConnectionPos0();
		Vector3 connector1Pos = GetConnectionPos1();
		
		if (type == Type.kLoad){
			transform.FindChild("Resistance").gameObject.SetActive(isInteractive && showResistance);
			transform.FindChild("BlackSquare").gameObject.SetActive(isInteractive && showResistance);
			SetupUVs (transform.FindChild("Resistance").gameObject, Mathf.Abs (useV1-useV0));
			transform.FindChild("Resistance").GetComponent<Renderer>().material.SetColor("_Color0", col0);
			transform.FindChild("Resistance").GetComponent<Renderer>().material.SetColor("_Color1", col1);
			transform.FindChild("Resistance").position = new Vector3(useH0  + squareGap, Mathf.Min (useV0, useV1) + squareGap, 0);
			transform.FindChild("BlackSquare").position =  new Vector3(useH0, Mathf.Min (useV0, useV1), 0.01f);
			
			float xScale = useH1 -useH0 - 2 * squareGap;
			float yScale = Mathf.Abs (useV1-useV0) - 2 * squareGap;
			
			xScale = Mathf.Max (xScale, 0);
			yScale = Mathf.Max (yScale, 0);
			
			transform.FindChild("Resistance").localScale = new Vector3(xScale, yScale, 1);
			transform.FindChild("BlackSquare").localScale = new Vector3(useH1 -useH0, useH1 -useH0, 1);
			
			transform.FindChild("Lightening0").gameObject.SetActive(isInteractive && enableLightening0);
			transform.FindChild("Lightening1").gameObject.SetActive(isInteractive && enableLightening1);
			transform.FindChild("Lightening2").gameObject.SetActive(isInteractive);
			
			transform.FindChild("ConnectionSphere0").gameObject.SetActive(isInteractive);
			transform.FindChild("ConnectionSphere1").gameObject.SetActive(false);
			
		}
		else{
			transform.FindChild("Lightening0").gameObject.SetActive(isInteractive && enableLightening0);
			transform.FindChild("Lightening1").gameObject.SetActive(isInteractive && enableLightening1);
			transform.FindChild("Lightening2").gameObject.SetActive(false);
			
			transform.FindChild("ConnectionSphere0").gameObject.SetActive(isInteractive);
			transform.FindChild("ConnectionSphere1").gameObject.SetActive(isInteractive);
			
		}

		
		Vector3 newNode0Pos = node0GO.transform.position;
		Vector3 newNode1Pos = node1GO.transform.position;
		
		// Debug text
		//transform.FindChild("Resistance").FindChild ("AVOWTextBox").gameObject.SetActive(false);
		//transform.FindChild("Resistance").FindChild ("AVOWTextBox").GetComponent<TextMesh>().text = GetID() + " - " + hOrder.ToString();
		
		
		
		// Otherwise, it doesn't work when they move
		if (true) {
		
			Lightening lightening0 = transform.FindChild("Lightening0").GetComponent<Lightening>();
			Lightening lightening1 = transform.FindChild("Lightening1").GetComponent<Lightening>();
			Lightening lightening2 = transform.FindChild("Lightening2").GetComponent<Lightening>();
			
			float pdSize = Mathf.Abs (useV1-useV0);

			// Node0 to connector 0
//				transform.FindChild("Lightening0").gameObject.SetActive(true);
			lightening0.startPoint = new Vector3(connector0Pos.x, newNode0Pos.y, lighteningZDepth);
			lightening0.endPoint = connector0Pos + new Vector3(0, 0, lighteningZDepth);
			lightening0.size = lighteningSize * pdSize;
			lightening0.numStages = 2;
			lightening0.ConstructMesh();


							
			// Connector1 to node1
//				transform.FindChild("Lightening1").gameObject.SetActive(true);
			lightening1.startPoint = new Vector3(connector1Pos.x, newNode1Pos.y, lighteningZDepth);
			lightening1.endPoint = connector1Pos + new Vector3(0, 0, lighteningZDepth);
			lightening1.size = lighteningSize * pdSize;
			lightening1.numStages = 2;
			lightening1.ConstructMesh();	
			
			// connector0 to connector1
//				transform.FindChild("Lightening0").gameObject.SetActive(true);
			lightening2.startPoint = connector0Pos + new Vector3(0, 0, lighteningZDepth);;
			lightening2.endPoint = connector1Pos + new Vector3(0, 0, lighteningZDepth);;
			lightening2.size =lighteningSize *  pdSize;
			lightening2.ConstructMesh();					
			
			
//			oldNode0Pos = newNode0Pos;
//			oldNode1Pos = newNode1Pos;
		}
		
		if (float.IsNaN(connector0Pos.x)){
			Debug.Log ("Error NAN");
		}
		// Put our connection spheres in the right place.
		float scale = 0.1f * Mathf.Abs (useV1-useV0);
		Transform connectionSphere0 = transform.FindChild("ConnectionSphere0");
		Transform connectionSphere1 = transform.FindChild("ConnectionSphere1");
		connectionSphere0.position = connector0Pos;
		connectionSphere0.localScale = new Vector3(scale, scale, scale);
		connectionSphere1.position = connector1Pos;
		connectionSphere1.localScale = new Vector3(scale, scale, scale);


		
			
	}
	
	string CreateFracString(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		//val *= AVOWCircuitCreator.singleton.currentLCM;
			MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return (integer * denominator + numerator).ToString() + (MathUtils.FP.Feq (denominator, 1) ? "" : "/" + denominator.ToString() );
	}
	
	public bool IsPointInsideGap(Vector3 pos){
		
		return (pos.x >= Mathf.Min (h0, h0 + hWidth) && 
		        pos.x <= Mathf.Max (h0, h0 + hWidth) && 
		        pos.y >= Mathf.Min (node0GO.GetComponent<AVOWNode>().voltage, node1GO.GetComponent<AVOWNode>().voltage) && 
		        pos.y <= Mathf.Max (node0GO.GetComponent<AVOWNode>().voltage, node1GO.GetComponent<AVOWNode>().voltage));	
	}
	
	
	void SetupUVs(GameObject go, float size){
	
		float originalRadius = Mathf.Sqrt (2) * 0.5f;
		float radius = originalRadius * size;
		
		// These are paremeters we would use to generate the size and angle
		float inStepPerc = 40;
		float sideDec = inStepPerc / 100;
		float op = Mathf.Sqrt ((sideDec - 1) * (sideDec - 1) + 1);
		float radInc = Mathf.Acos((2 - sideDec) / (op * Mathf.Sqrt(2)));
		float sizeInc = op / Mathf.Sqrt (2);
		
		// Given the size we are, we need to figure out what "step" we would have got to had we beed 
		// doing this stuff in a  loop
		float step = Mathf.Log (radius / originalRadius, sizeInc);
		
		// Now we can figure out what angle we need to turn
		float piBy8 = 0.785398163397448f;
		float radRot = piBy8 + radInc * step;
		
		// Figure out the UVs
		float p0x = radius * Mathf.Sin (radRot);
		float p0y = radius * Mathf.Cos (radRot);
		

		
		Vector2[] uvs = new Vector2[4];
		

		
		uvs[0] = new Vector2(0.5f + p0y, 0.5f - p0x);
		uvs[1] = new Vector2(0.5f - p0y, 0.5f + p0x);
		uvs[2] = new Vector2(0.5f + p0x, 0.5f + p0y);
		uvs[3] = new Vector2(0.5f - p0x, 0.5f - p0y);
		
		
		go.GetComponent<MeshFilter>().mesh.uv = uvs;

		go.GetComponent<Renderer>().material.SetFloat("_NormalRotationCos", Mathf.Cos(radRot - piBy8));
		go.GetComponent<Renderer>().material.SetFloat("_NormalRotationSin", Mathf.Sin(radRot - piBy8));
		
	
	}
}
