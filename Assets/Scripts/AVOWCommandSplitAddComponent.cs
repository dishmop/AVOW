using UnityEngine;
using System;

public class AVOWCommandSplitAddComponent : AVOWCommand{

	public GameObject nodeGO;
	public GameObject movedComponent;
	public GameObject prefab;
	GameObject newNodeGO;
	GameObject newComponentGO;
	bool isSplittingAtCell;
	
	enum UndoStepState{
		kRemoveComponent,
		kMergeNode
	}
	
	enum ExecuteStepState{
		kMakeGap,
		kFixComponent,
		kFinished
	}
	
	UndoStepState undoStep = UndoStepState.kRemoveComponent;
	ExecuteStepState executeStep = ExecuteStepState.kMakeGap;
	
	public static int debugCount = 0;
	
	public AVOWCommandSplitAddComponent(GameObject splitNodeGO, GameObject component, GameObject prefabToUse, bool isSplittingCell){
		nodeGO = splitNodeGO;
		movedComponent = component;
		prefab = prefabToUse;
		isSplittingAtCell = isSplittingCell;

//		Debug.Log (debugCount + " Construct: AVOWCommandSplitAddComponent: splitNodeGO = " + splitNodeGO.GetComponent<AVOWNode>().GetID() + ", component = " + component.GetComponent<AVOWComponent>().GetID());

		debugCount++;
		
	}
	
	public bool IsFinished(){	
		return executeStep == ExecuteStepState.kFinished;
	}

	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kMakeGap:{
				newNodeGO = AVOWGraph.singleton.SplitNode(nodeGO, movedComponent.GetComponent<AVOWComponent>());
				newNodeGO.GetComponent<AVOWNode>().isInteractive = false;
				
				newComponentGO = GameObject.Instantiate(prefab) as GameObject;
	

				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.resistanceAngle.Force(0);
				newComponent.resistanceAngle.Set(isSplittingAtCell ? 8 : 25);
				newComponent.isInteractive = false;
				
				AVOWGraph.singleton.PlaceComponent(newComponentGO, newNodeGO, nodeGO);
				AVOWSim.singleton.Recalc();
				executeStep = ExecuteStepState.kFixComponent;
				return false;
			}
			case ExecuteStepState.kFixComponent:{
				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.isInteractive = true;
				newComponent.resistanceAngle.Set(45);
				
				newNodeGO.GetComponent<AVOWNode>().isInteractive = true;
			
			
				
				executeStep = ExecuteStepState.kFinished;
			
			return true;
			}
		}
		return false;
		
		
	}

	public bool UndoStep(){
		AVOWComponent component = newComponentGO.GetComponent<AVOWComponent>();
		switch (undoStep){
			case UndoStepState.kRemoveComponent:{
				component.Kill (0);
				component.onDeadCommand = this;
				undoStep = UndoStepState.kMergeNode;
				return false;
			}
			case UndoStepState.kMergeNode:{
				AVOWGraph.singleton.MergeNodes(component.node1GO, component.node0GO);
				return true;
			}
		};
		return false;
	
	}
	
	public GameObject GetNewComponent(){
		return newComponentGO;
	}
	
	public GameObject GetNewNode(){
		return newNodeGO;
	}
	
	
}
