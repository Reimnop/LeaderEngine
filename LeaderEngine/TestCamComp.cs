using System;
using System.Collections.Generic;
using System.Text;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TestCamComp : Component
{
    public override void Start()
    {
        Application.instance.CursorGrabbed = true;
    }

    public override void Update()
    {
        float x = InputManager.GetAxis(Axis.Horizontal) * Time.deltaTime * 4.0f;
        float z = InputManager.GetAxis(Axis.Vertical) * Time.deltaTime * 4.0f;

        Vector3 right = Quaternion.FromEulerAngles(gameObject.transform.rotationEuler) * new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 forward = Quaternion.FromEulerAngles(gameObject.transform.rotationEuler) * new Vector3(0.0f, 0.0f, 1.0f);

        gameObject.transform.position += forward * z - right * x;

        Vector2 delta = Application.instance.MouseState.Delta / 400.0f;

        gameObject.transform.rotationEuler.Y -= delta.X;
        //gameObject.transform.rotationEuler.X += delta.Y;
    }
}
