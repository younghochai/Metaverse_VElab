using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ConductingHand : MonoBehaviour
{
    public GameObject handModel;
    public GameObject line;
    LineRenderer lineRenderer;

    int[] HandJointIdx = { 4, 6, 7, 8 , 11, 12, 13, 16, 17, 18, 21, 22, 23, 25, 26, 27 };

    string[] RightHandJointNames = new string[] { 
        "right_wrist",                                      // [0],
        "right_index1",  "right_index2",  "right_index3",   // [1],  [2],  [3]
        "right_middle1", "right_middle2", "right_middle3",  // [4],  [5],  [6]
        "right_pinky1",  "right_pinky2",  "right_pinky3",   // [7],  [8],  [9]
        "right_ring1",   "right_ring2",   "right_ring3",    // [10], [11], [12]
        "right_thumb1",  "right_thumb2",  "right_thumb3"    // [13], [14], [15]
    };

    string[] LeftHandJointNames = new string[] {
        "left_wrist",                                       // [0],
        "left_index1",   "left_index2",   "left_index3",    // [1],  [2],  [3]
        "left_middle1",  "left_middle2",  "left_middle3",   // [4],  [5],  [6]
        "left_pinky1",   "left_pinky2",   "left_pinky3",    // [7],  [8],  [9]
        "left_ring1",    "left_ring2",    "left_ring3",     // [10], [11], [12]
        "left_thumb1",   "left_thumb2",   "left_thumb3"     // [13], [14], [15]
    };

    public Dictionary<string, Transform> _transformFromName;


    void Start()
    {
        if (_transformFromName == null)
        {
            _transformFromName= new Dictionary<string, Transform>();
            Transform[] transforms = handModel.transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < HandJointIdx.Length; i++)
            {
                _transformFromName.Add(RightHandJointNames[i], transforms[HandJointIdx[i]]);

                Debug.Log("_transformFromName = (" + _transformFromName[RightHandJointNames[i]].name + ")");
            }
        }

        lineRenderer = line.GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Press Tap button");

            PlayPath();
        }
    }


    IEnumerator PlayPath()
    {
        int frames = lineRenderer.positionCount;
        Vector3[] path = new Vector3[frames];

        for (int i = 0; i < frames; i++)
        {
            path[i] = lineRenderer.GetPosition(i);
            _transformFromName["right_wrist"].transform.position = path[i];
        
            yield return new WaitForSeconds(0.025f);
        }

        yield break;
    }
}
