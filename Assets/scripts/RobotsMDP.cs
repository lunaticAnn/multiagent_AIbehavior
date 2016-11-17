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
	public int k_num_col_;
	public int k_num_row_;

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
	private Dictionary<V2Int,char> terrain_map_;
	public Dictionary<V2Int,char> GetTerrainMap(){return this.terrain_map_;}

	private List<V2Int> cur_evaders_pos_;
	private List<V2Int> cur_robots_pos_;
	private V2Int cur_player_pos_;
	//	List<V2Int> player_history_nodes = new List<V2Int>();

	// <state,value>
	private Dictionary<RobotsMDPState,double> state_value_map_;
	// used for iterate while changing state_value_map_
	// http://answers.unity3d.com/questions/409835/out-of-sync-error-when-iterating-over-a-dictionary.html
	List<RobotsMDPState> state_value_map_keys_;
	RobotsMDPState cur_s;

	List<List<SquareGrid.orientation>> group_action_list_;
	// for each agent, we have <pos, act, pos',Pr(pos,act,pos')>
	// then the transition model will return (state', Pr(state, actions of all agents, state')
	// use that to update utility
	// 1. pos is already known inside RobotsMDPState
	// 2. act:
	// {agent_index ~ up}
	Dictionary<int, SquareGrid.orientation> evaders_index_act_;
	Dictionary<int, SquareGrid.orientation> robots_index_act_;
	SquareGrid.orientation player_index_act_;
	// 3. pos' or outcome_act
	// every agent's 1 action has multiple possible outcome actions due to the noise
	// {agent_index ~ [up, down, left]}
	Dictionary<int, List<SquareGrid.orientation>> evaders_index_outcome_acts_;
	Dictionary<int, List<SquareGrid.orientation>> robots_index_outcome_acts_;
	List<SquareGrid.orientation> player_index_outcome_acts_;
	// 4. Pr(pos, act, pos')
	// {agent_index ~ [0.1, 0.2, 0.7]}
	Dictionary<int, List<double>> evaders_index_outcome_act_prs_;
	Dictionary<int, List<double>> robots_index_outcome_act_prs_;
	List<double> player_index_outcome_act_prs_;



	public void InitEverything(int[,] terrain_map){
		// init the terrain (obstacles)
		InitTerrainMap (terrain_map);
		// update the pos of each agent
		UpdateAgentsPos ();
		ComputeStateMap ();
		ComputeActionMap ();
	}

	public void InitTerrainMap(int[,] terrain_map) {
		cur_evaders_pos_ = new List<V2Int>();
		cur_robots_pos_ = new List<V2Int>();
		state_value_map_ = new Dictionary<RobotsMDPState,double>();
		this.k_num_row_ = terrain_map.GetLength (0);
		this.k_num_col_ = terrain_map.GetLength (1);
		this.terrain_map_ = new Dictionary<V2Int,char> ();
		for (int y = 0; y < this.k_num_row_; y++) {
			for (int x = 0; x < this.k_num_col_; x++) {
				if (terrain_map [y, x] == k_exit_num_) {
					this.terrain_map_ [new V2Int (x, y)] = k_exit_;
				} else if (terrain_map [y, x] == k_obstacle_num_) {
					this.terrain_map_ [new V2Int (x, y)] = k_obstacle_;
				} else if (terrain_map [y, x] == k_teleporter_num_) {
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
		this.cur_s = new RobotsMDPState (this.k_num_col_, this.k_num_row_, 
			this.cur_evaders_pos_, this.cur_robots_pos_, this.cur_player_pos_);
		Debug.LogWarning (this.cur_s);
	}

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

	void PermutationsWithRepetition(List<List<SquareGrid.orientation>> listList, 
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

	public void ComputeStateMap() {
		List<V2Int> possible_pos = new List<V2Int>();
		for (int y = 0; y < k_num_row_; y++) {
			for (int x = 0; x < k_num_col_; x++) {
				possible_pos.Add (new V2Int (x, y));
			}
		}
		V2Int[] possible_pos_arr = possible_pos.ToArray();
		List<List<V2Int>> possible_pos_arr_permutations = 
			ComputePermutations<V2Int>(possible_pos_arr, 
				this.cur_evaders_pos_.Count+this.cur_robots_pos_.Count+1);
		foreach (List<V2Int> pos_arr in possible_pos_arr_permutations) {
			bool good = true;
			foreach (V2Int pt in pos_arr) {
				if (this.terrain_map_ [pt] == k_obstacle_) {
					good = false;
					break;
				}
			}
			if (good == true) {
				List<V2Int> tmp_evaders_pos = new List<V2Int> ();
				List<V2Int> tmp_robots_pos = new List<V2Int> ();
				V2Int tmp_player_pos = new V2Int();
				for (int i = 0; i < cur_evaders_pos_.Count; i++) {
					tmp_evaders_pos.Add (pos_arr [i]);
				}
				for (int i = cur_evaders_pos_.Count; 
					i < cur_evaders_pos_.Count + cur_robots_pos_.Count; i++) {
					tmp_robots_pos.Add (pos_arr [i]);
				}
				tmp_player_pos = new V2Int(pos_arr [cur_evaders_pos_.Count + cur_robots_pos_.Count]);
				RobotsMDPState s = new RobotsMDPState (this.k_num_col_, this.k_num_row_, 
					tmp_evaders_pos, tmp_robots_pos, tmp_player_pos);
				this.state_value_map_.Add (s, 0.0);
			}
		}
		this.state_value_map_keys_ = new List<RobotsMDPState> (this.state_value_map_.Keys);
		Debug.LogWarning ("Number of state = "+this.state_value_map_.Count);
	}

	public void ComputeActionMap() {
		this.group_action_list_ = new List<List<SquareGrid.orientation>> ();
		List<SquareGrid.orientation> cur_list = new List<SquareGrid.orientation> ();
		List<SquareGrid.orientation> source = new List<SquareGrid.orientation> ();
		source.Add (SquareGrid.orientation.down);
		source.Add (SquareGrid.orientation.up);
		source.Add (SquareGrid.orientation.left);
		source.Add (SquareGrid.orientation.right);
		PermutationsWithRepetition (this.group_action_list_, cur_list, source, 
			this.cur_evaders_pos_.Count+this.cur_robots_pos_.Count+1);
		Debug.LogWarning ("Number of actions = "+this.group_action_list_.Count.ToString());
//		foreach (List<SquareGrid.orientation> act_arr in listList) {
//			foreach (SquareGrid.orientation act in act_arr) {
//				Debug.LogError (act);
//			}
//		}


		// -------------------------------------------
		// for each agent, we have <pos, act, pos',Pr(pos,act,pos')>
		// then the transition model will return (state', Pr(state, actions of all agents, state')
		// use that to update utility

		// 1. pos is already known inside RobotsMDPState
		// 2. act:
		// {agent_index ~ up}
		this.evaders_index_act_ = 
			new Dictionary<int, SquareGrid.orientation> ();
		this.robots_index_act_ = 
			new Dictionary<int, SquareGrid.orientation> ();
		this.player_index_act_ = new SquareGrid.orientation ();

		// 3. pos' or outcome_act
		// every agent's 1 action has multiple possible outcome actions due to the noise
		// {agent_index ~ [up, down, left]}
		this.evaders_index_outcome_acts_ = 
			new Dictionary<int, List<SquareGrid.orientation>> ();
		this.robots_index_outcome_acts_ = 
			new Dictionary<int, List<SquareGrid.orientation>> ();
		this.player_index_outcome_acts_ = 
			new List<SquareGrid.orientation> ();

		// 4. Pr(pos, act, pos')
		// {agent_index ~ [0.1, 0.2, 0.7]}
		this.evaders_index_outcome_act_prs_ = 
			new Dictionary<int, List<double>> ();
		this.robots_index_outcome_act_prs_ = 
			new Dictionary<int, List<double>> ();
		this.player_index_outcome_act_prs_ = 
			new List<double> ();
	}

	private void ValueIteration() {
		foreach (RobotsMDPState state in this.state_value_map_keys_) {
			double v = this.state_value_map_[state];

			foreach (List<SquareGrid.orientation> action_list in this.group_action_list_) {
				// for each agent, we have <pos, act, pos',Pr(pos,act,pos')>
				// then the transition model will return (state', Pr(state, actions of all agents, state')
				// use that to update utility

				// TODO: evader model
				// for now, evaders are not moving
				for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
					this.evaders_index_act_[i] = action_list [i];
					this.evaders_index_outcome_acts_ [i] = new List<SquareGrid.orientation> ();
					this.evaders_index_outcome_act_prs_ [i] = new List<double> ();
				}
				// robots are deterministic
				for (int i = this.cur_evaders_pos_.Count; 
					i < this.cur_evaders_pos_.Count + this.cur_robots_pos_.Count; i++) {
					this.robots_index_act_[i] = action_list [i];
					this.robots_index_outcome_acts_ [i] = new List<SquareGrid.orientation> ();
					this.robots_index_outcome_acts_[i].Add (action_list [i]);
					this.robots_index_outcome_act_prs_ [i] = new List<double> ();
					this.robots_index_outcome_act_prs_[i].Add (1.0);
				}
				// TODO: human model
				// for now, human is not moving
				this.player_index_act_ = action_list [cur_evaders_pos_.Count + cur_robots_pos_.Count];
				this.player_index_outcome_acts_ = new List<SquareGrid.orientation> ();
				this.player_index_outcome_acts_.Add (this.player_index_act_);
				this.player_index_outcome_act_prs_ = new List<double> ();
				this.player_index_outcome_act_prs_.Add (1.0);

				state.TransitionModel (
					this.evaders_index_act_,
					this.robots_index_act_,
					this.player_index_act_,
					this.evaders_index_outcome_acts_,
					this.robots_index_outcome_acts_,
					this.player_index_outcome_acts_,
					this.evaders_index_outcome_act_prs_,
					this.robots_index_outcome_act_prs_,
					this.player_index_outcome_act_prs_);
				break;
			}
			break;
		}
	}

	public List<int> ComputeOptActions() {
		ValueIteration ();
		List<int> opt_actions = new List<int> ();
		for (int i = 0; i < this.cur_robots_pos_.Count; i++) {
			//			opt_actions.Add (Random.Range (0, 4));
			opt_actions.Add (1);
		}
		return opt_actions;
	}


}
