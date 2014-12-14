using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class LuminiteNode : WIScript, ILuminite
		{		//spawns luminite over time
				public override bool UnloadWhenStacked {
						get {
								return false;
						}
				}

				public override bool CanEnterInventory {
						get {
								return false;
						}
				}

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				public LuminiteNodeState State = new LuminiteNodeState();

				public override void OnInitialized()
				{
						switch (worlditem.State) {
								case "Dark":
										if (!State.IsDark) {
												worlditem.State = "Light";
												State.AbsorbedDarkRot = State.MaxCharge;
										}
										break;

								case "Light":
										if (State.IsDark) {
												worlditem.State = "Dark";
										}
										break;

								case "Mined":
								default:
										State.AbsorbedDarkRot = 0f;
										break;
						}

						worlditem.Get <Damageable>().OnDie += OnDie;
				}

				public void OnDie()
				{
						switch (worlditem.State) {
								case "Mined":
										worlditem.Get <Damageable>().ResetDamage();
										State.TimeMined = WorldClock.Time;
										break;

								default:
										worlditem.Get <Damageable>().ResetDamage();
										//set luminite type to mined raw, then get a stack item for the player
										//add that to player inventory
										WorldItem rawLuminite = null;
										GenericWorldItem rawLuminiteItem = RawLuminiteLight;
										if (worlditem.State == "Dark") {
												rawLuminiteItem = RawLuminiteDark;
										}
										if (WorldItems.CloneWorldItem(
												rawLuminiteItem,
												new STransform(worlditem.tr),
												false,
												worlditem.Group,
												out rawLuminite)) {
												//this will drop to the players' feet
												rawLuminite.Props.Local.FreezeOnStartup = false;
												rawLuminite.Initialize();
												rawLuminite.SetMode(WIMode.World);
												rawLuminite.rigidbody.AddForce(worlditem.tr.up * 0.25f);
										}
										State.TimeMined = WorldClock.Time;
										State.AbsorbedDarkRot = 0f;
										worlditem.State = "Mined";
										break;
						}

				}

				public void OnVisible()
				{
						switch (worlditem.State) {
								case "Mined":
										if (State.IsReadyToRegrow) {
												//luminite always grows back as light
												//even if it was dark before
												worlditem.State = "Light";
												worlditem.Get <Damageable>().ResetDamage();
										}
										break;

								default:
										break;
						}
				}

				public static GenericWorldItem RawLuminiteLight {
						get {
								if (gRawLuminiteLight == null) {
										gRawLuminiteLight = new GenericWorldItem();
										gRawLuminiteLight.PackName = "Crystals";
										gRawLuminiteLight.PrefabName = "Raw Luminite 2";
										gRawLuminiteLight.DisplayName = "Raw Luminite";
										gRawLuminiteLight.State = "Light";
								}
								return gRawLuminiteLight;
						}
				}

				public static GenericWorldItem RawLuminiteDark {
						get {
								if (gRawLuminiteDark == null) {
										gRawLuminiteDark = new GenericWorldItem();
										gRawLuminiteDark.PackName = "Crystals";
										gRawLuminiteDark.PrefabName = "Raw Luminite 2";
										gRawLuminiteDark.DisplayName = "Raw Dark Luminite";
										gRawLuminiteDark.State = "Dark";
								}
								return gRawLuminiteDark;
						}
				}

				public bool IsDark {
						get {
								return worlditem.State == "Dark";
						}
				}

				public float AbsorbDarkrot(float amount)
				{
						float amountLeft = amount;
						if (State.IsDark) {
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

				protected static GenericWorldItem gRawLuminiteLight = null;
				protected static GenericWorldItem gRawLuminiteDark = null;
		}

		[Serializable]
		public class LuminiteNodeState
		{
				public float MaxCharge = 100.0f;
				public float AbsorbedDarkRot = 0.0f;
				public float AbsorbedLight = 100.0f;
				public float AbsorbedHeat = 100.0f;
				public double TimeMined = 0f;

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

				public bool IsReadyToRegrow {
						get {
								return WorldClock.Time > TimeMined + WorldClock.RTSecondsToGameSeconds(Globals.LuminiteRegrowTime);
						}
				}
		}
}