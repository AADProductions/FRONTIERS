using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
		public class Bed : WIScript
		{
				public BedState State = new BedState();

				#region who owns the bed

				public InnKeeper ParentInkeeper;
				public Character ParentResident;
				public Character Occupant;
				public bool IsEnclosed = true;

				public bool IsOccupied {
						get {
								return Occupant != null && !Occupant.IsDestroyed;
						}
				}

				public bool HasParentInnkeeper {
						get {
								return ParentInkeeper != null;
						}
				}

				public bool HasParentResident {
						get {
								return ParentResident != null;
						}
				}

				#endregion

				public bool CanBeUsedManually = true;
				public string RequiredState = string.Empty;
				public GameObject CameraTargetPosition;
				public GameObject CameraLookTarget;
				public Vector3 Offset;
				public Collider BedTrigger = null;
				public bool RequiresPayment = false;
				public int CostOfUse = 0;

				public Vector3 BedsidePosition {
						get {
								if (!mInitialized) {
										return transform.position;
								}
								if (mSleptManually) {
										return mLastBedsidePosition;
								} else {
										return worlditem.tr.position + Vector3.up + Vector3.forward;
								}
						}
				}

				public Vector3 SleepingPosition {
						get {
								return CameraTargetPosition.transform.position;
						}
				}

				public override void OnInitialized()
				{
						if (CameraTargetPosition == null) {
								CameraTargetPosition = gameObject.FindOrCreateChild("CameraTargetPosition").gameObject;
						}
						if (CameraLookTarget == null) {
								CameraLookTarget = CameraTargetPosition;
						}

						if (State.ForceCharacterToSleepOnStartup) {
								BedTrigger.enabled = true;
						}
				}

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						mSleptManually = false;

						if (!string.IsNullOrEmpty(RequiredState) && worlditem.State != RequiredState)
								return;

						if (!CanBeUsedManually || !State.HasBeenActivated) {
								return;
						}
						RequiresPayment = false;
						CostOfUse = 0;

						IStackOwner owner = null;
						if (worlditem.Group.HasOwner(out owner) && owner.IsWorldItem) {
								owner.worlditem.Is <InnKeeper>(out ParentInkeeper);
								owner.worlditem.Is <Character>(out ParentResident);
						}

						if (HasParentInnkeeper) {
								if (!ParentInkeeper.HasPaid) {
										RequiresPayment = true;
										CostOfUse = ParentInkeeper.PricePerNight;
								}
						} else if (HasParentResident) {
								//we can only sleep in residence houses if we know the character
								if (!Characters.KnowsPlayer(ParentResident.worlditem.FileName)) {
										return;
								}
						}

						WIListOption dawn = new WIListOption("Sleep 'til Dawn", "Dawn");
						if (WorldClock.IsTimeOfDay(TimeOfDay.ad_TimeDawn)
						 || RequiresPayment && !Player.Local.Inventory.InventoryBank.CanAfford(CostOfUse)) {
								dawn.Disabled = true;
						}
						options.Add(dawn);

						WIListOption noon = new WIListOption("Sleep 'til Noon", "Noon");
						if (WorldClock.IsTimeOfDay(TimeOfDay.ag_TimeNoon)
						 || RequiresPayment && !Player.Local.Inventory.InventoryBank.CanAfford(CostOfUse)) {
								noon.Disabled = true;
						}
						options.Add(noon);

						WIListOption dusk = new WIListOption("Sleep 'til Dusk", "Dusk");
						if (WorldClock.IsTimeOfDay(TimeOfDay.aj_TimeDusk)
						 || RequiresPayment && !Player.Local.Inventory.InventoryBank.CanAfford(CostOfUse)) {
								dusk.Disabled = true;
						}
						options.Add(dusk);

						WIListOption midnight = new WIListOption("Sleep 'til Midnight", "Midnight");
						if (WorldClock.IsTimeOfDay(TimeOfDay.aa_TimeMidnight)
						 || RequiresPayment && !Player.Local.Inventory.InventoryBank.CanAfford(CostOfUse)) {
								midnight.Disabled = true;
						}
						options.Add(midnight);

						if (RequiresPayment) {
								dawn.RequiredCurrencyType = WICurrencyType.A_Bronze;
								noon.RequiredCurrencyType = WICurrencyType.A_Bronze;
								dusk.RequiredCurrencyType = WICurrencyType.A_Bronze;
								midnight.RequiredCurrencyType = WICurrencyType.A_Bronze;

								dawn.CurrencyValue = CostOfUse;
								noon.CurrencyValue = CostOfUse;
								dusk.CurrencyValue = CostOfUse;
								midnight.CurrencyValue = CostOfUse;
						}
				}

				public void OnPlayerUseWorldItemSecondary(object dialogResult)
				{
						WIListResult result = (WIListResult)dialogResult;
						bool takeMoney = RequiresPayment;

						switch (result.SecondaryResult) {
								case "Dawn":
										TryToSleep(TimeOfDay.ac_TimePreDawn);
										break;
				
								case "Noon":
										TryToSleep(TimeOfDay.af_TimePreNoon);
										break;
				
								case "Dusk":
										TryToSleep(TimeOfDay.ai_TimePreDusk);
										break;
				
								case "Midnight":
										TryToSleep(TimeOfDay.al_TimePreMidnight);
										break;
				
								default:
										takeMoney = false;
										break;
						}

						if (takeMoney) {
								int numRemoved = 0;
								ParentInkeeper.State.TimeLastPaid = WorldClock.AdjustedRealTime;
								Player.Local.Inventory.InventoryBank.TryToRemove(CostOfUse, WICurrencyType.A_Bronze);
						}

						mSleptManually = true;
						mLastBedsidePosition = Player.Local.Position;
				}

				public void TryToSleep(TimeOfDay sleepUntil)
				{			
						if (Player.Local.Status.TryToSleep(this, sleepUntil)) {
								worlditem.ActiveStateLocked = false;
								worlditem.ActiveState = WIActiveState.Active;
								worlditem.ActiveStateLocked = true;
								BedTrigger.enabled = true;
								enabled = true;
								State.NumTimesUsed++;
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (State.ForceCharacterToSleepOnStartup) {
								IItemOfInterest potentialOccupant = null;
								if (WorldItems.GetIOIFromCollider(other, out potentialOccupant)) {
										if (potentialOccupant.IOIType == ItemOfInterestType.WorldItem && potentialOccupant.worlditem.Is <Character>(out Occupant)) {
												Occupant.SleepInBed(this);
												BedTrigger.enabled = false;
										}
								}
						}
				}

				public void OnTriggerExit(Collider other)
				{
						mSleptManually = false;
						if (!Player.Local.Status.State.IsSleeping) {
								BedTrigger.enabled = false;
						}
				}

				public void OnDrawGizmos()
				{
						Gizmos.color = Colors.Alpha(Color.yellow, 0.25f);
						if (CameraTargetPosition == null) {
								CameraTargetPosition = gameObject.FindOrCreateChild("CameraTargetPosition").gameObject;
						}
						if (CameraLookTarget == null) {
								CameraLookTarget = CameraTargetPosition;
						}
						Gizmos.DrawWireCube(CameraTargetPosition.transform.position, Vector3.one * 0.125f);
						Gizmos.DrawLine(CameraTargetPosition.transform.position, CameraTargetPosition.transform.position + CameraTargetPosition.transform.forward);
						Gizmos.DrawWireSphere(BedsidePosition, 1f);
				}

				[Serializable]
				public class BedState
				{
						public int NumTimesUsed = 0;
						public bool HasBeenActivated = true;
						public bool ForceCharacterToSleepOnStartup = false;
				}

				protected bool mSleptManually = false;
				protected Vector3 mLastBedsidePosition;
				protected Quaternion mLastBedsideRotation;
		}
}