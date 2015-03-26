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
	// We shouldn't need this
	public bool errorInBounds;
	
	// Set this if we want to force the mouse to pretend it is over a comonent when rejigging the camera
	public GameObject		mouseOverComponentForce = null;
	
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
		public float hOrder = -1;
		public List<AVOWComponent> components = new List<AVOWComponent>();
	};
	
	SimNode[] allSimNodes;
	SimBlock[] allSimBlocks;
	
	int[,]		permutations;
	List<int> validPermutations = new List<int>();
	
	// index of the simNode which is of a node that has a battery attached.
	int batteryNodeIndex = -1;
	
	
	
	public void Recalc(){
	
		List<GameObject> components = graph.allComponents;
		if (components.Count == 0){
			return;
		}

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
		
		CalcBounds();
		
	//	DebugPrintHOrder();
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}
	
	public void Initialise(){
	}
	
	public void GameUpdate(){
//		Debug.Log("Sim Update");
		
		RecordMousePos();
		Recalc();
		CalcMouseOffset();
//		
//		Debug.Log ("Print out order of node 1");
//		AVOWNode node0 = graph.allNodes[1].GetComponent<AVOWNode>();
//		for (int i = 0; i < node0.outComponents.Count; ++i){
//			AVOWComponent component = node0.outComponents[i].GetComponent<AVOWComponent>();
//			Debug.Log ("ID - " + component.GetID() + ", h0 = " + component.h0 + ", hOrder = " + component.hOrder);
//		}
		
		//AppHelper.Quit();
		
		// Record the hOrders
		List<GameObject> posComps  = graph.allComponents.OrderBy(obj => obj.GetComponent<AVOWComponent>().h0).ToList();
		
		for (int i = 0; i < posComps.Count; ++i){
			AVOWComponent component = posComps[i].GetComponent<AVOWComponent>();
			if (component.type == AVOWComponent.Type.kVoltageSource){
				component.hOrder = -1;
			}
			else{
				component.hOrder = i;
			}
		}
		// Ensure all components are sorted by horder (makes things easier to find
		graph.allComponents.Sort((obj1, obj2) => obj1.GetComponent<AVOWComponent>().hOrder.CompareTo(obj2.GetComponent<AVOWComponent>().hOrder));
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
				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node1.GetID() + " to " + node0.GetID() + " resistance = " + ((component.type == AVOWComponent.Type.kLoad) ? component.GetResistance().ToString () : "NULL") + " current = " + component.fwCurrent);
			}
			else{
				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node1.GetID() + " to " + node0.GetID() + " resistance = " + ((component.type == AVOWComponent.Type.kLoad) ? component.GetResistance().ToString () : "NULL") + " current = " + component.fwCurrent);
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
//	
//	void SortByOrdinal(AVOWNode node, AVOWComponent.FlowDirection dir){
//		if (dir == AVOWComponent.FlowDirection.kOut){
//			node.outComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().outNodeOrdinal.CompareTo (obj2.GetComponent<AVOWComponent>().outNodeOrdinal)));
//		}
//		else{
//			node.inComponents.Sort ((obj1, obj2) => (obj1.GetComponent<AVOWComponent>().inNodeOrdinal.CompareTo (obj2.GetComponent<AVOWComponent>().inNodeOrdinal)));
//		}
//	}
//	
//	void SortByOrdinal(AVOWNode node, AVOWComponent.FlowDirection dir){
//		if (dir == AVOWComponent.FlowDirection.kOut){
//			node.outComponents = node.outComponents.OrderBy (obj => obj.GetComponent<AVOWComponent>().outNodeOrdinal).ToList();
//		}
//		else{
//			node.inComponents = node.inComponents.OrderBy (obj => obj.GetComponent<AVOWComponent>().outNodeOrdinal).ToList ();
//		}
//	}


	void ConstructLists(){
	
		// Ensure all components are sorted by horder (makes things easier to find
		graph.allComponents.Sort((obj1, obj2) => obj1.GetComponent<AVOWComponent>().hOrder.CompareTo(obj2.GetComponent<AVOWComponent>().hOrder));
		
	
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
			
			
			// Check if this node has a battery attached
			foreach (GameObject go in node.components){
				if (go.GetComponent<AVOWComponent>().type == AVOWComponent.Type.kVoltageSource){
					batteryNodeIndex = nodeIndex;
				}
			}
				
			++nodeIndex;
		}
		
		// Create a lookup for all the components organised by which nodes they are between (direction matters)
		// At the same time construct a an "in" and "out" list for each node
		Dictionary<Eppy.Tuple<AVOWNode, AVOWNode, int>, List<AVOWComponent>> components = new Dictionary<Eppy.Tuple<AVOWNode, AVOWNode, int>, List<AVOWComponent>>  ();
		
		// Keep track of the number of lists we have associated with each node pair
		Dictionary<Eppy.Tuple<AVOWNode, AVOWNode>, int> blockTally = new Dictionary<Eppy.Tuple<AVOWNode, AVOWNode>, int>  ();
		
		// First clear the in/out lists
		foreach (GameObject go in graph.allNodes){
			AVOWNode node = go.GetComponent<AVOWNode>();
			node.inComponents.Clear();
			node.outComponents.Clear();
		}		
		
		foreach (GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			// Get list of components going in and out of the nodes at either end of this component (this won't include this one...yet)			
			List<GameObject> inComponents = component.inNodeGO.GetComponent<AVOWNode>().inComponents;
			List<GameObject> outComponents = component.outNodeGO.GetComponent<AVOWNode>().outComponents;

			// Check this first before making a normal key
			Eppy.Tuple<AVOWNode, AVOWNode> tallyKey = new Eppy.Tuple<AVOWNode, AVOWNode>(component.inNodeGO.GetComponent<AVOWNode>(), component.outNodeGO.GetComponent<AVOWNode>());
			
			int tally = 0;
			if (blockTally.ContainsKey(tallyKey)){
				tally = blockTally[tallyKey];
			}
			
			Eppy.Tuple<AVOWNode, AVOWNode, int> key = new Eppy.Tuple<AVOWNode, AVOWNode, int>(component.inNodeGO.GetComponent<AVOWNode>(), component.outNodeGO.GetComponent<AVOWNode>(), tally);
			
			if (components.ContainsKey(key)){
				// Check if the last component added to these  in/out node list is the most recent one added to this dicionary entry. If it is, we can add this one
				// If not we need to up the number (and somehow keep track of this number
				
				int numCompsDict = components[key].Count;
				int numCompsIn = inComponents.Count;
				int numCompsOut = outComponents.Count;
				
//				string thisCompID = component.GetID();
//				string blockID = components[key][numCompsDict-1].GetID();
				
				if (components[key][numCompsDict-1].gameObject == inComponents[numCompsIn-1] || components[key][numCompsDict-1].gameObject == outComponents[numCompsOut-1]){
					components[key].Add (component);
				}
				else{
				
					if (!blockTally.ContainsKey(tallyKey)) Debug.LogError ("No tally entry");
					
					tally++;
					
					Eppy.Tuple<AVOWNode, AVOWNode, int> newKey = new Eppy.Tuple<AVOWNode, AVOWNode, int>(component.inNodeGO.GetComponent<AVOWNode>(), component.outNodeGO.GetComponent<AVOWNode>(), tally);
					components.Add(newKey, new List<AVOWComponent>{});
					components[newKey].Add (component);
					blockTally[tallyKey] = tally;	
//					Debug.Log ("Created new block for component :" + component.GetComponent<AVOWComponent>().GetID());			
				}
			}
			else{
				// In this case, the tally should be zero
				if (tally != 0) Debug.LogError("Tally not zero, but no dictionary value found!");
				
				components.Add(key, new List<AVOWComponent>{});
				components[key].Add (component);
				blockTally.Add(tallyKey, tally);
			}
			inComponents.Add (go);
			outComponents.Add (go);
		}
		
	//	Debug.Log ("components.Count = " + components.Count);
		
		
		
		// Sort all out in/out lists
		foreach (GameObject go in graph.allNodes){
			AVOWNode node = go.GetComponent<AVOWNode>();
			node.inComponents.Sort ((obj1, obj2) => obj1.GetComponent<AVOWComponent>().hOrder.CompareTo(obj2.GetComponent<AVOWComponent>().hOrder));
			node.outComponents.Sort ((obj1, obj2) => obj1.GetComponent<AVOWComponent>().hOrder.CompareTo(obj2.GetComponent<AVOWComponent>().hOrder));
		}
