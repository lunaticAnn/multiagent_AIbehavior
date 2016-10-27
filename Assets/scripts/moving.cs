using UnityEngine;
using System.Collections;

public class moving : MonoBehaviour {
	

	/*===================================
	 * A moving object will move in the existing SquareGrid,
	 * It contains parameters:
	 * - current position,
	 *
	 * Also functions:
	 * moving(orientation)
	 * staying(staying in the current position)
	======================================*/
	//Constructor

	public moving(){  
		this.current_node=null;
		}

	//Current position for this moving object.
	public grid_node current_node{get;set;}


	protected bool valid_check(SquareGrid sg,grid_node target){
		if(target==null){
			return false;
		}
		grid_node new_node=sg.nodes.Find(n=>n.grid_position==target.grid_position);
		return sg.walkable(new_node);
	}

	protected void move_to_grid(SquareGrid sg,grid_node target){
		if(valid_check(sg,target)){
			grid_node pn=sg.nodes.Find(n=>n.grid_position==target.grid_position);
			transform.SetParent(pn.gameObject.transform);
			transform.localPosition=Vector3.zero+Vector3.back;
			current_node=target;
		}
		else 
			Debug.LogWarning("This position is not available in the grid.\n" +
				" Check the positions.");
	}



}
