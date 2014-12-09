using UnityEngine;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.Story.Conversations
{
	[Serializable]
	public class ConversationProps : Mod
	{
		public bool IsGeneric = false;
		public AvailabilityBehavior Availability = AvailabilityBehavior.Always;
		public int MaxTimesInitiated = 0;
		public Exchange DefaultOpeningExchange = new Exchange ();
		public List <Exchange> Exchanges = new List <Exchange> ();

		public int GetWordCount ( )
		{
			return DefaultOpeningExchange.GetWordCount ();
		}
	}

	[Serializable]
	public class ConversationState : Mod
	{
		public string CharacterName = string.Empty;
		public int NumTimesInitiated = 0;
		public bool LeaveOnEnd = false;
		public string LastChosenExchange = string.Empty;
		public string InitiateFailureResponse = string.Empty;
		public string OverrideOpeningExchange = string.Empty;
		public bool CanLeave = true;
		public List <ExchangeScript> GlobalScripts = new List <ExchangeScript> ();
		public SDictionary <string, SimpleVar> ConversationVariables = new SDictionary <string, SimpleVar> ();
		public SDictionary <string, int> CompletedExchanges = new SDictionary <string, int> ();
		public SDictionary <int, string> ExchangeNames = new SDictionary <int, string> ();
		public HashSet <string> DisabledExchanges = new HashSet <string> ();
		//conversation substitutions per-character
		public SDictionary <string, string> Substitutions = new SDictionary <string, string> ();
		public SDictionary <string, string> DTSOverrides = new SDictionary <string, string> ();
	}

	[Serializable]
	public enum ExchangeOutgoingStyle
	{
		Normal,
		SiblingsOff,
		ManualOnly,
		Stop,
	}
}