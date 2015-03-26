using UnityEngine;
using System;

public class AVOWCommandRemove : AVOWCommand{

	public GameObject 	inNodeGO;
	public GameObject 	outNodeGO;
	public GameObject 	removeComponentGO;
	public Vector3		cursorPos;
	
	
	enum GapType{
		kUndetermined,
		kOneOfMany,
		kOnlyOne
	}
	GapType gapType = GapType.kUndetermined;
	
	enum UndoStepState{
		kReplaceComponent,
		kWidenGap,
		kFinished
	}
	
	enum ExecuteStepState{
		kMakeGap,
		kRemoveComponent,
		kMergeNodes,
		kFinished
	}
	
	UndoStepState undoStep = UndoStepState.kFinished;
	ExecuteStepState executeStep = ExecuteStepState.kMakeGap;
	
	
	public AVOWCommandRemove(GameObject componentGO, Vector3 pos){
		removeComponentGO = componentGO;
		cursorPos = pos;
		gapType = DetermineType();
	//	Debug.Log("Removal type = " + gapType.ToString());
		Debug.Log("Removal type = " + (gapType == GapType.kOneOfMany ? "Thinny" : "Fatty"));
	}
	
	public bool IsFinished(){	
		return executeStep == ExecuteStepState.kFinished;
	}

	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kMakeGap:{
				AVOWComponent component = removeComponentGO.GetComponent<AVOWComponent>();
				
				Debug.Log ("Shriking component " + component.GetID() + " of type " + ((gapType == GapType.kOneOfMany) ? "One of many" : "Only one"));
				
				component.resistanceAngle.Set((gapType == GapType.kOnlyOne) ? 35 : 55);
				undoStep = UndoStepState.kWidenGap;
				executeStep = ExecuteStepState.kRemoveComponent;
				break;
			}
			case ExecuteStepState.kRemoveComponent:{
		
				AVOWComponent component = removeComponentGO.GetComponent<AVOWComponent>();
				
				inNodeGO = component.inNodeGO;
				outNodeGO = component.outNodeGO;
				if (gapType == GapType.kOnlyOne){
					component.Kill (1);
					executeStep = ExecuteStepState.kMergeNodes;
					component.onDeadCommand = this;
					component.onDeadCommandDoExecutate = true;
				}
				else{
					component.Kill (89);
					executeStep = ExecuteStepState.kRemoveComponent;
				}
				undoStep = UndoStepState.kReplaceComponent;
				
				break;
			}
			// We never actualy tun this code - which is a bug (so we run it in undo instead)
			case ExecuteStepState.kMergeNodes:{
				AVOWGraph.singleton.MergeNodes(inNodeGO, outNodeGO);
				executeStep = ExecuteStepState.kRemoveComponent;
				break;
			}
			
			
		}
		return false;
		
		
	}
	
	GapType DetermineType(){
		AVOWComponent component = removeComponentGO.GetComponent<AVOWComponent>();
		AVOWNode outNode = component.outNodeGO.GetComponent<AVOWNode>();
		AVOWNode inNode = component.inNodeGO.GetComponent<AVOWNode>();
		
		// If the nodes we are examining are on their way out, we need to consider the nodes they will eventually get attached to instead
		int countOut = 0;
		int countIn = 0;
		
		bool needToCheckOutNode = true;
		while (needToCheckOutNode){
			// test if there are any other components beteween these two nodes
			foreach (GameObject go in outNode.outComponents){
				if (go == null) continue;
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				if (!thisComponent.IsDying() && thisComponent.type == AVOWComponent.Type.kLoad  && removeComponentGO != go) countOut++;
			}
			foreach (GameObject go in inNode.inComponents){
				if (go == null) continue;
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				if (!thisComponent.IsDying() && thisComponent.type == AVOWComponent.Type.kLoad  && removeComponentGO != go) countIn++;
			}
			
			int countInOut = 0;
			AVOWNode newNode = null;
			foreach (GameObject go in outNode.inComponents){
				if (go == null) continue;
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				if (!thisComponent.IsDying()){
					countInOut++;
				}
				else{
					newNode = thisComponent.outNodeGO.GetComponent<AVOWNode>();
				}
			}
			if (countInOut == 0){
				if (newNode == null) Debug.Log ("Error(newNode == null) ");
				outNode = newNode;
			}else{
				needToCheckOutNode = false;
			}
		}
		
		bool needToCheckInNode = true;
		while (needToCheckInNode){
			int countOutIn = 0;
			AVOWNode newNode = null;
			foreach (GameObject go in inNode.outComponents){
				// Hmm this seems to cause a problem where when it gets hit we are stuck in an infinite loop - not sure how it happens though
				if (go == null) continue;
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				if (!thisComponent.IsDying()){
					countOutIn++;
				}
				else{
					newNode = thisComponent.outNodeGO.GetComponent<AVOWNode>();
				}
			}
			if (countOutIn == 0){
				if (newNode == null) Debug.LogError ("(newNode == null) ");
				inNode = newNode;
			}else{
				needToCheckInNode = false;
			}
		}
		
		



		int totalCount = 0;
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			if (go == null) continue;
			AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
			if (!thisComponent.IsDying()) totalCount++;
		}
		// if there is more than just this node beteeen them
		if ((countOut > 0 && countIn > 0) || totalCount == 2){
			
			return GapType.kOneOfMany;
		}
		else{
			return GapType.kOnlyOne;
			
		}
	}

	public bool UndoStep(){
		switch (undoStep){
			case UndoStepState.kWidenGap:{
				removeComponentGO.GetComponent<AVOWComponent>().resistanceAngle.Set (45);
				Debug.Log ("UndoStep");
				break;
			}
		
		}
	
	
		return false;
	
	}
	
	//not strctly the new one, but hey :)
	public GameObject GetNewComponent(){
		return removeComponentGO;
	}
	
	public GameObject GetNewNode(){
		return null;
	}
	
	
}
