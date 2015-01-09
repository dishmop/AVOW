using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWComponent : MonoBehaviour {

	// Intrinsic data
	public AVOWGraph.Node node0;
	public AVOWGraph.Node node1;
	public SpringValue resistanceAngle = new SpringValue(45);
	
	public float voltage;
	public enum Type{
		kVoltageSource,
		kLoad
	}
	public Type type;

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
	public float h0;
	public float h1;
	public Color col0;
	public Color col1;
	
	// UI layout
	public float tabSize;
	public float border;
	public int hOrder;	// components are sorted by this value when placed from left to right in diagram
	public bool isLayedOut;
	
	// Debug data
	static int staticCount = 0;
	int id;
	
	// Killing
	bool removeOnTarget = false;
	

	
	public AVOWGraph.Node GetOtherNode(AVOWGraph.Node node){
		if (node == node0) return node1;
		if (node == node1) return node0;
		return null;
	}
	
	public float GetResistance(){
		if (type != Type.kLoad){
			Debug.LogError ("Attempting to read resistance from a non-Load type");
		}
		return Mathf.Tan (Mathf.Deg2Rad * resistanceAngle.GetValue());
	}
	
	// Return the voltage from fromNode to the other node
	public float GetVoltage(AVOWGraph.Node fromNode){
		if (type != Type.kVoltageSource){
			Debug.LogError ("Attempting to read voltage from a non-VoltageSource type");
		}
		// Ensure we are being asked about a node we are connected to
		if (fromNode != node0 && fromNode != node1){
			Debug.LogError("Being asked about a voltgsge frop accross a node we are not connected to");
		}
		// Need to ensure we are returning the correct sign
		return (fromNode == node0) ? voltage : -voltage;
		
	}
	
	public void Kill(float targetRes){
		resistanceAngle.Set (targetRes);
		removeOnTarget = true;
	}
	
	void CheckForKillResistance(){
		if (!removeOnTarget) return;
		

		
		if (resistanceAngle.IsAtTarget()){
			AVOWGraph.singleton.RemoveComponent(gameObject);
		}
	}
	
	public bool IsBetweenNodes(AVOWGraph.Node nodeA, AVOWGraph.Node nodeB){
		if (node0 == nodeA && node1 == nodeB) return true;
		if (node0 == nodeB && node1 == nodeA) return true;
		return false;
	}
	
	public void ReplaceNode(AVOWGraph.Node existingNode, AVOWGraph.Node newNode){
		if (node0 == existingNode){
			SetNode0(newNode);
		} 
		else if (node1 == existingNode){
			SetNode1(newNode);
		}
		else{
			Debug.LogError ("Error replacing node");
		}
	}
	
	public float GetCurrent(AVOWGraph.Node fromNode){
		if (fromNode != node0 && fromNode != node1){
			Debug.LogError("Being asked about a voltage drop accross a node we are not connected to");
		}
		
		return fwCurrent * (IsForward(fromNode) ? 1 : -1);
	}	
	
	public void ClearLoopRecords(){
		loopRecords = new List<LoopRecord>();
	}
	
	public void AddLoopRecord(int loopId, AVOWGraph.Node fromNode){
		// Ensure we are being asked about a node we are connected to
		if (fromNode != node0 && fromNode != node1){
			Debug.LogError("Being asked about a voltage drop accross a node we are not connected to");
		}
		
		LoopRecord thisRecord = new LoopRecord();
		thisRecord.loopId = loopId;
		thisRecord.isForward = IsForward(fromNode);
		loopRecords.Add (thisRecord);
	}
	
	// Return true if we are traversing this component forwards (from node0 to node1) given that we
	// are coming from fromNode
	public bool IsForward(AVOWGraph.Node fromNode){
		return (fromNode == node0);
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
	
	public void SetNode0(AVOWGraph.Node node){
		node0 = node;
		AVOWTab tab = transform.FindChild("LowerTab").GetComponent<AVOWTab>();
		tab.SetNode(node);
		tab.SetAVOWComponent(this);
	}

	public void SetNode1(AVOWGraph.Node node){
		node1 = node;
		AVOWTab tab = transform.FindChild("UpperTab").GetComponent<AVOWTab>();
		tab.SetNode(node);
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
	
	}
	

	// Update is called once per frame
	void Update () {
		resistanceAngle.Update();
		CheckForKillResistance();
		
		float v0 = node0.voltage;
		float v1 = node1.voltage;
	
		
		border = 0;//0.2f * (h1-h0);
		tabSize = 0.2f * (v1-v0);

		if (type == Type.kLoad){
			transform.FindChild("Resistance").renderer.material.SetColor("_Color0", col0);
			transform.FindChild("Resistance").renderer.material.SetColor("_Color1", col1);
			transform.FindChild("Resistance").position = new Vector3(h0, v0, 0);
			transform.FindChild("Resistance").localScale = new Vector3(h1 - h0, v1-v0, 1);
			transform.FindChild("UpperTab").position = new Vector3(h1 - border, v1, -2);
			transform.FindChild("UpperTab").localScale = new Vector3((h1 - h0) - 2 * border, tabSize, 1);
			transform.FindChild("LowerTab").position = new Vector3(h0 + border, v0, -2);
			transform.FindChild("LowerTab").localScale = new Vector3((h1 - h0)  - 2 * border, tabSize, 1);	
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
}
