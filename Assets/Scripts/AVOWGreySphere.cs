using UnityEngine;
using System.Collections;

public class AVOWGreySphere : MonoBehaviour {

	public Vector3 vel = Vector3.zero;
	public Vector3 accn = Vector3.zero;
	public Color beatColor;
	public GameObject otherSphere;
	public GameObject lightGO;
	public GameObject massObj;
	
	
	
	
	
	
	public bool enableTrigger = false;
	public bool heartRestartTrigger =false;
	
	
	
	
	
	SpringValue beatIntensity = new SpringValue(0, SpringValue.Mode.kAsymptotic, 10f);
	
	Color	baseRimColor;
	Color 	baseReflectColor;
	Color 	baselightColor;
	
	float baseIntensity = 0;
	bool movingTowards  = false;
	
	bool isPrimeBeater = false;
	
	float bpm = 45;
	float beatTime = 0;
	bool regularBeats = false;
	float maxBPM = 60;
	float maxmaxBPM = 80;
	bool reachedMax = false;
	
	
	
	
	enum State{
		kNormal,
		kExpectant,
		kStartCourtship,
		kHalt,
		kGoTowards,
		kBeat2,
		kHalt2,
		kRetreat,
		kRetreatWait,
		kPause,
		kRush,
		kDance0,
		kDance1,
		kFinish
	}
	
	State state = State.kNormal;
	
	float waitUntil = 0;
	
	

	// Use this for initialization
	void Start () {
		baseRimColor = renderer.material.GetColor("_RimColour");
		baseReflectColor = renderer.material.GetColor ("_ReflectColour");
	}
	
	
	public void  SetRegularBeats(){
		if (!regularBeats){
			regularBeats = true;
			beatTime = Time.fixedTime + 1.0f / (bpm / 60f);
		}
	}
	
	public void SetExpectant(GameObject sphere, GameObject lightPass){
		lightGO = lightPass;
		otherSphere = sphere;
		state = State.kExpectant;
		isPrimeBeater = true;
		
		baselightColor = lightGO.GetComponent<Light>().color;
	}
	
	
	public void StartDancing(GameObject mass){
		massObj = mass;
		state = State.kDance1;
	
	}
	
