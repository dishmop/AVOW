using UnityEngine;
using System.Collections;

public class Lightening : MonoBehaviour {

	public Vector3		startPoint;
	public Vector3  	endPoint;
	public Vector3      endSD;
	public Vector3      startSD;
	public Vector3		midSD;
	public float		size;
	public int			numStages;
	public float		probOfChange;

	int			numPoints;
	int		 	numVerts;
	int			numTris;
	int			numTriIndicies;

	Vector3[]  	basePoints;
	Vector3[]  	points;
	Vector3[]	vertices;
	Vector2[]	uvs;
	int[]		tris;
	
	
	
	// Use this for initialization
	void Start () {
	
		ConstructMesh();
	
	}
	
	public void ConstructMesh(){
	
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
		// Create the points
		for (int i = 0; i < numPoints; ++i){
			basePoints[i] = startPoint + i * (endPoint - startPoint) / numStages;
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
		midSD = new Vector3(sdScalar, sdScalar, 0);
		for (int i = 0; i < numPoints; ++i){
			if (!useRandom || Random.Range (0f, 1f) < probOfChange){
				float distStart = (startPoint - basePoints[i]).magnitude;
				float distEnd = (endPoint - basePoints[i]).magnitude;
				Vector3 sd = CalcSD((float)i * 1f/(numPoints-1));
				points[i] = new Vector3 (GetNormalSample(basePoints[i].x, sd.x), GetNormalSample(basePoints[i].y, sd.y), 0);
			}
		}
	}
	
	// Pass in how far along the line we are (0 for start, 1 for end)
	Vector3 CalcSD(float dist){
		if (dist < 0.5f){
			return Vector3.Lerp (startSD, midSD, dist * 2f);
		}
		else{
			return Vector3.Lerp (midSD, endSD, (dist-0.5f) * 2f);
		}
	}
	
	void ConstructArrays(){
	
		numPoints = numStages + 1;
		points = new Vector3[numPoints];
		basePoints = new Vector3[numPoints];
		
		numVerts = (numStages + 1) * 2;
		vertices = new Vector3[numVerts];
		uvs = new Vector2[numVerts];
		
		numTris = numStages * 2;
		numTriIndicies = numTris * 3;
		
		tris = new int[numTriIndicies];
	}
	
	void FillUVs(){

		
		// At each vertex, we alternate the u from 0 to 1
		// every second vertex we alternate v from 0 to 1
		for (int i = 0; i < numVerts; ++i){
			float u = i % 2;
			float v = (i/2) % 2;
			uvs[i] = new Vector2(u, v);
		}
	}
	
	void FillTriangles(){

		
		int triIndex = 0;
		for (int i = 0; i < numStages; ++i){
			int firstVertIndex = i * 2;
			// Tri 1
			tris[triIndex++] = firstVertIndex;
			tris[triIndex++] = firstVertIndex + 3;
			tris[triIndex++] = firstVertIndex + 1;
			
			// Tri 2
			tris[triIndex++] = firstVertIndex;
			tris[triIndex++] = firstVertIndex + 2;
			tris[triIndex++] = firstVertIndex + 3;
		}

	
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
		Vector3 prevHalfWidth = new Vector3(size * 0.5f * prevLength.y, -size * 0.5f * prevLength.x, 0);
		vertices[0] = points[0] - prevHalfWidth;
		vertices[1] = points[0] + prevHalfWidth;
		
		// Start at 1 because we've already done the first pair
		Vector3 nextLength = new Vector3(0, 0, 0);
		Vector3 nextHalfWidth = new Vector3(0, 0, 0);
		for (int i = 1; i < numStages; ++i){
			nextLength = points[i+1] - points[i];
			nextLength.Normalize();
			nextHalfWidth = new Vector3(size * 0.5f * nextLength.y, -size * 0.5f * nextLength.x, 0);
			
			// Calc vertex indices we are going to write into
			int vi0 = i * 2;
			int vi1 = vi0 + 1;
			
			// Work out positions of vertices if there were no other stasges to consider
			vertices[vi0] = points[i] - nextHalfWidth;
			vertices[vi1] = points[i] + nextHalfWidth;
			
			// If the two "length" vectors are nearly parallel, then just leave them as they are
			// otherwise, adjust them to be at the intersectoin of the two stages
			if (!MathUtils.FP.Feq (Mathf.Abs(Vector3.Dot(prevLength,nextLength)), 1, 0.01f)){
				// Create four line euqations (p = v + lamba * r). two for the sides of the quad for the previous stage and two for the sides
				// of the quad for the next stage
				
				// Previous stage
				Vector3 prevV0 = vertices[vi0-2];
				Vector3 prevR0 = prevLength;
				Vector2 prevV1 = vertices[vi1-2];
				Vector2 prevR1 = prevLength;
				
				// Next stage
				Vector3 nextV0 = vertices[vi0];
				Vector3 nextR0 = nextLength;
				Vector2 nextV1 = vertices[vi1];
				Vector2 nextR1 = nextLength;	
				
				// Find intersection of the two pairs of lines
				vertices[vi0] = FindIntersetion(prevV0, prevR0, nextV0, nextR0);
				vertices[vi1] = FindIntersetion(prevV1, prevR1, nextV1, nextR1);
				
				
				// Test the cente lione
				Vector2 prevP2 = points[i-1];
				Vector2 prevR2 = points[i] - points[i-1];
				prevR2.Normalize();
				Vector2 halfWidthPrev2 =  new Vector3(size * 0.5f * prevR2.y, -size * 0.5f * prevR2.x);
				Vector2 nextP2 = points[i];
				Vector2 nextR2 = points[i + 1] - points[i];
				nextR2.Normalize();
				Vector2 halfWidthNext2 =  new Vector3(size * 0.5f * nextR2.y, -size * 0.5f * nextR2.x);
				
				prevP2 += halfWidthPrev2;
				nextP2 += halfWidthNext2;
				
				Vector3 testCentre = FindIntersetion(prevP2, prevR2, nextP2, nextR2);
				
				
				// test for errors
				if (vertices[vi0].y  + vertices[vi1].y > 2.2 && false){
					Debug.Log ("Detected an error");
					Debug.Log ("int i = 0;");
					for (int j = 0; j < numPoints; ++j){
						Debug.Log ("points[i++] = new Vector3 (" + points[j].x + "f, "  + points[j].y + "f, 0f);");
					}
					AppHelper.Quit();
					
				}

//				Vector3 test0 = FindIntersetion(nextV0, nextR0, prevV0, prevR0);
//				Vector3 test1 = FindIntersetion(nextV1, nextR1, prevV1, prevR1);			
			}
			prevLength = nextLength;			
		}
		
		// Do the last two points
		vertices[numVerts-2] = points[numStages] - nextHalfWidth;
		vertices[numVerts-1] = points[numStages] + nextHalfWidth;
		
		// Set the z to help debug
//		for (int i = 0; i < numVerts; ++i){
//			vertices[i].z = (i/2);
//		}
		
	}
	
	Vector3 FindIntersetion(Vector3 p0, Vector3 r0, Vector3 p1, Vector3 r1){
		float numerator = (p1.x-p0.x)*r0.y - (p1.y-p0.y)*r0.x;
		float denominator = r1.y*r0.x-r1.x*r0.y;
		if (MathUtils.FP.Feq(denominator, 0)){
			Debug.Log ("Divide by zero when intersecting lines");
		}
		float lambda1 = numerator / denominator;
		return p1 + lambda1 * r1;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMesh();
	
	}
	
	float GetNormalSample(float mean, float sd){
		float u1 = Random.Range(0f, 1f);
		float u2 = Random.Range(0f, 1f);

		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
		return mean + sd * randStdNormal; 
	}
}
