using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Avatar_Data : MonoBehaviour
{
    public PhotonView PV;
    public GameObject PythonNetwork;
    public Python_net Py_manager;

    PlayerScript plmanager;

    GameObject Head, Hips, Spine, LeftUpperArm, LeftForeArm, LeftHand, RightUpperArm, RightForeArm, RightHand;

    
    // Start is called before the first frame update
    void Start()
    {
        RightUpperArm = GameObject.Find("rShldrBend");
        RightForeArm = GameObject.Find("rForearmBend");
        RightHand = GameObject.Find("rHand");

        // Left arm part
        LeftUpperArm = GameObject.Find("lShldrBend");
        LeftForeArm = GameObject.Find("lForearmBend");
        LeftHand = GameObject.Find("lHand");

        PythonNetwork = GameObject.Find("pythonnetwork");
        Py_manager = PythonNetwork.GetComponent<Python_net>();


        plmanager = gameObject.GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(PV.IsMine)
        {
            
            if (plmanager.bl_loading_data)
            { 

            Py_manager.cur_plpose_vec.Add(RightUpperArm.transform.position);
            Py_manager.cur_plpose_vec.Add(RightForeArm.transform.position);
            Py_manager.cur_plpose_vec.Add(RightHand.transform.position);
            Py_manager.cur_plpose_vec.Add(LeftUpperArm.transform.position);
            Py_manager.cur_plpose_vec.Add(LeftForeArm.transform.position);
            Py_manager.cur_plpose_vec.Add(LeftHand.transform.position);
            
            }
            Py_manager.data_load_Available = plmanager.bl_loading_data;
        }
    }
}
