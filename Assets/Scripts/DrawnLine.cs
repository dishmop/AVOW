using UnityEngine;
using System.Collections;

public class DrawnLine : MonoBehaviour {
	public Vector2 	fromPos = new Vector2(0, 0);
	public Vector2 	toPos = new Vector2(0, 0);
	public Color	lineColor = Color.black;
	
	
	SpringValue drawLenMul = new SpringValue(0, SpringValue.Mode.kAsymptotic);
	
	
	Vector2 lastFromPos = new Vector2(0, 0);
	Vector2 lastToPos = new Vector2(0, 0);
	Color	lastCol = Color.black;

	// Use this for initialization
	void Start () {
		OnChangePoints();
	
	}
	
	// Use this for initialization
	void Awake () {
	}
	
	public void Draw(Vector2 fromPos, Vector2 toPos, Color col){
		this.fromPos = fromPos;
		this.toPos = toPos;
		this.lineColor = col;
		
		drawLenMul.Force(0);
		drawLenMul.Set(1);
		
	}
	
	// Update is called once per frame
	void Update () {
		drawLenMul.Update();
	
		if (!MathUtils.FP.Feq (fromPos.x, lastFromPos.x) ||
		    !MathUtils.FP.Feq (fromPos.y, lastFromPos.y) ||
		    !MathUtils.FP.Feq (toPos.x, lastToPos.x) ||
		    !MathUtils.FP.Feq (toPos.y, lastToPos.y) ||
		    !MathUtils.FP.Feq (lineColor.r, lastCol.r) ||
		    !MathUtils.FP.Feq (lineColor.g, lastCol.g) ||
		    !MathUtils.FP.Feq (lineColor.b, lastCol.b) ||
		    !MathUtils.FP.Feq (lineColor.a, lastCol.a) ||
		    !drawLenMul.IsAtTarget()){

			lastFromPos = fromPos;
			lastToPos = toPos;
			lastCol = lineColor;
		}
		OnChangePoints();		
	}
	
	void OnChangePoints(){
	
		Vector2 relVec = toPos - fromPos;
		float len = relVec.magnitude * drawLenMul.GetValue();
		transform.localPosition = new Vector3(fromPos.x, fromPos.y, 0);
		transform.FindChild("LocalTransform").localScale = new Vector3(1, len, 1);
		float angle = -Mathf.Atan2(relVec.x, relVec.y);
		transform.FindChild("LocalTransform").rotation = Quaternion.Euler(0, 0, 180 * angle / Mathf.PI);
		
		float yUV = len;
		Vector2[] uvs = new Vector2[4];
		uvs[0] = new Vector2(1, 0);
		uvs[1] = new Vector2(0, yUV);
		uvs[2] = new Vector2(1, yUV);
		uvs[3] = new Vector2(0, 0);
		
		transform.FindChild("LocalTransform").FindChild ("DrawnLineMesh").GetComponent<MeshFilter>().mesh.uv = uvs;
		Color test = 	transform.FindChild("LocalTransform").FindChild ("DrawnLineMesh").renderer.material.GetColor("_Color");
		transform.FindChild("LocalTransform").FindChild ("DrawnLineMesh").renderer.material.SetColor("_Color", lineColor);
		
	}
		
}