//		
		// Transfer all of these components into our block structure
		int numBlocks = components.Count;
		allSimBlocks = new SimBlock[numBlocks];
		
		int blockIndex = 0;
		foreach(KeyValuePair<Eppy.Tuple<AVOWNode, AVOWNode, int>, List<AVOWComponent>> item in components){
			SimBlock newBlock = new SimBlock();
			newBlock.components = item.Value;
			// order the components so the lowest hOrder value is at the beginning
			newBlock.components.Sort((obj1, obj2) => obj1.hOrder.CompareTo(obj2.hOrder));
			
			newBlock.hOrder = newBlock.components[0].hOrder;
			newBlock.nodes[kIn] = allSimNodes[idToIndexLookup[item.Key.Item1.id]];
			newBlock.nodes[kOut] = allSimNodes[idToIndexLookup[item.Key.Item2.id]];
			newBlock.ordinals[kIn] = -1;
			newBlock.ordinals[kOut] = -1;
			
			// Determine the overall width									
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
		
//		Debug.Log("totalPerms = "+ totalPerms);
		
		permutations = new int[totalPerms, numElements];
		// Create list of all these permutations
		int itemIndex = 0;
		int numRepeats = 1;
		for (int i = 0; i < allSimNodes.Length; ++i){
			SimNode node = allSimNodes[i];
			for (int j = 0; j < kNumDirs; j++){
				int[,] localPerms = MathUtils.FP.GeneratePermutations(node.blockArray[j].Length);
				
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
		nodeQueue.Enqueue(allSimNodes[batteryNodeIndex]);
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
		
		
		// Set up an h0 value for a node that is connected to the battery
		allSimNodes[batteryNodeIndex].h0 = 0;
		
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
	
	void CreateHOrderArray(float[] hOrderArray){
		Array.Sort (allSimBlocks, (obj1, obj2) => (obj1.h0.CompareTo(obj2.h0)));
		for (int i = 0; i < allSimBlocks.Length; ++i){
			hOrderArray[i] = allSimBlocks[i].hOrder;
		}

	}
	
	void CreateH0Array(float[] h0Array){
		Array.Sort (allSimBlocks, (obj1, obj2) => (obj1.hOrder.CompareTo(obj2.hOrder)));
		for (int i = 0; i < allSimBlocks.Length; ++i){
			h0Array[i] = allSimBlocks[i].h0;//+ 0.5f * allSimBlocks[i].hWidth;
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
			simNode.originalNode.HasBeenLayedOut();
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
	
	void CalcBounds(){
		// Keep track of global bounds of entire diagram (as we will use this if not over ay specific component)
		xMin = 100;
		yMin = 0;
		xMax = -1;
		yMax = 1;
		errorInBounds = false;
		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			
			// If this component has never been layed out, then ignore
			if (!component.hasBeenLayedOut) continue;
			
			float lowVoltage = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float highVoltage = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float lowCurrent = component.h0;
			float highCurrent = component.h0 + component.hWidth;
			
			
			
			
			// Keep track of global bounds
			if (float.IsNaN(lowCurrent) || float.IsNaN(lowVoltage) || float.IsNaN(highCurrent) || float.IsNaN(highVoltage)){
				Debug.Log ("Error in bounds");
				errorInBounds = true;
				continue;
			}
			xMin = Mathf.Min (xMin, lowCurrent);
			yMin = Mathf.Min (yMin, lowVoltage);
			xMax = Mathf.Max (xMax, highCurrent);
			yMax = Mathf.Max (yMax, highVoltage);
			
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

		foreach(GameObject go in graph.allComponents){
			AVOWComponent component = go.GetComponent<AVOWComponent>();
			
			
			// If this component has never been layed out, then ignore
			if (!component.hasBeenLayedOut) continue;
			
			float lowVoltage = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float highVoltage = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
			float lowCurrent = component.h0;
			float highCurrent = component.h0 + component.hWidth;
			

	
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
	
	
	// Records the position of the mouse as a local position in the component we specify
	bool RecordMousePosOverComponent(GameObject componentGO){
	
	
		if (componentGO == null){
			return false;
		}
	
		AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
		
		// If not a valid component - then do the normal method
		if (component == null || !component.hasBeenLayedOut || component.type != AVOWComponent.Type.kLoad){

			return false;
		}
	
		// Find mouse pos in screen space
		Vector3 screenPos = Input.mousePosition;
		screenPos.z = 0;
		
		// Find mouse pos in world space
		mouseWorldPos = Camera.main.ScreenToWorldPoint( screenPos);
		mouseWorldPos.z = 0;
		
		
		float lowVoltage = Mathf.Min (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
		float highVoltage = Mathf.Max (component.node0GO.GetComponent<AVOWNode>().voltage, component.node1GO.GetComponent<AVOWNode>().voltage);
		float lowCurrent = component.h0;
		float highCurrent = component.h0 + component.hWidth;
		

		mouseOverComponent = component;
		mouseOverXProp = (mouseWorldPos.x -  lowCurrent) / (highCurrent - lowCurrent);
		mouseOverYProp = (mouseWorldPos.y -  lowVoltage) / (highVoltage - lowVoltage);
		
		return true;
	}

	
	void CalcMouseOffset(){
		float xMinLocal = 100;
		float yMinLocal = 100;
		float xMaxLocal = -1;
		float yMaxLocal = -1;
		//Figure out world posiiton of the location in the diagram where the mouse was
		if (mouseOverComponent != null){
			xMinLocal = mouseOverComponent.h0;
			xMaxLocal = mouseOverComponent.h0 + mouseOverComponent.hWidth;
			yMinLocal = Mathf.Min (mouseOverComponent.node0GO.GetComponent<AVOWNode>().voltage, mouseOverComponent.node1GO.GetComponent<AVOWNode>().voltage);
			yMaxLocal = Mathf.Max (mouseOverComponent.node0GO.GetComponent<AVOWNode>().voltage, mouseOverComponent.node1GO.GetComponent<AVOWNode>().voltage);
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
				xMinLocal = Mathf.Min (xMinLocal, lowCurrent);
				yMinLocal = Mathf.Min (yMinLocal, lowVoltage);
				xMaxLocal = Mathf.Max (xMaxLocal, highCurrent);
				yMaxLocal = Mathf.Max (yMaxLocal, highVoltage);
				
			}
		}
		
		Vector3  worldPos = new Vector3(xMinLocal + mouseOverXProp * (xMaxLocal - xMinLocal), yMinLocal +mouseOverYProp * (yMaxLocal - yMinLocal), 0);
		worldPos.z = 0;
//		Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos);
//		screenPos.z = 0;
//		
	//	Vector3 offset = worldPos - mouseWorldPos;
		//Camera.main.gameObject.GetComponent<AVOWCamControl>().AddOffset(offset);
		
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
