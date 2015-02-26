using UnityEngine;
using System;
using Frontiers.Data;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public partial class WorldItem : MonoBehaviour, IWIBase, IUnloadableChild, IVisible
		{
				public Transform tr;
				public Rigidbody rb;
				public Bounds BaseObjectBounds;
				public Vector3 BasePivotOffset;
				public TNObject NObject;
				public ReceptaclePivot ParentPivot;

				public bool IsNObject {
						get {
								return NObject != null;
						}
				}

				public bool IsTemplate = true;
				public float LastActiveDistanceToPlayer = -1f;

				public WIActiveState ActiveState {
						get {
								return mActiveState;
						}
						set {
								if (mActiveStateLocked)
										return;

								if (value != mActiveState) {
										mLastActiveState = mActiveState;
										mActiveState = value;
										if (mActiveState == WIActiveState.Active) {
												LastActiveDistanceToPlayer = ActiveRadius * 0.5f;
										} else if (mActiveState == WIActiveState.Invisible) {
												LastActiveDistanceToPlayer = VisibleDistance * 0.95f;
										}
										WorldItems.Get.SendToStateList(this);
										Refresh();
								}
						}
				}

				public WILoadState LoadState {
						get {
								return mLoadState;
						}
						set {
								mLoadState = value;
						}
				}

				public float ActiveRadius {
						get {
								return Props.Local.ActiveRadius;
						}
						set {
								Props.Local.ActiveRadius = value;
						}
				}

				public bool ActiveStateLocked {
						get {
								return mActiveStateLocked;
						}
						set {
								if (mActiveStateLocked != value) {
										mActiveStateLocked = value;
										WorldItems.Get.SendToStateList(this);
								}
						}
				}

				public bool SaveStateLocked {
						get {
								return mLockSaveState;
						}
				}

				public float VisibleDistance { 
						get {
								return Props.Local.VisibleDistance * Globals.WorldItemVisibleDistanceMultiplier;
						}
				}

				public string GenerateFileName(int increment)
				{
						Props.Name.AutoIncrementFileName = true;
						Props.Name.FileNameIncrement = increment;
						#if UNITY_EDITOR
						if (Application.isPlaying) {
								#endif
								var enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext()) {
										//foreach (WIScript script in mScripts.Values) {
										if (!enumerator.Current.AutoIncrementFileName) {
												Props.Name.FileName = enumerator.Current.GenerateUniqueFileName(increment);
												Props.Name.AutoIncrementFileName = false;
												break;
										}
								}
								#if UNITY_EDITOR
						} else {
								WIScript[] wiScripts = gameObject.GetComponents <WIScript>();
								foreach (WIScript script in wiScripts) {
										if (!script.AutoIncrementFileName) {
												Props.Name.FileName = script.GenerateUniqueFileName(increment);
												Props.Name.AutoIncrementFileName = false;
												break;
										}
								}
						}
						#endif
						if (Props.Name.AutoIncrementFileName) {
								Props.Name.FileName = Props.Global.FileNameBase + "-" + Props.Name.FileNameIncrement.ToString();
						}
						name = Props.Name.FileName;
						return Props.Name.FileName;
				}

				protected bool mActiveStateLocked = false;
				protected WIFlags mFlags = null;
				protected WIActiveState mActiveState = WIActiveState.Invisible;
				protected WIActiveState mLastActiveState = WIActiveState.Invisible;
				protected WILoadState mLoadState = WILoadState.Initializing;
				protected StackItem mTemplateStackitem = null;

				#region WorldItem set state

				public WIGroup Group { get; set; }

				[NonSerialized]
				[HideInInspector]
				public WISaveState SaveState;
				[SerializeField]
				public WIProps Props = new WIProps();
				[SerializeField]
				public List <Renderer> Renderers = new List <Renderer>();
				[SerializeField]
				public List <Collider> Colliders = new List <Collider>();

				#endregion

				#region other

				//mostly convenience functions
				public bool HasPlayerFocus {
						get {
								return mHasPlayerFocus;
						}set {
								if (value) {
										mHasPlayerFocus = true;
										RefreshHud();
										OnGainPlayerFocus.SafeInvoke();
								} else {
										mHasPlayerFocus = false;
										RefreshHud();
										OnLosePlayerFocus.SafeInvoke();
								}
						}
				}

				public Transform HudTarget {
						get {
								if (HudTargeter != null) {
										return HudTargeter();
								}
								return transform;
						}
				}

				public bool Is(WIMode mode)
				{
						return Frontiers.Flags.Check((uint)Props.Local.Mode, (uint)mode, Frontiers.Flags.CheckType.MatchAny);
				}

				public bool Is(WILoadState loadState)
				{
						return Frontiers.Flags.Check((uint)mLoadState, (uint)loadState, Frontiers.Flags.CheckType.MatchAny);
				}

				public bool Is(WIActiveState activeState)
				{
						return Frontiers.Flags.Check((uint)mActiveState, (uint)activeState, Frontiers.Flags.CheckType.MatchAny);
				}

				public bool IsMadeOf(WIMaterialType materialType)
				{
						return Frontiers.Flags.Check((uint)Props.Global.MaterialType, (uint)materialType, Frontiers.Flags.CheckType.MatchAny);
				}

				public void ApplyForce(Vector3 force, Vector3 forcePoint)
				{
						if (rb == null || !CanBeCarried) {
								return;
						}

						ActiveState = WIActiveState.Active;
						rb.isKinematic = false;
						rb.useGravity = true;
						rb.AddForce(force, ForceMode.Impulse);
				}

				public bool HasPlayerAttention = false;

				public bool UseCustomDoppleganger {
						get {
								return Doppleganger != null;
						}
				}

				public void ClearStackContainer()
				{
						if (mStackContainer != null) {
								mStackContainer.Clear();
						}
						mStackContainer = null;
				}

				public void ClearStackItem()
				{
						if (mTemplateStackitem != null) {
								mTemplateStackitem.Clear();
								mTemplateStackitem = null;
						}
				}

				public WIStackContainer	StackContainer {
						get {
								if (IsStackContainer) {
										if (mStackContainer == null) {
												mStackContainer = Stacks.Create.StackContainer(this, this.Group);
										}
								}
								return mStackContainer;
						}
						set {
								mStackContainer = value;
								if (mStackContainer != null) {
										mStackContainer.Owner = this;
										mStackContainer.Group = this.Group;
								}
								Props.Local.IsStackContainer = mStackContainer != null;
						}
				}

				public string LightTemplateName {
						get {
								if (HasStates) {
										return States.CurrentState.LightTemplateName;
								}
								return Props.Local.LightTemplateName;
						}
				}

				public Vector3 LightOffset {
						get {
								if (HasStates) {
										return States.CurrentState.LightOffset;
								}
								return Props.Local.LightOffset;
						}
				}

				public bool TransformLocked {
						get {
								return mLockedParent != null;
						}
				}

				public Transform LockedParent {
						get {
								return mLockedParent;
						}
				}

				public bool LockTransform(Transform lockedParent)
				{
						if (mLockedParent != null) {
								return false;
						}
						mLockedParent = lockedParent;
						tr.parent = mLockedParent;
						return true;
				}

				public void RefreshTransform()
				{
						//gets the latest in-game position
						if (!mDestroyed) {
								if (tr == null) {
										tr = transform;
								}
								Props.Local.Transform.CopyFrom(tr);
								if (mAddedToGroupOnce) {
										//if we've never been added to a group then our chunk position will be meaningless
										//if we have, then we'll need it to show up correctly on a map etc
										WorldChunk chunk = Group.GetParentChunk();
										if (chunk != null) {
												Props.Local.ChunkPosition = WorldChunk.WorldPositionToChunkPosition(chunk.ChunkBounds, tr.position - chunk.ChunkOffset);
										} else {
												Props.Local.ChunkPosition = Vector3.zero;
										}
								}
						}
				}

				public void UnlockTransform()
				{		//automaticall unlock and put in group
						UnlockTransform(mLockedParent);
				}

				public bool UnlockTransform(Transform lockedParent)
				{
						bool unlocked = false;
						if (mLockedParent == null || lockedParent == mLockedParent) {
								//only unlock if it's the same parent asking
								mLockedParent = null;
								unlocked = true;
						}
						//put us back in the group if we've got nothing else
						if (tr.parent == null && mLockedParent == null && Group != null) {
								tr.parent = Group.tr;
						}
						return false;
				}

				public override int GetHashCode()
				{
						if (Group != null) {
								return (Group.Props.PathName + Props.Name.FileName).GetHashCode();
						}
						return base.GetHashCode();
				}

				public void RefreshHud()
				{
						if (HasPlayerFocus) {
								//GUIHud.Get.ClearFocusItem(worlditem);
								int lastHudPriority = 0;
								int currentHudPriority = 0;
								var enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext()) {
										currentHudPriority = enumerator.Current.OnRefreshHud(lastHudPriority);
										if (currentHudPriority < 0) {
												lastHudPriority = currentHudPriority;
												break;
										} else {
												lastHudPriority = Mathf.Max(currentHudPriority, lastHudPriority);
										}
								}
								//if nothing set the HUD priority, let the player know whether it can enter inventory
								if (lastHudPriority == 0) {
										if (CanEnterInventory && Player.Local.Inventory.CanItemFit(this)) {
												GUIHud.Get.ShowActions(this, UserActionType.ItemUse, UserActionType.ItemInteract, "Pick up", "Interact", HudTarget, GameManager.Get.GameCamera);
										} else {
												GUIHud.Get.ShowAction(this, UserActionType.ItemInteract, "Interact", HudTarget, GameManager.Get.GameCamera);
										}
								} else if (lastHudPriority < 0) {
										GUIHud.Get.ClearFocusItem(worlditem);
								}
						}
				}

				public void OnGainPlayerAttention()
				{
						HasPlayerAttention = true;
						RefreshHud();
				}

				public void OnLosePlayerAttention()
				{
						HasPlayerAttention = false;
						RefreshHud();
				}

				public bool HasLightSource {
						get {
								return Light != null;
						}
				}

				public WorldItemUsable Usable = null;
				public WorldNameCleaner DisplayNamer = null;
				public HudTargetSupplier HudTargeter = null;
				public PlayerFocusHighlight Highlight = null;
				public WIDoppleganger Doppleganger = null;
				public WIStates States = null;
				public WIHud HUD = null;
				public WorldLight Light = null;
				protected bool mHasPlayerFocus = false;
				protected WIScript CleanNameScript = null;
				protected WIScript FileNameScript = null;
				protected WIScript StackNameScript = null;
				protected Transform mLockedParent = null;
				[HideInInspector]
				protected WIStackContainer mStackContainer = null;
				//TEMP for refresh hud stuff
				protected static Trigger mTriggerCheck;
				protected static Receptacle mReceptacleCheck;
				protected static Container mContainerCheck;
				protected static PathMarker mPathMarkerCheck;

				#endregion

				#region mode settings

				public void RemoveFromGame()
				{	
						if (mDestroyed)
								return;

						//convenience
						SetMode(WIMode.RemovedFromGame);
				}

				protected void SetActive()
				{
						ActiveState = WIActiveState.Active;
				}

				protected void SetActive(bool active)
				{
						if (rb != null && !rb.isKinematic) {
								//we can't have disabled colliders with an active rb
								active = true;
						}

						//get rid of collider.enabled wherever possible
						if (HasStates && States.CurrentState != null) {
								if (States.CurrentState.StateCollider != null) {
										States.CurrentState.StateCollider.enabled = active;
								}
						} else {
								for (int i = 0; i < Colliders.Count; i++) {
										if (Colliders[i] != null) {
												Colliders[i].enabled = active;
										}
								}
						}

						if (Is(WILoadState.Initialized | WILoadState.PreparingToUnload | WILoadState.Unloading)) {
								try {
										if (active) {
												OnActive.SafeInvoke();
										} else {
												OnInactive.SafeInvoke();
										}
								} catch (Exception e) {
										Debug.LogException(e);
								}
						}
				}

				protected void SetVisible(bool visible)
				{	//don't set visible if the worlditem is hidden
						RefreshShadowCasters(visible);
						if (Is(WILoadState.Initialized | WILoadState.PreparingToUnload | WILoadState.Unloading)) {
								try {
										if (visible) {
												OnVisible.SafeInvoke();
										} else {
												OnInvisible.SafeInvoke();
										}
								} catch (Exception e) {
										Debug.LogException(e);
								}
						}
				}

				public void RefreshShadowCasters(bool visible)
				{
						if (mDestroyed)
								return;

						if (HasStates && States.CurrentState != null && States.CurrentState.StateRenderer != null) {
								States.CurrentState.StateRenderer.enabled = visible;
								States.CurrentState.StateRenderer.castShadows = WorldItems.ObjectShadows ? Props.Global.CastShadows : false;
								States.CurrentState.StateRenderer.receiveShadows = WorldItems.ObjectShadows ? Props.Global.CastShadows : false;
						} else {
								for (int i = 0; i < Renderers.Count; i++) {
										if (Renderers[i] != null) {
												Renderers[i].enabled = visible;
												Renderers[i].castShadows = WorldItems.ObjectShadows ? Props.Global.CastShadows : false;
												Renderers[i].receiveShadows = WorldItems.ObjectShadows ? Props.Global.CastShadows : false;
										}
								}
						}
				}

				public virtual void	SetMode(WIMode mode)
				{
						if (mDestroyed)
								return;

						if (!Is(WILoadState.Initialized | WILoadState.PreparingToUnload) || mode == WIMode.None)
								return;

						if (IsTemplate) {
								return;
						}

						Props.Local.PreviousMode = Props.Local.Mode;
						Props.Local.Mode = mode;

						switch (mode) {
								case WIMode.World:
										OnSetWorldMode(false);
										break;

								case WIMode.Hidden:
										OnSetHiddenMode();
										break;

								case WIMode.Destroyed:
										OnSetDestroyedMode();
										break;

								case WIMode.Frozen:
										OnSetWorldMode(true);
										break;

								case WIMode.Equipped:
										OnSetEquippedMode();
										break;

								case WIMode.RemovedFromGame:
										StartCoroutine(OnSetRemovedMode());
										break;

								default:
										//stacked, crafting, etc. all handle themselves
										//just set the mode
										break;
						}
						//we don't bother to check if we're already in this mode
						//sometimes calling mode is like a 'reset' button and we
						//don't want to prevent that from happening
						if (mode != WIMode.RemovedFromGame) {
								try {
										OnModeChange.SafeInvoke();
								} catch (Exception e) {
										Debug.LogError("Exception when changing mode, proceeding normally: " + e.ToString());
								}
						}
				}

				protected void OnSetWorldMode(bool freeze)
				{
						if (mDestroyed)
								return;

						if (rb != null) {
								rb.isKinematic = true;
								rb.useGravity = true;
						}

						if (Props.Global.ParentUnderGroup && !TransformLocked) {
								if (Group != null && tr.parent != Group.tr) {
										tr.parent = Group.tr;
								}
						}

						if (freeze) {
								Props.Local.Mode = WIMode.Frozen;
								if (rb != null) {
										rb.isKinematic = true;
										rb.useGravity = true;
								}
						} else {
								if (rb != null) {
										rb.isKinematic = false;
										rb.useGravity = true;
								}
						}

						ParentPivot = null;
						//if we're going into world mode then we're not in the stack any more
						OnRemoveFromStack.SafeInvoke();
						//set it to null because we don't need it any more
						OnRemoveFromStack = null;

						ActiveStateLocked = false;
						gameObject.layer = Globals.LayerNumWorldItemActive;
				}

				protected void OnSetEquippedMode()
				{
						if (HUD != null)
								HUD.Retire();

						if (rb != null) {
								rb.isKinematic = true;
						}

						ParentPivot = null;
						//put it somewhere well out of the way with a random offset
						rb.position = Vector3.one * 4000f;
						ActiveStateLocked = false;
						ActiveState = WIActiveState.Active;
						ActiveStateLocked = true;
				}

				protected void OnSetHiddenMode()
				{
						if (HUD != null)
								HUD.Retire();

						ParentPivot = null;
						//if we're going into hiding mode then we're not in the stack any more
						OnRemoveFromStack.SafeInvoke();
						//set it to null because we don't need it any more
						OnRemoveFromStack = null;

						if (rb != null) {
								rb.isKinematic = true;
						}

						ActiveStateLocked = false;
						ActiveState = WIActiveState.Invisible;
						ActiveStateLocked = true;
				}

				protected void OnSetDestroyedMode()
				{
						//TODO remove this mode it's no longer necessary
				}

				protected void OnFinishedUnloading()
				{
						if (HUD != null)
								HUD.Retire();

						ActiveState = WIActiveState.Invisible;
						//this will create a save state to be saved to disk over time
						WorldItems.Get.Save(this, true);//TODO make this NOT true
						//unloaded just means unloaded, not actually removed
						//so don't tell our stack that we've been removed
						//but do tell the group!
						Group.UnloadChildItem(this);
						//this will call OnRemovedFromGroup
						OnUnloaded.SafeInvoke();
						//now scripts will start unloading their stuff
						var enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext()) {
								//foreach (WIScript script in mScripts.Values) {
								enumerator.Current.enabled = false;
						}
						StopAllCoroutines();
				}

				protected IEnumerator OnSetRemovedMode()
				{
						try {
								//if we're going into world mode then we're not in the stack any more
								OnRemoveFromStack.SafeInvoke();
								//tell our group that we're permanently gone
								Group.RemoveChildItemFromGroup(this);
								//this will call OnRemovedFromGroup
								OnRemovedFromGame.SafeInvoke();
								//make sure to save the state so we know it's removed!
								PrepareToUnload();
						} catch (Exception e) {
								Debug.LogError("Exception while removing object: " + e.ToString());
						}

						while (!ReadyToUnload) {
								yield return null;
						}

						BeginUnload();

						while (!FinishedUnloading) {
								yield return null;
						}

						WorldItems.Unload(this);
				}

				#endregion

				#region destroy

				public void OnDestroy()
				{
						mDestroyed = true;
						OnRemoveFromStack = null;
						OnRemoveFromGroup = null;
						OnAddedToGroup = null;
						OnGroupLoaded = null;
						OnGroupUnloaded = null;
						OnInvisible = null;
						OnVisible = null;
						OnActive = null;
						OnStateChange = null;
				}

				protected bool mDestroyed = false;

				#endregion

				#if UNITY_EDITOR
				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.Button("Active state: " + worlditem.ActiveState.ToString());
						GUILayout.Button("Mode: " + worlditem.Mode.ToString());
						GUILayout.Button("Load state: " + worlditem.LoadState.ToString());
						if (worlditem.Is(WILoadState.Initialized)) {
								if (worlditem.IsStackContainer) {
										UnityEngine.GUI.color = Color.green;
										string contents = "Contents: ";
										foreach (WIStack stack in worlditem.StackContainer.StackList) {
												if (stack.HasTopItem) {
														contents += stack.TopItem.DisplayName + ", ";
												}
										}
										GUILayout.Button("Is stack container - " + contents);
								} else {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Button("Is NOT stack container");
								}
						}
						UnityEngine.GUI.color = Color.yellow;
						foreach (KeyValuePair <Type,WIScript> script in mScripts) {
								if (script.Value != null) {
										GUILayout.Button(script.Key.Name);
								}
						}

						if (Application.isEditor && !Application.isPlaying && HasSaveState) {
								foreach (KeyValuePair <string,string> scriptState in SaveState.Scripts) {
										GUILayout.Button(scriptState.Key);
								}
						}

				}
				#endif
		}
}