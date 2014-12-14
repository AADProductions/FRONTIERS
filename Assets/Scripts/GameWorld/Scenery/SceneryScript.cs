using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System;
using System.Reflection;

namespace Frontiers.World
{
		[Serializable]
		public class SceneryScript : MonoBehaviour
		{		//similar to WIScript & PlayerScript
				//but for chunk scenery
				public WorldChunk ParentChunk;
				public ChunkPrefabObject cfo;

				#region initialization

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
						mSceneryStateField	= mTrueType.GetField("State");

						if (mSceneryStateField != null) {
								//only save data if we have a member called 'State'
								mHasSceneryStateField = true;
						} else {
								mHasSceneryStateField = false;
						}
						mInitialized = true;
				}

				public Type ScriptType {
						get {
								return mScriptType;
						}
				}

				public string ScriptName {
						get {
								return mScriptName;
						}
				}

				public string SceneryState {
						get {
								string saveData = string.Empty;
								if (mHasSceneryStateField) {
										try {
												if (mSceneryStateField == null) {
														Debug.Log("Trigger state field is NULL");
												}
												saveData = XmlSerializeToString(mSceneryStateField.GetValue(this));
										} catch (Exception e) {
												Debug.LogError("triggerScript save data error, returning empty string! E:" + e.InnerException.ToString());
												return string.Empty;
										}
								}
								return saveData;
						}
				}

				public object StateObject {
						get {
								object stateObject = null;
								if (mHasSceneryStateField) {
										stateObject = mSceneryStateField.GetValue(this);
								}
								return stateObject;
						}
				}

				protected void InitializeFromState(WorldChunk parentChunk)
				{
						ParentChunk = parentChunk;

						OnInitialized();
				}

				#endregion

				protected virtual void OnInitialized()
				{
						return;
				}

				public virtual void OnPlayerEncounter()
				{
						return;
				}

				public virtual bool GetSceneryState(out string sceneryState)
				{
						sceneryState = SceneryState;
						return mHasSceneryStateField;
				}

				public virtual void UpdateSceneryState(string sceneryState, WorldChunk parentChunk)
				{
						//script props should have been updated by now
						if (mHasSceneryStateField && !string.IsNullOrEmpty(sceneryState)) {
								object sceneryStateValue = null;
								try {
										if (XmlDeserializeFromString(sceneryState, mSceneryStateField.FieldType, out sceneryStateValue)) {
												mSceneryStateField.SetValue(this, sceneryStateValue);
												//set the base state so we can access stuff like max times available
												mBaseState = sceneryStateValue as SceneryScriptState;
												//save the script name so we can load it properly later
												mBaseState.ScriptName = ScriptName;
										}
								} catch (Exception e) {
										Debug.LogError("Trigger init from save data error! E: " + e.ToString());
								}
						}

						InitializeFromState(parentChunk);
				}

				public void RefreshState()
				{
						CheckScriptProps();

						if (mHasSceneryStateField) {
								mBaseState = mSceneryStateField.GetValue(this) as SceneryScriptState;
						}
				}

				//TODO move this into GameData
				protected static string XmlSerializeToString(object objectInstance)
				{
						if (objectInstance == null) {
								Debug.Log("Object instance is null");
						}

						XmlSerializer serializer = null;
						try {
								serializer = new XmlSerializer(objectInstance.GetType());
						} catch (Exception e) {
								Debug.LogError("Caught inner exception in xmlserializetostring in scenery script: " + e.InnerException.ToString());
						}
						StringBuilder sb = new StringBuilder();

						using (TextWriter writer = new StringWriter(sb)) {
								serializer.Serialize(writer, objectInstance);
						}
						return sb.ToString();
				}

				protected static T XmlDeserializeFromString <T>(string objectData) where T : new()
				{
						object deserializedObject = new T();

						XmlDeserializeFromString(objectData, typeof(T), out deserializedObject);

						return (T)deserializedObject;
				}

				protected static bool XmlDeserializeFromString(string objectData, Type type, out object deserializedObject)
				{
						deserializedObject	= null;
						XmlSerializer serializer = null;

						try {
								serializer = new XmlSerializer(type);
						} catch (Exception e) {
								Debug.LogError(e.InnerException.ToString());
						}

						using (TextReader reader = new StringReader(objectData)) {
								deserializedObject = serializer.Deserialize(reader);
						}

						if (deserializedObject != null) {
								return true;
						}

						return false;
				}

				public static object XmlDeserializeFromString(string objectData, string typeName)
				{
						object deserializedObject = null;
						XmlDeserializeFromString(objectData, Type.GetType(typeName), out deserializedObject);
						return deserializedObject;
				}
				#if UNITY_EDITOR
				public virtual void OnEditorRefresh()
				{

				}
				#endif
				protected SceneryScriptState mBaseState = null;
				protected string mScriptName = string.Empty;
				protected Type mScriptType;
				protected Type mTrueType;
				protected bool mHasSceneryStateField = false;
				protected FieldInfo mSceneryStateField;
				protected bool mInitialized = false;
		}

		[Serializable]
		public class SceneryScriptState
		{
				public string ScriptName = "SceneryScript";
				[XmlIgnore]
				[NonSerialized]
				public SceneryScript sceneryScript;
		}
}