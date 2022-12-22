

//backupup




//  Copyright (c) 2003-2022 Movella Technologies B.V. or subsidiaries worldwide.
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//  
//  1.	Redistributions of source code must retain the above copyright notice,
//  	this list of conditions and the following disclaimer.
//  
//  2.	Redistributions in binary form must reproduce the above copyright notice,
//  	this list of conditions and the following disclaimer in the documentation
//  	and/or other materials provided with the distribution.
//  
//  3.	Neither the names of the copyright holders nor the names of their contributors
//  	may be used to endorse or promote products derived from this software without
//  	specific prior written permission.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
//  THE COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.IO;
//using System.Diagnostics;
using System.Collections.Generic;
using XsensDotPcSdk;
using Xsens;

public class XsensDot : MonoBehaviour
{
    public class CallbackHandler : XsDotCallback
    {
        private XsPortInfoArray _detectedDots;
        private bool _errorReceived;

        private uint _maxNumberOfPacketsInBuffer;
        private Dictionary<string, uint> _numberOfPacketsInBuffer;
        private Dictionary<string, Queue<XsDataPacket>> _packetBuffer;
        private object _lockThis;

        public bool ErrorReceived
        {
            get { return _errorReceived; }
            set { _errorReceived = value; }
        }

        private XsPortInfoArray DetectedDots
        {
            get { return _detectedDots; }
            set { _detectedDots = value; }
        }
        private uint MaxNumberOfPacketsInBuffer
        {
            get { return _maxNumberOfPacketsInBuffer; }
            set { _maxNumberOfPacketsInBuffer = value; }
        }

        private Dictionary<string, uint> NumberOfPacketsInBuffer
        {
            get { return _numberOfPacketsInBuffer; }
            set { _numberOfPacketsInBuffer = value; }
        }

        private Dictionary<string, Queue<XsDataPacket>> PacketBuffer
        {
            get { return _packetBuffer; }
            set { _packetBuffer = value; }
        }

        public CallbackHandler(uint maxBufferSize = 20)
            : base()
        {
            _detectedDots = new XsPortInfoArray();
            MaxNumberOfPacketsInBuffer = maxBufferSize;
            NumberOfPacketsInBuffer = new Dictionary<string, uint>();
            PacketBuffer = new Dictionary<string, Queue<XsDataPacket>>();
            _lockThis = new object();
        }

        public XsPortInfoArray getDetectedDots()
        {
            lock (_lockThis)
            {
                return DetectedDots;
            }
        }

        protected override void onAdvertisementFound(XsPortInfo portInfo)
        {
            lock (_lockThis)
            {
                Debug.Assert(portInfo != null);
                if (UserSettings.WhiteList.Length == 0 || Array.IndexOf(UserSettings.WhiteList, portInfo.bluetoothAddress().toString()) != -1)
                    DetectedDots.push_back(portInfo);
                else
                    Debug.Log("Ignoring " + portInfo.bluetoothAddress().toString());
            }
        }

        protected override void onBatteryUpdated(XsDotDevice device, int batteryLevel, int chargingStatus)
        {
            Debug.Log(device.deviceTagName() + "{0} BatteryLevel: {1} " + batteryLevel + "Charging status: {2}" + chargingStatus);
        }

        protected override void onError(XsString error)
        {
            Debug.Log("Error received: " + error);
            ErrorReceived = true;
        }

        public bool packetsAvailable()
        {
            for (uint i = 0; i < DetectedDots.size(); i++)
                if (!packetAvailable(DetectedDots.at(i).bluetoothAddress().toString()))
                    return false;
            return true;
        }

        public bool Specific_packetsAvailable(string address)
        {
            if (!packetAvailable(address))
                return false;
            return true;
        }
        public bool search_packetsAvailable(List<string> addresslist)
        {


            foreach (string address in addresslist)
                if (!packetAvailable(address))
                    return false;

            return true;
        }

        public bool packetAvailable(string bluetoothAddress)
        {
            lock (_lockThis)
                if (!NumberOfPacketsInBuffer.ContainsKey(bluetoothAddress))
                    return false;
            return NumberOfPacketsInBuffer[bluetoothAddress] > 0;
        }

        public XsDataPacket getNextPacket(string bluetoothAddress)
        {
            if (!packetAvailable(bluetoothAddress))
                return new XsDataPacket();
            lock (_lockThis)
            {
                XsDataPacket oldestPacket = PacketBuffer[bluetoothAddress].Peek();
                PacketBuffer[bluetoothAddress].Dequeue();
                --NumberOfPacketsInBuffer[bluetoothAddress];
                return oldestPacket;
            }
        }

