using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;
using Frontiers.World;
using Frontiers.World.Gameplay;
using ExtensionMethods;
using Frontiers.Data;
using System.Text;
using System.Reflection;

namespace Frontiers.World
{
		[Serializable]
		public class WITemplate : Mod
		{
				public WITemplate()
				{
				}

				public WITemplate(WorldItem worlditem)
				{
						Name = worlditem.Props.Name.PrefabName;
						StackItem si = worlditem.GetTemplate();
						Props.CopyLocalNames(si.Props);
						Props.CopyGlobalNames(si.Props);
						Props.CopyLocal(si.Props);
						Props.Global = worlditem.Props.Global;
						SaveState = si.SaveState;
						Props.Local.Mode = WIMode.Unloaded;
						Props.Local.PreviousMode = WIMode.Unloaded;
						WIStates states = null;
						if (worlditem.gameObject.HasComponent <WIStates>(out states)) {
								ObjectStates.AddRange(states.States);
						}
				}

				public WIProps Props = new WIProps();
				public WISaveState SaveState = null;
				public List <WIState> ObjectStates = new List <WIState>();
		}

		[Serializable]
		public class WorldItemPackPaths : Mod
		{
				public string PackPath = string.Empty;
				public List <string> Prefabs = new List <string>();
				public List <string> Meshes = new List <string>();
				public List <string> Materials = new List <string>();
				public List <string> Textures = new List <string>();
		}

		[Serializable]
		public class WIExamineInfo
		{
				public WIExamineInfo()
				{
				}

				public WIExamineInfo(string staticExamineMessage)
				{
						StaticExamineMessage = staticExamineMessage;
				}

				public WIExamineInfo(string staticExamineMessage, float minimumExamineSkill)
				{
						MinimumExamineSkill = minimumExamineSkill;
						StaticExamineMessage = staticExamineMessage;
				}

				public WIExamineInfo(string staticExamineMessage, float minimumExamineSkill, string requiredSkill)
				{
						StaticExamineMessage = staticExamineMessage;
						MinimumExamineSkill = minimumExamineSkill;
						RequiredSkill = requiredSkill;
				}

				public bool IsEmpty {
						get {
								return string.IsNullOrEmpty(StaticExamineMessage);
						}
				}

				public string OverrideDescriptionName = string.Empty;
				public string StaticExamineMessage = string.Empty;
				public string ExamineMessageOnFail = string.Empty;
				public float MinimumExamineSkill = 0f;
				public string RequiredSkill = string.Empty;
				public float RequiredSkillUsageLevel = 0f;
				public List <MobileReference> LocationsToReveal = new List<MobileReference>();
		}

		[Serializable]
		public class WIName
		{
				public string PackName = string.Empty;
				public string PrefabName = string.Empty;
				public string DisplayName = string.Empty;
				public string StackName = string.Empty;
				public string FileName = string.Empty;
				//public string FileNameIncremented	= string.Empty;
				public int FileNameIncrement = 0;
				public string QuestName = string.Empty;
				public bool AutoIncrementFileName = true;
		}

		[Serializable]
		public class WILocalProps
		{
				public bool HasInitializedOnce = false;
				public WIMode Mode = WIMode.World;
				public WIMode PreviousMode = WIMode.World;
				public bool FreezeOnStartup = true;
				public bool FreezeOnSleep = true;
				public float FreezeTimeout = 10.0f;
				public STransform Transform = STransform.zero;
				public SVector3 ChunkPosition = SVector3.zero;
				public float ActiveRadius = 5f;
				public float VisibleDistance = 50f;
				public bool IsStackContainer = false;
				public bool StolenGoods = false;
				public string Subcategory = string.Empty;
				public string LightTemplateName = string.Empty;
				public SVector3 LightOffset = SVector3.zero;
				public string CauseOfDestruction = string.Empty;
				public string DisplayNamerScript = string.Empty;
				public string StackNamerScript = string.Empty;
				public string HudTargetScript = string.Empty;
				public List <string> RemoveItemSkills = new List<string>();
		}

