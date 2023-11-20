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

public class motion_gesture : MonoBehaviour
{
    public GameObject from_play_script;
    public GameObject smpldata;
    public GameObject gesture_feedback;
    Transform joint_R_elbow;
    Transform joint_R_shoulder;
    Transform joint_R_wirst;
    AudioSource SoundEffect;

    const float PI = 3.141592f;
    Vector3 current_wrist_position, current_elbow_position ,stop_position, stop_vector, moving_vector;

    //Vector3 current_rotation, stop_rotation;
    //Vector3 rotation_difference;
    int standby_counter = 0;
    int other_pose_counter = 0;
    int start_counter = 0;
    int time_limit = 0;
    int edge_counter, middle_counter, wave_counter, delay_counter = 0;

    float dot, angle;
    float timer;
    float waitingTime;

    public string direction = "Default"; // Kiosk Trigger

    string[] _Senser10JointNames = new string[] {
                                                    "pelvis", "spine2",
                                                    "right_shoulder", "right_elbow",
                                                    "left_shoulder", "left_elbow",
                                                    "right_hip", "right_knee",
                                                    "left_hip", "left_knee" };
    public Text printMessage_2;
    bool is_standby = false;
    public bool is_ready_to_order = false;
    bool is_playing_avatar;
    bool is_ready_to_start;
    bool x_in_boundary, y_in_boundary, z_in_boundary;
    bool is_input_delay;

