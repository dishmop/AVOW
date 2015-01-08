using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	static List<AVOWTab>	tabs = new List<AVOWTab>();
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	
	AVOWTab selectedTab = null;
		

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

		AVOWGraph.singleton.PlaceComponent(resistors[0], node0, node1);
		AVOWGraph.singleton.PlaceComponent(resistors[1], node1, node2);
		AVOWGraph.singleton.PlaceComponent(resistors[2], node2, node1);
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
		
		bool  buttonClicked = (Input.GetMouseButtonDown(0) && !Input.GetKey (KeyCode.LeftControl));
		bool  buttonDown = (Input.GetMouseButton(0) && !Input.GetKey (KeyCode.LeftControl));
		
		if (buttonClicked || !buttonDown) selectedTab = null;
		// If we do not have anything selected
		if (selectedTab == null){
			foreach (AVOWTab tab in tabs){
				bool isInside = tab.IsContaining(mouseWorldPos);
				tab.SetMouseInside(isInside);
				if (isInside){
					if (buttonClicked){
						
						selectedTab = tab;
					}
				}
				tab.SetSelected(tab == selectedTab);
			}
		}
		// If we do have something selected, then we have different logic
		else{
			foreach (AVOWTab tab in tabs){
				tab.SetMouseInside(false);
			}			
			foreach (AVOWTab tab in tabs){
				// if we are in our select tabm then do nothing
				if (tab == selectedTab) continue;
				
				bool isInside = tab.IsContaining(mouseWorldPos);
				// If we are inside this tab, find all the other tabs which are
				// part of this node - as we are "inside" all of them now too
				if (isInside){
					AVOWGraph.Node node = tab.GetNode();
					foreach (GameObject go in node.components){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						AVOWTab otherTab = null;
						if (component.node0 == node){
							otherTab = go.transform.FindChild("LowerTab").GetComponent<AVOWTab>();
						}
						else{
							otherTab = go.transform.FindChild("UpperTab").GetComponent<AVOWTab>();
						}
						otherTab.SetMouseInside(true);
					}
				}

			}			
			
			// 
		}
		
		
		// If we have a selected tab, then figure out if any tabs need to be disabled
		

	
	}
}
