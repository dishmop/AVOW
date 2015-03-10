using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialTextPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float panelHeght = GetComponent<RectTransform>().anchorMax.y * Screen.height;
		transform.FindChild ("Image").FindChild ("Text").GetComponent<Text>().fontSize = (int)panelHeght / 8;
		
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
