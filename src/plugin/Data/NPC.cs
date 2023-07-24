using Newtonsoft.Json;

namespace NPCSystem;

[JsonObject(MemberSerialization.OptIn)]
public class NPC
{
    [JsonProperty("id")]
    [JsonConverter(typeof(ExtEnumDeserializer<NPCID>))]
    public NPCID ID;

    [JsonProperty("idle_animation")]
    private string _idleAnimation;

    [JsonProperty("container")]
    public string Container;

    public AnimationID IdleAnimation;
    public string ModID;

    public void Init()
    {
        IdleAnimation = new AnimationID(_idleAnimation);
    }
}