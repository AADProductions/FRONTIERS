using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.World.WIScripts
{
	public class SpawnFXOnDie : WIScript
	{
		[FrontiersFXAttribute]
		public string FXName;
		public MasterAudio.SoundType SoundTypeOnPlaced;
		public string SoundChildName;

		public override void OnModeChange ()
		{
			if (worlditem.Mode == WIMode.Destroyed) {
				FXManager.Get.SpawnFX (transform.position, FXName);
				MasterAudio.PlaySound (SoundTypeOnPlaced, transform, SoundChildName);
			}
		}
	}
}