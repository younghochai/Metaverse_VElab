using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenDataLoad : MonoBehaviour
{
    List<string> lst_DBpath = new List<string>();
    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();
    public List<List<List<Vector3>>> load_axis_list = new List<List<List<Vector3>>>();

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {

            toggle_interpol = !toggle_interpol;
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



            //string prefix = "QuaternionDB/gendata/fighting/";

            // case 2 
            string prefix = "QuaternionDB/gendata/case1Fix/case2/";

            //ReadAxis(prefix + "1_load.csv");
            ReadAxis(prefix + "gen_rot_try1_000467.csv");
            ReadAxis(prefix + "gen_rot_try1_004841.csv");
            ReadAxis(prefix + "poses_data.csv");
            ReadAxis(prefix + "000467_rotation.csv");
            ReadAxis(prefix + "004841_rotation.csv");
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

}



