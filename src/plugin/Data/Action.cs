using System.Collections.Generic;
using Newtonsoft.Json;

namespace NPCSystem;

[JsonObject(MemberSerialization.OptIn)]
public class Action
{
    [JsonProperty("id")]
    [JsonConverter(typeof(ExtEnumDeserializer<ActionID>))]
    public ActionID ID;

    [JsonProperty("script")]
    public Node Script;

    public string ModID;

    public void Init()
    {
    }

    public class Node
    {
        public string Type;
        public string Input;
        public int Duration = 40;
        public Node Next;
        public Dictionary<string, Node> Options = new();
    }

    public static class NodeType
    {
        public const string SetTempValue = "settempvalue";
        public const string GetTempValue = "gettempvalue";
        public const string SetValue = "setvalue";
        public const string GetValue = "getvalue";
        public const string RNG = "rng";
        public const string CheckObject = "checkobject";
        public const string ConsumeObject = "consumeobject";
        public const string SpawnObject = "spawnobject";
        public const string Move = "move";
        public const string Idle = "idle";
        public const string Action = "action";
        public const string Music = "music";
        public const string Sound = "sound";
        public const string Text = "text";
        public const string Prompt = "prompt";
        public const string Animation = "animation";
    }
}