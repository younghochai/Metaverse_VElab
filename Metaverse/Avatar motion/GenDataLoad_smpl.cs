using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;
public class GenDataLoad : MonoBehaviour
{
    List<string> lst_DBpath = new List<string>();
    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();
    public List<List<List<Vector3>>> load_axis_list = new List<List<List<Vector3>>>();


    public List<List<Quaternion>> gen_quat_list = new List<List<Quaternion>>();


    public List<Vector3> elbow_position = new List<Vector3>();
    public List<List<List<Vector3>>> Arm_position = new List<List<List<Vector3>>>();


    string[] _bodyJointNames = new string[] {"pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee", "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot", "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow", "left_wrist", "right_wrist", "left_hand", "right_hand", "transl" };
    public SMPLX smpl_manager;

    bool anim_play = false;
    bool anim_playing = false;
    bool brk_cur_play = false;
    bool toggle_interpol = true;

    int cur_act_idx;
    int cur_act_frm;
    int frm_strat = 0;
    int frame_cnt = 0;

    int draw_joint = 20; // 21
    int elbow_joint = 18; // 19 

    int switch_label;

    public Python_net connect;
    public GameObject PythonConnection;
    public Material gen;

    [Header("Key pose")]
    ///public TextMeshProUGUI[] Chat_Text;
    public TMP_InputField file_num;
    public TMP_InputField frame_num;
    public Button play_pose;

    //public Material gen;
    public Material origin;
    public Material slerp;

    private List<LineRenderer> line_renderer = new List<LineRenderer>();

    public GameObject beads;

    string prefix_global = "QuaternionDB/gendata/case1Fix/motiongpt/M007399/";
    private Dictionary<int, Color> color_Dict;

    float Radian2Degree = 180 / Mathf.PI;
    float Degree2Radian = Mathf.PI / 180;
    // Start is called before the first frame update
    void Start()
    {

        PythonConnection = GameObject.Find("PythonConnection");

        connect = PythonConnection.GetComponent<Python_net>();

        color_Dict = new Dictionary<int, Color>();


        color_Dict.Add(0, Color.red); // Vivid Raspberry
        color_Dict.Add(1, Color.green); // : Medium Spring Green
        color_Dict.Add(2, Color.yellow); // Cadmium Yellow
        color_Dict.Add(3, new Color(255, 127, 51)); // Crayola's Orange
        color_Dict.Add(4, new Color(255, 0, 102));  // Medium Red-Violet

        for (int j = 0; j < 4; j++)
        {

            List<List<Vector3>> load_axis_buf = new List<List<Vector3>>();
            for (int i = 0; i < 2; i++)
            {
                load_axis_buf.Add(new List<Vector3>());

            }
            Arm_position.Add(load_axis_buf);
        }


        //LineRenderer line_gen = new GameObject().AddComponent<LineRenderer>();

        //line_renderer.Add(line_gen);

        //line_gen.gameObject.transform.SetParent(GameObject.Find("right_wrist").transform);
        // line_gen.gameObject.transform.localPosition = Vector3.zero;
        //line_gen.material = gen;
        //line_gen.startWidth=0.1f;
        //line_gen.endWidth = 0.1f;
        //line_gen.startColor = Color.yellow;
        //line_gen.endColor= Color.yellow;

        //LineRenderer line_origin = new GameObject().AddComponent<LineRenderer>();

        //line_origin.gameObject.transform.SetParent(GameObject.Find("right_wrist").transform);
        //line_origin.gameObject.transform.localPosition = Vector3.zero;
        //line_origin.material = origin;
        //line_origin.startWidth = 0.1f;
        //line_origin.endWidth = 0.1f;
        //line_renderer.Add(line_origin);


        //LineRenderer line_slerp = new GameObject().AddComponent<LineRenderer>();

        //line_slerp.gameObject.transform.SetParent(GameObject.Find("right_wrist").transform);
        //line_slerp.material = slerp;
        //line_slerp.gameObject.transform.localPosition = Vector3.zero;
        //line_slerp.startWidth = 0.1f;
        //line_slerp.endWidth = 0.1f;

        //line_renderer.Add(line_slerp);


    }    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {

            toggle_interpol = !toggle_interpol;
        }