        protected override void onLiveDataAvailable(XsDotDevice device, XsDataPacket packet)
        {
            lock (_lockThis)
            {
                string bluetoothAddress = device.portInfo().bluetoothAddress().toString();
                if (NumberOfPacketsInBuffer.ContainsKey(bluetoothAddress))
                {
                    while (NumberOfPacketsInBuffer[bluetoothAddress] >= MaxNumberOfPacketsInBuffer)
                        getNextPacket(bluetoothAddress);
                }
                else
                    NumberOfPacketsInBuffer.Add(bluetoothAddress, 0);

                if (PacketBuffer.ContainsKey(bluetoothAddress))
                    PacketBuffer[bluetoothAddress].Enqueue(new XsDataPacket(packet));
                else
                {
                    PacketBuffer.Add(bluetoothAddress, new Queue<XsDataPacket>());
                    PacketBuffer[bluetoothAddress].Enqueue(new XsDataPacket(packet));
                }
                ++NumberOfPacketsInBuffer[bluetoothAddress];
                Debug.Assert(NumberOfPacketsInBuffer[bluetoothAddress] <= MaxNumberOfPacketsInBuffer);
            }
        }
    }

    List<XsDotDevice> deviceList = new List<XsDotDevice>();
    CallbackHandler callback = new CallbackHandler();
    XsVersion version = new XsVersion();
    XsDotConnectionManager manager = new XsDotConnectionManager();
    long startTime = XsTimeStamp.nowMs();
    public Quaternion[] sensors;
    bool sensing_State = true;
    string device_address = "empty";
    List<Quaternion> sensor_data;
    Quaternion init_quat = new Quaternion(0, 0, 0, 1);
    public Dictionary<string, Quaternion> sensing_data = new Dictionary<string, Quaternion>();
    public Dictionary<string, Quaternion> init_quat_data = new Dictionary<string, Quaternion>();
    bool first_setup = true;
    bool tracking_state = false;
    bool save_state = false;
    int file_count = 0;
    string[] Dot_indexlist = new string[] { "37:E9", "38:00", "A4", "05:EE", "42:3B", "A9" }; // F1 -> EE

    List<string> Dotaddresslist;
    List<List<Quaternion>> save_quat_list = new List<List<Quaternion>>();

    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();
    string[] DBpath = new string[] { "QuaternionDB/1shake hands.csv", "QuaternionDB/2raise arm.csv", "QuaternionDB/3down arm.csv", "QuaternionDB/4rot right.csv", "QuaternionDB/5rot comeback.csv" };
    List<string> lst_DBpath = new List<string>();


    Dictionary<int, string> address_joint_idx = new Dictionary<int, string>()
    {
      { 0, "D4:22:CD:00:05:EE" }, //0, "D4:22:CD:00:38:F1" 
      { 1, "D4:22:CD:00:38:A9" },
      { 2, "D4:22:CD:00:38:A4" },
      { 3, "D4:22:CD:00:37:E9" },
      { 4, "D4:22:CD:00:38:00" },
      { 5, "D4:22:CD:00:42:3B" }

    };

