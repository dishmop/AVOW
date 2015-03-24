using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Text;

public class AVOWCircuitSubsetter : MonoBehaviour {
	public static AVOWCircuitSubsetter singleton = null;

	// Global list of all subsets of our circuits - key is the subset, the value is the circuit it came from
	Dictionary<AVOWCircuitTarget, List<AVOWCircuitTarget>> subsets = new Dictionary<AVOWCircuitTarget, List<AVOWCircuitTarget>>();
	
	List<AVOWCircuitTarget>[]		organisedTargets;
	
	int numResistors = -1;
	
	
	FileStream	fileStream = null;

	// Use this for initialization
	void Start () {
		organisedTargets = new List<AVOWCircuitTarget>[6];
		for (int i = 0; i < organisedTargets.Length; ++i){
			organisedTargets[i] = new List<AVOWCircuitTarget>();
		}
		CalcSubsets();
		SetupOrganisedLists();
		WriteAllToLogFile();
		WriteExtremeToLogFile();
		WriteOrganisedToLogFile();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// Just to give it same interface as the circuit creator
	public void Initialise(int numResistors){
		this.numResistors = numResistors;
	}
	
	public List< AVOWCircuitTarget > GetResults(){
		return organisedTargets[numResistors];
		
	}
	
	
	void SetupOrganisedLists(){
		foreach (KeyValuePair<AVOWCircuitTarget, List<AVOWCircuitTarget>> item in subsets){

			int minResistors = 10;
			float minResistorsCurrent = 10f;
			
			AVOWCircuitTarget minResistorsTarget = null;
			foreach (AVOWCircuitTarget target in item.Value){
				
				bool hasNewMinResistors = false;
				// If we have the same number of resistors - pick the one with the minimum current
				if (target.componentDesc.Count == minResistors){
					if (target.totalCurrent < minResistorsCurrent){
						hasNewMinResistors = true;
					}
				}
				else if (target.componentDesc.Count < minResistors){
					hasNewMinResistors = true;
				}
				if (hasNewMinResistors){
					minResistors = target.componentDesc.Count;
					minResistorsTarget = target;
					minResistorsCurrent = target.totalCurrent;
				}
				
			}
			if (item.Key.hiddenComponents.Count > 0){
				AVOWCircuitTarget newTarget = CreateTargetFromSubset(item.Key, minResistorsTarget);
				organisedTargets[minResistors].Add (newTarget);
			}
		}
		
		// Sort all the lists and if there are ones with the same "solution" then
		// just keep the one with the least unhiddemn components
		for (int i = 0; i < organisedTargets.Length; ++i){
			organisedTargets[i].Sort ((obj1, obj2) => obj1.totalCurrent.CompareTo(obj2.totalCurrent));
			for (int j = 0; j < organisedTargets[i].Count; ){
				// For each element in the list, check if we have a smaller sibling somwhere and if we do
				// remove this one
				bool hasSmallerSibling = false;
				foreach (AVOWCircuitTarget testTarget in organisedTargets[i]){
					if (IsSmallerSiblingOf(testTarget, organisedTargets[i][j])){
						hasSmallerSibling = true;
						break;
					}
				}
				if (hasSmallerSibling){
					organisedTargets[i].RemoveAt(j);
				}
				else{
					++j;
				}
			
			}
		}
		
	}
	
	// Returns true if both targets have same Full target (after being unhidden all) and that the smaller one
	// has less (or the same) unhidden components - but also return false if they are actually identical
	bool IsSmallerSiblingOf(AVOWCircuitTarget smallTest, AVOWCircuitTarget bigTest){
		// if the full forms don't have the same number of components, then they are not of the same full form
		if (smallTest.componentDesc.Count + smallTest.hiddenComponents.Count != bigTest.componentDesc.Count + bigTest.hiddenComponents.Count){
			return false;
		}
		
		// If not the same total current then not the sme full verison
		if (!MathUtils.FP.Feq (smallTest.totalCurrent, bigTest.totalCurrent)){
			return false;
		}

		// If the little one doesn't have less non hidden components than the superset then it is not smaller
		if (smallTest.componentDesc.Count > bigTest.componentDesc.Count){
			return false;
		}
		
		AVOWCircuitTarget smallFull = new AVOWCircuitTarget(smallTest);
		smallFull.UnhideAllComponents();
		AVOWCircuitTarget bigFull = new AVOWCircuitTarget(bigTest);
		bigFull.UnhideAllComponents();
		if (!smallFull.Equals(bigFull)){
			return false;
		}
		// but if they are actually identical, then also return false (you are no tyour own sibling)
		if (smallTest.Equals(bigTest)){
			return false;
		}
		return true;
		
		
	}
	
	// Creates a new subset style target where the hidden members (if unhidden) would turn it into FullTarget
	// We assume that subset is indeed a subset of fullTarget
	AVOWCircuitTarget CreateTargetFromSubset(AVOWCircuitTarget subset, AVOWCircuitTarget fullTarget){
	
		// Make a list of all the components in the full target
		List<Vector3> fullComponentList = new List<Vector3>();
		foreach (Vector3 vals in fullTarget.componentDesc){
			fullComponentList.Add (vals);
		}
		
		// For each component int he subset, remove a matching one from our full list
		foreach (Vector3 vals in subset.componentDesc){
			int removalIndex = -1;
			for (int i = 0; i < fullComponentList.Count; ++i){
				if (MathUtils.FP.Feq (vals[0], fullComponentList[i][0])){
					removalIndex = i;
					break;
				}
			}
			if (removalIndex == -1){
				Debug.LogError("Being asked to create a target from a subset which is not a subset");
			}
			fullComponentList.RemoveAt(removalIndex);
		}
		
		//Now everything left over in the full list must be hidden in the subset
		AVOWCircuitTarget newTarget = new AVOWCircuitTarget(subset);
		newTarget.totalCurrent = fullTarget.totalCurrent;
		newTarget.hiddenComponents.Clear();
		foreach (Vector3 vals in fullComponentList){
			newTarget.hiddenComponents.Add (vals);
		}
		return newTarget;
	}
	
	void WriteAllToLogFile(){
		// If the directory doesn't exist, make it exist
		string pathname = Application.persistentDataPath + "/AVOWLogs/";
		if (!Directory.Exists(pathname)){
			Directory.CreateDirectory(pathname);
		}
		
		string filename = pathname + "/log.txt";
		fileStream = File.Create(filename);
		BinaryWriter bw = new BinaryWriter(fileStream);
	

		StringBuilder builder = new StringBuilder();
		builder.AppendLine("DebugPrintSubsets");
		int i = 0;
		foreach (KeyValuePair<AVOWCircuitTarget, List<AVOWCircuitTarget>> item in subsets){
			//bw.Write(i + " " + item.Key.MakeDebugString() +"\n");
			builder.AppendLine("\n" + i + " " + item.Key.MakeDebugString());
			foreach (AVOWCircuitTarget target in item.Value){
			//	bw.Write(" - " + target.MakeDebugString() +"\n");
				builder.AppendLine(" - " + target.MakeDebugString());
			}
			++i;
			
		}
		bw.Write (builder.ToString ());
		
		fileStream.Close();
		
		
	}
	
	
	void WriteOrganisedToLogFile(){
		// If the directory doesn't exist, make it exist
		string pathname = Application.persistentDataPath + "/AVOWLogs/";
		if (!Directory.Exists(pathname)){
			Directory.CreateDirectory(pathname);
		}
		
		string filename = pathname + "/logOrganised.txt";
		fileStream = File.Create(filename);
		BinaryWriter bw = new BinaryWriter(fileStream);
		
		
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("DebugPrintOrganisedTargets");
		for (int i = 0; i < organisedTargets.Length; ++i){
			builder.AppendLine("\nNum Resistors = " + i.ToString());
			foreach (AVOWCircuitTarget target in organisedTargets[i]){
				builder.AppendLine("    " + target.MakeDebugString());
			}
			
		}
		bw.Write (builder.ToString ());
		
		fileStream.Close();
		
		
	}
	
	
	void WriteExtremeToLogFile(){
		// If the directory doesn't exist, make it exist
		string pathname = Application.persistentDataPath + "/AVOWLogs/";
		if (!Directory.Exists(pathname)){
			Directory.CreateDirectory(pathname);
		}
		
		string filename = pathname + "/logExtremes.txt";
		fileStream = File.Create(filename);
		BinaryWriter bw = new BinaryWriter(fileStream);
		
		
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("DebugPrintSubsets");
		int i = 0;
		foreach (KeyValuePair<AVOWCircuitTarget, List<AVOWCircuitTarget>> item in subsets){
			//bw.Write(i + " " + item.Key.MakeDebugString() +"\n");
			builder.AppendLine("\n" + i + " " + item.Key.MakeDebugString());
			int minResistors = 10;
			float minResistorsCurrent = 10f;
			
			float minCurrent = 10f;
			int minCurrentNumResistors = 10;
			
			AVOWCircuitTarget minResistorsTarget = null;
			AVOWCircuitTarget minCurrentTarget = null;
			foreach (AVOWCircuitTarget target in item.Value){
			
				bool hasNewMinResistors = false;
				// If we have the same number of resistors - pick the one with the minimum current
				if (target.componentDesc.Count == minResistors){
					if (target.totalCurrent < minResistorsCurrent){
						hasNewMinResistors = true;
					}
				}
				else if (target.componentDesc.Count < minResistors){
					hasNewMinResistors = true;
				}
				if (hasNewMinResistors){
					minResistors = target.componentDesc.Count;
					minResistorsTarget = target;
					minResistorsCurrent = target.totalCurrent;
				}
				
				
				// If it is the same current then pick the one with the lowst num of resisors
				bool hasNewMinCurrent = false;
				if (MathUtils.FP.Feq (target.totalCurrent, minCurrent)){
					if (target.componentDesc.Count < minCurrentNumResistors){
						hasNewMinCurrent = true;						
					}
					
				} 
				else if (target.totalCurrent < minCurrent){
					hasNewMinCurrent = true;
				}
				if (hasNewMinCurrent){
					minCurrent = target.totalCurrent;
					minCurrentNumResistors = target.componentDesc.Count;
					minCurrentTarget = target;
					
				}
			}
			//	bw.Write(" - " + target.MakeDebugString() +"\n");
			builder.AppendLine(" - minResistorTarget = " + minResistorsTarget.MakeDebugString());
			builder.AppendLine(" - minCurrentTarget  = " + minCurrentTarget.MakeDebugString());
			++i;
			
		}
		bw.Write (builder.ToString ());
		
		fileStream.Close();
		
		
	}
	
	
	public void CalcSubsets(){
		for (int i = 5; i < 6; ++i){
			AVOWCircuitCreator.singleton.Initialise(i);
			bool isReady = false;
			while (!isReady){
				AVOWCircuitCreator.singleton.Update();
				isReady = AVOWCircuitCreator.singleton.IsReady();
			}
			foreach (AVOWCircuitTarget target in AVOWCircuitCreator.singleton.GetResults()){
				AddSubsets(target, target);
			}
		}
		
		

		
		// Sort each of the lists and remove duplicates
		foreach (KeyValuePair<AVOWCircuitTarget, List<AVOWCircuitTarget>> pair in subsets){
			List<AVOWCircuitTarget> noDupesList =  pair.Value.Distinct().ToList();
			pair.Value.Clear();
			foreach (AVOWCircuitTarget target in noDupesList){
				pair.Value.Add(target);
			}
			
		}

	}
	
	void AddSubsets(AVOWCircuitTarget key, AVOWCircuitTarget value){
//		// if the key has the same about as the value then it is not a subset in the way we are interested in
//		if (key.componentDesc.Count != value.componentDesc.Count){
			if (!subsets.ContainsKey(key)){
				subsets.Add(new AVOWCircuitTarget(key), new List<AVOWCircuitTarget>());
			}
			subsets[key].Add (new AVOWCircuitTarget(value));
//		}
		
		// Go through the key and remove one element and add the subsets of that
		// Though don't remove an element if we have removed one of the same value already
		AVOWCircuitTarget newKey;
		float lastSize = -1f;
		for (int i = 0; i < key.componentDesc.Count; ++i){
			Vector3 vals = key.componentDesc[i];
			if (!MathUtils.FP.Feq(vals[0], lastSize)){
				lastSize = vals[0];
				newKey = new AVOWCircuitTarget(key);
				newKey.HideComponent(i);
				if (newKey.componentDesc.Count > 0){
					AddSubsets(newKey, value);
				}
			}
			
		}
	
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
//	void DebugPrintSubsets(){
//		Debug.Log ("DebugPrintSubsets");
//		int i = 0;
//		foreach (KeyValuePair<AVOWCircuitTarget, List<AVOWCircuitTarget>> item in subsets){
//			Debug.Log (i + " " + item.Key.MakeDebugString());
//			foreach (AVOWCircuitTarget target in item.Value){
//				Debug.Log (" - " + target.MakeDebugString());
//			}
//			++i;
//			
//		}
//	}
}
