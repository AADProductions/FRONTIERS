using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.BaseWIScripts
{
		public class PathMarker : WIScript, IHasPosition
		{
				public int NumAttachedPaths {
						get {
								if (HasPathMarkerProps) {
										if (mProps.Branches.Count > 0) {
												//add 1 for the path we're already owned by
												return mProps.Branches.Count + 1;
										} else if (!string.IsNullOrEmpty(mProps.PathName)) {
												return 1;
										}
								}
								return 0;
						}
				}

				public bool IsActive {
						get {
								return HasPathMarkerProps && mProps.IsActive;
						}
				}

				public bool IsTerminal {
						get {
								return HasPathMarkerProps && mProps.IsTerminal;
						}
				}

				public IEnumerable <string> AttachedPathNames {
						get {
								if (!HasPathMarkerProps) {
										return gEmptyPathList;
								}
								gPathList.Clear();
								if (!string.IsNullOrEmpty(mProps.PathName)) {
										gPathList.Add(mProps.PathName);
								}
								foreach (KeyValuePair <string,int> branch in mProps.Branches) {
										gPathList.Add(branch.Key);
								}
								return gPathList;
						}
				}

				public Vector3 Position { get { return worlditem.tr.position; } }

				public override void OnInitialized()
				{
						worlditem.OnPlayerPlace += OnPlayerPlace;

						Visitable visitable = null;
						if (worlditem.Is <Visitable>(out visitable)) {
								visitable.OnItemOfInterestVisit += OnItemOfInterestVisit;
								visitable.OnItemOfInterestVisit += OnItemOfInterestLeave;
								//visitable.ItemsOfInterest.Add ("Character");
								visitable.ItemsOfInterest.SafeAdd("PathMarker");
						}
						if (!worlditem.Is<Receptacle>()) {
								worlditem.OnPlayerUse += OnPlayerUse;
						}
				}

				public bool HasPathMarkerProps {
						get {
								if (mProps != null && mProps != PathMarkerInstanceTemplate.Empty) {
										if (mProps.Owner != this) {
												mProps = null;
												return false;
										}
										return true;
								}
								return false;
						}
				}

				public void CreatePathMarkerProps()
				{
						if (HasPathMarkerProps) {
								return;
						}

						mProps = new PathMarkerInstanceTemplate();
						switch (worlditem.State) {
								case "PathMarker":
								default:
										mProps.Type = PathMarkerType.PathMarker;
										break;

								case "CrossMarker":
										mProps.Type = PathMarkerType.CrossMarker;
										break;
						}
						mProps.Position = worlditem.tr.position;
						mProps.Rotation = worlditem.tr.rotation.eulerAngles;
						mProps.Owner = this;

						Visitable visitable = worlditem.Get <Visitable>();
						mProps.Visitable = visitable.State;

						Revealable revealable = worlditem.Get <Revealable>();
						mProps.Revealable = revealable.State;
				}

				public override bool CanEnterInventory {
						get {
								if (HasPathMarkerProps && mProps.HasParentPath) {
										return false;
								}
								return true;
						}
				}

				public override bool CanBeDropped {
						get {
								return false;
						}
				}

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				protected GameWorld.TerrainHeightSearch mHit = new GameWorld.TerrainHeightSearch();

				public void OnPlayerUse()
				{
						Skill skill = null;
						if (Skills.Get.SkillByName("FastTravel", out skill)) {
								FastTravel(skill);
						}
				}

				public void RefreshPathMarkerProps(bool setPosition)
				{
						if (!mInitialized) {
								return;
						}

						if (HasPathMarkerProps) {
								if (setPosition) {
										worlditem.ActiveState = WIActiveState.Active;
										mHit.feetPosition = mProps.Position;
										mHit.overhangHeight = 4.0f;
										mHit.groundedHeight = 5.0f;
										mHit.ignoreWorldItems = true;//we want path markers to appear below people / signs / etc
										mHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mHit);
										worlditem.tr.position = mHit.feetPosition;
										worlditem.tr.rotation = Quaternion.Euler(0f, mProps.Rotation.y, 0f);
										//update the props position based on the new in-game height
										Props.Position = mHit.feetPosition;
								}

								Visitable visitable = worlditem.Get <Visitable>();
								visitable.State = Props.Visitable;

								Revealable revealable = worlditem.Get <Revealable>();
								revealable.State = Props.Revealable;

								//we only care about other items if we're a path origin
								//OR if we're the terminal of an active path
								if (Flags.Check((uint)PathMarkerType.PathOrigin, (uint)mProps.Type, Flags.CheckType.MatchAny) || (IsActive && IsTerminal)) {
										visitable.Trigger.enabled = true;
										SphereCollider sc = visitable.Trigger.VisitableCollider as SphereCollider;
										sc.radius = Globals.PathOriginTriggerRadius;
								} else {
										visitable.Trigger.enabled = false;
								}

								switch (Props.Type) {
										case PathMarkerType.PathMarker:
										default:
												worlditem.State = "PathMarker";
												break;

										case PathMarkerType.CrossMarker:
												worlditem.State = "CrossMarker";
												break;

										case PathMarkerType.Location:
												//locations don't change state
												break;
								}
						}
				}

				public bool FastTravel(Skill skill)
				{
						return TravelManager.Get.FastTravel(this);
				}

				public bool Move()
				{
						if (Paths.IsEvaluating) {
								GUI.GUIManager.PostWarning("Can't edit a path while evaluating");
								return false;
						}
						Player.Local.ItemPlacement.ItemForceCarry(worlditem);
						return true;
				}

				public bool RemoveFromPath()
				{
						if (Paths.IsEvaluating) {
								GUI.GUIManager.PostWarning("Can't edit a path while evaluating");
								return false;
						}
						return Paths.RemovePathMarkerFromActivePath(this);
				}

				public void OnPlayerPlace()
				{

				}

				public void OnItemOfInterestVisit()
				{
						if (!mInitialized) {
								return;
						}

						if (!HasPathMarkerProps) {
								return;
						}

						Visitable visitable = worlditem.Get <Visitable>();
						PathMarker pm = null;
						if (visitable.LastItemOfInterestToVisit != null && visitable.LastItemOfInterestToVisit.Is <PathMarker>(out pm)) {
								CheckPathConnections(pm);
						}
				}

				public void OnItemOfInterestLeave()
				{
						if (!mInitialized)
								return;

						if (!HasPathMarkerProps) {
								return;
						}

						Visitable visitable = worlditem.Get <Visitable>();
						PathMarker pm = null;
						if (visitable.LastItemOfInterestToLeave != null && visitable.LastItemOfInterestToLeave.Is <PathMarker>(out pm)) {
								//not sure what to do here
						}
				}

				protected void CheckPathConnections(PathMarker nearbyPathMarker)
				{		//TODO possibly remove this as Paths now handles this task
						//check the nearby path markers
						//if one of them is not a neigbor
						//then we know we're supposed to try and link it
						if (nearbyPathMarker == null || nearbyPathMarker.IsFinished) {
								return;
						} else if (!Paths.IsNeighbor(nearbyPathMarker, this)) {
								MarkerAlterAction alterAction = MarkerAlterAction.None;
								if (Paths.CanAttachPathMarker(nearbyPathMarker, this, out alterAction)) {
										//this calls for a skill
										Skill learnedSkill = null;
										PathSkill skillToUse = null;
										switch (alterAction) {
												case MarkerAlterAction.AppendToPath:
												case MarkerAlterAction.CreateBranch:
														if (Skills.Get.HasLearnedSkill("AlterPath", out learnedSkill)) {
																skillToUse = learnedSkill as PathSkill;
														}
														break;

												case MarkerAlterAction.CreatePath:
												case MarkerAlterAction.CreatePathAndBranch:
														if (Skills.Get.HasLearnedSkill("CreatePath", out learnedSkill)) {
																skillToUse = learnedSkill as PathSkill;
														}
														break;

												case MarkerAlterAction.None:
												default:
														break;
										}

										if (skillToUse != null) {
												Paths.CreateTemporaryConnection(nearbyPathMarker, this, alterAction, skillToUse);
										}
								}
						}
				}

				public bool SetProps(PathMarkerInstanceTemplate newProps)
				{
						if (newProps != null && (newProps == mProps)) {
								return true;
						}

						if (HasPathMarkerProps) {
								//tell the props we have now that it no longer has an instance
								mProps.HasInstance = false;
						}

						mProps = newProps;
						if (mProps == null) {
								//Debug.Log ("props were null in set props");
								name = "(Empty)";
						} else {
								mProps.Owner = this;
								name = mProps.PathName + "-" + mProps.IndexInParentPath.ToString();
						}
						RefreshPathMarkerProps(true);
						return true;
				}

				public PathMarkerInstanceTemplate Props {
						get {
								return mProps;
						}
				}

				protected PathMarkerInstanceTemplate mProps = null;
				protected static HashSet <string> gEmptyPathList = new HashSet<string>();
				protected static HashSet <string> gPathList = new HashSet<string>();
		}

		[Serializable]
		public class PathMarkerTemplate
		{
				public bool UseExistingMarker;
				public ulong ExistingMarkerID;
				public STransform Transform;
				public RevealableState Revealable;
				public VisitableState Visitable;
				public LocationState Props;
		}
}