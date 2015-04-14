using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProgressPanel : MonoBehaviour {
	public static ProgressPanel singleton = null;
	
	public GameObject darkSpherePrefab;
	public GameObject litSpherePrefab;
	
	
	int totalGoals;
	int numCompleteGoals;
	
	int lastTotalGoals;
	int lastNumCompleteGoals;
	
	Vector2 oldPos = Vector2.zero;

	// Use this for initialization
	void Start () {
		totalGoals = 0;
		numCompleteGoals = 0;
		lastTotalGoals = 0;
		lastNumCompleteGoals = 0;
		Rebuild();
	
	}
	
	public void IncCompleteGoals(){
		numCompleteGoals++;
	}
	
	
	public void SetTotalGoals(int total){
		totalGoals = total;
	}
	
	public void SetGoals(int total, int numComplete){
		totalGoals = total;
		numCompleteGoals = numComplete;
	}
	
	
	// Update is called once per frame
	void Update () {
		if (totalGoals != lastTotalGoals || numCompleteGoals != lastNumCompleteGoals){
			Rebuild();
			lastTotalGoals = totalGoals;
			lastNumCompleteGoals = numCompleteGoals;
			
		}
		Vector2 pos = GetComponent<RectTransform>().anchorMin;
		Vector2 newpos = new Vector2(AVOWConfig.singleton.GetSidePanelFrac() + 0.01f, pos.y);
		if (!MathUtils.FP.Feq ((newpos - oldPos).magnitude, 0)){
			GetComponent<RectTransform>().anchorMin = newpos;
			Rebuild ();
			oldPos = newpos;
			
		}
		
	
	}
	
	void SetupTransform(RectTransform transform, int ordinal){
		RectTransform thisRectTransform = GetComponent<RectTransform>();
		transform.SetParent(thisRectTransform);
		float invAspect = thisRectTransform.rect.height / thisRectTransform.rect.width;
		transform.anchorMin = new Vector2(ordinal * invAspect * 1.2f, 0f);
		transform.anchorMax = new Vector2(ordinal * invAspect * 1.2f + invAspect, 1f);
		transform.offsetMin = Vector2.zero;
		transform.offsetMax = new Vector2(1, 1);
	}
	
	
	void Rebuild(){
		foreach (Transform child in transform){
			GameObject.Destroy (child.gameObject);
		}
		for (int i = 0; i < numCompleteGoals; ++i){
			GameObject newObj = GameObject.Instantiate(litSpherePrefab);
			SetupTransform (newObj.GetComponent<RectTransform>(), i);
			
		}
		for (int i = numCompleteGoals; i < totalGoals; ++i){
			GameObject newObj = GameObject.Instantiate(darkSpherePrefab);
			SetupTransform (newObj.GetComponent<RectTransform>(), i);
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
