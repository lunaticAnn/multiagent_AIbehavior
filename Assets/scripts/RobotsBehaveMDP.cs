using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotsBehaveMDP : moving {

	//right now the robot behaves same as evador 
	//as they are all moving randomly.

	SquareGrid sg;

	// this only run once in the entire lifetime of the robot,
	// so there is no point in doing to_string, because the current_node is not updated
	void init_self (V2Int robot_pos) {
		sg=GridsGenerator.instance.g;
		current_node=sg.nodes.Find(n=>n.grid_position==robot_pos);
		if(current_node.occupied){Debug.LogWarning("Someone else is there.");}
		move_to_grid(sg,current_node);
//		Debug.LogWarning("Robot init itself");
	}

	void move(SquareGrid.orientation o) {
		grid_node target=sg.get_neighbour(current_node,o);
		move_to_grid(sg,target);
	}

	void mdp_move(int opt_action){
		move(SquareGrid.Four_dir[opt_action]);
	}
}