using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JointRange2 : MonoBehaviour
{
    //왼팔, 오른팔, 왼다리. 오른다리
    public BodyPart part1 = new BodyPart();
    public BodyPart part2 = new BodyPart();
    public BodyPart part3 = new BodyPart();
    public BodyPart part4 = new BodyPart();
       
    public List<Vector3> MergePatternList = new List<Vector3>();

    public List<Quaternion> UpperQuat = new List<Quaternion>();
    public List<Quaternion> LowerQuat = new List<Quaternion>();
    public List<Quaternion> HandQuat = new List<Quaternion>();
    public List<Quaternion> MergeQuat = new List<Quaternion>();


    public bool onTracking = false;

    public GameObject patternOBJ1, patternOBJ2, patternOBJ3, TestSphere;

    GameObject upperPattern, lowerPattern, handPattern;
    Quaternion initUpperRot, initLowerRot, initHandRot;

    public int Patternindex = 0;
    int mergePattern_index = 0;
    // Start is called before the first frame update
    void Start()
    {
      
        initialize_All();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            onTracking = true;
            StartCoroutine(RangeTracking());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            onTracking = false;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            //EachJointPattern();
        }

        if (Input.GetKeyDown(KeyCode.H))//초기화용
        {
            //initialize_All();
        }

        if (Input.GetKeyDown(KeyCode.A))    //벡터에서 다시 회전값 계산
        {
            ReCalc_LocalQuat(part1);
            ReCalc_LocalQuat(part2);
            ReCalc_LocalQuat(part3);
            ReCalc_LocalQuat(part4);

            Debug.Log("Finish Quat Calc");
        }
    }

    IEnumerator RangeTracking()
    {
        
        Patternindex++;
        DrawPartPattern(part1);
        DrawPartPattern(part2);
        DrawPartPattern(part3);
        DrawPartPattern(part4);

        yield return new WaitForSeconds(0.00f);
        if (onTracking == true)
        {
            StartCoroutine(RangeTracking());
        }

    }

    void ReCalc_LocalQuat(BodyPart partid)
    {
        List<Quaternion> tempUpper = new List<Quaternion>();
        List<Quaternion> tempLower = new List<Quaternion>();
        List<Quaternion> tempFinal = new List<Quaternion>();

        List<Quaternion> tempUpperGlobal = new List<Quaternion>();
        List<Quaternion> tempLowerGlobal = new List<Quaternion>();
        List<Quaternion> tempFinalGlobal = new List<Quaternion>();

        List<Quaternion> tempEachUpper = new List<Quaternion>();
        List<Quaternion> tempEachLower = new List<Quaternion>();
        List<Quaternion> tempEachFinal = new List<Quaternion>();

        for (int i=0; i< Patternindex; i++)
        {
            tempUpper.Add(Calc_Quat_VectorA_to_VectorB(partid.UpperPatternList[0], partid.UpperPatternList[i]));
            tempLower.Add(Calc_Quat_VectorA_to_VectorB(partid.LowerPatternList[0], partid.LowerPatternList[i]));
            tempFinal.Add(Calc_Quat_VectorA_to_VectorB(partid.FinalPatternList[0], partid.FinalPatternList[i]));

            tempUpperGlobal.Add(tempUpper[i]);
            tempLowerGlobal.Add(tempUpper[i] * tempLower[i]);
            tempFinalGlobal.Add(tempUpper[i] * tempLower[i] * tempFinal[i]);

            tempEachLower.Add(tempLowerGlobal[i] * Quaternion.Inverse(tempUpperGlobal[i]));
            tempEachFinal.Add(tempFinalGlobal[i] * Quaternion.Inverse(tempLowerGlobal[i]));
        }

        partid.SetUpperQuatLocal(tempUpper);
        partid.SetLowerQuatLocal(tempLower);
        partid.SetFinalQuatLocal(tempFinal);

        partid.SetUpperQuatGlobal(tempUpperGlobal);
        partid.SetLowerQuatGlobal(tempLowerGlobal);
        partid.SetFinalQuatGlobal(tempFinalGlobal);

        partid.EachJointGlobal1 = tempUpperGlobal;
        partid.EachJointGlobal2 = tempEachLower;
        partid.EachJointGlobal3 = tempEachFinal;


    }

    void Merge3Pattern()
    {

    }
    void DrawPartPattern(BodyPart partid)   //신체 부위에 따라 계산해서 그림
    {
        Vector3 UpperStart, UpperDest, UpperResult, LowerStart, LowerDest, LowerResult, FinalStart, FinalDest, FinalResult;
        Vector3 UpperPosNorm, LowerPosNorm, FinalPosNorm;

        UpperStart = partid.UpperRange.transform.position;
        UpperDest = partid.UpperVector.transform.position;
        UpperResult = UpperDest - UpperStart;

        LowerStart = partid.LowerRange.transform.position;
        LowerDest = partid.LowerVector.transform.position;
        LowerResult = LowerDest - LowerStart;

        FinalStart = partid.FinalRange.transform.position;
        FinalDest = partid.FinalVector.transform.position;
        FinalResult = FinalDest - FinalStart;

        UpperPosNorm = UpperResult.normalized / 10.0f;
        LowerPosNorm = Quaternion.Inverse(partid.UpperVector.transform.rotation) * LowerResult.normalized / 10.0f;
        FinalPosNorm = Quaternion.Inverse(partid.LowerVector.transform.rotation) * FinalResult.normalized / 10.0f;

        partid.UpperPatternList.Add(UpperResult.normalized / 10.0f);       
        partid.LowerPatternList.Add(Quaternion.Inverse(partid.UpperVector.transform.rotation) * LowerResult.normalized / 10.0f);
        partid.FinalPatternList.Add(Quaternion.Inverse(partid.LowerVector.transform.rotation) * FinalResult.normalized / 10.0f);

        upperPattern = Instantiate(patternOBJ1, new Vector3(0, 0, 0), Quaternion.identity, partid.UpperRange.transform);
        upperPattern.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        upperPattern.transform.localPosition = UpperPosNorm;
        upperPattern.name = "sphere" + Patternindex;

        lowerPattern = Instantiate(patternOBJ2, new Vector3(0, 0, 0), Quaternion.identity, partid.LowerRange.transform);
        lowerPattern.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        lowerPattern.transform.localPosition = LowerPosNorm;
        lowerPattern.name = "sphere" + Patternindex;

        handPattern = Instantiate(patternOBJ3, new Vector3(0, 0, 0), Quaternion.identity, partid.FinalRange.transform);
        handPattern.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        handPattern.transform.localPosition = FinalPosNorm;
        handPattern.name = "sphere" + Patternindex;

    }


    void initialize_All()
    {
        part1.UpperRange.transform.rotation = Quaternion.identity;
        part1.LowerRange.transform.rotation = Quaternion.identity;
        part1.FinalRange.transform.rotation = Quaternion.identity;

        part1.SetUpper(part1.UpperPart.transform.rotation);
        part1.SetLower(part1.LowerPart.transform.rotation);
        part1.SetFinal(part1.FinalPart.transform.rotation);

        part2.UpperRange.transform.rotation = Quaternion.identity;
        part2.LowerRange.transform.rotation = Quaternion.identity;
        part2.FinalRange.transform.rotation = Quaternion.identity;

        part2.SetUpper(part2.UpperPart.transform.rotation);
        part2.SetLower(part2.LowerPart.transform.rotation);
        part2.SetFinal(part2.FinalPart.transform.rotation);

        part3.UpperRange.transform.rotation = Quaternion.identity;
        part3.LowerRange.transform.rotation = Quaternion.identity;
        part3.FinalRange.transform.rotation = Quaternion.identity;

        part3.SetUpper(part3.UpperPart.transform.rotation);
        part3.SetLower(part3.LowerPart.transform.rotation);
        part3.SetFinal(part3.FinalPart.transform.rotation);

        part4.UpperRange.transform.rotation = Quaternion.identity;
        part4.LowerRange.transform.rotation = Quaternion.identity;
        part4.FinalRange.transform.rotation = Quaternion.identity;

        part4.SetUpper(part4.UpperPart.transform.rotation);
        part4.SetLower(part4.LowerPart.transform.rotation);
        part4.SetFinal(part4.FinalPart.transform.rotation);
    }

    Quaternion Calc_Quat_VectorA_to_VectorB(Vector3 A, Vector3 B)   //A에서 B로 가는 쿼터니온 계산
    {
        float thetha_radian;
        Vector3 Axis;
        Quaternion result;

        thetha_radian = Mathf.Acos(Vector3.Dot(A, B) / (A.magnitude * B.magnitude));

        //Debug.Log("각도 : " + thetha_radian);
        Axis = Vector3.Cross(A, B).normalized;

        result.w = Mathf.Cos(thetha_radian / 2);
        result.x = Mathf.Sin(thetha_radian / 2) * Axis.x;
        result.y = Mathf.Sin(thetha_radian / 2) * Axis.y;
        result.z = Mathf.Sin(thetha_radian / 2) * Axis.z;

        //Debug.Log("quat : " + result.w + "/" + result.x + "/" + result.y + "/" + result.z);

        return result;

    }

    IEnumerator Drawing_MergePattern(int index)
    {
        GameObject mergedPattern;

        if (index < 190)
        {
            mergedPattern = Instantiate(patternOBJ1, new Vector3(0, 0, 0), Quaternion.identity, TestSphere.transform);
        }
        else if (index >= 190 && index < 390)
        {
            mergedPattern = Instantiate(patternOBJ2, new Vector3(0, 0, 0), Quaternion.identity, TestSphere.transform);
        }
        else
        {
            mergedPattern = Instantiate(patternOBJ3, new Vector3(0, 0, 0), Quaternion.identity, TestSphere.transform);
        }

        mergedPattern.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        mergedPattern.transform.localPosition = MergePatternList[index];
        mergedPattern.name = "sphere" + index;

        yield return new WaitForSeconds(0.0f);
        if (index < MergePatternList.Count)
        {
            Debug.Log(mergePattern_index + "뭐징");
            mergePattern_index++;
            StartCoroutine(Drawing_MergePattern(mergePattern_index));
        }

    }

    

}

