using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AXeDebugger : MonoBehaviour
{
    [SerializeField] bool droneAxeDebuggerOn;
    [SerializeField] bool isMouseUse;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (droneAxeDebuggerOn)
        {
            DroneAXeDebuger();
        }

        if (isMouseUse)
        {
            // Mouse Lock

            Cursor.lockState = CursorLockMode.None;

            // Cursor visible

            Cursor.visible = true;
        }
        else
        {
            // Mouse Lock

            Cursor.lockState = CursorLockMode.Locked;

            // Cursor visible

            Cursor.visible = false;
        }
    }


    /// <summary>
    /// AXe 컨트롤러 스틱 수치 조정용
    /// </summary>
    public void DroneAXeDebuger()
    {
        print("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
        print("Throttle : " + InputDeviceChecker.instance.Throttle());
        print("Yaw : " + InputDeviceChecker.instance.Yaw());
        print("Pitch : " + InputDeviceChecker.instance.Pitch());
        print("Roll : " + InputDeviceChecker.instance.Roll());
        print("FlightMode : " + Input.GetAxis("FlightMode"));
        
        print("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
    }
}
