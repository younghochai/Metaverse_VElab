using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.UI.Image;

public class ConductingAvatar : MonoBehaviour
{
    CSVPlayer readCSVscript;
    QuatForSMPLX quat4smplx;

    TrailRenderer[] trailRenderer = new TrailRenderer[2];

    List<Vector3> originDir = new List<Vector3>();
    List<float> originAngle = new List<float>();

    List<Dictionary<int, float>> record1 = new List<Dictionary<int, float>>();
    List<Dictionary<int, float>> record2 = new List<Dictionary<int, float>>();

    public Transform[] pivot;
    public Transform[] shldr;
    public Transform[] wrist = new Transform[2];
    float angle, beforeAngle, finalAngle;
    //Vector3 dir = new Vector3(0.0f, 90.0f, 0.0f);
    Vector3 dir = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 finalDir;
    
    public float scalingValue = 50.0f;
    public int angleFilterSize = 3;
    public int dirFilterSize = 3;
    float angleX = 0.0f;
    float angleY = 0.0f;

    Vector3 rot = new Vector3(0.0f, 0.0f, 0.0f);

    void Start()
    {
        GameObject playController = GameObject.Find("Play Controller");

        quat4smplx = GetComponent<QuatForSMPLX>();
        readCSVscript = playController.GetComponent<CSVPlayer>();
        trailRenderer[0] = readCSVscript.trailRenderer[0];
        trailRenderer[1] = readCSVscript.trailRenderer[1];
        
        wrist[0] = readCSVscript.wristL;
        wrist[1] = readCSVscript.wristR;
    }


    void Update()
    {
        int count_L = trailRenderer[0].positionCount;
        int count_R = trailRenderer[1].positionCount;

        if (count_L > 0)
        {
            //Debug.Log("L wrist pos:" + trailRenderer[0].GetPosition(count_L - 1)
            //+ "\n\t    R wrist pos:" + trailRenderer[1].GetPosition(count_R - 1));
        }

        if (count_R > 2)
        {
            CalcAngularVelocity(count_R - 1, 1);

            //Debug.Log("wristR rot: " + wrist[1].localEulerAngles);
            //wrist[1].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, Vector3.forward);
            //wrist[1].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, dir);


            Vector3[] pos = new Vector3[2];

            pos[0] = trailRenderer[0].GetPosition(count_L - 2);
            pos[1] = trailRenderer[0].GetPosition(count_L - 1);

            //dir += (pos[1] - pos[0]);
            //Quaternion rot = dir * Quaternion.Euler(0.0f, 90.0f, 0.0f)

            float gapX = Mathf.Abs(pos[1].x) - Mathf.Abs(pos[0].x);
            float gapY = Mathf.Abs(pos[1].y) - Mathf.Abs(pos[0].y);
            //Debug.Log("X gap: "+ gapX + ",  Y gap: " + gapY);

            angleX += gapX;
            angleY += gapY;

            //wrist[1].localRotation = rot;
            //Debug.Log("wrist[1].localEulerAngles = " + wrist[1].localEulerAngles);


            StartCoroutine(quat4smplx.RotationDelay2(3, finalAngle * scalingValue, 1));
            //quat4smplx.PlayHandMotion(2, finalAngle * scalingValue, 1);

            //Debug.Log("Angle: " + finalAngle * scalingValue);
    }


        if (count_L > 2)
        {
            CalcAngularVelocity(count_L - 1, 0);

            //quat4smplx.PlayHandMotion(2, finalAngle * scalingValue, 0);

            //if (wrist[0].position.y > shldr[0].position.y)
            //{
            //    StartCoroutine(quat4smplx.RotationDelay2(1, finalAngle * scalingValue, 0));
            //}
            //else
            //{
                StartCoroutine(quat4smplx.RotationDelay2(3, finalAngle * scalingValue, 0));
            //}
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            //*** When recording... ***
            quat4smplx.SaveDic2Csv("C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.07-10/SMPLX-Unity/Assets/records/angle(1)_4.csv", record1);
            quat4smplx.SaveDic2Csv("C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.07-10/SMPLX-Unity/Assets/records/avg_angle(50,1)_4.csv", record2);
            //**************************
        }
    }


