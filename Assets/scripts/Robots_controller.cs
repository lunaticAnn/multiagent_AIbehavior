using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Robots_controller : MonoBehaviour {
    public static Robots_controller instance = null;
    public enum robot_state {hang,block_exit, corner_target};
    public robot_state my_state;

    List<Robot_behave> my_robots;
    List<grid_node> current_pos;
    SquareGrid sg;

    const int time_to_decide = 3;
    List<evador_behave> player_target = new List<evador_behave>();
    Dictionary<evador_behave, int> previous_step = new Dictionary<evador_behave, int>();
    Dictionary<evador_behave, int> current_step = new Dictionary<evador_behave, int>();
    Dictionary<evador_behave, int> rf_factor = new Dictionary<evador_behave, int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
        my_state = robot_state.hang;
       
    }

    public void init() {
        GameObject[] _rbs = GameObject.FindGameObjectsWithTag("robot");
        //get all the robots I am controlling
        my_robots = new List<Robot_behave>();
        foreach (GameObject _rb in _rbs)
        {
            my_robots.Add(_rb.GetComponent<Robot_behave>());
        }
        sg = GridsGenerator.instance.g;
        my_state = robot_state.block_exit;

    }

    public void update_state(robot_state r,evador_behave target=null) {
        
        //robots current positions
     
        switch (r) {
            case robot_state.block_exit:
                block_exit();
                return;
            case robot_state.corner_target:
                if (player_target.Count == 0)
                    create_target_list();
                refresh_target_list();
                evador_behave target_evador = Find_target(player_target);
                corner_target(target_evador);
                return;
        }
        //target
       
    }
    // Update is called once per frame

   

    //========================simple state machine=======================================
    void corner_target(evador_behave target)
    {
        if (target == null)
        {
            Debug.LogWarning("No target to be corner.");
            block_exit();
            return;
        }
        else {
            deploy_my_bots(target.current_node);
        }
    }

    //quick converge to its neighbors
    const int threat = 1;
    struct area
    {
        public List<grid_node> pos_in_area;
    }

   public void deploy_my_bots(grid_node target) {

        /*================this methods is only for 2 robot agents==================
         within the threat, Robots will try to maximize the intersection area with target evador
         which will have:
         2 list of potential movement
         2 iters iterating over the list
         return indexes of movements
         (this time:2*int)
       ============================2-robot-agents==================================*/
        area target_area = area_within_range(target);//default threat, can change to higher for test one-more step
        List<grid_node> potR0 = potential_movement(my_robots[0].current_node);
        List<grid_node> potR1 = potential_movement(my_robots[1].current_node);
        int max_score = 0;
        List<V2Int> good_strategy=new List<V2Int>();
        good_strategy.Add(new V2Int(0, 0));
        for (int index0 = 0; index0 < potR0.Count; index0++) {
            for (int index1 = 0; index1 < potR1.Count; index1++) {
                int current_score=strategy_evaluate(potR0[index0], potR1[index1], target_area);
                
                if (current_score > max_score)
                {
                    max_score = current_score;
                    good_strategy.Clear();
                    good_strategy.Add(new V2Int(index0, index1));
                }
                else if (current_score == max_score) {
                    good_strategy.Add(new V2Int(index0, index1));
                }
            }
        }
        int strategy_num = good_strategy.Count;
        Debug.LogWarning("number of strategies:"+strategy_num);
        int min_sum = 20;
        int best = 0;

        //=========choose a best strategy according to sum up of scores================
        for (int i = 0; i < strategy_num;i++) {
            grid_node next1 = potR0[good_strategy[i]._x];
            grid_node next2 = potR1[good_strategy[i]._y];
            int current_sum = Manhattan(next1.grid_position, target.grid_position)+ Manhattan(next2.grid_position, target.grid_position);
            if (current_sum < min_sum) {
                best = i;
                min_sum = current_sum;
            }
        }
        Debug.LogWarning("I choose:" + best);
        my_robots[0].SendMessage("move_received", potR0[good_strategy[best]._x]);
        my_robots[1].SendMessage("move_received", potR1[good_strategy[best]._y]);


        StartCoroutine("stage_yield");
        
        //==================2-robot-agents=======================
    }

    List<grid_node> potential_movement(grid_node n) {
        List<grid_node> p = new List<grid_node>();
        p.Add(n);
        foreach (grid_node nb in sg.my_neighbours(n)) {
            p.Add(nb);

        }
        return p;
    }

    int strategy_evaluate(grid_node nR1,grid_node nR2,area target) {
        area a1 = area_within_range(nR1);
        area a2 = area_within_range(nR2);
        area intersect1 = intersection(a1, target);
        area intersect2 = intersection(a2, target);
        int score = intersect1.pos_in_area.Count+intersect2.pos_in_area.Count;
        //better if target intersect with center in
        area overlay = intersection(intersect1, intersect2);
        score -= overlay.pos_in_area.Count;
        return score;
    }

    area area_within_range(grid_node x, int r=threat) {
        area target_area = new area();
        target_area.pos_in_area = new List<grid_node>();
        target_area.pos_in_area.Add(x);
        
        for (int i = 0; i < r; i++) {
            int num = target_area.pos_in_area.Count;
            for (int index = 0; index < num; index++)
            {
                foreach (grid_node nb in sg.pure_neighbour(target_area.pos_in_area[index]))
                {
                    if (!target_area.pos_in_area.Contains(nb))
                    {
                        target_area.pos_in_area.Add(nb);
                    }
                }
            }
        }
       
        return target_area;
    }

    area intersection(area a1, area a2) {
        area intersect = new area();
        intersect.pos_in_area = new List<grid_node>();
        foreach (grid_node n1 in a1.pos_in_area) {
            if (a2.pos_in_area.Contains(n1))
            {
                intersect.pos_in_area.Add(n1);
            }
        }

        return intersect;
    }


 void block_exit(){
        //separately move towards exit
        grid_node exit = sg.nodes.Find(n=>n.state==SquareGrid.grid_stat.exit);
        deploy_my_bots(exit);
    }

    void create_target_list()
    {
        player_target.Clear();
        GameObject[] target_evadors = GameObject.FindGameObjectsWithTag("evador");
        foreach (GameObject _e in target_evadors)
        {
            evador_behave eb = _e.GetComponent<evador_behave>();
            player_target.Add(eb);
            previous_step[eb] = 0;
            current_step[eb] = 0;
            rf_factor[eb] = 1;
        }
    }

    void refresh_target_list()
    {
        V2Int player_pos = GameObject.FindGameObjectWithTag("Player").GetComponent<moving>().current_node.grid_position;
        foreach (evador_behave _e in player_target)
        {
            previous_step[_e] = current_step[_e];
            current_step[_e] = Manhattan(player_pos, _e.current_node.grid_position);
        }
    }

    evador_behave Find_target(List<evador_behave> t)
    {
        int min_reward = 0;
        int reward;

        evador_behave target_now = null;
        foreach (evador_behave eb in t)
        {
            reward = 0;
            if (previous_step[eb] >= current_step[eb])
            {
                reward = -5 * rf_factor[eb];
                rf_factor[eb] += 1;

            }
            else { rf_factor[eb] = 1; }//clear target threat;
            reward += current_step[eb];
            if (target_now == null)
            {
                target_now = eb;
                min_reward = reward;
            }
            else
            {
                target_now = reward < min_reward ? eb : target_now;
            }
        }
        if (target_now)
            target_now.SendMessage("flash_me");
        return target_now;
    }

    int Manhattan(V2Int posA, V2Int posB)
    {
        return Mathf.Abs(posA._x - posB._x) + Mathf.Abs(posA._y - posB._y);
    }


    IEnumerator stage_yield()
    {
        yield return new WaitForSeconds(0.1f);
        StageController.instance.Stage_switch();
    }
    //=============================simple state machine====================================
}
