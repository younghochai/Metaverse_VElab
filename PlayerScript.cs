using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEditor;
using System.Windows.Forms;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    TextAsset csvFile;
    string[] records;
    string[] fields;
    public TextMeshProUGUI RLTText;


    Quaternion initQT, Revinit, initRotation, relativeAngle, northValue;
    Quaternion R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, R13, R14, R15;
    public Quaternion angle, target, frontRotate;
    public GameObject BodyObject;
    float Radian2Degree = 180 / Mathf.PI;
    float Degree2Radian = Mathf.PI / 180;

    public GameObject Head, Hips, Spine, LeftUpperArm, LeftForeArm, LeftHand, RightUpperArm, RightForeArm, RightHand;

    public GameObject XsensDotManager;


    public bool _00B43808 = false;
    public bool _00B438AE = false;
    public bool _00B438C7 = false;
    public bool _00B43923 = false;
    public bool _00B43926 = false;
    public bool _00B4391F = false;

    public bool startTracking = false;
    bool startData_recording = false;
    bool initCheck = true;

    public double EulerX, EulerY, EulerZ;
    public double quatW, quatX, quatY, quatZ;
    int index;

    public XsensDot manager;



    public Rigidbody RB;
    public PhotonView PV;
    public TextMeshProUGUI NickNameText;
    public float speed = 10f;
    public float rotatespeed = 10f;

    bool startDotdata = false;
    bool isGround;
    Vector3 curPos;
    Quaternion curRot;
    bool toggle_rot = false;
    Vector3 movement;
    float horizontal;
    float vertical;
    Quaternion senRot_pel, senRot_spine, senRot_RUA, senRot_RLA, senRot_LUA, senRot_LLA;
    Quaternion refRot_pel, refRot_spine, refRot_RUA, refRot_RLA, refRot_LUA, refRot_LLA;


    Quaternion seninit_pel, seninitRot_spine, seninitRot_RUA, seninitRot_RLA, seninitRot_LUA, seninitRot_LLA;

    bool[] first_frm = new bool[6] { true, true, true, true, true, true };


    List<Quaternion> load_quat = new List<Quaternion>();
    List<Quaternion> prev_loaded_pose = new List<Quaternion>();
    List<Quaternion> inint_pose_quatnew = new List<Quaternion>();
    List<Quaternion> sensing_quat = new List<Quaternion>();
    List<Quaternion> sensing_init_quat = new List<Quaternion>();


    // Action recognition variable

    List<Vector3> init_pose_vector = new List<Vector3>();
    bool first_frame = true;
    enum Avatar_state { Standing, RAF90, RAO90, RAF180 };
    Avatar_state cur_state;
    Dictionary<string, Vector3> rot_direction = new Dictionary<string, Vector3>()
    {
      { "RF", new Vector3(1,0,0) }, // Raise front
      { "LF", new Vector3(-1,0,0) }, // Lower front
      { "RS", new Vector3(0,0,-1) }, // Raise side
      {"LS", new Vector3(0,0,1) }, // Lower side
      {"YR", new Vector3(0,-1,0) }, // Y-axis right
      { "YL", new Vector3(0,1,0) } // Y-axis left

    };

    Dictionary<Avatar_state, string[]> comp_direction = new Dictionary<Avatar_state, string[]>()
    {
      {Avatar_state.Standing, new string[]{"RF","RS"} }, // Raise front
    {Avatar_state.RAF90, new string[]{"RF", "LF", "YR" } },
    {Avatar_state.RAO90, new string[]{"LS", "YL" } }
    };


    public Quaternion frontRotate_DOT;
    bool initCheck_DOT = true;
    // check inition value
    bool seninit_pel_bl = true;
    bool seninitRot_spine_bl = true;
    bool seninitRot_RUA_bl = true;
    bool seninitRot_RLA_bl = true;
    bool seninitRot_LUA_bl = true;
    bool seninitRot_LLA_bl = true;

    // DOT sensing
    List<string> keylist;
    Dictionary<int, string> address_joint_idx = new Dictionary<int, string>()
    {
      { 0, "D4:22:CD:00:05:EE" }, //0, "D4:22:CD:00:38:F1" 
      { 1, "D4:22:CD:00:38:A9" },
      { 2, "D4:22:CD:00:38:A4" },
      { 3, "D4:22:CD:00:37:E9" },
      { 4, "D4:22:CD:00:38:00" },
      { 5, "D4:22:CD:00:42:3B" }

    };

    public bool bl_loading_data = false;
    int loaded_data_range;
    int loaded_data_frame = 0;
    int action_class = 0;
    int[] atclass_Ar = new int[6]{0,0,0,0,0,0};
    void Start()
    {
  
        XsensDotManager = GameObject.Find("XsensDotManager");
        manager=XsensDotManager.GetComponent<XsensDot>();
               
        cur_state = Avatar_state.Standing;
    }


    void Awake()
    {
        // Nick name color
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }


    void Update()
    {

        if (PV.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {



                R1 = transform.rotation;
                R2 = Spine.transform.rotation;
                //R3 = Head.transform.rotation * Quaternion.Inverse(Spine.transform.rotation);
                R4 = RightUpperArm.transform.rotation;
                R5 = RightForeArm.transform.rotation;
                //R6 = RightHand.transform.rotation * Quaternion.Inverse(RightForeArm.transform.rotation);
                R7 = LeftUpperArm.transform.rotation;
                R8 = LeftForeArm.transform.rotation;


                inint_pose_quatnew.Add(transform.rotation);
                inint_pose_quatnew.Add(Spine.transform.rotation);
                inint_pose_quatnew.Add(RightUpperArm.transform.rotation);
                inint_pose_quatnew.Add(RightForeArm.transform.rotation);
                inint_pose_quatnew.Add(LeftUpperArm.transform.rotation);
                inint_pose_quatnew.Add(LeftForeArm.transform.rotation);

                StartCoroutine(Tracking());
            }

            else if (Input.GetKeyDown(KeyCode.X))
            {
                startTracking = false;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                action_class = 0;
                bl_loading_data = true;
              
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                action_class = 1;
                bl_loading_data = true;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                action_class = 2;
                bl_loading_data = true;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                action_class = 3;
                bl_loading_data = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                //startDotdata = !startDotdata;
                keylist = new List<string>(manager.sensing_data.Keys);
            }

            if (startDotdata)
            {
                for (int i = 0; i < keylist.Count; i++)
                {
                    Debug.Log("dot address : " + keylist[i]);
                    Debug.Log("quat  data:  " + manager.sensing_data[keylist[i]]);
                }

            }

            //if (bl_loading_data)
            //{


            //    if (initCheck_DOT == true)
            //    {
            //        initDirectionCalc_DOT(action_class);
            //    }
            //    //Debug.Log("load quat before prev cnt" + load_quat.Count);
            //    MovementParsing_Dot(action_class, loaded_data_frame);
            //    //Debug.Log("load quat before prev cnt" + load_quat.Count);

            //    senRot_pel = load_quat[0];

            //    senRot_RUA = load_quat[2];
            //    senRot_RLA = load_quat[3];
            //    senRot_LUA = load_quat[4];
            //    senRot_LLA = load_quat[5];

            //    transform.rotation = senRot_pel;
            //    RightUpperArm.transform.rotation = senRot_RUA;
            //    RightForeArm.transform.rotation = senRot_RLA;
            //    LeftUpperArm.transform.rotation = senRot_LUA;
            //    LeftForeArm.transform.rotation = senRot_LLA;

            //    //load_quat
            //    //Debug.Log("load quat _data before" + load_quat[2]);

            //    int frame_length = manager.load_quat_list[action_class][0].Count;
            //    load_quat.Clear();

            //    //Debug.Log("Loading" + senRot_RUA);
            //    //Debug.Log("load quat count" + load_quat.Count);

            //    loaded_data_frame++;

            //    if (loaded_data_frame == frame_length - 1)
            //    {
            //        Debug.Log("play load done" + loaded_data_frame);
            //        //loaded_data_frame
            //        bl_loading_data = false;
            //        loaded_data_frame = 0;

            //    }
            //}
            if (bl_loading_data)
            {
                inint_pose_quatnew.Add(transform.rotation);
                inint_pose_quatnew.Add(Spine.transform.rotation);
                inint_pose_quatnew.Add(RightUpperArm.transform.rotation);
                inint_pose_quatnew.Add(RightForeArm.transform.rotation);
                inint_pose_quatnew.Add(LeftUpperArm.transform.rotation);
                inint_pose_quatnew.Add(LeftForeArm.transform.rotation);

                if (initCheck_DOT == true)
                {
                    initDirectionCalc_DOT(0);
                }
                //Debug.Log("load quat before prev cnt" + load_quat.Count);
                MovementParsing_Dot_simple(action_class, loaded_data_frame); // change simple version
                
                senRot_pel = load_quat[0];

                senRot_RUA = load_quat[2];
                senRot_RLA = load_quat[3];
                senRot_LUA = load_quat[4];
                senRot_LLA = load_quat[5];

                //transform.rotation = senRot_pel;
                RightUpperArm.transform.rotation = senRot_RUA;
                RightForeArm.transform.rotation = senRot_RLA;
                LeftUpperArm.transform.rotation = senRot_LUA;
                LeftForeArm.transform.rotation = senRot_LLA;

             
                //load_quat
             
                int frame_length = manager.load_quat_list[action_class][0].Count;
                //load_quat.Clear();

             
                loaded_data_frame++;

                if (loaded_data_frame == frame_length - 1)
                {
                    //이전 프레임 데이터 초기화
                    prev_loaded_pose.Clear();

                    Debug.Log("play load done" + loaded_data_frame);
                    //loaded_data_frame
                    bl_loading_data = false;
                    loaded_data_frame = 0;
                    first_frame = true;
                    prev_loaded_pose = load_quat.ToList();

                 

                }

                load_quat.Clear();
            }

            if (startTracking == true)
            {
                if (bl_loading_data)
                {


                    if (initCheck_DOT == true)
                    {
                        initDirectionCalc_DOT(0);
                    }
                    //Debug.Log("load quat before prev cnt" + load_quat.Count);
                    MovementParsing_Dot(atclass_Ar, loaded_data_frame);
                    //Debug.Log("load quat before prev cnt" + load_quat.Count);

                    senRot_pel = load_quat[0];

                    senRot_RUA = load_quat[2];
                    senRot_RLA = load_quat[3];
                    senRot_LUA = load_quat[4];
                    senRot_LLA = load_quat[5];

                    //transform.rotation = senRot_pel;
                    RightUpperArm.transform.rotation = senRot_RUA;
                    RightForeArm.transform.rotation = senRot_RLA;
                    LeftUpperArm.transform.rotation = senRot_LUA;
                    LeftForeArm.transform.rotation = senRot_LLA;

                    //load_quat
                    //Debug.Log("load quat _data before" + load_quat[2]);

                    int frame_length = manager.load_quat_list[action_class][0].Count;
                    //load_quat.Clear();

                    //Debug.Log("Loading" + senRot_RUA);
                    //Debug.Log("load quat count" + load_quat.Count);

                    loaded_data_frame++;

                    if (loaded_data_frame == frame_length - 1)
                    {
                        //이전 프레임 데이터 초기화
                        prev_loaded_pose.Clear();

                        Debug.Log("play load done" + loaded_data_frame);
                        //loaded_data_frame
                        bl_loading_data = false;
                        loaded_data_frame = 0;
                        first_frame = true;
                        prev_loaded_pose = load_quat.ToList();

                    }

                    load_quat.Clear();
                }
                else
                {


                    //if (initCheck == true)
                    //{
                    //    initDirectionCalc();
                    //}
                    if (initCheck == true)
                    {
                        quatW = (float)manager.sensing_data[address_joint_idx[0]].w;
                        quatX = (float)manager.sensing_data[address_joint_idx[0]].x;
                        quatY = (float)manager.sensing_data[address_joint_idx[0]].y;
                        quatZ = (float)manager.sensing_data[address_joint_idx[0]].z;


                        initDirectionCalc();
                    }

                    MovementParsing_sensing_Dot();
                    //MovementParsing();

                    senRot_pel = sensing_quat[0];

                    senRot_RUA = sensing_quat[2];
                    senRot_RLA = sensing_quat[3];
                    senRot_LUA = sensing_quat[4];
                    senRot_LLA = sensing_quat[5];



                    RightUpperArm.transform.rotation = senRot_RUA;
                    RightForeArm.transform.rotation = senRot_RLA;
                    LeftUpperArm.transform.rotation = senRot_LUA;
                    LeftForeArm.transform.rotation = senRot_LLA;

                    if (first_frame)
                    {
                        save_init_pose();
                        first_frame = false;

                    }
                    else
                    {
                        //recognition();
                    }
                    sensing_quat.Clear();

                }


                // ← → 이동


                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");

                PV.RPC("runAvatar", RpcTarget.All);

            }
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else
        {
            transform.position = curPos;
            transform.rotation = curRot;
            transform.rotation = refRot_pel;
            RightUpperArm.transform.rotation = refRot_RUA;
            RightForeArm.transform.rotation = refRot_RLA;
            LeftUpperArm.transform.rotation = refRot_LUA;
            LeftForeArm.transform.rotation = refRot_LLA;
        }

        
    }
        [PunRPC]
        void runAvatar()
        {
            movement.Set(horizontal, 0, vertical);
            movement = movement.normalized * speed * Time.deltaTime;

            RB.MovePosition(transform.position + movement);


            if (horizontal == 0 && vertical == 0) return;

            Quaternion newRotation = Quaternion.LookRotation(movement);
            RB.rotation = Quaternion.Slerp(RB.rotation, newRotation, rotatespeed * Time.deltaTime);


            transform.position = curPos;
            transform.rotation = curRot;
            //transform.rotation = senRot_pel;

        }

        void MovementParsing_Dot(int[] action_class, int frame_cnt)
        {
            //Debug.Log(address_joint_idx.Count);

            for (int joint_idx = 0; joint_idx < address_joint_idx.Count; joint_idx++)
            {
                //relativeAngle = inint_pose_quatnew[joint_idx];
                int action_cls_j = action_class[joint_idx];
                if (action_cls_j == 0)
                {
                    load_quat.Add(prev_loaded_pose[joint_idx]);
                }
                else
                {
                    action_cls_j--;

                    Quaternion Revinit_dot = Quaternion.Inverse(manager.load_quat_list[action_cls_j][joint_idx][0]);
                    Quaternion angle_dot = (manager.load_quat_list[action_cls_j][joint_idx][frame_cnt]);
                    angle_dot = angle_dot * Revinit_dot;
                    angle_dot = angle_dot * frontRotate_DOT;

                    //Debug.Log("this point" + manager.load_quat_list[action_class][joint_idx][frame_cnt]);

                    angle_dot = SensorMapping_fix(angle_dot, inint_pose_quatnew[joint_idx]);
                    // Debug.Log("this angle" + angle_dot);
                    load_quat.Add(angle_dot);
                }
            }
            //Debug.Log("load quat _data insight" + load_quat[2]);

        }
    void MovementParsing_Dot_simple(int action_class, int frame_cnt)
    {
        //Debug.Log(address_joint_idx.Count);

        for (int joint_idx = 0; joint_idx < address_joint_idx.Count; joint_idx++)
        {
            

                Quaternion Revinit_dot = Quaternion.Inverse(manager.load_quat_list[action_class][joint_idx][0]);
                Quaternion angle_dot = (manager.load_quat_list[action_class][joint_idx][frame_cnt]);
                angle_dot = angle_dot * Revinit_dot;
            //angle_dot = angle_dot * frontRotate_DOT;

            //Debug.Log("this point" + manager.load_quat_list[action_class][joint_idx][frame_cnt]);
            Quaternion frontValue = EulerToQuat(new Vector3(0, 0, -90));

            angle_dot = Quaternion.Inverse(frontValue)*angle_dot * frontValue;
            angle_dot = SensorMapping_fix(angle_dot, inint_pose_quatnew[joint_idx]);
                // Debug.Log("this angle" + angle_dot);
                load_quat.Add(angle_dot);
            
        }
        //Debug.Log("load quat _data insight" + load_quat[2]);

    }

    void MovementParsing_sensing_Dot()
    {

        for (int joint_idx = 0; joint_idx < address_joint_idx.Count; joint_idx++)
        {
            if (first_frm[joint_idx])
            {

                sensing_init_quat.Add(manager.sensing_data[address_joint_idx[joint_idx]]);

                first_frm[joint_idx] = false;

            }

            Quaternion Revinit_dot = Quaternion.Inverse(sensing_init_quat[joint_idx]);
            Quaternion angle_dot = (manager.sensing_data[address_joint_idx[joint_idx]]);            
           
            angle_dot = SensorMapping_fix(angle_dot * Revinit_dot, inint_pose_quatnew[joint_idx]);  
            sensing_quat.Add(angle_dot);


            //angle_dot = angle_dot * Revinit_dot;

            //angle_dot = angle_dot * frontRotate; * frontRotate

            //Debug.Log("this test angle" + angle_dot+"///"+ inint_pose_quatnew[joint_idx]);

            //Debug.Log("this angle" + angle_dot);
        }


    }




    void recognition()
        {
            float[] Arm_angle;
            Vector3[] Arm_rot_axis;
            Arm_angle = cal_parameter_angle();
            switch (cur_state)
            {
                case Avatar_state.Standing:

                    if (Arm_angle[0] > 20)
                    {
                        Arm_rot_axis = cal_parameter_axis();

                        var rlt = comp_rot_direction(comp_direction[cur_state], Arm_rot_axis[0]);
                        RLTText.text = "state : standing + \n" + rlt;

                        // { Avatar_state.Standing, new string[] { "RF", "RS" } }, // Raise front
                        if (rlt == "RF")
                        {
                            bl_loading_data = true;
                            action_class = 1;
                            cur_state = Avatar_state.RAF90;
                        }
                        else if (rlt == "RS")
                        {
                            bl_loading_data = true;
                            action_class = 2;// no data
                            cur_state = Avatar_state.Standing;
                        }
                    }
                    else if (Arm_angle[2] > 20 && Arm_angle[1] > 20)
                    {
                        RLTText.text = "state : standing + \n" + "shake hand";
                        Arm_rot_axis = cal_parameter_axis();
                        var rlt = comp_rot_direction(comp_direction[cur_state], Arm_rot_axis[1]);
                        if (rlt == "RF")
                        {
                            bl_loading_data = true;
                            action_class = 0;
                        }
                    }
                    break;


                case Avatar_state.RAF90:

                    if (Arm_angle[0] > 20)
                    {
                        Arm_rot_axis = cal_parameter_axis();

                        var rlt = comp_rot_direction(comp_direction[cur_state], Arm_rot_axis[0]);

                        RLTText.text = "state : RAF90 + \n" + rlt;
                        //{Avatar_state.RAF90, new string[]{"RF", "LF", "YR" } },          

                        if (rlt == "RF")
                        {
                            bl_loading_data = true;
                            action_class = 2; // no data
                            cur_state = Avatar_state.Standing;
                        }
                        else if (rlt == "LF")
                        {
                            bl_loading_data = true;
                            action_class = 2;
                            cur_state = Avatar_state.Standing;
                        }
                        else if (rlt == "YR")
                        {
                            bl_loading_data = true;
                            action_class = 3;
                            cur_state = Avatar_state.RAO90;
                        }

                    }


                    break;



                case Avatar_state.RAO90:

                    //{Avatar_state.RAO90, new string[]{"LS", "YL" } }
                    if (Arm_angle[0] > 20)
                    {
                        Arm_rot_axis = cal_parameter_axis();

                        var rlt = comp_rot_direction(comp_direction[cur_state], Arm_rot_axis[0]);

                        RLTText.text = "state : RAO90 + \n" + rlt;
                        //{Avatar_state.RAF90, new string[]{"RF", "LF", "YR" } },          

                        if (rlt == "LS")
                        {
                            bl_loading_data = true;
                            action_class = 2; // no data
                            cur_state = Avatar_state.Standing;
                        }
                        else if (rlt == "YL")
                        {
                            bl_loading_data = true;
                            action_class = 4;
                            cur_state = Avatar_state.RAF90;
                        }
                    }
                    break;
                default: // do nothing;
                    break;
            }

        }

        string comp_rot_direction(string[] comp_dir, Vector3 rot_axis)
        {
            float angle_buf;
            Dictionary<string, float> com_rlt = new Dictionary<string, float>();


            foreach (string str_dir in comp_dir)
            {
                angle_buf = cal_bet_angle(rot_direction[str_dir], rot_axis);

                com_rlt.Add(str_dir, angle_buf);

            }


            var rlt = com_rlt.OrderBy(x => x.Value).First();

            if (rlt.Value > 20)
            {
                return "none";
            }
            else
                return rlt.Key;


        }




        void save_init_pose()
        {
            Vector3 UpperArm;
            Vector3 LowerArm;
            init_pose_vector.Clear();
            UpperArm = (RightForeArm.transform.position - RightUpperArm.transform.position).normalized;
            LowerArm = (RightHand.transform.position - RightForeArm.transform.position).normalized;

            init_pose_vector.Add(UpperArm);
            init_pose_vector.Add(LowerArm);




        }


        float[] cal_parameter_angle()
        {
            Vector3 cur_UpperArm;
            Vector3 cur_LowerArm;
            //init_pose_vector.Clear();

            float[] angle = new float[3];

            //float rotate_degree_u;
            //float rotate_degree_l ;

            cur_UpperArm = (RightForeArm.transform.position - RightUpperArm.transform.position).normalized;
            cur_LowerArm = (RightHand.transform.position - RightForeArm.transform.position).normalized;

            angle[0] = cal_bet_angle(cur_UpperArm, init_pose_vector[0]);
            angle[1] = cal_bet_angle(cur_LowerArm, init_pose_vector[1]);
            angle[2] = cal_bet_angle(cur_UpperArm, cur_LowerArm);
            return angle;
        }

        Vector3[] cal_parameter_axis()
        {
            Vector3 cur_UpperArm;
            Vector3 cur_LowerArm;
            Vector3[] axis = new Vector3[2];


            cur_UpperArm = (RightForeArm.transform.position - RightUpperArm.transform.position).normalized;
            cur_LowerArm = (RightHand.transform.position - RightForeArm.transform.position).normalized;

            axis[0] = Vector3.Cross(cur_UpperArm, init_pose_vector[0]);
            axis[1] = Vector3.Cross(cur_LowerArm, init_pose_vector[1]);
            return axis;
        }


        float cal_bet_angle(Vector3 cur, Vector3 init)
        {
            return Mathf.Acos(Vector3.Dot(cur, init)) * Mathf.Rad2Deg;
        }



        Quaternion MovementParsing_fix(int joint_idx, Quaternion in_data)
        {
            Quaternion angle;
            Quaternion Revinit = new Quaternion(0, 0, 0, 1);
            if (joint_idx == 2)
            {
                if (seninitRot_RUA_bl)
                {
                    seninitRot_RUA = new Quaternion(in_data.x, in_data.y, in_data.z, in_data.w);
                    seninitRot_RUA_bl = false;
                }
                Revinit = Quaternion.Inverse(seninitRot_RUA); // 센서의 첫데이터 센서 초기화
                relativeAngle = R4; //오른팔    // 오브젝트의 현재 자세                     
            }

            else if (joint_idx == 3) //오른팔 관절
            {
                if (seninitRot_RLA_bl)
                {
                    seninitRot_RLA = new Quaternion(in_data.x, in_data.y, in_data.z, in_data.w);
                    seninitRot_RLA_bl = false;
                }
                Revinit = Quaternion.Inverse(seninitRot_RLA);
                relativeAngle = R5;
            }

            else if (joint_idx == 4) //왼팔
            {

                if (seninitRot_LUA_bl)
                {
                    seninitRot_LUA = new Quaternion(in_data.x, in_data.y, in_data.z, in_data.w);
                    seninitRot_LUA_bl = false;
                }
                Revinit = Quaternion.Inverse(seninitRot_LUA);
                relativeAngle = R7;
            }

            else if (joint_idx == 5) //왼팔 관절
            {

                if (seninitRot_LLA_bl)
                {
                    seninitRot_LLA = new Quaternion(in_data.x, in_data.y, in_data.z, in_data.w);
                    seninitRot_LLA_bl = false;
                }
                Revinit = Quaternion.Inverse(seninitRot_LLA);
                relativeAngle = R8;
            }

            //initQT = new Quaternion((float)in_data.x, in_data.y, in_data.z, in_data.w);


            target = new Quaternion((float)in_data.x, in_data.y, in_data.z, in_data.w);
            target = target * Revinit;
            target = target * frontRotate;

            angle = SensorMapping_fix(target, relativeAngle);

            return angle;

            //transform.localRotation = initRotation;
            //transform.rotation = angle;
            //senRot_pel = angle;
        }

        void MovementParsing_pel(Quaternion in_pelquat)
        {
            relativeAngle = R1;

            Revinit = Quaternion.Inverse(initQT);
            target = new Quaternion((float)in_pelquat.x, in_pelquat.y, in_pelquat.z, in_pelquat.w);
            target = target * Revinit;
            target = target * frontRotate;
            angle = SensorMapping(target);

            if (startData_recording == true)
            {

            }
            senRot_pel = angle;
            transform.rotation = angle;

            //RUpper_Arm.transform.rotation;
        }





        void MovementParsing()
        {
            relativeAngle = R1;

            Revinit = Quaternion.Inverse(initQT);
            target = new Quaternion((float)quatX, (float)quatY, (float)quatZ, (float)quatW);
            target = target * Revinit;
            target = target * frontRotate;
            angle = SensorMapping(target);

            if (startData_recording == true)
            {

            }
            senRot_pel = angle;
            transform.rotation = angle;

            //RUpper_Arm.transform.rotation;
        }

        Quaternion EulerToQuat(Vector3 Angle)
        {
            float angleX, angleY, angleZ;
            Quaternion q;
            float q1, q2, q3, q4, scale;

            angleX = Angle.x * Degree2Radian;
            angleY = Angle.y * Degree2Radian;
            angleZ = Angle.z * Degree2Radian;

            //둘다 ZXY순서
            /* q1 = Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) + Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
             q2 = Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) + Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
             q3 = Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f) - Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f);
             q4 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f);*/

            /*q1 = Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) + Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
            q2 = Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f);
            q3 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f) + Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f);
            q4 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f);*/

            //XYZ순서
            q1 = Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) + Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
            q2 = -Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) + Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f);
            q3 = Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f);
            q4 = -Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f);


            scale = Mathf.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);

            q.w = q1 / scale;
            q.x = q2 / scale;
            q.y = q3 / scale;
            q.z = q4 / scale;

            return q;
        }

        Quaternion SensorMapping(Quaternion target)
        {
            Quaternion GlobalToUnity, UnityToPart;

            GlobalToUnity.w = -target.w;
            GlobalToUnity.x = -target.y;
            GlobalToUnity.y = target.z;
            GlobalToUnity.z = target.x;

            UnityToPart = GlobalToUnity * relativeAngle;

            return UnityToPart;
        }

        Quaternion SensorMapping_fix(Quaternion target, Quaternion rel_quat)
        {
            Quaternion GlobalToUnity, UnityToPart;

            GlobalToUnity.w = -target.w;
            GlobalToUnity.x = -target.y;
            GlobalToUnity.y = target.z;
            GlobalToUnity.z = target.x;

            UnityToPart = GlobalToUnity * rel_quat;

            return UnityToPart;
        }

        void initDirectionCalc()
        {
            initQT = new Quaternion((float)quatX, (float)quatY, (float)quatZ, (float)quatW);

            northValue = EulerToQuat(new Vector3(0, -90, 0));  //뒤집어지면 zx 부호를 바꾸자

            frontRotate = initQT * Quaternion.Inverse(northValue);

            initCheck = false;
        }

        void initDirectionCalc_DOT(int action_class)
        {
            Quaternion initQT = manager.load_quat_list[action_class][0][0];

            Quaternion northValue = EulerToQuat(new Vector3(0, -90, 0));  //뒤집어지면 zx 부호를 바꾸자 0 -90 0
        Quaternion frontValue = EulerToQuat(new Vector3(-90,0 ,90));
        frontRotate_DOT = initQT * Quaternion.Inverse(frontValue) * Quaternion.Inverse(northValue);

        initCheck_DOT = false;
        }




        IEnumerator Tracking()
        {
            yield return new WaitForSeconds(0.2f);
            Debug.Log("정면");
            startTracking = true;
        }

        [PunRPC]
        void JumpRPC()
        {
            RB.velocity = Vector3.zero;
            RB.AddForce(Vector3.up * 700);
        }



        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(senRot_pel);
                stream.SendNext(senRot_RUA);
                stream.SendNext(senRot_RLA);
                stream.SendNext(senRot_LUA);
                stream.SendNext(senRot_LLA);
            }
            else
            {
                curPos = (Vector3)stream.ReceiveNext();
                curRot = (Quaternion)stream.ReceiveNext();
                refRot_pel = (Quaternion)stream.ReceiveNext();
                refRot_RUA = (Quaternion)stream.ReceiveNext();
                refRot_RLA = (Quaternion)stream.ReceiveNext();
                refRot_LUA = (Quaternion)stream.ReceiveNext();
                refRot_LLA = (Quaternion)stream.ReceiveNext();



            }
        }
    }

