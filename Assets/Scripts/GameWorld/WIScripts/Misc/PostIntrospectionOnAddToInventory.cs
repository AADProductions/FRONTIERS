using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
		public class PostIntrospectionOnAddToInventory : WIScript
		{
				public string IntrospectionMessage;
				public float IntrospectionDelay = 1.0f;

				public override void OnModeChange()
				{
						if (mHasBeenTriggered) {
								return;
						}
			
						if (worlditem.Mode == WIMode.Stacked) {
								mHasBeenTriggered = true;
								StartCoroutine(PostIntrospection());
						}
				}

				public IEnumerator PostIntrospection()
				{
						double start = Frontiers.WorldClock.AdjustedRealTime;
						while (Frontiers.WorldClock.AdjustedRealTime < start + IntrospectionDelay) {
								yield return null;
						}
			
						GUI.GUIManager.PostIntrospection(IntrospectionMessage);
			
						GameObject.Destroy(this);
				}

				protected bool mHasBeenTriggered = false;
		}
}