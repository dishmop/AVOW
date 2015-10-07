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
	
	GameObject lighteningUpper;	
	GameObject lighteningLower;	
	GameObject lighteningBigRod;	
	GameObject lighteningLittleRod;	
	
	
	// Use this for initialization
	public void Initialise () {
		lightening1 = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening1.transform.parent = transform;
		lightening1.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere1").position;
		lightening1.GetComponent<Lightening>().endPoint = transform.FindChild("Sphere1").position;
		
		lightening2 = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lightening2.transform.parent = transform;
		lightening2.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
		lightening2.GetComponent<Lightening>().endPoint = transform.FindChild("Sphere2").position;
		
		lighteningUpper = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lighteningUpper.transform.parent = transform;
		lighteningUpper.GetComponent<Lightening>().startPoint = transform.FindChild("CellPlace").position;
		lighteningUpper.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position;	
		
		lighteningLower = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lighteningLower.transform.parent = transform;
		lighteningLower.GetComponent<Lightening>().startPoint = transform.FindChild("CellPlace").position;
		lighteningLower.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position;
		
		lighteningBigRod = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lighteningBigRod.transform.parent = transform;
		lighteningBigRod.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
		lighteningBigRod.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position;	
		
		lighteningLittleRod = GameObject.Instantiate(lighteningPrefab) as GameObject;
		lighteningLittleRod.transform.parent = transform;
		lighteningLittleRod.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
		lighteningLittleRod.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position;						
		
		transform.FindChild("CellPlace").GetComponent<Renderer>().enabled = false;
	}
	
	public bool IsDepleated(){
		return (charge < 0);
	
	}
	
	
	// Update is called once per frame
	public void RenderUpdate () {
		if (AVOWGraph.singleton.allComponents.Count > 0){
			lightening1.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere1").position;
			lightening1.GetComponent<Lightening>().endPoint = AVOWGraph.singleton.allComponents[0].transform.FindChild ("ConnectionSphere1").position;
			lightening1.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lightening1.GetComponent<Lightening>().ConstructMesh();
			
			lightening2.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
			lightening2.GetComponent<Lightening>().endPoint = AVOWGraph.singleton.allComponents[0].transform.FindChild ("ConnectionSphere0").position;
			lightening2.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lightening2.GetComponent<Lightening>().ConstructMesh();
			
			lighteningUpper.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere1").position;
			lighteningUpper.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position +  0.5f * new Vector3(0, transform.FindChild("CellPlace").localScale.y, 0);
			lighteningUpper.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lighteningUpper.GetComponent<Lightening>().ConstructMesh();		
			
			lighteningLower.GetComponent<Lightening>().startPoint = transform.FindChild("Sphere2").position;
			lighteningLower.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position - 0.5f * new Vector3(0, transform.FindChild("CellPlace").localScale.y, 0);
			lighteningLower.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lighteningLower.GetComponent<Lightening>().ConstructMesh();		
			
			
			lighteningBigRod.GetComponent<Lightening>().startPoint =  transform.FindChild("CellPlace").position + 0.5f * new Vector3(-transform.FindChild("CellPlace").localScale.x, transform.FindChild("CellPlace").localScale.y, 0);
			lighteningBigRod.GetComponent<Lightening>().endPoint =  transform.FindChild("CellPlace").position + 0.5f * new Vector3(transform.FindChild("CellPlace").localScale.x, transform.FindChild("CellPlace").localScale.y, 0);
			lighteningBigRod.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lighteningBigRod.GetComponent<Lightening>().disableWobble = true;
			lighteningBigRod.GetComponent<Lightening>().numStages = 2;
			lighteningBigRod.GetComponent<Lightening>().ConstructMesh();		
						
			lighteningLittleRod.GetComponent<Lightening>().startPoint = transform.FindChild("CellPlace").position + 0.5f * new Vector3(-0.5f * transform.FindChild("CellPlace").localScale.x, -transform.FindChild("CellPlace").localScale.y, 0);
			lighteningLittleRod.GetComponent<Lightening>().endPoint = transform.FindChild("CellPlace").position + 0.5f * new Vector3(0.5f * transform.FindChild("CellPlace").localScale.x, -transform.FindChild("CellPlace").localScale.y, 0);
			lighteningLittleRod.GetComponent<Lightening>().size = AVOWGraph.singleton.allComponents[0].GetComponent<AVOWComponent>().hWidth * lighteningSize;
			lighteningLittleRod.GetComponent<Lightening>().ConstructMesh();													
			
			
			
			
			
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
