using UnityEngine;
using System.Collections;

public class QuitOnEsc : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Test for exit
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (GameModeManager.singleton != null){
				GameModeManager.singleton.ReallyQuitGame();
			}
			else{
				AppHelper.Quit();
			}
		}
	
	}
}
