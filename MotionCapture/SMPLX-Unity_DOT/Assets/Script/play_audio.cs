using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class play_audio : MonoBehaviour
{

    AudioSource audioSource;
    AudioSource test_audio;
    AudioClip test_clip;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        test_clip = Resources.Load<AudioClip>("SE/SE2");
        test_clip = Resources.Load<AudioClip>("SE/settings");

        audioSource.clip = test_clip;
        audioSource.mute = false;
        audioSource.loop = false;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //audioSource.Play();

    }
}
