using UnityEngine;
using System;

public class AVOWCommandAddComponent : AVOWCommand{

	public GameObject node0GO;
	public GameObject node1GO;
	public GameObject prefab;
	GameObject newComponentGO;
	
	
	enum ExecuteStepState{
		kMakeGap,
		kFixComponent,
		kFinished
	}
	
	ExecuteStepState executeStep = ExecuteStepState.kMakeGap;
	
	
	public AVOWCommandAddComponent(GameObject fromNodeGO, GameObject toNodeGO, GameObject prefabToUse){
		node0GO = fromNodeGO;
		node1GO = toNodeGO;
		prefab = prefabToUse;
		
	}
	
	public bool IsFinished(){	
		return executeStep == ExecuteStepState.kFinished;
	}
	
	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kMakeGap:{
				newComponentGO = GameObject.Instantiate(prefab) as GameObject;
				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
			
				newComponent.resistanceAngle.Force(85);
				newComponent.resistanceAngle.Set(80);
				newComponent.isInteractive = false;
			
				AVOWGraph.singleton.PlaceComponent(newComponentGO, node0GO, node1GO);
				AVOWSim.singleton.Recalc();
				executeStep = ExecuteStepState.kFixComponent;
				return false;
			}
			case ExecuteStepState.kFixComponent:{
				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
				newComponent.isInteractive = true;
				newComponent.resistanceAngle.Set(45);
				executeStep = ExecuteStepState.kFinished;
				
				return true;
			}		
			
		}
		return false;
	}
		
	public bool UndoStep(){
		newComponentGO.GetComponent<AVOWComponent>().Kill (89);
		return true;
	}
	
	public GameObject GetNewComponent(){
		return newComponentGO;
	}
	
	public GameObject GetNewNode(){
		return null;
	}
	
	
}
