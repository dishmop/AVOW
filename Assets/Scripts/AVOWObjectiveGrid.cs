using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWObjectiveGrid : MonoBehaviour {
	public static AVOWObjectiveGrid singleton = null;
	
	public GameObject drawnLingPrefab;
	public GameObject pusher;
	public float 	  xMax = 0;
	
	// For some reason testing the lines themselves doesn't work
	float				finishTime;
	

	int thisLCM = 1;
	bool lastVis = false;
	int lastLCM = 1;
	int thisWidth = 1;
	int lastWidth = 1;
	
	
	public int GetLCM(){
		return thisLCM;
	}
	
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
		if (AVOWCircuitCreator.singleton.IsReady()){
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
		AVOWCircuitTarget goal = AVOWObjectives.singleton.GetGoal(AVOWVizObjectives.singleton.displayedObjective);
		
		int lcm = CalcDenominator(goal.totalCurrent);
		foreach (float val in goal.individualCurrents){
			lcm = MathUtils.FP.lcm(CalcDenominator(val), lcm);
		}
		return lcm;
	}
	
	int CalcWidth(){
		if (!AVOWObjectives.singleton.IsValidGoalIndex(AVOWVizObjectives.singleton.displayedObjective)) return 1;
		AVOWCircuitTarget goal = AVOWObjectives.singleton.GetGoal(AVOWVizObjectives.singleton.displayedObjective);
		
		int width = 0;
		foreach (float val in goal.individualCurrents){
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

	}
	
	void DrawGrid(int div, int length){
//		AVOWObjectiveManager.singleton.Construct(div, length);

		
		
	}
	

}
