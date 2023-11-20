using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ConductingHand : MonoBehaviour
{
    [SerializeField]
    GameObject smplxModel, handModel, wrist, middle_dist;


    public GameObject[] line;
    public Vector3[] path;
    LineRenderer lineRenderer;
    QuatForSMPLX quat4smplX;
    public Transform center;

    int[] HandJointIdx = { 6, 8, 9, 10, 18, 19, 20, 13, 14, 15, 23, 24, 25, 27, 28, 29 };

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
    public List<List<List<Quaternion>>> L_pose_data = new List<List<List<Quaternion>>>();
    public List<List<List<Quaternion>>> R_pose_data = new List<List<List<Quaternion>>>();


    void Start()
    {
        if (_transformFromName == null)
        {
            _transformFromName = new Dictionary<string, Transform>();
            Transform[] transforms = handModel.transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < HandJointIdx.Length; i++)
            {
                _transformFromName.Add(RightHandJointNames[i], transforms[HandJointIdx[i]]);

                Debug.Log("_transformFromName = (" + RightHandJointNames[i] + ", " + _transformFromName[RightHandJointNames[i]] + ")");
            }
        }


        quat4smplX = smplxModel.GetComponent<QuatForSMPLX>();
        L_pose_data = quat4smplX.L_pose_data;
        R_pose_data = quat4smplX.R_pose_data;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Press Tap button");

            GetPathValue(0);
            StartCoroutine(PlayPath(path));
            StartCoroutine(PlayMotion(1));
        }

    }


    // 복제된 라인 오브젝트로 포지션 값 가져오기
    void GetPathValue(int n)
    {
        line = GameObject.FindGameObjectsWithTag("Line");
        lineRenderer = line[n].GetComponent<LineRenderer>();

        int frames = lineRenderer.positionCount;
        path = new Vector3[frames];

        for (int i = 0; i < frames; i++)
        {
            path[i] = lineRenderer.GetPosition(i);
        }

        Vector3 vec;
        int index = 0;

        for (int i = 0; i < path.Length; i++)
        {
            if ((path[i].x != path[index].x) || (path[i].y != path[index].y) || (path[i].z != path[index].z))
            {
                vec = path[i] - path[index];
                index = i;

                Debug.Log("vec = " + vec + ", index: " + index);
            }

        }
    }

    // 궤적 따라서 움직이기
    IEnumerator PlayPath(Vector3[] path)
    {
        for (int i = 0; i < path.Length; i++)
        {    
            center.transform.position = path[i];
            
            yield return new WaitForSeconds(0.0025f);
        }

        yield break;
    }

    public IEnumerator PlayMotion(int p)
    {
        Debug.Log("Motion Start");

        float slerp_time = 60.0f;

        for (float t = 0; t <= slerp_time; t++)
        {

            for (int j = 1; j < HandJointIdx.Length; j++)
            {
                // Left
                Transform L_joints = _transformFromName[LeftHandJointNames[j]];
                Quaternion L_old_rot = L_joints.localRotation;

                L_joints.localRotation = Quaternion.Slerp(L_old_rot, L_pose_data[p][j][1] * Quaternion.Euler(0.0f, 0.0f, 90.0f), 1.0f);


                // Right
                Transform R_joints = _transformFromName[RightHandJointNames[j]];
                Quaternion R_old_rot = R_joints.localRotation;
                
                R_joints.localRotation = Quaternion.Slerp(R_old_rot, L_pose_data[p][j][1] * Quaternion.Euler(0.0f, 0.0f, 90.0f), 1.0f);
                
            }

            yield return new WaitForSeconds(0.0075f);
        }
    }

}
