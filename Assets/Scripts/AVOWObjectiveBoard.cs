using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectiveBoard : MonoBehaviour {

	public GameObject[] woodPrefabs;
	public GameObject[] coverPrefabs;
	public GameObject shadedPrefab;
	public int totalNumPanels = 50;
	
	public float coverMoveSpeed;
	public float completeWaitDuration;
	
	public enum LayoutMode {
		kRow,
		kStack,
	};	
	
	float maxShade = 0.675f;
	AVOWCircuitTarget currentTarget;
	AVOWCircuitTarget displayTarget;
	
	
	bool	horizontalFirst;
	
	
	float completeWaitTime;
	
	float gridWidth = 0;
	
	// The nth cover maps to which index of the display
//	int[]	coversToDisplayMapping;
	int[]	displayToCoversMapping;
	
	
	GameObject[] currentWood;
	GameObject[] currentCovers;
	GameObject shadedSquare;
	SpringValue shadeVal;
	
	enum State{
		kReady,
		kMovingToTarget,
		kMovingToComplete,
		kWaitingCompleted,
		kDroppingOff,
	};
	State state = State.kReady;
	
	public void MoveToRow(){
		displayTarget = CreateRowTarget (displayTarget);
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
		horizontalFirst = true;
	}
	
	public void MoveToTarget(AVOWCircuitTarget target){
		displayTarget = target;
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
		horizontalFirst = false;
	}
	
	public void MoveToOriginalTarget(){
		displayTarget = currentTarget;
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
		horizontalFirst = false;
	}
	
	public void MoveToComplete(){
		state = State.kMovingToComplete;
	}
	
	public void PrepareBoard(AVOWCircuitTarget target, LayoutMode layoutMode){
		currentTarget = target;
		ConstructGrid(currentTarget);
		switch (layoutMode){
			case LayoutMode.kRow:{
				CreateCovers(CreateRowTarget(currentTarget));
			break;
			}
			case LayoutMode.kStack:{
				CreateCovers(currentTarget);
				break;
			}
		}

	}
	
	public float GetWidth(){
		return gridWidth;
	}
	
	
	public bool IsReady(){
		return state == State.kReady;
	}
	
	public void ConstructBlankBoard(){
		if (currentWood != null){
			foreach (GameObject go in currentWood){
				GameObject.Destroy(go);
			}
			GameObject.Destroy(shadedSquare);
		}
		int numPanels = totalNumPanels;
		currentWood = new GameObject[numPanels];
		for (int i = 0; i < numPanels; ++i){
			currentWood[i] = GameObject.Instantiate(woodPrefabs[0]);
			currentWood[i].transform.parent = transform;
			currentWood[i].transform.localPosition = new Vector3(i+1, 0, 0.2f);
		}
		shadedSquare = GameObject.Instantiate(shadedPrefab);
		shadedSquare.transform.parent = transform;
		shadedSquare.transform.localPosition = new Vector3(0, 0, -0.1f);
		shadedSquare.transform.localScale = new Vector3(numPanels, 1, 1);
		shadeVal = new SpringValue(0);
		UpdateShadedSquare();
		
		
		// Remove any covers that already exist
		if (currentCovers != null){
			foreach (GameObject go in currentCovers){
				GameObject.Destroy (go);
			}
			currentCovers = null;
		}
		
		transform.localPosition = Vector3.zero;
	}
	
	public void ConstructGrid(AVOWCircuitTarget target){
		int division = target.lcm;
		int width = target.widthInLCMs;
		
		gridWidth = (float)width / (float)division;
	
		
		if (currentWood != null){
			foreach (GameObject go in currentWood){
				GameObject.Destroy(go);
			}
			GameObject.Destroy(shadedSquare);
		}
		int numPanels = totalNumPanels;
		currentWood = new GameObject[numPanels];
		for (int i = 0; i < numPanels; ++i){
			currentWood[i] = GameObject.Instantiate(woodPrefabs[division]);
			currentWood[i].transform.parent = transform;
			currentWood[i].transform.localPosition = new Vector3(i+1, 0, 0.2f);
		}
		
		shadedSquare = GameObject.Instantiate(shadedPrefab);
		shadedSquare.transform.parent = transform;
		shadedSquare.transform.localPosition = new Vector3(0, 0, -0.1f);
		shadedSquare.transform.localScale = new Vector3(numPanels, 1, 1);
		shadeVal = new SpringValue(maxShade);
		UpdateShadedSquare();
	}
	
	void UpdateShadedSquare(){
		if (shadeVal  == null) return;
		shadeVal.Update ();
		
		if (shadeVal.IsAtTarget() && MathUtils.FP.Feq (shadeVal.GetValue(), 0)){
			shadedSquare.SetActive(false);
		}
		else{
			shadedSquare.SetActive(true);
			shadedSquare.GetComponent<Renderer>().material.SetColor("_Color", new Color (0, 0, 0, shadeVal.GetValue()));
		}
	
	}
	
	public void BrightenBoard(float duration){
		shadeVal.Set (0);
		shadeVal.SetSpeed(maxShade/duration);
	}
	

	
//	public void CreateCoversInRow(){
//		if (currentCovers != null){
//			foreach (GameObject go in currentCovers){
//				GameObject.Destroy (go);
//			}
//		}
//		currentCovers = new GameObject[currentTarget.componentDesc.Count];
//		float cumWidth = 0;
//		for (int i = 0; i < currentTarget.componentDesc.Count; ++i){
//			float thisWidth = currentTarget.componentDesc[i][0];
//			
//			currentCovers[i] = GameObject.Instantiate(coverPrefab);
//			currentCovers[i].transform.parent = transform;
//			currentCovers[i].transform.localScale = new Vector3(thisWidth, thisWidth, thisWidth);
//			currentCovers[i].transform.localPosition = new Vector3(cumWidth, 0, -0.1f);
//			
//			cumWidth += thisWidth;
//		}
//	}
//	
	public AVOWCircuitTarget CreateRowTarget(AVOWCircuitTarget target){
		// Make a deep copy
		AVOWCircuitTarget newTarget = new AVOWCircuitTarget(target);
		
		// The compoentDescs should already be sorted in biggest to smallest order
		float cumWidth = 0;
		for (int i = 0; i < newTarget.componentDesc.Count; ++i){
			Vector3 newDesc = newTarget.componentDesc[i];
			newDesc[1] = cumWidth;
			newDesc[2] = 0;
			cumWidth += newDesc[0];
			newTarget.componentDesc[i] = newDesc;
		
		}
		return newTarget;
		
	}
	
	public void DestroyBoard(){
		if (currentCovers != null){
			foreach (GameObject go in currentCovers){
				GameObject.Destroy (go);
			}
			currentCovers= null;
		}
		
		if (currentWood != null){
			foreach (GameObject go in currentWood){
				GameObject.Destroy(go);
			}
			GameObject.Destroy(shadedSquare);
			currentWood = null;
			shadedSquare = null;
			shadeVal = null;
		}
		
	}
	
	public void CreateCovers(AVOWCircuitTarget target){
		displayTarget = target;
		if (currentCovers != null){
			foreach (GameObject go in currentCovers){
				GameObject.Destroy (go);
			}
		}
		currentCovers = new GameObject[target.componentDesc.Count];
		float cumWidth = 0;
		for (int i = 0; i < target.componentDesc.Count; ++i){
			float thisWidth = target.componentDesc[i][0];
			
			int sizePrefab = Mathf.RoundToInt(1f/thisWidth) - 1;
			currentCovers[i] = GameObject.Instantiate(coverPrefabs[sizePrefab]);
			currentCovers[i].transform.parent = transform;
			currentCovers[i].transform.localScale = new Vector3(thisWidth, thisWidth, thisWidth);
			currentCovers[i].transform.localPosition = new Vector3(target.componentDesc[i][1], target.componentDesc[i][2], -0.01f * (i + 1));
			
			cumWidth += thisWidth;
		}
		displayToCoversMapping = CreateIdentityMapping(target.componentDesc.Count);
	}
	
	public bool TestWidthsMatch(AVOWCircuitTarget testTarget){
		// Exclude the battery in the count
		if (testTarget.componentDesc.Count != currentTarget.componentDesc.Count) return false;
		
		for (int i = 0; i < testTarget.componentDesc.Count; ++i){
			if (!MathUtils.FP.Feq(testTarget.componentDesc[i][0], currentTarget.componentDesc[i][0])) return false;
		}
		return true;
	}
	
	public bool TestPositionMatch(AVOWCircuitTarget testTarget){		
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			
			if (!MathUtils.FP.Feq (currentCovers[displayToCoversMapping[i]].transform.localPosition.x, displayTarget.componentDesc[i][1])){
				return false;
				
			}
			if (!MathUtils.FP.Feq (currentCovers[displayToCoversMapping[i]].transform.localPosition.y, displayTarget.componentDesc[i][2])){
				return false;
				
			}
		}
		return true;
	}
	
	int[] CreateIdentityMapping(int length){
		int[] mapping = new int[length];
		for (int i = 0; i < length; ++i){
			mapping[i] = i;
		}
		return mapping;
	}
	
	
	void RecreateDisplayMapping(){
		int startIndex = 0;
		int[] mapping = CreateIdentityMapping(displayTarget.componentDesc.Count);
		int[] testMapping = new int[displayTarget.componentDesc.Count];
		
		while (startIndex < displayTarget.componentDesc.Count){
			float thisWidth = displayTarget.componentDesc[startIndex][0];
			int interval = 0;
			while ((startIndex + interval) < displayTarget.componentDesc.Count && MathUtils.FP.Feq (displayTarget.componentDesc[(startIndex + interval)][0], thisWidth)){
				interval++;
			}
			
			// Make a copy of our master mapping in test
			mapping.CopyTo(testMapping, 0);
			
			// Create local permutations
			int[,] perms = MathUtils.FP.GeneratePermutations(interval);
			float minCost = -1;
			
			// For each local permutation, make a test mapping
			for (int permNum = 0; permNum  < perms.GetLength(0); ++permNum){
				for (int i = 0; i < perms.GetLength(1); +++i){
					testMapping[startIndex + i] = startIndex + perms[permNum, i];
				}
				float thisCost = CalcCost(displayTarget, testMapping);
				if (minCost < 0 || thisCost < minCost){
					minCost = thisCost;
						
					// Store the minimal mapping in our master mapping 
					// Make a copy of our master mapping in test
					testMapping.CopyTo(mapping, 0);					
				}
				
			}
			
			// Set minimum cost into our master mapping and continue
			startIndex += interval;
		}
		mapping.CopyTo(displayToCoversMapping, 0);
	}
	
	float CalcCost(AVOWCircuitTarget target, int[] testTargetToCoverMapping){
		float totalCost = 0;
		for (int i = 0; i < target.componentDesc.Count; ++i){
			float coverPosX = currentCovers[testTargetToCoverMapping[i]].transform.localPosition.x;
			float coverPosY = currentCovers[testTargetToCoverMapping[i]].transform.localPosition.y;
			
			float descPosX = target.componentDesc[i][1];
			float descPosY = target.componentDesc[i][2];
			
			//totalCost += (coverPosY - descPosY) * (coverPosY - descPosY) + (coverPosX - descPosX) * (coverPosX - descPosX);
			float thisCost = Mathf.Abs(coverPosY - descPosY)  + Mathf.Abs (coverPosX - descPosX);
			// We add a bit to try and get things further to the rigth to tend to be hiegher up (all other things being equal). 
			float addCost = (horizontalFirst) ? Mathf.Abs(coverPosY - descPosX) : Mathf.Abs(coverPosX - descPosY);
			totalCost += thisCost * thisCost + addCost;
		}
		return totalCost;
	}
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		UpdateShadedSquare();
		
		switch (state){
			case State.kMovingToTarget:{
				if (UpdateMoveToTarget ()) state = State.kReady;
				break;
			}
			case State.kMovingToComplete:{
				if (UpdateMoveToComplete ()){
					completeWaitTime = Time.time + completeWaitDuration;
				state = State.kWaitingCompleted;
				}
				break;
			}
			case State.kWaitingCompleted:{
				if (Time.time > completeWaitTime){
					state = State.kDroppingOff;
				}
			    
				break;
			}
			case State.kDroppingOff:{
				if (DroppingOff()) state = State.kReady;
				break;
			}
			
			
			
		}
	}
	
	bool UpdateMoveToTargetHorzontal(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		
		bool finished = true;
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			float coverPosX = currentCovers[displayToCoversMapping[i]].transform.localPosition.x;
			float coverPosY = currentCovers[displayToCoversMapping[i]].transform.localPosition.y;
			float coverPosZ = currentCovers[displayToCoversMapping[i]].transform.localPosition.z;
			
			float descPosX = displayTarget.componentDesc[i][1];
			
			if (!MathUtils.FP.Feq (coverPosX, descPosX)){
				if (coverPosX < descPosX){
					coverPosX += Mathf.Min (deltaDist, descPosX - coverPosX);
				}
				else{
					coverPosX -= Mathf.Min (deltaDist, coverPosX - descPosX);
				}
				finished = false;
			}
			currentCovers[displayToCoversMapping[i]].transform.localPosition = new Vector3(coverPosX, coverPosY, coverPosZ);
		}		
		
		
		return finished;
	}
	
	bool UpdateMoveToTarget(){
		if (horizontalFirst){
			bool finished = UpdateMoveToTargetHorzontal();
			if (finished){
				finished =  UpdateMoveToTargetVertical();
			}
			return finished;
		}
		else{
			bool finished = UpdateMoveToTargetVertical();
			if (finished){
				finished =  UpdateMoveToTargetHorzontal();
			}
			return finished;
		}
	}
	
	
	bool UpdateMoveToTargetVertical(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		bool finished = true;
		
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			float coverPosX = currentCovers[displayToCoversMapping[i]].transform.localPosition.x;
			float coverPosY = currentCovers[displayToCoversMapping[i]].transform.localPosition.y;
			float coverPosZ = currentCovers[displayToCoversMapping[i]].transform.localPosition.z;
			
			float descPosY = displayTarget.componentDesc[i][2];
			
			if (!MathUtils.FP.Feq (coverPosY, descPosY)){
				if (coverPosY < descPosY){
					coverPosY += Mathf.Min (deltaDist, descPosY - coverPosY);
				}
				else{
					coverPosY -= Mathf.Min (deltaDist, coverPosY - descPosY);
				}
				finished = false;
				
			}
			currentCovers[displayToCoversMapping[i]].transform.localPosition = new Vector3(coverPosX, coverPosY, coverPosZ);
		}
		return finished;
		

	}

	bool UpdateMoveToComplete(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		
		float maxDistToMove = currentCovers[displayToCoversMapping[0]].transform.localPosition.x - (displayTarget.componentDesc[0][1]- displayTarget.totalCurrent);
		
		if (MathUtils.FP.Feq(maxDistToMove, 0)){
			return true;
		}
		
		for (int i = 0; i < currentCovers.Length; ++i){
			Vector3 pos = currentCovers[i].transform.localPosition;
			pos.x -= Mathf.Min (deltaDist, maxDistToMove);
			currentCovers[i].transform.localPosition = pos;
			
		}	
		return false;
	}
	
	bool DroppingOff(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		float minHeight = 1;
		foreach (GameObject go in currentCovers){
			go.transform.localPosition -= new Vector3(0, deltaDist, 0);
			minHeight = Mathf.Min (minHeight, go.transform.localPosition.y);
		}	
		return (minHeight < -1);
	}
	
}
