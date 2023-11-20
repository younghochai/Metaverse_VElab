using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.UI;

public class QuatForSMPLX : MonoBehaviour
{
//--------------- From SMPLX script -----------------
    SMPLX smplX;
    Dictionary<string, Transform> _transformFromName;
    bool isCoroutine;
//---------------------------------------------------


    Vector3 oldPos_wrist, newPos_wrist, oldRot_elbow, newRot_elbow;
    Vector3 elbowGap_rot, wristGap_pos;
    Quaternion oldQuat_elbow, newQuat_elbow, oldQuat_wrist, oldRot;
    public float angle;

    public float scalingValue = 4.0f;
    public float clamp = 0.5f;

    public List<List<float>> record1 = new List<List<float>>();
    public List<List<float>> record2 = new List<List<float>>();
    List<float> motion_num = new List<float>();


    public List<List<List<Quaternion>>> L_pose_data = new List<List<List<Quaternion>>>();
    public List<List<List<Quaternion>>> R_pose_data = new List<List<List<Quaternion>>>();
    public List<Vector3> pos_list = new List<Vector3>();

    public bool isMirrored = false;

    public GameObject linePref;
    [HideInInspector]
    public LineRenderer lineRenderer1, lineRenderer2;

    bool isDrew = false;

    ConductingHand handScript;
    CSVPlayer readCSVscript;

