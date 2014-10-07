﻿using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {


	public float 	zoomSpeed;
	Vector3			prevMousePos = new Vector3();
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		Camera camera = gameObject.GetComponent<Camera>();
		float wheelVal = Input.GetAxis("Mouse ScrollWheel");
		
		// We need to offset the camera so that the mouse pointer 
		// remains in the same position - in which case, get its current position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = transform.position.z - Camera.main.transform.position.z;
		
		
		if (wheelVal != 0){
			mousePos.z = transform.position.z - Camera.main.transform.position.z;
			Vector3 mouseOldWorldPos = camera.ScreenToWorldPoint( mousePos);

			camera.orthographicSize *= Mathf.Pow (zoomSpeed, -wheelVal);
		
			Vector3 mouseNewWorldPos = camera.ScreenToWorldPoint( mousePos);
			
			// Offset the camera so that the mouse is pointing at the same world position
			Vector3 camPos = gameObject.transform.position;
			camPos += (mouseOldWorldPos - mouseNewWorldPos);
			gameObject.transform.position = camPos;
		}
		
		if (Input.GetMouseButton(1)){
			Vector3 mouseOldWorldPos = camera.ScreenToWorldPoint( prevMousePos);
			Vector3 mouseNewWorldPos = camera.ScreenToWorldPoint( mousePos);
			
			// Offset the camera so that the mouse is pointing at the same world position
			Vector3 camPos = gameObject.transform.position;
			camPos += (mouseOldWorldPos - mouseNewWorldPos);
			gameObject.transform.position = camPos;
			
		
		}
		
		prevMousePos = mousePos;
		
	}
}
