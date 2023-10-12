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

    [JsonProperty("voice")]
    public string _voice;

    [JsonProperty("voice_pitch_min")]
    public float VoicePitchMin = 1;

    [JsonProperty("voice_pitch_max")]
    public float VoicePitchMax = 1;

    public AnimationID IdleAnimation;
    public string ModID;
    public SoundID Voice;

    public void Init()
    {
        IdleAnimation = new AnimationID(_idleAnimation);
        
        if (!string.IsNullOrEmpty(_voice))
        {
            Voice = SoundRegistry.GetSound(_voice, ModID);

            if (Voice == null)
            {
                ExtEnumBase.TryParse(typeof(SoundID), _voice, true, out var result);
                Voice = result as SoundID;
            }
        }
    }
}