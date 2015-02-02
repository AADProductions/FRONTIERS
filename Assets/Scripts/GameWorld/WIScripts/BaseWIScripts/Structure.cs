using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

using System.Xml.Serialization;

namespace Frontiers.World.BaseWIScripts
{
		public class Structure : WIScript
		{
				public StructureState State = new StructureState();

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

				public override bool ReadyToUnload {
						get {
								//it's being used as a template at the moment
								if (InUseAsTemplate > 0)
										return false;

								//we have to wait for these processes to be over before we can unload
								if (worlditem.Is(WIActiveState.Invisible)) {
										return true;
								}
								return !Is(StructureLoadState.ExteriorLoading | StructureLoadState.InteriorLoading);
						}
				}

				public override void BeginUnload()
				{
						//make sure characters don't fall through the ground
						for (int i = 0; i < SpawnedCharacters.Count; i++) {
								if (SpawnedCharacters[i] != null) {
										SpawnedCharacters[i].worlditem.Get<Motile>().enabled = false;
								}
						}
						if (StructureOwner != null) {
								StructureOwner.worlditem.Get<Motile>().enabled = false;
						}
						//structures will figure out if this is a good idea or not
						Structures.AddInteriorToUnload(this);
						Structures.AddExteriorToUnload(this);
				}

				public override bool FinishedUnloading {
						get {
								if (Is(StructureLoadState.ExteriorUnloaded) && MinorStructuresUnloaded) {
										return true;
								}
								return false;
						}
				}

				public bool ForceBuildInterior {
						get {
								return State.ForceBuildInterior || StructureShingle.PropertyIsDestroyed;
						}
				}
				public int InUseAsTemplate = 0;
				//structure-specific stuff
				public Character StructureOwner = null;
				public DeedOfOwnership Deed = new DeedOfOwnership();
				//hub actions
				public Action OnPreparingToBuild;
				public Action OnPlayerEnter;
				public Action OnPlayerExit;
				public Action OnExteriorLoaded;
				public Action OnInteriorLoaded;
				public Action OnStructureDestroyed;
				public Action OnStructureRestored;
				public Action OnOwnerCharacterSpawned;
				public Action OnCharacterSpawned;
				public Action OnEntranceClose;
				//characters and nodes
				public List <Character> SpawnedCharacters = new List <Character>();
				//entrances and shingles
				public Shingle StructureShingle;
				public List <Dynamic> OuterEntrances = new List <Dynamic>();
				public List <Dynamic> InnerEntrances = new List <Dynamic>();
				public List <Dynamic> OuterMachines = new List <Dynamic>();
				public List <Dynamic> InnerMachines = new List <Dynamic>();
				public Dictionary <string, Trigger> Triggers = new Dictionary <string, Trigger>();
				public List <MeshFilter> ExteriorMeshes = new List <MeshFilter>();
				public List <MeshFilter> InteriorMeshes = new List <MeshFilter>();
				public List <StructureTerrainLayer> ExteriorLayers = new List <StructureTerrainLayer>();
				//public List <Collider> ExteriorColliders = new List <Collider> ( );
				public List <BoxCollider> ExteriorBoxColliders = new List <BoxCollider>();
				public List <MeshCollider> ExteriorMeshColliders = new List <MeshCollider>();
				public List <StructureTerrainLayer> InteriorLayers = new List <StructureTerrainLayer>();
				//public List <Collider> InteriorColliders = new List <Collider> ();
				public List <BoxCollider> InteriorBoxColliders = new List <BoxCollider>();
				public List <MeshCollider> InteriorMeshColliders = new List <MeshCollider>();
				public List <BoxCollider> ExteriorBoxCollidersDestroyed = new List <BoxCollider>();
				public List <BoxCollider> InteriorBoxCollidersDestroyed = new List <BoxCollider>();
				public List <MeshCollider> ExteriorMeshCollidersDestroyed = new List <MeshCollider>();
				public List <MeshCollider> InteriorMeshCollidersDestroyed = new List <MeshCollider>();
				public bool DisplayExterior = true;
				public bool DisplayInterior = true;
				public List <Renderer> ExteriorRenderers = new List<Renderer>();
				public List <Renderer> ExteriorRenderersDestroyed = new List<Renderer>();
				public List <Renderer> ExteriorLodRenderers = new List<Renderer>();
				public List <Renderer> ExteriorLodRenderersDestroyed = new List<Renderer>();
				public List <Renderer> InteriorRenderers = new List<Renderer>();
				public List <Renderer> InteriorRenderersDestroyed = new List<Renderer>();
				public List <ChildPiece> DestroyedFires = null;
				public FXPiece[] DestroyedFX = null;

				public bool OwnerCharacterSpawned { 
						get {
								return StructureOwner != null;
						}
				}
				//loading and groups
				public StructureLoadState LoadState {
						get {
								return State.LoadState;
						}
						set {
								State.LoadState = value;
						}
				}

				public StructureLoadPriority LoadPriority = StructureLoadPriority.Adjascent;
				public GameObject StructureBase;
				public Transform StructureBaseMinor;
				public WIGroup StructureGroup;
				public Dictionary <int,WIGroup> StructureGroupInteriors = new Dictionary<int, WIGroup>();

				public string GroupNameExterior {
						get {
								return worlditem.FileName + "-EXT";
						}
				}

				public string GroupNameInterior(int interiorVariant)
				{
						return worlditem.FileName + "-INT-" + interiorVariant.ToString();
				}

				public bool Is(StructureLoadState loadState)
				{
						return Flags.Check((uint)State.LoadState, (uint)loadState, Flags.CheckType.MatchAny);
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						if (string.IsNullOrEmpty(State.TemplateName)) {
								//Debug.Log ("Template name was empty in structure setting it to default");
								State.TemplateName = "AgricultureCabin";
						}
						if (StructureBase != null && (string.IsNullOrEmpty(State.TemplateName) || State.TemplateName != StructureBuilder.GetTemplateName(StructureBase.name))) {
								State.TemplateName = StructureBuilder.GetTemplateName(StructureBase.name);
						}
						if (StructureBase != null) {
								State.PrimaryBuilderOffset = new STransform(StructureBase.transform);
						}

						if (string.IsNullOrEmpty(State.OwnerSpawn.TemplateName)) {
								State.OwnerSpawn.IsEmpty = true;
						}

						Revealable revealable = null;
						if (gameObject.HasComponent <Revealable>(out revealable)) {
								revealable.State.IconStyle = MapIconStyle.Small;
								revealable.State.LabelStyle = MapLabelStyle.MouseOver;
								if (string.IsNullOrEmpty(revealable.State.IconName) || revealable.State.IconName == "Outpost") {
										//inns and shops and residences may set this on their own
										//only set it if they don't
										revealable.State.IconName = "MapIconStructure";
								}
						}
						State.MinorStructures.Clear();
						StructureBuilder[] structureBuilders = transform.GetComponentsInChildren <StructureBuilder>();
						foreach (StructureBuilder builder in structureBuilders) {
								if (StructureBuilder.GetTemplateName(builder.name) != State.TemplateName) {
										if (builder.name.Contains("Destroyed")) {
												State.DestroyedMinorStructure.TemplateName = StructureBuilder.GetTemplateName(builder.name);
												State.DestroyedMinorStructure.Position = builder.transform.localPosition;
												State.DestroyedMinorStructure.Rotation = builder.transform.localRotation.eulerAngles;
										} else {
												MinorStructure ms = new MinorStructure();
												ms.TemplateName = StructureBuilder.GetTemplateName(builder.name);
												ms.Position = builder.transform.localPosition;
												ms.Rotation = builder.transform.localRotation.eulerAngles;
												State.MinorStructures.Add(ms);
										}
								}
						}

						UnityEditor.EditorUtility.SetDirty(this);
				}
				#endif
				public override void OnStartup()
				{
						State.DestroyedMinorStructure.LoadState = StructureLoadState.ExteriorUnloaded;
						State.LoadState = StructureLoadState.ExteriorUnloaded;
				}

