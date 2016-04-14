using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{	//DO NOT USE ON OBJECTS THAT DON'T BELONG TO A PARENT STRUCTURE
	public class DestroyOnStructureDestroyed : WIScript {

		protected Structure mParentStructure;

		public override void OnInitialized ()
		{
			//we can guarantee that the parent structure exists
			//but our group may not exist yet
			StartCoroutine (WaitForParentStructure ());
		}

		protected IEnumerator WaitForParentStructure () {
			while (worlditem.Group == null) {
				yield return null;
			}

			while (mParentStructure == null) {
				mParentStructure = worlditem.Group.GetParentStructure ();
				yield return null;
			}

			mParentStructure.OnStructureDestroyed += OnStructureDestroyed;
			yield break;
		}

		void OnStructureDestroyed () {
			worlditem.RemoveFromGame ();
		}
	}
}