    void Start()
    {
        xsensdot_pc_sdk.xsdotsdkDllVersion(version);
        Debug.Log("Using Xsens DOT SDK version: " + version.toXsString().toString());

        Debug.Log("Creating Xsens DOT Connection Manager object...");

        if (manager == null)
        {
            Debug.Log("Manager could not be constructed, exiting..");
            return;
        }

        // Create and attach callback handler to connection manager
        manager.addXsDotCallbackHandler(callback);
      

        //XsensDotSetup();
        // Start a scan and wait until we have found one or more Xsens DOT Devices
        Debug.Log("Scanning for devices...");
                

    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.O))
        {
            // first_sensing = false;
            Debug.Log("Press any key or wait 20 seconds to stop scanning...");
            Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");

        }
        else if (Input.GetKeyDown(KeyCode.I))
        {

            manager.enableDeviceDetection();
            // first_sensing = false;
            Debug.Log("Press any key or wait 20 seconds to stop scanning...");
            Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");

        }
        else if (Input.GetKeyDown(KeyCode.Semicolon))
        {

            manager.disableDeviceDetection();
            // first_sensing = false;
            Debug.Log("disableDeviceDetection");


        }
        else if (Input.GetKeyDown(KeyCode.Quote))
        {

            manager.reset();
            manager.addXsDotCallbackHandler(callback);
            Debug.Log("reset");


        }

        else if (Input.GetKeyDown(KeyCode.P))
        {

            manager.disableDeviceDetection();
            //Debug.Log("Stopped scanning for devices");
            if (first_setup)
            {
                first_setup = false;
                XsensDotSetup();

            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            first_setup = true;
            tracking_state = false;
            disconnect_DOT();
            //XsensDotSensing();
            //sensing_State = !sensing_State;
        }

        else if (Input.GetKeyDown(KeyCode.T))
        {
            save_init_quat();

        }

        else if (Input.GetKeyDown(KeyCode.Comma))
        {
            save_state = true;

        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            save_state = false;

            write_csv_file();
            reset_init_quat();
            file_count++;
        }
        else if (Input.GetKeyDown(KeyCode.Slash))
        {
            // data_read_csv();

            ReadQuaternion("QuaternionDB/DL_test.csv");
        }


        if (tracking_state)
            XsensDottracking();

        if (save_state)
            saving_quat();

    }

    void data_read_csv()
    {
        string[] arm = new string[] { "RightArm", "LeftArm" };
        string prefix = "QuaternionDB/";
        int data_num = 10;

        foreach (string each_arm in arm)
        {
            for (int data_cnt = 0; data_cnt < data_num; data_cnt++)
            {


                lst_DBpath.Add(prefix + each_arm + "/" + (data_cnt + 1).ToString() + ".csv");

            }



        }


        foreach (string file_path in DBpath)
        {


            ReadQuaternion(file_path);

        }
    }




    void ReadQuaternion(string file_path)
    {

        FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Quaternion>> load_quat_buf = new List<List<Quaternion>>();

        for (int i = 0; i < address_joint_idx.Count(); i++)
        {
            load_quat_buf.Add(new List<Quaternion>());

        }


        float[] data_val = new float[4];

        for (int line = 1; line < records.Length - 1; line++)
        {
            fields = records[line].Split(',');

            int fields_cnt = 0;

            for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
            {


                for (int val_idx = 0; val_idx < 4; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    //Debug.Log("loaded " + fields[fields_cnt]);
                    fields_cnt++;
                }

                //Debug.Log(new Quaternion(data_val[0], data_val[1], data_val[2], data_val[3]));
                load_quat_buf[device_idx].Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));




            }


        }
        Debug.Log("Quaternion reading done");
        sr.Close();
        quatStream.Close();


        load_quat_list.Add(load_quat_buf);

        return;
    }




    void save_init_quat()
    {

        init_quat_data = new Dictionary<string, Quaternion>(sensing_data);




        for (int i = 0; i < address_joint_idx.Count(); i++)
        {
            save_quat_list.Add(new List<Quaternion>());
            save_quat_list[i].Add(init_quat_data[address_joint_idx[i]]);
        }


    }

    void reset_init_quat()
    {


        save_quat_list.Clear();

        for (int i = 0; i < address_joint_idx.Count(); i++)
        {
            save_quat_list.Add(new List<Quaternion>());
            save_quat_list[i].Add(init_quat_data[address_joint_idx[i]]);
        }


    }


    void saving_quat()
    {

        for (int i = 0; i < address_joint_idx.Count(); i++)
        {

            save_quat_list[i].Add(sensing_data[address_joint_idx[i]]);


        }


    }

    void write_csv_file()
    {

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@file_count.ToString() + "test1.csv"))

        {


            var builder = new StringBuilder();


            file.WriteLine(address_joint_idx[0] + ", x, y, z," + address_joint_idx[1] + ", x, y, z," + address_joint_idx[2] + ", x, y, z," + address_joint_idx[3] + ", x, y, z," + address_joint_idx[4] + ", x, y, z," + address_joint_idx[5] + ", x, y, z");

            for (int frame = 0; frame < save_quat_list[0].Count(); frame++)
            {

                Debug.Log("save test frm cnt : "+ save_quat_list[0].Count());

                for (int joint = 0; joint < address_joint_idx.Count(); joint++)
                {
                    
                    builder.Append(save_quat_list[joint][frame].w.ToString() + ',');
                    builder.Append(save_quat_list[joint][frame].x.ToString() + ',');
                    builder.Append(save_quat_list[joint][frame].y.ToString() + ',');
                    if (joint == address_joint_idx.Count() - 1)
                        builder.Append(save_quat_list[joint][frame].z.ToString());
                    else
                        builder.Append(save_quat_list[joint][frame].z.ToString() + ',');
                }



                file.WriteLine(builder.ToString());

                builder.Clear();
            }

        }





    }



    void XsensDotSetup()
    {

        //Debug.Log("Scanning for devices...");
        ////if(first_sensing)
        //manager.enableDeviceDetection();

        //first_sensing = false;

        manager.disableDeviceDetection();
        Debug.Log("Stopped scanning for devices");

        if (callback.getDetectedDots().empty())
            throw new Exception("No Xsens DOT device(s) found. Aborting.");


        for (uint i = 0; i < callback.getDetectedDots().size(); i++)
        {
            XsPortInfo portInfo = callback.getDetectedDots().at(i);
            if (Dot_indexlist.Any(data => portInfo.bluetoothAddress().toString().Contains(data)))
            {
                device_address = portInfo.bluetoothAddress().toString();


                Debug.Log("Opening DOT with address: " + portInfo.bluetoothAddress().toString());
                if (!manager.openPort(portInfo))
                {
                    Debug.Log("Connection to Device " + portInfo.bluetoothAddress().toString() + " failed, retrying..");
                    Debug.Log("Device " + portInfo.bluetoothAddress().toString() + " retry connected: ");
                    if (!manager.openPort(portInfo))
                    {
                        Debug.Log("Could not open DOT. Reason: " + manager.lastResultText().toString());
                        continue;
                    }
                }


                XsDotDevice tempDevice = manager.device(portInfo.deviceId());
                if (tempDevice == null)
                    continue;

                XsDotDevice device = new XsDotDevice(tempDevice);
                deviceList.Add(device);
                sensing_data.Add(device.portInfo().bluetoothAddress().toString(), new Quaternion(0, 0, 0, 1));


                Debug.Log("Found a device with Tag: " + device.deviceTagName().toString() + "@ address: " + device.portInfo().bluetoothAddress().toString());

                //XsFilterProfileArray filterProfiles = device.getAvailableFilterProfiles();

                //Debug.Log("Available filter profiles: ");
                //for (uint j = 0; j < filterProfiles.size(); j++)
                //    Debug.Log(filterProfiles.at(j).label());
                                             

                Debug.Log("Putting device into measurement mode. ");
                if (!device.startMeasurement(XsPayloadMode.ExtendedEuler))
                {
                    Debug.Log("Could not put device into measurement mode. Reason: " + manager.lastResultText().toString());
                    continue;
                }


            }
        }
        Dotaddresslist = new List<string>(sensing_data.Keys);
        Debug.Log("Starting measurement...");

        Debug.Log("connected device num : " + deviceList.Count);
        tracking_state = true;
    }

    void XsensDottracking()
    {
        // Print SDK version



        //foreach (XsDotDevice device in deviceList)
        //    Console.Write("{0,-42}", device.portInfo().bluetoothAddress().toString());
        //Console.Write("\n");
        // Debug.Log(Dotaddresslist[0] + Dotaddresslist[3]);
        bool orientationResetDone = false;

        //Console.Write("working packet");
        if (callback.search_packetsAvailable(Dotaddresslist))
        {
            // Debug.Log("init packet");

            foreach (XsDotDevice device in deviceList)
            {
                // Retrieve a packet
                XsDataPacket packet = callback.getNextPacket(device.portInfo().bluetoothAddress().toString());
                //Debug.Log("working packet");
                if (packet.containsOrientation())
                {

                    XsEuler euler = packet.orientationEuler();
                    XsQuaternion quaternion = packet.orientationQuaternion();

                    // Quaternion buf= new Quaternion((float)quaternion.x(), (float)quaternion.y(), (float)quaternion.z(), (float)quaternion.w());
                    sensing_data[device.portInfo().bluetoothAddress().toString()] = new Quaternion((float)quaternion.x(), (float)quaternion.y(), (float)quaternion.z(), (float)quaternion.w());

                    //sensor_data.Add(buf);
                    //Debug.Log("Roll: "+ euler.roll() + " Pitch :" + euler.pitch() + "Yaw: "+ euler.yaw());
                    //Debug.Log("W: " + quaternion.w() + " X: " + quaternion.x() + " Y: " + quaternion.y() + " Z: " + quaternion.z());
                    //Console.Write("W:{0,7:f2}, X:{1,7:f2}, Y:{2,7:f2} Z:{2,7:f2}| ", quaternion.w(), quaternion.x(), quaternion.y(), quaternion.z());
                }

                packet.Dispose();
            }
            //if (!orientationResetDone && (XsTimeStamp.nowMs() - startTime) > 5000)
            //{
            //    foreach (XsDotDevice device in deviceList)
            //    {
            //        Console.Write("\nResetting heading for device {0}: ", device.portInfo().bluetoothAddress().toString());
            //        if (device.resetOrientation(XsResetMethod.XRM_Heading))
            //            Console.Write("OK");
            //        else
            //            Console.Write("NOK: {0}", device.lastResultText().toString());
            //    }
            //    Debug.Log("");
            //    orientationResetDone = true;
            //}
        }



    }


    void disconnect_DOT()
    {


        Debug.Log("Stopping measurement...");
        foreach (XsDotDevice device in deviceList)
        {
            if (!device.stopMeasurement())
                Console.Write("Failed to stop measurement.");
            //if (!device.disableLogging())
            //    Console.Write("Failed to disable logging.");
        }

        Debug.Log("Closing ports...");
        manager.close();

        Debug.Log("Successful exit.");
    }

}

