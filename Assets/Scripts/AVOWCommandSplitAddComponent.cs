using UnityEngine;
using System;

public class AVOWCommandSplitAddComponent : AVOWCommand{

	public GameObject nodeGO;
	public GameObject movedComponent;
	public GameObject prefab;
	GameObject newComponent;
	
	enum UndoStepState{
		kRemoveComponent,
		kMergeNode
	}
	
	UndoStepState step = UndoStepState.kRemoveComponent;
	
	public AVOWCommandSplitAddComponent(GameObject splitNodeGO, GameObject component, GameObject prefabToUse){
		nodeGO = splitNodeGO;
		movedComponent = component;
		prefab = prefabToUse;
		
	}

	public void Execute(){
		GameObject newNodeGO = AVOWGraph.singleton.SplitNode(nodeGO, movedComponent.GetComponent<AVOWComponent>());
		
		newComponent = GameObject.Instantiate(prefab) as GameObject;
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Force(0);
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Set(10);
		newComponent.SetActive(false);
		
		AVOWGraph.singleton.PlaceComponent(newComponent, newNodeGO, nodeGO);
		AVOWSim.singleton.Recalc();
		
		
	}

	public bool UndoStep(){
		AVOWComponent component = newComponent.GetComponent<AVOWComponent>();
		switch (step){
			case UndoStepState.kRemoveComponent:{
				component.Kill (0);
				component.onDeadCommand = this;
				step = UndoStepState.kMergeNode;
				return false;
			}
			case UndoStepState.kMergeNode:{
				AVOWGraph.singleton.MergeNodes(component.node0GO, component.node1GO);
				return true;
			}
		};
		return false;
	
	}
	
}