        if (connect.newrecongition)
        {


            switch (connect.predict_label)
            {
                case 1:
                    switch_label = 0;
                    break;

                case 2:
                    switch_label = 1;
                    break;
                case 3:
                    switch_label = 2;
                    break;

                case 4:
                    switch_label = 3;
                    break;
                case 5:
                    switch_label = 4;
                    break;

            }

            int act_label = switch_label;
            if (anim_playing)
            {

                StopAllCoroutines();
                if (toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }

            }
            else
            {
                anim_playing = true;

                StartCoroutine(avatar_play_sel(act_label));
            }


            connect.newrecongition = false;



        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            // data_read_csv();
            // data_read_csv_meta75(new string[] { "0", "1", "2", "3", "4", "5" }); // "0", "1", "2", "3",//"4", "5", "6", "7", "8", "9", "10", "11", "12","13", "18","19", "20","21", "22","23", "24","25", "26","27"

            // "0", "1", "2", "3", "4", "5"

            //ReadQuaternion("QuaternionDB/gendata/full_scenario.csv");

            //string prefix = "QuaternionDB/gendata/";

            //ReadAxis(prefix + "1_load.csv");

            //ReadAxis(prefix + "2_load_std.csv");
            //ReadAxis(prefix + "3_load.csv");
            //ReadAxis(prefix + "4_load.csv");
            //ReadAxis(prefix + "5_load.csv");





            // ISMAR Exper. case 2 
            string prefix = "QuaternionDB/gendata/case1Fix/case2/";

            // motion gpt test
            string prefix2 = "QuaternionDB/gendata/case1Fix/motiongpt/M007399/";
            //ReadAxis(prefix + "gen_rot_try1_000467.csv");

            ReadAxis(prefix2 + "M007399_0_0_seg_gen_smpl.csv");
            ReadAxis(prefix2 + "M007399_0_1_seg_gen_smpl.csv");
            ReadAxis(prefix + "poses_data.csv");
            ReadAxis(prefix2 + "M007399_0_2_seg_gen_smpl.csv");
            ReadAxis(prefix2 + "M007399_gt_smpl.csv");

            // ISMAR
            //ReadAxis(prefix + "gen_rot_try1_000467.csv");
            //ReadAxis(prefix + "gen_rot_try1_004841.csv");
            //ReadAxis(prefix + "poses_data.csv");
            //ReadAxis(prefix + "000467_rotation.csv");
            //ReadAxis(prefix + "004841_rotation.csv");

            // case 2 
            //string prefix = "QuaternionDB/gendata/fighting/";
            //ReadAxis(prefix + "1_load.csv");
            //ReadAxis(prefix + "2_load.csv");
            //ReadAxis(prefix + "3_load_fix.csv");
            //ReadAxis(prefix + "4_load_fix.csv");
            //ReadAxis(prefix + "5_load_fix.csv");


            Debug.Log("gen data load done");
        }



        if (Input.GetKeyDown(KeyCode.A))
        {
            anim_play = true;

            StartCoroutine(avatar_play());

        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            int act_label = 0;
            if (anim_playing)
            {

                StopAllCoroutines();
                if(toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }

            }
            else
            {
                anim_playing = true;

                StartCoroutine(avatar_play_sel(act_label));
            }

        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            int act_label = 1;
            if (anim_playing)
            {
                
                StopAllCoroutines();

                if (toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }

            }
            else { 
            anim_playing = true;
           
            StartCoroutine(avatar_play_sel(act_label));
            }

        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            int act_label = 2;
            if (anim_playing)
            {

                StopAllCoroutines();

                if (toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }


            }
            else
            {
                anim_playing = true;

                StartCoroutine(avatar_play_sel(act_label));
            }

        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            int act_label = 3;
            if (anim_playing)
            {

                StopAllCoroutines();

                if (toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }

            }
            else
            {
                anim_playing = true;

                StartCoroutine(avatar_play_sel(act_label));
            }

        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            int act_label = 4;
            if (anim_playing)
            {

                StopAllCoroutines();

                if (toggle_interpol)
                {
                    StartCoroutine(avatar_bet_play(cur_act_idx, cur_act_frm, act_label, frm_strat));
                }
                else
                {
                    StartCoroutine(avatar_play_sel(act_label));

                }


            }
            else
            {
                anim_playing = true;

                StartCoroutine(avatar_play_sel(act_label));
            }

            //anim_playing = true;

            //StartCoroutine(avatar_play_sel(4));

        }
     


        //    if (anim_play)
        //{
        //    Debug.Log("gen data rotate cnt"+ load_axis_list[0][0].Count);



        //    //for (int i = 1; i < _bodyJointNames.Length - 3; i++)
        //    //{


