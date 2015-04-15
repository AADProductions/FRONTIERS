using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Frontiers.GUI;

namespace Frontiers
{
		public class WorldMap : Manager
		{
				public static WorldMap Get;

				public override string GameObjectName {
						get {
								return "Frontiers_MapManager";
						}
				}

				public int NumXTiles;
				public int NumZTiles;
				public static Transform WorldMapTransform = null;
				//convenience functions, simpler to refer to WorldMap.x
				public static List <MobileReference> NewLocations {
						get {
								return Profile.Get.CurrentGame.NewLocations;
						}
				}

				public static List <MobileReference> MarkedLocations {
						get {
								return Profile.Get.CurrentGame.MarkedLocations;
						}
				}

				public static List <MobileReference> RevealedLocations {
						get {
								return Profile.Get.CurrentGame.RevealedLocations;
						}
				}

				public static List <string> LocationTypesToDisplay {
						get {
								return Profile.Get.CurrentGame.LocationTypesToDisplay;
						}
				}

				public static List <Path> RelevantPathsToDisplay {
						get {
								return Paths.Get.RelevantPaths;
						}
				}

				public static List <Path> NonRelevantPathsToDisplay {
						get {
								return Paths.Get.NonRelevantPaths;
						}
				}

				public static void ClearLog()
				{
						MarkedLocations.Clear();
						RevealedLocations.Clear();
						NewLocations.Clear();
				}

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						WorldMapTransform = Get.gameObject.CreateChild("WorldMapTransform").transform;
						WorldMapTransform.localScale = Vector3.one * (1 / 500f);
						WorldMapTransform.position = Vector3.zero;
						mRevealableToShow = new Dictionary <string, List <MobileReference>>();
				}

