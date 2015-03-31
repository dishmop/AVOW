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
	public GameObject tutorial;
	public GameObject camController;
	public GameObject objectiveManager;
	public GameObject telemetry;
	public GameObject battery;
	
	
	float gameStartTime;
	float gameTime;
	
	public float GetGameTime(){
		return gameTime;
	}
	
	public void ResetGameTime(){
		gameStartTime = Time.fixedTime;
		gameTime = 0;
	
	}
	
	// need to seralise the graph first becuase when we deserialise, we need to refernce objects whcih need to be thte
	public void SerialiseGameState(BinaryWriter bw){
		graph.GetComponent<AVOWGraph>().Serialise(bw);
		ui.GetComponent<AVOWUI>().Serialise(bw);
		objectiveManager.GetComponent<AVOWObjectiveManager>().Serialise(bw);
	}
	
	public void DeserialiseGameState(BinaryReader br){
		graph.GetComponent<AVOWGraph>().Deserialise(br);
		ui.GetComponent<AVOWUI>().Deserialise(br);
		objectiveManager.GetComponent<AVOWObjectiveManager>().Deserialise(br);
	}
	
	// Use this for initialization
	void Start () {
		config.GetComponent<AVOWConfig>().Initialise();
		telemetry.GetComponent<Telemetry>().Initialise();
		telemetry.GetComponent<AVOWTelemetry>().Initialise();
		sim.GetComponent<AVOWSim>().Initialise();
		gameModes.GetComponent<AVOWGameModes>().Initialise();
		circuitSubsetter.GetComponent<AVOWCircuitSubsetter>().Startup();
		backStory.GetComponent<AVOWBackStoryCutscene>().Initialise();
		objectiveManager.GetComponent<AVOWObjectiveManager>().Initialise();
		battery.GetComponent<AVOWBattery>().Initialise();
		
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		gameTime = Time.fixedTime - gameStartTime;
		if (!telemetry.GetComponent<Telemetry>().isPlaying){
			ui.GetComponent<AVOWUI>().GameUpdate();
			gameModes.GetComponent<AVOWGameModes>().GameUpdate();
			circuitCreator.GetComponent<AVOWCircuitCreator>().GameUpdate();
			graph.GetComponent<AVOWGraph>().GameUpdate();
			sim.GetComponent<AVOWSim>().GameUpdate();
			objectiveManager.GetComponent<AVOWObjectiveManager>().GameUpdate();
			tutorial.GetComponent<AVOWTutorialManager>().GameUpdate();
			telemetry.GetComponent<Telemetry>().GameUpdate();
		}
		else{
			gameModes.GetComponent<AVOWGameModes>().GameUpdate();
			telemetry.GetComponent<Telemetry>().GameUpdate();
		}
		
	}
	
	
	void Update(){
		if (!telemetry.GetComponent<Telemetry>().isPlaying){
			ui.GetComponent<AVOWUI>().RenderUpdate();
		}

		config.GetComponent<AVOWConfig>().RenderUpdate();
		gameModes.GetComponent<AVOWGameModes>().RenderUpdate();
		graph.GetComponent<AVOWGraph>().RenderUpdate();
		backStory.GetComponent<AVOWBackStoryCutscene>().RenderUpdate();
		camController.GetComponent<AVOWCamControl>().RenderUpdate();
		objectiveManager.GetComponent<AVOWObjectiveManager>().RenderUpdate();
		battery.GetComponent<AVOWBattery>().RenderUpdate();
		
		if (telemetry.GetComponent<Telemetry>().isRecording){
			AVOWTelemetry.singleton.WriteGameUpdateEvent();
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
}
