using UnityEngine;
using System.Collections;

public class V2Int {

	/*=============================
	 * Basic Vector2(int) Class 
 	* ============================*/
	public int _x;
	public int _y;

	public V2Int(int x,int y){
		this._x=x;
		this._y=y;
	}

	// deep copy constructor
	public V2Int(V2Int source){
		this._x=source._x;
		this._y=source._y;
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

	public override string ToString(){
		return "("+this._x+", "+this._y+")";
	}


//	https://msdn.microsoft.com/en-US/library/ms173147%28v=vs.80%29.aspx
	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		// If parameter cannot be cast to Point return false.
		V2Int p = obj as V2Int;
		if ((System.Object)p == null) {
			return false;
		}

		// Return true if the fields match:
		return (this._x == p._x) && (this._y == p._y);
	}

	public bool Equals(V2Int p) {
		// If parameter is null return false:
		if ((object)p == null) {
			return false;
		}
		// Return true if the fields match:
		return (this._x == p._x) && (this._y == p._y);
	}

	public override int GetHashCode() {
		return this._x ^ this._y;
	}
}