/*
 * Copyright (C) 2021 
 * Max-Planck-Gesellschaft zur Förderung der Wissenschaften e.V. (MPG),
 * acting on behalf of its Max Planck Institute for Intelligent Systems and
 * the Max Planck Institute for Biological Cybernetics. All rights reserved.
 *
 * Max-Planck-Gesellschaft zur Förderung der Wissenschaften e.V. (MPG) is
 * holder of all proprietary rights on this computer program. You can only use
 * this computer program if you have closed a license agreement with MPG or
 * you get the right to use the computer program from someone who is authorized
 * to grant you that right.
 * Any use of the computer program without a valid license is prohibited and
 * liable to prosecution.
 *
 * Contact: ps-license@tuebingen.mpg.de
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Joint recalculation
using LightweightMatrixCSharp;

public class SMPLX : MonoBehaviour
{
    public const int NUM_BETAS = 10;
    public const int NUM_EXPRESSIONS = 10;
    public const int NUM_JOINTS = 55;

    public enum ModelType {Unknown, Female, Neutral, Male};
    public enum HandPose {Flat, Relaxed, Test, AMASS, MANO_mean};
    public enum BodyPose {T, A, Test};

    public ModelType modelType = ModelType.Unknown;

    public float[] betas = new float[NUM_BETAS];
    public float[] expressions = new float[NUM_EXPRESSIONS];

    public bool usePoseCorrectives = true;
    public bool showJointPositions = false;

    private SkinnedMeshRenderer _smr = null;
    private Mesh _sharedMeshDefault = null;
    private bool _defaultShape = true;

    private int _numBetaShapes;
    private int _numExpressions;
    private int _numPoseCorrectives;

    private Mesh _bakedMesh = null;
    private Vector3[] _jointPositions = null;
    private Quaternion[] _jointRotations = null;


    public List<List<Quaternion>> L_load_data, R_load_data;
    public List<List<List<Quaternion>>> L_pose_data, R_pose_data;


    bool is_Coroutine = false;

    string[] _bodyJointNames = new string[] { 
        "pelvis", "left_hip", "right_hip",
        "spine1", "left_knee", "right_knee", "spine2", "left_ankle","right_ankle", "spine3", "left_foot","right_foot",
        "neck","left_collar","right_collar",
        "head","left_shoulder","right_shoulder", "left_elbow", "right_elbow","left_wrist","right_wrist",
        "jaw","left_eye_smplhf","right_eye_smplhf",
        
        "left_index1","left_index2","left_index3",
        "left_middle1","left_middle2","left_middle3",
        "left_pinky1","left_pinky2","left_pinky3",
        "left_ring1","left_ring2","left_ring3",
        "left_thumb1","left_thumb2","left_thumb3",
    
        "right_index1","right_index2","right_index3",
        "right_middle1","right_middle2","right_middle3",
        "right_pinky1","right_pinky2","right_pinky3",
        "right_ring1","right_ring2","right_ring3",
        "right_thumb1","right_thumb2","right_thumb3" 
    };
    string[] _handLeftJointNames = new string[] { 
        "left_index1",   "left_index2",   "left_index3",
        "left_middle1",  "left_middle2",  "left_middle3",
        "left_pinky1",   "left_pinky2",   "left_pinky3",
        "left_ring1",    "left_ring2",    "left_ring3",
        "left_thumb1",   "left_thumb2",   "left_thumb3" 
    };
    string[] _handRightJointNames = new string[] { 
        "right_index1",  "right_index2",  "right_index3",
        "right_middle1", "right_middle2", "right_middle3",
        "right_pinky1",  "right_pinky2",  "right_pinky3",
        "right_ring1",   "right_ring2",   "right_ring3",
        "right_thumb1",  "right_thumb2",  "right_thumb3" 
    };

    string[] _manualLeftJointNames = new string[] {
        "left_shoulder", "left_elbow",    "left_wrist",
        "left_index1",   "left_index2",   "left_index3",
        "left_middle1",  "left_middle2",  "left_middle3",
        "left_pinky1",   "left_pinky2",   "left_pinky3",
        "left_ring1",    "left_ring2",    "left_ring3",
        "left_thumb1",   "left_thumb2",   "left_thumb3"
    };

    string[] _manualRightJointNames = new string[] {
        "right_shoulder","right_elbow",   "right_wrist",
        "right_index1",  "right_index2",  "right_index3",
        "right_middle1", "right_middle2", "right_middle3",
        "right_pinky1",  "right_pinky2",  "right_pinky3",
        "right_ring1",   "right_ring2",   "right_ring3",
        "right_thumb1",  "right_thumb2",  "right_thumb3"
    };

    //string[] _sensorJointNames = new string[] {
    //    "right_shoulder","right_elbow", "left_shoulder", "left_elbow"
    //};


    float[] _handFlatLeft = new float[] { 0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f };
    float[] _handFlatRight = new float[] { 0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f };
    float[] _handRelaxedLeft = new float[] { 0.11167871206998825f, 0.042892176657915115f, -0.41644182801246643f, 0.10881132632493973f, -0.06598567962646484f, -0.7562199831008911f, -0.09639296680688858f, -0.09091565757989883f, -0.18845929205417633f, -0.1180950403213501f, 0.050943851470947266f, -0.529584527015686f, -0.14369840919971466f, 0.055241700261831284f, -0.7048571109771729f, -0.01918291673064232f, -0.09233684837818146f, -0.33791351318359375f, -0.4570329785346985f, -0.1962839514017105f, -0.6254575252532959f, -0.21465237438678741f, -0.06599828600883484f, -0.5068942308425903f, -0.3697243630886078f,   -0.060344625264406204f,  -0.07949022948741913f, -0.1418696939945221f, -0.08585263043642044f, -0.6355282664299011f, -0.3033415973186493f, -0.05788097530603409f, -0.6313892006874084f, -0.17612089216709137f, -0.13209307193756104f, -0.37335458397865295f, 0.8509643077850342f, 0.27692273259162903f, -0.09154807031154633f, -0.4998394250869751f, 0.02655647136271f, 0.05288087576627731f, 0.5355591773986816f, 0.04596104100346565f, -0.2773580253124237f };
    float[] _handRelaxedRight = new float[] { 0.11167871206998825f,-0.042892176657915115f,0.41644182801246643f,0.10881132632493973f,0.06598567962646484f,0.7562199831008911f,-0.09639296680688858f,0.09091565757989883f,0.18845929205417633f,-0.1180950403213501f,-0.050943851470947266f,0.529584527015686f,-0.14369840919971466f,-0.055241700261831284f,0.7048571109771729f,-0.01918291673064232f,0.09233684837818146f,0.33791351318359375f,-0.4570329785346985f,0.1962839514017105f,0.6254575252532959f,-0.21465237438678741f,0.06599828600883484f,0.5068942308425903f,-0.3697243630886078f,0.060344625264406204f,0.07949022948741913f,-0.1418696939945221f,0.08585263043642044f,0.6355282664299011f,-0.3033415973186493f,0.05788097530603409f,0.6313892006874084f,-0.17612089216709137f,0.13209307193756104f,0.37335458397865295f,0.8509643077850342f,-0.27692273259162903f,0.09154807031154633f,-0.4998394250869751f,-0.02655647136271f,-0.05288087576627731f,0.5355591773986816f,-0.04596104100346565f,0.2773580253124237f };
    float[] _handTestLeft = new float[] { 
        // Left_index
        0.011177415028214455f,  0.2590852677822113f,    0.11648038029670715f, 
       -0.13260921835899353f,   0.04760228097438812f,   0.4385834336280823f, 
        0.10277135670185089f,   0.17290548980236053f,   0.08385777473449707f, 
        // Left_middle
        0.0051428102888166904f, 0.09272782504558563f,   0.13272805511951447f,
        0.06987312436103821f,  -0.04163992777466774f,   0.3943122923374176f, 
       -0.07007572799921036f,   0.06433552503585815f,   0.15722286701202393f,
        // Left_pinky
        0.02963981218636036f,  -0.30420127511024475f,   0.14133693277835846f, 
        0.1646745651960373f,    0.14118221402168274f,   0.14176444709300995f,
        0.017472274601459503f,  0.19599202275276184f,   0.11721484363079071f,
        // Left_ring
        0.055301595479249954f,  0.01798488385975361f,   0.14360643923282623f,
        0.05487954616546631f,   0.01640693098306656f,   0.42330318689346313f,
       -0.030754704028367996f,  0.13594691455364227f,   0.13122965395450592f,
        // Left_thumb
       -0.1604069322347641f,   -0.09846090525388718f,   0.2918240427970886f,
       -0.18725790083408356f,  -0.1291208416223526f,    0.03440938517451286f,
       -0.018986323848366737f, -0.0005881433608010411f, 0.29313862323760986f 
    };
    float[] _handTestRight = new float[] { 0.01326274499297142f, -0.20413629710674286f, -0.15937039256095886f, -0.15078474581241608f, -0.04514279216527939f, -0.48306483030319214f, 0.10417323559522629f, -0.1836467832326889f, -0.10250554978847504f, 0.011679206974804401f, -0.07861433178186417f, -0.18614766001701355f, 0.08135887235403061f, 0.030331499874591827f, -0.43081870675086975f, -0.07722432166337967f, -0.07975051552057266f, -0.1706584393978119f, 0.13209141790866852f, 0.32395777106285095f, -0.3861182630062103f, 0.12642964720726013f, -0.13008570671081543f, -0.15177564322948456f, 0.021906709298491478f, -0.16517646610736847f, 0.02528587356209755f, 0.06796213984489441f, -0.025236478075385094f, -0.15547266602516174f, 0.060269180685281754f, -0.02008354291319847f, -0.4861707091331482f, -0.01259240135550499f, -0.1396889090538025f, -0.17316514253616333f, -0.14130337536334991f, 0.044829096645116806f, -0.2754846215248108f, -0.17891336977481842f, 0.14570343494415283f, -0.05630068853497505f, -0.01855561137199402f, 0.043099239468574524f, -0.1657646745443344f };
    

    Dictionary<string, Transform> _transformFromName;

    // Joint recalculation
    public static Dictionary<string, Matrix[]> JointMatrices = null;



    public void Awake()
    {
        if (_transformFromName == null)
        {
            _transformFromName = new Dictionary<string, Transform>();
            Transform[] transforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                _transformFromName.Add(t.name, t);
            }
        }

        if (_jointPositions == null)
        {
            _jointPositions = new Vector3[NUM_JOINTS];
            for (int i=0; i< NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointPositions[i] = joint.position;
            }
        }

        if (_jointRotations == null)
        {
            _jointRotations = new Quaternion[NUM_JOINTS];
        }

        if (SMPLX.JointMatrices == null)
            InitJointRegressor();

        if (_smr != null)
            return;

        _smr = GetComponentInChildren<SkinnedMeshRenderer>();

        // Get skinned mesh blend shape values
        _numBetaShapes = 0;
        _numExpressions = 0;
        _numPoseCorrectives = 0;

        int blendShapeCount = _smr.sharedMesh.blendShapeCount;
        for (int i=0; i<blendShapeCount; i++)
        {
            string name = _smr.sharedMesh.GetBlendShapeName(i);
            if (name.StartsWith("Shape"))
                _numBetaShapes++;
            else if (name.StartsWith("Exp"))
                _numExpressions++;
            else if (name.StartsWith("Pose"))
                _numPoseCorrectives++;
        }
        
    }

    private bool InitJointRegressor()
    {
        SMPLX.JointMatrices = new Dictionary<string, Matrix[]>();

        // Setup gender specific joint regressors
        string[] genders = new string[] {"female", "neutral", "male"};
        foreach (string gender in genders)
        {
            Debug.Log("[SMPL-X] Setup betas-to-joints regressor: " + gender);
            string name_betas = "betasToJoints_" + gender;
            string name_template = "templateJ_" + gender;

            Matrix[] betasToJoints = new Matrix[3];
            Matrix[] templateJ = new Matrix[3];
            for (int i=0; i<=2; i++)
            {
                betasToJoints[i] = new Matrix(NUM_JOINTS, NUM_BETAS);
                templateJ[i] = new Matrix(NUM_JOINTS, 1);
            }

            // Setup matrix values from JSON resource files
            string name = "smplx_betas_to_joints_" + gender;
            TextAsset ta = Resources.Load<TextAsset>(name);
            if (ta == null)
            {
                Debug.LogError("[SMPL-X] Cannot find betas-to-joint regressor: SMPLX/Resources/" + name);
                return false;
            }
            SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(ta.text);

            for (int i=0; i < NUM_JOINTS; i++)
            {
                // Init beta regressor matrix
                for (int j=0; j< NUM_BETAS; j++)
                {
                    (betasToJoints[0])[i, j] = node["betasJ_regr"][i][0][j].AsDouble;
                    (betasToJoints[1])[i, j] = node["betasJ_regr"][i][1][j].AsDouble;
                    (betasToJoints[2])[i, j] = node["betasJ_regr"][i][2][j].AsDouble;
                }

                // Init joint template matrix
                double x = node["template_J"][i][0].AsDouble;
                double y = node["template_J"][i][1].AsDouble;
                double z = node["template_J"][i][2].AsDouble;

                (templateJ[0])[i, 0] = x;
                (templateJ[1])[i, 0] = y;
                (templateJ[2])[i, 0] = z;
            }

            SMPLX.JointMatrices.Add(name_betas, betasToJoints);
            SMPLX.JointMatrices.Add(name_template, templateJ);
        }
        return true;
    }

    public bool HasBetaShapes()
    {
        return (_numBetaShapes > 0);
    }

    public bool HasExpressions()
    {
        return (_numExpressions > 0);
    }

    public bool HasPoseCorrectives()
    {
        return (_numPoseCorrectives > 0);
    }

    public Vector3[] GetJointPositions()
    {
        return _jointPositions;
    }

    public void SetBetaShapes()
    {
        if (! HasBetaShapes() )
        {
            Debug.LogError("[SMPL-X] ERROR: Cannot set beta shapes on model without beta shapes");
            return;
        }

        _defaultShape = true;
        for (int i=0; i<NUM_BETAS; i++)
        {
            _smr.SetBlendShapeWeight(i, betas[i] * 100); // blend shape weights are specified in percentage

            if (betas[i] != 0.0f)
                _defaultShape = false;
        }

        UpdateJointPositions();
    }

    public void SetExpressions()
    {
        if (! HasExpressions() )
        {
            Debug.LogError("[SMPL-X] ERROR: Cannot set expressions on model without expressions");
            return;
        }

        for (int i=0; i<NUM_EXPRESSIONS; i++)
            _smr.SetBlendShapeWeight(i + NUM_BETAS, expressions[i] * 100); // blend shape weights are specified in percentage
    }

    public void SnapToGroundPlane()
    {
        if (_bakedMesh == null)
            _bakedMesh = new Mesh();

        _smr.BakeMesh(_bakedMesh);
        Vector3[] vertices =_bakedMesh.vertices;
        float yMin = vertices[0].y;
        for (int i=1; i<vertices.Length; i++)
        {
            float y = vertices[i].y;

            if (y < yMin)
                yMin = y;
        }

        Vector3 localPosition = gameObject.transform.localPosition;
        if (Mathf.Abs(yMin) < 0.00001)
            yMin = 0.0f;

        localPosition.y = -yMin;
        gameObject.transform.localPosition = localPosition;

        // Update joint world positions
        UpdateJointPositions(false);

    }

    public void GetModelInfo(out int shapes, out int expressions, out int poseCorrectives)
    {
        shapes = _numBetaShapes;
        expressions = _numExpressions;
        poseCorrectives = _numPoseCorrectives;
    }

    // Return Unity Quaternion for given SMPL-X rodrigues notation
    public static Quaternion QuatFromRodrigues(float rodX, float rodY, float rodZ)
    {
        // Local joint coordinate systems
        //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
        //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
        Vector3 axis = new Vector3(-rodX, rodY, rodZ);
        float angle_deg = - axis.magnitude * Mathf.Rad2Deg;
        Vector3.Normalize(axis);

        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);
        
        return quat;
    }

    public void SetLocalJointRotation(string name, Quaternion quatLocal)
    {
        Transform joint = _transformFromName[name];
        joint.localRotation = quatLocal;
    }

    //public void JointRotationSlerp(string name, Quaternion quat1, Quaternion quat2)
    //{
    //    Transform joint = _transformFromName[name];
    //    joint.localRotation = Quaternion.Slerp(quat1, quat2, Time.deltaTime); 
    //}

    public void SetHandPose(HandPose pose)
    {
        float[] left = null;
        float[] right = null;

        if (pose == HandPose.Flat)
        {
            left = _handFlatLeft;
            right = _handFlatRight;
        }
        else if (pose == HandPose.Relaxed)
        {
            left = _handRelaxedLeft;
            right = _handRelaxedRight;
        }
        else if (pose == HandPose.Test)
        {
            left = _handTestLeft;
            right = _handTestRight;
        }

        if ((left != null) && (right != null))
        {
            for (int i=0; i<15; i++)
            {
                string name = _handLeftJointNames[i];
                float rodX = left[i*3 + 0];
                float rodY = left[i*3 + 1];
                float rodZ = left[i*3 + 2];
                Quaternion quat = QuatFromRodrigues(rodX, rodY, rodZ);
                SetLocalJointRotation(name, quat);

                name = _handRightJointNames[i];
                rodX = right[i*3 + 0];
                rodY = right[i*3 + 1];
                rodZ = right[i*3 + 2];
                quat = QuatFromRodrigues(rodX, rodY, rodZ);
                SetLocalJointRotation(name, quat);
            }
        }

        UpdateJointPositions(false);

    }

    public void ResetBodyPose()
    {
        foreach(string name in _bodyJointNames)
        {
            Transform joint = _transformFromName[name];
            joint.localRotation = Quaternion.identity;
        }

        UpdateJointPositions(false);
    }

    public void SetBodyPose(BodyPose pose)
    {
        if (pose == BodyPose.T)
        {
            ResetBodyPose();
        }
        else if (pose == BodyPose.A)
        {
            ResetBodyPose();
            SetLocalJointRotation("left_collar", Quaternion.Euler(0.0f, 0.0f, 10.0f));
            SetLocalJointRotation("left_shoulder", Quaternion.Euler(0.0f, 0.0f, 35.0f));
            SetLocalJointRotation("right_collar", Quaternion.Euler(0.0f, 0.0f, -10.0f));
            SetLocalJointRotation("right_shoulder", Quaternion.Euler(0.0f, 0.0f, -35.0f));
        }
        else if (pose == BodyPose.Test)
        {
            is_Coroutine = true;
            StartCoroutine("RotationDelay");
        }

        UpdatePoseCorrectives();
        UpdateJointPositions(false);
    }


    public void EnablePoseCorrectives(bool enabled)
    {
        usePoseCorrectives = enabled;
        if (usePoseCorrectives)
        {
            UpdatePoseCorrectives();
        }
        else
        {
            int blendShapeCount = _smr.sharedMesh.blendShapeCount;
            for (int i=0; i<blendShapeCount; i++)
            {
                string name = _smr.sharedMesh.GetBlendShapeName(i);
                if (name.StartsWith("Pose"))
                    _smr.SetBlendShapeWeight(i, 0.0f);
            }
        }
    }

    public void UpdatePoseCorrectives()
    {
        if (!usePoseCorrectives)
            return;

        if (! HasPoseCorrectives())
            return;

        // Body joint #0 has no pose correctives
        for (int i=1; i<_bodyJointNames.Length; i++)
        {
            string name = _bodyJointNames[i];
            Quaternion quat = _transformFromName[name].localRotation;

            // Local joint coordinate systems
            //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
            //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
            Quaternion quatSMPLX = new Quaternion(-quat.x, quat.y, quat.z, -quat.w);
            Matrix4x4 m = Matrix4x4.Rotate(quatSMPLX);
            // Subtract identity matrix to get proper pose shape weights
            m[0,0] = m[0,0] - 1.0f;
            m[1,1] = m[1,1] - 1.0f;
            m[2,2] = m[2,2] - 1.0f;
            
            // Get corrective pose start index
            int poseStartIndex = NUM_BETAS + NUM_EXPRESSIONS + (i-1)*9;

            _smr.SetBlendShapeWeight(poseStartIndex + 0, 100.0f * m[0,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 1, 100.0f * m[0,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 2, 100.0f * m[0,2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 3, 100.0f * m[1,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 4, 100.0f * m[1,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 5, 100.0f * m[1,2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 6, 100.0f * m[2,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 7, 100.0f * m[2,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 8, 100.0f * m[2,2]);
        }
    }

    public bool UpdateJointPositions(bool recalculateJoints = true)
    {
        if (HasBetaShapes() && recalculateJoints)
        {
            if (_sharedMeshDefault == null)
            {
                // Do not clone mesh if we haven't modified the shape parameters yet
                if (_defaultShape)
                    return false;

                // Clone default shared mesh so that we can modify later the shared mesh bind pose without affecting other shared instances.
                // Note that this will drastically increase the Unity scene file size and make Unity Editor very slow on save when multiple bodies like this are used.
                _sharedMeshDefault = _smr.sharedMesh;
                _smr.sharedMesh = (Mesh)Instantiate( _smr.sharedMesh );
                Debug.LogWarning("[SMPL-X] Cloning shared mesh to allow for joint recalculation on beta shape change [" + gameObject.name + "]. Note that this will increase the current scene file size significantly if model contains pose correctives.");
            }

            // Save pose and repose to T-Pose
            for (int i=0; i<NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointRotations[i] = joint.localRotation;
                joint.localRotation = Quaternion.identity;
            }

            // Create beta value matrix
            Matrix betaMatrix = new Matrix(NUM_BETAS, 1);
            for (int row = 0; row < NUM_BETAS; row++)
            {
                betaMatrix[row, 0] = betas[row];
            }

            // Apply joint regressor to beta matrix to calculate new joint positions
            string gender = "";
            if (modelType == SMPLX.ModelType.Female)
                gender = "female";
            else if (modelType == SMPLX.ModelType.Neutral)
                gender = "neutral";
            else if (modelType == SMPLX.ModelType.Male)
                gender = "male";
            else
            {
                Debug.LogError("[SMPL-X] ERROR: Joint regressor needs model type information (Female/Neutral/Male)");
                return false;
            }

            Matrix[] betasToJoints = SMPLX.JointMatrices["betasToJoints_" + gender];
            Matrix[] templateJ = SMPLX.JointMatrices["templateJ_" + gender];;

            Matrix newJointsX = betasToJoints[0] * betaMatrix + templateJ[0];
            Matrix newJointsY = betasToJoints[1] * betaMatrix + templateJ[1];
            Matrix newJointsZ = betasToJoints[2] * betaMatrix + templateJ[2];

            // Update joint position cache
            for (int index = 0; index < NUM_JOINTS; index++)
            {
                Transform joint = _transformFromName[_bodyJointNames[index]];

                // Convert regressor coordinate system (OpenGL) to Unity coordinate system by negating X value
                Vector3 position = new Vector3(-(float)newJointsX[index, 0], (float)newJointsY[index, 0], (float)newJointsZ[index, 0]);

                // Regressor joint positions from joint calculation are centered at origin in world space
                // Transform to game object space for correct world space position
                joint.position = gameObject.transform.TransformPoint(position);
            }

            // Set new bind pose
            Matrix4x4[] bindPoses = _smr.sharedMesh.bindposes;
            Transform[] bones = _smr.bones;
            for (int i=0; i<bones.Length; i++)
            {
                // The bind pose is bone's inverse transformation matrix.
                // Make this matrix relative to the avatar root so that we can move the root game object around freely.
                bindPoses[i] = bones[i].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;
            }
            _smr.sharedMesh.bindposes = bindPoses;

            // Restore pose
            for (int i=0; i<NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                joint.localRotation = _jointRotations[i];

                // Update joint position cache
                _jointPositions[i] = joint.position;

            }
        }
        else
        {
            for (int i=0; i<NUM_JOINTS; i++)
            {
                // Update joint position cache
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointPositions[i] = joint.position;
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!usePoseCorrectives)
            UpdatePoseCorrectives();
   

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            readData readData;
            readData = GetComponent<readData>();


            L_pose_data = readData.L_pose_idx;
            R_pose_data = readData.R_pose_idx;

            //load_sensor_data = readData.sensor_data;

            Debug.Log("Read Quaternion data");
        }


        //int joint_num = _manualLeftJointNames.Count();

        //if (is_Coroutine)
        //{
        //    Debug.Log("Motion Start");

        //    ResetBodyPose();

        //    for (int i = 0; i < joint_num; i++)
        //    {
        //        string l_name = _manualLeftJointNames[i];
        //        JointRotationSlerp(l_name, L_load_data[i], L_load_data[joint_num + i]);

        //        string r_name = _manualRightJointNames[i];
        //        JointRotationSlerp(r_name, R_load_data[i], R_load_data[joint_num + i]);
        //    }

        //    is_Coroutine = false;
        //}

    }

    IEnumerator RotationDelay()
    {
        Debug.Log("Motion Start");
        
        int joint_num = _manualLeftJointNames.Count();

        if (is_Coroutine)
        {
            ResetBodyPose();

            for (float i = 0; i < 60.0f; i++)
            {
                float t = i / 60.0f;

                for (int j = 0; j < joint_num; j++)
                {
                    int l_frame_num = L_pose_data[0][j].Count();
                    int r_frame_num = R_pose_data[0][j].Count();

                    string l_name = _manualLeftJointNames[j];
                    string r_name = _manualRightJointNames[j];

                    Transform l_joint = _transformFromName[l_name];
                    Transform r_joint = _transformFromName[r_name];


                    for (int k = 0; k < l_frame_num - 1; k++)
                        l_joint.localRotation = Quaternion.Slerp(L_pose_data[0][j][k], L_pose_data[2][j][k + 1], t);
        
                    for (int k = 0; k < r_frame_num - 1; k++)
                        r_joint.localRotation = Quaternion.Slerp(R_pose_data[0][j][k], R_pose_data[2][j][k + 1], t);


                }
                    yield return new WaitForSeconds(0.005f);

            }

            is_Coroutine = false;
        }



        //int joint_num = 4;

        // if (is_Coroutine)
        // {
        //     ResetBodyPose();

        //     for (int i = 0; i < load_sensor_data.Count() / joint_num; i++)
        //     {
        //         R_upperArm.rotation = load_sensor_data[i + 2];
        //         R_lowerArm.rotation = load_sensor_data[i + 3];
        //         L_upperArm.rotation = load_sensor_data[i + 4];
        //         L_lowerArm.rotation = load_sensor_data[i + 5];

        //         yield return new WaitForSeconds(1.0f);
        //     }

        //     is_Coroutine = false;
        // }



        /*
        int joint_num = _manualLeftJointNames.Count();

        if (is_Coroutine)
        {
            ResetBodyPose();

            // joint
            for (int i = 0; i < joint_num; i++)
            {
                // frame
                for (int j = 0; j < L_load_data[i].Count; j++)
                {
                    string l_name = _manualLeftJointNames[i];
                    SetLocalJointRotation(l_name, L_load_data[i][j]);

                    string r_name = _manualRightJointNames[i];
                    SetLocalJointRotation(r_name, R_load_data[i][j]);
                }

                yield return new WaitForSeconds(1.0f);

            }
            //for (int j = (L_load_data.Count() / joint_num) - 1; j >= 0 / joint_num; j--)
            //{
            //    for (int i = joint_num - 1; i >= 0; i--)
            //    {
            //        string l_name = _manualLeftJointNames[i];
            //        SetLocalJointRotation(l_name, L_load_data[joint_num * j + i]);

            //        string r_name = _manualRightJointNames[i];
            //        SetLocalJointRotation(r_name, R_load_data[joint_num * j + i]);
            //    }

            //    yield return new WaitForSeconds(1.0f);
            //}

            is_Coroutine = false;
        }
        */

        StopCoroutine("RotationDelay");
    }
}

