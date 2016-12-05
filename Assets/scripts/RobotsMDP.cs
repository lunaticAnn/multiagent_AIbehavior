using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
//http://answers.unity3d.com/questions/462042/unity-and-mathnet.html
// using MathMatrix = MathNet.Numerics.LinearAlgebra.Matrix<float>;
// using MathVector = MathNet.Numerics.LinearAlgebra.Vector<float>;

// Initialized in GridsGenerator.cs
// Accessed in StateController.cs
public class RobotsMDP : MonoBehaviour {

	// =======================================================================
	// coordinate system
	// =======================================================================

	// I am using xy system
	// 3
	// 2
	// 1
	// 0 1 2 3 4
	// each pos = (x,y)


	// =======================================================================
	// data members
	// =======================================================================

	public int k_num_col_;
	public int k_num_row_;

	// MDP
	private const float k_gamma_ = 1.0f;
	private const float k_epsilon_ = 0.001f;
	// 10*utility
	private const float k_reward_per_cornered_ = 200.0f;
	private const float k_reward_per_exit_ = -150.0f;
	private const float k_reward_per_action_ = -20.0f;
	private const float k_alpha_ = 0.0001f;

	private const float k_exited_sample_pr_ = 0.2f;
	private const float k_cornered_sample_pr_ = 0.1f;

	private const int k_obstacle_num_ = -1;
	private const int k_teleporter_num_ = 1;
	private const int k_exit_num_ = 9;
	public const char k_obstacle_ = 'o';
	public const char k_teleporter_ = 't';
	public const char k_exit_ = 'x';
	public const char k_unoccupied_ = '.';
	public const char k_robot_ = 'r';
	public const char k_evader_ = 'v';
	public const char k_player_ = 'p';

	private System.Random rnd_sample_type_ApproximateValueIteration_;
	private System.Random rnd_pos_ApproximateValueIteration_;
	private System.Random rnd_Shuffle_;
	private System.Random rnd_;

	private Dictionary<V2Int,char> terrain_map_;
	public Dictionary<V2Int,char> GetTerrainMap(){return this.terrain_map_;}

	private V2Int cornered_evader_pos_;
	private V2Int exited_evader_pos_;
	private SquareGrid.orientation dummy_evader_action_;

	private List<V2Int> cur_evaders_pos_;
	private List<V2Int> cur_robots_pos_;
	private List<V2Int> obstacles_pos_;
	private List<V2Int> exits_pos_;
	private List<V2Int> teleporter_pos_;
	private V2Int cur_player_pos_;
	//	List<V2Int> player_history_nodes = new List<V2Int>();

	// [evaders, robots, human]
	private List<List<SquareGrid.orientation>> joint_actions_;

	private string debug_content_;
	// =======================================================================
	// init functions
	// =======================================================================

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

	public void InitEverything(int[,] terrain_map){
		// http://stackoverflow.com/questions/1785744/how-do-i-seed-a-random-class-to-avoid-getting-duplicate-random-values
		this.rnd_sample_type_ApproximateValueIteration_ 
			= new System.Random (Guid.NewGuid ().GetHashCode ());
		this.rnd_pos_ApproximateValueIteration_ 
			= new System.Random (Guid.NewGuid ().GetHashCode ());
		this.rnd_Shuffle_ 
			= new System.Random (Guid.NewGuid ().GetHashCode ());
		this.rnd_ 
		= new System.Random (Guid.NewGuid ().GetHashCode ());
//		Debug.LogWarning (rnd_sample_type_ApproximateValueIteration_.Next(1,10));
//		Debug.LogWarning (rnd_pos_ApproximateValueIteration_.Next(1,10));
//		Debug.LogWarning (rnd_Shuffle_.Next(1,10));

		// init the terrain (obstacles)
		InitTerrainMap (terrain_map);
		// update the pos of each agent
		UpdateAgentsPos ();
		BuildActionMap ();
		this.cornered_evader_pos_ = new V2Int(-1,-1);
		this.exited_evader_pos_ = new V2Int(-2,-2);
		this.dummy_evader_action_ = SquareGrid.orientation.up;
		this.debug_content_ = "";
	}

