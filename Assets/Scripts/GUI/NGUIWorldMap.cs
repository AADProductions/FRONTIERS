using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;
using ExtensionMethods;
using System;

namespace Frontiers.GUI
{
	public class NGUIWorldMap : PrimaryInterface
	{
		//TODO this class has gotten massive
		//break it up into a couple of sub classes
		public static NGUIWorldMap Get;
		//movement, scaling, dragging
		public Camera MapBackgroundCamera;
		public Camera PlayerIconCamera;
		public PainterlyImageEffect ImageEffect;
		public PostEffectsBase VignetteEffect;
		public GameObject WorldMapMarkerAvatarPrefab;
		public GameObject PlayerMapMarkerAvatar;
		public List <GameObject> MarkedAvatars = new List<GameObject> ();
		public UILabel BuildingPathsLabel;
		public string SearchFilter;
		public float ScaleCurrent = 0.5f;
		public float ScaleMin = 0.1f;
		public float ScaleMax = 2.0f;
		public float ScrollSpeed = 4f;
		public float LabelScale = 35f;
		public float MaxIconDistanceFromCamera = 10f;
		public float MinIconScale = 0.0001f;
		public float MaxIconScale = 1f;
		public AnimationCurve ScaleCurve;
		public float ScaleTargetNormalized = 1f;
		public float LocationRadiusMax = 500f;
		public float LocationRadiusMin = 5f;
		public float NormalizedOpacityRange = 0.1f;
		public float NormalizedFadeRange = 0.1f;
		public float LocationScaleMultiplier = 1.0f;
		public float LocationMinZDistance = 10f;
		public float LocationMaxZDistance = 100f;
		public float PlayerPositionScaleMultiplier = 1.0f;
		public float ScrollWheelSensitivity = 0.1f;
		public UIPanel LabelsPanel;
		public UIPanel IconsPanel;
		public UIPanel MarkersPanel;
		public Transform ScaleObject;
		public Transform DragObject;
		public Transform MapCameraParent;
		public Transform MapCameraTransform;
		public Transform MapCameraPositionTransform;
		public Vector3 MapCameraRotation = new Vector3 (-40f, 0f, 0f);
		public Vector3 MapCameraMinPosition;
		public Vector3 MapCameraMaxPosition;
		public Bounds NavigationBounds;
		//display
		public List <GUIMapTile> ChunksDisplayed = new List <GUIMapTile> ();
		public List <GUIMapTile> EmptyChunkTiles = new List <GUIMapTile> ();
		public List <UISprite> MapMarkersDisplayed = new List<UISprite> ();
		public GameObject TilesParent;
		public GameObject WMTilePrefab;
		public Transform PlayerPosition;
		public Transform PlayerPositionSprite;
		public UIInput SearchFieldInput;
		public Renderer TilesBackground;
		public Renderer CloudsRenderer;
		public GUIMapTile[,] ChunkTileSet;
		public GameObject WMMarkPrefab;
		//movement
		public float MaxCameraDistanceFromDragObject = 2f;
		public float MoveForwardAmount;
		public float RotateAmount;
		public float RotateSensitivity = 0.25f;
		public float MovementSensitivity = 0.01f;
		//overlay
		public bool ShowMapKey = true;
		public Transform MapKeyPivot;
		public UISprite ToggleMapKeyButtonSprite;
		public Vector3 MapKeyPivotShowPosition;
		public Vector3 MapKeyPivotHidePosition;
		public UIPanel OverlayPanel;
		public UIPanel KeyClipPanel;
		public UIDraggablePanel KeyDragPanel;
		public UISlider DepthSlider;
		public UICheckbox MapKeyAllOrNone;
		public UILabel MapKeyAllOrNoneLabel;
		public UICheckbox MapKeyDistrictNames;
		public GameObject MapKeyCheckboxPrefab;
		public Transform MapKeyParent;
		public Transform PathsParent;
		public int CheckboxFlag;
		//label opacity
		public MapIconStyle CurrentStyle = MapIconStyle.Large;
		public float SmallStyleAlpha = 0f;
		public float MediumStyleAlpha = 0f;
		public float LargeStyleAlpha = 0f;
		public float ConstantStyleAlpha = 0f;
		//roads
		public List <LineRenderer> RelevantPaths = new List<LineRenderer> ();
		public List <LineRenderer> NonRelevantPaths = new List<LineRenderer> ();
		public UIButtonMessage CloseButton;
		//marked locations & indicators
		public List <WorldMapLocation> MarkedLocations = new List<WorldMapLocation> ();
		public List <UISprite> OffscreenIndicators = new List<UISprite> ();
		//interface helpers
		public GUIHudMiniAction NavigateControlAction;
		public GUIHudMiniAction MoveUpDownControlAction;
		public GUIHud.HudPrompt NavigatePrompt;
		public GUIHud.HudPrompt MoveUpDownPrompt;

		public override void GetActiveInterfaceObjects (List<Widget> currentObjects, int flag)
		{
			if (flag < 0) {
				flag = GUIEditorID;
			}

			FrontiersInterface.GetActiveInterfaceObjectsInTransform (IconsPanel.transform, NGUICamera, currentObjects, flag);

			FrontiersInterface.Widget w = new Widget (flag);
			w.SearchCamera = NGUICamera;

			w.BoxCollider = CloseButton.GetComponent <BoxCollider> ();
			currentObjects.Add (w);
			w.BoxCollider = DepthSlider.GetComponent <BoxCollider> ();
			currentObjects.Add (w);
			w.BoxCollider = MapKeyDistrictNames.GetComponent <BoxCollider> ();
			currentObjects.Add (w);
			w.BoxCollider = MapKeyAllOrNone.GetComponent<BoxCollider> ();
			currentObjects.Add (w);
			w.BoxCollider = ToggleMapKeyButtonSprite.transform.parent.GetComponent <BoxCollider> ();
			currentObjects.Add (w);

			if (ShowMapKey) {
				w.Flag = CheckboxFlag;
				//use the checkbox id so we can track when the checkbox is changed
				foreach (Transform icon in MapKeyParent) {
					w.BoxCollider = icon.GetComponent <BoxCollider> ();
					currentObjects.Add (w);
				}
			}


		}

