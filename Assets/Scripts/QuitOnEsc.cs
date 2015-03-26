using UnityEngine;
using System.Collections;

public class QuitOnEsc : MonoBehaviour {
	public float mouseMoveCursorTimeout = 5;
	float mouseMoveTime;
	

	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		
		//Time.timeScale = 0.2f;
	
	}
	
	// Update is called once per frame
	void Update () {
		// if the back story is on, then do a timed cursor
		Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		if (!MathUtils.FP.Feq (mouseDelta.magnitude, 0, 0.01f)){
			mouseMoveTime = Time.time;
		}
		
		// Test for exit
		if (Input.GetKeyDown (KeyCode.Escape)) {
			AppHelper.Quit();
		}
		
		Vector3 mousePos = Input.mousePosition;
		if (mousePos.x < Screen.width * AVOWConfig.singleton.GetSidePanelFrac() || 
			AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kMainMenu || 
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kStageComplete3 ||
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kStageComplete4 ||
		    AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kGameOver ){
			Cursor.visible = true;
			
		}
		else
		{
			if ( AVOWBackStoryCutscene.singleton.state != AVOWBackStoryCutscene.State.kOff){
				Cursor.visible = (Time.time < mouseMoveTime + mouseMoveCursorTimeout);
				
			}
			else{
				//otherwise we are probably just playing the game, so want to use the gane cursor
				Cursor.visible = false;
			}
			
		}
		
		Cursor.lockState = CursorLockMode.Confined;
		

		
	}
}
