using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectiveBoard : MonoBehaviour {

	public GameObject[] woodPrefabs;
	public GameObject coverPrefab;
	public float coverModeSpeed = 1f;
	
	AVOWCircuitTarget currentTarget;
	AVOWCircuitTarget displayTarget;
	
	// The nth cover maps to which index of the display
//	int[]	coversToDisplayMapping;
	int[]	displayToCoversMapping;
	
	
	GameObject[] currentWood;
	GameObject[] currentCovers;
	
	enum State{
		kReady,
		kMovingToTarget,
		kMovingToComplete
	};
	State state = State.kReady;
	
	public void MoveToRow(){
		displayTarget = CreateRowTarget (displayTarget);
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
	}
	
	public void MoveToTarget(AVOWCircuitTarget target){
		displayTarget = target;
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
	}
	
	public void MoveToOriginalTarget(){
		displayTarget = currentTarget;
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
	}
	
	public void MoveToComplete(){
		state = State.kMovingToComplete;
	}
	
	public void PrepareBoard(AVOWCircuitTarget target){
		currentTarget = target;
		ConstructGrid(currentTarget);
		CreateCovers(CreateRowTarget(currentTarget));
		//CreateCovers(currentTarget);
	}
	
	
	public bool IsReady(){
		return state == State.kReady;
	}
	
	public void ConstructGrid(AVOWCircuitTarget target){
		int division = target.lcm;
		int width = target.totalCurrentInLCMs;
		
		if (currentWood != null){
			foreach (GameObject go in currentWood){
				GameObject.Destroy(go);
			}
		}
		int numPanels = 1 + width / division;
		currentWood = new GameObject[numPanels];
		for (int i = 0; i < numPanels; ++i){
			currentWood[i] = GameObject.Instantiate(woodPrefabs[division-1]);
			currentWood[i].transform.parent = transform;
			currentWood[i].transform.localPosition = new Vector3(i+1, 0, 0.2f);
		}
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
			
			currentCovers[i] = GameObject.Instantiate(coverPrefab);
			currentCovers[i].transform.parent = transform;
			currentCovers[i].transform.localScale = new Vector3(thisWidth, thisWidth, thisWidth);
			currentCovers[i].transform.localPosition = new Vector3(target.componentDesc[i][1], target.componentDesc[i][2], -0.01f * i);
			
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
			totalCost += thisCost * thisCost;
		}
		return totalCost;
	}
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		switch (state){
			case State.kMovingToTarget:{
				if (UpdateMoveToTarget ()) state = State.kReady;
				break;
			}
			case State.kMovingToComplete:{
				if (UpdateMoveToComplete ()) state = State.kReady;
				break;
			}
			

		}
	}
	
	
	bool UpdateMoveToTarget(){
		float deltaDist = coverModeSpeed * Time.deltaTime;
		bool yFinished = true;
		
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			float coverPosX = currentCovers[displayToCoversMapping[i]].transform.localPosition.x;
			float coverPosY = currentCovers[displayToCoversMapping[i]].transform.localPosition.y;
			float coverPosZ = currentCovers[displayToCoversMapping[i]].transform.localPosition.z;
			
			float descPosX = displayTarget.componentDesc[i][1];
			float descPosY = displayTarget.componentDesc[i][2];
			
			if (!MathUtils.FP.Feq (coverPosY, descPosY)){
				if (coverPosY < descPosY){
					coverPosY += Mathf.Min (deltaDist, descPosY - coverPosY);
				}
				else{
					coverPosY -= Mathf.Min (deltaDist, coverPosY - descPosY);
				}
				yFinished = false;
				
			}
			currentCovers[displayToCoversMapping[i]].transform.localPosition = new Vector3(coverPosX, coverPosY, coverPosZ);
		}
		if (!yFinished) return false;
		
		bool xFinished = true;
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			float coverPosX = currentCovers[displayToCoversMapping[i]].transform.localPosition.x;
			float coverPosY = currentCovers[displayToCoversMapping[i]].transform.localPosition.y;
			float coverPosZ = currentCovers[displayToCoversMapping[i]].transform.localPosition.z;
			
			float descPosX = displayTarget.componentDesc[i][1];
			float descPosY = displayTarget.componentDesc[i][2];
			
			if (!MathUtils.FP.Feq (coverPosX, descPosX)){
				if (coverPosX < descPosX){
					coverPosX += Mathf.Min (deltaDist, descPosX - coverPosX);
				}
				else{
					coverPosX -= Mathf.Min (deltaDist, coverPosX - descPosX);
				}
				xFinished = false;
			}
			currentCovers[displayToCoversMapping[i]].transform.localPosition = new Vector3(coverPosX, coverPosY, coverPosZ);
		}		
		

		return xFinished && yFinished;
	}

	bool UpdateMoveToComplete(){
		return true;
	}
	
}
