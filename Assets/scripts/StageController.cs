using UnityEngine;
using System.Collections;

public class StageController : MonoBehaviour {
	/*================================================================
	 * The stage controller controls the whole stage of the game.
	 * Start: Initialize all the assets.
	 * Player_moving: player moves one-step
	 * Robots_moving: robots moves accordingly
	 * Evador_moving: evadors moves accordingly
	==================================================================*/

	public enum stage{Player_moving, Robot_moving, Evador_moving, Hang};
	//A back up hang for dealing with situations
	public stage current_stage;
	public static StageController instance=null;

	private static Hashtable next=new Hashtable()
	{{stage.Player_moving,stage.Robot_moving},
	 {stage.Robot_moving,stage.Evador_moving},
	 {stage.Evador_moving,stage.Player_moving}};

	void Awake () {
		if(instance==null)instance=this;
		else Destroy(gameObject);
		current_stage=stage.Hang;
	}


	public void Stage_switch () {
		if(current_stage==stage.Hang){
			current_stage=stage.Player_moving;
			return;
		}
		current_stage=(stage)next[current_stage];
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Space)){
			Stage_switch();
		}
		switch(current_stage){
		case stage.Player_moving:
			GridsGenerator.instance.player_instance.SendMessage("move");
			return;
		case stage.Robot_moving:
			return;
		case stage.Evador_moving:
			GridsGenerator.instance.evador_instance.SendMessage("random_move");
			return;
		default:
			return;
		}

	}
}
