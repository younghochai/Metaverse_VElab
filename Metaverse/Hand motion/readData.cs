using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

public class readData : MonoBehaviour
{
    //public List<Quaternion> load_data = new List<Quaternion>();
    public List<List<List<Quaternion>>> L_pose_idx = new List<List<List<Quaternion>>>();
    public List<List<List<Quaternion>>> R_pose_idx = new List<List<List<Quaternion>>>();
    public List<List<Quaternion>> L_data = new List<List<Quaternion>>();
    public List<List<Quaternion>> R_data = new List<List<Quaternion>>();
    public List<Quaternion> sensor_data = new List<Quaternion>();

    public string file_path = "C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.02/SMPLX-Unity/Assets/";


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            for (int idx = 1; idx < 5; idx++)
                ReadQuaternion(file_path + "basis_pose_" + idx + ".csv");
        }
    }

    //void ReadQuaternion(string file_path)
    //{

    //    FileStream fileStream = new FileStream(file_path, FileMode.OpenOrCreate);
    //    StreamReader sr = new StreamReader(fileStream);

    //    string[] row = sr.ReadToEnd().Split('\n');

    //    for (int i = 1; i < row.Length - 1; i++)
    //    {
    //        string[] column = row[i].Split(',');

    //        for (int j = 0; j < column.Length / 4; j++)
    //        {
    //            float[] joint = new float[4];

    //            joint[0] = float.Parse(column[j * 4]);
    //            joint[1] = float.Parse(column[j * 4 + 1]);
    //            joint[2] = float.Parse(column[j * 4 + 2]);
    //            joint[3] = float.Parse(column[j * 4 + 3]);

    //            Quaternion value = new Quaternion(joint[1], joint[2], joint[3], joint[0]);

    //            addQuatToList(sensor_data, value);
    //        }

    //    }

    //    sr.Close();
    //    fileStream.Close();


    //    Debug.Log("Quaternion reading done");

    //    return;
    //}


    void ReadQuaternion(string file_path)
    {
        FileStream fileStream = new FileStream(file_path, FileMode.OpenOrCreate);
        StreamReader sr = new StreamReader(fileStream);

        string[] row = sr.ReadToEnd().Split('\n');

        // i == frame
        for (int i = 2; i < row.Length; i++)
        {
            string[] column = row[i].Split(',');

            int joint_num = (column.Length / 3) / 2;


            // j == joint
            for (int j = 0; j < joint_num * 2; j++)
            {

                if (i == 2)
                {
                    L_data.Add(new List<Quaternion>());
                    R_data.Add(new List<Quaternion>());
                }


                float[] axis = new float[3];

                axis[0] = float.Parse(column[j * 3]);
                axis[1] = float.Parse(column[j * 3 + 1]);
                axis[2] = float.Parse(column[j * 3 + 2]);

                Quaternion value = Quaternion.Euler(new Vector3(axis[0], axis[1], axis[2]));


                // Left
                if (j < joint_num)
                    L_data[j].Add(value);

                // Right
                if (j >= joint_num)
                    R_data[j - joint_num].Add(value);

            }
        }

        L_pose_idx.Add(new List<List<Quaternion>>(L_data));
        R_pose_idx.Add(new List<List<Quaternion>>(R_data));


        sr.Close();
        fileStream.Close();


        Debug.Log("Quaternion reading done");

        return;
    }

}


/*
public class readData : MonoBehaviour
{
    public List<Quaternion> load_data = new List<Quaternion>();
    public List<Quaternion> data = new List<Quaternion>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            ReadQuaternion("C:/Users/pssil/OneDrive/πŸ≈¡ »≠∏È/velab/2023.02/SMPLX-Unity/Assets/test_data.csv");
        }
    }


    //void data_read_csv_meta75(string[] file_namelist)
    //{

    //    string prefix = "E:/Sensor_Dataload_0113/QuaternionDB/Dot/";

    //    foreach (string each_file in file_namelist)
    //    {
    //        lst_DBpath.Add(prefix + each_file + "test1.csv");
    //        Debug.Log(prefix + each_file + "test1.csv");
    //    }

    //    foreach (string file_path in lst_DBpath)
    //    {
    //        ReadQuaternion(file_path);
    //    }
    //}


    void ReadQuaternion(string file_path)
    {

        FileStream fileStream = new FileStream(file_path, FileMode.OpenOrCreate);
        StreamReader sr = new StreamReader(fileStream);

        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        for (int i = 2; i < records.Length; i++)
        {
            fields = records[i].Split(',');
            int fields_cnt = 0;

            float[] joint = new float[3];

            for (int j = 0; j < (fields.Length / 3); j++)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    joint[axis] = float.Parse(fields[fields_cnt]);
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

                //Quaternion value = new Quaternion(joint[0], joint[1], joint[2]);
                Quaternion value = Quaternion.Euler(new Vector3(joint[0], joint[1], joint[2]));

                data.Add(value);
            }

        }

        sr.Close();
        fileStream.Close();


        load_data = data;

        Debug.Log("Quaternion reading done");

        return;
    }
}
*/