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
    int edge_counter, middle_counter, wave_counter = 0;

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
    public Text printMessage_1;
    public Text printMessage_2;
    bool is_standby = false;
    public bool is_ready_to_order = false;
    bool is_playing_avatar;
    bool is_ready_to_start;
    bool x_in_boundary, y_in_boundary, z_in_boundary;




    // Start is called before the first frame update
    void Start()
    {
        waitingTime = 0.01667f;
        timer = 0.0f;

        from_play_script = GameObject.Find("Xsens");
        smpldata = GameObject.Find("smplx-neutral-se");
        gesture_feedback = GameObject.Find("border_gesture");


        printMessage_1 = GameObject.Find("print_msg_1").GetComponent<Text>();
        printMessage_2 = GameObject.Find("print_msg_2").GetComponent<Text>();
        //is_playing_avatar = GameObject.Find("pr");

        joint_R_elbow = smpldata.GetComponent<SMPLX>()._transformFromName["right_elbow"];
        joint_R_shoulder = smpldata.GetComponent<SMPLX>()._transformFromName["right_shoulder"];
        joint_R_wirst = smpldata.GetComponent<SMPLX>()._transformFromName["right_wrist"];

        SoundEffect = gesture_feedback.GetComponent<AudioSource>();
        SoundEffect.mute = false;
        SoundEffect.loop = false;
        SoundEffect.Play();
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
        current_wrist_position.x = joint_R_wirst.position.x;
        current_wrist_position.y = joint_R_wirst.position.y;
        current_wrist_position.z = joint_R_wirst.position.z;

        string print_message =  current_wrist_position.x.ToString("F3") + "(-0.75 ~ -0.25)\n" +
                                current_wrist_position.y.ToString("F3") + "( 2.0 ~  2.4)\n" +
                                current_wrist_position.z.ToString("F3") + "(-0.6 ~ 0.0)";

        printMessage_1.text = print_message;
        /////////////////함수화 고려부분///////////////////////////
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
            //printMessage_2.text = "제스쳐 스탠바이 모드로 진입합니다.";
            //Debug.Log("제스쳐 스탠바이 모드로 진입합니다.");
            is_standby = true;
            standby_counter = 0;
            other_pose_counter = 0;
            direction = "Ready";

            stop_vector = joint_R_wirst.position - joint_R_elbow.position;
            //stop_position = joint_R_wirst.position;

        }
        if (other_pose_counter >= 120)
        {
            //printMessage_2.text = "제스처 스탠바이모드가 아닙니다.";
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


            moving_vector = joint_R_wirst.position - joint_R_elbow.position;
            dot = Vector3.Dot(stop_vector, moving_vector);
            angle = Vector3.Angle(stop_vector, moving_vector);
            //string print_message = dot.ToString() + "\n" + angle.ToString();
            string print_message = "제스쳐 인식 준비완료. 현재 변화된 각도:\n" + angle.ToString();


            //string print_message = "Diif_X:" + rotation_difference.x.ToString("F3") + "\n" +
            //                       "Diif_Y:" + rotation_difference.y.ToString("F3") + "\n" +
            //                       "Diif_Z:" + rotation_difference.z.ToString("F3");
            printMessage_1.text = print_message;

            if (current_wrist_position.y > 2.8 && angle > 50) 
            {
                //Debug.Log("위쪽 제스쳐입니다.");
                printMessage_2.text = "위쪽 제스쳐입니다.";
                direction = "Up";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;
            }
            if (current_wrist_position.y < 1.85 && angle > 50)
            {
                //Debug.Log("아래쪽 제스쳐입니다.");
                printMessage_2.text = "아래쪽 제스쳐입니다.";
                direction = "Down";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }
            if (current_wrist_position.x > -0.1 && angle > 50)
            {
                //Debug.Log("왼쪽 제스쳐입니다.");
                printMessage_2.text = "왼쪽 제스쳐입니다.";
                direction = "Left";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }
            if (current_wrist_position.x < -0.85 && angle > 50)
            {
                //Debug.Log("오른쪽 제스쳐입니다.");
                printMessage_2.text = "오른쪽 제스쳐입니다.";
                direction = "Right";

                is_standby = false;
                standby_counter = 0;
                other_pose_counter = 0;

            }

        }
    }

    void StartKioskMotion() 
    {
        if (!is_ready_to_start) 
        {
            current_elbow_position = joint_R_elbow.position;
            current_wrist_position = joint_R_wirst.position;

            Vector3 temp_for_check = current_wrist_position - current_elbow_position;
            string testmsg = "X : " + temp_for_check.x.ToString("F3") + "\n" +
                             "Y : " + temp_for_check.y.ToString("F3") + "\n" +
                             "Z : " + temp_for_check.z.ToString("F3");
            printMessage_2.text = testmsg;


            if (temp_for_check.y > 0.38) { start_counter++; }
            else { start_counter = 0; }

            if (start_counter > 30) // 30프레임동안 손 위로 들고 있으면 흔들기 준비자세(짧다!)
            {
                stop_vector = joint_R_wirst.position - joint_R_elbow.position;
                is_ready_to_start = true;
            }
        }
        if (is_ready_to_start) 
        {
            moving_vector = joint_R_wirst.position - joint_R_elbow.position;
            angle = Vector3.Angle(stop_vector, moving_vector);
            printMessage_2.text = "벡터 내적의 차이:" + angle.ToString("F3") + "\nWave_count:" + wave_counter.ToString();
            //////
            if (angle > 40 && edge_counter == middle_counter) 
            {
                edge_counter++;
            }
            if (angle < 15 && edge_counter > middle_counter) 
            {
                middle_counter++;
                wave_counter++;
            }

            if (wave_counter > 5) 
            {
                Debug.Log("졸라흔들기 성공. 시작합니다.");
                printMessage_1.text = "손 열심히 흔들었구나... 이제 메뉴 받아줄게";
                edge_counter = 0;
                middle_counter = 0;
                wave_counter = 0;
                start_counter = 0;
                is_ready_to_order = true;
                is_ready_to_start = false;

            }

            /////////////////////////////////////////대기시간 초기화/////////////////////////////////////////////////
            time_limit++;
            if (time_limit > 300) 
            {
                Debug.Log("흔들기 실패. 다시 시작하세요.");

                edge_counter = 0;
                middle_counter = 0;
                start_counter = 0;
                wave_counter = 0;
                time_limit = 0;
                is_ready_to_start = false;
            }
             // 5초동안 안흔들면 초기값으로 처음으로 돌아가라
        }

        //1. 두 포지션의 Y축 차이가 일정 값 이상일때,(하박을 든다고 인식할때) 스타트_스탠바이모션으로 전환.
        //   이때, 해당 포지션 값을 갖고 와서 stop벡터로 얼려버림 
        //2. stop벡터와 moving벡터의 차이를 계산해서 사이각을 구함. 사이각은 항상 0 ~ N(<180)사이에 존재. 
        //   1) 5초의 제한시간이 있습니다. 2) 5초안에 좌로 흔들고 중앙찍고 
    }
    void Update()
    {
        is_playing_avatar = from_play_script.GetComponent<playscript>().is_play_avatar;
        timer += Time.deltaTime;

        if (is_playing_avatar)
        {
            if (timer > waitingTime)
       
            {
                if (!is_ready_to_order) 
                {
                    StartKioskMotion();
                }
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
