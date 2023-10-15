using System.Collections.Generic;
using System.Linq;
using Menu;
using NPCSystem.DevTools;
using UnityEngine;

namespace NPCSystem;

public class PromptMenu : Menu.Menu
{
    public static PromptMenu CurrentPrompt;

    private NPCObject owner;
    private Dictionary<string, Action.Node> options;
    private List<SimpleButton> buttons = new();
    private int timeOnScreen;

    public PromptMenu(ProcessManager manager, NPCObject owner, Dictionary<string, Action.Node> options) : base(manager, NPCEnums.NPCPromptMenu)
    {
        this.owner = owner;
        this.options = options;

        pages.Add(new Page(this, null, "main", 0));

        var size = 270f * options.Count;
        var kvpList = options.ToList();
        for (var i = 0; i < kvpList.Count; i++)
        {
            var key = kvpList[i].Key;
            var value = kvpList[i].Value;

            var simpleButton = new SimpleButton(this, pages[0], key, "OPTION_" + key, new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f - size * 0.5f + 270f * i + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 115f), new Vector2(250f, 30f));
            simpleButton.buttonBehav.greyedOut = true;
            buttons.Add(simpleButton);
            pages[0].subObjects.Add(simpleButton);
            if (i == 0)
            {
                pages[0].lastSelectedObject = simpleButton;
            }
        }

        var totalSize = 0f;
        foreach (var button in buttons)
        {
            var num7 = button.menuLabel.label.textRect.width + 20f;
            totalSize += num7 + 10f;
            button.SetSize(new Vector2(num7, 30f));
        }
        
        var currentOffset = 0f;
        foreach (var button in buttons)
        {
            button.pos.x = manager.rainWorld.options.ScreenSize.x * 0.5f - totalSize * 0.5f + currentOffset + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
            currentOffset += button.size.x + 10f;
        }
    }

    public override void Update()
    {
        base.Update();

        if (timeOnScreen >= 40)
        {
            foreach (var button in buttons)
            {
                button.buttonBehav.greyedOut = false;
            }
        }
        else
        {
            timeOnScreen++;
        }
    }

    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message.StartsWith("OPTION_"))
        {
            var key = message.Substring(7);
            owner.SetAction(options[key]);
            Exit();
        }
    }

    public void Exit()
    {
        pages[0].RemoveSprites();
        foreach (var obj in pages[0].subObjects.ToList())
        {
            pages[0].RemoveSubObject(obj);
        }

        container.RemoveAllChildren();
        container.RemoveFromContainer();
        
        cursorContainer.RemoveAllChildren();
        cursorContainer.RemoveFromContainer();
        
        CurrentPrompt = null;
    }
}