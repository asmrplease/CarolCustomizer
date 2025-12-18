using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Accessories;

internal partial class MutableModel
{
    AccessoryDescriptor model;
    bool targetActive = false;
    List<MutableMaterial> materials;
    OutfitCoordinator targetOutfit;

    public MutableModel(AccessoryDescriptor model)
    {
        this.model = model;
        this.materials = model.Materials
            .Select((x, i) => new MutableMaterial(x, model, i))
            .ToList();
        //hook coordinator change to ui target outfit change
        HandleCoordinatorChange(PlayerInstances.DefaultPlayer.outfitManager);
        
        //how do we connect this object to the target armature?
        //also how do the materials get handled?
        //we need a connection to the outfitmanager
        //when the outfit manager changes state,
            //reset this object to default
            //update any objects that are in the current outfit to match the new state
    }

    void HandleCoordinatorChange(OutfitCoordinator coordinator)
    {
        if (this.targetOutfit is not null)
        {
            this.targetOutfit.AccessoryChanged -= HandleAccChange;
        }
        this.targetOutfit = coordinator;
        this.targetOutfit.AccessoryChanged += HandleAccChange;
    }

    void HandleAccChange(AccessoryChangedEvent e)
    {
        if (e.Target != this.model) return;

        this.targetActive = e.Visible;
        e.State.Materials
            .Zip(this.materials, (desc, mut) => (desc, mut))
            .ForEach(tup => tup.mut.SetMaterial(tup.desc));
        this.OnChange?.Invoke();
    }

    void Reset()
    {
        this.targetActive = false;
        this.materials.ForEach(x => x.ResetMaterial());
    }
}


//the idea here is that if this is the only ui element for an accessory,
//we would want to be able to look up the ui element for an accessory
//via a dictionary index with linear time. 
internal partial class MutableModel : IEquatable<AccessoryDescriptor>
{
    public bool Equals(AccessoryDescriptor other)
    {
        return ((IEquatable<AccessoryDescriptor>)model).Equals(other);
    }

    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() == typeof(AccessoryDescriptor)) return Equals((AccessoryDescriptor)other);
        if (other.GetType() == typeof(MutableModel)) return this.model.Equals(other);
        return false;
    }

    public static bool operator ==(MutableModel left, AccessoryDescriptor right) => Equals(left, right);
    public static bool operator !=(MutableModel left, AccessoryDescriptor right) => !Equals(left, right);

    public override int GetHashCode() => model.GetHashCode();
}

internal partial class MutableModel : IUpdateable
{
    public Action OnChange { get; set; }
}

internal partial class MutableModel : IListable
{
    IListable idk => model as IListable;
    public Sprite Thumbnail => idk.Thumbnail;

    public string Header => idk.Header;

    public string Subheader => idk.Subheader;

    public Color BaseColor => Constants.DefaultColor;

    public Color HighlightColor => Constants.Highlight;

    public IEnumerable<IListable> Children => this.materials;

    public bool Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var results = idk.GetContextMenuItems();
        //add any mutable-only operations here.
        return results;
    }
}

internal partial class MutableModel : IToggleable
{
    public UnityAction<bool> OnToggle => (visible) => PlayerInstances.DefaultPlayer.outfitManager.SetAccessory(this.model, visible);
    bool IToggleable.ToggleState => this.targetActive;
}

