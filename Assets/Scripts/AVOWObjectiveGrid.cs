using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectiveGrid : MonoBehaviour {
	public static AVOWObjectiveGrid singleton = null;
	
	public GameObject drawnLingPrefab;
	public float 	  xMax = 0;
	
	// For some reason testing the lines themselves doesn't work
	float				finishTime;
	

	int thisLCM = 1;
	bool lastVis = false;
	int lastLCM = 1;
	int thisWidth = 1;
	int lastWidth = 1;
	
	
	void CalcBounds(){
		if (AVOWConfig.singleton.ShowGraphicObjectives()){
			xMax = transform.position.x + ((float)thisWidth) / (float)thisLCM;
		}
		else{
			xMax = transform.position.x;
		}
	}

	// Use this for initialization
	void Start () {
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (AVOWCircuitCreator.singleton.IsFinished()){
			thisLCM = CalcLCM();
			thisWidth = CalcWidth() + 1;
			if (lastVis != AVOWConfig.singleton.ShowGraphicObjectives() || lastLCM != thisLCM || lastWidth != thisWidth){
				DestroyGrid();
				DrawGrid (thisLCM, thisWidth);
				lastVis = AVOWConfig.singleton.ShowGraphicObjectives();
				lastLCM = thisLCM;
				lastWidth = thisWidth;
			}
			CalcBounds();
		}
	
	}
	
	int CalcLCM(){
		if (!AVOWObjectives.singleton.IsValidGoalIndex(AVOWVizObjectives.singleton.displayedObjective)) return 1;
		Eppy.Tuple<float, List<float>> goal = AVOWObjectives.singleton.GetGoal(AVOWVizObjectives.singleton.displayedObjective);
		
		int lcm = CalcDenominator(goal.Item1);
		foreach (float val in goal.Item2){
			lcm = MathUtils.FP.lcm(CalcDenominator(val), lcm);
		}
		return lcm;
	}
	
	int CalcWidth(){
		if (!AVOWObjectives.singleton.IsValidGoalIndex(AVOWVizObjectives.singleton.displayedObjective)) return 1;
		Eppy.Tuple<float, List<float>> goal = AVOWObjectives.singleton.GetGoal(AVOWVizObjectives.singleton.displayedObjective);
		
		int width = 0;
		foreach (float val in goal.Item2){
			width += CalcNumerator(val, thisLCM);
		}
		return width;
	}
	
	int CalcDenominator(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return denominator;
	}
	
	int CalcNumerator(float val, int useDenom){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return (useDenom /denominator) *  (numerator + integer * denominator);
	}
	
	
	void DestroyGrid(){
		for (int i = 0; i < transform.childCount; ++i){
			GameObject.Destroy(transform.GetChild(i).gameObject);
		}
	}
	
	public bool IsFinished(){
		return (finishTime < Time.time);
//		foreach(Transform child in transform){
//			if (!child.GetComponent<DrawnLine>().IsFinished()) return true;
//		}
//		return false;
	}
	
	void DrawGrid(int div, int length){
		if (!AVOWConfig.singleton.ShowGraphicObjectives()) return;
		
		float xLen = (float)length / (float)div;
		
		for (int i = 0; i < div + 1; ++i){
			DrawLine(0,  i /(float)div, xLen,  i / (float)div);
		}
		
		for (int i = 0; i < length; ++i){
			DrawLine(i /(float)div,  0, i /(float)div,  1);
		}
		
		finishTime = Time.time + 0.5f;
		
		
	}
	
	void DrawLine(float x0, float y0, float x1, float y1){
		GameObject newLine = GameObject.Instantiate(drawnLingPrefab) as GameObject;
		newLine.transform.parent = transform;
		newLine.GetComponent<DrawnLine>().Draw(new Vector2(x0, y0), new Vector2(x1, y1), new Color(64/255.0f, 64/255.0f, 64/255.0f));
	}
}
