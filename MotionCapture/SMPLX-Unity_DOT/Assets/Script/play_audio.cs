using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class play_audio : MonoBehaviour
{

    AudioSource audioSource; // 오디오 플레이어

    AudioClip test_clip; // 플레이할 파일
    AudioClip READY_SE;

    string Input_direction;
    bool is_play_once;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        test_clip = Resources.Load<AudioClip>("SE/SE2");
        test_clip = Resources.Load<AudioClip>("SE/settings");
        READY_SE = Resources.Load<AudioClip>("SE/ReadySE");

        //audioSource.clip = READY_SE;
        //audioSource.mute = false;
        //audioSource.loop = false;
        //audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Input_direction = GameObject.Find("Xsens").GetComponent<motion_gesture>().direction;

        //audioSource.Play();
        if (!is_play_once) 
        {
            if (Input_direction == "Ready") 
            {
                audioSource.clip = READY_SE;
                audioSource.mute = false;
                audioSource.loop = false;
                audioSource.Play();
                is_play_once = true;

            }
        }
        if (is_play_once) 
        {
        
        }
    }
}