    void CalcAngularVelocity(int n, int hand)
    {
        beforeAngle = finalAngle;

        Vector3 currentPos = trailRenderer[hand].GetPosition(n);
        Vector3 beforePos = trailRenderer[hand].GetPosition(n - 1);

        dir = (currentPos - beforePos).normalized;

        originDir.Add(dir);

        finalDir = NormalizeVector(originDir, dirFilterSize);
        //Debug.Log("dir = (" + dir.x +", " + dir.y + ", " + dir.z + ")\n\tfinalDir = (" + finalDir.x + ", " + finalDir.y + ", " + finalDir.z +")");
        
        //finalDir.x = MovingAverageFilter(originDirX, dirFilterSize);
        //finalDir.y = MovingAverageFilter(originDirY, dirFilterSize);
        //finalDir.z = MovingAverageFilter(originDirZ, dirFilterSize);

        Vector3 v1 = beforePos - pivot[hand].position;
        Vector3 v2 = currentPos - pivot[hand].position;

        angle = Mathf.Acos(Vector3.Dot(v1, v2) / (Vector3.Magnitude(v1) * Vector3.Magnitude(v2))) * (float)(180.0 / Mathf.PI);


        //*** When recording... ***
        Dictionary<int, float> dic = new Dictionary<int, float>();
        dic.Add(n, angle * scalingValue);
        record1.Add(dic);
        //**************************

        if (float.IsNaN(angle))
        {
            if (hand == 0)      angle = 0.0f;
            if (hand == 1)      angle = beforeAngle;
        }

        if (angle < -100.0f || angle > 100.0f)
        {
            angle = 100.0f;
        }

        //Debug.Log("Frame no." + n + ",      angle = " + angle * scalingValue);

        //*** When recording... ***
        Dictionary<int, float> dic1 = new Dictionary<int, float>();
        dic1.Add(n, angle);
        record1.Add(dic1);
        //**************************

        //finalAngle = angle;
        originAngle.Add(angle);
        finalAngle = MovingAverageFilter(originAngle, angleFilterSize);
        Debug.Log("origin: " + angle*scalingValue + ",   final: " + finalAngle*scalingValue);

        Quaternion rot = Quaternion.AngleAxis(finalAngle * scalingValue, wrist[hand].position) * Quaternion.Euler(finalDir);
        wrist[hand].localRotation = rot;
        //wrist[hand].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, finalDir);


        //*** When recording... ***
        Dictionary<int, float> dic2 = new Dictionary<int, float>();
        dic2.Add(n, finalAngle * scalingValue);
        record2.Add(dic2);
        //**************************
    }


    float MovingAverageFilter(List<float> origin, int filterSize)
    {
        int size;
        float finalValue;

        if (origin.Count < filterSize)      size = origin.Count;
        else                                size = filterSize;

        int index = origin.Count - 1;        
        float sum = 0;

        for (int i = 0; i < size; i++)
        {
            sum += origin[index - i];
        }

        float avg = sum / size;
        finalValue = avg;

        //Debug.Log("angle count: " + origin.Count + ",   final angle: " + finalAngle);

        return finalValue;
    }

    Vector3 NormalizeVector(List<Vector3> origin, int filterSize)
    {
        int size;
        Vector3 finalVec;

        if (origin.Count < filterSize) size = origin.Count;
        else size = filterSize;

        int index = origin.Count - 1;
        Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);

        for (int i = 0; i < size; i++)
        {
            sum += origin[index - i];
        }

        Vector3 avg = sum.normalized;
        finalVec = avg;

        //Debug.Log("finalVec(" + finalVec.x + ",  " + finalVec.y + ", " + finalVec.z+")");
        //Debug.Log("angle count: " + origin.Count + ",   final angle: " + finalAngle);

        return finalVec;
    }

}



