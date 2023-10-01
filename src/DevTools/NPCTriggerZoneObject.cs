using System.Linq;
using UnityEngine;

namespace NPCSystem.DevTools;

public class NPCTriggerZoneData : Pom.Pom.ManagedData
{
    public NPCID NPC => GetValue<NPCID>("NPC");

    [Pom.Pom.Vector2Field("size", 100, 100, Pom.Pom.Vector2Field.VectorReprType.rect)]
    public Vector2 size;

    [Pom.Pom.BooleanField("repeat", false, displayName: "Repeat?")]
    public bool repeat;

    [Pom.Pom.BooleanField("oncePerCycle", false, displayName: "Once Per Cycle?")]
    public bool oncePerCycle;

    [Pom.Pom.StringField("action", "", "Action")]
    public string action;

    public NPCTriggerZoneData(PlacedObject owner) : base(owner, new []
    {
        new Pom.Pom.ExtEnumField<NPCID>("NPC", NPCID.Example, displayName: "NPC")
    })
    {
    }
}

public class NPCTriggerZoneObject : UpdatableAndDeletable
{
    public NPC NPC;
    public NPCObject npcObject;

    private PlacedObject placedObject;
    private NPCTriggerZoneData data;
    private Vector2 pos => placedObject.pos;

    public bool triggered;
    
    public NPCTriggerZoneObject(PlacedObject placedObject, Room room)
    {
        this.placedObject = placedObject;
        this.room = room;
        data = (placedObject.data as NPCTriggerZoneData)!;
        NPC = NPCRegistry.GetNPC(data.NPC);
    }

    public override void Update(bool eu)
    {
        if (!data.oncePerCycle && room.PlayersInRoom.Count == 0)
        {
            triggered = false;
        }

        if (triggered && !data.repeat) return;
        
        if (npcObject == null)
        {
            npcObject = room.updateList.FirstOrDefault(x => x is NPCObject obj && obj.NPC == NPC) as NPCObject;
        }

        if (npcObject == null) return;

        if ((ActionRegistry.GetAction(data.action)?.Priority ?? 0) < npcObject.CurrentPriority) return;

        var startPos = pos;
        if (data.size.x < 0)
        {
            data.size.x = -data.size.x;
            startPos.x -= data.size.x;
        }
        if (data.size.y < 0)
        {
            data.size.y = -data.size.y;
            startPos.y -= data.size.y;
        }

        var wasTriggered = triggered;
        triggered = false;
        
        var affectedRect = new Rect(startPos, data.size);
        foreach (var player in room.PlayersInRoom)
        {
            if (player != null && player.room == room && affectedRect.Contains(player.mainBodyChunk.pos))
            {
                triggered = true;
                break;
            }
        }

        if (!wasTriggered && triggered)
        {
            npcObject.SetAction(data.action);
        }
    }
}