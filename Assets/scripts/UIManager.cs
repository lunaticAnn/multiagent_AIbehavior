using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public Text evador_score;
	
	void Start(){
		evador_score.text="evador:"+StageController.instance.score_evador;	
	}

	void Update () {
		evador_score.text="evador:"+StageController.instance.score_evador;	
	}
}
