using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
		public class TriggerMotileAction : WorldTrigger
		{
				public TriggerMotileActionState State = new TriggerMotileActionState();

				protected override bool OnCharacterEnter(Character character)
				{
						Motile motile = null;
						if (character.worlditem.Is<Motile>(out motile)) {
								if (State.ClearActions) {
										motile.StopMotileActions();
								}
								MotileAction newMotileAction = ObjectClone.Clone <MotileAction>(State.Action);
								if (State.SequenceNodes) {
										string targetName = newMotileAction.Target.FileName.Replace("[#]", State.NumTimesTriggered.ToString());
										newMotileAction.Target.FileName = targetName;
								}
								motile.PushMotileAction(newMotileAction, State.Priority);
								return true;
						}

						return false;
				}
		}

		[Serializable]
		public class TriggerMotileActionState : WorldTriggerState
		{
				public bool SequenceNodes = false;
				public bool ClearActions = false;
				public MotileAction Action = new MotileAction();
				public MotileActionPriority Priority = MotileActionPriority.ForceTop;
		}
}