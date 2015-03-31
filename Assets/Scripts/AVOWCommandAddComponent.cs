using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWCommandAddComponent : AVOWCommand{

	public GameObject node0GO;
	public GameObject node1GO;
	public GameObject prefab;
	GameObject newComponentGO;
	
	const int		kLoadSaveVersion = 1;		
	
	
	enum ExecuteStepState{
		kMakeGap,
		kFixComponent,
		kFinished
	}
	
	ExecuteStepState executeStep = ExecuteStepState.kMakeGap;
	
	
	public AVOWCommandAddComponent(){
		
	}
	
	
	public AVOWCommandAddComponent(GameObject fromNodeGO, GameObject toNodeGO, GameObject prefabToUse){
		node0GO = fromNodeGO;
		node1GO = toNodeGO;
		prefab = prefabToUse;
		
	}
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		AVOWGraph.singleton.SerialiseRef(bw, node0GO);
		AVOWGraph.singleton.SerialiseRef(bw, node1GO);
		AVOWGraph.singleton.SerialiseRef(bw, newComponentGO);
		bw.Write ((int)executeStep);
		
	}
	
	public void Deserialise(BinaryReader br){
		int version = br.ReadInt32();
		switch (version){
		case kLoadSaveVersion:{
			node0GO = AVOWGraph.singleton.DeseraliseRef(br);
			node1GO = AVOWGraph.singleton.DeseraliseRef(br);
			newComponentGO = AVOWGraph.singleton.DeseraliseRef(br);
			executeStep = (ExecuteStepState)br.ReadInt32 ();
			break;
		}
		}
	}
	
	public bool IsFinished(){	
		return executeStep == ExecuteStepState.kFinished;
	}
	
	public bool ExecuteStep(){
		switch (executeStep){
			case ExecuteStepState.kMakeGap:{
				newComponentGO = GameObject.Instantiate(prefab) as GameObject;
				AVOWComponent newComponent = newComponentGO.GetComponent<AVOWComponent>();
			
				newComponent.resistanceAngle.Force(89);
				newComponent.resistanceAngle.Set(70);
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
