using UnityEngine;
using System.Collections;

public class QuitOnEsc : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		
	//	Time.timeScale = 0.1f;
	
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
		
		Vector3 mousePos = Input.mousePosition;
		if (mousePos.x < Screen.width * 0.1f || 
			AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kMainMenu || 
			AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kStageComplete ||
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kGameOver ){
			Cursor.visible = true;
			
		}
		else
		{
			Cursor.visible = false;
			
		}
	
	}
}
