using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class AVOWSim : MonoBehaviour {
	public static AVOWSim singleton = null;
	
	public GameObject graphGO;
	
	AVOWGraph graph;
	
	public class LoopElement{
		public LoopElement(AVOWComponent component, AVOWNode fromNode){
			this.component = component;
			this.fromNode = fromNode;
		}
			
		public AVOWComponent component;
		public AVOWNode fromNode;
	}
	
	public List<List<LoopElement>> loops;
	
	// Global bounds
	public float xMin;
	public float yMin;
	public float xMax;
	public float yMax;
	
	
	// Recording the mouse posiiton
	Vector3			mouseWorldPos;
	AVOWComponent 	mouseOverComponent = null; 	// null if not over any compnent
	float			mouseOverXProp = 0.5f;		// Proportion of width from left of component (or entire diagram if null)
	float			mouseOverYProp = 0.5f;		// Proportio of height from bottom of component (or entire diagram if null)
	
	// Current solving	
	double epsilon = 0.0001;
	float[]						loopCurrents;
	/*
	class EmergencyOption{
		public EmergencyOption(AVOWComponent component, AVOWComponent.FlowDirection dir, int ordinalValue){
			this.component = component;
			this.dir = dir;
			this.ordinalValue = ordinalValue;
		}
		
		public AVOWComponent 				component;
		public AVOWComponent.FlowDirection 	dir;
		public int 							ordinalValue;
	};
	
	List<EmergencyOption>	emergencyOptions = new List<EmergencyOption>();
	*/
	// List of permutations matricies
	int[][,] cachedPermutations = new int[AVOWComponent.kOrdinalUnordered][,];
	
	
	static readonly int kOut = 0;
	static readonly int kIn = 1;
	static readonly int kNumDirs = 2;
	
	static int ReverseDirection(int dir){ return 1-dir;}
	
	class SimNode{
		public int id = -1;
		public List<SimBlock>[] blockList = new List<SimBlock>[kNumDirs];
		public SimBlock[][] blockArray = new SimBlock[kNumDirs][];
		public SimBlock[][] sortedBlockArray = new SimBlock[kNumDirs][];
		public float h0 = -1;
		public AVOWNode originalNode;
		public float hWidth;


	};
	
	class SimBlock{
		public int id = -1;
		public int[] ordinals = new int[kNumDirs];
		public SimNode[] nodes = new SimNode[kNumDirs];
		public float h0 = -1;
		public float hWidth = -1;
		public int hOrder = -1;
		public List<AVOWComponent> components = new List<AVOWComponent>();
	};
	
	SimNode[] allSimNodes;
	SimBlock[] allSimBlocks;
	
	int[,]		permutations;
	List<int> validPermutations = new List<int>();
	
	
	
	public void Recalc(){

		FindLoops();
		//DebugPrintLoops();
		
		RecordLoopsInComponents();
		SolveForCurrents();
		//DebugPrintLoopCurrents();
		
		StoreCurrentsInComponents();
		//DebugPrintComponentCurrents();
		
		CalcVoltages();
		//DebugPrintVoltages();
		
		//DebugPrintGraph();
		LayoutHOrder();
		
		//DebugPrintHOrder();
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	void FixedUpdate(){

		RecordMousePos();
		Recalc();
		CalcMouseOffset();
		
		//AppHelper.Quit();
	}
	
	
	void DebugPrintGraph(){
		Debug.Log ("Print graph");
		Debug.Log ("Nodes");
		foreach (GameObject nodeGO in graph.allNodes){
			Debug.Log("Node " + nodeGO.GetComponent<AVOWNode>().GetID());			
		}
		Debug.Log ("Components");
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			AVOWNode node0 = component.node0GO.GetComponent<AVOWNode>();
			AVOWNode node1 = component.node1GO.GetComponent<AVOWNode>();
			if (component.GetCurrent(component.node0GO) > 0){
				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node0.GetID() + " to " + node1.GetID());
			}
			else{
				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node1.GetID() + " to " + node0.GetID());
			}
		}
	}
	
	void DebugPrintLoops(){
		Debug.Log ("Printing loops");
		for (int i = 0; i < loops.Count; ++i){
			AVOWNode lastNode = loops[i][0].fromNode;
			string loopString = lastNode.GetID ();
			
			for (int j = 0; j < loops[i].Count; ++j){
				AVOWNode nextNode = loops[i][j].component.GetOtherNode(lastNode.gameObject).GetComponent<AVOWNode>();
				// print the connection and the final node
				loopString += "=" + loops[i][j].component.GetID () + "=>" + nextNode.GetID ();
				lastNode = nextNode;
			}
			Debug.Log ("Loop(" + i.ToString() + "): " + loopString );
			
		}
	}
	
	void DebugPrintLoopCurrents(){
		Debug.Log("Printing loop currents");
		for (int i = 0; i < loopCurrents.Length; ++i){
			Debug.Log ("Loop " + i + ": " + loopCurrents[i]);
			
		}
	}
	
	void Start(){
		graph = graphGO.GetComponent<AVOWGraph>();
		
	}
	
	
	// Fins a set of indendpent loops in the graph
	void FindLoops(){
		loops = new List<List<LoopElement>>();

		// If no nodes, then nothing to do
		if (graph.allNodes.Count == 0) return;

		// Get any node which is going to be our starting point for all traversals		
		AVOWNode startNode = graph.allNodes[0].GetComponent<AVOWNode>();
		
		
		// We have no components disabled then as we find loops, we disable one component at a time
		// until we can't find any more loops
		graph.ClearDisabledFlags();
		
		// We finish this loop when there are no loops left
		bool finished = false;
		while (!finished){
			graph.ClearVisitedFlags();
			
			// Create a stack of nodes which we use to traverse the graph
			Stack<AVOWNode> nodeStack = new Stack<AVOWNode>();
			Stack<AVOWComponent> componentStack = new Stack<AVOWComponent>();
			
			
			nodeStack.Push(startNode);
			
			// We finish this loop when we have found a loop (or we are sure there are none)
			bool foundLoop = false;
			while (!finished && !foundLoop){
			
				// We visit our current node
				AVOWNode currentNode = nodeStack.Peek();
				currentNode.visited = true;
				
				// Go through each connections from here in order and find one that has not yet been traversed
				AVOWComponent nextConnection = null;
				for (int i = 0; i < currentNode.components.Count; ++i){
					AVOWComponent component = currentNode.components[i].GetComponent<AVOWComponent>();
					if (!component.visited && !component.disable){
						nextConnection = component;
						nextConnection.visited = true;
						componentStack.Push (nextConnection);
						break;
					}
				}
				
				// If we found an untraversed connection then this is our next point in our path
				if (nextConnection != null){
					AVOWNode nextNode = nextConnection.GetOtherNode(currentNode.gameObject).GetComponent<AVOWNode>();
					
					if (nextNode == null){
						Debug.LogError ("Failed to find other end of connection when finding loops");
					}
					nodeStack.Push(nextNode);
					
					// If this node has already been visited, then we have found a loop
					if (nextNode.visited){
						foundLoop = true;
					}
					
				}
				// If we have not managed to find an untraversed connection, pop our current stack element
				else {
					nodeStack.Pop ();
					// if this is the last element in the node stack, then there will be no component associated with it
					if (componentStack.Count > 0)
						componentStack.Pop ();
				}
				
				// If there is nothing left on the stack then we have traversed the whole graph and not found a loop
				if (nodeStack.Count == 0){
					finished = true;
				}

			}
			if (foundLoop){
				
				// If we found a loop, then record it
				// We do this by stepping backwards down our stack until we find our current node again - this ensures thast we just get the loop
				// and no stragglers
				AVOWNode loopStartNode = nodeStack.Peek();
				AVOWComponent loopStartComponent = componentStack.Peek();
				
				List<LoopElement> thisLoop = new List<LoopElement>();
				thisLoop.Add (new LoopElement(componentStack.Pop (), nodeStack.Pop ()));
				
				while(nodeStack.Peek() != loopStartNode){
					thisLoop.Add (new LoopElement(componentStack.Pop (), nodeStack.Pop ()));
				}
				loops.Add (thisLoop);
				
				// Now disable one of the components in the loop so this loop is not found again
				loopStartComponent.disable = true;
			}
			
			
		}
		
		// Quick check - we should have just traversed a spanning tree for the graph
		// so everyone component should have been visited
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			if (!component.visited && !component.disable)
				Debug.LogError ("Spanning tree does not visit very node");
		}
		
	}
	
	
	void RecordLoopsInComponents(){
		graph.ClearLoopRecords();
		
		for (int i = 0; i < loops.Count; ++i){
			for (int j = 0; j < loops[i].Count; ++j){
				loops[i][j].component.AddLoopRecord(i, loops[i][j].fromNode.gameObject);
			}
		}
	
	}
	
	bool SolveForCurrents(){
	
		// Create arrays need to solve equation coeffs
		double [,] R = new double[loops.Count, loops.Count];
		double [,] V = new double[loops.Count, 1];
		
		// For through each loop in turn (for each row in the matrices)
		for (int i = 0; i < loops.Count; ++i){
			// For each connection in the loop, check the resistance and any voltage drop
			for (int j = 0; j < loops[i].Count; ++j){
			
				LoopElement loopElement = loops[i][j];
				
				// deal with things differently depending on whether this is a load or a voltage source
				if (loopElement.component.type == AVOWComponent.Type.kLoad){
					float resistance = loopElement.component.GetResistance();
					
					// For each component in the loop, add in resistances for each loop that passes through it
					foreach (AVOWComponent.LoopRecord record in loopElement.component.loopRecords){
						if (record.isForward == loopElement.component.IsForward(loopElement.fromNode.gameObject)){
							R[i, record.loopId] += resistance;
						}
                        else{
							R[i, record.loopId] -= resistance;
						}
					
					}
				}
				else if (loopElement.component.type == AVOWComponent.Type.kVoltageSource){
					V[i, 0] = loopElement.component.GetVoltage(loopElement.fromNode);
				}
				else{
					Debug.LogError ("Unknown type of component");
				}
			}
		}  
		
		// Currents
		double[,] I = new double[0,0];
		
		// IF we do not have full rankm then find a solution using the Moore-Pensrose Pseudo-inverse
		// Method taken from here: http://en.wikipedia.org/wiki/Moore%E2%80%93Penrose_pseudoinverse#The_iterative_method_of_Ben-Israel_and_Cohen
		// search for "A computationally simple and accurate way to compute the pseudo inverse " ont his page
		
		
		//		if (R.GetLength (0) != R.GetLength (1)){
		//			Debug.LogError ("Matrix is not square, yet we expect it to be!");
		//		}
		//		
		
		// First we need to calc the SVD of the matrix
		double[] W = null;	// S matrix as a vector (leading diagonal)
		double[,] U = null;
		double[,] Vt = null;
		alglib.svd.rmatrixsvd(R, R.GetLength (0), R.GetLength (1), 2, 2, 2, ref W, ref U, ref Vt);
		
		double[,] S = new double[W.GetLength (0), W.GetLength (0)];
		
		for (int i = 0; i < R.GetLength (0); ++i){
			S[i,i] = W[i];
		}
		//				MathUtils.Matrix.SVD(R, out S, out U, out Vt);
		// Log the results
		
		//		double[,] testR0 = MathUtils.Matrix.Multiply(U, S);
		//		double[,] testR1 = MathUtils.Matrix.Multiply(testR0, Vt);
		
		double[,] Ut = MathUtils.Matrix.Transpose(U);
		double[,] Vtt = MathUtils.Matrix.Transpose (Vt);
		
		// Get the psuedo inverse of the U matrix (which is diagonal)
		// The way we do this is by taking the recipricol of each diagonal element, leaving the (close to) zero's in place
		// and transpose (actually we don't need to transpose because we always have square matricies)
		
		// I assume this gets initialised with zeros
		double[,] SInv = new double[S.GetLength(0), S.GetLength(0)];
		
		for (int i = 0; i < S.GetLength(0); ++i){
			if (Math.Abs (S[i,i]) > epsilon){
				SInv[i,i] = 1.0 / S[i,i];
				
			}					
		}
		
		//Rinv = Vtt Uinv St
		double[,] RInvTemp = MathUtils.Matrix.Multiply (Vtt, SInv);
		double[,] RInv = MathUtils.Matrix.Multiply (RInvTemp, Ut);
		
		//		// Test thast we have a psueoinverse
		//		double[,] res0 = MathUtils.Matrix.Multiply(R, RInv);
		//		double[,] res1 = MathUtils.Matrix.Multiply(res0, R);
		
		I = new double[loops.Count, 1];
		I = MathUtils.Matrix.Multiply(RInv, V); 
		
//		// Check that we get V - if not we have an unsolvable set of equations and 
//		// it means we have a loop of zero resistance with a voltage different in it
//		double[,] testV = MathUtils.Matrix.Multiply(R, I);
//		
//		bool failed = false;
//		for (int i = 0; i < loops.Count; ++i){
//			// f we find a bad loop
//			if (Math.Abs (V[i, 0] - testV[i, 0]) > epsilon){
//				// Then follow this loop finding all the voltage sources and put them in Emergency mode
//				for (int j = 0; j < loops[i].Count; ++j){
//					BranchAddress thisAddr = loops[i][j];
//					CircuitElement thisElement = Circuit.singleton.GetElement(new GridPoint(thisAddr.x, thisAddr.y));
//					if (Mathf.Abs (thisElement.GetVoltageDrop(thisAddr.dir, true)) > epsilon){
//						thisElement.TriggerEmergency();
//						failed = true;
//					}
//					BranchAddress oppAddr = GetOppositeAddress(thisAddr);
//					CircuitElement oppElement = Circuit.singleton.GetElement(new GridPoint(oppAddr.x, oppAddr.y));
//					if (Mathf.Abs (oppElement.GetVoltageDrop(oppAddr.dir, false)) > epsilon){
//						oppElement.TriggerEmergency();
//						failed = true;
//					}
//				}
//			}
//		}
//		if (failed) return false;
		
		
		
		loopCurrents = new float[loops.Count];
		if (I.GetLength(0) != 0){
			for (int i = 0; i < loops.Count; ++i){
				loopCurrents[i] = (float)I[i,0];
			}
		}
		// all went well
		return true;
		
	}	
	
	
	void StoreCurrentsInComponents(){
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			float totalCurrent = 0;
			foreach (AVOWComponent.LoopRecord record in component.loopRecords){
				totalCurrent += loopCurrents[record.loopId] * (record.isForward ? 1 : -1);
			}
			component.fwCurrent = totalCurrent;
		}
	}
	
	void DebugPrintComponentCurrents(){
		Debug.Log ("Print component currents");
		foreach (GameObject componentGO in graph.allComponents){
			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
			Debug.Log ("Component " + component.GetID() + ": from " + component.node0GO.GetComponent<AVOWNode>().GetID () + " to " + component.node1GO.GetComponent<AVOWNode>().GetID() + ": current = " + component.fwCurrent + "A");
			
		}
	}
	
	void CalcVoltages(){
		graph.ClearVisitedFlags();

		// First find the voltage source
		AVOWComponent cell = graph.FindVoltageSource().GetComponent<AVOWComponent>();
		
		// Set up the voltage accross it
		cell.node0GO.GetComponent<AVOWNode>().voltage = 0;
		
		// We have now visited this cell and the first node. 
		cell.visited = true;
		cell.node0GO.GetComponent<AVOWNode>().visited = true;		
		Stack<AVOWNode> nodeStack = new Stack<AVOWNode>();
		
		nodeStack.Push(cell.node0GO.GetComponent<AVOWNode>());
		
		while(nodeStack.Count != 0){
			AVOWNode lastNode = nodeStack.Peek();
			
			//Find a component attached to this node which has not yet been visited
			GameObject go = lastNode.components.Find (obj => !obj.GetComponent<AVOWComponent>().visited);
			
			// If we found one, then work out the voltage on the other end and either push it onto the stack (we it is a new node)
			// or just check we are being consistent if we have been there before
			if (go != null){
				AVOWComponent thisComponent = go.GetComponent<AVOWComponent>();
				thisComponent.visited = true;
				
				// Calc the voltage at the other end of this component
				float voltageChange = thisComponent.GetCurrent(lastNode.gameObject) * thisComponent.GetResistance();
				AVOWNode nextNode = thisComponent.GetOtherNode(lastNode.gameObject).GetComponent<AVOWNode>();
				float nextNodeVoltage = lastNode.voltage - voltageChange;
				
				// If we have not yet visited this node, then set the voltage on it and add it to the stack
				if (!nextNode.visited){
					nextNode.voltage = nextNodeVoltage;
					nextNode.visited = true;
					nodeStack.Push (nextNode);	
				}
				// Otherwise, assert that the voltage is the same as what we just caluclated
				else{
					if (!MathUtils.FP.Feq(nextNode.voltage, nextNodeVoltage)){
						Debug.LogError ("Voltages accross components are not consitent");
					}
				}
			}
			// If we failed to find an unvisited component, then pop this node off the stack (as there is
			// nothing more we can do here) and work on the node below it
			else{
				nodeStack.Pop ();
			}
		}
	}
	
	void DebugPrintVoltages(){
		Debug.Log ("Printing voltages");

		foreach (GameObject nodeGO in graph.allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			Debug.Log ("Node " + node.GetID() + ": " + node.voltage + "V");
		}
	}
	
	void SortByOrdinal(AVOWNode node, AVOWComponent.FlowDirection dir){
		if (dir == AVOWComponent.FlowDirection.kOut){
			node.outComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().outNodeOrdinal.CompareTo (obj2.GetComponent<AVOWComponent>().outNodeOrdinal)));
		}
		else{
			node.inComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().inNodeOrdinal.CompareTo (obj2.GetComponent<AVOWComponent>().inNodeOrdinal)));
		}
	}
	