				public override void OnGameLoadFirstTime()
				{
						UIAtlas icons = Mats.Get.MapIconsAtlas;
						foreach (UIAtlas.Sprite sprite in icons.spriteList) {
								LocationTypesToDisplay.Add(sprite.name);
						}
				}
				//searches chunks and loads location data into the locationData list
				//only searches for locations added using the SetMapData function
				public IEnumerator LocationsForChunks(string chunkName, int chunkID, Queue <WorldMapLocation> locationData, List <MapMarker> activeMapMarkers)
				{	//Debug.Log ("WorldMap: LocationsForChunks");
						List <MobileReference> locationList = null;

						if (!mRevealableToShow.TryGetValue(chunkName, out locationList)) {
								//Debug.Log ("WorldMap: We have no locations for this chunk from SetMapData, returning immediately");
								yield break;
						}
						Vector3 chunkPosition;
						//add markers
						for (int i = 0; i < activeMapMarkers.Count; i++) {
								if (activeMapMarkers[i].ChunkID == chunkID) {
										chunkPosition = activeMapMarkers[i].ChunkPosition;
										WorldMapLocation wml = new WorldMapLocation(
												                  null,
												                  1,
												                  "Marker",
												                  string.Empty,
												                  string.Empty,
												                  true,
												                  false,
												                  "MapIconMapMarker",
												                  Color.white,
												                  MapIconStyle.Medium,
												                  MapLabelStyle.None,
												                  Vector3.zero,
												                  chunkPosition,
												                  false,
												                  false,
												                  LocationTypesToDisplay);
										locationData.Enqueue(wml);
								}
						}

						MobileReference currentLocation = null;
						for (int i = 0; i < locationList.Count; i++) {
								if (!LoadData) {
										//Debug.Log ("WorldMap: Load data is false, returning");
										yield break;
								}
								currentLocation = locationList[i];
								StackItem stackItem = null;
								if (WIGroups.LoadStackItem(currentLocation, out stackItem)) {
										chunkPosition = stackItem.ChunkPosition;
										//Debug.Log ("WorldMap: found stack item " + stackItem.DisplayName);
										//next we need to get the location state from the stack item
										LocationState ls = null;
										RevealableState rs = null;
										VisitableState vs = null;
										bool isNewLocation = false;
										bool isMarked = false;
										if (NewLocations.Count > 0) {
												for (int j = NewLocations.LastIndex(); j >= 0; j--) {
														if (NewLocations[j] == currentLocation) {
																NewLocations.RemoveAt(j);
																isNewLocation = true;
																break;
														}
												}
										}
										//Debug.Log ("current location " + currentLocation.FullPath + " is marked? " + isMarked.ToString ( ));
										isMarked = MarkedLocations.Contains(currentLocation);

										if (stackItem.GetStateData <RevealableState>(out rs)) {
												stackItem.GetStateData <LocationState>(out ls);
												stackItem.GetStateData <VisitableState>(out vs);
												//now convert it into a world map location
												WorldMapLocation wml = null;
												if (rs.CustomMapSettings || ls == null) {
														//non-custom settings come from the location
														//so we can only use custom settings if ls is not null
														//Debug.Log ("Custom settings for revealable with " + rs.IconName + " icon name");
														wml = new WorldMapLocation(
																currentLocation,
																stackItem.Props.Local.ActiveRadius,
																ls != null ? ls.Name.CommonName : string.Empty,
																ls != null ? ls.Name.ProperName : string.Empty,
																ls != null ? ls.Name.NickName : string.Empty,
																vs != null ? vs.HasBeenVisited : true,
																rs.MarkedForTriangulation,
																rs.IconName,
																rs.IconColor,
																rs.IconStyle,
																rs.LabelStyle,
																rs.IconOffset,
																chunkPosition,
																isNewLocation,
																isMarked,
																LocationTypesToDisplay);
														locationData.Enqueue(wml);
												} else {
														string iconName = "Outpost";
														MapIconStyle iconStyle = MapIconStyle.None;
														MapLabelStyle labelStyle = MapLabelStyle.None;
														Color32 iconColor = Color.gray;
														Vector3 iconOffset = Vector3.zero;

														GetIconProperties(stackItem, ls, rs, vs, ref iconName, ref iconStyle, ref labelStyle, ref iconColor, ref iconOffset);

														wml = new WorldMapLocation(
																currentLocation,
																stackItem.Props.Local.ActiveRadius,
																ls != null ? ls.Name.CommonName : string.Empty,
																ls != null ? ls.Name.ProperName : string.Empty,
																ls != null ? ls.Name.NickName : string.Empty,
																vs != null ? vs.HasBeenVisited : true,
																rs.MarkedForTriangulation,
																iconName,
																iconColor,
																iconStyle,
																labelStyle,
																iconOffset,
																chunkPosition,
																isNewLocation,
																isMarked,
																LocationTypesToDisplay);
														locationData.Enqueue(wml);
												}
										} else {
												//Debug.Log ("Didn't get revealable state data in " + currentLocation.FileName);
										}
								} else {
										//Debug.Log ("Didin't get stack item for location " + currentLocation.FullPath);
								}
								//clear the stack item, we don't need it any more
								if (stackItem != null) {
										stackItem.Clear();
								}
								yield return null;
						}
						locationList.Clear();
						yield break;
				}

				public static Color32 GetIconColor(LocationState ls, VisitableState vs)
				{
						return Color.white;
				}

				public static MapLabelStyle GetLabelStyle(LocationState ls)
				{
						return MapLabelStyle.MouseOver;
				}

				public static bool MarkLocation(MobileReference mr)
				{
						//Debug.Log ("marking location " + mr.FullPath);
						return MarkedLocations.SafeAdd(mr);
				}

				public static bool MarkLocation(WorldMapLocation wml)
				{
						//Debug.Log ("Marking location " + wml.Reference.FullPath);
						if (!MarkedLocations.SafeAdd(wml.Reference)) {
								MarkedLocations.Remove(wml.Reference);
								wml.IsMarked = false;
								wml.IsNew = false;
								return false;
						}
						wml.IsMarked = true;
						NGUIWorldMap.Get.RemoveMarkedLocationSprite(wml);
						NGUIWorldMap.Get.CreateMarkedLocationSprite(wml);
						return true;
				}

