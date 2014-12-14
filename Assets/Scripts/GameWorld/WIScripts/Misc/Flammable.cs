using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class Flammable : WIScript
		{
				public FlammableState State = new FlammableState();
				public bool PermanentIgnition = false;
				public bool DieOnDepleted = false;
				public bool FuelDoesDamage = false;
				public string RequiredState = string.Empty;
				public string IgnitedState = string.Empty;
				public string DepletedState = string.Empty;
				public IgnitionProbability Probability = IgnitionProbability.Moderate;
				public FireType Type;
				public Fire FireObject;
				public float FireScaleMultiplier = 1.0f;
				public Vector3 FireObjectOffset = Vector3.zero;
				public Action OnDepleted;
				public Action OnExtinguish;
				public Action OnIgnite;
				public bool RevealFlammableOnExamine = false;
				public bool CanAcceptFuel = false;

				public override bool CanEnterInventory {
						get {
								if (IsOnFire) {
										return false;
								}
								return true;
						}
				}

				public override bool CanBeCarried {
						get {
								if (IsOnFire) {
										return false;
								}
								return true;
						}
				}

				public bool CanBeIgnited {
						get {
								if (!string.IsNullOrEmpty(RequiredState) && worlditem.State != RequiredState) {
										return false;
								}
								if (IsDepleted)
										return false;

								if (worlditem.Mode != WIMode.World)
										return false;

								//if (worlditem.Is <Wet> ())
								//	return false;

								return true;
						}
				}

				public bool CanBeExtinguished {
						get {
								if (IsDepleted)
										return false;

								if (PermanentIgnition)
										return false;

								return true;
						}
				}

				[NObjectSync]
				public bool IsOnFire {
						get {
								State.IsOnFire = FireObject != null;
								return State.IsOnFire;
						}
						set {
								if (worlditem.IsNObject && !worlditem.NObject.isMine) {
										//if we're not the brain this value will set the fire state
										//this will also update the script state
										State.IsOnFire = value;
										SetOnFire(State.IsOnFire);
								}
								//else ignore this request
						}
				}

				[NObjectSync]
				public float FuelBurned {
						get {
								return State.FuelBurned;
						}
						set {
								State.FuelBurned = value;
						}
				}

				public float NormalizedFuelRemaining {
						get {
								return State.FuelRemaining / State.TotalFuel;
						}
				}

				[NObjectSync]
				public float TotalFuel {
						get {
								return State.TotalFuel;
						}
						set {
								State.TotalFuel = value;
						}
				}

				public bool IsDepleted {
						get {
								return FuelBurned > TotalFuel;
						}
				}

				public override void OnInitialized()
				{
						if (State.IgniteOnStartup) {
								Ignite("Default");
						}
				}

				public void AddFuel(Flammable fuelSource)
				{
						if (CanAcceptFuel && fuelSource != this) {
								State.TotalFuel += fuelSource.TotalFuel;
								fuelSource.worlditem.RemoveFromGame();
						}
				}

				public void SetStackedMode()
				{
						mHasBeenAddedToInventory = true;
				}

				public void Extinguish()
				{
						SetOnFire(false);
				}

				public bool BurnFuel(float fuelBurned)
				{
						if (State.InfiniteUntilPickedUp && !mHasBeenAddedToInventory) {
								return true;
						}

						State.FuelBurned += fuelBurned;

						if (FuelDoesDamage) {
								if (mCumulativeDamagePackage == null) {
										mCumulativeDamagePackage = new DamagePackage();
										mCumulativeDamagePackage.Point = transform.position;
										mCumulativeDamagePackage.SenderMaterial	= WIMaterialType.Fire;
										mCumulativeDamagePackage.SenderName = "Fire";
								}
								mCumulativeDamagePackage.DamageSent += fuelBurned;

								if (mCumulativeDamagePackage.DamageSent > 1.0f) {
										mCumulativeDamagePackage.Target = worlditem;
										DamageManager.Get.SendDamage(mCumulativeDamagePackage);
										mCumulativeDamagePackage = null;
								}
						}

						if (State.IsDepleted) {
								OnDepleted.SafeInvoke();
								if (!string.IsNullOrEmpty(DepletedState)) {
										worlditem.State = DepletedState;
								}
								if (DieOnDepleted) {
										Damageable damageable = null;
										if (worlditem.Is <Damageable>(out damageable)) {
												damageable.InstantKill("Fire");
										}
								}
								return false;
						}
						return true;
				}

				public void Ignite(string mode)
				{
						switch (mode) {
								case "Toggle":
										if (IsOnFire) {
												SetOnFire(false);
										} else {
												SetOnFire(true);
										}
										break;

								default:
										if (!IsOnFire) {
												SetOnFire(true);
										}
										break;
						}
				}

				protected void SetOnFire(bool onFire)
				{
						if (onFire && !IsDepleted) {
								//create the fire object
								GameObject newFireObject = gameObject.FindOrCreateChild("Fire").gameObject;
								FireObject = newFireObject.AddComponent <Fire>();
								FireObject.Offset = FireObjectOffset;
								FireObject.FireScaleMultiplier = FireScaleMultiplier;
								FireObject.Type = Type;
								FireObject.FuelSource = this;
								FireObject.ThermalState = GooThermalState.Igniting;
								if (!string.IsNullOrEmpty(IgnitedState)) {
										worlditem.State = IgnitedState;
								}
								if (OnIgnite != null) {
										OnIgnite();
								}
						} else {
								//destroy the fire object
								if (FireObject != null) {
										FireObject.Extinguish();
										if (OnExtinguish != null && OnExtinguish.Target != null) {
												OnExtinguish();
										}
								}
						}
				}

				public void OnAbsorbWorldItem(WorldItem worlditem)
				{
						Flammable flammable = null; 
						if (worlditem.Is <Flammable>(out flammable)) {
								State.TotalFuel += flammable.State.FuelRemaining;
						}
				}

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						Flammable flammable = null;
						if (CanAcceptFuel
						 && worlditem.Is(WIMode.World | WIMode.Frozen | WIMode.Placed)
						 && Player.Local.Tool.HasWorldItem
						 && Player.Local.Tool.worlditem.Is <Flammable>(out flammable)) {
								options.Add(new GUIListOption("Add " + flammable.worlditem.DisplayName + " to " + worlditem.DisplayName, "AddFuel"));
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "AddFuel":
				//perform the same check
										Flammable flammable = null;
										if (CanAcceptFuel
										&& worlditem.Is(WIMode.World | WIMode.Frozen | WIMode.Placed)
										&& Player.Local.Tool.HasWorldItem
										&& Player.Local.Tool.worlditem.Is <Flammable>(out flammable)) {
												AddFuel(flammable);//it will take care of the rest
										}
										break;

								default:
										break;
						}
				}

				public override void PopulateExamineList(System.Collections.Generic.List<WIExamineInfo> examine)
				{
						if (RevealFlammableOnExamine) {
								if (IsDepleted) {
										examine.Add(new WIExamineInfo("It's flammable, but its fuel has been depleted"));
								} else {
										examine.Add(new WIExamineInfo("It's flammable"));
								}
						}
				}

				public override void OnFinish()
				{
						OnDepleted = null;
						OnExtinguish = null;
						OnIgnite = null;
						base.OnFinish();
				}

				protected bool mHasBeenAddedToInventory = false;
				protected DamagePackage mCumulativeDamagePackage = null;
		}

		[Serializable]
		public class FlammableState
		{
				public float FuelRemaining {
						get {
								return TotalFuel - FuelBurned;
						}
				}

				public bool IsDepleted {
						get {
								return FuelBurned > TotalFuel;
						}
				}

				public float TotalFuel = 10.0f;
				public float FuelBurned = 0.0f;
				public bool IgniteOnStartup = false;
				public bool InfiniteUntilPickedUp = false;
				public bool IsOnFire = false;
		}

		public enum IgnitionProbability
		{
				Impossible,
				Low,
				Moderate,
				High,
				Extreme,
		}
}