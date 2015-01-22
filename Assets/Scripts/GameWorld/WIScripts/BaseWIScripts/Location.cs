using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.Story;
using ExtensionMethods;

namespace Frontiers.World.BaseWIScripts
{
		public class Location : WIScript, IComparable <Location>
		{		//used by pretty much every 'place' in the game including path markers
				//and used by world map in StackItem form
				//very very old class with a lot of gunk hanging around
				public bool UnloadOnInvisible = true;
				public Action OnLocationGroupLoaded;

				public override bool CanBeCarried {
						get {
								return false;
						}
				}

				public override bool CanEnterInventory {
						get {
								return false;
						}
				}

				public override bool CanBeDropped {
						get {
								return false;
						}
				}

				public bool IsCivilized {
						get {
								//will tie this to paths, eventually
								return State.IsCivilized;
						}
				}

				public override bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{		//TODO move this to path marker
						if (point.y < Biomes.Get.TideMaxElevation) {
								errorMessage = "The tide will wash it away";
								return false;
						}

						if (targetObject.IOIType != ItemOfInterestType.Scenery) {
								errorMessage = "Can only place on terrain";
								return false;
						}
						return true;
				}

				public override string DisplayNamer(int increment)
				{
						return State.Name.CommonName;
				}

				public override bool AutoIncrementFileName {
						get {
								return false;
						}
				}

				public override string GenerateUniqueFileName(int increment)
				{
						return State.Name.FileName;
				}

				public WIGroup LocationGroup {
						get {
								return mLocationGroup;
						}
				}

				public Vector3 WorldMapPosition {
						get {
								return transform.position;
						}
				}

				public Vector3 PlayerSpawnPosition {
						get {
								return transform.position;
						}
				}

				public Vector3 InGamePosition {
						get {
								return transform.position;
						}
				}

				public Vector3 RandomPilgrimPosition {
						get {
								return transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
						}
				}

				public SplineNode Node {
						get {
								if (mNode == null) {
										mNode = gameObject.GetComponent <SplineNode>();
								}
								return mNode;
						}
				}

				public LocationState State = new LocationState();

				public override void OnStartup()
				{
						CreateLocationGroup(false);
				}

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnInvisible += OnInvisible;
						worlditem.OnActive += OnActive;

						Visitable visitable = null;
						if (worlditem.Is <Visitable>(out visitable)) {
								visitable.OnPlayerVisit += OnPlayerVisit;
								visitable.OnPlayerLeave += OnPlayerLeave;
						}

						Revealable revealable = null;
						if (worlditem.Is <Revealable>(out revealable)) {
								revealable.OnReveal += OnReveal;
						}
				}

				public void OnInvisible()
				{
						if (State.UnloadOnInvisible) {
								mLocationGroup.Unload();
						}
				}

				public void OnGroupLoadStateChange()
				{
						if (mLocationGroup.Is(WIGroupLoadState.Loaded)) {
								OnLocationGroupLoaded.SafeInvoke();
						}
				}

				public void OnVisible()
				{
						CreateLocationGroup(true);
				}

				public void OnActive()
				{
						CreateLocationGroup(true);
				}

				public void Refresh()
				{
						//something something
				}

				public void OnReveal()
				{
						Refresh();
						//worlditem.SaveState ();
						//RefreshDebugSphere ();
				}

				public void OnPlayerVisit()
				{
						Refresh();
						//don't refresh visiting locations
						//they'll be refreshed automatically
						//worlditem.SaveState ();
						CreateLocationGroup(true);
				}

				public void OnPlayerLeave()
				{
						Refresh();
						//worlditem.SaveState ();
						//RefreshDebugSphere ();
				}

				protected void CreateLocationGroup(bool load)
				{
						if (mLocationGroup == null) {
								mLocationGroup = WIGroups.GetOrAdd(gameObject, State.Name.FileName, worlditem.Group, worlditem);
								mLocationGroup.OnLoadStateChange += OnGroupLoadStateChange;
						}
						if (load) {
								mLocationGroup.Load();
						}
				}

				#region helper functions

				#endregion

				#region IComparable

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						Location other = obj as Location;
						if (this == other) {
								return true;
						}

