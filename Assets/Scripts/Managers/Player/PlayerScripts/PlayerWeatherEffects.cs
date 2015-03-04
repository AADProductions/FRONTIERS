using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers {
		//TODO merge this class into palyer surroundings now that AtmoParticleManager is handling particles
	public class PlayerWeatherEffects : PlayerScript
	{
		public GameObject WindZone;

		public override void OnGameLoadFinish ()
		{
			WindZone = GameObject.Instantiate (Player.Get.LocalWindZone) as GameObject;
			WindZone.transform.parent = player.transform;
			WindZone.transform.ResetLocal ();

			WindZone.animation.Stop ();
			WindZone.animation.Rewind ("WindZoneIntensity");
		}

		public override void OnGameStart ()
		{
			enabled = true;
		}

		public void FixedUpdate ( )
		{
			if (!GameManager.Is (FGameState.InGame) || WindZone != null)
				return;

			WindZone.animation ["WindZoneIntensity"].normalizedTime = Biomes.Get.WindIntensity;
			WindZone.animation.Sample ();
		}
	}
}