		[Serializable]
		public class WIGlobalProps
		{
				public bool ParentUnderGroup = true;
				public WIFlags Flags = new WIFlags();
				public WIExamineInfo ExamineInfo = new WIExamineInfo();
				public SVector3 BaseRotation = SVector3.zero;
				public SVector3 PivotOffset = SVector3.zero;
				public SVector3 HUDOffset = SVector3.zero;
				public ItemWeight Weight = ItemWeight.Light;
				public WIMaterialType MaterialType = WIMaterialType.None;
				public WICurrencyType CurrencyType = WICurrencyType.None;
				public float BaseCurrencyValue = 0f;
				public bool UseCutoutShader = false;
				public bool UseRigidBody = true;
				public bool DynamicPrefab = false;
				public bool CastShadows = false;
				//parent object properties
				public int ParentLayer = Globals.LayerNumWorldItemActive;
				public string ParentTag = "GroundStone";
				public WIColliderType ParentColliderType = WIColliderType.Box;
				public string FileNameBase = string.Empty;
		}

		[Serializable]
		public class WIFlags
		{
				public static WIFlags All {
						get {
								WIFlags newFlags = new WIFlags();
								newFlags.Alignment = 0;
								newFlags.Faction = 0;
								newFlags.Occupation = 0;
								newFlags.Region = 0;
								newFlags.Subject = 0;
								newFlags.Wealth = 0;
								return newFlags;
						}
				}

				public static WIFlags Union(WIFlags flags1, WIFlags flags2)
				{
						WIFlags newFlags = ObjectClone.Clone <WIFlags>(flags1);
						newFlags.Alignment |= flags2.Alignment;
						newFlags.Faction |= flags2.Faction;
						newFlags.Occupation |= flags2.Occupation;
						newFlags.Region |= flags2.Region;
						newFlags.Subject |= flags2.Subject;
						newFlags.Wealth |= flags2.Wealth;
						return newFlags;
				}

				public static WIFlags Intersection(WIFlags flags1, WIFlags flags2)
				{
						WIFlags newFlags = ObjectClone.Clone <WIFlags>(flags1);
						newFlags.Alignment &= flags2.Alignment;
						newFlags.Faction &= flags2.Faction;
						newFlags.Occupation &= flags2.Occupation;
						newFlags.Region &= flags2.Region;
						newFlags.Subject &= flags2.Subject;
						newFlags.Wealth &= flags2.Wealth;
						return newFlags;
				}

				public virtual void CopyFrom(WIFlags other, bool includeSize)
				{
						if (includeSize) {
								Size = other.Size;
						}
						BaseRarity = other.BaseRarity;
						Wealth = other.Wealth;
						Alignment = other.Alignment;
						Occupation = other.Occupation;
						Region = other.Region;
						Subject = other.Subject;
						Faction = other.Faction;
				}

				public virtual void CopyFrom(WIFlags other)
				{
						Size = other.Size;
						BaseRarity = other.BaseRarity;
						Wealth = other.Wealth;
						Alignment = other.Alignment;
						Occupation = other.Occupation;
						Region = other.Region;
						Subject = other.Subject;
						Faction = other.Faction;
				}

				public virtual void Union(WIFlags other)
				{
						Wealth |= other.Wealth;
						Alignment |= other.Alignment;
						Occupation |= other.Occupation;
						Region |= other.Region;
						Subject |= other.Subject;
						Faction |= other.Faction;
				}

				public virtual void Intersection(WIFlags other)
				{
						Wealth &= other.Wealth;
						Alignment &= other.Alignment;
						Occupation &= other.Occupation;
						Region &= other.Region;
						Subject &= other.Subject;
						Faction &= other.Faction;
				}

				public virtual bool Check(WIFlags otherFlags)
				{
						return// (true);//TEMP
				Flags.Check(Occupation, otherFlags.Occupation, Flags.CheckType.MatchAny)
						&& Flags.Check(Alignment, otherFlags.Alignment, Flags.CheckType.MatchAny)
						&& Flags.Check(Faction, otherFlags.Faction, Flags.CheckType.MatchAny)
						&& Flags.Check(Region, otherFlags.Region, Flags.CheckType.MatchAny)
						&& Flags.Check(Subject, otherFlags.Subject, Flags.CheckType.MatchAny)
						&& Flags.Check(Wealth, otherFlags.Wealth, Flags.CheckType.MatchAny);
				}

