using UnityEngine;
using System.Collections;

public class BabyBlueParent : MonoBehaviour {

	public GameObject parent0;
	public GameObject parent1;
	
	public GameObject lighteningPrefab;
	
	public float sizeMul = 0;
	public Vector3	vel = Vector3.zero;
	
	public bool isActive  = true;
	
	public float lerpVal = 1;
	
	public bool updateSize = true;
	
	
//	Vector3 debugStartPos = new Vector3(0, 0, 210);
//	Vector3 debugEndPos = new Vector3(0, 10, 210);
	
	
	bool enableGrow = false;
	
	float underlyingSize;
	
//	GameObject[] squareSpheres;
	GameObject[] squareLightening = new GameObject[8];
	
	
	
	GameObject lightening0;
	GameObject lightening1;
	bool lightingCreated = false;
	
	public float rotSpeed = 3;
	public float rotSpeed2 = 0;
	
	bool isFalling; 
	
	// Use this for initialization
	void Start () {
//	
//		squareLightening[0] = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;
//		
//		
//		squareLightening[0].GetComponent<Lightening>().startPoint = debugStartPos;
//		squareLightening[0].GetComponent<Lightening>().endPoint = debugEndPos;
//		squareLightening[0].GetComponent<Lightening>().size = 1;
//		squareLightening[0].GetComponent<Lightening>().numStages = 5;
//		squareLightening[0].GetComponent<Lightening>().ConstructMesh();
	
	}
	
	public void StopSquareLightening(){
		foreach(GameObject go in squareLightening){
			GameObject.Destroy(go);
		}
	}
	
	public void SetFall(){
		isFalling = true;
	}
	
	
	public void Land(){
		isFalling = false;
		vel = Vector3.zero;
	}
	
	
	public void StartGrowing(){
		enableGrow = true;
	}
	
	public void CreateLightening(){
		lightening0 = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;

		
		lightening1 = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		lightingCreated = true;
		
	}
	
	public bool IsGrown(){
		return sizeMul >= 1f;
	}
	
	public void SetToActive(){
		isActive = true;
		lerpVal = 0;
	}
	
	public void NullParents(){
		parent0 = null;
		parent1 = null;
	}
	
	
	void UpdateLightening(){
		if (lightening0 != null){
			Vector3 fromHereToSphere = parent0.transform.position - transform.position;
			fromHereToSphere.Normalize();
			lightening0.GetComponent<Lightening>().startPoint = transform.position + fromHereToSphere * Mathf.Sqrt (3) * 0.5f * transform.localScale.x;
			lightening0.GetComponent<Lightening>().endPoint = parent0.transform.position - fromHereToSphere * 0.5f * parent0.transform.localScale.x;
			lightening0.GetComponent<Lightening>().size = 4 * underlyingSize;
			lightening0.GetComponent<Lightening>().numStages = 5;
			lightening0.GetComponent<Lightening>().ConstructMesh();
			lightening0.name = "CubeLightening0";
			
		}
		if (lightening1 != null){
			Vector3 fromHereToSphere = parent1.transform.position - transform.position;
			fromHereToSphere.Normalize();
			lightening1.GetComponent<Lightening>().startPoint = transform.position + fromHereToSphere * Mathf.Sqrt (3) * 0.5f * transform.localScale.x;
			lightening1.GetComponent<Lightening>().endPoint = parent1.transform.position - fromHereToSphere * 0.5f * parent1.transform.localScale.x;
			lightening1.GetComponent<Lightening>().size = 4 * underlyingSize;
			lightening1.GetComponent<Lightening>().numStages = 5;
			lightening1.GetComponent<Lightening>().ConstructMesh();
			lightening1.name = "CubeLightening1";
			
		}
	}
	
	public void DestroyLightening(){
		GameObject.Destroy(lightening0);
		lightening0  = null;
		GameObject.Destroy(lightening1);
		lightening1  = null;
	}
	
	
	public void UpdateSquareLightening(){
		Quaternion rot = transform.FindChild ("BabyBlueCube").rotation;
		Vector3[] corners = new Vector3[8];
		float size = 0.5f * transform.localScale.x;
		corners[0] = size * new Vector3(1, 1, 1);
		corners[1] = size * new Vector3(1, 1, -1);
		corners[2] = size * new Vector3(1, -1, 1);
		corners[3] = size * new Vector3(1, -1, -1);
		corners[4] = size * new Vector3(-1, 1, 1);
		corners[5] = size * new Vector3(-1, 1, -1);
		corners[6] = size * new Vector3(-1, -1, 1);
		corners[7] = size * new Vector3(-1, -1, -1);
		
		for (int i = 0; i < 8; ++i){
			corners[i] = transform.position + rot * corners[i] ;
		}
		
		for (int i = 0; i < 8; ++i){
			
			squareLightening[i].GetComponent<Lightening>().startPoint = corners[i];
			squareLightening[i].GetComponent<Lightening>().ConstructMesh();
			
		}
		
		
	}
	
	
	
