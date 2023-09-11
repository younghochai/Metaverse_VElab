
//  Copyright (c) 2003-2023 Movella Technologies B.V. or subsidiaries worldwide.
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

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using MovellaDotPcSdk;
using Xsens;

namespace Movella
{
    class XdpcHandler : XsDotCallback
    {
        /// <returns>
        /// A pointer to the XsDotConnectionManager
        /// </returns>
        public XsDotConnectionManager Manager { get; private set; }

        private object _lockThis;
        /// <returns>
        /// True if an error was received through the onError callback
        /// </returns>
        public bool ErrorReceived { get; private set; }
        /// <returns>
        /// True if the export has finished
        /// </returns>
        public bool ExportDone { get; private set; }
        /// <returns>
        /// Whether update done was received through the onDeviceUpdateDone callback
        /// </returns>
        /// <remarks>
        /// Set to False before starting another update.
        /// </remarks>
        public bool UpdateDone { get; set; }
        /// <returns>
        /// True if the device indicated the recording has stopped
        /// </returns>
        /// <remarks>
        /// Set to False before starting another recording.
        /// </remarks>
        public bool RecordingStopped { get; set; }
        /// <returns>
        /// The number of packets received during data export
        /// </returns>
        public int PacketsReceived { get; private set; }
        /// <returns>
        /// A list containing an XsDotDevice pointer for each Movella DOT device connected via Bluetooth
        /// </returns>
        public List<XsDotDevice> ConnectedDots { get; private set; }
        /// <returns>
        /// A list containing an XsDotUsbDevice pointer for each Movella DOT device connected via USB
        /// </returns>
        public List<XsDotUsbDevice> ConnectedUsbDots { get; private set; }

        private bool Closing { get; set; }
        private int ProgressCurrent { get; set; }
        private int ProgressTotal { get; set; }
        private uint MaxNumberOfPacketsInBuffer { get; set; }
        private XsPortInfoArray DetectedDots { get; set; }
        private Dictionary<string, uint> NumberOfPacketsInBuffer { get; set; }
        private Dictionary<string, Queue<XsDataPacket>> PacketBuffer { get; set; }
        private Dictionary<string, int> ProgressBuffer { get; set; }

        public XdpcHandler(uint maxBufferSize = 5)
           : base()
        {
            ErrorReceived = false;
            ExportDone = false;
            UpdateDone = false;
            RecordingStopped = false;
            Closing = false;
            DetectedDots = new XsPortInfoArray();
            ConnectedDots = new List<XsDotDevice>();
            ConnectedUsbDots = new List<XsDotUsbDevice>();
            MaxNumberOfPacketsInBuffer = maxBufferSize;
            NumberOfPacketsInBuffer = new Dictionary<string, uint>();
            PacketBuffer = new Dictionary<string, Queue<XsDataPacket>>();
            ProgressBuffer = new Dictionary<string, int>();
            _lockThis = new object();
        }

        /// <summary>
        /// Initialize the PC SDK
        /// - Prints the used PC SDK version to show we connected to XDPC
        /// - Constructs the connection manager used for discovering and connecting to DOTs
        /// - Connects this class as callback handler to the XDPC
        /// </summary>
        /// <returns>false if there was a problem creating a connection manager.</returns>
        public bool initialize()
        {
            // Print SDK version
            XsVersion version = new XsVersion();
            movelladot_pc_sdk.xsdotsdkDllVersion(version);
            Console.WriteLine("Using Movella DOT SDK version: {0}", version.toXsString().toString());

            Console.WriteLine("Creating Movella DOT Connection Manager object...");
            Manager = new XsDotConnectionManager();
            if (Manager == null)
            {
                Console.WriteLine("Manager could not be constructed, exiting..");
                return false;
            }

            // Attach callback handler (this) to connection manager
            Manager.addXsDotCallbackHandler(this);
            return true;
        }

        /// <summary>
        /// Close connections to any Movella DOT devices and destructs the connection manager created in initialize
        /// </summary>
        public void cleanup()
        {
            Console.WriteLine("Closing ports...");
            Closing = true;
            Manager.close();

            Console.WriteLine("Successful exit.");
        }

