using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
//		transform.FindChild("ExcludeToggle").gameObject.SetActive(drawTextInButtons && AVOWConfig.singleton.levelExcludeEdit);

		transform.FindChild("DisplayGoal").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("StoreGoal").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("LoadLevel").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("SaveLevel").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("ClearLevel").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("IncGoal").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("DecGoal").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("DispGoal").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("CopyPrevious").gameObject.SetActive(drawTextInButtons && AVOWLevelEditor.singleton.enableEditor);
		transform.FindChild("DispGoal").GetComponent<Text>().text = AVOWLevelEditor.singleton.currentGoal.ToString();
		
		
		
	}
}