				public override void OnInitialized()
				{
						if (string.IsNullOrEmpty(State.AmbientAudio.Key)) {
								State.AmbientAudio = GameWorld.Get.Settings.DefaultAmbientAudioInterior;
						}

						worlditem.OnVisible += OnVisible;
						worlditem.OnInactive += OnInactive;
						worlditem.OnInvisible += OnInvisible;
						worlditem.OnAddedToGroup += OnAddedToGroup;

						for (int i = 0; i < State.MinorStructures.Count; i++) {
								State.MinorStructures[i].LoadState = StructureLoadState.ExteriorUnloaded;
						}
						State.LoadState = StructureLoadState.ExteriorUnloaded;
						//get our shingle and set the parent structure
						StructureShingle = worlditem.Get <Shingle>();
						StructureShingle.SetParentStructure(this);
						//setting the parent structure will auto set up ownership
						//it will also set up stuff like whether we're destroyed
						//this redundancy sucks but it's necessary in this case
				}

				public override void OnModeChange()
				{
						if (worlditem.Mode == WIMode.Unloaded || worlditem.Mode == WIMode.RemovedFromGame) {
								State.LoadState = StructureLoadState.ExteriorUnloaded;
						}
				}

				public void OnAddedToGroup()
				{
						if (!State.HasSetFlags) {
								IStackOwner cityOwner = null;
								WIFlags ownerResidentFlags = null;
								WIFlags ownerStructureFlags = null;
								bool foundCityOwner = false;

								/*
								if (worlditem.Group.HasOwner (out cityOwner)) {
									//if we do, we'll want to combine our flags with the city flags
									City city = null;
									if (cityOwner.IsWorldItem && cityOwner.worlditem.Is <City> (out city)) {
										//Debug.Log ("Using owner as flag source");
										ownerResidentFlags = city.State.ResidentFlags;
										ownerStructureFlags = city.State.StructureFlags;
										foundCityOwner = true;
									}
								}
								//if (!foundCityOwner) {*/
								//using region as a flag source
								Region region = null;
								if (GameWorld.Get.RegionAtPosition(worlditem.Position, out region)) {
										ownerResidentFlags = region.ResidentFlags;
										ownerStructureFlags = region.StructureFlags;
								} else {
										return;
								}

								if (State.ExclusiveExtResidentFlags) {
										State.ExtResidentFlags.Intersection(ownerResidentFlags);
								} else {
										State.ExtResidentFlags.Union(ownerResidentFlags);
								}

								if (State.ExclusiveIntResidentFlags) {
										State.IntResidentFlags.Intersection(ownerResidentFlags);
								} else {
										State.IntResidentFlags.Union(ownerResidentFlags);
								}

								if (State.ExclusiveStructureFlags) {
										State.StructureFlags.Intersection(ownerStructureFlags);
								} else {
										State.StructureFlags.Union(ownerStructureFlags);
								}
						}
				}

				public void OnVisible()
				{
						if (worlditem.Is(WILoadState.Unloading | WILoadState.Unloaded)) {
								return;
						}

						if (!Is(StructureLoadState.ExteriorLoaded | StructureLoadState.InteriorLoaded)) {
								Structures.AddExteriorToLoad(this);
								if (ForceBuildInterior) {
										Structures.AddInteriorToLoad(this);
								}
						} else {
								SetDestroyed(StructureShingle.PropertyIsDestroyed);
						}
				}

				public void OnInactive()
				{
						RefreshColliders(false);
						if (Is(StructureLoadState.InteriorLoaded) && !ForceBuildInterior) {
								Structures.AddInteriorToUnload(this);
						}
				}

				public void OnInvisible()
				{
						RefreshColliders(false);
						RefreshRenderers(true);
				}

				public void RefreshRenderers(bool forceOn)
				{
						bool renderersEnabled = DisplayExterior && (forceOn || worlditem.Is(WIActiveState.Visible | WIActiveState.Active));
						//normal exterior renderers
						for (int i = 0; i < ExteriorRenderers.Count; i++) {
								if (ExteriorRenderers[i] != null) {
										ExteriorRenderers[i].enabled = renderersEnabled;
										ExteriorRenderers[i].castShadows = Structures.StructureShadows;
										ExteriorRenderers[i].receiveShadows = Structures.StructureShadows;
								}
						}
						//destroyed exterior renderers
						renderersEnabled &= !StructureShingle.PropertyIsDestroyed;
						for (int i = 0; i < ExteriorRenderersDestroyed.Count; i++) {
								ExteriorRenderersDestroyed[i].enabled = renderersEnabled;
								ExteriorRenderersDestroyed[i].castShadows = Structures.StructureShadows;
								ExteriorRenderersDestroyed[i].receiveShadows = Structures.StructureShadows;
						}
						for (int i = 0; i < ExteriorLodRenderersDestroyed.Count; i++) {
								ExteriorLodRenderersDestroyed[i].enabled = renderersEnabled;
								ExteriorLodRenderersDestroyed[i].castShadows = Structures.StructureShadows;
								ExteriorLodRenderersDestroyed[i].receiveShadows = Structures.StructureShadows;
						}

						renderersEnabled = DisplayInterior && (forceOn || IsDestroyed || worlditem.Is(WIActiveState.Visible | WIActiveState.Active));
						//normal interior renderers
						for (int i = 0; i < InteriorRenderers.Count; i++) {
								if (InteriorRenderers[i] != null) {
										InteriorRenderers[i].enabled = renderersEnabled;
										InteriorRenderers[i].castShadows = Structures.StructureShadows;
										InteriorRenderers[i].receiveShadows = Structures.StructureShadows;
								}
						}
						//destroyed interior renderers
						renderersEnabled &= !StructureShingle.PropertyIsDestroyed;
						for (int i = 0; i < InteriorRenderersDestroyed.Count; i++) {
								InteriorRenderersDestroyed[i].enabled = renderersEnabled;
								InteriorRenderersDestroyed[i].castShadows = Structures.StructureShadows;
								InteriorRenderersDestroyed[i].receiveShadows = Structures.StructureShadows;
						}

						//minor renderers
						for (int i = 0; i < State.MinorStructures.Count; i++) {
								State.MinorStructures[i].RefreshRenderers(true);
						}
				}

				public void RefreshColliders(bool forceOn)
				{
						bool enableColliders = DisplayExterior && (forceOn || worlditem.Is(WIActiveState.Active) || (Player.Local.Surroundings.IsVisitingStructure(this)));
						//normal exterior colliders
						for (int i = ExteriorBoxColliders.LastIndex(); i >= 0; i--) {
								if (ExteriorBoxColliders[i] != null) {
										ExteriorBoxColliders[i].rigidbody.detectCollisions = enableColliders;
								} else {
										ExteriorBoxColliders.RemoveAt(i);
								}
						}
						for (int i = ExteriorMeshColliders.LastIndex(); i >= 0; i--) {
								if (ExteriorMeshColliders[i] != null) {
										ExteriorMeshColliders[i].rigidbody.detectCollisions = enableColliders;
								} else {
										ExteriorMeshColliders.RemoveAt(i);
								}
						}
						//destroyed exterior colliders
						enableColliders &= !StructureShingle.PropertyIsDestroyed;
						for (int i = ExteriorBoxCollidersDestroyed.LastIndex(); i >= 0; i--) {
								if (ExteriorBoxCollidersDestroyed[i] == null) {
										ExteriorBoxCollidersDestroyed.RemoveAt(i);
								} else {
										ExteriorBoxCollidersDestroyed[i].rigidbody.detectCollisions = enableColliders;
								}
						}
						for (int i = ExteriorMeshCollidersDestroyed.LastIndex(); i >= 0; i--) {
								if (ExteriorMeshCollidersDestroyed[i] == null) {
										ExteriorMeshCollidersDestroyed.RemoveAt(i);
								} else {
										ExteriorMeshCollidersDestroyed[i].rigidbody.detectCollisions = enableColliders;
								}
						}

						//normal interior colliders
						enableColliders = DisplayInterior && (forceOn || IsDestroyed || Is(StructureLoadState.InteriorLoaded));
						for (int i = InteriorBoxColliders.LastIndex(); i >= 0; i--) {
								if (InteriorBoxColliders[i] != null) {
										InteriorBoxColliders[i].rigidbody.detectCollisions = enableColliders;
								} else {
										InteriorBoxColliders.RemoveAt(i);
								}
						}
						for (int i = InteriorMeshColliders.LastIndex(); i >= 0; i--) {
								if (InteriorMeshColliders[i] != null) {
										InteriorMeshColliders[i].rigidbody.detectCollisions = enableColliders;
								} else {
										InteriorMeshColliders.RemoveAt(i);
								}
						}
						//destroyed interior colliders
						enableColliders &= !StructureShingle.PropertyIsDestroyed;
						for (int i = InteriorBoxCollidersDestroyed.LastIndex(); i >= 0; i--) {
								if (InteriorBoxCollidersDestroyed[i] == null) {
										InteriorBoxCollidersDestroyed.RemoveAt(i);
								} else {
										InteriorBoxCollidersDestroyed[i].rigidbody.detectCollisions = enableColliders;
								}
						}
						for (int i = InteriorMeshCollidersDestroyed.LastIndex(); i >= 0; i--) {
								if (InteriorMeshCollidersDestroyed[i] == null) {
										InteriorMeshCollidersDestroyed.RemoveAt(i);
								} else {
										InteriorMeshCollidersDestroyed[i].rigidbody.detectCollisions = enableColliders;
								}
						}

				}