        /// <summary>
        /// Scan if any Movella DOT devices can be detected via Bluetooth
        /// Enables device detection in the connection manager and uses the	onAdvertisementFound callback to detect active Movella DOT devices
        /// Disables device detection when done
        /// </summary>
        public void scanForDots()
        {
            // Start a scan and wait until we have found one or more Movella DOT Devices
            Console.WriteLine("Scanning for devices...");
            Manager.enableDeviceDetection();

            Console.WriteLine("Press any key or wait 20 seconds to stop scanning...");
            bool waitForConnections = true;
            uint connectedDOTCount = 0;
            long startTime = XsTimeStamp.nowMs();
            do
            {
                System.Threading.Thread.Sleep(100);

                uint nextCount = detectedDots().size();
                if (nextCount != connectedDOTCount)
                {
                    Console.WriteLine("Number of detected DOTs: {0}. Press any key to start.", nextCount);
                    connectedDOTCount = nextCount;
                }
                if (!Console.IsInputRedirected)
                {
                    if (Console.KeyAvailable)
                        waitForConnections = false;
                }
            }
            while (waitForConnections && !ErrorReceived && (XsTimeStamp.nowMs() - startTime <= 20000));

            Manager.disableDeviceDetection();
            Console.WriteLine("Stopped scanning for devices");
        }

        /// <summary>
        /// Connects to Movella DOTs found via either USB or Bluetooth connection
        /// Uses the isBluetooth function of the XsPortInfo to determine if the device was detected
        /// via Bluetooth or via USB.Then connects to the device accordingly
        /// When using Bluetooth, a retry has been built in, since wireless connection sometimes just fails the 1st time
        /// Connected devices can be retrieved using either connectedDots() or connectedUsbDots()
        /// </summary>
        /// <remarks>USB and Bluetooth devices should not be mixed in the same session!</remarks>
        public void connectDots()
        {
            for (uint i = 0; i < detectedDots().size(); i++)
            {
                XsPortInfo portInfo = detectedDots().at(i);
                if (portInfo.isBluetooth())
                {
                    Console.WriteLine("Opening DOT with address: {0}", portInfo.bluetoothAddress().toString());
                    if (!Manager.openPort(portInfo))
                    {
                        Console.WriteLine("Connection to Device {0} failed, retrying..", portInfo.bluetoothAddress().toString());
                        Console.WriteLine("Device {0} retry connected: ", portInfo.bluetoothAddress().toString());
                        if (!Manager.openPort(portInfo))
                        {
                            Console.WriteLine("Could not open DOT. Reason: {0}", Manager.lastResultText().toString());
                            continue;
                        }
                    }
                    XsDotDevice tempDevice = Manager.device(portInfo.deviceId());
                    if (tempDevice == null)
                        continue;

                    XsDotDevice device = new XsDotDevice(tempDevice);
                    ConnectedDots.Add(device);
                    Console.WriteLine("Found a device with Tag: {0} @ address: {1}", device.deviceTagName().toString(), device.bluetoothAddress().toString());
                }
                else
                {
                    Console.WriteLine("Opening DOT with ID: {0} @ port: {1}, baudrate: {2}",
                        DetectedDots.at(i).deviceId().toXsString().toString(),
                        DetectedDots.at(i).portName().toString(),
                        DetectedDots.at(i).baudrate());
                    if (!Manager.openPort(portInfo))
                    {
                        Console.WriteLine("Could not open DOT. Reason: {0}", Manager.lastResultText().toString());
                        continue;
                    }
                    XsDotUsbDevice tempDevice = Manager.usbDevice(portInfo.deviceId());
                    if (tempDevice == null)
                        continue;

                    XsDotUsbDevice device = new XsDotUsbDevice(tempDevice);
                    ConnectedUsbDots.Add(device);
                    Console.WriteLine("Device: {0}, with ID: {1} opened.", device.productCode().toString(), device.deviceId().toXsString().toString());
                }
            }
        }

        /// <summary>
        /// Scans for USB connected Movella DOT devices for data export
        /// </summary>
        public void detectUsbDevices()
        {
            Console.WriteLine("Scanning for USB devices...");
            DetectedDots = Manager.detectUsbDevices();
        }

        /// <returns>
        /// An XsPortInfoArray containing information on detected Movella DOT devices
        /// </returns>
        public XsPortInfoArray detectedDots()
        {
            lock (_lockThis)
            {
                return DetectedDots;
            }
        }

