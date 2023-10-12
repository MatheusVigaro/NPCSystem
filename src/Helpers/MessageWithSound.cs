using HUD;

namespace NPCSystem;

public class MessageWithSound : DialogBox.Message
{
    public SoundID soundID;
    public VirtualMicrophone.SoundObject currentSound;
    public float pitchMin;
    public float pitchMax;

    public MessageWithSound(string text, float xOrientation, float yPos, int extraLinger, SoundID soundID, float pitchMin, float pitchMax) : base(text, xOrientation, yPos, extraLinger)
    {
        this.soundID = soundID;
        this.pitchMin = pitchMin;
        this.pitchMax = pitchMax;
    }
}