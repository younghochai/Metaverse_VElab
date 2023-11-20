using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Timers;
using Xsens;
using XDA;

public class XsensManage : MonoBehaviour
{
    enum States
    {
        DETECTING,
        CONNECTING,
        CONNECTED,
        ENABLED,
        OPERATIONAL,
        AWAIT_MEASUREMENT_START,
        MEASURING,
        AWAIT_RECORDING_START,
        RECORDING,
        FLUSHING
    };
    [System.Serializable]
    public class XsensDevice
    {
        public string deviceNum;
        public double EulerX, EulerY, EulerZ;
        public double QuatW, QuatX, QuatY, QuatZ;
        public double AccX, AccY, AccZ;
        public bool turnOn = false;

        public XsensDevice(string deviceid)
        {
            this.deviceNum = deviceid;
        }
    }
    public bool print_state;
    public bool _is_alignment = false;

    States _state;
    public XsensManage manager;

    XsDevice _MyWirelessMasterDevice;
    XsResetMethod XRM_Alignment;
    private System.Timers.Timer _portScanTimer;

    // Connected MTw's
    internal List<XsDevice> _mtws = new List<XsDevice>();

    private MyXda _myxda;
    // Callback handler to track connectivity and data
    private MyWirelessMasterCallback m_myWirelessMasterCallback;
    private Dictionary<XsDevice, MyMtwCallback> _measuringMtws;
    private Dictionary<uint, ConnectedMTwData> _connectedMtwData;

    private List<string> dockedMtwList = new List<string>();
    private List<string> connectedMtwList = new List<string>();

    Thread thread;
    [SerializeField]


    public List<XsensDevice> sensors = new List<XsensDevice>();


    // Use this for initialization
    void Start()
    {
        sensors.Add(new XsensDevice("00B42780")); // 1,
        sensors.Add(new XsensDevice("00B4278B")); // 2,
        sensors.Add(new XsensDevice("00B42790")); // 3, 
        sensors.Add(new XsensDevice("00B42799")); // 4, 
        sensors.Add(new XsensDevice("00B4279F")); // 5,
        sensors.Add(new XsensDevice("00B427A3")); // 6,
        sensors.Add(new XsensDevice("00B438AE")); // 7, 
        sensors.Add(new XsensDevice("00B438C7")); // 8,
        sensors.Add(new XsensDevice("00B4391F")); // 9,
        sensors.Add(new XsensDevice("00B43926")); // 0,   

        Debug.Log("Void Start part done!");
        //TestFuncion();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            //Debug.Log(xdatest._DetectedDevices[1].deviceId().toXsString().ToString());
            TestFuncion();
            //EnableStart();

        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            //Debug.Log(xdatest._DetectedDevices[1].deviceId().toXsString().ToString());
            //TestFuncion();
            EnableStart();

        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            MeasureStart();
        }

