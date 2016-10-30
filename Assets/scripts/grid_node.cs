using UnityEngine;
using System.Collections;
using System;

public class grid_node: MonoBehaviour{

	const float default_value=0f;

	public SquareGrid.grid_stat state;
	public V2Int grid_position;
	public bool occupied{get;set;}

	public float current_value{get;set;}
	public float last_value{get;set;}

	public grid_node(V2Int pos, SquareGrid.grid_stat g=SquareGrid.grid_stat.empty){
		this.grid_position=pos;
		this.state=g;
		this.occupied=false;
		current_value=default_value;
		last_value=default_value;
	}

	/*=========================!!!!!=========================
	 *this is only for the initialization for the values
	 *for other calculation, use direct get
	========================================================*/

	public void set_value(float r){current_value=r;last_value=current_value;}
			
}
