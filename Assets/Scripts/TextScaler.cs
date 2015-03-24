using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Place this on the object with the Text component in and reference the object with the size we are interested in
public class TextScaler : MonoBehaviour {
	
	public int numLinesToFit;
	public GameObject sizeGO;
	float tuneVal = 1.2f;	// Fixed for this font
	
	// Use this for initialization
	void Start () {
		
		
	}
	
	// Update is called once per frame
	void Update () {
		float screenHeight = sizeGO.GetComponent<RectTransform>().rect.height;
		
		transform.GetComponent<Text>().fontSize = (int)(screenHeight / ((float)numLinesToFit * tuneVal));
		
		
	}
}