        /// <returns>
        /// True if a data packet is available for each of the connected Movella DOT devices
        /// </returns>
        public bool packetsAvailable()
        {
            for (uint i = 0; i < DetectedDots.size(); i++)
                if (!packetAvailable(DetectedDots.at(i).bluetoothAddress().toString()))
                    return false;
            return true;
        }

        /// <param name="bluetoothAddress">The bluetooth address of the Movella DOT to check for a ready data packet.</param>
        /// <returns>True if a data packet is available for the Movella DOT with the provided bluetoothAddress</returns>
        public bool packetAvailable(string bluetoothAddress)
        {
            lock (_lockThis)
                if (!NumberOfPacketsInBuffer.ContainsKey(bluetoothAddress))
                    return false;
            return NumberOfPacketsInBuffer[bluetoothAddress] > 0;
        }

        /// <param name="bluetoothAddress">The bluetooth address of the Movella DOT to get the next packet for</param>
        /// <returns>
        /// The next available data packet for the Movella DOT with the provided bluetoothAddress
        /// </returns>
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

        /// <summary>
        /// Initialize internal progress buffer for an Movella DOT device
        /// </summary>
        /// <param name="bluetoothAddress">The bluetooth address of the Movella DOT device</param>
        public void addDeviceToProgressBuffer(string bluetoothAddress)
        {
            ProgressBuffer[bluetoothAddress] = 0;
        }

        /// <summary>
        /// The current progress indication of the Movella DOT with the provided bluetoothAddress
        /// </summary>
        /// <param name="bluetoothAddress">bluetoothAddress The bluetooth address of the Movella DOT device</param>
        public int progress(string bluetoothAddress)
        {
            return ProgressBuffer[bluetoothAddress];
        }

        /// <summary>
        /// Helper function for printing file export info to the command line.
        /// </summary>
        private void outputDeviceProgress()
        {
            Console.Write("\rExporting... ");
            if (ExportDone)
                Console.WriteLine("done!");
            else if (ProgressTotal != 0xffff)
                Console.Write("{0:F1}%", 100.0 * ProgressCurrent / ProgressTotal);
            else
                Console.Write("{0}", ProgressCurrent);
        }

        /// <summary>
        /// Called when an Movella DOT device advertisement was received. Updates m_detectedDots.
        /// </summary>
        /// <param name="portInfo">The XsPortInfo of the discovered information</param>
        protected override void onAdvertisementFound(XsPortInfo portInfo)
        {
            lock (_lockThis)
            {
                Debug.Assert(portInfo != null);
                if (UserSettings.WhiteList.Length == 0 || Array.IndexOf(UserSettings.WhiteList, portInfo.bluetoothAddress().toString()) != -1)
                    DetectedDots.push_back(portInfo);
                else
                    Console.WriteLine("Ignoring {0}", portInfo.bluetoothAddress().toString());
            }
        }

        /// <summary>
        /// Called when a battery status update is available. Prints to screen.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="batteryLevel">The battery level in percentage</param>
        /// <param name="chargingStatus">The charging status of the battery. 0: Not charging, 1: charging</param>
        protected override void onBatteryUpdated(XsDotDevice device, int batteryLevel, int chargingStatus)
        {
            Console.WriteLine("{0} BatteryLevel: {1} Charging status: {2}", device.deviceTagName(), batteryLevel, chargingStatus);
        }

        /// <summary>
        /// Called when an internal error has occurred. Prints to screen.
        /// </summary>
        /// <param name="result">The XsResultValue related to this error</param>
        /// <param name="error">The error string with information on the problem that occurred</param>
        protected override void onError(XsResultValue result, XsString error)
        {
            Console.WriteLine(result.ToString());
            Console.WriteLine("Error received: {0}", error);
            ErrorReceived = true;
        }

        /// <summary>
        /// Called when new data has been received from a device
        /// </summary>
        /// <remarks>
        /// Adds the new packet to the device's packet buffer
        /// Monitors buffer size, removes oldest packets if the size gets too big
        /// </remarks>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="packet">The data packet that has been received (and processed)</param>
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

        /// <summary>
        /// Called when a long-duration operation has made some progress or has completed.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="current">The current progress</param>
        /// <param name="total">The total work to be done. When \a current equals \a total, the task is completed</param>
        /// <param name="remark">An identifier for the task. This may for example be a filename for file read operations</param>
        protected override void onProgressUpdated(XsDotDevice device, int current, int total, XsString remark)
        {
            string address = device.portName().toString();
            if (!ProgressBuffer.ContainsKey(address))
                ProgressBuffer[address] = current;
            if (current > ProgressBuffer[address])
            {
                ProgressBuffer[address] = current;
                Console.Write("\r");
                if (remark != null)
                    Console.Write("Update: {0} Total: {1} Remark: {2}", current, total, remark.toString());
                else
                    Console.Write("Update: {0} Total: {1}", current, total);
            }
        }

