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

				public Action OnShow { get; set; }

				public Action OnHide { get; set; }

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

				public override void Awake()
				{
						base.Awake();
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
						if (!GameManager.Is(FGameState.InGame) && GameManager.State != FGameState.GamePaused) {
								return false;
						}

						if (Maximized) {
								return true;
						}

						if (!GUIManager.Get.GetFocus(this)) {
								return false;
						}

						GUIManager.Get.ReceiveInterfaceAction(InterfaceActionType.ToggleInterface, WorldClock.Time);

						GetPlayerAttention = false;

						if (MaximizeAvatarAction != AvatarAction.NoAction) {
								//Player.Get.AvatarActions.ReceiveAction (MaximizeAvatarAction, WorldClock.Time);
						}

						for (int i = 0; i < MasterAnchors.Count; i++) {
								MasterAnchors[i].relativeOffset = Vector2.zero;
						}

						MinimizeAllBut(Name);
						mMaximized = true;
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InterfaceToggle");
						OnShow.SafeInvoke();

						//while we're here, run the garbage collector! players won't notice a slight lag
						System.GC.Collect();

						return true;
				}

				public virtual bool Minimize()
				{
						if (!Maximized) {
								return true;
						}

						GUIManager.Get.ReceiveInterfaceAction(InterfaceActionType.ToggleInterface, WorldClock.Time);

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

				protected bool mMaximized = false;
				protected bool mShowQuickslots = true;

				#region static functions

				public static bool PrimaryShowQuickslots {
						get {
								foreach (PrimaryInterface pi in mInterfaceLookup.Values) {
										if (!pi.ShowQuickslots) {
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

				public static void MinimizeAll()
				{
						for (int i = 0; i < GUIManager.Get.PrimaryInterfaces.Count; i++) {
								PrimaryInterface primaryInterface = GUIManager.Get.PrimaryInterfaces[i] as PrimaryInterface;
								primaryInterface.Minimize();
						}
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
