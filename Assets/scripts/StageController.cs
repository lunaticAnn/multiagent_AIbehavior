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
	public int score_evador;
    public int movecount;
    public GameObject explosion;


    public static StageController instance=null;

	private bool stage_processed;
	private static Hashtable next=new Hashtable()
	{{stage.Player_moving,stage.Robot_moving},
	 {stage.Robot_moving,stage.Evador_moving},
	 {stage.Evador_moving,stage.Player_moving}};

	void Awake () {
		if(instance==null)instance=this;
		else Destroy(gameObject);
		current_stage=stage.Hang;
		score_evador=0;
        movecount = 0;
	}


	public void Stage_switch () {
		if(current_stage==stage.Hang){
            Robots_controller.instance.init();
			current_stage=stage.Player_moving;
			return;
		}
		current_stage=(stage)next[current_stage];
		stage_processed=false; //To control robot and evadors?
	}

	void Update(){

		//manually update the stages.
		if(Input.GetKeyDown(KeyCode.Space)){
			Stage_switch();
		}

		switch(current_stage){
		case stage.Player_moving:
                GameObject[] ev_instance = GameObject.FindGameObjectsWithTag("evador");
                if (ev_instance.Length == 0)
                {
                    NNprocessor.instance.print_results();
                    Application.LoadLevel(0);
                }
                GridsGenerator.instance.player_instance.SendMessage("move");
			return;

		case stage.Robot_moving:
			
			//this will be controlled all together
			//so I will change this one to a robot-controller.
			if(!stage_processed){
				stage_processed=true;
                    /*
                    GameObject[] robot_instance=GameObject.FindGameObjectsWithTag("robot");
                    foreach(GameObject rb in robot_instance)
                    rb.SendMessage("guess_and_move");

                    StartCoroutine("stage_yield");*/

                    GameObject[] evador_instance = GameObject.FindGameObjectsWithTag("evador");
                    evador_behave target=evador_instance[0].GetComponent<evador_behave>();
                    //Robots_controller.instance.update_state(Robots_controller.robot_state.block_exit);
                    Robots_controller.instance.update_state(Robots_controller.robot_state.corner_target,target);
                    //evador_instance[0].SendMessage("flash_me");
                }
                return;

		case stage.Evador_moving:
			
			//Add more protection method for time-consumption for computing
			if(!stage_processed){
				stage_processed=true;
				GameObject[] evador_instance=GameObject.FindGameObjectsWithTag("evador");
                    foreach (GameObject ev in evador_instance)
                    {
                        evador_behave eb = ev.GetComponent<evador_behave>();
                        if (cornered(eb)) {
                            Debug.Log("I am cornerd");
                            eb.SendMessage("explode", Color.red);
                            eb.current_node.occupied = false;
                            DestroyImmediate(eb.gameObject);
                        }
                    }
                

                    foreach (GameObject ev in evador_instance)
                    if(ev)
                        ev.SendMessage("IQ2_move");
                    //modify this to change method
                    
				StartCoroutine("stage_yield");
				}
			return;
		default:
			return;
		}
	}

    bool cornered(evador_behave eb)
    {
        /*=========================================================
		 * 1st: all neighbours are occupied;
		 * 2nd: they are occupied either by a player or a robot;
		=========================================================*/
        foreach (grid_node n in GridsGenerator.instance.g.my_neighbours(eb.current_node))
        {
            if (n.occupied == false) { Debug.LogWarning("some of my nbs is empty."); return false; }
            else if (n.gameObject.transform.GetChild(0).tag == "evador")
                return false;
        }
        return true;
    }


    IEnumerator stage_yield(){
		yield return new WaitForSeconds(0.1f);
		StageController.instance.Stage_switch();
	}

  

}