/* BackUp

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.UI.Image;

public class ConductingAvatar : MonoBehaviour
{
    CSVPlayer readCSVscript;
    QuatForSMPLX quat4smplx;

    TrailRenderer[] trailRenderer = new TrailRenderer[2];
    List<Vector3> originDir = new List<Vector3>();
    List<float> originAngle = new List<float>();
    List<float> originDirX = new List<float>();
    List<float> originDirY = new List<float>();
    List<float> originDirZ = new List<float>();
    List<Dictionary<int, float>> record1 = new List<Dictionary<int, float>>();
    List<Dictionary<int, float>> record2 = new List<Dictionary<int, float>>();

    public Transform[] pivot;
    public Transform[] shldr;
    public Transform[] wrist = new Transform[2];
    float angle, beforeAngle, finalAngle;
    //Vector3 dir = new Vector3(0.0f, 90.0f, 0.0f);
    Vector3 dir = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 finalDir;

    public float scalingValue = 50.0f;
    public int angleFilterSize = 3;
    public int dirFilterSize = 3;
    float angleX = 0.0f;
    float angleY = 0.0f;
    Vector3 initPos;
    Quaternion initRot;
    Vector3 rot = new Vector3(0.0f, 0.0f, 0.0f);

    void Start()
    {
        GameObject playController = GameObject.Find("Play Controller");

        quat4smplx = GetComponent<QuatForSMPLX>();
        readCSVscript = playController.GetComponent<CSVPlayer>();
        trailRenderer[0] = readCSVscript.trailRenderer[0];
        trailRenderer[1] = readCSVscript.trailRenderer[1];

        wrist[0] = readCSVscript.wristL;
        wrist[1] = readCSVscript.wristR;
    }


    void Update()
    {
        int count_L = trailRenderer[0].positionCount;
        int count_R = trailRenderer[1].positionCount;

        if (count_L > 0)
        {
            //Debug.Log("L wrist pos:" + trailRenderer[0].GetPosition(count_L - 1)
            //+ "\n\t    R wrist pos:" + trailRenderer[1].GetPosition(count_R - 1));
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            initPos = wrist[1].position;
            initRot = wrist[1].localRotation;
            Debug.Log("Press 'P' button\ninitPos = " + initPos);
        }

        if (count_R > 2)
        {
            CalcAngularVelocity(count_R - 1, 1);

            //Debug.Log("wristR rot: " + wrist[1].localEulerAngles);
            //wrist[1].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, Vector3.forward);
            //wrist[1].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, dir);


            Vector3[] pos = new Vector3[2];

            pos[0] = trailRenderer[0].GetPosition(count_L - 2);
            pos[1] = trailRenderer[0].GetPosition(count_L - 1);

            //dir += (pos[1] - pos[0]);
            //Quaternion rot = dir * Quaternion.Euler(0.0f, 90.0f, 0.0f)

            float gapX = Mathf.Abs(pos[1].x) - Mathf.Abs(pos[0].x);
            float gapY = Mathf.Abs(pos[1].y) - Mathf.Abs(pos[0].y);
            //Debug.Log("X gap: "+ gapX + ",  Y gap: " + gapY);

            angleX += gapX;
            angleY += gapY;

            //rot += dir * -50.0f;


            //wrist[1].localRotation = rot;
            //Debug.Log("wrist[1].localEulerAngles = " + wrist[1].localEulerAngles);


            StartCoroutine(quat4smplx.RotationDelay2(3, finalAngle * scalingValue, 1));
            //quat4smplx.PlayHandMotion(2, finalAngle * scalingValue, 1);

            //Debug.Log("Angle: " + finalAngle * scalingValue);
        }


        if (count_L > 2)
        {
            CalcAngularVelocity(count_L - 1, 0);

            //quat4smplx.PlayHandMotion(2, finalAngle * scalingValue, 0);

            //if (wrist[0].position.y > shldr[0].position.y)
            //{
            //    StartCoroutine(quat4smplx.RotationDelay2(1, finalAngle * scalingValue, 0));
            //}
            //else
            //{
            StartCoroutine(quat4smplx.RotationDelay2(3, finalAngle * scalingValue, 0));
            //}
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            //*** When recording... ***
            quat4smplx.SaveDic2Csv("C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.07-08/SMPLX-Unity/Assets/records/angle(1)_4.csv", record1);
            quat4smplx.SaveDic2Csv("C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.07-08/SMPLX-Unity/Assets/records/avg_angle(50,1)_4.csv", record2);
            //**************************
        }
    }


    void CalcAngularVelocity(int n, int hand)
    {
        beforeAngle = finalAngle;

        Vector3 currentPos = trailRenderer[hand].GetPosition(n);
        Vector3 beforePos = trailRenderer[hand].GetPosition(n - 1);

        dir = (currentPos - beforePos).normalized;

        originDir.Add(dir);
        //originDirX.Add(dir.x);
        //originDirY.Add(dir.y);
        //originDirZ.Add(dir.z);

        finalDir = NormalizeVector(originDir, dirFilterSize);
        //Debug.Log("dir = (" + dir.x +", " + dir.y + ", " + dir.z + ")\n\tfinalDir = (" + finalDir.x + ", " + finalDir.y + ", " + finalDir.z +")");
        //finalDir.x = MovingAverageFilter(originDirX, dirFilterSize);
        //finalDir.y = MovingAverageFilter(originDirY, dirFilterSize);
        //finalDir.z = MovingAverageFilter(originDirZ, dirFilterSize);

        Vector3 v1 = beforePos - pivot[hand].position;
        Vector3 v2 = currentPos - pivot[hand].position;

        angle = Mathf.Acos(Vector3.Dot(v1, v2) / (Vector3.Magnitude(v1) * Vector3.Magnitude(v2))) * (float)(180.0 / Mathf.PI);


        //*** When recording... ***
        Dictionary<int, float> dic = new Dictionary<int, float>();
        dic.Add(n, angle * scalingValue);
        record.Add(dic);
        //**************************

        if (float.IsNaN(angle))
        {
            if (hand == 0) angle = 0.0f;
            if (hand == 1) angle = beforeAngle;
        }

        if (angle < -100.0f || angle > 100.0f)
        {
            angle = 100.0f;
        }

        //Debug.Log("Frame no." + n + ",      angle = " + angle * scalingValue);

        //*** When recording... ***
        Dictionary<int, float> dic1 = new Dictionary<int, float>();
        dic1.Add(n, angle);
        record1.Add(dic1);
        //**************************

        //finalAngle = angle;
        originAngle.Add(angle);
        finalAngle = MovingAverageFilter(originAngle, angleFilterSize);
        Debug.Log("origin: " + angle * scalingValue + ",   final: " + finalAngle * scalingValue);

        Quaternion rot = Quaternion.AngleAxis(finalAngle * scalingValue, wrist[hand].position) * Quaternion.Euler(finalDir);
        wrist[hand].localRotation = rot;
        //wrist[hand].localRotation = Quaternion.AngleAxis(finalAngle * scalingValue, finalDir);


        //*** When recording... ***
        Dictionary<int, float> dic2 = new Dictionary<int, float>();
        dic2.Add(n, finalAngle * scalingValue);
        record2.Add(dic2);
        //**************************
    }


    float MovingAverageFilter(List<float> origin, int filterSize)
    {
        int size;
        float finalValue;

        if (origin.Count < filterSize) size = origin.Count;
        else size = filterSize;

        int index = origin.Count - 1;
        float sum = 0;

        for (int i = 0; i < size; i++)
        {
            sum += origin[index - i];
        }

        float avg = sum / size;
        finalValue = avg;

        //Debug.Log("angle count: " + origin.Count + ",   final angle: " + finalAngle);

        return finalValue;
    }

    Vector3 NormalizeVector(List<Vector3> origin, int filterSize)
    {
        int size;
        Vector3 finalVec;

        if (origin.Count < filterSize) size = origin.Count;
        else size = filterSize;

        int index = origin.Count - 1;
        Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);

        for (int i = 0; i < size; i++)
        {
            sum += origin[index - i];
        }

        Vector3 avg = sum.normalized;
        finalVec = avg;

        //Debug.Log("finalVec(" + finalVec.x + ",  " + finalVec.y + ", " + finalVec.z+")");
        //Debug.Log("angle count: " + origin.Count + ",   final angle: " + finalAngle);

        return finalVec;
    }

}
*/