[Serializable]
public class BodyPart
{
    public string PartName;
    public GameObject UpperPart, LowerPart, FinalPart;
    public GameObject UpperRange, LowerRange, FinalRange;
    public GameObject UpperVector, LowerVector, FinalVector;
    public GameObject UpperSphere, LowerSphere, FinalSphere;

    private Quaternion initUpperRot, initLowerRot, initFinalRot;

    public List<Vector3> UpperPatternList = new List<Vector3>();
    public List<Vector3> LowerPatternList = new List<Vector3>();
    public List<Vector3> FinalPatternList = new List<Vector3>();

    public List<Quaternion> UpperQuatLocal;
    public List<Quaternion> LowerQuatLocal;
    public List<Quaternion> FinalQuatLocal;

    public List<Quaternion> UpperQuatGlobal;
    public List<Quaternion> LowerQuatGlobal;
    public List<Quaternion> FinalQuatGlobal;

    public List<Quaternion> EachJointGlobal1;
    public List<Quaternion> EachJointGlobal2;
    public List<Quaternion> EachJointGlobal3;

    public BodyPart()
    {
        //Debug.Log("part create :" + PartName);
    }

    public void SetUpper(Quaternion value)
    {
        initUpperRot = value;
    }

    public void SetLower(Quaternion value)
    {
        initLowerRot = value;
    }

