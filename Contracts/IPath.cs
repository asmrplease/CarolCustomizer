namespace CarolCustomizer.Contracts;

internal interface IPath
{
    string Path { get; }
    PathType Type { get; }
}

internal enum PathType
{
    Filesystem = 0,
    Scene = 1,
    Convenience = 2,
}
