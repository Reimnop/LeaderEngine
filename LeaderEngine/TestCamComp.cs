using System;
using System.Collections.Generic;
using System.Text;
using LeaderEngine;

public class TestCamComp : Component
{
    public override void Update()
    {
        float x = InputManager.GetAxis(Axis.Horizontal);
        float y = InputManager.GetAxis(Axis.Vertical);

        Camera.main.gameObject.transform.position.X -= x * Time.deltaTime * 4.0f;
        Camera.main.gameObject.transform.position.Z += y * Time.deltaTime * 4.0f;
    }
}