        /// <summary>
        /// Called when the firmware update process has completed. Prints to screen.
        /// </summary>
        /// <param name="portInfo">The XsPortInfo of the updated device</param>
        /// <param name="result">The XsDotFirmwareUpdateResult of the firmware update</param>
        protected override void onDeviceUpdateDone(XsPortInfo portInfo, XsDotFirmwareUpdateResult result)
        {
            Console.WriteLine("");
            Console.WriteLine("{0} Firmware Update done. Result: {1}", portInfo.bluetoothAddress().toString(), movelladot_pc_sdk.XsDotFirmwareUpdateResultToString(result));
            UpdateDone = true;
        }

        /// <summary>
        /// Called when a recording has stopped. Prints to screen.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        protected override void onRecordingStopped(XsDotDevice device)
        {
            Console.WriteLine("");
            Console.WriteLine("{0} Recording stopped", device.deviceTagName().toString());
            RecordingStopped = true;
        }

        /// <summary>
        /// Called when the device state has changed.
        /// </summary>
        /// <remarks>
        /// Used for removing/disconnecting the device when it indicates a power down.
        /// </remarks>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="newState">The new device state</param>
        /// <param name="oldState">The old device state</param>
        protected override void onDeviceStateChanged(XsDotDevice device, XsDeviceState newState, XsDeviceState oldState)
        {
            if (newState == XsDeviceState.XDS_Destructing && !Closing)
            {
                ConnectedDots.Remove(device);
                Console.WriteLine("{0} Device powered down", device.deviceTagName().toString());
            }
        }

        /// <summary>
        /// Called when the device's button has been clicked. Prints to screen.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="timestamp">The timestamp at which the button was clicked</param>
        protected override void onButtonClicked(XsDotDevice device, uint timestamp)
        {
            Console.WriteLine("{0} Button clicked at {1}", device.deviceTagName().toString(), timestamp);
        }

        /// <summary>
        /// Called when a long-duration operation has made some progress or has completed. Used for printing data export progress information.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="current">The current progress</param>
        /// <param name="total">The total work to be done. When current equals total, the task is completed</param>
        /// <param name="identifier">An identifier for the task. This may for example be a filename for file read operations</param>
        protected override void onProgressUpdated(XsDotUsbDevice device, int current, int total, XsString identifier)
        {
            ProgressCurrent = current;
            ProgressTotal = total;
            outputDeviceProgress();
        }

        /// <summary>
        /// Called when new data has been received from a device that is exporting a recording via USB.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="packet">The data packet that has been received</param>
        protected override void onRecordedDataAvailable(XsDotUsbDevice device, XsDataPacket packet)
        {
            PacketsReceived++;
        }

        /// <summary>
        /// Called when a device that is exporting a recording is finished with exporting via USB.
        /// </summary>
        /// <remarks>
        /// This callback will occur in any sitation that stops the export of the recording, such as
        /// the export being completed, the export being stopped by request or an internal failure.
        /// </remarks>
        /// <param name="device">The device that initiated the callback</param>
        protected override void onRecordedDataDone(XsDotUsbDevice device)
        {
            ExportDone = true;
            outputDeviceProgress();
        }

        /// <summary>
        /// Called when new data has been received from a device that is exporting a recording via BLE.
        /// </summary>
        /// <param name="device">The device that initiated the callback</param>
        /// <param name="packet">The data packet that has been received</param>
        protected override void onRecordedDataAvailable(XsDotDevice device, XsDataPacket packet)
        {
            PacketsReceived++;
        }

        /// <summary>
        /// Called when a device that is exporting a recording is finished with exporting via BLE.
        /// </summary>
        /// <remarks>
        /// This callback will occur in any sitation that stops the export of the recording, such as
        /// the export being completed, the export being stopped by request or an internal failure.
        /// </remarks>
        /// <param name="device">The device that initiated the callback</param>
        protected override void onRecordedDataDone(XsDotDevice device)
        {
            ExportDone = true;
            outputDeviceProgress();
        }
    }
}
