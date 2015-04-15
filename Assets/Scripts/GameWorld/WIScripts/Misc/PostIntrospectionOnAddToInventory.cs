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
						yield return WorldClock.WaitForRTSeconds(IntrospectionDelay);
			
						GUI.GUIManager.PostIntrospection(IntrospectionMessage);
			
						GameObject.Destroy(this);
				}

				protected bool mHasBeenTriggered = false;
		}
}