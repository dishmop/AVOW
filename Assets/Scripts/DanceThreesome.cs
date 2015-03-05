using UnityEngine;
using System.Collections;

public class DanceThreesome : MonoBehaviour {

	public bool isActive = false;
	
	GameObject threesomeObj1;
	GameObject threesomeObj2;
	GameObject threesomeHome;
	
	float drag = 0;
	
	Vector3 vel = Vector3.zero;
	Vector3 accn = Vector3.zero;
	

	// Use this for initialization
	void Start () {
	
	}
	
	public void SetDrag(float drag){
		this.drag = drag; 
	}
	
	public void SetVelocity(Vector3 newVel){
		vel = Vector3.Lerp (newVel, vel, drag);
	}
	
	public void ForceVelocity(Vector3 newVel){
		vel = newVel;
	}
	public Vector3 GetVelocity(){
		return vel;
	}
	
	public void Start(GameObject obj1, GameObject obj2, GameObject objHome){
		threesomeObj1 = obj1;
		threesomeObj2 = obj2;
		threesomeHome = objHome;
		isActive =true;
		
		// Set up some initial accn to get things moving
		//accn = Vector3(0, 10f, 0);
		
	}
	
	
	// Update is called once per frame
	void Update () {
		if (!isActive) return;
		
		transform.position += vel * Time.fixedDeltaTime;
		
		
		Vector3 thisPos = transform.position;
		Vector3 otherPos1 = threesomeObj1.transform.position;
		Vector3 otherPos2 = threesomeObj2.transform.position;
		Vector3 avPos = 0.5f * ( otherPos1 + otherPos2);
		Vector3 homePos = threesomeHome.transform.position;
		
		Vector3 hereToOther1 = otherPos1 - thisPos;
		Vector3 hereToOther2 = otherPos2 - thisPos;
		Vector3 hereToAv = avPos - thisPos;
		Vector3 hereToHome = homePos - thisPos;
		
		float distToOther1 = hereToOther1.magnitude;
		hereToOther1.Normalize();
		float modDistToOther1 = distToOther1 - AVOWConfig.singleton.flockDesDistToOther;
		
		float distToOther2 = hereToOther2.magnitude;
		hereToOther2.Normalize();
		float modDistToOther2 = distToOther2 - AVOWConfig.singleton.flockDesDistToOther;
		
		// Construct a force vector
		Vector3 flockDirComp1 = threesomeObj1.GetComponent<DanceThreesome>().vel;
		Vector3 flockDirComp2 = threesomeObj2.GetComponent<DanceThreesome>().vel;
		
		Vector3 flockDistComp1 = hereToOther1 * modDistToOther1;
		Vector3 flockDistComp2 = hereToOther2 * modDistToOther2;
		
		Vector3 homingComp = hereToHome;
		
		// If the other sphere is in front of is, then try and catch  up
		float dotResult1 = Vector3.Dot (vel.normalized, hereToOther1);
		float dotResult2 = Vector3.Dot (vel.normalized, hereToOther2);
		
		float speedMod = AVOWConfig.singleton.flockSpeedMod * (dotResult1 + dotResult2);
		
			
		Vector3 thisVel = GetVelocity();
			
		accn = AVOWConfig.singleton.flockAlignCoef * (flockDirComp1 + flockDirComp2) + flockDistComp1 + flockDistComp2 + AVOWConfig.singleton.flockHomeCoef * homingComp;
		thisVel += accn * Time.fixedDeltaTime;
		float currentSpeed = thisVel.magnitude;
		float desSpeed = AVOWConfig.singleton.flockDesSpeed + speedMod;
		float useSpeed = Mathf.Lerp (currentSpeed, desSpeed, 0.1f);
		thisVel.Normalize();
		thisVel *= useSpeed;
		
		SetVelocity(thisVel);
		
		
	
	}
}