//	void SortByOrdinal(AVOWNode node, AVOWComponent.FlowDirection dir){
//		if (dir == AVOWComponent.FlowDirection.kOut){
//			node.outComponents = node.outComponents.OrderBy (obj => obj.GetComponent<AVOWComponent>().outNodeOrdinal).ToList();
//		}
//		else{
//			node.inComponents = node.inComponents.OrderBy (obj => obj.GetComponent<AVOWComponent>().outNodeOrdinal).ToList ();
//		}
//	}


	void ConstructLists(){
	
		// Setup widths of comonents and in/out nodes
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.hWidth = Mathf.Abs(component.fwCurrent);
			component.SetupInOutNodes();
		}
		
	
		int[] idToIndexLookup = new int[graph.maxNodeId + 1];
	
		// Make a copy of the nodes in the tree
		allSimNodes = new SimNode[graph.allNodes.Count];
		int nodeIndex = 0;
		foreach (GameObject nodeGO in graph.allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			SimNode newNode = new SimNode();
			newNode.id = node.id;
			newNode.blockList[kIn] = new List<SimBlock>();
			newNode.blockList[kOut] = new List<SimBlock>();
			newNode.originalNode = node;
			allSimNodes[nodeIndex] = newNode;
			idToIndexLookup[node.id] = nodeIndex;
			++nodeIndex;
		}
		
		// Create a lookup for all the components organised by which nodes they are between (direction matters)
		Dictionary<Eppy.Tuple<AVOWNode, AVOWNode>, List<AVOWComponent>> components = new Dictionary<Eppy.Tuple<AVOWNode, AVOWNode>, List<AVOWComponent>>  ();
		
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			Eppy.Tuple<AVOWNode, AVOWNode> key = new Eppy.Tuple<AVOWNode, AVOWNode>(component.inNodeGO.GetComponent<AVOWNode>(), component.outNodeGO.GetComponent<AVOWNode>());
			if (components.ContainsKey(key)){
				components[key].Add (component);
			}
			else{
				components.Add(key, new List<AVOWComponent>{});
				components[key].Add (component);
			}
		}
		
		// Transfer all of these components into our block structure
		int numBlocks = components.Count;
		allSimBlocks = new SimBlock[numBlocks];
		
		int blockIndex = 0;
		foreach(KeyValuePair<Eppy.Tuple<AVOWNode, AVOWNode>, List<AVOWComponent>> item in components){
			SimBlock newBlock = new SimBlock();
			newBlock.components = item.Value;
			// order the components so the lowest hOrder value is at the beginning
			newBlock.components.Sort((obj1, obj2) => obj1.hOrder.CompareTo(obj2.hOrder));
			
			newBlock.hOrder = newBlock.components[0].hOrder;
			newBlock.nodes[kIn] = allSimNodes[idToIndexLookup[item.Key.Item1.id]];
			newBlock.nodes[kOut] = allSimNodes[idToIndexLookup[item.Key.Item2.id]];
			newBlock.ordinals[kIn] = -1;
			newBlock.ordinals[kOut] = -1;
			
			// Determine the ovall width									
			newBlock.hWidth = 0;
			foreach (AVOWComponent component in newBlock.components){
				newBlock.hWidth  += component.hWidth;
			}
			
			// Add ourselves to the approprite lists on the nodes
			newBlock.nodes[kIn].blockList[kIn].Add(newBlock);
			newBlock.nodes[kOut].blockList[kOut].Add(newBlock);
			allSimBlocks[blockIndex] = newBlock;
			++blockIndex;
		}
		
		// for each block list on the nodes convert to an array (as these are quicker to index)
		foreach (SimNode node in allSimNodes){
			for (int i = 0; i < kNumDirs; ++i){
				node.blockArray[i] = new SimBlock[node.blockList[i].Count];
				node.blockList[i].CopyTo(node.blockArray[i]);
			}
		}
	}
	
	void ConstructPermutations(){
	
		// Count how many permutations each list in each node has
		int numNodes = allSimNodes.Length;
		int[,] numPerms = new int[kNumDirs, numNodes];
		int numElements = 0;		
		for (int i = 0; i < allSimNodes.Length; ++i){
			SimNode node = allSimNodes[i];
			for (int j = 0; j < kNumDirs; j++){
				numPerms[j, i] = (int)MathUtils.Int.Factorial(node.blockArray[j].Length);
				numElements += node.blockArray[j].Length;
			}
		}
		
		// The total number of permutations is the number of permutations in each block list multiplied together
		int totalPerms = 1;
		foreach (int perms in numPerms){
			totalPerms *= perms;
		}
		
		permutations = new int[totalPerms, numElements];
		// Create list of all these permutations
		int itemIndex = 0;
		int numRepeats = 1;
		for (int i = 0; i < allSimNodes.Length; ++i){
			SimNode node = allSimNodes[i];
			for (int j = 0; j < kNumDirs; j++){
				int[,] localPerms = GeneratePermutations(node.blockArray[j].Length);
				
				// Go through the perm indicies				
				int grandPermIndex = 0;
				for (int k = 0; k < totalPerms/numRepeats; ++k){
					int localPermIndex = k % localPerms.GetLength(0);
					for (int l = 0; l < numRepeats; ++l){
						// Loop through elements in sequence
						for (int m = 0; m < localPerms.GetLength(1); ++m){
							permutations[grandPermIndex, itemIndex + m] = localPerms[localPermIndex, m];
							
						}
						++grandPermIndex;
						
					}
						
				}
				itemIndex += node.blockArray[j].Length;
				
				numRepeats = numRepeats * localPerms.GetLength(0);
				if (grandPermIndex != totalPerms){
					Debug.LogError ("Have miscalculated number of permutations");
				}
				
			}

		}
	}
	
	float GetInvalidH0(){
		return Single.NaN;
	}
	
	bool IsInvalidH0(float test){
		return Single.IsNaN(test);
	}
	
	
	void ClearTestData(){
		foreach(SimNode node in allSimNodes){
			node.h0 = GetInvalidH0();
			node.hWidth = -1;
		}
		foreach(SimBlock block in allSimBlocks){
			block.h0 = GetInvalidH0();
			block.ordinals[kOut] = 	-1;
			block.ordinals[kIn] = 	-1;
		}
	}
	
	void SetupOrdinals(int i){
		// Which element we are giving an ordinal to
		int permIndex = 0;
		for (int j = 0; j < allSimNodes.Length; ++j){
			for (int k = 0; k < kNumDirs; ++k){
				// Set up the ordinals for this permutationt
				for (int l = 0; l < allSimNodes[j].blockArray[k].Length; ++l){
					allSimNodes[j].blockArray[k][l].ordinals[k] = permutations[i, permIndex];
					permIndex++;
				}
			}
		}
		
//		// Print out the permutations
//		Debug.Log ("Permutation: " + i);
//		for (int j = 0; j < allSimNodes.Length; ++j){
//			Debug.Log ("Nodex: " + allSimNodes[j].id);
//			for (int k = 0; k < kNumDirs; ++k){
//				Debug.Log (k == 0 ? "Flowing out" : "Flowing in");
//				for (int l = 0; l < allSimNodes[j].blockArray[k].Length; ++l){
//					Debug.Log ("Block id " + allSimNodes[j].blockArray[k][l].components[0].GetID () + "ordinal: " + allSimNodes[j].blockArray[k][l].ordinals[k] );
//				}
//			}
//		}
		
	}
	
	bool SetUpH0s(){
		// Traverse the graph from our first node only going to the next node when we have set its h0 - also watchout fo contradictions
		bool error = false;
		Queue<SimNode> nodeQueue = new Queue<SimNode>();
		nodeQueue.Enqueue(allSimNodes[0]);
		while (nodeQueue.Count > 0 && !error){
			SimNode node = nodeQueue.Dequeue();
			if (IsInvalidH0(node.h0)){
				Debug.LogError("Should not be processing a ndoe wth an invalid H0");
			}
			for (int dir = 0; dir< kNumDirs  && !error; ++dir){
				
				// Created sorted list of components according to their ordinal numbers
				//Array.Sort (node.blockArray[dir], (obj1, obj2) => obj1.ordinals[dir].CompareTo(obj2.ordinals[dir]));
				node.sortedBlockArray[dir] = node.blockArray[dir].OrderBy(obj => obj.ordinals[dir]).ToArray();
				
				// Go through blocks accumulating widths and setting h0 on blocks
				float width = node.h0;
				for (int j = 0; j < node.sortedBlockArray[dir].Length  && !error; ++j){
					SimBlock block = node.sortedBlockArray[dir][j];
					if (!IsInvalidH0(block.h0) && !MathUtils.FP.Feq (block.h0, width)){
						error = true;
					}
					else{
						block.h0 = width;
						width += block.hWidth;
						
						// set the node width to be this (when we have finished the loop, is will be correct)
						node.hWidth = width - node.h0;
					}
					// ALso, check if blocks are the first component on block on other end
					// and this node has no h0 and if so, set H0 on this and add to queue
					int reverseDir = ReverseDirection(dir);
					SimNode otherNode = block.nodes[reverseDir];
					if (block.ordinals[reverseDir] == 0 && IsInvalidH0(otherNode.h0)){
						otherNode.h0 = block.h0;
						nodeQueue.Enqueue(otherNode);
						
					}
				}
				
				
			}
		}
		return !error;
	}
	
	bool SetupPermutation(int i){
		ClearTestData();
		
		// set up an h0 value for an arbitrary node
		allSimNodes[0].h0 = 0;
		
		// Set up the ordinals
		SetupOrdinals(i);
		
		return SetUpH0s();
	}
	
	
	void TestPermutations(){
		validPermutations.Clear();
		
		// Iterate through all the permutations setting up the ordinals and then testing the result
		for (int i = 0; i < permutations.GetLength(0); ++i){
			if (SetupPermutation(i)){
				validPermutations.Add (i);
			}
		}
		if (validPermutations.Count == 0){
			Debug.LogError ("Failed to layout circuit");
		}
	}
	
	void CreateHOrderArray(int[] hOrderArray){
		Array.Sort (allSimBlocks, (obj1, obj2) => (obj1.h0.CompareTo(obj2.h0)));
		for (int i = 0; i < allSimBlocks.Length; ++i){
			hOrderArray[i] = allSimBlocks[i].hOrder;
		}

	}
	
	void CreateH0Array(float[] h0Array){
		Array.Sort (allSimBlocks, (obj1, obj2) => (obj1.hOrder.CompareTo(obj2.hOrder)));
		for (int i = 0; i < allSimBlocks.Length; ++i){
			h0Array[i] = allSimBlocks[i].h0;
		}
		
	}
	