				public static MapDirection GetClockwiseMapDirection(Transform lockObject)
				{
						switch (GetMapDirectionFromRotation(lockObject)) {
								case MapDirection.A_North:
										return MapDirection.B_NorthEast;

								case MapDirection.B_NorthEast:
										return MapDirection.C_East;

								case MapDirection.C_East:
										return MapDirection.D_SEast;

								case MapDirection.D_SEast:
										return MapDirection.E_South;

								case MapDirection.E_South:
										return MapDirection.F_SouthWest;

								case MapDirection.F_SouthWest:
										return MapDirection.G_West;

								case MapDirection.G_West:
										return MapDirection.H_NorthWest;

								case MapDirection.H_NorthWest:
										return MapDirection.A_North;

								case MapDirection.I_None:
								default:
										return MapDirection.B_NorthEast;
						}
				}

				public static string GetMapDirectionNameFromRotation(Transform lockObject)
				{
						MapDirection dir = GetMapDirectionFromRotation(lockObject);
						switch (dir) {
								case MapDirection.A_North:
										return "North";

								case MapDirection.B_NorthEast:
										return "Northeast";

								case MapDirection.C_East:
										return "East";

								case MapDirection.D_SEast:
										return "Southeast";

								case MapDirection.E_South:
										return "South";

								case MapDirection.F_SouthWest:
										return "Southwest";

								case MapDirection.G_West:
										return "West";

								case MapDirection.H_NorthWest:
										return "Northwest";

								case MapDirection.I_None:
								default:
										return "North";
						}
				}

				public static MapDirection GetMapDirectionFromRotation(Transform lockObject)
				{
						float rotation = lockObject.rotation.eulerAngles.y;
						if (rotation < (int)MapDirection.B_NorthEast) {
								return MapDirection.A_North;
						} else if (rotation < (int)MapDirection.C_East) {
								return MapDirection.B_NorthEast;
						} else if (rotation < (int)MapDirection.D_SEast) {
								return MapDirection.C_East;
						} else if (rotation < (int)MapDirection.E_South) {
								return MapDirection.D_SEast;
						} else if (rotation < (int)MapDirection.F_SouthWest) {
								return MapDirection.E_South;
						} else if (rotation < (int)MapDirection.G_West) {
								return MapDirection.F_SouthWest;
						} else if (rotation < (int)MapDirection.H_NorthWest) {
								return MapDirection.G_West;
						} else if (rotation < 360) {
								return MapDirection.H_NorthWest;
						} else {
								return MapDirection.A_North;
						}
				}