	public void Electrify(GameObject[] spheres){
		Quaternion rot = transform.FindChild ("BabyBlueCube").rotation;
		
//		squareSpheres = spheres;
		Vector3[] corners = new Vector3[8];
		float size = 0.5f * transform.localScale.x;
		corners[0] = size * new Vector3(1, 1, 1);
		corners[1] = size * new Vector3(1, 1, -1);
		corners[2] = size * new Vector3(1, -1, 1);
		corners[3] = size * new Vector3(1, -1, -1);
		corners[4] = size * new Vector3(-1, 1, 1);
		corners[5] = size * new Vector3(-1, 1, -1);
		corners[6] = size * new Vector3(-1, -1, 1);
		corners[7] = size * new Vector3(-1, -1, -1);
		
		for (int i = 0; i < 8; ++i){
			corners[i] = transform.position + rot * corners[i] ;
		}
		
		for (int i = 0; i < 8; ++i){
		
			Vector3 fromSphereToCube = transform.position - spheres[i].transform.position;
			fromSphereToCube.Normalize();
			Vector3 toPos = spheres[i].transform.position + 0.5f * spheres[i].transform.localScale.x * fromSphereToCube;

			squareLightening[i] = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			
			
			squareLightening[i].GetComponent<Lightening>().startPoint = corners[i];
			squareLightening[i].GetComponent<Lightening>().endPoint = toPos;
			squareLightening[i].GetComponent<Lightening>().size = 3;
			squareLightening[i].GetComponent<Lightening>().numStages = 15;
			squareLightening[i].GetComponent<Lightening>().ConstructMesh();
			squareLightening[i].name = "SquareLightening" + i.ToString();
			
		}
		
		transform.FindChild ("BabyBlueCube").GetComponent<BabyBlueCube>().ReinitialiseRotation();
		
	}
	
	
	// Update is called once per frame
	public void Update () {
		transform.FindChild ("BabyBlueCube").GetComponent<BabyBlueCube>().SetRotSpeed(rotSpeed);
		transform.FindChild ("BabyBlueCube").GetComponent<BabyBlueCube>().SetRotSpeed2(rotSpeed2);
		
		// Debug
//		if (squareLightening[0] != null){
//			squareLightening[0].GetComponent<Lightening>().startPoint = debugStartPos;
//			squareLightening[0].GetComponent<Lightening>().endPoint = debugEndPos;
//			squareLightening[0].GetComponent<Lightening>().ConstructMesh();
//			
//		}	
		
		if (!isActive) return;
		
		if (enableGrow)
			sizeMul = Mathf.Min (sizeMul + 0.001f, 1);
		if (parent0 != null && parent1 != null){
			Vector3 from0To1Centres = (parent1.transform.position - parent0.transform.position);
			float centreDist = from0To1Centres.magnitude;
			
			Vector3 from0To1Norm = from0To1Centres.normalized;
			Vector3 from0CentreTo0Edge = 0.5f * from0To1Norm * parent0.transform.localScale.x;
			Vector3 from0To1Edges = from0To1Norm * (centreDist - 0.5f * (parent0.transform.localScale.x + parent1.transform.localScale.x));
			Vector3 midPoint = parent0.transform.position + from0CentreTo0Edge + 0.5f * from0To1Edges;

			
			transform.position = Vector3.Lerp (transform.position, midPoint, lerpVal);
			Vector3 lookDir = parent0.transform.position - transform.position;
			transform.rotation = Quaternion.LookRotation(lookDir, new Vector3(0, 0, -1));
			
			if (updateSize){
				underlyingSize = 0.25f * from0To1Edges.magnitude * 0.5f;
				float scale =  underlyingSize * sizeMul;
				transform.localScale = new Vector3(scale, scale, scale);
			}
		}
		
		lerpVal = Mathf.Min (lerpVal + 0.001f, 1);
		
		if (!lightingCreated){
			CreateLightening();
		}
		UpdateLightening();
		
		
		transform.position += vel * Time.deltaTime;
		
		if (squareLightening[0] != null){
			rotSpeed = Mathf.Max(rotSpeed - 0.01f, 0f);
			rotSpeed2 = Mathf.Max(rotSpeed2 - 0.01f, 0f);
			
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, 0.01f);
			UpdateSquareLightening();
		}
		
		if (isFalling){
			vel.y -= 3.81f * Time.fixedDeltaTime;
		}
		
		
		
	
	}
	
}