        //    //    smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[0][i][frame_cnt].x, load_axis_list[0][i][frame_cnt].y, load_axis_list[0][i][frame_cnt].z));

        //    //}
        //    //smpl_manager.UpdatePoseCorrectives();
        //    //smpl_manager.UpdateJointPositions(false);
        //    //frame_cnt++;

        //    //for (int frame_cnt = 0; frame_cnt < load_axis_list[0][0].Count; frame_cnt++)
        //    //{

        //        for (int i = 1; i < _bodyJointNames.Length -3; i++)
        //        {


        //            smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[0][i][frame_cnt].x, load_axis_list[0][i][frame_cnt].y, load_axis_list[0][i][frame_cnt].z));

        //        }
        //        smpl_manager.UpdateJointPositions(false);

        //        Debug.Log("gen data cur frame"+ frame_cnt);
        //    frame_cnt++;
        //    //  }

        //    //for (int frame_cnt = 0; frame_cnt < load_quat_list[0][0].Count; frame_cnt++)
        //    //{

        //    //    for (int i = 1; i < _bodyJointNames.Length - 2; i++)
        //    //    {


        //    //        smpl_manager.SetLocalJointRotation(_bodyJointNames[i], load_quat_list[0][i][frame_cnt]);

        //    //    }

        //    //}
        //    if(frame_cnt == load_axis_list[0][0].Count)
        //    {
        //        Debug.Log("gen data rotate done");
        //        anim_play = false;
        //        frame_cnt = 0;

        //    }





