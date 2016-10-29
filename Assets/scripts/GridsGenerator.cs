using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridsGenerator : MonoBehaviour {
	public static GridsGenerator instance=null;

	const float grid_size=1f;

	public Vector3 start_pos;

	public GameObject grid_prefab;
	public GameObject player_prefab;
	public GameObject evador_prefab;
	public GameObject player_instance;
	public GameObject evador_instance;

	//=================editor convenience======================
	public Vector2 player_position;
	public Vector2 evador_position;

	public SquareGrid g;

	private int[,] test=new int[4,5]{{0,0,0,0,0},{9,1,-1,0,0},{-1,0,0,1,0},{0,0,0,0,0}};	 



	/*=================================================
	 * Inputs will be given in the form of grids that is:
	 * [[0, 0, 0, 0],
	 *  [9, 1,-1, 0],
	 *  [0, 0, 0, 1]]
	 * -1-obstacle
	 *  1-Teleporter
	 *  9-Exit
	 * 
	 * Player_pos V2Int
	 * AI_pos V2Int[nums]
	 * Envador_pos V2Int[nums]
	 * =================================================
	*/
	V2Int convertor(Vector2 src){
		return new V2Int(Mathf.RoundToInt(src.x),Mathf.RoundToInt(src.y)); 
	}



	void Awake () {
		if(instance==null)instance=this;
		else Destroy(gameObject);

	}

	//after all the initializations, tell stage manager I am ready.
	void Start () {
		g=Init_grid(test);
		//===================set camera position======================
		float length_w=test.GetLength(0)*grid_size/2;
		float length_h=test.GetLength(1)*grid_size/2;
		Camera.main.transform.position=new Vector3(length_w,length_h-1f,-10f);
		Camera.main.orthographicSize=Mathf.Ceil(Mathf.Max(length_h,length_w));
		//===================set camera position======================
		player_instance=Instantiate(player_prefab);
		player_instance.SendMessage("Init",convertor(player_position));
		evador_instance=Instantiate(evador_prefab);
		evador_instance.SendMessage("init_self",convertor(evador_position));
		StageController.instance.Stage_switch();
	}

	//This is also bullshit. 
	void Update(){
		if(Input.GetMouseButtonDown(0)){
			Ray r=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(r,out hit)){
				grid_node n=hit.collider.gameObject.GetComponent<grid_node>();
				List<grid_node> nbs=g.my_neighbours(n);
				IEnumerator c=flash_them(nbs);
				StartCoroutine(c);
			}
		}

	}


	IEnumerator flash_them(List<grid_node> nbs){
		foreach(grid_node nb in nbs){
			nb.gameObject.GetComponent<SpriteRenderer>().color=Color.red;}
			yield return new WaitForSeconds(0.5f);
		 foreach(grid_node nb in nbs){
			nb.gameObject.GetComponent<SpriteRenderer>().color=Color.yellow;}
			yield return new WaitForSeconds(0.5f);
		foreach(grid_node nb in nbs){
			nb.gameObject.gameObject.GetComponent<SpriteRenderer>().color=get_color(nb.state);}

	}



	public SquareGrid Init_grid(int[,] int_grid){
		/*======================================
		 * Initialize the grid with a integer matrix.
		========================================*/

		SquareGrid sg=new SquareGrid(int_grid.GetLength(1),int_grid.GetLength(0));

		for(int coord_x=0;coord_x<sg.Width;coord_x++)
			for(int coord_y=0;coord_y<sg.Height;coord_y++){
				V2Int pos=new V2Int(coord_x,coord_y);

				//instantiate new prefab
				GameObject grid=Instantiate(grid_prefab);
				grid.transform.position=get_pos(pos);
				grid.transform.SetParent(transform);

				//update grid_node according to input grid
				grid_node n=grid.GetComponent<grid_node>();
				n.grid_position=pos;
				n.state=(SquareGrid.grid_stat)SquareGrid.int_to_stat[int_grid[coord_y,coord_x]];

				//set color
				grid.GetComponent<SpriteRenderer>().color=get_color(n.state);

				//update node list for the grid
				sg.nodes.Add(n);

			}
		return sg;
	}

	


	Vector3 get_pos(V2Int pos){
		float _x=start_pos.x+pos._x*grid_size;
		float _y=start_pos.y+pos._y*grid_size;
		return new Vector3(_x,_y,0);
	}

	//this is bullshit,just for beauty
	Color get_color(SquareGrid.grid_stat state){
		switch(state){
		case SquareGrid.grid_stat.empty:
			return Color.white;
		case SquareGrid.grid_stat.obstacle:
			return Color.black;
		case SquareGrid.grid_stat.teleporter:
			return Color.blue;
		case SquareGrid.grid_stat.exit:
			return Color.green;
		default:
			return Color.black;
		}
	}
}
