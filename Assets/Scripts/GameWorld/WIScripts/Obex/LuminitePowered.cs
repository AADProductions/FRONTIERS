using UnityEngine;
using System.Collections;
using System;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class LuminitePowered : WIScript, IDispersible
		{
				public AudioSource PowerAudio;
				public LuminitePoweredState State = new LuminitePoweredState();
				public string FXOnLosePower = string.Empty;
				public string FXOnRestorePower = string.Empty;
				public string FXOnPowerSourceRemoved = string.Empty;
				//luminite power source props
				public Transform PowerSourceDopplegangerParent;
				public GameObject PowerSourceDoppleganger;
				public GenericWorldItem PowerSourceDopplegangerProps = new GenericWorldItem();
				public bool CanRemoveSource = true;
				public bool DispersesForever = false;

				public override void OnInitialized()
				{
						worlditem.OnAddedToGroup += OnAddedToGroup;
				}

				public void OnAddedToGroup()
				{
						if (HasPowerSource) {
								PowerAudio.Play();
								PowerSourceDoppleganger = WorldItems.GetDoppleganger(PowerSourceDopplegangerProps, PowerSourceDopplegangerParent, PowerSourceDoppleganger, WIMode.World);
						} else {
								PowerAudio.Stop();
						}
				}

				public bool HasPowerSource { 
						get {
								return State.HasPowerSource;
						}
						set {
								State.HasPowerSource = value;
								if (!State.HasPowerSource) {
										WorldItems.ReturnDoppleganger(PowerSourceDoppleganger);
								} else {
										PowerSourceDoppleganger = WorldItems.GetDoppleganger(PowerSourceDopplegangerProps, PowerSourceDopplegangerParent, PowerSourceDoppleganger, WIMode.World);
								}
								//this will power it down automatically
								HasPower = State.HasPowerSource;
						}
				}

				public bool HasPower {
						get {
								return State.HasPower;
						}
						set {
								if (State.HasPower) {
										if (!value) {
												State.HasPower = false;
												FXManager.Get.SpawnFX(worlditem.tr, FXOnLosePower);
												PowerAudio.Stop();
												OnLosePower.SafeInvoke();
												return;
										}
								} else {
										if (value) {
												State.HasPower = true;
												FXManager.Get.SpawnFX(worlditem.tr, FXOnRestorePower);
												PowerAudio.Play();
												OnRestorePower.SafeInvoke();
												return;
										}
								}
						}
				}

				public Action OnLosePower;
				public Action OnRestorePower;
				public Action OnPowerSourceRemoved;

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (CanRemoveSource) {
								if (State.HasPowerSource) {
										options.Add(new GUIListOption("Remove " + PowerSourceDopplegangerProps.DisplayName, "RemovePowerSource"));
								} else if (Player.Local.Tool.IsEquipped && Stacks.Can.Stack(Player.Local.Tool.worlditem, PowerSourceDopplegangerProps)) {
										options.Add(new GUIListOption("Add " + PowerSourceDopplegangerProps.DisplayName, "AddPowerSource"));
								}
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "RemovePowerSource":
										WIStackError error = WIStackError.None;
										if (Player.Local.Inventory.AddItems(PowerSourceDopplegangerProps.ToStackItem(), ref error)) {
												HasPowerSource = false;
												OnPowerSourceRemoved.SafeInvoke();
												FXManager.Get.SpawnFX(worlditem.tr.position, FXOnPowerSourceRemoved);
										}
										break;

								case "AddPowerSource":
										if (Player.Local.Tool.IsEquipped && Stacks.Can.Stack(Player.Local.Tool.worlditem, PowerSourceDopplegangerProps)) {
												PowerSourceDopplegangerProps.CopyFrom(Player.Local.Tool.worlditem);
												Player.Local.Tool.worlditem.RemoveFromGame();
												HasPowerSource = true;
										}
										break;

								default:
										break;
						}
				}

				#region IDispersible implementation

				public bool Disperse(float duration)
				{
						if (!IsDispersed) {
								HasPower = false;
								if (!DispersesForever) {
										StartCoroutine(DisperseOverTime(duration));
								}
								return true;
						}
						return false;
				}

				public bool IsDispersed {
						get {
								return !HasPower || mDispersingOverTime;
						}
				}

				#endregion

				protected IEnumerator DisperseOverTime(float dispersedDuration)
				{
						mDispersedStartTime = WorldClock.AdjustedRealTime;
						while (WorldClock.AdjustedRealTime - mDispersedStartTime < dispersedDuration) {
								yield return null;
						}
						//if our power source hasn't been removed
						//remove it here
						if (HasPowerSource) {
								HasPower = true;
						}
						yield break;
				}

				public override void PopulateExamineList(System.Collections.Generic.List<WIExamineInfo> examine)
				{
						if (HasPower) {
								examine.Add(new WIExamineInfo("It is powered by Luminite."));
						} else {
								if (HasPowerSource) {
										examine.Add(new WIExamineInfo("It has no power."));
								} else {
										examine.Add(new WIExamineInfo("Its power source has been removed."));
								}

						}
				}

				protected double mDispersedStartTime = 0f;
				protected bool mDispersingOverTime = false;
		}

		[Serializable]
		public class LuminitePoweredState
		{
				public bool HasPower = true;
				public bool HasPowerSource = true;
		}

		public interface IDispersible
		{
				bool Disperse(float duration);

				bool IsDispersed { get; }
		}
}