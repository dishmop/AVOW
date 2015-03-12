using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AVOWUIButton : MonoBehaviour {

	public AVOWUI.ToolMode mode;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.FindChild("SelectionFrame").gameObject.SetActive((mode == AVOWUI.singleton.GetUIMode()));
		// Override the create button
		if (mode == AVOWUI.ToolMode.kCreate){
			if (AVOWConfig.singleton.noResistorLimit){
				GetComponent<Text>().text = "Create";
				AVOWUI.singleton.canCreate = true;
			}
			else{
				int numComponentsLeft = (AVOWConfig.singleton.maxNumResistors - AVOWGraph.singleton.allComponents.Count + 1);
				GetComponent<Text>().text = "Create (" + numComponentsLeft.ToString() + ")";
				AVOWUI.singleton.canCreate = (numComponentsLeft > 0);
			}
		}
			
	}
}
