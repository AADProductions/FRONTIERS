using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class GuildLibraryBookstand : WIScript {

		GuildLibraryBookstandState State = new GuildLibraryBookstandState ();

		public override void OnInitialized ()
		{
			worlditem.OnVisible += OnVisible;
		}

		public override void PopulateRemoveItemSkills (System.Collections.Generic.HashSet<string> removeItemSkills)
		{
			removeItemSkills.Add ("GuildLibrary");
		}

		public void OnVisible ( )
		{
			if (TimeToFill) {
				TryToFillBookStand ();
			}
		}

		public bool TimeToFill {
			get {
				if (mLibrarySkill == null) {
					Skills.Get.SkillByName ("GuildLibrary", out mLibrarySkill);
				}
				return State.LastTimeFilled + mLibrarySkill.EffectTime > WorldClock.AdjustedRealTime;
			}
		}

		protected void TryToFillBookStand ()
		{
			if (worlditem.StackContainer.IsEmpty) {
				StackItem item = Books.Get.RandomBookAvatarStackItem ();
				WIStackError error = WIStackError.None;
				Stacks.Push.Item (worlditem.StackContainer, item, ref error);
				State.LastTimeFilled = WorldClock.AdjustedRealTime;
			}
		}

		protected static Skill mLibrarySkill;
	}

	[Serializable]
	public class GuildLibraryBookstandState {
		public double LastTimeFilled = 0f;
		public List <string> BooksAddedSoFar = new List <string> ();
	}
}