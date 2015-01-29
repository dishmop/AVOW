using UnityEngine;
using System;

public class AVOWCommandSplitAddComponent : AVOWCommand{

	public GameObject nodeGO;
	public GameObject movedComponent;
	public GameObject prefab;
	public GameObject newNodeGO;
	public GameObject newComponentGO;
	
	enum UndoStepState{
		kRemoveComponent,
		kMergeNode
	}
	
	enum ExecuteStepState{
		kMakeGap,
		kFixComponent
	}
	
	UndoStepState undoStep = UndoStepState.kRemoveComponent;
	ExecuteStepState executeStep = ExecuteStepState.kMakeGap;
	
	
	public AVOWCommandSplitAddComponent(GameObject splitNodeGO, GameObject component, GameObject prefabToUse){
		nodeGO = splitNodeGO;
		movedComponent = component;
		prefab = prefabToUse;
		
	}

	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kMakeGap:{
				newNodeGO = AVOWGraph.singleton.SplitNode(nodeGO, movedComponent.GetComponent<AVOWComponent>());
				
				newComponentGO = GameObject.Instantiate(prefab) as GameObject;
				newComponentGO.SetActive(false);

				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.resistanceAngle.Force(0);
				newComponent.resistanceAngle.Set(10);
				newComponent.isInteractive = false;
				
				AVOWGraph.singleton.PlaceComponent(newComponentGO, newNodeGO, nodeGO);
				AVOWSim.singleton.Recalc();
				return false;
			}
			case ExecuteStepState.kFixComponent:{
				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.isInteractive = true;
				newComponent.resistanceAngle.Set(45);
			
				AVOWNode newNode = newNodeGO.GetComponent<AVOWNode>();
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
	
}
