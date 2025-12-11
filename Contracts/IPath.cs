namespace CarolCustomizer.Contracts;

public interface IPath
{
    PathDescriptor PathDescriptor { get; }
}

public record PathDescriptor
{
    public PathDescriptor(string path, PathType type)
    {
        Path = path;
        Type = type;
    }

    public string Path { get; }
    public PathType Type { get; }
}

public enum PathType
{
    Filesystem = 0,
    Scene = 1,
    Convenience = 2,
}
