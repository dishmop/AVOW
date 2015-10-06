using UnityEngine;
using System.Collections;

public class TextureConstructor : MonoBehaviour {
	public Texture2D blueSquare;
	public Texture2D blueRod;
	public Texture2D blueBar;
	public Texture2D blueSquareX5;
	
	public Texture2D blueRodX5;
	
	public Texture2D greenSquare;
	public Texture2D greenRod;
	
	public Texture2D greySquare;
		
	
	
	public delegate Vector4 ColorSelector(float x);
	public delegate Color PixelShader(Vector2 uv, ColorSelector colSelector);
	
	// Use this for initialization
	void Start () {
	
		ConstructTexture(blueSquare, CalcSquareColor, CalcBlueCol);
		ConstructTexture(greenSquare, CalcSquareColor, CalcGreenCol);
		ConstructTexture(blueSquare, CalcSquareColor, CalcBlueCol);
		ConstructTexture(blueSquareX5, CalcSquareColorX5, CalcBlueCol);
		
		
		ConstructTexture(blueRod, CalcRod, CalcBlueCol);
		ConstructTexture(greenRod, CalcRod, CalcGreenCol);
		
		ConstructTexture(blueRodX5, CalcRodX5, CalcBlueCol);
		
		//		ConstructBlueRod();
//		ConstructBlueBar();
	
	}
	
	// Update is called once per frame
	void ConstructTexture (Texture2D texture, PixelShader shader, ColorSelector colSelector) {
		for (int x = 0; x < texture.width; ++x){
			for (int y = 0; y < texture.height; ++y){
				Vector2 uv = new Vector2((float)x / (float)texture.width, (float)y / (float)texture.height);
				texture.SetPixel(x, y, shader(uv, colSelector));
			}
		}
		texture.Apply();
	}
	
	Color CalcSquareColor(Vector2 uv, ColorSelector colSelector){
		float left = Mathf.Abs(uv[0]);
		float right = Mathf.Abs(1 - uv[0]);
		float top = Mathf.Abs(uv[1]);
		float bottom = Mathf.Abs(1 - uv[1]);
		
		Vector4 colLeft = 	colSelector(left);
		Vector4 colRight = 	colSelector(right);
		Vector4 colTop =	colSelector(top);
		Vector4 colBottom = colSelector(bottom);
		
		Vector4 resultCol =  (colLeft + colRight +  colTop + colBottom);
		return new Color(resultCol[0], resultCol[1], resultCol[2], resultCol[3]);
	}
	
	
	
	Color CalcRod(Vector2 uv, ColorSelector colSelector){
		//  return float4(0, 0, 0, 0);
		float xx = uv[0] - 0.5f;
		float yy = uv[1] - 0.5f;
		
		if (yy <= 0){
			float x = Mathf.Abs(xx);
			
			return colSelector(x);
		}
		else{
			float x = Mathf.Sqrt(xx * xx + yy * yy);
			
			return colSelector(x);
		}
	}
	
	Color CalcRodX5(Vector2 uv, ColorSelector colSelector){
		float sizeMul = 5;
		//  return float4(0, 0, 0, 0);
		float xx = uv[0] - 0.5f;
		float yy = uv[1] - 0.5f;
		
		xx *= 1f/sizeMul;
		yy *= 1f/sizeMul;
		
		if (yy <= 0){
			float x = Mathf.Abs(xx);
			
			return colSelector(x);
		}
		else{
			float x = Mathf.Sqrt(xx * xx + yy * yy);
			
			return colSelector(x);
		}
	}	
	
	
	Color CalcSquareColorX5(Vector2 uv, ColorSelector colSelector){
		float sizeMul = 5;
		
		float left = Mathf.Abs(uv[0]);
		float right = Mathf.Abs(1 - uv[0]);
		float top = Mathf.Abs(uv[1]);
		float bottom = Mathf.Abs(1 - uv[1]);
		
		left *= 1f/sizeMul;
		right *= 1f/sizeMul;
		top *= 1f/sizeMul;
		bottom *= 1f/sizeMul;
				
		Vector4 colLeft = 	colSelector(left);
		Vector4 colRight = 	colSelector(right);
		Vector4 colTop =	colSelector(top);
		Vector4 colBottom = colSelector(bottom);
		
		Vector4 resultCol =  (colLeft + colRight +  colTop + colBottom);
		return new Color(resultCol[0], resultCol[1], resultCol[2], resultCol[3]);
	}
		
		//		        	return lerp(_Color0, _Color1, val);
		
	
	float CalcCurve(float x, float offset)
	{
		//float offset = 0.5;
		return 1*(1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));
		
	}
	
	Vector4 CalcGreyCol(float x){		       		
		return new Vector4(CalcCurve(x, 0.01f), CalcCurve(x, 0.01f), CalcCurve(x, 0.01f), CalcCurve(x, 0.1f));
	}	
	
	Vector4 CalcBlueCol(float x){		       		
		return new Vector4(CalcCurve(x, 0.01f), CalcCurve(x, 0.01f), CalcCurve(x, 0.2f), CalcCurve(x, 0.1f));
	}
	
	Vector4 CalcGreenCol(float x){		       		
		return  0.8f * new Vector4(CalcCurve(x, 0.01f), CalcCurve(x, 0.2f), CalcCurve(x, 0.01f), CalcCurve(x, 0.1f));
	}
	
}
