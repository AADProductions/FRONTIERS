using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
    public class PromptUsableQuestItem : WIScript
    {
	public PromptUsableQuestItemState State = new PromptUsableQuestItemState();

	public override void OnInitialized () {
	    worlditem.OnPlayerUse += OnPlayerUse;
	}

	public override void PopulateOptionsList(System.Collections.Generic.List<WIListOption> options, List <string> message)
	{
	    if (Player.Local.Tool.IsEquipped && State.UsableQuestItemNames.Contains(Player.Local.Tool.worlditem.QuestName)) {
		options.Add(new WIListOption("Use " + Player.Local.Tool.worlditem.DisplayName, "Use"));
		mTargetWorldItem = Player.Local.Tool.worlditem;
	    }
	}

	public void OnPlayerUse()
	{
	    if (Player.Local.Tool.IsEquipped && State.UsableQuestItemNames.Contains(Player.Local.Tool.worlditem.QuestName)) {
		Player.Local.Tool.worlditem.OnPlayerUse.SafeInvoke();
	    }
	}

	public void OnPlayerUseWorldItemSecondary(object result)
	{
	    WIListResult secondaryResult = result as WIListResult;
	    switch (secondaryResult.SecondaryResult) {
		case "Use":
		    mTargetWorldItem.OnPlayerUse.SafeInvoke();
		    if (State.FinishOnUse) {
			Finish();
		    }
		    break;

		default:
		    break;
	    }
	}

	protected WorldItem mTargetWorldItem = null;
    }

    [Serializable]
    public class PromptUsableQuestItemState
    {
	public List <string> UsableQuestItemNames = new List <string>();
	public bool FinishOnUse = true;
    }
}