				public void RefreshShadowCasters()
				{
						for (int i = 0; i < ExteriorRenderers.Count; i++) {
								if (ExteriorRenderers[i] != null) {
										ExteriorRenderers[i].castShadows = Structures.StructureShadows;
										ExteriorRenderers[i].receiveShadows = Structures.StructureShadows;
								}
						}
						for (int i = 0; i < ExteriorRenderersDestroyed.Count; i++) {
								if (ExteriorRenderersDestroyed[i] != null) {
										ExteriorRenderersDestroyed[i].castShadows = Structures.StructureShadows;
										ExteriorRenderersDestroyed[i].receiveShadows = Structures.StructureShadows;
								}
						}
						for (int i = 0; i < InteriorRenderers.Count; i++) {
								if (InteriorRenderers[i] != null) {
										InteriorRenderers[i].castShadows = Structures.StructureShadows;
										InteriorRenderers[i].receiveShadows = Structures.StructureShadows;
								}
						}
						for (int i = 0; i < InteriorRenderersDestroyed.Count; i++) {
								if (InteriorRenderersDestroyed[i] != null) {
										InteriorRenderersDestroyed[i].castShadows = Structures.StructureShadows;
										InteriorRenderersDestroyed[i].receiveShadows = Structures.StructureShadows;
								}
						}
				}

				public void DestroyStructure()
				{
						SetDestroyed(true);
				}

				protected void SetDestroyed(bool destroyed)
				{
						if (destroyed) {
								//we want to destroy the structure
								//have we already been destroyed?
								if (StructureShingle.PropertyIsDestroyed) {
										//no need to announce anything
										//if we were destroyed within the last day
										//spawn smoldering smoke at each fire point
										//TODO spawn smoldering smoke
								} else {
										//otherwise spawn fires at each fire point
										//TODO spawn fires
										//then announce that we've been destroyed
										//create the fires
										if (DestroyedFires != null) {
												for (int i = 0; i < DestroyedFires.Count; i++) {
														ChildPiece fire = DestroyedFires[i];
														FXManager.Get.SpawnFire(fire.ChildName, StructureBase.transform, fire.Position, fire.Rotation, fire.Scale.x, false);
												}
										}
										if (DestroyedFX != null) {
												for (int i = 0; i < DestroyedFX.Length; i++) {
														FXManager.Get.SpawnFX(StructureBase.transform, DestroyedFX[i]);
												}
										}
										OnStructureDestroyed.SafeInvoke();
								}
								//if we have a destroyed minor structure build it here
								Structures.AddMinorToload(State.DestroyedMinorStructure, 100, worlditem);
								//it's not possible to enter a structure any more
								if (Player.Local.Surroundings.IsVisitingStructure(this)) {
										Player.Local.Surroundings.StructureExit(this);
								}
						} else {
								//we want to un-destroy the structure
								//turn on destroyed renderers
								if (!StructureShingle.PropertyIsDestroyed) {
										//no need to announce anything
										//if there are any fires get rid of them
								} else {
										//if we're destroyed
										//then we want to restore the structure
										OnStructureRestored.SafeInvoke();
								}
						}

						RefreshColliders(false);
						RefreshRenderers(false);
				}

				#region loading and unloading

				public bool MinorStructuresUnloaded {
						get {
								bool unloaded = true;
								for (int i = 0; i < State.MinorStructures.Count; i++) {
										if (State.MinorStructures[i].LoadState != StructureLoadState.ExteriorUnloaded) {
												unloaded = false;
												break;
										}
								}
								return unloaded;
						}
				}

				public bool GetDynamicTrigger(string triggerName, out Trigger trigger)
				{
						trigger = null;
						if (Triggers.TryGetValue(triggerName, out trigger)) {
								if (trigger == null) {
										//get rid of it
										//it's been unloaded
										Triggers.Remove(triggerName);
								}
						}
						return trigger != null;
				}

				public void AddDynamicTrigger(Trigger trigger)
				{
						if (!Triggers.ContainsKey(trigger.worlditem.FileName)) {
								Triggers.Add(trigger.worlditem.FileName, trigger);
						}
				}

				public void AddDynamicPrefab(Dynamic dynamic)
				{
						switch (dynamic.State.Type) {
								case WorldStructureObjectType.OuterEntrance:
										OuterEntrances.SafeAdd(dynamic);
										break;

								case WorldStructureObjectType.InnerEntrance:
										InnerEntrances.SafeAdd(dynamic);
										break;

								default:
										if (dynamic.worlditem.Group.Props.Interior) {
												InnerMachines.SafeAdd(dynamic);
										} else {
												OuterMachines.SafeAdd(dynamic);
										}
										break;
						}
				}

				public void OnLoadFinish(StructureLoadState finalState)
				{
						WorldChunk chunk = worlditem.Group.GetParentChunk();
						WIGroup group = StructureGroup;
						//Debug.Log("LOAD FINISH: Structure " + name + " load finish: " + finalState.ToString());
						switch (finalState) {
								case StructureLoadState.ExteriorLoaded:
								default:
										//we've finished loading
										//depending on our current state we may have been asked to unload
										//so take care of that now
										State.ExteriorLoadedOnce = true;
										//this will refresh colliders and renderers
										SetDestroyed(StructureShingle.PropertyIsDestroyed);
										SpawnExteriorCharacters();
										OnExteriorLoaded.SafeInvoke();
										break;

								case StructureLoadState.InteriorLoaded: 	
										//check each interior variant to see if it's been loaded once
										//if it hasn't been loaded once, add any action nodes that were just sent by the structure builder to the chunk
										//then use those action nodes to spawn characters
										List <ActionNodeState> actionNodes = null;
										List <int> interiorVariants = new List<int>();
										interiorVariants.Add(State.BaseInteriorVariant);
										interiorVariants.AddRange(State.AdditionalInteriorVariants);
										State.ExteriorLoadedOnce = true;//just in fucking case
										//this will refresh colliders and renderers
										SetDestroyed(StructureShingle.PropertyIsDestroyed);
										for (int i = 0; i < interiorVariants.Count; i++) {
												int interiorVariant = interiorVariants[i];
												State.InteriorsLoadedOnce.SafeAdd(interiorVariant);
												SpawnInteriorCharacters(interiorVariant);
										}
										//we've finished loading
										//reset this to eliminate any 'spawn point' loading priorities
										LoadPriority = StructureLoadPriority.Adjascent;
										OnInteriorLoaded.SafeInvoke();
										break;
						}
				}

