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
	public float tutorialSpeed = 1;
	
	public float flockDesDistToOther = 2f;
	public float flockDesSpeed = 1f;
	public float flockAlignCoef = 0.2f;
	public float flockHomeCoef= 1f;
	public float flockSpeedMod = 1f;
	public float flockSpiralCoef = 0;
	public bool flockReset = false;
	
	public bool tutDisableConnections = false;
	public bool tutDisableUIButtons = false;
	public bool tutDisableMouseButtton = false;
	public bool tutDisableBarConstruction = false;
	public bool tutDisable2ndComponentConnections = false;
	public bool tutDisable2ndBarConnections = false;
	public bool tutDisableComponentConstruction = false;
	
	
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
