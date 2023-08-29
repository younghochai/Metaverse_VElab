using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    //========================= Added by js =============================//
    public string timeSignature;       // Time Signature
    public int bpm;                    // BPM
    public Subject subject_name;
    public List<List<Quaternion>> load_init_quat = new List<List<Quaternion>>();   // Calibration quaternion data

    public enum Subject {
        JS, HB
    };  // Name of subjects

    public Vector3 elbowGap_rot, wristGap_pos;

    [HideInInspector]
    public Vector3 oldPos_wrist, newPos_wrist, oldRot_elbow, newRot_elbow;
    [HideInInspector]
    public Quaternion oldQuat_elbow, newQuat_elbow, oldQuat_wrist;

    Dictionary<string, Transform> _transformFromName;

    QuatForSMPLX quat4smplX = new QuatForSMPLX();
    //=================================================================//

    float speed = 0.5f;

    float hAxis;
    float vAxis;

    bool xDown;
    bool cDown;
    bool vDown;

    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();
    public List<List<List<Vector3>>> load_axis_list = new List<List<List<Vector3>>>();

    public SMPLX smpl_module;
    Vector3 moveVec;

    string[] _bodyJointNames = new string[] {"pelvis", "left_hip", "right_hip", "spine1", "left_knee",
                                             "right_knee", "spine2", "left_ankle", "right_ankle", "spine3",
                                             "left_foot", "right_foot", "neck", "left_collar", "right_collar",
                                             "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                                             "left_wrist", "right_wrist" };

    string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
                                                    "left_elbow","right_hip","right_knee","left_hip","left_knee"};

    //string[] _bodyCustomJointNames = new string[] { "pelvis" };

    void Start()
    {
        _transformFromName = smpl_module._transformFromName;
        quat4smplX = GetComponent<QuatForSMPLX>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        fileRead();
        _Animation();
        fileSave();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        xDown = Input.GetKeyDown(KeyCode.B);
        cDown = Input.GetKeyDown(KeyCode.N);
        vDown = Input.GetKeyDown(KeyCode.V);
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        transform.position += moveVec * speed * Time.deltaTime;
    }

    void fileRead()
    {
        if(xDown)
        {
            load_quat_list.Clear();     // Quaternion list reset (js)
            load_init_quat.Clear();     // Calibration quaternion data reset (js)

            //string prefix1 = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert/s1/";
            //string prefix2 = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert/s2/";
            //string prefix3 = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert/s3/";
            //string prefix4 = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert/s4/";
            //string prefix5 = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert/s5/";
            string prefix = "E:/CONDUCTING/conducting_sensing data/";

            /***************subject 1***************/
            //CSVReader(prefix1 + "acting1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "acting2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "acting3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix1 + "freestyle1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "freestyle2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "freestyle3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix1 + "rom1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "rom2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "rom3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix1 + "walking1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "walking2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix1 + "walking3_stageii_concat_root_body_pose.csv");

            /***************subject 2***************/
            //CSVReader(prefix2 + "acting1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "acting2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "acting3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix2 + "freestyle2_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix2 + "rom1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "rom2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "rom3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix2 + "walking1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "walking2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix2 + "walking3_stageii_concat_root_body_pose.csv");

            /***************subject 3***************/
            //CSVReader(prefix3 + "acting2_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix3 + "rom1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix3 + "rom2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix3 + "rom3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix3 + "walking1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix3 + "walking2_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix3 + "walking3_stageii_concat_root_body_pose.csv");

            /***************subject 4***************/
            //CSVReader(prefix4 + "freestyle1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix4 + "freestyle3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix4 + "rom3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix4 + "walking2_stageii_concat_root_body_pose.csv");

            /***************subject 5***************/
            //CSVReader(prefix5 + "freestyle1_stageii_concat_root_body_pose.csv");
            //CSVReader(prefix5 + "freestyle3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix5 + "rom3_stageii_concat_root_body_pose.csv");

            //CSVReader(prefix5 + "walking2_stageii_concat_root_body_pose.csv");

            //TXTReader(prefix + "alignpos.txt");
            //TXTReader(prefix + "walking1_stageii_concat_root_body_pose.txt");

            if (subject_name == Subject.JS)
            {
                TXTReader(prefix + subject_name + "/" + timeSignature + "_100_once.txt");
                CalibTXTReader(prefix + subject_name + "/CalibIMUData-000-171419.txt");
            }
            if (subject_name == Subject.HB)
            {
                TXTReader(prefix + subject_name + "/" + timeSignature + "_100_repeat.txt");
                CalibTXTReader(prefix + subject_name + "/CalibIMUData-000-163118.txt");
            }


            Debug.Log("TXT file load done");
            //Debug.Log("CSV file load done");
        }
    }

    void _Animation()
    {
        //avatar_play()가 켜져 있을 땐 txt를 읽을 수 없고 반대의 경우 csv를 읽을 수 없음.
        if (cDown)
        {
            //StartCoroutine(avatar_play());
            StartCoroutine(avator_play_custom());
        }

        //------------------------ Added by js --------------------------------
        //newPos_wrist = _transformFromName["right_wrist"].transform.position;
        //newRot_elbow = _transformFromName["right_elbow"].transform.eulerAngles;

        //newQuat_elbow = _transformFromName["right_elbow"].transform.rotation;

        //Debug.Log("elbow gap: " + elbowGap_rot);
        //---------------------------------------------------------------------
    }

    void fileSave()
    { 
        if(vDown)
        {
            string prefix = "C:/Users/pssil/PycharmProjects/pythonProject1/TotalCapture_csv_concat_convert_txt/";
            string txtFilePath = prefix + "walking1_stageii_concat_root_body_pose.txt";


            TXTWriter(txtFilePath);


            Debug.Log("CSV quaternion save done");
        }
    }

    IEnumerator avatar_play()
    {

        for (int frame_cnt = 0; frame_cnt < load_axis_list[0][0].Count; frame_cnt++)
        {

            for (int i = 0; i < _bodyJointNames.Length; i++)
            {
                smpl_module.SetLocalJointRotation(_bodyJointNames[i], QuatFromRodrigues(load_axis_list[0][i][frame_cnt].x, load_axis_list[0][i][frame_cnt].y, load_axis_list[0][i][frame_cnt].z));
            }
            smpl_module.UpdateJointPositions(false);
            yield return new WaitForSeconds(.025f);
        }
        yield break;
    }

    IEnumerator avator_play_custom()
    {
        float delay_time = 0.025f + (1 - bpm / 100) * 0.025f;  // correspond to BPM (js)

        for (int frame_cnt = 0; frame_cnt < load_quat_list[0][0].Count; frame_cnt++)
        {

            for (int i = 0; i < _bodyCustomJointNames.Length; i++)
            {
                //smpl_module.SetWorldJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
                //smpl_module.SetLocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
                
                //smpl_module.SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
            
                // Multiple inverse quaternion of an initial pose (js)
                Quaternion motion_quat = load_quat_list[0][i][frame_cnt];
                Quaternion init_quat = Quaternion.Inverse(load_init_quat[0][i]);
                smpl_module.SetWorld2LocalJointRotation(_bodyCustomJointNames[i], motion_quat * init_quat);

                //------------------------ Added by js --------------------------------
                StartCoroutine(quat4smplX.calcGapValue());                
                
                //oldPos_wrist = _transformFromName["right_wrist"].transform.position;
                //oldRot_elbow = _transformFromName["right_elbow"].transform.eulerAngles;
                ////oldQuat_elbow = _transformFromName["right_elbow"].transform.rotation;
                //oldQuat_wrist = _transformFromName["right_wrist"].transform.localRotation;

                //elbowGap_rot = newRot_elbow - oldRot_elbow;
                //wristGap_pos = (newPos_wrist - oldPos_wrist) * 10;

                //Debug.Log("** Wrist Position Gap: (" + (float)wristGap_pos.x + ", " + (float)wristGap_pos.y + ", " + (float)wristGap_pos.z + ") **");

                //Debug.Log("old: " + oldPos_wrist + ", " + oldRot_elbow);
                //Debug.Log("new: " + newPos_wrist + ", " + newRot_elbow);
                //Debug.Log("old_elbow: " + oldQuat_elbow);
                //Debug.Log("new_elbow: " + newQuat_elbow);
                //---------------------------------------------------------------------

            }
            smpl_module.UpdateJointPositions(false);
            yield return new WaitForSeconds(delay_time);

        }

        //------------------------ Added by js --------------------------------
        elbowGap_rot = new Vector3(0.0f, 0.0f, 0.0f);   // Clear the rotation gap between old and new value
        wristGap_pos = new Vector3(0.0f, 0.0f, 0.0f);   // Clear the position gap between old and new value
        //---------------------------------------------------------------------

        yield break;

    }

    void CSVReader(string file_path)
    {
        FileStream quatStream = new FileStream(file_path, FileMode.Open);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Vector3>> load_axis_buf = new List<List<Vector3>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_axis_buf.Add(new List<Vector3>());
        }

        float[] data_val = new float[3];

        for (int line = 0; line < records.Length; line++)
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
            fields = records[line].Split('\t');

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
                load_quat_buf[device_idx].Add(new Quaternion(-data_val[1], data_val[3], data_val[2], data_val[0]));
                //load_quat_buf[device_idx].Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
            }
        }
        Debug.Log("quaternion reading done");
        sr.Close();
        quatStream.Close();

        load_quat_list.Add(load_quat_buf);

        return;
    }

    
    // Read calibration quaternion data (init pose) (js)
    void CalibTXTReader(string file_path)
    {
        FileStream quatStream = new FileStream(file_path, FileMode.Open);
        StreamReader sr = new StreamReader(quatStream);

        string[] row = sr.ReadToEnd().Split('\n');
        string[] column;
        
        column = row[2].Split('\t');

        List<Quaternion> load_quat_buf = new List<Quaternion>();


        int joint_num = 0;

        for (int i = 0; i < column.Length / 4; i++)
        {
            float[] joint = new float[4];   
      
            for (int q = 0; q < 4; q++)
            {
                joint[q] = float.Parse(column[joint_num]);
                joint_num++;
            }

            Quaternion value = new Quaternion(-joint[1], joint[3], joint[2], joint[0]);
            load_quat_buf.Add(value);
        }

        Debug.Log("Reading initial quaternion is done");
        
        sr.Close();
        quatStream.Close();

        load_init_quat.Add(load_quat_buf);

        return;
    }
   


    void TXTWriter(string file_path) //ConvertToTXT
    {
        FileStream quatStream = new FileStream(file_path, FileMode.Create);
        StreamWriter sw = new StreamWriter(quatStream);
        List<List<Quaternion>> load_axis_buf = new List<List<Quaternion>>();

        sw.Write(load_axis_buf);

        sw.Close();
        quatStream.Close();

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

    public static Quaternion InverseQuatFromRodrigues(float rodX, float rodY, float rodZ)
    {
        // Local joint coordinate systems
        //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
        //   OpenGL: X-Right, Y-Up, Z-Back, Right-handed
        Vector3 axis = new Vector3(rodX, rodY, rodZ);
        float angle_deg = axis.magnitude * Mathf.Rad2Deg;
        Vector3.Normalize(axis);

        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);

        return quat;
    }
}