				public void OnUnloadFinish(StructureLoadState finalState)
				{
						switch (finalState) {
								case StructureLoadState.ExteriorUnloaded:
										//TODO announce this
										if (worlditem.Is(WILoadState.Initialized) && worlditem.Is(WIActiveState.Active)) {
												//whoops looks like we jumped the gun
												//better load again now
												Structures.AddExteriorToLoad(this);
										}
										break;
								case StructureLoadState.ExteriorLoaded://AKA interior unloaded
										//TODO announce this
										//don't bother with auto-loading
										//the player will push that on us
										UnloadStructureGroups(StructureLoadState.ExteriorLoaded);
										//if we're visible / active and we're supposd to build our interior automatically
										//do that now
										if (worlditem.Is(WILoadState.Initialized) && worlditem.Is(WIActiveState.Active | WIActiveState.Visible)) {
												if (ForceBuildInterior) {
														Structures.AddInteriorToLoad(this);
												}
										}
										break;
						}
				}

				public void UnloadStructureGroups(StructureLoadState forState)
				{
						if (forState == StructureLoadState.ExteriorLoaded) {
								foreach (KeyValuePair <int,WIGroup> interiorGroup in StructureGroupInteriors) {
										if (interiorGroup.Value != null) {
												interiorGroup.Value.Unload();
										}
								}
						} else {
								if (StructureGroup != null) {
										//this will automatically unload all the interior groups
										StructureGroup.Unload();
								}
								OuterEntrances.Clear();
								Triggers.Clear();
						}
						StructureGroupInteriors.Clear();
						InnerEntrances.Clear();
				}

				public IEnumerator CreateStructureGroups(StructureLoadState forState)
				{
						if (StructureGroup == null) {
								Location location = worlditem.Get <Location>();
								while (location.LocationGroup == null || !location.LocationGroup.Is(WIGroupLoadState.Loaded)) {
										if (location.LocationGroup != null && location.LocationGroup.Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
												yield break;
										}
										//wait for structure group to load
										yield return null;
								}
								StructureBase = gameObject.FindOrCreateChild(GroupNameExterior).gameObject;
								State.PrimaryBuilderOffset.ApplyTo(StructureBase.transform);
								StructureBase.transform.parent = worlditem.Group.tr;
								StructureGroup = WIGroups.GetOrAdd(StructureBase.gameObject, GroupNameExterior, worlditem.Group, worlditem);//set to worlditem initially, will be usurped by owner
								StructureGroup.ParentStructure = this;
						}

						StructureGroup.Load();

						while (!StructureGroup.Is(WIGroupLoadState.Loaded)) {
								if (StructureGroup.Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading)) {
										yield break;
								}
								yield return null;
						}

						if (forState == StructureLoadState.InteriorLoaded) {
								WIGroup interiorBaseGroup = WIGroups.GetOrAdd(GroupNameInterior(State.BaseInteriorVariant), StructureGroup, null);//worlditem);set to null so it will inherit parent owner
								interiorBaseGroup.Props.Interior = true;
								interiorBaseGroup.ParentStructure = this;
								if (!StructureGroupInteriors.ContainsKey(State.BaseInteriorVariant)) {
										StructureGroupInteriors.Add(State.BaseInteriorVariant, interiorBaseGroup);
								} else if (StructureGroupInteriors[State.BaseInteriorVariant] == null) {
										StructureGroupInteriors[State.BaseInteriorVariant] = interiorBaseGroup;
								}
								interiorBaseGroup.Load();
								while (!interiorBaseGroup.Is(WIGroupLoadState.Loaded)) {
										//interior base group isn't loaded yet, waiting...
										yield return null;
								}

								for (int i = 0; i < State.AdditionalInteriorVariants.Count; i++) {
										int interiorVariant = State.AdditionalInteriorVariants[i];
										WIGroup interiorGroup = WIGroups.GetOrAdd(GroupNameInterior(interiorVariant), StructureGroup, null);
										interiorGroup.Props.Interior = true;
										interiorGroup.ParentStructure = this;

										if (!StructureGroupInteriors.ContainsKey(interiorVariant)) {
												StructureGroupInteriors.Add(interiorVariant, interiorGroup);
										} else if (StructureGroupInteriors[interiorVariant] == null) {
												StructureGroupInteriors[interiorVariant] = interiorGroup;
										}
										if (!interiorGroup.Is(WIGroupLoadState.Loaded)) {
												interiorGroup.Load();
												while (!interiorGroup.Is(WIGroupLoadState.Loaded)) {
														//waiting for interior group to load...
														yield return null;
												}
										}
								}
						}

						yield break;
				}

				public void LoadMinorStructures(StructureLoadState forState)
				{
						if (forState != StructureLoadState.ExteriorLoaded) {
								return;
						}

						for (int i = 0; i < State.MinorStructures.Count; i++) {
								MinorStructure minor = State.MinorStructures[i];
								Structures.AddMinorToload(State.MinorStructures[i], i, worlditem);
						}
				}

				public void UnloadMinorStructures(StructureLoadState forState)
				{
						if (forState != StructureLoadState.ExteriorUnloaded) {
								return;
						}
						if (State.MinorStructures.Count > 0) {
								for (int i = 0; i < State.MinorStructures.Count; i++) {
										Structures.AddMinorToUnload(State.MinorStructures[i]);
								}
						}
				}

				#endregion

				#region outer entrances

				public bool AllOuterEntrancesClosed {
						get {
								bool allClosed = true;
								for (int i = 0; i < OuterEntrances.Count; i++) {
										Window window = null;
										Door door = null;
										if (OuterEntrances[i].worlditem.Is <Window>(out window)) {
												if (window.State.CurrentState != EntranceState.Closed) {
														allClosed = false;
														break;
												}
										} else if (OuterEntrances[i].worlditem.Is <Door>(out door)) {
												if (door.State.CurrentState != EntranceState.Closed) {
														allClosed = false;
														break;
												}
										}
								}
								return allClosed;
						}
				}

				public IEnumerator OnWindowOpen(Window window)
				{
						if (Is(StructureLoadState.InteriorLoaded)) {
								if (window.State.OuterEntrance) {
										RefreshColliders(true);
								}
								yield break;
						}

						Structures.AddInteriorToLoad(this);
						while (Is(StructureLoadState.ExteriorLoaded)) {
								yield return null;
						}

						if (Is(StructureLoadState.InteriorWaitingToLoad | StructureLoadState.InteriorLoading)) {
								//if our load state isn't exterior loaded
								//and it IS interiorloading
								//then everything went fine, proceed and show a loading dialog
								bool showLoadingScreen = GameManager.Is(FGameState.InGame);
								if (showLoadingScreen) {
										StartCoroutine(Frontiers.GUI.GUILoading.LoadStart(Frontiers.GUI.GUILoading.Mode.SmallInGame));
										Frontiers.GUI.GUILoading.ActivityInfo = "Loading Interior...";
										Frontiers.GUI.GUILoading.DetailsInfo = string.Empty;
								}

								while (Is(StructureLoadState.InteriorWaitingToLoad | StructureLoadState.InteriorLoading)) {
										yield return null;
								}

								RefreshRenderers(true);

								if (showLoadingScreen) {
										StartCoroutine(Frontiers.GUI.GUILoading.LoadFinish());
								}
						}
						yield break;
				}

				public IEnumerator OnDoorOpen(Door door)
				{
						if (Is(StructureLoadState.InteriorLoaded)) {
								if (door.State.OuterEntrance) {
										RefreshColliders(true);
								}
								yield break;
						}

						Structures.AddInteriorToLoad(this);
						while (Is(StructureLoadState.ExteriorLoaded)) {
								yield return null;
						}

						if (Is(StructureLoadState.InteriorWaitingToLoad | StructureLoadState.InteriorLoading)) {
								//if our load state isn't exterior loaded
								//and it IS interiorloading
								//then everything went fine, proceed and show a loading dialog
								bool showLoadingScreen = GameManager.Is(FGameState.InGame);
								if (showLoadingScreen) {
										StartCoroutine(Frontiers.GUI.GUILoading.LoadStart(Frontiers.GUI.GUILoading.Mode.SmallInGame));
										Frontiers.GUI.GUILoading.ActivityInfo = "Loading Interior...";
										Frontiers.GUI.GUILoading.DetailsInfo = string.Empty;
								}

								while (Is(StructureLoadState.InteriorWaitingToLoad | StructureLoadState.InteriorLoading)) {
										yield return null;
								}

								RefreshRenderers(true);

								if (showLoadingScreen) {
										StartCoroutine(Frontiers.GUI.GUILoading.LoadFinish());
								}
						}
						yield break;
				}

