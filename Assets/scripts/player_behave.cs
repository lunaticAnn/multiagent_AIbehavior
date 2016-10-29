using UnityEngine;
using System.Collections;

public class player_behave : moving {
	SquareGrid sg;
	private bool key_lock;

	static Hashtable arrow_to_orientation=new Hashtable(){
		{KeyCode.UpArrow, SquareGrid.orientation.up},
		{KeyCode.DownArrow, SquareGrid.orientation.down},
		{KeyCode.RightArrow, SquareGrid.orientation.right},
		{KeyCode.LeftArrow, SquareGrid.orientation.left}
	};
	private KeyCode[] keys=new KeyCode[4]{KeyCode.UpArrow,
										  KeyCode.DownArrow,
										  KeyCode.LeftArrow,
										  KeyCode.RightArrow};

	// Use this for initialization
	void Init (V2Int player_pos) {
		sg=GridsGenerator.instance.g;
		//if this position of grid is walkable
		V2Int start_pos=player_pos;
		current_node=sg.nodes.Find(n=>n.grid_position==start_pos);
		move_to_grid(sg,current_node);
		Debug.Log("Initialization finished.");

	}
		
	// Update is called once per frame
	void move() {
		key_lock=false;
		foreach(KeyCode k in keys){
			if(Input.GetKeyDown(k)){
				if(!key_lock){
				key_lock=true;
				grid_node target=sg.get_neighbour(current_node,
											(SquareGrid.orientation)arrow_to_orientation[k]);
				move_to_grid(sg,target);
				StageController.instance.Stage_switch();
				}
			}
		}
	}


}
