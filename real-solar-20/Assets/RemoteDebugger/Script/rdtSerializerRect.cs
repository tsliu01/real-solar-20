namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerRect : rdtSerializerInterface
    {
        private float height;
        private float width;
        private float x;
        private float y;

        public rdtSerializerRect(Rect r)
        {
            this.x = r.x;
            this.y = r.y;
            this.width = r.width;
            this.height = r.height;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Rect ToUnityType()
        {
            return new Rect(this.x, this.y, this.width, this.height);
        }
    }
}

