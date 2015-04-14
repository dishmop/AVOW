
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWUI : MonoBehaviour {
	public static AVOWUI singleton = null;
	
	
	public GameObject resistorPrefab;
	public GameObject cellPrefab;
	
	
	public GameObject cursorBlueCubePrefab;
	public GameObject cursorGreenCubePrefab;
	public GameObject cursorGreyCubePrefab;
	public GameObject lighteningPrefab;
	public GameObject greenLighteningPrefab;
	public GameObject electricAudioPrefab;
	public AudioSource spinAS;
	public AudioSource pingAS;
	
	
	public bool canCreate = true;
	bool lastCanCreate = true;
	
	AVOWGraph graph = null;
	
	SpringValue cubeBrightness = new SpringValue(0, SpringValue.Mode.kAsymptotic);
	float lastFlashTime = 0;
	
	const int		kLoadSaveVersion = 1;		
	GameObject electricAudio;
		
	public enum ToolMode{
		kCreate,
		kDelete
	}
	
	ToolMode mode = ToolMode.kCreate;
			
	AVOWUITool	uiTool;
		

	public Stack<AVOWCommand> 	commands = new Stack<AVOWCommand>();

	public void ResetOptFlags(){
		if (uiTool != null){
			uiTool.ResetOptFlags();
		}
	}
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
	

	public void Restart(){
		graph = null;
		RemakeUITool();
		
		if (electricAudio != null){
			GameObject.Destroy(electricAudio);
		}
		electricAudio = Instantiate(electricAudioPrefab);
		electricAudio.transform.parent = transform;
		
		electricAudio.transform.localPosition = Vector3.zero;
		electricAudio.GetComponent<AudioSource>().Play();
		electricAudio.GetComponent<AudioSource>().pitch = 2;
		SetElectricAudioVolume(0);
	}
	
	public void PlaySpin(){
		spinAS.Play ();
	}
	
	public void PlayPing(){
		if (!pingAS.isPlaying) pingAS.Play ();
	}
	
	public void SetElectricAudioVolume(float vol){
		if (electricAudio != null){
			if (AVOWGameModes.singleton.IsPlayingLevel()){
				electricAudio.GetComponent<AudioSource>().volume = 0.003f * vol;
			}
			else{
				electricAudio.GetComponent<AudioSource>().volume = 0;
			}
		}
	
	}
	
	void RemakeUITool(){
		if (uiTool != null){
			uiTool.OnDestroy();
			uiTool = null;
		}
		if (canCreate == false){
			SetDisableTool();
		}
		else{
			if (mode == ToolMode.kCreate){
				SetCreateTool();
			}
			else{
				SetDeleteTool();	
			}
		}
	}
	
	
	public AVOWUITool  GetUITool(){
		return uiTool;
	}
	
	public void RenderUpdate(){
		if (Time.fixedTime > lastFlashTime + 0.5f){
			cubeBrightness.Set (1);
			cubeBrightness.SetSpeed(2);
		}
		cubeBrightness.Update ();
		
		if (uiTool != null){
			GameObject temp = uiTool.GetCursorCube();
			if (temp == null){
				Debug.LogError("temp = null");
			}
			int numMaterials = uiTool.GetCursorCube().GetComponent<Renderer>().materials.Length;
			uiTool.GetCursorCube().GetComponent<Renderer>().materials[numMaterials - 1].SetFloat("_Intensity", cubeBrightness.GetValue());
			uiTool.RenderUpdate();
		}
	}
	

	
	public void GameUpdate(){
		
		if (AVOWCircuitCreator.singleton.IsReady()){
			if (graph == null){
				Startup();
			}
			
			if (AVOWGameModes.singleton.state == AVOWGameModes.GameModeState.kGameOver){
				if (uiTool != null){		
					uiTool.OnDestroy();
					uiTool = null;
				}
				return;
			
			}
			if (mode == ToolMode.kCreate && lastCanCreate != canCreate && uiTool != null && !uiTool.IsHolding()){
				lastCanCreate = canCreate;
				RemakeUITool();
			
			}
			uiTool.GameUpdate();
			uiTool.GetCursorCube().SetActive(!AVOWGameModes.singleton.showPointer);
		}

	}
	

	
	public void TriggerLight(){
		cubeBrightness.Force (1);
		cubeBrightness.Set (0);
		cubeBrightness.SetSpeed(10);
		lastFlashTime = Time.fixedTime;
		
	}
	
	public GameObject InstantiateBlueCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorBlueCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	public GameObject InstantiateGreenCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorGreenCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	public GameObject InstantiateGreyCursorCube(){
		GameObject obj = GameObject.Instantiate(cursorGreyCubePrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	
	public GameObject InstantiateLightening(){
		GameObject obj = GameObject.Instantiate(lighteningPrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	
	public GameObject InstantiateGreenLightening(){
		GameObject obj = GameObject.Instantiate(greenLighteningPrefab) as GameObject;
		obj.transform.parent = transform;
		return obj;
	}
	


	

	public ToolMode GetUIMode(){
		return mode;
	}

	
	void Startup(){
	
		graph = AVOWGraph.singleton;
		
		// Simple start
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		
		// For some reason, we can't just make a graph with nothing in it - we need to make a resisotr
		// then remove it
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node0GO);
		graph.allComponents[1].GetComponent<AVOWComponent>().Kill(45);
		
		/*
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		GameObject node2GO = graph.AddNode ();
		
		graph.PlaceComponent(GameObject.Instantiate(cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node1GO, node2GO);	
		graph.PlaceComponent(GameObject.Instantiate(resistorPrefab) as GameObject, node2GO, node0GO);	
		*/
		
		AVOWSim.singleton.Recalc();
		mode = ToolMode.kCreate;
		RemakeUITool();



		
	}
	
	int DetermineToolCode(){
		int code = -1;
		if (uiTool is AVOWUICreateTool){
			code = 1;
		}
		else if (uiTool is AVOWUIDeleteTool){
			code = 2;
		}
		else if (uiTool is AVOWUIDisabledTool){
			code = 3;
		}
		else{
			code = 0;
		}
		
		return code;
	}
	
	void ModifyToolSelection(int newCode){
		int currentCode = DetermineToolCode();
		if (newCode != currentCode){
			switch(newCode){
				case 0:{
					uiTool.OnDestroy();
					uiTool = null;
					break;
				}
				case 1:{
					SetCreateTool();
					break;
				}
				case 2:{
					SetDeleteTool();
					break;
				}
				case 3:{
					SetDisableTool();
					break;
				}
			}
		}


	}
	
	public void Serialise(BinaryWriter bw){
		int code = DetermineToolCode();

		bw.Write (kLoadSaveVersion);
		bw.Write ((int)mode);
		bw.Write (canCreate);
		bw.Write (lastCanCreate);
		bw.Write (electricAudio.GetComponent<AudioSource>().volume);
		bw.Write (code);
		if (uiTool != null){
			uiTool.Serialise(bw);
		}
		bw.Write (graph != null);
		
	}
	
	public void Deserialise(BinaryReader br){
		int version = br.ReadInt32();
		switch (version){
			case kLoadSaveVersion:{
				mode = (ToolMode) br.ReadInt32();
				canCreate = br.ReadBoolean();
				lastCanCreate = br.ReadBoolean();
				electricAudio.GetComponent<AudioSource>().volume = br.ReadSingle ();
			
				int code = br.ReadInt32();
				ModifyToolSelection(code);
				if (uiTool != null){
					uiTool.Deserialise(br);
				}
				break;
			}
		}
		bool hasGraph = br.ReadBoolean();
		if (hasGraph){
			graph = AVOWGraph.singleton;
		}
		else{
			graph = null;
		}
	}
	
	public GameObject PlaceResistor(GameObject node0GO, GameObject node1GO){
		GameObject newResistor = GameObject.Instantiate(resistorPrefab) as GameObject;
		graph.PlaceComponent(newResistor, node0GO, node1GO);	
		return newResistor;
	}
	
	public void SetCreateTool(){
		if (uiTool != null) uiTool.OnDestroy();
		uiTool = new AVOWUICreateTool();
		uiTool.Startup();
		PlayPing ();
		mode = ToolMode.kCreate;
	}
	
	public void SetDeleteTool(){
		if (uiTool != null) uiTool.OnDestroy();
		uiTool = new AVOWUIDeleteTool();
		uiTool.Startup();
		PlayPing ();
		mode = ToolMode.kDelete;
	}
	
	public void SetDisableTool(){
		if (uiTool != null) uiTool.OnDestroy();
		uiTool = new AVOWUIDisabledTool();
		uiTool.Startup();
	}
	
}

