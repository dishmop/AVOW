using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWCircuitCreator : MonoBehaviour {
	public static AVOWCircuitCreator singleton = null;
	
	public string 		filename = "LevelPerms";
	
	int maxNumResistors = -1;
	
	
	public enum State{
		kOff,
		kStart,
		kProcessing,
		kIsReady,
		kStop,
	}
	
	List< AVOWCircuitTarget > fracResults = new List< AVOWCircuitTarget > ();
	//List< Eppy.Tuple<float, List<float>> > lCMResults;
	
	int currentLCM = 1;
	int count = 0;
	
	State state = State.kOff;
	IEnumerator<AVOWCircuitTarget> it = null;
	
	int numUsed = 0;
	AVOWGraph graph;
	
	Dictionary<int, Eppy.Tuple<int[], int[]>> enumerationCache = new Dictionary<int, Eppy.Tuple<int[], int[]>>();
	
	Dictionary<int, Eppy.Tuple<int, int>> splitNodeCache = new Dictionary<int, Eppy.Tuple<int, int>>();
	
	Stack<AVOWCommand>	commands = new Stack<AVOWCommand>();
	
	public int GetLCM(){
		return currentLCM;
	}
	
	public bool IsReady(){
		return state == State.kIsReady || state == State.kOff;
	}
	
	public string CreateFilename(){
		return filename + "_" + maxNumResistors.ToString();
	}
	
	public List< AVOWCircuitTarget > GetResults(){
		return fracResults;
	
	}
	
	public void Initialise(int maxNumResistors){
		this.maxNumResistors = maxNumResistors;
		state = State.kStart;
	}
	
	public void Deinitialise(){
		state = State.kStop;
	}
	
	
	// We saving using the standard file system
	string BuildFullPath(){
		return Application.dataPath + "/Resources/Perms/" + CreateFilename() + ".bytes";
		
	}
	
	// We load using the resources
	string BuildResourcePath(){
		return "Perms/" + CreateFilename();
	}
	
	public void SerializePerms(Stream stream){
	
		BinaryWriter bw = new BinaryWriter(stream);
		
		bw.Write (fracResults.Count);
		for (int i = 0; i < fracResults.Count; ++i){
			bw.Write (fracResults[i].totalCurrent);
			bw.Write (fracResults[i].componentDesc.Count);
			for (int j = 0; j < fracResults[i].componentDesc.Count; ++j){
				bw.Write (fracResults[i].componentDesc[j][0]);
				bw.Write (fracResults[i].componentDesc[j][1]);
				bw.Write (fracResults[i].componentDesc[j][2]);
			}
		}
		
	}
	
	public void DeserializePerms(Stream stream){
		BinaryReader br = new BinaryReader(stream);
		
		int resultsCount = br.ReadInt32();
		fracResults = new List< AVOWCircuitTarget > ();
		for (int i = 0; i < resultsCount; ++i){
			float val = br.ReadSingle();
			AVOWCircuitTarget item = new AVOWCircuitTarget(val, new List<Vector3>());
			int listCount = br.ReadInt32();
			for (int j = 0; j < listCount; ++j){
				Vector3 desc = new Vector3();
				desc[0] = br.ReadSingle();
				desc[1] = br.ReadSingle();
				desc[2] = br.ReadSingle();
				item.componentDesc.Add (desc);
			}
			item.CalcStats();
			fracResults.Add (item);
		}
	}
	
	
	
	public bool LoadPerms(){

		TextAsset asset = Resources.Load(BuildResourcePath ()) as TextAsset;
		if (asset != null){
			//Debug.Log ("Loading asset");
			Stream s = new MemoryStream(asset.bytes);
			DeserializePerms(s);
			Resources.UnloadAsset(asset);
			return true;
		}	
		return false;			
	}
	
	// Does the actual serialising
	public void SavePerms(){
		#if UNITY_EDITOR		
		FileStream file = File.Create(BuildFullPath());
		
		SerializePerms(file);
		
		
		file.Close();
		
		// Ensure the assets are all realoaded and the cache cleared.
		UnityEditor.AssetDatabase.Refresh();
		#endif
		
	}	
	
	
	// Use this for initialization
	void Start () {
		graph = AVOWGraph.singleton;
		
		
	}
	
	// Update is called once per frame
	public void GameUpdate () {
		switch (state){
			case State.kStart:{
				if (LoadPerms()){
					PrintResults();
					PrintFracResults();
//					PostProcess();
					PrintResults();
					
					state = State.kIsReady;
				}
				else{
					it = GenerateIterate().GetEnumerator();
					state = State.kProcessing;
				}
				break;
			}
			case State.kProcessing:{
				if (it.MoveNext()){
					fracResults.Add (it.Current);
				}
				else{
					FinaliseResults();
					PrintResults();
					SavePerms();
					state = State.kIsReady;
				}
				break;
			}
			case State.kStop:{
				fracResults.Clear();
				state = State.kOff;
				break;	
			}
		}
		
	}
	
	
	public void Generate(){
	
		foreach (AVOWCircuitTarget item in GenerateIterate()){
			fracResults.Add(item);
			PrintResult(item);
		}
		FinaliseResults();
		PrintResults();
	}
	
	void FinaliseResults(){
		fracResults.Sort ((obj1, obj2) => Compare(obj1, obj2));
		
		// Remove duplicates
		List<AVOWCircuitTarget> newList = new  List<AVOWCircuitTarget>();
		
		// For some reason we can't test if these tuples are null
		AVOWCircuitTarget lastObj = null;

		for (int i = 0; i < fracResults.Count; ++i){
			AVOWCircuitTarget obj = fracResults[i];
			if (!IsSameWidth(lastObj, obj)){
				newList.Add (obj);
				lastObj = obj;
			}			
		}
		

		// Finally make the results point to the new list
		fracResults = newList;
		
		
	}
	
	int CalcDenominator(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return denominator;
	}
	
//	void PostProcess(){
//		// find lowest common multiplier
//		currentLCM = CalcDenominator(fracResults[0].Item1);
//		foreach (AVOWCircuitTarget obj in fracResults){
//			currentLCM = MathUtils.FP.lcm(CalcDenominator(obj.Item1), currentLCM);
//			foreach (float val in obj.Item2){
//				currentLCM = MathUtils.FP.lcm(CalcDenominator(val), currentLCM);
//			}
//		}
//		Debug.Log ("Lowest Common Multiplier = " + currentLCM);
//		
//		lCMResults = new List<AVOWCircuitTarget>();
//		
//		// Scale the results by this so everything is an integer
//		foreach (AVOWCircuitTarget obj in fracResults){
//			AVOWCircuitTarget newItem = new AVOWCircuitTarget(obj.Item1 * currentLCM, new List<float>());
//			foreach (float val in obj.Item2){
//				newItem.Item2.Add(val * currentLCM);
//			}
//			lCMResults.Add (newItem);
//		}
//	}
	
	int Compare(AVOWCircuitTarget obj1, AVOWCircuitTarget obj2){
		if (!MathUtils.FP.Feq (obj1.totalCurrent, obj2.totalCurrent)) return obj1.totalCurrent.CompareTo(obj2.totalCurrent);
		
		int len = Mathf.Min (obj1.componentDesc.Count, obj2.componentDesc.Count);
		for (int i = 0; i < len; ++i){
			if (!MathUtils.FP.Feq (obj1.componentDesc[i][0], obj2.componentDesc[i][0])) return obj1.componentDesc[i][0].CompareTo(obj2.componentDesc[i][0]);
		}
		
		if (obj1.componentDesc.Count > len) return 1;
		if (obj2.componentDesc.Count > len) return -1;
		
		return 0;
	}
	
	bool IsSameWidth(AVOWCircuitTarget obj1, AVOWCircuitTarget obj2){
		if (obj1 == null && obj2  != null) return false;
		if (obj1 != null && obj2  == null) return false;
		if (obj1 == null && obj2  == null) return true;
		
		if (!MathUtils.FP.Feq (obj1.totalCurrent, obj2.totalCurrent)) return false;
		
		if (obj1.componentDesc.Count != obj2.componentDesc.Count) return false;
		
		for (int i = 0; i < obj1.componentDesc.Count; ++i){
			float val1 = obj1.componentDesc[i][0];
			float val2 = obj2.componentDesc[i][0];
			if (!MathUtils.FP.Feq (val1, val2)) return false;
		}
		return true;
	}
	

	
	public IEnumerable<AVOWCircuitTarget> GenerateIterate(){
	
		for (int i = 0; i < maxNumResistors; ++i){
			foreach (var x in GenerateIterate (i+1)){
				yield return x;
			}
		}
		yield break;
	}
	
	
	
	IEnumerable<AVOWCircuitTarget> GenerateIterate(int numResistors){
		MakeBasic();
		
		foreach (var x in EnumerateAllOptions(numResistors)){
			yield return x;
		}
		
		ClearCircuit();
	}
	
	void PrintResults(){
		for (int i = 0; i < fracResults.Count; ++i){
			PrintResult (fracResults[i]);
		}
	}
	
	void PrintFracResults(){
		for (int i = 0; i < fracResults.Count; ++i){
			PrintFracResult (fracResults[i]);
		}
	}
	
	void PrintResult(AVOWCircuitTarget result){
		string text = result.totalCurrent.ToString() + ": ";
		List<Vector3> vals = result.componentDesc;
		for (int j = 0; j < vals.Count; ++j){
			text += vals[j].ToString();
			if (j != vals.Count-1){
				text += ", ";
			}
		}
		//Debug.Log(text);
	}
	
	string CreateFracString(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return "(" + (isNeg ? "-" : "") + integer.ToString() + " " + numerator.ToString() + "/" + denominator.ToString() + ")";
	}
	
	void PrintFracResult(AVOWCircuitTarget result){
		string text = CreateFracString(result.totalCurrent) + ": ";
		List<Vector3> vals = result.componentDesc;
		for (int j = 0; j < vals.Count; ++j){
			text += CreateFracString(vals[j][0]);
			if (j != vals.Count-1){
				text += ", ";
			}
		}
		//Debug.Log(text);
	}
	
	IEnumerable<AVOWCircuitTarget> EnumerateAllOptions(int numResistors){
		int numOptions = CalcNumOptions();

		// This is bodgey but need this incase we already had enoufh resistors
		if (graph.allComponents.Count == numResistors+ 1){
			AVOWSim.singleton.Recalc();
			AVOWCircuitTarget item = new AVOWCircuitTarget(graph);
			//Debug.Log (count.ToString () + ".......Total current = " + graph.GetTotalWidth());
			count++;
			yield return item;
		}
		else{
			for (int i = 0; i < numOptions; ++i){
				PerformNthOption(i);
				if (graph.allComponents.Count == numResistors+ 1){
					AVOWSim.singleton.Recalc();
					
					AVOWCircuitTarget item = new AVOWCircuitTarget(graph);
					//Debug.Log (count.ToString () + ".......Total current = " + graph.GetTotalWidth());
					count++;
					if (!AVOWSim.singleton.errorInBounds){
						yield return item;
					}
					
	//				result.Add(item);
				}
				else{
					foreach (var x in EnumerateAllOptions(numResistors))
					{
						yield return x;
					}
				}
				UndoLastCommand();
			}
		}
		
	}

	

	void MakeBasic(){
		
		// Simple start
		GameObject node0GO = graph.AddNode ();
		GameObject node1GO = graph.AddNode ();
		
		
		graph.PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.cellPrefab) as GameObject, node0GO, node1GO);
		graph.PlaceComponent(GameObject.Instantiate(AVOWUI.singleton.resistorPrefab) as GameObject, node1GO, node0GO);
		++numUsed;

	}
	
	void FillEnumatatedList(int n, out int[] first, out int[] second){
		int numCombinations = NChooseK(n, 2);
		first = new int[numCombinations];
		second = new int[numCombinations];
		
		int count = 0;
		for (int i = 0; i < n-1; ++i){
			for (int j = i+1; j < n; ++j){
				first[count] = i;
				second[count] = j;
				count++;
			}
		}
	}
	
	void GetEnumerations(int n, out int[] first, out int[] second){
		if (enumerationCache.ContainsKey(n)){
			
			Eppy.Tuple<int[], int[]> lists = enumerationCache[n];
			first = lists.Item1;
			second = lists.Item2;
		}
		else{
			FillEnumatatedList(n, out first, out second);
			enumerationCache.Add(n, new Eppy.Tuple<int[], int[]> (first, second));
		}
		
		
	}
	
	void ClearCircuit(){
	
		foreach (GameObject go in graph.allComponents){
			GameObject.Destroy(go);
		}
		
		foreach (GameObject go in graph.allNodes){
			GameObject.Destroy(go);
		}
		graph.allComponents.Clear();
		graph.allNodes.Clear();
		
	}
	
	int Factorial(int n){
		int result = 1;
		for (int i = 1; i <= n; ++i){
			result *= i;
		}
		return result;
		
	}
	
	int NChooseK(int n, int k){
		return Factorial(n) / (Factorial (k) * Factorial (n-k));
	}
	
	// Ths is the number of ways we can add a component between two existing nodes
	int CalcNumNodeNodeOptions(){
		return NChooseK(graph.allNodes.Count, 2);
	}

	// This is the number of ways we can add a component between a node and a component which is attached to it
	int CalcNumNodeComponentOptions(){
		splitNodeCache.Clear();
		int total = 0;
		int count = 0;
		for (int i = 0; i < graph.allNodes.Count; ++i){
			
			AVOWNode node = graph.allNodes[i].GetComponent<AVOWNode>();
			int compCount = node.components.Count;
			total += compCount;
			for (int j = 0; j < compCount; ++j){
				splitNodeCache.Add(count++, new Eppy.Tuple<int, int>(i,j));
			}
			
		}
		return total;
	}
	
	int CalcNumOptions(){
		return CalcNumNodeNodeOptions() + CalcNumNodeComponentOptions();
	}
	
	void PerformNthOption(int i){

		int totalNodeNode = CalcNumNodeNodeOptions();
		
		AVOWCommand command = null;
		if (i < totalNodeNode){
			int[] first = null;
			int[] second = null;
			GetEnumerations( graph.allNodes.Count, out first, out second);
			
		//	Debug.Log ("i = " + i + " first.Length = " + first.Length + ", second.Length = " + second.Length + ", graph.allNodes.Count = " + graph.allNodes.Count);
		//	Debug.Log ("first[i] = " + first[i] + ", second[i] = " + second[i]);
			command = new AVOWCommandAddComponent(graph.allNodes[first[i]], graph.allNodes[second[i]], AVOWUI.singleton.resistorPrefab);
		}
		else{
			int index = i - totalNodeNode;
			AVOWNode node = graph.allNodes[splitNodeCache[index].Item1].GetComponent<AVOWNode>();
			//Debug.Log ("index = " + index);
			command = new AVOWCommandSplitAddComponent(graph.allNodes[splitNodeCache[index].Item1], node.components[splitNodeCache[index].Item2], AVOWUI.singleton.resistorPrefab, false);
		
		}
		commands.Push(command);
		
		// Perform the command in steps until it is finished
		while (!command.ExecuteStep()){}
		
		graph.ForceComponentsToSize();
		
	}
	
	void UndoLastCommand(){
		AVOWCommand command = commands.Pop ();
		command.UndoStep();
		graph.ForceComponentsToSize();
		
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
