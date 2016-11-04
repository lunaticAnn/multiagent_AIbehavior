using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class evador_behave : moving {
	SquareGrid sg;
	public GameObject explosion;

	private List<GameObject> pursuers;

	void init_self (V2Int evador_pos) {
		sg=GridsGenerator.instance.g;
		current_node=sg.nodes.Find(n=>n.grid_position==evador_pos);
		if(current_node.occupied){Debug.LogWarning("Someone else is there.");}
		move_to_grid(sg,current_node);
		pursuers=new List<GameObject>();

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


	void IQ1_move(){
		/*this relatively clever movement is examine the whole grid 
		 * and use A* to move towards the exit as well as keep distance 
		 * with all the evadors;
		 * 
		 * the Heuristic function that I am using is:
		 * H(n)=Manhattan(n.grid_pos,exit)-Min(Manhattan(n.grid_pos,persuer)[])
		*/
		int threat=0;
		grid_node candidate=null;
		if(cornered()){
			current_node.occupied=false;
			explode(Color.red);
			DestroyImmediate(gameObject);
		}

		foreach(grid_node n in sg.my_neighbours(current_node)){			
			if(sg.walkable(n)&&(!n.occupied)){
				if (candidate==null){
					candidate=n;
					threat=evaluate_evador(n);
				}
				else{
					candidate=evaluate_evador(n)>threat?n:candidate;
				}
			}
		}
		if(candidate!=null){
			move_to_grid(sg,candidate);
			if(candidate.state==SquareGrid.grid_stat.exit){
				StageController.instance.score_evador+=1;
				//remove self from board
				current_node.occupied=false;
				explode(Color.green);
				DestroyImmediate(gameObject);}
		}
		
	}

	int evaluate_evador(grid_node n){
		//if uninitialized, initialize
		if(pursuers.Count==0){
			GameObject[] robots=GameObject.FindGameObjectsWithTag("robot");
			foreach (GameObject r in robots)
				pursuers.Add(r);
			pursuers.Add(GameObject.FindGameObjectWithTag("Player"));
		}

		//initialized, evaluate according to them
		int result=0;
		int[] threat_manhattan=new int[pursuers.Count];
		for(int i=0;i<pursuers.Count;i++){
			V2Int opponent_pos=pursuers[i].GetComponent<moving>().current_node.grid_position;
			threat_manhattan[i]=Manhattan(opponent_pos,current_node.grid_position);
		}

		V2Int exit_pos=sg.nodes.Find(x=>x.state==SquareGrid.grid_stat.exit).grid_position;

		return Manhattan(exit_pos,current_node.grid_position)-Mathf.Min(threat_manhattan);
	}


	int Manhattan(V2Int posA,V2Int posB){
		return Mathf.Abs(posA._x-posB._x)+Mathf.Abs(posA._y-posB._y);
	}


	void flash_me(){
		gameObject.GetComponent<SpriteRenderer>().color=Color.cyan;
		Invoke("norm_me",1f);
	}
	void norm_me(){
		gameObject.GetComponent<SpriteRenderer>().color=Color.red;
	}


}
