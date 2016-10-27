using UnityEngine;
using System.Collections;
using System;

public class grid_node: MonoBehaviour{

	public SquareGrid.grid_stat state;
	public V2Int grid_position;

	public grid_node(V2Int pos, SquareGrid.grid_stat g=SquareGrid.grid_stat.empty){
		this.grid_position=pos;
		this.state=g;
	}
		
}
