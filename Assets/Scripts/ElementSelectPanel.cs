﻿using UnityEngine;
using System.Collections;

public class ElementSelectPanel : MonoBehaviour {

	public GameObject 				leccyButtonPrefab;
	public CircuitElement.UIType 	uiTypeFilter;
	public float					xCount;		// Number of buttons in a row
	public float					yCount;		// Number of buttons in a column
	public string					defaultSelectionId;
	
	
	public AudioSource				pressSound;
	
	GameObject						defaultSelectionButton = null;
	
	
	public void ClearSelection(){
		foreach (Transform child in transform){
			child.GetComponent<LeccyUIButton>().isSelected = false;
		}
	}

	// Use this for initialization
	public void Start () {
		ElementFactory factory = ElementFactory.singleton;
		int numElements = factory.GetNumElements(uiTypeFilter);
		int xPos = 0;
		int yPos = 0;
		for (int i = 0; i < numElements; ++i){
			GameObject prefab = factory.GetPrefab(uiTypeFilter, i);
			
			// If not in edtor mode and this element is only available in the left, then skip this one
			if (prefab.GetComponent<CircuitElement>().IsEditorOnly() && !GameModeManager.singleton.enableEditor) continue;
			
			// If non to pick from, then don;t show it (unless in left mode)
			if (factory.GetStockRemaining(prefab.GetComponent<SerializationID>().id) == 0 && !GameModeManager.singleton.enableEditor) continue;
			
			GameObject newButton = Instantiate(leccyButtonPrefab) as GameObject;
			newButton.transform.SetParent(transform, false);
			
			// Set which circuit element we are referencing
			newButton.GetComponent<LeccyUIButton>().circuitElementPrefab = prefab;
			if (prefab.GetComponent<SerializationID>().id == defaultSelectionId) {
				newButton.GetComponent<LeccyUIButton>().isSelected = true;
				defaultSelectionButton = newButton;
			}
			
			// Set the transform
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(xPos/xCount, (yCount - yPos - 1f)/yCount);
			rectTransform.anchorMax = new Vector2((1f + xPos)/xCount, (yCount - yPos)/yCount);
			xPos++;
			if (xPos >= xCount){
				xPos = 0;
				yPos++;
			}
			rectTransform.pivot = new Vector2(0, 0);
			rectTransform.offsetMax = new Vector2(0, 0);
			rectTransform.offsetMin = new Vector2(0, 0);
		}
	}
	
	public void OnLoadLevel(){
		// Make the button which is our default select, selected again
		defaultSelectionButton.GetComponent<LeccyUIButton>().SetSelected();
	
	
	}
	
	public void OnClick(){
		pressSound.Play();
		
	}	
	
	public void Cleanup(){
		//Remove all the buttons
		foreach (Transform child in transform){
			Destroy (child.gameObject);
		}
	}
	

	
}
