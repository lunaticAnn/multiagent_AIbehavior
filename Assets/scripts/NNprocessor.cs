using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class NNprocessor : MonoBehaviour {
    TextAsset weights;
    List<float> current_weights;
    string parameter_path;
    const int feature_num = 10;
    public static NNprocessor instance = null;

    // Use this for initialization
    void Awake () {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
        parameter_path = Application.dataPath + "\\Resources\\param.txt";
        Init_weights(feature_num, 1f);
	}
	
	// Update is called once per frame
	void Init_weights (int count,float init_par) {
        StreamWriter sw = new StreamWriter(parameter_path);
        for(int i=0;i<count;i++)
            sw.WriteLine(init_par);
        sw.Close();
    }

    void update_weights(List<float> new_par){
        StreamWriter sw = new StreamWriter(parameter_path);
        for (int i = 0; i < new_par.Count; i++)
            sw.WriteLine(new_par[i]);
        sw.Close();
    }

    List<float> get_weights(int count) {
        List<float> my_params = new List<float>();
        StreamReader sr = new StreamReader(parameter_path);
        for ( int i = 0;i<count;i++) {
            string f = sr.ReadLine();
            my_params.Add(float.Parse(f));
        } ;
        sr.Close();
        return my_params;
    }

    void print_weights(List<float> weights) {
        for (int i = 0; i < weights.Count; i++) {
            Debug.Log("index "+i+":"+weights[i]);
        }
    }

    public void print_results() {
       string data_path = Application.dataPath + "\\Resources\\results_IQ1.txt";
        StreamWriter sw = new StreamWriter(data_path,true);
        string s= "EvadorGetOut:" + StageController.instance.score_evador;
        s+=" Time:" + Time.timeSinceLevelLoad.ToString();
        s+= " Moves:" + StageController.instance.movecount.ToString();
        sw.WriteLine(s);
        sw.Close();
    }


}
