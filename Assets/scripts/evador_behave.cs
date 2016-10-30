using UnityEngine;
using System.Collections;

public class evador_behave : moving {
	SquareGrid sg;
	public GameObject explosion;

	void init_self (V2Int evador_pos) {
		sg=GridsGenerator.instance.g;
		current_node=sg.nodes.Find(n=>n.grid_position==evador_pos);
		if(current_node.occupied){Debug.LogWarning("Someone else is there.");}
		move_to_grid(sg,current_node);

	}


	bool cornered(){
		/*=========================================================
		 * 1st: all neighbours are occupied;
		 * 2nd: they are occupied either by a player or a robot;
		=========================================================*/
		foreach(grid_node n in sg.my_neighbours(current_node)){
			if(n.occupied==false) return false;
			else if(n.gameObject.transform.GetChild(0).tag=="evador")
	     		return false;
		}return true;
	}

	void move(SquareGrid.orientation o) {
		
		//before move: chech whether go die
		if(cornered()){
			current_node.occupied=false;
			explode(Color.red);
			DestroyImmediate(gameObject);
		}

		grid_node target=sg.get_neighbour(current_node,o);
		move_to_grid(sg,target);

		if(target!=null){
			if(target.state==SquareGrid.grid_stat.exit){
				StageController.instance.score_evador+=1;
				//remove self from board
				current_node.occupied=false;
				explode(Color.green);
				DestroyImmediate(gameObject);}
		}
	}


	void explode(Color c){
		GameObject exp=Instantiate(explosion);
		exp.transform.position=transform.position;
		exp.GetComponent<ParticleSystem>().startColor=c;
		Destroy(exp,1f);
	}


	//shuffle a direction,move
	void random_move(){
		int i=Random.Range(0,4);
		move(SquareGrid.Four_dir[i]);
	}
}
