using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

// XXX: robots and evaders from prev round, player from the new round
// This state = an occupancy map implemented using a dictionary
public class RobotsMDPState
{
	private int k_num_row_;
	private int k_num_col_;
	public int GetNumRow(){return this.k_num_row_;}
	public int GetNumCol(){return this.k_num_col_;}

	public const char k_obstacle_ = RobotsMDP.k_obstacle_;
	public const char k_teleporter_ = RobotsMDP.k_teleporter_;
	public const char k_exit_ = RobotsMDP.k_exit_;
	public const char k_unoccupied_ = RobotsMDP.k_unoccupied_;
	public const char k_robot_ = RobotsMDP.k_robot_;
	public const char k_evader_ = RobotsMDP.k_evader_;
	public const char k_player_ = RobotsMDP.k_player_;

	// how many evaders are cornered in this configuration
	private int num_cornered_evader_;
	private int num_exited_evader_;

	public const double k_reward_per_cornered = 1000.0;
	public const double k_reward_per_exit = -2000.0;
	public const double k_reward_per_action = -100.0;
	// (s,a,s') => we use s' to decide the reward
	private double reward_;
	public double GetReward(){return this.reward_;}


	// XXX: this is in [(x,y)] or [(col,row)]
	private Dictionary<V2Int,char> occupancy_map_;
	public Dictionary<V2Int,char> GetOccupancyMap(){return this.occupancy_map_;}
	private Dictionary<V2Int,char> terrain_map_;

	private List<V2Int> state_evaders_pos_;
	private List<V2Int> state_robots_pos_;
	private V2Int state_player_pos_;

	public RobotsMDPState (int k_num_col, int k_num_row, List<V2Int> state_evaders_pos,
		List<V2Int> state_robots_pos, V2Int state_player_pos) {
		this.state_evaders_pos_ = new List<V2Int> ();
		foreach (V2Int x in state_evaders_pos) {
			this.state_evaders_pos_.Add (x);
		}
		this.state_robots_pos_ = new List<V2Int> ();
		foreach (V2Int x in state_robots_pos) {
			this.state_robots_pos_.Add (x);
		}
		this.state_player_pos_ = new V2Int (state_player_pos);

		terrain_map_ = RobotsMDP.instance.GetTerrainMap ();
		this.k_num_row_ = k_num_row;
		this.k_num_col_ = k_num_col;

		this.occupancy_map_ = new Dictionary<V2Int,char>();
		foreach (V2Int x in this.state_evaders_pos_){this.occupancy_map_ [x] = k_evader_;}
		foreach (V2Int x in this.state_robots_pos_){this.occupancy_map_ [x] = k_robot_;}
		this.occupancy_map_ [this.state_player_pos_] = k_player_;
		for (int y = 0; y < this.k_num_row_; y++) {
			for (int x = 0; x < this.k_num_col_; x++) {
				V2Int pos = new V2Int (x, y);
				if (!this.occupancy_map_.ContainsKey (pos)) {
					this.occupancy_map_ [pos] = '.';
				}
			}
		}
		CountCorneredEvader ();
		CountExitedEvader ();
		ComputeReward ();
	}

	// copy constructor
	public RobotsMDPState (RobotsMDPState source) {
		this.k_num_row_ = source.GetNumRow();
		this.k_num_col_ = source.GetNumCol();
		this.occupancy_map_ = new Dictionary<V2Int,char>(source.GetOccupancyMap());
	}

	private void ComputeReward(){
		this.reward_ = this.num_exited_evader_ * k_reward_per_exit +
			this.num_cornered_evader_ * k_reward_per_cornered + k_reward_per_action;
	}

	// check if exiting successfully
	private void CountExitedEvader() {
		int counter = 0;
		foreach (KeyValuePair<V2Int, char> kvp in this.occupancy_map_) {
			if (kvp.Value == k_evader_ && terrain_map_[kvp.Key] == k_exit_) {
				counter += 1;
			}
		}
		this.num_exited_evader_ = counter;
	}

