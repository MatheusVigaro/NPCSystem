using System.Collections.Generic;
using System.Linq;
using HUD;
using Music;
using NPCSystem.Fisobs;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using NodeType = NPCSystem.Action.NodeType;

namespace NPCSystem.DevTools;

public class NPCData : Pom.Pom.ManagedData
{
    public NPCID NPC => GetValue<NPCID>(nameof(NPC));

    [Pom.Pom.FloatField(nameof(scale), -5, 5, 1, 0.01f, displayName: "Scale")]
    public float scale;

    [Pom.Pom.IntegerField(nameof(rotation), -180, 180, 0, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Rotation")]
    public int rotation;

    [Pom.Pom.ExtEnumField<ShaderID>(nameof(shader), "Basic", displayName: "Shader")]
    public ShaderID shader;

    public NPCData(PlacedObject owner) : base(owner, new Pom.Pom.ManagedField[]
    {
        new Pom.Pom.ExtEnumField<NPCID>(nameof(NPC), NPCID.Example, displayName: nameof(NPC))
    })
    {
    }
}

public class NPCObject : UpdatableAndDeletable, IDrawable
{
    private const int MainSprite = 0;
    private const int KillSprite = 1;
    
    public NPC NPC;
    public int CurrentPriority;

    private PlacedObject placedObject;
    private NPCData data;
    private Vector2 pos
    {
        get => placedObject.pos;
        set => placedObject.pos = value;
    }

    private DialogBox dialogBox;

    private Animation animation;
    private Animation idleAnimation;
    private int animationTime;
    private FAtlasElement currentSprite;

    private Action.Node action;
    private int actionTime;

    private Dictionary<string, string> tempValues = new();
    private NPCSaveData saveData;

    private float killFac;
    private float lastKillFac;
    private Creature killTarget;
    
    public NPCObject(PlacedObject placedObject, Room room)
    {
        this.placedObject = placedObject;
        this.room = room;
        if (room.world.game.session is StoryGameSession session)
        {
            saveData = session.saveState.miscWorldSaveData.GetNPCSaveData();
        }
        data = (placedObject.data as NPCData)!;
        NPC = NPCRegistry.GetNPC(data.NPC);
        idleAnimation = animation = AnimationRegistry.GetAnimation(NPC.IdleAnimation);
    }

    public override void Update(bool eu)
    {
        lastKillFac = killFac;

        if (killTarget != null)
        {
            killFac += 0.025f;
            if (killFac >= 1f)
            {
                killTarget.mainBodyChunk.vel += Custom.RNV() * 12f;
                for (var k = 0; k < 20; k++)
                {
                    room.AddObject(new Spark(killTarget.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                }
                killTarget.Die();
                killTarget = null;
                killFac = 0f;
            }
        }
        else
        {
            killFac = 0;
        }

        if (room.game.cameras?.Length > 0 && room.game.cameras[0]?.hud != null && room.game.cameras[0].hud.dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
            dialogBox = room.game.cameras[0].hud.dialogBox;
        }

        if (room.PlayersInRoom.Count == 0)
        {
            action = null;
            animation = idleAnimation;
            return;
        }

        AdvanceAnimation();
        ExecuteCurrentAction();

        if (action != null && action.Next != null && actionTime >= action.Duration && action.Type != NodeType.Prompt)
        {
            SetAction(action.Next);
        }
        
        animationTime++;
        actionTime++;

        base.Update(eu);
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

    private void ExecuteCurrentAction()
    {
        if (action == null) return;

        switch (action.Type)
        {
            case NodeType.Prompt:
                PromptMenu.CurrentPrompt ??= new PromptMenu(room.game.manager, this, action.Options);
                break;
            case NodeType.Move:
                var input = action.Input.Split(',');
                var targetPos = new Vector2(int.Parse(input[0]), int.Parse(input[1]));
                if (pos != targetPos)
                {
                    var speed = input.Length > 2 ? float.Parse(input[2]) : 1;
                    pos = Custom.MoveTowards(pos, targetPos, speed);
                }
                break;
        }
    }

    public void SetAction(string newAction)
    {
        var act = ActionRegistry.GetAction(newAction);
        CurrentPriority = act?.Priority ?? 0;
        SetAction(act?.Script);
    }

    public void SetAction(Action.Node newAction)
    {
        if (newAction == null) return;

        action = newAction;
        actionTime = 0;

        switch (action.Type)
        {
            case NodeType.Idle:
            {
                SetAnimation(idleAnimation);
                break;
            }
            case NodeType.Action:
            {
                SetAction(action.Input);
                break;
            }
            case NodeType.Music:
            {
                if (room.game.manager.musicPlayer.song != null)
                {
                    room.game.manager.musicPlayer.song.StopAndDestroy();
                    room.game.manager.musicPlayer.song = null;
                }

                room.game.manager.musicPlayer.song = new Song(room.game.manager.musicPlayer, action.Input, MusicPlayer.MusicContext.StoryMode)
                {
                    stopAtGate = true,
                    stopAtDeath = true,
                    fadeInTime = 1,
                    playWhenReady = true
                };
                break;
            }
            case NodeType.Sound:
            {
                var sound = SoundRegistry.GetSound(action.Input, NPC.ModID);

                if (sound == null)
                {
                    ExtEnumBase.TryParse(typeof(SoundID), action.Input, true, out var result);
                    sound = result as SoundID;
                }

                room.PlaySound(sound, 0, 1, 1);
                break;
            }
            case NodeType.Text:
            {
                Message(action.Input, action.Duration);
                break;
            }
            case NodeType.Animation:
            {
                SetAnimation(action.Input);
                break;
            }
            case NodeType.ConsumeObject:
            case NodeType.CheckObject:
            {
                var result = "false";

                Item item = null;
                if (action.Input.StartsWith("."))
                {
                    item = ItemRegistry.GetItem(action.Input.TrimStart('.'));
                }

                foreach (var obj in room.updateList.ToList())
                {
                    if ((item != null && obj is ItemPO itemPO && itemPO.Item.ID == item.ID) || obj.GetType().Name.Equals(action.Input))
                    {
                        result = "true";
                        if (action.Type == NodeType.ConsumeObject)
                        {
                            obj.Destroy();
                        }

                        break;
                    }
                }

                if (action.Options.TryGetValue(result, out var nextAction) || ("false".Equals(result) && action.Options.TryGetValue("_", out nextAction)))
                {
                    SetAction(nextAction);
                }

                break;
            }
            case NodeType.SpawnObject:
            {
                if (action.Input.StartsWith("."))
                {
                    var item = ItemRegistry.GetItem(action.Input.TrimStart('.'));
                    ObjectSpawner.AddToRoom(new AbstractItem(room.world, item, room.GetWorldCoordinate(pos), new EntityID()));
                }
                else
                {
                    ObjectSpawner.AddToRoom(ObjectSpawner.CreateAbstractObjectSafe(action.Input.Split(','), room.abstractRoom, room.GetWorldCoordinate(pos)));
                }

                break;
            }
            case NodeType.SetValue:
            {
                var value = action.Input.Split(':');
                saveData.Set(value[0], value[1]);
                break;
            }
            case NodeType.GetValue:
            {
                if (saveData.TryGet<string>(action.Input, out var savedData) && action.Options.TryGetValue(savedData, out var result))
                {
                    SetAction(result);
                }
                else if (action.Options.TryGetValue("_", out result))
                {
                    SetAction(result);
                }
                break;
            }
            case NodeType.SetTempValue:
            {
                var value = action.Input.Split(':');
                tempValues[value[0]] = value[1];
                break;
            }
            case NodeType.GetTempValue:
            {
                if (tempValues.TryGetValue(action.Input, out var savedData) && action.Options.TryGetValue(savedData, out var result))
                {
                    SetAction(result);
                }
                else if (action.Options.TryGetValue("_", out result))
                {
                    SetAction(result);
                }
                break;
            }
            case NodeType.RNG:
                if (action.Options.Count > 0)
                {
                    SetAction(action.Options.ToList()[Random.Range(0, action.Options.Count)].Value);
                }
                break;
            case NodeType.Kill:
            {
                var result = "false";
                foreach (var crit in room.updateList.ToList().OfType<Creature>())
                {
                    if (crit.GetType().Name.Equals(action.Input))
                    {
                        killTarget = crit;
                        result = "true";
                        break;
                    }
                }

                if (action.Options.TryGetValue(result, out var nextAction) || ("false".Equals(result) && action.Options.TryGetValue("_", out nextAction)))
                {
                    SetAction(nextAction);
                }

                break;
            }
        }
    }

    public void SetAnimation(string newAnimation)
    {
        SetAnimation(AnimationRegistry.GetAnimation(newAnimation));
    }

    public void SetAnimation(Animation newAnimation)
    {
        newAnimation ??= idleAnimation;

        animation = newAnimation;
        animationTime = 0;
    }

    private void Message(string text, int duration)
    {
        if (dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
            dialogBox = room.game.cameras[0].hud.dialogBox;
        }

        dialogBox.messages.Add(new MessageWithSound(text, dialogBox.defaultXOrientation, dialogBox.defaultYPos, 0, NPC.Voice, NPC.VoicePitchMin, NPC.VoicePitchMax)
        {
            linger = duration
        });
        if (dialogBox.messages.Count == 1)
        {
            dialogBox.InitNextMessage();
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[MainSprite] = new FSprite("pixel");
        sLeaser.sprites[KillSprite] = new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["FlatLight"]
        };

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(NPC.Container ?? "Midground"));
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var mainSprite = sLeaser.sprites[MainSprite];
        var killSprite = sLeaser.sprites[KillSprite];
        
        if (currentSprite != null)
        {
            sLeaser.sprites[MainSprite].element = currentSprite;
        }

        mainSprite.rotation = data.rotation;
        mainSprite.scaleX = data.scale;
        mainSprite.scaleY = Mathf.Abs(data.scale);
        mainSprite.SetPosition(pos - camPos);
        mainSprite.shader = Custom.rainWorld.Shaders[data.shader.value];

        if (killFac > 0 && killTarget != null)
        {
            killSprite.isVisible = true;

            killSprite.x = Mathf.Lerp(killTarget.mainBodyChunk.lastPos.x, killTarget.mainBodyChunk.pos.x, timeStacker) - camPos.x;
            killSprite.y = Mathf.Lerp(killTarget.mainBodyChunk.lastPos.y, killTarget.mainBodyChunk.pos.y, timeStacker) - camPos.y;
            var f = Mathf.Lerp(lastKillFac, killFac, timeStacker);
            killSprite.scale = Mathf.Lerp(200f, 2f, Mathf.Pow(f, 0.5f));
            killSprite.alpha = Mathf.Pow(f, 3f);
        }
        else
        {
            killSprite.isVisible = false;
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        foreach (var sprite in sLeaser.sprites)
        {
            newContatiner.AddChild(sprite);
        }

        rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[KillSprite]);
    }
}