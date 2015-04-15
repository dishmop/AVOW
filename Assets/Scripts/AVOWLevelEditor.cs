using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWLevelEditor : MonoBehaviour {
	public static AVOWLevelEditor singleton = null;
	
	public bool enableEditor;
	public int levelNum;
	public int totalNumGoals;
	public int currentGoal;
	public int maxNumResistors;
	public string levelName = "";
	[Multiline]
	public string levelHint = "";
	const int maxNumTargets = 100;
	List<AVOWCircuitTarget> targets;
	
	// For level playback (just easier to keep this seperate)
	List<List<AVOWCircuitTarget>> playbackTargets;
	List<List<GoalType>> playbackGoalTypes;
	List<string> levelNames;
	List<string> levelHints;
	
	string 		filename = "EditorLevel";
	
	
	const int		kLoadSaveVersion = 2;	
	int				playbackLevelNum = -1;
	
	
	public enum GoalType{
		kReadyStacked,
		kStackedThenRowed,
		kRowed,
		kRowedThenMissing,
		kMissing
	}
	public GoalType 	currentGoalType;
	List<GoalType> goalTypes;
	
	public void LoadAllForPlayback(){
		playbackTargets = new List<List<AVOWCircuitTarget>>();
		playbackGoalTypes = new List<List<GoalType>>();
		levelNames = new List<string>();
		levelHints = new List<string>();
		levelNum = 0;
		bool ok = true;
		while (ok){
			ok = LoadLevelInternal();
			if (ok){
				List<AVOWCircuitTarget> newTargets = new List<AVOWCircuitTarget>();
				List<GoalType> newGoalTypes = new List<GoalType> ();
				playbackTargets.Add(newTargets);
				playbackGoalTypes.Add (newGoalTypes);
				for (int i = 0; i < totalNumGoals; ++i){
					newTargets.Add(targets[i]);
					newGoalTypes.Add (goalTypes[i]);
				}
				levelNames.Add (levelName);
				levelHints.Add (levelHint);
			}
			levelNum++;
			
		}
		levelNum = 0;
	}
	
	public void InitialisePlayback(int levelNum){
		playbackLevelNum = levelNum;
	}
	
	
	
	public List<AVOWCircuitTarget> GetCurrentGoals(){
		if (playbackLevelNum < 0){
			return targets;
		}
		else{
			return playbackTargets[playbackLevelNum];
		}
	}
	
	public List<GoalType> GetCurrentGoalTypes(){
		if (playbackLevelNum < 0){
			return goalTypes;
		}
		else{
			return playbackGoalTypes[playbackLevelNum];
		}
	}
	
	
	public string CreateFilename(){
		return filename + "_" + levelNum;
	}
	
	// We saving using the standard file system
	string BuildFullPath(){
		return Application.dataPath + "/Resources/EditorLevels/" + CreateFilename() + ".bytes";
		
	}
	
	// We load using the resources
	string BuildResourcePath(){
		return "EditorLevels/" + CreateFilename();
	}
	
	
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bw.Write (totalNumGoals);
		bw.Write (maxNumResistors);
		bw.Write (levelName);
		bw.Write (levelHint);
		
		for (int i = 0; i < totalNumGoals; ++i){
			targets[i].Serialise(bw);
		}
		for (int i = 0; i < totalNumGoals; ++i){
			bw.Write ((int)goalTypes[i]);
		}

		
	}
	
	
	public void Deserialise(BinaryReader br){
		ClearLevel();
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				totalNumGoals = br.ReadInt32 ();
				maxNumResistors = br.ReadInt32 ();
				levelName = br.ReadString();
				levelHint = br.ReadString();
				for (int i = 0; i < totalNumGoals; ++i){
					targets[i].Deserialise(br);
				}
				for (int i = 0; i < totalNumGoals; ++i){
					goalTypes[i] = (GoalType)br.ReadInt32 ();
				}	
				
				break;
			}
			case 1:{
				totalNumGoals = br.ReadInt32 ();
				maxNumResistors = br.ReadInt32 ();
				levelName = br.ReadString();
				
				
				levelHint = "No hint saved in this level";
				for (int i = 0; i < totalNumGoals; ++i){
					targets[i].Deserialise(br);
				}
				for (int i = 0; i < totalNumGoals; ++i){
					goalTypes[i] = (GoalType)br.ReadInt32 ();
				}	
				
				break;
			}
		}
	}
	public string GetHint(){
		return levelHints[playbackLevelNum];
	}
	
	public void DisplayGoal(){
		AVOWGraph.singleton.ConstructFromTarget(targets[currentGoal]);
		currentGoalType = goalTypes[currentGoal];
		AVOWObjectiveManager.singleton.InitialiseLevelFromEditor(currentGoal);
	}

	public void StoreGoal(){
		targets[currentGoal] = new AVOWCircuitTarget(AVOWGraph.singleton);
		goalTypes[currentGoal] = currentGoalType;
		DisplayGoal();
	}
	
	public void LoadLevel(){
	if (LoadLevelInternal()){
			DisplayGoal();
		}
	}
	
	public bool LoadLevelInternal(){
		
		TextAsset asset = Resources.Load(BuildResourcePath ()) as TextAsset;
		if (asset != null){
			Debug.Log ("Loading asset");
			Stream s = new MemoryStream(asset.bytes);
			Deserialise(new BinaryReader(s));
			Resources.UnloadAsset(asset);
			return true;
		}	
		return false;
	}
	
	
	public void SaveLevel(){
		#if UNITY_EDITOR		
		FileStream file = File.Create(BuildFullPath());
		
		Serialise(new BinaryWriter(file));
		
		
		file.Close();
		
		// Ensure the assets are all realoaded and the cache cleared.
		UnityEditor.AssetDatabase.Refresh();
		#endif
	}
	
	public void CopyPrevious(){
		if (currentGoal == 0) return;
		
		targets[currentGoal] = new AVOWCircuitTarget(targets[currentGoal-1]);
		goalTypes[currentGoal] = goalTypes[currentGoal-1];
		DisplayGoal();
	
	}
	
	public void IncGoal(){
		currentGoal = Mathf.Min (currentGoal+1, totalNumGoals-1);
		DisplayGoal();
	}
	
	public void DecGoal(){
		currentGoal = Mathf.Max (currentGoal-1, 0);
		DisplayGoal();
	}
	
	public void ClearLevel(){
		targets = new List<AVOWCircuitTarget>();
		for (int i = 0; i < maxNumTargets; ++i){
			targets.Add(new AVOWCircuitTarget());
		}
		
		goalTypes = new List<GoalType>();
		for (int i = 0; i < maxNumTargets; ++i){
			goalTypes.Add(GoalType.kReadyStacked);
		}
	}
	
	public int GetNumPlaybackLevels(){
		return playbackTargets.Count;
	}
	
	public string GetPlaybackLevelName(int levelNum){
		if (levelNum >= levelNames.Count){
			Debug.LogError("Error: GetPlaybackLevelName");
		}
		return levelNames[levelNum];
	}
	
	// Use this for initialization
	void Start () {
		ClearLevel();	
	}
	
	// Update is called once per frame
	void Update () {
	
		/// Logic is a latch which ust be reset 
		if (AVOWGameModes.singleton.state != AVOWGameModes.GameModeState.kPlayStage) return;
		
		if (enableEditor && Input.GetMouseButtonDown(0)){
		
			Vector3 mousePos = Input.mousePosition;
			if (mousePos.x < Screen.width * AVOWConfig.singleton.GetSidePanelFrac()){
				return;
			}
			
			mousePos.z = 0;
			Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
			
			
			int targetSquareIndex = AVOWObjectiveManager.singleton.InsideTargetTest(mouseWorldPos);
			Debug.Log ("targetSquareIndex = " + targetSquareIndex.ToString());
			if (targetSquareIndex >= 0){
				targets[currentGoal].HideComponent(targetSquareIndex);
				DisplayGoal();
			}
			else if (targetSquareIndex == -1){
				targets[currentGoal].UnhideAllComponents();
				DisplayGoal();
			}
			
			
		}
	
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
}
