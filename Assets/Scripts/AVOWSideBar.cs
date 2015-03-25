using UnityEngine;
using System.Collections;

public class AVOWSideBar : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<RectTransform>().anchorMin = new Vector2(0, AVOWConfig.singleton.GetBottomPanelFrac());
		GetComponent<RectTransform>().anchorMax = new Vector2(AVOWConfig.singleton.GetSidePanelFrac(), 1);
		
		bool drawTextInButtons = (AVOWConfig.singleton.GetSidePanelFrac() > 0.01f);
		
		transform.FindChild("ReturnButton").gameObject.SetActive(drawTextInButtons);
		transform.FindChild("ExcludeToggle").gameObject.SetActive(drawTextInButtons && AVOWConfig.singleton.levelExcludeEdit);
		
	}
}
