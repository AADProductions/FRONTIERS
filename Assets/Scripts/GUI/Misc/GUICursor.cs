using UnityEngine;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using System;

namespace Frontiers.GUI
{
	//TODO figure out how to get lock cursor working correctly on linux
	public class GUICursor : InterfaceActionFilter
	{
		public static GUICursor Get;
		public static bool gUseMouseLock = true;
		public UISprite SoftwareCursorSprite;
		public Texture2D CursorTexture;
		public Texture2D StackSplitTexture;
		public Texture2D StackQuickAddTexture;
		public float TargetOpacity = 1.0f;
		public float FadeTime = 1.0f;
		public float Alpha = 0.0f;
		public bool MoveCursorOnSelectGUIElements = false;
		public InterfaceActionFilter InterfaceActions;
		public Camera SearchCamera;
		public Vector3 ScreenSearchOrigin;
		public Vector3 ScreenSearchDirection;
		public Vector3 WorldSearchOrigin;
		public List <FrontiersInterface.Widget> CurrentObjects = new List <FrontiersInterface.Widget>();
		public List <WidgetSearch> CurrentSearches = new List<WidgetSearch>();

		public Bounds CurrentWidgetBounds {
			get {
				return CurrentWidget.ScreenBounds;
			}
		}

		public Bounds PreviousWidgetBounds {
			get {
				return PreviousWidget.ScreenBounds;
			}
		}

		public WidgetSearch CurrentWidget;
		public WidgetSearch PreviousWidget;
		public SearchDirection LastDirection;
		public Vector3 LastSearchDirection;
		public Vector3 LastMouseCursorResult;
		public FrontiersInterface LastInterface;
		public Bounds LastBrowserBounds;

		public void Start()
		{
			Subscribe(InterfaceActionType.SelectionRight, SelectionRight);
			Subscribe(InterfaceActionType.SelectionLeft, SelectionLeft);
			Subscribe(InterfaceActionType.SelectionUp, SelectionUp);
			Subscribe(InterfaceActionType.SelectionDown, SelectionDown);
			Behavior = PassThroughBehavior.PassThrough;

			SetCursorTexture("Default");
		}

		public override void WakeUp()
		{
			base.WakeUp();

			Screen.showCursor = false;
			Get = this;
		}

		public bool SelectionRight(double timeStamp)
		{
			if (WorldClock.RealTime > mLastInput + mPressInterval) {
				mLastInput = WorldClock.RealTime;
				if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.SupportsControllerSearch) {
					if (!SmartSearch(SearchDirection.Right)) {
						if (FindSearchOriginAndCamera()) {
							ScreenSearchDirection = SearchCamera.transform.right;
							FindnextObject(SearchDirection.Right);
						}
					}
				}
			}
			return true;
		}

