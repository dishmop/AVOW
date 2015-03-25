using UnityEngine;
using System.Collections;

public class AVOWObjectiveManager : MonoBehaviour {

	public static AVOWObjectiveManager singleton = null;
	public GameObject objectiveBoardPrefab;
	public float boardSpeed;
	public float unstackWaitDuration = 0.5f;
	public float levelStartPauseDuration;
	
	
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
	int currentGoalIndex;
	
	
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
	
	
	// reutrn -1 if no limit
	public int GetResistorLimit(){
		return resistorLimit;
	}
	
	
	public float GetMaxX(){
	
		if (boards[frontIndex] == null) return transform.position.x + 1f;
		
		return transform.position.x +  Mathf.Max (1, boards[frontIndex].GetComponent<AVOWObjectiveBoard>().GetWidth());
	}
	
	public void InitialiseLimitsOnly(int limit){
		resistorLimit = limit;
	}
		
	
	public void InitialiseLevel(int level){
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
		AVOWGameModes.singleton.TriggerLevelStartMessage();
	}
	
	public void InitialiseBlankBoard(){
		currentGoalIndex = 0;
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
	}
	
	public bool HasResistorLimit(){
		return resistorLimit >= 0;
	}
	
	public int GetNumFreeResistors(){
		return (GetResistorLimit() - AVOWGraph.singleton.GetNumConfirmedLoads());
	}
		// Use this for initialization
	void Start () {
		boards[0] = GameObject.Instantiate(objectiveBoardPrefab);
		boards[1] = GameObject.Instantiate(objectiveBoardPrefab);
		
		
		boards[0].transform.parent = transform;
		boards[1].transform.parent = transform;
		
		boards[0].transform.localPosition = Vector3.zero;
		boards[1].transform.localPosition = Vector3.zero;
		
		ForceBoardDepths();
		
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
	
	
	// Update is called once per frame
	void Update () {
	
		switch (state){
			case State.kPauseOnLevelStart:{
				if (Time.time > waitTime) state = State.kWaitForCircuitCreator;
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
				AVOWCircuitTarget target =null;
				if (layoutMode != AVOWObjectiveBoard.LayoutMode.kGappedRow){
					target = AVOWCircuitCreator.singleton.GetResults()[currentGoalIndex];
				}
				else{
					target = AVOWCircuitSubsetter.singleton.GetResults()[currentGoalIndex];
				}
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
			
				state = State.kSwapBoards0;
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
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToComplete();
					numBoardsToUnstack--;
					state = State.kGoalComplete1;
				}
				break;
			}
			case State.kGoalComplete1:{
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					currentGoalIndex++;
					int maxGoalNum = (layoutMode == AVOWObjectiveBoard.LayoutMode.kGappedRow) ? AVOWCircuitSubsetter.singleton.GetResults().Count : AVOWCircuitCreator.singleton.GetResults().Count;
					if (currentGoalIndex < maxGoalNum){
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
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
}
