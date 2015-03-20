using UnityEngine;
using System.Collections;

public class AVOWGameModes : MonoBehaviour {
	public static AVOWGameModes singleton = null;

	public enum GameModeState{
		kStartup,
		kMainMenu,
		kPlayStage,
		kStageComplete0,
		kStageComplete1,
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
	public GameObject greenBackground;
	public GameObject scenery;
	public GameObject pusher;
	
	int currentLevel = -1;
	
	const int kBackStoryIndex = -2;
	const int kTutorialIndex = -1;
	const int kFreePlayIndex = 0;
	// then level 1, 2.... etc.

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
	
	public int GetNumMainMenuButtons(){
		return 7;
	}
	
	public int GetMinMainMenuButton(){
		return -2;
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
		dlgPanel.SetActive(state == GameModeState.kGameOver);
		//dlgPanel.transform.FindChild("StageCompleteDlg").gameObject.SetActive(state == GameModeState.kStageComplete);
		dlgPanel.transform.FindChild("GameOverDlg").gameObject.SetActive(state == GameModeState.kGameOver);
		
		switch (state){
			case GameModeState.kStageComplete0:{
				// Get rid of all our resistance squares
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					if (component.type == AVOWComponent.Type.kLoad){
						component.Kill (89);
					}
				}
				pusher.GetComponent<AVOWPusher>().disableMovement = true;
				AVOWCamControl.singleton.disableMovement = true;
				if (AVOWGraph.singleton.allComponents.Count == 1){
					state = GameModeState.kStageComplete1;
				}
				AVOWConfig.singleton.tutDisableConnections = true;
				break;	
			}
			case GameModeState.kStageComplete1:{
				// Get rid of all our resistance squares
				float scaleVal = scenery.transform.localScale.x;
				scenery.transform.localScale = new Vector3(scaleVal * 1.02f, scaleVal * 1.02f, 1);
				break;	
			}
			case GameModeState.kMainMenu:{
				AVOWUI.singleton.enableToolUpdate = false;
				AVOWConfig.singleton.tutDisableConnections = false;
				break;	
			}
			default:{
				AVOWUI.singleton.enableToolUpdate = true;
				greenBackground.GetComponent<AVOWGreenBackground>().MakeSmall();
				pusher.GetComponent<AVOWPusher>().disableMovement = false;
				AVOWCamControl.singleton.disableMovement = false;
				scenery.transform.localScale = new Vector3(1, 1, 1);
				break;
			}
		}


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
	
	public void PlayTutorial(){
		
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
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWUI.singleton.Restart();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();
		
		
		state = GameModeState.kPlayStage;
	}
		
	void RestartFreePlayGame(){
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWUI.singleton.Restart();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();

		state = GameModeState.kPlayStage;
	}


	
	public void StartLevel(int levelNum){
		currentLevel = levelNum;
		if (currentLevel > 0){
			AVOWObjectiveManager.singleton.InitialiseLevel(levelNum);
			RestartNormalGame();
		}
		else{
			switch (currentLevel){
				case kBackStoryIndex:{
					PlayBackStory();
					break;
				}
				case kTutorialIndex:{
					PlayTutorial();
					break;
				}
				case kFreePlayIndex:{
					PlayFree();
					break;
				}
			}
		}
		
	}
	
	public void StartNextLevel(){
		currentLevel++;
		StartLevel (currentLevel);
		
	}
	
	void RestartNormalGame(){
		AVOWConfig.singleton.showObjectives = true;
		AVOWGraph.singleton.ClearCircuit();
		AVOWUI.singleton.Restart();
		AVOWBattery.singleton.ResetBattery();
		AVOWTutorialText.singleton.activated = false;
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(true);
		
		AVOWBattery.singleton.FreezeBattery();
		
		
		state = GameModeState.kPlayStage;
	}
	
	public void ReturnToGame(){
		state = GameModeState.kPlayStage;
	}	
	
	public void SetStageComplete(){
		state = GameModeState.kStageComplete0;
	}
	
	// This is a bit bodgey
	public void PreStageComplete(){
		greenBackground.GetComponent<AVOWGreenBackground>().MakeBig();
	}
	
	public void GoToMain(){
		AVOWBackStoryCutscene.singleton.StopBackStory();
		AVOWTutorialManager.singleton.StopTutorial();
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(false);
		AVOWCircuitCreator.singleton.Deinitialise();
		state = GameModeState.kMainMenu;
	}
	
	public void GoToMainFromComplete(){
		PlayFree();
		state = GameModeState.kMainMenu;
	}
	
	public string GetLevelName(int index){
		if (index > 0){
			return "Level " + index.ToString();
		}
		switch (index){
			case kFreePlayIndex:{
				return "Free play";
			}
			case kBackStoryIndex:{
				return "Back story";
			}
			case kTutorialIndex:{
				return "Tutorial";
			}
		}
		return "Unkown level";
	
	}
}
