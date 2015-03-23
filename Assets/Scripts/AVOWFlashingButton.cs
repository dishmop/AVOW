using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AVOWFlashingButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		Color col = transform.GetComponent<Text>().color;
		col.a = 0.75f + 0.25f * Mathf.Sin (2 * Mathf.PI * Time.time / AVOWConfig.singleton.buttonFlashRate);
		transform.GetComponent<Text>().color = col;
	
	
	
		//		Color col = transform.GetComponent<Button>().colors.normalColor;
		//		col.a = 0.75f + 0.25f * Mathf.Sin (AVOWConfig.singleton.buttonFlashRate * Time.time / 2 * Mathf.PI);
		//		transform.GetComponent<Button>().colors.normalColor.col;
	}
}