//}
///
/// 
/// 
/// 
////origin back up
////  Copyright (c) 2003-2022 Movella Technologies B.V. or subsidiaries worldwide.
////  All rights reserved.
////  
////  Redistribution and use in source and binary forms, with or without modification,
////  are permitted provided that the following conditions are met:
////  
////  1.	Redistributions of source code must retain the above copyright notice,
////  	this list of conditions and the following disclaimer.
////  
////  2.	Redistributions in binary form must reproduce the above copyright notice,
////  	this list of conditions and the following disclaimer in the documentation
////  	and/or other materials provided with the distribution.
////  
////  3.	Neither the names of the copyright holders nor the names of their contributors
////  	may be used to endorse or promote products derived from this software without
////  	specific prior written permission.
////  
////  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
////  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
////  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
////  THE COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
////  SPECIAL, EXEMPLARY OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
////  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
////  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY OR
////  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
////  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////  
//using UnityEngine;
//using System;
//using System.Linq;
////using System.Diagnostics;
//using System.Collections.Generic;
//using XsensDotPcSdk;
//using Xsens;

//public class XsensDot : MonoBehaviour
//{
//    public class CallbackHandler : XsDotCallback
//    {
//        private XsPortInfoArray _detectedDots;
//        private bool _errorReceived;

