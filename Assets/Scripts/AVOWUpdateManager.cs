using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWUpdateManager : MonoBehaviour {

	public static AVOWUpdateManager singleton = null;

	public GameObject config;
	public GameObject sim;
	public GameObject gameModes;
	public GameObject ui;
	public GameObject graph;
	public GameObject circuitCreator;
	public GameObject circuitSubsetter;
	public GameObject backStory;
	public GameObject tutorialManager;
	public GameObject tutorialText;
	public GameObject camController;
	public GameObject objectiveManager;
	public GameObject battery;
	public GameObject pusher;
	
	public float playbackSpeed = 1;
	
	float gameStartTime;
	float gameTime;
	
	float lastFrameTime = -0.016666f;
	float thisFrameTime = 0;
	float fps;
	
	public float GetGameTime(){
		return gameTime;
	}
	
	public void ResetGameTime(){
		gameTime = 0;
	
	}
	
	public long lastCount = 0;
	
	public long GetCountDelta(){
		return 0;
		
	}
	
	// need to seralise the graph first becuase when we deserialise, we need to refernce objects whcih need to be thte
	public void SerialiseGameState(BinaryWriter bw){
//		Debug.Log("SerialiseGameState: " + GetCountDelta());
		config.GetComponent<AVOWConfig>().Serialise(bw);
//		Debug.Log("AVOWConfig: " + GetCountDelta());
		
		gameModes.GetComponent<AVOWGameModes>().Serialise(bw);
//		Debug.Log("AVOWGameModes: " + GetCountDelta());
		
		graph.GetComponent<AVOWGraph>().Serialise(bw);
//		Debug.Log("AVOWGraph: " + GetCountDelta());
		
		ui.GetComponent<AVOWUI>().Serialise(bw);
//		Debug.Log("AVOWUI: " + GetCountDelta());
		
		objectiveManager.GetComponent<AVOWObjectiveManager>().Serialise(bw);
//		Debug.Log("AVOWObjectiveManager: " + GetCountDelta());
		
		tutorialText.GetComponent<AVOWTutorialText>().Serialise(bw);
//		Debug.Log("AVOWTutorialText: " + GetCountDelta());

		// Do global game stats
		bw.Write (fps);
	}
	
	public void DeserialiseGameState(BinaryReader br){
		config.GetComponent<AVOWConfig>().Deserialise(br);
		gameModes.GetComponent<AVOWGameModes>().Deserialise(br);
		graph.GetComponent<AVOWGraph>().Deserialise(br);
		ui.GetComponent<AVOWUI>().Deserialise(br);
		objectiveManager.GetComponent<AVOWObjectiveManager>().Deserialise(br);
		tutorialText.GetComponent<AVOWTutorialText>().Deserialise(br);
		fps = br.ReadSingle();
		Debug.Log ("FPS = " + fps);
	}
	
	// Use this for initialization
	void Start () {
		config.GetComponent<AVOWConfig>().Initialise();
		sim.GetComponent<AVOWSim>().Initialise();
		gameModes.GetComponent<AVOWGameModes>().Initialise();
		graph.GetComponent<AVOWGraph>().Initialise();
		tutorialText.GetComponent<AVOWTutorialText>().Initialise();
		circuitSubsetter.GetComponent<AVOWCircuitSubsetter>().Startup();
		backStory.GetComponent<AVOWBackStoryCutscene>().Initialise();
		objectiveManager.GetComponent<AVOWObjectiveManager>().Initialise();
		battery.GetComponent<AVOWBattery>().Initialise();
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		gameTime += Time.fixedDeltaTime;
		graph.GetComponent<AVOWGraph>().ResetOptFlags();
		objectiveManager.GetComponent<AVOWObjectiveManager>().ResetOptFlags();
		ui.GetComponent<AVOWUI>().ResetOptFlags();
		
		ui.GetComponent<AVOWUI>().GameUpdate();
		gameModes.GetComponent<AVOWGameModes>().GameUpdate();
		circuitCreator.GetComponent<AVOWCircuitCreator>().GameUpdate();
		graph.GetComponent<AVOWGraph>().GameUpdate();
		sim.GetComponent<AVOWSim>().GameUpdate();
		gameModes.GetComponent<AVOWGameModes>().GameUpdate();
		tutorialText.GetComponent<AVOWTutorialText>().GameUpdate();
		objectiveManager.GetComponent<AVOWObjectiveManager>().GameUpdate();
		tutorialManager.GetComponent<AVOWTutorialManager>().GameUpdate();
			
		// this is a bit hacky - check for errors in the sumulation
		if (sim.GetComponent<AVOWSim>().errorFlag){
			graph.GetComponent<AVOWGraph>().ClearCircuit();
			
			// Simple start
			GameObject node0GO = graph.GetComponent<AVOWGraph>().AddNode ();
			GameObject node1GO = graph.GetComponent<AVOWGraph>().AddNode ();
			
			
			graph.GetComponent<AVOWGraph>().PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.cellPrefab) as GameObject, node0GO, node1GO);
			
			// For some reason, we can't just make a graph with nothing in it - we need to make a resisotr
			// then remove it
			graph.GetComponent<AVOWGraph>().PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.resistorPrefab) as GameObject, node1GO, node0GO);
			graph.GetComponent<AVOWGraph>().allComponents[1].GetComponent<AVOWComponent>().Kill(45);			
			Debug.Log ("Detected erroronsous circuit - clearing the circuit");
			sim.GetComponent<AVOWSim>().errorFlag = false;
		}
		
	}
	
	
	void Update(){
		thisFrameTime = Time.time;
		fps = 1/(thisFrameTime - lastFrameTime);
		lastFrameTime = thisFrameTime;
		
		ui.GetComponent<AVOWUI>().RenderUpdate();

		config.GetComponent<AVOWConfig>().RenderUpdate();
		gameModes.GetComponent<AVOWGameModes>().RenderUpdate();
		tutorialText.GetComponent<AVOWTutorialText>().RenderUpdate();
		graph.GetComponent<AVOWGraph>().RenderUpdate();
		backStory.GetComponent<AVOWBackStoryCutscene>().RenderUpdate();
		camController.GetComponent<AVOWCamControl>().RenderUpdate();
		objectiveManager.GetComponent<AVOWObjectiveManager>().RenderUpdate();
		battery.GetComponent<AVOWBattery>().RenderUpdate();
		pusher.GetComponent<AVOWPusher>().RenderUpdate();

	}
	
	
	
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	void OnDestroy(){
		singleton = null;
	}	
}
