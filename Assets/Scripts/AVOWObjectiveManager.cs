using UnityEngine;
using System.Collections;

public class AVOWObjectiveManager : MonoBehaviour {

	public static AVOWObjectiveManager singleton = null;
	public GameObject objectiveBoardPrefab;
	
	// Front and back boards
	GameObject[] boards = new GameObject[2];
	int frontIndex = 0;
	int backIndex = 1;
	
	public float xMax = 2f;
	
	int resistorLimit = -1;
	int currentGoalIndex = 0;
	
	enum State{
		kNone,
		kWaitForCircuitCreator,
		kBuildBackBoard,
		kSwapBoards,
	};
	
	State state = State.kNone;
	
	
	// reutrn -1 if no limit
	public int GetResistorLimit(){
		return resistorLimit;
	}
	
	
	public void InitialiseLevel(int level){
		resistorLimit = level + 1;
		AVOWCircuitCreator.singleton.Initialise(resistorLimit);
		state = State.kWaitForCircuitCreator;
		currentGoalIndex = 0;
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
		SetBoardDepths();
		
	}
	
	void SwapBoards(){
		frontIndex = 1 - frontIndex;
		backIndex = 1 - backIndex;
	}
	
	void SetBoardDepths(){
		Vector3 backPos = boards[backIndex].transform.localPosition;
		backPos.z = 0.1f;
		boards[backIndex].transform.localPosition = backPos;
		
		Vector3 frontPos = boards[frontIndex].transform.localPosition;
		frontPos.z = 0f;
		boards[frontIndex].transform.localPosition = frontPos;
		
	//	BOARDS ARE NOT VBISIBLE
		
		
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
				boards[backIndex].GetComponent<AVOWObjectiveBoard>().PrepareBoard(AVOWCircuitCreator.singleton.GetResults()[currentGoalIndex]);
				state = State.kSwapBoards;
				break;
			}
			case State.kSwapBoards:{
				SwapBoards();
				state = State.kNone;
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
