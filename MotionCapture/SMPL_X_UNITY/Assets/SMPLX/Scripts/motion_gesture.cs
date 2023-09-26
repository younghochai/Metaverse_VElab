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
    Transform joint_R_elbow;
    Transform joint_R_shoulder;
    Transform joint_R_wirst;

    Vector3 current_position, stop_position;
    Vector3 current_rotation, stop_rotation;
    Vector3 rotation_difference;
    int standby_counter = 0;
    int other_pose_counter = 0;
    float timer;
    float waitingTime;

    string[] _Senser10JointNames = new string[] {
                                                    "pelvis", "spine2",
                                                    "right_shoulder", "right_elbow",
                                                    "left_shoulder", "left_elbow",
                                                    "right_hip", "right_knee",
                                                    "left_hip", "left_knee" };
    public Text printMessage_1;
    public Text printMessage_2;
    bool is_standby = false;
    bool is_playing_avatar;
    bool x_in_boundary, y_in_boundary, z_in_boundary;



    // Start is called before the first frame update
    void Start()
    {
        waitingTime = 0.01667f;
        timer = 0.0f;

        from_play_script = GameObject.Find("Xsens");
        smpldata = GameObject.Find("smplx-neutral-se");

        printMessage_1 = GameObject.Find("print_msg_1").GetComponent<Text>();
        printMessage_2 = GameObject.Find("print_msg_2").GetComponent<Text>();
        //is_playing_avatar = GameObject.Find("pr");

        joint_R_elbow = smpldata.GetComponent<SMPLX>()._transformFromName["right_elbow"];
        joint_R_shoulder = smpldata.GetComponent<SMPLX>()._transformFromName["right_shoulder"];
        joint_R_wirst = smpldata.GetComponent<SMPLX>()._transformFromName["right_wrist"];

        //play script에 있는 플레잉 변수 들고 와서 아바타 플레이시 동시에 구문진행되게 함
        printMessage_1.text =
            "HELLO UNITY!\n" +
            "THIS IS THREE LINE TEXT!\n" +
            "THIS LINE WILL SHOW YOU THAT SENSOR ROTATION VALUE!";
        printMessage_2.text =
            "HELLO UNITY!\n" +
            "THIS IS THREE LINE TEXT!\n" +
            "THIS LINE WILL SHOW YOU THAT SENSOR ROTATION VALUE!";

    }

    // Update is called once per frame

    void Standby() 
    {
        current_position.x = joint_R_wirst.position.x;
        current_position.y = joint_R_wirst.position.y;
        current_position.z = joint_R_wirst.position.z;

        //string print_message = current_position.x.ToString("F3") + "(-0.75 ~ -0.25)\n" +
        //                        current_position.y.ToString("F3") + "( 1.7 ~  2.3)\n" +
        //                        current_position.z.ToString("F3") + "(-0.3 ~ 0.3)";

        //printMessage_1.text = print_message;
        /////////////////함수화 고려부분///////////////////////////
        if (current_position.x > -0.75 && current_position.x < -0.25) x_in_boundary = true; else x_in_boundary = false;
        if (current_position.y > 2.0 && current_position.y < 2.4) y_in_boundary = true; else y_in_boundary = false;
        if (current_position.z > -0.6 && current_position.z < 0.0) z_in_boundary = true; else z_in_boundary = false;

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


        if (standby_counter >= 150)
        {
            printMessage_2.text = "제스쳐 스탠바이 모드로 진입합니다.";
            //Debug.Log("제스쳐 스탠바이 모드로 진입합니다.");
            is_standby = true;
            standby_counter = 0;
            other_pose_counter = 0;
            //stop_rotation.x = joint_R_elbow.localEulerAngles.x;
            //stop_rotation.y = joint_R_elbow.localEulerAngles.y;
            //stop_rotation.z = joint_R_elbow.localEulerAngles.z;


            stop_rotation.x = joint_R_elbow.eulerAngles.x;
            stop_rotation.y = joint_R_elbow.eulerAngles.y;
            stop_rotation.z = joint_R_elbow.eulerAngles.z;
        }
        if (other_pose_counter >= 120)
        {
            printMessage_2.text = "제스처 스탠바이모드가 아닙니다.";
            //Debug.Log("제스처 스탠바이모드가 아닙니다.");
            is_standby = false;
            standby_counter = 0;
            other_pose_counter = 0;
        }
        /////////////////함수화 고려부분///////////////////////////
    }

    void Gesture_recognization() 
    {
        if (is_standby) 
        {
            //current_rotation.x = joint_R_elbow.localEulerAngles.x;
            //current_rotation.y = joint_R_elbow.localEulerAngles.y;
            //current_rotation.z = joint_R_elbow.localEulerAngles.z;

            current_rotation.x = joint_R_elbow.eulerAngles.x;
            current_rotation.y = joint_R_elbow.eulerAngles.y;
            current_rotation.z = joint_R_elbow.eulerAngles.z;



            rotation_difference = stop_rotation - current_rotation;

            string print_message = "Diif_X:" + rotation_difference.x.ToString("F3") + "\n" +
                                   "Diif_Y:" + rotation_difference.y.ToString("F3") + "\n" +
                                   "Diif_Z:" + rotation_difference.z.ToString("F3");
            printMessage_1.text = print_message;

            //if(rotation_difference의 xyz중 하나(혹은 두개)가 주어진 각도를 넘어섰을 때)
            //  이것은 손을 흔드는 제스처다
            //  제스처에 따른 키오스크 액션 시작
            
            // 제스처 수만큼 반복... 
        }
    }
    void Update()
    {
        is_playing_avatar = from_play_script.GetComponent<playscript>().is_play_avatar;
        timer += Time.deltaTime;

        if (is_playing_avatar)
        {
            if (timer > waitingTime)
       
            {
                Standby();


                Gesture_recognization();


                timer = 0;
            }

        }
    }
}