				public void OnWindowClose(Window window)
				{
						//do this check here instead of in structures because it's expensive (?)
						/*
						if (Is (StructureLoadState.InteriorLoaded) && 
							AllOuterEntrancesClosed && 
							!State.ForceBuildInterior &&
							!Player.Local.Surroundings.IsInsideStructure (this)) {
							//Debug.Log ("All outer entrances are closed, UNLOADING now");
							foreach (WIGroup interiorGroup in StructureGroupInteriors.Values) {
								if (interiorGroup != null) {
									interiorGroup.Unload ();
								}
							}
							StructureGroupInteriors.Clear ();
							State.AdditionalInteriorVariants.Clear ();
							Structures.AddInteriorToUnload (this);
						} else {
							//Debug.Log ("All outer entrances are not closed");
						}
						*/
				}

				public void OnDoorClose(Door door)
				{
						//do this check herer instead of in structures because it's expensive (?)
				}

				#endregion

				#region character spawning

				public void SpawnExteriorCharacters()
				{
						if (GameManager.Get.TestingEnvironment)
								return;

						StructureGroup.Load();
						SpawnCharacters(StructureGroup.GetActionNodes(), State.ExteriorCharacters, StructureGroup, State.ExtResidentFlags);
						State.ExteriorCharactersSpawned = true;
				}

				public void SpawnInteriorCharacters(int interiorVariant)
				{
						if (GameManager.Get.TestingEnvironment)
								return;

						WIGroup interiorGroup = null;
						if (StructureGroupInteriors.TryGetValue(interiorVariant, out interiorGroup)) {
								SpawnCharacters(interiorGroup.GetActionNodes(), State.InteriorCharacters, interiorGroup, State.IntResidentFlags);
								State.InteriorCharactersSpawned.Add(interiorVariant);
						} else {
								Debug.Log ("Couldn't find interior variant " + interiorVariant.ToString () + " when spawning structure " + name);
						}
				}

				protected void SpawnCharacters(List <ActionNodeState> nodes, List <StructureSpawn> spawns, WIGroup group, WIFlags flags)
				{
						bool spawnedCharacter = false;
						bool spawnedOwner = false;
						Character character = null;
						for (int i = 0; i < spawns.Count; i++) {
								//spawn a character for every node
								//if the spawn has a custom conversation setting use that now
								spawnedCharacter |= SpawnCharacter(nodes, spawns[i], group, flags, out character);
						}

						if (!OwnerCharacterSpawned) {
								if (!State.OwnerSpawn.IsEmpty && State.OwnerSpawn.Interior == group.Props.Interior) {
										//spawn a character for the owner node
										spawnedCharacter |= SpawnCharacter(nodes, State.OwnerSpawn, group, flags, out StructureOwner);
										if (OwnerCharacterSpawned) {
												//set the owner of the structure group
												StructureGroup.Owner = StructureOwner.worlditem;
												OnOwnerCharacterSpawned.SafeInvoke();
										}
								}
						}
						//let everyone know what just happened
						if (spawnedCharacter) {
								OnCharacterSpawned.SafeInvoke();
						}
				}

				protected bool SpawnCharacter(List <ActionNodeState> nodes, StructureSpawn spawn, WIGroup group, WIFlags flags, out Character character)
				{
						bool spawnedCharacter = false;
						character = null;
						for (int j = 0; j < nodes.Count; j++) {
								//if we've found the node we're supposed to be using
								if (string.Equals(nodes[j].Name, spawn.ActionNodeName)) {
										ActionNodeState node = nodes[j];
										//do we use a custom speech or conversation?
										if (!string.IsNullOrEmpty(spawn.CustomConversation)) {
												node.CustomConversation = spawn.CustomConversation;
										}
										if (!string.IsNullOrEmpty(spawn.CustomSpeech)) {
												node.CustomSpeech = spawn.CustomSpeech;
										}
										node.OccupantIsDead = spawn.IsDead;
										spawnedCharacter = Characters.SpawnCharacter(node.actionNode, spawn.TemplateName, flags, group, out character);
										break;
								}
						}
						return spawnedCharacter;
				}

				#endregion

				#if UNITY_EDITOR
				//all of this stuff is for an in-game editor
				//and isn't included at runtime
				public StructureTemplate template = null;
				public StructureBuilder sb = null;
				public GameObject shingle = null;
				public bool ShowDefaultEditor = false;

