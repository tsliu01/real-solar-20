namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerVector4 : rdtSerializerInterface
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public rdtSerializerVector4(Vector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Vector4 ToUnityType()
        {
            return new Vector4(this.x, this.y, this.z, this.w);
        }
    }
}

