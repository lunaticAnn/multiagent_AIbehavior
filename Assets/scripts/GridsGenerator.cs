using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridsGenerator : MonoBehaviour {
	public static GridsGenerator instance=null;

	const float grid_size=1f;


	static Vector3 start_pos=Vector3.zero;

	public GameObject grid_prefab;
	public GameObject player_prefab;
	public GameObject robot_prefab;
	public GameObject evador_prefab;

	public GameObject player_instance;


	//=================editor convenience======================
	public Vector2 player_position;
	public Vector2[] evador_position;
	public Vector2[] robot_position;
	//================initial positions of all elements

	public SquareGrid g;

	private int[,] test=new int[4,5]{{0,0,0,0,0},{9,1,0,0,0},{0,0,0,1,0},{0,0,0,0,0}};

    /*private int[,] test = new int[6, 7] 
    { { 0, 0, 0, 0, 0, 0, 0 }, 
      { 9, 1, 0, 0, 0 , 0, 0 },
      { 0, 0, 0, 1, 0, 0, 0  }, 
      { 0, 0, 0, 0, 0 , 0, 0},
     { 0, 0, 0, 0, 0 , 0, 0},
     { 0, 0, 0, 0, 0 , 0, 0}};*/

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


    /*++++++++++++++++++++++++++++++++++++++++Reward Function++++++++++++++++++++++++++++++++++++++++++++++
	Call reward function as GridsGenerator.instance.get_grids(
		 V2Int current_pos:this is the position of object who is requiring MDP,
		 bool evador: true if it is evador, false if it is not.
	)
	I am still consfused about the reward function because it will be different
	if you CAN /CAN NOT stay at the same grid with someone else.
    ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
    const float r_exit_evador=100f;
	const float r_pursuer_evador=-110f;
	const float unmovable=-100f;
	const float r_next_to_evador=100f;

	public float[,] get_grids(V2Int current_pos,bool evador){
		//this will return the parsed grids from current game:
		//evador(true) will return the evadors rewards grid
		//evador(false) will return the rewards of pursuer
		float[,] Rg=new float[test.GetLength(0),test.GetLength(1)];
		//==================================Evador================================
		if(evador){
			//evador will update grid with exit and obstacles first;
			for(int i=0;i<test.GetLength(0);i++)
				for(int j=0;j<test.GetLength(1);j++){
					if(test[i,j]==9){Rg[i,j]=r_exit_evador;}
					else if(test[i,j]==-1){Rg[i,j]=unmovable;}
					else Rg[i,j]=0;
				    }
			
			foreach(grid_node n in g.nodes){
				if(n.occupied){
					//there is some one on the grid!
					V2Int pos=n.grid_position;
					if(current_pos==pos) continue;
					//if it is evador,just like obstacle
					if(n.gameObject.transform.GetChild(0).tag=="evador")
						Rg[pos._y,pos._x]=unmovable;
					else
						Rg[pos._y,pos._x]+=r_pursuer_evador;
				}
			}//end foreach
			return Rg;
		}//end evador
			
		//===================================Pursuer=================================
		for(int i=0;i<test.GetLength(0);i++)
			for(int j=0;j<test.GetLength(1);j++){
			 if(test[i,j]==-1){Rg[i,j]=unmovable;}
				else Rg[i,j]=0;
			}//update obstacles
		foreach(grid_node n in g.nodes){
			if(n.occupied){
				//we want to figure out where the evadors are;
					V2Int pos=n.grid_position;
					if(current_pos==pos) continue;
					if(n.gameObject.transform.GetChild(0).tag=="evador")
						Rg[pos._y,pos._x]=r_next_to_evador;
				}
				
			}
		return Rg;
	}

	void print_reward(float[,] grid){
		for(int i=0;i<grid.GetLength(0);i++){
			string s="";				
			for(int j=0;j<grid.GetLength(1);j++){
				s+=grid[i,j]+" ";
			} 
			Debug.Log(s);
		}
	}
	//++++++++++++++++++++++++++++++++++++++++Reward Function++++++++++++++++++++++++++++++++++++++++++++++




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

		foreach(Vector2 rp in robot_position){
		GameObject robot_single=Instantiate(robot_prefab);
		robot_single.SendMessage("init_self",convertor(rp));}

        
        StageController.instance.target_list = new Dictionary<int, evador_behave>();

        int tglistindex = 0;
        foreach (Vector2 vp in evador_position){
			GameObject evador_single=Instantiate(evador_prefab);
			evador_single.SendMessage("init_self",convertor(vp));
            StageController.instance.target_list[tglistindex] = evador_single.GetComponent<evador_behave>();
            tglistindex++;
        }	

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
            if (nb.occupied == false) 
			    nb.gameObject.GetComponent<SpriteRenderer>().color=Color.red;}
			yield return new WaitForSeconds(0.5f);
		 foreach(grid_node nb in nbs){
            if (nb.occupied == false)
                nb.gameObject.GetComponent<SpriteRenderer>().color=Color.yellow;}
			yield return new WaitForSeconds(0.5f);
		foreach(grid_node nb in nbs){
            if (nb.occupied == false)
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
				n.set_color(get_color(n.state));
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
	//consider change it to different sprites for initialization 

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
