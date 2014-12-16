using System;
using UnityEngine;

//using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.World.Gameplay;
using System.Collections;

namespace Frontiers
{
		[ExecuteInEditMode]
		public class Paths : Manager
		{
				public static Paths Get;

				public static bool IsEvaluating {
						get {
								return Get.Evaluator.Evaluating;
						}
				}

				public static bool HasActivePath {
						get {
								return ActivePath.spline.enabled;
						}
				}

				public static float NormalizedDistanceFromPath {
						get {
								if (HasActivePath) {
										return Player.Local.GroundPath.PlayerDistanceFromPath / Globals.PathStrayDistanceInMeters;
								}
								return 1f;
						}
				}

				public static float PlayerDistanceFromPassivePath;

				public static PathAvatar ActivePath {
						get {
								return Get.mActivePath;
						}
				}

				public string LoadPathName;
				public PathAvatar WorldPathPrefab;
				public SplineNode SplineNodePrefab;
				public PathDifficultyEvaluator Evaluator;
				public List <PathMarker> ActivePathMarkers = new List <PathMarker>();
				public List <PathConnectionAvatar> ActiveConnections = new List <PathConnectionAvatar>();
				public SplineAnimatorClosestPoint ActivePathFollower;
				public Path PassivePath = null;
				public FollowPath FollowPathSkill;

				public override void WakeUp()
				{
						Get = this;
				}

				public override void OnModsLoadFinish()
				{
						mLoadedPathsByName.Clear();
						mLoadedPaths.Clear();

						//load all paths
						Mods.Get.Runtime.LoadAvailableMods(mLoadedPaths, "Path");
						for (int i = 0; i < mLoadedPaths.Count; i++) {
								mLoadedPathsByName.Add(mLoadedPaths[i].Name, mLoadedPaths[i]);
								mNonRelevantPaths.Add(mLoadedPaths[i]);
						}
						LinkSharedPathMarkers();

						STransform trn = STransform.zero;
						WorldItem pathMarkerPrefab = null;
						if (WorldItems.Get.PackPrefab("WorldPathMarkers", "Path Marker 1", out pathMarkerPrefab)) {
								//create all of our world plants
								for (int i = 0; i < Globals.MaxSpawnedPlants; i++) {
										WorldItem worlditem = null;
										WorldItems.CloneWorldItem(pathMarkerPrefab, WIGroups.Get.Paths, out worlditem);
										worlditem.Initialize();
										worlditem.ActiveState = WIActiveState.Active;
										worlditem.ActiveStateLocked = true;
										PathMarker pathMarker = worlditem.Get <PathMarker>();
										pathMarker.SetProps(null);
										ActivePathMarkers.Add(pathMarker);
								}
						}

						mActivePath = GameObject.Instantiate(Get.WorldPathPrefab) as PathAvatar;
						mActivePath.spline.enabled = false;
						mActivePath.transform.parent = transform;
						ClearActivePath();

						mModsLoaded = true;
				}

				public override void OnGameStart()
				{	//create our path markers and start updating them
						mActivePathNodes = new List <SplineNode>();
						Vector3 splineNodePosition = new Vector3(0f, -1000f, 0f);
						for (int i = 0; i < Globals.MaxSplineNodesPerPath; i++) {
								GameObject newSplineNode = GameObject.Instantiate(SplineNodePrefab.gameObject, splineNodePosition, Quaternion.identity) as GameObject;
								newSplineNode.transform.parent = ActivePath.transform;
								newSplineNode.name = "Node " + i.ToString();
								splineNodePosition.Set(0f, splineNodePosition.y + 1f, 0f);
								mActivePathNodes.Add(newSplineNode.GetComponent <SplineNode>());
						}
						//store this because we use it frequently
						Skill followPathSkill = null;
						Skills.Get.SkillByName("FollowPath", out followPathSkill);
						FollowPathSkill = followPathSkill as FollowPath;
						StartCoroutine(UpdateWorldPathMarkers());
						StartCoroutine(UpdatePassivePath());
						mInitialized = true;
				}

				public IEnumerator UpdatePassivePath()
				{
//			while (mInitialized) {
//				while (!GameManager.Is(FGameState.InGame)) {
//					yield return new WaitForSeconds(1f);
//				}
//
//				if (HasPassivePath) {
//					//check to see if we're still intersecting with it
//					bool isInRange = false;
//					float currentDistance = 0f;
//					for (int i = 0; i < PassivePath.Templates.Count; i++) {
//						currentDistance = Vector3.Distance(Player.Local.Position, PassivePath.Templates[i].Position);
//						if (currentDistance < Globals.PathStrayDistanceInMeters) {
//							//we've found a path marker that's close to the player
//							//so this is still our passive path
//							PlayerDistanceFromPassivePath = currentDistance;
//							isInRange = true;
//							break;
//						}
//					}
//					if (!isInRange) {
//						Debug.Log("Stopped following " + PassivePath.Name);
//						//set our passive path to null
//						//on the next loop through we'll look for a new one
//						PassivePath = null;
//					}
//				} else {
//					PlayerDistanceFromPassivePath = 1f;
//					//if we don't have a passive path, but we do have an active path, wait
//					if (HasActivePath) {
//						yield return new WaitForSeconds(1f);
//					} else {
//						//if we don't have an active path, try to find a passive path
//						for (int i = 0; i < mLoadedPaths.Count; i++) {
//							if (PassivePath != null) {
//								//we set it on the last loop
//								break;
//							}
//							//does the player intersect with an active path?
//							Path loadedPath = mLoadedPaths[i];
//							mPassivePathBounds.center = loadedPath.PathBounds.center;
//							mPassivePathBounds.size = loadedPath.PathBounds.size;
//							if (mPassivePathBounds.Contains(Player.Local.Position)) {
//								//odds are we're close enough
//								bool isInRange = false;
//								float currentDistance = 0f;
//								for (int t = 0; t < PassivePath.Templates.Count; t++) {
//									currentDistance = Vector3.Distance(Player.Local.Position, PassivePath.Templates[t].Position);
//									if (currentDistance < Globals.PathStrayDistanceInMeters) {
//										//we've found a new passive path
//										FollowPathSkill.Use(true);
//										PassivePath = loadedPath;
//										PlayerDistanceFromPassivePath = currentDistance;
//										isInRange = true;
//										break;
//									}
//								}
//							}
//							yield return null;
//						}
//					}
//				}
//				yield return new WaitForSeconds(1f);
//			}
//
						yield break;
				}

