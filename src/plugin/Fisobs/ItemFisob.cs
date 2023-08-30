using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;

namespace NPCSystem.Fisobs;

public class ItemFisob : Fisob
{
    public ItemFisob(Item item) : base(item.AbstractObjectType)
    {
        Icon = new ItemIcon(item);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new AbstractItem(world, ItemRegistry.GetItem(entitySaveData.CustomData), entitySaveData.Pos, entitySaveData.ID);
    }

    public override global::Fisobs.Properties.ItemProperties Properties(PhysicalObject forObject)
    {
        if (forObject is not ItemPO item) return null;

        return new ItemProperties(item.Item);
    }
}