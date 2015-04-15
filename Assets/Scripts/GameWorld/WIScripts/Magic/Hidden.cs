using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		//hides all renderers in a world item
		//this is the effect of a magical spell
		//TODO determine if this is still in use (?)
		public class Hidden : WIScript
		{
				public GameObject HiddenFX;
				public string HiddenFXName = "HiddenEffect";
				public bool InEffect = true;

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				public override bool CanEnterInventory {
						get {
								return false;
						}
				}

				public override void OnInitialized()
				{		//always set to true on initialized
						//this script is 'stateless'
						InEffect = true;
						gameObject.layer = Globals.LayerNumHidden;
						HiddenFX = FXManager.Get.SpawnFX(gameObject, HiddenFXName);
				}

				public void Dispel(string source)
				{
						if (source == "Player") {
								Finish();
						}
				}

				public override void OnFinish()
				{
						InEffect = false;
						GameObject.Destroy(HiddenFX);
						gameObject.layer = Globals.LayerNumWorldItemActive;
				}
		}
}