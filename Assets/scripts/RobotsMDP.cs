using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Initialized in GridsGenerator.cs
// Accessed in StateController.cs
public class RobotsMDP : MonoBehaviour {
	// Static singleton property
	// https://www.youtube.com/watch?v=acn0ONc4G4M
	// we make it singleton so that we can call it from anywhere
	public static RobotsMDP instance;
	void Awake () {
		instance = this;
	}
//	private int a;
//	public void set_a (int aa) {
//		this.a = aa;
//	}
//	public int get_a (){
//		return a;
//	}
//	public override string ToString(){
//		return "MDP"+this.a;
//	}
//
//	void OnGUI () {
//		GUI.Label (new Rect (10, 10, 100, 100), "a = " + this.a);
//	}


	// -------------------------------------------
	// data members
	private const int num_col_ = 4;
	private const int num_row_ = 5;

	private List<V2Int> cur_evaders_pos_ = new List<V2Int>();
	private List<V2Int> cur_robots_pos_ = new List<V2Int>();
	private V2Int cur_player_pos_;
	//	List<V2Int> player_history_nodes = new List<V2Int>();

	private Dictionary<V2Int,double> reward_map_ = new Dictionary<V2Int,double>();

	RobotsMDPState cur_s;


	public void UpdateAgentsPos() {
		this.cur_evaders_pos_.Clear ();
		GameObject[] evaders_gameObjects = GameObject.FindGameObjectsWithTag ("evador");
		foreach (GameObject _e in evaders_gameObjects) {
			evador_behave eb = _e.GetComponent<evador_behave> ();
			this.cur_evaders_pos_.Add (eb.current_node.grid_position);
		}
		this.cur_robots_pos_.Clear ();
		GameObject[] robots_gameObjects = GameObject.FindGameObjectsWithTag ("robot");
		foreach (GameObject _r in robots_gameObjects) {
			RobotsBehaveMDP rb = _r.GetComponent<RobotsBehaveMDP> ();
//			Debug.LogWarning (rb);
//			Debug.LogWarning (rb.current_node);
//			Debug.LogWarning (rb.current_node.grid_position.ToString());

			this.cur_robots_pos_.Add (rb.current_node.grid_position);

		}
		this.cur_player_pos_ = GameObject.FindGameObjectWithTag ("Player")
			.GetComponent<moving> ().current_node.grid_position;


//		this.player_history_pos.Add(cur_player_pos_);
	}


	private int num_cornered_evader(RobotsMDPState s) {
		/*=========================================================
		 * 1st: all neighbours of evaders are occupied;
		 * 2nd: either by a player or a robot;
		=========================================================*/
		int counter = 0;
		bool cornered = true;
		foreach (V2Int e in s.GetCurEvadersPos()) {
			List<V2Int> neighbours_pos = this.my_neighbours_pos (e);
			Dictionary<V2Int,char> omap = s.GetOccupancyMap ();
			foreach (V2Int x in neighbours_pos) {
				if (omap.ContainsKey (x)) {
					if (omap [x] == '.') {
						cornered = false;
						break;
					}
				}
			}
			if (cornered==true)
				counter += 1;
		}
		return counter;
	}


	//All neighbours
	private List<V2Int> my_neighbours_pos(V2Int x){
		List<V2Int> neighbours_pos = new List<V2Int>();
		foreach(SquareGrid.orientation o in SquareGrid.Four_dir){
			V2Int oo = x+(V2Int)SquareGrid.orient [o];
//			Debug.LogWarning ("oo="+oo);
			if (oo._x >= 0 || oo._x < num_col_ || oo._y >= 0 || oo._y < num_row_) {
				neighbours_pos.Add (oo);
			}
		}
		return neighbours_pos;
	}



	// MDP to move towards an evader
	public List<int> ComputeOptActions() {

//		for (int i = 0; i < 1000; i++) {
//			reward_map_ [new V2Int (Random.Range (0, 1000), Random.Range (0, 1000))] = i;
//		}
//		foreach (KeyValuePair<V2Int, double> kvp in reward_map_) {
//			Debug.LogWarning ( string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
//		}
		//		myDictionary[new V2Int(1,2)] = 23.3;

		this.cur_s = new RobotsMDPState(4,5,this.cur_evaders_pos_, 
			this.cur_robots_pos_, this.cur_player_pos_);
		Debug.LogWarning ("cur_s = "+this.cur_s.ToString()
			+" \nnum_cornered="+num_cornered_evader (this.cur_s));

		// mdp
		on the map, hole????
		test cornered
		// value iteration






		List<int> opt_actions = new List<int> ();
		for (int i = 0; i < this.cur_robots_pos_.Count; i++) {
//			opt_actions.Add (Random.Range (0, 4));
			opt_actions.Add (1);
		}
		return opt_actions;
	}

}
