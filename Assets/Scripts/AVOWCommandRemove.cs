using UnityEngine;
using System;

public class AVOWCommandRemove : AVOWCommand{

	public GameObject 	nearNodeGO;
	public GameObject 	farNodeGO;
	public GameObject 	removeComponentGO;
	public Vector3		cursorPos;
	
	enum UndoStepState{
		kReplaceComponent,
		kFinished
	}
	
	enum ExecuteStepState{
		kRemoveComponent,
		kMergeNodes,
		kFinished
	}
	
	UndoStepState undoStep = UndoStepState.kReplaceComponent;
	ExecuteStepState executeStep = ExecuteStepState.kRemoveComponent;
	
	
	public AVOWCommandRemove(GameObject componentGO, Vector3 pos){
		removeComponentGO = componentGO;
		cursorPos = pos;
	}
	
	public bool IsFinished(){	
		return executeStep == ExecuteStepState.kFinished;
	}

	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kRemoveComponent:{
		
				AVOWComponent component = removeComponentGO.GetComponent<AVOWComponent>();
				
				// Find the nodes at either end of this component
				Vector3 connection0Pos = component.GetConnectionPos0();
				Vector3 connection1Pos = component.GetConnectionPos1();
				
				float dist0 = (cursorPos - connection0Pos).magnitude;
				float dist1 = (cursorPos - connection1Pos).magnitude;
				
				if (dist0 < dist1){
					nearNodeGO = component.node0GO;
					farNodeGO = component.node1GO;
				}
				else{
					nearNodeGO = component.node1GO;
					farNodeGO = component.node0GO;
				}
				
				Debug.Log ("NearNode = " + nearNodeGO.GetComponent<AVOWNode>().GetID());
			     
			    Debug.Log ("FarNode = " + farNodeGO.GetComponent<AVOWNode>().GetID());
			           
				// Remove the component
				AVOWNode nearNode = nearNodeGO.GetComponent<AVOWNode>();
				AVOWNode farNode = farNodeGO.GetComponent<AVOWNode>();
				
				bool nearIsHeigh = (nearNode.voltage > farNode.voltage);
				
				AVOWNode hiNode = (nearIsHeigh) ? nearNode : farNode;
				AVOWNode loNode = (nearIsHeigh) ? farNode : nearNode;
				
				// test if there are any other components beteween these two nodes
				int count = 0;
				foreach (GameObject go in hiNode.outComponents){
					AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
					if (thisComponent.GetOtherNode(hiNode.gameObject) == loNode.gameObject){
						count++;
					}
				
				}
				component.isInteractive = false;
				// if there is more than just this node beteeen them
				if (count > 1){
					component.Kill (89);
					executeStep = ExecuteStepState.kFinished;
					return true;
				}
				else{
					component.Kill (0);
					component.onDeadCommand = this;
					executeStep = ExecuteStepState.kMergeNodes;
					return false;
				
				}
	

			}
			// We never actualy tun this code - which is a bug (so we run it in undo instead)
			case ExecuteStepState.kMergeNodes:{
				AVOWGraph.singleton.MergeNodes(farNodeGO, nearNodeGO);
				break;
			}
			
			
		}
		return false;
		
		
	}

	public bool UndoStep(){
		// hack
		AVOWGraph.singleton.MergeNodes(farNodeGO, nearNodeGO);
		return false;
	
	}
	
	public GameObject GetNewComponent(){
		return null;
	}
	
	public GameObject GetNewNode(){
		return null;
	}
	
	
}
