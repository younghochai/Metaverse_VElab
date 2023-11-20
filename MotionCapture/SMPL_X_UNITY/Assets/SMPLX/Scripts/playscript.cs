using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    string[] _Senser8JointNames_UP_RIGHT_PART = new string[] {
                                                    "pelvis", "spine1","spine2","spine3",
                                                    "right_collar","right_shoulder", "right_elbow","right_wrist","right_hip","left_hip"}; // ���� �ΰ��� ����.
    string[] _Senser8JointNames_UP_LEFT_PART = new string[] {
                                                    "pelvis", "spine1","spine2","spine3",
                                                    "left_collar","left_shoulder", "left_elbow","left_wrist","right_hip","left_hip"};// ���� �ΰ��� ����.
    int number_of_IMU = 10;
    public bool is_play_avatar = false;
    bool is_printPoint = false;
    double QW1, QX1, QY1, QZ1;

    bool is_wait_calibration = false;
    int cali_waiting_counter = 0;

    List<float> coordinate_X = new List<float>{0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f};
    List<float> coordinate_Y= new List<float> { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
    List<float> coordinate_Z = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};

    Vector3 CoordinateRotate = new Vector3(0.0f,0.0f,0.0f);
    Vector3 QuatToEuler = new Vector3(0.0f, 0.0f, 0.0f);



    Quaternion temp_quaternion;
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

    List<Quaternion> final_Input_List = new List<Quaternion>
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
    public void GET_SENSOR_QDATA() 
    {


        sensordata = GameObject.Find("Xsens");
        smpldata = GameObject.Find("smplx-neutral-se"); //�ٸ� ���� �ҷ����⵵ �����ϰ� ��

        //number_of_IMU = sensordata.GetComponent<XsensManage>().sensors.Count;
        number_of_IMU = 6;


        for (int i = 0; i < number_of_IMU; i++)
        {
            QW1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatW;
            QX1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatX;
            QY1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatY;
            QZ1 = sensordata.GetComponent<XsensManage>().sensors[i].QuatZ;

            temp_quaternion.w = (float)QW1; 
            temp_quaternion.x = (float)QX1; 
            temp_quaternion.y = (float)QY1; 
            temp_quaternion.z = (float)QZ1;
            
            sensorQuatList[i] = temp_quaternion;


            if (is_play_avatar)
            {

                Quaternion coord = Quaternion.Euler(-90.0f, 180.0f, 0.0f);//ORIGINAL
                Quaternion coord_I = Quaternion.Inverse(coord);
                Quaternion heading_reset = Quaternion.Euler(-coordinate_X[i], -coordinate_Z[i], -coordinate_Y[i]);
                Quaternion heading_reset_I = Quaternion.Inverse(heading_reset);

                final_Input_List[i] = heading_reset * coord * sensorQuatList[i] * sensorQuatCaliList[i] * coord_I * heading_reset_I;

                smpldata.GetComponent<SMPLX>().SetWorld2LocalJointRotation(_Senser10JointNames[i],
                    heading_reset * coord * sensorQuatList[i] * sensorQuatCaliList[i] * coord_I * heading_reset_I);

            }
            if (is_recording)
            {
                if (i == 0)
                {
                    tempQuat = new string[24];
                    //tempQuat = new string[18];
                }

                tempQuat[4 * i + 0] = final_Input_List[i].w.ToString();
                tempQuat[4 * i + 1] = final_Input_List[i].x.ToString();
                tempQuat[4 * i + 2] = final_Input_List[i].y.ToString();
                tempQuat[4 * i + 3] = final_Input_List[i].z.ToString();


                if (i == number_of_IMU-1)
                {
                    csvSaveData.Add(tempQuat);

                }
            }
        }


    }
    private void SaveCSV()// 
    {

        DateTime now = DateTime.Now; // ���� ��¥�� �ð� ��������
        string dateTimeString = now.ToString("MM_dd_HH_mm_ss");
        Debug.Log(dateTimeString);
        string file_Name = dateTimeString + "_OUTPUT.csv";// ��� ���� �����ϱ�.
        string filePath = file_Name; // ������ ���� ��� �� �̸�
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
    public void ALIGN_COORDINATE() //IMU Heading reset.
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

            coordinate_X[i] = CoordinateRotate.x;
            coordinate_Y[i] = CoordinateRotate.y;
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


        if (Input.GetKeyDown(KeyCode.O)) 
        {
            is_printPoint = true;
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Debug.Log("Keyboard: , is pressed.\n��ȭ�� �����մϴ�.");
            csvSaveData = new List<string[]>();
            is_recording = true;
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            Debug.Log("Keyboard: . is pressed.\n��ȭ�� �����մϴ�.");
            is_recording = false;
            SaveCSV();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Keyboard: C is pressed.\n T-Pose ������ ���� ������ �����Ͽ����ϴ�.");
            //ȥ�ڼ� T��� ���� �����ð� �߰�.
          
            Debug.Log("��� �����ֽʽÿ�...");
            //GET_CALIB_POSE();


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
                Vector3 Print_current_ori = Quaternion.ToEulerAngles(sensorQuatList[i] * sensorQuatCaliList[i]);
                CoordinateRotate.x = (float)ConvertRadiansToDegrees(Print_current_ori.x);
                CoordinateRotate.y = (float)ConvertRadiansToDegrees(Print_current_ori.y);
                CoordinateRotate.z = (float)ConvertRadiansToDegrees(Print_current_ori.z);

                Debug.LogFormat("{0}�� ���� Calib���� ��: X: {1}, Y: {2}, Z: {3}", i,

                                                                           CoordinateRotate.x,
                                                                            CoordinateRotate.y,
                                                                            CoordinateRotate.z);

                //Vector3 xyzprint =  smpldata.GetComponent<SMPLX>().PrintLocalRotation(_Senser8JointNames_UP_RIGHT_PART[i]);
                //Debug.LogFormat("{0}�κ� ȸ�� ��: X: {1}, Y: {2}, Z: {3}", _Senser8JointNames_UP_RIGHT_PART[i],

                //                                                           xyzprint.x,
                //                                                            xyzprint.y,
                //                                                            xyzprint.z);
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

            //���⿡�� tpsoe �����ð� �߰�
            if (is_wait_calibration)
            {
                cali_waiting_counter++;
                Debug.Log("�ڼ��� �����ּ���.");
            }
            if (cali_waiting_counter > 300)
            {
                GET_CALIB_POSE();
                Debug.Log("Ķ���극�̼� �Ϸ�!");

                cali_waiting_counter = 0;
                is_wait_calibration = false;
            }

            timer = 0;
        }

    }



}
