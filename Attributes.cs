using System;

namespace ReflectionSingleton
{
    /// <summary>
    /// Injectable attribute is required to add class to the Singleton container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Injectable : Attribute { }


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectableMono : Attribute { }

    /// <summary>
    /// Injectable MonoBehaviour attribute is required to fill MonoBehaviour class by Singleton container instances
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectIntoMono : Attribute { }

    /// <summary>
    /// Singleton attribute is needed to get a class from a Singleton container
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Singleton : Attribute { }
}