	public void InitTerrainMap(int[,] terrain_map) {
		this.cur_evaders_pos_ = new List<V2Int>();
		this.cur_robots_pos_ = new List<V2Int>();
		this.obstacles_pos_ = new List<V2Int>();
		this.exits_pos_ = new List<V2Int>();
		this.teleporter_pos_ = new List<V2Int>();
		this.k_num_row_ = terrain_map.GetLength (0);
		this.k_num_col_ = terrain_map.GetLength (1);
		this.terrain_map_ = new Dictionary<V2Int,char> ();
		for (int y = 0; y < this.k_num_row_; y++) {
			for (int x = 0; x < this.k_num_col_; x++) {
				if (terrain_map [y, x] == k_exit_num_) {
					this.exits_pos_.Add (new V2Int (x, y));
					this.terrain_map_ [new V2Int (x, y)] = k_exit_;
				} else if (terrain_map [y, x] == k_obstacle_num_) {
					this.obstacles_pos_.Add (new V2Int (x, y));
					this.terrain_map_ [new V2Int (x, y)] = k_obstacle_;
				} else if (terrain_map [y, x] == k_teleporter_num_) {
					this.teleporter_pos_.Add (new V2Int (x, y));
					this.terrain_map_ [new V2Int (x, y)] = k_teleporter_;
				} else {
					this.terrain_map_ [new V2Int (x, y)] = k_unoccupied_;
				}
			}
		}
		string prt = "Terrain map:\n";
		for (int y = this.k_num_row_-1; y >= 0; y--) {
			string row = "";
			for (int x = 0; x < this.k_num_col_; x++) {
				row += this.terrain_map_ [new V2Int (x,y)];
			}
			prt += row+"\n";
		}
		Debug.LogWarning (prt);
	}

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
			this.cur_robots_pos_.Add (rb.current_node.grid_position);

		}
		this.cur_player_pos_ = GameObject.FindGameObjectWithTag ("Player")
			.GetComponent<moving> ().current_node.grid_position;
		//		this.player_history_pos.Add(cur_player_pos_);
	}


	// =======================================================================
	// utility functions
	// =======================================================================

	// Print out the permutations of the input 
	private static List<List<T>> ComputePermutations<T>(IEnumerable<T> input, int count) {
		List<List<T>> container = new List<List<T>> ();
		foreach (IEnumerable<T> permutation in PermuteUtils.Permute<T>(input, count)){
			List<T> sub_container = new List<T> ();
			foreach (T i in permutation){
				sub_container.Add (i);
			}
			container.Add (sub_container);
		}
		return container;
	}

	private void PermutationsWithRepetition(List<List<SquareGrid.orientation>> listList, 
		List<SquareGrid.orientation> cur_list, List<SquareGrid.orientation> source, 
		int number_elemenet){
		if (number_elemenet > 0) {
			foreach (SquareGrid.orientation oo in source) {
				List<SquareGrid.orientation> cur_list_tmp = new List<SquareGrid.orientation> (cur_list);
				cur_list_tmp.Add (oo);
				PermutationsWithRepetition (listList, cur_list_tmp, source, number_elemenet - 1);
			}
		} else {
			listList.Add (cur_list);
		}
	}

	private List<int> MinIndices(List<float> source){
		List<int> min_indices = new List<int>();
		float min_val = source[0];
		for (int i = 0; i < source.Count; i++) {
			if (source [i] < min_val) {
				min_indices.Clear ();
				min_indices.Add (i);
				min_val = source [i];
			} else if (source [i] == min_val) {
				min_indices.Add (i);
			}
		}
		return min_indices;
	}
	private List<int> MaxIndices(List<float> source){
		List<int> max_indices = new List<int>();
		float max_val = source[0];
		for (int i = 0; i < source.Count; i++) {
			if (source [i] > max_val) {
				max_indices.Clear ();
				max_indices.Add (i);
				max_val = source [i];
			} else if (source [i] == max_val) {
				max_indices.Add (i);
			}
		}
		return max_indices;
	}

	private string ActionsToString(List<List<SquareGrid.orientation>> actions){
		string str = "Number of actions = "+actions.Count.ToString()+"\nActions:";
		foreach (List<SquareGrid.orientation> act_arr in actions) {
			str += "\n" + ActionToString (act_arr);
		}
		return str;
	}

	private string ActionToString(List<SquareGrid.orientation> action){
		string str = "<";
		foreach (SquareGrid.orientation act in action) {
			str += act.ToString ()+", ";
		}
		str += ">";
		return str;
	}

	private string PositionListToString(List<V2Int> positions){
		string str = "Coordinates: ";
		foreach (V2Int pos in positions) {
			str += pos.ToString ()+", ";
		}
		return str;
	}

	private string DrawOccupancyMap(List<V2Int> evaders_pos, 
		List<V2Int> robots_pos, V2Int player_pos){
		Dictionary<V2Int,char> occupancy_map = new Dictionary<V2Int,char>();
		foreach (V2Int x in evaders_pos){occupancy_map [x] = k_evader_;}
		foreach (V2Int x in robots_pos){occupancy_map [x] = k_robot_;}
		occupancy_map [player_pos] = k_player_;
		foreach (KeyValuePair<V2Int,char> entry in this.terrain_map_) {
			if (entry.Value == k_obstacle_) {
				occupancy_map [entry.Key] = k_obstacle_;
			}
		}
		for (int y = 0; y < this.k_num_row_; y++) {
			for (int x = 0; x < this.k_num_col_; x++) {
				V2Int pos = new V2Int (x, y);
				if (!occupancy_map.ContainsKey (pos)) {
					occupancy_map [pos] = '.';
				}
			}
		}
		string ret_val = "-------------------\n";
		for (int y = this.k_num_row_-1; y >= 0; y--) {
			string row = "";
			for (int x = 0; x < this.k_num_col_; x++) {
				row += occupancy_map [new V2Int (x,y)];
			}
			ret_val += row+"\n";
		}
		return ret_val;
	}

	private List<V2Int> NeighborsPos(V2Int x, int num_col, int num_row){
		List<V2Int> neighbors_pos = new List<V2Int>();
		foreach(SquareGrid.orientation o in SquareGrid.Four_dir){
			V2Int oo = x+(V2Int)SquareGrid.orient [o];
			if (oo._x >= 0 && oo._x < num_col && oo._y >= 0 && oo._y < num_row) {
				neighbors_pos.Add (oo);
			}
		}
		return neighbors_pos;
	}

	// http://stackoverflow.com/questions/5080538/c-sharp-determine-duplicate-in-list
	private bool HasDuplicates<T>(List<T> myList) {
		var hs = new HashSet<T>();
		for (var i = 0; i < myList.Count; ++i) {
			if (!hs.Add(myList[i])) return true;
		}
		return false;
	}

	private V2Int CoordinateTransformD1ToD2 (int src){
		if (src >= 0 && src < this.k_num_col_ * this.k_num_row_) {
			return new V2Int ((int)(src % this.k_num_col_), (int)(src / this.k_num_col_));
		}
		Debug.LogError ("WTF CoordinateTransformD1ToD2 <"+src.ToString()+">");
		return new V2Int (-1, -1);
	}

	private int CoordinateTransformD2ToD1 (V2Int src){
		if (src._x >= 0 && src._x < this.k_num_col_ && src._y >= 0 && src._y < this.k_num_row_) {
			return src._x + src._y * this.k_num_col_;
		}
		Debug.LogError ("WTF CoordinateTransformD2ToD1 <"+src.ToString()+">");
		return -1;
	}

	// =======================================================================
	// core support functions
	// =======================================================================

	public void BuildActionMap() {
		this.joint_actions_ = new List<List<SquareGrid.orientation>> ();
		List<SquareGrid.orientation> cur_list = new List<SquareGrid.orientation> ();
		List<SquareGrid.orientation> source = new List<SquareGrid.orientation> ();
		source.Add (SquareGrid.orientation.down);
		source.Add (SquareGrid.orientation.up);
		source.Add (SquareGrid.orientation.left);
		source.Add (SquareGrid.orientation.right);
		PermutationsWithRepetition (this.joint_actions_, cur_list, source, 
			this.cur_evaders_pos_.Count+this.cur_robots_pos_.Count+1);
		Debug.LogWarning (this.ActionsToString(this.joint_actions_));
	}


	private Dictionary<V2Int,bool> BuildObstacleMap(List<V2Int> evaders_pos, 
		List<V2Int> robots_pos, V2Int player_pos){
		Dictionary<V2Int,bool> obstacles_map = new Dictionary<V2Int,bool> ();
		foreach (KeyValuePair<V2Int,char> entry in this.terrain_map_) {
			if (entry.Value == k_obstacle_) {
				obstacles_map [entry.Key] = true;
			}
		}
		foreach (V2Int tmp in evaders_pos) {
			// XXX: deactivated agent doesn't count
			if (tmp != this.cornered_evader_pos_ && tmp != this.exited_evader_pos_) {
				obstacles_map [tmp] = true;
			}
		}
		foreach (V2Int tmp in robots_pos) {
			obstacles_map [tmp] = true;
		}
		obstacles_map [player_pos] = true;
		return obstacles_map;
	}

	private void StateTransiter(List<SquareGrid.orientation> joint_action, 
		ref List<V2Int> evaders_pos,ref List<V2Int> robots_pos, ref V2Int player_pos){
		// XXX: if an agent moves into obstacles or other agents, stop
		// here all agents and obstacles are counted as obstacles
		Dictionary<V2Int,bool> obstacles_map = this.BuildObstacleMap(evaders_pos,robots_pos,player_pos);

		// sequence of moving is player, evaders, robots.
		// a. player is s_joint_action[-1] 
		V2Int prev_pos_player = new V2Int(player_pos);
		V2Int new_pos_player = new V2Int(prev_pos_player 
			+ (V2Int)(SquareGrid.orient [joint_action [joint_action.Count - 1]]));
		if (!new_pos_player.InMapBound (this.k_num_col_, this.k_num_row_)
			|| obstacles_map.ContainsKey (new_pos_player) == true) {
			player_pos = prev_pos_player;
		} else {
			obstacles_map [new_pos_player] = true;
			obstacles_map.Remove(prev_pos_player);
			player_pos = new_pos_player;
		}
		// b. robots
		List<V2Int> new_robots_pos = new List<V2Int>();
		for (int i = evaders_pos.Count; i < evaders_pos.Count + robots_pos.Count; i++) {
			V2Int prev_pos_robot = new V2Int(robots_pos [i - evaders_pos.Count]);
			V2Int new_pos_robot = new V2Int(prev_pos_robot + (V2Int)(SquareGrid.orient [joint_action [i]]));
			if (!new_pos_robot.InMapBound (this.k_num_col_, this.k_num_row_)
				|| obstacles_map.ContainsKey (new_pos_robot) == true) {
				new_robots_pos.Add(prev_pos_robot);
			} else {
				obstacles_map [new_pos_robot] = true;
				obstacles_map.Remove(prev_pos_robot);
				new_robots_pos.Add(new_pos_robot);
			}
		}
		robots_pos = new_robots_pos;
		// c. evaders
		List<V2Int> new_evaders_pos = new List<V2Int>();
		for (int i = 0; i < evaders_pos.Count; i++) {
			V2Int prev_pos_evader = new V2Int(evaders_pos [i]);
			// XXX: deactivated agent doesn't count
			if (prev_pos_evader == this.cornered_evader_pos_) {
				new_evaders_pos.Add (this.cornered_evader_pos_);
			} else if (prev_pos_evader == this.exited_evader_pos_) {
				new_evaders_pos.Add (this.exited_evader_pos_);
			} else {
				V2Int new_pos_evader = new V2Int(prev_pos_evader + (V2Int)(SquareGrid.orient [joint_action [i]]));
				if (!new_pos_evader.InMapBound (this.k_num_col_, this.k_num_row_)
					|| obstacles_map.ContainsKey (new_pos_evader) == true) {
					new_evaders_pos.Add(prev_pos_evader);
				} else {
					obstacles_map [new_pos_evader] = true;
					obstacles_map.Remove(prev_pos_evader);
					new_evaders_pos.Add(new_pos_evader);
				}
			}

		}
		evaders_pos = new_evaders_pos;
	}

	private void CheckCorneredExitedEvaders(ref List<V2Int> evaders_pos,
		List<V2Int> robots_pos, V2Int player_pos,
		ref int num_new_exited_evaders, ref int num_new_cornered_evaders){
		// a. check if there are any evaders exited successfully
		// b. check if there are any evaders cornered successfully
		// c. move exited and cornered evaders out of map
		num_new_exited_evaders = 0;
		num_new_cornered_evaders = 0;
		Dictionary<V2Int,bool> obstacles_map = this.BuildObstacleMap(evaders_pos,robots_pos,player_pos);
		for (int i = 0; i < evaders_pos.Count; i++) {
			V2Int evader_pos = evaders_pos [i];
			// if the evader is already cornered or exited, we ignore them
			if (evader_pos != cornered_evader_pos_ && evader_pos != exited_evader_pos_) {
				if (this.terrain_map_ [evader_pos] == k_exit_) {
					num_new_exited_evaders += 1;
					evaders_pos [i] = this.exited_evader_pos_;
				} else {
					bool cornered = true;
					List<V2Int> neighbours_pos = this.NeighborsPos (evader_pos, this.k_num_col_, this.k_num_row_);
					foreach (V2Int neighbour_pos in neighbours_pos) {
						if (!obstacles_map.ContainsKey (neighbour_pos)) {
							cornered = false;
							break;
						}
					}
					if (cornered == true) {
						num_new_cornered_evaders += 1;
						evaders_pos [i] = cornered_evader_pos_;
					}
				}
			}
		}
	}

	private float ComputeFactoredStateUtility(List<V2Int> evaders_pos, List<V2Int> robots_pos, 
		V2Int player_pos, List<float> feature_weight_list, ref List<float> feature_list){

		// 1 we combine player with robots into pursuers
		List<V2Int> pursuers_pos = new List<V2Int>();
		for (int i = 0; i < robots_pos.Count; i++) {
			pursuers_pos.Add (new V2Int(robots_pos [i]));
		}
		pursuers_pos.Add (new V2Int (player_pos));

		// 2 compute utility
		feature_list = new List<float>();
		// 2.1 evader pos, pursuer pos, obstacle pos, exit pos
		foreach (V2Int pos in evaders_pos) {
			if (pos == this.cornered_evader_pos_ || pos == this.exited_evader_pos_) {
				feature_list.Add (0.0f);
				feature_list.Add (0.0f);
			} else {
				feature_list.Add ((float)pos._x);
				feature_list.Add ((float)pos._y);
			}
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		foreach (V2Int pos in pursuers_pos) {
			feature_list.Add ((float)pos._x);
			feature_list.Add ((float)pos._y);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		foreach (V2Int pos in this.obstacles_pos_) {
			feature_list.Add ((float)pos._x);
			feature_list.Add ((float)pos._y);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		foreach (V2Int pos in this.exits_pos_) {
			feature_list.Add ((float)pos._x);
			feature_list.Add ((float)pos._y);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.2.1 \forall evader, \forall exit, dist(evader,exit), dist(evader,exit)^2
		foreach (V2Int ev in evaders_pos) {
			foreach (V2Int ex in this.exits_pos_) {
				float dist = 0.0f;
				if (ev == this.cornered_evader_pos_ || ev == this.exited_evader_pos_) {
					dist = 0.0f;
				} else {
					dist = (float)(ev.ManhattanDistance (ex));
				}
				feature_list.Add (dist);
				feature_list.Add (dist * dist);
			}
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.2.2 \forall evader, \prod_{exit} \prod dist(evader,exit)
		foreach (V2Int ev in evaders_pos) {					
			float product = 1.0f;
			if (ev == this.cornered_evader_pos_ || ev == this.exited_evader_pos_) {
				product = 0.0f;
			} else {
				foreach (V2Int ex in this.exits_pos_) {
					float dist = (float)(ev.ManhattanDistance (ex));
					product *= dist;
				}
			}
			feature_list.Add (product);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.3.1 \forall evader, \forall pursuer, dist(evader,pursuer), dist(evader,pursuer)^2
		foreach (V2Int e in evaders_pos) {
			foreach (V2Int p in pursuers_pos) {
				float dist = 0.0f;
				if (e == this.cornered_evader_pos_ || e == this.exited_evader_pos_) {
					dist = 0.0f;
				} else {
					dist = (float)(e.ManhattanDistance (p));
				}
				feature_list.Add (dist);
				feature_list.Add (dist * dist);
			}
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.3.2 \forall evader, \prod_{pursuer} \prod dist(evader,pursuer)
		foreach (V2Int e in evaders_pos) {
			float product = 1.0f;
			if (e == this.cornered_evader_pos_ || e == this.exited_evader_pos_) {
				product = 0.0f;
			} else {
				foreach (V2Int p in pursuers_pos) {
					float dist = (float)(e.ManhattanDistance (p));
					product *= dist;
				}
			}
			feature_list.Add (product);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.3.3 \forall pursuer, \prod_{evader} \prod dist(evader,pursuer)
		foreach (V2Int p in pursuers_pos) {
			float product = 1.0f;
			foreach (V2Int e in evaders_pos) {
				// XXX: it should not be 0 if only one evader is exited or cornered
				if (e == this.cornered_evader_pos_ || e == this.exited_evader_pos_) {
					continue;
				} else {
					float dist = (float)(p.ManhattanDistance (e));
					product *= dist;
				}
			}
			feature_list.Add (product);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.4.1 \forall pursuer, \forall pursuer, dist(pursuer,pursuer), dist(pursuer,pursuer)^2
		foreach (V2Int r1 in pursuers_pos) {
			foreach (V2Int r2 in pursuers_pos) {
				if (r1 != r2) {
					float dist = (float)(r1.ManhattanDistance (r2));
					feature_list.Add (dist);
					feature_list.Add (dist * dist);
				}
			}
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.4.2 \forall pursuer, \prod_{pursuer} \prod dist(pursuer,pursuer)
		foreach (V2Int r1 in pursuers_pos) {
			float product = 1.0f;
			foreach (V2Int r2 in pursuers_pos) {
				if (r1 != r2) {
					float dist = (float)(r1.ManhattanDistance (r2));
					product *= dist;
				}
			}
			feature_list.Add (product);
		}
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());
		// 2.5 bias
		feature_list.Add (1.0f);
//		Debug.LogWarning ("xxxxxxxxxx" + feature_list.Count.ToString ());

		// XXX: We cannot normalize it because they are different dimensions
		// We cannot make manhattan distance less than one. If d1<1,d2<1, d1*d2 < d1 (not make sense).
//		Debug.LogWarning ("num of features = "+feature_list.Count.ToString());
		this.debug_content_ += "Funct ComputeFactoredStateUtility: num of features = "+feature_list.Count.ToString() + "\n";
		string feature_list_str = "Feature values = ";
		foreach (float d in feature_list) {
			feature_list_str += d.ToString ()+", ";
		}
//		Debug.LogWarning (feature_list_str);
		this.debug_content_ += feature_list_str + "\n";
		string feature_weight_list_str = "Feature weights = ";
		foreach (float d in feature_weight_list) {
			feature_weight_list_str += d.ToString ()+", ";
		}
		//		Debug.LogWarning (feature_weight_list_str);
		this.debug_content_ += feature_weight_list_str + "\n";


		float state_utility = 0.0f;
//		Debug.LogAssertion (feature_list.Count == feature_weight_list.Count);
		for (int i = 0; i < feature_weight_list.Count; i++) {
			state_utility += feature_list[i] * feature_weight_list[i];
		}
		return state_utility;
	}

	private void ComputeOptimalAction(List<V2Int> evaders_pos, List<V2Int> robots_pos, 
		V2Int player_pos, List<float> feature_weight_list, 
		ref float best_utility_bellman_backed_up, ref List<SquareGrid.orientation> best_joint_action,
		ref float utility, ref List<float> feature_list, ref float reward){

		// ------------------------------------------------------------------------
		// 1 check cornered and exited
		// XXX: when sampling, we don't care about cornered or exited. We only randomly place agents on map.
		// so the first thing we should do is to check if cornered or exited happens in the current state map
		int num_new_exited_evaders = 0;
		int num_new_cornered_evaders = 0;
		this.CheckCorneredExitedEvaders (ref evaders_pos, robots_pos, player_pos, 
			ref num_new_exited_evaders, ref num_new_cornered_evaders);
		this.debug_content_ += "*2.2 after num_new_exited_evaders="+num_new_exited_evaders.ToString() +"\n";
		this.debug_content_ += "after num_new_cornered_evaders="+num_new_cornered_evaders.ToString() +"\n";
		if (num_new_exited_evaders != 0) {
			this.debug_content_ += "$$$EXIT" +"\n";
		}
		if (num_new_cornered_evaders != 0) {
			this.debug_content_ += "$$$CAPTURED" +"\n";
		}

		// ------------------------------------------------------------------------
		// 2 compute reward for the current state
		// XXX: compute reward of this state
		reward = k_reward_per_action_
			+ num_new_exited_evaders * k_reward_per_exit_
			+ num_new_cornered_evaders * k_reward_per_cornered_;
		
		// 3 compute the utility for the current state
		feature_list = new List<float> ();
		utility = ComputeFactoredStateUtility (evaders_pos, robots_pos, 
			player_pos, feature_weight_list, ref feature_list);

		// ------------------------------------------------------------------------
		// 4 bellman back-ups
		// ------------------------------------------------------------------------
		// 4.1 remove actions which are incompatible with evader model
		// 4.1.1 evader model
		// (1) closer to exit
		// (2) far from robots and humans
		List<SquareGrid.orientation> best_actions_evaders = new List<SquareGrid.orientation> ();
		for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
			V2Int evader_pos = evaders_pos [i];
			// XXX: when sampling, we don't care about cornered or exited. We only randomly place agents on map.
			// so the first thing we should do is to check if cornered or exited happens in the current state map
			// XXX: if the evader is cornered, there is no action available.
			// Then we assign it action dummy UP and put it at (-1,-1)
			if (evader_pos == this.cornered_evader_pos_ || evader_pos == this.exited_evader_pos_) {
				best_actions_evaders.Add (this.dummy_evader_action_);
			} else {
				SquareGrid.orientation best_action_evader;
				List<SquareGrid.orientation> feasible_actions_evader_tmp = 
					new List<SquareGrid.orientation> (SquareGrid.Four_dir);
				List<SquareGrid.orientation> feasible_actions_evader = 
					new List<SquareGrid.orientation> ();

				// here we are modeling the evaders. 
				// we assume that the evaders are smart enough to not moving towards obstacles, 
				// nor agents, nor out of map
				// remove actions towards out of map
				Dictionary<V2Int,bool> obstacles_map = this.BuildObstacleMap (evaders_pos, robots_pos, player_pos);
				foreach (SquareGrid.orientation action_evader in feasible_actions_evader_tmp) {
					V2Int new_evader_pos = evader_pos + (V2Int)(SquareGrid.orient [action_evader]);
					if (new_evader_pos.InMapBound (this.k_num_col_, this.k_num_row_)
						&& obstacles_map.ContainsKey (new_evader_pos) == false) {
						feasible_actions_evader.Add (action_evader);
					}
				}

				// distance for each of the 4 actions
				List<float> max_dists_evader_pursuers = new List<float> ();
				List<float> min_dists_evader_exits = new List<float> ();
				foreach (SquareGrid.orientation action_evader in feasible_actions_evader) {
					V2Int new_evader_pos = evader_pos +
						(V2Int)(SquareGrid.orient [action_evader]);
					List<float> dists_evader_pursuers = new List<float> ();
					List<float> dists_evader_exits = new List<float> ();
					foreach (V2Int robot_pos in robots_pos) {
						dists_evader_pursuers.Add (new_evader_pos.ManhattanDistance (robot_pos));
					}
					dists_evader_pursuers.Add (new_evader_pos.ManhattanDistance (player_pos));
					foreach (V2Int exit_pos in this.exits_pos_) {
						dists_evader_exits.Add (new_evader_pos.ManhattanDistance (exit_pos));
					}
					max_dists_evader_pursuers.Add (dists_evader_pursuers 
						[this.MaxIndices (dists_evader_pursuers) [0]]);
					min_dists_evader_exits.Add (dists_evader_exits 
						[this.MinIndices (dists_evader_exits) [0]]);
				}

				// find the best action based on its distance
				List<int> min_indices_min_dists_evader_exits = this.MinIndices (min_dists_evader_exits);
				if (min_indices_min_dists_evader_exits.Count == 1) {
					best_action_evader = feasible_actions_evader [min_indices_min_dists_evader_exits [0]];
				} else {
					List<float> sliced_max_dists_evader_pursuers = new List<float> ();
					foreach (int _ in min_indices_min_dists_evader_exits) {
						sliced_max_dists_evader_pursuers.Add (max_dists_evader_pursuers [_]);
					}
					List<int> max_indices_sliced_max_dists_evader_pursuers = 
						this.MaxIndices (sliced_max_dists_evader_pursuers);
					if (max_indices_sliced_max_dists_evader_pursuers.Count == 1) {
						best_action_evader = feasible_actions_evader 
							[min_indices_min_dists_evader_exits 
								[max_indices_sliced_max_dists_evader_pursuers [0]]];
					} else {
						best_action_evader = feasible_actions_evader 
							[min_indices_min_dists_evader_exits 
								[max_indices_sliced_max_dists_evader_pursuers 
									[rnd_.Next (0, max_indices_sliced_max_dists_evader_pursuers.Count)]]];
					}
				}
				best_actions_evaders.Add (best_action_evader);
			}
		}
		string best_actions_evaders_str = "best actions = ";
		foreach (SquareGrid.orientation best_action_evader in best_actions_evaders) {
			best_actions_evaders_str += best_action_evader.ToString () + ", ";
		}
		this.debug_content_ += best_actions_evaders_str +"\n";

		// 3.1.2 modify action list
		List<List<SquareGrid.orientation>> joint_actions = 
			new List<List<SquareGrid.orientation>> ();
		foreach (List<SquareGrid.orientation> joint_action in this.joint_actions_) {
			bool good = true;
			for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
				if (best_actions_evaders [i] != joint_action [i]) {
					good = false;
				}
			}
			if (good == true) {
				List<SquareGrid.orientation> tmp = new List<SquareGrid.orientation> ();
				for (int i = 0; i < joint_action.Count; i++) {
					tmp.Add (joint_action [i]);
				}
				joint_actions.Add (tmp);
			}
		}

		// ------------------------------------------------------------------------
		// 4.2 update value using bellman equation
		best_utility_bellman_backed_up = -1.0f;
		best_joint_action = new List<SquareGrid.orientation> ();

		for (int ii = 0; ii < joint_actions.Count; ii++) {
			List<SquareGrid.orientation> joint_action = joint_actions [ii];

			// 4.2.1 transition based on probability
			List<V2Int> new_evaders_pos = new List<V2Int> ();
			for (int i = 0; i < evaders_pos.Count; i++) {
				new_evaders_pos.Add (new V2Int (evaders_pos [i]));
			}
			List<V2Int> new_robots_pos = new List<V2Int> ();
			for (int i = 0; i < robots_pos.Count; i++) {
				new_robots_pos.Add (new V2Int (robots_pos [i]));
			}
			V2Int new_player_pos = new V2Int (new V2Int (player_pos));

			// XXX: (-1,-1) will still stay at (-1,-1)
			this.debug_content_ += "3.2 s_joint_action = " + this.ActionToString (joint_action) +"\n";
			this.debug_content_ += "prev state:\n" + this.DrawOccupancyMap
				(new_evaders_pos, new_robots_pos, new_player_pos) + "\n";
			this.StateTransiter (joint_action, ref new_evaders_pos, ref new_robots_pos, ref new_player_pos);
			this.debug_content_ += "next state:\n" + this.DrawOccupancyMap
				(new_evaders_pos, new_robots_pos, new_player_pos) + "\n";

			// 4.2.2 computing reward
			// 4.2.2.1 check if exiting successfully
			// 4.2.2.2 check if cornered
			// 4.2.2.3 move cornered or exited evaders away from the map
			num_new_exited_evaders = 0;
			num_new_cornered_evaders = 0;
			List<V2Int> new_evaders_pos_original = new List<V2Int> ();
			for (int i = 0; i < new_evaders_pos.Count; i++) {
				new_evaders_pos_original.Add (new V2Int(new_evaders_pos [i]));
			}
			this.CheckCorneredExitedEvaders (ref new_evaders_pos, new_robots_pos, new_player_pos, 
				ref num_new_exited_evaders, ref num_new_cornered_evaders);
			this.debug_content_ += "3.2.2 after num_new_exited_evaders="
				+ num_new_exited_evaders.ToString () + "\n";
			this.debug_content_ += "after num_new_cornered_evaders="
				+ num_new_cornered_evaders.ToString () + "\n";

			// 4.2.3 compute reward for the next state
			float new_reward = k_reward_per_action_
			                + num_new_exited_evaders * k_reward_per_exit_
			                + num_new_cornered_evaders * k_reward_per_cornered_;

			// 4.2.4 compute the utility for the next state
			List<float> new_feature_list = new List<float> ();
			float new_utility = ComputeFactoredStateUtility (new_evaders_pos, new_robots_pos, 
				new_player_pos, feature_weight_list, ref new_feature_list);

			// TODO: human model
			// TODO: determined
			// XXX: we don't use the reward of the next state. Instead, we use the reward of this state.
			float utility_bellman_backed_up = reward + k_gamma_ * new_utility;
			this.debug_content_ += "3.2.4 s_reward = " + reward.ToString () + "\n";
			this.debug_content_ += "new_s_utility = " + new_utility.ToString () + "\n";
			this.debug_content_ += "s_utility_bellman_backed_up = "
				+ utility_bellman_backed_up.ToString () + "\n";

			if (ii == 0) {
				best_utility_bellman_backed_up = utility_bellman_backed_up;
				best_joint_action = joint_action;
			} else if (utility_bellman_backed_up > best_utility_bellman_backed_up) {
				best_joint_action = joint_action;
			}
		}
		this.debug_content_ += "4.2.4 s_utility = " + utility.ToString () + "\n";
		this.debug_content_ += "best_utility_bellman_backed_up = "
			+ best_utility_bellman_backed_up.ToString () + "\n";
		this.debug_content_ += "best_joint_action = "
			+ this.ActionToString(best_joint_action) + "\n";

	}


	// =======================================================================
	// core functions
	// =======================================================================

	public List<SquareGrid.orientation> ComputeOptActions() {
		List<SquareGrid.orientation> robots_opt_cur_action = this.ApproximateValueIteration ();
//		List<int> opt_actions = new List<int> ();
//		for (int i = 0; i < this.cur_robots_pos_.Count; i++) {
//			//			opt_actions.Add (Random.Range (0, 4));
//			opt_actions.Add (1);
//		}
		return robots_opt_cur_action;
	}

	private List<SquareGrid.orientation> ApproximateValueIteration() {
//		Debug.LogWarning ("ApproximateValueIteration");
		this.debug_content_ += "ApproximateValueIteration\n";
		// ------------------------------------------------------------------------
		// 1. initiate theta - feature weight
		// ------------------------------------------------------------------------
		// 1.1 evader pos, robot pos, player pos, obstacle pos, exit pos
		List<float> feature_weight_list = new List<float> ();
		float base_val = 1.0f;
		List<V2Int> cur_pursuer_pos = new List<V2Int> ();
		for (int i = 0; i < this.cur_robots_pos_.Count; i++) {
			cur_pursuer_pos.Add (new V2Int (this.cur_robots_pos_ [i]));
		}
		cur_pursuer_pos.Add (new V2Int (this.cur_player_pos_));
		foreach (V2Int pos in this.cur_evaders_pos_) {
			feature_weight_list.Add (base_val);
			feature_weight_list.Add (base_val);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		foreach (V2Int pos in cur_pursuer_pos) {
			feature_weight_list.Add (base_val);
			feature_weight_list.Add (base_val);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		foreach (V2Int pos in this.obstacles_pos_) {
			feature_weight_list.Add (base_val);
			feature_weight_list.Add (base_val);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		foreach (V2Int pos in this.exits_pos_) {
			feature_weight_list.Add (base_val);
			feature_weight_list.Add (base_val);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.2.1 \forall evader, \forall exit, dist(evader,exit), dist(evader,exit)^2
		for (int i = 0; i < this.cur_evaders_pos_.Count * this.exits_pos_.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f);
			feature_weight_list.Add (base_val * 4.0f);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.2.2 \forall evader, \prod_{exit} \prod dist(evader,exit)
		for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f * (float)this.exits_pos_.Count);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.3.1 \forall evader, \forall pursuer, dist(evader,pursuer), dist(evader,pursuer)^2
		for (int i = 0; i < this.cur_evaders_pos_.Count * cur_pursuer_pos.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f);
			feature_weight_list.Add (base_val * 4.0f);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.3.2 \forall evader, \prod_{pursuer} \prod dist(evader,pursuer)
		for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f * (float)cur_pursuer_pos.Count);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.3.3 \forall pursuer, \prod_{evader} \prod dist(evader,pursuer)
		for (int i = 0; i < cur_pursuer_pos.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f * (float)this.cur_evaders_pos_.Count);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.4.1 \forall pursuer, \forall pursuer, dist(pursuer,pursuer), dist(pursuer,pursuer)^2
		// remove duplicates by "- cur_pursuer_pos.Count"
		// TODO: still have duplicates here!
		for (int i = 0; i < cur_pursuer_pos.Count * cur_pursuer_pos.Count - cur_pursuer_pos.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f);
			feature_weight_list.Add (base_val * 4.0f);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.4.2 \forall pursuer, \prod_{pursuer} \prod dist(pursuer,pursuer)
		for (int i = 0; i < cur_pursuer_pos.Count; i++) {
			feature_weight_list.Add (base_val * 2.0f * (float)cur_pursuer_pos.Count);
		}
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// 1.5 bias
		feature_weight_list.Add (base_val * 20.0f);
//		Debug.LogWarning ("xxxxxxxxxxx = "+feature_weight_list.Count.ToString());

		// XXX: normalize?
//		float sum = 0.0;
//		foreach (float d in feature_weight_list) {
//			sum += d;
//		}
//		for (int i = 0; i < feature_weight_list.Count; i++) {
//			feature_weight_list [i] /= sum;
//		}

		int num_feature = feature_weight_list.Count;
//		Debug.LogWarning ("num of features weight = " + num_feature.ToString ());
		this.debug_content_ += "*1. num of features weight = " + num_feature.ToString ()+"\n";

		string feature_weight_list_str = "";
		foreach (float d in feature_weight_list) {
			feature_weight_list_str += d.ToString () + ", ";
		}
//		Debug.LogWarning (feature_weight_list_str);
		this.debug_content_ += feature_weight_list_str+"\n";

//		MathVector feature_weight = MathVector.Build.Dense(feature_weight_list.ToArray());
//		Debug.LogWarning ("vector = "+feature_weight);

		bool converged = false;
		int counter = 0;
		while (true) {
			// ------------------------------------------------------------------------
			// 2 initialize the current state
			// ------------------------------------------------------------------------
			// 2.1 sample a state by flatting the map into 1D
			Dictionary<V2Int,bool> obstacles_map = new Dictionary<V2Int,bool> ();
			foreach (KeyValuePair<V2Int,char> entry in this.terrain_map_) {
				if (entry.Value == k_obstacle_) {
					obstacles_map [entry.Key] = true;
				}
			}
			List<V2Int> positions = new List<V2Int> ();
			List<V2Int> s_evaders_pos = new List<V2Int> ();
			List<V2Int> s_robots_pos = new List<V2Int> ();
			V2Int s_player_pos = new V2Int ();
			// robots + player
			List<V2Int> s_pursuers_pos = new List<V2Int> ();

			// 2.1.1 sample 3 different types by introducing a bias
			// Naturally
			// Pr(sample a state with EXITED agent) ~ 53/1000
			// Pr(sample a state with CORNERED agent) ~ 7/1000
			// Now through prob, EXITED sample ~ 194/1000
			// Now through prob, CORNERED sample ~ 103/1000
			string sample_type = "";
			double rand_double = this.rnd_sample_type_ApproximateValueIteration_.NextDouble ();
			if (rand_double < (double)k_cornered_sample_pr_) {
				// 2.1.1.1 sample a state with an evader cornered
				sample_type = "CORNERED";
				while (true) {
					V2Int pos = new V2Int (this.CoordinateTransformD1ToD2 
						(this.rnd_pos_ApproximateValueIteration_.Next (0, this.k_num_col_ * this.k_num_row_)));
					if (obstacles_map.ContainsKey (pos) == false) {
						// it should not be at exit
						if (this.terrain_map_ [pos] != k_exit_) {
							List<V2Int> neighbors_pos = this.NeighborsPos (pos, this.k_num_col_, this.k_num_row_);
							List<V2Int> open_neighbors_pos = new List<V2Int> ();
							foreach (V2Int neighbor_pos in neighbors_pos) {
								if (!obstacles_map.ContainsKey (neighbor_pos)) {
									open_neighbors_pos.Add (neighbor_pos);
								}
							}
							// if we have enough agent to corner it
							if (open_neighbors_pos.Count <= this.cur_robots_pos_.Count + 1) {
								s_evaders_pos.Add (pos);
								obstacles_map [pos] = true;
								foreach (V2Int open_neighbor_pos in open_neighbors_pos) {
									s_pursuers_pos.Add (open_neighbor_pos);
									obstacles_map [open_neighbor_pos] = true;
								}
								break;
							}
						}
					}
				}
			} else if (rand_double < (double)(k_cornered_sample_pr_ + k_exited_sample_pr_)) {
				// 2.1.1.2 sample a state with an evader exited
				sample_type = "EXITED";
				int exit_index = this.rnd_pos_ApproximateValueIteration_.Next (0, this.exits_pos_.Count);
				s_evaders_pos.Add (this.exits_pos_ [exit_index]);
				obstacles_map [this.exits_pos_ [exit_index]] = true;
			} else {
				// 2.1.1.3 sample a regular state
				sample_type = "Regular";
			}
			// 2.1.2 add extra agents until enough
			while (s_evaders_pos.Count < this.cur_evaders_pos_.Count) {
				V2Int pos = new V2Int (this.CoordinateTransformD1ToD2 
					(this.rnd_pos_ApproximateValueIteration_.Next (0, this.k_num_col_ * this.k_num_row_)));
				while (obstacles_map.ContainsKey (pos) == true) {
					pos = new V2Int (this.CoordinateTransformD1ToD2 
						(this.rnd_pos_ApproximateValueIteration_.Next (0, this.k_num_col_ * this.k_num_row_)));
				}
				s_evaders_pos.Add (pos);
				obstacles_map [pos] = true;
			}
			while (s_pursuers_pos.Count < this.cur_robots_pos_.Count + 1) {
				V2Int pos = new V2Int (this.CoordinateTransformD1ToD2 
					(this.rnd_pos_ApproximateValueIteration_.Next (0, this.k_num_col_ * this.k_num_row_)));
				while (obstacles_map.ContainsKey (pos) == true) {
					pos = new V2Int (this.CoordinateTransformD1ToD2 
						(this.rnd_pos_ApproximateValueIteration_.Next (0, this.k_num_col_ * this.k_num_row_)));
				}
				s_pursuers_pos.Add (pos);
				obstacles_map [pos] = true;
			}
			// 2.1.3 shuffle
			s_pursuers_pos.Shuffle();
			s_evaders_pos.Shuffle();
			s_player_pos = new V2Int (s_pursuers_pos [0]);
			for (int i = 1; i < s_pursuers_pos.Count; i++) {
				s_robots_pos.Add (s_pursuers_pos [i]);
			}
//			foreach (V2Int a in s_evaders_pos) {
//				Debug.LogWarning (a);
//			}
//			foreach (V2Int a in s_robots_pos) {
//				Debug.LogWarning (a);
//			}
//			Debug.LogWarning (s_player_pos);

//			Debug.LogWarning ("*2.1 Sample a "+sample_type+" state = \n" + this.DrawOccupancyMap 
//				(s_evaders_pos, s_robots_pos, s_player_pos));
			this.debug_content_ += "*2.1 Sample a "+sample_type+" state = \n" + this.DrawOccupancyMap 
				(s_evaders_pos, s_robots_pos, s_player_pos) + "\n";
			
//			Debug.LogAssertion (s_evaders_pos.Count == this.cur_evaders_pos_.Count);
//			Debug.LogAssertion (s_robots_pos.Count == this.cur_robots_pos_.Count);


			float best_s_utility_bellman_backed_up = -1.0f;
			List<SquareGrid.orientation> best_s_joint_action = new List<SquareGrid.orientation> ();
			float s_utility = 0.0f;
			List<float> s_feature_list = new List<float> ();
			float s_reward = 0.0f;
			this.ComputeOptimalAction (s_evaders_pos, s_robots_pos, s_player_pos, 
				feature_weight_list, ref best_s_utility_bellman_backed_up, ref best_s_joint_action,
				ref s_utility, ref s_feature_list, ref s_reward);


			// ------------------------------------------------------------------------
			// 4. Supervised learning
			// ------------------------------------------------------------------------
			// 4.1 gradient descent
			float update_magnitude = k_alpha_ * (best_s_utility_bellman_backed_up - s_utility);

			List<float> feature_weight_list_original = new List<float> ();
			for (int i = 0; i < feature_weight_list.Count; i++) {
				feature_weight_list_original.Add (feature_weight_list [i]);
			}
//			Debug.LogAssertion (feature_weight_list.Count == best_new_s_feature_list.Count);
			feature_weight_list_str = "Prev feature_weight_list:\n";
			foreach (float d in feature_weight_list) {
				feature_weight_list_str += d.ToString () + ", ";
			}
//			Debug.LogWarning (feature_weight_list_str);
			this.debug_content_ += feature_weight_list_str + "\n";
			
			for (int i = 0; i < feature_weight_list.Count; i++) {
				feature_weight_list [i] += update_magnitude * s_feature_list [i];
			}
			feature_weight_list_str = "Post feature_weight_list:\n";
			foreach (float d in feature_weight_list) {
				feature_weight_list_str += d.ToString () + ", ";
			}
//			Debug.LogWarning (feature_weight_list_str);
			this.debug_content_ += feature_weight_list_str + "\n";

//			Debug.LogAssertion (feature_weight_list.Count == feature_weight_list_original.Count);
			float max_diff = 0.0f;
			for (int i = 0; i < feature_weight_list.Count; i++) {
				float tmp = Mathf.Abs ((float)(feature_weight_list_original [i] - feature_weight_list [i]));
				if (tmp > max_diff) {
					max_diff = tmp;
				}
			}

			// ------------------------------------------------------------------------
			// 4. log
			// ------------------------------------------------------------------------
			// https://msdn.microsoft.com/en-us/library/aa735748(VS.71).aspx
			// Create an instance of StreamWriter to write text to a file.
			// The using statement also closes the StreamWriter.
			using (StreamWriter sw = new StreamWriter("./log.txt")) {
				sw.Write(this.debug_content_);
			}

			// ------------------------------------------------------------------------
			// 5. Convergence check
			// ------------------------------------------------------------------------
			// if delta < epsilon * (1. - gamma) * gamma
			float tolerance = 0.0f;
			if (k_gamma_ >= 1.0f) {
				// 10 ^ (-6)
				tolerance = k_epsilon_ * (1.0f - 0.01f) * 0.01f;
			} else {
				tolerance = k_epsilon_ * (1.0f - k_gamma_) * k_gamma_;
			}
			// for debugging
			tolerance = 0.1f;
			bool terminated = false;
			if (max_diff < tolerance) {
				converged = true;
				terminated = true;
			}
			counter += 1;
			if (counter >= 100) {
				terminated = true;
			}
			if (terminated == true) {
				break;
			}

//			break;
		}
		if (converged == true) {
			Debug.LogWarning ("CONVERGED!!!");
			this.debug_content_ += "CONVERGED!!!" + "\n";
		}

		float opt_cur_utility_bellman_backed_up = -1.0f;
		List<SquareGrid.orientation> opt_cur_joint_action = new List<SquareGrid.orientation> ();
		float opt_cur_utility = 0.0f;
		List<float> opt_cur_feature_list = new List<float> ();
		float opt_cur_reward = 0.0f;
		this.ComputeOptimalAction (this.cur_evaders_pos_, this.cur_robots_pos_, this.cur_player_pos_, 
			feature_weight_list, ref opt_cur_utility_bellman_backed_up, ref opt_cur_joint_action,
			ref opt_cur_utility, ref opt_cur_feature_list, ref opt_cur_reward);
		foreach (SquareGrid.orientation aa in opt_cur_joint_action) {
			Debug.LogWarning (aa);
		}
		List<SquareGrid.orientation> robots_opt_cur_action = new List<SquareGrid.orientation> ();
		for (int i = this.cur_evaders_pos_.Count; i < this.cur_evaders_pos_.Count 
			+ this.cur_robots_pos_.Count; i++) {
			robots_opt_cur_action.Add (opt_cur_joint_action [i]);
		}
		return robots_opt_cur_action;
	}
}

// http://stackoverflow.com/questions/273313/randomize-a-listt
public static class ThreadSafeRandom{
	[ThreadStatic] private static System.Random Local;
	public static System.Random ThisThreadsRandom {
		get {
			return Local ?? (Local = new System.Random (unchecked(Environment.TickCount * 31
			+ Thread.CurrentThread.ManagedThreadId)));
		}
	}
}
static class MyExtensions{
	public static void Shuffle<T>(this IList<T> list){
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = ThreadSafeRandom.ThisThreadsRandom.Next (n + 1);
			T value = list [k];
			list [k] = list [n];
			list [n] = value;
		}
	}
}