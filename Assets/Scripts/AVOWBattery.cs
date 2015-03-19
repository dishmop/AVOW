using UnityEngine;
using System.Collections;

public class AVOWBattery : MonoBehaviour {
	public static AVOWBattery singleton = null;

	public GameObject lighteningPrefab;
	public float lighteningSize = 0.5f;
	public float charge = 1;
	public float chargeRate = 0.0006f;
	bool frozen = false;
	
	GameObject lightening1;
	GameObject lightening2;

	// Use this for initialization
	void Start () {
		lightening1 = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening1.transform.parent = transform;
		lightening1.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere1").position;
		lightening1.GetComponent<Lightening>().endPoint = transform.FindChild("Sphere1").position;
		
		lightening2 = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening2.transform.parent = transform;
		lightening2.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
		lightening2.GetComponent<Lightening>().endPoint = transform.FindChild("Sphere2").position;
	}
	
	public bool IsDepleated(){
		return (charge < 0);
	
	}
	
	
	// Update is called once per frame
	void FixedUpdate () {
		if (AVOWGraph.singleton.allComponents.Count > 0){
			lightening1.GetComponent<Lightening>().endPoint = AVOWGraph.singleton.allComponents[0].transform.FindChild ("ConnectionSphere1").position;
			lightening1.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lightening1.GetComponent<Lightening>().ConstructMesh();
			lightening2.GetComponent<Lightening>().endPoint = AVOWGraph.singleton.allComponents[0].transform.FindChild ("ConnectionSphere0").position;
			lightening2.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lightening2.GetComponent<Lightening>().ConstructMesh();
			if (!frozen){
				charge -= chargeRate * Time.fixedDeltaTime * AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth;
			}
		}		
		transform.FindChild("Charge").localScale = new Vector3(1, charge, 1);
	
	}
	
	public void ResetBattery(){
		frozen = false;
		charge = 1;
	}
	
	public void FreezeBattery(){
		frozen = true;
	
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