//        private uint _maxNumberOfPacketsInBuffer;
//        private Dictionary<string, uint> _numberOfPacketsInBuffer;
//        private Dictionary<string, Queue<XsDataPacket>> _packetBuffer;
//        private object _lockThis;

//        public bool ErrorReceived
//        {
//            get { return _errorReceived; }
//            set { _errorReceived = value; }
//        }

//        private XsPortInfoArray DetectedDots
//        {
//            get { return _detectedDots; }
//            set { _detectedDots = value; }
//        }
//        private uint MaxNumberOfPacketsInBuffer
//        {
//            get { return _maxNumberOfPacketsInBuffer; }
//            set { _maxNumberOfPacketsInBuffer = value; }
//        }

//        private Dictionary<string, uint> NumberOfPacketsInBuffer
//        {
//            get { return _numberOfPacketsInBuffer; }
//            set { _numberOfPacketsInBuffer = value; }
//        }

//        private Dictionary<string, Queue<XsDataPacket>> PacketBuffer
//        {
//            get { return _packetBuffer; }
//            set { _packetBuffer = value; }
//        }

//        public CallbackHandler(uint maxBufferSize = 20)
//            : base()
//        {
//            _detectedDots = new XsPortInfoArray();
//            MaxNumberOfPacketsInBuffer = maxBufferSize;
//            NumberOfPacketsInBuffer = new Dictionary<string, uint>();
//            PacketBuffer = new Dictionary<string, Queue<XsDataPacket>>();
//            _lockThis = new object();
//        }

//        public XsPortInfoArray getDetectedDots()
//        {
//            lock (_lockThis)
//            {
//                return DetectedDots;
//            }
//        }

