using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWComponent : MonoBehaviour {

	// Intrinsic data
	public GameObject node0GO;
	public GameObject node1GO;
	public SpringValue resistanceAngle = new SpringValue(45);
	public float connectorProp = 0.1f;
	public float squareGap = 0.02f;
	public float lighteningSize = 0.5f;
	
	
	Vector3 oldNode0Pos = new Vector3(0, 0, 0);
	Vector3 oldNode1Pos = new Vector3(0, 0, 0);
	
	
	
	
	
			
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
	public bool disable;
	public float fwCurrent;
	
	// Visulation data
	public Color col0;
	public Color col1;
	public float tabSize;
	public float border;
	
	// Layout
	public int hOrder;				// components are sorted by this value when placed from left to right in diagram
	public float hWidth;			// Always known by the time we get to layout
	public float h0;				// Set to -1 if unknown
	public float h0LowerBound;		// Set to -1 if unknown
	public float h0UpperBound;		// Set to -1 if unknown
	public static int kOrdinalUnordered = 9999;
	public int inNodeOrdinal;		// the nth component on node0 flowing in this direction (=kOrdinaUnordered if unknown)
	public int outNodeOrdinal;		// the nth component on node1 flowing in this direction (=kOrdinaUnordered if unknown)
	public float inLocalH0;			// The starting position measured from the inNodes h0 (which we may not know)
	public float outLocalH0;		// The starting position measured from the outNodes h0 (which we may not know)
	public GameObject inNodeGO;	// references to node0 and node1 depdending on how the current is flowing
	public GameObject outNodeGO;	// references to node0 and node1 depdending on how the current is flowing
	public bool hasBeenLayedOut;	// false at first and then true after we have been layed out at least once. 
	
	// Debug data
	static int staticCount = 0;
	int id;
	
	// Killing
	bool removeOnTarget = false;
	public AVOWCommand onDeadCommand = null;
	

	
	public GameObject GetOtherNode(GameObject node){
		if (node == node0GO) return node1GO;
		if (node == node1GO) return node0GO;
		return null;
	}
	
	public float GetResistance(){
		if (type != Type.kLoad){
			Debug.LogError ("Attempting to read resistance from a non-Load type");
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
		AVOWUI.singleton.lockCreation = true;
	}
	
	void CheckForKillResistance(){
		if (!removeOnTarget) return;
		
		if (resistanceAngle.IsAtTarget()){
			if (onDeadCommand != null){
				onDeadCommand.UndoStep();
			}
			AVOWGraph.singleton.RemoveComponent(gameObject);
			// DEBUG
			AVOWUI.singleton.lockCreation = false;
			
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
		if (node0GO == existingNodeGO){
			SetNode0(newNodeGO);
		} 
		else if (node1GO == existingNodeGO){
			SetNode1(newNodeGO);
		}
		else{
			Debug.LogError ("Error replacing node");
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
		AVOWTab tab = transform.FindChild("LowerTab").GetComponent<AVOWTab>();
		tab.SetNode(nodeGO.GetComponent<AVOWNode>());
		tab.SetAVOWComponent(this);
	}

	public void SetNode1(GameObject nodeGO){
		node1GO = nodeGO;
		AVOWTab tab = transform.FindChild("UpperTab").GetComponent<AVOWTab>();
		tab.SetNode(nodeGO.GetComponent<AVOWNode>());
		tab.SetAVOWComponent(this);
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
	
		gameObject.SetActive(false);
	}
	
	
	public void HasBeenLayedOut(){
		hasBeenLayedOut = true;
		gameObject.SetActive(true);
	}
	

	// Update is called once per frame
	void Update () {
		
		resistanceAngle.Update();
		CheckForKillResistance();
		
		float v0 = node0GO.GetComponent<AVOWNode>().voltage;
		float v1 = node1GO.GetComponent<AVOWNode>().voltage;
		
		
		border = 0;//0.2f * (h1-h0);
		tabSize = 0.2f * (v1-v0);
		
		float h1 = h0 + hWidth;

		// Ensure the shape is always square
		float midX = (h0 + h1)/2.0f;
		float midY = (v0 + v1)/2.0f;
		float halfSize = 0.5f * Mathf.Min (Mathf.Abs (v1-v0), Mathf.Abs (h1-h0));
		
		float useH0 = midX - halfSize;
		float useH1 = midX + halfSize;
		float useV0 = midY - halfSize;
		float useV1 = midY + halfSize;
		
		
		// NOt sure why I need to do this but....
		if (v0 > v1){
			float temp = useV0;
			useV0 = useV1;
			useV1 = temp;
		}
	

		if (type == Type.kLoad){
			SetupUVs (transform.FindChild("Resistance").gameObject, Mathf.Abs (useV1-useV0));
			transform.FindChild("Resistance").renderer.material.SetColor("_Color0", col0);
			transform.FindChild("Resistance").renderer.material.SetColor("_Color1", col1);
			transform.FindChild("Resistance").position = new Vector3(useH0  + (useH1 -useH0) * squareGap, Mathf.Min (useV0, useV1) + Mathf.Abs (useV1-useV0) * squareGap, 0);
			transform.FindChild("Resistance").localScale = new Vector3((1-2 * squareGap) * (useH1 -useH0), (1-2 * squareGap) * Mathf.Abs (useV1-useV0), 1);
			transform.FindChild("UpperTab").position = new Vector3(useH1 - border, useV1, -2);
			transform.FindChild("UpperTab").localScale = new Vector3((useH1 - useH0) - 2 * border, tabSize, 1);
			transform.FindChild("LowerTab").position = new Vector3(useH0 + border, useV0, -2);
			transform.FindChild("LowerTab").localScale = new Vector3((useH1 - useH0)  - 2 * border, tabSize, 1);
			Vector3 newNode0Pos = node0GO.transform.FindChild("Sphere").transform.position;
			Vector3 newNode1Pos = node1GO.transform.FindChild("Sphere").transform.position;
			
			Vector3 top = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f - 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
			Vector3 bottom = new Vector3((useH1 + useH0) * 0.5f, (useV0 + useV1) * 0.5f + 0.5f * Mathf.Min (useH1 - useH0, useV1 - useV0));
			
			Vector3 connector0Pos = top + connectorProp * (bottom - top);
			Vector3 connector1Pos = bottom + connectorProp * (top - bottom);
			
			if (!MathUtils.FP.Feq ((oldNode0Pos - newNode0Pos).sqrMagnitude, 0) || !MathUtils.FP.Feq ((oldNode1Pos - newNode1Pos).sqrMagnitude, 0)) {
			
				Lightening lightening0 = transform.FindChild("Lightening0").GetComponent<Lightening>();
				Lightening lightening1 = transform.FindChild("Lightening1").GetComponent<Lightening>();
				Lightening lightening2 = transform.FindChild("Lightening2").GetComponent<Lightening>();
				

				// Node0 to connector 0
				transform.FindChild("Lightening0").gameObject.SetActive(true);
				lightening0.startPoint = newNode0Pos;
				lightening0.endPoint = connector0Pos;
				lightening0.size = lighteningSize * Mathf.Abs (useV1-useV0);
				lightening0.ConstructMesh();

				// connector0 to connector1
				transform.FindChild("Lightening1").gameObject.SetActive(true);
				lightening1.startPoint = connector0Pos;
				lightening1.endPoint = connector1Pos;
				lightening1.size =lighteningSize *  Mathf.Abs (useV1-useV0);
				lightening1.ConstructMesh();	
								
				// Connector1 to node1
				transform.FindChild("Lightening2").gameObject.SetActive(true);
				lightening2.startPoint = connector1Pos;
				lightening2.endPoint = newNode1Pos;
				lightening2.size = lighteningSize * Mathf.Abs (useV1-useV0);
				lightening2.ConstructMesh();	
				
				// Put our connection spheres in the right place.
				float scale = 0.1f * Mathf.Abs (useV1-useV0);
				Transform connectionSphere0 = transform.FindChild("ConnectionSphere0");
				Transform connectionSphere1 = transform.FindChild("ConnectionSphere1");
				connectionSphere0.position = connector0Pos;
				connectionSphere0.localScale = new Vector3(scale, scale, scale);
				connectionSphere1.position = connector1Pos;
				connectionSphere1.localScale = new Vector3(scale, scale, scale);
				
				
					
											
				oldNode0Pos = newNode0Pos;
				oldNode1Pos = newNode1Pos;
			}


		}
		else{
			transform.FindChild("VoltageSource").renderer.material.SetColor("_Color0", col0);
			transform.FindChild("VoltageSource").renderer.material.SetColor("_Color1", col1);
			transform.FindChild("VoltageSource").position = new Vector3(-h0, v0, 0);
			transform.FindChild("VoltageSource").localScale = new Vector3(h0 - h1, v1-v0, 1);

			transform.FindChild("UpperTab").position = new Vector3(h1 - border, v1, -2);
			transform.FindChild("UpperTab").localScale = new Vector3(2 * (h1 - h0) - 2 * border, -tabSize, 1);
			transform.FindChild("LowerTab").position = new Vector3(-h1 + border, v0, -2);
			transform.FindChild("LowerTab").localScale = new Vector3(2 * (h1 - h0)  - 2 * border, -tabSize, 1);	
		}
			
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

		go.renderer.material.SetFloat("_NormalRotationCos", Mathf.Cos(radRot - piBy8));
		go.renderer.material.SetFloat("_NormalRotationSin", Mathf.Sin(radRot - piBy8));
		
	
	}
}
