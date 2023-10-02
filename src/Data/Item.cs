using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace NPCSystem;

[JsonObject(MemberSerialization.OptIn)]
public class Item
{
    [JsonProperty("id")]
    [JsonConverter(typeof(ExtEnumDeserializer<ItemID>))]
    public ItemID ID;

    [JsonProperty("sprite")]
    private string _sprite;

    [JsonProperty("animation")]
    [JsonConverter(typeof(ExtEnumDeserializer<AnimationID>))]
    private AnimationID _animation;

    [JsonProperty("sprite_scale")]
    public float SpriteScale = 1;
    
    [JsonProperty("icon_sprite")]
    public string IconSprite;

    [JsonProperty("icon_color")]
    [JsonConverter(typeof(ColorDeserializer))]
    public Color IconColor = Color.white;

    [JsonProperty("glow_color")]
    [JsonConverter(typeof(ColorDeserializer))]
    public Color GlowColor = Color.white;

    [JsonProperty("glow_intensity")]
    public float GlowIntensity;

    [JsonProperty("always_glow")]
    public bool AlwaysGlow;

    [JsonProperty("throwable")]
    public bool Throwable;

    [JsonProperty("automatic_pickup")]
    public bool AutomaticPickup;
    
    [JsonProperty("edible")]
    public bool Edible;

    [JsonProperty("food_points")]
    public int FoodPoints;

    [JsonProperty("scav_score")]
    public int ScavScore;

    [JsonProperty("grabability")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Player.ObjectGrabability Grabability = Player.ObjectGrabability.OneHand;

    [JsonProperty("bites")]
    public int Bites;

    [JsonProperty("radius")]
    public float Radius = 10f;

    [JsonProperty("mass")]
    public float Mass = 0.05f;

    [JsonProperty("air_friction")]
    public float AirFriction = 0.93f;

    [JsonProperty("gravity")]
    public float Gravity = 0.6f;

    [JsonProperty("bounce")]
    public float Bounce = 0.2f;

    [JsonProperty("surface_friction")]
    public float SurfaceFriction = 0.7f;

    [JsonProperty("water_friction")]
    public float WaterFriction = 0.95f;

    [JsonProperty("buoyancy")]
    public float Buoyancy = 0.9f;

    [JsonProperty("collision_layer")]
    public int CollisionLayer = 2;

    public Animation Animation;
    public FAtlasElement Sprite;
    public AbstractPhysicalObject.AbstractObjectType AbstractObjectType;
    public string ModID;

    public string ElementPrefix => Utils.GetAtlasPrefix(ModID);
    public string AbstractObjectTypePrefix => Utils.GetItemPrefix(ModID);
    
    public void Init()
    {
        if (!string.IsNullOrEmpty(_sprite))
        {
            Sprite = Futile.atlasManager.GetElementWithName(ElementPrefix + _sprite);
        }

        if (_animation != null)
        {
            Animation = AnimationRegistry.GetAnimation(_animation);
        }

        AbstractObjectType = new AbstractPhysicalObject.AbstractObjectType(AbstractObjectTypePrefix + ID.value, true);
    }
}