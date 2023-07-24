using System.Collections.Generic;
using Newtonsoft.Json;

namespace NPCSystem;

[JsonObject(MemberSerialization.OptIn)]
public class Animation
{
    [JsonProperty("id")]
    [JsonConverter(typeof(ExtEnumDeserializer<AnimationID>))]
    public AnimationID ID;

    [JsonProperty("default_duration")]
    public int DefaultDuration = 1;

    [JsonProperty("frames")]
    private string[] _frames = { };

    [JsonProperty("startup_frames")]
    private string[] _startupFrames = { };
    
    public string ModID;

    public List<Frame> Frames = new();
    public List<Frame> StartupFrames = new();

    public string ElementPrefix => Utils.GetAtlasPrefix(ModID);

    public void Init()
    {
        ParseFrames(_startupFrames, StartupFrames);
        ParseFrames(_frames, Frames);
    }

    private void ParseFrames(string[] from, List<Frame> to)
    {
        foreach (var frameData in from)
        {
            if (string.IsNullOrEmpty(frameData)) continue;

            var data = frameData.Split(':');
            var elementName = data[0];

            var duration = data.Length > 1 ? int.Parse(data[1]) : DefaultDuration;
            var element = Futile.atlasManager.GetElementWithName(ElementPrefix + elementName);

            for (var i = 1; i <= duration; i++)
            {
                to.Add(new Frame(element));
            }
        }
    }

    public struct Frame
    {
        public FAtlasElement Element;

        public Frame(FAtlasElement element)
        {
            Element = element;
        }
    }
}