				protected Bounds mPassivePathBounds;

				public static void GeneratePath(string pathName, WorldChunk chunk)
				{
						if (ActivePath.spline.splineNodesArray.Count > 0) {
								Debug.Log("Attempting to generate an uncleared path");
								return;
						}
						ActivePath.name = pathName;
						ActivePath.PathChunk = chunk;
						ActivePath.transform.position = Vector3.zero;// chunk.ChunkOffset;
						Path path = null;
						if (!Get.mLoadedPathsByName.TryGetValue(pathName, out path)) {
								Debug.LogError("Path " + pathName + " doesn't exist!");
								return;
						}
						//tell the new path we're now active
						path.SetActive(true);
						//build the path avatar
						for (int i = 0; i < path.Templates.Count; i++) {
								SplineNode node = Get.mActivePathNodes[i];
								ActivePath.spline.splineNodesArray.Add(node);
								PathMarkerInstanceTemplate pm = path.Templates[i];
								node.transform.position = pm.Position;
								node.transform.rotation = Quaternion.Euler(pm.Rotation);
						}
						ActivePath.Refresh();
				}

				public static void ClearActivePath()
				{
						//tell the path we're no longer active
						Path existingActivePath = null;
						if (!string.IsNullOrEmpty(ActivePath.name) && Get.mLoadedPathsByName.TryGetValue(ActivePath.name, out existingActivePath)) {
								existingActivePath.SetActive(false);
						}
						ActivePath.spline.splineNodesArray.Clear();
						ActivePath.spline.enabled = false;
						ActivePath.name = string.Empty;
				}

				public bool PathNearPlayer(out Path path, int tieBreaker)
				{
						path = null;
						if (mRelevantPaths.Count > 0) {
								path = mRelevantPaths[Mathf.Abs(tieBreaker) % mRelevantPaths.Count];
						}
						return path != null;
				}

				protected void ReplaceAllTemplateInstances(PathMarkerInstanceTemplate pm1, PathMarkerInstanceTemplate pm2)
				{
						for (int i = 0; i < mLoadedPaths.Count; i++) {
								for (int j = 0; j < mLoadedPaths[i].Templates.Count; j++) {
										if (mLoadedPaths[i].Templates[j] == pm1) {
												mLoadedPaths[i].Templates[j] = pm2;
										}
								}
						}
				}

				protected void LinkSharedPathMarkers()
				{
						//we link up any shared path markers so they update each other
						//they're saved in an unlinked format so this is a necessary step
						for (int i = 0; i < mLoadedPaths.Count; i++) {
								Path path = mLoadedPaths[i];
								path.InitializeTemplates();
								//------OLD FANCYPANTS METHOD-----//
								/*
				//every path has some shared path markers
				//we know them when the instance template's path name isn't the owner path's name
				for (int j = 0; j < path.Templates.Count; j++) {
					PathMarkerInstanceTemplate pm = path.Templates [j];
					if (pm.PathName != path.Name) {
						Debug.Log ("Found path marker from path " + pm.PathName + " in path " + path.Name + " at index " + j.ToString ());
						//it's not our template
						//get it from the path that owns it
						Path otherPath = null;
						if (mLoadedPathsByName.TryGetValue (pm.PathName, out otherPath)) {
							//now here's the tricky part - the template we have won't have an accurate index
							//so we use the branches from the other path to determine where to put the template
							//we're looking for a branch to this path
							int otherTemplateIndex = -1;
							PathMarkerInstanceTemplate otherPm = null;
							for (int k = 0; k < otherPath.Templates.Count; k++) {
								PathMarkerInstanceTemplate checkPm = otherPath.Templates [k];
								//only look at templates that it owns
								if (checkPm.PathName == otherPath.Name) {
									if (checkPm.Branches.TryGetValue (path.Name, out otherTemplateIndex)) {
										//check to see if this index is the index we're looking for
										if (otherTemplateIndex == j) {
											otherPm = checkPm;
											break;
										} else {
											Debug.Log ("Found other template with link to path but template index was " + otherTemplateIndex.ToString () + " and not " + j.ToString ());
										}
									} else {
										Debug.Log ("Checking " + otherPath.Name + " template " + k.ToString () + " but it has no branches");
									}
								}
							}
							if (otherPm != null) {
								path.Templates [j] = otherPm;
							} else {
								Debug.Log ("Tried to find template from " + otherPath.Name + " but failed");
							}
						}
					}
				}*/
						}
						//-----NEW BRUTE FORCE METHOD-----//
						//we're going to link up every path marker by proximity
						//first-come, first-serve basis
						Path path1 = null;
						Path path2 = null;
						Path replaceCheckPath = null;
						PathMarkerInstanceTemplate pm1;
						PathMarkerInstanceTemplate pm2;
						Bounds path1Bounds;
						Bounds path2Bounds;

						float mergeDistance = 0.15f;
						for (int i = 0; i < mLoadedPaths.Count; i++) {
								path1 = mLoadedPaths[i];
								for (int j = 0; j < mLoadedPaths.Count; j++) {
										if (i != j) {
												path2 = mLoadedPaths[j];
												path1Bounds = path1.PathBounds;
												path2Bounds = path2.PathBounds;
												//extend the bounds a tad to leave room for error
												path1Bounds.size = path1Bounds.size * 1.05f;
												path2Bounds.size = path2Bounds.size * 1.05f;
												if (path1Bounds.Intersects(path2Bounds)) {
														//they occupy some of the same space, so check each path marker against every other
														for (int x = 0; x < path1.Templates.Count; x++) {
																for (int y = 0; y < path2.Templates.Count; y++) {
																		pm1 = path1.Templates[x];
																		pm2 = path2.Templates[y];
																		if (pm1 != pm2 && Vector3.Distance(pm1.Position, pm2.Position) < mergeDistance) {
																				//if they're not already the same (for some reason... just in case)
																				//BRUUUUUTE FOOOOOOORCE!
																				ReplaceAllTemplateInstances(pm1, pm2);
																		}
																}
														}
												}
										}
								}
						}
						//now that all the templates are shared
						//refresh the path templates
						for (int i = 0; i < mLoadedPaths.Count; i++) {
								mLoadedPaths[i].RefreshBranches();
						}
						//i can't believe actually works better
						//computers are fast, man
				}

