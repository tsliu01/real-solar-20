namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerVector2 : rdtSerializerInterface
    {
        public float x;
        public float y;

        public rdtSerializerVector2(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Vector2 ToUnityType()
        {
            return new Vector2(this.x, this.y);
        }
    }
}

