using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
	public class Cow : WIScript
	{
		public string MilkCategory = "Milk";

		public override void PopulateOptionsList (List <GUIListOption> options, List <string> message)
		{
			if (Player.Local.Tool.IsEquipped)
			{
				Receptacle recepticle = null;
				if (Player.Local.Tool.worlditem.Is <Receptacle> (out recepticle))
				{
					if (recepticle.HasRoom ( )) {
						options.Add (new GUIListOption ("Milk"));
					}
				}
			}

			if (!worlditem.Get <Creature> ().IsStunned) {
				options.Add (new GUIListOption ("Tip"));
			}
		}

		public void OnPlayerUseWorldItemSecondary (object secondaryResult)
		{
			OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
			switch (dialogResult.SecondaryResult)
			{
			case "Milk":
				//todo
				break;

			case "Tip":
				worlditem.Get <Creature> ().TryToStun (10f);
				worlditem.Get <Creature> ().OnTakeDamage ();//make a noise and flip out
				break;
				
			default:
				break;
			}
		}
	}
}