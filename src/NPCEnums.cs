using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;

namespace NPCSystem;

public static class NPCEnums
{
    public static readonly ProcessManager.ProcessID NPCPromptMenu = new("NPCPromptMenu", register: true);

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(ActionID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(NPCID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(AnimationID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ItemID).TypeHandle);

        foreach (var shader in Custom.rainWorld.Shaders.Keys.OrderBy(x => x))
        {
            _ = new ShaderID(shader, true);
        } 
    }
}

public class ActionID : ExtEnum<ActionID>
{
    public static readonly ActionID Example_Talk = new(nameof(Example_Talk), true);

    public ActionID(string value, bool register = false) : base(value, register)
    {
    }
}

public class NPCID : ExtEnum<NPCID>
{
    public static readonly NPCID Example = new(nameof(Example), true);

    public NPCID(string value, bool register = false) : base(value, register)
    {
    }
}

public class AnimationID : ExtEnum<AnimationID>
{
    public static readonly AnimationID Example_Idle = new(nameof(Example_Idle), true);
   
    public AnimationID(string value, bool register = false) : base(value, register)
    {
    }
}

public class ItemID : ExtEnum<ItemID>
{
    public static readonly ItemID Example = new(nameof(Example), true);

    public ItemID(string value, bool register = false) : base(value, register)
    {
    }
}

public class ShaderID : ExtEnum<ShaderID>
{
    public ShaderID(string value, bool register = false) : base(value, register)
    {
    }
}