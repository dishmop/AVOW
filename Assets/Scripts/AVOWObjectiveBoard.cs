using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;


public class AVOWObjectiveBoard : MonoBehaviour {

	// When instantiasting this we give them a name which is the index of the prefab - so we can save it out easily
	public GameObject[] woodPrefabs;
	public GameObject[] coverPrefabs;
	public GameObject shadedPrefab;
	public int totalNumPanels = 50;
	
	public float coverMoveSpeed;
	public float completeWaitDuration;
	
	public bool renderBackwards;
	
	public enum LayoutMode {
		kRow,
		kStack,
		kGappedRow,
	};	
	
	const int		kLoadSaveVersion = 1;
	
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
	
	
	
	public void Serialise(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		
		bw.Write (transform.localPosition);
		bw.Write (transform.localRotation);
		bw.Write (transform.localScale);
		
		bw.Write (renderBackwards);
		bw.Write (currentTarget != null);
		if (currentTarget != null){
			currentTarget.Serialise(bw);
		}
		bw.Write (displayTarget != null);
		if (displayTarget != null){
			displayTarget.Serialise(bw);
		}

		bw.Write (horizontalFirst);
		bw.Write (completeWaitTime);
		bw.Write (gridWidth);
		
		bw.Write (displayToCoversMapping != null);
		if (displayToCoversMapping != null){
			bw.Write (displayToCoversMapping.Length);
			for (int i = 0; i < displayToCoversMapping.Length; ++i){
				bw.Write (displayToCoversMapping[i]);
			}
		}
		
		bw.Write (currentWood != null);
		if (currentWood != null){
			bw.Write (currentWood.Length);
			for (int i = 0; i < currentWood.Length; ++i){
				bw.Write (currentWood[i].name);
				bw.Write (currentWood[i].transform.localPosition);
				bw.Write (currentWood[i].transform.localRotation);
				bw.Write (currentWood[i].transform.localScale);
			}
		}
		
		bw.Write (currentCovers != null);
		if (currentCovers != null){
			bw.Write (currentCovers.Length);
			for (int i = 0; i < currentCovers.Length; ++i){
				bw.Write (currentCovers[i].name);
				bw.Write (currentCovers[i].transform.localPosition);
				bw.Write (currentCovers[i].transform.localRotation);
				bw.Write (currentCovers[i].transform.localScale);
			}
		}
		
		bw.Write (shadedSquare != null);
		if (shadedSquare != null){
			bw.Write (shadedSquare.transform.localPosition);
			bw.Write (shadedSquare.transform.localRotation);
			bw.Write (shadedSquare.transform.localScale);
		}
		bw.Write (shadeVal != null);
		if (shadeVal != null){
			shadeVal.Serialise(bw);
		}
		
		bw.Write ((int)state);
		
	}
	
	
	public void Deserialise(BinaryReader br){
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				transform.localPosition = br.ReadVector3 ();
				transform.localRotation = br.ReadQuaternion ();
				transform.localScale = br.ReadVector3 ();
				
			
				renderBackwards = br.ReadBoolean();
				bool hasCurrentTarget = br.ReadBoolean();
				if (hasCurrentTarget){
					currentTarget = new AVOWCircuitTarget();
					currentTarget.Deserialise(br);
				}
				else{
					currentTarget = null;
				}
				
				bool hasDisplayTarget = br.ReadBoolean();
				if (hasDisplayTarget){
					displayTarget = new AVOWCircuitTarget();
					displayTarget.Deserialise(br);
				}
				else{
					displayTarget = null;
				}
					
				horizontalFirst = br.ReadBoolean ();
				completeWaitTime = br.ReadSingle();
				gridWidth = br.ReadSingle();
			
				bool hasMapping = br.ReadBoolean();
				if (hasMapping){
					int size = br.ReadInt32 ();
					displayToCoversMapping = new int[size];
					for (int i = 0; i < size; ++i){
						displayToCoversMapping[i] = br.ReadInt32();
					}
				}
				else{
					displayToCoversMapping = null;
				}
				
				
				if (currentWood != null){
					foreach (GameObject go in currentWood){
						GameObject.Destroy(go);
					}
					currentWood = null;
				}		
						
				bool hasWood = br.ReadBoolean ();
				if (hasWood){

					int size = br.ReadInt32 ();
					currentWood = new GameObject[size];
					for (int i = 0; i < size; ++i){
						string name = br.ReadString();
						currentWood[i] = GameObject.Instantiate(woodPrefabs[Convert.ToInt32(name)]);
						currentWood[i].transform.parent = transform;
						currentWood[i].transform.localPosition = br.ReadVector3();
						currentWood[i].transform.localRotation = br.ReadQuaternion();
						currentWood[i].transform.localScale = br.ReadVector3();
					}
				}

				
					
				if (currentCovers != null){
					foreach (GameObject go in currentCovers){
						GameObject.Destroy(go);
					}
					currentCovers = null;
				}			
					
				bool hasCovers = br.ReadBoolean ();
				if (hasCovers){
					int size = br.ReadInt32 ();
					currentCovers = new GameObject[size];
					for (int i = 0; i < size; ++i){
						string name = br.ReadString();
						currentCovers[i] = GameObject.Instantiate(coverPrefabs[Convert.ToInt32(name)]);
						currentCovers[i].transform.parent = transform;
						currentCovers[i].transform.localPosition = br.ReadVector3();
						currentCovers[i].transform.localRotation = br.ReadQuaternion();
						currentCovers[i].transform.localScale = br.ReadVector3();
					}
				}
				
				
				if (shadedSquare != null){
					GameObject.Destroy(shadedSquare);
				}
				bool hasShadedSquare = br.ReadBoolean();
				if (hasShadedSquare){
					shadedSquare = GameObject.Instantiate(shadedPrefab);
					shadedSquare.transform.parent = transform;
					shadedSquare.transform.localPosition = br.ReadVector3();
					shadedSquare.transform.localRotation = br.ReadQuaternion();
					shadedSquare.transform.localScale = br.ReadVector3();
				}
				bool hasShadeVale = br.ReadBoolean();
				if (hasShadeVale){
					shadeVal = new SpringValue(0);
					shadeVal.Deserialise(br);
				}
				else{
					shadeVal = null;
				}
				

				state = (State)br.ReadInt32 ();;
				break;
			}
		}
	}
	
	public void MoveToRow(){
		displayTarget = CreateRowTarget (displayTarget, false);
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
		horizontalFirst = true;
	}
	
	
	
	public void MoveToGappedRow(){
		displayTarget = CreateRowTarget (currentTarget, false);
		ReorderCoversToMatch(displayTarget);
		RecreateDisplayMapping();
		state = State.kMovingToTarget;
		horizontalFirst = true;
	}
	
	
	public void MoveToTarget(AVOWCircuitTarget target){
		displayTarget = ConstructCompatibleDisplayTarget(target);
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
		switch (layoutMode){
			case LayoutMode.kRow:{
			ConstructGrid(CreateRowTarget(currentTarget, true));
				CreateCovers(CreateRowTarget(currentTarget, true));
				break;
			}
			case LayoutMode.kGappedRow:{
				ConstructGrid(CreateRowTarget(currentTarget, false));
				CreateCovers(CreateRowTarget(currentTarget, false));
				break;
			}
			case LayoutMode.kStack:{
				ConstructGrid(currentTarget);
				CreateCovers(currentTarget);
				break;
			}
		}

	}
	
	public float GetWidth(){
		return Mathf.Max (1, gridWidth);
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
		gridWidth = 1;
		int numPanels = totalNumPanels;
		currentWood = new GameObject[numPanels];
		for (int i = 0; i < numPanels; ++i){
			currentWood[i] = GameObject.Instantiate(woodPrefabs[0]);
			currentWood[i].transform.parent = transform;
			float xPos = renderBackwards ? 0-i  : i+1;
			currentWood[i].transform.localPosition = new Vector3(xPos, 0, 0.2f);
			currentWood[i].name = "0";
		}
		shadedSquare = GameObject.Instantiate(shadedPrefab);
		shadedSquare.transform.parent = transform;
		shadedSquare.transform.localPosition = new Vector3(0, 0, -0.1f);
		shadedSquare.transform.localScale = new Vector3(renderBackwards ? -numPanels : numPanels, 1, 1);
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
		state = State.kReady;
	}
	
	public void ConstructGrid(AVOWCircuitTarget target){
		int division = target.lcm;
		int width = target.widthInLCMs;
		
		if (target.totalCurrent < 0){
			gridWidth = (float)width / (float)division;
		}
		else{
			gridWidth = target.totalCurrent;
		}
		
		
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
			float xPos = renderBackwards ? 0-i  : i+1;
			currentWood[i].transform.localPosition = new Vector3(xPos, 0, 0.2f);
			currentWood[i].name = division.ToString();
			
		}
		
		shadedSquare = GameObject.Instantiate(shadedPrefab);
		shadedSquare.transform.parent = transform;
		shadedSquare.transform.localPosition = new Vector3(0, 0, -0.1f);
		shadedSquare.transform.localScale = new Vector3(renderBackwards ? -numPanels : numPanels, 1, 1);
		shadeVal = new SpringValue(maxShade);
		UpdateShadedSquare();
	}
	
	// This constructs a target which we can display given that our goal is "target" - our covers may not currently
	// Match these sizes, though should at least be a subset of it
	AVOWCircuitTarget ConstructCompatibleDisplayTarget(AVOWCircuitTarget target){
		// Lets assume that the current display target does match the covers and try and make something which matches the current display target
		// This target has things in all the right places
		
		// Go through each of the components in target and match them with ones in display - if we can't match one, then remove it
		AVOWCircuitTarget newTarget = new AVOWCircuitTarget(target);
		List<Vector3> currentDisplayList = new List<Vector3>();
		foreach(Vector3 vals in displayTarget.componentDesc){
			currentDisplayList.Add (vals);	
		}
		
		for (int i = 0; i < newTarget.componentDesc.Count; ){
			float targetVal = newTarget.componentDesc[i][0];
			
			int removalIndex = -1;
			for (int j = 0; j < currentDisplayList.Count; ++j){
				if (MathUtils.FP.Feq(currentDisplayList[j][0], targetVal)){
					removalIndex = j;
					break;
				}
			}
			// If we couldn't find our target box in the set of displayed components - then remove it from the target
			// And no need to increment i
			if (removalIndex == -1){
				newTarget.HideComponent(i);
			}
			// Otherwise, we did find it - so should remove it from the display list to show it has already been accounted for
			else{
				currentDisplayList.RemoveAt(removalIndex);
				++i;
			}
		}
		
		// We should have at least accounted for everything in the display list
		if (currentDisplayList.Count != 0){
			Debug.LogError ("currentDisplayList.Count != 0");
		}
		// Also we should have the same set of targets in the newTarget as we had in the old one
		if (!newTarget.Equals (displayTarget)){
			Debug.LogError ("(!newTarget.Equals (displayTarget)");
		}
		return newTarget;

		
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
	public AVOWCircuitTarget CreateRowTarget(AVOWCircuitTarget target, bool showHidden){
		// Make a deep copy
		AVOWCircuitTarget newTarget = new AVOWCircuitTarget(target);
		if (showHidden) newTarget.UnhideAllComponents();
		
		// The compoentDescs should already be sorted in biggest to smallest order
		float cumWidth = 0;
		for (int i = 0; i < newTarget.componentDesc.Count; ++i){
			Vector3 newDesc = newTarget.componentDesc[i];
			newDesc[1] = cumWidth;
			newDesc[2] = 0;
			cumWidth += newDesc[0];
			newTarget.componentDesc[i] = newDesc;
		
		}
		newTarget.totalCurrent = -1;
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
	
	Vector3 GamePosToTransformPos(Vector3 pos, float sizeOfSquare, float totalCurrent){
		if(totalCurrent >= 0){
			return renderBackwards ? new Vector3(-totalCurrent + pos.x, pos.y, pos.z) : pos;
	  }
	  else{
			return renderBackwards ? new Vector3( - sizeOfSquare - pos.x, pos.y, pos.z) : pos;
		}
	}
	

	Vector3 TransformPosToGamePos(Vector3 pos, float sizeOfSquare, float totalCurrent){
		if(totalCurrent >= 0){
			return renderBackwards ? new Vector3(pos.x + totalCurrent, pos.y, pos.z) : pos;
		}
		else{
			return renderBackwards ? new Vector3( - sizeOfSquare - pos.x, pos.y, pos.z) : pos;
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
			currentCovers[i].transform.localPosition = GamePosToTransformPos(new Vector3(target.componentDesc[i][1], target.componentDesc[i][2], -0.01f * (i + 1)), thisWidth, target.totalCurrent);
			currentCovers[i].name = sizePrefab.ToString();
			cumWidth += thisWidth;
		}
		displayToCoversMapping = CreateIdentityMapping(target.componentDesc.Count);
	}
//	
//	public bool TestWidthsMatch(AVOWCircuitTarget testTarget){
//		if (testTarget.componentDesc.Count != currentTarget.componentDesc.Count) return false;
//		
//		for (int i = 0; i < testTarget.componentDesc.Count; ++i){
//			if (!MathUtils.FP.Feq(testTarget.componentDesc[i][0], currentTarget.componentDesc[i][0])) return false;
//		}
//		return true;
//	}
	
	public bool TestWidthsMatchWithGaps(AVOWCircuitTarget testTarget){
		// make a list of the goals we are trying to reach so we can cross them off as we go
		List<Vector3> goalList = new List<Vector3>();
		foreach (Vector3 vals in currentTarget.componentDesc){
			goalList.Add(vals);
		}
		
		foreach (Vector3 vals in testTarget.componentDesc){	
			// See if we have it in our goal list
			int removalIndex = -1;
			for (int i = 0; i < goalList.Count; ++i){
				if (MathUtils.FP.Feq (goalList[i][0], vals[0])){
					removalIndex = i;
					break;
				}
			}
			if (removalIndex != -1){
				goalList.RemoveAt(removalIndex);
			}
		}
		
		return goalList.Count == 0;
	}
	
//	public bool TestPositionMatch(AVOWCircuitTarget testTarget){		
//		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
//			
//			if (!MathUtils.FP.Feq (currentCovers[displayToCoversMapping[i]].transform.localPosition.x, displayTarget.componentDesc[i][1])){
//				return false;
//				
//			}
//			if (!MathUtils.FP.Feq (currentCovers[displayToCoversMapping[i]].transform.localPosition.y, displayTarget.componentDesc[i][2])){
//				return false;
//				
//			}
//		}
//		return true;
//	}
	
	int[] CreateIdentityMapping(int length){
		int[] mapping = new int[length];
		for (int i = 0; i < length; ++i){
			mapping[i] = i;
		}
		return mapping;
	}
	
	// This Target might just contain a subset of the blocks, say, N of them - we reorder our 
	// covers so that the sizes of the first N match those in target
	void ReorderCoversToMatch(AVOWCircuitTarget target){
		GameObject[] newCovers = new GameObject[currentCovers.Length];
		
		int coversIndex = 0;
		for (int i = 0; i < target.componentDesc.Count; ++i){	
			float targetSize = target.componentDesc[i][0];
			int removalIndex = -1;
			for (int j = 0; j < currentCovers.Length; ++j){
				float coverSize = currentCovers[j].transform.localScale.x;
				if (MathUtils.FP.Feq (coverSize, targetSize)){
					newCovers[coversIndex++] = currentCovers[j];
					removalIndex = j;
					break;
				}
			}
			if (removalIndex == -1){
				Debug.LogError (" (removalIndex != -1)");
			}
			// Remvoe it (a bit hacky)
			List<GameObject> foos = new List<GameObject>(currentCovers);
			foos.RemoveAt(removalIndex);
			currentCovers = foos.ToArray();
		}
		
		// Now add in the remaining covers (so we are still keeping track of them all)
		foreach (GameObject cover in currentCovers){
			newCovers[coversIndex++] = cover;
		} 
		currentCovers = newCovers;
		
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
			float squareSize = currentCovers[testTargetToCoverMapping[i]].transform.localScale.x;
			Vector3 localCoverPos = TransformPosToGamePos(currentCovers[testTargetToCoverMapping[i]].transform.localPosition, squareSize, target.totalCurrent);
			float coverPosX = localCoverPos.x;
			float coverPosY = localCoverPos.y;
			
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
	
	public void RenderUpdate(){
		UpdateShadedSquare();
	}
	
	// Update is called once per frame
	public void GameUpdate () {

		
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
			float squareSize = currentCovers[displayToCoversMapping[i]].transform.localScale.x;
			Vector3 localPos = TransformPosToGamePos( currentCovers[displayToCoversMapping[i]].transform.localPosition,  squareSize, displayTarget.totalCurrent);
			float coverPosX = localPos.x;
			float coverPosY = localPos.y;
			float coverPosZ = localPos.z;
			
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
			currentCovers[displayToCoversMapping[i]].transform.localPosition = GamePosToTransformPos(new Vector3(coverPosX, coverPosY, coverPosZ), squareSize, displayTarget.totalCurrent);
		}		
		
		
		return finished;
	}
	
	bool UpdateRemoveHidden(){
	
		if (currentCovers.Length == displayTarget.componentDesc.Count) return true;
		
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		
		
		// Get a list of all the covers which are not accounted for in the displayToCovers mapping
		bool[] coversRemove = new bool[currentCovers.Length];
		for (int i = 0; i < coversRemove.Length; ++i){
			coversRemove[i] = true;
		}
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			coversRemove[displayToCoversMapping[i]] = false;
		}
		float maxHeight = -100f;
		for (int i = 0; i < currentCovers.Length; ++i){
			if (coversRemove[i]){
				currentCovers[i].transform.localPosition -= new Vector3(0, deltaDist, 0);
				maxHeight = Mathf.Max(maxHeight, currentCovers[i].transform.localPosition.y);
				
			}
		}
		
		return ( maxHeight < -2);
		
		
	}
	
	bool UpdateMoveToTarget(){

		if (horizontalFirst){
			bool finished = UpdateRemoveHidden();
			if (finished){
				finished = UpdateMoveToTargetHorzontal();
			}
			if (finished){
				finished =  UpdateMoveToTargetVertical();
			}
			if (finished){
				if (currentCovers.Length != displayTarget.componentDesc.Count){
					CreateCovers(displayTarget);
				}
			}
			return finished;
		}
		else{
			bool finished = UpdateRemoveHidden();
			if (finished){
				finished = UpdateMoveToTargetVertical();
			}
			if (finished){
				finished =  UpdateMoveToTargetHorzontal();
			}
			if (finished){
				if (currentCovers.Length != displayTarget.componentDesc.Count){
					CreateCovers(displayTarget);
				}
			}
			return finished;
		}
	}
	
	
	bool UpdateMoveToTargetVertical(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		bool finished = true;
		
		for (int i = 0; i < displayTarget.componentDesc.Count; ++i){
			float squareSize = currentCovers[displayToCoversMapping[i]].transform.localScale.x;
			Vector3 localPos = TransformPosToGamePos( currentCovers[displayToCoversMapping[i]].transform.localPosition,  squareSize, displayTarget.totalCurrent);
			
			float coverPosX = localPos.x;
			float coverPosY = localPos.y;
			float coverPosZ = localPos.z;
			
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
			currentCovers[displayToCoversMapping[i]].transform.localPosition = GamePosToTransformPos(new Vector3(coverPosX, coverPosY, coverPosZ), squareSize, displayTarget.totalCurrent);
		}
		return finished;
		

	}

	bool UpdateMoveToComplete(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		
		//Vector3 coverPos = TransformPosToGamePos (currentCovers[displayToCoversMapping[0]].transform.localPosition, currentCovers[displayToCoversMapping[0]].transform.localScale.x, displayTarget.totalCurrent);
		Vector3 coverPos = currentCovers[displayToCoversMapping[0]].transform.localPosition;
		float maxDistToMove = renderBackwards ?  Mathf.Abs (coverPos.x - (displayTarget.componentDesc[0][1]) + transform.position.x) :  Mathf.Abs (coverPos.x - (displayTarget.componentDesc[0][1]- displayTarget.totalCurrent));
		
		if (MathUtils.FP.Feq(maxDistToMove, 0)){
			return true;
		}
		
		for (int i = 0; i < currentCovers.Length; ++i){
			Vector3 pos = TransformPosToGamePos (currentCovers[i].transform.localPosition, currentCovers[i].transform.localScale.x, displayTarget.totalCurrent);
			if (renderBackwards){
				pos.x += Mathf.Min (deltaDist, maxDistToMove);
			}
			else{
				pos.x -= Mathf.Min (deltaDist, maxDistToMove);
			}
			currentCovers[i].transform.localPosition = GamePosToTransformPos(pos, currentCovers[i].transform.localScale.x, displayTarget.totalCurrent);
			
		}	
		return false;
	}
	
	bool DroppingOff(){
		float deltaDist = coverMoveSpeed * Time.deltaTime;
		float minHeight = 1;
		foreach (GameObject go in currentCovers){
			if (go == null) continue;
			go.transform.localPosition -= new Vector3(0, deltaDist, 0);
			minHeight = Mathf.Min (minHeight, go.transform.localPosition.y);
		}	
		return (minHeight < -1);
	}
	
}
