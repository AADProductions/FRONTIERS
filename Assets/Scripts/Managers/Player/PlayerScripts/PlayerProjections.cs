using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.Data;

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
				public GameObject PilgrimDirectionArrowPrefab;
				public Projector CurrentPathMarkerProjector;
				public Projector WorldItemPlacementProjector;
				public List <GameObject> DirectionArrows = new List <GameObject>();
				public GameObject ArrowInFocus = null;

				public bool IsFocusingOnArrow {
						get {
								return ArrowInFocus != null;
						}
				}

				public bool GivingDirections {
						get {
								return DirectionArrows.Count > 0;
						}
				}

				public override void Awake()
				{
						DontDestroyOnLoad(PlayerProjectionsObject);
						base.Awake();
				}

				public void HighlightAttachedPaths(Location startLocation, PilgrimCallback chooseDirectionCallback)
				{
						if (DirectionArrows.Count > 0) {
								StopHighlightingPaths(false);
						}

						mChooseDirectionCallback = chooseDirectionCallback;

						//		foreach (string path in startLocation.AttachedPaths) {
						//			if (path.ContinuesInDirection (startLocation, PathDirection.Forward)) {
						//				GameObject arrowGameObject = GameObject.Instantiate (PilgrimDirectionArrowPrefab) as GameObject;
						//				arrowGameObject.transform.position	= startLocation.transform.position;
						//				PilgrimDirectionArrow arrow = arrowGameObject.GetComponent <PilgrimDirectionArrow> ();
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
						//				GameObject arrowGameObject = GameObject.Instantiate (PilgrimDirectionArrowPrefab) as GameObject;
						//				arrowGameObject.transform.position	= startLocation.transform.position;
						//				PilgrimDirectionArrow arrow = arrowGameObject.GetComponent <PilgrimDirectionArrow> ();
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
						if (makeChoice) {
								foreach (GameObject directionArrow in DirectionArrows) {
										PilgrimDirectionArrow arrow = directionArrow.GetComponent <PilgrimDirectionArrow>();
										arrow.OnChooseDirection();
								}
						}
						DirectionArrows.Clear();
						ArrowInFocus = null;
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

						Player.Get.UserActions.Subscribe(UserActionType.ItemUse, new ActionListener(ItemUse));
						Player.Get.UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(ActionCancel));

						WeaponTrajectory = gameObject.CreateChild("RangedWeaponTrajactory").gameObject.AddComponent <RangedWeaponTrajectory>();
				}

				public void PathMarkerVisit()
				{
						//		RefreshPathProjectors ( );
				}

				public bool ActionCancel(double timeStamp)
				{
						if (GivingDirections) {
								StopHighlightingPaths(false);
						}
						return true;
				}

				public bool ItemUse(double timeStamp)
				{
						if (GivingDirections && ArrowInFocus != null) {
								if (mChooseDirectionCallback != null) {
										PilgrimDirectionArrow arrow = ArrowInFocus.GetComponent <PilgrimDirectionArrow>();
										mChooseDirectionCallback(arrow.start, arrow.target, arrow.path, arrow.direction);
								}
						}
						return true;
				}

				public void Update()
				{
						if (GivingDirections) {
								RaycastHit arrowHit;
								if (Physics.Raycast(Player.Local.HeadPosition, Player.Local.FocusVector, out arrowHit, 25.0f, Globals.LayerGUIMap)) {
										GameObject hitObject = arrowHit.collider.transform.parent.gameObject;
										if (DirectionArrows.Contains(hitObject)) {
												ArrowInFocus = hitObject;
										}
								} else {
										ArrowInFocus = null;
								}
						}
						//		if (CurrentPathMarkerProjector.enabled == true)
						//		{
						//			CurrentPathMarkerProjector.transform.parent.Rotate (0f, 0.15f, 0f);
						//			CurrentPathMarkerProjector.orthographicSize = Mathf.Lerp (CurrentPathMarkerProjector.orthographicSize, 	mPathMarkerProjectorSizeTarget, 0.25f);
						//			CurrentPathMarkerProjector.material.color 	= Color.Lerp (CurrentPathMarkerProjector.material.color, 	mPathMarkerProjectionColorTarget, 0.025f);
						//			if (CurrentPathMarkerProjector.material.color.a > 0.5f)
						//			{
						//				mPathMarkerProjectionColorTarget.a = 0.0f;
						//			}
						//			if (CurrentPathMarkerProjector.material.color.a < 0.0015f)
						//			{
						//				CurrentPathMarkerProjector.enabled = false;
						//			}
						//		}


						//		if (Player.Local.ItemPlacement.PlacementPossible)
						//		{
						//			WorldItemPlacementProjector.enabled 				= true;
						//
						//			if (Player.Local.ItemPlacement.PlacementPermitted)
						//			{
						//				WorldItemPlacementProjector.material.color 		= Colors.Get.WorldItemPlacementPermitted;
						//			}
						//			else
						//			{
						//				WorldItemPlacementProjector.material.color 		= Colors.Get.WorldItemPlacementNotPermitted;
						//			}
						//
						//			if (Player.Local.ItemPlacement.PlacementOnTerrainPossible)
						//			{
						//				if (Player.Local.ItemPlacement.IsCarryingSomething)
						//				{
						//					WorldItemPlacementProjector.orthographicSize	= Player.Local.ItemPlacement.CarryObject.collider.bounds.size.x;
						//				}
						//				else
						//				{
						//					WorldItemPlacementProjector.orthographicSize 	= 0.25f;
						//				}
						//				WorldItemPlacementObject.transform.position 		= Player.Local.ItemPlacement.PlacementPreferredPoint + (Vector3.up * 5.0f);
						//			}
						//			else
						//			{
						//				WorldItemPlacementProjector.orthographicSize	= Player.Local.ItemPlacement.PlacementPreferredReceptacle.collider.bounds.size.x;
						//				WorldItemPlacementObject.transform.position 	= Player.Local.ItemPlacement.PlacementPreferredReceptacle.transform.position
						//																	+ Player.Local.ItemPlacement.PlacementPreferredReceptacle.ReceptacleOffset
						//																	+ (Vector3.up * 5.0f);
						//			}
						//		}
						//		else
						//		{
						//			WorldItemPlacementProjector.enabled = false;
						//			return;
						//		}
				}

				protected void SetProjectorsActive(bool active)
				{

				}

				protected void RefreshPathProjectors()
				{
						if (TravelManager.Get.LastVisitedPathMarker != null) {
								PlayerProjectionsObject.transform.position = TravelManager.Get.LastVisitedPathMarker.Position;
								CurrentPathMarkerProjector.enabled = true;
								CurrentPathMarkerProjector.material.color = Colors.PathColors.CurrentPathMarkerProjection;
								CurrentPathMarkerProjector.orthographicSize = 0.01f;
								//mPathMarkerProjectorSizeTarget = TravelManager.Get.LastVisitedPathMarker.State.Radius;
						}

						if (!Paths.HasActivePath) {
								SetProjectorsActive(false);
								return;
						} else {
								SetProjectorsActive(true);
						}

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

				protected PilgrimCallback mChooseDirectionCallback;
				protected float mPathMarkerProjectorSizeTarget;
				protected Color mPathMarkerProjectionColorTarget = Color.black;
		}
}