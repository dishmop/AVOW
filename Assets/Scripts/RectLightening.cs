using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class RectLightening : MonoBehaviour {

	public Vector3		bottomLeft;
	public Vector3  	topRight;
	public float		size;
	
	

	
	const int		kLoadSaveVersion = 1;	

//	public float avFeedbackZ;

	int		 	numVerts;
	int			numTris;
	int			numTriIndicies;

	Vector3[]	vertices;
	Vector2[]	uvs;
	int[]		tris;
	
	Vector3 localX;
	Vector3 localY;
//	Vector3 localZ;
	
	
	
	
	// Use this for initialization
	void Start () {
	
		ConstructMesh();

	
	}
	
	void HandleOrientation(){
		//transform.rotation = Quaternion.identity;
//		transform.Rotate(endPoint - startPoint, 1f);
//		
//		Quaternion quat = Quaternion.LookRotation(endPoint - startPoint, Camera.main.transform.position - transform.position);
//		transform.rotation = new Quaternion(quat
		
	}
	
	public void Serialise(BinaryWriter bw){
	
		bw.Write (kLoadSaveVersion);
		Debug.Log ("Trying to serialise RectLightening");
	}
	
	
	public void Deserialise(BinaryReader br){
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				Debug.Log ("Trying to deserialise RectLightening");
				break;
			}
		}
	}
	
	
	public void ConstructMesh(){
		transform.position = Vector3.zero;
		HandleOrientation();
		ConstructArrays();
		FillVertices();
		FillTriangles();
		FillUVs();	
				
		Mesh mesh = GetComponent<MeshFilter>().mesh;
	
		mesh.triangles = null;
		mesh.vertices = null;
		mesh.uv = null;
		
		mesh.vertices =  vertices;
		mesh.uv = uvs;
		mesh.triangles = tris;
	}
	
	void UpdateMesh(){
		
		FillVertices();
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		
		mesh.vertices =  vertices;
	}

	
	void ConstructArrays(){
	
		// We add 4 verts (and 2 tris) at either end to roudn the off
		
		// limit size
		size = Mathf.Min (size, Mathf.Abs (topRight.x - bottomLeft.x));
		size = Mathf.Min (size, Mathf.Abs (topRight.y - bottomLeft.y));
		
		numVerts = 16;
		vertices = new Vector3[numVerts];
		uvs = new Vector2[numVerts];
		
		numTris = 16;
		numTriIndicies = numTris * 3;
		
		tris = new int[numTriIndicies];
	}
	
	void FillUVs(){
		uvs[0] = new Vector2(0.5f, 0.5f);
		uvs[1] = new Vector2(0.5f, 0.5f);
		uvs[2] = new Vector2(0.5f, 0.5f);
		uvs[3] = new Vector2(0.5f, 0.5f);
		
		// Outer square		
		uvs[4] = new Vector2(0.5f, 0.5f);
		uvs[5] = new Vector2(0.5f, 0.5f);
		uvs[6] = new Vector2(0.5f, 0.5f);
		uvs[7] = new Vector2(0.5f, 0.5f);
		uvs[8] = new Vector2(0.5f, 0.5f);
		uvs[9] = new Vector2(0.5f, 0.5f);
		uvs[10] = new Vector2(0.5f, 0.5f);
		uvs[11] = new Vector2(0.5f, 0.5f);
		uvs[12] = new Vector2(0.5f, 0.5f);
		uvs[13] = new Vector2(0.5f, 0.5f);
		uvs[14] = new Vector2(0.5f, 0.5f);
		uvs[15] = new Vector2(0.5f, 0.5f);
		
		// Bottom left square
		uvs[0] = new Vector2(0f, 0f);
		uvs[15] = new Vector2(1f, 0f);
		uvs[4] = new Vector2(1f, 1f);
		uvs[5] = new Vector2(1f, 0f);
		
		// top left square
		uvs[1] = new Vector2(0f, 0f);
		uvs[6] = new Vector2(1f, 0f);
		uvs[7] = new Vector2(1f, 0f);
		uvs[8] = new Vector2(1f, 1f);
		
		// top right square
		uvs[2] = new Vector2(0f, 0f);
		uvs[9] = new Vector2(1f, 0f);
		uvs[10] = new Vector2(1f, 0f);
		uvs[11] = new Vector2(1f, 1f);		
		
		uvs[3] = new Vector2(0f, 0f);
		uvs[12] = new Vector2(1f, 0f);
		uvs[13] = new Vector2(1f, 0f);
		uvs[14] = new Vector2(1f, 1f);		
		
	}
	
	// Start with the two triangles making the quad at the bottom left orner of the 3X3 grid
	void FillTriangles(){

		int triIndex = 0;

		// bottom left corner
		// Tri 1
		tris[triIndex++] = 0;
		tris[triIndex++] = 15;
		tris[triIndex++] = 4;
		
		// Tri 2
		tris[triIndex++] = 0;
		tris[triIndex++] = 4;
		tris[triIndex++] = 5;
		
		
		// left side
		// Tri 1
		tris[triIndex++] = 0;
		tris[triIndex++] = 5;
		tris[triIndex++] = 1;
		
		// Tri 2
		tris[triIndex++] = 1;
		tris[triIndex++] = 5;
		tris[triIndex++] = 6;
		
		// Top Left
		// Tri 1
		tris[triIndex++] = 1;
		tris[triIndex++] = 6;
		tris[triIndex++] = 7;
		
		// Tri 2
		tris[triIndex++] = 1;
		tris[triIndex++] = 7;
		tris[triIndex++] = 8;
		
		// Top side
		// Tri 1
		tris[triIndex++] = 1;
		tris[triIndex++] = 8;
		tris[triIndex++] = 9;
		
		// Tri 2
		tris[triIndex++] = 1;
		tris[triIndex++] = 9;
		tris[triIndex++] = 2;
		
		// Top right corner
		// Tri 1
		tris[triIndex++] = 2;
		tris[triIndex++] = 9;
		tris[triIndex++] = 10;
		
		// Tri 2
		tris[triIndex++] = 2;
		tris[triIndex++] = 10;
		tris[triIndex++] = 11;		
		
		// right side
		// Tri 1
		tris[triIndex++] = 2;
		tris[triIndex++] = 11;
		tris[triIndex++] = 3;
		
		// Tri 2
		tris[triIndex++] = 3;
		tris[triIndex++] = 11;
		tris[triIndex++] = 12;											
		
		// bottom right corner
		// Tri 1
		tris[triIndex++] = 3;
		tris[triIndex++] = 12;
		tris[triIndex++] = 13;
		
		// Tri 2
		tris[triIndex++] = 3;
		tris[triIndex++] = 13;
		tris[triIndex++] = 14;											
		
		// Bottom side
		// Tri 1
		tris[triIndex++] = 3;
		tris[triIndex++] = 14;
		tris[triIndex++] = 15;
		
		// Tri 2
		tris[triIndex++] = 3;
		tris[triIndex++] = 15;
		tris[triIndex++] = 0;											
	}
	
	
	// This is a 3X3 grid with the centre missing.
	// The inner square has four vertices starts at bottom left clockwise from 0..3
	// The outer square has 12 verticies starting at bottom left clockwise from 4..15
	// The corner squares have their tri diagonals pointing outwards from the centre
	void FillVertices(){
		Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y);
		Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y);
		
		// Inner square
		float halfSize = 0.5f * size;
		vertices[0] = bottomLeft + new Vector3(halfSize, halfSize, 0);
		vertices[1] = topLeft + new Vector3(halfSize, -halfSize, 0);
		vertices[2] = topRight + new Vector3(-halfSize, -halfSize, 0);
		vertices[3] = bottomRight + new Vector3(-halfSize, halfSize, 0);

		// Outer square		
		vertices[4] = bottomLeft + new Vector3(-halfSize, -halfSize, 0);
		vertices[5] = bottomLeft + new Vector3(-halfSize, halfSize, 0);
		vertices[6] = topLeft + new Vector3(-halfSize, -halfSize, 0);
		vertices[7] = topLeft + new Vector3(-halfSize, halfSize, 0);
		vertices[8] = topLeft + new Vector3(halfSize, halfSize, 0);
		vertices[9] = topRight + new Vector3(-halfSize, halfSize, 0);
		vertices[10] = topRight + new Vector3(halfSize, halfSize, 0);
		vertices[11] = topRight + new Vector3(halfSize, -halfSize, 0);
		vertices[12] = bottomRight + new Vector3(halfSize, halfSize, 0);
		vertices[13] = bottomRight + new Vector3(halfSize, -halfSize, 0);
		vertices[14] = bottomRight + new Vector3(-halfSize, -halfSize, 0);
		vertices[15] = bottomLeft + new Vector3(halfSize, -halfSize, 0);
		
	}
	
	void OnDisable(){
	}
	
	// Update is called once per frame
	void Update () {
		HandleOrientation();
		
//		UpdateMesh();
//		for (int i = 0; i < vertices.Length-1; ++i){
//			Debug.DrawLine(transform.position + vertices[i], transform.position + vertices[i+1], Color.green);
//		}
//
//		for (int i = 0; i < points.Length-1; ++i){
//			Debug.DrawLine(transform.position + points[i], transform.position + points[i+1], Color.red);
//		}
		

	// Rotate around the axies from start to end
		
	}
	

}