//        protected override void onAdvertisementFound(XsPortInfo portInfo)
//        {
//            lock (_lockThis)
//            {
//                Debug.Assert(portInfo != null);
//                if (UserSettings.WhiteList.Length == 0 || Array.IndexOf(UserSettings.WhiteList, portInfo.bluetoothAddress().toString()) != -1)
//                    DetectedDots.push_back(portInfo);
//                else
//                    Debug.Log("Ignoring " + portInfo.bluetoothAddress().toString());
//            }
//        }

//        protected override void onBatteryUpdated(XsDotDevice device, int batteryLevel, int chargingStatus)
//        {
//            Debug.Log(device.deviceTagName() + "{0} BatteryLevel: {1} " + batteryLevel + "Charging status: {2}" + chargingStatus);
//        }

//        protected override void onError(XsString error)
//        {
//            Debug.Log("Error received: " + error);
//            ErrorReceived = true;
//        }

//        public bool packetsAvailable()
//        {
//            for (uint i = 0; i < DetectedDots.size(); i++)
//                if (!packetAvailable(DetectedDots.at(i).bluetoothAddress().toString()))
//                    return false;
//            return true;
//        }

//        public bool Specific_packetsAvailable(string address)
//        {
//            if (!packetAvailable(address))
//                return false;
//            return true;
//        }
//        public bool search_packetsAvailable(List<string> addresslist)
//        {
//            foreach (string address in addresslist)
//                if (!packetAvailable(address))
//                    return false;


//            return true;
//        }

//        public bool packetAvailable(string bluetoothAddress)
//        {
//            lock (_lockThis)
//                if (!NumberOfPacketsInBuffer.ContainsKey(bluetoothAddress))
//                    return false;
//            return NumberOfPacketsInBuffer[bluetoothAddress] > 0;
//        }

//        public XsDataPacket getNextPacket(string bluetoothAddress)
//        {
//            if (!packetAvailable(bluetoothAddress))
//                return new XsDataPacket();
//            lock (_lockThis)
//            {
//                XsDataPacket oldestPacket = PacketBuffer[bluetoothAddress].Peek();
//                PacketBuffer[bluetoothAddress].Dequeue();
//                --NumberOfPacketsInBuffer[bluetoothAddress];
//                return oldestPacket;
//            }
//        }

//        protected override void onLiveDataAvailable(XsDotDevice device, XsDataPacket packet)
//        {
//            lock (_lockThis)
//            {
//                string bluetoothAddress = device.portInfo().bluetoothAddress().toString();
//                if (NumberOfPacketsInBuffer.ContainsKey(bluetoothAddress))
//                {
//                    while (NumberOfPacketsInBuffer[bluetoothAddress] >= MaxNumberOfPacketsInBuffer)
//                        getNextPacket(bluetoothAddress);
//                }
//                else
//                    NumberOfPacketsInBuffer.Add(bluetoothAddress, 0);

//                if (PacketBuffer.ContainsKey(bluetoothAddress))
//                    PacketBuffer[bluetoothAddress].Enqueue(new XsDataPacket(packet));
//                else
//                {
//                    PacketBuffer.Add(bluetoothAddress, new Queue<XsDataPacket>());
//                    PacketBuffer[bluetoothAddress].Enqueue(new XsDataPacket(packet));
//                }
//                ++NumberOfPacketsInBuffer[bluetoothAddress];
//                Debug.Assert(NumberOfPacketsInBuffer[bluetoothAddress] <= MaxNumberOfPacketsInBuffer);
//            }
//        }
//    }

//    List<XsDotDevice> deviceList = new List<XsDotDevice>();
//    CallbackHandler callback = new CallbackHandler();
//    XsVersion version = new XsVersion();
//    XsDotConnectionManager manager = new XsDotConnectionManager();
//    long startTime = XsTimeStamp.nowMs();
//    public Quaternion[] sensors;
//    bool sensing_State = true;
//    string device_address = "empty";
//    List<Quaternion> sensor_data;
//    Quaternion init_quat = new Quaternion(0, 0, 0, 1);
//    public Dictionary<string, Quaternion> sensing_data = new Dictionary<string, Quaternion>();
//    bool first_setup = true;
//    bool tracking_state = false;
//    string[] Dot_indexlist = new string[] { "37:E9", "38:00", "A4", "F1", "3B", "A9" };
//    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();
//    List<string> Dotaddresslist;

//    void Start()
//    {
//        xsensdot_pc_sdk.xsdotsdkDllVersion(version);
//        Debug.Log("Using Xsens DOT SDK version: " + version.toXsString().toString());

