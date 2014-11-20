using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Frontiers
{
	public class PlayerScript : MonoBehaviour
	{
		public Type TrueType {
			get {
				return mTrueType;
			}
		}

		public LocalPlayer player;

		public virtual void Awake ()
		{	
			CheckScriptProps ();
		}

		public void CheckScriptProps ()
		{		
			if (mScriptType == null) {
				mScriptType = GetType ();
			}
			
			mTrueType = GetType ();
			mScriptName = mTrueType.Name;
			mPlayerStateField = mTrueType.GetField ("State");
			
			if (mPlayerStateField != null) {	//only save data if we have a member called 'State'
				mHasPlayerStateField = true;
			}
		}

		public virtual void Start ( )
		{
			enabled = false;
		}

		public void OnLocalPlayerCreated ( )
		{
			//Debug.Log ("Local player created in " + mTrueType.Name);
			player = Player.Local;
			player.Scripts.Add (this);
		}

		public virtual bool CanBeUnloaded {
			get {
				return true;
			}
		}

		public virtual bool LockQuickslots {
			get {
				return false;
			}
		}

		public virtual void AdjustPlayerMotor (ref float MotorAccelerationMultiplier, ref float MotorJumpForceMultiplier, ref float MotorSlopeAngleMultiplier)
		{
			//allows scripts to modify the player's movement, jumping and sliding motions
			return;
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

		public string PlayerState {
			get {
				string saveData = string.Empty;
				if (mHasPlayerStateField) {		
					try {
						saveData = XmlSerializeToString (mPlayerStateField.GetValue (this));
					} catch (Exception e) {
						Debug.LogWarning ("PlayerScript save data error in " + mScriptName + ", returning empty string! E:" + e.ToString ());
						return string.Empty;
					}
				}
				return saveData;
			}
		}

		public object StateObject {
			get {
				object stateObject = null;
				if (mHasPlayerStateField) {
					stateObject = mPlayerStateField.GetValue (this);
				}
				return stateObject;
			}
		}

		public virtual void Initialize ()
		{
			mInitialized = true;
		}

		public virtual bool SaveState (out string playerState)
		{
			playerState = PlayerState;
			return mHasPlayerStateField;
		}

		public void LoadState (string playerState)
		{
			//Debug.Log ("Calling update player state in player script");
			if (mHasPlayerStateField && !string.IsNullOrEmpty (playerState)) {
				object playerStateValue = null;
				try {
					if (XmlDeserializeFromString (playerState, mPlayerStateField.FieldType, out playerStateValue)) {
						mPlayerStateField.SetValue (this, playerStateValue);
					}
				} catch (Exception e) {
					Debug.LogError ("WIScript init from save data error! E: " + e.ToString ());
				}			
			}
			mStateLoaded = true;
			OnStateLoaded ();
		}

		public virtual void OnStateLoaded ()
		{

		}

		public virtual void OnGameLoadStart ()
		{

		}

		public virtual void OnGameLoadFinish ()
		{

		}

		public virtual void OnGameStartFirstTime ()
		{
			
		}

		public virtual void OnGameStart ()
		{

		}

		public virtual void OnGameUnload ()
		{
			enabled = false;
			StopAllCoroutines ();
		}

		public virtual void OnGamePause ()
		{

		}

		public virtual void OnGameContinue ()
		{

		}

		public virtual void OnLocalPlayerSpawn ()
		{

		}

		public virtual void OnLocalPlayerDespawn ()
		{

		}

		public virtual void OnLocalPlayerDie ()
		{

		}

		public virtual void OnRemotePlayerSpawn ()
		{

		}

		public virtual void OnRemotePlayerDie ()
		{

		}

		public virtual void Finish ()
		{
			player.Scripts.Remove (this);
		}

		protected string mScriptName = string.Empty;
		protected Type mScriptType;
		protected Type mTrueType;
		protected bool mHasPlayerStateField = false;
		protected FieldInfo mPlayerStateField;
		protected bool mInitialized = false;
		protected bool mStateLoaded = false;

		protected static string XmlSerializeToString (object objectInstance)
		{
			var serializer = new XmlSerializer (objectInstance.GetType ());
			var sb = new StringBuilder ();
		
			using (TextWriter writer = new StringWriter (sb)) {
				serializer.Serialize (writer, objectInstance);
			}		
			return sb.ToString ();
		}

		protected static T XmlDeserializeFromString <T> (string objectData) where T : new()
		{
			object deserializedObject = new T ();
		 	
			XmlDeserializeFromString (objectData, typeof(T), out deserializedObject);
			
			return (T)deserializedObject;
		}

		protected static bool XmlDeserializeFromString (string objectData, Type type, out object deserializedObject)
		{
			deserializedObject	= null;
			var serializer = new XmlSerializer (type);
		
			using (TextReader reader = new StringReader (objectData)) {
				deserializedObject = serializer.Deserialize (reader);
			}		
			
			if (deserializedObject != null) {
				return true;
			}
			
			return false;
		}

		public static object XmlDeserializeFromString (string objectData, string typeName)
		{
			object deserializedObject = null;
			XmlDeserializeFromString (objectData, Type.GetType (typeName), out deserializedObject);
			return deserializedObject;
		}
	}
}