//	bool IsLexLower(int[] hOrderArray1, int[] hOrderArray2){
//		for (int i = 0; i < hOrderArray1.Length; ++i){
//			if (hOrderArray1[i] < hOrderArray2[i]) return true;
//			if (hOrderArray1[i] > hOrderArray2[i]) return false;
//		}
//		return false;
//	}
	
	
	bool IsLexLower(float[] h0Array1, float[] h0Array2){
		for (int i = 0; i < h0Array1.Length; ++i){
			if (h0Array1[i] < h0Array2[i]) return true;
			if (h0Array1[i] > h0Array2[i]) return false;
		}
		return false;
	}

		void ApplyBestPermutation(){
		// array of HOrder values for a given permutation
		float[] hOrderArrayMin = new float[permutations.GetLength (1)];
		float[] hOrderArrayTest = new float[permutations.GetLength (1)];
		int bestI = -1;
		bool hasLowest = false;
		
		foreach(int i in validPermutations){
			SetupPermutation(i);
			CreateH0Array(hOrderArrayTest);
			//CreateHOrderArray(hOrderArrayTest);
			if (!hasLowest || IsLexLower(hOrderArrayTest, hOrderArrayMin)){
				hOrderArrayTest.CopyTo(hOrderArrayMin, 0);
				hasLowest = true;
				bestI = i;
			}
		}
		
		SetupPermutation(bestI);
		foreach(SimBlock block in allSimBlocks){
			float width = block.h0;
			for (int i = 0; i < block.components.Count; ++i){
				block.components[i].h0 = width;
				width += block.components[i].hWidth;
				block.components[i].HasBeenLayedOut();
			
			}
		}
		foreach(SimNode simNode in allSimNodes){	
			simNode.originalNode.h0 = simNode.h0;
			simNode.originalNode.hWidth = simNode.hWidth;
		}
		
	}

	void LayoutHOrder(){	
	
		// Construct simBlock and SimNodes according to direction of current flow
		ConstructLists();
		
		// Create list of all permutations or orderings of blocks on nodes
		ConstructPermutations();
		
		// For each permuation, set up positions of each componetn and each node early existing if we ever get a contradiction
		// Note that we can set localH0 on each component - at sme point, we can infer h0 on node and when we do, we can start testing validite.
		TestPermutations();
		
		// For all valid permutations, pick the best one (according to lexographical hOrder)
		ApplyBestPermutation();
		
	}