		public void OnClickCloseButton ()
		{
			ActionCancel (WorldClock.RealTime);
		}

		public void OnClickMapKeyCheckboxAllOrNone ()
		{
			if (!Maximized)
				return;

			mUpdatingCheckboxes = true;

			UICheckbox checkBox = null;
			WorldMap.LocationTypesToDisplay.Clear ();

			if (MapKeyAllOrNone.isChecked) {
				MapKeyDistrictNames.isChecked = false;
				foreach (Transform child in MapKeyParent) {
					if (child.gameObject.HasComponent <UICheckbox> (out checkBox)) {
						checkBox.isChecked = false;
					}
				}
			} else {
				MapKeyDistrictNames.isChecked = true;
				WorldMap.LocationTypesToDisplay.Add ("Descriptive");
				foreach (Transform child in MapKeyParent) {
					if (child.gameObject.HasComponent <UICheckbox> (out checkBox)) {
						checkBox.isChecked = true;
						WorldMap.LocationTypesToDisplay.Add (checkBox.name);
					}
				}
			}
			mUpdatingCheckboxes = false;
			mLabelsChanged = true;
		}

		public void OnClickMapKeyCheckboxDistrictName ()
		{
			if (!Maximized)
				return;

			if (MapKeyDistrictNames.isChecked) {
				WorldMap.LocationTypesToDisplay.SafeAdd ("Descriptive");
			} else {
				WorldMap.LocationTypesToDisplay.Remove ("Descriptive");
			}
			mLabelsChanged = true;
		}

		public static float GetWorldMapAtChunkPosition (Vector3 mapTilePosition, Texture2D miniHeightmap, float defaultHeight)
		{
			if (miniHeightmap == null) {
				//Debug.Log("no mini height map, skipping");
				return defaultHeight;
			}
			mUv.x = Mathf.InverseLerp (0f, 1f, mapTilePosition.x);
			mUv.y = Mathf.InverseLerp (0f, 1f, mapTilePosition.y);
			Color c;
			//Debug.Log("UVs from " + mapTilePosition.ToString() + ": " + mUv.ToString());
			//c = miniHeightmap.GetPixel(Mathf.FloorToInt(mUv.x * miniHeightmap.width), Mathf.FloorToInt(mUv.y * miniHeightmap.height));
			c = miniHeightmap.GetPixelBilinear (mUv.x, mUv.y);
			//Debug.Log("Color from UVS: " + c.r.ToString());
			return -(c.r * ChunkMeshMultiplier);
		}

		public static float GetWorldMapHeightAtPosition (Vector3 worldMapPosition, float defaultHeight, out float chunkElevation)
		{
			chunkElevation = -1f;
			LastCastStart = worldMapPosition + (Vector3.back * 5);
			LastCheckPoint = worldMapPosition;
			LastPointHit = worldMapPosition + (Vector3.forward * 5);
			if (Physics.Raycast (LastCastStart, Vector3.forward, out mWorldMapRaycast, 1000f, Globals.LayerGUIHUD)) {
				Rigidbody rb = mWorldMapRaycast.collider.attachedRigidbody;
				//Debug.Log("Hit a thing...");
				if (rb != null && rb.gameObject.HasComponent <GUIMapTile> (out mMapTileCheck)) {
					chunkElevation = mMapTileCheck.ChunkToDisplay.TileElevation;
					//Debug.Log("Got chunk " + mMapTileCheck.ChunkToDisplay.Name + " with position " + worldMapPosition.ToString());
					//the world map position isn't in local space
					//so get the local position
					//what a f'ing mess...
					PositionHelper.parent = mMapTileCheck.TileBackground;
					PositionHelper.position = worldMapPosition;
					worldMapPosition = PositionHelper.localPosition;
					return GetWorldMapAtChunkPosition (worldMapPosition, mMapTileCheck.ChunkToDisplay.MiniHeightmap, defaultHeight);
				}
			} else {
				//Debug.Log("Didn't hit anything, returning default height");
			}
			return defaultHeight;
		}

		public void OnDrawGizmos ()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere (LastCheckPoint, 0.125f);
			Gizmos.color = Color.white;
			Gizmos.DrawSphere (LastPointHit, 0.125f);
			Gizmos.color = Color.red;
			Gizmos.DrawLine (LastCastStart, LastPointHit);
			Gizmos.color = Color.white;
			if (mMapTileCheck != null) {
				Gizmos.DrawWireCube (mMapTileCheck.TileCollider.bounds.center, mMapTileCheck.TileCollider.bounds.size);
			}

			for (int i = 0; i < test.Count; i++) {
				Gizmos.color = Colors.Alpha (Color.cyan, 0.5f);
				Gizmos.DrawSphere (test [i], 0.01f);
				Gizmos.color = Colors.Alpha (Color.magenta, 0.25f);
				Gizmos.DrawCube (testResults [i], new Vector3 (0.1f, 0.1f, 0.01f));
			}

			Gizmos.color = Color.green;
			Gizmos.DrawWireCube (NavigationBounds.center, NavigationBounds.size);
		}

		public static Vector3 LastCheckPoint;
		public static Vector3 LastPointHit;
		public static Vector3 LastCastStart;
		public static float ChunkMeshMultiplier = 0.75f;
		protected static Vector2 mUv;
		protected static RaycastHit mWorldMapRaycast;
		protected static GUIMapTile mMapTileCheck;
		protected bool mUpdatingCheckboxes;

		public override bool ShowQuickslots {
			get {
				return !Maximized;
			}
			set { return; }
		}

