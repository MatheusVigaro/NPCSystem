using NPCSystem.Fisobs;

namespace NPCSystem.DevTools;

public class CustomItemData : Pom.Pom.ManagedData
{
    public ItemID Item => GetValue<ItemID>(nameof(Item));

    [Pom.Pom.IntegerField(nameof(minCycles), 0, 9, 0, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Min Cycles")]
    public int minCycles;

    [Pom.Pom.IntegerField(nameof(maxCycles), 0, 9, 0, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Max Cycles")]
    public int maxCycles;
    
    public CustomItemData(PlacedObject owner) : base(owner, new Pom.Pom.ManagedField[]
    {
        new Pom.Pom.ExtEnumField<ItemID>(nameof(Item), ItemID.Example, displayName: nameof(Item))
    })
    {
    }
}

public class CustomItemObjectSpawner : UpdatableAndDeletable
{
    private PlacedObject placedObject;
    private CustomItemData data;

    public CustomItemObjectSpawner(PlacedObject placedObject, Room room)
    {
        this.placedObject = placedObject;
        data = (placedObject.data as CustomItemData)!;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (slatedForDeletetion) return;

        var placedObjectIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        
        if (room.game.session is StoryGameSession session && session.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, placedObjectIndex)) return;

        var consumableData = new PlacedObject.ConsumableObjectData(placedObject)
        {
            minRegen = data.minCycles,
            maxRegen = data.maxCycles
        };

        var obj = new AbstractItem(room.world, ItemRegistry.GetItem(data.Item), room.GetWorldCoordinate(placedObject.pos), new EntityID(), room.abstractRoom.index, placedObjectIndex, consumableData);
        ObjectSpawner.AddToRoom(obj);
        obj.isConsumed = false;
        obj.Consume();

        Destroy();
    }
}