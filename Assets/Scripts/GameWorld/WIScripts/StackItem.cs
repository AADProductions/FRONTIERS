using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	[Serializable]
	public class StackItem : Mod, IWIBase
	{
		public StackItem ()
		{
			if (Props == null) {
				Props = new WIProps ();
			}
			if (SaveState == null) {
				SaveState = new WISaveState ();
			}
		}

		public bool IsEmpty {
			get {
				return Props == null || string.IsNullOrEmpty (Props.Name.PackName);
			}
		}
		//convenience
		[XmlIgnore]
		public STransform Transform { 
			get {
				return Props.Local.Transform;
			}
			set {
				Props.Local.Transform.CopyFrom (value);
			}
		}

		#region IWIBase implementation

		public WorldItem worlditem { get { return null; } }

		public bool Is (WIMode mode)
		{
			return Flags.Check ((uint)Props.Local.Mode, (uint)mode, Flags.CheckType.MatchAny);
		}

		public bool IsWorldItem { get { return false; } }

		public string StackName {
			get {
				return Props.Name.StackName;
			}
			set {
				Props.Name.StackName = value;
			}
		}

		public string PrefabName { get { return Props.Name.PrefabName; } }

		public string PackName { get { return Props.Name.PackName; } }

		public string DisplayName {
			get {
				if (string.IsNullOrEmpty (Props.Name.DisplayName)) {
					return StackName;
				}
				return Props.Name.DisplayName;
			}
			set {
				Props.Name.DisplayName = value;
			}
		}

		public bool HasStates { get { return SaveState.HasStates; } }

		public string State { get { return SaveState.LastState; } set { SaveState.LastState = value; } }

		public string FileName { get { return Props.Name.FileName; } }

		public string QuestName { get { return Props.Name.QuestName; } }

		public bool IsQuestItem { get { return Is <QuestItem> (); } }

		public string Subcategory { get { return Props.Local.Subcategory; } }

		public WIMode Mode {
			get {
				if (Props == null) {
					return WIMode.RemovedFromGame;
				} else {
					return Props.Local.Mode; 
				}
			}
		}

		public WISize Size { get { return GlobalProps.Flags.Size; } }

		public WICurrencyType CurrencyType { get { return GlobalProps.CurrencyType; } }

		public float BaseCurrencyValue { 
			get { 
				if (!Is<Currency> ()) {
					Props.Local.BaseCurrencyValue = CalculateLocalBaseCurrencyValue ();
				}
				return Props.Local.BaseCurrencyValue;
			}
		}

		public bool CanEnterInventory { get { return SaveState.CanEnterInventory; } }

		public bool CanBeCarried { get { return SaveState.CanBeCarried; } }

		public bool UnloadWhenStacked { get { return SaveState.UnloadWhenStacked && !Is <Container> (); } }

		public bool IsGeneric { get { return false; } }

		public SVector3 ChunkPosition { get { return Props.Local.ChunkPosition; } set { Props.Local.ChunkPosition = value; } }

		public void Clear ()
		{
			//don't bother to clear global props
			//we're not garbage collecting that anyway
			//mGlobalProps = null;
			mGroup = null;
			OnRemoveFromStack = null;
			OnRemoveFromGroup = null;
		}

		public bool UseRemoveItemSkill (HashSet <string> removeItemSkillNames, ref IStackOwner useTarget)
		{
			//first get our local remove item skills
			//this list is populatd by WIScripts
			List <string> localRemoveItemSkills = RemoveItemSkills;
			for (int i = 0; i < localRemoveItemSkills.Count; i++) {
				removeItemSkillNames.Add (localRemoveItemSkills [i]);
			}
			//then get our owner remove item skills
			//this is populated by the owner's WIScripts
			useTarget = null;
			IStackOwner groupOwner = null;
			if (Group.HasOwner (out groupOwner)) {
				useTarget = groupOwner;
				//make sure that we're not the owner of this group to prevent a loop
				if (useTarget.IsWorldItem && useTarget != this) {
					useTarget.UseRemoveItemSkill (removeItemSkillNames, ref useTarget);
				}
			}
			return removeItemSkillNames.Count > 0;
		}

		public List <string> RemoveItemSkills { get { return Props.Local.RemoveItemSkills; } }

		[XmlIgnore]
		public WIGroup Group {
			get {
				if (mGroup != null) {
					return mGroup;
				}
				return WIGroups.Get.World;
			}
			set {
				if (mGroup != value) {
					OnRemoveFromGroup.SafeInvoke ();
				}
				mGroup = value;
			}
		}

		[XmlIgnore]
		public Action OnRemoveFromStack	{ get; set; }

		[XmlIgnore]
		public Action OnRemoveFromGroup	{ get; set; }

		public bool CheckVisibility (Vector3 actorPosition)
		{
			return true;
		}

		public StackItem GetDuplicate (bool duplicateContainer)
		{
			StackItem stackItem = new StackItem ();
			stackItem.Props.CopyGlobalNames (Props);
			stackItem.Props.CopyLocal (Props);
			stackItem.Props.CopyLocalNames (Props);
			//we might have to continue using object clone in this case
			//look into alternatives
			stackItem.SaveState = ObjectClone.Clone <WISaveState> (SaveState);
			//if (duplicateContainer) {
			//	if (mStackContainer != null) {
			//		WIStackContainer newContainer = ObjectClone.Clone <WIStackContainer> (mStackContainer);
			//		newContainer.Owner = stackItem;
			//	}
			//}
			return stackItem;
		}

		#endregion

		#region basic props

		public SIProps Props = new SIProps ();
		public WISaveState SaveState = null;
		public MobileReference StaticReference = null;
		//these props are loaded on startup based on the name
		[XmlIgnore]
		public WIGlobalProps GlobalProps {
			get {
				if (mGlobalProps == null) {
					mGlobalProps = WorldItems.Get.GlobalPropsFromName (Props.Name.PackName, Props.Name.PrefabName);
				}
				return mGlobalProps;
			}
			set {
				mGlobalProps = value;
			}
		}

		protected WIGlobalProps mGlobalProps = null;

		#endregion

		#region script props

		public bool Is <T> () where T : WIScript
		{
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			return SaveState.Scripts.ContainsKey (typeof(T).Name);
		}

		public bool Has <T> () where T : WIScript
		{
			return Is <T> ();
		}

		public bool HasAtLeastOne (List <string> scriptNames)
		{
			for (int i = 0; i < scriptNames.Count; i++) {
				if (Is (scriptNames [i])) {
					return true;
				}
			}
			return false;
		}

		public bool Is (string scriptName)
		{
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			return SaveState.Scripts.ContainsKey (scriptName);
		}

		public void Add (string scriptName)
		{
			if (SaveState == null || SaveState.Scripts == null) {
				return;
			}
			//this will add a stateless script to the script states
			//this is kind of dangerous because we can't guarantee that it'll be a WIScript
			//TODO find a way to verify that it's a WIScript!
			if (!SaveState.Scripts.ContainsKey (scriptName)) {
				SaveState.Scripts.Add (scriptName, string.Empty);
			}
		}

		public bool GetStateData <T> (out T stateData) where T : class, new()
		{
			stateData = null;
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			string stateName = typeof(T).Name;
			string scriptName = ReplaceLastOccurrence (stateName, "State", "");
			string scriptStateString = string.Empty;
			//okay, here we're removing State from the state name type to get the script name
			//this is probably going to get people in shit tons of trouble but whatever
			if (SaveState.Scripts.TryGetValue (scriptName, out scriptStateString)) {	//alright we've got the data, now it's time to pray we can deserialize it
				stateData = WIScript.XmlDeserializeFromString <T> (scriptStateString);
				//TODO wrap this in try/catch?
				return true;
			}
			return false;
		}

		public bool GetStateOf <T> (out object stateData) where T: WIScript
		{
			stateData = null;
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			string scriptName = typeof(T).Name;
			string scriptStateName = typeof(T).FullName + "State";//<-this shit is going to get people in trouble, haha
			string scriptStateString = string.Empty;
			if (SaveState.Scripts.TryGetValue (scriptName, out scriptStateString)) {
				stateData = WIScript.XmlDeserializeFromString (scriptStateString, scriptStateName);
				if (stateData != null) {
					return true;
				} else {
					Debug.Log ("Couldn't deserialize state data for " + scriptStateName);
				}
			} else {
				Debug.Log ("Couldn't get state data for " + scriptName + " in " + FileName);
			}
			return false;
		}

		public bool SetStateData <T> (T stateData) where T : class, new()
		{
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			string stateStateName = stateData.GetType ().Name;
			string scriptName = stateStateName.Substring (0, stateStateName.Length - 5);//this removes "State"
			string scriptStateString = string.Empty;
			if (!SaveState.Scripts.ContainsKey (scriptName)) {
				return false;
			} else {
				scriptStateString = WIScript.XmlSerializeToString (stateData);
				SaveState.Scripts [scriptName] = scriptStateString;
			}
			return true;
		}

		public bool SetStateOf <T> (object stateData) where T : WIScript
		{
			if (SaveState == null || SaveState.Scripts == null) {
				return false;
			}

			string scriptName = typeof(T).Name;
			string scriptStateName = scriptName + "State";//<-this shit is going to get people in trouble, haha
			string scriptStateString = string.Empty;
			//make sure the type matches the state type
			if (stateData.GetType ().Name != scriptStateName) {
				return false;
			}
			if (!SaveState.Scripts.ContainsKey (scriptName)) {
				return false;
			}

			if (SaveState.Scripts.ContainsKey (scriptName)) {
				scriptStateString = WIScript.XmlSerializeToString (stateData);
				SaveState.Scripts [scriptName] = scriptStateString;
				return true;
			}
			return false;
		}

		public StackItem GetStackItem (WIMode stackItemMode)
		{
			return this;
		}

		protected int CalculateLocalBaseCurrencyValue ()
		{
			int baseCurrencyValue = Mathf.CeilToInt (GlobalProps.BaseCurrencyValue);
			if (SaveState != null && SaveState.Scripts != null) {
				var enumerator = this.SaveState.Scripts.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					//Debug.Log("Checking script " + enumerator.Current.Key);
					Type scriptType = System.Type.GetType ("Frontiers.World.WIScripts." + enumerator.Current.Key);
					if (scriptType == null) {
						//Debug.Log("Couldn't get script type for name " + enumerator.Current.Key);
					} else {
						var calculateLocalPriceMethod = scriptType.GetMethod ("CalculateLocalPrice", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
						if (calculateLocalPriceMethod != null) {
							//Debug.Log("Got calcualate price method from " + enumerator.Current.Key);
							object stateData = null;
							string scriptStateName = scriptType.FullName + "State";//<-this shit is going to get people in trouble, haha
							string scriptStateString = string.Empty;
							stateData = WIScript.XmlDeserializeFromString (enumerator.Current.Value, scriptStateName);
							baseCurrencyValue = (int)calculateLocalPriceMethod.Invoke (null, new object [] {
								baseCurrencyValue,
								this
							});
						}
					}
				}
			} else {
				Debug.Log ("Didn't have states");
			}
			if (Props.Local.CraftedByPlayer) {
				baseCurrencyValue = Mathf.CeilToInt (baseCurrencyValue * Globals.BaseValueCraftingBonus);
			}
			//apply wealth level
			baseCurrencyValue = Mathf.Max (baseCurrencyValue, Mathf.CeilToInt (baseCurrencyValue * FlagSet.GetAverageValue (GlobalProps.Flags.Wealth) * 0.25f));
			return baseCurrencyValue;
		}

		#endregion

		public void RemoveFromGame ()
		{
			//Debug.Log ("Removing " + Props.Name.FileName + " from game");
			Props.Local.PreviousMode = Props.Local.Mode;
			Props.Local.Mode = WIMode.RemovedFromGame;
			OnRemoveFromGroup.SafeInvoke ();
			mGroup = null;
			OnRemoveFromGroup = null;
			#if UNITY_EDITOR
			if (OnRemoveFromStack == null) {
				Debug.Log ("OnRemoveFromStack was null");
			}
			#endif
			OnRemoveFromStack.SafeInvoke ();
			OnRemoveFromStack = null;
		}

		#region stack container props

		public int NumItems {
			get {
				if (IsStackContainer) {
					return StackContainer.NumItems;
				}
				return 0;
			}
		}

		public bool IsStackContainer {
			get {	//forget about mIsStackContainer
				//this will tell us if it has a container script attached
				//it may not have the actual stack container yet
				//but the presence of the script tells us what we need to know
				return Is <Container> ();
			}
		}

		public bool UseAsContainerInInventory {
			get {
				return IsStackContainer && Props.Local.UseAsContainerInInventory;
			}
		}

		public WIStackMode StackMode {
			get {
				if (IsStackContainer) {
					return StackContainer.Mode;
				}
				return WIStackMode.Generic;
			}
		}

		public WIStackContainer StackContainer {
			get {
				if (Application.isPlaying && mStackContainer == null) {	//okay it's null
					if (IsStackContainer) {	//but we ARE supposed to have one
						//that must mean we've never actually created it yet
						//so create it now
						//Debug.Log("Stack container was null in stack item " + Props.Name + ", creating one now");
						mStackContainer = Stacks.Create.StackContainer (this, this.Group);
					}
				}
				//if it's null and we're not supposed to be one
				//oh well whatever
				return mStackContainer;
			}
			set {	//if this is getting set it's most likely by a WorldItem trying to flatten itself
				//that means our current stack container should be null
				//so make sure we don't already have a stack container, and if we do throw an exception
				if (mStackContainer != null) {
					throw new System.InvalidOperationException ("Trying to set an mStackContainer when the original is not NULL in " + this.FileName);
					return;
				}
				//why are you trying to set the stack container value to null?
				//that should never happen either
				if (mStackContainer != null && mStackContainer.Owner == this && value == null) {
					throw new System.ArgumentNullException ("Trying to set an mStackContainer to NULL " + this.FileName);
				}
				//permissions will have already been set elsewhere
				//all that's left is to mark this StackItem as the owner
				mStackContainer = value;
				if (mStackContainer != null) {
					mStackContainer.Owner = this;
				}
			}
		}

		public void Refresh ()
		{
			//TODO
		}

		#endregion

		protected WIStackContainer mStackContainer = null;
		protected WIGroup mGroup = null;

		public static string ReplaceLastOccurrence (string Source, string Find, string Replace)
		{
			int Place = Source.LastIndexOf (Find);
			string result = Source.Remove (Place, Find.Length).Insert (Place, Replace);
			return result;
		}
	}
}