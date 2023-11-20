using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
public class Python_net : MonoBehaviour
{
    TcpClient client;
    string serverIP = "127.0.0.1";
    int port = 8001;
    byte[] receivedBuffer;
    StreamReader reader;
    bool socketReady = false;
    NetworkStream stream;
    string load_path= "Assets/Metaverse_BCA/BCA_test_data_label0_datacnt_7.txt";
    float[] ld_bcadata;



    public bool stream_write = false;
    public bool is_connect_close = false;
    public bool is_connect_open = false;



    //bool stream_write = false;
    int send_cnt = 0;
    int data_Frm;
    public List<Vector3> cur_plpose_vec = new List<Vector3>();
    public List<float> sendDataQuaternion = new List<float>();
    public bool data_load_Available = false;

    public int predict_label = -1;

    public bool newrecongition = false;
    // Start is called before the first frame update
    void Start()
    {
        //ld_bcadata = ReadBCA_Data(load_path);
        //Debug.Log("The BCA putted array");

        //data_Frm = ld_bcadata.Length / 12;

        //CheckReceive();
    }
    // git test
    // Update is called once per frame
    void Update()
    {
        if (is_connect_open)
        {
            if (!socketReady)
            {
                Debug.Log("CheckReceive :" + is_connect_open);
                CheckReceive();
                is_connect_open = false;
            }

        }
        if (socketReady)
        {


            if (stream.DataAvailable)
            {
                receivedBuffer = new byte[100];
                stream.Read(receivedBuffer, 0, receivedBuffer.Length); // stream에 있던 바이트배열 내려서 새로 선언한 바이트배열에 넣기
                string msg = Encoding.UTF8.GetString(receivedBuffer, 0, receivedBuffer.Length); // byte[] to string
                Debug.Log("recognition Result :" + int.Parse(msg));
                predict_label = int.Parse(msg);
                newrecongition = true;
            }


            if (is_connect_close)
            {

                client.Close();
                CloseSocket();

                is_connect_close = false;
                socketReady = false;
            }                
            


            if (Input.GetKeyDown(KeyCode.Q))
            {
                //reader.Close();
                client.Close();
                CloseSocket();

            }

            if (Input.GetKeyDown(KeyCode.A))
            {

                stream_write = true;
            }


        }

        if (stream_write)
        {
            //data_write_frm();

            if (sendDataQuaternion.Count > 0)
            {
                ld_bcadata = sendDataQuaternion.ToArray();
                data_Frm = ld_bcadata.Length / 24;
                //Debug.Log("Read Current Data : " + data_Frm);

                sendDataQuaternion.Clear();

                //Debug.Log("cur frm send : ");
                var byteArray = new byte[24 * 4];
                Buffer.BlockCopy(ld_bcadata, 0, byteArray, 0, byteArray.Length);
            }










        }


        caldata_write_frm();


    }

    float[] vec2BCA()
    {
        List<float> buf = new List<float>();

        if (cur_plpose_vec.Count>6)
        {
            Vector3 coordX = (cur_plpose_vec[3] - cur_plpose_vec[0]).normalized;
            Vector3 coordY = new Vector3(0,1,0);
            Vector3 coordZ = Vector3.Cross(coordX, coordY);


            Vector3 RUA = (cur_plpose_vec[1] - cur_plpose_vec[0]).normalized;
            Vector3 RLA = (cur_plpose_vec[2] - cur_plpose_vec[1]).normalized;
            Vector3 LUA = (cur_plpose_vec[4] - cur_plpose_vec[3]).normalized;
            Vector3 LLA = (cur_plpose_vec[5] - cur_plpose_vec[4]).normalized;


            buf.Add(Vector3.Angle(coordX, RUA)* Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordY, RUA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordZ, RUA) * Mathf.Deg2Rad);

            buf.Add(Vector3.Angle(coordX, RLA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordY, RLA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordZ, RLA) * Mathf.Deg2Rad);

            buf.Add(Vector3.Angle(coordX, LUA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordY, LUA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordZ, LUA) * Mathf.Deg2Rad);

            buf.Add(Vector3.Angle(coordX, LLA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordY, LLA) * Mathf.Deg2Rad);
            buf.Add(Vector3.Angle(coordZ, LLA) * Mathf.Deg2Rad);

            //buf.Add(coordX.x);
            //buf.Add(coordX.y);
            //buf.Add(coordX.z);

            //buf.Add(coordY.x);
            //buf.Add(coordY.y);
            //buf.Add(coordY.z);

            //buf.Add(coordZ.x);
            //buf.Add(coordZ.y);
            //buf.Add(coordZ.z);

            //buf.Add(RUA.x);
            //buf.Add(RUA.y);
            //buf.Add(RUA.z);

            //buf.Add(RLA.x);
            //buf.Add(RLA.y);
            //buf.Add(RLA.z);

            //buf.Add(LUA.x);
            //buf.Add(LUA.y);
            //buf.Add(LUA.z);

            //buf.Add(LLA.x);
            //buf.Add(LLA.y);
            //buf.Add(LLA.z);

            cur_plpose_vec.RemoveRange(0, 6);
        }




