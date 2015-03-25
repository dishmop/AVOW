using UnityEngine;
using System.Collections;

public class AVOWSceneConstructor : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject notchedStripPrefabUpper;
	public GameObject notchedStripPrefabLower;
	public GameObject blackStripPrefab;

	
	public int wallWidth;
	public int wallHeight;
	

	
	
	// Use this for initialization
	void Start () {
		int halfWidth = wallWidth/2;
		int halfheight = wallHeight/2;
		
		for (int x = 0; x < wallWidth; ++x){
			for (int y = 0; y < wallHeight; ++y){
				int xPos = (x - halfWidth);
				int yPos = (y - halfheight);
				
				if (xPos < 0 || yPos != 0){
					GameObject newWall = GameObject.Instantiate(wallPrefab);
					newWall.transform.parent = transform;
					newWall.transform.localPosition = new Vector3((float)xPos, (float)yPos, wallPrefab.transform.position.z);
				}
			}
		}
		
		// Find the stub for the upper strip prefabs
		Transform upperStripStub = transform.FindChild("NotchedStripStubUpper");
		for (int x = 0; x < halfWidth; ++x){
			GameObject newStripUnit = GameObject.Instantiate(notchedStripPrefabUpper);
			newStripUnit.transform.parent = upperStripStub.transform;
			newStripUnit.transform.localPosition = new Vector3((float)x, 0, notchedStripPrefabUpper.transform.position.z);
		}
	
		// Find the stub for the lower strip prefabs
		Transform lowerStripStub = transform.FindChild("NotchedStripStubLower");
		for (int x = 0; x < halfWidth; ++x){
			GameObject newStripUnit = GameObject.Instantiate(notchedStripPrefabLower);
			newStripUnit.transform.parent = lowerStripStub.transform;
			newStripUnit.transform.localPosition = new Vector3((float)x, 0, notchedStripPrefabLower.transform.position.z);
		}	
		
		
		// Find the stub for the upper strip prefabs
		Transform upperBlackStripStub = transform.FindChild("BlackStripStubUpper");
		for (int x = 0; x < halfWidth; ++x){
			GameObject newStripUnit = GameObject.Instantiate(blackStripPrefab);
			newStripUnit.transform.parent = upperBlackStripStub.transform;
			newStripUnit.transform.localPosition = new Vector3((float)x, 0, blackStripPrefab.transform.position.z);
		}
		
		// Find the stub for the lower strip prefabs
		Transform lowerBlackStripStub = transform.FindChild("BlackStripStubLower");
		for (int x = 0; x < halfWidth; ++x){
			GameObject newStripUnit = GameObject.Instantiate(blackStripPrefab);
			newStripUnit.transform.parent = lowerBlackStripStub.transform;
			newStripUnit.transform.localPosition = new Vector3((float)x, 0, blackStripPrefab.transform.position.z);
		}	
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
