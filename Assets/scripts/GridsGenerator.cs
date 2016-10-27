using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridsGenerator : MonoBehaviour {
	const float grid_size=1f;

	public Vector3 start_pos;

	public GameObject grid_prefab;
	public GameObject player_prefab;

	private int[,] test=new int[3,4]{{0,0,0,0},{9,1,-1,0},{0,0,0,1}};
	private V2Int player_pos=new V2Int(0,0);	 

	Hashtable pos_to_obj;
	public SquareGrid g;


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
	 * 
	 * =================================================
	*/

	// Use this for initialization
	void Start () {
		g=Init_grid(test);
		Instantiate(player_prefab);
	}


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
		pos_to_obj=new Hashtable();
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

				//hash position to the object
				pos_to_obj[pos]=grid;
			}
		return sg;
	}

	


	Vector3 get_pos(V2Int pos){
		float _x=start_pos.x+pos._x*grid_size;
		float _y=start_pos.y+pos._y*grid_size;
		return new Vector3(_x,_y,0);
	}

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