						return (this.State.Name.FileName == other.State.Name.FileName);
				}

				public bool Equals(Location p)
				{
						if (p == null) {
								return false;
						}

						return (this.State.Name.FileName == p.State.Name.FileName);
				}

				public int CompareTo(Location other)
				{
						return worlditem.ActiveRadius.CompareTo(other.worlditem.ActiveRadius);
				}

				public override int GetHashCode()
				{
						return State.Name.GetHashCode();
				}

				#endregion

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						mWorldItem = gameObject.GetComponent <WorldItem>();
						if (string.IsNullOrEmpty(State.Name.FileName) || State.Name.FileName != name) {
								State.Name.FileName = name;
						}
						worlditem.Props.Name.FileName = State.Name.FileName;

						//if our parent group is the chunk then we should unload on invisible
						if (worlditem.Group != null && (worlditem.Group.name == "AG" || worlditem.Group.name == "BG" || worlditem.Group.name == "TR")) {
								State.UnloadOnInvisible = true;
						} else {
								State.UnloadOnInvisible = false;
						}

						Revealable revealable = null;
						if (gameObject.HasComponent <Revealable>(out revealable) && !revealable.State.CustomMapSettings) {
								switch (State.Type) {
										case "CapitalCity":
												revealable.State.IconStyle = MapIconStyle.AlwaysVisible;
												revealable.State.LabelStyle = MapLabelStyle.AlwaysVisible;
												revealable.State.IconName = "MapIconCapitalCity";
												break;

										case "City":
												revealable.State.IconStyle = MapIconStyle.Large;
												revealable.State.LabelStyle = MapLabelStyle.MouseOver;
												revealable.State.IconName = "MapIconCity";
												break;

										case "Shingle":
												//let the various structure scripts take care of this
												break;

										case "District":
										default:
												revealable.State.IconStyle = MapIconStyle.None;
												revealable.State.LabelStyle = MapLabelStyle.Descriptive;
												break;

										case "Campsite":
												revealable.State.IconStyle = MapIconStyle.Large;
												revealable.State.LabelStyle = MapLabelStyle.MouseOver;
												revealable.State.IconName = "MapIconCampsite";
												break;

										case "Cemetary":
												revealable.State.IconStyle = MapIconStyle.Large;
												revealable.State.LabelStyle = MapLabelStyle.MouseOver;
												revealable.State.IconName = "MapIconCemetary";
												break;

										case "WayStone":
												revealable.State.IconStyle = MapIconStyle.Medium;
												revealable.State.LabelStyle = MapLabelStyle.None;
												revealable.State.IconName = "MapIconWayStone";
												break;

										case "Gateway":
												revealable.State.IconStyle = MapIconStyle.Large;
												revealable.State.LabelStyle = MapLabelStyle.None;
												revealable.State.IconName = "MapIconGateway";
												break;

										case "CrossRoads":
												revealable.State.IconStyle = MapIconStyle.Large;
												revealable.State.LabelStyle = MapLabelStyle.None;
												revealable.State.IconName = "MapIconCrossRoads";
												break;
								}
						}

						if (State.Name.CommonName == "Shingle") {
								State.Name.CommonName = Frontiers.Data.GameData.AddSpacesToSentence(State.Name.FileName);
						}

						//make sure our visible radius encompasses our child location visible radius
						Bounds activeRadiusBounds = new Bounds(transform.position, Vector3.one);
						Bounds visibleDistanceBounds = new Bounds(transform.position, Vector3.one);
						foreach (Transform child in transform) {
								WorldItem childWorlditem = null;
								if (child.gameObject.HasComponent <WorldItem>(out childWorlditem)) {
										Bounds childActiveRadiusBounds = new Bounds(childWorlditem.transform.position, Vector3.one * childWorlditem.Props.Local.ActiveRadius);
										Bounds childVisibleDistanceBounds = new Bounds(childWorlditem.transform.position, Vector3.one * (childWorlditem.Props.Local.VisibleDistance + childWorlditem.Props.Local.ActiveRadius));
										activeRadiusBounds.Encapsulate(childActiveRadiusBounds);
										visibleDistanceBounds.Encapsulate(visibleDistanceBounds);
								}
						}
						float maxActiveRadius = Mathf.Max(Mathf.Max(activeRadiusBounds.size.x, activeRadiusBounds.size.y), activeRadiusBounds.size.z);
						float maxVisibleDistance = Mathf.Max(Mathf.Max(visibleDistanceBounds.size.x, visibleDistanceBounds.size.y), visibleDistanceBounds.size.z);

						worlditem.Props.Local.ActiveRadius = Mathf.Max(worlditem.Props.Local.ActiveRadius, maxActiveRadius);
						worlditem.Props.Local.VisibleDistance = Mathf.Max(worlditem.Props.Local.VisibleDistance, maxVisibleDistance - worlditem.Props.Local.ActiveRadius);
				}

				public override void OnEditorLoad()
				{
						mWorldItem = gameObject.GetComponent <WorldItem>();
						SphereCollider sc = gameObject.GetComponent <SphereCollider>();
						if (sc != null) {
								sc.radius = worlditem.Props.Local.ActiveRadius;
						}
				}
				#endif
				public void OnDrawGizmos()
				{
						if (worlditem == null)
								return;

						Color color = Colors.Saturate(Colors.ColorFromString(State.Type, 100));
						color.a = 0.125f;
						Gizmos.color = color;
						Color locColor = Color.white;

						switch (State.Type) {
								case "City":
										Gizmos.DrawSphere(transform.position, worlditem.ActiveRadius);

										locColor = Color.Lerp(Color.red, Color.white, 0.5f);
										locColor.a = 0.25f;
										Gizmos.color = locColor;
										Gizmos.DrawWireSphere(transform.position, worlditem.ActiveRadius + worlditem.VisibleDistance);
										break;

								case "Woods":
										Gizmos.DrawSphere(transform.position, worlditem.ActiveRadius);

										locColor = Color.Lerp(Color.green, Color.white, 0.5f);
										locColor.a = 0.25f;
										Gizmos.color = locColor;
										Gizmos.DrawWireSphere(transform.position, worlditem.ActiveRadius + worlditem.VisibleDistance);
										break;

								case "Campsite":

										Gizmos.DrawSphere(transform.position, worlditem.ActiveRadius);

										locColor = Color.Lerp(Color.blue, Color.magenta, 0.5f);
										locColor.a = 0.25f;
										Gizmos.color = locColor;
										Gizmos.DrawWireSphere(transform.position, worlditem.ActiveRadius + worlditem.VisibleDistance);
										break;

								case "District":
										Gizmos.DrawSphere(transform.position, worlditem.ActiveRadius);

										locColor = Color.Lerp(Color.blue, Color.white, 0.5f);
										locColor.a = 0.25f;
										Gizmos.color = locColor;
										Gizmos.DrawWireSphere(transform.position, worlditem.ActiveRadius + worlditem.VisibleDistance);
										break;

								case "CrossStreet":
										Gizmos.color = Color.yellow;
										Gizmos.DrawWireSphere(transform.position, 7);
										break;

								case "PathMarker":
										Gizmos.color = Color.red;
										Gizmos.DrawWireSphere(transform.position, 2);
										break;

								case "Den":
								case "PlantPatch":
								default:
										Gizmos.color = Color.magenta;
										Gizmos.DrawWireSphere(transform.position, worlditem.ActiveRadius);
										break;
						}
				}

				protected bool mHasCreatedGraphNode = false;
				protected WIGroup mLocationGroup = null;
				protected HashSet <Path> mAttachedPaths = new HashSet <Path>();
				protected SplineNode mNode;
		}

		[Serializable]
		public class LocationName
		{
				public string FileName = string.Empty;
				public string CommonName = string.Empty;
				public string NickName = string.Empty;
				public string ProperName = string.Empty;
				public string WarlockName = string.Empty;
				public string ObexName = string.Empty;
				public string GivenName = string.Empty;
		}

		[Serializable]
		public class LocationState : IComparable <LocationState>
		{
				public LocationName Name = new LocationName();
				public string Type = "None";
				public LocationTerrainType TerrainType = LocationTerrainType.AboveGround;
				public OceanMode Ocean = OceanMode.Default;
				public STransform Transform = new STransform();
				public int PathFlags = 0;
				public int SurvivalFlags = 0;
				public bool LoadsChildren = false;
				public int OverlapPriority = 0;
				public bool IsCivilized = true;
				public bool IsDangerous = false;
				public bool PlacedOnSolidTerrainMesh = false;
				public bool UnloadOnInvisible = false;
				public float RenderDistanceOverride = -1f;
				public float MistIntensityOverride = 0f;
				public float WindIntensityOverride = 0f;

				#region IComparable

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						LocationState other = obj as LocationState;
						if (this == other) {
								return true;
						}

						return (this.Name.FileName == other.Name.FileName);
				}

				public bool Equals(LocationState p)
				{
						if (p == null) {
								return false;
						}

						return (this.Name.FileName == p.Name.FileName);
				}

				public int CompareTo(LocationState other)
				{
						return OverlapPriority.CompareTo(other.OverlapPriority);
				}

				public override int GetHashCode()
				{
						return Name.GetHashCode();
				}

				#endregion

		}
}