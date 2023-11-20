using LightweightMatrixCSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.Video;

public class CSVPlayer : MonoBehaviour
{
    bool zDown;
    bool bDown;


    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();


    string[] _bodyJointNames = new string[] {"pelvis", "left_hip", "right_hip", "spine1", "left_knee",
                                             "right_knee", "spine2", "left_ankle", "right_ankle", "spine3",
                                             "left_foot", "right_foot", "neck", "left_collar", "right_collar",
                                             "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                                             "left_wrist", "right_wrist" };  //AMASS 22 Joint

    //string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
    //                                                "left_elbow","right_hip","right_knee","left_hip","left_knee"}; //awinda 10 Joint

    string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
                                                    "left_elbow"};

    //-----------------------Added by JS-----------------------------
    QuatForSMPLX quat4smplX;

    Vector3 currentPos, pastPos, oldPos;
    [HideInInspector]
    public float angle = 0.0f;
    List<List<float>> LPosList = new List<List<float>>();
    List<List<float>> RPosList = new List<List<float>>();
    public Transform wristL, wristR;
    
    public List<SMPLX> smplX = new List<SMPLX>();
    public List<TrailRenderer> trailRenderer = new List<TrailRenderer>();
    VideoPlayer videoPlayer;
    public Slider slider;

    float dataValue;
    float motionLength, musicLength;
    int currentFrame;

    bool isPlaying;
    //---------------------------------------------------------------


    void Start()
    {

        quat4smplX = smplX[1].gameObject.GetComponent<QuatForSMPLX>();
        videoPlayer = GameObject.Find("Video").GetComponent<VideoPlayer>();
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
            string prefix = "C:/Users/pssil/OneDrive/¹ÙÅÁ È­¸é/velab/2023.07-10/SMPLX-Unity/Assets/Data/";   //root file path

            TXTReader(prefix + "test5.csv");
            //TXTReader(prefix + "1¹ø°î 2È¸Â÷.csv");

            //Debug.Log("TXT file load done");
            Debug.Log("CSV file load done");
        
            
            motionLength = load_quat_list[0][0].Count;
            musicLength = videoPlayer.frameCount;
        }
    }

    public void _Animation()
    {
        
        if (bDown || isPlaying)
        {
            float value;

            if (bDown)
                value = 0.0f;

            else
                value = slider.value;

            ///float value = slider.value;

            StartCoroutine(avatar_play_custom(value));
            isPlaying = false;
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Debug.Log("Current Motion Frame: " + currentFrame);
        }

        currentPos = wristL.position;
        
    }  


    public IEnumerator avatar_play_custom(float value) //awinda Xsens data player
    {

        int motion_startFrame = (int)(value * motionLength / 100);
        int music_startFrame = (int)(value * musicLength / 100);

        Debug.Log("Start Motion Frame: " + motion_startFrame + ", Start Music Frame: " + music_startFrame);


        videoPlayer.frame = music_startFrame;
        videoPlayer.Play();


        for (int f = motion_startFrame; f < load_quat_list[0][0].Count; f++)
        {

            dataValue = (f / motionLength) * 100;
            slider.value = dataValue;

            currentFrame = f;


            for (int i = 0; i < _bodyCustomJointNames.Length; i++)
            {
                smplX[0].SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][f]);
                smplX[1].SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][f]);
            }


            LPosList.Add(new List<float> { wristL.position.x, wristL.position.y, wristL.position.z });
            //RPosList.Add(new List<float> { wristR.position.x, wristR.position.y, wristR.position.z });
            

            smplX[0].UpdateJointPositions(false);
            smplX[1].UpdateJointPositions(false);
            yield return new WaitForSeconds(.01645f);

            //Debug.Log("Slider Value = " + dataValue);
            //Debug.Log("Video Time = " + musicLength);


            int n = LPosList.Count;
            pastPos = wristL.position;

            if (n >= 5)
            {
                oldPos = new Vector3(LPosList[n - 4][0], LPosList[n - 4][1], LPosList[n - 4][2]);

            //    Debug.Log("pastPos: (" + wristL.position.x + ",  " + wristL.position.y + ",  " + wristL.position.z +
            //    ")\noldPos[" + (n - 4) + "]: (" + LPosList[n - 4][0] + ",  " + LPosList[n - 4][1] + ",  " + LPosList[n - 4][2] + ")");
            }


            if (dataValue != slider.value)
            {
                //videoPlayer.Pause();
                trailRenderer[0].Clear();
                trailRenderer[1].Clear();
                trailRenderer[2].Clear();
                trailRenderer[3].Clear();

                isPlaying = true;
                yield break;
            }
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
