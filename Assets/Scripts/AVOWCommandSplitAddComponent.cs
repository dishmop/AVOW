using UnityEngine;
using System;

public class AVOWCommandSplitAddComponent : AVOWCommand{

	public AVOWGraph.Node node;
	public GameObject movedComponent;
	public GameObject prefab;
	GameObject newComponent;
	
	enum UndoStepState{
		kRemoveComponent,
		kMergeNode
	}
	
	UndoStepState step = UndoStepState.kRemoveComponent;
	
	public AVOWCommandSplitAddComponent(AVOWGraph.Node splitNode, GameObject component, GameObject prefabToUse){
		node = splitNode;
		movedComponent = component;
		prefab = prefabToUse;
		
	}

	public void Execute(){
		AVOWGraph.Node newNode = AVOWGraph.singleton.SplitNode(node, movedComponent.GetComponent<AVOWComponent>());
		
		newComponent = GameObject.Instantiate(prefab) as GameObject;
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Force(0);
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Set(45);
		
		AVOWGraph.singleton.PlaceComponent(newComponent, newNode, node);
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
				AVOWGraph.singleton.MergeNodes(component.node0, component.node1);
				return true;
			}
		};
		return false;
	
	}
	
}
