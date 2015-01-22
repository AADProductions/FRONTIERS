using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Frontiers.World;

namespace Frontiers {

	[Serializable]
	public class WIStackContainer : WIStack
	{
		public WIStackContainer ( )
		{
			mRefreshContainerAction = Refresh;
			SetStackList (mStackList);
		}
		
		public WIStackContainer (IStackOwner owner)
		{
			mOwner = owner;
			mRefreshContainerAction = Refresh;
		}
		
		public WIStackContainer GetDuplicate (IStackOwner owner)
		{
			return new WIStackContainer (owner);
		}	

		public override WIStackMode Mode {
			get {
				return mMode;
			}
			set {
				mMode = value;
				for (int i = 0; i < StackList.Count; i++) {
					StackList [i].Mode = mMode;
				}
			}
		}

		[XmlIgnoreAttribute]
		public override WIGroup Group {
			get {
				return mGroup;
			}
			set {
				if (mGroup != value) {
					mGroup = value;
					for (int i = 0; i < StackList.Count; i++) {
						StackList [i].Group = value;
					}
				}
			}
		}

		[XmlIgnoreAttribute]
		public override IStackOwner Owner
		{
			get {
				return mOwner;
			}
			set {
				//TODO set owner on items (?)
				mOwner = value;
			}
		}

		public string GroupPath {
			get {
				if (mGroup != null) {
					return mGroup.Path;
				}
				return null;
			}
			set {
				if (value != null) {
					WIGroup group = null;
					//TODO verify that this works
					if (WIGroups.FindGroup (value, out group)) {
						Group = group;
					}
				}
			}
		}

		public string OwnerPath {
			get {
				if (mOwner != null && mOwner.IsWorldItem) {
					return mOwner.worlditem.StaticReference.FullPath;
				}
				return null;
			}
			set {
				if (value != null) {
					//Debug.Log ("WISTACKCONTAINER: SETTING OWNER IN WISTACKCONTAINER");
				}
			}
		}

		public List <WIStack> StackList {
			get {
				return mStackList;
			}
			set {
				SetStackList (value);
			}
		}

		public string StackName
		{
			get {
				return mOwner.StackName;
			}
		}

		public override WISize Size
		{
			get {
				return Owner.Size;
			}
		}

		public override bool HasTopItem {
			get {
				if (StackList.Count > 0) {
					return StackList [0].HasTopItem;
				}
				return false;
			}
		}

		public bool HasEmptyStackList
		{
			get {
				for (int i = 0; i < StackList.Count; i++) {
					if (StackList [i].IsEmpty) {
						return true;
					}
				}
				return false;
			}
		}

		public override IWIBase TopItem
		{
			get {
				IWIBase firstItem = null;
				Stacks.Find.FirstItem (this, out firstItem);
				return firstItem;
			}
		}

		public int NumStacks
		{
			get {
				if (StackList != null) {
					if (mNumStacks >= 0) {
						return mNumStacks;
					}
					return StackList.Count;
				}
				return 0;
			}
		}

		public override int NumItems
		{
			get {
				int numItems = 0;
				foreach (WIStack stack in StackList) {
					numItems += stack.NumItems;
				}
				return numItems;
			}
		}

		public override bool HasOwner (out IStackOwner owner)
		{
			owner = mOwner;
			return mOwner != null;
		}

		public override void Refresh ( )
		{
			mRefreshAction.SafeInvoke ();
		}

		public void SetNumStacks (int numStacks)
		{	//this sets the mode for any stacks above this index to Disabled
			numStacks = Mathf.Clamp (numStacks, 1, Globals.MaxStacksPerContainer);
			for (int i = 0; i < StackList.Count; i++) {
				if (i < numStacks) {
					StackList [i].Disabled = false;
				} else {
					StackList [i].Disabled = true;
				}
			}
			mNumStacks = numStacks;
		}

		public void SetStackList (List <WIStack> stackList)
		{
			if (mStackList != null && stackList != mStackList) {
				mStackList = null;
			}
			mStackList = stackList;
			for (int i = 0; i < StackList.Count; i++) {
				WIStack stack = StackList [i];
				//tell the stacks to report to this object
				//other objects will have to subscribe to the container
				stack.RefreshAction += mRefreshContainerAction;
				stack.Container	= this;
				stack.Mode = mMode;
			}
		}

		public override void Clear ()
		{
			for (int i = 0; i < StackList.Count; i++) {
				StackList [i].Clear ();
			}
			StackList.Clear ();
		}

		protected List <WIStack> mStackList = new List <WIStack> ( );

		[NonSerialized]
		protected System.Action mRefreshContainerAction;
		[NonSerialized]
		protected IStackOwner mOwner;
		[NonSerialized]
		protected int mNumStacks = -1;
	}
}