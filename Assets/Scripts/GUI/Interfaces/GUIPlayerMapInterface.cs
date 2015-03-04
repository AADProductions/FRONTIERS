using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontiers;

using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIPlayerMapInterface : PrimaryInterface
		{
				public Camera RegionLabelsCamera;
				public GameObject MapDestinationPrefab;
				public GameObject MapDestinationPanel;
				public GameObject MapObject;
				public GameObject PlayerPositionObject;
				public UILabel MapTitleLabel;
				public UICheckbox ViewPathsCheckbox;
				public UICheckbox ViewLocationsCheckbox;
				public UICheckbox ViewPassabilityCheckbox;
				public GUIWorldPathBrowser PathBrowser;
				public UIAnchor WorldPathBrowserAnchor;
				public UIAnchor LocationsBrowserAnchor;
				public int MinimizedMapInterfaceDepth	= -3;
				public int MaximizedMapInterfaceDepth	= 4;
				public int MinimizedPathInterfaceDepth	= -2;
				public int MaximizedPathInterfaceDepth	= 5;

				public override void Start()
				{
						base.Start();

						//Player.Get.AvatarActions.Subscribe (AvatarAction.RegionLoad, new ActionListener (RegionLoad));
						//Player.Get.AvatarActions.Subscribe (AvatarAction.PathChange, new ActionListener (PathChange));

						//		if (GameManager.Get.RegionMap != null)
						//		{
						//			GameManager.Get.RegionMap.OnInterfaceCreated (this);
						//		}
				}

				public void RegionLoad()
				{
						//		ClearMap ( );
						//		AlignLabelsToRegion ( );
						//		RefreshMap ( );
						//		RefreshPaths ( );
				}
				//	public GUIRegionMapLocation		AddLocation (Location newDestination)
				//	{
				//		return null;
				//////		//Debug.Log ("Adding to map");
				////		GameObject newMapDestinationGameObject 	= NGUITools.AddChild (MapDestinationPanel, MapDestinationPrefab);
				////		newMapDestinationGameObject.transform.localScale = Vector3.one * 0.0005f;
				////		newMapDestinationGameObject.transform.Rotate (90.0f, 0f, 0f);
				////		GUIRegionMapLocation newMapDestination 	= newMapDestinationGameObject.GetComponent <GUIRegionMapLocation> ( );
				////		newMapDestination.Destination			= newDestination;
				////		newMapDestination.Refresh (true);
				////
				////		RefreshMap ( );
				////
				////		return newMapDestination;
				//	}

				public override void Update()
				{
						//		if (Player.Local.State.HasSpawned && GameManager.GameState == FrontiersGameState.InWorld)
						//		{
						//			Vector3 iconPosition = Player.PlayerRegionMapPosition;
						//			PlayerPositionObject.transform.localPosition = new Vector3 (iconPosition.x, PlayerPositionObject.transform.localPosition.y, iconPosition.z);
						//			PlayerPositionObject.transform.localRotation = Quaternion.identity;
						//			PlayerPositionObject.transform.Rotate (0.0f, Player.Rotation.y, 0.0f);
						//		}
						//		
						//		if (Maximized)
						//		{
						//			RegionLabelsCamera.transform.position 	= GameManager.Get.Travel.Navigation.WorldMapCamera.transform.position;
						//			RegionLabelsCamera.transform.rotation	= GameManager.Get.Travel.Navigation.WorldMapCamera.transform.rotation;
						//			RegionLabelsCamera.orthographicSize		= GameManager.Get.Travel.Navigation.WorldMapCamera.orthographicSize;
						//		}
				}
				//	public override void			LateUpdate ( )
				//	{
				//		base.LateUpdate ( );
				//
				//		WorldPathBrowserAnchor.relativeOffset = new Vector2 (Mathf.Lerp (WorldPathBrowserAnchor.relativeOffset.x, mWorldPathBrowserOffsetTarget, mBrowserAnimationSpeed), 0.0f);
				//		LocationsBrowserAnchor.relativeOffset = new Vector2 (Mathf.Lerp (LocationsBrowserAnchor.relativeOffset.x, mLocationsBrowserOffsetTarget, mBrowserAnimationSpeed), 0.0f);
				//	}
				public void ClearMap()
				{
						//		foreach (Transform child in MapDestinationPanel.transform)
						//		{
						//			GameObject.Destroy (child.gameObject);
						//		}
				}

				public void AlignLabelsToRegion()
				{
						//		MapObject.transform.parent 			= WorldRegionManager.Get.CurrentLoadedRegion.WorldMapBounds.transform;
						//		MapObject.transform.localPosition 	= Vector3.zero;
						//		MapObject.transform.localScale		= Vector3.one;
						//		MapObject.transform.Translate (Vector3.up);
				}

				public void RefreshMap()
				{
						//		MapTitleLabel.text			= WorldRegionManager.Get.CurrentLoadedRegion.StackName;
				}

				public void PathChange()
				{
						//		RefreshMap ( );
						//		RefreshPaths ( );
				}

				public void RefreshPaths()
				{
						//		IEnumerable <Path> visibleWorldPaths =
						//			from path in Paths.Get.DynamicPaths
						//			where path.Props.IsHidden	== false
						//				&& path.HasBeenRevealed == true
						//				&& path.ParentRegion 	== WorldRegionManager.Get.CurrentLoadedRegion
						//			select path;
						//
						//		PathBrowser.ReceiveFromParentEditor (visibleWorldPaths);
				}

				public override bool Maximize()
				{
						//		if (base.Maximize ( ))
						//		{
						//			RegionLabelsCamera.enabled						= true;
						////			GameManager.Get.Travel.Navigation.Mode			= WorldMapNavigation.WorldMapMode.Move;
						//			RefreshPaths ( );
						//			return true;
						//		}
						//
						return false;
				}

				public override bool Minimize()
				{
						//		if (base.Minimize ( ))
						//		{
						//			RegionLabelsCamera.enabled						= false;
						//			GameManager.Get.Travel.Navigation.Mode 			= WorldMapNavigation.WorldMapMode.FollowMiniMap;
						//			return true;
						//		}
						//
						return false;
				}

				public void OnChangeViewPassability()
				{
						//		MapPassability.renderer.enabled = ViewPassabilityCheckbox.isChecked;
				}

				public void OnChangeViewPaths()
				{
						//		if (ViewPathsCheckbox.isChecked)
						//		{
						//			mWorldPathBrowserOffsetTarget	= 0.0f;
						//		}
						//		else
						//		{
						//			mWorldPathBrowserOffsetTarget	= 0.25f;
						//		}
				}

				public void OnChangeViewLocations()
				{
						//		if (ViewLocationsCheckbox.isChecked)
						//		{
						//			mLocationsBrowserOffsetTarget = 0.0f;
						//		}
						//		else
						//		{
						//			mLocationsBrowserOffsetTarget = -.25f;
						//		}
				}

				protected float mLocationsBrowserOffsetTarget = 0.0f;
				protected float mWorldPathBrowserOffsetTarget = 0.0f;
				protected float mBrowserAnimationSpeed = 0.25f;
		}
}