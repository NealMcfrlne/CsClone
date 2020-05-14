using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Look : MonoBehaviourPunCallbacks
{
    public static bool cursorLocked=true;
    public Transform player;
    public Transform cam;
    public Transform weapon;

    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;

    private Quaternion camCenter;
    void Start()
    {
        camCenter = cam.localRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        SetY();
        SetX();
        UpdateLockCursor();
    }

    void SetY()
    {
        float input = Input.GetAxis("Mouse Y")*ySensitivity*Time.fixedDeltaTime;
        //to add a rotations you have to multiply
        Quaternion adj = Quaternion.AngleAxis(input, -Vector3.right);
        Quaternion delta = cam.localRotation * adj;

        if(Quaternion.Angle(camCenter,delta)<maxAngle)
        {
            cam.localRotation = delta;
        }
        weapon.rotation = cam.rotation;
    }

    void SetX()
    {
        float input = Input.GetAxis("Mouse X") * xSensitivity * Time.fixedDeltaTime;
        //to add a rotations you have to multiply
        Quaternion adj = Quaternion.AngleAxis(input, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;
    }

    void UpdateLockCursor()
    {
        if(cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = true;
            }
        }
    }
}
