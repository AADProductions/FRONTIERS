using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	[Serializable]
	public class MotileProperties
	{
		public MotileGoToMethod DefaultGoToMethod = MotileGoToMethod.Pathfinding;
		public float HoverHeight = 2f;
		public bool Hovers = false;
		public bool UseKinematicBody = true;
		public bool CanJump = true;
		public float HoverChangeSpeed = 1.0f;
		public float SpeedRun = 4.5f;
		public float SpeedWalk = 2.5f;
		public float SpeedWounded = 0.5f;
		public float SpeedIdleWalk = 0.5f;
		public float SpeedAttack = 0.125f;
		public float JumpForce = 7.5f;
		public float RotationChangeSpeed = 2.5f;
		public float MovementChangeSpeed = 4.0f;
		public float IdleWanderThreshold = 0.9f;
		public float IdleWaitThreshold = 0.05f;
		public float MaxElevationChange = 0.5f;
		public float GroundedHeight = 0.25f;
		public float RVORadius = 1.0f;
	}

	[Serializable]
	public class CharacterName
	{
		public void Generate (CharacterFlags combinedFlags)
		{
			GenericIdentifier = "Settler";
			return;
		}

		public string GenericIdentifier;
		public string FileName;
		public string Prefix;
		public string FirstName;
		public string LastName;
		public string MiddleName;
		public string NickName;
		public string PostFix;
		public int SpawnValue;
	}

	[Serializable]
	public class CharacterFlags : WIFlags
	{
		public CharacterFlags ()
		{

		}

		public bool Check (CharacterFlags anyOfThese)
		{
			return (Flags.Check (CharacterBodyLayout, anyOfThese.CharacterBodyLayout, Flags.CheckType.MatchAny)
			&& Flags.Check (Alignment, anyOfThese.Alignment, Flags.CheckType.MatchAny)
			&& Flags.Check (Ethnicity, anyOfThese.Ethnicity, Flags.CheckType.MatchAny)
			&& Flags.Check (Faction, anyOfThese.Faction, Flags.CheckType.MatchAny)
			&& Flags.Check (Gender, anyOfThese.Gender, Flags.CheckType.MatchAny)
			&& Flags.Check (GeneralAge, anyOfThese.GeneralAge, Flags.CheckType.MatchAny)
			&& Flags.Check (Occupation, anyOfThese.Occupation, Flags.CheckType.MatchAny)
			&& Flags.Check (Region, anyOfThese.Region, Flags.CheckType.MatchAny));
		}

		public void Intersection (CharacterFlags other)
		{
			Gender = Gender & other.Gender;
			GeneralAge = GeneralAge & other.GeneralAge;
			Ethnicity = Ethnicity & other.Ethnicity;
			Occupation = Occupation & other.Occupation;
			Credentials1 = Credentials1 & other.Credentials1;
			Credentials2 = Credentials2 & other.Credentials2;
			Credentials3 = Credentials3 & other.Credentials3;
			Credentials4 = Credentials4 & other.Credentials4;
			Credentials5 = Credentials5 & other.Credentials5;
			Faction = Faction & other.Faction;
			Region = Region & other.Region;
			Alignment = Alignment & other.Alignment;
			Wealth = Wealth & other.Wealth;
		}

		public void SafeIntersection (CharacterFlags other)
		{
			int gender = Gender & other.Gender;
			int generalAge = GeneralAge & other.GeneralAge;
			int ethnicity = Ethnicity & other.Ethnicity;
			int occupation = Occupation & other.Occupation;
			int credentials1 = Credentials1 & other.Credentials1;
			int credentials2 = Credentials2 & other.Credentials2;
			int credentials3 = Credentials3 & other.Credentials3;
			int credentials4 = Credentials4 & other.Credentials4;
			int credentials5 = Credentials5 & other.Credentials5;
			int faction = Faction & other.Faction;
			int region = Region & other.Region;
			int alignment = Alignment & other.Alignment;
			int wealth = Wealth & other.Wealth;

			Gender = (gender > 0) ? gender : Gender;
			GeneralAge = (generalAge > 0) ? generalAge : GeneralAge;
			Ethnicity = (ethnicity > 0) ? ethnicity : Ethnicity;
			Occupation = (occupation > 0) ? occupation : Occupation;
			Credentials1 = (credentials1 > 0) ? credentials1 : Credentials1;
			Credentials2 = (credentials2 > 0) ? credentials2 : Credentials2;
			Credentials3 = (credentials3 > 0) ? credentials3 : Credentials3;
			Credentials4 = (credentials4 > 0) ? credentials4 : Credentials4;
			Credentials5 = (credentials5 > 0) ? credentials5 : Credentials5;
			Faction = (faction > 0) ? faction : Faction;
			Region = (region > 0) ? region : Region;
			Alignment = (alignment > 0) ? alignment : Alignment;
			Wealth = (wealth > 0) ? wealth : Wealth;
		}

		public void Union (CharacterFlags other)
		{
			Gender = Gender | other.Gender;
			GeneralAge = GeneralAge | other.GeneralAge;
			Ethnicity = Ethnicity | other.Ethnicity;
			Occupation = Occupation | other.Occupation;
			Credentials1 = Credentials1 | other.Credentials1;
			Credentials2 = Credentials2 | other.Credentials2;
			Credentials3 = Credentials3 | other.Credentials3;
			Credentials4 = Credentials4 | other.Credentials4;
			Credentials5 = Credentials5 | other.Credentials5;
			Faction = Faction | other.Faction;
			Region = Region | other.Region;
			Alignment = Alignment | other.Alignment;
			Wealth = Wealth | other.Wealth;
		}

		public override void Union (WIFlags other)
		{
			Alignment = other.Alignment | Alignment;
			Occupation = other.Occupation | Occupation;
			Region = other.Region | Region;
			Faction = other.Faction | Faction;
			Wealth = other.Wealth | Wealth;
		}

		public override void Intersection (WIFlags other)
		{
			Alignment = other.Alignment & Alignment;
			Occupation = other.Occupation & Occupation;
			Region = other.Region & Region;
			Faction = other.Faction & Faction;
			Wealth = other.Wealth & Wealth;
		}

		public void SafeIntersection (WIFlags other)
		{
			int alignment = other.Alignment & Alignment;
			int occupation = other.Occupation & Occupation;
			int region = other.Region & Region;
			int faction = other.Faction & Faction;
			int wealth = other.Wealth & Wealth;

			Alignment = (alignment > 0) ? alignment : Alignment;
			Occupation = (occupation > 0) ? occupation : Occupation;
			Region = (region > 0) ? region : Region;
			Faction = (faction > 0) ? faction : Faction;
			Wealth = (wealth > 0) ? wealth : Wealth;
		}

		public void ChooseMajorValues (int tieBreaker)
		{
			//don't touch character body layout
			//we'll let the texture do that
			GeneralAge = FlagSet.GetFlagBitValue (GeneralAge, tieBreaker, 1);
			Ethnicity = FlagSet.GetFlagBitValue (Ethnicity, tieBreaker, 1);
		}

		public void ChooseMinorValues (int tieBreaker)
		{
			Occupation = FlagSet.GetFlagBitValue (Occupation, tieBreaker, 1);
			Credentials1 = FlagSet.GetFlagBitValue (Credentials1, tieBreaker, 1);
			Credentials2 = FlagSet.GetFlagBitValue (Credentials2, tieBreaker, 1);
			Credentials3 = FlagSet.GetFlagBitValue (Credentials3, tieBreaker, 1);
			Credentials4 = FlagSet.GetFlagBitValue (Credentials4, tieBreaker, 1);
			Credentials5 = FlagSet.GetFlagBitValue (Credentials5, tieBreaker, 1);
			Faction = FlagSet.GetFlagBitValue (Faction, tieBreaker, 1);
			Region = FlagSet.GetFlagBitValue (Region, tieBreaker, 1);
			Alignment = FlagSet.GetFlagBitValue (Alignment, tieBreaker, 1);
			Wealth = FlagSet.GetFlagBitValue (Wealth, tieBreaker, 1);
		}

		[FrontiersBitMask ("CharacterBodyLayout")]
		public int CharacterBodyLayout = 0;
		[FrontiersBitMask ("Gender")]
		public int Gender = 0;
		[FrontiersBitMask ("GeneralAge")]
		public int GeneralAge = 0;
		[FrontiersBitMask ("Ethnicity")]
		public int Ethnicity = 0;
		[FrontiersBitMaskAttribute ("CredentialsAristocracy")]
		public int Credentials1 = 0;
		[FrontiersBitMaskAttribute ("CredentialsBandit")]
		public int Credentials2 = 0;
		[FrontiersBitMaskAttribute ("CredentialsGuild")]
		public int Credentials3 = 0;
		[FrontiersBitMaskAttribute ("CredentialsSoldier")]
		public int Credentials4 = 0;
		[FrontiersBitMaskAttribute ("CredentialsWarlock")]
		public int Credentials5 = 0;
	}

	[Serializable]
	public class CharacterTemplate : Mod, IComparable <CharacterTemplate>
	{
		public int CompareTo (CharacterTemplate other)
		{
			return Name.CompareTo (other.Name);
		}

		public CharacterTemplateType TemplateType = CharacterTemplateType.Generic;
		public Frontiers.World.WIScripts.CharacterState StateTemplate = new Frontiers.World.WIScripts.CharacterState ();
		public TalkativeState TalkativeTemplate	= new TalkativeState ();
		public MotileState MotileTemplate = new MotileState ();
		public HostileState HostileTemplate = new HostileState ();
		public LookerState LookerTemplate = new LookerState ();
		public ListenerState ListenerTemplate = new ListenerState ();
		public DamageableState DamageableTemplate = new DamageableState ();
		public WIExamineInfo ExamineInfo = new WIExamineInfo ();
		public string BodyNameMale;
		public string BodyNameFemale;
		public List <string> CustomWIScripts = new List <string> ();
		public List <string> AvailableFaceTexturesMale = new List <string> ();
		public List <string> AvailableBodyTexturesMale = new List <string> ();
		public List <string> AvailableFaceTexturesFemale = new List <string> ();
		public List <string> AvailableBodyTexturesFemale = new List <string> ();
		public List <string> TextureKeywords = new List <string> ();
		//[FrontiersAvailableModsAttribute("Category")]
		public string InventoryFillCategory = string.Empty;
		public bool UseDefaultMotile = true;
		public bool UseDefaultDamageable = true;
		public bool UseDefaultLookerState = true;
		public bool UseDefaultListenerState = true;
	}
}