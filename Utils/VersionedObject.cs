using Newtonsoft.Json;
using System;

namespace CarolCustomizer.Utils;
public class VersionedObject
{
    public string versionString { get; private set; }
    public Version version { get; private set; }

    [JsonConstructor]
    public VersionedObject(string version)
    {
        this.versionString = version;
        this.version = new Version(version);
    }
}
