using System;
using RWCustom;
using UnityEngine;

namespace NPCSystem.Fisobs;

public class ItemPO : PlayerCarryableItem, IPlayerEdible, IDrawable
{
    public int BitesLeft => bites;
    public int FoodPoints => Item.FoodPoints;
    public bool Edible => Item.FoodPoints > 0;
    public bool AutomaticPickUp => Item.AutomaticPickup;

    public readonly AbstractItem AbstractItem;

    private int bites;
    private float darkness;
    private float lastDarkness;
    private Vector2 rotation;
    private Vector2 lastRotation;
    private int animationTime;
    private FAtlasElement currentSprite;

    public Item Item => AbstractItem.Item;
    
    private Animation animation => Item.Animation;

    public ItemPO(AbstractItem abstractItem) : base(abstractItem)
    {
        AbstractItem = abstractItem;
        bites = Item.Bites;
        
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), Item.Radius, Item.Mass);
             
        bodyChunkConnections = Array.Empty<BodyChunkConnection>();

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

        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = Mathf.Abs(rotation.y);
        }
        
        if (firstChunk.ContactPoint.y < 0)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
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
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite(Item.Sprite ?? Futile.atlasManager.GetElementWithName("pixel"));

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
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
            sLeaser.sprites[0].element = currentSprite;
        }
        
        sLeaser.sprites[0].SetPosition(pos);
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rot);
    }
    
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
         sLeaser.sprites[0].color = Color.Lerp(Color.white, palette.blackColor, darkness);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Items");
        foreach (var sprite in sLeaser.sprites)
        {
            sprite.RemoveFromContainer();
            newContatiner.AddChild(sprite);
        }
    }
}