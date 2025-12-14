using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Contracts;
public interface IAssetLoadResponder<T>
{
    void OnAssetLoaded(T asset);
    void OnAssetUnloaded(T asset);
}

public interface ISourceLoadResponder<T> where T : ISourceDescriptor
{
    void OnSourceLoaded(T source);
    void OnSourceUnloaded(SourceDescriptor descriptor);
}

//there are some assets that can be dynamically loaded and unloaded
//if an object is given one of those assets, and the asset unloads,
//the object must remove any references its holding

//this is similar to a source awaiter, where a scene will take some time to load a material,
//and an object requests a callback when the asset is available. 

//we have several objects that handle recipe or outfit loading
//each individual recipe wants to respond to a source load/unload
//the list of recipes wants to respond to a recipe load/unload