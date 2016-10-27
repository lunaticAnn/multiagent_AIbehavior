using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SquareGrid  {
	
	/*================================
	 * A square-grid has boundarys, 
	 * Hashtable for deployment
	 * <converting the integer grids to assets>
	 * Hashtable for MDPs
	 * ===============================*/
	public int Width,Height;
	public List<grid_node> nodes;
	public enum grid_stat{obstacle,empty,teleporter,exit};
	public enum orientation{up,down,left,right};
	//mapping from grid to the states of grids
	public static Hashtable int_to_stat = new Hashtable()
	{
		{ -1, grid_stat.obstacle},
		{ 0,  grid_stat.empty },
		{ 1,  grid_stat.teleporter},
		{ 9,  grid_stat.exit}
	};

	private grid_node invalid_grid=new grid_node(new V2Int(-1,-1));

	private orientation[] Four_dir=new orientation[4]
								{orientation.up,
								orientation.down,
								orientation.left,
								orientation.right};
	
	//vector representative of orientations
	private Hashtable orient = new Hashtable()
	{
		{ orientation.up, new V2Int(0,1)},
		{ orientation.down, new V2Int(0,-1)},
		{ orientation.left, new V2Int(-1,0)},
		{ orientation.right, new V2Int(1,0)}
	};


		
	public SquareGrid(int width,int height){
		this.Width=width;
		this.Height=height;
		this.nodes=new List<grid_node>();

	}


	public bool walkable(grid_node n){
		bool x_in_range=(n.grid_position._x<this.Width)&&(n.grid_position._x>=0);
		bool y_in_range=(n.grid_position._y<this.Height)&&(n.grid_position._y>=0);
		bool not_obstacle=(n.state!=grid_stat.obstacle);
		return x_in_range&&y_in_range&&not_obstacle;
	}



	public grid_node get_neighbour(grid_node n,orientation o){
		V2Int nb_pos=n.grid_position+(V2Int)orient[o];
		grid_node nb=nodes.Find(such_node=>such_node.grid_position==nb_pos);
		if (nb!=null)
			if(walkable(nb))return nb;

		return null;
	}

	public List<grid_node> my_neighbours(grid_node n){
		List<grid_node> neighbour=new List<grid_node>();
		foreach(orientation o in Four_dir){
			grid_node nb=get_neighbour(n,o);
			if (nb!=null)neighbour.Add(nb);
			}
			return neighbour;
	}



}
