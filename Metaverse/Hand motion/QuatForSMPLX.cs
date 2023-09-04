using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class QuatForSMPLX : MonoBehaviour
{
//--------------- From SMPLX script -----------------
    SMPLX smplX;
    Dictionary<string, Transform> _transformFromName;
    bool is_Coroutine;
//---------------------------------------------------


    Vector3 oldPos_wrist, newPos_wrist, oldRot_elbow, newRot_elbow;
    Vector3 elbowGap_rot, wristGap_pos;
    Quaternion oldQuat_elbow, newQuat_elbow, oldQuat_wrist;

    public float magnify = 4.0f;
    public float clamp = 0.5f;

    List<List<float>> records = new List<List<float>>();
    List<float> motion_num = new List<float>();


    public List<List<List<Quaternion>>> L_pose_data = new List<List<List<Quaternion>>>();
    public List<List<List<Quaternion>>> R_pose_data = new List<List<List<Quaternion>>>();
    public List<Vector3> pos_list = new List<Vector3>();

    public bool is_mirrored = true;

    public GameObject linePref;
    [HideInInspector]
    public LineRenderer lineRenderer;

    public bool isDrew = false;

    ConductingHand handScript;

    private string file_path = "C:/Users/pssil/OneDrive/바탕 화면/velab/2023.07-08/SMPLX-Unity/Assets/";


    string[] _manualLeftJointNames = new string[] {
        "left_shoulder", "left_elbow",    "left_wrist",         // [0],  [1],  [2]
        "left_index1",   "left_index2",   "left_index3",        // [3],  [4],  [5]
        "left_middle1",  "left_middle2",  "left_middle3",       // [6],  [7],  [8]
        "left_pinky1",   "left_pinky2",   "left_pinky3",        // [9],  [10], [11]
        "left_ring1",    "left_ring2",    "left_ring3",         // [12], [13], [14]
        "left_thumb1",   "left_thumb2",   "left_thumb3"         // [15], [16], [17]
    };

    string[] _manualRightJointNames = new string[] {
        "right_shoulder","right_elbow",   "right_wrist",        // [0],  [1],  [2]
        "right_index1",  "right_index2",  "right_index3",       // [3],  [4],  [5]
        "right_middle1", "right_middle2", "right_middle3",      // [6],  [7],  [8]
        "right_pinky1",  "right_pinky2",  "right_pinky3",       // [9],  [10], [11]
        "right_ring1",   "right_ring2",   "right_ring3",        // [12], [13], [14]
        "right_thumb1",  "right_thumb2",  "right_thumb3"        // [15], [16], [17]
    };



    private void Start()
    {
        smplX = GetComponent<SMPLX>();
        _transformFromName = smplX._transformFromName;
        is_Coroutine = smplX.is_Coroutine;

        handScript = GameObject.Find("SteamVR_female_hand_right 1").GetComponent<ConductingHand>();
    }


    void Update()
    {

        newPos_wrist = _transformFromName["right_wrist"].transform.position;
        newRot_elbow = _transformFromName["right_elbow"].transform.eulerAngles;


        if (Input.GetKeyDown(KeyCode.Slash))
        {
            // Read the file and Save quaternion data
            for (int i = 0; i < 8; i++)
                ReadQuaternion(file_path + "basis_pose_" + i.ToString() + ".csv", i);


            // Check saved values
            for (int a = 0; a < L_pose_data.Count; a++)
            {
                for (int b = 0; b < L_pose_data[a].Count; b++)
                {
                    for (int c = 0; c < L_pose_data[a][b].Count; c++)
                        Debug.Log("L_pose_data[" + a + "][" + b + "][" + c + "] = " + L_pose_data[a][b][c]);
                }
            }
        }

        else if (Input.GetKeyDown(KeyCode.Keypad0))     // Flat
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(0));
            //StartCoroutine(handScript.PlayMotion(0));

            Debug.Log("Play 'Flat' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))     // Bent
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(1));
            //StartCoroutine(handScript.PlayMotion(1));

            Debug.Log("Play 'Bent' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))     // Pursed
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(2));
            //StartCoroutine(handScript.PlayMotion(2));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))     // O sign
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(3));
            //StartCoroutine(handScript.PlayMotion(3));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))     // Fist
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(4));
            //StopCoroutine(handScript.PlayMotion(4));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))     // Feeling 1 (Pinky up)
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(5));
            //StopCoroutine(handScript.PlayMotion(5));

            Debug.Log("Play 'Feeling 1' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))     // Pointing
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(6));
            //StopCoroutine(handScript.PlayMotion(6));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7))     // Feeling 2 (Holding out)
        {
            is_Coroutine = true;
            StartCoroutine(RotationDelay(7));
            //StopCoroutine(handScript.PlayMotion(7));
        }

        //rotateWrist();


        // Activate a function 'avator_play_custom' from 'Player.cs' and Instantiate line prefab
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject currentLine = Instantiate(linePref, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
        }
        
        DrawPath(true);

        smplX.UpdatePoseCorrectives();
        smplX.UpdateJointPositions(false);

    }

    public IEnumerator calcGapValue()
    {
        oldPos_wrist = _transformFromName["right_wrist"].transform.position;
        oldRot_elbow = _transformFromName["right_elbow"].transform.eulerAngles;

        oldQuat_wrist = _transformFromName["right_wrist"].transform.localRotation;
        
        //Debug.Log("** Wrist Position Gap: (" + (float)wristGap_pos.x + ", " + (float)wristGap_pos.y + ", " + (float)wristGap_pos.z + ") **");


        elbowGap_rot = newRot_elbow - oldRot_elbow;
        wristGap_pos = (newPos_wrist - oldPos_wrist) * 10;

        //List<float> old_elbow_list = new List<float> { oldRot_elbow.x, oldRot_elbow .y, oldRot_elbow.z};
        //List<float> elbow_gap_list = new List<float> { elbowGap_rot.x, elbowGap_rot.y, elbowGap_rot.z};
        //List<float> wrist_rot_list = new List<float> { _transformFromName["right_wrist"].transform.localEulerAngles.x, _transformFromName["right_wrist"].transform.localEulerAngles.y, _transformFromName["right_wrist"].transform.localEulerAngles.z };
        //List<float> wrist_gap_list = new List<float> { wristGap_pos.x, wristGap_pos.y, wristGap_pos.z };
        //records.Add(wrist_gap_list);

        
        yield return new WaitForSeconds(1.0f);

        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    elbowGap_rot = new Vector3(0.0f, 0.0f, 0.0f);   // Clear the rotation gap between old and new value
        //    wristGap_pos = new Vector3(0.0f, 0.0f, 0.0f);   // Clear the position gap between old and new value

        //yield break;
        //}
    }

    void DrawPath(bool isDrew)
    {
        if (isDrew == true)
        {
            Vector3 current_pos = _transformFromName["right_wrist"].transform.position;
            pos_list.Add(current_pos);

            lineRenderer.positionCount = pos_list.Count;
            lineRenderer.SetPositions(pos_list.ToArray());
        }
    }


    // Read quaternion data from CSV file and Add to lists
    void ReadQuaternion(string file_path, int idx)
    {
        List<List<Quaternion>> L_joint_data = new List<List<Quaternion>>();
        List<List<Quaternion>> R_joint_data = new List<List<Quaternion>>();


        FileStream fileStream = new FileStream(file_path, FileMode.OpenOrCreate);
        StreamReader sr = new StreamReader(fileStream);

        string[] row = sr.ReadToEnd().Split('\n');


        // [i]: frame index
        for (int i = 2; i < row.Length; i++)
        { 

            string[] column = row[i].Split(',');

            // # of joints
            int joint_num = (column.Length / 3) / 2;


            if (i == 2)
            {
                for (int n = 0; n < joint_num; n++)
                {
                    L_joint_data.Add(new List<Quaternion>());
                    R_joint_data.Add(new List<Quaternion>());
                }
            }

            if (!is_mirrored)
            {
                // [j]: joint index
                for (int j = 0; j < joint_num * 2; j++)
                {
                    float[] axis = new float[3];

                    axis[0] = float.Parse(column[j * 3]);
                    axis[1] = float.Parse(column[j * 3 + 1]);
                    axis[2] = float.Parse(column[j * 3 + 2]);

                    // Save the data transformed to quternion
                    Quaternion value = Quaternion.Euler(new Vector3(axis[0], axis[1], axis[2]));

                    // Left
                    if (j < joint_num)
                        L_joint_data[j].Add(value);

                    // Right
                    else
                        R_joint_data[j - joint_num].Add(value);
                }
            }

            else
            {
                for (int j = 0; j < joint_num; j++)
                {
                    float[] axis = new float[3];

                    axis[0] = float.Parse(column[j * 3]);
                    axis[1] = float.Parse(column[j * 3 + 1]);
                    axis[2] = float.Parse(column[j * 3 + 2]);

                    // Save the data transformed to quternion
                    Quaternion value_L = Quaternion.Euler(new Vector3(axis[0], axis[1], axis[2]));
                    Quaternion value_R = Quaternion.Euler(new Vector3(axis[0], -axis[1], -axis[2]));

                    // Left
                    L_joint_data[j].Add(value_L);

                    // Right
                    R_joint_data[j].Add(value_R);
                }
            }

        }

        sr.Close();
        fileStream.Close();

        L_pose_data.Add(new List<List<Quaternion>>(L_joint_data));
        R_pose_data.Add(new List<List<Quaternion>>(R_joint_data));


        Debug.Log("Reading quaternion data is done");

        return;

    }


    // Rotate hand joints(finger joints) by data of hand motion lists
    /* [p]: pose index    [j]: joint index      [f]: frame index */
    public IEnumerator RotationDelay(int p)
    {
        Debug.Log("Motion Start: " + p);


        // # of joints
        int joint_num = _manualLeftJointNames.Count();


        if (is_Coroutine)
        {
            //ResetBodyPose();

            // Calculate the time parameter of Quaternion slerp function
            //float slerp_time = 60.0f * Random.Range(2.0f, 3.5f);
            float slerp_time = 60.0f;

            //Debug.Log("slerp_time = " + slerp_time);

            for (float t = 0; t <= slerp_time; t++)
            {
                float sec = t / slerp_time;


                for (int j = 3; j < joint_num; j++)
                {
                    // Left
                    int l_frame_num = L_pose_data[p][j].Count;
                    Transform l_joints = _transformFromName[_manualLeftJointNames[j]];


                    for (int f = 0; f < l_frame_num; f++)
                    {
                        Quaternion old_rot = l_joints.localRotation;

                        l_joints.localRotation = Quaternion.Slerp(old_rot, L_pose_data[p][j][l_frame_num - 1], sec);
                        //l_joints.localRotation = Quaternion.Slerp(L_pose_data[p][j][f], L_pose_data[p][j][f + 1], sec);

                        //Debug.Log("start: L_pose_data[" + p + "][" + j + "][" + f + "]" + ", end: L_pose_data[" + p + "][" + j + "][" + (f + 1) + "]");
                    }


                    // Right
                    int r_frame_num = R_pose_data[p][j].Count;
                    Transform r_joints = _transformFromName[_manualRightJointNames[j]];


                    for (int f = 0; f < r_frame_num; f++)
                    {
                        Quaternion old_rot = r_joints.localRotation;

                        r_joints.localRotation = Quaternion.Slerp(old_rot, R_pose_data[p][j][l_frame_num - 1], sec);
                        //r_joints.localRotation = Quaternion.Slerp(R_pose_data[p][j][f], R_pose_data[p][j][f + 1], sec);
                    }
                }

                yield return new WaitForSeconds(0.0075f);

            }

            yield break;
        }

        is_Coroutine = false;
    }

    // Apply to rotate the wrist and map hand motions (considering to elbow rot gap)
    void rotateWrist()
    {
        //_transformFromName["right_wrist"].transform.localRotation = Quaternion.Euler(elbowGap_rot * magnify);
        //_transformFromName["right_wrist"].transform.localEulerAngles = elbowGap_rot * magnify;

        //Vector3 wrist_angle = _transformFromName["right_wrist"].transform.localEulerAngles = elbowGap_rot * magnify;

        //if (wrist_angle.x > 180) {
        //    wrist_angle.x -= 360.0f;
        //    wrist_angle.x = Mathf.Clamp(wrist_angle.x, -180, 180);
        //}
        //if (wrist_angle.y > 180) {
        //    wrist_angle.y -= 360.0f;
        //    wrist_angle.y = Mathf.Clamp(wrist_angle.y, -180, 180);
        //}
        //if (wrist_angle.z > 180) {
        //    wrist_angle.z -= 360.0f;
        //    wrist_angle.z = Mathf.Clamp(wrist_angle.z, -180, 180);
        //}


        _transformFromName["right_wrist"].transform.localRotation = Quaternion.Slerp(oldQuat_wrist, Quaternion.Euler(elbowGap_rot * magnify), clamp);


        if (wristGap_pos.x != 0 || wristGap_pos.y != 0 || wristGap_pos.z != 0)
        {

            for (int j = 3; j < _manualRightJointNames.Length; j++)
            {
                int p;
                float t = 0.1f;


                Transform r_joints = _transformFromName[_manualRightJointNames[j]];
                Quaternion old_rot = r_joints.localRotation;

                if ((wristGap_pos.x > 0 && wristGap_pos.y < 0) || (wristGap_pos.x < 0 && wristGap_pos.y < 0))   // 오른쪽 아래 방향
                {
                    p = 0;
                    r_joints.localRotation = Quaternion.Slerp(old_rot, R_pose_data[p][j][R_pose_data[p][j].Count - 1], t);

                    //Debug.Log("Play 'Flat' motion");
                    motion_num.Add(p);
                }

                if ((wristGap_pos.x < 0 && wristGap_pos.y > 0) || (wristGap_pos.x > 0 && wristGap_pos.y > 0))   // 왼쪽 위 방향
                {
                    p = 7;
                    r_joints.localRotation = Quaternion.Slerp(old_rot, R_pose_data[p][j][R_pose_data[p][j].Count - 1], t);

                    //Debug.Log("Play 'Feeling 2' motion");
                    motion_num.Add(p);
                }

                if (wristGap_pos.x < 0 && wristGap_pos.y < 0 && wristGap_pos.z < 0) // 왼쪽 아래 안쪽 방향
                {
                    p = 1;
                    r_joints.localRotation = Quaternion.Slerp(old_rot, R_pose_data[p][j][R_pose_data[p][j].Count - 1], t);

                    //Debug.Log("Play 'Bent' motion");
                    motion_num.Add(p);
                }
                
            }

        }
    }

    void saveCSVfile(string file_path)
    {
        FileStream fs = new FileStream(file_path + "/records/wrist pos_4.4_100.csv", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        for (int i = 0; i < records.Count; i++)
            sw.WriteLine("{0}, {1}, {2}", records[i][0], records[i][1], records[i][2]);

        Debug.Log("records.Count: " + records.Count);

        //for (int i = 0; i < motion_num.Count; i++)
        //    sw.WriteLine("{0}", motion_num[i]);

        sw.Close();
        fs.Close();


        records.Clear();

        return;
    }
}





