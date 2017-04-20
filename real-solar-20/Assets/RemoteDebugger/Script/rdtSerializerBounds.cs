namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerBounds : rdtSerializerInterface
    {
        private rdtSerializerVector3 centre;
        private rdtSerializerVector3 size;

        public rdtSerializerBounds(Bounds b)
        {
            this.centre = new rdtSerializerVector3(b.center);
            this.size = new rdtSerializerVector3(b.size);
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Bounds ToUnityType()
        {
            return new Bounds(this.centre.ToUnityType(), this.size.ToUnityType());
        }
    }
}