				public static bool IsNeighbor(PathMarker pathMarker1, PathMarker pathMarker2)
				{
						//has no props or no path? easy, not neighbors
						if ((!pathMarker1.HasPathMarkerProps || !pathMarker1.Props.HasParentPath) || (!pathMarker2.HasPathMarkerProps || !pathMarker2.Props.HasParentPath)) {
								return false;
						}
						PathMarkerInstanceTemplate pathMarker1Template = pathMarker1.Props;
						PathMarkerInstanceTemplate pathMarker2Template = pathMarker2.Props;
						//are they the same path marker? not neighbors
						if (pathMarker1Template == pathMarker2Template) {
								return false;
						}
						//on the same path? check if the difference in indexes is 1
						if (pathMarker1Template.ParentPath == pathMarker2Template.ParentPath) {
								//if it is then they're next to each other
								return Math.Abs(pathMarker1Template.IndexInParentPath - pathMarker2Template.IndexInParentPath) <= 1;
						}
						//if they're on different paths, we need to use branches to figure this out
						//check to see if one path is attached to the other with branches
						int pathMarker1IndexInPathMarker2Path = -1;
						int pathMarker2IndexInPathMarker1Path = -1;
						if (pathMarker1Template.Branches.TryGetValue(pathMarker2Template.PathName, out pathMarker1IndexInPathMarker2Path)) {
								//this means path marker 1 is used in path marker 2's path
								//if the difference between their indexes is 1 they're neighbors
								return Math.Abs(pathMarker1IndexInPathMarker2Path - pathMarker2Template.IndexInParentPath) <= 1;
						} else if (pathMarker2Template.Branches.TryGetValue(pathMarker1Template.PathName, out pathMarker2IndexInPathMarker1Path)) {
								return Math.Abs(pathMarker2IndexInPathMarker1Path - pathMarker1Template.IndexInParentPath) <= 1;
						}
						//nothing else to check
						return false;
				}

