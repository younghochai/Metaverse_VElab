using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class QuatDataManager : MonoBehaviour
{
    public List<Quaternion> Unity_QuatList_1 = new List<Quaternion>();  //몸 A3
    public List<Quaternion> Unity_QuatList_2 = new List<Quaternion>();  //허리 80
    public List<Quaternion> Unity_QuatList_3 = new List<Quaternion>();  //오른쪽 어깨 9F  
    public List<Quaternion> Unity_QuatList_4 = new List<Quaternion>();  //오른쪽 팔꿈치 8B
    public List<Quaternion> Unity_QuatList_5 = new List<Quaternion>();  //왼쪽 어깨 90
    public List<Quaternion> Unity_QuatList_6 = new List<Quaternion>();  //왼쪽 팔꿈치 99

    public List<Quaternion> diffList = new List<Quaternion>();

  
    public List<Vector3> Raw_to_Euler = new List<Vector3>();
    public List<Vector3> Diff_to_Euler = new List<Vector3>();
    public List<Vector3> Sensor_Diff_euelr = new List<Vector3>();
    public List<Vector3> Case1List = new List<Vector3>();
    public List<Vector3> Case2List = new List<Vector3>();

    public List<Vector3> RUpper_axis = new List<Vector3>();
    public List<Vector3> RLower_axis = new List<Vector3>();
    public List<Vector3> LUpper_axis = new List<Vector3>();
    public List<Vector3> LLower_axis = new List<Vector3>();
    public List<Vector3> Hand_axis = new List<Vector3>();

    public Quaternion init_hips_rot, init_RupperArm_rot, init_RlowerArm_rot, init_LupperArm_rot, init_LlowerArm_rot, init_hand_rot;

    string filePath1 = "Assets/Resources/Angle_Varying1.csv";
    string filePath2 = "Assets/Resources/Angle_Varying2.csv";
    string filePath3 = "Assets/Resources/Angle_Varying3.csv";
    string filePath4 = "Assets/Resources/Angle_Varying4.csv";
    string filePath5 = "Assets/Resources/Angle_Varying5.csv";
    //string filePath6 = "Assets/Resources/Angle_Varying6.csv";
    //string filePath7 = "Assets/Resources/Angle_Varying7.csv";

    string fileTest = "Assets/Resources/datadefault.csv";
    string fileTestWrite = "Assets/Resources/Euler.csv";
    string fileEulerUp = "Assets/Resources/Euler.csv";
    string fileEulerDown = "Assets/Resources/EulerDown.csv";
    string fileQuat = "Assets/Resources/QuatOrder.csv";

    float Radian2Degree = 180 / Mathf.PI;
    float Degree2Radian = Mathf.PI / 180;

    FileStream fs, Test;
    StreamWriter sw;
    StreamReader sr;
    string[] records;
    public int File_index = 1;

    public bool DrawAxis = false;
    public Quaternion firstLine = Quaternion.identity;
    public Quaternion attention;

    public bool CheckToUp = true;
    
    // Start is called before the first frame update
    void Start()
    {
        //fs = new FileStream(filePath1, FileMode.OpenOrCreate);
        //Test = new FileStream(fileTestWrite, FileMode.OpenOrCreate);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))       //동작 1
        {
            fs.Close();

            fs = new FileStream(filePath1, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            File_index = 1;
            Debug.Log("File 1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))       //동작 2
        {
            fs.Close();

            fs = new FileStream(filePath2, FileMode.OpenOrCreate);

            File_index = 2;
            Debug.Log("File 2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))       //동작 3
        {
            fs.Close();

            fs = new FileStream(filePath3, FileMode.OpenOrCreate);

            File_index = 3;
            Debug.Log("File 3");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))       //동작 4
        {
            fs.Close();

            fs = new FileStream(filePath4, FileMode.OpenOrCreate);

            File_index = 4;
            Debug.Log("File 4");
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))       //동작 5
        {
            fs.Close();

            fs = new FileStream(filePath5, FileMode.OpenOrCreate);

            File_index = 5;
            Debug.Log("File 5");
        }

        if (Input.GetKeyDown(KeyCode.J))        //Euler.csv에 파일 쓰기
        {
            WriteToFiles();
        }

        if (Input.GetKeyDown(KeyCode.K))        //Read
        {
            if((Unity_QuatList_2.Count + Unity_QuatList_3.Count + Unity_QuatList_4.Count) > 0)
            {
                Unity_QuatList_2.Clear();
                Unity_QuatList_3.Clear();
                Unity_QuatList_4.Clear();
            }

            ReadFiles();
        }

        if (Input.GetKeyDown(KeyCode.B))        
        {
            fs.Close();

            fs = new FileStream(fileTest, FileMode.OpenOrCreate);
            Quaternion testData, temp, pelvis, temp1, temp2;
            
            string[] fields;
            sr = new StreamReader(fs);
            records = sr.ReadToEnd().Split('\n');

            //attention.w = 0.0534851f;
            //attention.x = 0.133592f;
            //attention.y = 0.68101f;
            //attention.z = 0.717996f;
            for (int i = 2; i < records.Length - 1; i++)
            {
                fields = records[i].Split(',');

                testData.w = float.Parse(fields[1]);
                testData.x = float.Parse(fields[2]);
                testData.y = float.Parse(fields[3]);
                testData.z = float.Parse(fields[4]);


                Unity_QuatList_2.Add(testData);
                Raw_to_Euler.Add(Quat_To_Euler(testData));

            }


            //////////////////////////////////////////////ashock's
            //for (int i = 2; i < records.Length - 1; i++)
            //{
            //    fields = records[i].Split(',');

            //    testData.w = float.Parse(fields[1]);
            //    testData.x = float.Parse(fields[2]);
            //    testData.y = float.Parse(fields[3]);
            //    testData.z = float.Parse(fields[4]);

            //    pelvis.w = float.Parse(fields[5]);
            //    pelvis.x = float.Parse(fields[6]);
            //    pelvis.y = float.Parse(fields[7]);
            //    pelvis.z = float.Parse(fields[8]);

            //    temp1 = Quaternion.Inverse(pelvis) * testData * Quaternion.Inverse(attention);            
            //    //temp2 = Quaternion.Inverse(pelvis) * temp1;

            //    Unity_QuatList_2.Add(temp1);

            //}


            ////////////////////////매 프레임 사이 계산
            //for (int i = 1; i < Unity_QuatList_2.Count; i++)
            //{
            //    Quaternion temp2;

            //    temp2 = Unity_QuatList_2[i] * Quaternion.Inverse(Unity_QuatList_2[i - 1]);

            //    diffList.Add(Quaternion.Normalize(temp2));
            //}


            ////////////////////////////처음과 각 프레임을 계산
            //temp = Unity_QuatList_2[0];
            //for (int i = 0; i < Unity_QuatList_2.Count; i++)
            //{
            //    temp1 = Unity_QuatList_2[i] * Quaternion.Inverse(temp);


            //    diffList.Add(Quaternion.Normalize(temp1));
            //    Diff_to_Euler.Add(Quaternion.ToEulerAngles(temp1) * Radian2Degree);
            //}

            /////////////////////////////각도 확인
            temp = Unity_QuatList_2[10];

            for(int i=10; i<Unity_QuatList_2.Count; i++)
            {
                temp1 = Unity_QuatList_2[i] * Quaternion.Inverse(temp);
                diffList.Add(Quaternion.Normalize(temp1));

                Sensor_Diff_euelr.Add(Quat_To_Euler(temp1));

                Diff_to_Euler.Add(Quaternion.ToEulerAngles(SensorMapping(temp1)) * Radian2Degree);
            }



            Debug.Log("File Reading done");
            sr.Close();
            return;

        }

        if (Input.GetKeyDown(KeyCode.N))    //가이드 데이터 리딩용
        {
            //ReadEulerAngle();
            
            ReadQuaternion();
        }

    }

    void WriteToFiles()
    {

        ////////////기본 파일 쓰기
        //sw = new StreamWriter(fs);
        //sw.WriteLine("Unity Data Recording");
        //sw.WriteLine("counter, quatW, quatX, quatY, quatZ order, UpperArm / Lower Arm / Hand ");
        //sw.WriteLine("init hip/upper/lower/hand rot," + init_hips_rot.w + "," + init_hips_rot.x + "," + init_hips_rot.y + "," + init_hips_rot.z + "," +
        //    init_upperArm_rot.w + "," + init_upperArm_rot.x + "," + init_upperArm_rot.y + "," + init_upperArm_rot.z + "," +
        //    init_lowerArm_rot.w + "," + init_lowerArm_rot.x + "," + init_lowerArm_rot.y + "," + init_lowerArm_rot.z + "," +
        //    init_hand_rot.w + "," + init_hand_rot.x + "," + init_hand_rot.y + "," + init_hand_rot.z);

        //for(int i=0; i< Unity_QuatList_2.Count; i++)
        //{
        //    sw.WriteLine(i + "," + Unity_QuatList_2[i].w + "," + Unity_QuatList_2[i].x + "," + Unity_QuatList_2[i].y + "," + Unity_QuatList_2[i].z + "," +
        //        Unity_QuatList_3[i].w + "," + Unity_QuatList_3[i].x + "," + Unity_QuatList_3[i].y + "," + Unity_QuatList_3[i].z + "," +
        //        Unity_QuatList_4[i].w + "," + Unity_QuatList_4[i].x + "," + Unity_QuatList_4[i].y + "," + Unity_QuatList_4[i].z);
        //}

        //Debug.Log("File Writing done");
        //sw.Close();

        /////////////////////테스트용 파일 쓰기
        fs.Close();
        sw = new StreamWriter(Test);
        sw.WriteLine("Unity Data Recording");
        sw.WriteLine("counter, vector x, vector y, vector z order, Changed data / Raw Data / ");

        //for (int i = 0; i < Raw_to_Euler.Count; i++)
        //{
        //    //sw.WriteLine(i + "," + Diff_to_Euler[i].x + "," + Diff_to_Euler[i].y + "," + Diff_to_Euler[i].z + "," + Raw_to_Euler[i].x + "," +
        //    //    Raw_to_Euler[i].y + "," + Raw_to_Euler[i].z + "," + Sensor_Diff_euelr[i].x + "," + Sensor_Diff_euelr[i].y + "," + Sensor_Diff_euelr[i].z);

        //    sw.WriteLine(i + "," + Raw_to_Euler[i].x + "," + Raw_to_Euler[i].y + "," + Raw_to_Euler[i].z);
        //}

        for (int i = 0; i < Sensor_Diff_euelr.Count; i++)
        {
            //sw.WriteLine(i + "," + Sensor_Diff_euelr[i].x + "," + Sensor_Diff_euelr[i].y + "," + Sensor_Diff_euelr[i].z);       //오일러각 출력

            sw.WriteLine(i + "," + diffList[i].w + "," + diffList[i].x + "," + diffList[i].y + "," + diffList[i].z);       //쿼터니언 출력
        }

        Debug.Log("File Writing done");
        sw.Close();
        Test.Close();
    }

    void ReadFiles()
    {
        string[] fields;
        sr = new StreamReader(fs);
        records = sr.ReadToEnd().Split('\n');

        for (int i = 2; i < records.Length - 1; i++)
        {
            fields = records[i].Split(',');

            Quaternion upperData, lowerData, handData;
            

            if (i == 2)
            {
                Quaternion hips;
                hips.w = float.Parse(fields[1]);
                hips.x = float.Parse(fields[2]);
                hips.y = float.Parse(fields[3]);
                hips.z = float.Parse(fields[4]);

                upperData.w = float.Parse(fields[5]);
                upperData.x = float.Parse(fields[6]);
                upperData.y = float.Parse(fields[7]);
                upperData.z = float.Parse(fields[8]);

                lowerData.w = float.Parse(fields[9]);
                lowerData.x = float.Parse(fields[10]);
                lowerData.y = float.Parse(fields[11]);
                lowerData.z = float.Parse(fields[12]);

                handData.w = float.Parse(fields[13]);
                handData.x = float.Parse(fields[14]);
                handData.y = float.Parse(fields[15]);
                handData.z = float.Parse(fields[16]);

                init_hips_rot = hips;
                init_RupperArm_rot = upperData;
                init_RlowerArm_rot = lowerData;
                init_hand_rot = handData;
                continue;
            }

            upperData.w = float.Parse(fields[1]);
            upperData.x = float.Parse(fields[2]);
            upperData.y = float.Parse(fields[3]);
            upperData.z = float.Parse(fields[4]);

            lowerData.w = float.Parse(fields[5]);
            lowerData.x = float.Parse(fields[6]);
            lowerData.y = float.Parse(fields[7]);
            lowerData.z = float.Parse(fields[8]);

            handData.w = float.Parse(fields[9]);
            handData.x = float.Parse(fields[10]);
            handData.y = float.Parse(fields[11]);
            handData.z = float.Parse(fields[12]);


            Unity_QuatList_2.Add(upperData);
            Unity_QuatList_3.Add(lowerData);
            Unity_QuatList_4.Add(handData);

            if(DrawAxis == true)
            {
                Vector3 UpperVector, LowerVector, HandVector;

                UpperVector.x = float.Parse(fields[13]);
                UpperVector.y = float.Parse(fields[14]);
                UpperVector.z = float.Parse(fields[15]);

                LowerVector.x = float.Parse(fields[16]);
                LowerVector.y = float.Parse(fields[17]);
                LowerVector.z = float.Parse(fields[18]);

                HandVector.x = float.Parse(fields[19]);
                HandVector.y = float.Parse(fields[20]);
                HandVector.z = float.Parse(fields[21]);

                RUpper_axis.Add(UpperVector);
                RLower_axis.Add(LowerVector);
                Hand_axis.Add(HandVector);

            }

        }

        Debug.Log("File Reading done");
        sr.Close();
        return;

    }

    void ReadEulerAngle()
    {
        fs.Close();
        Test.Close();

        //if(CheckToUp == true)
        //{
        //    fs = new FileStream(fileEulerUp, FileMode.OpenOrCreate);
        //}
        //else
        //{
        //    fs = new FileStream(fileEulerDown, FileMode.OpenOrCreate);
        //}

        fs = new FileStream(fileEulerUp, FileMode.OpenOrCreate);

        Vector3 goUp_c1, goUp_c2, goDown_c1, goDown_c2;

        string[] fields;
        sr = new StreamReader(fs);
        records = sr.ReadToEnd().Split('\n');

        //if(CheckToUp == true)
        //{
        //    for (int i = 2; i < records.Length - 1; i++)
        //    {
        //        fields = records[i].Split(',');

        //        goUp_c1.x = float.Parse(fields[1]);
        //        goUp_c1.y = float.Parse(fields[2]);
        //        goUp_c1.z = float.Parse(fields[3]);

        //        goUp_c2.x = float.Parse(fields[4]);
        //        goUp_c2.y = float.Parse(fields[5]);
        //        goUp_c2.z = float.Parse(fields[6]);

        //        Case1List.Add(goUp_c1);
        //        Case2List.Add(goUp_c2);

        //    }
        //}

        //else
        //{
        //    for (int i = 2; i < records.Length - 1; i++)
        //    {
        //        fields = records[i].Split(',');

        //        goDown_c1.x = float.Parse(fields[1]);
        //        goDown_c1.y = float.Parse(fields[2]);
        //        goDown_c1.z = float.Parse(fields[3]);

        //        goDown_c2.x = float.Parse(fields[4]);
        //        goDown_c2.y = float.Parse(fields[5]);
        //        goDown_c2.z = float.Parse(fields[6]);

        //        Case1List.Add(goDown_c1);
        //        Case2List.Add(goDown_c2);

        //    }
        //}

        for (int i = 2; i < records.Length - 1; i++)
        {
            fields = records[i].Split(',');

            goUp_c1.x = float.Parse(fields[1]);
            goUp_c1.y = float.Parse(fields[2]);
            goUp_c1.z = float.Parse(fields[3]);

            
            Case1List.Add(goUp_c1);
           

        }


            Debug.Log("File Reading done");
        sr.Close();
        return;
    }

    Vector3 Quat_To_Euler(Quaternion Q)  //쿼터니언 데이터를 오일러로 변환
    {
        float Roll, Pitch, Yaw;
        float inputY, inputX;
        float q0 = Q.w, q1 = Q.x, q2 = Q.y, q3 = Q.z;
        //위키 공식
        float sinp;
        Roll = Mathf.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (q1 * q1 + q2 * q2)) * Radian2Degree;

        sinp = 2 * (q0 * q2 - q3 * q1);

        if (Mathf.Abs(sinp) >= 1)
        {
            if (sinp >= 0)
            {
                Pitch = Mathf.PI / 2.0f * Radian2Degree;
            }
            else
            {
                Pitch = -Mathf.PI / 2.0f * Radian2Degree;
            }
        }
        else
        {
            Pitch = Mathf.Asin(sinp) * Radian2Degree;
        }

        Yaw = Mathf.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (q2 * q2 + q3 * q3)) * Radian2Degree;


        //Manual 공식
        //inputY = 2 * (q0 * q1 + q2 * q3);
        //inputX = -1 + 2 * (q0 * q0 + q3 * q3);

        //Roll = Mathf.Atan2(inputY, (inputX)) * 180.0f / Mathf.PI;
        ////Debug.Log("Roll x, y 판별 : " + inputX + "/ " + inputY);


        //Pitch = -Mathf.Asin(2 * (-q0 * q2 + q3 * q1)) * 180.0f / Mathf.PI;

        //inputY = 2 * (q0 * q3 + q1 * q2);
        //inputX = -1 + 2 * (q0 * q0 + q1 * q1);

        //Yaw = Mathf.Atan2(inputY, (inputX)) * 180.0f / Mathf.PI;
        //Debug.Log("Yaw x, y 판별 : " + inputX + "/ " + inputY);
        return new Vector3(Roll, Pitch, Yaw);
    }

    Quaternion SensorMapping(Quaternion target)
    {
        Quaternion GlobalToUnity;

        GlobalToUnity.w = -target.w;
        GlobalToUnity.x = -target.y;
        GlobalToUnity.y = target.z;
        GlobalToUnity.z = target.x;

        
        //Debug.Log("relative : " + transform + relativeAngle);

        return GlobalToUnity;
    }

    void ReadQuaternion()
    {
        FileStream quatStream = new FileStream(fileQuat, FileMode.OpenOrCreate);

        sr = new StreamReader(quatStream);
        string[] fields;
        records = sr.ReadToEnd().Split('\n');

        Quaternion xRot, yRot, zRot, finalRot;

        for (int i = 2; i < records.Length - 1; i++)
        {
            fields = records[i].Split(',');

            xRot.w = float.Parse(fields[1]);
            xRot.x = float.Parse(fields[2]);
            xRot.y = float.Parse(fields[3]);
            xRot.z = float.Parse(fields[4]);

            yRot.w = float.Parse(fields[5]);
            yRot.x = float.Parse(fields[6]);
            yRot.y = float.Parse(fields[7]);
            yRot.z = float.Parse(fields[8]);

            zRot.w = float.Parse(fields[9]);
            zRot.x = float.Parse(fields[10]);
            zRot.y = float.Parse(fields[11]);
            zRot.z = float.Parse(fields[12]);

            //finalRot = xRot * yRot * zRot;

            finalRot = zRot * yRot;

            diffList.Add(finalRot);

            Sensor_Diff_euelr.Add(Quat_To_Euler(finalRot));
        }
        Debug.Log("Quaternion reading done");
        sr.Close();
        quatStream.Close();

        return;
    }

    

    Quaternion EulerToQuat(Vector3 Angle)
    {
        float angleX, angleY, angleZ;
        Quaternion q;
        float q1, q2, q3, q4, scale;

        angleX = Angle.x * Degree2Radian;
        angleY = Angle.y * Degree2Radian;
        angleZ = Angle.z * Degree2Radian;

        //ZYX -> 순서로 회전
        q1 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) + Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f);
        q2 = Mathf.Sin(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleY / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f);
        q3 = -Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleY / 2.0f) + Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleZ / 2.0f);
        q4 = Mathf.Sin(angleX / 2.0f) * Mathf.Sin(angleY / 2.0f) * Mathf.Cos(angleZ / 2.0f) + Mathf.Sin(angleZ / 2.0f) * Mathf.Cos(angleX / 2.0f) * Mathf.Cos(angleY / 2.0f);


        scale = Mathf.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);

        q.w = q1 / scale;
        q.x = q2 / scale;
        q.y = q3 / scale;
        q.z = q4 / scale;

        return q;
    }


}
