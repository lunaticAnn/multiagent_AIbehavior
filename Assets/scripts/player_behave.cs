using UnityEngine;
using System.Collections;

public class player_behave : moving {
	SquareGrid sg;
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
	void Start () {
		sg=GameObject.Find("Main_controller").GetComponent<GridsGenerator>().g;
		//if this position of grid is walkable
		V2Int start_pos=new V2Int(0,0);
		current_node=sg.nodes.Find(n=>n.grid_position==start_pos);
		move_to_grid(sg,current_node);
		Debug.Log("Initialization finished.");

	}
		
	// Update is called once per frame
	void Update () {
		foreach(KeyCode k in keys){
			if(Input.GetKeyDown(k)){
				grid_node target=sg.get_neighbour(current_node,
											(SquareGrid.orientation)arrow_to_orientation[k]);
				move_to_grid(sg,target);
			}
		}
	}


}
