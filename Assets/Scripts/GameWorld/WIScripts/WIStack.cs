using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;

namespace Frontiers
{
		[Serializable]
		public class WIStack
		{
				public WIStack()
				{
						LockRefreshAction = false;
				}

				public virtual WIStackMode Mode {
						get {
								return mMode;
						}
						set {
								//TODO update state based on mode
								mMode = value;
						}
				}

				[XmlIgnoreAttribute]
				public virtual WIGroup Group {
						get {
								return mGroup;
						}
						set {
								//TODO set group on items (?)
								mGroup = value;
						}
				}

				[XmlIgnoreAttribute]
				public System.Action RefreshAction {
						get {
								return mRefreshAction;
						}
						set {
								//if (!LockRefreshAction) {
								mRefreshAction = value;
								//} else {
								//Debug.Log ("Tried to set refresh action in stack and can't");
								//}
						}
				}

				[XmlIgnoreAttribute]
				public System.Action DestroyedAction {
						get {
								return mDestroyedAction;
						}
						set {
								mDestroyedAction = value;
						}
				}

				[XmlIgnoreAttribute]
				[NonSerialized]
				public WIStackContainer Container;

				public bool SendCurrencyToBank {
						get {
								return Bank != null;
						}
				}

				[XmlIgnoreAttribute]
				[NonSerialized]
				public IBank Bank;

				[XmlIgnoreAttribute]
				public virtual IStackOwner Owner {
						get {
								if (BelongsToContainer) {
										return Container.Owner;
								}
								return null;
						}
						set {
								return;
						}
				}

				[XmlIgnoreAttribute]
				public virtual List <IWIBase> Items {
						get {
								if (mItems == null) {
										mItems = new List <IWIBase>();
								}
								return mItems;
						}
				}

				public string GroupPath {
						get {
								if (mGroup == null) {
										return WIGroups.Get.World.Path;
								}
								return mGroup.Path;
						}
						set {
								WIGroups.FindGroup(value, out mGroup);
								for (int i = 0; i < Items.Count; i++) {
										if (Items[i] != null) {
												Items[i].Group = mGroup;
										}
								}
						}
				}

				public virtual void Clear()
				{
						if (mItems != null) {
								mItems.Clear();
						}
						mItems = null;
				}

				public void SetItems(List <IWIBase> items, bool clearExisting)
				{
						if (mItems == null) {
								mItems = items;
								return;
						} else if (mItems.Count == 0) {
								mItems = null;
								mItems = items;
								return;
						} else if (clearExisting) {
								mItems.Clear();
								mItems = null;
						}
						mItems = items;

						if (Group == WIGroups.Get.Player) {
								for (int i = 0; i < mItems.Count; i++) {
										if (mItems[i].IsQuestItem) {
												Player.Local.Inventory.AddQuestItem(mItems[i].QuestName);
										}
								}
						}
				}
				//do not touch these xml attributes
				//saving and loading will explode
				[XmlArray("SerializableItems", IsNullable = true)]
				[XmlArrayItem("StackItem", Type = typeof(StackItem), IsNullable = true)]
				public virtual StackItem [] SerializableItems {
						get {
								if (mItems == null || mItems.Count == 0) {
										return null;
								}
								StackItem[] stackItemArray = new StackItem [Items.Count];
								for (int i = 0; i < Items.Count; i++) {
										stackItemArray[i] = mItems[i].GetStackItem(WIMode.None);
								}
								return stackItemArray;
						}
						set {
								if (value != null) {
										for (int i = 0; i < value.Length; i++) {
												StackItem stackItem = value[i];
												stackItem.Group = mGroup;
												Items.Add(value[i]);
										}
										Array.Clear(value, 0, value.Length);
								}
						}
				}

				public bool Disabled {
						get {
								return mDisabled;
						}
						set {
								if (mDisabled != value) {
										mDisabled = value;
										Refresh();
								}
						}
				}

				public WISize StackMaxSize = WISize.Huge;
				[XmlIgnoreAttribute]
				[NonSerialized]
				public bool LockRefreshAction = false;

				public virtual bool BelongsToContainer {
						get {
								return Container != null;
						}
				}

				public virtual bool HasOwner(out IStackOwner owner)
				{
						owner = null;
						if (BelongsToContainer) {
								return Container.HasOwner(out owner);
						}
						return false;
				}

				public virtual WISize Size {
						get {
								if (Mode == WIStackMode.Enabler) {
										return WISize.NoLimit;
								}
								IStackOwner owner = null;
								if (HasOwner(out owner)) {
										return owner.Size;
								}
								return StackMaxSize;
						}
				}

				public virtual IWIBase TopItem {
						get {
								return Items[Items.Count - 1];
						}
				}

				public virtual bool HasTopItem {
						get {
								return NumItems > 0;
						}
				}

				public virtual int NumItems {
						get {
								if (Disabled) {
										return 0;
								}
								return Items.Count;
						}
				}

				public virtual int SpaceLeft {
						get {
								return MaxItems - NumItems;
						}
				}

				public virtual int MaxItems {
						get {
								int maxItems = 1;

								if (Disabled) {
										maxItems = 0;
								} else {
										switch (Mode) {
												case WIStackMode.Enabler:
												case WIStackMode.Wearable:
												case WIStackMode.Receptacle:
														break;

												case WIStackMode.Generic:
														if (HasTopItem) {
																maxItems = WorldItems.MaxItemsFromSize(TopItem.Size);
														} else {
																//this will only be true until the top item is empty
																maxItems = WorldItems.MaxItemsFromSize(WISize.Tiny);
														}
														break;

												default:
														break;
										}
								}
								return maxItems;
						}
				}

				public virtual bool IsEmpty {
						get {
								return NumItems == 0;
						}
				}

				public virtual bool IsFull {
						get {
								return (NumItems >= MaxItems) || Disabled;
						}
				}

				public virtual void OnItemRemoved()
				{	//this will automatically result in a Refresh call
						Stacks.Clear.DestroyedOrMovedItems(this);
				}

				public virtual void OnItemAdded()
				{	//this will automatically result in a Refresh call
						Stacks.Clear.DestroyedOrMovedItems(this);
				}

				public virtual void OnDestroyed()
				{	//stacks will take care of clearing the item list
						//just tell your owner that you've been destroyed
						mDestroyedAction.SafeInvoke();
				}

				public virtual void Refresh()
				{
						mRefreshAction.SafeInvoke();
						if (BelongsToContainer) {
								Container.Refresh();
						}
				}

				[XmlIgnore]
				protected WIGroup mGroup;
				[XmlIgnore]
				protected System.Action mRefreshAction = null;
				[XmlIgnore]
				protected System.Action mDestroyedAction = null;
				protected WIStackMode mMode = WIStackMode.Generic;
				protected bool mDisabled = false;
				protected List <IWIBase> mItems;
		}
}