        return buf.ToArray();


    }

    void rt_connect()
    {

        if (stream.DataAvailable)
        {
            receivedBuffer = new byte[100];
            stream.Read(receivedBuffer, 0, receivedBuffer.Length); // stream에 있던 바이트배열 내려서 새로 선언한 바이트배열에 넣기
            string msg = Encoding.UTF8.GetString(receivedBuffer, 0, receivedBuffer.Length); // byte[] to string
            Debug.Log(msg);
            string temp = Regex.Replace(msg, @"\D", "");
            int num = int.Parse(temp);
            Debug.Log(num);

            if (num > 5)
            {
                var data = Encoding.UTF8.GetBytes("close");
                stream.Write(data, 0, data.Length);
            }


        }

    }

    void data_write()
    {
        var byteArray = new byte[ld_bcadata.Length * 4];
        Buffer.BlockCopy(ld_bcadata, 0, byteArray, 0, byteArray.Length);



        //var data = Encoding.UTF8.GetBytes("close");
        stream.Write(byteArray, 0, ld_bcadata.Length * 4);
    }
    void caldata_write_frm()
    {
        if (data_load_Available)
        {
            float[] cal_data = vec2BCA();

            Debug.Log("cur frm send : " + send_cnt);
            var byteArray = new byte[12 * 4];
            Buffer.BlockCopy(cal_data, 0, byteArray, 0, byteArray.Length);

            //var floatArray2 = new float[byteArray.Length / 4];
            //Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);
            //Debug.Log(floatArray2[0]);

            //var data = Encoding.UTF8.GetBytes("close");
            stream.Write(byteArray, 0, 12 * 4);
            send_cnt++;
        }




    }

    void data_write_frm()
    {
        if (send_cnt < data_Frm)
        {

            Debug.Log("cur frm send : " + send_cnt);
            var byteArray = new byte[12 * 4];
            Buffer.BlockCopy(ld_bcadata, send_cnt * 12*4, byteArray, 0, byteArray.Length);

            var floatArray2 = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);
            Debug.Log(floatArray2[0]);

            //var data = Encoding.UTF8.GetBytes("close");
            stream.Write(byteArray, 0, 12 * 4);
            send_cnt++;
        }
       



    }

    

    void CheckReceive()
    {
        if (socketReady) return;
        try
        {
            client = new TcpClient(serverIP, port);

            if (client.Connected)
            {
                stream = client.GetStream();
                Debug.Log("Connect Success");
                socketReady = true;
            }

        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }

    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    void CloseSocket()
    {
        if (!socketReady) return;

        //reader.Close();
        client.Close();
        socketReady = false;
    }



    float[] ReadBCA_Data(string file_path)
    {

        FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<float> load_BCA_buf = new List<float>();

        int joint_num = 4;
        int coord_num = 3;

      

        for (int line = 0; line < records.Length - 1; line++)
        {
            fields = records[line].Split('\t');

            int fields_cnt = 0;

            for (int joint_cnt = 0; joint_cnt < joint_num; joint_cnt++)
            {


                for (int coord_cnt = 0; coord_cnt < coord_num; coord_cnt++)
                {
                    load_BCA_buf.Add(float.Parse(fields[fields_cnt]));
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

  




            }


        }
        Debug.Log("BCA reading done");
        sr.Close();
        quatStream.Close();


        return load_BCA_buf.ToArray();
    }


}