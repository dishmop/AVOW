using UnityEngine;
using System.Collections;

public class AVOWUpdateManager : MonoBehaviour {

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
	
	// Use this for initialization
	void Start () {
		config.GetComponent<AVOWConfig>().Initialise();
		sim.GetComponent<AVOWSim>().Initialise();
		gameModes.GetComponent<AVOWGameModes>().Initialise();
		circuitSubsetter.GetComponent<AVOWCircuitSubsetter>().Startup();
		backStory.GetComponent<AVOWBackStoryCutscene>().Initialise();
		objectiveManager.GetComponent<AVOWObjectiveManager>().Initialise();
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		ui.GetComponent<AVOWUI>().FixedUpdate();
		gameModes.GetComponent<AVOWGameModes>().GameUpdate();
		circuitCreator.GetComponent<AVOWCircuitCreator>().GameUpdate();
//		circuitSubsetter.GetComponent<AVOWCircuitSubsetter>().GameUpdate();
		graph.GetComponent<AVOWGraph>().GameUpdate();
		sim.GetComponent<AVOWSim>().GameUpdate();
		tutorial.GetComponent<AVOWTutorialManager>().GameUpdate();
		
	}
	
	
	void Update(){
		config.GetComponent<AVOWConfig>().RenderUpdate();
		ui.GetComponent<AVOWUI>().RenderUpdate();
		gameModes.GetComponent<AVOWGameModes>().RenderUpdate();
		graph.GetComponent<AVOWGraph>().RenderUpdate();
		backStory.GetComponent<AVOWBackStoryCutscene>().RenderUpdate();
		camController.GetComponent<AVOWCamControl>().RenderUpdate();
		objectiveManager.GetComponent<AVOWObjectiveManager>().RenderUpdate();
	}
}
