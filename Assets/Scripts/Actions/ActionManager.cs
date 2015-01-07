using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using InControl;

namespace Frontiers
{
	public class ActionManager <T> : Manager where T : struct, IConvertible, IComparable, IFormattable
	{
		public override string GameObjectName {
			get {
				return "Frontiers_InputManager";
			}
		}

		public List <ActionSetting> Settings = new List<ActionSetting>();
		public static ActionReceiver <T> InterfaceReceiver = null;
		public static ActionReceiver <T> PlayerReceiver = null;
		public static double TimeStamp = 0f;
		public static float RawMouseAxisX = 0.0f;
		public static float RawMouseAxisY = 0.0f;
		public static float RawMovementAxisX = 0.0f;
		public static float RawMovementAxisY = 0.0f;
		public static int LastMouseClick = 0;
		public static KeyCode LastKey = KeyCode.None;
		public InputControlType MouseXAxis = InputControlType.RightStickX;
		public InputControlType MouseYAxis = InputControlType.RightStickY;
		public InputControlType MovementXAxis = InputControlType.LeftStickX;
		public InputControlType MovementYAxis = InputControlType.LeftStickY;
		public UnityInputDeviceProfile KeyboardAndMouseProfile;

		public override void Initialize()
		{
			ClearSettings();
			PushDefaulSettings();
			AddDaisyChains();
			CreateKeyboardAndMouseProfile();
			if (KeyboardAndMouseProfile != null) {
				InputManager.AttachDevice(new UnityInputDevice(KeyboardAndMouseProfile));
			}
			base.Initialize();
		}

		public void PushSettings(List <ActionSetting> newSettings)
		{
			ClearSettings();
			AddDaisyChains();
		}

		protected void ClearSettings()
		{
			Settings.Clear();

			mKeyDownMappings.Clear();
			mKeyUpMappings.Clear();
			mKeyHoldMappings.Clear();
			mAxisChangeMappings.Clear();
			mDaisyChains.Clear();
		}

		protected virtual void PushDefaulSettings()
		{
			return;
		}

		protected virtual void AddDaisyChains()
		{
			return;
		}

		protected virtual void CreateKeyboardAndMouseProfile()
		{
			return;
		}

		public bool HasInterfaceReceiver {
			get {
				return InterfaceReceiver != null;
			}
		}

		public bool HasPlayerReceiver {
			get {
				return PlayerReceiver != null;
			}
		}

		public void AddDaisyChain(T action, T daisyChainedAction)
		{
			if (action.Equals(daisyChainedAction)) {
				Debug.LogError("Can't daisy chain the same action");
				return;
			}
			//TODO check for third-level daisy chains?
			List <T> actionList;
			if (mDaisyChains.TryGetValue(action, out actionList)) {
				actionList.Add(daisyChainedAction);
			} else {
				actionList = new List<T>();
				actionList.Add(daisyChainedAction);
				mDaisyChains.Add(action, actionList);
			}
		}

		public void AddMapping(InputControlType input, ActionSettingType actionType, T action)
		{

			List <T> actionList;
			Dictionary <InputControlType, List <T>> mappings = mKeyDownMappings;

			switch (actionType) {
				case ActionSettingType.None:
				case ActionSettingType.Down:
					break;

				case ActionSettingType.Hold:
					mappings = mKeyHoldMappings;
					break;

				case ActionSettingType.Up:
					mappings = mKeyUpMappings;
					break;

			}

			if (mappings.TryGetValue(input, out actionList)) {
				actionList.SafeAdd(action);
			} else {
				actionList = new List <T>();
				actionList.Add(action);
				mappings.Add(input, actionList);
			}
		}

		public void AddAxisChange(InputControlType axis, T action)
		{
			List <T> actionList;
			if (mAxisChangeMappings.TryGetValue(axis, out actionList)) {
				actionList.Add(action);
			} else {
				actionList = new List<T>();
				actionList.Add(action);
				mAxisChangeMappings.Add(axis, actionList);
			}
		}

		public void AddKeyUp(InputControlType key, string actionName, T action, bool hide)
		{
			List <T> actionList;
			if (mKeyUpMappings.TryGetValue(key, out actionList)) {
				actionList.Add(action);
			} else {
				actionList = new List<T>();
				actionList.Add(action);
				mKeyUpMappings.Add(key, actionList);
			}
			//Settings.Add(new ActionSetting(action, actionName, ActionSettingType.Up, key, 0, string.Empty, hide));
		}

