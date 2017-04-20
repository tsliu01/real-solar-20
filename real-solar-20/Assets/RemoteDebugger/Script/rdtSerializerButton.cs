namespace Hdg
{
    using System;

    [Serializable]
    public class rdtSerializerButton : rdtSerializerInterface
    {
        public bool Pressed;

        public rdtSerializerButton(bool inpressed)
        {
            this.Pressed = inpressed;
        }

        public object Deserialize(rdtSerializerRegistry registry)
        {
            return this;
        }

        public bool Equals(rdtSerializerButton p)
        {
            if (p == null)
            {
                return false;
            }
            return (this.Pressed == p.Pressed);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as rdtSerializerButton);
        }

        public override int GetHashCode()
        {
            return this.Pressed.GetHashCode();
        }
    }
}

