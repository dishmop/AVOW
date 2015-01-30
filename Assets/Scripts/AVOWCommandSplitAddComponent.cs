﻿using UnityEngine;
using System;

public class AVOWCommandSplitAddComponent : AVOWCommand{

	public GameObject nodeGO;
	public GameObject movedComponent;
	public GameObject prefab;
	GameObject newNodeGO;
	GameObject newComponentGO;
	
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
	
	
	public AVOWCommandSplitAddComponent(GameObject splitNodeGO, GameObject component, GameObject prefabToUse){
		nodeGO = splitNodeGO;
		movedComponent = component;
		prefab = prefabToUse;
		
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
				newComponentGO.SetActive(false);

				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.resistanceAngle.Force(0);
				newComponent.resistanceAngle.Set(10);
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
