using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Models.Accessories;
public class LiveHair : AccessoryDescriptor
{
    StoredHair referenceHair;

    public LiveHair(StoredHair reference, SkeletonManager skeleton) 
        : base(reference.AssetName, Constants.HairstyleSourceName)
    {
        
    }


}