				public static bool CanAttachPathMarker(PathMarker pathMarker, PathMarker pathOriginMarker, out AlterAction alterAction)
				{
						alterAction = AlterAction.None;
						/*
			//					CHECK STUFF:
			//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//pathmarker attributes:		| PM has path 	| PMO has path	| Paths = same	| PM = terminal	| POM = terminal| PM = Origin	| POM = Origin	| Flwg PM path	| Flwg POM path	|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		AppendToPath		| FALSE		| TRUE		|		|		| TRUE		|		| FALSE		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		AppendToPath		| FALSE		| TRUE		|		|		| TRUE		| FALSE		| TRUE		| 		| TRUE		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		CreatePath		| FALSE		| FALSE		|		|		|		| FALSE		| TRUE		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		CreatePath		| FALSE		| TRUE		|		|		|		| FALSE		| TRUE		| 		| FALSE		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		CreatePathAndBranch	| FALSE		| TRUE		|		|		| FALSE		| FALSE		| TRUE		|		| FALSE		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		CreatePathAndBranch	| TRUE		| FALSE		|		| FALSE		|		| FALSE		| TRUE		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		CreateBranch		| TRUE		| TRUE		| FALSE		| 		| 		| 		| 		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		None			|		|		|		|		|		| TRUE		|		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		None			|		|		|		|		|		|		| FALSE		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			//		None			| TRUE		| TRUE		| TRUE		| TRUE		|		|		|		|		|		|
			//					-------------------------------------------------------------------------------------------------------------------------------------------------
			*/

						#region checkPathProps
						//basic requirements first
						if (!pathOriginMarker.HasPathMarkerProps) {
								//Debug.Log ("Path origin marker has no props, cannot connect markers");
								return false;
						}
						PathMarkerInstanceTemplate pmOriginTemplate = pathOriginMarker.Props;

						bool pmHasPath = false;
						bool pmOriginHasPath = false;
						bool pathsAreSame = false;
						bool pmIsTerminal = false;
						bool pmOriginIsTerminal = false;
						bool pmOriginIsOriginType = false;
						bool flwgPmOriginPath = false;

						if (!pathMarker.HasPathMarkerProps) {
								//create props for the path marker so we can work with it
								pathMarker.CreatePathMarkerProps();
						}
						PathMarkerInstanceTemplate pmTemplate = pathMarker.Props;

						//gather all our data
						if (Flags.Check((uint)PathMarkerType.PathOrigin, (uint)pmOriginTemplate.Type, Flags.CheckType.MatchAny)) {
								pmOriginIsOriginType = true;
						}
						if (pmTemplate.HasParentPath) {
								pmHasPath = true;
								pmIsTerminal = (pmTemplate.IndexInParentPath == 0 || pmTemplate.IndexInParentPath == pmTemplate.ParentPath.Templates.LastIndex());
						}
						if (pmOriginTemplate.HasParentPath) {
								pmOriginHasPath = true;
								pmOriginIsTerminal = (pmOriginTemplate.IndexInParentPath == 0 || pmOriginTemplate.IndexInParentPath == pmOriginTemplate.ParentPath.Templates.LastIndex());
								flwgPmOriginPath = ActivePath.name == pmOriginTemplate.PathName;
						}
						if (pmHasPath && pmOriginHasPath) {
								pathsAreSame = (pmTemplate.ParentPath == pmOriginTemplate.ParentPath);
						}
						#endregion

						//now we have all the info we need to figure out our action
						if (
								pmHasPath == false &&
								pmOriginHasPath == true &&
								pmOriginIsTerminal == true &&
								pmOriginIsOriginType == false) {
								alterAction = AlterAction.AppendToPath;
						} else if (
								pmHasPath == false &&
								pmOriginHasPath == true &&
								pmOriginIsTerminal == true &&
								pmOriginIsOriginType == true &&
								flwgPmOriginPath == true) {
								alterAction = AlterAction.AppendToPath;
						} else if (
								pmHasPath == false &&
								pmOriginHasPath == false &&
								pmOriginIsOriginType == true) {
								alterAction = AlterAction.CreatePath;
						} else if (
								pmHasPath == false &&
								pmOriginHasPath == true &&
								pmOriginIsOriginType == true &&
								flwgPmOriginPath == false) {
								alterAction = AlterAction.CreatePath;
						} else if (
								pmHasPath == false &&
								pmOriginHasPath == true &&
								pmOriginIsTerminal == false &&
								pmOriginIsOriginType == true &&
								flwgPmOriginPath == false) {
								alterAction = AlterAction.CreatePathAndBranch;
						} else if (
								pmHasPath == true &&
								pmOriginHasPath == false &&
								pmIsTerminal == false &&
								pmOriginIsOriginType == true) {
								alterAction = AlterAction.CreatePathAndBranch;
						} else if (
								pmHasPath == true &&
								pmOriginHasPath == true &&
								pathsAreSame == false) {
								alterAction = AlterAction.CreateBranch;
						}

						//Debug.Log ("path can be connected with action " + alterAction.ToString ());
						return alterAction != AlterAction.None;
				}

				public static void AttachPathMarker(PathMarker pathMarker, PathMarker pathOriginMarker, bool attachToEnd)
				{
						//assumptions
						// - pathMarker has props
						// - pathOriginMarker has props
						// - pathOriginMarker can accept links (eg is a Cross, Location or Campsite)
						// - both have parent paths
						// - parent paths are different
						//------
						//we attach a path marker to another path marker by creating a branch
						//a branch is used when two separate paths want to use the same marker
						//to create a branch we add the pathOriginMarker to the pathMarker's path
						//then let pathOriginMarker know that pathMarker's path is using it and at what index
						//the first thing is to figure out whether we're linking a path marker at the end or the start
						PathMarkerInstanceTemplate pathMarkerTemplate = pathMarker.Props;
						PathMarkerInstanceTemplate pathOriginMarkerTemplate = pathOriginMarker.Props;
						Path pathMarkerPath = pathMarker.Props.ParentPath;
						if (attachToEnd) {
								//if we do this no reconstruction is necessary
								//because the indexes of all previous templates is the same
								pathMarkerPath.Templates.Add(pathOriginMarkerTemplate);
						} else {
								//if we do this then a bit of reconstruction is necessary
								//the order of templates in the array has changed
								//that means any branches using these path markers will have changed
								//so update all the branches
								pathMarkerPath.Templates.Insert(0, pathOriginMarkerTemplate);
								//then make sure all the branches are using the new indexes
								pathMarkerPath.RefreshBranches();
						}
				}