		public void OnClickMapKeyCheckbox ()
		{
			if (!Maximized) {
				return;
			}

			if (mUpdatingCheckboxes)
				return;

			if (!GameManager.Is (FGameState.GamePaused | FGameState.InGame)) {
				return;
			}

			WorldMap.LocationTypesToDisplay.Clear ();
			UICheckbox checkBox = null;
			if (MapKeyDistrictNames.isChecked) {
				WorldMap.LocationTypesToDisplay.Add ("Descriptive");
			}
			foreach (Transform child in MapKeyParent) {
				if (child.gameObject.HasComponent <UICheckbox> (out checkBox)) {
					if (checkBox.isChecked) {
						WorldMap.LocationTypesToDisplay.Add (checkBox.name);
					}
				}
			}
			mLabelsChanged = true;
		}

		protected bool mLabelsChanged = false;

		public override void WakeUp ()
		{
			base.WakeUp ();

			Get = this;
			MapKeyPivot.gameObject.SetActive (false);
			SearchFieldInput.eventReceiver = gameObject;
			SearchFieldInput.functionName = "OnSearchFieldChange";
			SearchFieldInput.functionNameEnter = "OnSearchFieldChange";
			SupportsControllerSearch = false;
			//PlayerPositionScaleMultiplier = 0.00075f;
			CheckboxFlag = GUIManager.GetNextGUIID ();
			mKeyClipPanelStartupPostion = KeyClipPanel.transform.localPosition;
			DisableInput ();

			mCustomVRSettings = true;
			mAxisLock = false;
			mCursorLock = false;
			mQuadZOffset = 1.1f;
		}

		public void OnSearchFieldChange ()
		{
			SearchFieldInput.selected = true;
			SearchFilter = SearchFieldInput.text.ToLower ();
			SearchFieldInput.label.text = SearchFilter + "[FF00FF] " + SearchFieldInput.caratChar + "[-]";
			mLabelsChanged = true;
		}

		public override void Start ()
		{
			Subscribe (InterfaceActionType.SelectionNext, new ActionListener (SelectionNext));
			Subscribe (InterfaceActionType.SelectionPrev, new ActionListener (SelectionPrev));
			base.Start ();
		}

		public bool SelectionNext (double timeStamp)
		{
			if (Maximized) {
				DepthSlider.sliderValue += ScrollWheelSensitivity;
			}
			return true;
		}

		public bool SelectionPrev (double timeStamp)
		{
			if (Maximized) {
				DepthSlider.sliderValue -= ScrollWheelSensitivity;
			}
			return true;
		}

