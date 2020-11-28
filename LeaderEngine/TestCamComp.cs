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

        Vector3 move = gameObject.transform.forward * z + gameObject.transform.right * x;

        gameObject.transform.position.X += move.X;
        gameObject.transform.position.Z += move.Z;

        if (InputManager.GetKey(Keys.Space))
        {
            gameObject.transform.position.Y += Time.deltaTime * 2.5f;
        }
        if (InputManager.GetKey(Keys.LeftShift))
        {
            gameObject.transform.position.Y -= Time.deltaTime * 2.5f;
        }

        Vector2 delta = Application.instance.MouseState.Delta * Time.deltaTime * 4.0f;

        gameObject.transform.rotationEuler.Y -= delta.X;
        gameObject.transform.rotationEuler.X -= delta.Y;
    }
}
