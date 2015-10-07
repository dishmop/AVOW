using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWConfig : MonoBehaviour {

	public static AVOWConfig singleton = null;

	public bool showTotals = true;
	public bool showIndividuals = true;
	public bool showObjectives = true;
	public bool objectivesAsText = true;
	public bool playbackEnable = true;
	public bool showMetal = true;

	public bool  modifiedNodeLengths = false;
	public float tutorialSpeed = 1;
	
	public float sideBarPixels = 0.2f;
	public float bottomBarFrac = 0.2f;
	public float buttonFlashRate = 1f;
	public bool levelExcludeEdit = false;
	
	public float flockDesDistToOther = 2f;
	public float flockDesSpeed = 1f;
	public float flockAlignCoef = 0.2f;
	public float flockHomeCoef= 1f;
	public float flockSpeedMod = 1f;
	public float flockSpiralCoef = 0;
	public bool flockReset = false;
	
	public bool tutDisableMouseMove = false;
	public bool tutDisableConnections = false;
	public bool tutDisableCreateUIButton = false;
	public bool tutDisableDestroyUIButton = false;
	public bool tutDisableMouseButtton = false;
	public bool tutDisableBarConstruction = false;
	public bool tutDisable2ndComponentConnections = false;
	public bool tutDisable2ndBarConnections = false;
	public bool tutDisableComponentConstruction = false;
	
	public float minComponentWidth = 1;
	
	const int		kLoadSaveVersion = 1;	
	
	SpringValue bottomPanelUseFrac = new SpringValue(0, SpringValue.Mode.kLinear);
	SpringValue sidePanelUseFrac = new SpringValue(0, SpringValue.Mode.kLinear);
	
	
	float sideBarFrac;
	
	public SpringValue cubeToCursor = new SpringValue(1, SpringValue.Mode.kLinear, 1f);
	
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
	
	
	public void Initialise(){
		// Work out the sidebar fract from the pixels
		sideBarFrac = sideBarPixels / Screen.width;
		
	
		bottomPanelUseFrac.SetSpeed (0.1f/bottomBarFrac);
		sidePanelUseFrac.SetSpeed (0.1f/sideBarFrac);
	}
	
	public Vector3 GetViewCentre(){
		Vector3 min = new Vector3(Screen.width * GetSidePanelFrac(), Screen.height * GetBottomPanelFrac(), 0);
		Vector3 max = new Vector3(Screen.width, Screen.height, 0);
		
		return new Vector3(Mathf.Lerp (min.x, max.x, 0.53f), Mathf.Lerp (min.y, max.y, 0.5f), Mathf.Lerp (min.z, max.z, 0.5f));
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	// Update is called once per frame
	public void RenderUpdate () {
		bottomPanelUseFrac.Update ();
		sidePanelUseFrac.Update ();
		
		if (tutDisableMouseMove){
			cubeToCursor.Force (0);
		}
		else if (cubeToCursor.GetDesValue() == 0){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.lockState = CursorLockMode.Confined;
			cubeToCursor.Set (1);
		}
		cubeToCursor.Update ();
		
		// Work out the smallest object we have
		minComponentWidth = 1;
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kLoad && component.hWidth > 0.01f && MathUtils.FP.Feq (component.resistanceAngle.GetValue(), 45)){
				minComponentWidth = Mathf.Min (minComponentWidth, component.hWidth);
			}
		}
		
		
	}
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bottomPanelUseFrac.Serialise(bw);
		sidePanelUseFrac.Serialise(bw);
		bw.Write (sideBarFrac);
		
	}
	

	public void Deserialise(BinaryReader br){
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				bottomPanelUseFrac.Deserialise(br);
				sidePanelUseFrac.Deserialise(br);
				sideBarFrac = br.ReadSingle ();
				break;
			}
		
		}
	}


		
}
