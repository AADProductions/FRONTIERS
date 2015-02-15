using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
	public partial class WorldItem
		{
				#region Script Management

				public WIScript	GetOrAdd(string scriptName)
				{
						WIScript wiScript = null;
						var enumerator = mScripts.GetEnumerator();
						while (enumerator.MoveNext ()) {
						//foreach (KeyValuePair <Type, WIScript> script in mScripts) {
								if (scriptName == enumerator.Current.Key.Name) {
										wiScript = enumerator.Current.Value;
										break;
								}
						}
						if (wiScript == null) {
								wiScript = gameObject.AddComponent(scriptName) as WIScript;
						}
						return wiScript;
				}

				public void	Add(string scriptName)
				{
						gameObject.AddComponent(scriptName);
				}

				public bool Add(string scriptName, out WIScript wiScript)
				{
						wiScript = gameObject.AddComponent(scriptName) as WIScript;
						return true;
				}

				public bool Get(string scriptName, out WIScript wiScript)
				{
						wiScript = null;
						bool result = false;
						var enumerator = mScripts.GetEnumerator();
						while (enumerator.MoveNext ()) {
						//foreach (KeyValuePair <Type, WIScript> script in mScripts) {
								if (scriptName == enumerator.Current.Key.Name) {
										wiScript = enumerator.Current.Value;
										result = true;
										break;
								}
						}
						return result;
				}

				public T GetOrAdd <T>() where T : WIScript
				{
						if (mDestroyed)
								return null;

						Type scriptType = typeof(T);
						WIScript worldItemScript = null;
						if (mScriptsAddedWhileInitializing != null) {
								for (int i = 0; i < mScriptsAddedWhileInitializing.Count; i++) {
										if (mScriptsAddedWhileInitializing[i].ScriptType == scriptType) {
												return mScriptsAddedWhileInitializing[i] as T;
										}
								}
						}

						if (!mScripts.TryGetValue(scriptType, out worldItemScript)) {
								T worldItemScriptAsT = gameObject.AddComponent <T>();
								//don't add the script - it will add itself on enable
								return worldItemScriptAsT;
						}
						return worldItemScript as T;
				}

				public bool AddScript(WIScript worldItemScript)
				{
						if (mScripts.ContainsKey(worldItemScript.ScriptType)) {
								//Debug.Log("Couldn't add script " + worldItemScript.ScriptName + ", already exists");
								return false;
						} else {
								if (mScriptsAddedWhileInitializing != null) {
										//if this isn't null then it means we're in the middle of initializing
										//this is actually a safer check than the load state since it's a sub-part of initializing
										mScriptsAddedWhileInitializing.SafeAdd(worldItemScript);
								} else {
										mScripts.Add(worldItemScript.ScriptType, worldItemScript);
										OnScriptAdded.SafeInvoke();
								}
								return true;
						}
				}

				public void RemoveScript(WIScript worldItemScript)
				{
						Type scriptType = worldItemScript.ScriptType;
						if (mScripts.TryGetValue(scriptType, out worldItemScript) == true) {
								mScripts.Remove(scriptType);
								GameObject.DestroyObject(worldItemScript, 0.1f);
						}
				}

				public void PopulateOptionsList(List <WIListOption> options, List <string> message, bool includeInteract)
				{
						if (CanBeCarried && !CanEnterInventory && Is(WIMode.Placed | WIMode.Frozen | WIMode.World)) {
								if (gCarryOption == null) {
										gCarryOption = new WIListOption("Carry", "Carry");
								}
								if (Player.Local.ItemPlacement.IsCarryingSomething) {
										gCarryOption.Disabled = true;
								} else {
										gCarryOption.Disabled = false;
								}
								options.Add(gCarryOption);
						} else {
								//Debug.Log("Can't carry item");
						}

						var enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (WIScript script in mScripts.Values) {
								enumerator.Current.PopulateOptionsList(options, message);
						}

						if (HasStates) {
								States.PopulateOptionsList(options, message);
						}
				}

				protected static WIListOption gCarryOption;

				public void OnPlayerUseWorldItemSecondary(object dialogResult)
				{
						WIListResult result = (WIListResult)dialogResult;

						switch (result.SecondaryResult) {
								case "Carry":
										//Debug.Log("Carrying item");
										Player.Local.ItemPlacement.ItemCarry(this, false);
										break;

								default:
										break;
						}
				}

				public List <string> ScriptNames {
						get {
								//we return a new list every time
								//this generates garbage but it's necessary for some initialization steps
								List <string> scriptNames = new List<string>();
								#if UNITY_EDITOR
								if (Application.isPlaying) {
								#endif
										var systemType = mScripts.Keys.GetEnumerator();
										while (systemType.MoveNext ()) {
												//foreach (System.Type type in mScripts.Keys) {
												scriptNames.Add(systemType.Current.Name);
										}
								#if UNITY_EDITOR
								} else {
										Component[] scripts = gameObject.GetComponents <WIScript>();
										foreach (WIScript script in scripts) {
												script.CheckScriptProps();
												scriptNames.Add(script.ScriptName);
										}
								}
								#endif
								return scriptNames;
						}
				}

				public T Get<T>() where T : WIScript
				{
						if (mDestroyed)
								return null;
						#if UNITY_EDITOR
						if (Application.isPlaying) {
						#endif
								try {
										return mScripts[typeof(T)] as T;
								} catch (Exception e) {
										//Debug.Log("trying to get " + typeof(T).ToString() + " in " + name + " resulted in NULL" + e.ToString());
										return null;
								}
						#if UNITY_EDITOR
						} else {
								return gameObject.GetComponent <T>();
						}
						#endif
				}

				public bool Has<T>() where T : WIScript
				{
						if (mDestroyed)
								return false;

						return Is <T>();
				}

				public bool Has<T>(out T script) where T : WIScript
				{
						if (mDestroyed) {
								script = null;
								return false;
						}

						if (Application.isPlaying) {
								return Is <T>(out script);
						} else {
								return gameObject.HasComponent<T>(out script);
						}
				}

				public bool HasAll(List <string> wiScriptTypes)
				{
						if (mDestroyed)
								return false;

						bool hasAll = true;
						for (int i = 0; i < wiScriptTypes.Count; i++) {
								if (!Is(wiScriptTypes[i])) {
										hasAll = false;
										break;
								}
						}
						return hasAll;
				}

				public bool HasAtLeastOne(List <string> wiScriptTypes)
				{
						if (mDestroyed)
								return false;

						if (wiScriptTypes.Count == 0) {
								return true;
						}

						bool hasAtLeastOne = false;
						for (int i = 0; i < wiScriptTypes.Count; i++) {
								if (Is(wiScriptTypes[i])) {
										hasAtLeastOne = true;
										break;
								}
						}
						return hasAtLeastOne;
				}

				public bool Has(string scriptName)
				{
						return Is(scriptName);
				}

				public bool Is<T>() where T : WIScript
				{
						if (mDestroyed)
								return false;

						if (Is(WILoadState.Initialized)) {
								return mScripts.ContainsKey(typeof(T));
						} else {
								return gameObject.HasComponent <T>();
						}
				}

				public bool Is(string scriptName)
				{
						if (mDestroyed)
								return false;

						bool isScript = false;
						var enumerator = mScripts.Keys.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (Type scriptType in mScripts.Keys) {
								if (enumerator.Current.Name == scriptName) {
										isScript = true;
										break;
								}
						}
						return isScript;
				}

				public bool	Is(string scriptName, out WIScript script)
				{
						bool isScript = false;
						script = null;
						var enumerator = mScripts.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (KeyValuePair <Type,WIScript> scriptPair in mScripts) {
								if (enumerator.Current.Key.Name == scriptName) {
										script = enumerator.Current.Value;
										isScript = true;
										break;
								}
						}
						return isScript;
				}

				public bool Is<T>(out T script) where T : WIScript
				{
						if (mDestroyed) {
								script = null;
								return false;
						}

						Type scriptType = typeof(T);
						WIScript worldItemScript = null;

						if (!Is(WILoadState.Initialized)) { //if we're not initialized we might have the component
								//but only as a regular component, not a WIScript
								return gameObject.HasComponent <T>(out script);
						} else if (mScripts.TryGetValue(scriptType, out worldItemScript) == true) {	//if we're initialized then it should be in our script lookup
								script = worldItemScript as T;
								return true;
						}

						script = null;
						return false;
				}

				public bool Is(Type scriptType)
				{
						return mScripts.ContainsKey(scriptType);
				}

				public bool GetStateOf<T>(out object stateObject) where T : WIScript
				{
						stateObject	= null;
						T wiScript = null;
						if (Is<T>(out wiScript)) {
								stateObject = wiScript.StateObject;
								return true;
						}
						return false;
				}

				public bool SetStateOf<T>(object stateObject) where T : WIScript
				{
						stateObject	= null;
						T wiScript = null;
						if (Is<T>(out wiScript)) {
								wiScript.StateObject = stateObject;
								return true;
						}
						return false;
				}
				//updates all its scripts from the stack item state
				//removes any scripts that are not in the stack item state
				//the state is cleared by this process
				public void ReceiveState(ref StackItem newState)
				{
						//Debug.Log("Receiving state in " + name);
						//lock the world item to prevent saving as we change the script states
						mLockSaveState = true;

						if (Props == null) {
								//we're not initialized yet but that's ok
								Props = new WIProps();
						}
						//before we do any script stuff, set the props
						//don't touch the global props because those are kept by the client's worlditem manager
						Props.CopyGlobalNames(newState.Props);
						Props.CopyLocal(newState.Props);
						Props.CopyLocalNames(newState.Props);
						name = Props.Name.FileName;

						//next copy the stack container
						mStackContainer = newState.StackContainer;
						//later on, make sure to update ownership
						//so the stack container has no idea that anything has happened

						#if UNITY_EDITOR
						if (Application.isEditor && !Application.isPlaying) {
								//we're doing it in the editor
								foreach (KeyValuePair <string,string> scriptState in newState.SaveState.Scripts) {
										//this will be removed eventually
										WIScript wiScript = gameObject.GetComponent(scriptState.Key) as WIScript;
										if (wiScript == null) {
												//if the world item doesn't have the script, add it
												wiScript = gameObject.AddComponent(scriptState.Key) as WIScript;
										}
										if (wiScript != null) {
												wiScript.CheckScriptProps();
												wiScript.SaveState = scriptState.Value;
										} else {
												Debug.LogError("Script " + scriptState.Key + " was NULL");
										}
								}
								State = newState.State;//this is kind of a kludge
								SaveState = newState.SaveState;

								//we're done!
								//unlock the world item
								mLockSaveState = false;
								return;
						}
						#endif

						if (Is(WILoadState.Initialized) || IsTemplate) {
				//Debug.Log("We're initialized, so set states now");
								//get the script names for possible removal later (before new scripts are added)
								//keep a list of newly added scripts to call OnInitialize
								List <string> scriptNames = ScriptNames;
								List <WIScript> newWiScripts = new List<WIScript>();
								//check all the script states in the new state
								var enumerator = newState.SaveState.Scripts.GetEnumerator();
								//foreach (KeyValuePair <string,string> scriptState in newState.SaveState.Scripts) {
								while (enumerator.MoveNext ()) {
										KeyValuePair <string,string> scriptState = enumerator.Current;
										WIScript wiScript = null;
										if (!Has(scriptState.Key)) {
												//if we don't have that script add it
												//then initialize it with the new state
												if (Add(scriptState.Key, out wiScript)) {
														wiScript.Initialize(scriptState.Value);
														newWiScripts.Add(wiScript);
												}
										} else {
												if (Get(scriptState.Key, out wiScript)) {
														//initialize the script with the new state
														//if the script is already initialized it will just be re-initialized
														//no need to call OnInitialize for these
														wiScript.Initialize(scriptState.Value);
												}
										}
								}

								//now check and see if there are any scripts we should remove
								for (int i = 0; i < scriptNames.Count; i++) {
										if (!newState.SaveState.Scripts.ContainsKey(scriptNames[i])) {
												//get the script and finish it
												//this will automatically remove it
												WIScript wiScript = null;
												if (Get(scriptNames[i], out wiScript)) {
														wiScript.Finish();
												}
										}
								}
								//next, call OnInitializedFirstTime
								for (int k = 0; k < newWiScripts.Count; k++) {
										newWiScripts[k].OnInitializedFirstTime();
								}
								//next, call OnInitialized for any scripts that were added
								for (int j = 0; j < newWiScripts.Count; j++) {
										newWiScripts[j].OnInitialized();
								}

								if (mStackContainer != null) {
										mStackContainer.Owner = this;
										mStackContainer.Group = Group;
										mStackContainer.Refresh();
								}
						} else {
								//we're not initialized or a template
								//just set our startup states and Initialize will take care of the rest
				//Debug.Log("Saving our state for later");
								SaveState = ObjectClone.Clone <WISaveState> (newState.SaveState);
						}
						//we're done!
						//unlock the world item
						mLockSaveState = false;
						//nuke the save state
						newState.Clear();
						newState = null;
				}

				protected Dictionary <Type, WIScript> mScripts = new Dictionary <Type, WIScript>();

				#endregion

				#region WIScript contributed values

				public bool UnloadWhenStacked {
						get {
								bool result = true;
								if (HasStates && !States.UnloadWhenStacked) {
										result = false;
								} else {
										var enumerator = mScripts.Values.GetEnumerator();
										while (enumerator.MoveNext ()) {
												//foreach (WIScript script in mScripts.Values) {
												if (!enumerator.Current.UnloadWhenStacked) {
														return false;
												}
										}
								}
								return true;
						}
				}

				public bool CanEnterInventory {
						get {
								if (mDestroyed
								    || Props.Global.Flags.Size == WISize.Huge
								    || Props.Global.Flags.Size == WISize.NoLimit
								    || Props.Global.Weight == ItemWeight.Unliftable) {
										return false;
								}

								if (HasStates && !States.CanEnterInventory) {
										return false;
								}

								var enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext ()) {
										//foreach (WIScript script in mScripts.Values) {
										if (!enumerator.Current.CanEnterInventory) {
												return false;
										}
								}
								return true;
						}
				}

				public virtual bool CanBeCarried {
						get {
								if (Props.Global.Weight == ItemWeight.Unliftable || Props.Global.Flags.Size == WISize.NoLimit) {
										return false;
								}

								if (HasStates && !States.CanBeCarried) {
										return false;
								}

								var enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext ()) {
										//foreach (WIScript script in mScripts.Values) {
										if (!enumerator.Current.CanBeCarried) {
												return false;
										}
								}
								return true;
						}
				}

				public virtual bool CanBeDropped {
						get {
								if (Size == WISize.Tiny) {
										return false;
								}

								if (HasStates && !States.CanBeDropped) {
										return false;
								}

								var enumerator = mScripts.Values.GetEnumerator();
								while (enumerator.MoveNext ()) {
										//foreach (WIScript script in mScripts.Values) {
										if (!enumerator.Current.CanBeDropped) {
												return false;
										}
								}
								return true;
						}
				}

				public bool IsStackContainer {
						get {
								if (Is <Container>()) {
										Props.Local.IsStackContainer = true;
								} else {
										Props.Local.IsStackContainer = false;
								}
								return Props.Local.IsStackContainer;
						}
				}

				public void Examine(List <WIExamineInfo> examineInfo)
				{
						if (!Props.Global.ExamineInfo.IsEmpty) {
								examineInfo.Add(Props.Global.ExamineInfo);
						}
						var enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (WIScript script in mScripts.Values) {
								enumerator.Current.PopulateExamineList(examineInfo);
						}
				}

				public bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						bool result = true;
						errorMessage = string.Empty;
						var enumerator = mScripts.Values.GetEnumerator();
						while (enumerator.MoveNext ()) {
								//foreach (WIScript script in mScripts.Values) {
								if (!enumerator.Current.CanBePlacedOn(targetObject, point, normal, ref errorMessage)) {
										return false;
								}
						}
						return true;
				}

				public virtual bool IsUsable {
						get {
								return Usable != null && Usable.IsAvailable;
						}
				}

				#endregion

				#region Initialization

				public bool HasSaveState {
						get {
								return SaveState != null && SaveState.Scripts != null;
						}
				}

				public virtual void Awake()
				{
						if (!Application.isPlaying) {
								return;
						}

						DontDestroyOnLoad(tr);

						mLoadState = WILoadState.Uninitialized;
						Usable = gameObject.GetComponent <WorldItemUsable>();
						NObject = gameObject.GetComponent <TNObject>();

						if (rb != null) {
								rb.useGravity = true;
								rb.interpolation = RigidbodyInterpolation.None;
								rb.isKinematic	= true;
								rb.inertiaTensor = Vector3.one;//necessary?
						}

						//these won't invoke actions
						//because we're not initialized
						SetActive(false);
						SetVisible(false);
				}

				public void InitializeTemplate()
				{
						if (!IsTemplate)// || !Application.isPlaying)
							return;

						if (tr == null) {
								Dynamic dynamic = null;
								tr = transform;
								if (gameObject.HasComponent <Dynamic>(out dynamic) && dynamic.DynamicPrefabBase != null) {
										tr = dynamic.DynamicPrefabBase.transform;
								}
						}

						if (rb == null) {
								rb = tr.rigidbody;
								if (rb == null) {
										rb = rigidbody;
								}
						}
						//gameObject.name = Props.Name.PrefabName;
						//transform.position = Vector3.zero;
						//kill all this stuff, let WIScripts add it
						SaveState = null;
						mStackContainer = null;
						Props.Local.IsStackContainer = false;

						mLastActiveState = WIActiveState.Invisible;
						mActiveState = WIActiveState.Invisible;

						Props.Local.Mode = WIMode.Frozen;
						Props.Local.PreviousMode = WIMode.Frozen;

						mFlags = null;

						try {
								worlditem.Renderers.Clear();
								worlditem.Colliders.Clear();

								if (gameObject.HasComponent <WIStates>(out States)) {
										States.InitializeTemplate();
								} else {
										//states handle colliders and renderers
										//if we don't have one, add them to the renderers array here
										if (worlditem.renderer != null) {
												worlditem.Renderers.Add(worlditem.renderer);
										} else {
												Renderer[] renderers = tr.GetComponentsInChildren <Renderer>(true);
												for (int i = 0; i < renderers.Length; i++) {
														if (renderers[i].particleSystem == null) {
																worlditem.Renderers.Add(renderers[i]);
														}
												}
										}
										if (worlditem.collider != null) {
												worlditem.Colliders.Add(worlditem.collider);
										} else {
												Collider[] colliders = tr.GetComponentsInChildren <Collider>(true);
												for (int i = 0; i < colliders.Length; i++) {
														worlditem.Colliders.Add(colliders[i]);
												}
										}
								}

								WIScript[] wiScripts = gameObject.GetComponents <WIScript>();
								foreach (WIScript script in wiScripts) {
										script.InitializeTemplate();
								}
								mTemplateStackitem = new StackItem();
								mTemplateStackitem.Props.CopyLocal(Props);
								mTemplateStackitem.Props.CopyName(Props);
								mTemplateStackitem.SaveState = GetSaveState(true);

								for (int i = Renderers.LastIndex(); i >= 0; i--) {
										if (Renderers[i] == null) {
												Renderers.RemoveAt(i);
										}
								}
								for (int i = Colliders.LastIndex(); i >= 0; i--) {
										if (Colliders[i] == null) {
												Colliders.RemoveAt(i);
										}
								}

								//calculate base object bounds from current renderers
								BaseObjectBounds = new Bounds();
								BaseObjectBounds.center = tr.position;
								BaseObjectBounds.size = Vector3.zero;
								if (Renderers.Count > 0) {
										for (int i = 0; i < Renderers.Count; i++) {
												BaseObjectBounds.Encapsulate(Renderers[i].bounds);
										}
								} else {
										BaseObjectBounds.size = Vector3.one;
								}
						} catch (Exception e) {
								Debug.LogError("Exception initializing " + name + ": " + e.ToString());
						}
						//now figure out the y offset
						//if the actual gameobject pivot is approximately near the base of the bounds
						//don't bother to create an offset
						//otherwise set it to the difference between the bounds center and position
						if (Mathf.Approximately(BaseObjectBounds.min.y, tr.position.y)) {
								BasePivotOffset.y = 0f;
						} else {
								BasePivotOffset.y = tr.position.y - BaseObjectBounds.min.y;
						}
				}

				List <WIScript> mScriptsAddedWhileInitializing = null;

				public void Initialize()
				{
						#if UNITY_EDITOR
						if (!Application.isPlaying) {
								//Debug.Log ("Application not playing, returning");
								return;
						}
						#endif

						if (!Is(WILoadState.Uninitialized)) {
								//Debug.Log("Already initialized in " + name + ", not initializing again");
								return;
						}

						LoadState = WILoadState.Initializing;

						try {
								bool checkSaveState	= HasSaveState;
								//start by adding scripts that aren't on the default prefab
								if (checkSaveState) {
										List <string> currentScriptNames = ScriptNames;
										var nameEnumerator = SaveState.Scripts.Keys.GetEnumerator ();
										while (nameEnumerator.MoveNext ()) {
										//foreach (string SaveStatecriptName in SaveState.Scripts.Keys) {
												if (!currentScriptNames.Contains(nameEnumerator.Current)) {//SaveStatecriptName)) {
														gameObject.AddComponent(nameEnumerator.Current);//SaveStatecriptName);
												}
										}
								}
								//use this to avoid freaking out our dictionary when calling OnInitialized
								List <WIScript> initializedScripts = new List <WIScript>();
								//create this to catch scripts added during this process
								mScriptsAddedWhileInitializing = new List <WIScript>();
								//scripts add themselves on Awake so at this point our
								//dictionary will have all of them
								//check for namers only if we've specified them
								bool checkDisplayNamer = (!string.IsNullOrEmpty(Props.Local.DisplayNamerScript));
								bool checkHUDTarget = (!string.IsNullOrEmpty(Props.Local.HudTargetScript));
								var enumerator = mScripts.Values.GetEnumerator ();
								while (enumerator.MoveNext ()) {
								//foreach (WIScript script in mScripts.Values) {
										WIScript script = enumerator.Current;
										if (checkSaveState) {
												string saveState = string.Empty;
												if (SaveState.Scripts.TryGetValue(script.ScriptName, out saveState)) {
														script.Initialize(saveState);
												}
										}
										if (checkDisplayNamer) {
												if (string.Equals(script.ScriptName, Props.Local.DisplayNamerScript)) {
														DisplayNamer = new WorldNameCleaner(script.DisplayNamer);
														checkDisplayNamer = false;
												}
										}
										if (checkHUDTarget) {
												if (string.Equals(script.ScriptName, Props.Local.HudTargetScript)) {
														HudTargeter = new HudTargetSupplier(script.HudTargeter);
														checkHUDTarget	= false;
												}
										}
										script.Initialize();
										script.OnStartup();
										initializedScripts.Add(script);
								}

								if (HasStates) {
										States.Initialize();
								}

								//at this point we're done with the save state
								//so clear it out for the gc
								if (SaveState != null) {
									if (SaveState.Scripts != null) {
										SaveState.Scripts.Clear ();
										SaveState.Scripts = null;
									}
									SaveState = null;
								}

								//now see if there are any scripts that we added during initialization
								for (int i = 0; i < mScriptsAddedWhileInitializing.Count; i++) {
										WIScript script = mScriptsAddedWhileInitializing[i];
										script.Initialize();
										script.OnStartup();
										//make sure to add it to the lookup
										mScripts.Add(script.ScriptType, script);
										initializedScripts.Add(script);
								}
								//clear the list
								//we will be using it for OnInitializedFirstTime in a moment
								if (mScriptsAddedWhileInitializing.Count > 0) {
										OnScriptAdded.SafeInvoke ();
								}
								mScriptsAddedWhileInitializing.Clear();

								Highlight = gameObject.GetOrAdd <PlayerFocusHighlight>();
								//HUD = gameObject.GetOrAdd <WIHud> ();
								//HUD.worlditem = this;
								//HUD.Use = true;

								//now call OnInitialized on all scripts
								//calling it here will ensure all other scripts
								//on the WorldItem are initialized with states
								if (!Props.Local.HasInitializedOnce) {
										//clear the stack container, we don't want it
										ClearStackContainer();
										ClearStackItem();
										Props.Local.HasInitializedOnce = true;
										for (int i = 0; i < initializedScripts.Count; i++) {
												initializedScripts[i].OnInitializedFirstTime();
										}
								}

								for (int i = 0; i < initializedScripts.Count; i++) {	//this will get called whether we've
										//called Initialize on the WIScript or not
										initializedScripts[i].OnInitialized();
								}

								//same drill here - initialize the scripts that have just been added
								for (int i = 0; i < mScriptsAddedWhileInitializing.Count; i++) {
										WIScript script = mScriptsAddedWhileInitializing[i];
										script.OnStartup();
										script.Initialize();
										//don't forget to add it to the lookup!
										try {
												mScripts.Add(script.ScriptType, script);
										} catch (Exception e) {
												Debug.LogError("Script " + script.ScriptName + " already exists in " + name + ": " + e.ToString());
										}
										script.OnInitializedFirstTime();
										script.OnInitialized();
								}
								if (mScriptsAddedWhileInitializing.Count > 0) {
										OnScriptAdded.SafeInvoke ();
								}

								RefreshNames(false);

								LoadState = WILoadState.Initialized;
								ActiveState = WIActiveState.Invisible;

								//clear and set to null, we're done with this
								mScriptsAddedWhileInitializing.Clear();
								mScriptsAddedWhileInitializing = null;

								//if we had script states, send to props position
								//note: this might result in weird behavior, keep an eye on it

								//finally now that we've been added to the group and moved to our final position
								//set the worlditem mode
								//this is to prevent rigidbody errors on parenting to group
								if (Props.Local.FreezeOnStartup) {
										SetMode(WIMode.Frozen);
								} else {
										SetMode(Props.Local.PreviousMode);
								}

								//make sure our stack container's owner is set properly
								//this will work with startup states as well
								if (mStackContainer != null) {
										mStackContainer.Owner = this;
										mStackContainer.Group = Group;
								}

								Group.AddChildItem(this);
								//this should immediately call OnAddedToGroup
								//in which case our position will be updated

								//this flag will ensure proper behavior in SendToGroupPosition
								mAddedToGroupOnce = true;

								OnStateChange += Refresh;
								OnModeChange += Refresh;
								OnPlayerEncounter += SetActive;
								//at this point we'll call on visible etc
						} catch (Exception e) {
								Debug.LogError(e);
								LoadState = WILoadState.Uninitialized;
						}
						WorldItems.OnWorldItemInitialized(this);
				}

				protected void RefreshNames(bool force)
				{
						if (!IsTemplate && Application.isPlaying || force) {
								//this is a little weird, but it basically
								//ensures that Props.Name gets set right
								//away in case we create any StackItems
								//from this WorldItem
						}
				}

				#endregion

				#region Load and Save

				public MobileReference StaticReference {
						get {
								if (mStaticReference == null) {
										mStaticReference = new MobileReference();
								}
								if (Group != null && (mStaticReference.GroupPath != Group.Props.PathName || mStaticReference.FileName != FileName)) {
										mStaticReference.GroupPath = Group.Props.PathName;
										mStaticReference.FileName = FileName;
										mStaticReference.Refresh();
								} else if (mStaticReference.FileName != FileName) {
										mStaticReference.GroupPath = string.Empty;
										mStaticReference.FileName = FileName;
										mStaticReference.Refresh();
								}
								return mStaticReference;
						}
				}

				protected MobileReference mStaticReference;

				public void	SendToGroupPosition()
				{
						//most dynamic objects will not update themselves
						if (Props.Global.ParentUnderGroup && !TransformLocked && Group != null) {
								tr.parent = Group.tr;
								//this is called by the group whenever we're added
								//but our transform is only valid if we're loading from a state
								//otherwise we want to update our props transform
								if (mAddedToGroupOnce) {
										Props.Local.Transform.Position = tr.localPosition;
										Props.Local.Transform.Rotation = tr.localRotation.eulerAngles;
								} else {
										//if we just received a state then use our props to update our position
										tr.localPosition = Props.Local.Transform.Position;
										tr.localRotation = Quaternion.Euler(Props.Local.Transform.Rotation);
										//sadly we can't use rb.position
										//(maybe I can use inverse transform?)
								}
						}
				}

				public StackItem GetTemplate()
				{
						if (IsTemplate && mTemplateStackitem == null) {
								mTemplateStackitem = new StackItem();
								//mTemplateStackitem.Props = new WIProps();
								mTemplateStackitem.Props.CopyLocal(Props);
								mTemplateStackitem.Props.CopyName(Props);
								mTemplateStackitem.SaveState = GetSaveState(true);
						} else if (mTemplateStackitem == null) {
								//Debug.Log("Not template so returning a null template");
						}
						return mTemplateStackitem;
				}

				public StackItem GetStackItem(WIMode stackItemMode)
				{
						StackItem newStackItem = null;
						WIMode worldItemMode = stackItemMode;

						if (IsTemplate) {
								newStackItem = GetTemplate().GetDuplicate(true);
								newStackItem.Name = Props.Name.PrefabName;
								#if UNITY_EDITOR
								if (Application.isPlaying) {
								#endif
										newStackItem.Group = Group;
								#if UNITY_EDITOR
								}
								#endif
						} else {
								//get the latest updated names before we copy them
								RefreshNames(true);
								//get the latest position in local props before we copy them
								RefreshTransform();

								newStackItem = new StackItem();
								//newStackItem.Props = new SIProps();
								newStackItem.Props.CopyLocal(Props);
								newStackItem.Props.CopyName(Props);
								newStackItem.GlobalProps = Props.Global;

								newStackItem.Name = Props.Name.FileName;
								newStackItem.SaveState = GetSaveState(false);
								newStackItem.StaticReference = StaticReference;

								#if UNITY_EDITOR
								if (Application.isPlaying) {
								#endif
										newStackItem.Group = Group;
								#if UNITY_EDITOR
								}
								#endif

								if (stackItemMode == WIMode.None) {
										newStackItem.Props.Local.Mode = WIMode.World;
								}
								if (stackItemMode == WIMode.Unloaded) {
										worldItemMode = WIMode.RemovedFromGame;
								}

								newStackItem.Props.Local.Mode = stackItemMode;

								if (IsStackContainer) {	//if we already have a container, pass it along
										//beware of exceptions!
										//try {
										newStackItem.StackContainer = mStackContainer;
										//} catch (Exception e) {	//aw snap
										//just proceed normally for now
										//	Debug.LogException (e);
										//}
								}
								SetMode(worldItemMode);
						}

						return newStackItem;
				}

				public WISaveState GetSaveState(bool asTemplate)
				{		//the 'guts' of a stack item
						//contains the last known values of can be carried, dropped etc
						//as well as a state for every WIScript
						WISaveState saveState = new WISaveState();
						if (Application.isPlaying && !asTemplate) {
								var enumerator = mScripts.Values.GetEnumerator ();
								while (enumerator.MoveNext ()) {
										try {
												saveState.Scripts.Add(enumerator.Current.ScriptName, enumerator.Current.SaveState);
										} catch (Exception e) {
												Debug.LogError("Exception in worlditem " + name + ":" + e.ToString());
										}
								}
								saveState.CanBeCarried = CanBeCarried;
								saveState.CanBeDropped = CanBeDropped;
								saveState.CanEnterInventory = CanEnterInventory;
								saveState.UnloadWhenStacked = UnloadWhenStacked;
								saveState.HasStates = HasStates;
								//if the worlditem is loaded then its state will be set
								//so use that
								if (Is(WILoadState.Initialized)) {
										saveState.LastState = State;
								} else if (HasSaveState) {
										//otherwise use whatever the state WILL be when it's initialized
										//assuming the worlditem has one
										saveState.LastState = SaveState.LastState;
								}
								//if we actually have a state make sure our last state is as accurate as possible
								if (HasStates && (string.IsNullOrEmpty(saveState.LastState) || saveState.LastState == "Default")) {
										saveState.LastState = States.DefaultState;
								}
								//and we're done!
						} else {
								//get the template version of the save state
								//this is used to store templates by the editor
								//slightly different & slightly less efficient
								saveState.CanBeCarried = CanBeCarried;
								saveState.CanBeDropped = CanBeDropped;
								saveState.CanEnterInventory = CanEnterInventory;
								saveState.UnloadWhenStacked = true;
								saveState.HasStates = HasStates;
								if (HasStates) {
										if ((!HasSaveState || string.IsNullOrEmpty(SaveState.LastState)) || SaveState.LastState == "Default") {
												saveState.LastState = State;
										} else if (HasSaveState) {
												saveState.LastState = SaveState.LastState;
										}
								}
								//if application isn't playing then scripts won't be loaded
								Component[] wiScripts = gameObject.GetComponents <WIScript>();
								foreach (WIScript script in wiScripts) {
										if (!script.IsFinished) {
												script.CheckScriptProps();
												try {
														saveState.Scripts.Add(script.ScriptName, script.SaveState);
												} catch (Exception e) {
														Debug.Log("Exception in worlditem " + name + ":" + e.ToString());
												}
												saveState.CanBeCarried &= script.CanBeCarried;
												saveState.CanBeDropped &= script.CanBeDropped;
												saveState.CanEnterInventory &= script.CanEnterInventory;
												saveState.UnloadWhenStacked &= script.UnloadWhenStacked;
										}
								}
								//set this worlditem's save state to the state we just created
								SaveState = saveState;
						}
						return saveState;
				}

				#if UNITY_EDITOR
				public void OnEditorRefresh()
				{
						Component[] wiScripts = gameObject.GetComponents <WIScript>();
						foreach (WIScript script in wiScripts) {
								script.OnEditorRefresh();
								UnityEditor.EditorUtility.SetDirty(script);
						}
						if (name != Props.Name.PrefabName && name != Props.Global.FileNameBase) {
								string[] splitName = name.Split(new string [] { "-" }, StringSplitOptions.RemoveEmptyEntries);
								Props.Name.FileName = splitName[0];
						}
						UnityEditor.EditorUtility.SetDirty(this);
						UnityEditor.EditorUtility.SetDirty(gameObject);
				}

				public void OnEditorLoad()
				{
						Component[] wiScripts = gameObject.GetComponents <WIScript>();
						foreach (WIScript script in wiScripts) {
								script.OnEditorLoad();
						}
				}
				#endif
				protected bool mLockSaveState = false;
				protected bool mAddedToGroupOnce = false;

				#endregion

		}
}