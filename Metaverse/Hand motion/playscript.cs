using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;




public class playscript : MonoBehaviour
{
    float timer;
    float waitingTime;

    public GameObject sensordata;
    public GameObject smpldata;

    //JEONG JINWOO NEW VAR 
    string[] _Senser10JointNames = new string[] { 
                                                    "pelvis", "spine2", 
                                                    "right_shoulder", "right_elbow", 
                                                    "left_shoulder", "left_elbow", 
                                                    "right_hip", "right_knee", 
                                                    "left_hip", "left_knee" };
    int number_of_IMU = 10;
    bool is_play_avatar = false;
    double QW1, QX1, QY1, QZ1;
    List<float> coordinate_Z = new List<float>
    {
        0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f
    };
    Vector3 CoordinateRotate = new Vector3(0.0f,0.0f,0.0f);
    Quaternion q0;
    List<Quaternion> sensorQuatList = new List<Quaternion>
    {
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
    };
    List<Quaternion> sensorQuatCaliList = new List<Quaternion>
    {
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
        new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
    };
    // for save csv
    List<string[]> csvSaveData = new List<string[]>();
    string[] tempQuat;
    bool is_recording = false;

    //JEONG JINWOO NEW VAR
    public void GET_SENSOR_QDATA() // �����ո� ������ �Ŷ� ���� �ΰ��� ��� �ɴϴ�. Ǯ Ʈ��ŷ�ҰŸ� �ٸ� �Լ� ������ �ݺ��� ������
    {


        sensordata = GameObject.Find("Xsens");
        number_of_IMU = sensordata.GetComponent<XsensManage>().sensors.Count;

        for (int i = 0; i < number_of_IMU; i++)
        {
            QW1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatW;
            QX1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatX;
            QY1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatY;
            QZ1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatZ;

            q0.w = (float)QW1; q0.x = (float)QX1; q0.y = (float)QY1; q0.z = (float)QZ1;                                    
            sensorQuatList[i] = q0;


            if (is_recording)
            {
                if (i == 0)
                {
                    tempQuat = new string[40];
                }

                tempQuat[4 * i + 0] = q0.w.ToString(); tempQuat[4 * i + 1] = q0.x.ToString(); tempQuat[4 * i + 2] = q0.y.ToString(); tempQuat[4 * i + 3] = q0.z.ToString();
                if (i == 9)
                {
                    csvSaveData.Add(tempQuat);
                    
                }
            }
        }
        //Debug.LogFormat("���������� �׽�Ʈ:\n{0}, {1}, {2}, {3}", q0.w, q0.x, q0.y, q0.z);
        if (is_play_avatar)
        {
            smpldata = GameObject.Find("smplx-neutral-se");

            for (int i = 0; i < number_of_IMU; i++) 

            {
                Quaternion coord = Quaternion.Euler(-90.0f, 180.0f - coordinate_Z[i], 0.0f);
                Quaternion coord_I = Quaternion.Inverse(coord);
                smpldata.GetComponent<SMPLX>().SetWorld2LocalJointRotation(_Senser10JointNames[i], coord * sensorQuatList[i] * sensorQuatCaliList[i] * coord_I);

            }

        }


    }
    private void SaveCSV()
    {
        string filePath = "test.csv"; // ������ ���� ��� �� �̸�
        StreamWriter sw = new StreamWriter(filePath, true); // true: ���� ���Ͽ� �̾� ����

        foreach (string[] row in csvSaveData)
        {
            string line = string.Join(",", row); // �迭�� �� ��Ҹ� ��ǥ�� �����Ͽ� ���ڿ� ����
            sw.WriteLine(line); // ���Ͽ� �� �� �� ����
        }

        sw.Close(); // ���� �ݱ�
    }
    public void GET_CALIB_POSE()
    {
        for (int i = 0; i < number_of_IMU; i++)
        {
            sensorQuatCaliList[i] = Quaternion.Inverse(sensorQuatList[i]);
        }
    }
    public void ALIGN_COORDINATE() 
    {
        Debug.Log("Coordinate Aligned.");

        for (int i = 0; i < number_of_IMU; i++)
        {
            Vector3 Print_current_ori = Quaternion.ToEulerAngles(sensorQuatList[i]);

            CoordinateRotate.x = (float)ConvertRadiansToDegrees(Print_current_ori.x);
            CoordinateRotate.y = (float)ConvertRadiansToDegrees(Print_current_ori.y);
            CoordinateRotate.z = (float)ConvertRadiansToDegrees(Print_current_ori.z);

            Debug.LogFormat("{0}�� ���� Calib���� ��: X: {1}, Y: {2}, Z: {3}", i,

                                                                       CoordinateRotate.x,
                                                                        CoordinateRotate.y,
                                                                        CoordinateRotate.z);
            //��� ������ align ������ �Է�.
            coordinate_Z[i] = CoordinateRotate.z;

        }

    }
    public static double ConvertRadiansToDegrees(double radians)
    {
        double degrees = (180 / Math.PI) * radians;
        return (degrees);
    }
    public void KEYBOARD_INPUT_CASE()
    {
        // W , E, M, X, TŰ�� �̹� �Ҵ�Ǿ�����.
        // C: T���� ������ ������ Calibrate
        // P : ���� ��ü ���� ������ ���(1ȸ)
        // S : �ƹ�Ÿ �����̴� �� ����.


        //if (Input.GetKeyDown(KeyCode.Comma))
        //{
        //    Debug.Log("Keyboard: , is pressed.\n��ȭ�� �����մϴ�.");
        //    is_recording = true;
        //}
        //if (Input.GetKeyDown(KeyCode.Period))
        //{
        //    Debug.Log("Keyboard: . is pressed.\n��ȭ�� �����մϴ�.");
        //    is_recording = false;
        //    SaveCSV();
        //}
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Keyboard: C is pressed.\n T-Pose ������ ���� ������ �����Ͽ����ϴ�.");
            GET_CALIB_POSE();
            for (int i = 0; i < number_of_IMU; i++)
            {
                Debug.LogFormat("{0}�� ���� Ķ���극�̼�: W: {1}, X: {2}, Y: {3}, Z: {4}",
                                i,
                                sensorQuatCaliList[i].w,
                                sensorQuatCaliList[i].x,
                                sensorQuatCaliList[i].y,
                                sensorQuatCaliList[i].z);
            }

        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Keyboard: V is pressed.\n������ Heading Reset. ����̽����� �߻��ϴ� �帮��Ʈ ������ �����մϴ�."); 
            ALIGN_COORDINATE();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Keyboard: P is pressed.\nPrint all Sensor Data in Euler Angle.");
            for (int i = 0; i < number_of_IMU; i++)
            {
                Vector3 Print_current_ori = Quaternion.ToEulerAngles(sensorQuatList[i]);
                CoordinateRotate.x = (float)ConvertRadiansToDegrees(Print_current_ori.x);
                CoordinateRotate.y = (float)ConvertRadiansToDegrees(Print_current_ori.y);
                CoordinateRotate.z = (float)ConvertRadiansToDegrees(Print_current_ori.z);

                Debug.LogFormat("{0}�� ���� Calib���� ��: X: {1}, Y: {2}, Z: {3}", i,

                                                                           CoordinateRotate.x,
                                                                            CoordinateRotate.y,
                                                                            CoordinateRotate.z);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Keyboard: S is pressed.\n Start avatar estimation.");
            is_play_avatar = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Playerscript ���� �Ǿ����ϴ�.");
        timer = 0.0f;
        waitingTime = 0.01667f;
        //waitingTime = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        KEYBOARD_INPUT_CASE();
        timer += Time.deltaTime;
        //Debug.LogFormat("{0}", timer);
        if (timer > waitingTime)
        {
            GET_SENSOR_QDATA();
            timer = 0;
        }

    }



}
