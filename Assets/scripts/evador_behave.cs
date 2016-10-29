using UnityEngine;
using System.Collections;

public class evador_behave : moving {
	SquareGrid sg;

	// Use this for initialization
	bool player_there(V2Int evador_pos){
		V2Int player_pos=(GridsGenerator.instance.player_instance.GetComponent<player_behave>().current_node.grid_position);
		return evador_pos==player_pos;
	}


	void init_self (V2Int evador_pos) {
		if(!(player_there(evador_pos))){
			sg=GridsGenerator.instance.g;
			current_node=sg.nodes.Find(n=>n.grid_position==evador_pos);
			move_to_grid(sg,current_node);
			Debug.Log("Evador initialization finished."+evador_pos._x+","+evador_pos._y);
			}
		else{
			Debug.LogWarning("Player has been there.");
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
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