/*
public class readData : MonoBehaviour
{
    public List<Quaternion> load_data = new List<Quaternion>();
    public List<Quaternion> data = new List<Quaternion>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            ReadQuaternion("C:/Users/pssil/OneDrive/바탕 화면/velab/2023.02/SMPLX-Unity/Assets/test_data.csv");
        }
    }


    //void data_read_csv_meta75(string[] file_namelist)
    //{

    //    string prefix = "E:/Sensor_Dataload_0113/QuaternionDB/Dot/";

    //    foreach (string each_file in file_namelist)
    //    {
    //        lst_DBpath.Add(prefix + each_file + "test1.csv");
    //        Debug.Log(prefix + each_file + "test1.csv");
    //    }

    //    foreach (string file_path in lst_DBpath)
    //    {
    //        ReadQuaternion(file_path);
    //    }
    //}


    void ReadQuaternion(string file_path)
    {

        FileStream fileStream = new FileStream(file_path, FileMode.OpenOrCreate);
        StreamReader sr = new StreamReader(fileStream);

        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        for (int i = 2; i < records.Length; i++)
        {
            fields = records[i].Split(',');
            int fields_cnt = 0;

            float[] joint = new float[3];

            for (int j = 0; j < (fields.Length / 3); j++)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    joint[axis] = float.Parse(fields[fields_cnt]);
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

                //Quaternion value = new Quaternion(joint[0], joint[1], joint[2]);
                Quaternion value = Quaternion.Euler(new Vector3(joint[0], joint[1], joint[2]));

                data.Add(value);
            }

        }

        sr.Close();
        fileStream.Close();


        load_data = data;

        Debug.Log("Quaternion reading done");

        return;
    }
}
*/