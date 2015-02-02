using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World {
	public class TriggerCharacterDeath : WorldTrigger
	{
			public TriggerGameObjectMessageState State = new TriggerGameObjectMessageState();

			protected override bool OnCharacterEnter(Character character)
			{
					if (State.CharacterName == character.worlditem.FileName) {
							Damageable damageable = null;
							if (character.worlditem.Is <Damageable>(out damageable) && !damageable.IsDead) {
									damageable.InstantKill(State.MaterialType, State.CauseOfDeath, State.SpawnDamage);
									return true;
							}
					}
					return false;
			}
	}

	[Serializable]
	public class TriggerGameObjectMessageState : WorldTriggerState
	{
			public string CharacterName	= string.Empty;
			public string CauseOfDeath	= string.Empty;
			public WIMaterialType MaterialType = WIMaterialType.None;
			public bool SpawnDamage = false;
	}
}