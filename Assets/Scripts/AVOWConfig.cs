using UnityEngine;
using System.Collections;

public class AVOWConfig : MonoBehaviour {

	public static AVOWConfig singleton = null;

	public bool showTotals = true;
	public bool showIndividuals = true;
	public bool hideObjectives = false;
	public bool noResistorLimit = false;
	public int maxNumResistors = 3;
	public bool useLCM = false;
	public bool modifiedNodeLengths = false;
	public bool visualiseTargets = false;
	

	
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
