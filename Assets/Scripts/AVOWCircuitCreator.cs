using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AVOWCircuitCreator : MonoBehaviour {
	public static AVOWCircuitCreator singleton = null;
	
	public string 		filename = "LevelPerms";
	
	public int currentLCM = 1;
	
	public enum State{
		kStart,
		kProcessing,
		kFinished
	}
	public 	List< Eppy.Tuple<float, List<float>> > results = new List< Eppy.Tuple<float, List<float>> > ();
	
	int count = 0;
	
	 State state = State.kStart;
	IEnumerator<Eppy.Tuple<float, List<float>>> it = null;
	
	int maxNumResistors = 5;
	int numUsed = 0;
	AVOWGraph graph;
	
	Dictionary<int, Eppy.Tuple<int[], int[]>> enumerationCache = new Dictionary<int, Eppy.Tuple<int[], int[]>>();
	
	Dictionary<int, Eppy.Tuple<int, int>> splitNodeCache = new Dictionary<int, Eppy.Tuple<int, int>>();
	
	Stack<AVOWCommand>	commands = new Stack<AVOWCommand>();
	
	
	
	public bool IsFinished(){
		return state == State.kFinished;
	}
	
	public string CreateFilename(){
		return filename + "_" + maxNumResistors.ToString();
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
		
		bw.Write (results.Count);
		for (int i = 0; i < results.Count; ++i){
			bw.Write (results[i].Item1);
			bw.Write (results[i].Item2.Count);
			for (int j = 0; j < results[i].Item2.Count; ++j){
				bw.Write (results[i].Item2[j]);
			}
		}
		
	}
	
	public void DeserializePerms(Stream stream){
		BinaryReader br = new BinaryReader(stream);
		
		int resultsCount = br.ReadInt32();
		results = new List< Eppy.Tuple<float, List<float>> > ();
		for (int i = 0; i < resultsCount; ++i){
			float val = br.ReadSingle();
			Eppy.Tuple<float, List<float>> item = new Eppy.Tuple<float, List<float>>(val, new List<float>());
			int listCount = br.ReadInt32();
			for (int j = 0; j < listCount; ++j){
				float newVal = br.ReadSingle();
				item.Item2.Add (newVal);
			}
			results.Add (item);
		}
		
		
	}
	
	
	
	public bool LoadPerms(){

		TextAsset asset = Resources.Load(BuildResourcePath ()) as TextAsset;
		if (asset != null){
			Debug.Log ("Loading asset");
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
	void Update () {
		switch (state){
			case State.kStart:{
				if (LoadPerms()){
					PrintResults();
					PrintFracResults();
					PostProcess();
					PrintResults();
					
					state = State.kFinished;
				}
				else{
					it = GenerateIterate().GetEnumerator();
					state = State.kProcessing;
				}
				break;
			}
			case State.kProcessing:{
				if (it.MoveNext()){
					results.Add (it.Current);
					}
				else{
					FinaliseResults();
					PrintResults();
					SavePerms();
					state = State.kFinished;
				}
				break;
			}
		}
		
	}
	
	
	public void Generate(){
	
		foreach (Eppy.Tuple<float, List<float>> item in GenerateIterate()){
			results.Add(item);
			PrintResult(item);
		}
		FinaliseResults();
		PrintResults();
	}
	
	void FinaliseResults(){
		results.Sort ((obj1, obj2) => Compare(obj1, obj2));
		
		// Remove duplicates
		List<Eppy.Tuple<float, List<float>>> newList = new  List<Eppy.Tuple<float, List<float>>>();
		
		// For some reason we can't test if these tuples are null
		Eppy.Tuple<float, List<float>> lastObj = new Eppy.Tuple<float, List<float>>(-1, null);

				for (int i = 0; i < results.Count; ++i){
			Eppy.Tuple<float, List<float>> obj = results[i];
			if (!IsSame(lastObj, obj)){
				newList.Add (obj);
				lastObj = obj;
			}			
		}
		

		// Finally make the results point to the new list
		results = newList;
		
		
	}
	
	int CalcDenominator(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return denominator;
	}
	
	void PostProcess(){
		// don;t do this
		return;
		// find lowest common multiplier
		currentLCM = CalcDenominator(results[0].Item1);
		foreach (Eppy.Tuple<float, List<float>> obj in results){
			currentLCM = MathUtils.FP.lcm(CalcDenominator(obj.Item1), currentLCM);
			foreach (float val in obj.Item2){
				currentLCM = MathUtils.FP.lcm(CalcDenominator(val), currentLCM);
			}
		}
		Debug.Log ("Lowest Common Multiplier = " + currentLCM);
		
		List<Eppy.Tuple<float, List<float>>> newList = new List<Eppy.Tuple<float, List<float>>>();
		
		// Scale the results by this so everything is an integer
		foreach (Eppy.Tuple<float, List<float>> obj in results){
			Eppy.Tuple<float, List<float>> newItem = new Eppy.Tuple<float, List<float>>(obj.Item1 * currentLCM, new List<float>());
			foreach (float val in obj.Item2){
				newItem.Item2.Add(val * currentLCM);
			}
			newList.Add (newItem);
		}
		
		results = newList;
		
	}
	
	int Compare(Eppy.Tuple<float, List<float>> obj1, Eppy.Tuple<float, List<float>> obj2){
		if (!MathUtils.FP.Feq (obj1.Item1, obj2.Item1)) return obj1.Item1.CompareTo(obj2.Item1);
		
		int len = Mathf.Min (obj1.Item2.Count, obj2.Item2.Count);
		for (int i = 0; i < len; ++i){
			if (!MathUtils.FP.Feq (obj1.Item2[i], obj2.Item2[i])) return obj1.Item2[i].CompareTo(obj2.Item2[i]);
		}
		
		if (obj1.Item2.Count > len) return 1;
		if (obj2.Item2.Count > len) return -1;
		
		return 0;
	}
	
	bool IsSame(Eppy.Tuple<float, List<float>> obj1, Eppy.Tuple<float, List<float>> obj2){
		if (obj1.Item2 == null && obj2.Item2  != null) return false;
		if (obj1.Item2  != null && obj2.Item2  == null) return false;
		
		if (!MathUtils.FP.Feq (obj1.Item1, obj2.Item1)) return false;
		
		if (obj1.Item2.Count != obj2.Item2.Count) return false;
		
		for (int i = 0; i < obj1.Item2.Count; ++i){
			float val1 = obj1.Item2[i];
			float val2 = obj2.Item2[i];
			if (!MathUtils.FP.Feq (val1, val2)) return false;
		}
		return true;
	}
	

	
	public IEnumerable<Eppy.Tuple<float, List<float>>> GenerateIterate(){
	
		for (int i = 0; i < maxNumResistors; ++i){
			foreach (var x in GenerateIterate (i+1)){
				yield return x;
			}
		}
		yield break;
	}
	
	
	
	IEnumerable<Eppy.Tuple<float, List<float>>> GenerateIterate(int numResistors){
		MakeBasic();
		
		foreach (var x in EnumerateAllOptions(numResistors)){
			yield return x;
		}
		
		ClearCircuit();
	}
	
	void PrintResults(){
		for (int i = 0; i < results.Count; ++i){
			PrintResult (results[i]);
		}
	}
	
	void PrintFracResults(){
		for (int i = 0; i < results.Count; ++i){
			PrintFracResult (results[i]);
		}
	}
	
	void PrintResult(Eppy.Tuple<float, List<float>> result){
		string text = result.Item1.ToString() + ": ";
		List<float> vals = result.Item2;
		for (int j = 0; j < vals.Count; ++j){
			text += vals[j].ToString();
			if (j != vals.Count-1){
				text += ", ";
			}
		}
		Debug.Log(text);
	}
	
	string CreateFracString(float val){
		int denominator;
		int numerator;
		int integer;
		bool isNeg;
		MathUtils.FP.CalcFraction(val, out integer, out numerator, out denominator, out isNeg);
		return "(" + (isNeg ? "-" : "") + integer.ToString() + " " + numerator.ToString() + "/" + denominator.ToString() + ")";
	}
	
	void PrintFracResult(Eppy.Tuple<float, List<float>> result){
		string text = CreateFracString(result.Item1) + ": ";
		List<float> vals = result.Item2;
		for (int j = 0; j < vals.Count; ++j){
			text += CreateFracString(vals[j]);
			if (j != vals.Count-1){
				text += ", ";
			}
		}
		Debug.Log(text);
	}
	
	IEnumerable<Eppy.Tuple<float, List<float>>> EnumerateAllOptions(int numResistors){
		int numOptions = CalcNumOptions();

		// This is bodgey but need this incase we already had enoufh resistors
		if (graph.allComponents.Count == numResistors+ 1){
			AVOWSim.singleton.Recalc();
			
			Eppy.Tuple<float, List<float>> item = new Eppy.Tuple<float, List<float>>(AVOWSim.singleton.xMax, new List<float>() );
			foreach (GameObject go in graph.allComponents){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				if (component.type == AVOWComponent.Type.kVoltageSource) continue;
				if (MathUtils.FP.Feq (component.hWidth, 0)) continue;
				item.Item2.Add(component.hWidth);
			}
			item.Item2.Sort((obj1, obj2) => obj1.CompareTo(obj2));
			Debug.Log (count.ToString () + ".......Total current = " + AVOWSim.singleton.xMax);
			count++;
			yield return item;
		}
		else{
			for (int i = 0; i < numOptions; ++i){
				PerformNthOption(i);
				if (graph.allComponents.Count == numResistors+ 1){
					AVOWSim.singleton.Recalc();
					
					Eppy.Tuple<float, List<float>> item = new Eppy.Tuple<float, List<float>>(AVOWSim.singleton.xMax, new List<float>() );
					foreach (GameObject go in graph.allComponents){
						AVOWComponent component = go.GetComponent<AVOWComponent>();
						if (component.type == AVOWComponent.Type.kVoltageSource) continue;
						item.Item2.Add(component.hWidth);
					}
					item.Item2.Sort((obj1, obj2) => obj1.CompareTo(obj2));
					Debug.Log (count.ToString() + ".......Total current = " + AVOWSim.singleton.xMax);
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
			
			Debug.Log ("i = " + i + " first.Length = " + first.Length + ", second.Length = " + second.Length + ", graph.allNodes.Count = " + graph.allNodes.Count);
			Debug.Log ("first[i] = " + first[i] + ", second[i] = " + second[i]);
			command = new AVOWCommandAddComponent(graph.allNodes[first[i]], graph.allNodes[second[i]], AVOWUI.singleton.resistorPrefab);
		}
		else{
			int index = i - totalNodeNode;
			AVOWNode node = graph.allNodes[splitNodeCache[index].Item1].GetComponent<AVOWNode>();
			Debug.Log ("index = " + index);
			command = new AVOWCommandSplitAddComponent(graph.allNodes[splitNodeCache[index].Item1], node.components[splitNodeCache[index].Item2], AVOWUI.singleton.resistorPrefab);
		
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
