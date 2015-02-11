
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	
	public GameObject cursorBlueCubePrefab;
	public GameObject cursorGreenCubePrefab;
	public GameObject lighteningPrefab;
	
	
	public enum ToolMode{
		kCreate,
		kDelete
	}
	
	ToolMode mode = ToolMode.kCreate;
			
	AVOWUITool	uiTool;
	

	public Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();

	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	

	void Update(){
		uiTool.Update();
	}
	
	public GameObject InstantiateBlueCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorBlueCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	public GameObject InstantiateGreenCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorGreenCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	
	public GameObject InstantiateLightening(){
		GameObject obj = GameObject.Instantiate(lighteningPrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	


	

	public ToolMode GetUIMode(){
		return mode;
	}

	
	void Start(){
		
		AVOWGraph graph = AVOWGraph.singleton;

		// Simple start
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();

				
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node0GO);
		

		
		
		/*
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		GameObject node2GO = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2GO, node0GO);	
		*/
		
		AVOWSim.singleton.Recalc();
		uiTool = new AVOWUICreateTool();
		
		mode = ToolMode.kCreate;
		
		uiTool.Start();
		
	}
	
	public void SetCreateTool(){
		uiTool.OnDestroy();
		uiTool = new AVOWUICreateTool();
		uiTool.Start();
		mode = ToolMode.kCreate;
	}
	
	public void SetDeleteTool(){
		uiTool.OnDestroy();
		uiTool = new AVOWUIDeleteTool();
		uiTool.Start();
		mode = ToolMode.kDelete;
	}
	
}

