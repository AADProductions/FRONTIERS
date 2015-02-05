using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Feeble : WIScript
		{
				public FeebleState State = new FeebleState();

				/*public override int OnRefreshHud(int lastHudPriority)
				{
						if (hud.SetActiveType (HudActiveType.OnWorldMode)) {
							//Debug.Log ("Refreshing active type");
							mFeebleElement = hud.GetOrAddElement (HudElementType.ProgressBar, "Feeble");
							mFeebleElement.Initialize (Colors.Get.HUDPositiveFGColor, Colors.Get.HUDNegativeBGColor, Colors.Get.PingNegativeColor);
							mFeebleElement.ProgressValue = State.NormalizedStrength;
						} else {
							mFeebleElement = null;
						}
				}*/

				public override void OnInitialized()
				{
						if (!worlditem.Is <Motile>(out mMotile)) {
								Finish();
						}
						worlditem.RefreshHud();
				}

				public void Update()
				{
						if (!mInitialized) {
								return;
						}

						State.StrengthLost += (float)(mMotile.CurrentMovementSpeed * State.StrengthLossPerMeterPerMinute * WorldClock.DeltaTimeMinutes);

						float normalizedStrength = State.NormalizedStrength;

						if (normalizedStrength <= 0.0f) {
								Damageable damageable = null;
								if (worlditem.Is <Damageable>(out damageable)) {
										damageable.InstantKill(WIMaterialType.None, "Exhaustion", false);
								} else {
										worlditem.SetMode(WIMode.Destroyed);
										Finish();
								}
						}

						if (mFeebleElement != null) {
								mFeebleElement.ProgressValue = normalizedStrength;
								if (normalizedStrength < 0.5f) {
										mFeebleElement.Ping(0.5f);
								} else {
										mFeebleElement.StopPing();
								}
						}
				}

				protected GUIHudElement mFeebleElement = null;
				protected Motile mMotile = null;
		}

		[Serializable]
		public class FeebleState
		{
				public float StrengthLossPerMeterPerMinute	= 0.025f;
				public float StrengthLost = 0.0f;
				public float TotalStrength = 100.0f;

				public float NormalizedStrength {
						get {
								return (TotalStrength - StrengthLost) / TotalStrength;
						}
				}

				public FeebleResult LossOfStrengthResult = FeebleResult.Die;
		}

		public enum FeebleResult
		{
				Die,
		}
}
