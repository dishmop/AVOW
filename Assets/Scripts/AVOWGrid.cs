using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class AVOWGrid : MonoBehaviour {

	public static AVOWGrid singleton = null;

	public int gridWidth;
	public int gridHeight;
	public GameObject gridSquarePrefab;
	public GameObject[,] gridObjects = null;
	
	const int		kLoadSaveVersion = 1;		


	public 	void Save(BinaryWriter bw){
		bw.Write (kLoadSaveVersion);
		bw.Write (gridWidth);
		bw.Write (gridHeight);
	}
	
	public 	void Load(BinaryReader br){
		int oldGridWidth = gridWidth;
		int oldGridHeight = gridHeight;
		
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				gridWidth = br.ReadInt32();	
				gridHeight = br.ReadInt32();	
				break;
			}
		}
		// If things have chnaged since we last made the grid, remake it
		if (oldGridWidth != gridWidth || oldGridHeight != gridHeight){
			CreateGrid();
		}
	}
	

	
	GameObject ConstructBespokeGridObject(GridPoint point){
	
		GameObject newObj = Instantiate(
			gridSquarePrefab, 
			new Vector3(point.x , point.y , 0), 
			Quaternion.identity)
			as GameObject;
		newObj.transform.parent = transform;
		return newObj;
	}
	
	
	public bool IsPointInGrid(GridPoint point){
		return 	point.x >= 0 &&
				point.y >= 0 &&
				point.x < gridWidth &&
				point.y < gridHeight;		
	}

	public bool IsPointInGrid(int x, int y){
		return 	x >= 0 &&
				y >= 0 &&
				x < gridWidth &&
				y < gridHeight;		
	}			
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	
	
	void CreateGrid(){
		// if we already have a load of grig obejcts, destroy them all
		if (gridObjects != null){
			for (int x = 0; x < gridWidth; ++x){
				for (int y = 0; y < gridHeight; ++y){
					Destroy(gridObjects[x,y]);
				}
				
			}
			
		}
		
		gridObjects = new GameObject[gridWidth, gridHeight];
	
		for (int x = 0; x < gridWidth; ++x){
			for (int y = 0; y < gridHeight; ++y){
				gridObjects[x,y] = ConstructBespokeGridObject(new GridPoint(x,y));
			}
			
		}
		
	}
	

	// Use this for initialization
	void Start () {
		CreateGrid ();		
	
	}
	

}
