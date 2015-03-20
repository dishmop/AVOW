using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AVOWMainMenu : MonoBehaviour {

	public GameObject buttonPrefab;

	// Use this for initialization
	void Start () {
	
		int numButtons = AVOWGameModes.singleton.GetNumMainMenuButtons();
		int minButtonIndex = AVOWGameModes.singleton.GetMinMainMenuButton();
		
		float top = 0.8f;
		float bottom = 0.1f;
		float step = (top - bottom) / numButtons;
		float gap = step * 0.8f;
		
		for (int i = 0; i < numButtons; ++i){	
			GameObject newButton = GameObject.Instantiate(buttonPrefab);
			newButton.transform.SetParent(transform);
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			int posIndex = numButtons - 1 - i;
			rectTransform.anchorMin = new Vector2(0.1f, bottom + posIndex * step);
			rectTransform.anchorMax = new Vector2(0.9f, bottom + posIndex * step + gap);
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			newButton.GetComponent<Text>().text = AVOWGameModes.singleton.GetLevelName(i + minButtonIndex);
			newButton.GetComponent<AVOWMenuButton>().levelNum = i + minButtonIndex;
		}
	
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
