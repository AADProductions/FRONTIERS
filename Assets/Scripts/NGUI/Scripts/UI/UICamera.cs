//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright Â© 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script should be attached to each camera that's used to draw the objects with
/// UI components on them. This may mean only one camera (main camera or your UI camera),
/// or multiple cameras if you happen to have multiple viewports. Failing to attach this
/// script simply means that objects drawn by this camera won't receive UI notifications:
/// 
/// - OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// - OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// - OnSelect (selected) is sent when a mouse button is released on the same object as it was pressed on.
/// - OnClick (int button) is sent with the same conditions as OnSelect, with the added check to see if the mouse has not moved much.
/// - OnDoubleClick (int button) is sent when the click happens twice within a fourth of a second.
/// - OnDrag (delta) is sent when a mouse or touch gets pressed on a collider and starts dragging it.
/// - OnDrop (gameObject) is sent when the mouse or touch get released on a different collider than the one that was being dragged.
/// - OnInput (text) is sent when typing (after selecting a collider by clicking on it).
/// - OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
/// - OnScroll (float delta) is sent out when the mouse scroll wheel is moved.
/// - OnKey (KeyCode key) is sent when keyboard or controller input is used.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Camera")]
//[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
		/// <summary>
		/// Whether the touch event will be sending out the OnClick notification at the end.
		/// </summary>

		public enum ClickNotification
		{
				None,
				Always,
				BasedOnDelta,
		}

		/// <summary>
		/// Ambiguous mouse, touch, or controller event.
		/// </summary>

		public class MouseOrTouch
		{
				public Vector2 pos;
				// Current position of the mouse or touch event
				public Vector2 delta;
				// Delta since last update
				public Vector2 totalDelta;
				// Delta since the event started being tracked
				public Camera pressedCam;
				// Camera that the OnPress(true) was fired with
				public GameObject current;
				// The current game object under the touch or mouse
				public GameObject pressed;
				// The last game object to receive OnPress
				public float clickTime = 0f;
				// The last time a click event was sent out
				public ClickNotification clickNotification = ClickNotification.Always;
		}

		class Highlighted
		{
				public GameObject go;
				public int counter = 0;
		}

		/// <summary>
		/// Whether the mouse input is used.
		/// </summary>

		public bool useMouse = true;
		/// <summary>
		/// Whether the touch-based input is used.
		/// </summary>

		public bool useTouch = true;
		/// <summary>
		/// Whether the keyboard events will be processed.
		/// </summary>

		public bool useKeyboard = true;
		/// <summary>
		/// Whether the joystick and controller events will be processed.
		/// </summary>

		public bool useController = true;
		/// <summary>
		/// Which layers will receive events.
		/// </summary>

		public LayerMask eventReceiverMask = -1;
		/// <summary>
		/// How long of a delay to expect before showing the tooltip.
		/// </summary>

		public float tooltipDelay = 1f;
		/// <summary>
		/// Whether the tooltip will disappear as soon as the mouse moves (false) or only if the mouse moves outside of the widget's area (true).
		/// </summary>

		public bool stickyTooltip = true;
		/// <summary>
		/// How far the mouse is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
		/// </summary>

		public float mouseClickThreshold = 10f;
		/// <summary>
		/// How far the touch is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
		/// </summary>

		public float touchClickThreshold = 40f;
		/// <summary>
		/// Raycast range distance. By default it's as far as the camera can see.
		/// </summary>

		public float rangeDistance = -1f;
		/// <summary>
		/// Name of the axis used for scrolling.
		/// </summary>

		public string scrollAxisName = "Mouse ScrollWheel";
		/// <summary>
		/// Name of the axis used to send up and down key events.
		/// </summary>

		public string verticalAxisName = "Vertical";
		/// <summary>
		/// Name of the axis used to send left and right key events.
		/// </summary>

		public string horizontalAxisName = "Horizontal";

		[System.Obsolete("Use UICamera.currentCamera instead")]
		static public Camera lastCamera { get { return currentCamera; } }

		[System.Obsolete("Use UICamera.currentTouchID instead")]
		static public int lastTouchID { get { return currentTouchID; } }

		/// <summary>
		/// Position of the last touch (or mouse) event.
		/// </summary>

		static public Vector2 lastTouchPosition = Vector2.zero;
		/// <summary>
		/// Last raycast hit prior to sending out the event. This is useful if you want detailed information
		/// about what was actually hit in your OnClick, OnHover, and other event functions.
		/// </summary>

		static public RaycastHit lastHit;
		/// <summary>
		/// Last camera active prior to sending out the event. This will always be the camera that actually sent out the event.
		/// </summary>

		static public Camera currentCamera = null;
		/// <summary>
		/// ID of the touch or mouse operation prior to sending out the event. Mouse ID is '-1' for left, '-2' for right mouse button, '-3' for middle.
		/// </summary>

		static public int currentTouchID = -1;
		/// <summary>
		/// Current touch, set before any event function gets called.
		/// </summary>

		static public MouseOrTouch currentTouch = null;
		/// <summary>
		/// Whether an input field currently has focus.
		/// </summary>

		static public bool inputHasFocus = false;
		/// <summary>
		/// If events don't get handled, they will be forwarded to this game object.
		/// </summary>

		static public GameObject fallThrough;
		// List of all active cameras in the scene
		static List<UICamera> mList = new List<UICamera>();
		// List of currently highlighted items
		static List<Highlighted> mHighlighted = new List<Highlighted>();
		// Selected widget (for input)
		static GameObject mSel = null;
		// Mouse events
		static MouseOrTouch[] mMouse = new MouseOrTouch[] { new MouseOrTouch(), new MouseOrTouch(), new MouseOrTouch() };
		// The last object to receive OnHover
		static GameObject mHover;
		// Joystick/controller/keyboard event
		static MouseOrTouch mController = new MouseOrTouch();
		// Used to ensure that joystick-based controls don't trigger that often
		static double mNextEvent = 0f;
		// List of currently active touches
		Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();
		// Tooltip widget (mouse only)
		GameObject mTooltip = null;
		// Mouse input is turned off on iOS
		Camera mCam = null;
		LayerMask mLayerMask;
		double mTooltipTime = 0f;
		bool mIsEditor = false;

		/// <summary>
		/// Helper function that determines if this script should be handling the events.
		/// </summary>

		bool handlesEvents { get { return eventHandler == this; } }

		/// <summary>
		/// Caching is always preferable for performance.
		/// </summary>

		public Camera cachedCamera {
				get {
						if (mCam == null)
								mCam = GetComponent<Camera>();
						return mCam;
				}
		}

		/// <summary>
		/// The object the mouse is hovering over.
		/// </summary>

		static public GameObject hoveredObject { get { return mHover; } }

		/// <summary>
		/// Option to manually set the selected game object.
		/// </summary>

		static public GameObject selectedObject {
				get {
						return mSel;
				}
				set {
						if (mSel != value) {
								if (mSel != null) {
										UICamera uicam = FindCameraForLayer(mSel.layer);
					
										if (uicam != null) {
												currentCamera = uicam.mCam;
												mSel.SendMessage("OnSelect", false, SendMessageOptions.DontRequireReceiver);
												if (uicam.useController || uicam.useKeyboard)
														Highlight(mSel, false);
										}
								}

								mSel = value;

								if (mSel != null) {
										UICamera uicam = FindCameraForLayer(mSel.layer);

										if (uicam != null) {
												currentCamera = uicam.mCam;
												if (uicam.useController || uicam.useKeyboard)
														Highlight(mSel, true);
												mSel.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
										}
								}
						}
				}
		}

		/// <summary>
		/// Clear the list on application quit (also when Play mode is exited)
		/// </summary>

		void OnApplicationQuit()
		{
				mHighlighted.Clear();
		}

		/// <summary>
		/// Convenience function that returns the main HUD camera.
		/// </summary>

		static public Camera mainCamera {
				get {
						UICamera mouse = eventHandler;
						return (mouse != null) ? mouse.cachedCamera : null;
				}
		}

		/// <summary>
		/// Event handler for all types of events.
		/// </summary>

		static public UICamera eventHandler {
				get {
						for (int i = 0; i < mList.Count; ++i) {
								// Invalid or inactive entry -- keep going
								UICamera cam = mList[i];
								if (cam == null || !cam.enabled || !cam.gameObject.activeSelf)
										continue;
								return cam;
						}
						return null;
				}
		}

		/// <summary>
		/// Static comparison function used for sorting.
		/// </summary>

		static int CompareFunc(UICamera a, UICamera b)
		{
				if (a.cachedCamera.depth < b.cachedCamera.depth)
						return 1;
				if (a.cachedCamera.depth > b.cachedCamera.depth)
						return -1;
				return 0;
		}

		/// <summary>
		/// Returns the object under the specified position.
		/// </summary>

		static bool Raycast(Vector3 inPos, ref RaycastHit hit)
		{
				for (int i = 0; i < mList.Count; ++i) {
						UICamera cam = mList[i];
			
						// Skip inactive scripts
						if (!cam.enabled || !cam.gameObject.activeSelf)
								continue;

						// Convert to view space
						currentCamera = cam.cachedCamera;
						Vector3 pos = currentCamera.ScreenToViewportPoint(inPos);

						// If it's outside the camera's viewport, do nothing
						if (!Frontiers.VRManager.VRMode) {
								if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
										continue;
						}

						// Cast a ray into the screen
						Ray ray = currentCamera.ScreenPointToRay(inPos);

						// Raycast into the screen
						int mask = currentCamera.cullingMask & (int)cam.eventReceiverMask;
						float dist = (cam.rangeDistance > 0f) ? cam.rangeDistance : currentCamera.farClipPlane - currentCamera.nearClipPlane;
						if (Physics.Raycast(ray, out hit, dist, mask))
								return true;
				}
				return false;
		}

		/// <summary>
		/// Find the camera responsible for handling events on objects of the specified layer.
		/// </summary>

		static public UICamera FindCameraForLayer(int layer)
		{
				int layerMask = 1 << layer;

				for (int i = 0; i < mList.Count; ++i) {
						UICamera cam = mList[i];
						Camera uc = cam.cachedCamera;
						if ((uc != null) && (uc.cullingMask & layerMask) != 0)
								return cam;
				}
				return null;
		}

		/// <summary>
		/// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
		/// </summary>

		static int GetDirection(KeyCode up, KeyCode down)
		{
				if (Input.GetKeyDown(up))
						return 1;
				if (Input.GetKeyDown(down))
						return -1;
				return 0;
		}

		/// <summary>
		/// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
		/// </summary>

		static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
		{
				if (Input.GetKeyDown(up0) || Input.GetKeyDown(up1))
						return 1;
				if (Input.GetKeyDown(down0) || Input.GetKeyDown(down1))
						return -1;
				return 0;
		}

		/// <summary>
		/// Using the joystick to move the UI results in 1 or -1 if the threshold has been passed, mimicking up/down keys.
		/// </summary>

		static int GetDirection(string axis)
		{
				double time = Frontiers.WorldClock.RealTime;

				if (mNextEvent < time) {
						double val = Input.GetAxis(axis);

						if (val > 0.75) {
								mNextEvent = time + 0.25;
								return 1;
						}

						if (val < -0.75) {
								mNextEvent = time + 0.25;
								return -1;
						}
				}
				return 0;
		}

		/// <summary>
		/// Returns whether the widget should be currently highlighted as far as the UICamera knows.
		/// </summary>

		static public bool IsHighlighted(GameObject go)
		{
				for (int i = mHighlighted.Count; i > 0;) {
						Highlighted hl = mHighlighted[--i];
						if (hl.go == go)
								return true;
				}
				return false;
		}

		/// <summary>
		/// Apply or remove highlighted (hovered) state from the specified object.
		/// </summary>

		static void Highlight(GameObject go, bool highlighted)
		{
				if (go != null) {
						for (int i = mHighlighted.Count; i > 0;) {
								Highlighted hl = mHighlighted[--i];

								if (hl == null || hl.go == null) {
										mHighlighted.RemoveAt(i);
								} else if (hl.go == go) {
										if (highlighted) {
												++hl.counter;
										} else if (--hl.counter < 1) {
												mHighlighted.Remove(hl);
												go.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
										}
										return;
								}
						}

						if (highlighted) {
								Highlighted hl = new Highlighted();
								hl.go = go;
								hl.counter = 1;
								mHighlighted.Add(hl);
								go.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
						}
				}
		}

		/// <summary>
		/// Get or create a touch event.
		/// </summary>

		MouseOrTouch GetTouch(int id)
		{
				MouseOrTouch touch;

				if (!mTouches.TryGetValue(id, out touch)) {
						touch = new MouseOrTouch();
						mTouches.Add(id, touch);
				}
				return touch;
		}

		/// <summary>
		/// Remove a touch event from the list.
		/// </summary>

		void RemoveTouch(int id)
		{
				mTouches.Remove(id);
		}

		/// <summary>
		/// Add this camera to the list.
		/// </summary>

		void Awake()
		{
				if (Application.platform == RuntimePlatform.Android ||
				  Application.platform == RuntimePlatform.IPhonePlayer) {
						useMouse = false;
						useTouch = true;
						useKeyboard = false;
						useController = false;
				} else if (Application.platform == RuntimePlatform.PS3 ||
				       Application.platform == RuntimePlatform.XBOX360) {
						useMouse = false;
						useTouch = false;
						useKeyboard = false;
						useController = true;
				} else if (Application.platform == RuntimePlatform.WindowsEditor ||
				       Application.platform == RuntimePlatform.OSXEditor) {
						mIsEditor = true;
				}

				// Save the starting mouse position
				mMouse[0].pos.x = Frontiers.InterfaceActionManager.MousePosition.x;//Input.mousePosition.x;
				mMouse[0].pos.y = Frontiers.InterfaceActionManager.MousePosition.y;//Input.mousePosition.y;
				lastTouchPosition = mMouse[0].pos;

				// Add this camera to the list
				mList.Add(this);
				mList.Sort(CompareFunc);

				// If no event receiver mask was specified, use the camera's mask
				if (eventReceiverMask == -1)
						eventReceiverMask = GetComponent<Camera>().cullingMask;
		}

		/// <summary>
		/// Remove this camera from the list.
		/// </summary>

		void OnDestroy()
		{
				mList.Remove(this);
		}

		/// <summary>
		/// Update the object under the mouse if we're not using touch-based input.
		/// </summary>

		//	void FixedUpdate ()
		//	{
		//		if (useMouse && Application.isPlaying && handlesEvents)
		//		{
		//			GameObject go = Raycast(Input.mousePosition, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
		//			for (int i = 0; i < 3; ++i) mMouse[i].current = go;
		//		}
		//	}

		/// <summary>
		/// Check the input and send out appropriate events.
		/// </summary>

		void Update()
		{
				// Only the first UI layer should be processing events
				if (!Application.isPlaying || !handlesEvents)
						return;

				// Update mouse input
				if (useMouse || (useTouch && mIsEditor))
						ProcessMouse();

				// Process touch input
				if (useTouch)
						ProcessTouches();

				// Clear the selection on escape
				//if (useKeyboard && mSel != null && Input.GetKeyDown(KeyCode.Escape))
				//		selectedObject = null;

				// Forward the input to the selected object
				if (mSel != null && Frontiers.InterfaceActionManager.AvailableKeyDown) {
						string input = Input.inputString;

						// Adding support for some macs only having the "Delete" key instead of "Backspace"
						if (useKeyboard && Input.GetKeyDown(KeyCode.Delete))
								input += "\b";

						if (input.Length > 0) {
								if (!stickyTooltip && mTooltip != null)
										ShowTooltip(false);
								mSel.SendMessage("OnInput", input, SendMessageOptions.DontRequireReceiver);
						}
						// Update the keyboard and joystick events
						//ProcessOthers();
				} else
						inputHasFocus = false;

				// If it's time to show a tooltip, inform the object we're hovering over
				/*if (useMouse && mHover != null) {
						float scroll = Input.GetAxis(scrollAxisName);
						if (scroll != 0f)
								mHover.SendMessage("OnScroll", scroll, SendMessageOptions.DontRequireReceiver);

						if (mTooltipTime != 0f && mTooltipTime < Frontiers.WorldClock.RealTime) {
								mTooltip = mHover;
								ShowTooltip(true);
						}
				}*/
		}

		/// <summary>
		/// Update mouse input.
		/// </summary>

		void ProcessMouse()
		{
				if (useMouse && Application.isPlaying && handlesEvents) {
						GameObject go = Raycast(Frontiers.InterfaceActionManager.MousePosition/*Input.mousePosition*/, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
						for (int i = 0; i < 3; ++i)
								mMouse[i].current = go;
				}

				bool updateRaycast = (Time.timeScale < 0.9f);

				if (!updateRaycast) {
						/*
						for (int i = 0; i < 3; ++i) {
								if (Input.GetMouseButton(i) || Input.GetMouseButtonUp(i)) {
										updateRaycast = true;
										break;
								}
						}
						*/
						//now using action mananger
						if ((Frontiers.InterfaceActionManager.Get.CursorClickDown || Frontiers.InterfaceActionManager.Get.CursorClickUp)
								|| (Frontiers.InterfaceActionManager.Get.CursorRightClickDown || Frontiers.InterfaceActionManager.Get.CursorRightClickUp)) {
								updateRaycast = true;
						}
				}

				// Update the position and delta
				mMouse[0].pos = Frontiers.InterfaceActionManager.MousePosition;//Input.mousePosition;
				mMouse[0].delta = mMouse[0].pos - lastTouchPosition;

				bool posChanged = (mMouse[0].pos != lastTouchPosition);
				lastTouchPosition = mMouse[0].pos;

				// Update the object under the mouse
				if (updateRaycast)
						mMouse[0].current = Raycast(Frontiers.InterfaceActionManager.MousePosition/*Input.mousePosition*/, ref lastHit) ? lastHit.collider.gameObject : fallThrough;

				// Propagate the updates to the other mouse buttons
				for (int i = 1; i < 3; ++i) {
						mMouse[i].pos = mMouse[0].pos;
						mMouse[i].delta = mMouse[0].delta;
						mMouse[i].current = mMouse[0].current;
				}

				// Is any button currently pressed?
				bool isPressed = false;

				/*
				for (int i = 0; i < 3; ++i) {
						if (Input.GetMouseButton(i)) {
								isPressed = true;
								break;
						}
				}
				*/
				//Changed
				isPressed = Frontiers.InterfaceActionManager.Get.CursorClickDown | Frontiers.InterfaceActionManager.Get.CursorRightClickDown;

				if (isPressed) {
						// A button was pressed -- cancel the tooltip
						mTooltipTime = 0f;
				} else if (posChanged && (!stickyTooltip || mHover != mMouse[0].current)) {
						if (mTooltipTime != 0f) {
								// Delay the tooltip
								mTooltipTime = Frontiers.WorldClock.RTDeltaTime + tooltipDelay;
						} else if (mTooltip != null) {
								// Hide the tooltip
								ShowTooltip(false);
						}
				}

				// The button was released over a different object -- remove the highlight from the previous
				if (!isPressed && mHover != null && mHover != mMouse[0].current) {
						if (mTooltip != null)
								ShowTooltip(false);
						Highlight(mHover, false);
						mHover = null;
				}

				/*
				// Process all 3 mouse buttons as individual touches
				for (int i = 0; i < 3; ++i) {
						bool pressed = Input.GetMouseButtonDown(i);
						bool unpressed = Input.GetMouseButtonUp(i);

						currentTouch = mMouse[i];
						currentTouchID = -1 - i;

						// We don't want to update the last camera while there is a touch happening
						if (pressed)
								currentTouch.pressedCam = currentCamera;
						else if (currentTouch.pressed != null)
								currentCamera = currentTouch.pressedCam;

						// Process the mouse events
						ProcessTouch(pressed, unpressed);
				}
				*/

				//this now uses interface action manager
				#region left click
				bool pressed = Frontiers.InterfaceActionManager.Get.CursorClickDown;
				bool unpressed = Frontiers.InterfaceActionManager.Get.CursorClickUp;

				currentTouch = mMouse[0];
				currentTouchID = -1;

				// We don't want to update the last camera while there is a touch happening
				if (pressed)
						currentTouch.pressedCam = currentCamera;
				else if (currentTouch.pressed != null)
						currentCamera = currentTouch.pressedCam;

				// Process the mouse events
				ProcessTouch(pressed, unpressed);
				#endregion

				#region cursor right click
				pressed = Frontiers.InterfaceActionManager.Get.CursorRightClickDown;
				unpressed = Frontiers.InterfaceActionManager.Get.CursorRightClickUp;

				currentTouch = mMouse[1];
				currentTouchID = -1;

				// We don't want to update the last camera while there is a touch happening
				if (pressed)
						currentTouch.pressedCam = currentCamera;
				else if (currentTouch.pressed != null)
						currentCamera = currentTouch.pressedCam;

				// Process the mouse events
				ProcessTouch(pressed, unpressed);
				#endregion

				currentTouch = null;

				// If nothing is pressed and there is an object under the touch, highlight it
				if (!isPressed && mHover != mMouse[0].current) {
						mTooltipTime = Frontiers.WorldClock.RealTime + tooltipDelay;
						mHover = mMouse[0].current;
						Highlight(mHover, true);
				}
		}

		/// <summary>
		/// Update touch-based events.
		/// </summary>

		void ProcessTouches()
		{
				for (int i = 0; i < Input.touchCount; ++i) {
						Touch input = Input.GetTouch(i);
						currentTouchID = input.fingerId;
						currentTouch = GetTouch(currentTouchID);

						bool pressed = (input.phase == TouchPhase.Began);
						bool unpressed = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

						if (pressed) {
								currentTouch.delta = Vector2.zero;
						} else {
								// Although input.deltaPosition can be used, calculating it manually is safer (just in case)
								currentTouch.delta = input.position - currentTouch.pos;
						}

						currentTouch.pos = input.position;
						currentTouch.current = Raycast(currentTouch.pos, ref lastHit) ? lastHit.collider.gameObject : fallThrough;
						lastTouchPosition = currentTouch.pos;

						// We don't want to update the last camera while there is a touch happening
						if (pressed)
								currentTouch.pressedCam = currentCamera;
						else if (currentTouch.pressed != null)
								currentCamera = currentTouch.pressedCam;

						// Process the events from this touch
						ProcessTouch(pressed, unpressed);

						// If the touch has ended, remove it from the list
						if (unpressed)
								RemoveTouch(currentTouchID);
						currentTouch = null;
				}
		}

		/// <summary>
		/// Process keyboard and joystick events.
		/// </summary>

		void ProcessOthers()
		{
				currentTouchID = -100;
				currentTouch = mController;

				// If this is an input field, ignore WASD and Space key presses
				inputHasFocus = (mSel != null && mSel.GetComponent<UIInput>() != null);

				// Enter key and joystick button 1 keys are treated the same -- as a "click"
				bool returnKeyDown	= (useKeyboard && (Input.GetKeyDown(KeyCode.Return) || (!inputHasFocus && Input.GetKeyDown(KeyCode.Space))));
				bool buttonKeyDown	= (useController && Input.GetKeyDown(KeyCode.JoystickButton0));
				bool returnKeyUp	= (useKeyboard && (Input.GetKeyUp(KeyCode.Return) || (!inputHasFocus && Input.GetKeyUp(KeyCode.Space))));
				bool buttonKeyUp	= (useController && Input.GetKeyUp(KeyCode.JoystickButton0));

				bool down	= returnKeyDown || buttonKeyDown;
				bool up = returnKeyUp || buttonKeyUp;

				if (down || up) {
						currentTouch.current = mSel;
						ProcessTouch(down, up);
				}

				int vertical = 0;
				int horizontal = 0;

				if (useKeyboard) {
						if (inputHasFocus) {
								vertical += GetDirection(KeyCode.UpArrow, KeyCode.DownArrow);
								horizontal += GetDirection(KeyCode.RightArrow, KeyCode.LeftArrow);
						} else {
								vertical += GetDirection(KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
								horizontal += GetDirection(KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);
						}
				}

				if (useController) {
						if (!string.IsNullOrEmpty(verticalAxisName))
								vertical += GetDirection(verticalAxisName);
						if (!string.IsNullOrEmpty(horizontalAxisName))
								horizontal += GetDirection(horizontalAxisName);
				}

				// Send out key notifications
//		if (vertical != 0) mSel.SendMessage("OnKey", vertical > 0 ? KeyCode.UpArrow : KeyCode.DownArrow, SendMessageOptions.DontRequireReceiver);
//		if (horizontal != 0) mSel.SendMessage("OnKey", horizontal > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow, SendMessageOptions.DontRequireReceiver);
//		if (useKeyboard && Input.GetKeyDown(KeyCode.Tab)) mSel.SendMessage("OnKey", KeyCode.Tab, SendMessageOptions.DontRequireReceiver);
//		if (useController && Input.GetKeyUp(KeyCode.JoystickButton1)) mSel.SendMessage("OnKey", KeyCode.Escape, SendMessageOptions.DontRequireReceiver);

				currentTouch = null;
		}

		/// <summary>
		/// Process the events of the specified touch.
		/// </summary>

		void ProcessTouch(bool pressed, bool unpressed)
		{
				// Send out the press message
				if (pressed) {
						if (mTooltip != null)
								ShowTooltip(false);
						currentTouch.pressed = currentTouch.current;
						currentTouch.clickNotification = ClickNotification.Always;
						currentTouch.totalDelta = Vector2.zero;
						if (currentTouch.pressed != null)
								currentTouch.pressed.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);

						// Clear the selection
						if (currentTouch.pressed != mSel) {
								if (mTooltip != null)
										ShowTooltip(false);
								selectedObject = null;
						}
				} else if (currentTouch.pressed != null && currentTouch.delta.magnitude != 0f) {
						if (mTooltip != null)
								ShowTooltip(false);
						currentTouch.totalDelta += currentTouch.delta;

						bool isDisabled = (currentTouch.clickNotification == ClickNotification.None);
						currentTouch.pressed.SendMessage("OnDrag", currentTouch.delta, SendMessageOptions.DontRequireReceiver);

						if (isDisabled) {
								// If the notification status has already been disabled, keep it as such
								currentTouch.clickNotification = ClickNotification.None;
						} else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta) {
								// If the notification is based on delta and the delta gets exceeded, disable the notification
								float threshold = (currentTouch == mMouse[0]) ? mouseClickThreshold : Mathf.Max(touchClickThreshold, Screen.height * 0.1f);

								if (currentTouch.totalDelta.magnitude > threshold) {
										currentTouch.clickNotification = ClickNotification.None;
								}
						}
				}

				// Send out the unpress message
				if (unpressed) {
						if (mTooltip != null)
								ShowTooltip(false);

						if (currentTouch.pressed != null) {
								currentTouch.pressed.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);

								// Send a hover message to the object, but don't add it to the list of hovered items as it's already present
								// This happens when the mouse is released over the same button it was pressed on, and since it already had
								// its 'OnHover' event, it never got Highlight(false), so we simply re-notify it so it can update the visible state.
								if (currentTouch.pressed == mHover)
										currentTouch.pressed.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);

								// If the button/touch was released on the same object, consider it a click and select it
								if (currentTouch.pressed == currentTouch.current) {
										if (currentTouch.pressed != mSel) {
												mSel = currentTouch.pressed;
												currentTouch.pressed.SendMessage("OnSelect", true, SendMessageOptions.DontRequireReceiver);
										} else {
												mSel = currentTouch.pressed;
										}

										// If the touch should consider clicks, send out an OnClick notification
										if (currentTouch.clickNotification != ClickNotification.None) {
												double time = Frontiers.WorldClock.RealTime;

												currentTouch.pressed.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);

												if (currentTouch.clickTime + 0.25f > time) {
														currentTouch.pressed.SendMessage("OnDoubleClick", SendMessageOptions.DontRequireReceiver);
												}
												currentTouch.clickTime = (float)time;
										}
								} else { // The button/touch was released on a different object
										// Send a drop notification (for drag & drop)
										if (currentTouch.current != null)
												currentTouch.current.SendMessage("OnDrop", currentTouch.pressed, SendMessageOptions.DontRequireReceiver);
								}
						}
						currentTouch.pressed = null;
				}
		}

		/// <summary>
		/// Show or hide the tooltip.
		/// </summary>

		public void ShowTooltip(bool val)
		{
				mTooltipTime = 0f;
				if (mTooltip != null)
						mTooltip.SendMessage("OnTooltip", val, SendMessageOptions.DontRequireReceiver);
				if (!val)
						mTooltip = null;
		}
}