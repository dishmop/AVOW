using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectives : MonoBehaviour {
	public static AVOWObjectives singleton = null;

	public GameObject		textBoxPrefab;
	public float 			ySpacing;
	public float 			xSpacing;
	public int				maxList = 7;
	public Color			colNotYetDoneAndInactive;
	public Color			colNotYetDoneAndActive;
	public Color			colDone;
	public Color			colDoneAndForgotten;
	public float			xMax = 0;
	public int 				currentObjective = 0;
	public bool				isComplete = false;
	
	SpringValue				yOffset = new SpringValue(0, SpringValue.Mode.kAsymptotic);
	
	List<GameObject>[]		textBoxes;
	Vector3 				basePos;
	
	bool 					lastUseLCM;
	bool					lastShowTotals;
	bool					lastShowIndividuals;
	bool 					lastShowObjectives;
	bool					firstTime = true;
	bool					lastObjectivesAsText;
	
	int						lastObjective = -1;
	
	// Use this for initialization
	void Start () {
		basePos = transform.position;
	
	}
	
	public void Restart(){
		firstTime = true;
		currentObjective = 0;
		isComplete = false;
		yOffset.Force(0);
		if (AVOWConfig.singleton.maxNumResistors > 0)
			CreateTextBoxes();
	
	}
	
	bool TestForConfigChange(){
		bool ret = false;
		if (firstTime) ret = true;
		if (lastUseLCM != AVOWConfig.singleton.useLCM) ret = true;
		if (lastShowTotals != AVOWConfig.singleton.showTotals) ret = true;
		if (lastShowIndividuals != AVOWConfig.singleton.showIndividuals) ret = true;
		if (lastShowObjectives != AVOWConfig.singleton.showObjectives) ret = true;
		if (lastObjectivesAsText != AVOWConfig.singleton.objectivesAsText) ret = true;
		lastUseLCM = AVOWConfig.singleton.useLCM;
		lastShowTotals = AVOWConfig.singleton.showTotals;
		lastShowIndividuals = AVOWConfig.singleton.showIndividuals;
		lastShowObjectives = AVOWConfig.singleton.showObjectives;
		lastObjectivesAsText = AVOWConfig.singleton.objectivesAsText;
		firstTime = false;
		return ret;
		
	}
	
	// Update is called once per frame
	void Update () {
		if (AVOWConfig.singleton.maxNumResistors == 0){
			ProcessChangeToConfig();
			return;
		}
		if (AVOWCircuitCreator.singleton.IsFinished()){
			if (textBoxes == null){
				CreateTextBoxes();
			}
			else{
				isComplete = TestForComplete();
				ProcessChangeToObjective();
				ProcessChangeToConfig();
				SetColors();
				yOffset.Update();
				Vector3 pos = transform.position;
				transform.position = new Vector3(pos.x, basePos.y+yOffset.GetValue(), pos.z);
			}
			CalcBounds();
		}
		if (isComplete){
			AVOWBattery.singleton.FreezeBattery();
		}
	
	}
	
	void CalcBounds(){
		if (!AVOWConfig.singleton.ShowTextObjectives()){
			xMax = transform.position.x;
		}
		else if (AVOWConfig.singleton.showIndividuals)
			xMax = transform.position.x + xSpacing * ( AVOWConfig.singleton.maxNumResistors);
		else if (!AVOWConfig.singleton.showIndividuals && AVOWConfig.singleton.showTotals)
			xMax = transform.position.x + xSpacing * ( 1);
		else 
			xMax = transform.position.x;
	
	}
	
	public Eppy.Tuple<float, List<float>> GetCurrentGoal(){

		return AVOWCircuitCreator.singleton.GetResults()[currentObjective];
	}
	
	public bool IsValidGoalIndex(int index){
		return (index >= 0 && index < AVOWCircuitCreator.singleton.GetResults().Count);
	}
	
	public Eppy.Tuple<float, List<float>> GetGoal(int index){
		
		return AVOWCircuitCreator.singleton.GetResults()[index];
	}
	
	void IsAchieving(Eppy.Tuple<float, List<float>> goal, out bool total, out bool[] individual){
		individual = new bool[goal.Item2.Count];
		if (AVOWGraph.singleton.HasHalfFinishedComponents()){
			total = false;
			return;
		}
		
		total = MathUtils.FP.Feq(goal.Item1, AVOWCircuitCreator.singleton.GetLCM() * AVOWSim.singleton.xMax);
		
		List<float> currentVals = new List<float>();
		foreach(GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kLoad) currentVals.Add (component.hWidth);
		}
		
		for (int i = 0; i < goal.Item2.Count; ++i){
			float target = goal.Item2[i];
			int remIndex = -1;
			for (int j = 0; j < currentVals.Count; ++j){
				float testVal = AVOWCircuitCreator.singleton.GetLCM() * currentVals[j];
				if (MathUtils.FP.Feq (target, testVal)){
					remIndex = j;
					break;
				}
			}
			if (remIndex != -1){
				individual[i] = true;
				currentVals.RemoveAt(remIndex);
			}
		}
		

		
	}
	
	void CreateTextBoxes(){
		
		DestroyBoxes();
		textBoxes = new List<GameObject>[AVOWCircuitCreator.singleton.GetResults().Count];
		
		List<Eppy.Tuple<float, List<float>>> results = AVOWCircuitCreator.singleton.GetResults();
		for (int i = 0; i < textBoxes.Length; ++i){
			if (Mathf.Abs(i - currentObjective) > maxList) continue;
			
			Color thisCol;
			if (i == currentObjective){
				thisCol = colNotYetDoneAndActive;
			}
			else if (i < currentObjective){
				thisCol	= colDoneAndForgotten;
			}
			else{
				thisCol = colNotYetDoneAndInactive;
			}
	
			
			textBoxes[i] = new List<GameObject>();
			GameObject newBox = GameObject.Instantiate(textBoxPrefab, transform.position + new Vector3(0, -ySpacing * i, 0), Quaternion.identity) as GameObject;
			
			newBox.transform.parent = transform;
			newBox.GetComponent<TextMesh>().text = "(" + CreateFracString(results[i].Item1) + ")";
			newBox.GetComponent<TextMesh>().color = thisCol;
			textBoxes[i].Add(newBox);
						
			for (int j = 0; j < results[i].Item2.Count; ++j){
				GameObject newBox2 = GameObject.Instantiate(textBoxPrefab, transform.position + new Vector3(xSpacing * (j + 1), -ySpacing * i, 0), Quaternion.identity) as GameObject;
				
				newBox2.transform.parent = transform;
				newBox2.GetComponent<TextMesh>().text = CreateFracString(results[i].Item2[j]);
				newBox2.GetComponent<TextMesh>().color = thisCol;
				
				textBoxes[i].Add(newBox2);
			}
		}
	}
	
	void DestroyBoxes(){
		if (textBoxes == null) return;
		
		foreach (List<GameObject> entry in textBoxes){
			if (entry != null){
				foreach (GameObject go in entry){
					GameObject.Destroy(go);
				}
			}
		}
		textBoxes = null;
	}
	
	void ProcessChangeToConfig(){
		if (TestForConfigChange()){
			// The LCM usage might have chnaged - so remake
			CreateTextBoxes();
			
			// Set which ones are visible
			for (int i = 0; i < textBoxes.Length; ++i){
				if (textBoxes[i] == null) continue;
				textBoxes[i][0].SetActive(AVOWConfig.singleton.showTotals && AVOWConfig.singleton.ShowTextObjectives());
				for (int j = 1; j < textBoxes[i].Count; ++j){
					textBoxes[i][j].SetActive(AVOWConfig.singleton.showIndividuals && AVOWConfig.singleton.ShowTextObjectives());
				}
			}
			
		}
	}
	
	void ProcessChangeToObjective(){
		if (lastObjective != currentObjective){
			int maxObjective = AVOWCircuitCreator.singleton.GetResults().Count-1;
			if (currentObjective > maxObjective){
				currentObjective = maxObjective;
				AVOWGameModes.singleton.SetStageComplete();
			}
			else{
				yOffset.Set(yOffset.GetDesValue() + ySpacing * (currentObjective - lastObjective));
			}
			lastObjective = currentObjective;
			// Pretend firs ttime so we re calc everything
			firstTime = true;
		}
	}
	
	bool TestForComplete(){

		bool total;
		bool[] individual;
		IsAchieving(AVOWCircuitCreator.singleton.GetResults()[currentObjective], out total, out individual);
		
		if (!total) return false;


				for (int i = 1; i < textBoxes[currentObjective].Count; ++i){
			if (!individual[i-1]){
				return false;
			}
		}
		
		// Is complete
		currentObjective++;
		return true;
	}
	
	void SetColors(){
		bool total;
		bool[] individual;
		IsAchieving(AVOWCircuitCreator.singleton.GetResults()[currentObjective], out total, out individual);
		
		if (total){
			textBoxes[currentObjective][0].GetComponent<TextMesh>().color = colDone;
		}
		else{
			textBoxes[currentObjective][0].GetComponent<TextMesh>().color = colNotYetDoneAndActive;
		
		}
		for (int i = 1; i < textBoxes[currentObjective].Count; ++i){
			if (individual[i-1]){
				textBoxes[currentObjective][i].GetComponent<TextMesh>().color = colDone;
			}
			else{
				textBoxes[currentObjective][i].GetComponent<TextMesh>().color = colNotYetDoneAndActive;
			}
		}

	}
	

//	
//	string CreateText(Eppy.Tuple<float, List<float>> goal){
//		string text = CreateFracString(goal.Item1) + ": ";
//		for (int i = 0; i < goal.Item2.Count; ++i){
//			text += CreateFracString(goal.Item2[i]);
//			text += "     ";
//			
//		}
//		return text;
//	
//	}
	
	string CreateFracString(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		//val *= AVOWCircuitCreator.singleton.currentLCM;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return (integer * denominator + numerator).ToString() + (MathUtils.FP.Feq (denominator, 1) ? "" : "/" + denominator.ToString() );
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
	}
		
}
