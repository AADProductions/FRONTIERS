using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Luminite : WIScript, ILuminite
		{
				public LuminiteState State = new LuminiteState();

				public override void OnInitialized()
				{
						if (IsDark) {
								worlditem.State = "Dark";
						}
				}

				public bool IsDark {
						get {
								return worlditem.State == "Dark";
						}
				}

				public void OnDie()
				{

				}

				public float AbsorbDarkrot(float amount)
				{
						float amountLeft = amount;
						if (State.IsDark || State.IsEncasedInGlass) {
								return amountLeft;
						} else {
								float capacity = (State.MaxDarkRot - State.AbsorbedDarkRot);
								if (amount <= capacity) {
										amountLeft = 0;
										State.AbsorbedDarkRot += amount;
								} else {
										amountLeft = amount - capacity;
										State.AbsorbedDarkRot += capacity;
								}
								if (State.IsDark) {
										worlditem.State = "Dark";
								}
						}
						return amountLeft;
				}
		}

		[Serializable]
		public class LuminiteState
		{
				public bool IsEncasedInGlass = true;
				public float MaxCharge = 100.0f;
				public float AbsorbedDarkRot = 0.0f;
				public float AbsorbedLight = 100.0f;
				public float AbsorbedHeat = 100.0f;

				public bool IsDepleted {
						get {
								return AbsorbedLight <= 0.0f && AbsorbedHeat <= 0.0f;
						}
				}

				public bool IsDark {
						get {
								return AbsorbedDarkRot >= MaxDarkRot;
						}
				}

				public float LightSourceBrightness {
						get {
								float baseEmit = MaxEmitPerHour;
								if (AbsorbedLight > 0 && AbsorbedHeat > 0) {
										//if we have light and heat do normal brightness
										baseEmit = MaxEmitPerHour;
								} else if (AbsorbedLight > 0 || AbsorbedHeat > 0) {
										//if we have light but no heat or vice versa
										//only do half brightness
										baseEmit *= 0.5f;
								} else {
										//if we're depleted
										baseEmit = 0.0f;
								}
								return baseEmit * Globals.LuminiteEmissionToLightBrightnessMultiplier;
						}
				}

				public float TotalCharge {
						get {
								return AbsorbedHeat + AbsorbedLight;
						}
				}

				public float MaxDarkRot {
						get {
								return MaxCharge;
						}
				}

				public float MaxAbsorbLightPerHour {
						get {
								float baseAbsorb = 10.0f;
								return baseAbsorb;
						}
				}

				public float MaxAbsorbHeatPerHour {
						get {
								float baseAbsorb = 10.0f;
								return baseAbsorb;
						}
				}

				public float MaxEmitPerHour {
						get {
								float baseEmit = 10.0f;
								return baseEmit;
						}
				}
		}

		public interface ILuminite
		{
				bool IsDark { get; }

				float AbsorbDarkrot(float amount);
		}
}