				public static void CreateLocationLabel(GUI.GUIMapTile mapTile, WorldMapLocation wml)
				{
						//Debug.Log ("Creating location label for " + wml.Name);

						bool createIcon = true;
						bool createLabel = true;

						switch (wml.LabelStyle) {
								case MapLabelStyle.AlwaysVisible:
								default:
										createLabel = true;
										break;

								case MapLabelStyle.None:
										createLabel = false;
										break;

								case MapLabelStyle.Descriptive:
										createIcon = false;
										createLabel = true;
										break;
						}

						if (createIcon) {
								//sort it into the right list based on icon
								switch (wml.IconStyle) {
										case MapIconStyle.Small:
												mapTile.SmallLocations.Add(wml);
												break;

										case MapIconStyle.Medium:
												mapTile.MediumLocations.Add(wml);
												break;

										case MapIconStyle.Large:
												mapTile.LargeLocations.Add(wml);
												break;

										case MapIconStyle.AlwaysVisible:
												mapTile.ConstantLocations.Add(wml);
												break;

										case MapIconStyle.None:
										default:
												//Debug.Log("Icon style was none in " + wml.IconName);
				//NONE
												createIcon = false;
				//does it have a label?
												if (createLabel) {
														//if so, keep it in the constant list
														mapTile.ConstantLocations.Add(wml);
												} else {
														//otherwise, just ignore it
														//dont bother to create labels
														//return;
												}
												break;
								}
						} else {
								mapTile.ConstantLocations.Add(wml);
						}

						Vector3 mapTilePosition = new Vector3(wml.ChunkPosition.x, wml.ChunkPosition.z, 0f);//-(wml.ChunkPosition.y));
						wml.LocationTransform = mapTile.TileBackground.gameObject.CreateChild(wml.Name).transform;
						mapTilePosition.z = NGUIWorldMap.GetWorldMapAtChunkPosition(mapTilePosition, mapTile.ChunkToDisplay.MiniHeightmap, 0f);
						wml.LocationTransform.localPosition = mapTilePosition;

						if (createLabel) {
								float mapTileScale = 1f;//Mathf.Clamp (location.Radius * gLocationRadiusMult, 1f, 5f);
								GameObject newWMLabelGo	= NGUITools.AddChild(NGUIWorldMap.Get.LabelsPanel.gameObject, mapTile.WMLabelPrefab);
								UILabel label = newWMLabelGo.GetComponent <UILabel>();
								label.name = wml.Name;
								label.text = wml.Name;
								if (wml.LabelStyle == MapLabelStyle.Descriptive) {
										label.font = Mats.Get.CleanHandwriting42Font;
								} else {
										label.font = Mats.Get.CleanHandwriting42Font;//PrintingPress40Font;
								}
								wml.Label = label;
								if (!createIcon) {
										wml.LabelPosition = Vector3.zero;
								}
								wml.LabelTransform = label.cachedTransform;
								wml.LabelTransform.localPosition = mapTilePosition + wml.LabelPosition + wml.IconOffset;
								wml.LabelTransform.localScale = Vector3.one;
								label.alpha = 0f;
								label.enabled = true;
						}

						if (createIcon) {
								GameObject newWMIconGo = NGUITools.AddChild(NGUIWorldMap.Get.IconsPanel.gameObject, mapTile.WMIconPrefab);
								UISprite icon = newWMIconGo.GetComponent <UISprite>();
								WMIcon wmIcon = newWMIconGo.GetComponent <WMIcon>();
								wmIcon.OnClick += wml.OnClick;
								wmIcon.Reference = wml.Reference;
								icon.name = wml.Name;
								icon.spriteName = wml.IconName;
								wml.Icon = icon;
								wml.IconTransform = wml.Icon.cachedTransform;
								wml.IconTransform.localPosition = mapTilePosition + wml.IconOffset;
								wml.IconTransform.localScale = Vector3.one * wml.IconScale;
								icon.alpha = 0f;
								icon.enabled = true;
								wml.Collider = newWMIconGo.AddComponent <SphereCollider>();//for mouseovers

								if (wml.IsMarked || wml.IsNew) {
										NGUIWorldMap.Get.CreateMarkedLocationSprite(wml);
								}
						} else {
								//Debug.Log ("No create icon with icon name " + wml.IconName + " and icon style " + wml.IconStyle.ToString ());
						}
				}

