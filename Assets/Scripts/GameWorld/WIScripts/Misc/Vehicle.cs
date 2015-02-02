using UnityEngine;
using System.Collections;
using System;
using ExtensionMethods;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Vehicle : WIScript
		{
				public float RotationInertia {
						get {
								return RotationInertiaBase * State.RotationInertiaMultiplier;
						}
				}

				public float Acceleration {
						get {
								return AccelerationBase * State.AccelerationMultiplier;
						}
				}

				public PlayerMountType MountType = PlayerMountType.Air;
				public VehicleState State = new VehicleState();
				public TemperatureRange MaxTemperature = TemperatureRange.C_Warm;
				public TemperatureRange MinTemperature = TemperatureRange.C_Warm;
				public Action OnMount;
				public Action OnDismount;
				public string MountOptionText = "Mount";
				public string RequireState;
				public string ControllerState;
				public string CameraState;
				public LocalPlayer Occupant;
				public Transform MountPoint;
				public float RotationInertiaBase = 1f;
				public float AccelerationBase = 1f;

				public bool IsOccupied {
						get {
								return Occupant != null;
						}
				}

				public override void PopulateOptionsList(System.Collections.Generic.List<WIListOption> options, List <string> message)
				{
						if (!string.IsNullOrEmpty(RequireState) && worlditem.State != RequireState) {
								return;
						}
						options.Add(new WIListOption(MountOptionText, "LocalPlayerMount"));
				}

				public void OnPlayerUseWorldItemSecondary(object result)
				{
						WIListResult dialogResult = result as WIListResult;
						switch (dialogResult.SecondaryResult) {
								case "LocalPlayerMount":
										Mount(Player.Local);
										break;
						}
				}

				public override void OnInitialized()
				{
						Player.Get.UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(Dismount));
						MountPoint = gameObject.FindOrCreateChild("MountPoint");
				}

				public void Mount(LocalPlayer newOccupant)
				{
						if (IsOccupied)
								return;

						worlditem.ActiveStateLocked = false;
						worlditem.ActiveState = WIActiveState.Visible;
						worlditem.ActiveStateLocked = true;

						Occupant = newOccupant;
						Occupant.SetControllerState(ControllerState, true);
						Occupant.SetCameraState(CameraState, true);
						Occupant.transform.position = MountPoint.position;
						Occupant.transform.rotation = MountPoint.rotation;

						Occupant.FPSController.Mount = this;

						OnMount.SafeInvoke();
						enabled = true;
				}

				public bool Dismount(double timeStamp)
				{
						if (!IsOccupied)
								return true;

						worlditem.ActiveStateLocked = false;
						worlditem.ActiveState = WIActiveState.Active;

						Occupant.SetControllerState(ControllerState, false);
						Occupant.SetCameraState(CameraState, false);
						Occupant = null;
						OnDismount.SafeInvoke();
						enabled = false;
						return true;
				}

				public void Update()
				{
						Player.Local.Status.ActiveStateList.SafeAdd("Airborne");
				}

				public void UpdateOccupantPosition(Vector3 position, Quaternion rotation)
				{
						transform.position = position;
						transform.rotation = Quaternion.Lerp(transform.rotation, rotation, (float)(WorldClock.ARTDeltaTime * RotationInertia));
				}
		}

		public class VehicleState
		{
				public float RotationInertiaMultiplier = 1f;
				public float AccelerationMultiplier = 1f;
		}
}