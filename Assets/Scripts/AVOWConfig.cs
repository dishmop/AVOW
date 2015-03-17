using UnityEngine;
using System.Collections;

public class AVOWConfig : MonoBehaviour {

	public static AVOWConfig singleton = null;

	public bool showTotals = true;
	public bool showIndividuals = true;
	public bool showObjectives = true;
	public bool objectivesAsText = true;

	public bool  noResistorLimit = false;
	public int 	 maxNumResistors = 3;
	public bool  useLCM = false;
	public bool  modifiedNodeLengths = false;
	public float tutorialSpeed = 1;
	
	public float sideBarFrac = 0.2f;
	public float bottomBarFrac = 0.2f;
	
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
	
	SpringValue bottomPanelUseFrac = new SpringValue(0, SpringValue.Mode.kLinear);
	SpringValue sidePanelUseFrac = new SpringValue(0, SpringValue.Mode.kLinear);
	
	public void DisplayBottomPanel(bool show){
		bottomPanelUseFrac.Set (show ? bottomBarFrac :  0);
		
	}
	
	public void DisplaySidePanel(bool show){
		sidePanelUseFrac.Set (show ? sideBarFrac :  0);
		
	}

	public float GetBottomPanelFrac(){
		return bottomPanelUseFrac.GetValue();
	}
	
	public float GetSidePanelFrac(){
		return sidePanelUseFrac.GetValue();
	}
	
	public bool ShowTextObjectives(){
		return showObjectives && objectivesAsText;
	}
	
	
	public bool ShowGraphicObjectives(){
		return showObjectives && !objectivesAsText;
	}
	
	
	public void Start(){
		bottomPanelUseFrac.SetSpeed (0.1f/bottomBarFrac);
		sidePanelUseFrac.SetSpeed (0.1f/sideBarFrac);
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
		bottomPanelUseFrac.Update ();
		sidePanelUseFrac.Update ();
		
	}
}
