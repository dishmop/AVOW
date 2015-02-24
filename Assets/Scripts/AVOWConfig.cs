using UnityEngine;
using System.Collections;

public class AVOWConfig : MonoBehaviour {

	public static AVOWConfig singleton = null;

	public bool showTotals = true;
	public bool showIndividuals = true;
	public bool showObjectives = true;
	public bool objectivesAsText = true;

	public bool noResistorLimit = false;
	public int 	maxNumResistors = 3;
	public bool useLCM = false;
	public bool modifiedNodeLengths = false;
	
	
	public bool ShowTextObjectives(){
		return showObjectives && objectivesAsText;
	}
	
	
	public bool ShowGraphicObjectives(){
		return showObjectives && !objectivesAsText;
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