				public virtual bool IsEmpty {
						get {
								return Wealth <= 0 && Alignment <= 0 && Occupation <= 0 && Region <= 0 && Subject <= 0 && Faction <= 0;
						}
				}

				public virtual void FillEmpty()
				{
						if (Wealth == Int32.MaxValue) {
								Wealth = 0;
						}
						if (Alignment == Int32.MaxValue) {
								Alignment = 0;
						}
						if (Occupation == Int32.MaxValue) {
								Occupation = 0;
						}
						if (Region == Int32.MaxValue) {
								Region = 0;
						}
						if (Subject == Int32.MaxValue) {
								Subject = 0;
						}
						if (Faction == Int32.MaxValue) {
								Faction = 0;
						}
				}

				public override string ToString()
				{
						return "Wealth: " + Wealth.ToString() + ", Alignment: " + Alignment.ToString()
						+ ", Occupation: " + Occupation.ToString() + ", Region: " + Region.ToString() + ", Subject: " + Subject.ToString()
						+ ", Faction: " + Faction.ToString();

				}

				public WISize Size = WISize.NoLimit;
				public WIRarity BaseRarity = WIRarity.Common;
				[FrontiersBitMaskAttribute("Wealth")]
				public int Wealth = 0;
				[FrontiersBitMaskAttribute("Alignment")]
				public int Alignment = 0;
				[FrontiersBitMaskAttribute("Occupation")]
				public int Occupation = 0;
				[FrontiersBitMaskAttribute("Region")]
				public int Region = 0;
				[FrontiersBitMaskAttribute("BookSubject")]
				public int Subject = 0;
				[FrontiersBitMaskAttribute("Faction")]
				public int Faction = 0;

				public static int RarityToInt(WIRarity rarity, int multiplier)
				{
						switch (rarity) {
								case WIRarity.Common:
								default:
										return 5 * multiplier;

								case WIRarity.Uncommon:
										return 3 * multiplier;

								case WIRarity.Rare:
										return 1 * multiplier;

								case WIRarity.Exclusive:
										return 1;
						}
				}
		}

		[Serializable]
		public class SIProps
		{
				//stack item props don't store global props
				//those are looked up at runtime
				public WIName Name = new WIName();
				public WILocalProps Local = new WILocalProps();

				public void CopyGlobalNames(SIProps copyFrom)
				{
						Name.PackName = copyFrom.Name.PackName;
						Name.PrefabName	= copyFrom.Name.PrefabName;
				}

				public void CopyLocalNames(SIProps copyFrom)
				{
						Name.DisplayName = copyFrom.Name.DisplayName;
						Name.FileName = copyFrom.Name.FileName;
						Name.FileNameIncrement = copyFrom.Name.FileNameIncrement;
						Name.QuestName = copyFrom.Name.QuestName;
						Name.StackName = copyFrom.Name.StackName;
						Name.AutoIncrementFileName = copyFrom.Name.AutoIncrementFileName;
				}

				public void CopyName(SIProps copyFrom)
				{
						Name = ObjectClone.Clone <WIName>(copyFrom.Name);
				}

				public void CopyLocal(SIProps copyFrom)
				{
						Local = ObjectClone.Clone <WILocalProps>(copyFrom.Local);
				}
		}

		[Serializable]
		[XmlInclude(typeof(SIProps))]
		public class WIProps : SIProps
		{
				//worlditem props do store global props
				//these are saved by templates
				public WIGlobalProps Global = new WIGlobalProps();

				public bool HasGlobalProps {
						get {
								return Global != null;
						}
				}

				public void GetGlobal()
				{
						WorldItems.Get.GlobalProps(this);
				}

				public void CopyGlobal(WIProps copyFrom)
				{
						Global = copyFrom.Global;// ObjectClone.Clone <WIGlobalProps> (copyFrom.Global);
				}
		}

		[Serializable]
		public class WISaveState
		{
				public bool CanEnterInventory = false;
				public bool CanBeCarried = false;
				public bool CanBeDropped = false;
				public bool UnloadWhenStacked = true;
				public bool HasStates = false;
				public string LastState = string.Empty;
				public SDictionary <string,string> Scripts = new SDictionary <string, string>();
		}
}
