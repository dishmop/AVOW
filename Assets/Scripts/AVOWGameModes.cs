using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;

public class AVOWGameModes : MonoBehaviour {
	public static AVOWGameModes singleton = null;

	public enum GameModeState{
		kSplashScreen,
		kPreMainMenu,
		kMainMenu,
		kPlayStage,
		kStageComplete0,
		kStageComplete1,
		kStageComplete2,
		kStageComplete3,
		kStageComplete4,
		kGameOver

	}

	public GameObject screenSpaceUI;
	public GameObject mainMenuPanel;
	public GameObject levelCompleteDlg;
	public GameObject gameCompleteDlg;
	public GameObject splashScreen;
	public GameObject whitePanel;
	public GameObject levelStartMsg;
	public GameObject sidePanel;
	public GameObject backStory;
	public GameObject tutorialText;
	public GameObject hintButton;
	public GameObject levelDisplay;
	public GameObject greenBackground;
	public GameObject scenery;
	public GameObject pusher;
	
	public float levelStartMsgDuration;
	public float levelStartFadeInDuration;
	public float levelStartFadeOutDuration;
	
	public GameModeState state = GameModeState.kSplashScreen;
	
	public bool showPointer = false;
	public float waitForTimedHintDuration;
	
	const int		kLoadSaveVersion = 1;	
	
	int currentLevel = -1;

	
//	const int kBackStoryIndex = -2;
	const int kTutorialIndex = -1;
	const int kFreePlayIndex = 0;
	// then level 1, 2.... etc.

	enum CameraChoice{
		kNone,
		kGameCam,
		kBackStoryCam,
	}
	
	CameraChoice currentCamSelect = CameraChoice.kNone;
	
	float levelStartMsgTime = -100f;
	bool showHint = false;
	bool showTimeHint = false;
	bool hasShownHint = false;
	float lastGoalTime = 10000f;
	
	
	// Set by buttons - and the level only ACTUALLY started from the gameupdate
	int triggerStartLevel = kTutorialIndex - 1;
	
