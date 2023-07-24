using System;
using System.Linq;
using HUD;
using Music;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using NodeType = NPCSystem.Action.NodeType;

namespace NPCSystem.DevTools;

public class NPCData : Pom.Pom.ManagedData
{
    public NPCID NPC => GetValue<NPCID>("NPC");

    public NPCData(PlacedObject owner) : base(owner, new []
    {
        new Pom.Pom.ExtEnumField<NPCID>("NPC", NPCID.Example, displayName: "NPC")
    })
    {
    }
}

public class NPCObject : UpdatableAndDeletable, IDrawable
{
    public NPC NPC;

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
    
    public NPCObject(PlacedObject placedObject, Room room)
    {
        this.placedObject = placedObject;
        this.room = room;
        data = (placedObject.data as NPCData)!;
        NPC = NPCRegistry.GetNPC(data.NPC);
        idleAnimation = animation = AnimationRegistry.GetAnimation(NPC.IdleAnimation);
        
        if (room.game.cameras[0].hud.dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
        }

        dialogBox = room.game.cameras[0].hud.dialogBox;
    }

    public override void Update(bool eu)
    {
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
                if (PromptMenu.CurrentPrompt == null)
                {
                    PromptMenu.CurrentPrompt = new PromptMenu(room.game.manager, this, action.Options);
                }
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
        SetAction(ActionRegistry.GetAction(newAction).Script);
    }

    public void SetAction(Action.Node newAction)
    {
        action = newAction;
        actionTime = 0;

        if (newAction == null) return;
        
        switch (action.Type)
        {
            case NodeType.Idle:
                SetAnimation(idleAnimation);
                break;
            case NodeType.Action:
                SetAction(action.Input);
                break;
            case NodeType.Music:
                if (room.game.manager.musicPlayer.song != null)
                {
                    room.game.manager.musicPlayer.song.StopAndDestroy();
                    room.game.manager.musicPlayer.song = null;
                }

                room.game.manager.musicPlayer.song = new Song(room.game.manager.musicPlayer, action.Input, MusicPlayer.MusicContext.StoryMode)
                {
                    stopAtGate = true,
                    stopAtDeath = true,
                    fadeInTime = 1
                };
                room.game.manager.musicPlayer.song.playWhenReady = true;
                break;
            case NodeType.Sound:
                room.PlaySound(new SoundID(action.Input), 0, 1, 1);
                break;
            case NodeType.Text:
                Message(action.Input, action.Duration);
                break;
            case NodeType.Animation:
                SetAnimation(action.Input);
                break;
            case NodeType.ConsumeObject:
            case NodeType.CheckObject:
                var result = "false";
                foreach (var obj in room.updateList.ToList())
                {
                    if (obj.GetType().Name.Equals(action.Input))
                    {
                        result = "true";
                        if (action.Type == NodeType.ConsumeObject)
                        {
                            obj.Destroy();
                        }
                        break;
                    }
                }

                if (action.Options.TryGetValue(result, out var nextAction))
                {
                    SetAction(nextAction);
                }
                break;
            case NodeType.SpawnObject:
                ObjectSpawner.AddToRoom(ObjectSpawner.CreateAbstractObjectSafe(new []{ action.Input }, room.abstractRoom, room.GetWorldCoordinate(pos)));
                break;
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
        dialogBox.messages.Add(new DialogBox.Message(text, dialogBox.defaultXOrientation, dialogBox.defaultYPos, 0)
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
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("pixel");
        
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(NPC.Container ?? "Midground"));
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (currentSprite != null)
        {
            sLeaser.sprites[0].element = currentSprite;
        }
        sLeaser.sprites[0].SetPosition(pos - camPos);
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
    }
}