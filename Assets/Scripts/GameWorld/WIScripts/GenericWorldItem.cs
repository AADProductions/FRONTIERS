using UnityEngine;
using Frontiers;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Frontiers.World
{
		[Serializable]
		public class GenericWorldItem : IEqualityComparer <GenericWorldItem>, IEquatable <GenericWorldItem>
		{
				//the least possible amount of information that you need
				//to describe a worlditem
				//used for dopplegangers mostly
				public GenericWorldItem()
				{

				}

				[XmlIgnore]
				public bool HasFlags {
						get {
								return flags != null;
						}
				}

				[XmlIgnore]
				public WIFlags Flags {
						get {
								return flags;
						}
						set {
								flags = value;
						}
				}

				public string PackName = string.Empty;
				public string PrefabName = string.Empty;
				public string StackName = string.Empty;
				public string State = "Default";
				public string Subcategory = string.Empty;
				public string DisplayName = string.Empty;
				public int InstanceWeight = 1;
				public bool LimitInstances = false;
				public int MaxInstances = 0;

				public static bool IsNullOrEmpty(GenericWorldItem gwi)
				{
						return (gwi == null || string.IsNullOrEmpty(gwi.PackName) || string.IsNullOrEmpty(gwi.PrefabName));
				}

				public bool Equals(GenericWorldItem gwi1, GenericWorldItem gwi2)
				{
						if (gwi1 == null || gwi2 == null) {
								return false;
						}

						return  (string.Equals(gwi1.PackName, gwi2.PackName)
						&& string.Equals(gwi1.PrefabName, gwi2.PrefabName)
						&& string.Equals(gwi1.StackName, gwi2.StackName));
						//&& string.Equals (gwi1.Subcategory, gwi2.Subcategory)
						//&& string.Equals (gwi1.State, gwi2.State));
				}

				public int GetHashCode(GenericWorldItem a)
				{
						if (a == null) {
								return 0;
						}

						return a.PackName.GetHashCode()
						+ a.PrefabName.GetHashCode()
						+ a.StackName.GetHashCode();
						//+ a.State.GetHashCode ()
						//+ a.Subcategory.GetHashCode ();
				}

				public bool Equals(GenericWorldItem other)
				{
						if (other == null) {
								return false;
						}

						return string.Equals(PackName, other.PackName) && string.Equals(PrefabName, other.PrefabName);
				}

				public override string ToString()
				{
						if (string.IsNullOrEmpty(Subcategory) && string.IsNullOrEmpty(State)) {
								return string.Format("{0}|StackName={1}|DisplayName={2}", InstanceWeight, StackName, DisplayName);
						}

						if (string.IsNullOrEmpty(Subcategory)) {
								return string.Format("{0}|StackName={1}|DisplayName={2}|State={3}", InstanceWeight, StackName, DisplayName, State);
						}

						if (string.IsNullOrEmpty(State)) {
								return string.Format("{0}|StackName={1}|DisplayName={2}|Subcategory={3}", InstanceWeight, StackName, DisplayName, Subcategory);
						}

						return string.Format("{0}|StackName={1}|DisplayName={2}|Subcategory={3}|State={4}", InstanceWeight, StackName, DisplayName, Subcategory, State);
				}

				public GenericWorldItem(string packName, string prefabName)
				{
						PackName = packName;
						PrefabName = prefabName;
				}

				public GenericWorldItem(IWIBase item)
				{
						if (item == null) {
								Clear();
								return;
						}

						PackName = item.PackName;
						PrefabName = item.PrefabName;
						StackName = item.StackName;
						DisplayName = item.DisplayName;
						Subcategory = item.Subcategory;
						State = item.State;
				}

				public GenericWorldItem(WorldItem item)
				{
						if (item == null) {
								Clear();
								return;
						}

						PackName = item.Props.Name.PackName;
						PrefabName = item.Props.Name.PrefabName;
						StackName = item.Props.Name.StackName;
						DisplayName = item.Props.Name.DisplayName;
						Subcategory = item.Props.Local.Subcategory;
						State = item.State;
				}

				public StackItem ToStackItem()
				{
						StackItem stackItem = null;
						WorldItems.Get.StackItemFromGenericWorldItem(this, out stackItem);
						return stackItem;
				}

				[XmlIgnore]
				public bool IsEmpty {
						get {
								return (
								        string.IsNullOrEmpty(PackName)
								        || string.IsNullOrEmpty(PrefabName));
						}
						set {
								Clear();
						}
				}

				public void Clear()
				{
						PackName = string.Empty;
						PrefabName = string.Empty;
						StackName = string.Empty;
						State = string.Empty;
						Subcategory = string.Empty;
						DisplayName = string.Empty;
						InstanceWeight = 1;
						LimitInstances = false;
						MaxInstances = 0;
				}

				public void CopyFrom(WorldItem worldItem)
				{
						if (worldItem == null) {
								Clear();
								return;
						}

						PackName = worldItem.PackName;
						PrefabName = worldItem.PrefabName;
						StackName = worldItem.StackName;
						State = worldItem.State;
						Subcategory = worldItem.Subcategory;
						DisplayName = worldItem.DisplayName;
						TOD = WorldClock.TimeOfDayCurrent;
						TOY = WorldClock.TimeOfYearCurrent;
				}

				public void CopyFrom(StackItem stackItem)
				{
						if (stackItem == null) {
								Clear();
								return;
						}

						PackName = stackItem.PackName;
						PrefabName = stackItem.PrefabName;
						StackName = stackItem.StackName;
						State = stackItem.State;
						Subcategory = stackItem.Subcategory;
						DisplayName = stackItem.DisplayName;
						TOD = WorldClock.TimeOfDayCurrent;
						TOY = WorldClock.TimeOfYearCurrent;
				}

				public void CopyFrom(GenericWorldItem genericWorldItem)
				{
						if (genericWorldItem == null) {
								Clear();
								return;
						}

						PackName = genericWorldItem.PackName;
						PrefabName = genericWorldItem.PrefabName;
						StackName = genericWorldItem.StackName;
						State = genericWorldItem.State;
						Subcategory = genericWorldItem.Subcategory;
						DisplayName = genericWorldItem.DisplayName;
						InstanceWeight = genericWorldItem.InstanceWeight;
						LimitInstances = genericWorldItem.LimitInstances;
						MaxInstances = genericWorldItem.MaxInstances;
						TOD = genericWorldItem.TOD;
						TOY = genericWorldItem.TOY;
				}

				public void CopyFrom(IWIBase iwiBase)
				{
						if (iwiBase == null) {
								Clear();
						}

						if (iwiBase.IsWorldItem) {
								CopyFrom(iwiBase.worlditem);
						} else {
								CopyFrom(iwiBase.GetStackItem(WIMode.Stacked));
						}
				}

				public static GenericWorldItem Empty {
						get {
								//uuuuugh garbage
								return new GenericWorldItem();
						}
				}
				//these are to store temporal information
				//some objects are seasonal and change appearance based on time of day or year
				public TimeOfDay TOD = TimeOfDay.ff_All;
				public TimeOfYear TOY = TimeOfYear.AllYear;
				protected WIFlags flags = null;
		}
}