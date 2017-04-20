namespace Hdg
{
    using System;
    using UnityEngine;

    [Serializable]
    public class rdtSerializerMatrix4x4 : rdtSerializerInterface
    {
        private rdtSerializerVector4 col0;
        private rdtSerializerVector4 col1;
        private rdtSerializerVector4 col2;
        private rdtSerializerVector4 col3;

        public rdtSerializerMatrix4x4(Matrix4x4 m)
        {
            this.col0 = new rdtSerializerVector4(m.GetColumn(0));
            this.col1 = new rdtSerializerVector4(m.GetColumn(1));
            this.col2 = new rdtSerializerVector4(m.GetColumn(2));
            this.col3 = new rdtSerializerVector4(m.GetColumn(3));
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this.ToUnityType();
        }

        public Matrix4x4 ToUnityType()
        {
            Matrix4x4 matrixx = new Matrix4x4();
            matrixx.SetColumn(0, this.col0.ToUnityType());
            matrixx.SetColumn(1, this.col1.ToUnityType());
            matrixx.SetColumn(2, this.col2.ToUnityType());
            matrixx.SetColumn(3, this.col3.ToUnityType());
            return matrixx;
        }
    }
}

