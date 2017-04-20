namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerColor : rdtSerializerInterface
    {
        public float a;
        public float b;
        public float g;
        public float r;

        public rdtSerializerColor(Color c)
        {
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
            this.a = c.a;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Color ToUnityType()
        {
            return new Color(this.r, this.g, this.b, this.a);
        }
    }
}