    private string file_path = "C:/Users/pssil/OneDrive/바탕 화면/velab/2023.07-10/SMPLX-Unity/Assets/";


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
        readCSVscript = GameObject.Find("Play Controller").GetComponent<CSVPlayer>();
        _transformFromName = smplX._transformFromName;
        isCoroutine = smplX.is_Coroutine;
        
    }


    void Update()
    {

        newPos_wrist = _transformFromName["right_wrist"].transform.position;
        newRot_elbow = _transformFromName["right_elbow"].transform.eulerAngles;


        // Read the file and Save quaternion data
        if (Input.GetKeyDown(KeyCode.Slash))
        {
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
            isCoroutine = true;
            StartCoroutine(RotationDelay(0));
            //StartCoroutine(handScript.PlayMotion(0));

            Debug.Log("Play 'Flat' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))     // Bent
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(1));
            //StartCoroutine(handScript.PlayMotion(1));

            Debug.Log("Play 'Bent' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))     // Pursed
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(2));
            //StartCoroutine(handScript.PlayMotion(2));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))     // O sign
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(3));
            //StartCoroutine(handScript.PlayMotion(3));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))     // Fist
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(4));
            //StopCoroutine(handScript.PlayMotion(4));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))     // Feeling 1 (Pinky up)
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(5));
            //StopCoroutine(handScript.PlayMotion(5));

            Debug.Log("Play 'Feeling 1' motion");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))     // Pointing
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay(6));
            //StopCoroutine(handScript.PlayMotion(6));
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7))     // Feeling 2 (Holding out)
        {
            isCoroutine = true;
            StartCoroutine(RotationDelay2(7, 150, 1));
            //StopCoroutine(handScript.PlayMotion(7));
        }

        //RotateWrist();

        //if (Input.GetKeyDown(KeyCode.Comma))
        //{
        //    //GameObject R_path = Instantiate(linePref, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        //    //R_path.tag = "Line";
        //    //lineRenderer2 = R_path.GetComponent<LineRenderer>();
            
        //    GameObject L_path = Instantiate(linePref, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        //    L_path.tag = "Line";
        //    lineRenderer1 = L_path.GetComponent<LineRenderer>();

        //    //isDrew = true;

        //SaveDic2Csv(file_path + "/records/L wrist position_04.csv", record1);
        //    SaveList2Csv(file_path + "/records/R wrist position_04.csv", record2);
        //}

        if (Input.GetKeyDown(KeyCode.Period))
        {
            isDrew = false;
        }

        DrawPath(isDrew);

        smplX.UpdatePoseCorrectives();
        smplX.UpdateJointPositions(false);

    }

    void CalcVector(Vector3 current, Vector3 past, Vector3 old)
    {
        Vector3 vec1 = current - past;
        Vector3 vec2 = current - old;

        angle = Mathf.Acos(Vector3.Dot(vec1, vec2) / (Vector3.Magnitude(vec1) * Vector3.Magnitude(vec2))) * (float)(180.0 / Mathf.PI);

        Debug.Log("vec1: " + vec1 + ",  vec2: " + vec2 + "\nangle = " + angle);


        //if (angle != float.NaN && angle <= 100.0f)
        //{
        //    StartCoroutine(RotationDelay2(3, angle));
        //}
        //if (angle != float.NaN && angle > 100.0f)
        //{
        //    StartCoroutine(RotationDelay2(0, angle));
        //}
    }


    public IEnumerator CalcGapValue()
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
            Vector3 current_pos = _transformFromName["left_wrist"].transform.position;
            pos_list.Add(current_pos);

            lineRenderer1.positionCount = pos_list.Count;
            lineRenderer1.SetPositions(pos_list.ToArray());
        }

        else
            pos_list.Clear();
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

            if (!isMirrored)
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


    /* [p]: pose index    [j]: joint index      [f]: frame index */
    public void PlayHandMotion(int p, float angle, int hand)
    {

        if (angle < -100.0f || angle > 100.0f)   angle = 100.0f;

        angle *= 10.0f;
        float ratio = angle *Time.deltaTime;
        //Debug.Log("ratio = " + ratio);


        if (hand == 0)
        {
            //int frame_num = L_pose_data[p][3].Count;

            for (int f = 1; f < L_pose_data[p][3].Count; f++)
            {

                for (int j = 3; j < _manualLeftJointNames.Length; j++)
                {
                    Transform l_joints = _transformFromName[_manualLeftJointNames[j]];

                    Quaternion oldRot = l_joints.localRotation;

                    //Quaternion nowRot = Quaternion.Slerp(L_pose_data[p][j][0], L_pose_data[p][j][1], ratio);
                    Quaternion nowRot = Quaternion.Slerp(L_pose_data[p][j][0], L_pose_data[p][j][1], ratio);

                    Quaternion.Slerp(oldRot, nowRot, 1.0f);
                    //l_joints.localRotation = Quaternion.Slerp(L_pose_data[p][j][f], L_pose_data[p][j][f + 1], sec);

                    //Debug.Log("start: L_pose_data[" + p + "][" + j + "][" + f + "]" + ", end: L_pose_data[" + p + "][" + j + "][" + (f + 1) + "]");

                }
            }
        }

        if (hand == 1)
        {
            int frame_num = R_pose_data[p][3].Count;

            for (int f = 0; f < frame_num; f++)
            {

                for (int j = 3; j < _manualRightJointNames.Length; j++)
                {
                    Transform r_joints = _transformFromName[_manualRightJointNames[j]];
                    Quaternion old_rot = r_joints.localRotation;

                    r_joints.localRotation = Quaternion.Slerp(old_rot, R_pose_data[p][j][f], ratio);

                }
            }
        }

    }


    /* [p]: pose index    [j]: joint index      [f]: frame index */
    public IEnumerator RotationDelay2(int p, float angle, int hand)
    {

        float ratio = angle*2 / 100.0f;
        
        if (hand == 0)
        {
            for (float t = 0; t <= angle; t++)
            {
                //for (int f = 0; f < L_pose_data[p][3].Count; f++)
                //{
                    for (int j = 3; j < _manualLeftJointNames.Length; j++)
                    {
                        Transform l_joints = _transformFromName[_manualLeftJointNames[j]];

                        Quaternion oldRot = l_joints.localRotation;

                        //l_joints.localRotation = Quaternion.Slerp(old_rot, L_pose_data[p][j][f], ratio);
                        Quaternion nowRot = Quaternion.Slerp(L_pose_data[p][j][0], L_pose_data[p][j][1], ratio);

                        l_joints.localRotation = Quaternion.Slerp(oldRot, nowRot, t/angle);
                    }
                    yield return new WaitForSeconds(0.0025f);
                //}

                //Debug.Log("ratio: " + ratio);
            }
        }


        if (hand == 1)
        {
            for (float t = 0; t <= angle; t++)
            {
                for (int j = 3; j < _manualRightJointNames.Length; j++)
                {
                    Transform r_joints = _transformFromName[_manualRightJointNames[j]];

                    Quaternion nowRot = Quaternion.Slerp(R_pose_data[p][j][0], R_pose_data[p][j][1], ratio);
                    r_joints.localRotation = nowRot;
                }

                yield return new WaitForSeconds(0.0025f);
            }
        }

        yield break;
    }



    // Rotate hand joints(finger joints) by data of hand motion lists
    /* [p]: pose index    [j]: joint index      [f]: frame index */
    public IEnumerator RotationDelay(int p)
    {
        Debug.Log("Motion Start: " + p);


        // # of joints
        int joint_num = _manualLeftJointNames.Count();


        if (isCoroutine)
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

        isCoroutine = false;
    }

    // Apply to rotate the wrist and map hand motions (considering to elbow rot gap)
    void RotateWrist()
    {
        //_transformFromName["right_wrist"].transform.localRotation = Quaternion.Euler(elbowGap_rot * scalingValue);
        //_transformFromName["right_wrist"].transform.localEulerAngles = elbowGap_rot * scalingValue;

        //Vector3 wrist_angle = _transformFromName["right_wrist"].transform.localEulerAngles = elbowGap_rot * scalingValue;

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


        _transformFromName["right_wrist"].transform.localRotation = Quaternion.Slerp(oldQuat_wrist, Quaternion.Euler(elbowGap_rot * scalingValue), clamp);


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

    void SaveList2Csv(string file_path, List<List<float>> record)
    {
        FileStream fs = new FileStream(file_path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);


        for (int i = 0; i < record.Count; i++)
            sw.WriteLine("{0}, {1}, {2}", record[i][0], record[i][1], record[i][2]);

        Debug.Log("record.Count: " + record.Count);

        //for (int i = 0; i < motion_num.Count; i++)
        //    sw.WriteLine("{0}", motion_num[i]);

        sw.Close();
        fs.Close();


        record.Clear();

        return;
    }

    public void SaveDic2Csv(string file_path, List<Dictionary<int, float>> record)
    {
        FileStream fs = new FileStream(file_path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);


        for (int i = 0; i < record.Count; i++)
        {
            foreach (KeyValuePair<int, float> pairs in record[i])
            {
                sw.WriteLine("{0}, {1}", pairs.Key, pairs.Value);
            }
        }

        Debug.Log("record.Count: " + record.Count);

        //for (int i = 0; i < motion_num.Count; i++)
        //    sw.WriteLine("{0}", motion_num[i]);

        sw.Close();
        fs.Close();


        record.Clear();

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