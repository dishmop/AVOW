using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class AVOWObjectiveManager : MonoBehaviour {

	public static AVOWObjectiveManager singleton = null;
	public GameObject objectiveBoardPrefab;
	public float boardSpeed;
	public float unstackWaitDuration = 0.5f;
	public float levelStartPauseDuration;
	
	const int		kLoadSaveVersion = 1;	
	
	
	public string filename = "ExcludedGoals";
	bool[][]	excludedGoals;
	
	bool firstUpdate = true;
	
	
	// Front and back boards
	GameObject[] boards = new GameObject[2];
	int frontIndex = 0;
	int backIndex = 1;
	
	float waitTime;
	
	int numBoardsToUnstack;
	bool hasMovedToRow;
	

	AVOWObjectiveBoard.LayoutMode layoutMode = AVOWObjectiveBoard.LayoutMode.kRow;
	
	
	float backBoardDepth = 0.5f;
	float frontBoardDepth = 0;
	float boardDepthSpeed = 1f;
	
	int resistorLimit = -1;
	int currentGoalIndex = -1;
	int currentLevel = -1;
	
	float[] optValues = new float[13];
	bool valuesHaveChanged;
		
	
	
	// Not serialised
	bool initialisedLimitsOnly;
	
	enum State{
		kNone,
		kPauseOnLevelStart,
		kWaitForCircuitCreator,
		kBuildBackBoard,
		kSwapBoards0,
		kSwapBoards1,
		kPlay,
		kGoalComplete0,
		kGoalComplete1,
		kLevelComplete0,
		kLevelComplete1,
	};
	
	State state = State.kNone;
	
	// This must be reset at the beginning of each GameUpdate frame
	public void ResetOptFlags(){
		if (boards[0] != null){
			boards[0].GetComponent<AVOWObjectiveBoard>().ResetOptFlags();
			boards[1].GetComponent<AVOWObjectiveBoard>().ResetOptFlags();
		}
		valuesHaveChanged = false;
	}
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		
		bw.Write (boards[0] != null);
		if (boards[0] != null){
			boards[0].GetComponent<AVOWObjectiveBoard>().Serialise(bw);
			boards[1].GetComponent<AVOWObjectiveBoard>().Serialise(bw);
		}
		
		bw.Write (valuesHaveChanged);
		if (valuesHaveChanged){
			bw.Write (firstUpdate);
			bw.Write (frontIndex);
			bw.Write (backIndex);
			bw.Write (waitTime);
			bw.Write (numBoardsToUnstack);
			bw.Write (hasMovedToRow);
			bw.Write ((int)layoutMode);
			bw.Write (backBoardDepth);
			bw.Write (frontBoardDepth);
			
			bw.Write (resistorLimit);
			bw.Write (currentGoalIndex);
		   	bw.Write (currentLevel);
			
			bw.Write ((int)state);
		}
	}
	
	public void Deserialise(BinaryReader br){
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				bool hasBoards = br.ReadBoolean();
				if (hasBoards && boards[0] == null){
					ConstructBoards();
				}
				else if (!hasBoards && boards[0] != null){
					GameObject.Destroy (boards[0]);
					GameObject.Destroy (boards[1]);
					boards[0] = null;
					boards[1] = null;
				}
				
				if (boards[0] != null){
					boards[0].GetComponent<AVOWObjectiveBoard>().Deserialise(br);
					boards[1].GetComponent<AVOWObjectiveBoard>().Deserialise(br);
				}
				
				valuesHaveChanged = 	br.ReadBoolean();
				if (valuesHaveChanged){
					firstUpdate = 			br.ReadBoolean();
					frontIndex = 			br.ReadInt32();
					backIndex = 			br.ReadInt32();
					waitTime = 				br.ReadSingle();
					numBoardsToUnstack = 	br.ReadInt32();
					hasMovedToRow = 		br.ReadBoolean();
					layoutMode = 			(AVOWObjectiveBoard.LayoutMode)br.ReadInt32();
					backBoardDepth = 		br.ReadSingle();
					frontBoardDepth = 		br.ReadSingle();		
					resistorLimit = 		br.ReadInt32();
					currentGoalIndex = 		br.ReadInt32();
					currentLevel = 			br.ReadInt32();
					state = 				(State)br.ReadInt32 ();
				}

				break;
			}
		}
	}
	
	void ResetOptValues(){
		int i = 0;
		optValues[i++] = Convert.ToSingle(firstUpdate);
		optValues[i++] = Convert.ToSingle(frontIndex);
		optValues[i++] = Convert.ToSingle(backIndex);
		optValues[i++] = Convert.ToSingle(waitTime);
		optValues[i++] = Convert.ToSingle(numBoardsToUnstack);
		optValues[i++] = Convert.ToSingle(hasMovedToRow);
		optValues[i++] = Convert.ToSingle(layoutMode);
		optValues[i++] = Convert.ToSingle(backBoardDepth);
		optValues[i++] = Convert.ToSingle(frontBoardDepth);
		optValues[i++] = Convert.ToSingle(resistorLimit);
		optValues[i++] = Convert.ToSingle(currentGoalIndex);
		optValues[i++] = Convert.ToSingle(currentLevel);
		optValues[i++] = Convert.ToSingle(state);

			
	}
	
	
	void TestIfValuesHaveChanged(){
		bool diff = false;
		int i = 0;
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(firstUpdate)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(frontIndex)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(backIndex)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(waitTime)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(numBoardsToUnstack)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(hasMovedToRow)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(layoutMode)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(backBoardDepth)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(frontBoardDepth)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(resistorLimit)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(currentGoalIndex)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(currentLevel)));
		diff = diff || (!MathUtils.FP.Feq (optValues[i++], Convert.ToSingle(state)));
		
		
		//also want to force the firs ttime through to update everytthing
		if (diff){
			valuesHaveChanged = true;
			ResetOptValues();
		}
		
	}
	
	
	// reutrn -1 if no limit
	public int GetResistorLimit(){
		return resistorLimit;
	}
		
	
	public float GetMinX(){
	
		if (boards[frontIndex] == null) return transform.position.x;
		
		return transform.position.x -  Mathf.Max (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().GetWidth());
	}
	
	public void InitialiseLimitsOnly(int limit){
		resistorLimit = limit;
		initialisedLimitsOnly = true;
	}
		

	// Levels start at level1 and go up to this number not inclusive
	public int GetMaxLevelNum(){
		return AVOWGameModes.singleton.GetNumMainMenuButtons() + AVOWGameModes.singleton.GetMinMainMenuButton();
	}
	
	public bool IsCurrentGoalExcluded(){
		if (currentLevel < 0 || currentGoalIndex < 0 || currentGoalIndex >= GetGoalTargets().Count) return false;
		return excludedGoals[currentLevel][currentGoalIndex];
	}
	
	public void ToggleExcludeCurrentGoal(){
		excludedGoals[currentLevel][currentGoalIndex] = !excludedGoals[currentLevel][currentGoalIndex];
		SaveExcludedLevels();
	}
	
	public void InitialiseLevel(int level){
		currentLevel = level;
		initialisedLimitsOnly = false;
		numBoardsToUnstack = 0;
		switch (level){
			case 1:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kStack;
				resistorLimit = 2;
				break;
			}
			case 2:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kStack;
				resistorLimit = 3;
				break;
			}
			case 3:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kStack;
				resistorLimit = 4;
				break;
			}
			case 4:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kRow;
				numBoardsToUnstack = 4;
				resistorLimit = 4;
				break;
			}
			case 5:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kRow;
				resistorLimit = 5;
				break;
			}
			case 6:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kGappedRow;
				numBoardsToUnstack = 3;
				resistorLimit = 3;
				break;
			}
			case 7:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kGappedRow;
				numBoardsToUnstack = 3;
				resistorLimit = 4;
				break;
			}
			case 8:{
				layoutMode = AVOWObjectiveBoard.LayoutMode.kGappedRow;
				resistorLimit = 5;
				break;
			}
		}
		
		InitialiseBlankBoard();
		if (layoutMode !=  AVOWObjectiveBoard.LayoutMode.kGappedRow){
			AVOWCircuitCreator.singleton.Initialise(resistorLimit);
		}
		else{
			AVOWCircuitSubsetter.singleton.Initialise(resistorLimit);
		}
		
		state = State.kPauseOnLevelStart;
		waitTime = Time.time + levelStartPauseDuration;
		
	

	}
	
	public void InitialiseBlankBoard(){
		boards[frontIndex].GetComponent<AVOWObjectiveBoard>().ConstructBlankBoard();
		boards[backIndex].GetComponent<AVOWObjectiveBoard>().ConstructBlankBoard();
	}
	
	// Call to stop the game
	public void StopObjectives(){
		state = State.kNone;
		InitialiseBlankBoard();
	}
	

	public void DeinitialiseLevel(){
		AVOWCircuitCreator.singleton.Deinitialise();
		resistorLimit = -1;
		state = State.kNone;
		currentGoalIndex = -1;
		currentLevel = -1;
	}
	
	public bool HasResistorLimit(){
		return resistorLimit >= 0;
	}
	
	public int GetNumFreeResistors(){
		return (GetResistorLimit() - AVOWGraph.singleton.GetNumConfirmedLoads());
	}
	
	public bool IsOnLastLevel(){
		return currentLevel == GetMaxLevelNum() - 1;
	}
	
	void ConstructBoards(){
		
		boards[0] = GameObject.Instantiate(objectiveBoardPrefab);
		boards[1] = GameObject.Instantiate(objectiveBoardPrefab);
		
		
		boards[0].transform.parent = transform;
		boards[1].transform.parent = transform;
		
		boards[0].transform.localPosition = Vector3.zero;
		boards[1].transform.localPosition = Vector3.zero;	
	}
	
	// Use this for initialization
	public void Initialise () {
		ConstructBoards();
		
		ForceBoardDepths();
		
	}
	
	// We saving using the standard file system
	string BuildFullPath(){
		return Application.dataPath + "/Resources/RuntimeData/" + filename + ".bytes";
		
	}
	
	
	// We load using the resources
	string BuildResourcePath(){
		return "RuntimeData/" + filename;
	}
	
	
	public void SerializeExcludedGoals(Stream stream){
		
		BinaryWriter bw = new BinaryWriter(stream);
		
		bw.Write (excludedGoals.Length);
		for (int i = 0; i < excludedGoals.Length; ++i){
			int numGoals = excludedGoals[i] == null ? 0 : excludedGoals[i].Length;
			bw.Write (numGoals);
			for (int j = 0; j < numGoals; ++j){
				bw.Write (excludedGoals[i][j]);
			}
		}
		
	}
	
	public void DeserializeExcludedGoals(Stream stream){
		BinaryReader br = new BinaryReader(stream);
		
		int numLevels = br.ReadInt32();
		for (int i = 0; i < numLevels; ++i){
			int numGoals =  br.ReadInt32();
			
			for (int j = 0; j < numGoals; ++j){
				bool exlude = br.ReadBoolean();
				if (i < excludedGoals.Length && j < excludedGoals[i].Length){
					excludedGoals[i][j] = exlude;
				}
			}
		}
	}
	
	void ConstructExcludedGoals(){
		
		excludedGoals = new bool[GetMaxLevelNum()][]; 
		
		// Create arrys of the correct suze
		for (int i = 1; i < GetMaxLevelNum(); ++i){
			InitialiseLevel(i);
			while (!AVOWCircuitCreator.singleton.IsReady()){
				AVOWCircuitCreator.singleton.GameUpdate ();
			}
			excludedGoals[i] = new bool[GetGoalTargets().Count];
		}
		DeinitialiseLevel();
	}


	
	public bool LoadExcludedGoals(){
		
		TextAsset asset = Resources.Load(BuildResourcePath ()) as TextAsset;
		if (asset != null){
			Debug.Log ("Loading Excluded goals asset");
			Stream s = new MemoryStream(asset.bytes);
			DeserializeExcludedGoals(s);
			Resources.UnloadAsset(asset);
			return true;
		}	
		return false;			
	}
	
	// Does the actual serialising
	public void SaveExcludedLevels(){
		#if UNITY_EDITOR		
		FileStream file = File.Create(BuildFullPath());
		
		SerializeExcludedGoals(file);
		
		
		file.Close();
		
		// Ensure the assets are all realoaded and the cache cleared.
		UnityEditor.AssetDatabase.Refresh();
		#endif
		
	}	
	
	void SwapBoards(){
		frontIndex = 1 - frontIndex;
		backIndex = 1 - backIndex;
	}
	
	void ForceBoardDepths(){
		Vector3 backPos = boards[backIndex].transform.localPosition;
		backPos.z = backBoardDepth;
		boards[backIndex].transform.localPosition = backPos;
		
		Vector3 frontPos = boards[frontIndex].transform.localPosition;
		frontPos.z = frontBoardDepth;
		boards[frontIndex].transform.localPosition = frontPos;
	}
	
	bool UpdateBoardDepths(){
		float moveDist = boardDepthSpeed * Time.deltaTime;
		
		Vector3 backPos = boards[backIndex].transform.localPosition;
		backPos.z += Mathf.Min(moveDist, backBoardDepth - backPos.z);
		boards[backIndex].transform.localPosition = backPos;
		
		Vector3 frontPos = boards[frontIndex].transform.localPosition;
		frontPos.z -= Mathf.Min(moveDist, frontPos.z - frontBoardDepth);
		boards[frontIndex].transform.localPosition = frontPos;
		
		return MathUtils.FP.Feq (frontPos.z - frontBoardDepth, 0);
	}
	
	List<AVOWCircuitTarget> GetGoalTargets(){
		if (layoutMode != AVOWObjectiveBoard.LayoutMode.kGappedRow){
			return AVOWCircuitCreator.singleton.GetResults();
		}
		else{
			return AVOWCircuitSubsetter.singleton.GetResults();
		}
	}
	
	
	public void GameUpdate(){
		// We need to do this because when we stop playback we can be in some kind of state which we no longer want to be in
		if (initialisedLimitsOnly){
			state = State.kNone;
		}
		
		if (firstUpdate){
			ConstructExcludedGoals();
			LoadExcludedGoals();
			firstUpdate  = false;
		}
		
		switch (state){
			case State.kPauseOnLevelStart:{
				if (AVOWCircuitCreator.singleton.IsReady()){
					// Calc total numer of valid goals
					if (excludedGoals != null && excludedGoals[excludedGoals.Length-1] != null){
						int count = 0;
						for (int i = 0; i < GetGoalTargets().Count;  ++i){
							if (!excludedGoals[currentLevel][i]){
								++count;
							}
						}
						ProgressPanel.singleton.SetGoals(count, 0);
					}	
				}
				if (Time.time > waitTime){
					currentGoalIndex = FindNextValidGoal(-1);
					state = State.kWaitForCircuitCreator;
				}
				break;
			}
			case State.kWaitForCircuitCreator:{
				if (AVOWCircuitCreator.singleton.IsReady()){
					state = State.kBuildBackBoard;
				}
				break;
			}
			case State.kBuildBackBoard:{
				
				AVOWObjectiveBoard board = boards[backIndex].GetComponent<AVOWObjectiveBoard>();
				AVOWCircuitTarget target = GetGoalTargets()[currentGoalIndex];
				if (layoutMode == AVOWObjectiveBoard.LayoutMode.kStack){
					board.PrepareBoard(target, AVOWObjectiveBoard.LayoutMode.kStack);
				}
				if (layoutMode == AVOWObjectiveBoard.LayoutMode.kRow){
					if (numBoardsToUnstack > 0){
						board.PrepareBoard(target, AVOWObjectiveBoard.LayoutMode.kStack);
						hasMovedToRow = false;
					}
					else{
						board.PrepareBoard(target, AVOWObjectiveBoard.LayoutMode.kRow);
					}
				}
				if (layoutMode == AVOWObjectiveBoard.LayoutMode.kGappedRow){
					if (numBoardsToUnstack > 0){
						board.PrepareBoard(target, AVOWObjectiveBoard.LayoutMode.kRow);
						hasMovedToRow = false;
					}
					else{
						board.PrepareBoard(target, AVOWObjectiveBoard.LayoutMode.kGappedRow);
					}
				}				
				boards[backIndex].transform.localPosition = Vector3.zero;
				ForceBoardDepths();
				//				AVOWBattery.singleton.ResetBattery();
				
				AVOWTelemetry.singleton.WriteStartGoalEvent(currentGoalIndex);
				
				state = State.kSwapBoards0;
				boards[backIndex].GetComponent<AVOWObjectiveBoard>().TriggerWhoosh();
				break;
			}
			case State.kSwapBoards0:{
				Vector3 pos = boards[frontIndex].transform.position;
				pos.y -= boardSpeed * Time.deltaTime;
				boards[frontIndex].transform.position = pos;
				
				if (pos.y < -1){
					SwapBoards();
					boards[frontIndex].GetComponent<AVOWObjectiveBoard>().BrightenBoard(0.5f);
					state = State.kSwapBoards1;
				}
				break;
			}
			case State.kSwapBoards1:{
				Vector3 pos = boards[backIndex].transform.position;
				pos.y -= boardSpeed * Time.deltaTime;
				boards[backIndex].transform.position = pos;
				
				if (UpdateBoardDepths()){
					waitTime = Time.time + unstackWaitDuration;
					state = State.kPlay;
				}
				break;
			}
			case State.kPlay:{
				if (numBoardsToUnstack > 0 && Time.time > waitTime && !hasMovedToRow){
					if (layoutMode == AVOWObjectiveBoard.LayoutMode.kRow){
						boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToRow();
					}
					else{
						boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToGappedRow();
					}
					hasMovedToRow = true;
				}
				if (!AVOWGraph.singleton.HasHalfFinishedComponents()){
					AVOWCircuitTarget currentGraphAsTarget = new AVOWCircuitTarget(AVOWGraph.singleton);
					if (layoutMode != AVOWObjectiveBoard.LayoutMode.kGappedRow){
						if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().TestWidthsMatchWithGaps(currentGraphAsTarget)){
							boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToTarget(currentGraphAsTarget);
							state = State.kGoalComplete0;
						}
					}
					else{
						if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().TestWidthsMatchWithGaps(currentGraphAsTarget)){
							boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToTarget(currentGraphAsTarget);
							state = State.kGoalComplete0;
						}
					}
				}
				break;
			}
			
			case State.kGoalComplete0:{
				AVOWGameModes.singleton.OnGoalComplete();
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToComplete();
					numBoardsToUnstack--;
					state = State.kGoalComplete1;
				}
				break;
			}
			case State.kGoalComplete1:{
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					currentGoalIndex = FindNextValidGoal(currentGoalIndex);
					
					if (currentGoalIndex < GetGoalTargets().Count){
						state = State.kBuildBackBoard;
					}
					else{
						state = State.kLevelComplete0;
					}
				}
				break;
			}
			case State.kLevelComplete0:{
				Vector3 pos = boards[frontIndex].transform.position;
				pos.y -= boardSpeed * Time.deltaTime;
				boards[frontIndex].transform.position = pos;
				AVOWGameModes.singleton.PreStageComplete();
				
				
				if (pos.y < -1){
					AVOWGameModes.singleton.SetStageComplete();
					boards[0].GetComponent<AVOWObjectiveBoard>().DestroyBoard();
					boards[1].GetComponent<AVOWObjectiveBoard>().DestroyBoard();
					state = State.kLevelComplete1;
				}
				break;		
			}
			case State.kLevelComplete1:{
				
				break;		
			}
		}
		
		for (int i = 0; i < 2; ++i){
			boards[i].GetComponent<AVOWObjectiveBoard>().GameUpdate();
		}
		
		TestIfValuesHaveChanged();
		
	}

	
	
	// Update is called once per frame
	public void RenderUpdate () {
	
		for (int i = 0; i < 2; ++i){
			boards[i].GetComponent<AVOWObjectiveBoard>().RenderUpdate();
		}
		
	}
	
	int FindNextValidGoal(int currentIndex){
		do{
			currentIndex++;
		} while (currentIndex < GetGoalTargets().Count && excludedGoals[currentLevel][currentIndex] && !AVOWConfig.singleton.levelExcludeEdit);
		return currentIndex;
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
}