		public void AddKeyDown(InputControlType key, string actionName, T action, bool hide)
		{
			List <T> actionList;
			if (mKeyDownMappings.TryGetValue(key, out actionList)) {
				actionList.Add(action);
			} else {
				actionList = new List<T>();
				actionList.Add(action);
				mKeyDownMappings.Add(key, actionList);
			}
			//Settings.Add(new ActionSetting(action, actionName, ActionSettingType.Down, key, 0, string.Empty, hide));
		}

		public void AddKeyHold(InputControlType key, string actionName, T action, bool hide)
		{
			List <T> actionList;
			if (mKeyHoldMappings.TryGetValue(key, out actionList)) {
				actionList.Add(action);
			} else {
				actionList = new List<T>();
				actionList.Add(action);
				mKeyHoldMappings.Add(key, actionList);
			}
			//Settings.Add(new ActionSetting(action, actionName, ActionSettingType.Hold, key, 0, string.Empty, hide));
		}

		public void Update()
		{
			if (!mInitialized || (!HasInterfaceReceiver && !HasPlayerReceiver) || mSuspended) {
				return;
			}

			RawMouseAxisX = (float)InputManager.ActiveDevice.GetControl(MouseXAxis).Value;//Input.GetAxisRaw("Mouse X");
			RawMouseAxisY = (float)InputManager.ActiveDevice.GetControl(MouseYAxis).Value;//Input.GetAxisRaw("Mouse Y");
			RawMovementAxisX = (float)InputManager.ActiveDevice.GetControl(MovementXAxis).Value;// Input.GetAxisRaw("Horizontal");
			RawMovementAxisY = (float)InputManager.ActiveDevice.GetControl(MovementYAxis).Value;//Input.GetAxisRaw("Vertical");

			TimeStamp = WorldClock.Time;

			CheckKeyUpMappings();
			CheckKeyDownMappings();
			CheckKeyHoldMappings();
			CheckAxisChanges();
			OnUpdate();
		}

		protected T ConvertToEnum(int enumValue)
		{
			//LET ME CONSTRAIN TO TYPE ENUM C# FFS
			return EnumUtils.ParseEnum <T>(enumValue, false);
		}

		protected void Send(T action, double timeStamp)
		{	
			if (HasInterfaceReceiver && InterfaceReceiver(action, TimeStamp)) {//send to interface first
				if (HasPlayerReceiver) {
					PlayerReceiver(action, TimeStamp);//if that doesn't score a hit, send to player
					//see if any actions are supposed to be daisy-chained
					List <T> daisyChainedActions = null;
					//TODO prevent endless daisy chains!
					if (mDaisyChains.TryGetValue(action, out daisyChainedActions)) {
						for (int i = 0; i < daisyChainedActions.Count; i++) {
							Send(daisyChainedActions[i], timeStamp);
						}
					}
				}
			}
		}

		protected void CheckKeyDownMappings()
		{
			var enumerator = mKeyDownMappings.GetEnumerator();
			while (enumerator.MoveNext()) {
				//foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyDownMappings) {
				keyMapping = enumerator.Current;
				if (InputManager.ActiveDevice.GetControl(keyMapping.Key).WasPressed) {
					//LastKey = keyMapping.Key;
					for (int i = 0; i < keyMapping.Value.Count; i++) {
						Send(keyMapping.Value[i], TimeStamp);
					}
				}
			}
		}

		protected void CheckKeyHoldMappings()
		{
			var enumerator = mKeyHoldMappings.GetEnumerator();
			while (enumerator.MoveNext()) {
				//foreach (KeyValuePair<KeyCode, List<T>> keyMapping in mKeyHoldMappings) {
				keyMapping = enumerator.Current;
				if (InputManager.ActiveDevice.GetControl(keyMapping.Key).IsPressed) {
					for (int i = 0; i < keyMapping.Value.Count; i++) {
						Send(keyMapping.Value[i], TimeStamp);
					}
				}
			}
		}

		protected void CheckKeyUpMappings()
		{
			var enumerator = mKeyUpMappings.GetEnumerator();
			while (enumerator.MoveNext()) {
				//foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyUpMappings) {
				keyMapping = enumerator.Current;
				if (InputManager.ActiveDevice.GetControl(keyMapping.Key).WasReleased) {
					for (int i = 0; i < keyMapping.Value.Count; i++) {
						Send(keyMapping.Value[i], TimeStamp);
					}
				}
			}
		}

