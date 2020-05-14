using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{
    public float intensity;
    public float smooth;
    public bool isMine;

    private Quaternion originRotation;

    private void Start()
    {
        originRotation = transform.localRotation;
    }
    private void Update()
    {
        UpdateSway();
    }

    private void UpdateSway()
    {
        //controls
        float xMouse = Input.GetAxis("Mouse X");
        float yMouse = Input.GetAxis("Mouse Y");

        if(!isMine)
        {
            xMouse = 0;
            yMouse = 0;
        }

        //calculate target rotation
        Quaternion xAdj = Quaternion.AngleAxis(-intensity * xMouse, Vector3.up);
        Quaternion yAdj = Quaternion.AngleAxis(intensity * yMouse, Vector3.right);
        Quaternion targetRotation = originRotation * xAdj * yAdj;

        //rotate torwards target location
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
    }
}