				public static void SetActivePath(string pathName, WorldChunk parentChunk)
				{
						if (string.IsNullOrEmpty(pathName) || (!string.IsNullOrEmpty(ActivePath.name) && pathName == ActivePath.name)) {
								return;
						}

						if (IsEvaluating) {
								Get.Evaluator.StopEvaluating();
						}

						ClearActivePath();
						GeneratePath(pathName, parentChunk);
						EvaluateDifficulty(ActivePath);
						Get.ActivePathFollower.spline = ActivePath.spline;
						Get.ActivePathFollower.target = Player.Local.Body.transform;
						Get.ActivePathFollower.spline.enabled = true;
						Get.FollowPathSkill.FollowPathPassively();

						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.PathStartFollow), WorldClock.Time);
				}

				public static SplineNode GeneratePathMarker(PathMarkerInstanceTemplate state, WIGroup group)
				{
//			WorldItem pathMarkerWorldItem = WorldItems.CloneWorldItem ("WorldPathMarkers", "Path Marker 1", state.WorldTransform, group);
//			PathMarker pathMarker = pathMarkerWorldItem.GetComponent <PathMarker> ();
//			pathMarker.Props = state;
//			pathMarkerWorldItem.Initialize ();
						SplineNode splineNode = null;//pathMarker.gameObject.GetOrAdd <SplineNode> ();
						return splineNode;
				}

				public static void EvaluateDifficulty(PathAvatar path)
				{
						return;//TEMP
						GUIManager.PostInfo("Evaluating path difficulty...");

						foreach (PathSegment segment in path.Segments) {
								CheckSegmentDifficulty(segment);
						}
				}

				public static void CheckSegmentDifficulty(PathSegment segment)
				{
						return; //TEMP
						Get.Evaluator.EvaluateSegment(segment);
				}

				public static void GetAllNeighbors(PathMarkerInstanceTemplate start, PathMarkerType desiredTypes, Dictionary <PathMarkerInstanceTemplate,int> neighbors)
				{
						//if there's no path there are no neighbors
						if (!start.HasParentPath) {
								Debug.Log("Path marker had no parent path, returning");
								return;
						}
						//Path startPath = start.ParentPath;
						//int startIndex = start.IndexInParentPath;
						//first check the parent path for neighbors
						//GetNeighborsOnPath (startPath, startIndex, desiredTypes, neighbors);
						//now check for neighbors in branches
						foreach (KeyValuePair <string,int> branch in start.Branches) {
								Path branchPath = null;
								int branchStartIndex = branch.Value;
								if (Get.mLoadedPathsByName.TryGetValue(branch.Key, out branchPath)) {
										GetNeighborsOnPath(branchPath, branchStartIndex, desiredTypes, neighbors);
								}
						}
				}

				public static void GetNeighborsOnPath(Path path, int startIndex, PathMarkerType desiredTypes, Dictionary <PathMarkerInstanceTemplate,int> neighbors)
				{
						//Debug.Log ("Getting neighbors on path " + path.Name + " starting at index " + startIndex.ToString ());
						bool searchForward = startIndex < path.Templates.LastIndex();
						int forwardIndex = startIndex + 1;
						while (searchForward) {
								PathMarkerInstanceTemplate forwardPm = path.Templates[forwardIndex];
								if (Flags.Check((uint)desiredTypes, (uint)forwardPm.Type, Flags.CheckType.MatchAny)) {
										//if it's not filtered out, add it and quit searching
										if (neighbors.ContainsKey(forwardPm)) {
												neighbors[forwardPm] = forwardIndex;
										} else {
												neighbors.Add(forwardPm, forwardIndex);
										}
										searchForward = false;
										//Debug.Log ("Found path marker forward");
								} else {
										forwardIndex++;
										searchForward = forwardIndex < path.Templates.Count;
								}
						}
						bool searchBackwards = startIndex > 0;
						int backwardsIndex = startIndex - 1;
						while (searchBackwards) {
								PathMarkerInstanceTemplate backPm = path.Templates[backwardsIndex];
								if (Flags.Check((uint)desiredTypes, (uint)backPm.Type, Flags.CheckType.MatchAny)) {
										//if it's not filtered out, add it and quit searching
										if (neighbors.ContainsKey(backPm)) {
												neighbors[backPm] = backwardsIndex;
										} else {
												neighbors.Add(backPm, backwardsIndex);
										}
										searchBackwards = false;
										//Debug.Log ("Found path marker backwards");
								} else {
										backwardsIndex--;
										searchBackwards = backwardsIndex >= 0;
								}
						}
				}

				public static bool GetNeighborInDirection(float metersStart, PathDirection direction, out PathMarkerInstanceTemplate neighbor)
				{
						neighbor = null;
						return false;
				}

				public static bool GetPathInDirection(Location start, Vector3 direction, out Path path)
				{
						path = null;
						bool result = false;
//			Vector3 point = Vector3.MoveTowards (start.transform.position, direction, 10.0f);
//			float smallestDistanceSoFar = Mathf.Infinity;
//			foreach (string attachedPath in start.AttachedPaths) {
//				Vector3 actualPoint = attachedPath.PathPointFromPosition (point);
//				float currentDistance	= Vector3.Distance (actualPoint, point);
//				if (currentDistance < smallestDistanceSoFar) {
//					smallestDistanceSoFar = currentDistance;
//					path = attachedPath;
//					result = true;
//				}
//			}
						return result;
				}

				public static bool GetNextPathToDestination(Location start, Location end, ref Path currentPath, ref float currentMeters, ref PathDirection currentDirection)
				{
						bool result = false;

//			foreach (string path in start.AttachedPaths) {
//				if (path != currentPath) {
//					//check to see if we're lucky and the path contains our next location
//					if (path.ContainsLocation (end.name)) {
//						currentPath = path;
//						currentMeters = path.MetersFromPosition (end.InGamePosition);
//						currentDirection	= path.DirectionToPosition (currentMeters, end.InGamePosition);
//						result = true;
//						break;
//					}
//				}
//			}
						return result;
				}

				public static bool ReachedEnd(Spline path, Vector3 point, PathDirection direction, float tolerance)
				{
						bool result = false;
						float splineParam = path.GetClosestPointParam(point, 1, 0f, 1f, 0.05f);

						if (direction == PathDirection.Forward || direction == PathDirection.None) {
								if (1.0f - splineParam <= tolerance) {
										result = true;
								}
						} else {
								if (splineParam <= tolerance) {
										result = true;
								}
						}
						return result;
				}

				public static float PathDifficultyToMetersPerHour(PathDifficulty difficulty)
				{
						float metersPerHour = Globals.PlayerAverageMetersPerHour;
						switch (difficulty) {
								case PathDifficulty.Easy:
										metersPerHour = Globals.PathEasyMetersPerHour;
										break;

								case PathDifficulty.Moderate:
										metersPerHour = Globals.PathModerateMetersPerHour;
										break;

								case PathDifficulty.Difficult:
										metersPerHour = Globals.PathDifficultMetersPerHour;
										break;

								case PathDifficulty.Deadly:
										metersPerHour = Globals.PathDeadlyMetersPerHour;
										break;

								case PathDifficulty.Impassable:
										metersPerHour = Globals.PathImpassibleMetersPerHour;
										break;

								default:
										break;
						}
						return metersPerHour;
				}

				public static PathMarkerInstanceTemplate GetMarkerClosestTo(Path path, Vector3 position)
				{
						PathMarkerInstanceTemplate marker = path.Templates[0];
						float distanceToMarker = 0f;
						float closestDistanceSoFar = Mathf.Infinity;
						for (int i = 0; i < path.Templates.Count; i++) {
								distanceToMarker = Vector3.Distance(position, path.Templates[i].Position);
								if (distanceToMarker < closestDistanceSoFar) {
										marker = path.Templates[i];
										closestDistanceSoFar = distanceToMarker;
								}
						}
						return marker;
				}

				public PathMarkerInstanceTemplate FirstMarkerWithinRange(float min, float max, Path onPath, Vector3 position)
				{
						PathMarkerInstanceTemplate marker = onPath.Templates[0];
						float distance = 0f;
						for (int i = 0; i < onPath.Templates.Count; i++) {	
								distance = Vector3.Distance(position, onPath.Templates[i].Position);
								if (distance < max && distance > min) {
										marker = onPath.Templates[i];
										break; 
								}
						}
						return marker;
				}

				public static bool GetNextMarkerInDirection(Path path, PathDirection direction, PathMarkerInstanceTemplate currentMarker, out PathMarkerInstanceTemplate nextMarker)
				{
						nextMarker = null;
						int index = path.Templates.IndexOf (currentMarker);

						if (index < 0) {
								//it's not in the current path dummy
								return false;
						}

						switch (direction) {
								case PathDirection.Forward:
										index++;
										if (index < path.Templates.Count) {
												nextMarker = path.Templates [index];
										}
										break;

								case PathDirection.Backwards:
								default:
										index--;
										if (index > 0) {
												nextMarker = path.Templates.PrevItem(index);
										}
										break;
						}
						return nextMarker != null;
				}

				public static bool AddPathMarkerToActivePath(PathMarker pathMarker)
				{
						if (!HasActivePath || pathMarker.NumAttachedPaths >= Globals.MaxPathMarkerAttachedPaths) {
								////Debug.Log ("Either no active path or path marker has too many attached paths");
								return false;
						}

						string activePathName = ActivePath.name;
						WorldChunk chunk = ActivePath.PathChunk;
						int pathMarkerIndex = 0;
						if (pathMarker.HasPathMarkerProps && pathMarker.Props.Branches.TryGetValue(activePathName, out pathMarkerIndex)) {
								////Debug.Log ("Path marker is already a part of path " + activePathName);
								return false;
						}
						//find the best place to add the new path marker
						float meters = ActivePath.MetersFromPosition(pathMarker.Position);
						float metersForward = 0f;
						float metersBackward = 0f;
						PathMarkerInstanceTemplate forwardNeighbor = null;
						PathMarkerInstanceTemplate backwardNeighbor = null;

						return false;
				}

				public static bool RemovePathMarkerFromActivePath(PathMarker pathMarker)
				{
						if (!HasActivePath || !pathMarker.HasPathMarkerProps) {
								////Debug.Log ("Either no active path or path marker has no props");
								return false;
						}
						////Debug.Log ("Removing path marker from active path...");
//			string activePathName = ActivePath.name;
//			WorldChunk chunk = pathMarker.Props.ParentChunk;
//			int pathMarkerIndex = 0;
//			if (!pathMarker.Props.AttachedPaths.TryGetValue (activePathName, out pathMarkerIndex)) {
//				//if the attached path isn't in the lookup then it's not in the active path
//				////Debug.Log ("path marker doesn't belong to active path " + activePathName + ", apparently");
//				return false;
//			}
//			//remove the attached path from the path marker
//			pathMarker.Props.AttachedPaths.Remove (ActivePath.name);
//			//then make sure it's not associated with our instance assigner
//			PathMarkerInstanceMappings.Remove (pathMarker);
//			//now remove it from the chunk path data...
//			SDictionary <int, PathMarkerInstanceTemplate> pathLookup = null;
//			if (chunk.PathData.PathMarkersByPathName.TryGetValue (activePathName, out pathLookup))
//			{
//				List <PathMarkerInstanceTemplate> newPathList = new List<PathMarkerInstanceTemplate> (pathLookup.Count);
//				PathLookupToList (pathLookup, newPathList);
//				//remove the marker index
//				//convert it back into a dictionary
//				//then return it to the chunk data
//				newPathList.RemoveAt (pathMarkerIndex);
//				PathListToLookup (newPathList, pathLookup);
//				chunk.PathData.PathMarkersByPathName [activePathName] = pathLookup;
//			}
//			//now, does this thing still have paths attached? if not we need to remove it from the pool
//			if (pathMarker.NumAttachedPaths == 0 && pathMarker.Props.Type != PathMarkerType.Location) {
//				//it's a generic path marker which means we should add it to inventory
//				//do this by setting it to hidden (because we want to continue using it) and adding a stack item instead
//				StackItem removedPathMarker = pathMarker.worlditem.GetStackItem (WIMode.Stacked);
//				WIStackError error = WIStackError.None;
//				Player.Local.Inventory.AddItems (removedPathMarker, WIGroups.Get.Paths, ref error);
//				pathMarker.worlditem.SetMode (WIMode.Hidden);
//			}
//			//next, call refresh on the chunk
//			//this will clean out the old templates and rebuild the quad tree
//			chunk.RefreshPathMarkerInstanceTemplates ();
//			//finally, refresh active path
//			ClearActivePath ();
//			SetActivePath (activePathName, chunk);
						return true;
				}

				public static void PathLookupToList(SDictionary <int,PathMarkerInstanceTemplate> pathLookup, List <PathMarkerInstanceTemplate> pathList)
				{
						pathList.Clear();
						for (int i = 0; i < pathLookup.Count; i++) {
								pathList.Add(pathLookup[i]);//add the template in this spot to the new lookup
						}
				}

				public static void PathListToLookup(List <PathMarkerInstanceTemplate> pathList, SDictionary <int, PathMarkerInstanceTemplate> pathLookup)
				{
						pathLookup.Clear();
						for (int i = 0; i < pathList.Count; i++) {
								pathLookup.Add(i, pathList[i]);
						}
				}

				public static float MetersToParam(float meters, float splineLength)
				{
						return (Mathf.Clamp(meters, 0f, Mathf.Infinity) / Globals.InGameUnitsToMeters) / splineLength;
				}

				public static bool MoveAlongPath(ref float currentMeters, float metersToMove, PathDirection direction, float metersStart, float metersEnd)
				{
						bool isInRange = true;
						if (direction == PathDirection.Forward) {
								currentMeters += metersToMove;
								if (currentMeters > metersEnd) {
										//////Debug.Log (currentMeters.ToString () + " was greater than meters end " + metersEnd.ToString ());
										isInRange = false;
								}
						} else {
								currentMeters -= metersToMove;
								if (currentMeters < metersStart) {
										//////Debug.Log (currentMeters.ToString () + " was below meters start " + metersStart.ToString ());
										isInRange = false;
								}
						}
						return isInRange;
				}

				public static float MoveAlongPath(float currentMeters, float metersToMove, PathDirection direction)
				{	//this actually did something useful a while back
						//it might again later, so i'm keeping it
						if (direction == PathDirection.Forward) {
								return currentMeters + metersToMove;
						}
						return currentMeters - metersToMove;
				}

				public static PathDirection ReverseDirection(PathDirection direction)
				{
						switch (direction) {
								case PathDirection.Forward:
										return PathDirection.Backwards;

								case PathDirection.Backwards:
								default:
										return PathDirection.Forward;
						}
				}

				public static void CreateTemporaryConnection(PathMarker pathMarker, PathMarker pathOrigin, Paths.AlterAction alterAction, PathSkill skillToUse)
				{
						bool foundExisting = false;
						for (int i = Get.ActiveConnections.LastIndex(); i >= 0; i--) {
								PathConnectionAvatar ac = Get.ActiveConnections[i];
								if (ac != null && !ac.IsFinished) {
										if (ac.ConnectionPathMarker == pathMarker && ac.ConnectionPathOrigin == pathOrigin && alterAction == alterAction && ac.SkillToUse == skillToUse) {
												foundExisting = true;
												break;
										}
								}
						}

						if (!foundExisting) {
								GameObject connectionObject = Get.gameObject.CreateChild("Connection").gameObject;
								PathConnectionAvatar pca = connectionObject.AddComponent <PathConnectionAvatar>();
								pca.SetConnection(pathMarker, pathOrigin, alterAction, skillToUse);
								Get.ActiveConnections.Add(pca);
								pca.IsActive = true;
						}
				}

				protected IEnumerator UpdateActiveConnections()
				{
						while (GameManager.State != FGameState.Quitting) {
								while (!GameManager.Is(FGameState.InGame)) {
										yield return null;
								}

								PathConnectionAvatar topSoFar = null;

								if (ActiveConnections.Count > 0) {
										for (int i = ActiveConnections.LastIndex(); i >= 0; i--) {
												PathConnectionAvatar ac = ActiveConnections[i];
												if (ac == null || ac.IsFinished) {
														ActiveConnections.RemoveAt(i);
												} else {
														ac.IsActive = false;
														//figure out which ac is going to be active
														//based on what the player is looking at etc
														if (topSoFar == null) {
																topSoFar = ac;
														} else {
																if (ac.OriginDistanceToPlayer < topSoFar.OriginDistanceToPlayer
																    || ac.FocusOffset < topSoFar.FocusOffset) {
																		topSoFar = ac;
																}
														}
												}
										}
								}

								if (topSoFar != null) {
										topSoFar.IsActive = true;
								}

								yield return null;
						}
				}

				protected IEnumerator UpdateWorldPathMarkers()
				{
						if (GameManager.Get.TestingEnvironment) {
								yield break;
						}

						while (GameManager.State != FGameState.Quitting) {
								while (!GameManager.Is(FGameState.InGame)) {
										yield return null;
								}

								yield return new WaitForSeconds(0.1f);
								//check to see if our paths are still relevant
								Bounds chunkBounds = GameWorld.Get.PrimaryChunk.ChunkBounds;

								mIrrelevantPathMarkers.Clear();
								mNonRelevantPaths.Clear();
								mRelevantPaths.Clear();
								//check non-relevant paths to see if they're now relevant
								for (int i = 0; i < mLoadedPaths.Count; i++) {
										mPathBounds = mLoadedPaths[i].PathBounds;
										if (chunkBounds.Intersects(mPathBounds)) {
												mRelevantPaths.Add(mLoadedPaths[i]);
										} else {
												mNonRelevantPaths.Add(mLoadedPaths[i]);
										}
								}
								yield return null;
								//finally, check all the path markers in the relevant paths for the nearest path markers
								PathMarkerInstanceTemplate irrelevantTemplate = null;
								PathMarker irrelevantPathMarker = null;
								//get all the path markers that need to be reassigned
								for (int i = 0; i < ActivePathMarkers.Count; i++) {
										irrelevantPathMarker = ActivePathMarkers[i];
										if (!Player.Local.ColliderBounds.Contains(irrelevantPathMarker.worlditem.tr.position)) {
												mIrrelevantPathMarkers.Enqueue(irrelevantPathMarker);
												irrelevantPathMarker.SetProps(null);
												irrelevantPathMarker.worlditem.tr.Translate(5000f, -5000f, 0f);
										} else if (irrelevantPathMarker.Props.HasParentPath && irrelevantPathMarker.Props.ParentPath.Name != ActivePath.name) {
												//while we're here, check if the relevant path marker is within stray distance
												//if it is, then we're following this path passively
												float distance = Vector3.Distance(Player.Local.Position, irrelevantPathMarker.worlditem.Position);
												if (distance < Globals.PathStrayDistanceInMeters) {
														PlayerDistanceFromPassivePath = distance;
														SetActivePath(irrelevantPathMarker.Props.ParentPath.Name, GameWorld.Get.PrimaryChunk);
												}
										}
								}
								//then reassign them to the nearest path markers
								List <PathMarkerInstanceTemplate> relevantTemplates = FindTemplatesInNeedOfInstances(Player.Local.ColliderBounds);
								for (int i = 0; i < relevantTemplates.Count; i++) {
										if (mIrrelevantPathMarkers.Count > 0) {
												irrelevantPathMarker = mIrrelevantPathMarkers.Dequeue();
												irrelevantPathMarker.SetProps(relevantTemplates[i]);
												break;
										}
								}
								yield return null;
						}
						yield break;
				}

				protected Bounds mPathBounds;
				protected Queue <PathMarker> mIrrelevantPathMarkers = new Queue<PathMarker>();

				protected List <PathMarkerInstanceTemplate> FindTemplatesInNeedOfInstances(Bounds bounds)
				{
						List <PathMarkerInstanceTemplate> relevantTemplates = new List <PathMarkerInstanceTemplate>();
						for (int i = 0; i < mRelevantPaths.Count; i++) {
								Path path = mRelevantPaths[i];
								for (int j = 0; j < path.Templates.Count; j++) {
										if (!path.Templates[j].HasInstance && bounds.Contains(path.Templates[j].Position)) {
												relevantTemplates.Add(path.Templates[j]);
										}
								}
						}
						return relevantTemplates;
				}

				public static string CleanPathName(string pathName)
				{
						return WorldItems.CleanWorldItemName(pathName);
				}

				protected IEnumerator UpdatePathWeed()
				{
						//TODO re-implement this
						//it adds plants to the path that you have to clear away
						while (GameWorld.Get.WorldLoaded) {
								yield return new WaitForSeconds(1f);
						}
						yield break;
				}
				#if UNITY_EDITOR
				public void OnDrawGizmos()
				{
						for (int i = 0; i < mRelevantPaths.Count; i++) {
								PathEditor.DrawPathGizmo(mRelevantPaths[i], true, Vector3.zero, Colors.Saturate(Colors.ColorFromString(mRelevantPaths[i].Name, 100)));
						}

						for (int i = 0; i < mNonRelevantPaths.Count; i++) {
								PathEditor.DrawPathGizmo(mNonRelevantPaths[i], false, Vector3.zero, Colors.Saturate(Colors.ColorFromString(mNonRelevantPaths[i].Name, 100)));
						}
				}
				#endif
				public enum AlterAction
				{
						None,
						AppendToPath,
						CreatePath,
						CreatePathAndBranch,
						CreateBranch,
				}

				protected PathAvatar mActivePath;
				protected List <Path> mRelevantPaths = new List <Path>();
				protected List <Path> mNonRelevantPaths = new List <Path>();
				protected List <Path> mLoadedPaths = new List <Path>();
				protected Dictionary <string, Path> mLoadedPathsByName = new Dictionary<string, Path>();
				protected List <SplineNode> mActivePathNodes = new List <SplineNode>();
				protected GameObject mNameEntryDialog = null;
		}
}