//        Debug.Log("Creating Xsens DOT Connection Manager object...");

//        if (manager == null)
//        {
//            Debug.Log("Manager could not be constructed, exiting..");
//            return;
//        }

//        // Create and attach callback handler to connection manager

//        manager.addXsDotCallbackHandler(callback);
//        //XsPortInfo portInfo =

//        //XsensDotSetup();
//        // Start a scan and wait until we have found one or more Xsens DOT Devices

//        Debug.Log("Scanning for devices...");
//        // manager.enableDeviceDetection();
//        //if (first_sensing)
//        //manager.enableDeviceDetection();
//        //do
//        //{
//        //    Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");
//        //}
//        //while (callback.getDetectedDots().size() <= 15);

//    }

//    void Update()
//    {


//        if (Input.GetKeyDown(KeyCode.O))
//        {


//            // first_sensing = false;
//            Debug.Log("Press any key or wait 20 seconds to stop scanning...");
//            Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");

//        }
//        else if (Input.GetKeyDown(KeyCode.I))
//        {

//            manager.enableDeviceDetection();
//            // first_sensing = false;
//            Debug.Log("Press any key or wait 20 seconds to stop scanning...");
//            Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");

//        }
//        else if (Input.GetKeyDown(KeyCode.Semicolon))
//        {

//            manager.disableDeviceDetection();
//            // first_sensing = false;
//            Debug.Log("disableDeviceDetection");


//        }
//        else if (Input.GetKeyDown(KeyCode.Quote))
//        {

//            manager.reset();
//            manager.addXsDotCallbackHandler(callback);
//            Debug.Log("reset");


//        }

//        else if (Input.GetKeyDown(KeyCode.P))
//        {


//            manager.disableDeviceDetection();
//            //Debug.Log("Stopped scanning for devices");
//            if (first_setup)
//            {
//                first_setup = false;
//                XsensDotSetup();

//            }
//        }

//        else if (Input.GetKeyDown(KeyCode.LeftBracket))
//        {
//            first_setup = true;
//            tracking_state = false;
//            disconnect_DOT();
//            //XsensDotSensing();
//            //sensing_State = !sensing_State;
//        }

//        if (tracking_state)
//            XsensDottracking();



//    }




//    void XsensDotSetup()
//    {

//        //Debug.Log("Scanning for devices...");
//        ////if(first_sensing)
//        //manager.enableDeviceDetection();

//        //first_sensing = false;
//        //Debug.Log("Press any key or wait 20 seconds to stop scanning...");



//        //Debug.Log("Number of detected DOTs: " + callback.getDetectedDots().size() + ". Press any key to start.");



//        manager.disableDeviceDetection();
//        Debug.Log("Stopped scanning for devices");

//        if (callback.getDetectedDots().empty())
//            throw new Exception("No Xsens DOT device(s) found. Aborting.");


//        for (uint i = 0; i < callback.getDetectedDots().size(); i++)
//        {
//            XsPortInfo portInfo = callback.getDetectedDots().at(i);
//            if (Dot_indexlist.Any(data => portInfo.bluetoothAddress().toString().Contains(data)))
//            {
//                device_address = portInfo.bluetoothAddress().toString();


//                Debug.Log("Opening DOT with address: " + portInfo.bluetoothAddress().toString());
//                if (!manager.openPort(portInfo))
//                {
//                    Debug.Log("Connection to Device " + portInfo.bluetoothAddress().toString() + " failed, retrying..");
//                    Debug.Log("Device " + portInfo.bluetoothAddress().toString() + " retry connected: ");
//                    if (!manager.openPort(portInfo))
//                    {
//                        Debug.Log("Could not open DOT. Reason: " + manager.lastResultText().toString());
//                        continue;
//                    }
//                }


//                XsDotDevice tempDevice = manager.device(portInfo.deviceId());
//                if (tempDevice == null)
//                    continue;

//                XsDotDevice device = new XsDotDevice(tempDevice);
//                deviceList.Add(device);
//                sensing_data.Add(device.portInfo().bluetoothAddress().toString(), new Quaternion(0, 0, 0, 1));


//                Debug.Log("Found a device with Tag: " + device.deviceTagName().toString() + "@ address: " + device.portInfo().bluetoothAddress().toString());

//                //XsFilterProfileArray filterProfiles = device.getAvailableFilterProfiles();

