using UnityEngine;
using System.Collections;
using System;

//public class grid_node: MonoBehaviour{
public class grid_node: MonoBehaviour,IHeapItem<grid_node>{

	const float default_value=0f;

	public SquareGrid.grid_stat state;
	public V2Int grid_position;
	public bool occupied{get;set;}
    public bool not_ideal;

	public float current_value{get;set;} //current_value???
	public float last_value{get;set;}//last_value???
	Color origin_color;
	//added attributes for A*
	public int gCost;
	public int hCost;
	public grid_node parent;

	int heapItemIndex;

	public grid_node(V2Int pos, SquareGrid.grid_stat g=SquareGrid.grid_stat.empty){
		this.grid_position=pos;
		this.state=g;
		this.occupied = false;
        this.not_ideal = false; 
		current_value=default_value;
		last_value=default_value;
	}

	public int fCost {
		get {
			return gCost + hCost;
		}
	}

	public void set_color(Color c){
		origin_color=c;
	}


	/*=========================!!!!!=========================
	 *this is only for the initialization for the values
	 *for other calculation, use direct get
	========================================================*/

	public void set_value(float r){current_value=r;last_value=current_value;}

	//===============================visual Elements==============================
	void flash_me(){
		gameObject.GetComponent<SpriteRenderer>().color=Color.magenta;
		Invoke("norm_me",1f);
	}
	void norm_me(){
		gameObject.GetComponent<SpriteRenderer>().color=origin_color;
	}
	//===============================visual Elements==============================

	public int heap_index{
		get{ 
			return heapItemIndex;
		}

		set{ 
			heapItemIndex = value;
		}
	}

	public int CompareTo(grid_node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
		
}
