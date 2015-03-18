using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWVizTotals : MonoBehaviour {
	public static AVOWVizTotals singleton = null;

	public GameObject lineGOPrefab;
	
	public Color			colNotYetDoneAndInactive;
	public Color			colNotYetDoneAndActive;
	public Color			colDone;
	public Color			colDoneAndForgotten;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		SetColors();
	}
	
	void SetColors(){
		for (int i = 0; i < transform.childCount; ++i){
			DrawnLine line = transform.GetChild(i).GetComponent<DrawnLine>();
			if (i < AVOWVizObjectives.singleton.displayedObjective){
				line.lineColor = colDoneAndForgotten;
			}
			else if (i == AVOWVizObjectives.singleton.displayedObjective && i != AVOWObjectives.singleton.currentObjective){
				line.lineColor = colDone;
			}
			else if (i == AVOWVizObjectives.singleton.displayedObjective){
				line.lineColor = colNotYetDoneAndActive;
			}

			else{
				line.lineColor = colNotYetDoneAndInactive;
			}
		}
	}
	
	public void BuildLines(){
		DestroyLines();
		List< AVOWCircuitTarget > results = AVOWCircuitCreator.singleton.GetResults();
		for (int i = 0; i < results.Count; ++i){
			float total = results[i].totalCurrent;
			
			GameObject newLine = GameObject.Instantiate(lineGOPrefab) as GameObject;
			newLine.transform.parent = transform;
			newLine.GetComponent<DrawnLine>().Draw(new Vector2 (total, 0), new Vector2(total, -0.1f), new Color(128, 128, 128));
		}
	}
	
	public void DestroyLines(){
		foreach(Transform child in transform){
			GameObject.Destroy(child.gameObject);
		}
	}
	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}
}