		public bool SelectionLeft(double timeStamp)
		{
			if (GUIManager.Get.HasActiveInterface && WorldClock.RealTime > mLastInput + mPressInterval) {
				mLastInput = WorldClock.RealTime;
				if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.SupportsControllerSearch) {
					if (!SmartSearch(SearchDirection.Left)) {
						if (FindSearchOriginAndCamera()) {
							ScreenSearchDirection = -SearchCamera.transform.right;
							FindnextObject(SearchDirection.Left);
						}
					}
				}
			}
			return true;
		}

		public bool SelectionUp(double timeStamp)
		{
			if (WorldClock.RealTime > mLastInput + mPressInterval) {
				mLastInput = WorldClock.RealTime;
				if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.SupportsControllerSearch) {
					if (!SmartSearch(SearchDirection.Up)) {
						if (FindSearchOriginAndCamera()) {
							ScreenSearchDirection = SearchCamera.transform.up;
							FindnextObject(SearchDirection.Up);
						}
					}
				}
			}
			return true;
		}

		public bool SelectionDown(double timeStamp)
		{
			if (WorldClock.RealTime > mLastInput + mPressInterval) {
				mLastInput = WorldClock.RealTime;
				if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.SupportsControllerSearch) {
					if (!SmartSearch(SearchDirection.Down)) {
						if (FindSearchOriginAndCamera()) {
							ScreenSearchDirection = -SearchCamera.transform.up;
							FindnextObject(SearchDirection.Down);
						}
					}
				}
			}
			return true;
		}

		protected bool SmartSearch(SearchDirection direction)
		{
			LastDirection = direction;
			//try to find a next widget intelligently based on what we're looking at
			//this might save us the trouble of getting EVERY widget ever
			if (CurrentWidget.IsEmpty) {
				if (UICamera.hoveredObject != null) {
					SetCurrentWidget(UICamera.hoveredObject.GetComponent <BoxCollider>(), GUIManager.Get.ActiveCamera);
				}
			}
			//disabling smart search for now since we're actually getting better results with raw search
			//i may come back to this later as i improve navigation
			/*if (!CurrentWidget.IsEmpty) {
							if (CurrentWidget.Collider.CompareTag(Globals.TagBrowserObject)) {
								Debug.Log("Found browser object");
								GameObject nextBrowserObject = null;
								switch (direction) {
									case SearchDirection.Up:
										if (GUIBrowserObject.GetPrevBrowserObject(CurrentWidget.Collider.gameObject, out nextBrowserObject)) {
											Debug.Log("Got prev browser object " + nextBrowserObject.name);
										}
										break;

									case SearchDirection.Down:
										if (GUIBrowserObject.GetNextBrowserObject(CurrentWidget.Collider.gameObject, out nextBrowserObject)) {
											Debug.Log("Got prev browser object " + nextBrowserObject.name);
										}
										break;

									default:
									case SearchDirection.Left:
									case SearchDirection.Right:
																	//if we've already selected a browser object we don't want to select another one
										break;
								}

								if (nextBrowserObject != null) {
									SetCurrentWidget(nextBrowserObject.GetComponent <BoxCollider>(), GUIManager.Get.ActiveCamera);
									return true;
								} else {
									Debug.Log("Couldn't get browser object");
								}

							} else if (CurrentWidget.Collider.gameObject.HasComponent <GUITabButton>(out mTabButtonCheck)) {
								GUITabButton nextButton = null;
								if (mTabButtonCheck.TabParent.HorizontalButtons) {
									switch (direction) {
										case SearchDirection.Left:
											mTabButtonCheck.TabParent.GetPrevButton(mTabButtonCheck, out nextButton);
											break;

										case SearchDirection.Right:
											mTabButtonCheck.TabParent.GetNextButton(mTabButtonCheck, out nextButton);
											break;

										case SearchDirection.Down:
											if (mTabButtonCheck.TabParent.HasSubTabs) {
												Debug.Log("Has sub-tabs");
												GUITabs subTabs = null;
												bool foundButton = false;
												int maxTries = 100;
												int numTries = 0;
												while (nextButton == null && numTries < maxTries) {
													if (mTabButtonCheck.TabParent.GetNextSubTab(null, out subTabs)) {
														//get the first button in the sub tabs
														subTabs.GetNextButton(null, out nextButton);
													}
													numTries++;
												}
											}
											break;

										case SearchDirection.Up:
											if (mTabButtonCheck.TabParent.HasParentTabs) {
												mTabButtonCheck.TabParent.ParentTabs.GetNextButton(null, out nextButton);
											}
											break;
									}
								} else {
									switch (direction) {
										case SearchDirection.Up:
											if (!mTabButtonCheck.TabParent.GetPrevButton(mTabButtonCheck, out nextButton)) {
												if (mTabButtonCheck.TabParent.HasParentTabs) {
													mTabButtonCheck.TabParent.ParentTabs.GetNextButton(null, out nextButton);
												}
											}
											break;

										case SearchDirection.Down:
											mTabButtonCheck.TabParent.GetNextButton(mTabButtonCheck, out nextButton);
											break;

										default:
											break;
									}
								}

								if (nextButton != null) {
									SetCurrentWidget(nextButton.GetComponent <BoxCollider>(), GUIManager.Get.ActiveCamera);
									return true;
								}
							}
						}
						*/
			return false;
		}

		protected void DoSearch(SearchDirection direction)
		{
			if (mDoingSearch)
				return;

			mDoingSearch = true;

			CurrentSearches.Clear();
			FrontiersInterface.Widget nextObject = new FrontiersInterface.Widget();
			WidgetSearch search = new WidgetSearch();
			Bounds checkWidgetBounds = new Bounds();
			Vector3 widgetPos = Vector3.zero;
			Vector3 widgetDir = Vector3.zero;
			int maxDist = 1;
			int PrimaryAxis = 0;
			int secondaryAxis = 0;

			//get all the box colliders in the interface
			if (LastInterface == null) {
				//whoops, probably a premature call
				return;
			}

			switch (direction) {
				default:
				case SearchDirection.Up:
				case SearchDirection.Down:
					PrimaryAxis = Mathf.FloorToInt(CurrentWidgetBounds.center.y * maxDist);
					secondaryAxis = Mathf.FloorToInt(CurrentWidgetBounds.center.x * maxDist);
					break;

				case SearchDirection.Left:
				case SearchDirection.Right:
					PrimaryAxis = Mathf.FloorToInt(CurrentWidgetBounds.center.x * maxDist);
					secondaryAxis = Mathf.FloorToInt(CurrentWidgetBounds.center.y * maxDist);
					break;

			}
			CurrentObjects.Clear();
			LastInterface.GetActiveInterfaceObjects(CurrentObjects);
			//filter out any widgets that we don't want
			for (int i = CurrentObjects.LastIndex(); i >= 0; i--) {
				nextObject = CurrentObjects[i];
				if (nextObject.IsEmpty || !nextObject.Collider.enabled || !nextObject.Collider.gameObject.activeSelf || nextObject.Collider.gameObject.layer == Globals.LayerNumGUIRaycastIgnore) {
					CurrentObjects.RemoveAt(i);
				} else {
					//browser objects are handled by smart search
					//they gunk things up so ignore them unless they're visible
					widgetPos = SearchCamera.WorldToScreenPoint(nextObject.Collider.bounds.center);
					if (widgetPos.x < 0 || widgetPos.x > Screen.width || widgetPos.y < 0 || widgetPos.y > Screen.height) {
						CurrentObjects.RemoveAt(i);
					}
				}
			}
			for (int i = 0; i < CurrentObjects.Count; i++) {
				nextObject = CurrentObjects[i];
				if (nextObject.Collider == CurrentWidget.BoxCollider) {
					//skip any we don't want
					continue;
				}
				//get our search info from the object
				search.WorldBounds = nextObject.Collider.bounds;
				search.AttachedBrowser = nextObject.AttachedBrowser;
				widgetPos = nextObject.SearchCamera.WorldToScreenPoint(search.WorldBounds.center);
				widgetPos.z = 0f;
				search.ScreenBounds = new Bounds(widgetPos, search.WorldBounds.size);
				widgetDir = (widgetPos - ScreenSearchOrigin).normalized;

				switch (direction) {
					default:
					case SearchDirection.Up:
						search.Dot = Vector3.Dot(Vector3.up, widgetDir);
						search.PrimaryAxis = Mathf.FloorToInt(widgetPos.y * maxDist) - PrimaryAxis;
						search.SecondaryAxis = Mathf.FloorToInt(widgetPos.x * maxDist);
						if (search.SecondaryAxis < secondaryAxis) {
							search.SecondaryAxis = secondaryAxis - search.SecondaryAxis;
						} else {
							search.SecondaryAxis = search.SecondaryAxis - secondaryAxis;
						}
						break;

					case SearchDirection.Down:
						search.Dot = Vector3.Dot(Vector3.down, widgetDir);
						search.PrimaryAxis = PrimaryAxis - Mathf.FloorToInt(widgetPos.y * maxDist);
						search.SecondaryAxis = Mathf.FloorToInt(widgetPos.x * maxDist);
						if (search.SecondaryAxis < secondaryAxis) {
							search.SecondaryAxis = secondaryAxis - search.SecondaryAxis;
						} else {
							search.SecondaryAxis = search.SecondaryAxis - secondaryAxis;
						}
						break;

					case SearchDirection.Left:
						search.Dot = Vector3.Dot(Vector3.left, widgetDir);
						search.PrimaryAxis = PrimaryAxis - Mathf.FloorToInt(widgetPos.x * maxDist);
						search.SecondaryAxis = Mathf.FloorToInt(widgetPos.y * maxDist);
						if (search.SecondaryAxis < secondaryAxis) {
							search.SecondaryAxis = secondaryAxis - search.SecondaryAxis;
						} else {
							search.SecondaryAxis = search.SecondaryAxis - secondaryAxis;
						}
						break;

					case SearchDirection.Right:
						search.Dot = Vector3.Dot(Vector3.right, widgetDir);
						search.PrimaryAxis = Mathf.FloorToInt(widgetPos.x * maxDist) - PrimaryAxis;
						search.SecondaryAxis = Mathf.FloorToInt(widgetPos.y * maxDist);
						if (search.SecondaryAxis < secondaryAxis) {
							search.SecondaryAxis = secondaryAxis - search.SecondaryAxis;
						} else {
							search.SecondaryAxis = search.SecondaryAxis - secondaryAxis;
						}
						break;

				}
				if (search.PrimaryAxis > 0) {
					//don't include it if we're not in the right general direction
					search.Distance = Mathf.FloorToInt(Vector3.Distance(widgetPos, ScreenSearchOrigin) * maxDist);
					CurrentSearches.Add(search);
				}
			}
			//this will sort them so we can pick the best one
			//best one is at 0, worse one is last
			//we keep the rest for debugging purposes
			CurrentSearches.Sort();
			mDoingSearch = false;
		}

		public void FindnextObject(SearchDirection direction)
		{
			DoSearch(direction);
			//now that we have all our contenders
			//find the best possible option
			if (CurrentSearches.Count > 0) {
				SetCurrentWidget(CurrentSearches[0]);
			} else {
				CurrentWidget.IsEmpty = true;
				return;
			}

			if (VRManager.OculusModeEnabled) {
				//TODO
				//if we've selected a browser object and we're in oculus mode
				//move the browser object to the center of the browser
			}

			Vector3 widgetScreenPos = CurrentWidget.ScreenBounds.center;
			LastMouseCursorResult = CurrentWidget.WorldBounds.center;
			InterfaceActionManager.Get.SetMousePosition(Mathf.FloorToInt(widgetScreenPos.x), Mathf.FloorToInt(widgetScreenPos.y));
		}

		protected bool FindSearchOriginAndCamera()
		{
			if (GUIManager.Get.HasActiveInterface) {
				SearchCamera = GUIManager.Get.ActiveCamera;
			} else {
				SearchCamera = null;
				return false;
			}

			if (LastInterface == null || LastInterface != GUIManager.Get.TopInterface) {
				//if this interface isn't the same as the top interface
				//start over from the top right
				LastInterface = GUIManager.Get.TopInterface;
				SearchCamera = LastInterface.NGUICamera;
				CurrentWidget.IsEmpty = true;
				CurrentWidget.ScreenBounds = new Bounds(ScreenSearchOrigin, Vector3.one * 0.1f);
				if (VRManager.OculusModeEnabled) {
					//if oculus mode is enabled we're going to be helpful by moving the cursor around automatically
					//TODO move mouse to first item in new interface
				}
			}
			ScreenSearchOrigin = Input.mousePosition;
			ScreenSearchOrigin.z = 0f;
			WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(ScreenSearchOrigin);

			if (UICamera.hoveredObject != null) {
				if (CurrentWidget.IsEmpty || CurrentWidget.BoxCollider.gameObject != UICamera.hoveredObject.gameObject) {
					SetCurrentWidget(UICamera.hoveredObject.GetComponent <BoxCollider>(), GUIManager.Get.ActiveCamera);
				}
			}

			if (CurrentWidget.IsEmpty) {
				CurrentWidget.ScreenBounds = new Bounds(ScreenSearchOrigin, Vector3.one * 0.1f);
			}

			return true;
		}

		protected void SetCurrentWidget(BoxCollider newWidgetCollider, Camera searchCamera)
		{
			WidgetSearch w = new WidgetSearch();
			if (!w.IsEmpty) {
				w.ScreenBounds = newWidgetCollider.bounds;
				w.ScreenBounds.center = searchCamera.WorldToScreenPoint(w.ScreenBounds.center);
				SetCurrentWidget(w);
			}
		}

		protected void SetCurrentWidget(WidgetSearch newWidget)
		{
			PreviousWidget = CurrentWidget;
			CurrentWidget = newWidget;
			LastSearchDirection = CurrentWidgetBounds.center - ScreenSearchOrigin;
			if (SearchCamera != null) {
				//if the search has an attached scroll bar
				//that means we need to move the scroll bar until the cursor is within the browser bounds
				Vector3 widgetScreenPos = SearchCamera.WorldToScreenPoint(CurrentWidgetBounds.center);
				if (newWidget.AttachedBrowser != null) {
					//see if the attached item is visible
					Vector3 browserPosition = newWidget.AttachedBrowser.BrowserClipPanel.transform.position;
					Vector4 browserClipRange = newWidget.AttachedBrowser.BrowserClipPanel.clipRange;
					Vector3 browserScale = newWidget.AttachedBrowser.BrowserClipPanel.transform.lossyScale;
					browserPosition.x = browserPosition.x + (browserClipRange.x * browserScale.x);
					browserPosition.y = browserPosition.y + (browserClipRange.y * browserScale.y);
					Vector3 browserSize = new Vector3(browserClipRange.z * browserScale.x, browserClipRange.w * browserScale.y, 100f);
					LastBrowserBounds.center = browserPosition;
					LastBrowserBounds.size = browserSize;
					//that should put us in world space
					Debug.Log("Has attached browser at " + browserPosition.ToString());
					if (!LastBrowserBounds.Contains(CurrentWidgetBounds.center)) {
						Debug.Log("Current widget was NOT visible");

					}
				} else {
					Debug.Log("Had no attached browser");
				}
				InterfaceActionManager.Get.SetMousePosition(Mathf.FloorToInt(widgetScreenPos.x), Mathf.FloorToInt(widgetScreenPos.y));
			}
		}
		#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			Gizmos.color = Color.Lerp(Color.magenta, Color.cyan, 0.5f);
			Gizmos.DrawWireCube(LastBrowserBounds.center, LastBrowserBounds.size);

			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(ScreenSearchOrigin, 0.05f);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(LastMouseCursorResult, 0.05f);
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(CurrentWidgetBounds.center, CurrentWidgetBounds.size);
			Gizmos.DrawWireCube(CurrentWidgetBounds.center, CurrentWidgetBounds.size * 1.005f);
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(PreviousWidgetBounds.center, PreviousWidgetBounds.size);
			Gizmos.color = Color.cyan;
			switch (LastDirection) {
				default:
				case SearchDirection.Up:
					DrawArrow.ForGizmo(PreviousWidgetBounds.center, Vector3.up * 0.25f);
					break;

				case SearchDirection.Down:
					DrawArrow.ForGizmo(PreviousWidgetBounds.center, Vector3.down * 0.25f);
					break;

				case SearchDirection.Left:
					DrawArrow.ForGizmo(PreviousWidgetBounds.center, Vector3.left * 0.25f);
					break;

				case SearchDirection.Right:
					DrawArrow.ForGizmo(PreviousWidgetBounds.center, Vector3.right * 0.25f);

					break;

			}
			Gizmos.color = Color.magenta;
			if (LastSearchDirection != Vector3.zero) {
				DrawArrow.ForGizmo(PreviousWidgetBounds.center + Vector3.up * 0.25f, LastSearchDirection * 0.35f, 0.2f, 25f);
			}

			if (CurrentSearches.Count > 0) {
				//skip the first one
				Color searchColor = Color.green;
				for (int i = 0; i < CurrentSearches.Count; i++) {
					float normalizedAmount = (float)i / (float)(CurrentSearches.Count);
					int dot = Mathf.CeilToInt(CurrentSearches[i].Dot * WidgetSearch.gMaxDot);
					if (CurrentSearches[i].PrimaryAxis >= 0) {
						TextGizmo.Draw(SearchCamera, CurrentSearches[i].WorldBounds.center + Vector3.up * 0.01f, dot.ToString());
						TextGizmo.Draw(SearchCamera, CurrentSearches[i].WorldBounds.center - Vector3.up * 0.02f, CurrentSearches[i].Distance.ToString());
						TextGizmo.Draw(SearchCamera, CurrentSearches[i].WorldBounds.center - Vector3.up * 0.06f, CurrentSearches[i].PrimaryAxis.ToString());
					} else {
						TextGizmo.Draw(SearchCamera, CurrentSearches[i].WorldBounds.center, "NEGATIVE");
					}
					if (i == 0) {
						searchColor = Color.green;
						Gizmos.color = searchColor;
						Gizmos.DrawWireCube(CurrentSearches[i].WorldBounds.center, CurrentSearches[i].WorldBounds.size * 0.98f);
						Gizmos.DrawWireCube(CurrentSearches[i].WorldBounds.center, CurrentSearches[i].WorldBounds.size * 1.015f);
					} else {
						if (dot > WidgetSearch.gDotThreshold) {
							searchColor = Colors.Alpha(Colors.BlendThree(Color.Lerp(Color.green, Color.yellow, 0.2f), Color.yellow, Color.magenta, normalizedAmount), 0.5f);
						} else {
							searchColor = Color.red;
						}
					}
					Gizmos.color = searchColor;
					Gizmos.DrawCube(CurrentSearches[i].WorldBounds.center, CurrentSearches[i].WorldBounds.size);
					Gizmos.color = Colors.Alpha(searchColor, 0.5f);
					Gizmos.DrawWireCube(CurrentSearches[i].WorldBounds.center, CurrentSearches[i].WorldBounds.size * 0.99f);
				}
			}
		}
		#endif
		public void Update()
		{
			if (!GUIManager.ShowCursor) {
				LockCursor();
			} else {
				ReleaseCursor();
			}

			if (VRManager.OculusModeEnabled) {
				Screen.showCursor = false;
				if (GUIManager.ShowCursor) {
					SoftwareCursorSprite.enabled = true;
					Vector3 finalPosition = GUIManager.Get.BaseCamera.ScreenToWorldPoint(Input.mousePosition);
					finalPosition.z = -1f;
					SoftwareCursorSprite.transform.position = finalPosition;
				} else {
					SoftwareCursorSprite.enabled = false;
				}
			} else {
				Screen.showCursor = GUIManager.ShowCursor;
				SoftwareCursorSprite.enabled = false;
			}

			#if UNITY_EDITOR
			//i use these for testing
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				Debug.Log("Search up");
				ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				ScreenSearchOrigin.z = 0f;
				WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				DoSearch(SearchDirection.Up);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				Debug.Log("Search down");
				ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				ScreenSearchOrigin.z = 0f;
				WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				DoSearch(SearchDirection.Down);
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				Debug.Log("Search left");
				ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				ScreenSearchOrigin.z = 0f;
				WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				DoSearch(SearchDirection.Left);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow)) {
				Debug.Log("Search right");
				ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				ScreenSearchOrigin.z = 0f;
				WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
				DoSearch(SearchDirection.Right);
			}
			#endif
		}

		protected void LockCursor()
		{
			Screen.lockCursor = true;
		}

		protected void ReleaseCursor()
		{
			Screen.lockCursor = false;
		}

		public void SetCursorTexture(string cursorName)
		{
			//TODO put textures in a list
			switch (cursorName.ToLower()) {
				case "stacksplit":
					Cursor.SetCursor(StackSplitTexture, Vector2.zero, CursorMode.Auto);
					break;

				case "stackquickadd":
					Cursor.SetCursor(StackQuickAddTexture, Vector2.zero, CursorMode.Auto);
					break;

				default:
					Cursor.SetCursor(CursorTexture, Vector2.zero, CursorMode.Auto);
					break;
			}
		}

		protected double mLastInput = -1f;
		protected double mPressInterval = 0.35f;
		protected bool mDoingSearch = false;
		protected GUITabButton mTabButtonCheck = null;

		public enum SearchDirection
		{
			Up,
			Down,
			Left,
			Right
		}

		[Serializable]
		public struct WidgetSearch : IComparable <WidgetSearch>
		{
			public bool IsEmpty {
				get {
					return BoxCollider == null;
				}
				set {
					if (value) {
						BoxCollider = null;
					}
				}
			}

			public Collider BoxCollider;
			public Bounds ScreenBounds;
			public Bounds WorldBounds;
			public IGUIBrowser AttachedBrowser;
			public int PrimaryAxis;
			public int SecondaryAxis;
			public int Distance;
			public float Dot;

			public int CompareTo(WidgetSearch other)
			{
				//super explicit version
				int thisDot = Mathf.CeilToInt(Dot * gMaxDot);
				int otherDot = Mathf.CeilToInt(other.Dot * gMaxDot);
				int distanceDelta = Mathf.Abs(Distance - other.Distance);
				if (thisDot > gDotThreshold) {
					if (PrimaryAxis < other.PrimaryAxis) {
						//almost instant win
						if (Distance < other.Distance) {
							return -1;
						} else if (Distance == other.Distance) {
							//not quite a lose
							//still closer than other dot matches
							return 0;
						} else {
							//oops we suck
							return 1;
						}
					}
					//we're already in good shape
					if (thisDot > otherDot) {
						//we have the advantage
						//unless we're way farther away
						if (Distance >= other.Distance) {
							if (distanceDelta < gDistanceDeltaThreshold) {
								return 1;
							} else {
								//if we're below the threshold, win
								return -1;
							}
						} else {
							//if we're closer and our dot is bigger, win
							return -1;
						}
					} else if (thisDot == otherDot) {
						//if it's the same dot distance automatically wins
						if (Distance >= other.Distance) {
							return 1;
						} else {
							return -1;
						}
					} else {// if (thisDot < otherDot) {
						//if our dot is less, we may still have an advantage
						//if the difference between distances is great enough
						if (Distance < gMinOverrideDistance) {
							return -1;
						} else {
							if (Distance < other.Distance) {
								return -1;
							} else {
								return 1;
							}
						}
					}
				} else if (otherDot < gDotThreshold) {
					//auto fail
					return 1;
				} else {
					return 0;
				}
			}

			public static int gMaxDot = 100;
			public static int gDotThreshold = 80;
			public static int gDistanceDeltaThreshold = 50;
			public static int gMinOverrideDistance = 50;
		}
	}
}