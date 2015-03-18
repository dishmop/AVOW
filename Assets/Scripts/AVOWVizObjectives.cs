using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AVOWVizObjectives : MonoBehaviour {
	public static AVOWVizObjectives singleton = null;

	public GameObject gridGOPrefab;
	public GameObject lineGOPrefab;
	public GameObject metalCoverPrefab;
	public Color	  col = new Color(128, 128, 128);
	
	public float successDelayTime = 0;

	
//	AVOWObjectiveGrid	grid;
//	DrawnLine			line;
	
	public int displayedObjective = -1;
	
	enum State{
		kNone,
		kBuild,
		kPlay,
		kDestroy,
		kFinalise0,
		kFinalise1,
		kFinalise2,
		kFinalise3,
		kFinalise4,
		kFinalise5
	}
	
	State state = State.kNone;
	
	class Box{
		public Box(float size, float xPos, float yPos, Color col, GameObject coverPrefab, Transform parent){
			this.size = size;
			this.xPos = new SpringValue(xPos, SpringValue.Mode.kLinear, 2);
			this.yPos = new SpringValue(yPos, SpringValue.Mode.kLinear, 2);
			this.col = col;
			
			coverGO = GameObject.Instantiate(coverPrefab) as GameObject;
			coverGO.transform.parent = parent;
			coverGO.transform.localScale = new Vector3(size, size, size);
			SetupBoxes();
		}
		
		public void Update(){
			xPos.Update();
			yPos.Update();
			//	if (!xPos.IsAtTarget() || !yPos.IsAtTarget()) 
			MoveBoxes();
		}
		
		void SetupBoxes(){
			float currentXPos = xPos.GetValue();
			float currentYpos = yPos.GetValue();
			
			coverGO.transform.localPosition = new Vector3(xPos.GetValue(), yPos.GetValue(), coverGO.transform.parent.position.z);
		}
		
		
		void MoveBoxes(){
			float currentXPos = xPos.GetValue();
			float currentYpos = yPos.GetValue();
			
			coverGO.transform.localPosition = new Vector3(xPos.GetValue(), yPos.GetValue(), coverGO.transform.parent.position.z);
			
		}
		
		public void Destroy(){
			GameObject.Destroy(coverGO);
		}
		
		public float size;
		
		public GameObject coverGO;
		
		public SpringValue xPos;
		public SpringValue yPos;
		public Color col;
	//	public float border = 0.01f;
	};
	
	class BoxDrawn{
		public BoxDrawn(float size, float xPos, float yPos, Color col, GameObject lineGOPrefab, Transform parent){
			this.size = size;
			this.xPos = new SpringValue(xPos, SpringValue.Mode.kLinear, 2);
			this.yPos = new SpringValue(yPos, SpringValue.Mode.kLinear, 2);
			this.col = col;

			for (int i = 0; i < 4; ++i){
				lines[i] = GameObject.Instantiate(lineGOPrefab) as GameObject;
				lines[i].transform.parent = parent;
			}
			SetupBoxes();
		}
		
		public void Update(){
			xPos.Update();
			yPos.Update();
		//	if (!xPos.IsAtTarget() || !yPos.IsAtTarget()) 
			MoveBoxes();
		}
		
		void SetupBoxes(){
			float currentXPos = xPos.GetValue();
			float currentYpos = yPos.GetValue();

			
			Vector2 bottomLeft = new Vector2(currentXPos + border, currentYpos + border);
			Vector2 topRight = new Vector2(currentXPos + size - border, currentYpos + size - border);
			Vector2 topLeft = new Vector2(currentXPos + border, currentYpos + size - border);
			Vector2 bottomRight =  new Vector2(currentXPos + size - border, currentYpos + border);
			
			lines[0].GetComponent<DrawnLine>().Draw(bottomLeft, topLeft, col);
			lines[1].GetComponent<DrawnLine>().Draw(topLeft, topRight, col);
			lines[2].GetComponent<DrawnLine>().Draw(topRight, bottomRight, col);
			lines[3].GetComponent<DrawnLine>().Draw(bottomRight, bottomLeft, col);
			
		}
		
		
		void MoveBoxes(){
			float currentXPos = xPos.GetValue();
			float currentYpos = yPos.GetValue();
			
			Vector2 bottomLeft = new Vector2(currentXPos + border, currentYpos + border);
			Vector2 topRight = new Vector2(currentXPos + size - border, currentYpos + size - border);
			Vector2 topLeft = new Vector2(currentXPos + border, currentYpos + size - border);
			Vector2 bottomRight =  new Vector2(currentXPos + size - border, currentYpos + border);
			
			lines[0].GetComponent<DrawnLine>().Move(bottomLeft, topLeft, col);
			lines[1].GetComponent<DrawnLine>().Move(topLeft, topRight, col);
			lines[2].GetComponent<DrawnLine>().Move(topRight, bottomRight, col);
			lines[3].GetComponent<DrawnLine>().Move(bottomRight, bottomLeft, col);
			
		}
		
		public void Destroy(){
			for (int i = 0; i < 4; ++i){
				GameObject.Destroy (lines[i]);
			}
		}
		
		public float size;

		public GameObject[] lines = new GameObject[4];
		
		public SpringValue xPos;
		public SpringValue yPos;
		public Color col;
		public float border = 0.01f;
	};
	
	Box[] boxes;
	GameObject totalLine;

	// Use this for initialization
	void Start () {
//		grid = gridGOPrefab.GetComponent<AVOWObjectiveGrid>();
//		line = lineGOPrefab.GetComponent<DrawnLine>();
	
	}
	
	public void Rebuild(){
		state = State.kDestroy;
	}
	

	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(gridGOPrefab.transform.position.x, gridGOPrefab.transform.position.y, transform.position.z);
	
		switch (state){
			case State.kNone:{
				if (!AVOWObjectives.singleton.isComplete && AVOWConfig.singleton.ShowGraphicObjectives() && AVOWObjectiveGrid.singleton.IsFinished()){
					state = State.kBuild;
				}
	
				break;
			}
			case State.kBuild:{
				AVOWCircuitTarget goal	= AVOWObjectives.singleton.GetCurrentGoal();
				if (AVOWConfig.singleton.showTotals){
					totalLine = GameObject.Instantiate(lineGOPrefab) as GameObject;
					totalLine.transform.parent = transform;
					totalLine.GetComponent<DrawnLine>().Draw(new Vector2(goal.totalCurrent, 0), new Vector2(goal.totalCurrent, 1), new Color(1, 0, 0));
				}
				
			
				boxes = new Box[goal.individualCurrents.Count];
				float cumWidth = 0;
				for (int i = 0; i < goal.individualCurrents.Count; ++i){
					float thisWidth = goal.individualCurrents[i];
					
					boxes[i] = new Box(thisWidth, cumWidth, 0, col, metalCoverPrefab, transform);
					cumWidth += thisWidth;
				}
				AVOWBattery.singleton.ResetBattery();
			
				state = State.kPlay;
				
				break;
			}
			
			case State.kPlay:{
				if (displayedObjective != AVOWObjectives.singleton.currentObjective && AllBoxesAreOnTarget()){	
					HandleBoxPositions();
					state = State.kFinalise0;
				}
				if (!AVOWConfig.singleton.ShowGraphicObjectives()){
					state = State.kDestroy;
				}
				break;
			}
			case State.kFinalise0:{
				if (AllBoxesAreOnTarget()){
					SetupFinalise1();
					state = State.kFinalise1;
				}
				break;
			}
			case State.kFinalise1:{

				if (AllBoxesAreOnTarget()){
					SetupFinalise2();
					state = State.kFinalise2;
				}
				break;
			}
			case State.kFinalise2:{

				if (AllBoxesAreOnTarget()){
					state = State.kFinalise3;
					successDelayTime = Time.time + 0.2f;
				}
				break;
			}			
			case State.kFinalise3:{
				
				if (Time.time > successDelayTime)
					state = State.kFinalise4;
				break;
			}
			case State.kFinalise4:{
				SetupFinalise4();
				state = State.kFinalise5;
				break;
			}			
			case State.kFinalise5:{
				if (AllBoxesAreOnTarget()){
					state = State.kDestroy;
				}
				break;
			}
			case State.kDestroy:{
				if (boxes != null){
					foreach (Box box in boxes){
						box.Destroy();
					}
				}
				displayedObjective = AVOWObjectives.singleton.currentObjective;
				boxes = null;
				GameObject.Destroy (totalLine);
				totalLine = null;
				state = State.kNone;
				break;
			}
		};
		
		// Ensure the boxes are updaing
		if (boxes != null){
			foreach (Box box in boxes){
				box.Update();
			}
		}
	}
	
	void SetupFinalise1(){
		// Ignore the battery - which should always be the first one.
		if (AVOWGraph.singleton.allComponents.Count < 2) return;
		
		List<GameObject> componentGOs = new List<GameObject>();
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (!component.isInteractive) continue;
			componentGOs.Add (go);
		}
		
		List<GameObject> sortedList = componentGOs.OrderBy(x => x.GetComponent<AVOWComponent>().GetComponent<AVOWComponent>().inNodeGO.GetComponent<AVOWNode>().voltage).
			ThenBy(x => x.GetComponent<AVOWComponent>().h0).ToList();
		componentGOs = sortedList;
		
		// Go through the boxes and see if we have achieved any
		for (int i = 0; i < boxes.Length; ++i){
			GameObject foundGO = null;
			foreach (GameObject go in componentGOs){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (MathUtils.FP.Feq (component.hWidth, boxes[i].size)){
					foundGO = go;
					break;
				}
			}
			if (foundGO){
				boxes[i].xPos.Set (foundGO.GetComponent<AVOWComponent>().h0);
				boxes[i].col = new Color(0, 1, 0);
				componentGOs.Remove (foundGO);
			}
			
		}
	}
	
	void SetupFinalise2(){
		float xDiff = gridGOPrefab.transform.position.x;
		foreach(Box box in boxes){
			box.xPos.Set (box.xPos.GetDesValue() - xDiff);
		}
	}
	
	
	void SetupFinalise4(){
		foreach(Box box in boxes){
			box.yPos.Set (box.yPos.GetDesValue() - 1.5f);
		}
	}
	
	bool AllBoxesAreOnTarget(){
	
		foreach (Box box in boxes){
			if (!box.xPos.IsAtTarget() || !box.yPos.IsAtTarget()) return false;
		}
		return true;
	
	}
	
	
	void HandleBoxPositions(){
		// Ignore the battery - which should always be the first one.
		if (AVOWGraph.singleton.allComponents.Count < 2) return;
		
		List<GameObject> componentGOs = new List<GameObject>();
		foreach (GameObject go in AVOWGraph.singleton.allComponents){
			if (go == null) continue;
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (!component.isInteractive) continue;
			componentGOs.Add (go);
		}
		
		List<GameObject> sortedList = componentGOs.OrderBy(x => x.GetComponent<AVOWComponent>().GetComponent<AVOWComponent>().inNodeGO.GetComponent<AVOWNode>().voltage).
			ThenBy(x => x.GetComponent<AVOWComponent>().h0).ToList();
		componentGOs = sortedList;
		
		// Go through the boxes and see if we have achieved any
		for (int i = 0; i < boxes.Length; ++i){
			GameObject foundGO = null;
			foreach (GameObject go in componentGOs){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (MathUtils.FP.Feq (component.hWidth, boxes[i].size)){
					foundGO = go;
					break;
				}
			}
			if (foundGO){
				boxes[i].yPos.Set (foundGO.GetComponent<AVOWComponent>().inNodeGO.GetComponent<AVOWNode>().voltage);
				boxes[i].col = new Color(0, 1, 0);
				componentGOs.Remove (foundGO);
			}
			else{
				boxes[i].yPos.Set (0);
				boxes[i].col = col;
			}
			
		}
		

		
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}
}
