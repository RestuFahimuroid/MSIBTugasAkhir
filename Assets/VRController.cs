using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Valve.VR;

public class VRController : MonoBehaviour
{
    public float m_Gravity = 30.0f;
    public float m_Sensitivity = 0.1f;
    public float m_MaxSpeed = 0.1f;
    public float m_RotateIncrement = 90;
    public SteamVR_Action_Boolean m_RotatePress = null;
    public SteamVR_Action_Boolean m_MovePress = null;
    public SteamVR_Action_Vector2 m_MoveValue = null;

    private float m_speed = 0.0f;
    private CharacterController m_CharacterController = null;
    private Transform m_CameraRig = null;
    private Transform m_Head = null;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();

    }

    // Start is called before the first frame update
    void Start()
    {
        m_CameraRig = SteamVR_Render.Top().origin;
        m_Head = SteamVR_Render.Top().head;
    }

    // Update is called once per frame
    void Update()
    {
        //HandleHead();
        HandleHeight();
        CalculateMovement();
        snapRotation();

    }

    private void HandleHead()
    {
        //Store Current
        Vector3 oldPosition = m_CameraRig.position;
        Quaternion oldRotation = m_CameraRig.rotation;

        //Rotation
        transform.eulerAngles = new Vector3(0.0f, m_Head.rotation.eulerAngles.y, 0.0f);

        //Restore
        m_CameraRig.position = oldPosition;
        m_CameraRig.rotation = oldRotation;

    }

    private void HandleHeight()
    {
        // Get the Head in local Space
        float headHeight = Mathf.Clamp(m_Head.localPosition.y, 1, 2);
        m_CharacterController.height = headHeight;

        //Cut in Half
        Vector3 newCenter = Vector3.zero;
        newCenter.y = m_CharacterController.height / 2;
        newCenter.y += m_CharacterController.skinWidth;

        //Move capsule in local space
        newCenter.x = m_Head.localPosition.x;
        newCenter.z = m_Head.localPosition.z;

        //Rotation
        // newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0)* newCenter;


        //Apply
        m_CharacterController.center = newCenter;

    }

    private void CalculateMovement()
    {
        //Figure out Movement orientation
        // Vector3 orientationEuler = new Vector3(0.0f, m_Head.eulerAngles.y, 0.0f);
        Quaternion orientation = CalculateOrientation();
        Vector3 movement = Vector3.zero;

        //if not moving
        if (m_MoveValue.axis.magnitude == 0 )
        {
            m_speed = 0;
        }

        //Add,clamp
        m_speed += m_MoveValue.axis.magnitude * m_Sensitivity;
        m_speed = Mathf.Clamp(m_speed, -m_MaxSpeed, m_MaxSpeed);

        //Orientation and Gravity
        movement += orientation * (m_speed * Vector3.forward);
        movement.y = m_Gravity * Time.deltaTime;

        //Apply
        m_CharacterController.Move(movement * Time.deltaTime);
    }

    private void snapRotation()
    {
        float snapValue = 0.0f;

        if (m_RotatePress.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            snapValue = -MathF.Abs(m_RotateIncrement);
        }
        if (m_RotatePress.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            snapValue = MathF.Abs(m_RotateIncrement);
        }

        transform.RotateAround(m_Head.position, Vector3.up, snapValue);
    }

    private Quaternion CalculateOrientation()
    {
        float rotation =  MathF.Atan2(m_MoveValue.axis.x, m_MoveValue.axis.y);
        rotation *= Mathf.Rad2Deg;

        Vector3 orientationEuler = new Vector3(0.0f, m_Head.eulerAngles.y, 0.0f);
        return Quaternion.Euler(orientationEuler);
    }

}
