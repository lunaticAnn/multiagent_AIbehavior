using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class NNprocessor : MonoBehaviour {
    TextAsset weights;
    List<float> current_weights;
    string parameter_path;
    public int current_index;

    const int feature_num = 6;

    public static NNprocessor instance = null;
    public Dictionary<int,int[]> order_index;

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
        init_source();
        //current_index = Random.Range(0, feature_num);
        current_index = 3; 
    }

    void init_source() {
        order_index = new Dictionary<int, int[]>();
        int[] order = new int[3] { 0, 1, 2 };
        order_index[0] = order;
        order = new int[3] { 0, 2, 1 };
        order_index[1] = order;
        order = new int[3] { 1, 0, 2 };
        order_index[2] = order;
        order = new int[3] { 1, 2, 0 };
        order_index[3] = order;
        order = new int[3] { 2, 0, 1 };
        order_index[4] = order;
        order = new int[3] { 2, 1, 0 };
        order_index[5] = order;
    }
	
	// Update is called once per frame
	void Init_weights (int count,float init_par) {
        StreamWriter sw = new StreamWriter(parameter_path);
        for(int i=0;i<count;i++)
            sw.WriteLine(init_par);
        sw.Close();
    }

    void write_weights(List<float> new_par){
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

   public void update_weights(int index,float to_change)
    {
        List<float> my_weights = get_weights(feature_num);
        my_weights[index] += to_change;
        write_weights(my_weights); 
    }

    void print_weights(List<float> weights) {
        for (int i = 0; i < weights.Count; i++) {
            Debug.Log("index "+i+":"+weights[i]);
        }
    }

    public void print_results() {
       string data_path = Application.dataPath + "\\Resources\\_results.txt";
        StreamWriter sw = new StreamWriter(data_path,true);
        string s= "EvadorGetOut:" + StageController.instance.score_evador;
        s+=" Time:" + Time.timeSinceLevelLoad.ToString();
        s+= " Moves:" + StageController.instance.movecount.ToString();
        sw.WriteLine(s);
        sw.Close();
    }


}
