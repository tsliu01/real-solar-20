namespace Hdg
{
    using System;

    public interface rdtSerializerInterface
    {
        object Deserialize(rdtSerializerRegistry registry);
    }
}