//                //Debug.Log("Available filter profiles: ");
//                //for (uint j = 0; j < filterProfiles.size(); j++)
//                //    Debug.Log(filterProfiles.at(j).label());

//                //Debug.Log("Current filter profile: " + device.onboardFilterProfile().label());
//                //if (device.setOnboardFilterProfile(new XsString("General")))
//                //    Debug.Log("Successfully set filter profile to General");
//                //else
//                //    Debug.Log("Failed to set filter profile!");

//                //Debug.Log("Setting quaternion CSV output");
//                //device.setLogOptions(XsLogOptions.Euler);

//                //XsString logFileName = new XsString("logfile_" + portInfo.bluetoothAddress().toString().Replace(':', '-') + ".csv");
//                //Debug.Log("Enable logging to: " + logFileName.toString());
//                //if (!device.enableLogging(logFileName))
//                //{
//                //    Debug.Log("Failed to enable logging. Reason: " + manager.lastResultText().toString());
//                //    continue;
//                //}

//                Debug.Log("Putting device into measurement mode. ");
//                if (!device.startMeasurement(XsPayloadMode.ExtendedEuler))
//                {
//                    Debug.Log("Could not put device into measurement mode. Reason: " + manager.lastResultText().toString());
//                    continue;
//                }


//            }
//        }
//        Dotaddresslist = new List<string>(sensing_data.Keys);
//        Debug.Log("Starting measurement...");

//        Debug.Log("connected device num : " + deviceList.Count);
//        tracking_state = true;
//    }

//    void XsensDottracking()
//    {
//        // Print SDK version



//        //foreach (XsDotDevice device in deviceList)
//        //    Console.Write("{0,-42}", device.portInfo().bluetoothAddress().toString());
//        //Console.Write("\n");

//        bool orientationResetDone = false;
//        //Debug.Log("init packeEEEt");
//        //Console.Write("working packet");
//        if (callback.search_packetsAvailable(Dotaddresslist))
//        {
//            //Debug.Log("init packet");

//            foreach (XsDotDevice device in deviceList)
//            {
//                // Retrieve a packet
//                XsDataPacket packet = callback.getNextPacket(device.portInfo().bluetoothAddress().toString());
//                //Debug.Log("working packet");
//                if (packet.containsOrientation())
//                {

//                    XsEuler euler = packet.orientationEuler();
//                    XsQuaternion quaternion = packet.orientationQuaternion();

//                    // Quaternion buf= new Quaternion((float)quaternion.x(), (float)quaternion.y(), (float)quaternion.z(), (float)quaternion.w());
//                    sensing_data[device.portInfo().bluetoothAddress().toString()] = new Quaternion((float)quaternion.x(), (float)quaternion.y(), (float)quaternion.z(), (float)quaternion.w());

//                    //sensor_data.Add(buf);
//                    //Debug.Log("Roll: "+ euler.roll() + " Pitch :" + euler.pitch() + "Yaw: "+ euler.yaw());
//                    //Debug.Log("W: "+ quaternion.w() + " X: " + quaternion.x() + " Y: "+ quaternion.y() + " Z: "+ quaternion.z());
//                    //Console.Write("W:{0,7:f2}, X:{1,7:f2}, Y:{2,7:f2} Z:{2,7:f2}| ", quaternion.w(), quaternion.x(), quaternion.y(), quaternion.z());
//                }

//                packet.Dispose();
//            }
//            //if (!orientationResetDone && (XsTimeStamp.nowMs() - startTime) > 5000)
//            //{
//            //    foreach (XsDotDevice device in deviceList)
//            //    {
//            //        Console.Write("\nResetting heading for device {0}: ", device.portInfo().bluetoothAddress().toString());
//            //        if (device.resetOrientation(XsResetMethod.XRM_Heading))
//            //            Console.Write("OK");
//            //        else
//            //            Console.Write("NOK: {0}", device.lastResultText().toString());
//            //    }
//            //    Debug.Log("");
//            //    orientationResetDone = true;
//            //}
//        }



//    }


//    void disconnect_DOT()
//    {


//        Debug.Log("Stopping measurement...");
//        foreach (XsDotDevice device in deviceList)
//        {
//            if (!device.stopMeasurement())
//                Console.Write("Failed to stop measurement.");
//            //if (!device.disableLogging())
//            //    Console.Write("Failed to disable logging.");
//        }

//        Debug.Log("Closing ports...");
//        manager.close();

//        Debug.Log("Successful exit.");
//    }


//}
