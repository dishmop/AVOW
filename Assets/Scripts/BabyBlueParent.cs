using UnityEngine;
using System.Collections;

public class BabyBlueParent : MonoBehaviour {

	public GameObject parent0;
	public GameObject parent1;
	
	public GameObject lighteningPrefab;
	
	public float sizeMul = 0;
	bool enableGrow = false;
	
	float underlyingSize;
	
	
	GameObject lightening0;
	GameObject lightening1;
	bool lightingCreated = false;

	// Use this for initialization
	void Start () {

	
	}
	
	public void StartGrowing(){
		enableGrow = true;
	}
	
	public void CreateLightening(){
		lightening0 = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;

		
		lightening1 = GameObject.Instantiate(lighteningPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		
	}
	
	public bool IsGrown(){
		return sizeMul >= 1f;
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
	
	// Update is called once per frame
	void Update () {
		if (enableGrow)
			sizeMul = Mathf.Min (sizeMul + 0.001f, 1);
		if (parent0 != null && parent1 != null){
			Vector3 from0To1Centres = (parent1.transform.position - parent0.transform.position);
			float centreDist = from0To1Centres.magnitude;
			
			Vector3 from0To1Norm = from0To1Centres.normalized;
			Vector3 from0CentreTo0Edge = 0.5f * from0To1Norm * parent0.transform.localScale.x;
			Vector3 from0To1Edges = from0To1Norm * (centreDist - 0.5f * (parent0.transform.localScale.x + parent1.transform.localScale.x));
			Vector3 midPoint = parent0.transform.position + from0CentreTo0Edge + 0.5f * from0To1Edges;

			
			transform.position = midPoint;
			Vector3 lookDir = parent0.transform.position - transform.position;
			transform.rotation = Quaternion.LookRotation(lookDir, new Vector3(0, 0, -1));
			
			underlyingSize = 0.25f * from0To1Edges.magnitude * 0.5f;
			float scale =  underlyingSize * sizeMul;
			transform.localScale = new Vector3(scale, scale, scale);
		}
		
		if (!lightingCreated){
			CreateLightening();
			lightingCreated = true;
		}
		UpdateLightening();
		
		
	}
}
