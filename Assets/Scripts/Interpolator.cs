using UnityEngine;
using System.Collections;


// Utility class for group all the info we need for each measurement
class IntTransform{
	public Vector3 		pos = Vector3.zero;
	public Quaternion 	rot = Quaternion.identity;
	public float 		time = 0;
}

// The actual script class
public class Interpolator : MonoBehaviour {

	// Not sure why we can only get this once and 
	Transform parentTransform;
	
	// How much to look ahead to make a virtual future state to head otwards
	float lookAheadTime;


	// These all get asigned in Start()
	IntTransform lastPhysicsTrans;
	IntTransform thisPhysicsTrans;
	IntTransform lastRenderTrans;
	
	void RegisterFixedTransform(Vector3 pos, Quaternion rot, float time){
		lastPhysicsTrans = thisPhysicsTrans;
		thisPhysicsTrans.pos = pos;
		thisPhysicsTrans.rot = rot;
		thisPhysicsTrans.time = time;
	}
		

	// Use this for initialization
	void Start () {
		lastPhysicsTrans = new IntTransform();
		thisPhysicsTrans = new IntTransform();
		lastRenderTrans = new IntTransform();
		
		lookAheadTime = Time.fixedDeltaTime;

		if (transform.parent == null){
			Debug.LogError("Interpolator does not have a parent");
		}
		parentTransform = transform.parent.transform;
		RegisterFixedTransform(transform.position, transform.rotation, Time.fixedTime - 2.0f*Time.fixedDeltaTime);
		RegisterFixedTransform(transform.position, transform.rotation, Time.fixedTime - Time.fixedDeltaTime);
		lastRenderTrans.pos = transform.position;
		lastRenderTrans.time = Time.fixedTime - Time.fixedDeltaTime;
		
		
	}

	IntTransform LerpDC(IntTransform from , IntTransform to, float thisTime){
		float t = (thisTime - from.time) / (to.time - from.time);
		IntTransform result = new IntTransform();
		result.time = thisTime;
		result.pos = Vector3.Lerp(from.pos, to.pos, t);
		result.rot = Quaternion.Slerp(from.rot, to.rot, t);
		return result;
	}
		// Update is called once per frame
	void Update () {
	
		// Make a position in the future
		float futureTime = thisPhysicsTrans.time + lookAheadTime;
		IntTransform futureTrans = LerpDC (lastPhysicsTrans, thisPhysicsTrans, futureTime);
		
		// Now go towards that from where we last rendered ourselves
		IntTransform nowTrans = LerpDC (lastRenderTrans, futureTrans, Time.time);
		transform.position = nowTrans.pos;
		transform.rotation = nowTrans.rot;
		
		// Register our new render posiiton
		lastRenderTrans = nowTrans;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		RegisterFixedTransform(parentTransform.position, parentTransform.rotation, Time.fixedTime); 
		
	}	
}
