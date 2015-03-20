using UnityEngine;
using System.Collections;

public class AVOWObjectiveManager : MonoBehaviour {

	public static AVOWObjectiveManager singleton = null;
	public GameObject objectiveBoardPrefab;
	public float boardSpeed;
	
	// Front and back boards
	GameObject[] boards = new GameObject[2];
	int frontIndex = 0;
	int backIndex = 1;
	
	enum LayoutMode {
		kRow,
		kStack
	};
	LayoutMode layoutMode = LayoutMode.kRow;
	
	
	float backBoardDepth = 0.5f;
	float frontBoardDepth = 0;
	float boardDepthSpeed = 1f;
	
	int resistorLimit = -1;
	int currentGoalIndex;
	
	bool isFirstBoard;
	
	enum State{
		kNone,
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
	
	public void InitialiseLevel(int level){
		switch (level){
			case 1:{
				layoutMode = LayoutMode.kStack;
				resistorLimit = 3;
				break;
			}
			case 2:{
				layoutMode = LayoutMode.kStack;
				resistorLimit = 4;
				break;
			}
			case 3:{
				layoutMode = LayoutMode.kRow;
				resistorLimit = 4;
				break;
			}
			case 4:{
				layoutMode = LayoutMode.kRow;
				resistorLimit = 5;
				break;
			}
		}
		
		AVOWCircuitCreator.singleton.Initialise(resistorLimit);
		state = State.kWaitForCircuitCreator;
		currentGoalIndex = 0;
		isFirstBoard = true;
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
			case State.kWaitForCircuitCreator:{
				if (AVOWCircuitCreator.singleton.IsReady()){
					state = State.kBuildBackBoard;
				}
				break;
			}
			case State.kBuildBackBoard:{
				boards[backIndex].GetComponent<AVOWObjectiveBoard>().PrepareBoard(AVOWCircuitCreator.singleton.GetResults()[currentGoalIndex], layoutMode == LayoutMode.kStack);
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
				
				if (pos.y < -1 || isFirstBoard){
					SwapBoards();
					boards[frontIndex].GetComponent<AVOWObjectiveBoard>().BrightenBoard(0.5f);
					state = State.kSwapBoards1;
					isFirstBoard = false;
				}
				break;
			}
			case State.kSwapBoards1:{
				Vector3 pos = boards[backIndex].transform.position;
				pos.y -= boardSpeed * Time.deltaTime;
				boards[backIndex].transform.position = pos;
				
				if (UpdateBoardDepths()){
					state = State.kPlay;
				}
				break;
			}
			case State.kPlay:{
				if (!AVOWGraph.singleton.HasHalfFinishedComponents()){
					AVOWCircuitTarget currentGraphAsTarget = new AVOWCircuitTarget(AVOWGraph.singleton);
					if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().TestWidthsMatch(currentGraphAsTarget)){
						boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToTarget(currentGraphAsTarget);
						state = State.kGoalComplete0;
					}
			    }
				break;
			}
			case State.kGoalComplete0:{
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					boards[frontIndex].GetComponent<AVOWObjectiveBoard>().MoveToComplete();
					state = State.kGoalComplete1;
				}
				break;
			}
			case State.kGoalComplete1:{
				if (boards[frontIndex].GetComponent<AVOWObjectiveBoard>().IsReady()){
					currentGoalIndex++;
					if (currentGoalIndex < AVOWCircuitCreator.singleton.GetResults().Count){
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
