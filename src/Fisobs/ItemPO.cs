using System;
using RWCustom;
using UnityEngine;

namespace NPCSystem.Fisobs;

public class ItemPO : PlayerCarryableItem, IPlayerEdible, IDrawable
{
    public int BitesLeft => bites;
    public int FoodPoints => Item.FoodPoints;
    public bool Edible => Item.Edible || Item.FoodPoints > 0;
    public bool AutomaticPickUp => Item.AutomaticPickup;

    public readonly AbstractItem AbstractItem;

    private int bites;
    private float darkness;
    private float lastDarkness;
    private Vector2 rotation;
    private Vector2 lastRotation;
    private int animationTime;
    private FAtlasElement currentSprite;
    private LightSource lightSource;

    private float lastGlimmer;
    private float glimmer;
    private float glimmerProg;
    private float glimmerSpeed;
    private int glimmerWait;

    public Item Item => AbstractItem.Item;
    
    private Animation animation => Item.Animation;

    public ItemPO(AbstractItem abstractItem) : base(abstractItem)
    {
        AbstractItem = abstractItem;
        bites = Item.Bites;
        
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), Item.Radius, Item.Mass);
             
        bodyChunkConnections = Array.Empty<BodyChunkConnection>();

        glimmerProg = 1;

        airFriction = Item.AirFriction;
        gravity = Item.Gravity;
        bounce = Item.Bounce;
        surfaceFriction = Item.SurfaceFriction;
        collisionLayer = Item.CollisionLayer;
        waterFriction = Item.WaterFriction;
        buoyancy = Item.Buoyancy;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        lastRotation = rotation;
        lastGlimmer = glimmer;

        if (Item.Glimmer)
        {
            glimmer = Mathf.Sin(glimmerProg * Mathf.PI) * UnityEngine.Random.value;

            if (glimmerProg < 1f)
            {
                glimmerProg = Mathf.Min(1f, glimmerProg + glimmerSpeed);
            }
            else if (glimmerWait > 0)
            {
                glimmerWait--;
            }
            else
            {
                glimmerWait = UnityEngine.Random.Range(20, 40);
                glimmerProg = 0f;
                glimmerSpeed = 1f / Mathf.Lerp(5f, 15f, UnityEngine.Random.value);
            }
        }

        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = Mathf.Abs(rotation.y);
        }
        
        if (firstChunk.ContactPoint.y < 0)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
        }
        
        if (lightSource != null)
        {
            lightSource.stayAlive = true;
            lightSource.setPos = firstChunk.pos;
            if (lightSource.slatedForDeletetion || (!Item.AlwaysGlow && room.Darkness(firstChunk.pos) == 0f))
            {
                lightSource = null;
            }
        }
        else if (Item.GlowIntensity > 0 && (Item.AlwaysGlow || room.Darkness(firstChunk.pos) > 0f))
        {
            lightSource = new LightSource(firstChunk.pos, environmentalLight: false, Item.GlowColor, this);
            lightSource.requireUpKeep = true;
            lightSource.setRad = Item.GlowIntensity;
            lightSource.setAlpha = 1f;
            room.AddObject(lightSource);
        }

        animationTime++;
        
        AdvanceAnimation();
    }
    
    private void AdvanceAnimation()
    {
        if (animation == null) return;

        if (animationTime < animation.StartupFrames.Count)
        {
            currentSprite = animation.StartupFrames[animationTime].Element;
        }
        else if (animation.Frames.Count > 0)
        {
            currentSprite = animation.Frames[(animationTime - animation.StartupFrames.Count) % animation.Frames.Count].Element;
        }
    }

    public void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        bites--;
        room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1)
        {
            ((Player)grasp.grabber).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public void ThrowByPlayer()
    {
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite(Item.Sprite ?? Futile.atlasManager.GetElementWithName("pixel"))
        {
            scaleX = Item.SpriteScale,
            scaleY = Mathf.Abs(Item.SpriteScale)
        };

        sLeaser.sprites[1] = new FSprite(Item.GlimmerSprite ?? Futile.atlasManager.GetElementWithName("pixel"))
        {
            scaleX = Item.SpriteScale,
            scaleY = Mathf.Abs(Item.SpriteScale),
            isVisible = Item.Glimmer
        };
        
        sLeaser.sprites[2] = new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"],
            isVisible = Item.Glimmer
        };


        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var mainSprite = sLeaser.sprites[0];
        var glimmerSprite = sLeaser.sprites[1];
        var glimmerGlow = sLeaser.sprites[2];
        
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
        var rot = Vector3.Slerp(lastRotation, rotation, timeStacker);

        lastDarkness = darkness;
        darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (darkness != lastDarkness)
        {
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        }

        if (currentSprite != null)
        {
            mainSprite.element = currentSprite;
        }
        
        mainSprite.SetPosition(pos);
        mainSprite.rotation = Custom.VecToDeg(rot);
        mainSprite.color = color;

        if (Item.Glimmer)
        {
            var currentGlimmer = Mathf.Lerp(lastGlimmer, glimmer, timeStacker);

            glimmerSprite.x = mainSprite.x;
            glimmerSprite.y = mainSprite.y;
            glimmerSprite.rotation = mainSprite.rotation;
            glimmerSprite.alpha = Mathf.Lerp(1.3f, 0.5f, darkness) * currentGlimmer;

            //-- 20/16 comes from the original code, 6 is the size of the pearl sprite, we divided by it and multiply by our sprite to get the size for our glimmer
            var currentScale = currentGlimmer * 20f / 16f / 6f;

            glimmerGlow.SetPosition(mainSprite.GetPosition());
            glimmerGlow.rotation = mainSprite.rotation;
            glimmerGlow.scale = Item.SpriteScale * Item.Sprite.sourceRect.width * currentScale;
            glimmerGlow.alpha = currentGlimmer * 0.5f;
        }
    }
    
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Color.Lerp(Color.white, palette.blackColor, darkness);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Items");
        for (var i = 0; i < sLeaser.sprites.Length; i++)
        {
            var sprite = sLeaser.sprites[i];
            sprite.RemoveFromContainer();
            
            //-- Glimmer glow
            if (i == 2)
            {
                rCam.ReturnFContainer("Foreground").AddChild(sprite);
            }
            else
            {
                newContatiner.AddChild(sprite);
            }
        }
    }
}