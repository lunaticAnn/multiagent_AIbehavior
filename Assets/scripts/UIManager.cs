using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Text timer;
	public Text evador_score;
    public Text movecount;
	
	void Start(){
		evador_score.text="evador:"+StageController.instance.score_evador;	
	}

	void Update () {
		evador_score.text="evador:"+StageController.instance.score_evador;
        timer.text = Time.timeSinceLevelLoad.ToString();
        movecount.text = "Moves:" + StageController.instance.movecount.ToString();
        /*
        if (StageController.instance.movecount > 40) {
            NNprocessor.instance.update_weights(NNprocessor.instance.current_index, 3);
            //NNprocessor.instance.print_results();
            Application.LoadLevel(0);
        }*/
	}
}
