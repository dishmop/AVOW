using UnityEngine;
using System.Collections;

public class AVOWGreySphere : MonoBehaviour {

	public Vector3 vel;
	public Color beatColor;
	public GameObject otherSphere;
	public GameObject lightGO;
	
	
	SpringValue beatIntensity = new SpringValue(0, SpringValue.Mode.kAsymptotic, 10f);
	
	Color	baseRimColor;
	Color 	baseReflectColor;
	Color 	baselightColor;
	
	
	
	enum State{
		kNormal,
		kExpectant,
		kStartCourtship,
		kHalt,
		kGoTowards,
		kBeat2,
		kHalt2,
		kRetreat,
		kPause,
		kRush,
		kDance,
		kFinish
	}
	
	State state = State.kNormal;
	
	float waitUntil = 0;
	
	

	// Use this for initialization
	void Start () {
		baseRimColor = renderer.material.GetColor("_RimColour");
		baseReflectColor = renderer.material.GetColor ("_ReflectColour");
	}
	
	public void SetExpectant(GameObject sphere, GameObject lightPass){
		lightGO = lightPass;
		otherSphere = sphere;
		state = State.kExpectant;
		
		baselightColor = lightGO.GetComponent<Light>().color;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position += vel * Time.fixedDeltaTime;
		
		switch (state){
			case State.kExpectant:{
				// We trigger when the two are moving away from each other
				Vector3 fromhereToThere = otherSphere.transform.position - transform.position;
				float dotResult = Vector3.Dot(fromhereToThere, vel);
				if (dotResult < 0){
					BeatHeart();
					otherSphere.GetComponent<AVOWGreySphere>().BeatHeart();
					state = State.kNormal;
				}
				break;
			}
			case State.kStartCourtship:{
				state = State.kHalt;
				if (otherSphere	!= null){
					otherSphere.GetComponent<AVOWGreySphere>().otherSphere = gameObject;
				}
				break;
			}
			case State.kHalt:{
				vel = vel * 0.98f;
				if (Mathf.Abs(vel.x)< 0.001f){
					state = State.kGoTowards;
					vel = Vector3.zero;
				}
				break;
			}
			case State.kGoTowards:{
				Vector3 hereToThere = otherSphere.transform.position - transform.position;
				vel += hereToThere * 0.001f;
				float sizes = 0.5f * (otherSphere.transform.localScale.x + transform.localScale.x);
				if ((otherSphere.transform.position - transform.position).magnitude < sizes + 0.2f){
					BeatHeart ();
					waitUntil = Time.fixedTime + 0.25f;
				state = State.kBeat2;
				}
				break;
			}
			case State.kBeat2:{
				if (Time.fixedTime > waitUntil){
					vel = Vector3.zero;
				
					state = State.kHalt2;
				}
				break;
			}
			case State.kHalt2:{
				vel = Vector3.zero;
				
				break;
			}
		}
	}
	
	void Update(){
		
		Color rimColor = Color.Lerp(baseRimColor, beatColor, beatIntensity.GetValue());
		renderer.material.SetColor("_RimColour", rimColor);
		
		Color reflectColor = Color.Lerp(baseReflectColor, beatColor, beatIntensity.GetValue());
		renderer.material.SetColor("_ReflectColour", reflectColor);
		beatIntensity.Update();
		
		if (lightGO != null){
		
			Color lightColor = Color.Lerp(baselightColor, beatColor, beatIntensity.GetValue());
			lightGO.GetComponent<Light>().color = lightColor;
			lightGO.transform.position = 0.5f * (transform.position + otherSphere.transform.position);
			lightGO.GetComponent<Light>().intensity = beatIntensity.GetValue();
		}
	
	}
	
	
	public void BeatHeart(){
		GetComponent<AudioSource>().Play ();
		//renderer.material.SetColor("_RimColour", Color.green);
	}
	
	public void StartCourtship(){
			state = State.kStartCourtship;
	}
	
	void OnAudioFilterRead(float[] data, int channels){

		float total = 0;
		for (int i = 0; i < data.Length; ++i){
			total += Mathf.Abs (data[i]);
		}
		float val = 5 * total /data.Length; 
		if (val > 1) val = 1;
		beatIntensity.Set (val);
		
	}
}
