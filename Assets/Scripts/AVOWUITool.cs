using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AVOWUITool{
	
	protected float hysteresisFactor = 0.5f;
	protected float maxLighteningDist = 0.2f;
	protected float 		uiZPos;
	protected GameObject insideCube;
	
	
	
	// Return tru if you don;t want this tool to be snatched away
	// becuase it is in the middle of doing something
	public virtual bool IsBeingUsed(){
		return false;
	}
	
	protected class OrderBlock{
		
		public float minOrder = 99;
		public float maxOrder = -99;
		public float minPos = 99;
		public float maxPos = -99;
		
		public void AddComponent(AVOWComponent component){
			minOrder = Mathf.Min(minOrder, component.hOrder);
			maxOrder = Mathf.Max(maxOrder, component.hOrder);
			minPos = Mathf.Min(minPos, component.h0);
			maxPos = Mathf.Max(maxPos, component.h0 + component.hWidth);
		}
	}
	
	public virtual void Start(){}
	public virtual void Update(){}
	public virtual void OnDestroy(){}
	
	
	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	protected float FindClosestComponent(Vector3 pos, GameObject nodeGO, GameObject currentSelection, float minDist, ref GameObject closestComponent, ref Vector3 closestPos){
		AVOWNode node = nodeGO.GetComponent<AVOWNode>();
		
		// Make a copy of the list of compoents
		List<GameObject> components = node.components.GetRange(0, node.components.Count);
		
		if (currentSelection != null && currentSelection.GetComponent<AVOWComponent>() != null){
			components.Add(currentSelection);
		}
		
		
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			
			if (!component.isInteractive) continue;
			
			float thisDist = 0;
			Vector3 thisPos = Vector3.zero;
			// Check which of the two connectors to use
			if (node == component.node0GO.GetComponent<AVOWNode>()){
				thisPos = component.GetConnectionPos0();
			}
			else if (node == component.node1GO.GetComponent<AVOWNode>()){
				thisPos = component.GetConnectionPos1();
			}
			// If it is neither - then check if etierh of the nodes are non-interactive, if so, then it is that one
			else if (!component.node0GO.GetComponent<AVOWNode>().isInteractive){
				thisPos = component.GetConnectionPos0();
			}
			else if (!component.node1GO.GetComponent<AVOWNode>().isInteractive){
				thisPos = component.GetConnectionPos1();
			}
			else{
				continue;
			}
			thisPos.z = uiZPos;
			thisDist = (thisPos - pos).magnitude;
			
			// If this is the current Component, reduce the distance (for the purposes of hyserisis)
			if (go == currentSelection){
				thisDist *= hysteresisFactor;
			}
			
			if (thisDist < minDist){
				minDist = thisDist;
				closestComponent = go;
				closestPos = thisPos;
				
			}
			
		}	
		return closestComponent ? minDist : maxLighteningDist;
	}
	
	
	
	// The current compoent is one that we should include in our search (even if it is not strictly connected to this node)
	protected float FindClosestComponentCentre(Vector3 pos, GameObject currentSelection, float minDist, ref GameObject closestComponent, ref Vector3 closestPos){
		
		List<GameObject> components = AVOWGraph.singleton.allComponents;
		
		foreach (GameObject go in components){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			if (component.type == AVOWComponent.Type.kVoltageSource) continue;
			if (!component.isInteractive) continue;
			
			float thisDist = 0;
			Vector3 thisPos = 0.5f * (component.GetConnectionPos0() + component.GetConnectionPos1());
			
			
			thisPos.z = uiZPos;
			thisDist = (thisPos - pos).magnitude;
			
			// If this is the current Component, reduce the distance (for the purposes of hyserisis)
			if (go == currentSelection){
				thisDist *= hysteresisFactor;
			}
			
			if (thisDist < minDist){
				minDist = thisDist;
				closestComponent = go;
				closestPos = thisPos;
				
			}
			
		}	
		closestPos.z = uiZPos;
		return closestComponent ? minDist : maxLighteningDist;
	}	
	
	protected void FindClosestPointOnNode(Vector3 pos, GameObject nodeGO, ref Vector3 closestPos){
		
		AVOWNode node = nodeGO.GetComponent<AVOWNode>();
		
		// If inside the h-range of the node
		if (pos.x > node.h0 && pos.x < node.h0 + node.hWidth){
			closestPos = new Vector3(pos.x, node.voltage, uiZPos);
			
		}
		else{
			if (pos.x <= node.h0){
				closestPos = new Vector3(node.h0, node.voltage, uiZPos);
			}
			else{
				closestPos = new Vector3(node.h0 + node.hWidth, node.voltage, uiZPos);
			}
		}
		closestPos.z = uiZPos;
		
	}
	
	protected float FindClosestNode(Vector3 pos, GameObject ignoreNode, float minDist, GameObject currentSelection, ref GameObject closestNode, ref Vector3 closestPos){
		
		foreach (GameObject go in AVOWGraph.singleton.allNodes){
			
			if (go == ignoreNode) continue;
			
			AVOWNode node = go.GetComponent<AVOWNode>();
			
			if (!node.isInteractive) continue;
			
			// If inside the h-range of the node
			float thisDist = -1;
			Vector3 thisPos = Vector3.zero;
			if (pos.x > node.h0 && pos.x < node.h0 + node.hWidth){
				thisDist = Mathf.Abs(pos.y - node.voltage);
				thisPos = new Vector3(pos.x, node.voltage, uiZPos);
				
			}
			else{
				if (pos.x <= node.h0){
					thisPos = new Vector3(node.h0, node.voltage, uiZPos);
				}
				else{
					thisPos = new Vector3(node.h0 + node.hWidth, node.voltage, uiZPos);
				}
				thisDist = (pos - thisPos).magnitude;
			}
			if (go == currentSelection){
				thisDist *= hysteresisFactor;
			}
			if (thisDist < minDist){
				minDist = thisDist;
				closestNode = go;
				closestPos = thisPos;
			}
		}	
		closestPos.z = uiZPos;
		
		return closestNode ? minDist : maxLighteningDist;
	}
	
	protected virtual GameObject InstantiateCursorCube(){
		return null;
	}
	
	
	protected void ActiveCubeAtCursor(GameObject cursor){
		// Create a new cube which will travel to the gap
		insideCube = InstantiateCursorCube();
		insideCube.transform.position = cursor.transform.position;
		insideCube.transform.rotation = cursor.transform.rotation;
		
		Material[] metalMaterials = new Material[1];
		metalMaterials[0] = cursor.renderer.materials[0];
		insideCube.renderer.materials = metalMaterials;
	}
	
	
	protected void ActiveCubeAtComponent(GameObject component){
		Transform resistanceTransform = component.transform.FindChild("Resistance");
		
		// Create a new cube which will travel to the cursor
		insideCube = InstantiateCursorCube();
		Vector3 targetScale = new Vector3(resistanceTransform.localScale.x, resistanceTransform.localScale.x, resistanceTransform.localScale.x);
		
		insideCube.transform.localScale = targetScale;
		insideCube.transform.position = resistanceTransform.position +  + 0.5f * targetScale;
		insideCube.transform.rotation = resistanceTransform.rotation;
		
		Material[] metalMaterials = new Material[1];
		metalMaterials[0] = insideCube.renderer.materials[0];
		insideCube.renderer.materials = metalMaterials;
		
		component.GetComponent<AVOWComponent>().isInteractive = false;
	}
	
	protected void RemoveMetal(GameObject cursor){
		Material[] highlightMaterials = new Material[1];
		highlightMaterials[0] = cursor.renderer.materials[1];
		cursor.renderer.materials = highlightMaterials;
		
	}
	
	// return dist remaining position of cube
	protected float LerpToComponent(AVOWComponent component, float lerpSpeed){
		Transform resistanceTransform = component.gameObject.transform.FindChild("Resistance");
		
		if (insideCube == null) return 0;
		
		//Orentation
		Quaternion targetOrient = resistanceTransform.rotation;
		Quaternion currentOrient = insideCube.transform.rotation;
		currentOrient = Quaternion.Slerp(currentOrient, targetOrient, lerpSpeed);
		insideCube.transform.rotation = currentOrient;
		
		// Scale
		Vector3 targetScale = new Vector3(resistanceTransform.localScale.x, resistanceTransform.localScale.x, resistanceTransform.localScale.x);
		Vector3 currentScale = insideCube.transform.localScale;
		currentScale = Vector3.Lerp(currentScale, targetScale, lerpSpeed);
		insideCube.transform.localScale = new Vector3(currentScale.x, currentScale.x, currentScale.x);
		
		// Position
		Vector3 targetPos = resistanceTransform.position + 0.5f * targetScale;
		Vector3 currentPos = insideCube.transform.position;
		currentPos = Vector3.Lerp(currentPos, targetPos, lerpSpeed);
		insideCube.transform.position = currentPos;
		
		return (currentPos - targetPos).magnitude;
	}
	
	// return dist remaining position of cube
	protected float LerpToCursor(GameObject cursor, float lerpSpeed){
		
		Transform cursorTransform = cursor.transform;
		
		// Hmm seem to get this error sometimes - try and catch it
		if (insideCube == null){
			Debug.LogError ("Trying to lerp to null inside cube");
			return 0;
		}
		
		//Orentation
		Quaternion targetOrient = cursorTransform.rotation;
		Quaternion currentOrient = insideCube.transform.rotation;
		currentOrient = Quaternion.Slerp(currentOrient, targetOrient, lerpSpeed);
		insideCube.transform.rotation = currentOrient;
		
		// Scale
		Vector3 targetScale = cursorTransform.localScale;
		Vector3 currentScale = insideCube.transform.localScale;
		currentScale = Vector3.Lerp(currentScale, targetScale, lerpSpeed);
		insideCube.transform.localScale = new Vector3(currentScale.x, currentScale.x, currentScale.x);
		
		// Position
		Vector3 targetPos = cursorTransform.position;
		Vector3 currentPos = insideCube.transform.position;
		currentPos = Vector3.Lerp(currentPos, targetPos, lerpSpeed);
		insideCube.transform.position = currentPos;	
		
		return (currentPos - targetPos).magnitude;
	}
	
	
	protected GameObject RejoinToCursor(GameObject cursor){
		GameObject newCursorCube = InstantiateCursorCube();
		newCursorCube.transform.position = cursor.transform.position;
		newCursorCube.transform.rotation = cursor.transform.rotation;
		newCursorCube.transform.localScale = cursor.transform.localScale;
		
		GameObject.Destroy(insideCube);
		insideCube = null;
		
		
		
		GameObject.Destroy (cursor);
		return newCursorCube;
	}
	
	
	
	
	
}