		protected void CheckAxisChanges()
		{
			var enumerator = mAxisChangeMappings.GetEnumerator();
			while (enumerator.MoveNext()) {
				//foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyUpMappings) {
				keyMapping = enumerator.Current;
				InputControl c = InputManager.ActiveDevice.GetControl(keyMapping.Key);								
				if (c.HasChanged || c.Value != 0f) {
					for (int i = 0; i < keyMapping.Value.Count; i++) {
						Send(keyMapping.Value[i], TimeStamp);
					}
				}
			}
		}

		protected virtual void OnUpdate()
		{
			return;
		}

		protected KeyValuePair <InputControlType, List <T>> keyMapping;
		protected Dictionary <InputControlType, List <T>> mKeyDownMappings = new Dictionary <InputControlType, List <T>>();
		protected Dictionary <InputControlType, List <T>> mKeyUpMappings = new Dictionary <InputControlType, List <T>>();
		protected Dictionary <InputControlType, List <T>> mKeyHoldMappings = new Dictionary <InputControlType, List <T>>();
		protected Dictionary <InputControlType, List <T>> mAxisChangeMappings = new Dictionary<InputControlType, List<T>>();
		protected Dictionary <T, List <T>> mDaisyChains = new Dictionary<T, List<T>>();
		//event listners for NGUI
		protected bool mHover = false;
		protected bool mHoverChanged = false;
		protected bool mClick = false;
		protected bool mDoubleClick = false;
		protected bool mFallThroughEvent = false;
		protected bool mClickEventSent = false;
		protected bool mSuspended = false;
		//protected Dictionary <T, List <ActionReceiver <T>>> mListeners = new Dictionary <T, List <ActionReceiver <T>>> ( );

		#region NGUI functions

		//TODO actually implement these
		public void OnHover(bool isOver)
		{
			if (isOver != mHover) {
				mHover = isOver;
				mHoverChanged = true;
				mFallThroughEvent = true;
			}
		}
		//– Sent out when the mouse hovers over the collider or moves away from it. Not sent on touch-based devices.
		public void OnPress(bool isDown)
		{
			//– Sent when a mouse button (or touch event) gets pressed over the collider (with ‘true’) and when it gets released (with ‘false’, sent to the same collider even if it’s released elsewhere).
		}

		public void OnClick()
		{
			mClick = true;
			mFallThroughEvent = true;
		}
		//— Sent to a mouse button or touch event gets released on the same collider as OnPress. UICamera.currentTouchID tells you which button was clicked.
		public void OnDoubleClick()
		{
			mDoubleClick = true;
			mFallThroughEvent = true;
		}
		//— Sent when the click happens twice within a fourth of a second. UICamera.currentTouchID tells you which button was clicked.
		public void OnSelect(bool selected)
		{
			//Debug.Log ("OnSelect");
		}
		//– Same as OnClick, but once a collider is selected it will not receive any further OnSelect events until you select some other collider.
		public void OnDrag(Vector2 delta)
		{
			//Debug.Log ("OnDrag");
		}
		//– Sent when the mouse or touch is moving in between of OnPress(true) and OnPress(false).
		public void OnDrop(GameObject drag)
		{
			//Debug.Log ("OnDrop");
		}
		//– Sent out to the collider under the mouse or touch when OnPress(false) is called over a different collider than triggered the OnPress(true) event. The passed parameter is the game object of the collider that received the OnPress(true) event.
		public void OnInput(string text)
		{
			//Debug.Log ("OnInput: " + text);
		}
		//– Sent to the same collider that received OnSelect(true) message after typing something. You likely won’t need this, but it’s used by UIInput
		public void OnTooltip(bool show)
		{
			//Debug.Log ("OnTooltip: " + show);
		}
		//– Sent after the mouse hovers over a collider without moving for longer than tooltipDelay, and when the tooltip should be hidden. Not sent on touch-based devices.
		public void OnScroll(float delta)
		{
			//Debug.Log ("OnScroll: " + delta);
		}
		//is sent out when the mouse scroll wheel is moved.
		public void OnKey(KeyCode key)
		{
			////Debug.Log ("OnKey: " + key);
		}
		//is sent when keyboard or controller input is used.

		#endregion

	}

	[Serializable]
	public class ActionSetting//: IComparable <ActionSetting>
	{
		public ActionSettingType ActionType;
	}
}
