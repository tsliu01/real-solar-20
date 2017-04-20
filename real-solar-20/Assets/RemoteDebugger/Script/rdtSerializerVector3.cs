namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerVector3 : rdtSerializerInterface
    {
        public float x;
        public float y;
        public float z;

        public rdtSerializerVector3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Vector3 ToUnityType()
        {
            return new Vector3(this.x, this.y, this.z);
        }
    }
}