	// Update is called once per frame
	public void FixedUpdate () {
		transform.position += vel * Time.fixedDeltaTime + 0.5f * accn * Time.fixedDeltaTime * Time.fixedDeltaTime;
		vel += accn * Time.fixedDeltaTime;
		
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
				if (vel.magnitude < 0.1f)
					vel += hereToThere * 0.01f;
				float sizes = 0.5f * (otherSphere.transform.localScale.x + transform.localScale.x);
				if ((otherSphere.transform.position - transform.position).magnitude < sizes + 0.2f){
					BeatHeart ();
					baseIntensity = 0.2f;
					waitUntil = Time.fixedTime + 0.15f;
					state = State.kBeat2;
				}
				break;
			}
			case State.kBeat2:{
				vel *= 0.9f;	
				if (Time.fixedTime > waitUntil){
					vel = Vector3.zero;
				
					
					waitUntil = Time.fixedTime + 3;
					state = State.kHalt2;
				}
				break;
			}
			case State.kHalt2:{
				if (Time.fixedTime > waitUntil){
				
					state = State.kRetreat;
				}
			
				break;
			}
			case State.kRetreat:{
				Vector3 thereToHere = transform.position - otherSphere.transform.position;
				
				Vector3 offset = new Vector3(-thereToHere.y, thereToHere.x, thereToHere.z);
			                             
				accn = thereToHere * 1f+ offset * 1f + new Vector3(0.2f, 0.3f, 0);
				waitUntil = Time.fixedTime + 1;
				state = State.kRetreatWait;
				
				break;
			}
			case State.kRetreatWait:{
				if (Time.time > waitUntil){
					state = State.kDance0;
				}
				break;
			}
			case State.kDance0:{
				if (enableTrigger){
					AVOWTutorialManager.singleton.Trigger();
					enableTrigger = false;
				}
				break;
			}
			case State.kDance1:{
				// Setup params
				
				Vector3 thisPos = transform.position;
				thisPos.z = 0;
				Vector3 otherPos = otherSphere.transform.position;
				otherPos.z = 0;
				Vector3 massPos = massObj.transform.position;
				massPos.z = 0;
				
				Vector3 hereToThere = otherPos - thisPos;
				float flockDist = hereToThere.magnitude;
				hereToThere.Normalize();
				flockDist = flockDist - AVOWConfig.singleton.flockDesDistToOther;
				
				Vector3 hereToHome = massPos - thisPos;
			
				// Construct a force vector
				Vector3 flockDirComp = otherSphere.GetComponent<AVOWGreySphere>().vel;
				Vector3 flockDistComp = hereToThere * flockDist;
				Vector3 homingComp = hereToHome;
//				Vector3 drag = vel;
//				float velSze = drag.magnitude;
//				drag.Normalize();
//					
//				drag *= (desSpeed-velSze);

				// If the other sphere is in front of is, then try and catch  up
				float dotResult = Vector3.Dot (vel.normalized, hereToThere.normalized);
				float speedMod = AVOWConfig.singleton.flockSpeedMod * dotResult;
					
					
				// Create a spiral vector (which nudges them to always be rotating clockwise around the mass)
				Vector3 spiralPosVec = hereToHome;
				spiralPosVec.Normalize();
				Vector3 spiralVec = Vector3.Cross (spiralPosVec, new Vector3(0, 0, 1));
				
				accn = AVOWConfig.singleton.flockAlignCoef * flockDirComp + flockDistComp + AVOWConfig.singleton.flockHomeCoef * homingComp + AVOWConfig.singleton.flockSpiralCoef * spiralVec;
				vel.z = 0;
				float currentSpeed = vel.magnitude;
				float desSpeed = AVOWConfig.singleton.flockDesSpeed + speedMod;
				float useSpeed = Mathf.Lerp (currentSpeed, desSpeed, 0.1f);
				vel.Normalize();
				vel *= useSpeed;
				
				if (AVOWConfig.singleton.flockReset){
					AVOWConfig.singleton.flockReset = false;
					AVOWTutorialManager.singleton.state = AVOWTutorialManager.State.kDebugResetDance;
				}
				
				// We trigger when the two have been moving towards one another and how are moving away
				bool triggerBeat = false;
				
				if (!regularBeats){
					if (movingTowards){
						Vector3 fromhereToThere = otherSphere.transform.position - transform.position;
						if (Vector3.Dot(fromhereToThere, vel) < 0){
							triggerBeat = true;
							movingTowards = false;
						}
					
					}
					else{
						Vector3 fromhereToThere = otherSphere.transform.position - transform.position;
						if (Vector3.Dot(fromhereToThere, vel) > 0){
							movingTowards = true;
						}
					}
				}
				else{
					if (Time.fixedTime > beatTime){
						beatTime = Time.fixedTime + 1.0f / (bpm / 60f);
						triggerBeat = true;
						if (bpm < maxBPM || (bpm < maxmaxBPM && heartRestartTrigger)){
							bpm += 1f;
							Debug.Log ("BPM = " + bpm.ToString());
						}
						
					}
					
				}
				if (triggerBeat){
					if (!IsBeating() && !otherSphere.GetComponent<AVOWGreySphere>().IsBeating()){
						BeatHeart();
						otherSphere.GetComponent<AVOWGreySphere>().BeatHeart();
					}
				}
				
				if (bpm >= maxBPM && !reachedMax){
					reachedMax = true;
					AVOWTutorialManager.singleton.Trigger();
				}
		
				break;
			}
		}
	}
	
	bool IsBeating(){
		return GetComponent<AudioSource>().isPlaying;
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
		float val = 15 * total /data.Length; 
		if (val > 1) val = 1;
		beatIntensity.Set (val + baseIntensity);
		
	}
}
