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
    // Start is called before the first frame update
    void Start()
    {
        ld_bcadata = ReadBCA_Data(load_path);
        Debug.Log("The BCA putted array");



        CheckReceive();
    }
    // git test
    // Update is called once per frame
    void Update()
    {

        if (socketReady)
        {


            if (stream.DataAvailable)
            {
                receivedBuffer = new byte[100];
                stream.Read(receivedBuffer, 0, receivedBuffer.Length); // stream에 있던 바이트배열 내려서 새로 선언한 바이트배열에 넣기
                string msg = Encoding.UTF8.GetString(receivedBuffer, 0, receivedBuffer.Length); // byte[] to string
                Debug.Log("recognition Result :" + msg);
            
            }




                if (Input.GetKeyDown(KeyCode.Q))
            {
                //reader.Close();
                client.Close();
                CloseSocket();

            }

            if (Input.GetKeyDown(KeyCode.A))
            {

                data_write();
            }
            

        }






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