				public static string GetIconProperties(
						StackItem stackitem,
						LocationState ls,
						RevealableState rs, 
						VisitableState vs,
						ref string iconName, 
						ref MapIconStyle iconStyle,
						ref MapLabelStyle labelStyle, 
						ref Color32 iconColor,
						ref Vector3 iconOffset)
				{

						iconName = "MapIconOutpost";
						iconStyle = MapIconStyle.Small;
						labelStyle = MapLabelStyle.None;
						iconColor = Color.grey;

						switch (ls.Type) {
								case "City":
										if (stackitem.Props.Local.ActiveRadius > Globals.CityMinimumRadius) {
												iconName = "MapIconCity";
												iconStyle = MapIconStyle.Large;
												labelStyle = MapLabelStyle.MouseOver;
										} else if (stackitem.Props.Local.ActiveRadius > Globals.TownMinimumRadius) {
												iconStyle = MapIconStyle.Medium;
												iconName = "MapIconTown";
												labelStyle = MapLabelStyle.MouseOver;
										} else {
												iconName = "MapIconOutpost";
												iconStyle = MapIconStyle.Small;
												labelStyle = MapLabelStyle.MouseOver;
										}
										break;

								case "CapitalCity":
										iconName = "MapIconCapitalCity";
										iconStyle = MapIconStyle.AlwaysVisible;
										labelStyle = MapLabelStyle.AlwaysVisible;
										break;

								case "Woods":
										iconName = "MapIconWoods";
										iconStyle = MapIconStyle.Large;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Den":
										iconName = "MapIconAnimalDen";
										iconStyle = MapIconStyle.Medium;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "CrossRoads":
										iconName = "MapIconCrossRoads";
										iconStyle = MapIconStyle.AlwaysVisible;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "WayStone":
										iconName = "MapIconWayStone";
										iconStyle = MapIconStyle.Small;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Shingle":
								case "Residence":
								case "Shop":
										iconName = "MapIconStructure";
										iconStyle = MapIconStyle.Small;
										labelStyle = MapLabelStyle.MouseOver;
										StructureState ss = null;
										if (stackitem.GetStateData <StructureState>(out ss)) {
												iconOffset.x = -ss.PrimaryBuilderOffset.Position.x;
												iconOffset.y = -ss.PrimaryBuilderOffset.Position.z;
										}
										break;

								case "Landmark":
										iconName = "MapIconLandmark";
										iconStyle = MapIconStyle.AlwaysVisible;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "HouseOfHealing":
										iconName = "MapIconHouseOfHealing";
										iconStyle = MapIconStyle.Large;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Gateway":
										iconName = "MapIconGateway";
										iconStyle = MapIconStyle.Large;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Cemetary":
										iconName = "MapIconCemetary";
										iconStyle = MapIconStyle.Medium;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Dungeon":
										iconName = "MapIconCave";
										iconStyle = MapIconStyle.Medium;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "Campsite":
										iconName = "MapIconCampsite";
										iconStyle = MapIconStyle.Medium;
										labelStyle = MapLabelStyle.MouseOver;
										break;

								case "District":
								case "CrossStreet":
								default:
										iconStyle = MapIconStyle.None;
										labelStyle = MapLabelStyle.Descriptive;
										break;
						}

						if (rs.UnknownUntilVisited) {
								if (vs == null || vs.NumTimesVisited <= 0) {
										iconName = "MapIconUnknown";
								}
						}
						return iconName;
				}

				public static MapIconStyle GetIconStyle(LocationState ls)
				{
						return MapIconStyle.Small;
				}

				public IEnumerator LocationData(Queue <MobileReference> locations, Queue <WorldMapLocation> locationData)
				{
						//		while (locations.Count > 0)
						//		{	//get the next in the queue
						//			MobileReference locationReference 	= locations.Dequeue ( );
						//			StackItem location					= null;
						//			if (WIGroups.LoadStackItem (locationReference, out location))
						//			{
						//				//get the location state
						//				LocationState locationState = null;
						//				if (location.GetStateData <LocationState> (out locationState))
						//				{	//hooray, we've done it!
						//					locationData.Enqueue (new WorldMapLocation (locationState));
						//				}
						//				//wait a tick
						//				yield return null;
						//			}
						//		}
						yield break;
				}
				//uses global settings to convert world position to world map position
				public static Vector3 WPtoWMP(Vector3 WP)
				{
						return WorldMapTransform.TransformPoint(WP);
				}
				//uses global settings to convert world position to ??? figure this out again lol
				public static Vector3 WPtoMMP(Vector3 WP)
				{
						return WP;
				}

				public static bool LoadData {
						get {
								return mLoadData;
						}
						set {
								mLoadData = value;
						}
				}

				public static void SetMapData(List <MobileReference> revealable)
				{

						mRevealableToShow.Clear();
						string fullPath = null;
						string chunkName = null;
						List <MobileReference> locationList = null;
						//do this to filter out duplicates and to extract chunk names
						for (int i = 0; i < revealable.Count; i++) {
								chunkName = revealable[i].ChunkName;
								if (!mRevealableToShow.TryGetValue(chunkName, out locationList)) {
										locationList = new List <MobileReference>();
										mRevealableToShow.Add(chunkName, locationList);
								}
								locationList.SafeAdd(revealable[i]);
						}
						//ok now that everything's ready to go
				}

