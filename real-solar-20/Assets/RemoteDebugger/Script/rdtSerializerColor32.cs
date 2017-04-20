namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerColor32 : rdtSerializerInterface
    {
        public byte a;
        public byte b;
        public byte g;
        public byte r;

        public rdtSerializerColor32(Color32 c)
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

        public Color32 ToUnityType()
        {
            return new Color32(this.r, this.g, this.b, this.a);
        }
    }
}

