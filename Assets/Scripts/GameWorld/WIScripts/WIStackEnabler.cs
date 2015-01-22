using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using System.Xml.Serialization;

namespace Frontiers
{
		//this stack is used by inventories to store stack lists
		//it has its own private stack that it never swaps
		//when this stack contains a WorldItem / StackItem that is a container
		//the class will report itself as Enabled and can deliver the container's stacks
		[Serializable]
		public class WIStackEnabler : IStackOwner
		{
				public WIStackEnabler()
				{
						mRefreshEnablerAction = Refresh;
				}
				//this stack is owned by the stack enabler
				//its mode is set to Enabler so it is never, ever swapped
				//swapping this stack will mean bad things
				public WIStack EnablerStack {
						get {
								return mEnablerStack;
						}
						set {
								if (value == null) {
										throw new ArgumentNullException("WISTACKENABLER: Can't assign mEnabler stack in " + DisplayName + " - value is null");
								} else if (mEnablerStack != null) {	//only set this if the stack is null
										//otherwise DO NOT SET IT, seriously people
										//do you want bugs? this is how you get bugs
										throw new InvalidOperationException("WISTACKENABLER: Can't assign mEnabler stack in " + DisplayName + " - already assigned");
								} else {
										//Debug.Log ("WISTACKENABLER: Setting enabler stack in WIStackEnabler");
										//everything's fine, nothing to see
										mEnablerStack = value;
										mEnablerStack.Mode = WIStackMode.Enabler;
								}
						}
				}

				[XmlIgnore]
				public WIGroup Group {
						set {
								if (HasEnablerStack) {
										EnablerStack.Group = value;
								}
						}
						get {
								if (HasEnablerStack) {
										return EnablerStack.Group;
								}
								//TODO try to choose a group intelligently
								return WIGroups.Get.World;
						}
				}

				public bool HasEnablerStack {
						get {
								return mEnablerStack != null;
						}
				}

				public bool HasEnablerTopItem {
						get {
								//raw containers are always enabled
								if (mUseRawContainer)
										return true;

								//this just tells us if we have a top item
								//doesn't tell us whether it's a container
								return HasEnablerStack && mEnablerStack.HasTopItem;
						}
				}

				public bool HasEnablerContainer {
						get {
								//raw containers are always enabled
								if (mUseRawContainer)
										return true;

								if (HasEnablerStack && mEnablerStack.HasTopItem) {
										//it may not actually have a container yet
										//but the script will tell us if it's supposed to
										return mEnablerStack.TopItem.IsStackContainer;
								}
								return false;
						}
				}

				public WIStackContainer EnablerContainer {
						get {
								//raw containers are always enabled
								if (mUseRawContainer) {
										return mRawContainer;
								}

								//leaving this one as unsafe, going to assume that
								//people have called HasEnablerContainer first
								return mEnablerStack.TopItem.StackContainer;
						}
						set {
								mRawContainer = value;
								if (mRawContainer != null) {
										mUseRawContainer = true;
								}
						}
				}

				public bool IsEnabled {
						get {
								if (mUseRawContainer) {
										//our stack contaier is always enabled
										return true;
								}
								//this will be the most-checked variable
								//if Enabled is true, it tells us that we have an enabler container
								//if it's false, it tells us that there's something wrong along the way
								if (HasEnablerContainer) {//if the stack list is null
										//or it just doesn't have any stacks (for some weird reason)
										//then we're not enabled
										return EnablerContainer.NumStacks > 0;
								}
								return false;
						}
				}

				public bool UseRawContainer {
			//TODO break this functionality out into a different class?
			//seems to cause problems
						get {
								return mUseRawContainer;
						}
						set {
								mUseRawContainer = value;
								if (mUseRawContainer) {
										if (mRawContainer == null) {
												mRawContainer = Stacks.Create.StackContainer(this, Group);
										}
								}
								//don't destroy the raw container
								//it won't be serialized anyway
						}
				}
				//Display is the GUI item that displays the stack enabler
				//TODO look into removing this link it just causes problems
				[XmlIgnore]
				public Frontiers.GUI.GUIObject Display;
				[XmlIgnore]
				public Action RefreshAction;

				public List <WIStack> EnablerStacks {
						get {
								//we want this to be error free
								//so return an empty list if we don't have an enabler
								if (mEnablerStacks == null) {
										mEnablerStacks = new List <WIStack>();
								} else {
										mEnablerStacks.Clear();
								}
								if (HasEnablerContainer) {	//if we do have one add range
										mEnablerStacks.AddRange(EnablerContainer.StackList);
								}
								return mEnablerStacks;
						}
				}

				protected List <WIStack> mEnablerStacks = null;

				public void Initialize()
				{
						if (mInitialized)
								return;

						mInitialized = true;
						mEnablerStack.RefreshAction += mRefreshEnablerAction;
						//now this can never be over-ridden by something else
						mEnablerStack.LockRefreshAction = true;
				}

				#region IStackOwner implementation

				//since we actually own the mEnablerStack we have to implement IStackOwner interface
				//all names derive from DisplayName - that will likely be set by whatever interface is using this class
				//i'm leaving open RemoveItemSkill just in case though why it would ever be used in this context is beyond me
				public string DisplayName {
						get {
								if (mUseRawContainer) {
										return "Container";
								}

								if (HasEnablerTopItem) {
										//pass along the top item's name
										return mEnablerStack.TopItem.DisplayName;
								} else {
										//show up as an empty thing
										return "(Empty)";
								}
						}
				}

				public WorldItem worlditem { get { return null; } }

				public bool IsWorldItem { get { return false; } }

				public string StackName { get { return DisplayName; } }

				public string FileName { get { return DisplayName; } }

				public string QuestName { get { return string.Empty; } }

				public bool UseRemoveItemSkill(HashSet <string> removeItemSkillNames, ref IStackOwner useTarget)
				{
						return false;
				}

				public List <string> RemoveItemSkills { get { return mRemoveItemSkills; } }

				public WISize Size { get { return WISize.NoLimit; } }

				public bool CheckVisibility(Vector3 actorPosition)
				{	//what the hell is this supposed to do again?
						return true;
				}

				public void Refresh()
				{
						RefreshAction.SafeInvoke();
				}

				#endregion

				[XmlIgnore]
				protected Action mRefreshEnablerAction;
				protected List <string> mRemoveItemSkills = new List <string>();
				protected bool mInitialized	= false;
				protected bool mUseRawContainer = false;
				protected WIStack mEnablerStack	= null;
				protected WIStackContainer mRawContainer = null;
		}
}