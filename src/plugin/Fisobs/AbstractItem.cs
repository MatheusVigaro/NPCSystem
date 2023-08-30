using Fisobs.Core;

namespace NPCSystem.Fisobs;

public class AbstractItem : AbstractConsumable
{
    public readonly Item Item;

    public AbstractItem(World world, Item item, WorldCoordinate pos, EntityID ID, int originRoom = -1, int placedObjectIndex = -1, PlacedObject.ConsumableObjectData consumableData = null) 
        : base(world, item.AbstractObjectType, null, pos, ID, originRoom, placedObjectIndex, consumableData)
    {
        Item = item;
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject != null) return;

        realizedObject = new ItemPO(this);
    }

    public override string ToString()
    {
        return this.SaveToString(Item.ID.value);
    }
}