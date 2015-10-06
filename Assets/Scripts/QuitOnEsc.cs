using UnityEngine;
using System.Collections;

public class QuitOnEsc : MonoBehaviour {
	public float mouseMoveCursorTimeout = 5;
	

	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		
		//Time.timeScale = 0.2f;
	
	}
	
	// Update is called once per frame
	void Update () {
		// if the back story is on, then do a timed cursor
		
		// Test for exit
		if (Input.GetKeyDown (KeyCode.Escape)) {
			AppHelper.Quit();
		}
		
		Vector3 mousePos = Input.mousePosition;
		if (mousePos.x < Screen.width * AVOWConfig.singleton.GetSidePanelFrac() || 
			AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kSplashScreen || 
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kMainMenu || 
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kStageComplete3 ||
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kStageComplete4 ||
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kGameOver ){
			Cursor.visible = true;
			
		}
		else
		{
			Cursor.visible = AVOWGameModes.singleton.showPointer;
			
		}
		
		Cursor.lockState = CursorLockMode.Confined;
		

		
	}
}