////////////////////////////////////////////////////////////////////////////////
// Custom editor code
////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
[CustomEditor(typeof(SMPLX))]
public class SMPLX_Editor : Editor {

    private SMPLX _target;
    private SerializedProperty _modelTypeProperty;
    private bool _showShape = true;
    private bool _showExpression = true;
    private bool _autoSnapToGroundPlane = true;
    private string _modelInfoText;

    void Awake() 
    {
        _target = (SMPLX)target;
        _target.Awake(); // initialize member values in Editor mode

        int shapes, expressions, poseCorrectives;
        _target.GetModelInfo(out shapes, out expressions, out poseCorrectives);
        _modelInfoText = string.Format("Model: {0} beta shapes, {1} expressions, {2} pose correctives", shapes, expressions, poseCorrectives);
    }

    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        _modelTypeProperty = serializedObject.FindProperty("modelType");
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(_target, _target.name); // allow GUI undo in custom editor
        Color defaultColor=GUI.backgroundColor;

        using (new EditorGUILayout.VerticalScope("Box")) 
        {
            // Info
            EditorGUILayout.HelpBox(_modelInfoText, MessageType.None);

            // Shape
            if (_target.HasBetaShapes() || _target.HasPoseCorrectives() )
            {
                using (new EditorGUILayout.VerticalScope("Box")) 
                {
                    using (new EditorGUILayout.VerticalScope("Box")) 
                    {
                        GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button("Shape"))
                            _showShape = ! _showShape;
                        GUI.backgroundColor = defaultColor;

                        if (_target.HasPoseCorrectives())
                        {
                            float labelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 200;
                            bool usePoseCorrectivesNew = EditorGUILayout.Toggle("Use Pose Correctives", _target.usePoseCorrectives);
                            if (usePoseCorrectivesNew != _target.usePoseCorrectives)
                            {
                                if (usePoseCorrectivesNew)
                                    _target.EnablePoseCorrectives(true);
                                else
                                    _target.EnablePoseCorrectives(false);
                            }
                            EditorGUIUtility.labelWidth = labelWidth;
                        }

                        if (_target.HasBetaShapes())
                        {
                            EditorGUILayout.PropertyField(_modelTypeProperty);
                        }

                    }
                    if (_showShape && _target.HasBetaShapes())
                    {
                        using (new EditorGUILayout.VerticalScope("Box")) 
                        {
                            for (int i=0; i<SMPLX.NUM_BETAS; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Beta " + i, GUILayout.Width(50));
                                _target.betas[i] = EditorGUILayout.Slider(_target.betas[i], -5, 5);
                                // no effect: GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }

                            float labelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 200;
                            _autoSnapToGroundPlane = EditorGUILayout.Toggle("Snap Feet To Local Ground Plane", _autoSnapToGroundPlane);
                            EditorGUIUtility.labelWidth = labelWidth;

                        }
                        using (new EditorGUILayout.VerticalScope("Box")) 
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Set"))
                            {
                                _target.SetBetaShapes();

                                if (_autoSnapToGroundPlane)
                                    _target.SnapToGroundPlane();

                            }
                            if (GUILayout.Button("Random"))
                            {
                                for (int i=0; i<SMPLX.NUM_BETAS; i++)
                                {
                                    _target.betas[i] = Random.Range(-2.0f, 2.0f);
                                }
                                _target.SetBetaShapes();

                                if (_autoSnapToGroundPlane)
                                    _target.SnapToGroundPlane();
                            }
                            if (GUILayout.Button("Reset"))
                            {
                                for (int i=0; i<SMPLX.NUM_BETAS; i++)
                                {
                                    _target.betas[i] = 0.0f;
                                }
                                _target.SetBetaShapes();

                                if (_autoSnapToGroundPlane)
                                    _target.SnapToGroundPlane();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            // Expression
            if (_target.HasExpressions())
            {
                using (new EditorGUILayout.VerticalScope("Box")) 
                {
                    using (new EditorGUILayout.VerticalScope("Box")) 
                    {
                        GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button("Expression"))
                            _showExpression = ! _showExpression;
                        GUI.backgroundColor = defaultColor;
                    }

                    if (_showExpression)
                    {
                        using (new EditorGUILayout.VerticalScope("Box")) 
                        {
                            for (int i=0; i<SMPLX.NUM_BETAS; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Exp " + i, GUILayout.Width(50));
                                _target.expressions[i] = EditorGUILayout.Slider(_target.expressions[i], -2, 2);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        using (new EditorGUILayout.VerticalScope("Box")) 
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Set"))
                            {
                                _target.SetExpressions();
                            }
                            if (GUILayout.Button("Random"))
                            {
                                for (int i=0; i<SMPLX.NUM_EXPRESSIONS; i++)
                                {
                                    _target.expressions[i] = Random.Range(-2.0f, 2.0f);
                                }
                                _target.SetExpressions();
                            }
                            if (GUILayout.Button("Reset"))
                            {
                                for (int i=0; i<SMPLX.NUM_EXPRESSIONS; i++)
                                {
                                    _target.expressions[i] = 0.0f;
                                }
                                _target.SetExpressions();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            // Pose
            using (new EditorGUILayout.VerticalScope("Box")) 
            {
                using (new EditorGUILayout.VerticalScope("Box")) 
                {
                    GUI.backgroundColor = Color.yellow;
                    GUILayout.Button("Pose");
                    GUI.backgroundColor = defaultColor;

                }

                using (new EditorGUILayout.VerticalScope("Box")) 
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Body Pose", GUILayout.Width(100));
                    if (GUILayout.Button("T-Pose"))
                    {
                        _target.SetBodyPose(SMPLX.BodyPose.T);
                    }
                    if (GUILayout.Button("A-Pose"))
                    {
                        _target.SetBodyPose(SMPLX.BodyPose.A);
                    }
                    if (GUILayout.Button("Test"))
                    {
                        _target.SetBodyPose(SMPLX.BodyPose.Test);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Hand Pose", GUILayout.Width(100));
                    if (GUILayout.Button("    Flat    "))
                    {
                        _target.SetHandPose(SMPLX.HandPose.Flat);
                    }
                    if (GUILayout.Button("Relaxed"))
                    {
                        _target.SetHandPose(SMPLX.HandPose.Relaxed);
                    }
                    if (GUILayout.Button("Test"))
                    {
                        _target.SetHandPose(SMPLX.HandPose.Test);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }          

            // Drawing
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                using (new EditorGUILayout.VerticalScope("Box")) 
                {
                    GUI.backgroundColor = Color.yellow;
                    GUILayout.Button("Drawing");
                    GUI.backgroundColor = defaultColor;
                }

                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 200;
                    bool showJointPositions = EditorGUILayout.Toggle("Show Joint Positions", _target.showJointPositions);
                    if (showJointPositions != _target.showJointPositions)
                    {
                        if (showJointPositions)
                            _target.UpdateJointPositions(false);

                        _target.showJointPositions = showJointPositions;
                        SceneView.RepaintAll();
                    }
                    EditorGUIUtility.labelWidth = labelWidth;
                }
            }
        }

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        if (! _target.showJointPositions)
            return;

        Handles.color = Color.yellow;

        Vector3[] jointPositions = _target.GetJointPositions();
        foreach (Vector3 pos in jointPositions)
        {
            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.025f, EventType.Repaint);
        }
    }
}
#endif // UNITY_EDITOR

/*
**Backup**

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Joint recalculation
using LightweightMatrixCSharp;

public class SMPLX : MonoBehaviour
{
    public const int NUM_BETAS = 10;
    public const int NUM_EXPRESSIONS = 10;
    public const int NUM_JOINTS = 55;

    public enum ModelType {Unknown, Female, Neutral, Male};
    public enum HandPose {Flat, Relaxed, Test, AMASS, MANO_mean};
    public enum BodyPose {T, A, Test};

    public ModelType modelType = ModelType.Unknown;

    public float[] betas = new float[NUM_BETAS];
    public float[] expressions = new float[NUM_EXPRESSIONS];

    public bool usePoseCorrectives = true;
    public bool showJointPositions = false;

    private SkinnedMeshRenderer _smr = null;
    private Mesh _sharedMeshDefault = null;
    private bool _defaultShape = true;

    private int _numBetaShapes;
    private int _numExpressions;
    private int _numPoseCorrectives;

    private Mesh _bakedMesh = null;
    private Vector3[] _jointPositions = null;
    private Quaternion[] _jointRotations = null;


    //public Transform L_upperArm, L_lowerArm, R_upperArm, R_lowerArm;
    public List<Quaternion> data = new List<Quaternion>();
    bool is_Coroutine = false;

    string[] _bodyJointNames = new string[] { 
        "pelvis", "left_hip", "right_hip",
        "spine1", "left_knee", "right_knee", "spine2", "left_ankle","right_ankle", "spine3", "left_foot","right_foot",
        "neck","left_collar","right_collar",
        "head","left_shoulder","right_shoulder", "left_elbow", "right_elbow","left_wrist","right_wrist",
        "jaw","left_eye_smplhf","right_eye_smplhf",
        
        "left_index1","left_index2","left_index3",
        "left_middle1","left_middle2","left_middle3",
        "left_pinky1","left_pinky2","left_pinky3",
        "left_ring1","left_ring2","left_ring3",
        "left_thumb1","left_thumb2","left_thumb3",
    
        "right_index1","right_index2","right_index3",
        "right_middle1","right_middle2","right_middle3",
        "right_pinky1","right_pinky2","right_pinky3",
        "right_ring1","right_ring2","right_ring3",
        "right_thumb1","right_thumb2","right_thumb3" 
    };
    string[] _handLeftJointNames = new string[] { 
        "left_index1",   "left_index2",   "left_index3",
        "left_middle1",  "left_middle2",  "left_middle3",
        "left_pinky1",   "left_pinky2",   "left_pinky3",
        "left_ring1",    "left_ring2",    "left_ring3",
        "left_thumb1",   "left_thumb2",   "left_thumb3" 
    };
    string[] _handRightJointNames = new string[] { 
        "right_index1",  "right_index2",  "right_index3",
        "right_middle1", "right_middle2", "right_middle3",
        "right_pinky1",  "right_pinky2",  "right_pinky3",
        "right_ring1",   "right_ring2",   "right_ring3",
        "right_thumb1",  "right_thumb2",  "right_thumb3" 
    };

    float[] _handFlatLeft = new float[] { 0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f };
    float[] _handFlatRight = new float[] { 0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f };
    float[] _handRelaxedLeft = new float[] { 0.11167871206998825f, 0.042892176657915115f, -0.41644182801246643f, 0.10881132632493973f, -0.06598567962646484f, -0.7562199831008911f, -0.09639296680688858f, -0.09091565757989883f, -0.18845929205417633f, -0.1180950403213501f, 0.050943851470947266f, -0.529584527015686f, -0.14369840919971466f, 0.055241700261831284f, -0.7048571109771729f, -0.01918291673064232f, -0.09233684837818146f, -0.33791351318359375f, -0.4570329785346985f, -0.1962839514017105f, -0.6254575252532959f, -0.21465237438678741f, -0.06599828600883484f, -0.5068942308425903f, -0.3697243630886078f,   -0.060344625264406204f,  -0.07949022948741913f, -0.1418696939945221f, -0.08585263043642044f, -0.6355282664299011f, -0.3033415973186493f, -0.05788097530603409f, -0.6313892006874084f, -0.17612089216709137f, -0.13209307193756104f, -0.37335458397865295f, 0.8509643077850342f, 0.27692273259162903f, -0.09154807031154633f, -0.4998394250869751f, 0.02655647136271f, 0.05288087576627731f, 0.5355591773986816f, 0.04596104100346565f, -0.2773580253124237f };
    float[] _handRelaxedRight = new float[] { 0.11167871206998825f,-0.042892176657915115f,0.41644182801246643f,0.10881132632493973f,0.06598567962646484f,0.7562199831008911f,-0.09639296680688858f,0.09091565757989883f,0.18845929205417633f,-0.1180950403213501f,-0.050943851470947266f,0.529584527015686f,-0.14369840919971466f,-0.055241700261831284f,0.7048571109771729f,-0.01918291673064232f,0.09233684837818146f,0.33791351318359375f,-0.4570329785346985f,0.1962839514017105f,0.6254575252532959f,-0.21465237438678741f,0.06599828600883484f,0.5068942308425903f,-0.3697243630886078f,0.060344625264406204f,0.07949022948741913f,-0.1418696939945221f,0.08585263043642044f,0.6355282664299011f,-0.3033415973186493f,0.05788097530603409f,0.6313892006874084f,-0.17612089216709137f,0.13209307193756104f,0.37335458397865295f,0.8509643077850342f,-0.27692273259162903f,0.09154807031154633f,-0.4998394250869751f,-0.02655647136271f,-0.05288087576627731f,0.5355591773986816f,-0.04596104100346565f,0.2773580253124237f };
    float[] _handTestLeft = new float[] { 
        // Left_index
        0.011177415028214455f,  0.2590852677822113f,    0.11648038029670715f, 
       -0.13260921835899353f,   0.04760228097438812f,   0.4385834336280823f, 
        0.10277135670185089f,   0.17290548980236053f,   0.08385777473449707f, 
        // Left_middle
        0.0051428102888166904f, 0.09272782504558563f,   0.13272805511951447f,
        0.06987312436103821f,  -0.04163992777466774f,   0.3943122923374176f, 
       -0.07007572799921036f,   0.06433552503585815f,   0.15722286701202393f,
        // Left_pinky
        0.02963981218636036f,  -0.30420127511024475f,   0.14133693277835846f, 
        0.1646745651960373f,    0.14118221402168274f,   0.14176444709300995f,
        0.017472274601459503f,  0.19599202275276184f,   0.11721484363079071f,
        // Left_ring
        0.055301595479249954f,  0.01798488385975361f,   0.14360643923282623f,
        0.05487954616546631f,   0.01640693098306656f,   0.42330318689346313f,
       -0.030754704028367996f,  0.13594691455364227f,   0.13122965395450592f,
        // Left_thumb
       -0.1604069322347641f,   -0.09846090525388718f,   0.2918240427970886f,
       -0.18725790083408356f,  -0.1291208416223526f,    0.03440938517451286f,
       -0.018986323848366737f, -0.0005881433608010411f, 0.29313862323760986f 
    };
    float[] _handTestRight = new float[] { 0.01326274499297142f, -0.20413629710674286f, -0.15937039256095886f, -0.15078474581241608f, -0.04514279216527939f, -0.48306483030319214f, 0.10417323559522629f, -0.1836467832326889f, -0.10250554978847504f, 0.011679206974804401f, -0.07861433178186417f, -0.18614766001701355f, 0.08135887235403061f, 0.030331499874591827f, -0.43081870675086975f, -0.07722432166337967f, -0.07975051552057266f, -0.1706584393978119f, 0.13209141790866852f, 0.32395777106285095f, -0.3861182630062103f, 0.12642964720726013f, -0.13008570671081543f, -0.15177564322948456f, 0.021906709298491478f, -0.16517646610736847f, 0.02528587356209755f, 0.06796213984489441f, -0.025236478075385094f, -0.15547266602516174f, 0.060269180685281754f, -0.02008354291319847f, -0.4861707091331482f, -0.01259240135550499f, -0.1396889090538025f, -0.17316514253616333f, -0.14130337536334991f, 0.044829096645116806f, -0.2754846215248108f, -0.17891336977481842f, 0.14570343494415283f, -0.05630068853497505f, -0.01855561137199402f, 0.043099239468574524f, -0.1657646745443344f };
    

    Dictionary<string, Transform> _transformFromName;

    // Joint recalculation
    public static Dictionary<string, Matrix[]> JointMatrices = null;


    public void Awake()
    {
        if (_transformFromName == null)
        {
            _transformFromName = new Dictionary<string, Transform>();
            Transform[] transforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                _transformFromName.Add(t.name, t);
            }
        }

        if (_jointPositions == null)
        {
            _jointPositions = new Vector3[NUM_JOINTS];
            for (int i=0; i< NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointPositions[i] = joint.position;
            }
        }

        if (_jointRotations == null)
        {
            _jointRotations = new Quaternion[NUM_JOINTS];
        }

        if (SMPLX.JointMatrices == null)
            InitJointRegressor();

        if (_smr != null)
            return;

        _smr = GetComponentInChildren<SkinnedMeshRenderer>();

        // Get skinned mesh blend shape values
        _numBetaShapes = 0;
        _numExpressions = 0;
        _numPoseCorrectives = 0;

        int blendShapeCount = _smr.sharedMesh.blendShapeCount;
        for (int i=0; i<blendShapeCount; i++)
        {
            string name = _smr.sharedMesh.GetBlendShapeName(i);
            if (name.StartsWith("Shape"))
                _numBetaShapes++;
            else if (name.StartsWith("Exp"))
                _numExpressions++;
            else if (name.StartsWith("Pose"))
                _numPoseCorrectives++;
        }
        
    }

    private bool InitJointRegressor()
    {
        SMPLX.JointMatrices = new Dictionary<string, Matrix[]>();

        // Setup gender specific joint regressors
        string[] genders = new string[] {"female", "neutral", "male"};
        foreach (string gender in genders)
        {
            Debug.Log("[SMPL-X] Setup betas-to-joints regressor: " + gender);
            string name_betas = "betasToJoints_" + gender;
            string name_template = "templateJ_" + gender;

            Matrix[] betasToJoints = new Matrix[3];
            Matrix[] templateJ = new Matrix[3];
            for (int i=0; i<=2; i++)
            {
                betasToJoints[i] = new Matrix(NUM_JOINTS, NUM_BETAS);
                templateJ[i] = new Matrix(NUM_JOINTS, 1);
            }

            // Setup matrix values from JSON resource files
            string name = "smplx_betas_to_joints_" + gender;
            TextAsset ta = Resources.Load<TextAsset>(name);
            if (ta == null)
            {
                Debug.LogError("[SMPL-X] Cannot find betas-to-joint regressor: SMPLX/Resources/" + name);
                return false;
            }
            SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(ta.text);

            for (int i=0; i < NUM_JOINTS; i++)
            {
                // Init beta regressor matrix
                for (int j=0; j< NUM_BETAS; j++)
                {
                    (betasToJoints[0])[i, j] = node["betasJ_regr"][i][0][j].AsDouble;
                    (betasToJoints[1])[i, j] = node["betasJ_regr"][i][1][j].AsDouble;
                    (betasToJoints[2])[i, j] = node["betasJ_regr"][i][2][j].AsDouble;
                }

                // Init joint template matrix
                double x = node["template_J"][i][0].AsDouble;
                double y = node["template_J"][i][1].AsDouble;
                double z = node["template_J"][i][2].AsDouble;

                (templateJ[0])[i, 0] = x;
                (templateJ[1])[i, 0] = y;
                (templateJ[2])[i, 0] = z;
            }

            SMPLX.JointMatrices.Add(name_betas, betasToJoints);
            SMPLX.JointMatrices.Add(name_template, templateJ);
        }
        return true;
    }

    public bool HasBetaShapes()
    {
        return (_numBetaShapes > 0);
    }

    public bool HasExpressions()
    {
        return (_numExpressions > 0);
    }

    public bool HasPoseCorrectives()
    {
        return (_numPoseCorrectives > 0);
    }

    public Vector3[] GetJointPositions()
    {
        return _jointPositions;
    }

    public void SetBetaShapes()
    {
        if (! HasBetaShapes() )
        {
            Debug.LogError("[SMPL-X] ERROR: Cannot set beta shapes on model without beta shapes");
            return;
        }

        _defaultShape = true;
        for (int i=0; i<NUM_BETAS; i++)
        {
            _smr.SetBlendShapeWeight(i, betas[i] * 100); // blend shape weights are specified in percentage

            if (betas[i] != 0.0f)
                _defaultShape = false;
        }

        UpdateJointPositions();
    }

    public void SetExpressions()
    {
        if (! HasExpressions() )
        {
            Debug.LogError("[SMPL-X] ERROR: Cannot set expressions on model without expressions");
            return;
        }

        for (int i=0; i<NUM_EXPRESSIONS; i++)
            _smr.SetBlendShapeWeight(i + NUM_BETAS, expressions[i] * 100); // blend shape weights are specified in percentage
    }

    public void SnapToGroundPlane()
    {
        if (_bakedMesh == null)
            _bakedMesh = new Mesh();

        _smr.BakeMesh(_bakedMesh);
        Vector3[] vertices =_bakedMesh.vertices;
        float yMin = vertices[0].y;
        for (int i=1; i<vertices.Length; i++)
        {
            float y = vertices[i].y;

            if (y < yMin)
                yMin = y;
        }

        Vector3 localPosition = gameObject.transform.localPosition;
        if (Mathf.Abs(yMin) < 0.00001)
            yMin = 0.0f;

        localPosition.y = -yMin;
        gameObject.transform.localPosition = localPosition;

        // Update joint world positions
        UpdateJointPositions(false);

    }

    public void GetModelInfo(out int shapes, out int expressions, out int poseCorrectives)
    {
        shapes = _numBetaShapes;
        expressions = _numExpressions;
        poseCorrectives = _numPoseCorrectives;
    }

    // Return Unity Quaternion for given SMPL-X rodrigues notation
    public static Quaternion QuatFromRodrigues(float rodX, float rodY, float rodZ)
    {
        // Local joint coordinate systems
        //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
        //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
        Vector3 axis = new Vector3(-rodX, rodY, rodZ);
        float angle_deg = - axis.magnitude * Mathf.Rad2Deg;
        Vector3.Normalize(axis);

        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);
        
        return quat;
    }

    public void SetLocalJointRotation(string name, Quaternion quatLocal)
    {
        Transform joint = _transformFromName[name];
        joint.localRotation = quatLocal;
    }

    public void JointRotationSlerp(string name, Quaternion quat1, Quaternion quat2)
    {
        Transform joint = _transformFromName[name];
        joint.localRotation = Quaternion.Slerp(quat1, quat2, Time.deltaTime); 
    }

    public void SetHandPose(HandPose pose)
    {
        float[] left = null;
        float[] right = null;

        if (pose == HandPose.Flat)
        {
            left = _handFlatLeft;
            right = _handFlatRight;
        }
        else if (pose == HandPose.Relaxed)
        {
            left = _handRelaxedLeft;
            right = _handRelaxedRight;
        }
        else if (pose == HandPose.Test)
        {
            left = _handTestLeft;
            right = _handTestRight;
        }
        else if (pose == HandPose.AMASS)
        {
            left = _handAmassLeft;
            right = _handAmassRight;
        }
        else if (pose == HandPose.MANO_mean)
        {
            left = _handMANOLeft;
            right = _handMANORight;
        }

        if ((left != null) && (right != null))
        {
            for (int i=0; i<15; i++)
            {
                string name = _handLeftJointNames[i];
                float rodX = left[i*3 + 0];
                float rodY = left[i*3 + 1];
                float rodZ = left[i*3 + 2];
                Quaternion quat = QuatFromRodrigues(rodX, rodY, rodZ);
                SetLocalJointRotation(name, quat);

                name = _handRightJointNames[i];
                rodX = right[i*3 + 0];
                rodY = right[i*3 + 1];
                rodZ = right[i*3 + 2];
                quat = QuatFromRodrigues(rodX, rodY, rodZ);
                SetLocalJointRotation(name, quat);
            }
        }

        UpdateJointPositions(false);

    }

    public void ResetBodyPose()
    {
        foreach(string name in _bodyJointNames)
        {
            Transform joint = _transformFromName[name];
            joint.localRotation = Quaternion.identity;
        }

        UpdateJointPositions(false);
    }

    public void SetBodyPose(BodyPose pose)
    {
        if (pose == BodyPose.T)
        {
            ResetBodyPose();
        }
        else if (pose == BodyPose.A)
        {
            ResetBodyPose();
            SetLocalJointRotation("left_collar", Quaternion.Euler(0.0f, 0.0f, 10.0f));
            SetLocalJointRotation("left_shoulder", Quaternion.Euler(0.0f, 0.0f, 35.0f));
            SetLocalJointRotation("right_collar", Quaternion.Euler(0.0f, 0.0f, -10.0f));
            SetLocalJointRotation("right_shoulder", Quaternion.Euler(0.0f, 0.0f, -35.0f));
        }
        else if (pose == BodyPose.Test)
        {
            
            is_Coroutine = true;
            StartCoroutine("RotationDelay");

        }
        UpdatePoseCorrectives();
        UpdateJointPositions(false);
    }


    public void EnablePoseCorrectives(bool enabled)
    {
        usePoseCorrectives = enabled;
        if (usePoseCorrectives)
        {
            UpdatePoseCorrectives();
        }
        else
        {
            int blendShapeCount = _smr.sharedMesh.blendShapeCount;
            for (int i=0; i<blendShapeCount; i++)
            {
                string name = _smr.sharedMesh.GetBlendShapeName(i);
                if (name.StartsWith("Pose"))
                    _smr.SetBlendShapeWeight(i, 0.0f);
            }
        }
    }

    public void UpdatePoseCorrectives()
    {
        if (!usePoseCorrectives)
            return;

        if (! HasPoseCorrectives())
            return;

        // Body joint #0 has no pose correctives
        for (int i=1; i<_bodyJointNames.Length; i++)
        {
            string name = _bodyJointNames[i];
            Quaternion quat = _transformFromName[name].localRotation;

            // Local joint coordinate systems
            //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
            //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
            Quaternion quatSMPLX = new Quaternion(-quat.x, quat.y, quat.z, -quat.w);
            Matrix4x4 m = Matrix4x4.Rotate(quatSMPLX);
            // Subtract identity matrix to get proper pose shape weights
            m[0,0] = m[0,0] - 1.0f;
            m[1,1] = m[1,1] - 1.0f;
            m[2,2] = m[2,2] - 1.0f;
            
            // Get corrective pose start index
            int poseStartIndex = NUM_BETAS + NUM_EXPRESSIONS + (i-1)*9;

            _smr.SetBlendShapeWeight(poseStartIndex + 0, 100.0f * m[0,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 1, 100.0f * m[0,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 2, 100.0f * m[0,2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 3, 100.0f * m[1,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 4, 100.0f * m[1,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 5, 100.0f * m[1,2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 6, 100.0f * m[2,0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 7, 100.0f * m[2,1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 8, 100.0f * m[2,2]);
        }
    }

    public bool UpdateJointPositions(bool recalculateJoints = true)
    {
        if (HasBetaShapes() && recalculateJoints)
        {
            if (_sharedMeshDefault == null)
            {
                // Do not clone mesh if we haven't modified the shape parameters yet
                if (_defaultShape)
                    return false;

                // Clone default shared mesh so that we can modify later the shared mesh bind pose without affecting other shared instances.
                // Note that this will drastically increase the Unity scene file size and make Unity Editor very slow on save when multiple bodies like this are used.
                _sharedMeshDefault = _smr.sharedMesh;
                _smr.sharedMesh = (Mesh)Instantiate( _smr.sharedMesh );
                Debug.LogWarning("[SMPL-X] Cloning shared mesh to allow for joint recalculation on beta shape change [" + gameObject.name + "]. Note that this will increase the current scene file size significantly if model contains pose correctives.");
            }

            // Save pose and repose to T-Pose
            for (int i=0; i<NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointRotations[i] = joint.localRotation;
                joint.localRotation = Quaternion.identity;
            }

            // Create beta value matrix
            Matrix betaMatrix = new Matrix(NUM_BETAS, 1);
            for (int row = 0; row < NUM_BETAS; row++)
            {
                betaMatrix[row, 0] = betas[row];
            }

            // Apply joint regressor to beta matrix to calculate new joint positions
            string gender = "";
            if (modelType == SMPLX.ModelType.Female)
                gender = "female";
            else if (modelType == SMPLX.ModelType.Neutral)
                gender = "neutral";
            else if (modelType == SMPLX.ModelType.Male)
                gender = "male";
            else
            {
                Debug.LogError("[SMPL-X] ERROR: Joint regressor needs model type information (Female/Neutral/Male)");
                return false;
            }

            Matrix[] betasToJoints = SMPLX.JointMatrices["betasToJoints_" + gender];
            Matrix[] templateJ = SMPLX.JointMatrices["templateJ_" + gender];;

            Matrix newJointsX = betasToJoints[0] * betaMatrix + templateJ[0];
            Matrix newJointsY = betasToJoints[1] * betaMatrix + templateJ[1];
            Matrix newJointsZ = betasToJoints[2] * betaMatrix + templateJ[2];

            // Update joint position cache
            for (int index = 0; index < NUM_JOINTS; index++)
            {
                Transform joint = _transformFromName[_bodyJointNames[index]];

                // Convert regressor coordinate system (OpenGL) to Unity coordinate system by negating X value
                Vector3 position = new Vector3(-(float)newJointsX[index, 0], (float)newJointsY[index, 0], (float)newJointsZ[index, 0]);

                // Regressor joint positions from joint calculation are centered at origin in world space
                // Transform to game object space for correct world space position
                joint.position = gameObject.transform.TransformPoint(position);
            }

            // Set new bind pose
            Matrix4x4[] bindPoses = _smr.sharedMesh.bindposes;
            Transform[] bones = _smr.bones;
            for (int i=0; i<bones.Length; i++)
            {
                // The bind pose is bone's inverse transformation matrix.
                // Make this matrix relative to the avatar root so that we can move the root game object around freely.
                bindPoses[i] = bones[i].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;
            }
            _smr.sharedMesh.bindposes = bindPoses;

            // Restore pose
            for (int i=0; i<NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                joint.localRotation = _jointRotations[i];

                // Update joint position cache
                _jointPositions[i] = joint.position;

            }
        }
        else
        {
            for (int i=0; i<NUM_JOINTS; i++)
            {
                // Update joint position cache
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointPositions[i] = joint.position;
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!usePoseCorrectives)
            UpdatePoseCorrectives();
   

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            readData readData;
            readData = GetComponent<readData>();


            data = readData.load_data;

            Debug.Log("Read Quaternion data");
        }

    }

    IEnumerator RotationDelay()
    {
        Debug.Log("Motion Start");

        if (is_Coroutine)
        {
            ResetBodyPose();
            for (int i = 0; i < data.Count / 6; i++)
            {
                //SetLocalJointRotation("left_shoulder", Quaternion.Euler(data[i * 6]));
                //SetLocalJointRotation("left_elbow", Quaternion.Euler(data[i * 6 + 1]));
                //SetLocalJointRotation("left_wrist", Quaternion.Euler(data[i * 6 + 2]));
                //SetLocalJointRotation("right_shoulder", Quaternion.Euler(data[i * 6 + 3]));
                //SetLocalJointRotation("right_elbow", Quaternion.Euler(data[i * 6 + 4]));
                //SetLocalJointRotation("right_wrist", Quaternion.Euler(data[i * 6 + 5]));

                SetLocalJointRotation("left_shoulder", data[i * 6]);
                SetLocalJointRotation("left_elbow", data[i * 6 + 1]);
                SetLocalJointRotation("left_wrist", data[i * 6 + 2]);
                SetLocalJointRotation("right_shoulder", data[i * 6 + 3]);
                SetLocalJointRotation("right_elbow", data[i * 6 + 4]);
                SetLocalJointRotation("right_wrist", data[i * 6 + 5]);

                //Debug.Log("left_shouler = " + _transformFromName["left_shouler"].transform.localRotation);

                yield return new WaitForSeconds(0.02f);
            }
            for (int i = data.Count / 6; i > 0 ; i--)
            {

                SetLocalJointRotation("left_shoulder", data[i*6 - 6]);
                SetLocalJointRotation("left_elbow", data[i*6 - 5]);
                SetLocalJointRotation("left_wrist", data[i*6 - 4]);
                SetLocalJointRotation("right_shoulder", data[i*6 - 3]);
                SetLocalJointRotation("right_elbow", data[i*6 - 2]);
                SetLocalJointRotation("right_wrist", data[i*6 - 1]);

                //Debug.Log("left_shouler = " + _transformFromName["left_shouler"].transform.localRotation);

                yield return new WaitForSeconds(0.02f);
            }
            is_Coroutine = false;
        }

        StopCoroutine("RotationDelay");
    }
}
*/



// non-revised copy

///*
// * Copyright (C) 2021 
// * Max-Planck-Gesellschaft zur Förderung der Wissenschaften e.V. (MPG),
// * acting on behalf of its Max Planck Institute for Intelligent Systems and
// * the Max Planck Institute for Biological Cybernetics. All rights reserved.
// *
// * Max-Planck-Gesellschaft zur Förderung der Wissenschaften e.V. (MPG) is
// * holder of all proprietary rights on this computer program. You can only use
// * this computer program if you have closed a license agreement with MPG or
// * you get the right to use the computer program from someone who is authorized
// * to grant you that right.
// * Any use of the computer program without a valid license is prohibited and
// * liable to prosecution.
// *
// * Contact: ps-license@tuebingen.mpg.de
// */

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

// Joint recalculation
//using LightweightMatrixCSharp;

//public class SMPLX : MonoBehaviour
//{
//    public const int NUM_BETAS = 10;
//    public const int NUM_EXPRESSIONS = 10;
//    public const int NUM_JOINTS = 55;

//    public enum ModelType { Unknown, Female, Neutral, Male };
//    public enum HandPose { Flat, Relaxed };
//    public enum BodyPose { T, A };

//    public ModelType modelType = ModelType.Unknown;

//    public float[] betas = new float[NUM_BETAS];
//    public float[] expressions = new float[NUM_EXPRESSIONS];

//    public bool usePoseCorrectives = true;
//    public bool showJointPositions = false;

//    private SkinnedMeshRenderer _smr = null;
//    private Mesh _sharedMeshDefault = null;
//    private bool _defaultShape = true;

//    private int _numBetaShapes;
//    private int _numExpressions;
//    private int _numPoseCorrectives;

//    private Mesh _bakedMesh = null;
//    private Vector3[] _jointPositions = null;
//    private Quaternion[] _jointRotations = null;

//    string[] _bodyJointNames = new string[] { "pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee", "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot", "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow", "left_wrist", "right_wrist", "jaw", "left_eye_smplhf", "right_eye_smplhf", "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3", "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
//    string[] _handLeftJointNames = new string[] { "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3" };
//    string[] _handRightJointNames = new string[] { "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
//    float[] _handFlatLeft = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
//    float[] _handFlatRight = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
//    float[] _handRelaxedLeft = new float[] { 0.11167871206998825f, 0.042892176657915115f, -0.41644182801246643f, 0.10881132632493973f, -0.06598567962646484f, -0.7562199831008911f, -0.09639296680688858f, -0.09091565757989883f, -0.18845929205417633f, -0.1180950403213501f, 0.050943851470947266f, -0.529584527015686f, -0.14369840919971466f, 0.055241700261831284f, -0.7048571109771729f, -0.01918291673064232f, -0.09233684837818146f, -0.33791351318359375f, -0.4570329785346985f, -0.1962839514017105f, -0.6254575252532959f, -0.21465237438678741f, -0.06599828600883484f, -0.5068942308425903f, -0.3697243630886078f, -0.060344625264406204f, -0.07949022948741913f, -0.1418696939945221f, -0.08585263043642044f, -0.6355282664299011f, -0.3033415973186493f, -0.05788097530603409f, -0.6313892006874084f, -0.17612089216709137f, -0.13209307193756104f, -0.37335458397865295f, 0.8509643077850342f, 0.27692273259162903f, -0.09154807031154633f, -0.4998394250869751f, 0.02655647136271f, 0.05288087576627731f, 0.5355591773986816f, 0.04596104100346565f, -0.2773580253124237f };
//    float[] _handRelaxedRight = new float[] { 0.11167871206998825f, -0.042892176657915115f, 0.41644182801246643f, 0.10881132632493973f, 0.06598567962646484f, 0.7562199831008911f, -0.09639296680688858f, 0.09091565757989883f, 0.18845929205417633f, -0.1180950403213501f, -0.050943851470947266f, 0.529584527015686f, -0.14369840919971466f, -0.055241700261831284f, 0.7048571109771729f, -0.01918291673064232f, 0.09233684837818146f, 0.33791351318359375f, -0.4570329785346985f, 0.1962839514017105f, 0.6254575252532959f, -0.21465237438678741f, 0.06599828600883484f, 0.5068942308425903f, -0.3697243630886078f, 0.060344625264406204f, 0.07949022948741913f, -0.1418696939945221f, 0.08585263043642044f, 0.6355282664299011f, -0.3033415973186493f, 0.05788097530603409f, 0.6313892006874084f, -0.17612089216709137f, 0.13209307193756104f, 0.37335458397865295f, 0.8509643077850342f, -0.27692273259162903f, 0.09154807031154633f, -0.4998394250869751f, -0.02655647136271f, -0.05288087576627731f, 0.5355591773986816f, -0.04596104100346565f, 0.2773580253124237f };

//    Dictionary<string, Transform> _transformFromName;

//     Joint recalculation
//    public static Dictionary<string, Matrix[]> JointMatrices = null;

//    public void Awake()
//    {
//        if (_transformFromName == null)
//        {
//            _transformFromName = new Dictionary<string, Transform>();
//            Transform[] transforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
//            foreach (Transform t in transforms)
//            {
//                _transformFromName.Add(t.name, t);
//            }
//        }

//        if (_jointPositions == null)
//        {
//            _jointPositions = new Vector3[NUM_JOINTS];
//            for (int i = 0; i < NUM_JOINTS; i++)
//            {
//                Transform joint = _transformFromName[_bodyJointNames[i]];
//                _jointPositions[i] = joint.position;
//            }
//        }

//        if (_jointRotations == null)
//        {
//            _jointRotations = new Quaternion[NUM_JOINTS];
//        }

//        if (SMPLX.JointMatrices == null)
//            InitJointRegressor();

//        if (_smr != null)
//            return;

//        _smr = GetComponentInChildren<SkinnedMeshRenderer>();

//         Get skinned mesh blend shape values
//        _numBetaShapes = 0;
//        _numExpressions = 0;
//        _numPoseCorrectives = 0;

//        int blendShapeCount = _smr.sharedMesh.blendShapeCount;
//        for (int i = 0; i < blendShapeCount; i++)
//        {
//            string name = _smr.sharedMesh.GetBlendShapeName(i);
//            if (name.StartsWith("Shape"))
//                _numBetaShapes++;
//            else if (name.StartsWith("Exp"))
//                _numExpressions++;
//            else if (name.StartsWith("Pose"))
//                _numPoseCorrectives++;
//        }
//    }

//    private bool InitJointRegressor()
//    {
//        SMPLX.JointMatrices = new Dictionary<string, Matrix[]>();

//         Setup gender specific joint regressors
//        string[] genders = new string[] { "female", "neutral", "male" };
//        foreach (string gender in genders)
//        {
//            Debug.Log("[SMPL-X] Setup betas-to-joints regressor: " + gender);
//            string name_betas = "betasToJoints_" + gender;
//            string name_template = "templateJ_" + gender;

//            Matrix[] betasToJoints = new Matrix[3];
//            Matrix[] templateJ = new Matrix[3];
//            for (int i = 0; i <= 2; i++)
//            {
//                betasToJoints[i] = new Matrix(NUM_JOINTS, NUM_BETAS);
//                templateJ[i] = new Matrix(NUM_JOINTS, 1);
//            }

//             Setup matrix values from JSON resource files
//            string name = "smplx_betas_to_joints_" + gender;
//            TextAsset ta = Resources.Load<TextAsset>(name);
//            if (ta == null)
//            {
//                Debug.LogError("[SMPL-X] Cannot find betas-to-joint regressor: SMPLX/Resources/" + name);
//                return false;
//            }
//            SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(ta.text);

//            for (int i = 0; i < NUM_JOINTS; i++)
//            {
//                 Init beta regressor matrix
//                for (int j = 0; j < NUM_BETAS; j++)
//                {
//                    (betasToJoints[0])[i, j] = node["betasJ_regr"][i][0][j].AsDouble;
//                    (betasToJoints[1])[i, j] = node["betasJ_regr"][i][1][j].AsDouble;
//                    (betasToJoints[2])[i, j] = node["betasJ_regr"][i][2][j].AsDouble;
//                }

//                 Init joint template matrix
//                double x = node["template_J"][i][0].AsDouble;
//                double y = node["template_J"][i][1].AsDouble;
//                double z = node["template_J"][i][2].AsDouble;

//                (templateJ[0])[i, 0] = x;
//                (templateJ[1])[i, 0] = y;
//                (templateJ[2])[i, 0] = z;
//            }

//            SMPLX.JointMatrices.Add(name_betas, betasToJoints);
//            SMPLX.JointMatrices.Add(name_template, templateJ);
//        }
//        return true;
//    }

//    public bool HasBetaShapes()
//    {
//        return (_numBetaShapes > 0);
//    }

//    public bool HasExpressions()
//    {
//        return (_numExpressions > 0);
//    }

//    public bool HasPoseCorrectives()
//    {
//        return (_numPoseCorrectives > 0);
//    }

//    public Vector3[] GetJointPositions()
//    {
//        return _jointPositions;
//    }

//    public void SetBetaShapes()
//    {
//        if (!HasBetaShapes())
//        {
//            Debug.LogError("[SMPL-X] ERROR: Cannot set beta shapes on model without beta shapes");
//            return;
//        }

//        _defaultShape = true;
//        for (int i = 0; i < NUM_BETAS; i++)
//        {
//            _smr.SetBlendShapeWeight(i, betas[i] * 100); // blend shape weights are specified in percentage

//            if (betas[i] != 0.0f)
//                _defaultShape = false;
//        }

//        UpdateJointPositions();
//    }

//    public void SetExpressions()
//    {
//        if (!HasExpressions())
//        {
//            Debug.LogError("[SMPL-X] ERROR: Cannot set expressions on model without expressions");
//            return;
//        }

//        for (int i = 0; i < NUM_EXPRESSIONS; i++)
//            _smr.SetBlendShapeWeight(i + NUM_BETAS, expressions[i] * 100); // blend shape weights are specified in percentage
//    }

//    public void SnapToGroundPlane()
//    {
//        if (_bakedMesh == null)
//            _bakedMesh = new Mesh();

//        _smr.BakeMesh(_bakedMesh);
//        Vector3[] vertices = _bakedMesh.vertices;
//        float yMin = vertices[0].y;
//        for (int i = 1; i < vertices.Length; i++)
//        {
//            float y = vertices[i].y;

//            if (y < yMin)
//                yMin = y;
//        }

//        Vector3 localPosition = gameObject.transform.localPosition;
//        if (Mathf.Abs(yMin) < 0.00001)
//            yMin = 0.0f;

//        localPosition.y = -yMin;
//        gameObject.transform.localPosition = localPosition;

//         Update joint world positions
//        UpdateJointPositions(false);

//    }

//    public void GetModelInfo(out int shapes, out int expressions, out int poseCorrectives)
//    {
//        shapes = _numBetaShapes;
//        expressions = _numExpressions;
//        poseCorrectives = _numPoseCorrectives;
//    }

//     Return Unity Quaternion for given SMPL-X rodrigues notation
//    public static Quaternion QuatFromRodrigues(float rodX, float rodY, float rodZ)
//    {
//         Local joint coordinate systems
//           SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
//           Unity:  X-Left,  Y-Up, Z-Back, Left-handed
//        Vector3 axis = new Vector3(-rodX, rodY, rodZ);
//        float angle_deg = -axis.magnitude * Mathf.Rad2Deg;
//        Vector3.Normalize(axis);

//        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);

//        return quat;
//    }

//    public void SetLocalJointRotation(string name, Quaternion quatLocal)
//    {
//        Transform joint = _transformFromName[name];
//        joint.localRotation = quatLocal;
//    }

//    public void SetHandPose(HandPose pose)
//    {
//        float[] left = null;
//        float[] right = null;

//        if (pose == HandPose.Flat)
//        {
//            left = _handFlatLeft;
//            right = _handFlatRight;
//        }
//        else if (pose == HandPose.Relaxed)
//        {
//            left = _handRelaxedLeft;
//            right = _handRelaxedRight;
//        }

//        if ((left != null) && (right != null))
//        {
//            for (int i = 0; i < 15; i++)
//            {
//                string name = _handLeftJointNames[i];
//                float rodX = left[i * 3 + 0];
//                float rodY = left[i * 3 + 1];
//                float rodZ = left[i * 3 + 2];
//                Quaternion quat = QuatFromRodrigues(rodX, rodY, rodZ);
//                SetLocalJointRotation(name, quat);

//                name = _handRightJointNames[i];
//                rodX = right[i * 3 + 0];
//                rodY = right[i * 3 + 1];
//                rodZ = right[i * 3 + 2];
//                quat = QuatFromRodrigues(rodX, rodY, rodZ);
//                SetLocalJointRotation(name, quat);
//            }
//        }

//        UpdateJointPositions(false);

//    }

//    public void ResetBodyPose()
//    {
//        foreach (string name in _bodyJointNames)
//        {
//            Transform joint = _transformFromName[name];
//            joint.localRotation = Quaternion.identity;
//        }

//        UpdateJointPositions(false);
//    }

//    public void SetBodyPose(BodyPose pose)
//    {
//        if (pose == BodyPose.T)
//        {
//            ResetBodyPose();
//        }
//        else if (pose == BodyPose.A)
//        {
//            ResetBodyPose();
//            SetLocalJointRotation("left_collar", Quaternion.Euler(0.0f, 0.0f, 10.0f));
//            SetLocalJointRotation("left_shoulder", Quaternion.Euler(0.0f, 0.0f, 35.0f));
//            SetLocalJointRotation("right_collar", Quaternion.Euler(0.0f, 0.0f, -10.0f));
//            SetLocalJointRotation("right_shoulder", Quaternion.Euler(0.0f, 0.0f, -35.0f));
//        }
//        UpdatePoseCorrectives();
//        UpdateJointPositions(false);
//    }

//    public void EnablePoseCorrectives(bool enabled)
//    {
//        usePoseCorrectives = enabled;
//        if (usePoseCorrectives)
//        {
//            UpdatePoseCorrectives();
//        }
//        else
//        {
//            int blendShapeCount = _smr.sharedMesh.blendShapeCount;
//            for (int i = 0; i < blendShapeCount; i++)
//            {
//                string name = _smr.sharedMesh.GetBlendShapeName(i);
//                if (name.StartsWith("Pose"))
//                    _smr.SetBlendShapeWeight(i, 0.0f);
//            }
//        }
//    }

//    public void UpdatePoseCorrectives()
//    {
//        if (!usePoseCorrectives)
//            return;

//        if (!HasPoseCorrectives())
//            return;

//         Body joint #0 has no pose correctives
//        for (int i = 1; i < _bodyJointNames.Length; i++)
//        {
//            string name = _bodyJointNames[i];
//            Quaternion quat = _transformFromName[name].localRotation;

//             Local joint coordinate systems
//               Unity:  X-Left,  Y-Up, Z-Back, Left-handed
//               SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
//            Quaternion quatSMPLX = new Quaternion(-quat.x, quat.y, quat.z, -quat.w);
//            Matrix4x4 m = Matrix4x4.Rotate(quatSMPLX);
//             Subtract identity matrix to get proper pose shape weights
//            m[0, 0] = m[0, 0] - 1.0f;
//            m[1, 1] = m[1, 1] - 1.0f;
//            m[2, 2] = m[2, 2] - 1.0f;

//             Get corrective pose start index
//            int poseStartIndex = NUM_BETAS + NUM_EXPRESSIONS + (i - 1) * 9;

//            _smr.SetBlendShapeWeight(poseStartIndex + 0, 100.0f * m[0, 0]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 1, 100.0f * m[0, 1]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 2, 100.0f * m[0, 2]);

//            _smr.SetBlendShapeWeight(poseStartIndex + 3, 100.0f * m[1, 0]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 4, 100.0f * m[1, 1]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 5, 100.0f * m[1, 2]);

//            _smr.SetBlendShapeWeight(poseStartIndex + 6, 100.0f * m[2, 0]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 7, 100.0f * m[2, 1]);
//            _smr.SetBlendShapeWeight(poseStartIndex + 8, 100.0f * m[2, 2]);
//        }
//    }

//    public bool UpdateJointPositions(bool recalculateJoints = true)
//    {
//        if (HasBetaShapes() && recalculateJoints)
//        {
//            if (_sharedMeshDefault == null)
//            {
//                 Do not clone mesh if we haven't modified the shape parameters yet
//                if (_defaultShape)
//                    return false;

//                 Clone default shared mesh so that we can modify later the shared mesh bind pose without affecting other shared instances.
//                 Note that this will drastically increase the Unity scene file size and make Unity Editor very slow on save when multiple bodies like this are used.
//                _sharedMeshDefault = _smr.sharedMesh;
//                _smr.sharedMesh = (Mesh)Instantiate(_smr.sharedMesh);
//                Debug.LogWarning("[SMPL-X] Cloning shared mesh to allow for joint recalculation on beta shape change [" + gameObject.name + "]. Note that this will increase the current scene file size significantly if model contains pose correctives.");
//            }

//             Save pose and repose to T-Pose
//            for (int i = 0; i < NUM_JOINTS; i++)
//            {
//                Transform joint = _transformFromName[_bodyJointNames[i]];
//                _jointRotations[i] = joint.localRotation;
//                joint.localRotation = Quaternion.identity;
//            }

//             Create beta value matrix
//            Matrix betaMatrix = new Matrix(NUM_BETAS, 1);
//            for (int row = 0; row < NUM_BETAS; row++)
//            {
//                betaMatrix[row, 0] = betas[row];
//            }

//             Apply joint regressor to beta matrix to calculate new joint positions
//            string gender = "";
//            if (modelType == SMPLX.ModelType.Female)
//                gender = "female";
//            else if (modelType == SMPLX.ModelType.Neutral)
//                gender = "neutral";
//            else if (modelType == SMPLX.ModelType.Male)
//                gender = "male";
//            else
//            {
//                Debug.LogError("[SMPL-X] ERROR: Joint regressor needs model type information (Female/Neutral/Male)");
//                return false;
//            }

//            Matrix[] betasToJoints = SMPLX.JointMatrices["betasToJoints_" + gender];
//            Matrix[] templateJ = SMPLX.JointMatrices["templateJ_" + gender]; ;

//            Matrix newJointsX = betasToJoints[0] * betaMatrix + templateJ[0];
//            Matrix newJointsY = betasToJoints[1] * betaMatrix + templateJ[1];
//            Matrix newJointsZ = betasToJoints[2] * betaMatrix + templateJ[2];

//             Update joint position cache
//            for (int index = 0; index < NUM_JOINTS; index++)
//            {
//                Transform joint = _transformFromName[_bodyJointNames[index]];

//                 Convert regressor coordinate system (OpenGL) to Unity coordinate system by negating X value
//                Vector3 position = new Vector3(-(float)newJointsX[index, 0], (float)newJointsY[index, 0], (float)newJointsZ[index, 0]);

//                 Regressor joint positions from joint calculation are centered at origin in world space
//                 Transform to game object space for correct world space position
//                joint.position = gameObject.transform.TransformPoint(position);
//            }

//             Set new bind pose
//            Matrix4x4[] bindPoses = _smr.sharedMesh.bindposes;
//            Transform[] bones = _smr.bones;
//            for (int i = 0; i < bones.Length; i++)
//            {
//                 The bind pose is bone's inverse transformation matrix.
//                 Make this matrix relative to the avatar root so that we can move the root game object around freely.
//                bindPoses[i] = bones[i].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;
//            }
//            _smr.sharedMesh.bindposes = bindPoses;

//             Restore pose
//            for (int i = 0; i < NUM_JOINTS; i++)
//            {
//                Transform joint = _transformFromName[_bodyJointNames[i]];
//                joint.localRotation = _jointRotations[i];

//                 Update joint position cache
//                _jointPositions[i] = joint.position;

//            }
//        }
//        else
//        {
//            for (int i = 0; i < NUM_JOINTS; i++)
//            {
//                 Update joint position cache
//                Transform joint = _transformFromName[_bodyJointNames[i]];
//                _jointPositions[i] = joint.position;
//            }
//        }

//        return true;
//    }

//     Update is called once per frame
//    void Update()
//    {
//        if (!usePoseCorrectives)
//            UpdatePoseCorrectives();
//    }
//}

////////////////////////////////////////////////////////////////////////////////
// Custom editor code
////////////////////////////////////////////////////////////////////////////////
//#if UNITY_EDITOR
//[CustomEditor(typeof(SMPLX))]
//public class SMPLX_Editor : Editor
//{

//    private SMPLX _target;
//    private SerializedProperty _modelTypeProperty;
//    private bool _showShape = true;
//    private bool _showExpression = true;
//    private bool _autoSnapToGroundPlane = true;
//    private string _modelInfoText;

//    void Awake()
//    {
//        _target = (SMPLX)target;
//        _target.Awake(); // initialize member values in Editor mode

//        int shapes, expressions, poseCorrectives;
//        _target.GetModelInfo(out shapes, out expressions, out poseCorrectives);
//        _modelInfoText = string.Format("Model: {0} beta shapes, {1} expressions, {2} pose correctives", shapes, expressions, poseCorrectives);
//    }

//    void OnEnable()
//    {
//         Fetch the objects from the GameObject script to display in the inspector
//        _modelTypeProperty = serializedObject.FindProperty("modelType");
//    }

//    public override void OnInspectorGUI()
//    {
//        Undo.RecordObject(_target, _target.name); // allow GUI undo in custom editor
//        Color defaultColor = GUI.backgroundColor;

//        using (new EditorGUILayout.VerticalScope("Box"))
//        {
//             Info
//            EditorGUILayout.HelpBox(_modelInfoText, MessageType.None);

//             Shape
//            if (_target.HasBetaShapes() || _target.HasPoseCorrectives())
//            {
//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    using (new EditorGUILayout.VerticalScope("Box"))
//                    {
//                        GUI.backgroundColor = Color.yellow;
//                        if (GUILayout.Button("Shape"))
//                            _showShape = !_showShape;
//                        GUI.backgroundColor = defaultColor;

//                        if (_target.HasPoseCorrectives())
//                        {
//                            float labelWidth = EditorGUIUtility.labelWidth;
//                            EditorGUIUtility.labelWidth = 200;
//                            bool usePoseCorrectivesNew = EditorGUILayout.Toggle("Use Pose Correctives", _target.usePoseCorrectives);
//                            if (usePoseCorrectivesNew != _target.usePoseCorrectives)
//                            {
//                                if (usePoseCorrectivesNew)
//                                    _target.EnablePoseCorrectives(true);
//                                else
//                                    _target.EnablePoseCorrectives(false);
//                            }
//                            EditorGUIUtility.labelWidth = labelWidth;
//                        }

//                        if (_target.HasBetaShapes())
//                        {
//                            EditorGUILayout.PropertyField(_modelTypeProperty);
//                        }

//                    }
//                    if (_showShape && _target.HasBetaShapes())
//                    {
//                        using (new EditorGUILayout.VerticalScope("Box"))
//                        {
//                            for (int i = 0; i < SMPLX.NUM_BETAS; i++)
//                            {
//                                EditorGUILayout.BeginHorizontal();
//                                EditorGUILayout.LabelField("Beta " + i, GUILayout.Width(50));
//                                _target.betas[i] = EditorGUILayout.Slider(_target.betas[i], -5, 5);
//                                 no effect: GUILayout.FlexibleSpace();
//                                EditorGUILayout.EndHorizontal();
//                            }

//                            float labelWidth = EditorGUIUtility.labelWidth;
//                            EditorGUIUtility.labelWidth = 200;
//                            _autoSnapToGroundPlane = EditorGUILayout.Toggle("Snap Feet To Local Ground Plane", _autoSnapToGroundPlane);
//                            EditorGUIUtility.labelWidth = labelWidth;

//                        }
//                        using (new EditorGUILayout.VerticalScope("Box"))
//                        {
//                            EditorGUILayout.BeginHorizontal();
//                            if (GUILayout.Button("Set"))
//                            {
//                                _target.SetBetaShapes();

//                                if (_autoSnapToGroundPlane)
//                                    _target.SnapToGroundPlane();

//                            }
//                            if (GUILayout.Button("Random"))
//                            {
//                                for (int i = 0; i < SMPLX.NUM_BETAS; i++)
//                                {
//                                    _target.betas[i] = Random.Range(-2.0f, 2.0f);
//                                }
//                                _target.SetBetaShapes();

//                                if (_autoSnapToGroundPlane)
//                                    _target.SnapToGroundPlane();
//                            }
//                            if (GUILayout.Button("Reset"))
//                            {
//                                for (int i = 0; i < SMPLX.NUM_BETAS; i++)
//                                {
//                                    _target.betas[i] = 0.0f;
//                                }
//                                _target.SetBetaShapes();

//                                if (_autoSnapToGroundPlane)
//                                    _target.SnapToGroundPlane();
//                            }
//                            EditorGUILayout.EndHorizontal();
//                        }
//                    }
//                }
//            }

//             Expression
//            if (_target.HasExpressions())
//            {
//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    using (new EditorGUILayout.VerticalScope("Box"))
//                    {
//                        GUI.backgroundColor = Color.yellow;
//                        if (GUILayout.Button("Expression"))
//                            _showExpression = !_showExpression;
//                        GUI.backgroundColor = defaultColor;
//                    }

//                    if (_showExpression)
//                    {
//                        using (new EditorGUILayout.VerticalScope("Box"))
//                        {
//                            for (int i = 0; i < SMPLX.NUM_BETAS; i++)
//                            {
//                                EditorGUILayout.BeginHorizontal();
//                                EditorGUILayout.LabelField("Exp " + i, GUILayout.Width(50));
//                                _target.expressions[i] = EditorGUILayout.Slider(_target.expressions[i], -2, 2);
//                                EditorGUILayout.EndHorizontal();
//                            }
//                        }
//                        using (new EditorGUILayout.VerticalScope("Box"))
//                        {
//                            EditorGUILayout.BeginHorizontal();
//                            if (GUILayout.Button("Set"))
//                            {
//                                _target.SetExpressions();
//                            }
//                            if (GUILayout.Button("Random"))
//                            {
//                                for (int i = 0; i < SMPLX.NUM_EXPRESSIONS; i++)
//                                {
//                                    _target.expressions[i] = Random.Range(-2.0f, 2.0f);
//                                }
//                                _target.SetExpressions();
//                            }
//                            if (GUILayout.Button("Reset"))
//                            {
//                                for (int i = 0; i < SMPLX.NUM_EXPRESSIONS; i++)
//                                {
//                                    _target.expressions[i] = 0.0f;
//                                }
//                                _target.SetExpressions();
//                            }
//                            EditorGUILayout.EndHorizontal();
//                        }
//                    }
//                }
//            }

//             Pose
//            using (new EditorGUILayout.VerticalScope("Box"))
//            {
//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    GUI.backgroundColor = Color.yellow;
//                    GUILayout.Button("Pose");
//                    GUI.backgroundColor = defaultColor;

//                }

//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Body Pose", GUILayout.Width(100));
//                    if (GUILayout.Button("T-Pose"))
//                    {
//                        _target.SetBodyPose(SMPLX.BodyPose.T);
//                    }
//                    if (GUILayout.Button("A-Pose"))
//                    {
//                        _target.SetBodyPose(SMPLX.BodyPose.A);
//                    }
//                    EditorGUILayout.EndHorizontal();

//                    EditorGUILayout.BeginHorizontal();
//                    EditorGUILayout.LabelField("Hand Pose", GUILayout.Width(100));
//                    if (GUILayout.Button("    Flat    "))
//                    {
//                        _target.SetHandPose(SMPLX.HandPose.Flat);
//                    }
//                    if (GUILayout.Button("Relaxed"))
//                    {
//                        _target.SetHandPose(SMPLX.HandPose.Relaxed);
//                    }
//                    EditorGUILayout.EndHorizontal();
//                }
//            }

//             Drawing
//            using (new EditorGUILayout.VerticalScope("Box"))
//            {
//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    GUI.backgroundColor = Color.yellow;
//                    GUILayout.Button("Drawing");
//                    GUI.backgroundColor = defaultColor;
//                }

//                using (new EditorGUILayout.VerticalScope("Box"))
//                {
//                    float labelWidth = EditorGUIUtility.labelWidth;
//                    EditorGUIUtility.labelWidth = 200;
//                    bool showJointPositions = EditorGUILayout.Toggle("Show Joint Positions", _target.showJointPositions);
//                    if (showJointPositions != _target.showJointPositions)
//                    {
//                        if (showJointPositions)
//                            _target.UpdateJointPositions(false);

//                        _target.showJointPositions = showJointPositions;
//                        SceneView.RepaintAll();
//                    }
//                    EditorGUIUtility.labelWidth = labelWidth;
//                }
//            }
//        }

//         Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
//        serializedObject.ApplyModifiedProperties();
//    }

//    public void OnSceneGUI()
//    {
//        if (!_target.showJointPositions)
//            return;

//        Handles.color = Color.yellow;

//        Vector3[] jointPositions = _target.GetJointPositions();
//        foreach (Vector3 pos in jointPositions)
//        {
//            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.025f, EventType.Repaint);
//        }
//    }
//}
//#endif // UNITY_EDITOR
