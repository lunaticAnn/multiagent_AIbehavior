using UnityEngine;
using System.Collections;

public class evador_behave : moving {
	SquareGrid sg;

	// Use this for initialization


	void init_self (V2Int evador_pos) {
		sg=GridsGenerator.instance.g;
		current_node=sg.nodes.Find(n=>n.grid_position==evador_pos);
		if(current_node.occupied){Debug.LogWarning("Someone else is there.");}
		move_to_grid(sg,current_node);

	}
	
	// Update is called once per frame
	void move(SquareGrid.orientation o) {
		grid_node target=sg.get_neighbour(current_node,o);
		move_to_grid(sg,target);
	}

	//shuffle a direction,move
	void random_move(){
		int i=Random.Range(0,4);
		move(SquareGrid.Four_dir[i]);
	}
}
