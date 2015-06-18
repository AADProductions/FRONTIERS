using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Frontiers.Story
{
	[Serializable]
	public class Speech : Mod
	{
		public Speech ()
		{
			Type = "Speech";
			Name = "New Speech";
			Description = "A new speech";
		}

		public string Text = string.Empty;
		public int Flags = 0;
		public float AudibleRange = 5.0f;
		public int NumTimesStarted = 0;
		public int NumTimesCompleted = 0;

		public int NumTimesInterrupted {
			get {
				return NumTimesStarted - NumTimesCompleted;
			}
		}

		public bool Loops = false;
		public bool CanBeInterrupted = true;
		public string OnAudibleCommand = string.Empty;
		public string OnInterruptCommand = string.Empty;
		public string OnFinishCommand = string.Empty;
		public string OnFinishMessage = string.Empty;
		public string OnFinishMessageParam = string.Empty;
		public string OnFinishMissionActivate = string.Empty;
		public string OnFinishObjectiveActivate	= string.Empty;
		public int MaxLoops = 1;

		public int NumPages {
			get {
				if (mSpeechPages == null) {
					BuildSpeechPages ();
				}
				return mSpeechPages.Count;
			}
		}

		[XmlIgnore]
		public int NumUsers = 0;
		public SDictionary <string,int>	StartedByCharacters = new SDictionary <string, int> ();
		public SDictionary <string,int>	FinishedByCharacters	= new SDictionary <string, int> ();

		public void StartSpeech (string characterName)
		{
			if (StartedByCharacters.ContainsKey (characterName)) {
				int numTimes = StartedByCharacters [characterName] + 1;
				StartedByCharacters [characterName]	= numTimes;
			} else {
				StartedByCharacters.Add (characterName, 1);
			}
			NumTimesStarted++;
			NumUsers++;
		}

		public void InterruptSpeech (string characterName)
		{
			NumUsers--;
		}

		public void FinishSpeech (string characterName)
		{
			if (FinishedByCharacters.ContainsKey (characterName)) {
				int numTimes = FinishedByCharacters [characterName] + 1;
				FinishedByCharacters [characterName] = numTimes;
			} else {
				FinishedByCharacters.Add (characterName, 1);
			}
			NumTimesCompleted++;
			NumUsers--;
		}

		public int NumTimesStartedBy (string characterName)
		{
			int numTimes = 0;
			StartedByCharacters.TryGetValue (characterName, out numTimes);
			return numTimes;
		}

		public int NumTimesFinishedBy (string characterName)
		{
			int numTimes = 0;
			FinishedByCharacters.TryGetValue (characterName, out numTimes);
			return numTimes;
		}

		public bool GetPage (ref string nextPage, ref float nextPageDuration, ref int lastPageIndex, bool autoWrap)
		{
			//Debug.Log ("Getting page " + lastPageIndex.ToString ());
			if (mSpeechPages == null) {
				BuildSpeechPages ();
			}

			if (mSpeechPages.Count == 0) {
				//Debug.Log ("There are no speech pages, returning false");
				return false;
			}

			lastPageIndex++;
			if (mSpeechPages.Count == 1) {
				if (lastPageIndex == 0) {
					KeyValuePair <string, float> nextPagePair = mSpeechPages [lastPageIndex];
					nextPage = nextPagePair.Key;
					nextPageDuration = nextPagePair.Value;
					//Debug.Log ("Got speech page " + nextPage);
					return true;
				} else {
					//Debug.Log ("Last page index was out of range, returning false");
					lastPageIndex = -1;
					return false;
				}
			} else {
				if (lastPageIndex >= mSpeechPages.Count) {
					//Debug.Log ("Last page index is greater than the page count, returning false");
					lastPageIndex = -1;
					return false;
				} else {
					KeyValuePair <string, float> nextPagePair = mSpeechPages [lastPageIndex];
					nextPage = nextPagePair.Key;
					nextPageDuration = nextPagePair.Value;
					//Debug.Log ("Got next page");
					return true;			
				}
			}
		}

		protected void BuildSpeechPages ()
		{
			//Debug.Log ("Building speech pages...");
			mSpeechPages = new List <KeyValuePair <string, float>> ();
			string text = Text.Replace ("[start]", "");
			text = text.Replace ("[stop]", "");
			string[] pages = null;
			if (text.Contains (gPageBreakSeparator)) {
				pages = text.Split (new string [] { gPageBreakSeparator }, StringSplitOptions.RemoveEmptyEntries);
			} else {
				pages = new string[] { text };
			}
			for (int i = 0; i < pages.Length; i++) {
				float pageDuration = -1.0f;
				string pageText = string.Empty;
				string pageColor = string.Empty;
				List <string> finalPageLines = new List <string> ();
				//reset this to false for page
				bool isDescription = false;
				//split the text into pages using [PageBreak]
				//then interpret each line of the text, identifying tags along the way
				string[] pageLines = pages [i].Split (gNewlineSeparators, StringSplitOptions.RemoveEmptyEntries);
				//Debug.Log ("There are " + pageLines.Length.ToString () + " pages in this speech");
				for (int j = 0; j < pageLines.Length; j++) {
					//Debug.Log ("Page line: " + pageLines [j]);
					if (pageLines [j].StartsWith (gTagSeparator)) {	//it's a tag
						string[] splitTagLine = pageLines [j].Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string tag = splitTagLine [0].ToLower ();
						switch (tag) {
						case "#duration":
							string durationText = splitTagLine [1].Replace ("f", "");
							float.TryParse (durationText, out pageDuration);
							break;
						case "#description":
							isDescription = Boolean.Parse (splitTagLine [1].ToLower ());
							break;

						case "#color":
							pageColor = splitTagLine [1].ToUpper ();
							break;
						default:
							break;
						}
					} else {
						string cleanPageLine = pageLines [j];
						cleanPageLine = Frontiers.Data.GameData.InterpretScripts (cleanPageLine, Profile.Get.CurrentGame.Character, null);
						for (int k = 0; k < gIllegalCharacters.Length; k++) {
							cleanPageLine.Replace (gIllegalCharacters [k], "");
						}

						if (isDescription) {
							//if it's a description, wrap the line in ( ) with newlines
							cleanPageLine = Colors.ColorWrap (" (" + cleanPageLine + ") ", Color.gray, true);
						}
						finalPageLines.Add (cleanPageLine);
					}
				}
				//alright we have our final page info
				pageText = finalPageLines.JoinToString ("");
				//Debug.Log ("Final page text: " + pageText);
				if (pageDuration <= 0f) {
					pageDuration = Mathf.Max (mDurationPerChar * pageText.Length, mMinimumDuration);
				}
				if (!string.IsNullOrEmpty (pageColor)) {
					pageText = Colors.ColorWrap (pageText, pageColor, true);
				}
				if (!string.IsNullOrEmpty (pageText)) {
					mSpeechPages.Add (new KeyValuePair <string, float> (pageText, pageDuration));
				}
			}
		}

		protected float mDurationPerChar = 0.025f;
		protected float mMinimumDuration = 3.0f;
		protected List <KeyValuePair <string, float>> mSpeechPages = null;
		protected static string[] gNewlineSeparators = new string [] {
			"\n",
			"\n\r"
		};
		//TODO replace this with regex
		protected static string[] gIllegalCharacters = new string [] {
			":",
			";",
			"<",
			">",
			"@",
			"#",
			"$",
			"%",
			"^",
			"^",
			"&",
			"*",
			"(",
			")",
			"/",
			"\\",
			"|",
			"[",
			"]",
			"{",
			"}",
			"_",
			"+",
			"=",
			"~",
			"`"
		};
		protected static string gPageBreakSeparator = "[pagebreak]";
		protected static string gTagSeparator = "#";
	}
}
