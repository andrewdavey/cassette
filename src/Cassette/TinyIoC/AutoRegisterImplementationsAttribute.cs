using System;

namespace Cassette.TinyIoC
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class AutoRegisterImplementationsAttribute : Attribute
    {
    }
}