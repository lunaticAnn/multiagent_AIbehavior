using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		grid_node target=sg.get_neighbour(current_node,o);
		move_to_grid(sg,target);
	}

	//shuffle a direction,move
	void random_move(){
		int i=Random.Range(0,4);
		move(SquareGrid.Four_dir[i]);
	}

	const int time_to_decide=3;
	List<evador_behave> player_target=new List<evador_behave>();
	Dictionary<evador_behave,int> previous_step=new Dictionary<evador_behave,int>();
	Dictionary<evador_behave,int> current_step=new Dictionary<evador_behave,int>();
	Dictionary<evador_behave,int> rf_factor=new Dictionary<evador_behave,int>();



	void guess_and_move(){
		/*==========================Guess and move===========================
		 * According to the player's move, the robot will guess
		 * which evador the player is targeting, and also move towards
		 * that evador.
		 * Right now I suggest players are targeting the Evador _e when:
		 * - they keep shortening the manhattan distance between _e and themselves;
		 * - if there are multiple, choose the closest one.
		 * - I suppose evador are always running to exit,from the robot's propose.
		=========================Guess and move==============================*/

		if(player_target.Count==0)
			create_target_list();
		refresh_target_list();
		evador_behave target_evador=Find_target(player_target);
		grid_node exit_node=sg.nodes.Find(x=>x.state==SquareGrid.grid_stat.exit);
		grid_node target_grid=next_closest_to_target(target_evador.current_node,exit_node);
		target_grid.SendMessage("flash_me");
		grid_node candidate=next_closest_to_target(current_node,target_grid);
		if(candidate!=null)
			move_to_grid(sg,candidate);

	}

	void create_target_list(){
		player_target.Clear();
		GameObject[] target_evadors=GameObject.FindGameObjectsWithTag("evador");
		foreach(GameObject _e in target_evadors){
			evador_behave eb=_e.GetComponent<evador_behave>();
			player_target.Add(eb);		
			previous_step[eb]=0;
			current_step[eb]=0;
			rf_factor[eb]=1;
		}
	}

	void refresh_target_list(){
		V2Int player_pos=GameObject.FindGameObjectWithTag("Player").GetComponent<moving>().current_node.grid_position;
		foreach(evador_behave _e in player_target){
			previous_step[_e]=current_step[_e];
			current_step[_e]=Manhattan(player_pos,_e.current_node.grid_position);
		}
	}

	evador_behave Find_target(List<evador_behave> t){
		int min_reward=0;
		int reward;

		evador_behave target_now=null;
		foreach(evador_behave eb in t){
			reward=0;
			if((int)previous_step[eb]>=current_step[eb]){
				reward=-30*rf_factor[eb];
				rf_factor[eb]+=1;

			}
			else{rf_factor[eb]=1;}//clear target threat;
			reward+=Manhattan(current_node.grid_position,eb.current_node.grid_position);
			if(target_now==null){
				target_now=eb;
				min_reward=reward;
			}
			else{
				target_now=reward<min_reward?eb:target_now;
			}
		}
		target_now.SendMessage("flash_me");
		return target_now;
	}

	int Manhattan(V2Int posA,V2Int posB){
		return Mathf.Abs(posA._x-posB._x)+Mathf.Abs(posA._y-posB._y);
	}

	grid_node next_closest_to_target(grid_node myself, grid_node target){
		int min_dist=0;
		grid_node candidate=null;
		foreach(grid_node n in sg.my_neighbours(myself)){
			if(candidate==null){
				candidate=n;
				min_dist=Manhattan(n.grid_position,target.grid_position);
			}
			else{
				candidate=Manhattan(n.grid_position,target.grid_position)<min_dist?n:candidate;
			}
		}
		return candidate;
	}


}

/*==========================Ideas of MDP_v2=======================
     * For v2, the goal of the robots is to minimize the reward of the 
     * evador who has the largest reward.Meanwhile, instead of using 
     * player, we will design a robot using MDP with constant probabitlity.
     * We call it as semi-human robot. 
     * Therefore, another goal for the robot is to maximize the reward 
	 * of the semi-human robot. (how to learn maximize other's reward?).
     ================================================================*/
