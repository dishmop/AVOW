using UnityEngine;
using System.Collections;

public class AVOWGameModes : MonoBehaviour {
	public static AVOWGameModes singleton = null;

	public enum GameModeState{
		kStartup,
		kMainMenu,
		kPlayStage,
		kStageComplete,
		kGameOver

	}
	public GameModeState state = GameModeState.kMainMenu;
	public int stage = 0;
	public int level = 0;
	public GameObject mainMenuPanel;
	public GameObject dlgPanel;
	public GameObject sidePanel;
	public GameObject backStory;
	public GameObject tutorialText;
	
	
	enum CameraChoice{
		kNone,
		kGameCam,
		kBackStoryCam,
	}

	// Use this for initialization
	void Start () {
		SelectCamera(CameraChoice.kGameCam);
		tutorialText.SetActive(true);

	
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
	
		if (AVOWBattery.singleton.IsDepleated()){
			AVOWBattery.singleton.FreezeBattery();
			state = GameModeState.kGameOver;
			foreach (GameObject go in AVOWGraph.singleton.allComponents){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (component.type == AVOWComponent.Type.kLoad){
					component.Kill (89);
				}
			}
		}
		
		mainMenuPanel.SetActive(state == GameModeState.kMainMenu);
		dlgPanel.SetActive(state == GameModeState.kStageComplete || state == GameModeState.kGameOver);
		dlgPanel.transform.FindChild("StageCompleteDlg").gameObject.SetActive(state == GameModeState.kStageComplete);
		dlgPanel.transform.FindChild("GameOverDlg").gameObject.SetActive(state == GameModeState.kGameOver);
	}
	
	public void PlayFree(){

		AVOWTutorialText.singleton.activated = false;
		SelectCamera(CameraChoice.kGameCam);
		sidePanel.SetActive(true);
		backStory.SetActive(false);
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(true);
		
		RestartFreePlayGame();
	}
	
	public void PlayTutorial1(){
		
		AVOWTutorialText.singleton.activated = true;
		SelectCamera(CameraChoice.kGameCam);
		AVOWConfig.singleton.DisplayBottomPanel(true);
		AVOWConfig.singleton.DisplaySidePanel(true);
		backStory.SetActive(false);
		
		AVOWTutorialManager.singleton.StartTutorial();
		
		RestartTutorialGame();
	}
	
	public void PlayBackStory(){

		
		AVOWTutorialText.singleton.activated = true;
		AVOWConfig.singleton.DisplayBottomPanel(true);
		AVOWConfig.singleton.DisplaySidePanel(false);
		SelectCamera(CameraChoice.kBackStoryCam);
		AVOWBackStoryCutscene.singleton.StartBackStory();
		backStory.SetActive(true);
		
		RestartFreePlayGame();
	}
	

	
	void SelectCamera(CameraChoice cam){
		// First set them all inactive
		BackStoryCamera.singleton.gameObject.SetActive(false);
		AVOWCamControl.singleton.gameObject.SetActive(false);
		
		BackStoryCamera.singleton.gameObject.SetActive(false);
		switch (cam){
		 	case CameraChoice.kGameCam:{
				AVOWCamControl.singleton.gameObject.SetActive(true);
				break;
		 	}
		 	case CameraChoice.kBackStoryCam:{
				BackStoryCamera.singleton.gameObject.SetActive(true);
				break;
		 	}
		}
	}
	void RestartTutorialGame(){
		AVOWConfig.singleton.maxNumResistors = 5;
		AVOWConfig.singleton.noResistorLimit = false;
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWCircuitCreator.singleton.Restart();
		AVOWUI.singleton.Restart();
		AVOWObjectives.singleton.Restart();
		AVOWVizTotals.singleton.DestroyLines();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();
		
		
		state = GameModeState.kPlayStage;
	}
		
	void RestartFreePlayGame(){
		AVOWConfig.singleton.maxNumResistors = 0;
		AVOWConfig.singleton.noResistorLimit = true;
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWCircuitCreator.singleton.Restart();
		AVOWUI.singleton.Restart();
		AVOWObjectives.singleton.Restart();
		AVOWVizTotals.singleton.DestroyLines();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();


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
		AVOWConfig.singleton.showObjectives = true;
		AVOWGraph.singleton.ClearCircuit();
		AVOWCircuitCreator.singleton.Restart();
		AVOWUI.singleton.Restart();
		AVOWObjectives.singleton.Restart();
		AVOWVizObjectives.singleton.Rebuild();
		AVOWVizTotals.singleton.BuildLines();
		AVOWBattery.singleton.ResetBattery();
		AVOWTutorialText.singleton.activated = false;
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(true);
		
		state = GameModeState.kPlayStage;
	}
	
	public void ReturnToGame(){
		state = GameModeState.kPlayStage;
	}	
	
	public void SetStageComplete(){
		state = GameModeState.kStageComplete;
	}
	
	public void GoToMain(){
		AVOWBackStoryCutscene.singleton.StopBackStory();
		AVOWTutorialManager.singleton.StopTutorial();
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(false);
		state = GameModeState.kMainMenu;
	}
	
	public void GoToMainFromComplete(){
		PlayFree();
		state = GameModeState.kMainMenu;
	}
}
