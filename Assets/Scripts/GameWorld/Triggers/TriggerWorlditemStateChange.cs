using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;

namespace Frontiers.World
{
		public class TriggerWorlditemStateChange : WorldTrigger
		{
				public TriggerWorlditemStateChangeState State = new TriggerWorlditemStateChangeState();
				public WorldItem TargetWorlditem;

				public override bool OnPlayerEnter()
				{
						if (TargetWorlditem == null) {
								if (!WIGroups.FindChildItem(State.WorlditemPath, out TargetWorlditem)) {
										return false;
								}
						}
						if (!string.IsNullOrEmpty(State.OriginalState)) {
								if (TargetWorlditem.State != State.OriginalState) {
										return false;
								}
						}
						TargetWorlditem.State = State.NewState;
						return true;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						if (TargetWorlditem != null) {
								State.WorlditemPath = TargetWorlditem.StaticReference.FullPath;
						}
				}
				#endif
		}

		[Serializable]
		public class TriggerWorlditemStateChangeState : WorldTriggerState
		{
				public string WorlditemPath = string.Empty;
				public string OriginalState = string.Empty;
				public string NewState = string.Empty;
		}
}