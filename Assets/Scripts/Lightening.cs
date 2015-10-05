using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Lightening : MonoBehaviour {

	public Vector3		startPoint;
	public Vector3  	endPoint;
	public Vector3      endSD;
	public Vector3      startSD;
	public Vector3		midSD;
	public float		size;
	public int			numStages;
	public float		probOfChange;
	public bool 		disableWobble;
	
	
	const int		kLoadSaveVersion = 1;	

//	public float avFeedbackZ;

	int			numPoints;
	int		 	numVerts;
	int			numTris;
	int			numTriIndicies;

	Vector3[]  	basePoints;
	Vector3[]  	points;
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
		bw.Write (gameObject.activeSelf);
		bw.Write (startPoint);
		bw.Write (endPoint);
		bw.Write (endSD);
		bw.Write (startSD);
		bw.Write (midSD);
		bw.Write (size);
		bw.Write (numStages);
		bw.Write (probOfChange);	
	}
	
	public void Deserialise(BinaryReader br){
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				bool isActive = br.ReadBoolean();
				startPoint =  br.ReadVector3();
				endPoint = br.ReadVector3();
				endSD = br.ReadVector3();
				startSD =  br.ReadVector3();
				midSD = br.ReadVector3();
				size =  br.ReadSingle();
				numStages = br.ReadInt32();
				probOfChange = br.ReadSingle();
				ConstructMesh();
				gameObject.SetActive(isActive);
				break;
			}
		}
	}
	
	
	public void ConstructMesh(){
		transform.position = startPoint;
		HandleOrientation();
		ConstructArrays();
		FillBasePoints();
		FillPoints(false);
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
		
		FillBasePoints();
		FillPoints(true);
		FillVertices();
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		
		mesh.vertices =  vertices;
	}
	
	void FillBasePoints(){
		Vector3 hereToCam = Camera.main.transform.position - transform.position;
		
		// If we are looking at it head on, make it disspear
//		float dotRes = 1 - Mathf.Abs (Vector3.Dot (hereToCam.normalized, (endPoint - startPoint).normalized));
//		renderer.material.SetFloat("_Alpha", dotRes);
//		
		
	
		localY = (endPoint - startPoint).normalized;
		localX = Vector3.Cross(localY, hereToCam).normalized;
//		localZ = Vector3.Cross (localY, localX);
		
		// Create the points
		for (int i = 0; i < numPoints; ++i){
			basePoints[i] =  i * (endPoint - startPoint) / numStages;
		}
	}
	
	void FillPoints(bool useRandom){
//		int i = 0;
//		points[i++] = new Vector3 (0.5f, 0f, 0f);	
//		points[i++] = new Vector3 (0.4862927f, 0.3157371f, 0f);
//		points[i++] = new Vector3 (0.5680748f, 0.6618122f, 0f);
//		points[i++] = new Vector3 (0.5f, 1f, 0f);
	
		// Set up defaults
		
		float sdScalar = 0.02f * (startPoint - endPoint).magnitude;
		midSD = new Vector3(sdScalar, sdScalar, sdScalar);
		for (int i = 0; i < numPoints; ++i){
			if (!useRandom || Random.Range (0f, 1f) < probOfChange){
//				float distStart = (startPoint - basePoints[i]).magnitude;
//				float distEnd = (endPoint - basePoints[i]).magnitude;
				Vector3 sd = CalcSD((float)i * 1f/(numPoints-1));
				points[i] = basePoints[i] + GetNormalSample(0, sd.x) * localX;
			}
		}
	}
	
	// Pass in how far along the line we are (0 for start, 1 for end)
	Vector3 CalcSD(float dist){
		if (disableWobble) return Vector3.zero;
		
		if (dist < 0.5f){
			return Vector3.Lerp (startSD, midSD, dist * 2f);
		}
		else{
			return Vector3.Lerp (midSD, endSD, (dist-0.5f) * 2f);
		}
	}
	
	void ConstructArrays(){
	
		// We add 4 verts (and 2 tris) at either end to roudn the off
	
		numPoints = numStages + 1;
		points = new Vector3[numPoints];
		basePoints = new Vector3[numPoints];
		
		numVerts = (numStages + 5) * 2;
		vertices = new Vector3[numVerts];
		uvs = new Vector2[numVerts];
		
		numTris = (numStages + 4) * 2;
		numTriIndicies = numTris * 3;
		
		tris = new int[numTriIndicies];
	}
	
	void FillUVs(){

		
		// At each vertex, we alternate the u from 0 to 1
		// every second vertex we alternate v from 0 to 1
		uvs[0] = new Vector2(0, 1f);
		uvs[1] = new Vector2(1, 1f);
		uvs[2] = new Vector2(0, 0.5f);
		uvs[3] = new Vector2(1, 0.5f);
		for (int i = 4; i < numVerts-4; ++i){
			float u = i % 2;
			float v = 0.5f * ((i/2) % 2);
			uvs[i] = new Vector2(u, v);
		}
		uvs[numVerts-4] = new Vector2(0, 0.5f);
		uvs[numVerts-3] = new Vector2(1, 0.5f);
		uvs[numVerts-2] = new Vector2(0, 1f);
		uvs[numVerts-1] = new Vector2(1, 1f);		
	}
	
	void FillTriangles(){

		int triIndex = 0;

		// Tri 1
		tris[triIndex++] = 0;
		tris[triIndex++] = 1;
		tris[triIndex++] = 3;
		
		// Tri 2
		tris[triIndex++] = 0;
		tris[triIndex++] = 3;
		tris[triIndex++] = 2;
		
				
		for (int i = 0; i < numStages; ++i){
			int firstVertIndex = 4 + i * 2;
			// Tri 1
			tris[triIndex++] = firstVertIndex;
			tris[triIndex++] = firstVertIndex + 1;
			tris[triIndex++] = firstVertIndex + 3;
			
			// Tri 2
			tris[triIndex++] = firstVertIndex;
			tris[triIndex++] = firstVertIndex + 3;
			tris[triIndex++] = firstVertIndex + 2;
		}
		int vertIndex = 4 + (numStages+1) * 2;
		
		// Tri 1
		tris[triIndex++] = vertIndex;
		tris[triIndex++] = vertIndex + 1;
		tris[triIndex++] = vertIndex + 3;
		
		// Tri 2
		tris[triIndex++] = vertIndex;
		tris[triIndex++] = vertIndex + 3;	
		tris[triIndex++] = vertIndex + 2;
		
	
	}
	
	void FillVertices(){
	
		// Test
//		Vector3 p0 = new Vector3(3, 1, 0);
//		Vector3 r0 = new Vector3(0, 2, 0);
//		Vector3 p1 = new Vector3(1, 2, 0);
//		Vector3 r1 = new Vector3(1.5f, 0,0);
//		Vector3 inters = FindIntersetion(p0, r0, p1, r1);
		
//		Vector3 p0 = points[0];
//		Vector3 r0 = points[1] - points[0];
//		r0.Normalize();
//		Vector3 halfWidth0 = new Vector3(size * 0.5f * r0.y, -size * 0.5f * r0.x, 0);
//		Vector3 p1 = points[1];
//		Vector3 r1 = points[2] - points[1];
//		r1.Normalize();
//		Vector3 halfWidth1 = new Vector3(size * 0.5f * r1.y, -size * 0.5f * r1.x, 0);
//		p0 += halfWidth0;
//		p1 += halfWidth1;
//		Vector3 inters = FindIntersetion(p0, r0, p1, r1);
		
		// First and last vertices are just perpendicular to vector
		// Calced by taking pFirst to pLast and cross producting with positive z
		// Then first vertex is this
		
		Vector3 prevLength = points[1] - points[0];
		prevLength.Normalize();
		Vector3 prevHalfWidth = size * 0.5f * localX;
		
		// Do the round at the end
		
		vertices[0] = points[0] - prevLength * size * 0.5f - prevHalfWidth;
		vertices[1] = points[0] - prevLength * size * 0.5f + prevHalfWidth;
		vertices[2] = points[0] - prevHalfWidth;
		vertices[3] = points[0] + prevHalfWidth;
		
		// Now start the shaft
		vertices[4] = points[0] - prevHalfWidth;
		vertices[5] = points[0] + prevHalfWidth;
		
		// Start at 1 because we've already done the first pair
		Vector3 nextLength = new Vector3(0, 0, 0);
		Vector3 nextHalfWidth = new Vector3(0, 0, 0);
		for (int i = 3; i < numStages + 2; ++i){
			nextLength = points[i+1-2] - points[i-2];
			nextLength.Normalize();
			nextHalfWidth = size * 0.5f * localX;
			
			// Calc vertex indices we are going to write into
			int vi0 = i * 2;
			int vi1 = vi0 + 1;
			
			// Work out positions of vertices if there were no other stasges to consider
			vertices[vi0] = points[i-2] - nextHalfWidth;
			vertices[vi1] = points[i-2] + nextHalfWidth;
			
			
			// If the two "length" vectors are nearly parallel, then just leave them as they are
			// otherwise, adjust them to be at the intersectoin of the two stages
			if (!MathUtils.FP.Feq (Mathf.Abs(Vector3.Dot(prevLength,nextLength)), 1, 0.01f)){
				// Create four line euqations (p = v + lamba * r). two for the sides of the quad for the previous stage and two for the sides
				// of the quad for the next stage
				
				// Previous stage
				Vector3 prevV0 = vertices[vi0-2];
				Vector3 prevR0 = prevLength;
				Vector3 prevV1 = vertices[vi1-2];
				Vector3 prevR1 = prevLength;
				
				// Next stage
				Vector3 nextV0 = vertices[vi0];
				Vector3 nextR0 = nextLength;
				Vector3 nextV1 = vertices[vi1];
				Vector3 nextR1 = nextLength;	
				
				// Find intersection of the two pairs of lines
				vertices[vi0] = FindIntersetion(prevV0, prevR0, nextV0, nextR0);
				vertices[vi1] = FindIntersetion(prevV1, prevR1, nextV1, nextR1);


				// Test the cente lione
				Vector2 prevP2 = points[i-1-2];
				Vector2 prevR2 = points[i-2] - points[i-1-2];
				prevR2.Normalize();
				Vector2 halfWidthPrev2 =  new Vector3(size * 0.5f * prevR2.y, -size * 0.5f * prevR2.x);
				Vector2 nextP2 = points[i-2];
				Vector2 nextR2 = points[i + 1-2] - points[i-2];
				nextR2.Normalize();
				Vector2 halfWidthNext2 =  new Vector3(size * 0.5f * nextR2.y, -size * 0.5f * nextR2.x);
				
				prevP2 += halfWidthPrev2;
				nextP2 += halfWidthNext2;

				
			}

			prevLength = nextLength;			
		}
		
		// Do the last two points
		vertices[numVerts-6] = points[numStages] - nextHalfWidth;
		vertices[numVerts-5] = points[numStages] + nextHalfWidth;
		
		// Do the round at the end
		prevLength = points[numStages] - points[numStages-1];
		prevLength.Normalize();
		
		vertices[numVerts-4] = points[numStages] - nextHalfWidth;
		vertices[numVerts-3] = points[numStages] + nextHalfWidth;
		vertices[numVerts-2] = points[numStages] + size * 0.5f * prevLength - nextHalfWidth;
		vertices[numVerts-1] = points[numStages] + size * 0.5f * prevLength + nextHalfWidth;
		
		

		
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
	
	Vector3 FindIntersetionOld(Vector3 p0, Vector3 r0, Vector3 p1, Vector3 r1){
		float numerator = (p1.x-p0.x)*r0.y - (p1.y-p0.y)*r0.x;
		float denominator = r1.y*r0.x-r1.x*r0.y;
		if (MathUtils.FP.Feq(denominator, 0)){
//			Debug.Log ("Divide by zero when intersecting lines");
			return p0;
		}
		float lambda1 = numerator / denominator;
		return p1 + lambda1 * r1;
	}
	
	Vector3 FindIntersetion(Vector3 p, Vector3 u, Vector3 q, Vector3 v){
		Vector3 w = p - q;

		float a = Vector3.Dot (u, u);
		float b = Vector3.Dot (u, v);
		float c = Vector3.Dot (v, v);
		float d = Vector3.Dot (u, w);
		float e = Vector3.Dot (v, w);
		
		float s = (b * e - c * d) / (a * c - b * b);
		float t = (a * e - b * d) / (a * c - b * b);
		
		Vector3 ip = p + s * u;
		Vector3 iq = q + t * v;
		return 0.5f * (ip + iq);
		

	}
	

	
	float GetNormalSample(float mean, float sd){
		float u1 = Random.Range(0f, 1f);
		float u2 = Random.Range(0f, 1f);

		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
		return mean + sd * randStdNormal; 
	}
}
