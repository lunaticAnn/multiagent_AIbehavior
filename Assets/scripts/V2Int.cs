using UnityEngine;
using System.Collections;

public class V2Int  {

	/*=============================
	 * Basic Vector2(int) Class 
	 * ============================*/
		public int _x;
		public int _y;

		public V2Int(int x,int y){
			this._x=x;
			this._y=y;
		}

		public static V2Int operator +(V2Int lh,V2Int rh){
			return new V2Int(lh._x+rh._x,lh._y+rh._y);
		}

	    public static bool operator ==(V2Int lh,V2Int rh){
			return (lh._x==rh._x)&&(lh._y==rh._y);
		}
		
		public static bool operator !=(V2Int lh,V2Int rh){
			return (lh._x!=rh._x)||(lh._y!=rh._y);
		}

}
