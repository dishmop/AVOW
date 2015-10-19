using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AVOWMainMenu : MonoBehaviour {

	public GameObject buttonPrefab;
	List<GameObject> buttons = new List<GameObject>();

	// Use this for initialization
	void Start () {
	

	
		int numButtons = AVOWGameModes.singleton.GetNumMainMenuButtons() + 1;
		int minButtonIndex = AVOWGameModes.singleton.GetMinMainMenuButton();
		
		int creditsIndex = numButtons-1;
		int clearPlayerPrefsIndex = -1;
		if (AVOWGameModes.singleton.enableClearPlayerPrefs){
			numButtons++;
			clearPlayerPrefsIndex = numButtons-1;
		}
		
		float top = 0.8f;
		float bottom = 0.17f;
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
			if (i == creditsIndex){
				newButton.GetComponent<Text>().text = "Credits";
				newButton.GetComponent<AVOWMenuButton>().levelNum = i + minButtonIndex;
			}
			else if (i == clearPlayerPrefsIndex){
				newButton.GetComponent<Text>().text = "Clear Player Prefs";
				newButton.GetComponent<AVOWMenuButton>().levelNum = i + minButtonIndex;
			}
			else{
				newButton.GetComponent<Text>().text = AVOWGameModes.singleton.GetLevelName(i + minButtonIndex);
				newButton.GetComponent<AVOWMenuButton>().levelNum = i + minButtonIndex;
			}
			buttons.Add(newButton);
			SetButtonEnable(i);
		}
	
	
	}
	
	// Update is called once per frame
	void Update () {
		SetButtonEnable(3);
		SetButtonEnable(4);
	}
	
	void SetButtonEnable(int index){
		if (index != 3 && index != 4) return;
		
		int minButtonIndex = AVOWGameModes.singleton.GetMinMainMenuButton();
		buttons[index].GetComponent<Button>().interactable = (PlayerPrefs.GetInt(AVOWGameModes.singleton.GetLevelName(index + minButtonIndex-1)) == 42);
	}
}
