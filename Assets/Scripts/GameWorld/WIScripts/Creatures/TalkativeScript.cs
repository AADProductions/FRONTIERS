using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System.Xml;
using System.Xml.Serialization;

namespace Frontiers.World.Gameplay
{
	[Serializable]
	[XmlInclude (typeof(TalkativeMissionCompleted))]
	[XmlInclude (typeof(TalkativeObjectiveCompleted))]
	[XmlInclude (typeof(TalkativeMissionActive))]
	[XmlInclude (typeof(TalkativeObjectiveActive))]
	[XmlInclude (typeof(TalkativeExchangeCompleted))]
	public class TalkativeScript
	{
		public string Description = "New Change";
		public string OriginalConversationName = string.Empty;
		public string SubstitutedConvsersationName	= string.Empty;

		public bool				Substitute (ref string currentConversationName)
		{	//if the current name is the name we're supposed to substitute
			//and the condition checks out
			//substitute the name and return true
			if (currentConversationName == OriginalConversationName
			    &&	CheckSubstitutionRule ()) {
				//Debug.Log ("Substituting in talkative script!");
				currentConversationName = SubstitutedConvsersationName;
				return true;
			}
			return false;
		}

		protected virtual bool	CheckSubstitutionRule ()
		{
			return false;
		}
	}

	[Serializable]
	public class TalkativeMissionCompleted : TalkativeScript
	{
		public string MissionName	= string.Empty;

		protected override bool CheckSubstitutionRule ()
		{
			bool completed = false;
			if (Missions.Get.MissionCompletedByName (MissionName, ref completed)) {
				return completed;
			}
			return false;
		}
	}

	[Serializable]
	public class TalkativeObjectiveCompleted : TalkativeScript
	{
		public string MissionName = string.Empty;
		public string ObjectiveName	= string.Empty;

		protected override bool CheckSubstitutionRule ()
		{
			bool completed = false;
			if (Missions.Get.ObjectiveCompletedByName (MissionName, ObjectiveName, ref completed)) {
				return completed;
			}
			return false;
		}
	}

	[Serializable]
	public class TalkativeMissionActive : TalkativeScript
	{
		public string MissionName = string.Empty;

		protected override bool CheckSubstitutionRule ()
		{
			MissionStatus status = MissionStatus.Dormant;
			if (Missions.Get.MissionStatusByName (MissionName, ref status)) {
				return Flags.Check ((uint)status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
			}
			return false;
		}
	}

	[Serializable]
	public class TalkativeObjectiveActive : TalkativeScript
	{
		public string MissionName = string.Empty;
		public string ObjectiveName = string.Empty;

		protected override bool CheckSubstitutionRule ()
		{
			MissionStatus status = MissionStatus.Dormant;
			if (Missions.Get.ObjectiveStatusByName (MissionName, ObjectiveName, ref status)) {
				return Flags.Check ((uint)status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny);
			}
			return false;
		}
	}

	[Serializable]
	public class TalkativeExchangeCompleted : TalkativeScript
	{
		public string ConversationName	= string.Empty;
		public string ExchangeName = string.Empty;
		public bool RequireConversationInitiated = true;

		protected override bool CheckSubstitutionRule ()
		{
			//Debug.Log ("Checking if we've had exchange " + ConversationName + ", " + ExchangeName);
			return Conversations.Get.HasCompletedExchange (ConversationName, ExchangeName, RequireConversationInitiated);
		}
	}
}