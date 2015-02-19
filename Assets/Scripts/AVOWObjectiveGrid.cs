using UnityEngine;
using System.Collections;

public class AVOWObjectiveGrid : MonoBehaviour {
	public GameObject drawnLingPrefab;
	
	bool lastVis = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
		if (lastVis != AVOWConfig.singleton.visualiseTargets){
			DestroyGrid();
			DrawGrid (5, 6);
			lastVis = AVOWConfig.singleton.visualiseTargets;
		}
	
	}
	
	void DestroyGrid(){
		while(transform.childCount > 0){
			GameObject.Destroy(transform.GetChild(0).gameObject);
		}
	}
	
	void DrawGrid(int div, int length){
		if (!AVOWConfig.singleton.visualiseTargets) return;
		
		float xLen = (float)length / (float)div;
		
		for (int i = 0; i < div + 1; ++i){
			DrawLine(0,  i /(float)div, xLen,  i / (float)div);
		}
		
		for (int i = 0; i < length; ++i){
			DrawLine(i /(float)div,  0, i /(float)div,  1);
		}
		
		
	}
	
	void DrawLine(float x0, float y0, float x1, float y1){
		GameObject newLine = GameObject.Instantiate(drawnLingPrefab) as GameObject;
		newLine.transform.parent = transform;
		newLine.GetComponent<DrawnLine>().Draw(new Vector2(x0, y0), new Vector2(x1, y1), new Color(64/255.0f, 64/255.0f, 64/255.0f));
	}
}
