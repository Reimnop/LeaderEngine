using LeaderEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEditor.Logic.Memes
{
    public class LEADER_SPINNNN : Component
    {
        public float power = 1.0f;

        public override void Update()
        {
            gameObject.transform.rotationEuler.X += Time.deltaTime * power;
            gameObject.transform.rotationEuler.Y -= Time.deltaTime * power * 0.8f;
            gameObject.transform.rotationEuler.Z += Time.deltaTime * power * 1.4f;
        }
    }
}
