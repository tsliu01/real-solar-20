namespace Hdg
{
    using System;

    [Serializable]
    public class rdtSerializerSlider : rdtSerializerInterface
    {
        public float LimitMax;
        public float LimitMin;
        public float Value;

        public rdtSerializerSlider(float invalue, float inmin, float inmax)
        {
            this.Value = invalue;
            this.LimitMin = inmin;
            this.LimitMax = inmax;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this;
        }

        public bool Equals(rdtSerializerSlider p)
        {
            if (p == null)
            {
                return false;
            }
            return (((this.Value == p.Value) && (this.LimitMin == p.LimitMin)) && (this.LimitMax == p.LimitMax));
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as rdtSerializerSlider);
        }

        public override int GetHashCode()
        {
            return ((this.Value.GetHashCode() ^ this.LimitMin.GetHashCode()) ^ this.LimitMax.GetHashCode());
        }
    }
}

