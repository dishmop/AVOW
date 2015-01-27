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

	public void Execute(){
		newComponent = GameObject.Instantiate(prefab) as GameObject;
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Force(80);
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Set(45);
		
		AVOWGraph.singleton.PlaceComponent(newComponent, node0GO, node1GO);
		AVOWSim.singleton.Recalc();
		
	}

	public bool UndoStep(){
		newComponent.GetComponent<AVOWComponent>().Kill (89);
		return true;
	}
	
}
