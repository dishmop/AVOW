using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;

public class SubstanceBaker : MonoBehaviour {
	public ProceduralMaterial[] materials;
	ProceduralMaterial currentMat;
	
	int index = 0;
	
	enum State{
		kWaiting,
		kStartBuild,
		kProcessing,
		kExtract,
		kRefreshResources,
		kFinish,
	};
	State state = State.kWaiting;
	
	public void TriggerBake(){
		if (state == State.kWaiting){
			state = State.kStartBuild;
			index = 0;
		}
	}

	// Use this for initialization
	void FixedUpdate () {
		
		switch (state){
			case State.kStartBuild:{
				if (index >= materials.Count ()){
					state = State.kRefreshResources;
				}
				else{
					currentMat = materials[index];
					currentMat.isReadable = true;
					currentMat.RebuildTextures();
					state = State.kProcessing;
				}
				break;
			}
			case State.kProcessing:{
				if (!currentMat.isProcessing){
					state = State.kExtract;	
				}
				break;
			}
			case State.kExtract:{
				

				Extract("_MainTex");
				Extract("_BumpMap");
				Extract("_ParallaxMap");
				
				++index;
				state = State.kStartBuild;
				break;
				
			}
			case State.kRefreshResources:{
				AssetDatabase.Refresh();
				state = State.kFinish;
				break;
				
			}
		}

	
	}
	

	void Extract(string textureName){
		ProceduralTexture mainTex = currentMat.GetTexture(textureName) as ProceduralTexture;
		Color32[] pixels = mainTex.GetPixels32(0, 0, mainTex.width, mainTex.height);
		string name = currentMat.name + textureName;
		SaveTexture(name, pixels, mainTex.width, mainTex.height);
	}
	
	void SaveTexture(string name, Color32[] pixels, int width, int height){
		Texture2D newTexture = new Texture2D(width, height);
		Color[] newPixel = new Color[pixels.Count()];
		for (int i= 0 ; i < pixels.Count(); ++i){
			newPixel[i] = pixels[i];
		}
		
		newTexture.SetPixels(newPixel);
		byte[] bytes = newTexture.EncodeToPNG();
		
		File.WriteAllBytes(Application.dataPath + "/Resources/SubstanceBakes/" + name + ".png", bytes);
		Object.Destroy(newTexture);
		
	}
	
	void OnGUI(){
		GUI.Label(new Rect(10, 10, 400, 50), "State = " + state.ToString() + ", index = " + index);
	}
}
