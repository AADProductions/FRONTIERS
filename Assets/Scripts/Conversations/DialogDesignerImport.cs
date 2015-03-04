//using UnityEngine;
//using System;
//using System.IO;
//using System.Text;
//using System.Xml;
//using System.Runtime.Serialization;
//using System.Xml.Serialization;
//using System.Collections;
//using System.Collections.Generic;
//using DialogDesigner;
//using Frontiers;
//using Frontiers.Data;
//using Frontiers.World;
//using Frontiers.World.Gameplay;
//using Frontiers.World.Gameplay;
//using Frontiers.Story;
//using System.Globalization;
//
//public class DialogDesignerImport : MonoBehaviour
//{
//	public List <string> DialogDirectories = new List<string> ();
//	public List <KeyValuePair <string,string>>	AssetsToSave = new List <KeyValuePair <string,string>> ();
//	public KeyValuePair <string,string> textAsset;
//	public DialogDesigner.dialog dialogObject;
//	public string exchangePrefix = "E-";
//	public Dictionary <string, SimpleVar> variables = new Dictionary <string, SimpleVar> ();
//	public Dictionary <int, string> exchangeLookup = new Dictionary <int, string> ();
//	public Dictionary <string, HashSet <int>>	manualOptions = new Dictionary <string, HashSet<int>> ();
//	public Conversation convertedDialog;
//	#if UNITY_EDITOR
//	public void GetDialogAssets ()
//	{
//		foreach (string DialogDirectory in DialogDirectories) {
//			//load all book assets
//			List <string> filesInDirectory	= new List <string> ();
//			//#if UNITY_STANDALONE_WIN
//			if (Directory.Exists (DialogDirectory)) {
//				////Debug.Log ("Directory " + directory + " exists");
//				System.IO.DirectoryInfo filesDirectory = new System.IO.DirectoryInfo (DialogDirectory);
//				foreach (System.IO.FileInfo file in filesDirectory.GetFiles ( )) {
//					if (!file.Name.Contains ("_DTS")) {
//						////Debug.Log ("Adding file in directory " + file.Name);
//						filesInDirectory.Add (System.IO.Path.GetFileName (file.Name));
//					}
//				}
//			}
//			
//			//Debug.Log ("Found " + filesInDirectory.Count + " files in directory " + DialogDirectory);
//			
//			foreach (string fileInDirectory in filesInDirectory) {
//				string finalPath = System.IO.Path.Combine (DialogDirectory, fileInDirectory);
//				if (File.Exists (finalPath)) {
//					//Debug.Log ("Loading " + fileInDirectory);
//					string fileData = File.ReadAllText (finalPath);
//					AssetsToSave.Add (new KeyValuePair <string,string> (fileInDirectory, fileData));
//				} else {
//					//Debug.Log (finalPath + " didn't exist");
//				}
//			}
//		}
//	}
//
//	public void									Start ()
//	{	
//		GetDialogAssets ();
//
//		convertedDialog = gameObject.GetOrAdd <Conversation> ();
//		foreach (KeyValuePair <string,string> assetToSave in AssetsToSave) {
//			variables.Clear ();
//			exchangeLookup.Clear ();
//			manualOptions.Clear ();
//
//			convertedDialog.State = new ConversationState ();
//			convertedDialog.Props = new ConversationProps ();
//
//			textAsset = assetToSave;
//			MemoryStream memStream = new MemoryStream (Encoding.ASCII.GetBytes (textAsset.Value));
//			var serializer = new XmlSerializer (typeof(DialogDesigner.dialog));
//			dialogObject = (DialogDesigner.dialog)serializer.Deserialize (memStream);
//
//			//get variables
//			foreach (DialogDesigner.variable var in dialogObject.variables) {	////Debug.Log ("Found variable " + var.name + " in dialog designer " + var.type);
//				if (var.type == "Boolean") {
//					//ExchangeScript globalScript = InterpretCustomScript (var.description, null);
//				} else {
//					string variableName = var.name;
//					int defaultValue = Int32.Parse (var.defaultValue);
//					int minValue = 0;
//					int maxValue = 0;
//
//					string[] splitMinMaxVars	= var.description.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//					foreach (string splitMinMax in splitMinMaxVars) {
//						////Debug.Log ("min max: " + splitMinMax);
//						string[] splitMinMaxVar = splitMinMax.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
//						string minMaxVarName = splitMinMaxVar [0];
//						string minMaxVarVal = splitMinMaxVar [1];
//
//						switch (minMaxVarName.ToLower ()) {
//						case "min":
//						default:
//							minValue = Int32.Parse (minMaxVarVal);
//							break;
//
//						case "max":
//							maxValue = Int32.Parse (minMaxVarVal);
//							break;
//						}
//					}
//
//					SimpleVar stateVar = new SimpleVar ();
//					stateVar.DefaultValue	= defaultValue;
//					stateVar.Min = minValue;
//					stateVar.Max = maxValue;
//					variables.Add (variableName, stateVar);
//					convertedDialog.State.ConversationVariables.Add (var.name, stateVar);
//					////Debug.Log ("Added variable " + var.name + " count is now " + convertedDialog.State.ConversationVariables.Count.ToString ( ) + " in " + textAsset.name);
//				}
//			}
//
//			convertedDialog.Props.Name = GetNameFromDialogName (assetToSave.Key);
//			convertedDialog.Props.DefaultOpeningExchange	= new Exchange ();
//			
//			int outgoingLoop = 0;
//			int outgoingDepth = 0;
//			int exchangeNumber = 1;
//			
//			int ddIndex	= 0;
//			AssignIndexToOption (dialogObject.options [0], ref ddIndex);
//			
//			foreach (DialogDesigner.option opt in dialogObject.options) {
//				GenerateExchangesFromOptions (convertedDialog.Props.DefaultOpeningExchange, opt, ref outgoingLoop, outgoingDepth, ref exchangeNumber);
//				outgoingLoop++;
//			}			
//			LinkManualOptions (convertedDialog.Props.DefaultOpeningExchange);
//
//			convertedDialog.Refresh ();
//			convertedDialog.EditorSave ();
//		}
//	}
//
//	public void 								AssignIndexToOption (DialogDesigner.option option, ref int currentIndex)
//	{
//		option.index = currentIndex;
//		currentIndex++;
//		foreach (DialogDesigner.option nextOption in option.options) {
//			AssignIndexToOption (nextOption, ref currentIndex);
//		}
//	}
//
//	public void 								GenerateExchangesFromOptions (Exchange currentExchange, DialogDesigner.option currentOption, ref int outgoingLoop, int depth, ref int exchangeNumber)
//	{		
//		string cleanPlayerDialog = StripText (currentOption.text);
//		string[] splitPlayerDialogLines	= cleanPlayerDialog.Split (new string [] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
//		string finalPlayerDialog = string.Empty;
//		foreach (string splitPlayerLine in splitPlayerDialogLines) {
//			////Debug.Log (splitPlayerLine);
//			if (!splitPlayerLine.Contains ("#")) {
//				finalPlayerDialog += splitPlayerLine;
//			}
//		}
//		finalPlayerDialog = finalPlayerDialog.Replace ("[PageBreak]", "{PageBreak}");
//
//
//		//CHARACTER RESPONSE
//		string cleanResponse = StripText (currentOption.script);
//		string[] splitResponseLines = cleanResponse.Split (new string [] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
//		string finalResponse = string.Empty;
//		foreach (string splitResponseNameCheck in splitResponseLines) {
//			if (splitResponseNameCheck.Contains ("#exchangename")) {
//				string cleanExchangeName = splitResponseNameCheck.Replace ("#exchangename ", "");
//				cleanExchangeName = splitResponseNameCheck.Replace ("#exchangename=", "");//arg get rid of this
//				cleanExchangeName = splitResponseNameCheck.Replace ("#exchangename", "");//arg get rid of this
//				currentExchange.Name = cleanExchangeName.Trim ();
//				//Debug.Log ("Giving dialog exchange name " + currentExchange.Name);
//			}
//		}
//
//		if (string.IsNullOrEmpty (currentExchange.Name)) {	//give it the default name if it hasn't been set yet
//			currentExchange.Name = exchangePrefix + exchangeNumber.ToString ("D4");
//		} else {
//			//Debug.Log ("Exchange is already named " + currentExchange.Name);
//		}
//		if (currentExchange.DisplayOrder < 0) {
//			currentExchange.DisplayOrder = exchangeNumber;
//		}
//		convertedDialog.State.ExchangeNames.Add (exchangeNumber, currentExchange.Name);
//		//add this to the lookup so we can manually link stuff up later
//		exchangeLookup.Add (currentOption.index, currentExchange.Name);
//		//use this to store dd indexes
//		HashSet <int> manualOptionIndexes = new HashSet <int> ();
//		//add this list to lookup to link stuff up later
//		manualOptions.Add (currentExchange.Name, manualOptionIndexes);
//
//		foreach (string splitResponseLine in splitResponseLines) {
//			if (!splitResponseLine.Contains ("#")) {	//it's not a script
//				if (splitResponseLine == "stop") {	//special case, this stops the conversation
//					//this also overrides ExchangeOutgoingStyle.SiblingsOff
//					currentExchange.OutgoingStyle = ExchangeOutgoingStyle.Stop;
//				} else if (splitResponseLine == "[AllOff]") {
//					currentExchange.OutgoingStyle = ExchangeOutgoingStyle.ManualOnly;
//				} else if (splitResponseLine == "option-off-forever") {
//					string script = splitResponseLine.Replace ("option-off-forever", "");
//					string[] offForeverExchanges = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//					SetExchangeEnabled see = new SetExchangeEnabled ();
//					see.Enabled = false;
//					see.Exchanges = offForeverIndexes;
//					currentExchange.Scripts.Add (see);
//				} else {
//					finalResponse += splitResponseLine;
//				}
//			} else {	//it is a script
//				InterpretScript (splitResponseLine, currentExchange, manualOptionIndexes);
//			}
//		}
//		finalResponse = finalResponse.Replace ("[PageBreak]", "{PageBreak}");
//
//		currentExchange.PlayerDialog = finalPlayerDialog;
//		currentExchange.CharacterResponse = finalResponse;
//
//		if (!string.IsNullOrEmpty (currentOption.condition)) {
//			InterpretCondition (currentOption.condition, currentExchange);
//		}
//		exchangeNumber++;
//		
//		int outgoingDepth = depth + 1;
//		foreach (DialogDesigner.option outgoingOption in currentOption.options) {
//			Exchange outgoingExchange = new Exchange ();
//			currentExchange.OutgoingChoices.Add (outgoingExchange);
//			GenerateExchangesFromOptions (outgoingExchange, outgoingOption, ref outgoingLoop, outgoingDepth, ref exchangeNumber);
//			outgoingLoop++;
//		}
//	}
//
//	public void 								InterpretCondition (string condition, Exchange exchange)
//	{
//		string[] splitConditions = condition.Split (new string [] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
//
//		foreach (string splitCondition in splitConditions) {
//			////Debug.Log ("Found condition " + splitCondition);
//			splitCondition.Replace (" ", "");//get rid of spaces
//			string variableName = string.Empty;
//			string splitValueCheck = string.Empty;
//			int variableValue = 0;
//			bool foundVariable = false;
//			foreach (string var in variables.Keys) {
//				if (splitCondition.Contains (var)) {
//					variableName = var;
//					foundVariable	= true;
//					break;
//				}
//			}
//			bool foundCheckType = false;
//			VariableCheckType checkType = VariableCheckType.GreaterThan;
//			//if we haven't found it, forget it
//			if (foundVariable) {
//				if (splitCondition.Contains (">=")) {
//					checkType = VariableCheckType.GreaterThanOrEqualTo;
//					splitValueCheck	= ">=";
//					foundCheckType = true;
//				} else if (splitCondition.Contains ("<=")) {
//					checkType = VariableCheckType.LessThanOrEqualTo;
//					splitValueCheck	= "<=";
//					foundCheckType = true;
//				} else if (splitCondition.Contains (">")) {
//					checkType = VariableCheckType.GreaterThan;
//					splitValueCheck	= ">";
//					foundCheckType = true;
//				} else if (splitCondition.Contains ("<")) {
//					checkType = VariableCheckType.LessThan;
//					splitValueCheck	= "<";
//					foundCheckType = true;
//				}
//			}
//
//			bool foundValue = false;
//			if (foundVariable && foundCheckType) {
//				////Debug.Log ("Found check type " + splitValueCheck + " and variable " + variableName);
//				string[] splitVariableValue = splitCondition.Split (new string [] { splitValueCheck }, StringSplitOptions.RemoveEmptyEntries);
//				if (splitVariableValue.Length > 1) {
//					variableValue = Int32.Parse (splitVariableValue [1]);
//					foundValue = true;
//				}
//			}
//
//			//OK we have everything we need
//			if (foundCheckType && foundVariable && foundValue) {
//				RequireConversationVariable requireConversationVariable = new RequireConversationVariable ();
//				requireConversationVariable.CheckType = checkType;
//				requireConversationVariable.VariableName	= variableName;
//				requireConversationVariable.VariableValue	= variableValue;
//				////Debug.Log ("Adding RequireConversationVariable " + requireConversationVariable.CheckType.ToString ( ) + ", " + requireConversationVariable.VariableName + ", " + requireConversationVariable.VariableValue.ToString ( ));
//				exchange.Scripts.Add (requireConversationVariable);
//			}
//		}
//	}
//
//	public void 								InterpretScript (string script, Exchange exchange, HashSet <int> manualOptionLinks)
//	{
//		if (script.Contains ("#frontiers")) {	//this is a custom frontiers script
//			InterpretCustomScript (script, exchange);
//		} else if (script.Contains ("#substitutedts ")) {
//			script = script.Replace ("#substitutedts ", "");
//			SubstituteConversation sc = new SubstituteConversation ();
//			if (script.Contains ("CharacterName")) {	//if it contains a character name then we're doing a new conversation for a different character
//				string[] subParts = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//				foreach (string subPart in subParts) {	//if it contains an = then it's the conversation specification
//					if (subPart.Contains ("=")) {
//						//Debug.Log ("this must be the old and new " + subPart);
//						string[] oldAndNew = subPart.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
//						sc.OldConversationName = GetNameFromDialogName (oldAndNew [0]);
//						sc.NewConversationName = GetNameFromDialogName (oldAndNew [1]);
//					} else {
//						//Debug.Log ("This must be the character name: " + subPart);
//						sc.CharacterName = subPart;
//					}
//				}
//			} else {	//otherwise it'll just be the conversation name
//				sc.CharacterName = string.Empty;
//				sc.OldConversationName = string.Empty;
//				sc.NewConversationName = GetNameFromDialogName (script);
//				sc.DTSOverride = true;
//			}
//			exchange.Scripts.Add (sc);
//		} else if (script.Contains ("#exchangename ")) {
//			exchange.Name = script.Replace ("#exchangename ", "");
//		}
//		////Debug.Log ("Interpreting script " + script);
//		else if (script.Contains ("#alwaysinclude")) {
//			exchange.AlwaysInclude = true;
//		} else if (script.Contains ("#off-forever")) {
//			exchange.Availability = AvailabilityBehavior.Once;
//		} else if (script.Contains ("#siblings-off")) {
//			////Debug.Log ("Setting siblings off");
//			if (exchange.OutgoingStyle != ExchangeOutgoingStyle.Stop) {	//Stop overrides siblings off
//				exchange.OutgoingStyle = ExchangeOutgoingStyle.SiblingsOff;
//			} else {
//				////Debug.Log ("Couldn't set to siblings off, already set to stop");
//			}
//		}
//		//look for on/off
//		else if (script.Contains ("#on")) {
//			////Debug.Log ("Checking for manually linked options");
//			script = script.Replace ("#on", "");
//			string[] manualOptions = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//			foreach (string manualOption in manualOptions) {
//				////Debug.Log ("Checking manual option " + manualOption);
//				int manualOptionIndex = 0;
//				if (Int32.TryParse (manualOption, out manualOptionIndex)) {
//					////Debug.Log ("adding manual option " + manualOption + " (" + manualOptionIndex.ToString ( ) + ")");
//					manualOptionLinks.Add (manualOptionIndex);
//				}
//			}
//			return;
//		} else if (script.Contains ("#substitute")) {
//			script = script.Replace ("#substitute ", "");
//			SubstituteConversation sc = new SubstituteConversation ();
//			if (script.Contains ("CharacterName")) {	//if it contains a character name then we're doing a new conversation for a different character
//				string[] subParts = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//				foreach (string subPart in subParts) {	//if it contains an = then it's the conversation specification
//					if (subPart.Contains ("=")) {
//						//Debug.Log ("this must be the old and new " + subPart);
//						string[] oldAndNew = subPart.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
//						sc.OldConversationName = GetNameFromDialogName (oldAndNew [0]);
//						sc.NewConversationName = GetNameFromDialogName (oldAndNew [1]);
//					} else {
//						//Debug.Log ("This must be the character name: " + subPart);
//						sc.CharacterName = GetNameFromDialogName (subPart);
//					}
//				}
//			} else {	//otherwise it'll just be the conversation name
//				sc.CharacterName = string.Empty;
//				sc.OldConversationName = string.Empty;
//				sc.NewConversationName = script;
//				sc.DTSOverride = false;
//			}
//			exchange.Scripts.Add (sc);
//		} else if (script.Contains ("#hidevar")) {
//			script = script.Replace ("#hidevar ", "");
//			ShowVariable sv = new ShowVariable ();
//			sv.Show = false;
//			sv.VariableName = script;
//			exchange.Scripts.Add (sv);
//		} else if (script.Contains ("#showvar")) {
//			script = script.Replace ("#showvar ", "");
//			ShowVariable sv = new ShowVariable ();
//			sv.Show = true;
//			sv.VariableName = script;
//			exchange.Scripts.Add (sv);
//		} else {
//			//if we've gotten this far
//			//look for set variables
//			if (script.Contains ("#set")) {	//we're setting variables, so add scripts
//				////Debug.Log ("Script " + script + " contains #set");
//				script = script.Replace (" = ", "=");
//				string[] setPieces = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//				//first one will be #set, next will be variable
//				string variableSetCommand = setPieces [1];
//				foreach (KeyValuePair <string, SimpleVar> variable in variables) {
//					string incrementCheck = variable.Key + "++";
//					string decrementCheck = variable.Key + "--";
//					string setCheck = "=";
//					////Debug.Log ("Looking for " + incrementCheck + " and " + decrementCheck);
//					if (variableSetCommand.Contains (incrementCheck)) {
//						//check for increments
//						bool alreadySet = false;
//						foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
//							if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already incrementing this variable - if so increment it more
//								ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
//								if (changeConversationVariable.VariableName == variable.Key
//								    && changeConversationVariable.ChangeType == ChangeVariableType.Increment) {	//increment it once more
//									changeConversationVariable.SetValue++;
//									alreadySet = true;
//								}
//							}
//						}
//						//if we got this far and didn't set it, make a new one
//						if (!alreadySet) {
//							ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
//							newChangeConvoVariable.CallOn = ExchangeAction.Choose;
//							newChangeConvoVariable.VariableName = variable.Key;
//							newChangeConvoVariable.ChangeType = ChangeVariableType.Increment;
//							newChangeConvoVariable.SetValue = 1;
//							exchange.Scripts.Add (newChangeConvoVariable);
//						}
//					} else if (variableSetCommand.Contains (decrementCheck)) {
//						bool alreadySet = false;
//						foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
//							if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already incrementing this variable - if so increment it more
//								ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
//								if (changeConversationVariable.VariableName == variable.Key
//								    && changeConversationVariable.ChangeType == ChangeVariableType.Decrement) {	//increment it once more
//									changeConversationVariable.SetValue++;
//									alreadySet = true;
//								}
//							}
//						}
//						//if we got this far and didn't set it, make a new one
//						if (!alreadySet) {
//							ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
//							newChangeConvoVariable.CallOn = ExchangeAction.Choose;
//							newChangeConvoVariable.VariableName = variable.Key;
//							newChangeConvoVariable.ChangeType = ChangeVariableType.Decrement;
//							newChangeConvoVariable.SetValue = 1;
//							exchange.Scripts.Add (newChangeConvoVariable);
//						}
//					} else if (variableSetCommand.Contains (setCheck)) {
//						//replace spaces so we can split along the =
//						variableSetCommand = variableSetCommand.Replace (" ", "");
//						string[] splitSetArray	= variableSetCommand.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
//						bool alreadySet = false;
//						if (splitSetArray.Length > 1) {
//							int newValue = Int32.Parse (splitSetArray [1]);
//							foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
//								if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already setting this variable - if so replace the set value
//									ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
//									if (changeConversationVariable.VariableName == variable.Key
//									    && changeConversationVariable.ChangeType == ChangeVariableType.SetValue) {	
//										changeConversationVariable.SetValue = newValue;
//										alreadySet = true;
//									}
//								}
//							}
//							//if we got this far and didn't set it, make a new one
//							if (!alreadySet) {
//								ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
//								newChangeConvoVariable.CallOn = ExchangeAction.Choose;
//								newChangeConvoVariable.VariableName = variable.Key;
//								newChangeConvoVariable.ChangeType = ChangeVariableType.SetValue;							
//								newChangeConvoVariable.SetValue = newValue;
//								exchange.Scripts.Add (newChangeConvoVariable);
//							}
//						}
//					}
//				}
//			}
//		}
//	}
//
//	public ExchangeScript						InterpretCustomScript (string script, Exchange exchange)
//	{
//		ExchangeScript exchangeScript = null;
//		//creates a script from sett
//		string scriptType = string.Empty;
//		Dictionary <string, string> values = new Dictionary <string, string> ();
//
//		string[] splitScript = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
//		//#frontiers|NameOfType, var=value, var=value
//
//		foreach (string splitScriptPiece in splitScript) {
//			if (splitScriptPiece.Contains ("#frontiers")) {
//				////Debug.Log ("Checking split script type " + splitScriptPiece);
//				//this declares the name of the type
//				//#frontiers, NameOfType
//				string[] splitScriptType = splitScript [0].Split (new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
//				scriptType = splitScriptType [1];
//			} else if (splitScriptPiece.Contains ("=")) {
//				////Debug.Log ("Checking split script value " + splitScriptPiece);
//				//this contains a value
//				string[] splitScriptValue = splitScriptPiece.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
//				try {
//					values.Add (splitScriptValue [0], splitScriptValue [1]);
//				} catch (Exception e) {
//					//Debug.Log ("Had trouble with split script value (length " + splitScriptValue.Length.ToString () + e.ToString ()); 
//				}
//			}
//		}
//
//		//did we get what we need?
//		if (!string.IsNullOrEmpty (scriptType) && values.Count > 0) {
//			////Debug.Log ("Creating script of type " + scriptType);
//			Type exchangeScriptType = Type.GetType ("Frontiers.Story." + scriptType);
//			if (exchangeScriptType != null) {
//				exchangeScript = (ExchangeScript)Activator.CreateInstance (exchangeScriptType);
//				//set values using reflection
//				foreach (KeyValuePair <string, string> checkPair in values) {
//					KeyValuePair <string, string> valuePair = checkPair;
//					//make sure ConversationName variables are conformed!
//					if (valuePair.Key.Contains ("ConversationName")) {
//						valuePair = new KeyValuePair<string, string> (valuePair.Key, DialogDesignerImport.GetNameFromDialogName (valuePair.Value));
//					}
//
//					System.Reflection.FieldInfo fieldInfo = exchangeScriptType.GetField (valuePair.Key);
//					if (fieldInfo != null) {
//						System.ComponentModel.StringConverter converter = new System.ComponentModel.StringConverter ();
//						////Debug.Log ("Found field " + valuePair.Key + ", setting to " + valuePair.Value);
//						Type fieldType = fieldInfo.FieldType;
//						System.Object convertedValue = null;
//						switch (fieldType.Name) {	//custom for custom enums
//						case "Int32":
//							convertedValue = (int)Int32.Parse (valuePair.Value);
//							break;
//
//						case "Boolean":
//							convertedValue = (bool)Boolean.Parse (valuePair.Value);
//							break;
//
//						case "String":
//							convertedValue = valuePair.Value;
//							break;
//
//						case "VariableCheckType":
//						case "MissionStatus":
//						case "MissionOriginType":
//						case "EmotionalState":
//						case "LiveTargetType":
//						case "MotileActionPriority":
//						case "AvailabilityBehavior":
//						case "ExchangeAction":
//							////Debug.Log ("Parsing enum " + valuePair.Value);
//							convertedValue = Enum.Parse (fieldType, valuePair.Value, true);
//							break;
//
//						case "MotileAction":
//							//this is a biggie...
//							//we have to split up the values and assign them separately
//							MotileAction action = new MotileAction ();
//							MobileReference mobileReference = new MobileReference ();
//							string[] splitActionVars = valuePair.Value.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
//							switch (splitActionVars [0]) {
//							case "GoToQuestActionNode":
//								//should be 3 more variables - mobileReference, expiration and a float value IF expiration is not Never
//								string[] splitMobileReference = splitActionVars [1].Split (new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
//								mobileReference.GroupPath = splitMobileReference [0];
//								mobileReference.FileName = splitMobileReference [1];
//								action.Target = mobileReference;
//
//								action.Expiration = (MotileExpiration)Enum.Parse (typeof(MotileExpiration), splitActionVars [1]);
//								switch (action.Expiration) {
//								case MotileExpiration.Duration:
//									action.RTDuration = float.Parse (splitActionVars [2]);
//									break;
//									
//								case MotileExpiration.TargetInRange:
//									action.Range = float.Parse (splitActionVars [2]);
//									break;
//									
//								case MotileExpiration.TargetOutOfRange:
//									action.OutOfRange = float.Parse (splitActionVars [2]);
//									break;
//									
//								case MotileExpiration.Never:
//								default:
//									//no last value
//									break;
//								}
//								break;
//
//							case "FollowPlayer":
//								action.Type = MotileActionType.FollowTargetHolder;
//								action.FollowType = MotileFollowType.Follower;
//								action.Expiration = MotileExpiration.Never;
//								break;
//
//							default:
//								break;
//							}
//							convertedValue = action;
//							break;
//
//						case "List`1":
//							////Debug.Log ("Parsing string list");
//							string[] splitValues = valuePair.Value.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
//							convertedValue = new List <string> (splitValues);
////							foreach (string splitValue in splitValues)
////							{
////								//Debug.Log (splitValue);
////							}
//							break;
//
//						default:
//							////Debug.Log ("Type of : " + fieldType.Name);
//							convertedValue = converter.ConvertTo (valuePair.Value, fieldType);
//							break;
//						}
//						fieldInfo.SetValue (exchangeScript as System.Object, convertedValue);
//					}
//				}
//				if (exchange != null) {
//					exchange.Scripts.Add (exchangeScript);
//				}
//			} else {
//				////Debug.Log ("type " + scriptType + " returned no system type");
//			}
//		}
//		return exchangeScript;
//	}
//
//	public void									LinkManualOptions (Exchange currentExchange)
//	{
//		HashSet <int> manualOptionsList = null;
//		if (manualOptions.TryGetValue (currentExchange.Name, out manualOptionsList)) {
//			foreach (int manualOptionIndex in manualOptionsList) {
//				string exchangeName = string.Empty;
//				if (exchangeLookup.TryGetValue (manualOptionIndex, out exchangeName)) {
//					if (!currentExchange.LinkedOutgoingChoices.Contains (exchangeName)) {
//						////Debug.Log ("Adding manual optino " + exchangeName + " (index " + manualOptionIndex.ToString ( ) + ") to exchange " + currentExchange.Name);
//						currentExchange.LinkedOutgoingChoices.Add (exchangeName);
//					}
//				}
//			}
//		}
//		foreach (Exchange childExchange in currentExchange.OutgoingChoices) {
//			LinkManualOptions (childExchange);
//		}
//	}
//
//	public string 								StripText (string optionText)
//	{
//		if (string.IsNullOrEmpty (optionText)) {
//			return string.Empty;	
//		}
//		
//		string strippedText = optionText.Replace ("]]>", "");
//		strippedText = strippedText.Replace ("![CDATA[", "");
//		return strippedText;
//	}
//	#endif
//	public static string 						GetNameFromDialogName (string fileName)
//	{
//		fileName = fileName.Replace (".dlg", "");
//		fileName = fileName.Replace (".xml", "");
//		fileName = fileName.Replace ("_", " ");
//		fileName = fileName.Replace ("-", " ");
//		fileName = fileName.ToLower ();
//		//now we're ready to capitalize all first letters
//		TextInfo textInfo = new CultureInfo ("en-US", false).TextInfo;
//		fileName = textInfo.ToTitleCase (fileName);
//		//turn this shit into regex, please
//		string number = new string (fileName [fileName.Length - 1], 1);
//		int intNum = 0;
//		if (!Int32.TryParse (number, out intNum)) {	//if the last char isn't a number, add one
//			fileName += " 01";
//		}
//		fileName = fileName.Replace (" 1", " 01");
//		fileName = fileName.Replace (" 2", " 02");
//		fileName = fileName.Replace (" 3", " 03");
//		fileName = fileName.Replace (" 4", " 04");
//		fileName = fileName.Replace (" 5", " 05");
//		fileName = fileName.Replace (" 6", " 06");
//		fileName = fileName.Replace (" 7", " 07");
//		fileName = fileName.Replace (" 8", " 08");
//		fileName = fileName.Replace (" 9", " 09");
//		fileName = fileName.Replace (" ", "-");
//		return fileName;
//	}
//}