				public Transform CreateSignboardTransform()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor(true);
						Mods.Get.Editor.LoadMod <StructureTemplate>(ref template, "Structure", State.TemplateName);
						Transform signboard = StructureBase.FindOrCreateChild("__SIGNBOARD");
						if (template.CommonSignboardOffset != null) {
								template.CommonSignboardOffset.ApplyTo(signboard);
						}
						return signboard;
				}

				public void OnDrawGizmos()
				{
						if (template != null && StructureBase != null) {
								Gizmos.color = Color.cyan;
								if (sb == null || shingle == null) {
										sb = StructureBase.GetOrAdd <StructureBuilder>();
										shingle = sb.gameObject.FindOrCreateChild("__SHINGLE").gameObject;
								}
								Gizmos.DrawWireSphere(shingle.transform.position, 1);
						}
				}

				public void DrawEditor()
				{
						if (Application.isPlaying) {
								UnityEngine.GUI.color = Color.cyan;
								GUILayout.Label("Triggers:");
								foreach (KeyValuePair <string, Trigger> trigger in Triggers) {
										GUILayout.Button(trigger.Key);
								}
								GUILayout.Label("-------");
								return;
						}

						hasOwner = false;
						hasAtLeastOneSpawn = false;

						MissionInteriorController mic = gameObject.GetComponent <MissionInteriorController>();

						UnityEngine.GUI.color = Color.gray;
						if (!ShowDefaultEditor) {
								if (GUILayout.Button("Show default editor")) {
										ShowDefaultEditor = true;
								}
						} else if (GUILayout.Button("Hide default editor")) {
								ShowDefaultEditor = false;
						}

						if (GUILayout.Button("Refresh")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor(true);
								Mods.Get.Editor.LoadMod <StructureTemplate>(ref template, "Structure", State.TemplateName);
						}

						ActionNodeState ownerNode = null;
						List <string> problemsWithStructure = new List<string>();
						bool foundSpawn = false;

						if (template == null) {
								UnityEngine.GUI.color = Color.yellow;
								GUILayout.Label("No template found");
						} else {
								UnityEngine.GUI.color = Color.cyan;
								DrawActionNodes("Exterior", template.Exterior.ActionNodes, State.ExteriorCharacters, problemsWithStructure, false);

								InteriorVariantEnum intVar = (InteriorVariantEnum)State.BaseInteriorVariant;
								intVar = (InteriorVariantEnum)UnityEditor.EditorGUILayout.EnumPopup("Base Interior Variant", intVar);
								State.BaseInteriorVariant = (int)intVar;

								if (mic == null) {
										for (int i = 0; i < template.InteriorVariants.Count; i++) {
												DrawInteriorGroup(i, problemsWithStructure);
										}
								} else {
										UnityEngine.GUI.color = Color.gray;
										GUILayout.Label("-------------\n(Interior states controlled by MIC)\n-------------");
										State.InteriorCharacters.Clear();
								}
						}

						if (StructureBase == null) {
								if (!string.IsNullOrEmpty(State.TemplateName)) {
										Transform structureBase = transform.FindChild(State.TemplateName + "-STR");
										if (structureBase != null) {
												StructureBase = structureBase.gameObject;
										} else {
												problemsWithStructure.Add("0: No structure base");
												UnityEngine.GUI.color = Color.gray;
												if (GUILayout.Button("Create structure base")) {
														StructureBase = gameObject.FindOrCreateChild(State.TemplateName + "-STR").gameObject;
														StructureBase.GetOrAdd <StructureBuilder>();
														State.PrimaryBuilderOffset.ApplyTo(StructureBase.transform);
												}
										}						 
								}
						}

						foreach (MinorStructure minorStructure in State.MinorStructures) {
								if (string.IsNullOrEmpty(minorStructure.TemplateName)) {
										problemsWithStructure.Add("2: Minor structure template name is empty");
								} else {
										GameObject minorStructureBase = gameObject.FindOrCreateChild(minorStructure.TemplateName + "-STR").gameObject;
										minorStructure.Position = minorStructureBase.transform.localPosition;
										minorStructure.Rotation = minorStructureBase.transform.localRotation.eulerAngles;
										UnityEditor.EditorUtility.SetDirty(this);
								}
						}

						if (StructureBase != null) {
								UnityEngine.GUI.color = Color.gray;
								if (GUILayout.Button("Refresh offset")) {
										State.PrimaryBuilderOffset = new STransform(StructureBase.transform);
										State.TemplateName = StructureBuilder.GetTemplateName(StructureBase.name);
										UnityEditor.EditorUtility.SetDirty(this);
								}
						}

						if (string.IsNullOrEmpty(State.TemplateName)) {
								if (StructureBase != null) {
										State.TemplateName = StructureBuilder.GetTemplateName(StructureBase.name);
										UnityEditor.EditorUtility.SetDirty(this);
								} else {
										problemsWithStructure.Add("2: No template specified!");
								}
						}

						if (StructureBase != null && !string.IsNullOrEmpty(State.TemplateName)) {
								UnityEngine.GUI.color = Color.gray;
								sb = StructureBase.GetOrAdd <StructureBuilder>();
								if (GUILayout.Button("Build structure footprint")) {
										sb.CreateStructureFootprint();
								}
								if (shingle != null) {
										if (GUILayout.Button("Apply default shingle offset")) {
												sb.transform.parent = null;
												transform.position = shingle.transform.position;
												sb.transform.parent = transform;
										}
								} else {
										if (GUILayout.Button("Clear Temporary Structure")) {
												StructureBuilder.EditorClearStructure(sb.transform);
										}
								}
						}

						if (template != null && !Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor(true);
								//check templates for missing characters
								List <string> availableTemplates = Mods.Get.Editor.Available("Character");
								if (!string.IsNullOrEmpty(State.OwnerSpawn.TemplateName)) {
										if (!availableTemplates.Contains(State.OwnerSpawn.TemplateName)) {
												problemsWithStructure.Add("1: Char template " + State.OwnerSpawn.TemplateName + " is not available");
										}
								}
								foreach (StructureSpawn spawn in State.InteriorCharacters) {
										if (!availableTemplates.Contains(spawn.TemplateName)) {
												problemsWithStructure.Add("1: Char template " + spawn.TemplateName + " is not available");
										}
								}
								foreach (StructureSpawn spawn in State.ExteriorCharacters) {
										if (!availableTemplates.Contains(spawn.TemplateName)) {
												problemsWithStructure.Add("1: Char template " + spawn.TemplateName + " is not available");
										}
								}
								foreach (string availableTemplate in availableTemplates) {

								}
						}


						if (mic == null) {
								if (!hasOwner) {
										problemsWithStructure.Add("1: Has no owner spawn (meant to be ownerless?)");
								}
								if (!hasAtLeastOneSpawn) {
										problemsWithStructure.Add("2: Has no spawns in use");
								}

								GUILayout.BeginVertical();
								if (problemsWithStructure.Count > 0) {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Label("Problems with structure:");
										foreach (string problem in problemsWithStructure) {
												if (problem.StartsWith("0: ")) {
														UnityEngine.GUI.color = Color.cyan;
												} else if (problem.StartsWith("1:")) {
														UnityEngine.GUI.color = Color.yellow;
												} else if (problem.StartsWith("2:")) {
														UnityEngine.GUI.color = Color.red;
												}
												GUILayout.Label(problem);
										}
								}
								GUILayout.EndVertical();
						} else {
								DrawMissionInteriorControllerEditor();
						}
				}

				protected void DrawInteriorGroup(int interiorVariant, List <string> problemsWithStructure)
				{
						bool isBaseGroup = interiorVariant == State.BaseInteriorVariant;
						bool isSelected = State.AdditionalInteriorVariants.Contains(interiorVariant);
						string baseString = string.Empty;
						if (isSelected || isBaseGroup) {
								UnityEngine.GUI.color = Color.yellow;
								if (isBaseGroup) {
										UnityEngine.GUI.color = Color.cyan;
										baseString = " (BASE)";
								}
						} else {
								UnityEngine.GUI.color = Color.gray;
						}

						bool useAsVariant = GUILayout.Toggle(isSelected || isBaseGroup, "Interior Variant " + interiorVariant.ToString() + baseString + " (" + template.InteriorVariants[interiorVariant].Description + ")");
						//jesus christ...
						if (useAsVariant && !(isSelected || isBaseGroup)) {
								if (!isBaseGroup) {
										State.AdditionalInteriorVariants.Add(interiorVariant);
								}
						}

						if (isSelected || isBaseGroup) {
								if (interiorVariant < template.InteriorVariants.Count) {
										DrawActionNodes("Interior " + interiorVariant.ToString(), template.InteriorVariants[interiorVariant].ActionNodes, State.InteriorCharacters, problemsWithStructure, true);
								}
						} else {
								GUILayout.Label("(Not in use)");
						}
				}

				protected bool hasOwner = false;
				protected bool hasAtLeastOneSpawn = false;

				protected void DrawActionNodes(string type, List <ActionNodeState> charNodes, List <StructureSpawn> spawns, List <string> problemsWithStructure, bool interior)
				{
						//first draw the owner node
						foreach (ActionNodeState ac in charNodes) {
								if (ac.UseAsSpawnPoint) {
										DrawActionNode(ac, spawns, ref State.OwnerSpawn, interior);
								}
						}
				}

				public void DrawMissionInteriorControllerEditor()
				{
						UnityEngine.GUI.color = Color.yellow;
						MissionInteriorController mic = gameObject.GetComponent <MissionInteriorController>();
						GUILayout.Label("---Mission interior controller:---");
						if (GUILayout.Button("Add controller state")) {
								mic.State.Conditions.Add(new MissionInteriorCondition());
						}
						MissionInteriorCondition micConditionToDelete = null;
						MissionInteriorCondition micConditionToMoveUp = null;
						MissionInteriorCondition micConditionToMoveDown = null;

						List <MissionInteriorCondition> conditions = new List<MissionInteriorCondition>();
						conditions.AddRange(mic.State.Conditions);
						conditions.Insert(0, mic.State.Default);

						int micIndex = 0;

						foreach (MissionInteriorCondition micCondition in conditions) {
								bool isBase = (micCondition == mic.State.Default);
								if (!isBase && !micCondition.Visible) {
										UnityEngine.GUI.color = Color.cyan;
										if (GUILayout.Button("Show " + micIndex.ToString() + " " + micCondition.Description)) {
												micCondition.Visible = true;
										}
								} else {

										GUILayout.BeginHorizontal();
										UnityEngine.GUI.color = Color.yellow;
										if (isBase) {
												UnityEngine.GUI.color = Color.gray;
										}
										if (GUILayout.Button("up", GUILayout.Width(25)) && !isBase) {
												micConditionToMoveUp = micCondition;
										}
										if (GUILayout.Button("dn", GUILayout.Width(25)) && !isBase) {
												micConditionToMoveDown = micCondition;
										}
										if (!isBase) {
												if (GUILayout.Button(micCondition.Priority.ToString() + "+", GUILayout.Width(25))) {
														micCondition.Priority++;
												}
												if (GUILayout.Button("-", GUILayout.Width(25))) {
														micCondition.Priority--;
												}
										}
										GUILayout.EndHorizontal();

										if (!isBase) {
												GUILayout.BeginHorizontal();
												UnityEngine.GUI.color = Color.gray;
												if (!string.IsNullOrEmpty(micCondition.MissionName)) {
														UnityEngine.GUI.color = Color.green;
												}
												GUILayout.Label("When ", GUILayout.Width(40));
												micCondition.MissionName = Mods.ModsEditor.GUILayoutAvailable(micCondition.MissionName, "Mission", 100);
												GUILayout.Label(" is of status ", GUILayout.Width(75));
												micCondition.Status = (MissionStatus)UnityEditor.EditorGUILayout.EnumPopup(micCondition.Status, GUILayout.Width(100));

												UnityEngine.GUI.color = Color.red;
												if (GUILayout.Button("X", GUILayout.MaxWidth(20))) {
														micConditionToDelete = micCondition;
												}

												GUILayout.EndHorizontal();

												if (!string.IsNullOrEmpty(micCondition.MissionName)) {
														GUILayout.BeginHorizontal();

														UnityEngine.GUI.color = Color.gray;
														if (micCondition.SecondMissionOperator != MissionStatusOperator.None) {
																UnityEngine.GUI.color = Color.green;
														}
														micCondition.SecondMissionOperator = (MissionStatusOperator)UnityEditor.EditorGUILayout.EnumPopup(micCondition.SecondMissionOperator, GUILayout.Width(40));
														if (micCondition.SecondMissionOperator != MissionStatusOperator.None) {
																micCondition.SecondMissionName = Mods.ModsEditor.GUILayoutAvailable(micCondition.SecondMissionName, "Mission", true, "None", 100);
																GUILayout.Label(" is of status ", GUILayout.Width(30));
																micCondition.SecondMissionStatus = (MissionStatus)UnityEditor.EditorGUILayout.EnumPopup(micCondition.SecondMissionStatus, GUILayout.Width(100));
														}
														GUILayout.EndHorizontal();
												} else {
														micCondition.SecondMissionOperator = MissionStatusOperator.None;
														micCondition.SecondMissionName = string.Empty;
														micCondition.SecondMissionStatus = MissionStatus.Dormant;
												}

												GUILayout.BeginHorizontal();

												UnityEngine.GUI.color = Color.gray;
												if (!string.IsNullOrEmpty(micCondition.ObjectiveName)) {
														UnityEngine.GUI.color = Color.green;
												}
												GUILayout.Label("and ", GUILayout.Width(35));
												micCondition.ObjectiveName = Mods.ModsEditor.GUILayoutMissionObjective(micCondition.MissionName, micCondition.ObjectiveName, true, "None", 100);
												GUILayout.Label(" is of status ", GUILayout.Width(30));
												micCondition.ObjectiveStatus = (MissionStatus)UnityEditor.EditorGUILayout.EnumPopup(micCondition.ObjectiveStatus, GUILayout.Width(100));

												GUILayout.EndHorizontal();

												if (!string.IsNullOrEmpty(micCondition.ObjectiveName)) {
														GUILayout.BeginHorizontal();

														UnityEngine.GUI.color = Color.gray;
														if (micCondition.SecondObjectiveOperator != MissionStatusOperator.None) {
																UnityEngine.GUI.color = Color.green;
														}
														micCondition.SecondObjectiveOperator = (MissionStatusOperator)UnityEditor.EditorGUILayout.EnumPopup(micCondition.SecondObjectiveOperator, GUILayout.Width(40));
														if (micCondition.SecondObjectiveOperator != MissionStatusOperator.None) {
																micCondition.SecondObjectiveName = Mods.ModsEditor.GUILayoutMissionObjective(micCondition.MissionName, micCondition.SecondObjectiveName, true, "None", 100);
																GUILayout.Label(" is of status ", GUILayout.Width(30));
																micCondition.SecondObjectiveStatus = (MissionStatus)UnityEditor.EditorGUILayout.EnumPopup(micCondition.SecondObjectiveStatus, GUILayout.Width(100));
														}
														GUILayout.EndHorizontal();
												} else {
														micCondition.SecondObjectiveOperator = MissionStatusOperator.None;
														micCondition.SecondObjectiveName = string.Empty;
														micCondition.SecondObjectiveStatus = MissionStatus.Dormant;
												}

												GUILayout.BeginHorizontal();

												UnityEngine.GUI.color = Color.gray;
												if (!string.IsNullOrEmpty(micCondition.MissionVariableName)) {
														UnityEngine.GUI.color = Color.green;
												}
												GUILayout.Label("and ", GUILayout.Width(35));
												micCondition.MissionVariableName = Mods.ModsEditor.GUILayoutMissionVariable(micCondition.MissionName, micCondition.MissionVariableName, true, "None", 200);
												GUILayout.Label(" is ", GUILayout.Width(30));
												micCondition.CheckType = (VariableCheckType)UnityEditor.EditorGUILayout.EnumPopup(micCondition.CheckType, GUILayout.Width(150));
												GUILayout.Label(" than ", GUILayout.Width(40));
												micCondition.MissionVariableValue = UnityEditor.EditorGUILayout.IntField(micCondition.MissionVariableValue, GUILayout.MaxWidth(50));

												GUILayout.EndHorizontal();
										}
				
										GUILayout.BeginHorizontal();

										UnityEngine.GUI.color = Color.cyan;
										GUILayout.EndHorizontal();
										GUILayout.Toggle(true, "0 (Base)");
										if (template.InteriorVariants.Count > 0) {
												for (int j = 0; j < template.InteriorVariants[0].ActionNodes.Count; j++) {
														ActionNodeState ac = template.InteriorVariants[0].ActionNodes[j];
														DrawActionNode(ac, micCondition.StateVariable.AdditionalInteriorCharacters, ref micCondition.StateVariable.OwnerSpawn, true);
												}
												for (int i = 1; i < template.InteriorVariants.Count; i++) {
														UnityEngine.GUI.color = Color.gray;
														if (micCondition.StateVariable == null) {
																micCondition.StateVariable = new MissionInteriorStructureState();
														}
														bool isUsingThis = micCondition.StateVariable.InteriorVariants.Contains(i);
														if (isUsingThis) {
																UnityEngine.GUI.color = Color.cyan;
														}
														bool shouldBeUsingThis = GUILayout.Toggle(isUsingThis, i.ToString() + " (" + template.InteriorVariants[i].Description + ")");
														if (isUsingThis && !shouldBeUsingThis) {
																micCondition.StateVariable.InteriorVariants.Remove(i);
														} else if (!isUsingThis && shouldBeUsingThis) {
																micCondition.StateVariable.InteriorVariants.Add(i);
														}
														if (isUsingThis) {
																for (int j = 0; j < template.InteriorVariants[i].ActionNodes.Count; j++) {
																		ActionNodeState ac = template.InteriorVariants[i].ActionNodes[j];
																		DrawActionNode(ac, micCondition.StateVariable.AdditionalInteriorCharacters, ref micCondition.StateVariable.OwnerSpawn, true);
																}
														}
												}
										}
								}

								if (!isBase && micCondition.Visible) {
										UnityEngine.GUI.color = Color.cyan;
										if (GUILayout.Button("Hide " + micIndex.ToString())) {
												micCondition.Visible = false;
										}
								}
								GUILayout.Label("");//spacer

								micIndex++;
						}


						if (micConditionToDelete != null) {
								mic.State.Conditions.Remove(micConditionToDelete);
						}
						if (micConditionToMoveUp != null) {
								int oldIndex = mic.State.Conditions.IndexOf(micConditionToMoveUp);
								int newIndex = oldIndex - 1;
								//Debug.Log ("Moving from " + oldIndex.ToString () + " to " + newIndex.ToString ());
								if (newIndex < 0) {
										newIndex = 0;
								}
								mic.State.Conditions.Remove(micConditionToMoveUp);
								mic.State.Conditions.Insert(newIndex, micConditionToMoveUp);
						} else if (micConditionToMoveDown != null) {
								int oldIndex = mic.State.Conditions.IndexOf(micConditionToMoveDown);
								int newIndex = oldIndex + 1;
								//Debug.Log ("Moving from " + oldIndex.ToString () + " to " + newIndex.ToString ());
								mic.State.Conditions.Remove(micConditionToMoveDown);
								if (newIndex >= mic.State.Conditions.Count) {
										mic.State.Conditions.Add(micConditionToMoveDown);
								} else {
										mic.State.Conditions.Insert(newIndex, micConditionToMoveDown);
								}
						}
				}

				protected void DrawActionNode(ActionNodeState ac, List <StructureSpawn> charNodes, ref StructureSpawn ownerSpawn, bool interior)
				{

						UnityEngine.GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						GUILayout.Label(" ", GUILayout.Width(25));
						StructureSpawn ss = null;
						bool isInUse = FindStructureSpawnForActionNode(ac, out ss, ownerSpawn, charNodes);
						bool isOwner = false;
						if (!isInUse) {
								UnityEngine.GUI.color = Color.gray;
						} else {
								hasAtLeastOneSpawn = true;
								isOwner = (ss == ownerSpawn);
						}
						if (isOwner) {
								UnityEngine.GUI.color = Color.green;
								hasOwner = true;
						}
						bool shouldBeInUse = GUILayout.Toggle(isInUse, ac.Name, GUILayout.Width(300));
						if (!isInUse && shouldBeInUse) {
								ss = new StructureSpawn();
								ss.ActionNodeName = ac.Name;
								ss.TemplateName = ac.OccupantName;
								charNodes.Add(ss);
						} else if (isInUse && !shouldBeInUse) {
								charNodes.Remove(ss);
						}
						bool shouldBeOwner = GUILayout.Toggle(isOwner, "Owner", GUILayout.Width(75));
						if (isInUse && !isOwner && shouldBeOwner) {
								ownerSpawn = ss;
								charNodes.Remove(ss);
						} else if (isInUse && isOwner && !shouldBeOwner) {
								charNodes.Remove(ss);
								ownerSpawn = new StructureSpawn();
						}
						DrawStructureSpawn(ss, ownerSpawn, ac, isInUse);

						ownerSpawn = ownerSpawn;
						GUILayout.EndHorizontal();
				}

				protected bool DrawStructureSpawn(StructureSpawn ss, StructureSpawn ownerSpawn, ActionNodeState ac, bool isInUse)
				{
						bool isOwnerSpawn = (ss != null && ss == ownerSpawn);
						bool isDead = false;
						if (ss != null) {
								isDead = ss.IsDead;
						}
						Color overallColor = Color.gray;
						if (isInUse) {
								overallColor = Color.cyan;
								if (isOwnerSpawn) {
										overallColor = Color.green;
								}
						}
						string templateName = ac.OccupantName;
						string convoName = ac.CustomConversation;
						string dtsName = ac.CustomSpeech;

						Color templateColor = overallColor;
						Color convoColor = overallColor;
						Color dtsColor = overallColor;
						if (isInUse) {
								if (!string.IsNullOrEmpty(ss.TemplateName) && ss.TemplateName != templateName) {
										templateColor = Color.yellow;
										templateName = ss.TemplateName;
								}
								if (!string.IsNullOrEmpty(ss.CustomConversation)) {
										convoColor = Color.yellow;
										convoName = ss.CustomConversation;
								}
								if (!string.IsNullOrEmpty(ss.CustomSpeech)) {
										dtsColor = Color.yellow;
										dtsName = ss.CustomSpeech;
								}
						}
						UnityEngine.GUI.color = templateColor;
						GUILayout.Label("Template:");
						templateName = Mods.ModsEditor.GUILayoutAvailable(templateName, "Character", true, ac.OccupantName, 100);

						UnityEngine.GUI.color = convoColor;
						GUILayout.Label("Convo:");
						convoName = Mods.ModsEditor.GUILayoutAvailable(convoName, "Conversation", true, ac.CustomConversation, 250);

						UnityEngine.GUI.color = dtsColor;
						GUILayout.Label("Speech:");
						dtsName = Mods.ModsEditor.GUILayoutAvailable(dtsName, "Speech", true, ac.CustomSpeech, 250);

						UnityEngine.GUI.color = overallColor;
						isDead = GUILayout.Toggle(isDead, new GUIContent("IsDead"));

						if (isInUse) {
								ss.IsDead = isDead;
								ss.TemplateName = templateName;
								ss.CustomConversation = convoName;
								ss.CustomSpeech = dtsName;
								return true;
						} else {
								return false;
						}
				}

				protected bool FindStructureSpawnForActionNode(ActionNodeState ac, out StructureSpawn ss, StructureSpawn ownerSpawn, List <StructureSpawn> spawns)
				{
						ss = null;
						foreach (StructureSpawn checkSS in spawns) {
								if (checkSS.ActionNodeName == ac.Name) {
										ss = checkSS;
										break;
								}
						}

						if (ss == null) {
								if (ownerSpawn.ActionNodeName == ac.Name) {
										ss = ownerSpawn;
								}
						}

						return ss != null;
				}

				protected void GetActionNodes(StructureTemplateGroup templateGroup, List <ActionNodeState> charNodes, List <ActionNodeState> randNodes, List <string> problemsWithStructure)
				{
						foreach (ActionNodeState actionNode in templateGroup.ActionNodes) {
								if (actionNode.Type != ActionNodeType.StructureOwnerSpawn) {
										if (actionNode.UseAsSpawnPoint && actionNode.Users != ActionNodeUsers.PlayerOnly) {
												if (actionNode.UseGenericTemplate) {
														randNodes.Add(actionNode);
												} else {
														charNodes.Add(actionNode);
												}
										}
								}
						}
				}

				public enum InteriorVariantEnum
				{
						I_0 = 0,
						I_1,
						I_2,
						I_3,
						I_4,
						I_5,
						I_6,
						I_7,
						I_8,
						I_9,
				}
				#endif
		}

		[Serializable]
		public class StructureState
		{
				//static template properties
				[FrontiersAvailableModsAttribute("Structure")]
				public string TemplateName = string.Empty;
				public int BaseInteriorVariant = 0;
				public List <int> AdditionalInteriorVariants = new List <int>();
				public STransform PrimaryBuilderOffset = new STransform();
				public MinorStructure DestroyedMinorStructure = new MinorStructure();
				public List <MinorStructure> MinorStructures = new List<MinorStructure>();
				//flags and settings
				public bool IsSafeLocation = false;
				public bool IsOwnedByPlayer = false;
				public bool IsRespawnStructure = false;
				public bool ForceBuildInterior = false;
				public bool HasSetFlags = false;
				public PlayerIDFlag PlayerOwnerID = PlayerIDFlag.Local;
				public AmbientAudioManager.ChunkAudioItem AmbientAudio = new AmbientAudioManager.ChunkAudioItem();
				public TimeOfDay GenericEntrancesLockedTimes = TimeOfDay.ff_All;
				public TimeOfDay OwnerKnockAvailability = TimeOfDay.a_None;
				//building state
				public bool ExteriorLoadedOnce = false;
				public List <int> InteriorsLoadedOnce = new List <int>();
				public List <int> InteriorCharactersSpawned = new List <int>();
				public bool ExteriorCharactersSpawned = false;
				//characters state
				public StructureSpawn OwnerSpawn = new StructureSpawn(string.Empty, "StructureOwner");
				public List <StructureSpawn> ExteriorCharacters = new List <StructureSpawn>();
				public List <StructureSpawn> InteriorCharacters = new List <StructureSpawn>();
				//flags
				public bool ExclusiveStructureFlags = false;
				public bool ExclusiveIntResidentFlags = false;
				public bool ExclusiveExtResidentFlags = false;
				public WIFlags StructureFlags = new WIFlags();
				public CharacterFlags IntResidentFlags = new CharacterFlags();
				public CharacterFlags ExtResidentFlags = new CharacterFlags();
				//destruction properties
				public float TimeDestroyed = 0f;
				public StructureLoadState LoadState = StructureLoadState.ExteriorUnloaded;
		}

		[Serializable]
		public class StructureSpawn
		{
				public StructureSpawn()
				{

				}

				public StructureSpawn(string templateName, string actionNodeName)
				{
						TemplateName = templateName;
						ActionNodeName = actionNodeName;
				}

				public bool IsEmpty = false;
				[FrontiersAvailableModsAttribute("Character")]
				public string TemplateName;
				public string ActionNodeName;
				[FrontiersAvailableModsAttribute("Speech")]
				public string CustomSpeech;
				[FrontiersAvailableModsAttribute("Conversation")]
				public string CustomConversation;
				[XmlIgnore]
				[NonSerialized]
				public bool HasSpawned = false;
				public bool Interior = true;
				public bool IsDead = false;
				//public int InteriorVariant = 0;
				//public bool Random = false;
		}

		[Serializable]
		public class DeedOfOwnership
		{
		}
}