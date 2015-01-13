using UnityEngine;
using System;

public class AVOWCommandAddComponent : AVOWCommand{

	public AVOWGraph.Node node0;
	public AVOWGraph.Node node1;
	public GameObject prefab;
	GameObject newComponent;
	
	public AVOWCommandAddComponent(AVOWGraph.Node fromNode, AVOWGraph.Node toNode, GameObject prefabToUse){
		node0 = fromNode;
		node1 = toNode;
		prefab = prefabToUse;
		
	}

	public void Execute(){
		newComponent = GameObject.Instantiate(prefab) as GameObject;
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Force(89);
		newComponent.GetComponent<AVOWComponent>().resistanceAngle.Set(45);
		
		AVOWGraph.singleton.PlaceComponent(newComponent, node0, node1);
		AVOWSim.singleton.Recalc();
		
	}

	public bool UndoStep(){
		newComponent.GetComponent<AVOWComponent>().Kill (89);
		return true;
	}
	
}