// backup sensing part
//else
//{
//    // pelvis rotation 
//    quatW = (float)manager.sensing_data[address_joint_idx[0]].w;
//    quatX = (float)manager.sensing_data[address_joint_idx[0]].x;
//    quatY = (float)manager.sensing_data[address_joint_idx[0]].y;
//    quatZ = (float)manager.sensing_data[address_joint_idx[0]].z;

    //    if (initCheck == true)
    //    {
    //        initDirectionCalc();
    //    }

    //    //MovementParsing();


    //    // Right Upper arm rotation
    //    int joint_idx = 2;
    //    Quaternion quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

    //    senRot_RUA = MovementParsing_fix(joint_idx, quat_buf);
    //    //RightUpperArm.transform.rotation = senRot_RUA;


    //    // Right Lower arm rotation
    //    joint_idx = 3;
    //    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

    //    senRot_RLA = MovementParsing_fix(joint_idx, quat_buf);
    //    //RightForeArm.transform.rotation = senRot_RLA;

    //    // Left Upper arm rotation
    //    joint_idx = 4;
    //    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

    //    senRot_LUA = MovementParsing_fix(joint_idx, quat_buf);
    //    //LeftUpperArm.transform.rotation = senRot_LUA;

    //    // Left Lower arm rotation
    //    joint_idx = 5;
    //    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
    //                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

    //    senRot_LLA = MovementParsing_fix(joint_idx, quat_buf);




    //    RightUpperArm.transform.rotation = senRot_RUA;
    //    RightForeArm.transform.rotation = senRot_RLA;
    //    LeftUpperArm.transform.rotation = senRot_LUA;
    //    LeftForeArm.transform.rotation = senRot_LLA;


    // else
    //                {


    //                    if (initCheck == true)
    //                    {
    //                        initDirectionCalc();
    //}


    //MovementParsing_sensing_Dot();
    ////MovementParsing();

    //senRot_pel = sensing_quat[0];

    //senRot_RUA = sensing_quat[2];
    //senRot_RLA = sensing_quat[3];
    //senRot_LUA = sensing_quat[4];
    //senRot_LLA = sensing_quat[5];



    //RightUpperArm.transform.rotation = senRot_RUA;
    //RightForeArm.transform.rotation = senRot_RLA;
    //LeftUpperArm.transform.rotation = senRot_LUA;
    //LeftForeArm.transform.rotation = senRot_LLA;

    //if (first_frame)
    //{
    //    save_init_pose();
    //    first_frame = false;

    //}
    //else
    //{
    //    //recognition();
    //}
    //sensing_quat.Clear();

    //                }



