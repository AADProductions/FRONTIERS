using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class ActionManager <T> : Manager
{
	public override string GameObjectName {
		get {
			return "Frontiers_InputManager";
		}
	}

	public static ActionReceiver <T> InterfaceReceiver = null;
	public static ActionReceiver <T> PlayerReceiver = null;
	public static double TimeStamp = 0f;
	public static float RawMouseAxisX = 0.0f;
	public static float RawMouseAxisY = 0.0f;
	public static float RawControllerAxisX	= 0.0f;
	public static float RawControllerAxisY	= 0.0f;
	public static int LastMouseClick = 0;
	public static KeyCode LastKey = KeyCode.None;

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

	public void AddDoubleClick (T action)
	{
		mDoubleClickMappings.Add (action);
	}

	public void AddHover (bool hover, T action)
	{
		List <T> actionList;
		if (mHoverMappings.TryGetValue (hover, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List <T> ();
			actionList.Add (action);
			mHoverMappings.Add (hover, actionList);
		}
	}

	public void AddButtonKeyCombo (int button, KeyCode key, T action)
	{
		List <T> actionList;
		KeyValuePair <int, KeyCode> comboKey = new KeyValuePair <int, KeyCode> (button, key);
		if (mButtonKeyCombos.TryGetValue (comboKey, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List <T> ();
			actionList.Add (action);
		}
		mButtonKeyCombos.Add (comboKey, actionList);
	}

	public void AddKeyDoubleTap (KeyCode key, T action)
	{
		List <T> actionList;
		if (mKeyDoubleTapMappings.TryGetValue (key, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List <T> ();
			actionList.Add (action);
			mKeyDoubleTapMappings.Add (key, actionList);
		}
	}

	public void AddKeyUp (KeyCode key, T action)
	{
		List <T> actionList;
		if (mKeyUpMappings.TryGetValue (key, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mKeyUpMappings.Add (key, actionList);
		}
	}

	public void AddKeyDown (KeyCode key, T action)
	{
		List <T> actionList;
		if (mKeyDownMappings.TryGetValue (key, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mKeyDownMappings.Add (key, actionList);
		}
	}

	public void AddKeyHold (KeyCode key, T action)
	{
		List <T> actionList;
		if (mKeyHoldMappings.TryGetValue (key, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mKeyHoldMappings.Add (key, actionList);
		}

//		//Debug.Log ("Mapping key hold " + key + " to action " + action);
	}

	public void AddMouseButtonUp (int button, T action)
	{
		List <T> actionList;
		if (mButtonUpMappings.TryGetValue (button, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mButtonUpMappings.Add (button, actionList);
		}
	}

	public void AddMouseButtonDown (int button, T action)
	{
		List <T> actionList;
		if (mButtonDownMappings.TryGetValue (button, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mButtonDownMappings.Add (button, actionList);
		}
	}

	public void AddMouseButtonHold (int button, T action)
	{
		List <T> actionList;
		if (mButtonHoldMappings.TryGetValue (button, out actionList)) {
			actionList.Add (action);
		} else {
			actionList = new List<T> ();
			actionList.Add (action);
			mButtonHoldMappings.Add (button, actionList);
		}
	}

	public void AddMouseMove (T action)
	{
		mMouseMove = action;
	}

	public void AddAxisMove (T action)
	{
		mAxisMove = action;
	}

	public void AddMouseWheelUp (T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddMouseWheelDown (T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddJoystickAxisPos (string axisName, T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddJoystickAxisNeg (string axisName, T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddJoystickButtonDown (string buttonName, T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddJoystickButtonUp (string buttonName, T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void AddJoystickButtonHold (string buttonName, T action)
	{
		//throw new System.NotImplementedException ();
	}

	public void Update ()
	{
		if (!mInitialized || (!HasInterfaceReceiver && !HasPlayerReceiver) || mSuspended) {
			return;
		}

		float rawMouseAxisX	= Input.GetAxisRaw ("Mouse X");
		float rawMouseAxisY	= Input.GetAxisRaw ("Mouse Y");
		float rawControllerAxisX = Input.GetAxisRaw ("Horizontal");
		float rawControllerAxisY = Input.GetAxisRaw ("Vertical");

		if ((Mathf.Abs (rawMouseAxisX - RawMouseAxisX) > 0.00001f) || (Mathf.Abs (rawMouseAxisY - RawMouseAxisY) > 0.00001f)) {
			RawMouseAxisX = rawMouseAxisX;
			RawMouseAxisY = rawMouseAxisY;
			mMouseMovement = true;
		}

		if (!Mathf.Approximately (rawControllerAxisX, RawControllerAxisX) || !Mathf.Approximately (rawControllerAxisY, RawControllerAxisY)) {
			RawControllerAxisX = rawControllerAxisX;
			RawControllerAxisY = rawControllerAxisY;
			mAxisMovement = true;
		}

		TimeStamp = WorldClock.Time;

		CheckMouseMovement ();
		CheckControllerMovement ();
		
		CheckButtonKeyCombos ();
		CheckKeyUpMappings ();
		CheckKeyDownMappings ();
		CheckKeyHoldMappings ();
		CheckKeyDoubleTapMappings ();

		CheckMouseButtonDownMappings ();
		CheckMouseButtonUpMappings ();
		CheckMouseButtonHoldMappings ();
		CheckMouseWheelMapping ();

		PurgeUnusedTaps ();
		PurgeKeyButtonCombos ();

//		mClickEventSent = false;
	}

	protected void CheckMouseMovement ()
	{
		if (mMouseMovement) {
			Send (mMouseMove, TimeStamp);
			mMouseMovement = false;
		}

		if (mAxisMovement) {
			Send (mAxisMove, TimeStamp);
			mAxisMovement = false;
		}
	}

	protected void CheckControllerMovement ()
	{
		//throw new System.NotImplementedException ();
	}

	protected void CheckButtonKeyCombos ()
	{
		foreach (KeyValuePair <int, KeyCode> key in mButtonKeyCombos.Keys) {
			if (Input.GetKey (key.Value)) {
				if (Input.GetMouseButtonDown (key.Key)) {
					mKeysUsedInCombos.Add (key.Value);
					mButtonsUsedInCombos.Add (key.Key);
					List <T> actions;
					if (mButtonKeyCombos.TryGetValue (key, out actions)) {
						for (int i = 0; i < actions.Count; i++){
							Send (actions [i], TimeStamp);
						}
					}
				}
			}
		}
	}

	protected void Send (T action, double timeStamp)
	{	
		if (HasInterfaceReceiver && InterfaceReceiver (action, TimeStamp)) {//send to interface first
			if (HasPlayerReceiver) {
				PlayerReceiver (action, TimeStamp);//if that doesn't score a hit, send to player
			}
		}
	}

	protected void CheckHoverMappings ()
	{
		if (!mHoverChanged) {
			return;
		}

		foreach (KeyValuePair <bool, List <T>> hoverMapping in mHoverMappings) {
			if (mHover == hoverMapping.Key) {
				for (int i = 0; i < hoverMapping.Value.Count; i++){
					Send (hoverMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckKeyDownMappings ()
	{
		foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyDownMappings) {
			if (Input.GetKeyDown (keyMapping.Key) && !mKeysUsedInCombos.Contains (keyMapping.Key)) {
				LastKey = keyMapping.Key;
				for (int i = 0; i < keyMapping.Value.Count; i++){
					Send (keyMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckKeyHoldMappings ()
	{
		foreach (KeyValuePair<KeyCode, List<T>> keyMapping in mKeyHoldMappings) {
			if (Input.GetKey (keyMapping.Key) && !mKeysUsedInCombos.Contains (keyMapping.Key)) {
				for (int i = 0; i < keyMapping.Value.Count; i++){
					Send (keyMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckKeyUpMappings ()
	{
		foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyUpMappings) {
			if (Input.GetKeyUp (keyMapping.Key) && !mKeysUsedInCombos.Contains (keyMapping.Key)) {
				for (int i = 0; i < keyMapping.Value.Count; i++){
					Send (keyMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckKeyDoubleTapMappings ()
	{
		foreach (KeyValuePair <KeyCode, List <T>> keyMapping in mKeyDoubleTapMappings) {
			if (Input.GetKeyDown (keyMapping.Key) && !mKeysUsedInCombos.Contains (keyMapping.Key)) {
				if (mSingleTaps.ContainsKey (keyMapping.Key)) {
					float doubleTapTime = mSingleTaps [keyMapping.Key];
					if (Time.time < doubleTapTime) {//if it's pressed in time for a double tap
						//call our listeners
						for (int i = 0; i < keyMapping.Value.Count; i++){
							Send (keyMapping.Value [i], TimeStamp);
						}
						mSingleTaps.Remove (keyMapping.Key);
					}
				} else {
					//if it's note there, add it
					mSingleTaps.Add (keyMapping.Key, Time.time + Globals.DoubleTapInterval);
				}
			}
		}
	}

	protected void CheckClickMappings ()
	{
		if (!mClick) {
			mClickEventSent = false;
			return;
		}
		foreach (KeyValuePair <int, List<T>> buttonMapping in mButtonDownMappings) {
			for (int i = 0; i < buttonMapping.Value.Count; i++){
				Send (buttonMapping.Value [i], TimeStamp);
			}
		}

		mClickEventSent = true;
	}

	protected void CheckDoubleClickMappings ()
	{
		if (!mDoubleClick) {
			return;
		}
		foreach (T action in mDoubleClickMappings) {
			Send (action, TimeStamp);
		}
	}

	protected void CheckMouseButtonDownMappings ()
	{
		foreach (KeyValuePair <int, List <T>> buttonMapping in mButtonDownMappings) {
			if (Input.GetMouseButtonDown (buttonMapping.Key) && !mButtonsUsedInCombos.Contains (buttonMapping.Key)) {
				LastMouseClick = buttonMapping.Key;
				for (int i = 0; i < buttonMapping.Value.Count; i++){
					Send (buttonMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckMouseButtonUpMappings ()
	{
		foreach (KeyValuePair <int, List <T>> buttonMapping in mButtonUpMappings) {
			if (Input.GetMouseButtonUp (buttonMapping.Key) && !mButtonsUsedInCombos.Contains (buttonMapping.Key)) {
				LastMouseClick = buttonMapping.Key;
				for (int i = 0; i < buttonMapping.Value.Count; i++){
					Send (buttonMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckMouseButtonHoldMappings ()
	{
		foreach (KeyValuePair<int, List<T>> buttonMapping in mButtonHoldMappings) {
			if (Input.GetMouseButton (buttonMapping.Key) && !mButtonsUsedInCombos.Contains (buttonMapping.Key)) {
				LastMouseClick = buttonMapping.Key;
				for (int i = 0; i < buttonMapping.Value.Count; i++){
					Send (buttonMapping.Value [i], TimeStamp);
				}
			}
		}
	}

	protected void CheckMouseWheelMapping ()
	{
		if (Input.GetAxis ("Mouse ScrollWheel") > Globals.MouseScrollSensitivity) {
			Send (mMouseWheelDown, TimeStamp);
		}

		if (Input.GetAxis ("Mouse ScrollWheel") < -Globals.MouseScrollSensitivity) {
			Send (mMouseWheelUp, TimeStamp);
		}
	}

	protected void PurgeKeyButtonCombos ()
	{
		mKeysUsedInCombos.Clear ();
		mButtonsUsedInCombos.Clear ();
	}

	protected void PurgeUnusedTaps ()
	{
		List <KeyCode> keysToRemove = new List <KeyCode> ();
		foreach (KeyValuePair <KeyCode, float> singleTap in mSingleTaps) {
			if (Time.time > singleTap.Value) {
				keysToRemove.Add (singleTap.Key);
			}
		}

		foreach (KeyCode key in keysToRemove) {
			mSingleTaps.Remove (key);
		}
	}

	protected Dictionary <KeyCode, List <T>> mKeyDoubleTapMappings	= new Dictionary <KeyCode, List <T>> ();
	protected Dictionary <KeyCode, List <T>> mKeyDownMappings = new Dictionary <KeyCode, List <T>> ();
	protected Dictionary <KeyCode, List <T>> mKeyUpMappings = new Dictionary <KeyCode, List <T>> ();
	protected Dictionary <KeyCode, List <T>> mKeyHoldMappings = new Dictionary <KeyCode, List <T>> ();
	protected Dictionary <int, List <T>> mButtonDownMappings = new Dictionary <int, List <T>> ();
	protected Dictionary <int, List <T>> mButtonUpMappings = new Dictionary <int, List <T>> ();
	protected Dictionary <int, List <T>> mButtonHoldMappings = new Dictionary <int, List <T>> ();
	protected Dictionary <bool, List <T>> mHoverMappings = new Dictionary <bool, List <T>> ();
	protected Dictionary <KeyCode, float> mSingleTaps = new Dictionary <KeyCode, float> ();
	protected Dictionary <string, List <T>> mJoystickButtonDownMappings = new Dictionary <string, List<T>> ();
	protected Dictionary <string, List <T>> mJoystickButtonUpMappings = new Dictionary <string, List<T>> ();
	protected Dictionary <string, List <T>> mJoystickButtonHoldMappings = new Dictionary <string, List<T>> ();
	protected Dictionary <string, List <T>> mJoystickAxisPosMappings = new Dictionary <string, List<T>> ();
	protected Dictionary <string, List <T>> mJoystickAxisNegMappings = new Dictionary <string, List<T>> ();
	protected T mAxisMove;
	protected T mMouseMove;
	protected T mMouseWheelUp;
	protected T mMouseWheelDown;
	protected List <T> mDoubleClickMappings	= new List <T> ();
	protected Dictionary <KeyValuePair <int, KeyCode>, List <T>> mButtonKeyCombos = new Dictionary <KeyValuePair <int, KeyCode>, List <T>> ();
	protected List <KeyCode> mKeysUsedInCombos = new List <KeyCode> ();
	protected List <int> mButtonsUsedInCombos	= new List <int> ();
	protected bool mMouseMovement = false;
	protected bool mAxisMovement = false;
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
	public void OnHover (bool isOver)
	{
		if (isOver != mHover) {
			mHover = isOver;
			mHoverChanged = true;
			mFallThroughEvent = true;
		}
	}
	//– Sent out when the mouse hovers over the collider or moves away from it. Not sent on touch-based devices.
	public void OnPress (bool isDown)
	{
		//– Sent when a mouse button (or touch event) gets pressed over the collider (with ‘true’) and when it gets released (with ‘false’, sent to the same collider even if it’s released elsewhere).
	}

	public void OnClick ()
	{
		mClick = true;
		mFallThroughEvent = true;
	}
	//— Sent to a mouse button or touch event gets released on the same collider as OnPress. UICamera.currentTouchID tells you which button was clicked.
	public void OnDoubleClick ()
	{
		mDoubleClick = true;
		mFallThroughEvent = true;
	}
	//— Sent when the click happens twice within a fourth of a second. UICamera.currentTouchID tells you which button was clicked.
	public void OnSelect (bool selected)
	{
		//Debug.Log ("OnSelect");
	}
	//– Same as OnClick, but once a collider is selected it will not receive any further OnSelect events until you select some other collider.
	public void OnDrag (Vector2 delta)
	{
		//Debug.Log ("OnDrag");
	}
	//– Sent when the mouse or touch is moving in between of OnPress(true) and OnPress(false).
	public void OnDrop (GameObject drag)
	{
		//Debug.Log ("OnDrop");
	}
	//– Sent out to the collider under the mouse or touch when OnPress(false) is called over a different collider than triggered the OnPress(true) event. The passed parameter is the game object of the collider that received the OnPress(true) event.
	public void OnInput (string text)
	{
		//Debug.Log ("OnInput: " + text);
	}
	//– Sent to the same collider that received OnSelect(true) message after typing something. You likely won’t need this, but it’s used by UIInput
	public void OnTooltip (bool show)
	{
		//Debug.Log ("OnTooltip: " + show);
	}
	//– Sent after the mouse hovers over a collider without moving for longer than tooltipDelay, and when the tooltip should be hidden. Not sent on touch-based devices.
	public void OnScroll (float delta)
	{
		//Debug.Log ("OnScroll: " + delta);
	}
	//is sent out when the mouse scroll wheel is moved.
	public void OnKey (KeyCode key)
	{
		////Debug.Log ("OnKey: " + key);
	}
	//is sent when keyboard or controller input is used.
	#endregion
}
