using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


// XXX: robots and evaders from prev round, player from the new round
// This state = an occupancy map
public class RobotsMDPState
{
	private int num_row_;
	private int num_col_;
	private List<V2Int> cur_evaders_pos_;
	private List<V2Int> cur_robots_pos_;
	private V2Int cur_player_pos_;
	// XXX: this is in [(x,y)] or [(col,row)]
	private Dictionary<V2Int,char> occupancy_map_;

	public int GetNumRow(){return num_row_;}
	public int GetNumCol(){return num_col_;}
	public List<V2Int> GetCurEvadersPos(){return cur_evaders_pos_;}
	public List<V2Int> GetCurRobotsPos(){return cur_robots_pos_;}
	public V2Int GetCurPlayerPos(){return cur_player_pos_;}
	public Dictionary<V2Int,char> GetOccupancyMap(){return this.occupancy_map_;}

	public RobotsMDPState (int num_row, int num_col, 
		List<V2Int> cur_evaders_pos, List<V2Int> cur_robots_pos, V2Int cur_player_pos) {
		this.num_row_ = num_row;
		this.num_col_ = num_col;

		cur_evaders_pos_ = new List<V2Int>();
		for (int i = 0; i < cur_evaders_pos.Count; i++) {
			this.cur_evaders_pos_.Add(new V2Int(cur_evaders_pos[i]));
		}
		cur_robots_pos_ = new List<V2Int>();
		for (int i = 0; i < cur_robots_pos.Count; i++) {
			this.cur_robots_pos_.Add(new V2Int(cur_robots_pos[i]));
		}
		this.cur_player_pos_ = new V2Int (cur_player_pos);

		this.occupancy_map_ = new Dictionary<V2Int,char>();
		foreach (V2Int x in this.cur_evaders_pos_){this.occupancy_map_ [x] = 'e';}
		foreach (V2Int x in this.cur_robots_pos_){this.occupancy_map_ [x] = 'r';}
		this.occupancy_map_ [this.cur_player_pos_] = 'p';
		for (int y = 0; y < this.num_row_; y++) {
			for (int x = 0; x < this.num_col_; x++) {
				if (!this.occupancy_map_.ContainsKey (new V2Int (x, y))) {
					this.occupancy_map_ [new V2Int (x, y)] = '.';
				}
			}
		}
	}

	// copy constructor
	public RobotsMDPState (RobotsMDPState source) {
		this.num_row_ = source.GetNumRow();
		this.num_col_ = source.GetNumCol();

		cur_evaders_pos_ = new List<V2Int>();
		List<V2Int> tmp_es = source.GetCurEvadersPos ();
		for (int i = 0; i < tmp_es.Count; i++) {
			this.cur_evaders_pos_.Add(new V2Int(tmp_es[i]));
		}
		cur_robots_pos_ = new List<V2Int>();
		List<V2Int> tmp_rs = source.GetCurRobotsPos ();
		for (int i = 0; i < tmp_rs.Count; i++) {
			this.cur_robots_pos_.Add(new V2Int(tmp_rs[i]));
		}
		this.cur_player_pos_ = new V2Int (source.GetCurPlayerPos());
		this.occupancy_map_ = new Dictionary<V2Int,char>(source.GetOccupancyMap());
	}

	public override string ToString(){
		string ret_val = "state-------------------\n";
//		ret_val += this.cur_evaders_pos_.Count+" evaders:\n";
//		for (int i = 0; i < this.cur_evaders_pos_.Count; i++) {
//			ret_val += this.cur_evaders_pos_ [i].ToString () + " ";
//		}
//		ret_val += "\n"+this.cur_robots_pos_.Count+" robots:\n";
//		for (int i = 0; i < this.cur_robots_pos_.Count; i++) {
//			ret_val += this.cur_robots_pos_ [i].ToString () + "; ";
//		}
//		ret_val += "\nplayer: " + this.cur_player_pos_;
		for (int y = this.num_row_-1; y >= 0; y--) {
			string row = "";
			for (int x = 0; x < this.num_col_; x++) {
				row += this.occupancy_map_ [new V2Int (x,y)];
			}
			ret_val += row+"\n";
		}
		return ret_val;
	}













}