	public void TriggerStartLevel(int levelNum){
		triggerStartLevel = levelNum;
	}
	
	
	
	
	
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bw.Write ((int)state);
		bw.Write (currentLevel);
		bw.Write ((int)currentCamSelect);
		bw.Write (levelStartMsgTime);
		bw.Write (triggerStartLevel);
		bw.Write (showPointer);
		bw.Write (showHint);
		bw.Write (showTimeHint);
		bw.Write (hasShownHint);
		bw.Write (lastGoalTime);
		
	}
	
	
	public void Deserialise(BinaryReader br){
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				state = (GameModeState)br.ReadInt32 ();
				currentLevel = br.ReadInt32 ();
				currentCamSelect = (CameraChoice)br.ReadInt32();
				levelStartMsgTime = br.ReadSingle();
				triggerStartLevel = br.ReadInt32 ();
				showPointer = br.ReadBoolean();
				showHint = br.ReadBoolean ();
				showTimeHint = br.ReadBoolean();
				hasShownHint = br.ReadBoolean();
				lastGoalTime = br.ReadSingle();
				SelectCamera(currentCamSelect);
				
				break;
			}
		}
	}

	// Use this for initialization
	public void Initialise () {
		backStory.SetActive(false);
		SelectCamera(CameraChoice.kGameCam);
		tutorialText.SetActive(true);
		if (state == GameModeState.kSplashScreen){
			state = GameModeState.kPreMainMenu;
		}
		else{
			OnActivateSplash();
		}

	}
	
	
	void OnLeaveSplashScreen(){
		
	}
	
	
	
		
	void OnActivateSplash(){
//		if (splashScreen.transform.FindChild("InputField").FindChild("Text").GetComponent<Text>().text != ""){
//			splashScreen.transform.FindChild("InputField").FindChild("Placeholder").GetComponent<Text>().enabled = false;
//		}
	}
	
	public int GetNumMainMenuButtons(){
		return 	AVOWLevelEditor.singleton.GetNumPlaybackLevels() + 2;
	}
	
	public int GetMinMainMenuButton(){
		return -1;
	}
	
	// Use this for initialization
	void Awake () {
	
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
		screenSpaceUI.SetActive(true);
		
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	
	void CreateHintText(){
		AVOWTutorialText.singleton.ClearText();
		AVOWTutorialText.singleton.AddPause(1);
		if (showTimeHint){
			AVOWTutorialText.singleton.AddText("If you get stuck, there is a hint button on the top right corner of the screen.");
			lastGoalTime = 10000f;
			return;
		}
		AVOWTutorialText.singleton.AddText(AVOWLevelEditor.singleton.GetHint());
	}
	
	public bool IsPlayingLevel(){
		return state == GameModeState.kPlayStage && currentLevel >= -1;
	}
	
	public void ShowPointer(){
		showPointer = true;
	}
	
	public void HidePointer(){
		showPointer = false;;
	}
	
	public void TriggerHint(){
		AVOWUI.singleton.PlayPing();
		showHint = true;
		showTimeHint = false;
		hasShownHint = true;
		CreateHintText();
	}
	
	public void TriggerTimerHint(){
		showHint = true;
		showTimeHint = true;
		CreateHintText();
	}
	
	public void OnGoalComplete(){
		showHint = false;
		showTimeHint = false;
		lastGoalTime = AVOWUpdateManager.singleton.GetGameTime();
		if (currentLevel != 4){
			AVOWTutorialText.singleton.ClearText();
		}
	}
	
	public void GameUpdate(){
		if (triggerStartLevel >= kTutorialIndex){
			StartLevel(triggerStartLevel);
			triggerStartLevel = kTutorialIndex-1;
		
		}
		if (currentLevel > 0 && currentLevel != 4 && !hasShownHint && AVOWUpdateManager.singleton.GetGameTime() >  lastGoalTime + waitForTimedHintDuration && AVOWObjectiveManager.singleton.currentGoalIndex < 4){
			TriggerTimerHint();
		}
		
		if (!IsPlayingLevel()){
			ProgressPanel.singleton.SetGoals(0, 0);
		}
		
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
		
		

		switch (state){
			case GameModeState.kPreMainMenu:{
				AVOWLevelEditor.singleton.LoadAllForPlayback();
				state = GameModeState.kMainMenu;
				break;
			}
			case GameModeState.kMainMenu:{
				AVOWUpdateManager.singleton.ResetGameTime();
				
				break;
			}

			case GameModeState.kStageComplete0:{
				// Get rid of all our resistance squares
				foreach (GameObject go in AVOWGraph.singleton.allComponents){
					AVOWComponent component = go.GetComponent<AVOWComponent>();
					if (component.type == AVOWComponent.Type.kLoad){
						component.Kill (89);
					}
				}
				
				AVOWConfig.singleton.DisplaySidePanel(false);
				
				
				pusher.GetComponent<AVOWPusher>().disableMovement = true;
				AVOWCamControl.singleton.disableMovement = true;
				if (AVOWGraph.singleton.allComponents.Count == 1){
					state = GameModeState.kStageComplete1;
				}
				AVOWConfig.singleton.tutDisableConnections = true;
				break;	
			}
			case GameModeState.kStageComplete4:{
				break;	
			}
			default:{
	//			AVOWUI.singleton.enableToolUpdate = true;
	//			greenBackground.GetComponent<AVOWGreenBackground>().MakeSmall();
	//			pusher.GetComponent<AVOWPusher>().disableMovement = false;
	//			AVOWCamControl.singleton.disableMovement = false;
	//			scenery.transform.localScale = new Vector3(1, 1, 1);
				break;
			}
		}
	}
	
	// Update is called once per frame
	public void RenderUpdate () {
	
		// Hints
		hintButton.SetActive(currentLevel > 0 && currentLevel !=4 && state == GameModeState.kPlayStage);
		if (currentLevel > 0 && state == GameModeState.kPlayStage && currentLevel != 4){
			AVOWConfig.singleton.DisplayBottomPanel(showHint);
		}
		
		
		
	
		//dlgPanel.SetActive(state == GameModeState.kGameOver || state == GameModeState.kStageComplete3);
		mainMenuPanel.SetActive(state == GameModeState.kMainMenu);
		if (state == GameModeState.kStageComplete3 || state == GameModeState.kStageComplete4){
			if (AVOWObjectiveManager.singleton.IsOnLastLevel()){
				gameCompleteDlg.SetActive (true);
			}
			else{
				levelCompleteDlg.SetActive (true);
			}
		}
		else{
			levelCompleteDlg.SetActive (false);
			gameCompleteDlg.SetActive (false);
		}
//		sidePanel.transform.FindChild("ExcludeToggle").gameObject.SetActive(AVOWConfig.singleton.levelExcludeEdit);
//		sidePanel.transform.FindChild("ExcludeToggle").GetComponent<Text>().text = AVOWObjectiveManager.singleton.IsCurrentGoalExcluded() ? "Include Goal" : "Exclude Goal";
		
		splashScreen.SetActive(state == GameModeState.kSplashScreen);
		
		HandleLevelStartMsg();
		
		switch (state){
			case GameModeState.kMainMenu:{
				whitePanel.SetActive(false);
				break;
				
			}
			case GameModeState.kPlayStage:{
				whitePanel.SetActive(false);
				levelDisplay.transform.FindChild("ButtonPanel").FindChild("LevelText").GetComponent<Text>().text = GetLevelName(currentLevel, true);
				break;
				
			}			
			case GameModeState.kStageComplete1:{
				// Get rid of all our resistance squares
				float scaleVal = scenery.transform.localScale.x;
				scenery.transform.localScale = new Vector3(scaleVal * 1.02f, scaleVal * 1.02f, 1);
				if (scaleVal > 2f){
					state = GameModeState.kStageComplete2;
					whitePanel.SetActive(true);
					whitePanel.GetComponent<Image>().color = new Color(1, 1, 1, 0);
				}
				break;	
			}
			case GameModeState.kStageComplete2:{
				float scaleVal = scenery.transform.localScale.x;
				scenery.transform.localScale = new Vector3(scaleVal * 1.02f, scaleVal * 1.02f, 1);
				Color col = whitePanel.GetComponent<Image>().color;
				col.a += 0.01f;
				whitePanel.GetComponent<Image>().color = col;
				levelCompleteDlg.transform.FindChild("LevelComplete").GetComponent<Text>().text = "Congratulations!\n" + GetLevelName(currentLevel) + " complete";
				levelCompleteDlg.transform.FindChild("Continue").GetComponent<Text>().text = "Continue to " + GetLevelName(currentLevel + 1);
				levelCompleteDlg.transform.FindChild("Continue").GetComponent<AVOWMenuButton>().levelNum = currentLevel + 1;
				if (col.a >= 1){
					state = GameModeState.kStageComplete3;
				}
				
				//levelCompleteDlg.
				break;	
			}
			case GameModeState.kStageComplete3:{
				Color col = whitePanel.GetComponent<Image>().color;
				col.a -= 0.01f;
				whitePanel.GetComponent<Image>().color = col;
				if (col.a <= 0){
					state = GameModeState.kStageComplete4;
				}
				break;	
			}
		}
	



	}
	
	public void TriggerLevelStartMessage(){
		levelStartMsgTime = AVOWUpdateManager.singleton.GetGameTime();
	
	}
	
	public void ClearLevelStartMessage(){
		levelStartMsgTime = -100f;
	}
	
	
	public void PlayFree(){
		AVOWTutorialText.singleton.activated = false;
		SelectCamera(CameraChoice.kGameCam);
		sidePanel.SetActive(true);
		backStory.SetActive(false);
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(true);
		AVOWObjectiveManager.singleton.InitialiseLimitsOnly(7);
		
		RestartFreePlayGame();
	}
	
	public void PlayTutorial(){
		AVOWTutorialText.singleton.activated = true;
		SelectCamera(CameraChoice.kGameCam);
		AVOWConfig.singleton.DisplayBottomPanel(true);
		AVOWConfig.singleton.DisplaySidePanel(true);
		backStory.SetActive(false);
		AVOWObjectiveManager.singleton.InitialiseLimitsOnly(5);
		AVOWTutorialManager.singleton.StartTutorial();
		
		RestartTutorialGame();
	}
	
	public void PlayBackStory(){
		scenery.SetActive(false);
		
		AVOWTutorialText.singleton.activated = true;
		AVOWConfig.singleton.DisplayBottomPanel(true);
		AVOWConfig.singleton.DisplaySidePanel(false);
		SelectCamera(CameraChoice.kBackStoryCam);
//		AVOWBackStoryCutscene.singleton.StartBackStory();
		backStory.SetActive(true);
		
		RestartFreePlayGame();
	}
	

	
	void SelectCamera(CameraChoice cam){
		currentCamSelect = cam;
		// First set them all inactive
	//	BackStoryCamera.singleton.gameObject.SetActive(false);
		AVOWCamControl.singleton.gameObject.SetActive(false);
		
	//	BackStoryCamera.singleton.gameObject.SetActive(false);
		switch (cam){
		 	case CameraChoice.kGameCam:{
				AVOWCamControl.singleton.gameObject.SetActive(true);
				break;
		 	}
		 	case CameraChoice.kBackStoryCam:{
			//	BackStoryCamera.singleton.gameObject.SetActive(true);
				break;
		 	}
		}
	}
	
	
	// THe level complete routine rather messes up the scenery - this puts it all back
	public void ResetScenery(){
		scenery.SetActive(true);
		scenery.transform.localScale = new Vector3(1, 1, 1);	
		whitePanel.SetActive(false);
		levelCompleteDlg.SetActive(false);
		pusher.GetComponent<AVOWPusher>().disableMovement = false;
		AVOWCamControl.singleton.disableMovement = false;
		AVOWConfig.singleton.tutDisableConnections = false;
		greenBackground.GetComponent<AVOWGreenBackground>().MakeSmall();
	}
	
	void RestartTutorialGame(){
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWUI.singleton.Restart();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();
		AVOWObjectiveManager.singleton.InitialiseBlankBoard();
		
		
		state = GameModeState.kPlayStage;
	}
		
	void RestartFreePlayGame(){
		AVOWConfig.singleton.showObjectives = false;
		AVOWGraph.singleton.ClearCircuit();
		AVOWUI.singleton.Restart();
		AVOWBattery.singleton.ResetBattery();
		AVOWBattery.singleton.FreezeBattery();
		AVOWObjectiveManager.singleton.InitialiseBlankBoard();
		
		state = GameModeState.kPlayStage;
	}



	
	void StartLevel(int levelNum){
		ResetScenery();
		
		currentLevel = levelNum;
		hasShownHint = false;
		lastGoalTime = 0;
		showHint = false;
		
		AVOWUpdateManager.singleton.ResetGameTime();

		/// Sert up recording
		
		if (currentLevel > 0){
			AVOWObjectiveManager.singleton.InitialiseLevel(levelNum);
			AVOWGameModes.singleton.TriggerLevelStartMessage();

			
			SelectCamera(CameraChoice.kGameCam);
			RestartNormalGame();
			if (currentLevel == 4){
				AVOWObjectiveManager.singleton.EnableManualTrigger(true);
				AVOWConfig.singleton.DisplayBottomPanel(true);
				Explanation.singleton.state = Explanation.State.kIntro;
			}
		}
		else{
			switch (currentLevel){

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
	
//	void StartNextLevel(){
//		currentLevel++;
//		TriggerStartLevel (currentLevel);
//		
//	}
	
	void HandleLevelStartMsg(){
		if (AVOWUpdateManager.singleton.GetGameTime() > levelStartMsgTime + levelStartMsgDuration || AVOWUpdateManager.singleton.GetGameTime() < levelStartMsgTime){
			levelStartMsg.SetActive(false);
		}
		else{
			levelStartMsg.SetActive(true);
			// work out the alpha value of the text
			float alpha = 1;
			// If fade in
			if (AVOWUpdateManager.singleton.GetGameTime() < levelStartMsgTime + levelStartFadeInDuration){
				alpha = (AVOWUpdateManager.singleton.GetGameTime() - levelStartMsgTime) / levelStartFadeInDuration;
			}
			// If fade out
			else if (AVOWUpdateManager.singleton.GetGameTime() > levelStartMsgTime + levelStartMsgDuration - levelStartFadeOutDuration){
				alpha = (levelStartMsgTime + levelStartMsgDuration - AVOWUpdateManager.singleton.GetGameTime()) / levelStartFadeOutDuration;
			}
			// Set the title color
			Color titleCol = levelStartMsg.transform.FindChild ("LevelTitle").GetComponent<Text>().color;
			titleCol.a = alpha;
			levelStartMsg.transform.FindChild ("LevelTitle").GetComponent<Text>().color = titleCol;

			// Set the message color			
			Color msgCol = levelStartMsg.transform.FindChild ("Message").GetComponent<Text>().color;
			msgCol.a = alpha;
			levelStartMsg.transform.FindChild ("Message").GetComponent<Text>().color = msgCol;
			levelStartMsg.transform.FindChild ("LevelTitle").GetComponent<Text>().text = GetLevelName(currentLevel, true);
			
		}
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
	
	public void ToggleExcludeCurrentGoal(){
		AVOWObjectiveManager.singleton.ToggleExcludeCurrentGoal();
	}
	
	
	public void GoToMain(){
		Explanation.singleton.OnLeave();
		AVOWUI.singleton.PlayPing ();
		backStory.SetActive(false);
		AVOWLevelEditor.singleton.LoadAllForPlayback();
//		AVOWBackStoryCutscene.singleton.StopBackStory();
		AVOWTutorialManager.singleton.StopTutorial();
		AVOWConfig.singleton.DisplayBottomPanel(false);
		AVOWConfig.singleton.DisplaySidePanel(false);
		AVOWCircuitCreator.singleton.Deinitialise();
		AVOWObjectiveManager.singleton.StopObjectives();
		SelectCamera(CameraChoice.kGameCam);
		ClearLevelStartMessage();
		if (state == GameModeState.kSplashScreen){
			OnLeaveSplashScreen();
		}
		AVOWObjectiveManager.singleton.EnableManualTrigger(false);
		state = GameModeState.kPreMainMenu;
	}
	
//	public void GoToMainFromComplete(){
//		PlayFree();
//		state = GameModeState.kMainMenu;
//	}

	public string GetLevelName(int index){
		return GetLevelName(index, false);
	}
	
	public string GetLevelName(int index, bool withLineBreak){
		// Check if it is the last one

		if (index > 0 && index <= AVOWLevelEditor.singleton.GetNumPlaybackLevels()){
			if (!withLineBreak){
				return ((index < 4) ? ("Level " + index.ToString() + ": ") : "") + AVOWLevelEditor.singleton.GetPlaybackLevelName(index-1);
			}
			else{
				return ((index < 4) ? ("Level " + index.ToString() + ":\n") : "") + AVOWLevelEditor.singleton.GetPlaybackLevelName(index-1);
			}
			
		}

		switch (index){
			case kFreePlayIndex:{
				return "Free play";
			}

			case kTutorialIndex:{
				return "Tutorial";
			}
		}
		return "Unkown level";
	
	}
}
