using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class XRoadServiceAttribute : Attribute
{
    public string Service { get; }

    public XRoadServiceAttribute(string service)
    {
        Service = service;
    }
}

