using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Frontiers.World.WIScripts
{
		public class WIScript : MonoBehaviour, IUnloadable
		{
				public WorldItem worlditem {
						get {
								return mWorldItem;
						}
				}

				public string StackName {
						get {
								return worlditem.StackName;
						}
				}

				public bool IsDestroyed { get { return mDestroyed; } }

				public virtual bool EnableAutomatically { get { return false; } }

				#region script info / state

				public Type ScriptType {
						get {
								return mScriptType;
						}
				}

				public Type TrueType {
						get {
								return mTrueType;
						}
				}

				public string ScriptName {
						get {
								return mScriptName;
						}
				}

				public string SaveState {
						get {
								string saveData = string.Empty;
								if (mHasSaveStateField) {
										try {
												saveData = XmlSerializeToString(mSaveStateField.GetValue(this));
										} catch (Exception e) {
												Debug.LogError("WIScript save data error in " + mScriptName + " - E:" + e.ToString());
												return string.Empty;
										}
								}
								return saveData;
						}
						set {
								if (mHasSaveStateField && !string.IsNullOrEmpty(value)) {
										System.Object stateObject = null;
										if (XmlDeserializeFromString(value, mSaveStateField.FieldType, out stateObject)) {
												mSaveStateField.SetValue(this, stateObject);
										} else {
												Debug.LogError("Couldn't set save state in " + mScriptName);
										}
								}
						}
				}

				public object StateObject {
						get {
								object stateObject = null;
								if (mHasSaveStateField) {
										stateObject = mSaveStateField.GetValue(this);
								}
								return stateObject;
						}
						set {	//setting the state object is a big deal
								//it wipes the existing object
								if (mHasSaveStateField) {
										try {
												mSaveStateField.SetValue(this, value);
										} catch (Exception e) {
												Debug.LogError("Attempted to set state object in WIScript: " + e.ToString());
										}
								}
						}
				}

				public virtual bool Initialized {
						get {
								return mInitialized;
						}
				}

				public void Initialize()
				{
						//used when there is no save state
						if (!EnableAutomatically) {
								enabled = false;
						}
						mInitialized = true;
				}

				public void Initialize(string saveState)
				{
						if (mInitialized) {
								Reinitialize(saveState);
								return;
						}

						mInitialized = true;

						if (mHasSaveStateField && !string.IsNullOrEmpty(saveState)) {
								object saveStateValue = null;
								try {//TODO get rid of try/catch somehow
										if (XmlDeserializeFromString(saveState, mSaveStateField.FieldType, out saveStateValue)) {
												mSaveStateField.SetValue(this, saveStateValue);
										}
								} catch (Exception e) {
										Debug.LogError("WIScript init from save data error! E: " + e.ToString());
								}
						}

						worlditem.OnStateChange += OnStateChange;
						worlditem.OnModeChange += OnModeChange;
				}
				//this sets the state of the script without doing anything else
				//it assumes that it's already in the world and doesn't need to set itself up
				protected void Reinitialize(string saveState)
				{
						if (mHasSaveStateField && !string.IsNullOrEmpty(saveState)) {
								object saveStateValue = null;
								try {//TODO get rid of try/catch somehow
										if (XmlDeserializeFromString(saveState, mSaveStateField.FieldType, out saveStateValue)) {
												mSaveStateField.SetValue(this, saveStateValue);
										}
								} catch (Exception e) {
										Debug.LogError("WIScript init from save data error! E: " + e.ToString());
								}
						}
				}

				public virtual void OnStartup()
				{

				}

				public virtual void OnInitialized()
				{

				}

				public virtual void OnInitializedFirstTime()
				{
			
				}

				public virtual bool GetSaveState(out string saveState)
				{
						saveState = SaveState;
						return mHasSaveStateField;
				}

				#endregion

				#region load / unload

				public bool IsFinished {
						get {
								return mFinished;
						}
				}

				public virtual bool PrepareToUnload()
				{
						return true;
				}

				public virtual bool ReadyToUnload {
						get {
								return true;
						}
				}

				public virtual void BeginUnload()
				{

				}
				//this request must not halt any process that's necessary to unload the worlditem
				//its purpose is to veto the worlditem's attempt to cancel unload
				public virtual bool TryToCancelUnload()
				{
						return true;
				}

				public virtual bool FinishedUnloading {
						get {
								return true;
						}
				}

				public virtual bool SaveItemOnUnloaded {
						get {
								return true;
						}
				}

				#endregion

				#region naming, HUD & prices

				public virtual string FileNamer(int increment)
				{
						return gameObject.name + "-" + increment;
				}

				public virtual string DisplayNamer(int increment)
				{
						return gameObject.name;
				}

				public virtual string StackNamer(int increment)
				{
						return gameObject.name;
				}

				public virtual Transform HudTargeter()
				{
						return transform;
				}

				public virtual int OnRefreshHud(int lastHudPriority)
				{
						return lastHudPriority;
				}

				public virtual bool UsesHud {
						get {
								return false;
						}
				}

				public int LocalPrice(int basePrice)
				{
						if (mLocalPriceCalculator != null) {
								System.Object stateData = null;
								if (mHasSaveStateField) {
										stateData = mSaveStateField.GetValue(this);
								}
								return mLocalPriceCalculator(basePrice, worlditem);
						}
						return basePrice;
				}

				public int GlobalPrice(int basePrice)
				{
						if (mGlobalPriceCalculator != null) {
								return mGlobalPriceCalculator(basePrice);
						}
						return basePrice;
				}

				#endregion

				#region interaction and placement

				public virtual bool AutoIncrementFileName {
						get {
								return true;
						}
				}

				public virtual string GenerateUniqueFileName(int increment)
				{
						return worlditem.Props.Name.FileName;
				}

				public virtual bool CanBeDropped {
						get {
								return true;
						}
				}

				public virtual bool CanBeCarried {
						get {
								return true;
						}
				}

				public virtual bool CanEnterInventory {
						get {
								return true;
						}
				}

				public virtual bool UnloadWhenStacked {
						get {
								return true;
						}
				}

				public virtual bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						return true;
				}

				#endregion

				public virtual void Awake()
				{
						CheckScriptProps();
				}

				public void CheckScriptProps()
				{
						if (mScriptType == null) {
								mScriptType = GetType();
						}

						mTrueType = GetType();
						mScriptName = mTrueType.Name;
						mSaveStateField	= mTrueType.GetField("State");

						if (mSaveStateField != null) {	//only save data if we have a member called 'State'
								mHasSaveStateField = true;
						}

						if (mLocalPriceCalculator == null) {
								var staticPriceCalculator = mTrueType.GetMethod("CalculateLocalPrice", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
								if (staticPriceCalculator != null) {
										Debug.Log("Created local price calculator delegate in " + mTrueType.Name);
										mLocalPriceCalculator = (LocalPriceCalculator)Delegate.CreateDelegate(typeof(LocalPriceCalculator), staticPriceCalculator);
								}
						}
				}

				public virtual void OnEnable()
				{
						if (mInitialized) {
								//this will allow us to enabled / disable scripts
								//without destroying them
								return;
						}

						if (Application.isPlaying) {

								mWorldItem = gameObject.GetComponent <WorldItem>();
								if (worlditem == null) {
										Debug.Log("WORLDITEM WAS NULL IN " + name);
										return;
								}

								if (!worlditem.AddScript(this)) {	//only allow 1 type of each script to a worlditem
										GameObject.Destroy(this);
								}
				
								if (worlditem.Is(WILoadState.Initialized | WILoadState.Initializing)) {
										//if a worlditem is already initialized
										//(or it's in the process of initializing)
										//then we've been added to the
										//worlditem during gamplay - that means we don't wait for the worlditem
										//to initialize us / supply a saved sate - we initialize ourselves
										Initialize();
										OnInitialized();
										//and since it will be our first time for this script to be initialized
										//(even if the worlditem has been initialized before)
										//we call OnInitializedFirstTime as well
										OnInitializedFirstTime();
								}
						}
				}

				public virtual void InitializeTemplate()
				{
						CheckScriptProps();

						mWorldItem = gameObject.GetComponent <WorldItem>();


						if (mGlobalPriceCalculator == null) {
								var staticPriceCalculator = GetType().GetMethod("CalculateGlobalPrice", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
								if (staticPriceCalculator != null) {
										Debug.Log("Created global price calculator delegate in " + GetType().Name);
										mGlobalPriceCalculator = (GlobalPriceCalculator)Delegate.CreateDelegate(typeof(GlobalPriceCalculator), staticPriceCalculator);
								}
						}
				}

				public void Finish()
				{
						if (mFinished)
								return;

						if (worlditem != null) {
								OnFinish();
								worlditem.RemoveScript(this);
						}
						enabled = false;
						mFinished = true;
				}

				public virtual void OnModeChange()
				{

				}

				public virtual void OnStateChange()
				{

				}

				public virtual void OnFinish()
				{
						//final cleaning up goes in here
				}

				public void OnDestroy()
				{
						//the only thing we should be doing is calling Finish / OnFinish
						if (!mFinished) {
								Finish();
						}
						mDestroyed = true;
				}

				public virtual void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						return;
				}

				public virtual void PopulateExamineList(List <WIExamineInfo> examine)
				{
						return;
				}

				public virtual void PopulateRemoveItemSkills(HashSet <string> removeItemSkills)
				{
						return;
				}
				#if UNITY_EDITOR
				public virtual void OnEditorRefresh()
				{

				}

				public virtual void OnEditorLoad()
				{

				}
				#endif
				protected WorldItem mWorldItem;
				protected string mScriptName = string.Empty;
				protected Type mScriptType;
				protected Type mTrueType;
				protected bool mHasSaveStateField = false;
				protected FieldInfo mSaveStateField;
				protected LocalPriceCalculator mLocalPriceCalculator = null;
				protected GlobalPriceCalculator mGlobalPriceCalculator = null;
				protected bool mInitialized = false;
				protected bool mFinished = false;
				protected bool mDestroyed = false;

				public static string XmlSerializeToString(object objectInstance)
				{
						var serializer = new XmlSerializer(objectInstance.GetType());
						var sb = new StringBuilder();

						using (TextWriter writer = new StringWriter(sb)) {
								serializer.Serialize(writer, objectInstance);
						}
						return sb.ToString();
				}

				public static T XmlDeserializeFromString <T>(string objectData) where T : new()
				{
						object deserializedObject = new T();

						XmlDeserializeFromString(objectData, typeof(T), out deserializedObject);

						return (T)deserializedObject;
				}

				public static bool XmlDeserializeFromString(string objectData, Type type, out object deserializedObject)
				{
						deserializedObject	= null;
						var serializer = new XmlSerializer(type);

						using (TextReader reader = new StringReader(objectData)) {
								deserializedObject = serializer.Deserialize(reader);
						}

						if (deserializedObject != null) {
								return true;
						} else {
								//Debug.Log ("Deserialized object was null when attempting to deserialize " + objectData + " into type " + type.ToString ());
						}

						return false;
				}

				public static object XmlDeserializeFromString(string objectData, string typeName)
				{
						object deserializedObject = null;
						Type type = Type.GetType(typeName);
						if (type == null || string.IsNullOrEmpty(objectData)) {
								//Debug.Log ("Couldn't get type from typename " + typeName);
								return null;
						}

						if (!XmlDeserializeFromString(objectData, Type.GetType(typeName), out deserializedObject)) {
								//Debug.Log ("Couldn't deserialize from string for some reason");
						}

						return deserializedObject;
				}
		}
}