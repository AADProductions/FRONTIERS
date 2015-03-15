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
				public FrontiersInterface CurrentActiveInterface;
				public Camera SearchCamera;
				public Vector3 ScreenSearchOrigin;
				public Vector3 ScreenSearchDirection;
				public Vector3 WorldSearchOrigin;
				public List <FrontiersInterface.Widget> CurrentObjects = new List <FrontiersInterface.Widget>();
				public List <WidgetSearch> CurrentSearches = new List<WidgetSearch>();
				public float gDistanceWeight = 1f;
				public float gPrimaryAxisWeight = 1.75f;
				public float gPrimaryOverlapWeight = 1.75f;
				public float gSecondaryAxisWeight = 0.25f;
				public float gDotWeight = 0.75f;
				public int gDotThreshold = 75;

				public Bounds CurrentWidgetBounds {
						get {
								return CurrentSearch.ScreenBounds;
						}
				}

				public Bounds PreviousSearchBounds {
						get {
								return PreviousSearch.ScreenBounds;
						}
				}

				public WidgetSearch CurrentSearch;
				public WidgetSearch PreviousSearch;
				public SearchDirection LastDirection;
				public Vector3 LastSearchDirection;
				public Vector3 LastMouseCursorResult;
				public Bounds LastBrowserBounds;

				public bool ReadyForNextPress {
						get {
								double timeSinceLastInput = WorldClock.RealTime - mLastInput;
								if (timeSinceLastInput > mPressInterval * 1.1f) {
										mHolding = false;
										mStartHold = -1f;
								} else {
										//see if we've held long enough
										if (mStartHold > 0) {
												if (WorldClock.RealTime > mStartHold + mHoldInterval) {
														mHolding = true;
												}
										} else {
												mStartHold = WorldClock.RealTime;
										}
								}
								//if we've been holding it for a while return quick interval, otherwise normal interval
								if (mHolding) {
										return WorldClock.RealTime > mLastInput + mPressIntervalShort;
								} else {
										return WorldClock.RealTime > mLastInput + mPressInterval;
								}
						}
				}

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
						if (ReadyForNextPress) {
								mLastInput = WorldClock.RealTime;
								if (FindSearchOriginAndCamera(true)) {
										ScreenSearchDirection = SearchCamera.transform.right;
										FindNextObject(SearchDirection.Right);
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
								}
						}
						return true;
				}

				public bool SelectionLeft(double timeStamp)
				{
						if (ReadyForNextPress) {
								mLastInput = WorldClock.RealTime;
								if (FindSearchOriginAndCamera(true)) {
										ScreenSearchDirection = -SearchCamera.transform.right;
										FindNextObject(SearchDirection.Left);
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
								}
						}
						return true;
				}

				public bool SelectionUp(double timeStamp)
				{
						if (ReadyForNextPress) {
								mLastInput = WorldClock.RealTime;
								if (FindSearchOriginAndCamera(true)) {
										ScreenSearchDirection = SearchCamera.transform.up;
										FindNextObject(SearchDirection.Up);
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
								}
						}
						return true;
				}

				public bool SelectionDown(double timeStamp)
				{
						if (ReadyForNextPress) {
								mLastInput = WorldClock.RealTime;
								if (FindSearchOriginAndCamera(true)) {
										ScreenSearchDirection = -SearchCamera.transform.up;
										FindNextObject(SearchDirection.Down);
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
								}
						}
						return true;
				}

				public void SelectWidget(FrontiersInterface.Widget w)
				{
						if (FindSearchOriginAndCamera(false)) {
								WidgetSearch s = new WidgetSearch(w);
								if (!w.IsEmpty) {
										s.WorldBounds = w.BoxCollider.bounds;
										s.ScreenBounds.min = w.SearchCamera.WorldToScreenPoint(s.WorldBounds.min);
										s.ScreenBounds.max = w.SearchCamera.WorldToScreenPoint(s.WorldBounds.max);
										s.ScreenBounds.center = w.SearchCamera.WorldToScreenPoint(s.WorldBounds.center);
										SetCurrentSearch(s);
								}
						}
				}

				protected void DumbSearch(SearchDirection direction)
				{
						if (mDoingSearch)
								return;

						mDoingSearch = true;

						CurrentObjects.Clear();
						//if the cutscene is active, see if there are any cutscene interfaces
						if (Cutscene.IsActive) {
								Cutscene.GetActiveInterfaceObjects(CurrentObjects);
						}

						CurrentSearches.Clear();
						LastDirection = direction;
						FrontiersInterface.Widget nextObject = new FrontiersInterface.Widget();
						WidgetSearch search;
						Bounds checkWidgetBounds = new Bounds();
						Vector3 widgetPos = Vector3.zero;
						Vector3 widgetDir = Vector3.zero;
						Vector3 primaryMax = SearchCamera.WorldToScreenPoint(CurrentSearch.WorldBounds.max);
						Vector3 primaryMin = SearchCamera.WorldToScreenPoint(CurrentSearch.WorldBounds.min);
						Vector3 searchMin = Vector3.zero;
						Vector3 searchMax = Vector3.zero;
						float primaryAxis = 0;
						float secondaryAxis = 0;
						float currentSize = 0f;
						float searchSize = 0f;
						float smallestProportion = 0f;

						switch (direction) {
								default:
								case SearchDirection.Up:
								case SearchDirection.Down:
										primaryAxis = CurrentSearch.ScreenBounds.center.y;
										secondaryAxis = CurrentSearch.ScreenBounds.center.x;
										currentSize = primaryMax.x - primaryMin.x;
										break;

								case SearchDirection.Left:
								case SearchDirection.Right:
										primaryAxis = CurrentSearch.ScreenBounds.center.x;
										secondaryAxis = CurrentSearch.ScreenBounds.center.y;
										currentSize = primaryMax.y - primaryMin.y;
										break;

						}
						if (CurrentActiveInterface != null) {
								CurrentActiveInterface.GetActiveInterfaceObjects(CurrentObjects);
						}

						for (int i = 0; i < CurrentObjects.Count; i++) {
								nextObject = CurrentObjects[i];
								if (nextObject.BoxCollider == CurrentSearch.Widget.BoxCollider || nextObject.BoxCollider == null || !nextObject.BoxCollider.enabled) {
										//skip any we don't want
										continue;
								}
								widgetPos = SearchCamera.WorldToScreenPoint(nextObject.BoxCollider.bounds.center);
								if (widgetPos.x < 0 || widgetPos.x > Screen.width || widgetPos.y < 0 || widgetPos.y > Screen.height) {
										//skip any that aren't on screen
										continue;
								}
								//get our search info from the object
								search = new WidgetSearch(nextObject);
								search.WorldBounds = search.Widget.BoxCollider.bounds;
								widgetPos.z = 0f;
								widgetDir = (widgetPos - ScreenSearchOrigin).normalized;
								searchMin = SearchCamera.WorldToScreenPoint(search.WorldBounds.min);
								searchMax = SearchCamera.WorldToScreenPoint(search.WorldBounds.max);
								search.ScreenBounds.min = searchMin;
								search.ScreenBounds.max = searchMax;
								search.ScreenBounds.center = widgetPos;

								switch (direction) {
										default:
										case SearchDirection.Up:
												search.PrimaryAxis = Mathf.Clamp(widgetPos.y - primaryAxis, 0, WidgetSearch.gMaxPrimaryAxis);
												if (search.PrimaryAxis < 0) {
														break;
												}
												search.Dot = Vector3.Dot(Vector3.up, widgetDir) * WidgetSearch.gMaxDot;
												if (search.Dot < WidgetSearch.gDotThreshold) {
														break;
												}

												searchSize = searchMax.x - searchMin.x;
												search.SecondaryAxis = widgetPos.x;
												if (search.SecondaryAxis < secondaryAxis) {
														search.SecondaryAxis = Mathf.Clamp(secondaryAxis - search.SecondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												} else {
														search.SecondaryAxis = Mathf.Clamp(search.SecondaryAxis - secondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												}
												if (primaryMax.x < searchMin.x || searchMax.x < primaryMin.x) {
														search.PrimaryOverlap = 0;
												} else {
														if (searchSize < currentSize) {
																smallestProportion = searchSize / currentSize;
														} else {
																smallestProportion = currentSize / searchSize;
														}
														search.PrimaryOverlap = (Mathf.Clamp(primaryMax.x - searchMin.x, 0, searchSize) / searchSize) * smallestProportion;
												}
												search.Distance = Mathf.Clamp(Vector3.Distance(widgetPos, ScreenSearchOrigin), 0, WidgetSearch.gMaxDist);
												search.FinalScore = WidgetSearch.CalculateScore(search);
												CurrentSearches.Add(search);
												break;

										case SearchDirection.Down:
												search.PrimaryAxis = Mathf.Clamp(primaryAxis - widgetPos.y, 0, WidgetSearch.gMaxPrimaryAxis);
												if (search.PrimaryAxis < 0) {
														break;
												}
												search.Dot = Vector3.Dot(Vector3.down, widgetDir) * WidgetSearch.gMaxDot;
												if (search.Dot < WidgetSearch.gDotThreshold) {
														break;
												}
												searchSize = searchMax.x - searchMin.x;
												search.SecondaryAxis = widgetPos.x;
												if (search.SecondaryAxis < secondaryAxis) {
														search.SecondaryAxis = Mathf.Clamp(secondaryAxis - search.SecondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												} else {
														search.SecondaryAxis = Mathf.Clamp(search.SecondaryAxis - secondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												}
												if (primaryMax.x < searchMin.x || searchMax.x < primaryMin.x) {
														search.PrimaryOverlap = 0;
												} else {
														if (searchSize < currentSize) {
																smallestProportion = searchSize / currentSize;
														} else {
																smallestProportion = currentSize / searchSize;
														}
														search.PrimaryOverlap = (Mathf.Clamp(primaryMax.x - searchMin.x, 0, searchSize) / searchSize) * smallestProportion;
												}
												search.Distance = Mathf.Clamp(Vector3.Distance(widgetPos, ScreenSearchOrigin), 0, WidgetSearch.gMaxDist);
												search.FinalScore = WidgetSearch.CalculateScore(search);
												CurrentSearches.Add(search);
												break;

										case SearchDirection.Left:
												search.PrimaryAxis = Mathf.Clamp(primaryAxis - widgetPos.x, 0, WidgetSearch.gMaxPrimaryAxis);
												if (search.PrimaryAxis < 0) {
														break;
												}
												search.Dot = Vector3.Dot(Vector3.left, widgetDir) * WidgetSearch.gMaxDot;
												if (search.Dot < WidgetSearch.gDotThreshold) {
														break;
												}
												searchSize = searchMax.y - searchMin.y;
												search.SecondaryAxis = widgetPos.y;
												if (search.SecondaryAxis < secondaryAxis) {
														search.SecondaryAxis = Mathf.Clamp(secondaryAxis - search.SecondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												} else {
														search.SecondaryAxis = Mathf.Clamp(search.SecondaryAxis - secondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												}
												if (primaryMax.y < searchMin.y || searchMax.y < primaryMin.y) {
														search.PrimaryOverlap = 0;
												} else {
														if (searchSize < currentSize) {
																smallestProportion = searchSize / currentSize;
														} else {
																smallestProportion = currentSize / searchSize;
														}
														search.PrimaryOverlap = (Mathf.Clamp(primaryMax.y - searchMin.y, 0, searchSize) / searchSize) * smallestProportion;
												}
												search.Distance = Mathf.Clamp(Vector3.Distance(widgetPos, ScreenSearchOrigin), 0, WidgetSearch.gMaxDist);
												search.FinalScore = WidgetSearch.CalculateScore(search);
												CurrentSearches.Add(search);
												break;

										case SearchDirection.Right:
												search.PrimaryAxis = Mathf.Clamp(widgetPos.x - primaryAxis, 0, WidgetSearch.gMaxPrimaryAxis);
												if (search.PrimaryAxis < 0) {
														break;
												}
												search.Dot = Vector3.Dot(Vector3.right, widgetDir) * WidgetSearch.gMaxDot;
												if (search.Dot < WidgetSearch.gDotThreshold) {
														break;
												}
												searchSize = searchMax.y - searchMin.y;
												search.SecondaryAxis = widgetPos.y;
												if (search.SecondaryAxis < secondaryAxis) {
														search.SecondaryAxis = Mathf.Clamp(secondaryAxis - search.SecondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												} else {
														search.SecondaryAxis = Mathf.Clamp(search.SecondaryAxis - secondaryAxis, 0, WidgetSearch.gMaxSecondaryAxis);
												}
												if (primaryMax.y < searchMin.y || searchMax.y < primaryMin.y) {
														search.PrimaryOverlap = 0;
												} else {
														//overlap is a normalized percentage of overlap multiplied by the proportion of one button size to the other
														if (searchSize < currentSize) {
																smallestProportion = searchSize / currentSize;
														} else {
																smallestProportion = currentSize / searchSize;
														}
														search.PrimaryOverlap = (Mathf.Clamp(primaryMax.y - searchMin.y, 0, searchSize) / searchSize) * smallestProportion;
												}
												search.Distance = Mathf.Clamp(Vector3.Distance(widgetPos, ScreenSearchOrigin), 0, WidgetSearch.gMaxDist);
												search.FinalScore = WidgetSearch.CalculateScore(search);
												CurrentSearches.Add(search);
												break;

								}
						}
						//this will sort them so we can pick the best one
						//best one is at 0, worse one is last
						//we keep the rest for debugging purposes
						CurrentSearches.Sort();
						mDoingSearch = false;
				}

				public void FindNextObject(SearchDirection direction)
				{
						DumbSearch(direction);
						if (CurrentSearches.Count > 0) {
								//search for the first widget that ISN'T the current widget
								if (CurrentSearch.Widget.IsEmpty) {
										SetCurrentSearch(CurrentSearches[0]);
								} else {
										for (int i = 0; i < CurrentSearches.Count; i++) {
												if (!CurrentSearches[i].Widget.IsEmpty && CurrentSearches[i].Widget.BoxCollider != CurrentSearch.Widget.BoxCollider) {
														SetCurrentSearch(CurrentSearches[i]);
														break;
												}
										}
								}
						} else {
								CurrentSearch.Widget.IsEmpty = true;
								return;
						}

						if (VRManager.VRModeEnabled) {
								//TODO
								//if we've selected a browser object and we're in oculus mode
								//move the browser object to the center of the browser
						}
						//CopyMousePositionFromCurrentSearch();
				}

				protected bool FindSearchOriginAndCamera(bool autoSetFirstObject)
				{
						if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.SupportsControllerSearch) {
								SearchCamera = GUIManager.Get.ActiveCamera;
								if (CurrentActiveInterface == null || CurrentActiveInterface != GUIManager.Get.TopInterface) {
										//if this interface isn't the same as the top interface
										//start over from the top right
										FrontiersInterface lastInterface = CurrentActiveInterface;
										CurrentActiveInterface = GUIManager.Get.TopInterface;
										if (lastInterface != CurrentActiveInterface && autoSetFirstObject) {
												//get the first interface element
												WidgetSearch s = new WidgetSearch(CurrentActiveInterface.FirstInterfaceObject);
												if (!s.Widget.IsEmpty) {
														SetCurrentSearch(s);
														return false;
												}
										}
										CurrentSearch.Widget = CurrentActiveInterface.FirstInterfaceObject;
										CurrentSearch.ScreenBounds = new Bounds(ScreenSearchOrigin, Vector3.one * 0.1f);
								}
						} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
								SearchCamera = Cutscene.CurrentCutscene.ActiveCamera;
						} else {
								SearchCamera = null;
								return false;
						}

						ScreenSearchOrigin = Input.mousePosition;
						ScreenSearchOrigin.z = 0f;
						WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(ScreenSearchOrigin);

						if (UICamera.hoveredObject != null && (CurrentSearch.Widget.IsEmpty || CurrentSearch.Widget.BoxCollider.gameObject != UICamera.hoveredObject.gameObject)) {
								WidgetSearch s = new WidgetSearch();
								s.Widget.BoxCollider = UICamera.hoveredObject.GetComponent <Collider>();
								s.Widget.SearchCamera = GUIManager.Get.ActiveCamera;
								SetCurrentSearch(s);
						}

						if (CurrentSearch.Widget.IsEmpty) {
								CurrentSearch.ScreenBounds = new Bounds(ScreenSearchOrigin, Vector3.one * 0.1f);
						}

						return true;
				}

				protected void SetCurrentSearch(WidgetSearch newSearch)
				{
						PreviousSearch = CurrentSearch;
						CurrentSearch = newSearch;
						LastSearchDirection = CurrentWidgetBounds.center - ScreenSearchOrigin;

						//if the search has an attached scroll bar
						//that means we need to move the scroll bar until the cursor is within the browser bounds
						if (CurrentSearch.Widget.IsBrowserObject) {
								//if we're moving to another widget in the same browser object, just go to the next one
								if (PreviousSearch.Widget.IsBrowserObject
								    && PreviousSearch.Widget.BrowserObject != CurrentSearch.Widget.BrowserObject
								    && (PreviousSearch.Widget.BrowserObject.ParentBrowser == CurrentSearch.Widget.BrowserObject.ParentBrowser)) {
										LastBrowserBounds = CurrentSearch.Widget.BrowserObject.ParentBrowser.FocusOn(CurrentSearch.Widget.BrowserObject);
								} else {
										//if our last widget was not a browser object, get the first available
										//if the first available is on-screen, use that
										FrontiersInterface.Widget w = CurrentSearch.Widget.BrowserObject.ParentBrowser.FirstInterfaceObject;
										if (!w.IsEmpty && w.IsBrowserObject && CurrentSearch.Widget.BrowserObject.ParentBrowser.IsBrowserObjectVisible(w.BrowserObject, 1.0f, out LastBrowserBounds)) {
												CurrentSearch.Widget = w;
												LastBrowserBounds = CurrentSearch.Widget.BrowserObject.ParentBrowser.FocusOn(CurrentSearch.Widget.BrowserObject);
										}
										//in either case, focus on the result
										LastBrowserBounds = CurrentSearch.Widget.BrowserObject.ParentBrowser.FocusOn(CurrentSearch.Widget.BrowserObject);
								}
						}
						//recalculate the widget screen bounds just in case we moved it
						CurrentSearch.WorldBounds = CurrentSearch.Widget.BoxCollider.bounds;
						Vector3 widgetPos = SearchCamera.WorldToScreenPoint(CurrentSearch.WorldBounds.center);
						widgetPos.z = 0f;
						CurrentSearch.ScreenBounds = new Bounds(widgetPos, CurrentSearch.WorldBounds.size);
						CopyMousePositionFromCurrentSearch();
						UICamera.selectedObject = CurrentSearch.Widget.BoxCollider.gameObject;
				}

				protected void CopyMousePositionFromCurrentSearch()
				{
						Vector3 widgetScreenPos = SearchCamera.WorldToScreenPoint(CurrentSearch.WorldBounds.center);
						LastMouseCursorResult = CurrentSearch.WorldBounds.center;
						InterfaceActionManager.Get.SetMousePosition(Mathf.FloorToInt(widgetScreenPos.x), Mathf.FloorToInt(widgetScreenPos.y));
				}
				#if UNITY_EDITOR
				public void OnDrawGizmos()
				{
						for (int i = 0; i < CurrentSearches.Count; i++) {
								WidgetSearch w = CurrentSearches[i];
								w.FinalScore = WidgetSearch.CalculateScore(w);
								CurrentSearches[i] = w;
						}
						CurrentSearches.Sort();

						Gizmos.color = Color.Lerp(Color.magenta, Color.cyan, 0.5f);
						Gizmos.DrawWireCube(LastBrowserBounds.center, LastBrowserBounds.size);

						if (LastDirection == SearchDirection.Left || LastDirection == SearchDirection.Right) {
								Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
								Gizmos.DrawLine(CurrentSearch.WorldBounds.max + (Vector3.left * 100f), CurrentSearch.WorldBounds.max + (Vector3.right * 100f));
								Gizmos.color = Color.blue;
								Gizmos.DrawLine(CurrentSearch.WorldBounds.min + (Vector3.left * 100f), CurrentSearch.WorldBounds.min + (Vector3.right * 100f));
						} else {
								Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
								Gizmos.DrawLine(CurrentSearch.WorldBounds.max + (Vector3.down * 100f), CurrentSearch.WorldBounds.max + (Vector3.up * 100f));
								Gizmos.color = Color.blue;
								Gizmos.DrawLine(CurrentSearch.WorldBounds.min + (Vector3.down * 100f), CurrentSearch.WorldBounds.min + (Vector3.up * 100f));
						}

						Gizmos.color = Color.yellow;
						Gizmos.DrawSphere(ScreenSearchOrigin, 0.05f);
						Gizmos.color = Color.red;
						Gizmos.DrawSphere(LastMouseCursorResult, 0.05f);
						Gizmos.color = Color.green;
						Gizmos.DrawWireCube(CurrentWidgetBounds.center, CurrentWidgetBounds.size);
						Gizmos.DrawWireCube(CurrentWidgetBounds.center, CurrentWidgetBounds.size * 1.005f);
						Gizmos.color = Color.red;
						Gizmos.DrawWireCube(PreviousSearchBounds.center, PreviousSearchBounds.size);
						Gizmos.color = Color.cyan;
						switch (LastDirection) {
								default:
								case SearchDirection.Up:
										DrawArrow.ForGizmo(PreviousSearchBounds.center, Vector3.up * 0.25f);
										break;

								case SearchDirection.Down:
										DrawArrow.ForGizmo(PreviousSearchBounds.center, Vector3.down * 0.25f);
										break;

								case SearchDirection.Left:
										DrawArrow.ForGizmo(PreviousSearchBounds.center, Vector3.left * 0.25f);
										break;

								case SearchDirection.Right:
										DrawArrow.ForGizmo(PreviousSearchBounds.center, Vector3.right * 0.25f);

										break;

						}
						Gizmos.color = Color.magenta;
						if (LastSearchDirection != Vector3.zero) {
								DrawArrow.ForGizmo(PreviousSearchBounds.center + Vector3.up * 0.25f, LastSearchDirection * 0.35f, 0.2f, 25f);
						}

						if (CurrentSearches.Count > 0) {
								//skip the first one
								Color searchColor = Color.green;
								for (int i = 0; i < CurrentSearches.Count; i++) {
										WidgetSearch s = CurrentSearches[i];
										float normalizedAmount = (float)i / (float)(CurrentSearches.Count);
										TextGizmo.Draw(SearchCamera, s.WorldBounds.center + Vector3.up * 0.01f, Mathf.FloorToInt(s.FinalScore * 1000).ToString());
										//TextGizmo.Draw(SearchCamera, s.WorldBounds.center + Vector3.up * 0.01f, Mathf.FloorToInt(s.PrimaryOverlap).ToString());
										if (i == 0) {
												searchColor = Color.green;
												Gizmos.color = searchColor;
												Gizmos.DrawWireCube(s.WorldBounds.center, s.WorldBounds.size * 0.98f);
												Gizmos.DrawWireCube(s.WorldBounds.center, s.WorldBounds.size * 1.015f);
										} else {
												if (s.Dot * WidgetSearch.gDotWeight > WidgetSearch.gDotThreshold) {
														searchColor = Colors.Alpha(Colors.BlendThree(Color.Lerp(Color.green, Color.yellow, 0.2f), Color.yellow, Color.magenta, normalizedAmount), 0.5f);
												} else {
														searchColor = Color.red;
												}
										}
										Gizmos.color = searchColor;
										Gizmos.DrawCube(s.WorldBounds.center, s.WorldBounds.size);
										Gizmos.color = Colors.Alpha(searchColor, 0.5f);
										Gizmos.DrawWireCube(s.WorldBounds.center, s.WorldBounds.size * 0.99f);
								}
						}
				}
				#endif
				public void Update()
				{
						WidgetSearch.gDistanceWeight = gDistanceWeight;
						WidgetSearch.gPrimaryAxisWeight = gPrimaryAxisWeight;
						WidgetSearch.gPrimaryOverlapWeight = gPrimaryOverlapWeight;
						WidgetSearch.gSecondaryAxisWeight = gSecondaryAxisWeight;
						WidgetSearch.gDotWeight = gDotWeight;
						WidgetSearch.gDotThreshold = gDotThreshold;

						if (!GUIManager.ShowCursor) {
								LockCursor();
						} else {
								ReleaseCursor();
						}

						if (VRManager.VRModeEnabled) {
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
								DumbSearch(SearchDirection.Up);
						}
						if (Input.GetKeyDown(KeyCode.DownArrow)) {
								Debug.Log("Search down");
								ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								ScreenSearchOrigin.z = 0f;
								WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								DumbSearch(SearchDirection.Down);
						}
						if (Input.GetKeyDown(KeyCode.LeftArrow)) {
								Debug.Log("Search left");
								ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								ScreenSearchOrigin.z = 0f;
								WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								DumbSearch(SearchDirection.Left);
						}
						if (Input.GetKeyDown(KeyCode.RightArrow)) {
								Debug.Log("Search right");
								ScreenSearchOrigin = Input.mousePosition;//SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								ScreenSearchOrigin.z = 0f;
								WorldSearchOrigin = SearchCamera.ScreenToWorldPoint(Input.mousePosition);
								DumbSearch(SearchDirection.Right);
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

				protected double mStartHold;
				protected double mHoldInterval = 1.25f;
				protected double mLastInput = -1f;
				protected double mPressIntervalShort = 0.1f;
				protected double mPressInterval = 0.275f;
				protected bool mHolding = true;
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
						public WidgetSearch(FrontiersInterface.Widget w)
						{
								Widget = w;
								ScreenBounds = gEmptyBounds;
								WorldBounds = gEmptyBounds;
								PrimaryOverlap = 0f;
								PrimaryAxis = 0;
								SecondaryAxis = 0;
								Distance = 0;
								Dot = 0;
								FinalScore = Mathf.NegativeInfinity;
						}

						public FrontiersInterface.Widget Widget;
						public Bounds ScreenBounds;
						public Bounds WorldBounds;
						public float PrimaryAxis;
						public float PrimaryOverlap;
						public float SecondaryAxis;
						public float Distance;
						public float Dot;
						public float FinalScore;

						public static float CalculateScore(WidgetSearch s)
						{
								float finalScore =
										((1f - (s.Distance / gMaxDist)) * gDistanceWeight) +
										((1f - (s.PrimaryAxis / gMaxPrimaryAxis)) * gPrimaryAxisWeight) +
										(s.PrimaryOverlap * gPrimaryOverlapWeight) +
										((1f - (s.SecondaryAxis / gMaxSecondaryAxis)) * gSecondaryAxisWeight) +
										((s.Dot / gMaxDot) * gDotWeight);
								return finalScore;
						}

						public int CompareTo(WidgetSearch other)
						{		
								return other.FinalScore.CompareTo(FinalScore);								
						}

						public static int gMaxDot = 100;
						public static int gMaxDist = 2000;
						public static int gMaxPrimaryAxis = 1000;
						public static int gMaxSecondaryAxis = 1000;
						public static int gDotThreshold = 75;
						public static int gDistanceDeltaThreshold = 50;
						public static int gMinOverrideDistance = 50;
						public static float gDistanceWeight = 1f;
						public static float gPrimaryAxisWeight = 1.75f;
						public static float gPrimaryOverlapWeight = 1f;
						public static float gSecondaryAxisWeight = 0.25f;
						public static float gDotWeight = 0.75f;
						public static Bounds gEmptyBounds;
				}
		}
}