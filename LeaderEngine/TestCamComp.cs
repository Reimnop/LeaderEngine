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

        gameObject.transform.position += gameObject.transform.Forward * z + Vector3.Cross(gameObject.transform.Forward, gameObject.transform.Up).Normalized() * x;

        Vector2 delta = Application.instance.MouseState.Delta / 400.0f;

        gameObject.transform.rotationEuler += gameObject.transform.Up * delta.X;
        gameObject.transform.rotationEuler += gameObject.transform.Right * delta.Y;
    }
}
