using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers
{
		public class PlayerProjections : PlayerScript
		{
				//catch-all class for any visuals the player projects into the world
				//mostly used for weapon trajectories
				//will also be used for triangualtion
				//and for issuing directions to characters in the form of arrows
				//kind of a mess at the moment
				public RangedWeaponTrajectory WeaponTrajectory;
				public GameObject HighlightObject;
				public GameObject PlayerProjectionsObject;
				public GameObject WorldItemPlacementObject;
				public GameObject DirectionArrowPrefab;
				public Projector WorldItemPlacementProjector;

				public Projector CompassProjector;
				public Vector3 CompassProjectorTargetPosition;
				public float CompassProjectorTargetScale;
				public Color CompassProjectorTargetColor;
				public Color CompassProjectorCurrentColor;

				public List <DirectionArrow> DirectionArrows = new List <DirectionArrow>();
				public Transform DirectionArrowsParent;
				public GameObject ArrowInFocus = null;
				public Light FastTravelSpotlight;

				public bool GivingDirections {
						get {
								return DirectionArrows.Count > 0;
						}
				}

				public override void Awake()
				{
						DirectionArrowsParent = new GameObject("Direction Arrows Parent").transform;
						DontDestroyOnLoad(PlayerProjectionsObject);
						base.Awake();
				}

				public void ShowFastTravelChoices(PathMarkerInstanceTemplate currentMarker, List<TravelManager.FastTravelChoice> choices)
				{
						ClearDirectionalArrows();
						DirectionArrowsParent.position = currentMarker.Position;
						foreach (TravelManager.FastTravelChoice choice in choices) {
								GameObject newDirectionArrowGameObject = GameObject.Instantiate(DirectionArrowPrefab, currentMarker.Position, Quaternion.identity) as GameObject;
								newDirectionArrowGameObject.transform.parent = DirectionArrowsParent;
								DirectionArrow newDirectionArrow = newDirectionArrowGameObject.GetComponent <DirectionArrow>();
								newDirectionArrow.Choice = choice;
								newDirectionArrow.OnGainFocus += OnFocusOnArrow;
								DirectionArrows.Add(newDirectionArrow);
						}
						ShowCompass(currentMarker.Position);
				}

				public void OnFocusOnArrow()
				{
						if (GivingDirections) {
								for (int i = 0; i < DirectionArrows.Count; i++) {
										if (DirectionArrows[i].HasPlayerFocus) {
												TravelManager.Get.ConsiderChoice(DirectionArrows[i].Choice);
												break;
										}
								}
						}
				}

				public void ClearDirectionalArrows()
				{
						for (int i = DirectionArrows.LastIndex(); i >= 0; i--) {
								if (DirectionArrows[i] != null) {
										DirectionArrows[i].ShrinkAndDestroy();
										DirectionArrows.RemoveAt(i);
								}
						}
						HideCompass();
				}

				public void HighlightAttachedPaths(Location startLocation, PilgrimCallback chooseDirectionCallback)
				{
						if (DirectionArrows.Count > 0) {
								StopHighlightingPaths(false);
						}

						mChooseDirectionCallback = chooseDirectionCallback;

						//		foreach (string path in startLocation.AttachedPaths) {
						//			if (path.ContinuesInDirection (startLocation, PathDirection.Forward)) {
						//				GameObject arrowGameObject = GameObject.Instantiate (DirectionArrowPrefab) as GameObject;
						//				arrowGameObject.transform.position	= startLocation.transform.position;
						//				DirectionArrow arrow = arrowGameObject.GetComponent <DirectionArrow> ();
						//
						//				arrow.Projections = this;
						//				arrow.path = path;
						//				arrow.direction = PathDirection.Forward;
						//				arrow.start = new MobileReference (startLocation.worlditem.FileName, startLocation.worlditem.Group.Props.PathName);
						//
						//				DirectionArrows.Add (arrow.gameObject);
						//			}
						//
						//			if (path.ContinuesInDirection (startLocation, PathDirection.Backwards)) {
						//				GameObject arrowGameObject = GameObject.Instantiate (DirectionArrowPrefab) as GameObject;
						//				arrowGameObject.transform.position	= startLocation.transform.position;
						//				DirectionArrow arrow = arrowGameObject.GetComponent <DirectionArrow> ();
						//
						//				arrow.Projections = this;
						//				arrow.path = path;
						//				arrow.direction = PathDirection.Backwards;
						//				arrow.start = new MobileReference (startLocation.worlditem.FileName, startLocation.worlditem.Group.Props.PathName);
						//
						//				DirectionArrows.Add (arrow.gameObject);
						//			}
						//		}
				}

				public void StopHighlightingPaths(bool makeChoice)
				{
						/*if (makeChoice) {
								foreach (GameObject directionArrow in DirectionArrows) {
										DirectionArrow arrow = directionArrow.GetComponent <DirectionArrow>();
										arrow.OnChooseDirection();
								}
						}
						DirectionArrows.Clear();
						ArrowInFocus = null;*/
				}

				public void HighlightLocation(Location location)
				{
						GameObject.Instantiate(HighlightObject, location.InGamePosition, Quaternion.identity);
						//		CurrentPathMarkerProjector.enabled 				= true;
						//		mPathMarkerProjectionColorTarget				= Colors.PathColors.CurrentPathMarkerProjection;
						//		CurrentPathMarkerProjector.material.color 		= Colors.Alpha (mPathMarkerProjectionColorTarget, 0f);
						//		CurrentPathMarkerProjector.orthographicSize 	= 0.01f;
						//		mPathMarkerProjectorSizeTarget					= location.Props.Radius;
						//
						//		PlayerProjectionsObject.transform.position = location.InGamePosition + Vector3.up;
				}

				public override void Initialize()
				{
						base.Initialize();

						WeaponTrajectory = gameObject.CreateChild("RangedWeaponTrajactory").gameObject.AddComponent <RangedWeaponTrajectory>();

						CompassProjector = new GameObject("Player Compass Projector").AddComponent <Projector>();
						CompassProjector.enabled = false;
						CompassProjector.orthographic = true;
						CompassProjector.material = Mats.Get.CompassProjectorMaterial;
						CompassProjector.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
						CompassProjector.farClipPlane = 10f;
						CompassProjector.nearClipPlane = 0.1f;
						CompassProjector.ignoreLayers = Globals.LayerWorldItemActive;
						CompassProjector.gameObject.layer = Globals.LayerNumScenery;

						enabled = true;
				}

				public void PathMarkerVisit()
				{
						//		RefreshPathProjectors ( );
				}

				public void TryToSelectArrow()
				{
						if (GivingDirections) {
								bool choseArrow = false;
								for (int i = 0; i < DirectionArrows.Count; i++) {
										if (DirectionArrows[i].HasPlayerFocus) {
												DirectionArrows[i].HasBeenSelected = true;
												TravelManager.Get.MakeChoice(DirectionArrows[i].Choice);
												Debug.Log("Choosing item");
												choseArrow = true;
												break;
										}
								}
								if (choseArrow) {
										ClearDirectionalArrows();
								}
						}
				}

				protected void SetProjectorsActive(bool active)
				{

				}

				protected void RefreshPathProjectors()
				{
						/*if (TravelManager.Get.LastVisitedPathMarker != null) {
								PlayerProjectionsObject.transform.position = TravelManager.Get.LastVisitedPathMarker.Position;
								CompassProjector.enabled = true;
								CompassProjector.material.color = Colors.PathColors.CurrentPathMarkerProjection;
								CompassProjector.orthographicSize = 0.01f;
								//mPathMarkerProjectorSizeTarget = TravelManager.Get.LastVisitedPathMarker.State.Radius;
						}

						if (!Paths.HasActivePath) {
								SetProjectorsActive(false);
								return;
						} else {
								SetProjectorsActive(true);
						}*/

						//		ForwardPathSegment.transform.parent				= WorldRegionManager.Get.CurrentLoadedRegion.RegionMapBounds.transform;
						//		ForwardPathSegment.transform.localPosition		= ForwardPathCamera.transform.localPosition;

						//		Bounds forwardSegmentRegionMapRenderBounds		= Paths.ActivePath.CurrentSegment.RegionMapRenderBounds;
						//		Bounds forwardSegmentInGameRenderBounds			= WorldRegionManager.Get.CurrentLoadedRegion.RegionMapBoundsToInGameBounds (forwardSegmentRegionMapRenderBounds);
						//
						//		Vector3 forwardPathProjectorPosition			= new Vector3 (forwardSegmentInGameRenderBounds.center.x, 300.0f, forwardSegmentInGameRenderBounds.center.z);
						//		ForwardPathProjector.transform.position		 	= WorldRegionManager.Get.CurrentLoadedRegion.RegionMapPositionToInGamePosition (ForwardPathSegment.RegionMapRenderBounds.center);
						//		ForwardPathProjector.transform.Translate (0f, 300f, 0f);
						//		ForwardPathProjector.orthographicSize			= ForwardPathSegment.RegionMapRenderBounds.extents.x * WorldRegionManager.Get.CurrentLoadedRegion.RegionMapWidth;

						//		BackwardPathProjector.transform.position		= pathProjectorPosition;
						//		BackwardPathProjector.orthographicSize			= WorldRegionManager.Get.CurrentLoadedRegion.RegionMapWidth / 2.0f;

						//		Vector3 pathCameraPosition						= new Vector3 (0.0f, 1.0f, 0.0f);
						//
						//		ForwardPathCamera.transform.localPosition		= ForwardPathSegment.RegionMapRenderBounds.center;
						//		ForwardPathCamera.orthographicSize				= ForwardPathSegment.RegionMapRenderBounds.extents.x;
						//
						//		BackwardPathCamera.transform.parent				= WorldRegionManager.Get.CurrentLoadedRegion.WorldMapBounds.transform;
						//		BackwardPathCamera.transform.position			= pathCameraPosition;
						//		BackwardPathCamera.orthographicSize				= WorldRegionManager.Get.CurrentLoadedRegion.WorldMapSize / 2.0f;
				}

				public void ShowCompass (Vector3 pathMarkerPosition) {
						CompassProjectorTargetPosition = pathMarkerPosition + Vector3.up;
						if (!CompassProjector.enabled) {
								//move the compass right away if it's off
								CompassProjector.transform.position = CompassProjectorTargetPosition;
								CompassProjector.orthoGraphicSize = 0f;
								CompassProjectorCurrentColor = Color.black;
						}
						CompassProjectorTargetScale = 1.75f;
						CompassProjectorTargetColor = Colors.Get.MessageInfoColor;
						CompassProjectorTargetPosition = pathMarkerPosition;
						CompassProjector.enabled = true;
				}

				public void HideCompass ( ) {
						if (CompassProjector.enabled) {
								CompassProjectorTargetScale = 0.5f;
								CompassProjectorTargetColor = Color.black;
						}
				}

				public void FixedUpdate ( ) {
						if (GivingDirections) {
								if (Physics.Raycast(Player.Local.HeadPosition, Player.Local.FocusVector, out mArrowHit, Globals.RaycastAllFocusDistance, Globals.LayerScenery)) {
										for (int i = 0; i < DirectionArrows.Count; i++) {
												if (DirectionArrows[i].Collider == mArrowHit.collider) {
														DirectionArrows[i].HasPlayerFocus = true;
												} else {
														DirectionArrows[i].HasPlayerFocus = false;
												}
										}
								} else {
										for (int i = 0; i < DirectionArrows.Count; i++) {
												DirectionArrows[i].HasPlayerFocus = false;
										}
								}
						}
				}

				public void Update ( ) {
						if (CompassProjector.enabled) {
								CompassProjector.transform.position = Vector3.Lerp(CompassProjector.transform.position, CompassProjectorTargetPosition, 0.4f);
								CompassProjector.orthoGraphicSize = Mathf.Lerp(CompassProjector.orthoGraphicSize, CompassProjectorTargetScale, 0.125f);
								CompassProjectorCurrentColor = Color.Lerp(CompassProjectorCurrentColor, CompassProjectorTargetColor, 0.25f);
								CompassProjector.material.color = CompassProjectorCurrentColor;
								//we're supposed to be shutting it off
								if (CompassProjectorTargetScale == 0f) {
										if (CompassProjector.orthoGraphicSize < 0.001f) {
												CompassProjector.enabled = false;
										}
								}
						}
				}

				protected RaycastHit mArrowHit;
				protected PilgrimCallback mChooseDirectionCallback;
				protected float mPathMarkerProjectorSizeTarget;
				protected Color mPathMarkerProjectionColorTarget = Color.black;
		}
}