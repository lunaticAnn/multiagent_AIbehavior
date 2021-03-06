﻿using UnityEngine;
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




	//================================Visial Elements=====================================
	void explode(Color c){
		GameObject exp=Instantiate(explosion);
		exp.transform.position=transform.position;
		exp.GetComponent<ParticleSystem>().startColor=c;
		Destroy(exp,1f);
	}

	void flash_me(){
		gameObject.GetComponent<SpriteRenderer>().color=Color.cyan;
		Invoke("norm_me",1f);
	}
	void norm_me(){
		gameObject.GetComponent<SpriteRenderer>().color=Color.red;
	}
	//================================Visial Elements=====================================


	//===============================Random move=====================================
	void move(SquareGrid.orientation o) {
		
		//before move: chech whether go die
	

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
		

	//shuffle a direction,move
	void random_move(){
		int i=Random.Range(0,4);
		move(SquareGrid.Four_dir[i]);
	}
	//===============================Random move=====================================

	//===============================Simple Greedy Move=========================

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

	//===============================A* Search=========================

	void IQ2_move(){
		grid_node candidate=null;

            /* set next move with A* pathfinding and avoid pursuers*/
            candidate = evador_next(sg, current_node);
            if (candidate.state == SquareGrid.grid_stat.exit)
            {
                StageController.instance.score_evador += 1;
                //remove self from board
                move_to_grid(sg, candidate);
                current_node.occupied = false;
                explode(Color.green);
                DestroyImmediate(gameObject);
            }
            else
            {
                move_to_grid(sg, candidate);
            }
        
	}

	grid_node astar(SquareGrid sg,grid_node startNode){
		//* the Heuristic function that I am using is:
		//* Manhattan(n.grid_pos,exit)
		grid_node candidate=null;
		grid_node targetNode = sg.nodes.Find(n=>n.state==SquareGrid.grid_stat.exit); //Is that right??
		Heap<grid_node> openSet = new Heap<grid_node>(sg.Width*sg.Height); //Is that right??
		//Heap<grid_node> openSet = new Heap<grid_node>(20);
		//HashSet<grid_node> closedSet = new HashSet<grid_node>();
		List<grid_node> closedSet = new List<grid_node>();
		openSet.Add(startNode);

		while(openSet.count > 0){
			grid_node currentNode = openSet.remove_first();
			closedSet.Add(currentNode);

			if(currentNode == targetNode){
				//RetracePath(startNode,targetNode);
				candidate=closedSet[1]; //the next step??
				break;
			}

			foreach(grid_node n in sg.my_neighbours(currentNode)){
				if(!sg.walkable(n) || closedSet.Contains(n)|| n.occupied){
					continue;
				}

				int newMovementCostToNeighbour=currentNode.gCost + Manhattan(currentNode.grid_position,n.grid_position);
				if(newMovementCostToNeighbour<n.gCost || !openSet.Contains(n)){
					n.gCost = newMovementCostToNeighbour; 
					n.hCost = Manhattan(n.grid_position,targetNode.grid_position);
					n.parent = currentNode;

					if(!openSet.Contains(n)){
						openSet.Add(n);
					}
				}
			}

		}
		candidate=closedSet.Count>1?closedSet[1]:startNode; 
		return candidate;

	}

	grid_node evador_next(SquareGrid sg,grid_node current_node){
		grid_node candidate = astar(sg,current_node);
		Debug.Log (candidate == null);
		//if uninitialized, initialize
		if(pursuers.Count==0){
			GameObject[] robots=GameObject.FindGameObjectsWithTag("robot");
			foreach (GameObject r in robots)
				pursuers.Add(r);
			pursuers.Add(GameObject.FindGameObjectWithTag("Player"));
		}
        //initialized, evaluate according to them

       /* do
        {
          foreach (grid_node n in dangerous_spot())
            if (sg.walkable(n) && (!n.not_ideal))
            {
                if (candidate.grid_position == n.grid_position && candidate != current_node)
                    //If the possible next node of each pursuers overlaps the candidate,
                    //see the candidate node as an obstacle
                    candidate.not_ideal = true;//is that right??
                                       //recursive call, it will move since there are only three pursuers
                candidate = astar(sg, current_node);
                break;  
            }
                
        } while (candidate.not_ideal);

        foreach (grid_node n in sg.my_neighbours(current_node)) {
            n.not_ideal = false;
        }*/

        return candidate;
	}


    List<grid_node> dangerous_spot(){
        List<grid_node> nodes = new List<grid_node>();
        for (int i = 0; i < pursuers.Count; i++)
            foreach (grid_node n in sg.my_neighbours(pursuers[i].GetComponent<moving>().current_node)) {
                if (sg.walkable(n)) {
                    nodes.Add(n);
                }
            }
        return nodes;
    }
}