	// check if cornered
	private void CountCorneredEvader() {
		/*=========================================================
		 * 1st: all neighbours of evaders are occupied;
		 * 2nd: either by a player or a robot;
		=========================================================*/
		int counter = 0;
		foreach (KeyValuePair<V2Int, char> kvp in this.occupancy_map_) {
			if (kvp.Value == k_evader_) {
				bool cornered = true;
				List<V2Int> neighbours_pos = this.NeighborsPos (kvp.Key);
				foreach (V2Int x in neighbours_pos) {
					if (this.occupancy_map_.ContainsKey (x)) {
						if (this.occupancy_map_ [x] == '.') {
							cornered = false;
							break;
						}
					}
				}
				if (cornered == true)
					counter += 1;
			}
		}
		this.num_cornered_evader_ = counter;
	}

	//All neighbours
	private List<V2Int> NeighborsPos(V2Int x){
		List<V2Int> neighbours_pos = new List<V2Int>();
		foreach(SquareGrid.orientation o in SquareGrid.Four_dir){
			V2Int oo = x+(V2Int)SquareGrid.orient [o];
			//			Debug.LogWarning ("oo="+oo);
			if (oo._x >= 0 || oo._x < this.k_num_col_ || oo._y >= 0 || oo._y < this.k_num_row_) {
				neighbours_pos.Add (oo);
			}
		}
		return neighbours_pos;
	}

	public override string ToString(){
		string ret_val = "state-------------------\n";
//		ret_val += this.state_evaders_pos_.Count+" evaders:\n";
//		for (int i = 0; i < this.state_evaders_pos_.Count; i++) {
//			ret_val += this.state_evaders_pos_ [i].ToString () + " ";
//		}
//		ret_val += "\n"+this.state_robots_pos_.Count+" robots:\n";
//		for (int i = 0; i < this.state_robots_pos_.Count; i++) {
//			ret_val += this.state_robots_pos_ [i].ToString () + "; ";
//		}
//		ret_val += "\nplayer: " + this.state_player_pos_;
		for (int y = this.k_num_row_-1; y >= 0; y--) {
			string row = "";
			for (int x = 0; x < this.k_num_col_; x++) {
				row += this.occupancy_map_ [new V2Int (x,y)];
			}
			ret_val += row+"\n";
		}
		ret_val += "num_cornered_evader_=" + this.num_cornered_evader_;
		ret_val += "\num_exited_evader_=" + this.num_exited_evader_;
		ret_val += "\nreward_=" + this.reward_;
		return ret_val;
	}

	public void TransitionModel(
		Dictionary<int, SquareGrid.orientation> evaders_index_act,
		Dictionary<int, SquareGrid.orientation> robots_index_act,
		SquareGrid.orientation player_index_act,
		Dictionary<int, List<SquareGrid.orientation>> evaders_index_outcome_acts,
		Dictionary<int, List<SquareGrid.orientation>> robots_index_outcome_acts,
		List<SquareGrid.orientation> player_index_outcome_acts,
		Dictionary<int, List<double>> evaders_index_outcome_act_prs,
		Dictionary<int, List<double>> robots_index_outcome_act_prs, 
		List<double> player_index_outcome_act_prs) {
		Debug.LogWarning ("1111111111111");
		IList<char> chars = "abc".ToList();
		List<string> allCombinations = new List<string>();
		for (int i = 1; i <= chars.Count; i++)
		{
			var combis = new Facet.Combinatorics.Combinations<char>(
				chars, i, Facet.Combinatorics.GenerateOption.WithRepetition);
			foreach (IList<char> cs in combis) {
				string s = "";
				foreach (char c in cs) {
					s += c;
				}
				allCombinations.Add (s);
			}
//			allCombinations.AddRange(combis.Select(c => string.Join("", c)));
		}
		foreach (var combi in allCombinations)
			Debug.LogWarning (combi);

//		Application.Quit();
		System.Environment.Exit(0);

	}











}