//else
//{
//    // pelvis rotation 
//    quatW = (float)manager.sensing_data[address_joint_idx[0]].w;
//    quatX = (float)manager.sensing_data[address_joint_idx[0]].x;
//    quatY = (float)manager.sensing_data[address_joint_idx[0]].y;
//    quatZ = (float)manager.sensing_data[address_joint_idx[0]].z;

//    if (initCheck == true)
//    {
//        initDirectionCalc();
//    }

//    //MovementParsing();


//    // Right Upper arm rotation
//    int joint_idx = 2;
//    Quaternion quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

//    senRot_RUA = MovementParsing_fix(joint_idx, quat_buf);
//    //RightUpperArm.transform.rotation = senRot_RUA;


//    // Right Lower arm rotation
//    joint_idx = 3;
//    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

//    senRot_RLA = MovementParsing_fix(joint_idx, quat_buf);
//    //RightForeArm.transform.rotation = senRot_RLA;

//    // Left Upper arm rotation
//    joint_idx = 4;
//    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

//    senRot_LUA = MovementParsing_fix(joint_idx, quat_buf);
//    //LeftUpperArm.transform.rotation = senRot_LUA;

//    // Left Lower arm rotation
//    joint_idx = 5;
//    quat_buf = new Quaternion((float)manager.sensing_data[address_joint_idx[joint_idx]].x,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].y,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].z,
//                                         (float)manager.sensing_data[address_joint_idx[joint_idx]].w);

//    senRot_LLA = MovementParsing_fix(joint_idx, quat_buf);




//    RightUpperArm.transform.rotation = senRot_RUA;
//    RightForeArm.transform.rotation = senRot_RLA;
//    LeftUpperArm.transform.rotation = senRot_LUA;
//    LeftForeArm.transform.rotation = senRot_LLA;


//}