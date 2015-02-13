using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectives : MonoBehaviour {

	public GameObject		textBoxPrefab;
	public float 			ySpacing;
	public float 			xSpacing;
	public int				maxList = 7;
	public Color			colNotYetDoneAndInactive;
	public Color			colNotYetDoneAndActive;
	public Color			colDone;
	public Color			colDoneAndForgotten;
	public GameObject		gameOverText;
	
	SpringValue				yOffset = new SpringValue(0, SpringValue.Mode.kAsymptotic);
	
	List<GameObject>[]		textBoxes;
	int 					currentObjective = 0;
	Vector3 				basePos;
	
	// Use this for initialization
	void Start () {
		basePos = transform.position;
	
	}
	
	// Update is called once per frame
	void Update () {
		if (AVOWCircuitCreator.singleton.IsFinished()){
			if (textBoxes == null){
				CreateTextBoxes();
			}
			else{
				SetColors();
				yOffset.Update();
				Vector3 pos = transform.position;
				transform.position = new Vector3(pos.x, basePos.y+yOffset.GetValue(), pos.z);
			}
		}
	
	}
	
	void IsAchieving(Eppy.Tuple<float, List<float>> goal, out bool total, out bool[] individual){
		individual = new bool[goal.Item2.Count];
		if (AVOWGraph.singleton.HasHalfFinishedComponents()){
			total = false;
			return;
		}
		
		total = MathUtils.FP.Feq(goal.Item1, AVOWSim.singleton.xMax);
		
		List<float> currentVals = new List<float>();
		foreach(GameObject go in AVOWGraph.singleton.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			currentVals.Add (component.hWidth);
		}
		
		for (int i = 0; i < goal.Item2.Count; ++i){
			float target = goal.Item2[i];
			int remIndex = -1;
			for (int j = 0; j < currentVals.Count; ++j){
				float testVal = currentVals[j];
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
		textBoxes = new List<GameObject>[AVOWCircuitCreator.singleton.results.Count];
		
		List<Eppy.Tuple<float, List<float>>> results = AVOWCircuitCreator.singleton.results;
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
			newBox.GetComponent<TextMesh>().text = CreateFracString(results[i].Item1);
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
	
	void SetColors(){
		bool total;
		bool[] individual;
		IsAchieving(AVOWCircuitCreator.singleton.results[currentObjective], out total, out individual);
		
		bool IsComplete = true;
		if (total){
			textBoxes[currentObjective][0].GetComponent<TextMesh>().color = colDone;
		}
		else{
			IsComplete = false;
			textBoxes[currentObjective][0].GetComponent<TextMesh>().color = colNotYetDoneAndActive;
		
		}
		
		for (int i = 1; i < textBoxes[currentObjective].Count; ++i){
			if (individual[i-1]){
				textBoxes[currentObjective][i].GetComponent<TextMesh>().color = colDone;
			}
			else{
				textBoxes[currentObjective][i].GetComponent<TextMesh>().color = colNotYetDoneAndActive;
				IsComplete = false;
			}
			
		}
		if (IsComplete){
			if (currentObjective < AVOWCircuitCreator.singleton.results.Count-1){
				currentObjective++;
				yOffset.Set(yOffset.GetDesValue() + ySpacing);
				CreateTextBoxes();
			}
			else{
				gameOverText.SetActive(true);
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
}
