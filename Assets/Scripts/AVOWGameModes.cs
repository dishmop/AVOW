using UnityEngine;
using System.Collections;

public class AVOWGameModes : MonoBehaviour {
	public static AVOWGameModes singleton = null;

	public enum GameModeState{
		kStartup,
		kMainMenu,
		kPlayStage,
		kStageComplete

	}
	public GameModeState state = GameModeState.kMainMenu;
	public int stage = 0;
	public int level = 0;
	public GameObject mainMenuPanel;

	// Use this for initialization
	void Start () {

	
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	
	// Update is called once per frame
	void Update () {
	
		mainMenuPanel.SetActive(state == GameModeState.kMainMenu);
	}
	
	public void PlayFree(){
		state = GameModeState.kPlayStage;
	}

	public void PlayEasy(){
		state = GameModeState.kPlayStage;
	}
	
	public void PlayMedium(){
		state = GameModeState.kPlayStage;
	}
	
	public void PlayHard(){
		state = GameModeState.kPlayStage;
	}

	public void PlayVeryHard(){
		state = GameModeState.kPlayStage;
	}
	
	public void GoToMain(){
		state = GameModeState.kMainMenu;
	}
}