/*
	
	bool LayoutHOrder(){	
		// Get ready for layout
		graph.ClearLayoutFlags();
		
		// Order the list of components for each node according to hOrder (not sure if this is necessary)
		foreach (AVOWNode node in graph.allNodes){
			node.components.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
			
		}
		// Order the "allComponents" list in the same way
		graph.allComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
		
		
		// Set up the widths of  components and also the in and out lists on the relevent nodes
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.hWidth = Mathf.Abs(component.fwCurrent);
			component.SetupInOutNodes();
			component.outNode.outComponents.Add (go);
			component.inNode.inComponents.Add (go);
		}
		
		foreach (AVOWNode node in graph.allNodes){
			float outWidth = 0;
			foreach (GameObject go in node.outComponents){
				outWidth += go.GetComponent<AVOWComponent>().hWidth;
				
			}
			float inWidth = 0;
			foreach (GameObject go in node.inComponents){
				inWidth += go.GetComponent<AVOWComponent>().hWidth;
				
			}
			// Check that in and outs agree and then set the node width 
			if (!MathUtils.FP.Feq(inWidth, outWidth)){
				Debug.LogError ("In and out widths on this node are not the same");
			}
			node.hWidth = inWidth;
		}
			
		// We know that the most left component has h0=0
		AVOWComponent firstComponent = graph.allComponents[0].GetComponent<AVOWComponent>();
		firstComponent.SetupInOutNodes();
		firstComponent.h0 = 0;
		firstComponent.inNodeOrdinal = 0;
		firstComponent.outNodeOrdinal = 0;	
		
		SortByOrdinal(firstComponent.outNode, AVOWComponent.FlowDirection.kOut);
		SortByOrdinal(firstComponent.inNode, AVOWComponent.FlowDirection.kIn);
		
		// Now we have some information and need other information. We have a set of inference rules
		// each one passes over all the data attempting to add new information. If after going through all
		// the rules we have not added any new information, then we are stuck (for the moment).
		int count = 0;
		while (!graph.IsAllLayedOut()){
			int numInfosAdded = 0;
			emergencyOptions.Clear();
			
			numInfosAdded += ApplyFirstComponentRule();
			numInfosAdded += ApplyOtherFirstComponentRule();
			numInfosAdded += ApplyNoFreedomRule();
			numInfosAdded += ApplyOrderedNeighboursRule();
			numInfosAdded += ApplyPositionedNeighboursRule();
			numInfosAdded += ApplyBoundsRule();
			
			if (numInfosAdded == 0){
				numInfosAdded += TryEmergencyOption();
			}
			
			// If we weren't able to add any new info (before we finished
			// then we need a "push" - yet to be figured out
			if (numInfosAdded == 0){
				Debug.Log ("Cannot add more info to information graph - breaking out of loop");
				break;
			}
			
			if (count++ > 50){
				Debug.Log ("Gone round 50 times probably stuck!");
				break;
			}
		}
		
		// All compoennts have now been layed out
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			component.hasBeenLayedOut = true;
		}
	
		return true;
	
	}
	
	
	// If this is the first component (ordinal == 0) in a given direction of flow and we know h0, then we can set
	// h0 on the node too
	int ApplyFirstComponentRule(){
		int infosAdded = 0;
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.h0 >= 0){
				if (component.outNodeOrdinal == 0 && component.outNode.h0 < 0){
					component.outNode.h0 = component.h0;
					infosAdded++;
				}
				if (component.inNodeOrdinal == 0 && component.inNode.h0 < 0){
					component.inNode.h0 = component.h0;
					infosAdded++;
				}
				
			}
		}
		return infosAdded;
		
	}
	
	// If we know h0 on the node and this is the first component in a given direction of flow on that node,
	// then we can set h0 on the component (note that this can happen because the node.h0 may have been set by
	// the first component in the other flow direction). 
	int ApplyOtherFirstComponentRule(){
		int infosAdded = 0;
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			if (component.h0 < 0){
				if (component.inNodeOrdinal == 0 && component.inNode.h0 >= 0){
					component.h0 = component.inNode.h0;
					infosAdded++;
				}
				if (component.outNodeOrdinal == 0 && component.outNode.h0 >= 0){
					component.h0 = component.outNode.h0;
					infosAdded++;
				}
				
			}
		}
		return infosAdded;		
	}
	
	
	// Used by ApplyNoFreedomRule
	int ApplyNoFreedomToOuts(AVOWNode node){
	
		int infosAdded = 0;
		AVOWNode toNode = null;
		bool allSameToNode = true;
		int	minHOrder = -1;
		AVOWComponent minHOrderComponent = null;
		int lastValidOrdinal = -1;
		
		// for all the components flowing out of this node find one that has not got an ordinal
		// Then record its toNode and check that all the others without an ordinal have the same toNode
		// Record which component of these has the lowest hOrder
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			
			// If we haven't yet found one without an ordinal
			// Then record it
			if (toNode == null){
				if (component.outNodeOrdinal == AVOWComponent.kOrdinalUnordered){
					toNode = component.GetOtherNode(node);
					minHOrder = component.hOrder;
					minHOrderComponent = component;
				}
				else{
					lastValidOrdinal = component.outNodeOrdinal;
				}
			}
			// Otherwise we need to check that all the others have the same "from" and "to" nodes
			// and record which of those has the lowest hOrder
			else{
				// the list should be ordered such that all components with an ordinal are at the beginning
				// so if we have found one without an ordinal we should not be expecting to find any more with one
				if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){	
					Debug.LogError ("Error - found component with ordinal sorted after those without");
				}
				// Check if this component is between the same nodes that out first one without an ordinal is between
				// Remember that they will all be flowing out from this node
				if (component.IsBetweenNodes(node, toNode)){
					
					// check if this is the "mininal hOrdered' one
					if (component.hOrder < minHOrder){
						minHOrder = component.hOrder;
						minHOrderComponent = component;
					}
				}
				else
					// If we have found another component which has different toNode then we are not constrained and we
					// have the freedom to reorder these compoennts (which will have a material impact on the layout)
					// so cannot do anything
				{
					allSameToNode = false;
				}
			}
		}
		// If we found a nonordinalled component and all the other components haver the same
		// to and From nodes - so we can set the ordinal of the minmal one to be one more than
		// the last valid one
		if (toNode != null && allSameToNode){
			minHOrderComponent.outNodeOrdinal = lastValidOrdinal + 1;
			// Need to reorder our list
			SortByOrdinal(node, AVOWComponent.FlowDirection.kOut);
			// Record that we have made some changes
			infosAdded++;
			
		}
		return infosAdded;
	}
	
	// Used by ApplyNoFreedomRule
	int ApplyNoFreedomToIns(AVOWNode node){
	
		int infosAdded = 0;
		AVOWNode fromNode = null;
		bool allSameToNode = true;
		int	minHOrder = -1;
		AVOWComponent minHOrderComponent = null;
		int lastValidOrdinal = -1;
		
		// for all the components flowing out of this node find one that has not got an ordinal
		// Then record its toNode and check that all the others without an ordinal have the same toNode
		// Record which component of these has the lowest hOrder
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
			
			// If we haven't yet found one without an ordinal
			// Then record it
			if (fromNode == null){
				if (component.inNodeOrdinal == AVOWComponent.kOrdinalUnordered){
					fromNode = component.GetOtherNode(node);
					minHOrder = component.hOrder;
					minHOrderComponent = component;
				}
				else{
					lastValidOrdinal = component.inNodeOrdinal;
				}
			}
			// Otherwise we need to check that all the others have the same "from" and "to" nodes
			// and record which of those has the lowest hOrder
			else{
				// the list should be ordered such that all components with an ordinal are at the beginning
				// so if we have found one without an ordinal we should not be expecting to find any more with one
				if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){	
					Debug.LogError ("Error - found component with ordinal sorted after those without");
				}
				// Check if this component is between the same nodes that out first one without an ordinal is between
				// Remember that they will all be flowing out from this node
				if (component.IsBetweenNodes(node, fromNode)){
					
					// check if this is the "mininal hOrdered' one
					if (component.hOrder < minHOrder){
						minHOrder = component.hOrder;
						minHOrderComponent = component;
					}
				}
				else
					// If we have found another component which has different fromNode then we are not constrained and we
					// have the freedom to reorder these compoennts (which will have a material impact on the layout)
					// so cannot do anything
				{
					allSameToNode = false;
				}
			}
		}
		// If we found a nonordinalled component and all the other components haver the same
		// to and From nodes - so we can set the ordinal of the minmal one to be one more than
		// the last valid one
		if (fromNode != null && allSameToNode){
			minHOrderComponent.inNodeOrdinal = lastValidOrdinal + 1;
			// Need to reorder our list
			SortByOrdinal(node, AVOWComponent.FlowDirection.kIn);
			// Record that we have made some changes
			infosAdded++;
		}
		return infosAdded;
	}
	
	
	
	// If we are the only component left that has not got an ordinal number on a particular node then
	// We can give ourselves an ordinal number (we are the one that is left)
	// By extension this also applies if there are several components left to be "ordinalled" as long as all those
	// components are going between the same nodes.
	int ApplyNoFreedomRule(){
		int infosAdded = 0;
		
		foreach (AVOWNode node in graph.allNodes){
			infosAdded += ApplyNoFreedomToOuts(node);
			infosAdded += ApplyNoFreedomToIns(node);
		}

		return infosAdded;		
	}	
	
	int ApplyOrderedNeighbourToOuts(AVOWNode node){
		int infosAdded = 0;
		
		// Create a loopup for the highest ordinals we have organised by which other node the components go to
		Dictionary<AVOWNode, int> knownOrdinals = new Dictionary<AVOWNode, int>();
		
		// Create a lookup of all the components which have no ordinal, but do have a "hightest ordinal" in the previous lookup
		Dictionary<AVOWNode, List<AVOWComponent>> processableComponents = new Dictionary<AVOWNode, List<AVOWComponent>> ();
		
		// This depends on the fact that the outComponents list is sorted so all those with ordinals already are at the front
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			AVOWNode otherNode = component.GetOtherNode(node);
			
			// If we have an ordinal, make (or update) our ordinals lookup
			if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				if (knownOrdinals.ContainsKey(otherNode)){
					knownOrdinals[otherNode] = component.outNodeOrdinal;
				}
				else{
					knownOrdinals.Add(otherNode, component.outNodeOrdinal);
				}
			}
			// If we don't have an ordinal for this component, check if we have already encounted a component that DOES have an ordinal
			// which is going to this other node
			else{
				if (knownOrdinals.ContainsKey(otherNode)){
					// Have we already added any such compoments
					if (processableComponents.ContainsKey(otherNode)){
						processableComponents[otherNode].Add (component);
					}
					else{
						processableComponents.Add (otherNode, new List<AVOWComponent>());
						processableComponents[otherNode].Add (component);
					}
					
				}
				
			}
		}
		foreach (KeyValuePair<AVOWNode, List<AVOWComponent>> dicEntry in processableComponents){
		
			// Sort the list of components according to hOrder number
			List<AVOWComponent> components = dicEntry.Value;
			components.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
		
			// Get the highest value ordinal number that we do know for this connection
			int lastValueOrdinal = knownOrdinals[dicEntry.Key];
			
			// Set up the ordinals on the components in the list
			for (int i = 0; i < components.Count; ++i){
				components[i].outNodeOrdinal = ++lastValueOrdinal;
				infosAdded++;
			}
		}
		
		if (infosAdded > 0){
			SortByOrdinal(node, AVOWComponent.FlowDirection.kOut);
		}
		
		
		return infosAdded;
	}
	
	int ApplyOrderedNeighbourToIns(AVOWNode node){
		int infosAdded = 0;
		
		// Create a loopup for the highest ordinals we have organised by which other node the components go to
		Dictionary<AVOWNode, int> knownOrdinals = new Dictionary<AVOWNode, int>();
		
		// Create a lookup of all the components which have no ordinal, but do have a "hightest ordinal" in the previous lookup
		Dictionary<AVOWNode, List<AVOWComponent>> processableComponents = new Dictionary<AVOWNode, List<AVOWComponent>> ();
		
		// This depends on the fact that the inComponents list is sorted so all those with ordinals already are at the front
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
			AVOWNode otherNode = component.GetOtherNode(node);
			
			// If we have an ordinal, make (or update) our ordinals lookup
			if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				if (knownOrdinals.ContainsKey(otherNode)){
					knownOrdinals[otherNode] = component.inNodeOrdinal;
				}
				else{
					knownOrdinals.Add(otherNode, component.inNodeOrdinal);
				}
			}
			// If we don't have an ordinal for this component, check if we have already encounted a component that DOES have an ordinal
			// which is going to this other node
			else{
				if (knownOrdinals.ContainsKey(otherNode)){
					// Have we already added any such compoments
					if (processableComponents.ContainsKey(otherNode)){
						processableComponents[otherNode].Add (component);
					}
					else{
						processableComponents.Add (otherNode, new List<AVOWComponent>());
						processableComponents[otherNode].Add (component);
					}
					
				}
				
			}
		}
		foreach (KeyValuePair<AVOWNode, List<AVOWComponent>> dicEntry in processableComponents){
			
			// Sort the list of components according to hOrder number
			List<AVOWComponent> components = dicEntry.Value;
			components.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
			
			// Get the highest value ordinal number that we do know for this connection
			int lastValueOrdinal = knownOrdinals[dicEntry.Key];
			
			// Set up the ordinals on the components in the list
			for (int i = 0; i < components.Count; ++i){
				components[i].inNodeOrdinal = ++lastValueOrdinal;
				infosAdded++;
			}
		}
		
		if (infosAdded > 0){
			SortByOrdinal(node, AVOWComponent.FlowDirection.kIn);
		}
		
		
		return infosAdded;
	}
	
	// If we have the ordinal of a comopnent for a given node, and there are other components going between the same 
	// set of nodes - then we can give them ordinals too
	int ApplyOrderedNeighboursRule(){
		int infosAdded = 0;
		
		// Do this by examining the in and out components for each node		
		foreach (AVOWNode node in graph.allNodes){
			infosAdded += ApplyOrderedNeighbourToOuts(node);
			infosAdded += ApplyOrderedNeighbourToIns(node);
			
		}
		
		return infosAdded;		
	}	
	
	
	int AppllyPositionedNeighbourToOuts(AVOWNode node){
		int infosAdded = 0;	
		
		// check
		int ordinalCheck = 0;
		
		// Note that this list is sorted
		node.outOrdinalledWidth = 0;
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			
			if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				// assert that they go up in ones
				if (component.outNodeOrdinal != ordinalCheck){
					Debug.LogError ("Ordinal values do not seem to be sequential");
				}
				ordinalCheck++;
				if (component.outLocalH0  != node.outOrdinalledWidth){
					component.outLocalH0 = node.outOrdinalledWidth;
					++infosAdded;
				}
				node.outOrdinalledWidth += component.hWidth;
				
				// If we have not got an h0 then set up the actual h0
				if (component.h0 < 0){
					component.h0 = component.outLocalH0 + node.h0;
					++infosAdded;
				}
				
			}
		}
		return infosAdded;
	}
	
	int AppllyPositionedNeighbourToIns(AVOWNode node){
		int infosAdded = 0;	
		
		// check
		int ordinalCheck = 0;
		
		// Note that this list is sorted
		node.inOrdinalledWidth = 0;
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
			
			if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				// assert that they go up in ones
				if (component.inNodeOrdinal != ordinalCheck){
					Debug.LogError ("Ordinal values do not seem to be sequential");
				}
				ordinalCheck++;
				if (component.inLocalH0  != node.inOrdinalledWidth){
					component.inLocalH0 = node.inOrdinalledWidth;
					++infosAdded;
				}
				node.inOrdinalledWidth += component.hWidth;
				
				// If we have not got an h0 then set up the actual h0
				if (component.h0 < 0){
					component.h0 = component.inLocalH0 + node.h0;
					++infosAdded;
				}
				
			}
		}
		return infosAdded;
	}
	

	
		
	// If we know the h0 of a component in a node and its ordinal (which should be a certainty) then we can also set the h0
	// of any other ordinaled componeonts
	int ApplyPositionedNeighboursRule(){
		int infosAdded = 0;
		
		// Do this by examining the in and out components for each node		
		foreach (AVOWNode node in graph.allNodes){
			if (node.h0 >= 0){
				infosAdded += AppllyPositionedNeighbourToOuts(node);
				infosAdded += AppllyPositionedNeighbourToIns(node);
			}
			
		}
		
		return infosAdded;		
	}	
	

	
	
	void RestrictBounds(AVOWNode node, float lowerConstraint, float upperConstraint){
		// Chek the constraints are consistent with the numbers we have already
		if (!MathUtils.FP.Fgeq(node.h0UpperBound, lowerConstraint) || !MathUtils.FP.Fleq(node.h0LowerBound, upperConstraint)){
			Debug.LogError ("Inconsistent constraints");
		}

		
		// if we have bounds already then just add the new constraints
		node.h0LowerBound = Mathf.Max (node.h0LowerBound, lowerConstraint);
		node.h0UpperBound = Mathf.Min (node.h0UpperBound, upperConstraint);
	}
	
	void RestrictBounds(AVOWComponent component, float lowerConstraint, float upperConstraint){
		// Chek the constraints are consistent with the numbers we have already
		if (!MathUtils.FP.Fgeq(component.h0UpperBound, lowerConstraint) || !MathUtils.FP.Fleq(component.h0LowerBound, upperConstraint)){
			Debug.LogError ("Inconsistent constraints");
		}

		// if we have bounds already then just add the new constraints
		component.h0LowerBound = Mathf.Max (component.h0LowerBound, lowerConstraint);
		component.h0UpperBound = Mathf.Min (component.h0UpperBound, upperConstraint);
	}	
	
	void ModifyNodeOutBounds(AVOWNode node){
		
		float cumulativeWidth = 0;
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			
			// If we know the ordinal
			if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				RestrictBounds(node, component.h0LowerBound - cumulativeWidth, component.h0UpperBound - cumulativeWidth);
				cumulativeWidth += component.hWidth;
			}
			// If we don't know the ordinal
			else{
				// Hmm, this node.Width should probably be the sum of the widths of all the components in this block (i.e. going between the same 
				// nodes)
				RestrictBounds(node, component.h0LowerBound - node.hWidth + component.hWidth, component.h0UpperBound - cumulativeWidth);
			}
		}
	}
	
	void ModifyNodeInBounds(AVOWNode node){
		
		float cumulativeWidth = 0;
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
						
			// If we know the ordinal
			if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				RestrictBounds(node, component.h0LowerBound - cumulativeWidth, component.h0UpperBound - cumulativeWidth);
				cumulativeWidth += component.hWidth;
			}
			// If we don't know the ordinal
			else{
				// Hmm, this node.Width should probably be the sum of the widths of all the components in this block (i.e. going between the same 
				// nodes)
				RestrictBounds(node, component.h0LowerBound - node.hWidth + component.hWidth, component.h0UpperBound - cumulativeWidth);
			}
		}
	}
	
	int ModifyComponentOutBounds(AVOWNode node){
		int infosAdded = 0;
		
		// Go through all the comopnents flowing out of this node and set ther bounds
		float cumulativeWidth = 0;
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			
			float oldLowerBound = component.h0LowerBound;
			float oldUpperBound = component.h0UpperBound;
			
			
			if (component.h0 < 0){
				// If we know the ordinal
				if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){
					RestrictBounds(component, node.h0LowerBound + cumulativeWidth, node.h0UpperBound + cumulativeWidth);
					cumulativeWidth += component.hWidth;
				}
				// If we don't know the ordinal
				else{
					RestrictBounds(component, node.h0LowerBound + cumulativeWidth, node.h0UpperBound + node.hWidth - component.hWidth);
				}
			}
			// If we update the bounds, then record that fact
			if (component.h0LowerBound != oldLowerBound || component.h0UpperBound != oldUpperBound){
				infosAdded++;
			}
		}
		return infosAdded;
	}
	
	int ModifyComponentInBounds(AVOWNode node){
		int infosAdded = 0;
		
		// Go through all the comopnents flowing in of this node and set ther bounds
		float cumulativeWidth = 0;
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
			
			float oldLowerBound = component.h0LowerBound;
			float oldUpperBound = component.h0UpperBound;
			
			
			if (component.h0 < 0){
				// If we know the ordinal
				if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){
					RestrictBounds(component, node.h0LowerBound + cumulativeWidth, node.h0UpperBound + cumulativeWidth);
					cumulativeWidth += component.hWidth;
				}
				// If we don't know the ordinal
				else{
					RestrictBounds(component, node.h0LowerBound + cumulativeWidth, node.h0UpperBound + node.hWidth - component.hWidth);
				}
			}
			// If we update the bounds, then record that fact
			if (component.h0LowerBound != oldLowerBound || component.h0UpperBound != oldUpperBound){
				infosAdded++;
			}
		}
		return infosAdded;
	}
	
	// Update any bounds information for h0 on nodes and components
	int ModifyBounds(){
		int infosAdded = 0;
		
		// First do the obvious (if h0 is set, then set the bounds around it)
		foreach (AVOWNode node in graph.allNodes){
			if (node.h0 >= 0 && (node.h0LowerBound != node.h0 || node.h0UpperBound != node.h0)){
				node.h0LowerBound = node.h0;
				node.h0UpperBound = node.h0;
				++infosAdded;				
			}
		}
		
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			if (component.h0 >= 0 && (component.h0LowerBound != component.h0 || component.h0UpperBound != component.h0)){
				component.h0LowerBound = component.h0;
				component.h0UpperBound = component.h0;
				++infosAdded;				
			}
		}
			
		
		// Do the nodes
		foreach (AVOWNode node in graph.allNodes){
			float oldLowerBound = node.h0LowerBound;
			float oldUpperBound = node.h0UpperBound;
			
			// If we knew the position absolutley then it would already have bee sorted			
			if (node.h0 < 0){
				// Check any components flowing out of it
				ModifyNodeOutBounds(node);
				ModifyNodeInBounds(node);

			}
			// If we update the bounds, then record that fact
			if (node.h0LowerBound != oldLowerBound || node.h0UpperBound != oldUpperBound){
				infosAdded++;
			}
		}
		// Do the components 
		// we do this node by node as it is more efficient
		foreach (AVOWNode node in graph.allNodes){
			infosAdded += ModifyComponentOutBounds(node);
			infosAdded += ModifyComponentInBounds(node);
		}
		return infosAdded;
	}
	
	
	
	// Regsiter there there seems to be a valid option to set this components ordinal value to ordinalValue
	// We don't just "do it" as we wait to make sure there are no other options and then pick the "highst HOrder" option to go with
	void RegisterEmergencyOption(AVOWComponent component, AVOWComponent.FlowDirection dir, int ordinalValue){
		emergencyOptions.Add(new EmergencyOption(component, dir, ordinalValue));
	}
	
	int TryEmergencyOption(){
		// If nothing to try, try nothing
		if (emergencyOptions.Count == 0) return 0;
		
		// Find the option with the lowest hOrder component
		int hMinOrder = AVOWComponent.kOrdinalUnordered;
		foreach (EmergencyOption option in emergencyOptions){
			if (option.component.hOrder < hMinOrder){
				hMinOrder = option.component.hOrder;
			}
		}
		// Make a shrunk list of just those options which involve that component
		List<EmergencyOption> shrunkList = new List<EmergencyOption>();
		foreach (EmergencyOption option in emergencyOptions){
			if (option.component.hOrder == hMinOrder){
				shrunkList.Add (option);
			}
		}	
		// Now consider the node which we are being asked to ordinate with respect to
		// Find the one with the left most position which has been allocagted to ordinalled components
		// so far
		// If there is more than one with the same posiiton, it probably doesn't matter which of the options we choose
		EmergencyOption useOption = null;
		foreach (EmergencyOption option in shrunkList){
			float minLeftPos = AVOWGraph.kMaxUpperBound;
			float leftPos = -1;
			if (option.dir == AVOWComponent.FlowDirection.kOut){
				leftPos = option.component.outNode.h0UpperBound + option.component.outNode.outOrdinalledWidth;
			}
			else{
				leftPos = option.component.inNode.h0UpperBound + option.component.inNode.inOrdinalledWidth;
			}
			if (leftPos < minLeftPos){
				minLeftPos = leftPos;
				useOption = option;
			}
		}
		
		// We should have an option to use
		if (useOption == null){
			Debug.LogError ("We do not have an option to use");
		}
		
		if (useOption.dir == AVOWComponent.FlowDirection.kOut){
			useOption.component.outNodeOrdinal = useOption.ordinalValue;
			SortByOrdinal(useOption.component.outNode, AVOWComponent.FlowDirection.kOut);
		}
		else{
			useOption.component.inNodeOrdinal = useOption.ordinalValue;
			SortByOrdinal(useOption.component.inNode, AVOWComponent.FlowDirection.kIn);
		}
		return 1;
		
		
		
	}*/
	
	
	
	void HeapsPermutations(int n, int[] sequencePass, ref int[,] outputArr, ref int outputIndex){
		int[] sequence = new int[sequencePass.Length];
		sequencePass.CopyTo(sequence, 0);
		//		int[] sequence = sequencePass;
		
		
		// If n==1 Copy to the output array and increment outputIndex
		if (n ==0){
			for (int i = 0; i < sequence.Length; ++i){
				outputArr[outputIndex, i] = sequence[i];
			}
			++outputIndex;
		}
		else{
			for (int i = 0; i < n; ++i){
				HeapsPermutations(n - 1, sequence, ref outputArr, ref outputIndex);
				// if i is even
				int j = ((i % 2) == 0) ? 0 : i;
				// Swap i an j
				int temp = sequence[n-1];
				sequence[n-1] = sequence[j];
				sequence[j] = temp;
			}
		}
	}
	
	int[,] GeneratePermutations(int numItems){
		if (cachedPermutations[numItems] == null){
			int numPerms = (int)MathUtils.Int.Factorial(numItems);
			
			int[,] permutations = new int[numPerms, numItems];
			int[] sequence = new int[numItems];
			// Fill sequence
			for (int i = 0; i < numItems; ++i){
				sequence[i] = i;
			}
			// Generate the permutations
			int outputIndex = 0;
			HeapsPermutations(numItems, sequence, ref permutations, ref outputIndex);
			cachedPermutations[numItems]  = permutations;
		}
		return cachedPermutations[numItems] ;
	}
	/*
	int ApplyBoundsRuleToOutBounds(AVOWNode node){
		int infosAdded = 0;
		
		// Create a loopup for the highest ordinals we have organised by which other node the components go to
		Dictionary<AVOWNode, float> freeWidths = new Dictionary<AVOWNode, float>();
		
		// Create a lookup of all the components which have no ordinal organised into groups flowing to the same component
		Dictionary<AVOWNode, List<AVOWComponent>> freeComponents = new Dictionary<AVOWNode, List<AVOWComponent>> ();
		float cumulativeWidth = 0;
		int highestOrdinal = -1;
		for (int i = 0; i < node.outComponents.Count; ++i){
			AVOWComponent component = node.outComponents[i].GetComponent<AVOWComponent>();
			AVOWNode otherNode = component.GetOtherNode(node);
			
			// If we have an ordinal, then sum up our width so far
			if (component.outNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				cumulativeWidth += component.hWidth;
				highestOrdinal = component.outNodeOrdinal;
				
			}
			// If not, it goes in the bucket of free components
			else{
				if (freeComponents.ContainsKey(otherNode)){
					freeComponents[otherNode].Add (component);
					freeWidths[otherNode] += component.hWidth;
				}
				else{
					freeComponents.Add(otherNode, new List<AVOWComponent>());
					freeWidths.Add (otherNode, 0);
					freeComponents[otherNode].Add (component);
					freeWidths[otherNode] += component.hWidth;
				}
			}
			
		}
		int numFreeBlocks = freeComponents.Count;
		
		// If less than 2 free blocks then this is dealt with by other rules
		if (numFreeBlocks < 2) return 0;
		
		// Sort the lists 
		foreach (List<AVOWComponent> list in freeComponents.Values){
			list.Sort ((obj1, obj2) => (obj1.hOrder.CompareTo (obj2.hOrder)));
		}		
		
		AVOWNode[] freeBlockNodes = new AVOWNode[numFreeBlocks];
		freeComponents.Keys.CopyTo (freeBlockNodes, 0);
		
		// Now we have cumulativeWidth tells us how much space is used up by components which are already fixed in place on this node
		// We have a set of lists of components  - where each list has all componetns flowing to (and from) the same node
		// we hve the width of each such set of components
		// We have an array of nodes (which wil let us look up the above lists).
		
		// Given an ordering of indexes e.g. (1, 3, 0, 2) we should be able to determine if this yields positions which contradict the 
		// bounds of the node on the other end of the components
		int[,] permutations = GeneratePermutations(numFreeBlocks);
		int numPerms = permutations.GetLength(0);
		
		// DEBUG print out the permutations
//		if (numFreeBlocks > 0 ){
//			Debug.Log ("Print Permutations:");
//			for (int i = 0; i < numPerms; ++i){
//				string seq = "";
//				for (int j = 0; j < numFreeBlocks; ++j){
//					seq += permutations[i, j];
//				}
//				Debug.Log ("Permuation " + i.ToString () + ": " + seq);
//			}
//		}
		
		bool[] isValidSequence = new bool[numPerms];
		// Implement a number of tests for consistence on a given sequence
		for (int i = 0; i < numPerms; ++i){
			// Keep track of the start position of each block
			float blockH0 = cumulativeWidth;
			
			// Go through this permutation and check its validity 
			bool permIsValid = true;
			for (int j = 0; j < numFreeBlocks; ++j){
				AVOWNode otherNode = freeBlockNodes[permutations[i, j]];
				float blockWidth = freeWidths[otherNode];
				// Get lowest hOrder component from the block
				AVOWComponent firstComponent = freeComponents[otherNode][0];
				
				// Find bounds of the block (given the ordering we are testing) - note that this is upper bound of h1 (not h0)
				float blockH0LowerBound = node.h0LowerBound + blockH0;
				float blockH1UpperBound = node.h0UpperBound + blockH0 + blockWidth;
				
				// Test against bounds of other node
				// --------------
				
				// The widest gap in which this block has to fit (note that this is an interval, rather than 
				// a single position we are specifying the bounds for. i.e. upper bound is the upperbound of
				// right hand edge and lower bound is lowerbound of left hand edge
				float nodeSpaceLowerBound;
				float nodeSpaceUpperBound;
				
				// If we know the relative position of this component on the inNode
				if (firstComponent.inLocalH0 >= 0){
					nodeSpaceLowerBound = otherNode.h0LowerBound + firstComponent.inLocalH0;
					nodeSpaceUpperBound = otherNode.h0UpperBound + firstComponent.inLocalH0 + blockWidth;
				}
				// If it is a free component
				else{
					nodeSpaceLowerBound = otherNode.h0LowerBound + otherNode.inOrdinalledWidth;
					nodeSpaceUpperBound = otherNode.h0UpperBound + otherNode.hWidth;
				}
				
				// Test if there is a position where we can place this block which conforms to both sets of constraints
				if (!MathUtils.FP.Fleq ( blockH0LowerBound, nodeSpaceUpperBound - blockWidth) || !MathUtils.FP.Fgeq ( blockH1UpperBound, nodeSpaceLowerBound + blockWidth)){
					permIsValid = false;
					break;
				}
				
				// Test against component bounds
				// ---------------------------
				int numComponentsInBlock = freeComponents[otherNode].Count;
				AVOWComponent lastComponent = freeComponents[otherNode][numComponentsInBlock - 1];
				float componentBlockLowerBound = firstComponent.h0LowerBound;
				float componentBlockUpperBound = lastComponent.h0UpperBound + lastComponent.hWidth;
				
				if (!MathUtils.FP.Fleq(blockH0LowerBound, componentBlockUpperBound - blockWidth) || !MathUtils.FP.Fgeq ( blockH1UpperBound, componentBlockLowerBound + blockWidth)){
					permIsValid = false;
					break;
				}				
				
				// Increment h0
				blockH0 += blockWidth;
				
			}
			isValidSequence[i] = permIsValid;
		}
		
		// go through the potentially valid permutations and check if the first block is always consistent. If it is,
		// We can place the first component from that block next in line (and the other rules should fill int he gaps)
		int firstBlock = -1;
		bool ok = true;
		for (int i = 0; i < numPerms; ++i){
			if (isValidSequence[i]){
				if (firstBlock == -1) firstBlock = permutations[i, 0];
				else if (firstBlock != permutations[i, 0]) ok = false;
			}
		}
		if (firstBlock == -1){
			Debug.LogError ("Should have at least one first block");
		}
		if (ok){
			// Find the first component in the block
			AVOWNode otherNode = freeBlockNodes[firstBlock];
			// Get lowest hOrder component from the block
			AVOWComponent thisComponent = freeComponents[otherNode][0];
			thisComponent.outNodeOrdinal = highestOrdinal  + 1;
			SortByOrdinal(node, AVOWComponent.FlowDirection.kOut);
			
			infosAdded++;
		}
		// Otherwise we have multiple valid options
		else{
			for (int i = 0; i < numPerms; ++i){
				if (isValidSequence[i]){
					AVOWComponent thisComponent = freeComponents[freeBlockNodes[permutations[i, 0]]][0];
					RegisterEmergencyOption(thisComponent, AVOWComponent.FlowDirection.kOut, highestOrdinal  + 1);
				}
			}
		}
		
		return infosAdded;
	}
	
	int ApplyBoundsRuleToInBounds(AVOWNode node){
		int infosAdded = 0;
		
		// Create a loopup for the highest ordinals we have organised by which other node the components go to
		Dictionary<AVOWNode, float> freeWidths = new Dictionary<AVOWNode, float>();
		
		// Create a lookup of all the components which have no ordinal organised into groups flowing to the same component
		Dictionary<AVOWNode, List<AVOWComponent>> freeComponents = new Dictionary<AVOWNode, List<AVOWComponent>> ();
		float cumulativeWidth = 0;
		int highestOrdinal = -1;
		for (int i = 0; i < node.inComponents.Count; ++i){
			AVOWComponent component = node.inComponents[i].GetComponent<AVOWComponent>();
			AVOWNode otherNode = component.GetOtherNode(node);
			
			// If we have an ordinal, then sum up our width so far
			if (component.inNodeOrdinal != AVOWComponent.kOrdinalUnordered){
				cumulativeWidth += component.hWidth;
				highestOrdinal = component.inNodeOrdinal;
			}
			// If not, it goes in the bucket of free components
			else{
				if (freeComponents.ContainsKey(otherNode)){
					freeComponents[otherNode].Add (component);
					freeWidths[otherNode] += component.hWidth;
				}
				else{
					freeComponents.Add(otherNode, new List<AVOWComponent>());
					freeWidths.Add (otherNode, 0);
					freeComponents[otherNode].Add (component);
					freeWidths[otherNode] += component.hWidth;
				}
			}
			
		}
		int numFreeBlocks = freeComponents.Count;
		
		// If less than 2 free blocks then this is dealt with by other rules
		if (numFreeBlocks < 2) return 0;
		
		// Sort the lists 
		foreach (List<AVOWComponent> list in freeComponents.Values){
			list.Sort ((obj1, obj2) => (obj1.hOrder.CompareTo (obj2.hOrder)));
		}

		AVOWNode[] freeBlockNodes = new AVOWNode[numFreeBlocks];
		freeComponents.Keys.CopyTo (freeBlockNodes, 0);
		
		// Now we have cumulativeWidth tells us how much space is used up by components which are already fixed in place on this node
		// We have a set of lists of components  - where each list has all componetns flowing to (and from) the same node
		// we hve the width of each such set of components
		// We have an array of nodes (which will let us look up the above lists).
		// We have lower ad upper bounds on h0 for this node
		// We have highestOrdinal, the highest ordinal that a component has been given on this node
		
		// Given an ordering of indexes e.g. (1, 3, 0, 2) we should be able to determine if this yields positions which contradict the 
		// bounds of the node on the other end of the components
		int numPerms = (int)MathUtils.Int.Factorial(numFreeBlocks);
		int[,] permutations = new int[numPerms, numFreeBlocks];
		int[] sequence = new int[numFreeBlocks];
		// Fill sequence
		for (int i = 0; i < numFreeBlocks; ++i){
			sequence[i] = i;
		}
		// Generate the permutations
		int outputIndex = 0;
		HeapsPermutations(numFreeBlocks, sequence, ref permutations, ref outputIndex);
		
		// DEBUG print out the permutations
//		if (numFreeBlocks > 0){
//			Debug.Log ("Print Permutations:");
//			for (int i = 0; i < numPerms; ++i){
//				string seq = "";
//				for (int j = 0; j < numFreeBlocks; ++j){
//					seq += permutations[i, j];
//				}
//				Debug.Log ("Permuation " + i.ToString () + ": " + seq);
//			}
//		}
		
		bool[] isValidSequence = new bool[numPerms];
		// Implement a number of tests for consistence on a given sequence
		for (int i = 0; i < numPerms; ++i){
			// Keep track of the start position of each block
			float blockH0 = cumulativeWidth;
			
			// Go through this permutation and check its validity 
			bool permIsValid = true;
			for (int j = 0; j < numFreeBlocks; ++j){
				AVOWNode otherNode = freeBlockNodes[permutations[i, j]];
				float blockWidth = freeWidths[otherNode];
				// Get lowest hOrder component from the block
				AVOWComponent firstComponent = freeComponents[otherNode][0];
				
				// Find bounds of the block (given the ordering we are testing) - note that this is upper bound of h1 (not h0)
				float blockH0LowerBound = node.h0LowerBound + blockH0;
				float blockH1UpperBound = node.h0UpperBound + blockH0 + blockWidth;
				
				// Test against bounds of other node
				// --------------
				
				// The widest gap in which this block has to fit (note that this is an interval, rather than 
				// a single position we are specifying the bounds for. i.e. upper bound is the upperbound of
				// right hand edge and lower bound is lowerbound of left hand edge
				float nodeSpaceLowerBound;
				float nodeSpaceUpperBound;
				
				// If we know the relative position of this component on the outNode
				if (firstComponent.outLocalH0 >= 0){
					nodeSpaceLowerBound = otherNode.h0LowerBound + firstComponent.outLocalH0;
					nodeSpaceUpperBound = otherNode.h0UpperBound + firstComponent.outLocalH0 + blockWidth;
				}
				// If it is a free component
				else{
					nodeSpaceLowerBound = otherNode.h0LowerBound + otherNode.outOrdinalledWidth;
					nodeSpaceUpperBound = otherNode.h0UpperBound + otherNode.hWidth;
				}
				
				// Test if there is a position where we can place this block which conforms to both sets of constraints
				if (!MathUtils.FP.Fleq ( blockH0LowerBound, nodeSpaceUpperBound - blockWidth) || !MathUtils.FP.Fgeq ( blockH1UpperBound, nodeSpaceLowerBound + blockWidth)){
					permIsValid = false;
					break;
				}
				
				// Test against component bounds
				// ---------------------------
				int numComponentsInBlock = freeComponents[otherNode].Count;
				AVOWComponent lastComponent = freeComponents[otherNode][numComponentsInBlock - 1];
				float componentBlockLowerBound = firstComponent.h0LowerBound;
				float componentBlockUpperBound = lastComponent.h0UpperBound + lastComponent.hWidth;
				
				if (!MathUtils.FP.Fleq(blockH0LowerBound, componentBlockUpperBound - blockWidth) || !MathUtils.FP.Fgeq ( blockH1UpperBound, componentBlockLowerBound + blockWidth)){
					permIsValid = false;
					break;
				}				
				
				// Increment h0
				blockH0 += blockWidth;
				
			}
			isValidSequence[i] = permIsValid;
		}
		
		// go through the potentially valid permutations and check if the first block is always consistent. If it is,
		// We can place the first component from that block next in line (and the other rules should fill int he gaps)
		int firstBlock = -1;
		bool ok = true;
		for (int i = 0; i < numPerms; ++i){
			if (isValidSequence[i]){
				if (firstBlock == -1) firstBlock = permutations[i, 0];
				else if (firstBlock != permutations[i, 0]) ok = false;
			}
		}
		if (firstBlock == -1){
			Debug.LogError ("Should have at least one first block");
		}
		if (ok){
			// Find the first component in the block
			AVOWNode otherNode = freeBlockNodes[firstBlock];
			// Get lowest hOrder component from the block
			AVOWComponent thisComponent = freeComponents[otherNode][0];
			thisComponent.inNodeOrdinal = highestOrdinal  + 1;
			SortByOrdinal(node, AVOWComponent.FlowDirection.kIn);
			infosAdded++;
		}
		// Otherwise we have multiple valid options
		else{
			for (int i = 0; i < numPerms; ++i){
				if (isValidSequence[i]){
					AVOWComponent thisComponent = freeComponents[freeBlockNodes[permutations[i, 0]]][0];
					RegisterEmergencyOption(thisComponent, AVOWComponent.FlowDirection.kIn, highestOrdinal  + 1);
				}
			}
		}		
		
		return infosAdded;
	}	

	// Create lower and upper bounds for each component and each node by using info about positional information of the things attached to them
	// then try different orderings of the unordered components in each node and see if there is only one oriding which satifies the bounds
	// at the other end of each component
	int ApplyBoundsRule(){
		int infosAdded = 0;
		
		infosAdded += ModifyBounds();
		
		// Do this by examining the in and out components for each node		
		foreach (AVOWNode node in graph.allNodes){
			infosAdded += ApplyBoundsRuleToOutBounds(node);
			infosAdded += ApplyBoundsRuleToInBounds(node);			
		}
		
		return infosAdded;		
	}	
	
*/
	

	
//	class GOComparer : IComparer<GameObject>
//	{
//		public int Compare(GameObject obj1, GameObject obj2)
//		{
//			return obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder);
//		}
//	}
	/*
	bool LayoutHOrder(){
		// Figure out the width of each node. The current flowing in == current flowing out == width
		foreach (AVOWNode node in graph.allNodes){
			float currentIn = 0;
			float currentOut = 0;
			foreach (GameObject go in node.components){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				float current = component.GetCurrent(node);
				if (current > 0) currentOut += Mathf.Abs(current);
				else currentIn += Mathf.Abs(current);
			}
			node.hWidth = currentIn;
		}
		
		
		// Order all the components in the most lists according to their H ordering
		foreach (AVOWNode node in graph.allNodes){
			//node.components.Sort (new GOComparer());
			node.components.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));
			
		}
		// Order the "allComponents" list in the same way
		graph.allComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().hOrder.CompareTo (obj2.GetComponent<AVOWComponent>().hOrder)));

		// Now traverse the graph setting up the horizontal positions of each component
		// We say a component is "visited" if it has been positioned
		// We say a node is "visited" if all the components in it have been positioned
		// We add a node to the queue if we can position all its components
		graph.ClearLayoutFlags();
		
		// Find the first component in the H ordering- we know this should be placed as far to the 
		// left as possible and both of the nodes it is attached to wil also be as far to the left as
		// possible
		AVOWComponent firstComponent = graph.allComponents[0].GetComponent<AVOWComponent>();
		firstComponent.h0 = 0;
		firstComponent.h1 = Mathf.Abs(firstComponent.fwCurrent);
		
		// Set up the nodes at either end and start the counters on them
		firstComponent.node0.h0 = 0;
		firstComponent.node0.hIn = 0;
		firstComponent.node0.hOut = 0;
		firstComponent.node0.hasBegunLayout = true;
		
		firstComponent.node1.h0 = 0;
		firstComponent.node1.hIn = 0;
		firstComponent.node1.hOut = 0;
		firstComponent.node1.hasBegunLayout = true;
		
		
		// Layout any component which are also between exactly these nodes
		int lastComponentsDone = 0;
		int componentsDone = 0;
		if (firstComponent.GetCurrent (firstComponent.node0) > 0)
			componentsDone += LayoutAllBetweenOrdered(firstComponent.node0, firstComponent.node1);
		else
			componentsDone += LayoutAllBetweenOrdered(firstComponent.node1, firstComponent.node0);
		
		while (componentsDone < graph.allComponents.Count){
			// Debug - this should never happen
			if (lastComponentsDone == componentsDone){
				Debug.LogError ("Failed to process any compnents - but not finished either");
				return false;
			}
			lastComponentsDone = componentsDone;
			// Go through each of the components in order
			for (int i = 0; i < graph.allComponents.Count; ++i){
				AVOWComponent component = graph.allComponents[i].GetComponent<AVOWComponent>();
				
				// If we have already been visited, then nothing to do
				if (component.visited) continue;
				
				// If this is the lowest ordered component on a node and is the first component on the other node
				// then we can set up the "other" node
				if (IsLowestComponent(component, component.node0) && IsFirstComponentOn(component, component.node1)){
					if (component.GetCurrent (component.node0) > 0){
						component.node1.hOut = component.node0.hOut;
						component.node1.hIn = component.node0.hOut;
						component.node1.hasBegunLayout = true;
					}
					else{
						component.node1.hOut = component.node0.hIn;
						component.node1.hIn = component.node0.hIn;
						component.node1.hasBegunLayout = true;
					}
					
				}
				else if (IsLowestComponent(component, component.node1) && IsFirstComponentOn(component, component.node0)){
					if (component.GetCurrent (component.node1) > 0){
						component.node0.hOut = component.node1.hOut;
						component.node0.hIn = component.node1.hOut;
						component.node0.hasBegunLayout = true;
					}
					else{
						component.node0.hOut = component.node1.hIn;
						component.node0.hIn = component.node1.hIn;
						component.node0.hasBegunLayout = true;
					}
				}
				// If we are the lowest component on one node and if our placement position on that node is also the next placable position on the
				// the "other" node, then we can place it
				if (IsLowestComponent(component, component.node0)){
					// Come from node 0
					if (component.GetCurrent(component.node0) > 0){
						if (MathUtils.FP.Feq (component.node0.hOut, component.node1.hIn)){
							componentsDone += LayoutAllBetweenOrdered(component.node0, component.node1);
						}
					}
					// come from node1
					else{
						if (MathUtils.FP.Feq (component.node0.hIn, component.node1.hOut)){
							componentsDone += LayoutAllBetweenOrdered(component.node1, component.node0);
						}
					}
				}
				else if (IsLowestComponent(component, component.node1)){
					// Come from node 1
					if (component.GetCurrent(component.node1) > 0){
						if (MathUtils.FP.Feq (component.node1.hOut, component.node0.hIn)){
							componentsDone += LayoutAllBetweenOrdered(component.node1, component.node0);
						}
					}
					else{
						if (MathUtils.FP.Feq (component.node1.hIn, component.node0.hOut)){
							componentsDone += LayoutAllBetweenOrdered(component.node0, component.node1);
						}
					}
				}
				
			}
			
		}
		return true;
		
	}
	
	bool IsFirstComponentOn(AVOWComponent component, AVOWNode node){
		// If we have begun the layout, then there must already be a component here
		if (node.hasBegunLayout) return false;
		
		// Test if this component is the lowest order component in this direction on this node
		for (int i = 0; i < node.components.Count; ++i){
			AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
			
			// if either both positive, or both negative - then we can test if they are equal
			if (testComponent.GetCurrent (node) * component.GetCurrent(node) > 0){
				return testComponent == component;
			}
		}
		// If we got to here then this component doesn't exist on this node - which is an error
		Debug.LogError ("This component does not exist on this node");
		return false;
	}
	
	bool IsLowestComponent(AVOWComponent component, AVOWNode node){
		// If ths node has not started to be layed out, then we do not know where it is
		if (!node.hasBegunLayout) return false;
		
		// if this is the lowest ordered component which has not yet been layed out on this node, then
		// we can lay it out (dealing with in and out components seperately
		
//		// DEBUG
//		for (int i = 0; i < node.components.Count; ++i){
//			AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
//			
//			string idString = testComponent.GetID();
//			Debug.Log (idString);
//		}
		
		
		
		// If this component is flowing OUT of this node
		if (component.GetCurrent (node) > 0){
			for (int i = 0; i < node.components.Count; ++i){
				AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
				
				if (!testComponent.visited && testComponent.GetCurrent (node) > 0 && testComponent.hOrder < component.hOrder) return false;
				if (testComponent == component) return true;
			}
		}
		else{
			for (int i = 0; i < node.components.Count; ++i){
				AVOWComponent testComponent = node.components[i].GetComponent<AVOWComponent>();
				
				if (!testComponent.visited && testComponent.GetCurrent (node) <= 0 && testComponent.hOrder < component.hOrder) return false;
				if (testComponent == component) return true;
			}
		}

		// We should neve get here
		Debug.LogError("Failed to find component in Node");
		return false;
		
	}
	
	int LayoutAllBetweenOrdered(AVOWNode nodeA, AVOWNode nodeB){
		int count = 0;
		
		for (int i = 0; i < graph.allComponents.Count; ++i){
			AVOWComponent component = graph.allComponents[i].GetComponent<AVOWComponent>();
				
			float absCurrent = Mathf.Abs(component.fwCurrent);
			if (!component.visited && component.IsBetweenNodes(nodeA, nodeB) && (component.GetCurrent (nodeA) > 0)){
				component.h0 = nodeA.hOut;
				nodeA.hOut += absCurrent;
				nodeB.hIn += absCurrent;
				component.h1 = nodeA.hOut;

				count++;
				component.visited = true;
				component.isLayedOut = true;
			}
		}
		return count;
		
	}
	
	*/
	
	void DebugPrintHOrder(){
		Debug.Log ("Printing HOrder Nodes");
		foreach(GameObject nodeGO in graph.allNodes){
			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
			Debug.Log ("Node " + node.GetID() + ": h0 = " + node.h0 + ", hWidth = " + node.hWidth + ", visited = " + node.visited);
		}
		
		Debug.Log ("Printing HOrder Components");
		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			Debug.Log ("Component " + component.GetID() + ": h0 = " + component.h0 + ", hWidth = " + component.hWidth + ", visited = " + component.visited);
		}
	}
	
	
	// Records the position of the mouse as a local position in one of the components
	void RecordMousePos(){
		// Find mouse pos in screen space
		Vector3 screenPos = Input.mousePosition;
		screenPos.z = 0;
		
		// Find mouse pos in world space
		mouseWorldPos = Camera.main.ScreenToWorldPoint( screenPos);
		mouseWorldPos.z = 0;
		
		// check which component we are over
		mouseOverComponent = null;
		// Keep track of global bounds of entire diagram (as we will use this if not over ay specific component)
		xMin = 100;
		yMin = 100;
		xMax = -1;
		yMax = -1;
		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			
			// If this component has never been layed out, then ignore
			if (!component.hasBeenLayedOut) continue;
			
			float lowVoltage = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float highVoltage = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float lowCurrent = component.h0;
			float highCurrent = component.h0 + component.hWidth;
			

			
			
			// Keep track of global bounds
			xMin = Mathf.Min (xMin, lowCurrent);
			yMin = Mathf.Min (yMin, lowVoltage);
			xMax = Mathf.Max (xMax, highCurrent);
			yMax = Mathf.Max (yMax, highVoltage);
			
			// Only register if over a resistor
			if (component.type == AVOWComponent.Type.kLoad &&
				mouseWorldPos.x > lowCurrent && 
			    mouseWorldPos.x < highCurrent && 
			    mouseWorldPos.y > lowVoltage && 
			    mouseWorldPos.y < highVoltage){
			    
			    if (mouseOverComponent != null){
			    	Debug.Log ("Error1 - Our mouse is over two components");
			    }
				mouseOverComponent = component;
				mouseOverXProp = (mouseWorldPos.x -  lowCurrent) / (highCurrent - lowCurrent);
				mouseOverYProp = (mouseWorldPos.y -  lowVoltage) / (highVoltage - lowVoltage);
					
			}
		}
		if (mouseOverComponent == null){
			mouseOverXProp = (mouseWorldPos.x -  xMin) / (xMax - xMin);
			mouseOverYProp = (mouseWorldPos.y -  yMin) / (yMax - yMin);
		}
		
	}

	
	void CalcMouseOffset(){
		float xMin = 100;
		float yMin = 100;
		float xMax = -1;
		float yMax = -1;
		//Figure out world posiiton of the location in the diagram where the mouse was
		if (mouseOverComponent != null){
			xMin = mouseOverComponent.h0;
			xMax = mouseOverComponent.h0 + mouseOverComponent.hWidth;
			yMin = Mathf.Min (mouseOverComponent.node0GO.GetComponent<AVOWNode>().voltage, mouseOverComponent.node1GO.GetComponent<AVOWNode>().voltage);
			yMax = Mathf.Max (mouseOverComponent.node0GO.GetComponent<AVOWNode>().voltage, mouseOverComponent.node1GO.GetComponent<AVOWNode>().voltage);
			// If a voltage source then need to mirror it all
//			if (mouseOverComponent.type == AVOWComponent.Type.kVoltageSource){
//				float temp = xMin;
//				xMin = -xMax;
//				xMax = -temp;
//			}		
			
		}
		else{

			foreach(GameObject go in graph.allComponents){
				AVOWComponent component = go.GetComponent<AVOWComponent>();
				
				float lowVoltage = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
				float highVoltage = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
				float lowCurrent = component.h0;
				float highCurrent = component.h0 + component.hWidth;
				
					
				
				// Keep track of global bounds
				xMin = Mathf.Min (xMin, lowCurrent);
				yMin = Mathf.Min (yMin, lowVoltage);
				xMax = Mathf.Max (xMax, highCurrent);
				yMax = Mathf.Max (yMax, highVoltage);
				
			}
		}
		
		Vector3  worldPos = new Vector3(xMin + mouseOverXProp * (xMax - xMin), yMin +mouseOverYProp * (yMax - yMin), 0);
		worldPos.z = 0;
//		Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos);
//		screenPos.z = 0;
//		
		Vector3 offset = worldPos - mouseWorldPos;
		Camera.main.gameObject.GetComponent<AVOWCamControl>().AddOffset(offset);
		
//		if (offset.sqrMagnitude != 0){
//			Debug.Log("***Moving****");
//			Debug.Log("prop = " + mouseOverXProp + ", " + mouseOverYProp);
//			Debug.Log ("yMin = " + yMin + " yMax = " + yMax);
//			Debug.Log("worldPos = " + worldPos.x + ", " + worldPos.y);
//			Debug.Log("mouseWorldPos = " + mouseWorldPos.x + ", " + mouseWorldPos.y);
//			Debug.Log("offset = " + offset.x + ", " + offset.y);
//		}
		
		
	}
	
}
