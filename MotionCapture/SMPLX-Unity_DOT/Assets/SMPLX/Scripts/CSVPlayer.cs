using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVPlayer : MonoBehaviour
{
    bool zDown;
    bool bDown;

    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();

    public SMPLX smpl_module;

    string[] _bodyJointNames = new string[] {"pelvis", "left_hip", "right_hip", "spine1", "left_knee",
                                             "right_knee", "spine2", "left_ankle", "right_ankle", "spine3",
                                             "left_foot", "right_foot", "neck", "left_collar", "right_collar",
                                             "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                                             "left_wrist", "right_wrist" };  //AMASS 22 Joint

    //string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
    //                                                "left_elbow","right_hip","right_knee","left_hip","left_knee"}; //awinda 10 Joint

    string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
                                                    "left_elbow"};

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        fileRead();
        _Animation();
    }

    void GetInput()
    {
        zDown = Input.GetKeyDown(KeyCode.Z);
        bDown = Input.GetKeyDown(KeyCode.B);
    }

    void fileRead()
    {
        if(zDown)
        {
            //string prefix = "D:/Unity/SMPLX-Unity_DOT/";   //root file path  
            string prefix = "D:/Velab_github/Metaverse_VElab/MotionCapture/SMPLX-Unity_DOT/";   //root file path  

            TXTReader(prefix + "1016test.csv");

            //Debug.Log("TXT file load done");
            Debug.Log("CSV file load done");
        }
    }

    void _Animation()
    {
        if(bDown)
        {
            StartCoroutine(avatar_play_custom());
        }
    }

    IEnumerator avatar_play_custom() //awinda Xsens data player
    {
        for (int frame_cnt = 0; frame_cnt < load_quat_list[0][0].Count; frame_cnt++)
        {

            for (int i = 0; i < _bodyCustomJointNames.Length; i++)
            {
                smpl_module.SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
            }
            smpl_module.UpdateJointPositions(false);
            yield return new WaitForSeconds(.0166667f);
        }
        yield break;
    }

    void TXTReader(string file_path) 
    {
        FileStream quatStream = new FileStream(file_path, FileMode.Open);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Quaternion>> load_quat_buf = new List<List<Quaternion>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_quat_buf.Add(new List<Quaternion>());
        }

        float[] data_val = new float[4];

        for (int line = 0; line < records.Length; line++)
        {
            fields = records[line].Split(',');

            int fields_cnt = 0;

            for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
            {
                for (int val_idx = 0; val_idx < 4; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    fields_cnt++;
                }
                load_quat_buf[device_idx].Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
            }
        }
        Debug.Log("quaternion reading done");
        sr.Close();
        quatStream.Close();

        load_quat_list.Add(load_quat_buf);

        return;
    }
}
