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
	const int maxNumTargets = 100;
	List<AVOWCircuitTarget> targets;
	
	// For level playback (just easier to keep this seperate)
	List<AVOWCircuitTarget>[] playbackTargets;
	
	public string 		filename = "EditorLevel";
	
	
	const int		kLoadSaveVersion = 1;	
	
	
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
		int i = 0;
		bool ok = true;
		while (ok){
			
		}
	}
	
	public List<AVOWCircuitTarget> GetCurrentGoal(){
		return targets;
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
	
	public void DisplayGoal(){
		AVOWGraph.singleton.ConstructFromTarget(targets[currentGoal]);
		currentGoalType = goalTypes[currentGoal];
		AVOWObjectiveManager.singleton.InitialiseLevelFromEditor(currentGoal, currentGoalType);
	}

	public void StoreGoal(){
		targets[currentGoal] = new AVOWCircuitTarget(AVOWGraph.singleton);
		goalTypes[currentGoal] = currentGoalType;
		DisplayGoal();
	}
	
	public void LoadLevel(){
		LoadLevelInternal();
	}
	
	public bool LoadLevelInternal(){
		
		TextAsset asset = Resources.Load(BuildResourcePath ()) as TextAsset;
		if (asset != null){
			Debug.Log ("Loading asset");
			Stream s = new MemoryStream(asset.bytes);
			Deserialise(new BinaryReader(s));
			Resources.UnloadAsset(asset);
			DisplayGoal();
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
	
	// Use this for initialization
	void Start () {
		ClearLevel();
	
	}
	
	// Update is called once per frame
	void Update () {
	
		/// Logic is a latch which ust be reset 
		
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
