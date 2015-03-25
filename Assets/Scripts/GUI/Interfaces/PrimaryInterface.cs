using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class PrimaryInterface : FrontiersInterface, IGUITabOwner
		{
				public InterfaceActionType ToggleAction = InterfaceActionType.NoAction;
				public AvatarAction MaximizeAvatarAction = AvatarAction.NoAction;
				public AvatarAction MinimizeAvatarAction = AvatarAction.NoAction;
				public bool HoldFocus = false;
				public bool DisableGameObjectOnMinimize = true;

				public Action OnShow { get; set; }

				public Action OnHide { get; set; }

				public virtual bool ReadyToMaximize {
						get {
								return WorldClock.RealTime > (mLastTimeMinimized + MinimimToggleInterval);
						}
				}

				public virtual bool ReadyToMinimize {
						get {
								return WorldClock.RealTime > (mLastTimeMaximized + MinimimToggleInterval);
						}
				}

				public virtual bool Visible { get { return Maximized; } }

				public virtual bool ShowQuickslots {
						get {
								return mShowQuickslots;
						}
						set {
								mShowQuickslots = value;
						}
				}

				public virtual bool CanShowTab(string tabName, GUITabs tabs)
				{
						return true;
				}

				public bool Maximized {
						get {
								return mMaximized;
						}
				}

				public override InterfaceType Type {
						get {
								return InterfaceType.Primary;
						}
				}

				public virtual void OnEnable()
				{

				}

				public virtual void OnDisable()
				{

				}

				public override void WakeUp()
				{
						base.WakeUp();

						if (mInterfaceLookup == null) {
								mInterfaceLookup = new Dictionary <string, PrimaryInterface>();
						}
						if (ToggleAction != InterfaceActionType.NoAction) {
								Subscribe(ToggleAction, new ActionListener(Toggle));
						}
						PrimaryInterface.mInterfaceLookup.Add(Name, this);

						GUITabs tabs = gameObject.GetComponent <GUITabs>();
						if (tabs != null) {
								tabs.Initialize(this);
						}

						if (UserActions == null) {
								UserActions = gameObject.GetComponent <UserActionReceiver>();
						}
				}

				public override void Start()
				{
						base.Start();
						//always let other interfaces know what's up
						FilterExceptions |= InterfaceActionType.FlagsTogglePrimary;

						UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(ActionCancel));
						Minimize();
				}

				public virtual bool ActionCancel(double timeStamp)
				{
						if (mMaximized) {
								Minimize();
						}
						return true;
				}

				public virtual bool Maximize()
				{
						if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused)) {
								//Debug.Log("Can't maximize in " + name + ", not in game or paused");
								return false;
						}

						if (Maximized || mMaximizing) {
								//Debug.Log("Already maximize in " + name + ", proceeding");
								return true;
						}

						if (!ReadyToMaximize) {
								//Debug.Log("Not ready to maximize in " + name);
								return false;
						}

						if (!GUIManager.Get.GetFocus(this)) {
								//Debug.Log("Couldn't get focus to maximized in " + name);
								return false;
						}

						mMaximizing = true;

						SendToggleInterfaceAction();

						GetPlayerAttention = false;

						if (MaximizeAvatarAction != AvatarAction.NoAction) {
								//Player.Get.AvatarActions.ReceiveAction (MaximizeAvatarAction, WorldClock.AdjustedRealTime);
						}

						for (int i = 0; i < MasterAnchors.Count; i++) {
								MasterAnchors[i].relativeOffset = Vector2.zero;
						}

						if (DisableGameObjectOnMinimize) {
								gameObject.SetActive(true);
						}

						MinimizeAllBut(Name);
						mMaximized = true;
						OnShow.SafeInvoke();

						//while we're here, run the garbage collector! players won't notice a slight lag
						System.GC.Collect();

						mLastTimeMaximized = WorldClock.RealTime;
						mMaximizing = false;
						return true;
				}

				public virtual bool Minimize()
				{
						if (!Maximized || mMinimizing) {
								//Debug.Log("Already minimizing in " + name + ", proceeding");
								return true;
						}

						if (!ReadyToMinimize) {
								//Debug.Log("Not ready to minimize in " + name);
								return false;
						}

						mMinimizing = true;

						SendToggleInterfaceAction();

						GUIManager.Get.ReleaseFocus(this);

						for (int i = 0; i < MasterAnchors.Count; i++) {
								MasterAnchors[i].relativeOffset = Vector2.one;
						}

						if (MinimizeAvatarAction != AvatarAction.NoAction) {

						}
						mMaximized = false;

						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InterfaceToggle");
						//TODO optimize this
						OnHide.SafeInvoke();

						//while we're here, run the garbage collector! players won't notice a slight lag
						System.GC.Collect();

						mLastTimeMinimized = WorldClock.RealTime;
						mMinimizing = false;

						if (DisableGameObjectOnMinimize) {
								gameObject.SetActive(false);
						}

						return true;
				}

				public override bool GainFocus()
				{
						enabled = true;
						HasFocus = true;
						EnableInput();
						UserActions.GainFocus();
						OnGainFocus.SafeInvoke();
						return true;
				}

				public override bool LoseFocus()
				{
						if (HoldFocus) {
								return false;
						}
						HasFocus = false;
						DisableInput();
						UserActions.LoseFocus();
						OnLoseFocus.SafeInvoke();
						return true;
				}

				public virtual bool Toggle(double timeStamp)
				{
						if (Maximized) {
								Minimize();
						} else {
								Maximize();
						}
						return true;
				}

				protected double mLastTimeMinimized;
				protected double mLastTimeMaximized;
				protected static double MinimimToggleInterval = 0.25f;
				protected bool mMaximized = false;
				protected bool mMaximizing = false;
				protected bool mMinimizing = false;
				protected bool mShowQuickslots = true;

				#region static functions

				public static void ResetToggle () {
						gSentToggleActionThisFrame = false;
				}

				public static void SendToggleInterfaceAction() {
						//when maximizing & minimizing interfaces we send out a toggle interface action
						//these can sometimes add up quickly so instead of sending them directly
						//interfaces call this method to make sure it only happens once per frame
						if (!gSentToggleActionThisFrame) {
								gSentToggleActionThisFrame = true;
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InterfaceToggle");
								GUIManager.Get.ReceiveInterfaceAction(InterfaceActionType.ToggleInterface, WorldClock.AdjustedRealTime);
						}
				}

				protected static bool gSentToggleActionThisFrame = false;

				public static bool PrimaryShowQuickslots {
						get {
								var piLookup = mInterfaceLookup.GetEnumerator();
								while (piLookup.MoveNext()) {
										if (!piLookup.Current.Value.ShowQuickslots) {
												return false;
										}
								}
								return true;
						}
				}

				public static void MinimizeInterface(string interfaceName)
				{
						PrimaryInterface primaryInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out primaryInterface)) {
								primaryInterface.Minimize();
						}
				}

				public static bool MinimizeAll()
				{
						for (int i = 0; i < GUIManager.Get.PrimaryInterfaces.Count; i++) {
								PrimaryInterface primaryInterface = GUIManager.Get.PrimaryInterfaces[i] as PrimaryInterface;
								if (!primaryInterface.Minimize()) {
										return false;
								}
						}
						return true;
				}

				public static void MinimizeAllBut(string interfaceName)
				{
						for (int i = 0; i < GUIManager.Get.PrimaryInterfaces.Count; i++) {
								PrimaryInterface primaryInterface = GUIManager.Get.PrimaryInterfaces[i] as PrimaryInterface;
								if (primaryInterface.Name != interfaceName && primaryInterface.Maximized) {
										primaryInterface.Minimize();
								}
						}
				}

				public static void MaximizeInterface(string interfaceName, string functionName, string sendToItem)
				{
						PrimaryInterface primaryInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out primaryInterface)) {
								primaryInterface.Maximize();
								primaryInterface.SendMessage(functionName, sendToItem, SendMessageOptions.DontRequireReceiver);
						} else {
								Debug.Log("Couldn't find interface " + interfaceName);
						}
				}

				public static void MaximizeInterface(string interfaceName, string functionName, GameObject sendToItem)
				{
						PrimaryInterface primaryInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out primaryInterface)) {
								primaryInterface.Maximize();
								primaryInterface.SendMessage(functionName, sendToItem, SendMessageOptions.DontRequireReceiver);
						} else {
								Debug.Log("Couldn't find interface " + interfaceName);
						}
				}

				public static void MaximizeInterface(string interfaceName)
				{
						PrimaryInterface primaryInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out primaryInterface)) {
								primaryInterface.Maximize();
						}
				}

				public static void MaximizeInterface(string interfaceName, string functionName)
				{
						PrimaryInterface primaryInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out primaryInterface)) {
								primaryInterface.Maximize();
								primaryInterface.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
						}
				}

				public static bool IsMaximized(string interfaceName)
				{
						PrimaryInterface activatedInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out activatedInterface)) {
								return activatedInterface.Maximized;
						}
						return false;
				}

				public static bool WantsPlayerAttention(string interfaceName)
				{
						PrimaryInterface activatedInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out activatedInterface)) {
								return activatedInterface.GetPlayerAttention;
						}
						return false;
				}

				public static void NeedsPlayerAttention(string interfaceName)
				{
						PrimaryInterface activatedInterface = null;
						if (mInterfaceLookup.TryGetValue(interfaceName, out activatedInterface)) {
								activatedInterface.GetPlayerAttention = true;
								//Player.Get.AvatarActions.ReceiveAction (AvatarAction.InterfaceGetAttention);
						}
				}

				protected static Dictionary <string, PrimaryInterface> mInterfaceLookup;

				#endregion

		}
}