        //}




    }

    void data_read_csv_meta75(string[] file_namelist)
    {

        string prefix = "QuaternionDB/gendata/";

        foreach (string each_file in file_namelist)
        {
            //string cur_file_path = prefix + each_file + "test1.csv";
            string cur_file_path = prefix + "full_scenario.csv";
            lst_DBpath.Add(cur_file_path);


            ReadQuaternion(cur_file_path);

            //Debug.Log(prefix + each_file + "test1.csv");
        }

        //foreach (string file_path in lst_DBpath)
        //{

        //     ReadQuaternion(file_path);

        //}





    }


    void cal_Slerp_data(int action_label, int[] frame_num)
    {
        List<List<Quaternion>> load_quat_buf = new List<List<Quaternion>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_quat_buf.Add(new List<Quaternion>());

        }


        for (int frame_idx = 0; frame_idx < frame_num.Length - 1; frame_idx++)
        {
            int start_quat_idx = frame_num[frame_idx];
            int end_quat_idx = frame_num[frame_idx + 1];

            int maxframe = end_quat_idx - start_quat_idx - 1;
            //Debug.Log(frame_idx);
            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {
                load_quat_buf[i].Add(QuatFromRodrigues(load_axis_list[action_label][i][frame_idx].x, load_axis_list[action_label][i][frame_idx].y, load_axis_list[action_label][i][frame_idx].z));
            }


            for (int frame_cnt = 0; frame_cnt < maxframe; frame_cnt++)
            {
                float ratio = ((float)frame_cnt) / maxframe;

                for (int i = 1; i < _bodyJointNames.Length - 3; i++)
                {

                    //mixed_quat_list[joint_idx].Add(Quaternion.Slerp(pose_list[file_cnt_i][joint_idx], pose_list[file_cnt_j][joint_idx], ratio));


                    load_quat_buf[i].Add(Quaternion.Slerp(QuatFromRodrigues(load_axis_list[action_label][i][frame_idx].x, load_axis_list[action_label][i][frame_idx].y, load_axis_list[action_label][i][frame_idx].z),
                        QuatFromRodrigues(load_axis_list[action_label][i][frame_idx + 1].x, load_axis_list[action_label][i][frame_idx + 1].y, load_axis_list[action_label][i][frame_idx + 1].z), ratio));


                }


            }

            // Debug.Log(frame_idx);

        }

        int last_quat_idx = frame_num.Length - 1;
        for (int i = 1; i < _bodyJointNames.Length - 3; i++)
        {
            load_quat_buf[i].Add(QuatFromRodrigues(load_axis_list[action_label][i][last_quat_idx].x, load_axis_list[action_label][i][last_quat_idx].y, load_axis_list[action_label][i][last_quat_idx].z));
        }

        gen_quat_list = load_quat_buf;


        // load_quat_list.Add(gen_quat_list);

        Debug.Log("gen data ");
        Debug.Log(gen_quat_list[3].Count);
        Debug.Log(load_quat_buf[3].Count);


    }



    IEnumerator avatar_bet_play(int pr_action_num, int frame_ednum, int post_action_num, int frame_stnum)
    {
        int maxframe = Random.Range(10, 20);

        for (int frame_idx = 0; frame_idx < maxframe; frame_idx++)
        {
            float ratio = ((float)frame_idx) / maxframe;

            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {

                //mixed_quat_list[joint_idx].Add(Quaternion.Slerp(pose_list[file_cnt_i][joint_idx], pose_list[file_cnt_j][joint_idx], ratio));

                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], 
                    Quaternion.Slerp(QuatFromRodrigues(load_axis_list[pr_action_num][i][frame_ednum].x, load_axis_list[pr_action_num][i][frame_ednum].y, load_axis_list[pr_action_num][i][frame_ednum].z), 
                    QuatFromRodrigues(load_axis_list[post_action_num][i][frame_stnum].x, load_axis_list[post_action_num][i][frame_stnum].y, load_axis_list[post_action_num][i][frame_stnum].z), ratio)
                    );

            }



            yield return new WaitForSeconds(.025f);
            Debug.Log("bet action play");

        }

        yield return StartCoroutine(avatar_play_sel(post_action_num, frame_stnum)); // start coroutine



        ////Color c = renderer.material.color;
        //for (int frame_cnt = frame_stnum; frame_cnt < load_axis_list[action_num][0].Count; frame_cnt++)
        //{

        //    for (int i = 1; i < _bodyJointNames.Length - 3; i++)
        //    {


        //        smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[action_num][i][frame_cnt].x, load_axis_list[action_num][i][frame_cnt].y, load_axis_list[action_num][i][frame_cnt].z));

        //    }
        //    smpl_manager.UpdateJointPositions(false);


        //    cur_act_idx = action_num;
        //    cur_act_frm = frame_cnt;

        //    yield return new WaitForSeconds(.025f);


        //}
        //anim_playing = false;
        //yield break;
    }




    IEnumerator avatar_play_sel(int action_num, int frame_stnum = 0)
    {

        cur_act_idx = action_num;
        //Color c = renderer.material.color;
        for (int frame_cnt = frame_stnum; frame_cnt < load_axis_list[action_num][0].Count; frame_cnt++)
        {

            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {


                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[action_num][i][frame_cnt].x, load_axis_list[action_num][i][frame_cnt].y, load_axis_list[action_num][i][frame_cnt].z));

            }
            smpl_manager.UpdateJointPositions(false);


          
           cur_act_frm= frame_cnt;

            yield return new WaitForSeconds(.025f);

           
        }
        anim_playing = false;
        yield break;
    }


    IEnumerator avatar_play_slerp(int action_num, int frame_stnum = 0)
    {
        //LineRenderer temp;
        Debug.Log("test gen data");
        Debug.Log(gen_quat_list[3].Count);
        // line_slerp.positionCount = gen_quat_list[3].Count;
        line_renderer[action_num].positionCount = gen_quat_list[3].Count;

        cur_act_idx = action_num;

        for (int frame_cnt = frame_stnum; frame_cnt < gen_quat_list[3].Count; frame_cnt++)
        {



            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {
                if (i == 5)
                    continue;
                else if (i == 4)
                    continue;
                else if (i == 7)
                    continue;

                else if (i == 8)
                    continue;
                else if (i == 1)
                    continue;
                else if (i == 2)
                    continue;
                else if (i == 12)
                    continue;
                else if (i == 15)
                    continue;

                else if (i == 10)
                    continue;

                else if (i == 11)
                    continue;

                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], gen_quat_list[i][frame_cnt]);

            }


            smpl_manager.UpdateJointPositions(false);


            //elbow_position.Add(smpl_manager.GetJointPositions()[19]);


            line_renderer[action_num].SetPosition(frame_cnt, smpl_manager.GetJointPositions()[draw_joint]);
            //line_gen.SetPosition(frame_cnt, smpl_manager.GetJointPositions()[19]);
            cur_act_frm = frame_cnt;

            yield return new WaitForSeconds(.025f);


        }


        anim_playing = false;
        yield break;
    }


    IEnumerator avatar_play_gen_beads(int action_num, int frame_stnum = 0)
    {
        //LineRenderer temp;



        cur_act_idx = action_num;

        for (int frame_cnt = frame_stnum; frame_cnt < load_axis_list[action_num][0].Count; frame_cnt++)
        {

            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {
                if (i == 5)
                    continue;
                else if (i == 4)
                    continue;
                else if (i == 7)
                    continue;

                else if (i == 8)
                    continue;
                else if (i == 1)
                    continue;
                else if (i == 2)
                    continue;
                else if (i == 12)
                    continue;
                else if (i == 15)
                    continue;

                else if (i == 10)
                    continue;

                else if (i == 11)
                    continue;


                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[action_num][i][frame_cnt].x, load_axis_list[action_num][i][frame_cnt].y, load_axis_list[action_num][i][frame_cnt].z));

            }
            smpl_manager.UpdateJointPositions(false);


            elbow_position.Add(smpl_manager.GetJointPositions()[draw_joint]);

            Instantiate(beads, smpl_manager.GetJointPositions()[draw_joint], Quaternion.identity);
            cur_act_frm = frame_cnt;

            yield return new WaitForSeconds(.025f);


        }

        anim_playing = false;
        yield break;
    }

    public void One_pose_play()
    {
        string file_num_str = file_num.text;
        string frame_num_str = frame_num.text;

        int file = int.Parse(file_num_str);
        int pose = int.Parse(frame_num_str);


        if (file == 5)
        {
            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
            {
                if (i == 5)
                    continue;
                else if (i == 4)
                    continue;
                else if (i == 7)
                    continue;

                else if (i == 8)
                    continue;
                else if (i == 1)
                    continue;
                else if (i == 2)
                    continue;
                else if (i == 12)
                    continue;
                else if (i == 15)
                    continue;

                else if (i == 10)
                    continue;

                else if (i == 11)
                    continue;

                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], gen_quat_list[i][pose]);

            }
            smpl_manager.UpdateJointPositions(false);
             else
            {

                for (int i = 0; i < _bodyJointNames.Length - 3; i++)
                {


                    //if (i == 5)
                    //    continue;
                    //else if (i == 4)
                    //    continue;
                    //else if (i == 7)
                    //    continue;

                    //else if (i == 8)
                    //    continue;
                    //else if (i == 1)
                    //    continue;
                    //else if (i == 2)
                    //    continue;
                    //else if (i == 12)
                    //    continue;
                    //else if (i == 15)
                    //    continue;

                    //else if (i == 10)
                    //    continue;

                    //else if (i == 11)
                    //    continue;

                    smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[file][i][pose].x, load_axis_list[file][i][pose].y, load_axis_list[file][i][pose].z));

                }
                smpl_manager.UpdateJointPositions(false);


            }


        }



    }
    IEnumerator avatar_play_whole(int action_num, int frame_stnum = 0)
    {

        int save_num = action_num;
        Debug.Log("check here");
        for (int frame_cnt = frame_stnum; frame_cnt < load_axis_list[action_num][0].Count; frame_cnt++)
        {

            //smpl_manager.Setglobalposition(_bodyJointNames[0], load_axis_list[action_num][0][frame_cnt]);


            for (int i = 0; i < _bodyJointNames.Length - 3; i++)
            {


                smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[action_num][i][frame_cnt].x, load_axis_list[action_num][i][frame_cnt].y, load_axis_list[action_num][i][frame_cnt].z));
                // smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[action_num][i][frame_cnt].x, load_axis_list[action_num][i][frame_cnt].y, load_axis_list[action_num][i][frame_cnt].z));
                //smpl_manager.Setlocalposition(_bodyJointNames[i], load_axis_list[action_num][i][frame_cnt]);
                //Debug.Log("play this");
            }
            smpl_manager.UpdateJointPositions(false);
            if (action_num > 2)
                save_num = action_num - 1;

            Arm_position[save_num][0].Add(smpl_manager.GetJointPositions()[elbow_joint]);
            Arm_position[save_num][1].Add(smpl_manager.GetJointPositions()[draw_joint]);
            cur_act_frm = frame_cnt;

            yield return new WaitForSeconds(.025f);
        }
        anim_playing = false;
        yield break;
    }



    IEnumerator avatar_play()
    {
        //Color c = renderer.material.color;
        for (int frame_cnt = 0; frame_cnt < load_axis_list[0][0].Count; frame_cnt++)
        {

            for (int i = 1; i < _bodyJointNames.Length - 3; i++)
        {


            smpl_manager.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[0][i][frame_cnt].x, load_axis_list[0][i][frame_cnt].y, load_axis_list[0][i][frame_cnt].z));

        }
        smpl_manager.UpdateJointPositions(false);

            yield return new WaitForSeconds(.025f);
     
        }

        yield break;
    }


    void ReadAxis(string file_path)
    {

        FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Vector3>> load_axis_buf = new List<List<Vector3>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_axis_buf.Add(new List<Vector3>());

        }


        float[] data_val = new float[3];

        for (int line = 1; line < records.Length - 1; line++)
        {
            fields = records[line].Split(',');

            int fields_cnt = 0;

            for (int device_idx = 0; device_idx < (fields.Length / 3); device_idx++)
            {


                for (int val_idx = 0; val_idx < 3; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

                //Debug.Log(new Quaternion(data_val[0], data_val[1], data_val[2], data_val[3]));
                load_axis_buf[device_idx].Add(new Vector3(data_val[0], data_val[1], data_val[2]));




            }


        }
        Debug.Log("Axis reading done");
        sr.Close();
        quatStream.Close();


        load_axis_list.Add(load_axis_buf);

        return;
    }
    void write_csv_file()
    {
        for (int repeat = 0; repeat < Arm_position.Count(); repeat++)
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(prefix_global + repeat.ToString() + "_test1.csv"))

            {


                var builder = new StringBuilder();


                //file.WriteLine(address_joint_idx[0] + ", x, y, z," + address_joint_idx[1] + ", x, y, z," + address_joint_idx[2] + ", x, y, z," + address_joint_idx[3] + ", x, y, z," + address_joint_idx[4] + ", x, y, z," + address_joint_idx[5] + ", x, y, z");
                for (int frame = 0; frame < Arm_position[repeat][0].Count(); frame++)
                {

                    Debug.Log("save test frm cnt : " + Arm_position[repeat][0].Count());

                    for (int joint = 0; joint < Arm_position[repeat].Count(); joint++)
                    {

                        builder.Append(Arm_position[repeat][joint][frame].x.ToString() + ',');
                        builder.Append(Arm_position[repeat][joint][frame].y.ToString() + ',');
                        //builder.Append(Arm_position[repeat][joint][frame].z.ToString() + ',');
                        if (joint == Arm_position[repeat].Count() - 1)
                            builder.Append(Arm_position[repeat][joint][frame].z.ToString());
                        else
                            builder.Append(Arm_position[repeat][joint][frame].z.ToString() + ',');
                    }




                    file.WriteLine(builder.ToString());

                    builder.Clear();
                }
            }

    }

    void ReadQuaternion(string file_path)
    {

        FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Quaternion>> load_quat_buf = new List<List<Quaternion>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_quat_buf.Add(new List<Quaternion>());

        }


        float[] data_val = new float[4];

        for (int line = 1; line < records.Length - 1; line++)
        {
            fields = records[line].Split(',');

            int fields_cnt = 0;

            for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
            {


                for (int val_idx = 0; val_idx < 4; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

                //Debug.Log(new Quaternion(data_val[0], data_val[1], data_val[2], data_val[3]));
                load_quat_buf[device_idx].Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));




            }


        }
        Debug.Log("Quaternion reading done");
        sr.Close();
        quatStream.Close();


        load_quat_list.Add(load_quat_buf);

        return;
    }
    public static Quaternion QuatFromRodrigues(float rodX, float rodY, float rodZ)
    {
        // Local joint coordinate systems
        //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
        //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
        Vector3 axis = new Vector3(-rodX, rodY, rodZ);
        float angle_deg = -axis.magnitude * Mathf.Rad2Deg;
        Vector3.Normalize(axis);

        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);

        return quat;
    }


    Quaternion EulerToQuat(Vector3 Angle)
    {
        float angleX, angleY, angleZ;
        Quaternion q;
        float q1, q2, q3, q4, scale;

        angleX = Angle.x * Degree2Radian;
        angleY = Angle.y * Degree2Radian;
        angleZ = Angle.z * Degree2Radian;
        //둘다 ZXY순서
        /* q1 = Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) + Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
         q2 = Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) + Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
         q3 = Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f) - Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f);
         q4 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f);*/


        return q;
    }



    Quaternion RotPelvis(Quaternion smpl_quat)
    {
        Quaternion data;
        Quaternion frontValue = EulerToQuat(new Vector3(0, 0, 89.9f));
        Vector3 axis = new Vector3(0, 1, 0);
        float angle_deg = 90;
        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);

        data = Quaternion.Inverse(quat) * smpl_quat * quat;

        frontValue = EulerToQuat(new Vector3(0, 0, 90));

        data = Quaternion.Inverse(frontValue) * data * frontValue;

        return data;
    }


}



