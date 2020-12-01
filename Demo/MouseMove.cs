using System;
using System.Collections.Generic;
using System.Text;
using LeaderEngine;
using OpenTK.Mathematics;

namespace Demo
{
    public class MouseMove : Component
    {
        public override void Start()
        {
            Application.instance.CursorGrabbed = true;
        }

        public override void Update()
        {
            float x = InputManager.GetAxis(Axis.Horizontal) * Time.deltaTime * 4.0f;
            float z = InputManager.GetAxis(Axis.Vertical) * Time.deltaTime * 4.0f;

            Vector2 delta = Application.instance.MouseState.Delta / 550.0f;

            gameObject.transform.rotationEuler -= new Vector3(delta.Y, delta.X, 0.0f);
            gameObject.transform.position += x * gameObject.transform.right + z * gameObject.transform.forward;
        }
    }
}
