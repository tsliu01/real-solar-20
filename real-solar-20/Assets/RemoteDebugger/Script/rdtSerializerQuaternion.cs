namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerQuaternion : rdtSerializerInterface
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public rdtSerializerQuaternion(Quaternion v)
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

        public object ToUnityType()
        {
            return new Quaternion(this.x, this.y, this.z, this.w);
        }
    }
}

