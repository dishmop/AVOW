using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	static List<AVOWTab>	tabs = new List<AVOWTab>();
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
		

	public void RegisterTab(AVOWTab tab){
		tabs.Add(tab);
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	
	void Start(){
		AVOWGraph.Node node0 = AVOWGraph.singleton.AddNode ();
		AVOWGraph.Node node1 = AVOWGraph.singleton.AddNode ();
		AVOWGraph.Node node2 = AVOWGraph.singleton.AddNode ();
		
		GameObject[] resistors = new GameObject[3];
		resistors[0] = GameObject.Instantiate(resistorPrefab) as GameObject;
		resistors[1] = GameObject.Instantiate(resistorPrefab) as GameObject;
		resistors[2] = GameObject.Instantiate(resistorPrefab) as GameObject;
		
		GameObject cell = GameObject.Instantiate(cellPrefab) as GameObject;
		
		// We want 
		AVOWGraph.singleton.PlaceComponent(resistors[0], node0, node1);
		AVOWGraph.singleton.PlaceComponent(resistors[1], node1, node2);
		AVOWGraph.singleton.PlaceComponent(resistors[2], node1, node2);
		AVOWGraph.singleton.PlaceComponent(cell, node0, node2);
		
		bool ok = AVOWGraph.singleton.ValidateGraph();
		if (!ok){
			Debug.LogError ("built an invalid graph");
		}
		
		
	}
	
	

	// Update is called once per frame
	void Update () {
		// Get the mouse position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
							
													
		foreach (AVOWTab tab in tabs){
			bool isInside = tab.IsContaining(mouseWorldPos);
			tab.SetMouseInside(isInside);
		}

	
	}
}