		public override bool Maximize ()
		{
			if (base.Maximize ()) {
				if (PositionHelper == null) {
					PositionHelper = new GameObject ("PositionHelper").transform;
				}

				#if UNITY_EDITOR
				bool vrModeEnabled = VRManager.VRMode | VRManager.VRTestingMode;
				#else
								bool vrModeEnabled = VRManager.VRMode;
				#endif

				if (vrModeEnabled) {
					InterfaceActionManager.SuspendCursorControllerMovement = true;
					//NavigatePrompt = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.MouseX, "Navigate");
					MoveUpDownPrompt = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.ScrollWheel, "Up/Down");
				} else {
					InterfaceActionManager.SuspendCursorControllerMovement = false;
					//NavigatePrompt = new GUIHud.HudPrompt (ActionSetting.InputAxis.MovementX, ActionSetting.InputAxis.None, "Navigate");
					MoveUpDownPrompt = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.ScrollWheel, "Up/Down");
				}
				NavigatePrompt = GUIHud.GetBindings (NavigatePrompt);
				MoveUpDownPrompt = GUIHud.GetBindings (MoveUpDownPrompt);
				GUIHud.GUIHudMode mode = GUIHud.GUIHudMode.MouseAndKeyboard;
				if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
					mode = GUIHud.GUIHudMode.Controller;
				}
				NavigateControlAction.IncludeOpposingAxisByDefault = true;
				MoveUpDownControlAction.IncludeOpposingAxisByDefault = true;
				NavigatePrompt = GUIHud.RefreshHudAction (NavigatePrompt, NavigateControlAction, mode, false);
				MoveUpDownPrompt = GUIHud.RefreshHudAction (MoveUpDownPrompt, MoveUpDownControlAction, mode, false);

				gameObject.SetActive (true);
				CloseButton.gameObject.SetActive (true);
				DepthSlider.gameObject.SetActive (true);
				DepthSlider.sliderValue = 0.8f;
				OverlayPanel.gameObject.SetActive (true);
				MapKeyPivot.gameObject.SetActive (true);
				OverlayPanel.enabled = true;
				KeyClipPanel.enabled = true;
				KeyDragPanel.enabled = true;
				MapBackgroundCamera.enabled = true;
				PlayerIconCamera.enabled = true;
				//tell the world map what to show - this will trickle down into the building of the tileset
				DragObject.localPosition = Vector3.zero;
				try {
					for (int i = 0; i < GameWorld.Get.Regions.Count; i++) {
						MobileReference capital = GameWorld.Get.Regions [i].Capital;
						//Debug.Log("Adding capital " + capital.FullPath + " to locations to display");
						if (!MobileReference.IsNullOrEmpty (capital)) {
							Profile.Get.CurrentGame.RevealedLocations.SafeAdd (capital);
						}
					}
				} catch (Exception e) {
					Debug.LogError ("Error while maximinzing world map, proceeding normally: " + e.ToString ());
				}

				PlayerPosition.localPosition = Vector3.zero;
				Vector3 playerRotation = Player.Local.Rotation.eulerAngles;
				PlayerPosition.localRotation = Quaternion.Euler (0f, 180f, playerRotation.y);
				PlayerPosition.parent = null;

				Vector3 playerPosition = Player.Local.Position;
				playerPosition = new Vector3 (playerPosition.x, playerPosition.z, 0f);
				DragObject.localPosition = playerPosition;

				PlayerPosition.parent = DragObject;
				PlayerPosition.localPosition = playerPosition;
				PlayerPosition.localScale = Vector3.one;

				WorldMap.SetMapData (Profile.Get.CurrentGame.RevealedLocations);
				WorldMap.LoadData = true;
				BuildTileSet ();

				TilesBackground.transform.parent = DragObject;
				TilesBackground.transform.localPosition = new Vector3 (0f, 0f, 1f);
				TilesBackground.transform.localScale = Vector3.one * 50000f;
				TilesBackground.enabled = true;
				CloudsRenderer.enabled = true;

				MapCameraPositionTransform.position = PlayerPosition.position;// + (MapCameraTransform.up * 10f);
				Vector3 mapCameraTransformPosition = MapCameraPositionTransform.localPosition;
				mapCameraTransformPosition.z = 0f;
				MapCameraPositionTransform.localPosition = mapCameraTransformPosition;
				mLastPositionHitCollider = DragObject.localPosition;

				DepthSlider.gameObject.SetLayerRecursively (Globals.LayerNumGUIRaycast);
				UIInput.current = SearchFieldInput;

				MoveForwardAmount = 0f;//0.01f;
				RotateAmount = 0f;

				NavigationBounds.center = DragObject.position;
				NavigationBounds.size = Vector3.one;
				for (int i = 0; i < ChunksDisplayed.Count; i++) {
					//while we're here, set the max bounds
					NavigationBounds.Encapsulate (ChunksDisplayed [i].TileCollider.bounds);
				}
				NavigationBounds.size = NavigationBounds.size * 1.1f;

				BuildPaths ();

				VRManager.Get.UsePaperRenderer = true;
				VRManager.Get.ResetInterfacePosition ();

				return true;
			}
			return false;
		}

		public override void EnableInput ()
		{
			//base.EnableInput ();
			OverlayPanel.gameObject.SetLayerRecursively (Globals.LayerNumGUIRaycast);
		}

		public override void DisableInput ()
		{
			//base.DisableInput ();
			OverlayPanel.gameObject.SetLayerRecursively (Globals.LayerNumGUIRaycastIgnore);

		}

		public override bool Minimize ()
		{
			if (base.Minimize ()) {
				PlayerPosition.parent = TilesParent.transform;

				InterfaceActionManager.SuspendCursorControllerMovement = false;

				gameObject.SetActive (false);
				CloseButton.gameObject.SetActive (false);
				DepthSlider.gameObject.SetActive (false);
				OverlayPanel.gameObject.SetActive (false);
				MapKeyPivot.gameObject.SetActive (false);

				OverlayPanel.enabled = false;
				KeyClipPanel.enabled = false;
				KeyDragPanel.enabled = false;
				MapBackgroundCamera.enabled = false;
				PlayerIconCamera.enabled = false;
				TilesBackground.enabled = false;
				CloudsRenderer.enabled = false;
				//this will stop all loading coroutines
				WorldMap.LoadData = false;
				ClearTileSet ();

				if (UIInput.current == SearchFieldInput) {
					UIInput.current = null;
				}

				DepthSlider.gameObject.SetLayerRecursively (Globals.LayerNumGUIRaycastIgnore);

				MarkedAvatars.Clear ();//they'll be destroyed in the map tiles
				mBuildingPaths = false;
				//we've been fiddling with cameras so make sure it's set up right
				VRManager.Get.RefreshSettings (false);

				VRManager.Get.UsePaperRenderer = false;
				return true;
			}
			return false;
		}

		public void CreateMarkedLocationSprite (WorldMapLocation wml)
		{
			GameObject newWMAttentionGo = NGUITools.AddChild (MarkersPanel.gameObject, WMMarkPrefab);
			UISprite attention = newWMAttentionGo.GetComponent <UISprite> ();
			wml.Attention = attention;
			if (wml.IsMarked) {
				wml.Attention.color = Colors.Alpha (Colors.Get.GeneralHighlightColor, 1f);
			} else {
				wml.Attention.color = Colors.Get.WarningHighlightColor;
			}
			wml.AttentionTransform = attention.transform;
			wml.AttentionTransform.localScale = wml.IconTransform.localScale;
			wml.Attention.alpha = 0f;
			wml.Attention.enabled = true;
			newWMAttentionGo.transform.localPosition = wml.IconTransform.localPosition + Vector3.forward;//always behind icon
		}

		public void RemoveMarkedLocationSprite (WorldMapLocation wml)
		{
			if (wml.Attention != null) {
				GameObject.Destroy (wml.Attention.gameObject);
				wml.Attention = null;
			}
		}

		public void OnClickToggleMapKey ()
		{
			ShowMapKey = !ShowMapKey;
			if (ShowMapKey) {
				ToggleMapKeyButtonSprite.transform.localRotation = Quaternion.Euler (0f, 0f, 180f);
			} else {
				ToggleMapKeyButtonSprite.transform.localRotation = Quaternion.identity;
			}
		}

		protected bool mTest = false;
		protected float mLastCameraHeight;
		public List <Vector3> test = new List<Vector3> ();
		public List <Vector3> testResults = new List<Vector3> ();
		public GUIMapTile mt;

		public override void Update ()
		{
			base.Update ();

			if (!Maximized) {
				BuildingPathsLabel.enabled = false;
				return;
			}

			//#if UNITY_EDITOR
			//bool vrModeEnabled = VRManager.VRMode | VRManager.VRTestingMode;
			//#else
			bool vrModeEnabled = VRManager.VRMode;
			//#endif

			SupportsControllerSearch = vrModeEnabled;
			if (ShowMapKey) {
				if (!MapKeyPivot.localPosition.IsApproximately (MapKeyPivotShowPosition, 0.01f)) {
					MapKeyPivot.localPosition = Vector3.Lerp (MapKeyPivot.localPosition, MapKeyPivotShowPosition, 0.25f);
					GUICursor.Get.RefreshCurrentSearch ();
				}
				//if the last selected widget was a checkbox id
				//make sure the checkbox is visible
				if (GUICursor.Get.WidgetSelectedRecently && GUICursor.Get.LastSelectedWidgetFlag == CheckboxFlag) {
					FocusClipPanelOnPosition (GUICursor.Get.CurrentSearch.WorldBounds.center, KeyClipPanel, KeyDragPanel, 0.5f, mKeyClipPanelStartupPostion.y);
					GUICursor.Get.RefreshCurrentSearch ();
				}
			} else {
				MapKeyPivot.localPosition = Vector3.Lerp (MapKeyPivot.localPosition, MapKeyPivotHidePosition, 0.25f);
			}

			if (vrModeEnabled) {
				MapBackgroundCamera.clearFlags = CameraClearFlags.SolidColor;
				MapBackgroundCamera.backgroundColor = Colors.Alpha (Color.black, 1f);
				MapBackgroundCamera.renderingPath = RenderingPath.VertexLit;
				MapBackgroundCamera.depth = 0.05f;
				MapBackgroundCamera.fieldOfView = 60f;//GameManager.Get.GameCamera.fieldOfView;
				VignetteEffect.enabled = false;
				ImageEffect.enabled = false;
				GUIManager.Get.PrimaryCamera.clearFlags = CameraClearFlags.Depth;
			} else {
				MapBackgroundCamera.clearFlags = CameraClearFlags.Depth;
				MapBackgroundCamera.renderingPath = RenderingPath.Forward;
				MapBackgroundCamera.depth = 0f;
				MapBackgroundCamera.fieldOfView = 60f;
				VignetteEffect.enabled = true;
				ImageEffect.enabled = true;
				GUIManager.Get.PrimaryCamera.clearFlags = CameraClearFlags.Depth;
			}

			float depth = (DepthSlider.sliderValue * 4);//the depth slider has 4 discrete settings
			if (depth > 3f) {
				CurrentStyle = MapIconStyle.Small;
			} else if (depth > 2f) {
				CurrentStyle = MapIconStyle.Medium;
			} else if (depth > 1f) {
				CurrentStyle = MapIconStyle.Large;
			} else {
				CurrentStyle = MapIconStyle.AlwaysVisible;
			}
			float newScaleTargetNormalized = ScaleCurve.Evaluate (DepthSlider.sliderValue);
			ScaleTargetNormalized = Mathf.Lerp (ScaleTargetNormalized, newScaleTargetNormalized, (float)(WorldClock.RTDeltaTime * ScrollSpeed));
			ScaleCurrent = Mathf.Lerp (ScaleMin, ScaleMax, ScaleTargetNormalized);


			Vector3 playerSpritePosition = PlayerPosition.position;
			PositionHelper.parent = DragObject;
			PositionHelper.position = playerSpritePosition;
			float elevation = 0f;
			playerSpritePosition.z = GetWorldMapHeightAtPosition (PositionHelper.position, playerSpritePosition.z, out elevation);// * 2.2f;
			playerSpritePosition.y = 0f;
			playerSpritePosition.x = 0f;
			PlayerPositionSprite.localPosition = playerSpritePosition;

			bool hitCollider = false;
			Vector3 mapCameraParentPosition = MapCameraPositionTransform.position + Vector3.Lerp (MapCameraMinPosition, MapCameraMaxPosition, ScaleTargetNormalized);
			//figure out if we're intersecting the ground
			PositionHelper.parent = DragObject;
			PositionHelper.position = mapCameraParentPosition;
			float cameraHeight = mapCameraParentPosition.z;
			float worldHeight = GetWorldMapHeightAtPosition (PositionHelper.position, mapCameraParentPosition.z, out elevation) * 2.2f;
			hitCollider = elevation > 0f;
			if (hitCollider) {
				//Debug.Log("Camera height is " + cameraHeight.ToString() + ", world height is " + worldHeight.ToString());
				if (worldHeight < cameraHeight) {
					//Debug.Log("Setting camera height to world height");
					cameraHeight = worldHeight;
				}
			}
			cameraHeight = Mathf.Lerp (mLastCameraHeight, cameraHeight, 0.5f);
			mapCameraParentPosition.z = cameraHeight;
			mLastCameraHeight = cameraHeight;
			MapCameraParent.position = mapCameraParentPosition;

			if (!mBuildingPaths) {
				ImageEffect.intensity = 0.875f;

				if (BuildingPathsLabel.enabled) {
					BuildingPathsLabel.color = Colors.Alpha (Colors.Get.MenuButtonTextColorDefault, Mathf.Lerp (BuildingPathsLabel.alpha, 0f, 0.25f));
					if (BuildingPathsLabel.alpha < 0.001f) {
						BuildingPathsLabel.enabled = false;
					}
				}

				if (vrModeEnabled) {
					float newMoveForwardAmount = (InterfaceActionManager.RawMouseAxisY * (float)WorldClock.RTDeltaTimeSmooth) * MovementSensitivity;
					float newRotateAmount = (InterfaceActionManager.RawMouseAxisX * (float)WorldClock.RTDeltaTimeSmooth) * RotateSensitivity;
					MoveForwardAmount = Mathf.Lerp (MoveForwardAmount, (MoveForwardAmount - newMoveForwardAmount), 0.75f);
					RotateAmount = Mathf.Lerp (RotateAmount, (RotateAmount - newRotateAmount), 0.5f);
				} else {
					float newMoveForwardAmount = (UserActionManager.RawMovementAxisY * (float)WorldClock.RTDeltaTimeSmooth) * MovementSensitivity;
					float newRotateAmount = (UserActionManager.RawMovementAxisX * (float)WorldClock.RTDeltaTimeSmooth) * RotateSensitivity;
					MoveForwardAmount = Mathf.Lerp (MoveForwardAmount, (MoveForwardAmount - newMoveForwardAmount), 0.75f);
					RotateAmount = Mathf.Lerp (RotateAmount, (RotateAmount - newRotateAmount), 0.5f);
				}

				Vector3 dragObjectPosition = DragObject.localPosition;
				if (Mathf.Abs (MoveForwardAmount) > 0.00025f) {
					DragObject.Translate (MapCameraParent.up * MoveForwardAmount);
					dragObjectPosition = DragObject.localPosition;
					dragObjectPosition.z = 0f;
					DragObject.localPosition = dragObjectPosition;
				}
				mLastPositionHitCollider = dragObjectPosition;

				if (Mathf.Abs (RotateAmount) > 0.001f) {
					MapCameraParent.Rotate (0f, 0f, RotateAmount);
				}

				MoveForwardAmount = Mathf.Lerp (MoveForwardAmount, 0f, 0.05f);
				RotateAmount = Mathf.Lerp (RotateAmount, 0f, 0.5f);
			} else {
				ImageEffect.intensity = 0f;
				BuildingPathsLabel.enabled = true;
				BuildingPathsLabel.alpha = 1f;
				BuildingPathsLabel.color = Color.Lerp (Colors.Get.GeneralHighlightColor, Colors.Darken (Colors.Get.GeneralHighlightColor), Mathf.Abs (Mathf.Sin (Time.time * 5f)));
			}

			mPathTextureOffset.x += (float)(0.225f * WorldClock.RTDeltaTimeSmooth);
			Mats.Get.WorldMapPathMaterial.SetTextureOffset ("_MainTex", mPathTextureOffset);

			mMarkedLocationsCheck++;
			if (mMarkedLocationsCheck < 20) {
				return;
			}
			mMarkedLocationsCheck = 0;

			MarkedLocations.Clear ();
			for (int i = 0; i < ChunksDisplayed.Count; i++) {
				for (int j = 0; j < ChunksDisplayed [i].SmallLocations.Count; j++) {
					if (ChunksDisplayed [i].SmallLocations [j].IsMarked) {
						MarkedLocations.Add (ChunksDisplayed [i].SmallLocations [j]);
					}
				}
				for (int j = 0; j < ChunksDisplayed [i].MediumLocations.Count; j++) {
					if (ChunksDisplayed [i].MediumLocations [j].IsMarked) {
						MarkedLocations.Add (ChunksDisplayed [i].MediumLocations [j]);
					}
				}
				for (int j = 0; j < ChunksDisplayed [i].LargeLocations.Count; j++) {
					if (ChunksDisplayed [i].LargeLocations [j].IsMarked) {
						MarkedLocations.Add (ChunksDisplayed [i].LargeLocations [j]);
					}
				}
				for (int j = 0; j < ChunksDisplayed [i].ConstantLocations.Count; j++) {
					if (ChunksDisplayed [i].ConstantLocations [j].IsMarked) {
						MarkedLocations.Add (ChunksDisplayed [i].ConstantLocations [j]);
					}
				}
			}
			UpdateMarkedLocations ();
		}

		protected Vector3 mPathTextureOffset;
		protected Vector3 mLastPositionHitCollider;
		protected Vector3 mKeyClipPanelStartupPostion;
		protected int mMarkedLocationsCheck;

		public void UpdateMarkedLocations ()
		{
			for (int i = 0; i < MarkedAvatars.Count; i++) {
				MarkedAvatars [i].SetActive (false);
			}

			for (int i = 0; i < MarkedLocations.Count; i++) {
				WorldMapLocation ml = MarkedLocations [i];
				GameObject marker = null;
				if (MarkedAvatars.LastIndex () < i) {
					marker = GameObject.Instantiate (WorldMapMarkerAvatarPrefab) as GameObject;
					marker.transform.parent = ml.LocationTransform.parent.parent;
					marker.transform.localScale = Vector3.one;
					MarkedAvatars.Add (marker);
				} else {
					marker = MarkedAvatars [i];
				}
				marker.SetActive (true);
				marker.transform.position = ml.LocationTransform.position + Vector3.back;
			}
		}

		public void LateUpdate ()
		{
			UpdateLabels (ScaleTargetNormalized + 0.25f);
		}

		public void BuildPaths ()
		{
			if (!mBuildingPaths) {
				mBuildingPaths = true;
				StartCoroutine (BuildPathsOverTime ());
			}
		}

		protected void UpdateLabels (float scale)
		{
			//we have to update the fading on map lables based on scale
			//figure out label opacity
			float wcDeltaTime = (float)(WorldClock.RTDeltaTime);
			switch (CurrentStyle) {
			case MapIconStyle.AlwaysVisible:
				ConstantStyleAlpha = Mathf.Lerp (ConstantStyleAlpha, 1f, wcDeltaTime);
				LargeStyleAlpha = Mathf.Lerp (LargeStyleAlpha, 0f, wcDeltaTime);
				MediumStyleAlpha = Mathf.Lerp (MediumStyleAlpha, 0f, wcDeltaTime);
				SmallStyleAlpha = Mathf.Lerp (SmallStyleAlpha, 0f, wcDeltaTime);
				break;

			case MapIconStyle.Large:
				ConstantStyleAlpha = Mathf.Lerp (ConstantStyleAlpha, 1f, wcDeltaTime);
				LargeStyleAlpha = Mathf.Lerp (LargeStyleAlpha, 1f, wcDeltaTime);
				MediumStyleAlpha = Mathf.Lerp (MediumStyleAlpha, 0.25f, wcDeltaTime);
				SmallStyleAlpha = Mathf.Lerp (SmallStyleAlpha, 0f, wcDeltaTime);
				break;

			case MapIconStyle.Medium:
				ConstantStyleAlpha = Mathf.Lerp (ConstantStyleAlpha, 1f, wcDeltaTime);
				LargeStyleAlpha = Mathf.Lerp (LargeStyleAlpha, 0f, wcDeltaTime);
				MediumStyleAlpha = Mathf.Lerp (MediumStyleAlpha, 1f, wcDeltaTime);
				SmallStyleAlpha = Mathf.Lerp (SmallStyleAlpha, 0.25f, wcDeltaTime);
				break;

			case MapIconStyle.Small:
				ConstantStyleAlpha = Mathf.Lerp (ConstantStyleAlpha, 0f, wcDeltaTime);
				LargeStyleAlpha = Mathf.Lerp (LargeStyleAlpha, 0f, wcDeltaTime);
				MediumStyleAlpha = Mathf.Lerp (MediumStyleAlpha, 0f, wcDeltaTime);
				SmallStyleAlpha = Mathf.Lerp (SmallStyleAlpha, 1f, wcDeltaTime);
				break;

			case MapIconStyle.None:
				ConstantStyleAlpha = Mathf.Lerp (ConstantStyleAlpha, 0f, wcDeltaTime);
				LargeStyleAlpha = Mathf.Lerp (LargeStyleAlpha, 0f, wcDeltaTime);
				MediumStyleAlpha = Mathf.Lerp (MediumStyleAlpha, 0f, wcDeltaTime);
				SmallStyleAlpha = Mathf.Lerp (SmallStyleAlpha, 0f, wcDeltaTime);
				break;
			}

			if (mLabelsChanged) {
				if (!string.IsNullOrEmpty (SearchFilter)) {
					for (int i = 0; i < ChunksDisplayed.Count; i++) {
						for (int j = 0; j < ChunksDisplayed [i].SmallLocations.Count; j++) {
							ChunksDisplayed [i].SmallLocations [j].UpdateType (WorldMap.LocationTypesToDisplay, SearchFilter);
						}
						for (int j = 0; j < ChunksDisplayed [i].MediumLocations.Count; j++) {
							ChunksDisplayed [i].MediumLocations [j].UpdateType (WorldMap.LocationTypesToDisplay, SearchFilter);
						}
						for (int j = 0; j < ChunksDisplayed [i].LargeLocations.Count; j++) {
							ChunksDisplayed [i].LargeLocations [j].UpdateType (WorldMap.LocationTypesToDisplay, SearchFilter);
						}
						for (int j = 0; j < ChunksDisplayed [i].ConstantLocations.Count; j++) {
							ChunksDisplayed [i].ConstantLocations [j].UpdateType (WorldMap.LocationTypesToDisplay, SearchFilter);
						}
					}
				} else {
					for (int i = 0; i < ChunksDisplayed.Count; i++) {
						for (int j = 0; j < ChunksDisplayed [i].SmallLocations.Count; j++) {
							ChunksDisplayed [i].SmallLocations [j].UpdateType (WorldMap.LocationTypesToDisplay);
						}
						for (int j = 0; j < ChunksDisplayed [i].MediumLocations.Count; j++) {
							ChunksDisplayed [i].MediumLocations [j].UpdateType (WorldMap.LocationTypesToDisplay);
						}
						for (int j = 0; j < ChunksDisplayed [i].LargeLocations.Count; j++) {
							ChunksDisplayed [i].LargeLocations [j].UpdateType (WorldMap.LocationTypesToDisplay);
						}
						for (int j = 0; j < ChunksDisplayed [i].ConstantLocations.Count; j++) {
							ChunksDisplayed [i].ConstantLocations [j].UpdateType (WorldMap.LocationTypesToDisplay);
						}
					}
				}
				mLabelsChanged = false;
			}

			//update the label types
			for (int i = 0; i < ChunksDisplayed.Count; i++) {
				for (int j = 0; j < ChunksDisplayed [i].SmallLocations.Count; j++) {
					ChunksDisplayed [i].SmallLocations [j].UpdateLabel (scale, LabelScale, MaxIconDistanceFromCamera, MinIconScale, MaxIconScale, 1f, MapBackgroundCamera, NGUICamera);
				}
				for (int j = 0; j < ChunksDisplayed [i].MediumLocations.Count; j++) {
					ChunksDisplayed [i].MediumLocations [j].UpdateLabel (scale, LabelScale, MaxIconDistanceFromCamera, MinIconScale, MaxIconScale, 1f, MapBackgroundCamera, NGUICamera);
				}
				for (int j = 0; j < ChunksDisplayed [i].LargeLocations.Count; j++) {
					ChunksDisplayed [i].LargeLocations [j].UpdateLabel (scale, LabelScale, MaxIconDistanceFromCamera, MinIconScale, MaxIconScale, 1f, MapBackgroundCamera, NGUICamera);
				}
				for (int j = 0; j < ChunksDisplayed [i].ConstantLocations.Count; j++) {
					ChunksDisplayed [i].ConstantLocations [j].UpdateLabel (scale, LabelScale, MaxIconDistanceFromCamera, MinIconScale, MaxIconScale, 1f, MapBackgroundCamera, NGUICamera);
				}
			}
		}

		protected void BuildTileSet ()
		{	//create a new array based on the number of chunk tiles defined in world settings
			//there won't be a chunk for every tile - we'll fill in the empty chunks with empty tiles
			int numChunkTilesX = GameWorld.Get.Settings.NumChunkTilesX;
			int numChunkTilesZ	= GameWorld.Get.Settings.NumChunkTilesZ;
			ChunkTileSet = new GUIMapTile [numChunkTilesX, numChunkTilesZ];

			for (int x = 0; x < numChunkTilesX; x++) {
				for (int z = 0; z < numChunkTilesZ; z++) {	//create the basic tile but don't initialize it fully
					CreateMapTile (x, z);
				}
			}

			WorldChunk worldChunk = null;
			for (int i = 0; i < GameWorld.Get.WorldChunks.Count; i++) {
				worldChunk = GameWorld.Get.WorldChunks [i];
				GUIMapTile associatedMapTile = null;
				if (worldChunk.State.DisplaySettings.ArbitraryTilePosition) {
					associatedMapTile = CreateMapTile ();
					associatedMapTile.InitializeAsArbitraryChunk (new WorldMapChunk (worldChunk), DragObject.transform);
				} else {
					int xTilePosition = worldChunk.State.XTilePosition;
					int yTilePosition = worldChunk.State.ZTilePosition;
					associatedMapTile = ChunkTileSet [xTilePosition, yTilePosition];
					associatedMapTile.InitializeAsChunk (new WorldMapChunk (worldChunk));
				}
			}	

			for (int i = 0; i < ChunksDisplayed.Count; i++) {
				ChunksDisplayed [i].BlendEdges (ChunksDisplayed);
			}

			for (int i = 0; i < ChunksDisplayed.Count; i++) {
				//while we're here, set the max bounds
				StartCoroutine (WorldMap.Get.LocationsForChunks (
					ChunksDisplayed [i].ChunkToDisplay.Name,
					ChunksDisplayed [i].ChunkToDisplay.ChunkID,
					ChunksDisplayed [i].LocationsToDisplay,
					Player.Local.Surroundings.State.ActiveMapMarkers));
			}
		}

		protected GUIMapTile CreateMapTile ()
		{
			GameObject newMapTileGameObject = NGUITools.AddChild (TilesParent, WMTilePrefab);
			GUIMapTile newMapTile = newMapTileGameObject.GetComponent <GUIMapTile> ();
			newMapTile.name = "Empty";
			ChunksDisplayed.Add (newMapTile);
			return newMapTile;
		}

		protected GUIMapTile CreateMapTile (int x, int z)
		{
			GameObject newMapTileGameObject = NGUITools.AddChild (TilesParent, WMTilePrefab);
			GUIMapTile newMapTile = newMapTileGameObject.GetComponent <GUIMapTile> ();
			newMapTile.name = ("Empty " + x + " " + z);
			newMapTile.SetEmptyTileOffset (x, z, DragObject.transform);
			ChunkTileSet [x, z] = newMapTile;
			ChunksDisplayed.Add (newMapTile);
			return newMapTile;
		}

		IEnumerator BuildPathsOverTime ()
		{
			/*double waitUntil = WorldClock.RealTime + 1f;
							while (WorldClock.RealTime < waitUntil) {
								yield return null;
							}*/
			bool isActive = false;
			for (int i = 0; i < WorldMap.RelevantPathsToDisplay.Count; i++) {
				Path pathToDisplay = WorldMap.RelevantPathsToDisplay [i];
				isActive = Paths.HasActivePath && Paths.ActivePath.name.Equals (pathToDisplay.Name);
				var enumerator = BuildPath (pathToDisplay, isActive, RelevantPaths);
				while (enumerator.MoveNext ()) {
					yield return null;
				}
			}

			for (int i = 0; i < WorldMap.NonRelevantPathsToDisplay.Count; i++) {
				Path pathToDisplay = WorldMap.NonRelevantPathsToDisplay [i];
				isActive = Paths.HasActivePath && Paths.ActivePath.name.Equals (pathToDisplay.Name);
				var enumerator = BuildPath (pathToDisplay, isActive, NonRelevantPaths);
				while (enumerator.MoveNext ()) {
					yield return null;
				}
			}
			MoveForwardAmount = 0.0125f;
			mBuildingPaths = false;
		}

		public static Transform PathHelper;
		public static Transform PositionHelper;
		protected bool mBuildingPaths = false;

		protected IEnumerator BuildPath (Path path, bool isActive, List<LineRenderer> paths)
		{
			for (int i = 0; i < paths.Count; i++) {
				if (paths [i].name == path.Name) {
					paths [i].gameObject.SetActive (true);
					paths [i].sharedMaterial.color = Colors.Get.WorldMapPathColor;
					yield break;
				}
			}

			if (PathHelper == null) {
				PathHelper = PathsParent.gameObject.CreateChild ("PathHelper").transform;
			}

			GameObject lineRendererGameObject = PathsParent.gameObject.CreateChild (path.Name).gameObject;
			lineRendererGameObject.layer = Globals.LayerNumGUIMap;
			LineRenderer lineRenderer = lineRendererGameObject.AddComponent <LineRenderer> ();
			lineRenderer.useWorldSpace = false;
			lineRenderer.sharedMaterial = Mats.Get.WorldMapPathMaterial;
			lineRenderer.sharedMaterial.color = Colors.Get.WorldMapPathColor;
			lineRenderer.castShadows = false;
			lineRenderer.receiveShadows = false;
			lineRenderer.enabled = false;
			lineRenderer.SetVertexCount (path.Templates.Count);
			float chunkElevation = 1f;
			int stopToYield = 0;
			lineRenderer.SetWidth (0.005f, 0.005f);
			for (int i = 0; i < path.Templates.Count; i++) {
				//jesus f'ing christ...
				PathHelper.parent = PathsParent;
				PathHelper.localScale = Vector3.one;
				Vector3 mapTilePosition = path.Templates [i].Position;
				mapTilePosition = new Vector3 (mapTilePosition.x, mapTilePosition.z, -1f);
				PathHelper.localPosition = mapTilePosition;
				mapTilePosition = PathHelper.position;
				//test.Add(mapTilePosition);
				float newHeight = GetWorldMapHeightAtPosition (mapTilePosition, 0f, out chunkElevation);
				//FUCKING GAMMA CORRECTION, ARG
				newHeight *= 2.2f;
				//testResults.Add(mapTilePosition);
				//Debug.Log("Resulting height: " + newHeight.ToString());
				mapTilePosition.z = newHeight;
				PathHelper.position = mapTilePosition;
				lineRenderer.SetPosition (i, PathHelper.localPosition);
				stopToYield++;
				if (stopToYield > 15) {
					stopToYield = 0;
					yield return null;
				}

				if (!Maximized) {
					break;
				}
			}

			if (!Maximized) {
				GameObject.Destroy (lineRenderer.gameObject);
				yield break;
			}

			lineRenderer.enabled = true;
			paths.Add (lineRenderer);
			yield break;
		}

		protected void ClearTileSet ()
		{
			foreach (LineRenderer lr in RelevantPaths) {
				lr.gameObject.SetActive (false);
			}
			foreach (GUIMapTile mapTile in ChunksDisplayed) {
				GameObject.Destroy (mapTile.gameObject);
			}
			foreach (GUIMapTile emptyTile in EmptyChunkTiles) {
				GameObject.Destroy (emptyTile.gameObject);
			}
			if (ChunkTileSet != null) {
				System.Array.Clear (ChunkTileSet, 0, ChunkTileSet.GetLength (0) * ChunkTileSet.GetLength (1));
			}
			ChunksDisplayed.Clear ();
			EmptyChunkTiles.Clear ();
			ChunkTileSet = null;
		}

		protected bool mHasGrabbedBackground = false;
		protected Vector3 mBackgroundStartPosition = Vector3.zero;
		protected Vector3 mBackgroundCurrentGrabOffset = Vector3.zero;
		protected Vector3 mBackgroundStartGrabOffset = Vector3.zero;
	}
}