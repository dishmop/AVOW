using UnityEngine;
using System;

public class AVOWCommandAddComponent : AVOWCommand{

	public GameObject node0GO;
	public GameObject node1GO;
	public GameObject prefab;
	GameObject newComponent;
	
	public AVOWCommandAddComponent(GameObject fromNodeGO, GameObject toNodeGO, GameObject prefabToUse){
		node0GO = fromNodeGO;
		node1GO = toNodeGO;
		prefab = prefabToUse;
		
	}

	public bool ExecuteStep(){
		newComponent = GameObject.Instantiate(prefab) as GameObject;
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Force(85);
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Set(80);
		
		AVOWGraph.singleton.PlaceComponent(newComponent, node0GO, node1GO);
		AVOWSim.singleton.Recalc();
		return true;
		
	}

	public bool UndoStep(){
		newComponent.GetComponent<AVOWComponent>().Kill (89);
		return true;
	}
	
}
