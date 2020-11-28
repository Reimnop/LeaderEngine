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
        float x = InputManager.GetAxis(Axis.Horizontal) * Time.deltaTime * 2.5f;
        float z = InputManager.GetAxis(Axis.Vertical) * Time.deltaTime * 2.5f;

        gameObject.transform.position += gameObject.transform.forward * z + gameObject.transform.right * x;

        Vector2 delta = Application.instance.MouseState.Delta * Time.deltaTime * 4.0f;

        gameObject.transform.rotationEuler.Y -= delta.X;
        //gameObject.transform.rotationEuler.X -= delta.Y;
    }
}