    public void SetFinal(Quaternion value)
    {
        initFinalRot = value;
    }

    public void SetUpperQuatLocal(List<Quaternion> listValue)
    {
        UpperQuatLocal = listValue;
    }

    public void SetLowerQuatLocal(List<Quaternion> listValue)
    {
        LowerQuatLocal = listValue;
    }

    public void SetFinalQuatLocal(List<Quaternion> listValue)
    {
        FinalQuatLocal = listValue;
    }

    public void SetUpperQuatGlobal(List<Quaternion> listValue)
    {
        UpperQuatGlobal = listValue;
    }

    public void SetLowerQuatGlobal(List<Quaternion> listValue)
    {
        LowerQuatGlobal = listValue;
    }

    public void SetFinalQuatGlobal(List<Quaternion> listValue)
    {
        FinalQuatGlobal = listValue;
    }

    public Quaternion GetUpper()
    {
        return initUpperRot;
    }

    public Quaternion GetLower()
    {
        return initLowerRot;
    }

    public Quaternion GetFinal()
    {
        return initFinalRot;
    }

    public List<Quaternion> GetUpperQuatGlobal()
    {
        return UpperQuatGlobal;
    }

    public List<Quaternion> GetLowerQuatGlobal()
    {
        return LowerQuatGlobal;
    }

    public List<Quaternion> GetFinalQuatGlobal()
    {
        return FinalQuatGlobal;
    }
}
