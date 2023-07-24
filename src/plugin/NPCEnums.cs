using System.Runtime.CompilerServices;

namespace NPCSystem;

public static class NPCEnums
{
    public static readonly ProcessManager.ProcessID NPCPromptMenu = new("NPCPromptMenu", register: true);

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(ActionID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(NPCID).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(AnimationID).TypeHandle);
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