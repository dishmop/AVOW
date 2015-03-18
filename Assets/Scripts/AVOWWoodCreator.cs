using UnityEngine;
using System.Collections;

public class AVOWWoodCreator : MonoBehaviour {

	public static AVOWWoodCreator singleton = null;

	public GameObject[] woodPrefabs;
	
	GameObject[] currentWood;
	
	
	public void Construct(int division, int width){
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
			currentWood[i].transform.localPosition = new Vector3(i+1, 0, 0);
		}
		
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
}
