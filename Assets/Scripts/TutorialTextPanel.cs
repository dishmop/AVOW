using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialTextPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<RectTransform>().anchorMax = new Vector2(1, AVOWConfig.singleton.GetBottomPanelFrac());
		float panelHeght = GetComponent<RectTransform>().anchorMax.y * Screen.height;
		
		transform.FindChild ("Image").FindChild ("Text").GetComponent<Text>().fontSize = (int)panelHeght / 10;
		
	
	}
}