				public static void RevealAll()
				{
						if (!mRevealingAll) {
								mRevealingAll = true;
								Get.StartCoroutine(Get.RevealAllOverTime());
						}

				}

				protected IEnumerator RevealAllOverTime () {
						Queue<StackItem> itemsToReveal = new Queue<StackItem>();
						Debug.Log("Searching for items...");
						yield return StartCoroutine(WIGroups.GetAllStackItemsByType(WIGroups.Get.World.Path, new List <string>() { "Location" }, GroupSearchType.SavedOnly, itemsToReveal));
						Debug.Log("Adding items to revealed list...");
						while (itemsToReveal.Count > 0) {
								StackItem item = itemsToReveal.Dequeue();
								if (item != null) {
										RevealedLocations.SafeAdd(item.StaticReference);
								}
								yield return null;
						}
						Debug.Log("Done revealing");
				}

				protected static Dictionary <string, List <MobileReference>> mRevealableToShow;
				protected bool mGatheringLocationData = false;
				protected static bool mLoadData = true;
				protected static bool mRevealingAll = false;
		}
		//this struct is constantly updated by GUIWorldMap to reflect what the player is looking at
		//the queue sent to GetLocationsForChunks is then sorted to reflect the new priority
		[Serializable]
		public struct WorldMapChunk
		{
				public static WorldMapChunk Empty {
						get {
								return new WorldMapChunk(Vector3.zero, 0f);
						}
				}

				public WorldMapChunk(Vector3 tileOffset, float tileScale)
				{
						Name = string.Empty;
						ChunkID = -1;
						DistanceFromFocus = Mathf.Infinity;
						Mode = ChunkMode.Unloaded;
						TileOffset = tileOffset;
						TileScale = tileScale;
						TileElevation = 0f;
						DisplaySettings = null;
						LocationsToDisplay = new Queue <WorldMapLocation>();
						MiniHeightmap = null;
						BlendIDTop = -1;
						BlendIDBottom = -1;
						BlendIDLeft = -1;
						BlendIDRight = -1;
						MagicChunkScale = 6.5f;
				}

				public WorldMapChunk(WorldChunk chunk)
				{
						Name = chunk.Name;
						ChunkID = chunk.State.ID;
						DistanceFromFocus = Mathf.Infinity;
						Mode = chunk.CurrentMode;
						TileOffset = chunk.State.TileOffset;
						TileScale = chunk.State.SizeX;
						TileElevation = chunk.TerrainData.HeightmapHeight;
						DisplaySettings = chunk.State.DisplaySettings;
						LocationsToDisplay = new Queue <WorldMapLocation>();
						MiniHeightmap = null;
						Mods.Get.Runtime.ChunkMap(ref MiniHeightmap, Name, "MiniHeightMap");
						BlendIDTop = chunk.State.NeighboringChunkTop;
						BlendIDBottom = chunk.State.NeighboringChunkBot;
						BlendIDLeft = chunk.State.NeighboringChunkLeft;
						BlendIDRight = chunk.State.NeighboringChunkRight;
						MagicChunkScale = 6.5f;
				}

				public bool IsEmpty { get { return ChunkID < 1; } }

				public string Name;
				public int ChunkID;
				public int BlendIDTop;
				public int BlendIDBottom;
				public int BlendIDLeft;
				public int BlendIDRight;
				public float DistanceFromFocus;
				public Vector3 TileOffset;
				public float TileScale;
				public float TileElevation;
				public float MagicChunkScale;
				public ChunkDisplaySettings DisplaySettings;
				public ChunkMode Mode;
				public Queue <WorldMapLocation>	LocationsToDisplay;
				public Texture2D MiniHeightmap;
		}

		[Serializable]
		public class WorldMapLocation
		{
				public WorldMapLocation(
						MobileReference reference,
						float radius,
						string name,
						string properName,
						string alternateName,
						bool visited,
						bool markedForTriangulation,
						string iconName,
						Color32 iconColor,
						MapIconStyle iconStyle,
						MapLabelStyle labelStyle,
						Vector3 iconOffset,
						Vector3 chunkPosition,
						bool isNew,
						bool isMarked,
						List <string> typesToDisplay)
				{
						Reference = reference;
						Radius = radius;
						Name = name;
						mLowerName = Name.ToLower();
						ProperName = properName;
						AlternateName = alternateName;
						Visited = visited;
						MarkedForTriangulation = markedForTriangulation;
						IconName = iconName;
						IconColor = Color.gray;//iconColor;
						IconStyle = iconStyle;
						LabelStyle = labelStyle;
						ChunkPosition = chunkPosition;
						if (LabelStyle == MapLabelStyle.Descriptive) {
								LabelColor = Colors.Get.WorldMapLabelDescriptiveColor;
						} else {
								LabelColor = Colors.Get.WorldMapLabelColor;
						}

						//Debug.Log ("Icon name: " + IconName);

						Label = null;
						Icon = null;
						LabelTransform = null;
						IconTransform = null;
						LabelPosition = Vector3.zero;
						IconPosition = Vector3.zero;
						IconOffset = iconOffset;
						LabelScale = 1f;
						IconScale = 1f;
						IconAlpha = 1f;
						IsNew = isNew;
						IsMarked = isMarked;
						Display = typesToDisplay.Contains(IconName) || (reference == null);

						switch (IconStyle) {
								case MapIconStyle.AlwaysVisible:
										IconScale = 250f;
										LabelScale = 1.1f;
										break;

								case MapIconStyle.Large:
								case MapIconStyle.Medium:
								case MapIconStyle.Small:
								case MapIconStyle.None:
								default:
										IconScale = 75f;
										LabelScale = 1f;
										break;
						}
						//LabelPosition = Vector3.up * IconScale;
				}

				public void UpdateType(List <string> typesToDisplay)
				{
						Display = (LabelStyle == MapLabelStyle.Descriptive && typesToDisplay.Contains("Descriptive")) || typesToDisplay.Contains(IconName);
						SearchHit = false;
				}

				public void UpdateType(List <string> typesToDisplay, string searchFilter)
				{
						if (mLowerName.Contains(searchFilter)) {
								SearchHit = true;
								Display = (LabelStyle == MapLabelStyle.Descriptive && typesToDisplay.Contains("Descriptive")) || typesToDisplay.Contains(IconName);
						} else {
								Display = false;
						}
				}

				public void UpdateLabel(float scale, float labelScale, float maxDistance, float minScale, float maxScale, float alpha, Camera perspectiveCamera, Camera interfaceCamera)
				{
						Vector3 pos = perspectiveCamera.WorldToViewportPoint(LocationTransform.position);
						float distanceFromCamera = 0f;
						// Determine the visibility and the target alpha
						bool isVisible = (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
						// If visible, update the position
						if (isVisible) {
								if (LabelTransform != null) {
										LabelTransform.position = interfaceCamera.ViewportToWorldPoint(pos);
										pos = LabelTransform.localPosition;
								} else if (IconTransform != null) {
										IconTransform.position = interfaceCamera.ViewportToWorldPoint(pos);
										pos = IconTransform.localPosition;
								}
								pos.x = Mathf.RoundToInt(pos.x);
								pos.y = Mathf.RoundToInt(pos.y);

								float fadeStart = maxDistance * 0.75f;
								distanceFromCamera = Vector3.Distance(perspectiveCamera.transform.position, LocationTransform.position);
								pos.z = -distanceFromCamera;
								scale = Mathf.Clamp(scale / distanceFromCamera, minScale, maxScale);

								if (distanceFromCamera > maxDistance) {
										isVisible = false;
										alpha = 0f;
								} else if (distanceFromCamera >= fadeStart) {
										float fadeAmount = (distanceFromCamera - fadeStart) / (maxDistance - fadeStart);
										alpha = Mathf.Lerp(alpha, 0f, fadeAmount);
								}
						}

						if (!isVisible) {
								//Debug.Log(Name + " was not visible: " + pos.ToString());
								if (Label != null) {
										Label.enabled = false;
								}
								if (Icon != null) {
										Icon.enabled = false;
								}
								if (Attention != null) {
										Attention.enabled = false;
								}
								if (Collider != null) {
										Collider.enabled = false;
								}
								return;
						} else {
								if (LabelTransform != null) {
										LabelTransform.localPosition = pos;
										Label.enabled = true;
										Label.depth = Mathf.FloorToInt(-distanceFromCamera * 1000);
								}
								if (IconTransform != null) {
										IconTransform.localPosition = pos;
										Icon.enabled = true;
										Icon.depth = Mathf.FloorToInt(-distanceFromCamera * 1000);
								}
								if (AttentionTransform != null) {
										AttentionTransform.localPosition = pos;
										Attention.enabled = true;
										Attention.depth = Mathf.FloorToInt(-distanceFromCamera * 1000);
								}
								if (Collider != null) {
										Collider.enabled = true;
								}
						}

						bool mouseOver = Icon != null && (UICamera.hoveredObject == Icon.gameObject);
						if (Label != null) {
								Label.color = Colors.Alpha(LabelColor, Label.alpha);
								if (Display) {
										if (SearchHit) {
												Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
										} else {
												switch (LabelStyle) {
														case MapLabelStyle.MouseOver:
																if (mouseOver || IsMarked) {
																		Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
																} else {
																		Label.alpha = Mathf.Lerp(Label.alpha, 0f, 0.25f);
																}
																break;

														case MapLabelStyle.Descriptive:
																//fade out descriptive labels when we get clsoe
																if (distanceFromCamera < (maxDistance / 5)) {
																		alpha = 0f;
																}
																Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
																break;

														case MapLabelStyle.AlwaysVisible:
																if (mouseOver || IsMarked) {
																		Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
																} else {
																		Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
																}
																break;

														default:
																Label.alpha = Mathf.Lerp(Label.alpha, alpha, 0.25f);
																break;
												}
										}
								} else {
										Label.alpha = Mathf.Lerp(Label.alpha, 0f, 0.25f);
								}
								LabelTransform.localScale = ((LabelScale * labelScale) * scale) * Vector3.one;
								//LabelTransform.position = pos;
						}

						if (Icon != null) {
								if (Display) {
										if (IsMarked) {
												Icon.color = Attention.color;
										} else {
												Icon.color = Colors.Alpha(IconColor, alpha);
										}
										Icon.alpha = Mathf.Lerp(Icon.alpha, alpha, 0.25f);
										IconTransform.localScale = (IconScale * scale) * Vector3.one;
								} else {
										Icon.alpha = Mathf.Lerp(Icon.alpha, 0f, 0.25f);
								}
						}

						if (Attention != null) {
								if (Display) {
										if (IsMarked || IsNew) {
												Attention.alpha = Mathf.Lerp(Attention.alpha, alpha, 0.25f);
										} else {
												Attention.alpha = Mathf.Lerp(Attention.alpha, 0f, 0.25f);
										}
								} else {
										Attention.alpha = Mathf.Lerp(Attention.alpha, 0f, 0.25f);
								}
								AttentionTransform.localScale = IconTransform.localScale;
						}
				}

				public void OnClick()
				{
						WorldMap.MarkLocation(this);
				}

				public MobileReference Reference;
				public string Name;
				public string ProperName;
				public string AlternateName;
				public float Radius;
				public bool Visited;
				public bool MarkedForTriangulation;
				public string IconName;
				public Color32 IconColor;
				public Color32 LabelColor;
				public MapIconStyle IconStyle;
				public MapLabelStyle LabelStyle;
				public Vector3 ChunkPosition;
				public UILabel Label;
				public UISprite Icon;
				public UISprite Attention;
				public Transform LocationTransform;
				public Transform LabelTransform;
				public Transform IconTransform;
				public Transform AttentionTransform;
				public Vector3 LabelPosition;
				public Vector3 IconPosition;
				public Vector3 IconOffset;
				public float LabelScale;
				public float IconScale;
				public float IconAlpha;
				public bool Display;
				public SphereCollider Collider;
				public bool SearchHit;
				public bool IsNew;
				public bool IsMarked;
				protected string mLowerName;
		}
}