        else if (Input.GetKeyDown(KeyCode.X))
        {
            ClosedSimulate();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (print_state == false)
            {

                print_state = true;
            }
            else if (print_state == true)
            {
                Debug.Log("중단합니다.");
                print_state = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.A)) //Alignment 
        {
            Debug.Log("Alignment Start?");
            _is_alignment = true;
        }
        PrintSensorQuat();
    }

    void TestFuncion()
    {
        _measuringMtws = new Dictionary<XsDevice, MyMtwCallback>();
        _connectedMtwData = new Dictionary<uint, ConnectedMTwData>();

        // create xda instance and set up callback handling
        _myxda = new MyXda();
        _myxda.WirelessMasterDetected += new EventHandler<PortInfoArg>(_myxda_WirelessMasterDetected);
        _myxda.DockedMtwDetected += new EventHandler<PortInfoArg>(_myxda_DockedMtwDetected);
        _myxda.MtwUndocked += new EventHandler<PortInfoArg>(_myxda_MtwUndocked);
        _myxda.OpenPortSuccessful += new EventHandler<PortInfoArg>(_myxda_OpenPortSuccessful);
        _myxda.OpenPortFailed += new EventHandler<PortInfoArg>(_myxda_OpenPortFailed);

        m_myWirelessMasterCallback = new MyWirelessMasterCallback();
        m_myWirelessMasterCallback.MtwWireless += new EventHandler<DeviceIdArg>(_callbackHandler_MtwWireless);
        m_myWirelessMasterCallback.MtwDisconnected += new EventHandler<DeviceIdArg>(_callbackHandler_MtwDisconnected);
        m_myWirelessMasterCallback.MeasurementStarted += new EventHandler<DeviceIdArg>(_callbackHandler_MeasurementStarted);
        m_myWirelessMasterCallback.MeasurementStopped += new EventHandler<DeviceIdArg>(_callbackHandler_MeasurementStopped);
        m_myWirelessMasterCallback.DeviceError += new EventHandler<DeviceErrorArgs>(_callbackHandler_DeviceError);
        m_myWirelessMasterCallback.WaitingForRecordingStart += new EventHandler<DeviceIdArg>(_callbackHandler_WaitingForRecordingStart);
        m_myWirelessMasterCallback.RecordingStarted += new EventHandler<DeviceIdArg>(_callbackHandler_RecordingStarted);
        //m_myWirelessMasterCallback.ProgressUpdate += new EventHandler<ProgressUpdateArgs>(_callbackHandler_ProgressUpdate);

        thread = new Thread(_myxda.scanPorts);
        thread.IsBackground = true;
        thread.Start();

        //_portScanTimer = new System.Timers.Timer();
        //_portScanTimer.Interval = 1000;
        //_portScanTimer.Elapsed += scanPorts;

        _state = States.DETECTING;
        Debug.Log("Detecting...");

        //_portScanTimer.Enabled = true;

    }

    private void scanPorts(object sender, EventArgs e)
    {
        thread = new Thread(_myxda.scanPorts);
        thread.IsBackground = true;
        thread.Start();

    }


    void _myxda_WirelessMasterDetected(object sender, PortInfoArg e)
    {
        switch (_state)
        {
            case States.DETECTING:
                {
                    Debug.Log(String.Format("Master Detected. Port: {0}, ID: {1}", e.PortInfo.portName().toString(), e.PortInfo.deviceId().toXsString().toString()));
                    _state = States.CONNECTING;
                    Debug.Log("이제 포트연다.");

                    while (_state == States.CONNECTING)
                    {
                        _myxda.openPort(e.PortInfo);
                    }

                }
                break;

            default:
                Debug.Log("포트 못 열었는데??.");
                break;
        }

    }

    void _myxda_DockedMtwDetected(object sender, PortInfoArg e)
    {
        Debug.Log(String.Format("MTw Docked. Port: {0}, ID: {1}", e.PortInfo.portName().toString(), e.PortInfo.deviceId().toXsString().toString()));

        String mtwId = e.PortInfo.deviceId().toXsString().toString();
        if (!dockedMtwList.Contains(mtwId))
        {
            dockedMtwList.Add(mtwId);
            Debug.Log(String.Format("Docked MTw list ({0}):", dockedMtwList.Count));
        }

    }
    void _myxda_MtwUndocked(object sender, PortInfoArg e)
    {
        Debug.Log(String.Format("MTw Undocked. Port: {0}, ID: {1}", e.PortInfo.portName().toString(), e.PortInfo.deviceId().toXsString().toString()));

        String mtwId = e.PortInfo.deviceId().toXsString().toString();

        dockedMtwList.Remove(mtwId);
        Debug.Log(String.Format("Docked MTw list ({0}):", dockedMtwList.Count));
    }
    void _myxda_OpenPortSuccessful(object sender, PortInfoArg e)
    {
        // Update the UI			
        switch (_state)
        {
            case States.CONNECTING:
                if (e.PortInfo.deviceId().isWirelessMaster())
                {
                    // Set the label to indicate the ID of the station.
                    //labelStationId.Text = e.PortInfo.deviceId().toXsString().toString();
                    _MyWirelessMasterDevice = _myxda.getDevice(e.PortInfo.deviceId());

                    // Attach the callback handler. This causes events to arrive in m_myWirelessMasterCallback.
                    _MyWirelessMasterDevice.addCallbackHandler(m_myWirelessMasterCallback);

                    _state = States.CONNECTED;
                    Debug.Log(String.Format("Master Connected. Port: {0}, ID: {1}", e.PortInfo.portName().toString(), e.PortInfo.deviceId().toXsString().toString()));

                    // Be sure to start with radio disabled
                    if (_MyWirelessMasterDevice.isRadioEnabled())
                    {
                        SetRadioChannel(-1);
                    }


                }
                break;
            default:
                Debug.Log("문제가 생긴듯?");
                break;
        }
    }
    void _myxda_OpenPortFailed(object sender, PortInfoArg e)
    {
        if (e.PortInfo.deviceId().isWirelessMaster())
        {
            Debug.Log(String.Format("Connect to wireless master failed. Port: {0}", e.PortInfo.portName().toString()));
        }
        else
        {
            Debug.Log(String.Format("Connect to device failed. Port: {0}", e.PortInfo.portName().toString()));
        }

        switch (_state)
        {
            case States.CONNECTING:
                Debug.Log("Closing XDA");
                _myxda.reset();
                _state = States.DETECTING;
                break;
            default:
                break;
        }
    }

    void _callbackHandler_MtwWireless(object sender, DeviceIdArg e)
    {

        Debug.Log(String.Format("MTw Connected. ID: {0}", e.DeviceId.toXsString().toString()));

        String mtwIdStr = e.DeviceId.toXsString().toString();
        ConnectedMTwData connectedMtwData = new ConnectedMTwData();
        if (!connectedMtwList.Contains(mtwIdStr))
        {
            connectedMtwList.Add(mtwIdStr);

            // This is a new MTw, add it.
            connectedMtwData._rssi = 0;
            connectedMtwData._frameSkipsList = new List<int>();
            _connectedMtwData[e.DeviceId.toInt()] = connectedMtwData;

            for (int i = 0; i < sensors.Count; i++)
            {
                if (string.Equals(sensors[i].deviceNum, mtwIdStr))
                {
                    sensors[i].turnOn = true;
                }
            }

            Debug.Log(String.Format("Connected MTw list ({0}):", connectedMtwList.Count));
        }

    }

    void _callbackHandler_MtwDisconnected(object sender, DeviceIdArg e)
    {
        String mtwIdStr = e.DeviceId.toXsString().toString();
        Debug.Log(String.Format("MTw Disconnected. ID: {0}", mtwIdStr));

        //Int32 index = connectedMtwList.BinarySearch(mtwIdStr);
        if (connectedMtwList.Contains(mtwIdStr))
        {
            // Found --> delete
            connectedMtwList.Remove(mtwIdStr);
            _connectedMtwData.Remove(e.DeviceId.toInt());


            Debug.Log(String.Format("Connected MTw list ({0}):", connectedMtwList.Count));
        }
    }

    void _callbackHandler_MeasurementStarted(object sender, DeviceIdArg e)
    {
        Debug.Log(String.Format("Measurement Started. ID: {0}", e.DeviceId.toXsString().toString()));

        if (_myxda.getDevice(e.DeviceId).deviceId().toInt() == _MyWirelessMasterDevice.deviceId().toInt())
        {
            switch (_state)
            {
                case States.AWAIT_MEASUREMENT_START:
                    {
                        // Get the MTws that are measuring and attach callback handlers
                        clearMeasuringMtws();
                        List<XsDeviceId> deviceIds = m_myWirelessMasterCallback.getConnectedMtws();
                        foreach (XsDeviceId devId in deviceIds)
                        {
                            XsDevice mtw = _myxda.getDevice(devId);
                            MyMtwCallback callback = new MyMtwCallback();

                            //Debug.Log("테스트 진행" + mtw.deviceId().toXsString().toString());
                            // connect signals
                            callback.DataAvailable += new EventHandler<DataAvailableArgs>(_callbackHandler_DataAvailable);

                            mtw.addCallbackHandler(callback);
                            _measuringMtws[mtw] = callback;
                        }
                        _state = States.MEASURING;
                    }
                    break;

                case States.RECORDING:
                case States.FLUSHING:
                    Debug.Log(String.Format("Recording Finished. ID: {0}", e.DeviceId.toXsString().toString()));
                    // Ready recording (flushing also ready), so file can be closed.
                    _MyWirelessMasterDevice.closeLogFile();
                    _state = States.MEASURING;
                    break;
                default:
                    break;
            }
        }
    }
    void _callbackHandler_MeasurementStopped(object sender, DeviceIdArg e)
    {
        Debug.Log(String.Format("Measurement Stopped. ID: {0}", e.DeviceId.toXsString().toString()));
        if (e.DeviceId.toInt() == _MyWirelessMasterDevice.deviceId().toInt())
        {
            clearMeasuringMtws();
            _state = States.OPERATIONAL;
        }
    }
    void _callbackHandler_DeviceError(object sender, DeviceErrorArgs e)
    {
        Debug.Log(String.Format("ERROR. ID: {0}", e.DeviceId.toXsString().toString()));
        switch (_state)
        {
            case States.AWAIT_MEASUREMENT_START:
                _state = States.ENABLED;
                break;
            default:
                break;
        }
    }
    void _callbackHandler_WaitingForRecordingStart(object sender, DeviceIdArg e)
    {
        Debug.Log(String.Format("Waiting for recording start. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
        _state = States.AWAIT_RECORDING_START;
    }
    void _callbackHandler_RecordingStarted(object sender, DeviceIdArg e)
    {
        if (_state == States.AWAIT_RECORDING_START)
        {
            Debug.Log(String.Format("Waiting for recording start. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
            _state = States.RECORDING;
        }
    }
    /*void _callbackHandler_ProgressUpdate(object sender, ProgressUpdateArgs e)
    {
        if (_state == States.FLUSHING && e.Identifier == "Flushing")
        {
            if (comboBoxUpdateRate.SelectedIndex == 0)
            {
                // Nothing to flush when at the highest update rate.
                _MyWirelessMasterDevice.abortFlushing();
                Debug.Log(String.Format("Flushing aborted. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
            }

            if (e.Total != 0 && comboBoxUpdateRate.SelectedIndex != 0)
            {
                // Only do this when there is still data to be flushed
                // and not the highest update rate was selected.
                progressBarFlushing.Maximum = e.Total;
                progressBarFlushing.Value = e.Current;
            }
        }
    }*/

    

    void _callbackHandler_DataAvailable(object sender, DataAvailableArgs e)
    {
        String mtwIdStr = e.Device.deviceId().toXsString().toString();


        if (!connectedMtwList.Contains(mtwIdStr))
        {
            Debug.Log(String.Format("Obsolete data received of an MTw {0} that's no longer in the list.", mtwIdStr));
            return;
        }

        if (!e.Packet.containsSdiData())
        {
            Debug.Log(String.Format("Packet received of an MTw {0} not containing data.", mtwIdStr));
            return;
        }

        // Getting SDI data.
        XsSdiData sdiData = e.Packet.sdiData();

        _connectedMtwData[e.Device.deviceId().toInt()]._rssi = e.Packet.rssi();

        if (e.Packet.containsOrientation())
        {
            //Getting Euler angles.
            //Debug.Log("오리엔테이션 데이터 패킷에 포함됩니다.");

            XsEuler oriEuler = e.Packet.orientationEuler();

            _connectedMtwData[e.Device.deviceId().toInt()]._orientation = oriEuler;

            XsQuaternion oriQuat = e.Packet.orientationQuaternion();

            _connectedMtwData[e.Device.deviceId().toInt()]._quaternion = oriQuat;




        }
        //if (e.Packet.containsFreeAcceleration()) 
        //{
        //    //Debug.Log("가속도 데이터 존재.");
        //    XsVector free_acc = e.Packet.freeAcceleration();
        //    //XsVector free_acc = e.Packet.calibratedAcceleration();

        //    _connectedMtwData[e.Device.deviceId().toInt()]._acceleration = free_acc;
        //    uint vecLength = free_acc.size();

        //    // Debug.LogFormat("value12: {0}", _connectedMtwData[e.Device.deviceId().toInt()]._acceleration);
        //    //Debug.LogFormat("벡터의 사이즈: {0}", vecLength);

        //}


        // -- Determine effective update rate percentage --

        // Determine the number of frames over which the SDI data in this
        // packet was determined.
        int frameSkips;


        if (e.Packet.frameRange().last() > e.Packet.frameRange().first())
        {
            frameSkips = e.Packet.frameRange().last() - e.Packet.frameRange().first() - 1;
        }
        else
        {
            // Rollover (internal framecounter is unsigned 16 bits integer)
            frameSkips = 65535 + e.Packet.frameRange().last() - e.Packet.frameRange().first() - 1;
        }

        _connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList.Add(frameSkips);
        _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips = _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips + (uint)frameSkips;
        _connectedMtwData[e.Device.deviceId().toInt()]._effectiveUpdateRate = (int)(100 * (1 - (float)_connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips / (float)(_connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList.Count() + _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips)));

        while (_connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList.Count() + _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips > 99 && _connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList.Count() > 0)
        {
            _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips = _connectedMtwData[e.Device.deviceId().toInt()]._sumFrameSkips - (uint)_connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList[0];
            _connectedMtwData[e.Device.deviceId().toInt()]._frameSkipsList.RemoveAt(0);
        }

        for (int i = 0; i < sensors.Count; i++)
        {
            if (string.Equals(sensors[i].deviceNum, e.Device.deviceId().toXsString().toString()) && sensors[i].turnOn == true)   //id를 비교해서 연결상태인지 확인
            {
                sensors[i].EulerX = _connectedMtwData[e.Device.deviceId().toInt()]._orientation.x();
                sensors[i].EulerY = _connectedMtwData[e.Device.deviceId().toInt()]._orientation.y();
                sensors[i].EulerZ = _connectedMtwData[e.Device.deviceId().toInt()]._orientation.z();

                sensors[i].QuatW = _connectedMtwData[e.Device.deviceId().toInt()]._quaternion.normalized().w();
                sensors[i].QuatX = _connectedMtwData[e.Device.deviceId().toInt()]._quaternion.normalized().x();
                sensors[i].QuatY = _connectedMtwData[e.Device.deviceId().toInt()]._quaternion.normalized().y();
                sensors[i].QuatZ = _connectedMtwData[e.Device.deviceId().toInt()]._quaternion.normalized().z();

                sensors[i].AccX = _connectedMtwData[e.Device.deviceId().toInt()]._acceleration.value(0); //x
                sensors[i].AccY = _connectedMtwData[e.Device.deviceId().toInt()]._acceleration.value(1); //Y
                sensors[i].AccZ = _connectedMtwData[e.Device.deviceId().toInt()]._acceleration.value(2); //Z

            }
            if (_is_alignment)
            {
                Debug.Log(e.Device.resetOrientation(XRM_Alignment));

                Debug.LogFormat("W: {0}, X: {1}, Y: {2}, Z: {3}", sensors[i].QuatW, sensors[i].QuatX, sensors[i].QuatY, sensors[i].QuatZ);
                _is_alignment = false;
            }
        }

        //Debug.Log(_connectedMtwData[e.Device.deviceId().toInt()]+"test1");
        //Debug.Log(e.Device.deviceId().toXsString().toString() + "test2");
        // Display data when MTw selected.
        //displayMtwData(_connectedMtwData[e.Device.deviceId().toInt()]);
    }

    void displayMtwData(ConnectedMTwData mtwData)
    {


        // Display Euler angles (if available).
        if (mtwData._containsOrientation)
        {
            Debug.Log(String.Format("{0,-5:f2} [deg]", mtwData._orientation.x()) + "X");
            Debug.Log(String.Format("{0,-5:f2} [deg]", mtwData._orientation.y()) + "Y");
            Debug.Log(String.Format("{0,-5:f2} [deg]", mtwData._orientation.z()) + "Z");
        }

        // Display effective update rate.
        Debug.Log(String.Format("{0} [%]", mtwData._effectiveUpdateRate));
    }

    // Track connection changes
    void _callbackHandler_MtwConnectionChanged(object sender, MtwEventArgs e)
    {
        if (e.Connected)
        {
            _mtws.Add(e.Mtw);
        }
        else if (_mtws.Contains(e.Mtw))
        {
            _mtws.Remove(e.Mtw);
        }

    }

    private void clearMeasuringMtws()
    {
        lock (_measuringMtws)
        {
            foreach (KeyValuePair<XsDevice, MyMtwCallback> item in _measuringMtws)
            {
                item.Key.clearCallbackHandlers();
            }
        }
        _measuringMtws.Clear();

    }

    /*private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        _portScanTimer.Enabled = false;
        _batteryLevelRequestTimer.Enabled = false;

        clearMeasuringMtws();

        if (_MyWirelessMasterDevice != null)
            _MyWirelessMasterDevice.clearCallbackHandlers();
        m_myWirelessMasterCallback.Dispose();

        _myxda.Dispose();
        _myxda = null;
    }*/

    private void ClosedSimulate()
    {
        /*_portScanTimer.Enabled = false;

        clearMeasuringMtws();

        if (_MyWirelessMasterDevice != null)
            _MyWirelessMasterDevice.clearCallbackHandlers();
        m_myWirelessMasterCallback.Dispose();

        _myxda.Dispose();
        _myxda = null;
        */
        //_portScanTimer.Enabled = false;
        thread.Join();
        Debug.Log("종료!!!!");
        _MyWirelessMasterDevice.disableRadio();
        //XsControl closePort = new XsControl();
        //closePort.close();
        _myxda.reset();


    }

    /*private void btnEnable_Click(object sender, EventArgs e)
    {
        SetRadioChannel(_state == States.CONNECTED ? 11 : -1);
    }*/

    private void EnableStart()
    {
        Debug.Log("states?" + _state);
        SetRadioChannel(_state == States.CONNECTED ? 12 : -1);  //일단 12채널로 시작
    }


    private void SetRadioChannel(int channel)
    {
        if (_MyWirelessMasterDevice.enableRadio(channel))
        {
            if (channel != -1)
            {
                Debug.Log(String.Format("Master Enabled. ID: {0}, Channel: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), channel));

                // Supported update rates and maximum available from xda
                XsIntArray supportedRates = _MyWirelessMasterDevice.supportedUpdateRates();
                _state = States.ENABLED;
            }
            else
            {
                Debug.Log(String.Format("Master Disabled. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                //connectedMtwList.Items.Clear();
                _state = States.CONNECTED;
            }
        }
        else
        {
            if (channel != -1)
            {
                Debug.Log(String.Format("Failed to enable wireless master. ID: {0}, Channel: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), channel));
            }
            else
            {
                Debug.Log(String.Format("Failed to disable wireless master. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
            }
        }
    }

    /*private void btnMeasure_Click(object sender, EventArgs e)
    {
        switch (_state)
        {
            case States.ENABLED:
            case States.OPERATIONAL:
                {
                    // First set the update rate
                    int desiredUpdateRate = 40;     //40, 60, 80, 100, 120 중 하나 지정
                    if (desiredUpdateRate != -1 && desiredUpdateRate != _MyWirelessMasterDevice.updateRate())
                    {
                        if (_MyWirelessMasterDevice.setUpdateRate(desiredUpdateRate))
                        {
                            Debug.Log(String.Format("Update rate set. ID: {0}, Rate: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), desiredUpdateRate));
                        }
                        else
                        {
                            Debug.Log(String.Format("Failed to set update rate. ID: {0}, Rate: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), desiredUpdateRate));
                        }
                    }


                    States bkpState = _state;
                    // Set the state to AWAIT_MEASUREMENT_START and go to measurement
                    _state = States.AWAIT_MEASUREMENT_START;

                    if (_MyWirelessMasterDevice.gotoMeasurement())
                    {
                        Debug.Log(String.Format("Waiting for measurement start. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                    else
                    {
                        // If gotoMeasurement fails revert the state
                        _state = bkpState;
                    }

                }
                break;

            case States.MEASURING:
                {
                    if (_MyWirelessMasterDevice.gotoConfig())
                    {
                        Debug.Log(String.Format("Stopping measurement. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                    else
                    {
                        Debug.Log(String.Format("Failed to stop measurement. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                }
                break;
            default:
                break;

        }
    }*/

    private void MeasureStart()
    {
        switch (_state)
        {
            case States.ENABLED:
            case States.OPERATIONAL:
                {
                    // First set the update rate
                    int desiredUpdateRate = 60;     //40, 60, 80, 100, 120 중 하나 지정
                    if (desiredUpdateRate != -1 && desiredUpdateRate != _MyWirelessMasterDevice.updateRate())
                    {
                        if (_MyWirelessMasterDevice.setUpdateRate(desiredUpdateRate))
                        {
                            Debug.Log(String.Format("Update rate set. ID: {0}, Rate: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), desiredUpdateRate));
                        }
                        else
                        {
                            Debug.Log(String.Format("Failed to set update rate. ID: {0}, Rate: {1}", _MyWirelessMasterDevice.deviceId().toXsString().toString(), desiredUpdateRate));
                        }
                    }


                    States bkpState = _state;
                    // Set the state to AWAIT_MEASUREMENT_START and go to measurement
                    _state = States.AWAIT_MEASUREMENT_START;

                    if (_MyWirelessMasterDevice.gotoMeasurement())
                    {
                        Debug.Log(String.Format("Waiting for measurement start. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                    else
                    {
                        // If gotoMeasurement fails revert the state
                        _state = bkpState;
                    }

                }
                break;

            case States.MEASURING:
                {
                    if (_MyWirelessMasterDevice.gotoConfig())
                    {
                        Debug.Log(String.Format("Stopping measurement. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                    else
                    {
                        Debug.Log(String.Format("Failed to stop measurement. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                    }
                }
                break;
            default:
                break;

        }
        Debug.Log("after measurestart" + _state);
    }

    /*private void btnRecord_Click(object sender, EventArgs e)
    {
        switch (_state)
        {
            case States.MEASURING:
                {
                    // -- Start the recording --

                    // Get the filename from the input and creating a log file.
                    String logFilename = textBoxFilename.Text;
                    if (_MyWirelessMasterDevice.createLogFile(new XsString(logFilename)) == XsResultValue.XRV_OK)
                    {
                        if (_MyWirelessMasterDevice.startRecording())
                        {

                        }
                        else
                        {
                            Debug.Log(String.Format("Failed to start recording. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));
                        }
                    }
                    else
                    {
                        Debug.Log(String.Format("Failed to create log file: {0}", logFilename));
                    }
                }
                break;

            case States.RECORDING:
                {
                    // -- Stop the recording --
                    _state = States.FLUSHING;
                    _MyWirelessMasterDevice.stopRecording();
                    Debug.Log(String.Format("Stopping recording. ID: {0}", _MyWirelessMasterDevice.deviceId().toXsString().toString()));

                }
                break;

            default:
                break;
        }
    }*/
    private void PrintSensorQuat() 
    {
        if (print_state) 
        {
            for (int sensor_num = 0; sensor_num < 6; sensor_num++)
            {
                Debug.LogFormat("Sensor {0} \n W: {1}, X: {2}, Y: {3}, Z: {4}", sensor_num, sensors[sensor_num].QuatW, sensors[sensor_num].QuatX, sensors[sensor_num].QuatY, sensors[sensor_num].QuatZ);
            }
            StartCoroutine(WaitAndPrint(3.0f));
           
        }


    }
    IEnumerator WaitAndPrint(float seconds)
    {
        // seconds만큼 대기합니다.
        yield return new WaitForSeconds(seconds);

        //// 대기 후에 이 부분이 실행됩니다.
        //Debug.Log("Waited for " + seconds + " seconds.");

        //// 다른 작업 수행 가능
        //Debug.Log("Coroutine finished!");
    }
}
