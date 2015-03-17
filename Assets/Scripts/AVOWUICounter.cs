using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AVOWUICounter : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update (){
		if (AVOWConfig.singleton.noResistorLimit){
			transform.FindChild("Label").GetComponent<Text>().text = "∞";
			
		}
		else{
			int numComponentsLeft = (AVOWConfig.singleton.maxNumResistors - AVOWGraph.singleton.GetNumConfirmedLoads());
			transform.FindChild("Label").GetComponent<Text>().text = numComponentsLeft.ToString();
			AVOWUI.singleton.canCreate = (numComponentsLeft > 0);
		}
	}
}
