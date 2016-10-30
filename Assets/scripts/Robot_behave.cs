using UnityEngine;
using System.Collections;

public class Robot_behave : moving {

	//right now the robot behaves same as evador 
	//as they are all moving randomly.

	SquareGrid sg;

	void init_self (V2Int robot_pos) {
		sg=GridsGenerator.instance.g;
		current_node=sg.nodes.Find(n=>n.grid_position==robot_pos);
		if(current_node.occupied){Debug.LogWarning("Someone else is there.");}
		move_to_grid(sg,current_node);

	}
		
	void move(SquareGrid.orientation o) {
		StageController.instance.Stage_switch();
		grid_node target=sg.get_neighbour(current_node,o);
		move_to_grid(sg,target);
	}

	//shuffle a direction,move
	void random_move(){
		int i=Random.Range(0,4);
		move(SquareGrid.Four_dir[i]);
	}
}
