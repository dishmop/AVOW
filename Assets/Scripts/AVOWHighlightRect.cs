using UnityEngine;
using System.Collections;

public class AVOWHighlightRect : MonoBehaviour {

	public GameObject barObj;
	public Vector3 bottomLeft;
	public Vector3 topRight;
	
	float timeAtTrigger = -100;
	public float fadeDuration = 1;
	bool triggeredAlready;
	
	GameObject[] sides = new GameObject[4];
	
	public void Trigger(Vector3 bottomLeft, Vector3 topRight){
		this.bottomLeft = bottomLeft;
		this.topRight = topRight;
		timeAtTrigger = AVOWUpdateManager.singleton.GetGameTime();
		PositionSquare();
		triggeredAlready = false;
		
		
	}
	
	float CalcBrightness(){
		// should only ever trigger once
		if (triggeredAlready) return 0;

		float thisTime = AVOWUpdateManager.singleton.GetGameTime();
		if (thisTime < timeAtTrigger) return 0;
		if (thisTime > timeAtTrigger + fadeDuration){
			triggeredAlready = true;
			return 0;
		}
		return 0.5f - 0.5f * Mathf.Cos(2 * 3.14159f * (thisTime - timeAtTrigger) / fadeDuration);
		
	}


	
	void ConstructSquare(){
		for (int i = 0; i < 4; ++i){
			sides[i] = GameObject.Instantiate(barObj);
			sides[i].transform.parent = transform;
		}
	
	}
	
	void PositionSquare(){
		// Make other corners
		Vector3 locBottomLeft = Vector3.zero;
		Vector3 locTopRight = topRight - bottomLeft;
		Vector3 locBottomRight = new Vector3(locTopRight.x, locBottomLeft.y, 0.5f * (locTopRight.z + locBottomLeft.z));
		Vector3 locTopLeft = new Vector3(locBottomLeft.x, locTopRight.y, 0.5f * (locTopRight.z + locBottomLeft.z));
		
		// Place ourselves at the bottom left corner
		transform.position = bottomLeft;
		
		// Left side
		sides[0].transform.localPosition = locBottomLeft + new Vector3(0.5f, 0, 0);
		sides[0].transform.localRotation = Quaternion.Euler(0, 0, 90);
		sides[0].transform.localScale = new Vector3(locTopRight.y, 1, 0);
		sides[0].GetComponent<Renderer>().material.SetFloat("_GapProp", 1);

		// Right side
		sides[1].transform.localPosition = locBottomRight + new Vector3(0.5f, 0, 0);
		sides[1].transform.localRotation = Quaternion.Euler(0, 0, 90);
		sides[1].transform.localScale = new Vector3(locTopRight.y, 1, 0);
		sides[1].GetComponent<Renderer>().material.SetFloat("_GapProp", 1);
		

		// Bottom side
		sides[2].transform.localPosition = locBottomLeft + new Vector3(0, -0.5f, 0);
		sides[2].transform.localRotation = Quaternion.identity;
		sides[2].transform.localScale = new Vector3(locTopRight.x, 1, 0);
		sides[2].GetComponent<Renderer>().material.SetFloat("_GapProp", 1);
		
		
		// top side
		sides[3].transform.localPosition = locTopLeft + new Vector3(0, -0.5f, 0);
		sides[3].transform.localRotation = Quaternion.identity;
		sides[3].transform.localScale = new Vector3(locTopRight.x, 1, 0);
		sides[3].GetComponent<Renderer>().material.SetFloat("_GapProp", 1);
		
	}
	
	public bool IsFinished(){
		float thisTime = AVOWUpdateManager.singleton.GetGameTime();
		
		return (thisTime > timeAtTrigger + fadeDuration);
	}

	// Use this for initialization
	void Start () {
		ConstructSquare();
	
	}
	
	// Update is called once per frame
	void Update () {
		float brightness = CalcBrightness();
		foreach (GameObject go in sides){
			go.GetComponent<Renderer>().material.SetFloat("_IntensityFull", brightness);
		}

	
	}
}