    void COMMAND_WITH_ARROWS()
    {
        //direction_from_motion_gesture = "Defualt"; //�������� ��Ʈ���Ҷ��¿��� ��Ȱ��ȭ ���Ѿ���
        //ȭ��ǥŰ�� �𷺼� �� �ֱ�.
        if (Input.GetKeyDown(KeyCode.R)){is_ready_to_order = true; }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { direction = "Up"; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { direction = "Down"; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { direction = "Left"; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { direction = "Right"; }
    }

    // Start is called before the first frame update
    void Start()
    {
        waitingTime = 0.01667f;
        timer = 0.0f;

        from_play_script = GameObject.Find("Xsens"); // ������ ���ʹϾ� ��� ���� ���ؼ�....
        smpldata = GameObject.Find("smplx-neutral-se"); // �ȸ��̳� ���, �ȶ��� ��ġ, ������ ������� ���� smpl���� ����
        gesture_feedback = GameObject.Find("border_gesture"); // ����� �ǵ���� �ַ��� ��...����.

        is_ready_to_order = true;
        //printMessage_1 = GameObject.Find("print_msg_1").GetComponent<Text>();
        printMessage_2 = GameObject.Find("print_msg_2").GetComponent<Text>();

        joint_R_elbow = smpldata.GetComponent<SMPLX>()._transformFromName["right_elbow"];
        joint_R_shoulder = smpldata.GetComponent<SMPLX>()._transformFromName["right_shoulder"];
        joint_R_wirst = smpldata.GetComponent<SMPLX>()._transformFromName["right_wrist"];


        //play script�� �ִ� �÷��� ���� ��� �ͼ� �ƹ�Ÿ �÷��̽� ���ÿ� ��������ǰ� ��
        //printMessage_1.text =


    }

    // Update is called once per frame

    void Standby() 
    {
        current_wrist_position.x = joint_R_wirst.position.x;
        current_wrist_position.y = joint_R_wirst.position.y;
        current_wrist_position.z = joint_R_wirst.position.z;

        string print_message =  current_wrist_position.x.ToString("F3") + "(-0.75 ~ -0.25)\n" +
                                current_wrist_position.y.ToString("F3") + "( 2.0 ~  2.4)\n" +
                                current_wrist_position.z.ToString("F3") + "(-0.6 ~ 0.0)";

        //printMessage_1.text = print_message;
        /////////////////�Լ�ȭ ����κ�///////////////////////////
        if (current_wrist_position.x > -0.75 && current_wrist_position.x < -0.25) x_in_boundary = true; else x_in_boundary = false;
        if (current_wrist_position.y > 2.0 && current_wrist_position.y < 2.4) y_in_boundary = true; else y_in_boundary = false;
        if (current_wrist_position.z > -0.6 && current_wrist_position.z < 0.0) z_in_boundary = true; else z_in_boundary = false;

        if (x_in_boundary && y_in_boundary && z_in_boundary)
        {
            other_pose_counter = 0;
            standby_counter++;
        }
        else
        {
            standby_counter = 0;
            other_pose_counter++;

        }


        if (standby_counter >= 45)
        {
            printMessage_2.text = "������ ���Ĺ��� ���� �����մϴ�.";
            Debug.Log("������ ���Ĺ��� ���� �����մϴ�.");
            is_standby = true;
            standby_counter = 0;
            other_pose_counter = 0;
            direction = "Ready";

            stop_vector = joint_R_wirst.position - joint_R_elbow.position;
            //stop_position = joint_R_wirst.position;

        }
        if (other_pose_counter >= 120)
        {
            printMessage_2.text = "����ó ���Ĺ��̸�尡 �ƴմϴ�.";
            Debug.Log("����ó ���Ĺ��̸�尡 �ƴմϴ�.");
            is_standby = false;
            standby_counter = 0;
            other_pose_counter = 0;
        }
        /////////////////�Լ�ȭ ����κ�///////////////////////////
    }

    void Gesture_recognization() 
    {
        if (is_standby) 
        {


            moving_vector = joint_R_wirst.position - joint_R_elbow.position;
            dot = Vector3.Dot(stop_vector, moving_vector);
            angle = Vector3.Angle(stop_vector, moving_vector);
            //string print_message = dot.ToString() + "\n" + angle.ToString();
            string print_message = "������ �ν� �غ�Ϸ�. ���� ��ȭ�� ����:\n" + angle.ToString();


            //string print_message = "Diif_X:" + rotation_difference.x.ToString("F3") + "\n" +
            //                       "Diif_Y:" + rotation_difference.y.ToString("F3") + "\n" +
            //                       "Diif_Z:" + rotation_difference.z.ToString("F3");
            //printMessage_1.text = print_message;

            if (current_wrist_position.y > 2.8 && angle > 50) 
            {
                Debug.Log("���� �������Դϴ�.");
                printMessage_2.text = "���� �������Դϴ�.";
                direction = "Up";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;
            }
            if (current_wrist_position.y < 1.85 && angle > 50)
            {
                Debug.Log("�Ʒ��� �������Դϴ�.");
                printMessage_2.text = "�Ʒ��� �������Դϴ�.";
                direction = "Down";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }
            if (current_wrist_position.x > -0.1 && angle > 50)
            {
                Debug.Log("���� �������Դϴ�.");
                printMessage_2.text = "���� �������Դϴ�.";
                direction = "Left";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }
            if (current_wrist_position.x < -0.85 && angle > 50)
            {
                Debug.Log("������ �������Դϴ�.");
                printMessage_2.text = "������ �������Դϴ�.";
                direction = "Right";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }

        }
    }

    //void StartKioskMotion() 
    //{
    //    if (!is_ready_to_start) 
    //    {
    //        current_elbow_position = joint_R_elbow.position;
    //        current_wrist_position = joint_R_wirst.position;

    //        Vector3 temp_for_check = current_wrist_position - current_elbow_position;
    //        string testmsg = "X : " + temp_for_check.x.ToString("F3") + "\n" +
    //                         "Y : " + temp_for_check.y.ToString("F3") + "\n" +
    //                         "Z : " + temp_for_check.z.ToString("F3");
    //        //printMessage_2.text = testmsg;


    //        if (temp_for_check.y > 0.38) { start_counter++; }
    //        else { start_counter = 0; }

    //        if (start_counter > 30) // 30�����ӵ��� �� ���� ��� ������ ���� �غ��ڼ�(ª��!)
    //        {
    //            stop_vector = joint_R_wirst.position - joint_R_elbow.position;
    //            is_ready_to_start = true;
    //        }
    //    }
    //    if (is_ready_to_start) 
    //    {
    //        moving_vector = joint_R_wirst.position - joint_R_elbow.position;
    //        angle = Vector3.Angle(stop_vector, moving_vector);
    //        //printMessage_2.text = "���� ������ ����:" + angle.ToString("F3") + "\nWave_count:" + wave_counter.ToString();
    //        //////
    //        if (angle > 40 && edge_counter == middle_counter) 
    //        {
    //            edge_counter++;
    //        }
    //        if (angle < 15 && edge_counter > middle_counter) 
    //        {
    //            middle_counter++;
    //            wave_counter++;
    //            Debug.Log("����弼��.");

    //        }

    //        if (wave_counter > 3) 
    //        {
    //            Debug.Log("�������� ����. �����մϴ�.");
    //            //printMessage_1.text = "�� ������ ��������... ���� �޴� �޾��ٰ�";
    //            edge_counter = 0;
    //            middle_counter = 0;
    //            wave_counter = 0;
    //            start_counter = 0;
    //            is_ready_to_order = true;
    //            is_ready_to_start = false;

    //        }

    //        /////////////////////////////////////////���ð� �ʱ�ȭ/////////////////////////////////////////////////
    //        time_limit++;
    //        if (time_limit > 300) 
    //        {
    //            Debug.Log("���� ����. �ٽ� �����ϼ���.");

    //            edge_counter = 0;
    //            middle_counter = 0;
    //            start_counter = 0;
    //            wave_counter = 0;
    //            time_limit = 0;
    //            is_ready_to_start = false;
    //        }
    //         // 5�ʵ��� ������ �ʱⰪ���� ó������ ���ư���
    //    }

    }
    void Update()
    {
        //COMMAND_WITH_ARROWS();
        is_playing_avatar = from_play_script.GetComponent<playscript>().is_play_avatar;
        timer += Time.deltaTime;

        if (is_playing_avatar)
        {
            //is_input_delay = GameObject.Find("Screen").GetComponent<kiosk>().is_loop_once_finished;
            //if (is_input_delay)
            //{
            //    direction = "Defualt";
            //    is_input_delay = false;
            //}
            if (timer > waitingTime)
            {

                //if (!is_ready_to_order) 
                //{
                //    StartKioskMotion();
                //}
                if (is_ready_to_order) 
                {
                    Standby();
                    Gesture_recognization();
                }



                timer = 0;
            }

        }
    }
}
