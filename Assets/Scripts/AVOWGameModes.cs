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
	public GameObject dlgPanel;

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
		dlgPanel.SetActive(state == GameModeState.kStageComplete);
		dlgPanel.transform.FindChild("StageCompleteDlg").gameObject.SetActive(state == GameModeState.kStageComplete);
	}
	
	public void PlayFree(){
		AVOWConfig.singleton.maxNumResistors = 2;
		AVOWConfig.singleton.noResistorLimit = true;
		AVOWConfig.singleton.hideObjectives = true;
		AVOWGraph.singleton.ClearCircuit();
		AVOWCircuitCreator.singleton.Restart();
		AVOWUI.singleton.Restart();
		AVOWObjectives.singleton.Restart();
		state = GameModeState.kPlayStage;
	}

	public void PlayEasy(){
		AVOWConfig.singleton.maxNumResistors = 2;
		RestartNormalGame();
	}
	
	public void PlayMedium(){
		AVOWConfig.singleton.maxNumResistors = 3;
		RestartNormalGame();
	}
	
	public void PlayHard(){
		AVOWConfig.singleton.maxNumResistors = 4;
		RestartNormalGame();
	}

	public void PlayVeryHard(){
		AVOWConfig.singleton.maxNumResistors = 5;
		RestartNormalGame();

	}
	
	void RestartNormalGame(){
		AVOWConfig.singleton.noResistorLimit = false;
		AVOWConfig.singleton.hideObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWCircuitCreator.singleton.Restart();
		AVOWUI.singleton.Restart();
		AVOWObjectives.singleton.Restart();
		state = GameModeState.kPlayStage;
	}
	
	public void ReturnToGame(){
		state = GameModeState.kPlayStage;
	}	
	
	public void SetStageComplete(){
		state = GameModeState.kStageComplete;
	}
	
	public void GoToMain(){
		state = GameModeState.kMainMenu;
	}
	
	public void GoToMainFromComplete(){
		PlayFree();
		state = GameModeState.kMainMenu;
	}
}
