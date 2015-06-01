using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;
using Frontiers.GUI;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;

namespace Frontiers
{
	public class PlayerStatus : PlayerScript, IGUIParentEditor <MessageCancelDialogResult>
	{
		public PlayerStatusState State = new PlayerStatusState();

		public GameObject NGUIObject { get { return gameObject; } set { return; } }

		public string LatestCauseOfDeath = string.Empty;
		public TemperatureRange LatestTemperatureRaw;
		public TemperatureRange LatestTemperatureAdjusted;
		public TemperatureRange LatestTemperatureExposure;
		public double CheckConditionsInterval = 0.15f;
		public List <AvatarAction> RecentActions = new List <AvatarAction>();
		public List <string> ActiveStateList = new List <string>();
		public List <string> CustomStateList = new List <string>();
		public bool RefreshKeeperStates = false;
		public List <StatusKeeper> StatusKeepers = new List<StatusKeeper>();

		public bool IsStateActive(string stateName)
		{	//TODO make this better
			return ActiveStateList.Contains(stateName);
		}

		#region initialization

		public override void OnGameLoadFinish()
		{
			HashSet <Condition> activeConditions = new HashSet<Condition>();

			mStatusKeeperLookup.Clear();
			List <string> StatusKeeperNames = Mods.Get.Available("StatusKeeper");
			for (int i = 0; i < StatusKeeperNames.Count; i++) {
				StatusKeeper statusKeeper = null;
				if (Mods.Get.Runtime.LoadMod <StatusKeeper>(ref statusKeeper, "StatusKeeper", StatusKeeperNames[i])) {
					statusKeeper.Initialize();
					mStatusKeeperLookup.Add(statusKeeper.Name, statusKeeper);
					StatusKeepers.Add(statusKeeper);
					if (statusKeeper.Name.Contains("Reputation")) {
						statusKeeper.DefaultState.UseNeutralUrgency = true;
						foreach (StatusKeeperState state in statusKeeper.AlternateStates) {
							state.UseNeutralUrgency = true;
						}
					} else if (statusKeeper.Name.Contains("Temperature")) {
						statusKeeper.DefaultState.UseNeutralUrgency = false;
						foreach (StatusKeeperState state in statusKeeper.AlternateStates) {
							state.UseNeutralUrgency = false;
						}
					}
				}
			}
			StatusKeepers.Sort();
			//check to see whether the status value is zero
			StatusKeeper health = null;
			if (mStatusKeeperLookup.TryGetValue("Health", out health) && Mathf.Approximately(health.Value, 0f)) {
				//if it's zero odds are the status keepers haven't been initialized to default values
				for (int i = 0; i < StatusKeepers.Count; i++) {
					StatusKeepers[i].Reset();
				}
			}
		}

		public override void OnGameStartFirstTime()
		{
			ResetStatusKeepers();
		}

		public override void OnGameStart()
		{
			if (GameManager.Get.TestingEnvironment)
				return;

			Player.Get.AvatarActions.Subscribe(AvatarAction.ItemCraft, new ActionListener(ItemCraft));
			Player.Get.AvatarActions.Subscribe(AvatarAction.ItemCraft, new ActionListener(ItemCarry));

			Player.Get.AvatarActions.Subscribe(AvatarAction.FastTravelStart, new ActionListener(FastTravelStart));
			Player.Get.AvatarActions.Subscribe(AvatarAction.FastTravelStop, new ActionListener(FastTravelStop));

			Player.Get.AvatarActions.Subscribe(AvatarAction.NpcConverseStart, new ActionListener(InteractionWithCharacterStart));
			Player.Get.AvatarActions.Subscribe(AvatarAction.NpcConverseEnd, new ActionListener(InteractionWithCharacterFinish));

			player.Surroundings.OnExposureDecrease += OnExposureDecrease;
			player.Surroundings.OnExposureIncrease += OnExposureIncrease;
			player.Surroundings.OnHeatDecrease += OnHeatDecrease;
			player.Surroundings.OnHeatIncrease += OnHeatIncrease;

			GUIPlayerStatusInterface.Get.Initialize();

			enabled = true;
		}

		public override void OnLocalPlayerDie()
		{
			RecentActions.Clear();
			ClearConditions();
			ActiveStateList.Clear();
			ResetStatusKeepers();
		}

		public override void OnLocalPlayerDespawn()
		{
			RecentActions.Clear();
			ClearConditions();
			ActiveStateList.Clear();
			ResetStatusKeepers();
		}

		public void ClearConditions()
		{
			for (int i = 0; i < State.ActiveConditions.Count; i++) {
				State.ActiveConditions[i].Cancel();
			}
			State.ActiveConditions.Clear();
		}

		public override bool SaveState(out string playerState)
		{
			//we have to save our status keepers
			for (int i = 0; i < StatusKeepers.Count; i++) {
				Mods.Get.Runtime.SaveMod <StatusKeeper>(StatusKeepers[i], "StatusKeeper", StatusKeepers[i].Name);
			}
			return base.SaveState(out playerState);
		}

		public void ResetStatusKeepers()
		{
			for (int i = 0; i < StatusKeepers.Count; i++) {
				StatusKeepers[i].Reset();
				Mods.Get.Runtime.SaveMod <StatusKeeper>(StatusKeepers[i], "StatusKeeper", StatusKeepers[i].Name);
			}
		}

		public override void OnLocalPlayerSpawn()
		{
			if (SpawnManager.Get.UseStartupPosition) {
				for (int i = 0; i < SpawnManager.Get.CurrentStartupPosition.StatusValues.Count; i++) {
					StatusKeeper statusKeeper = null;
					if (GetStatusKeeper(SpawnManager.Get.CurrentStartupPosition.StatusValues[i].StatusKeeperName, out statusKeeper)) {
						statusKeeper.SetValue(SpawnManager.Get.CurrentStartupPosition.StatusValues[i].Value, false);
					}
				}
			}

			RecentActions.Clear();

			for (int i = State.ActiveConditions.LastIndex (); i >= 0; i--) {
				if (State.ActiveConditions [i].HasExpired) {
					State.ActiveConditions.RemoveAt (i);
				}
			}

			if (!mCheckingStatusKeepers) {
				mCheckingStatusKeepers = true;
				StartCoroutine(CheckStatusKeepers());
			}
			if (!mCheckingActiveConditions) {
				mCheckingActiveConditions = true;
				StartCoroutine(CheckActiveConditions());
			}
			if (!mCheckingEnvironment) {
				mCheckingEnvironment = true;
				StartCoroutine(CheckEnvironment());
			}

			StartCoroutine(CheckActiveStateList());

			for (int i = 0; i < State.ActiveConditions.Count; i++) {
				Condition activeCondition = State.ActiveConditions[i];
				for (int j = 0; j < StatusKeepers.Count; j++) {
					StatusKeeper statusKeeper = StatusKeepers[j];
					if (activeCondition.HasSymptomFor(statusKeeper.Name)) {//just add it to the conditions list
						//don't bother with ReceiveCondition
						//we'll check for expiration later
						statusKeeper.Conditions.Add(activeCondition);
					}
				}
			}
		}

		#endregion

		public void OnRespawnBedFound(Bed respawnBed)
		{
			player.State.Transform.Position = respawnBed.BedsidePosition;
			player.Spawn();
			respawnBed.TryToSleep(WorldClock.Get.TimeOfDayAfter(WorldClock.TimeOfDayCurrent));
		}

		public bool TryToWakeUp(string cause)
		{
			if (!State.IsSleeping)
				return false;

			if (mChildEditor != null) {
				GUIEditor <MessageCancelDialogResult> editor = mChildEditor.GetComponent <GUIEditor <MessageCancelDialogResult>>();
				editor.ActionCancel(WorldClock.AdjustedRealTime);
				return false;
			}

			Player.Local.Position = mLastBed.BedsidePosition;
			Player.Local.State.Transform.Position = Player.Local.Position;
			Player.Local.RestoreControl(false);
			Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalWakeUp, WorldClock.AdjustedRealTime);
			WorldClock.Get.SetTargetSpeed(1.0f);
			State.IsSleeping = false;
			State.LastSleepInterruption = cause;
			double timeSlept = WorldClock.SecondsToHours(WorldClock.AdjustedRealTime - State.StartSleepTime);
			if (timeSlept > Globals.WellRestedHours) {
				AddCondition("WellRested");
			}
			return true;
		}

		public bool TryToSleep(Bed bed, TimeOfDay timeOfDay)
		{
			if (State.IsSleeping)
				return false;

			if (player.Status.HasCondition("BurnedByFire")) {
				GUIManager.PostDanger("You're on fire! Put yourself out first!");
				return false;
			}

			if (player.Surroundings.IsInDanger) {
				GUIManager.PostDanger("You can't sleep while you're in danger");
				return false;
			}

			mLastBed = bed;

			//get the time we want to sleep until
			State.SleepTarget = WorldClock.FutureTime(WorldClock.Get.HoursUntilTimeOfDay(timeOfDay), TimeUnit.Day);
			//set the player's hijack points
			player.HijackControl();
			player.State.HijackMode = PlayerHijackMode.LookAtTarget;
			player.SetHijackTargets(bed.CameraTargetPosition, bed.CameraLookTarget);
			//create the dialog
			MessageCancelDialogResult result = new MessageCancelDialogResult();
			result.Message = "Sleeping...";
			result.CanCancel = true;
			result.CancelButton = "Wake up";
			mChildEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIMessageCancelDialog, false);
			GUIManager.SendEditObjectToChildEditor <MessageCancelDialogResult>(new ChildEditorCallback <MessageCancelDialogResult>(ReceiveFromChildEditor), mChildEditor, result);

			//start sleeping and send avatar action
			State.IsSleeping = true;
			State.StartSleepTime = WorldClock.AdjustedRealTime;
			Player.Get.AvatarActions.ReceiveAction((AvatarAction.SurvivalSleep), WorldClock.AdjustedRealTime);
			WorldClock.Get.SetTargetSpeed(WorldClock.gTimeScaleSleep);
			StartCoroutine(Sleep());
			return true;
		}

		public void ReceiveFromChildEditor(MessageCancelDialogResult editObject, IGUIChildEditor<MessageCancelDialogResult> childEditor)
		{
			if (!State.IsSleeping)
				return;

			mChildEditor = null;
			TryToWakeUp(string.Empty);
		}

		public bool GetStatusKeeper(string statusKeeperName, out StatusKeeper statusKeeper)
		{
			return mStatusKeeperLookup.TryGetValue(statusKeeperName, out statusKeeper);
		}

		public float GetStatusValue(string statusKeeperName)
		{
			if (mStatusKeeperLookup.TryGetValue(statusKeeperName, out mStatusKeeperValueCheck)) {
				return mStatusKeeperValueCheck.Value;
			}
			return 0f;
		}

		StatusKeeper mStatusKeeperValueCheck = null;

		#region StartFinish

		public IEnumerator Sleep()
		{
			while (State.IsSleeping && WorldClock.AdjustedRealTime < State.SleepTarget) {
				if (player.Surroundings.IsInDanger) {
					TryToWakeUp("In Danger");
					yield break;
				}
				yield return null;
			}
			TryToWakeUp("Finished");
			yield break;
		}

		public bool InteractionWithCharacterStart(double timeStamp)
		{
			State.IsInteractingWithCharacter = true;
			return true;
		}

		public bool InteractionWithCharacterFinish(double timeStamp)
		{
			State.IsInteractingWithCharacter = false;
			return true;
		}

		public bool FastTravelStart(double timeStamp)
		{
			State.IsTraveling = true;
			return true;
		}

		public bool FastTravelStop(double timeStamp)
		{
			State.IsTraveling = false;
			return true;
		}

		#endregion

		#region conditions and status

		public void AddCondition(string conditionName)
		{
			Debug.Log ("Adding condition " + conditionName);
			bool conditionAdded = false;
			//first see if the condition is already present
			//if it is, don't clone the condition, stack it instead of creating a new one
			for (int i = 0; i < State.ActiveConditions.Count; i++) {
				Condition activeCondition = State.ActiveConditions[i];
				if (string.Equals(activeCondition.Name, conditionName) && !activeCondition.HasExpired) {//double its duration so it'll last twice as long
					Debug.Log ("Condition is already active, not adding new");
					activeCondition.IncreaseDuration(activeCondition.Duration * Globals.StatusKeeperTimecale);
					return;
				}
			}

			Condition condition = null;
			if (Frontiers.Conditions.Get.ConditionByName (conditionName, out condition)) {	//if we have no active conditions try looking it up
				Debug.Log ("Condition found, adding now");
				condition.Initialize ();
				State.ActiveConditions.Add (condition);
				for (int i = 0; i < StatusKeepers.Count; i++) {
					StatusKeeper statusKeeper = StatusKeepers [i];
					if (condition.HasSymptomFor (statusKeeper.Name)) {
						conditionAdded = true;
						statusKeeper.ReceiveCondition (condition);
					}
				}
			} else {
				Debug.Log ("Condition " + conditionName + " not found");
			}

			if (conditionAdded) {
				Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalConditionAdd, WorldClock.RealTime);
			}
		}

		public void RemoveCondition(string conditionName)
		{
			bool conditionRemoved = false;
			for (int i = State.ActiveConditions.LastIndex(); i >= 0; i--) {
				if (State.ActiveConditions[i].Name.Equals(conditionName)) {
					State.ActiveConditions[i].Cancel();//it will be removed on the next check
					State.ActiveConditions.RemoveAt(i);
					conditionRemoved = true;
				}
			}
			if (conditionRemoved) {
				Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalConditionRemove, WorldClock.RealTime);
			}
		}

		public void RestoreStatus(PlayerStatusRestore restore, string type)
		{
			float restoreToFloat = Frontiers.Status.RestoreToFloat(restore);
			RestoreStatus(restoreToFloat, type);
		}

		public void RestoreStatus(PlayerStatusRestore restore, string type, float multiplier)
		{
			float restoreToFloat = Frontiers.Status.RestoreToFloat(restore) * multiplier;
			RestoreStatus(restoreToFloat, type);
		}

		public void RestoreStatus(float restore, string type)
		{
			StatusKeeper status = null;
			if (GetStatusKeeper(type, out status)) {
				status.ChangeValue(restore, StatusSeekType.Positive);
			}
		}

		public void ReduceStatus(float reduce, string type)
		{
			StatusKeeper status = null;
			if (GetStatusKeeper(type, out status)) {
				status.ChangeValue(reduce, StatusSeekType.Negative);
				if (status.Value < 0f) {
					//we're taking away from something that's already really low
					status.Ping = true;
				}
			}
		}

		public void ReduceStatus(PlayerStatusRestore reduce, string type, float multiplier)
		{
			ReduceStatus(Frontiers.Status.RestoreToFloat(reduce) * multiplier, type);
		}

		public void ReduceStatus(PlayerStatusRestore reduce, string type)
		{
			ReduceStatus(Frontiers.Status.RestoreToFloat(reduce), type);
		}

		public bool HasCondition(string conditionName)
		{	//TODO move this into a lookup
			for (int i = 0; i < State.ActiveConditions.Count; i++) {
				Condition condition = State.ActiveConditions[i];
				//foreach (Condition condition in State.ActiveConditions) {
				if (condition.Name == conditionName && !condition.HasExpired) {
					return true;
				}
			}
			return false;
		}

		#endregion

		protected IEnumerator CheckActiveConditions()
		{
			mLastConditionCheckTime = WorldClock.AdjustedRealTime;

			while (mCheckingActiveConditions) {

				while (!GameManager.Is(FGameState.InGame)) {
					yield return null;
				}

				while (!player.HasSpawned) {
					mLastConditionCheckTime = WorldClock.AdjustedRealTime;
					yield return null;
				}

				//use the last checked time to get a delta time
				double deltaTime = (WorldClock.AdjustedRealTime - mLastConditionCheckTime) * Globals.StatusKeeperTimecale;
				float lightExposure = player.Surroundings.LightExposure;
				float heatExposure = player.Surroundings.HeatExposure;

				for (int i = State.ActiveConditions.LastIndex(); i >= 0; i--) {
					//Debug.Log ("Checking condition " + State.ActiveConditions [i].Name + " with delta time " + deltaTime.ToString());
					if (State.ActiveConditions [i].CheckExpired (
						    deltaTime,
							lightExposure,
							heatExposure,
						    RecentActions,
						    State.ActiveConditions,
						    ActiveStateList)) {	//it's toast, remove it
						//Debug.Log ("Condition " + State.ActiveConditions [i].Name + " is expired, removing");
						State.ActiveConditions.RemoveAt (i);
						Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalConditionRemove, WorldClock.RealTime);
					}
					yield return null;//TODO check if this is wise?
				}
				mLastConditionCheckTime = WorldClock.AdjustedRealTime;
				double waitUntil = WorldClock.AdjustedRealTime + 0.1f;
				while (WorldClock.AdjustedRealTime > waitUntil) {
					yield return null;
				}
				yield return null;
			}
			mCheckingActiveConditions = false;
			yield break;
		}
		//we keep an active state list of strings in addition to our local settings
		//this is make it easy for modders who want to add new states that affect the player
		//status keepers use this list to determine their active state
		//this picks apart our current location info and figures out what states to apply to status keepers
		protected IEnumerator CheckActiveStateList()
		{		//state order
			//0 - Sleeping			- general / outside or in unowned/unsafe structure
			//1 - SleepingSafely	- in campsite or in bed in owned structure
			//3 - ExposedCold		- outside in cold weather
			//4 - ExposedWarm		- outside in warm weather
			//2 - InSafeLocation 	- includes owned structures
			//3 - InCivilization 	- most structures and cities, campsites, path connected to civ
			//4 - Traveling			- fast travel
			//5 - InWild			- default state
			//6 - InDanger			- maybe when there are hostiles around, gives temporary strength boost (?) haven't decided

			mCheckingActiveStateList = true;
			while (!mInitialized) {
				yield return null;
			}
		
			while (mCheckingActiveStateList) {	//this is the info we need to build the state list
				while (!GameManager.Is(FGameState.InGame)) {
					yield return null;
				}

				if (player.IsDead) {
					//wait until we're not dead
					yield return null;
				}

				bool insideStructure = false;
				bool isUnderground = false;			
				bool isExposedCold = false;
				bool isExposedWarm = false;			
				bool sleeping = false;
				bool inSafeLocation = false;
				bool inCivilization = false;
				bool sleepingSafely = false;
				bool traveling = false;
				bool inDanger = false;
				bool isWarmedByFire = false;

				traveling = State.IsTraveling;
				if (!traveling) {	//we can rule a few things out if we're not travling...
					sleeping = State.IsSleeping;
					insideStructure	= player.Surroundings.IsInsideStructure || (sleeping && mLastBed.IsEnclosed);
					inCivilization	= player.Surroundings.IsInCivilization;
					inSafeLocation	= player.Surroundings.IsInSafeLocation;
					isUnderground = player.Surroundings.IsUnderground;
					sleepingSafely	= sleeping && (inSafeLocation);// || (inCivilization && player.Surroundings.CurrentLocation.State.Type == "Campsite"));//TODO potentially unsafe
				}

				bool checkForFire = false;
				//check our termperature - the biome will return the correct temperature based on tiem of day / season
				//this is the 'raw' temperature before we apply clothing and other modifiers to it
				LatestTemperatureRaw = GameWorld.Get.StatusTemperature(player.Position, WorldClock.TimeOfDayCurrent, WorldClock.TimeOfYearCurrent, isUnderground, insideStructure, inCivilization);
				LatestTemperatureAdjusted = LatestTemperatureRaw;
				if (player.Surroundings.HasNearbyFires) {
					//fires make our temperature higher
					if (player.Surroundings.HeatExposure > 1f) {
						//we're exposed to burning temperatures!
						LatestTemperatureAdjusted = TemperatureRange.E_DeadlyHot;
						//check to see if we're on fire later
						checkForFire = true;
					} else {
						isWarmedByFire = true;
						//we're not burning but we are warmer than usual
						//if it's deadly cold or cold bump up the temperature
						switch (LatestTemperatureRaw) {
							case TemperatureRange.A_DeadlyCold:
							case TemperatureRange.B_Cold:
																//no penalty for being near a fire, ever
														//case TemperatureRange.C_Warm:
								LatestTemperatureAdjusted = (TemperatureRange)((int)LatestTemperatureRaw + 1);
								break;

							default:
								break;
						}
					}
				}

				//finally, use our clothing to determine our temperature exposure
				LatestTemperatureExposure = player.Wearables.AdjustTemperatureExposure(LatestTemperatureAdjusted);
				//now that we've adjusted for clothing, if we were supposed to check for fire, do it now
				//TEMP disabled
				/*if (checkForFire && LatestTemperatureExposure == TemperatureRange.E_DeadlyHot) {
										AddCondition("BurnedByFire");
								}*/
				//we've given the player a million chances to get their temp up / down to 'Warm'
				//so at this point if they haven't done it they're going to suffer a penalty
				switch (LatestTemperatureExposure) {
					case TemperatureRange.A_DeadlyCold:
					case TemperatureRange.B_Cold:
						isExposedCold = true;
						break;

					default:
						break;

					case TemperatureRange.D_Hot:
					case TemperatureRange.E_DeadlyHot:
						isExposedWarm = true;
						break;
				}

				inDanger = player.Surroundings.IsInDanger;

				mNewStateList.Clear();

				if (sleeping) {
					mNewStateList.Add("Sleeping");
				}
				if (sleepingSafely) {
					mNewStateList.Add("SleepingSafely");
				}
				if (inSafeLocation) {
					mNewStateList.Add("InSafeLocation");
				}
				if (inCivilization) {
					mNewStateList.Add("InCivilization");
				}
				if (traveling) {
					mNewStateList.Add("Traveling");
				}
				mNewStateList.Add("Default");//always add default state (in the wild)
				if (inDanger) {
					mNewStateList.Add("InDanger");
				}
				if (insideStructure) {
					mNewStateList.Add("InsideStructure");
				} else {
					if (isUnderground) {
						mNewStateList.Add("Underground");
					} else {
						mNewStateList.Add("Outside");
					}
				}
				if (isExposedCold) {
					mNewStateList.Add("ExposedCold");
				}
				if (isExposedWarm) {
					mNewStateList.Add("ExposedWarm");
				}
				if (isWarmedByFire) {
					mNewStateList.Add("WarmedByFire");
				}

				//check whether we were in civilization last frame
				if (mInCivilizationLastFrame && !inCivilization) {
					//if we were in civ last frame and aren't now
					Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalCivilizationLeave, WorldClock.AdjustedRealTime);
				} else if (!mInCivilizationLastFrame && inCivilization) {
					//if we weren't in civ last frame and are now
					Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalCivilizationEnter, WorldClock.AdjustedRealTime);
				}
				mInCivilizationLastFrame = inCivilization;

				//automatically apply if we have more states than before
				bool applyState = (mNewStateList.Count != ActiveStateList.Count);
				if (!applyState) {	//if we have the same number, verify that they're actually the same
					for (int i = 0; i < mNewStateList.Count; i++) {	//if there's a different state in there
						if (mNewStateList[i] != ActiveStateList[i]) {	//apply the stae
							applyState = true;
							break;
						}
					}
				}
				//wait a tick
				yield return null;
				if (applyState) {
					ActiveStateList.Clear();
					ActiveStateList.AddRange(mNewStateList);
					ActiveStateList.AddRange(CustomStateList);
					for (int i = 0; i < StatusKeepers.Count; i++) {//they'll apply the states in the order provided
						StatusKeepers[i].SetState(ActiveStateList);
					}
					CheckForFlows();
				}
				double waitUntil = WorldClock.AdjustedRealTime + 0.05f;
				while (WorldClock.AdjustedRealTime > waitUntil) {
					yield return null;
				}
				yield return null;
			}
			mCheckingActiveStateList = false;
			yield break;
		}

		protected List <string> mNewStateList = new List<string>();

		protected IEnumerator CheckEnvironment()
		{
			while (mCheckingEnvironment) {
				while (!GameManager.Is(FGameState.InGame)) {
					yield return null;
				}

				while (player.IsDead) {
					//wait until we're not dead
					yield return null;
				}

				//check if we're wet
				//TODO make the actual environment do this to us, it shouldn't happen here
				if (Player.Local.Surroundings.State.IsInWater) {
					AddCondition("Wet");
				} else if (Player.Local.Surroundings.State.ExposedToRain) {
					AddCondition("Wet");
				}

				double waitUntil = WorldClock.AdjustedRealTime + 1f;
				while (WorldClock.AdjustedRealTime > waitUntil) {
					yield return null;
				}
				yield return null;
			}
			yield break;
			mCheckingEnvironment = false;
		}

		protected IEnumerator CheckStatusKeepers()
		{
			while (mCheckingStatusKeepers) {
				while (GameManager.Get.JustLookingMode) {
					yield return null;
				}

				while (player.IsDead || !player.HasSpawned) {
					//wait until we're not dead
					yield return null;
				}

				double deltaTime = WorldClock.ARTDeltaTime * Globals.StatusKeeperTimecale;
				for (int i = 0; i < StatusKeepers.Count; i++) {
					StatusKeeper statusKeeper = StatusKeepers[i];
					statusKeeper.UpdateState(deltaTime);
					statusKeeper.ApplyConditions(deltaTime, RecentActions, State.ActiveConditions, ActiveStateList);
					if (Globals.StatusKeeperNegativeFlowMultiplier > 0 || Globals.StatusKeeperPositiveFlowMultiplier > 0) {
						statusKeeper.ApplyStatusFlows(deltaTime);
					}
					//are we dead?
					if (statusKeeper.Name.Equals("Health")) {
						if (statusKeeper.NormalizedValue <= 0f) {
							//we're dead! try to die here
							player.Die(string.Empty);
						}
					}
				}
				yield return null;
			}
			mCheckingStatusKeepers = false;
			yield break;
		}

		protected GUIDeathDialog mDeathDialog = null;

		public void CheckForFlows()
		{
			mGatheredFlows.Clear();
			for (int i = 0; i < StatusKeepers.Count; i++) {
				StatusKeeper statusKeeper = StatusKeepers[i];
				StatusFlow underflow = null;
				StatusFlow overflow = null;
				if (statusKeeper.HasOverflowToSend(out overflow)) {
					List <StatusFlow> flows = null;
					if (!mGatheredFlows.TryGetValue(overflow.TargetName, out flows)) {
						flows = new List <StatusFlow>();
						mGatheredFlows.Add(overflow.TargetName, flows);
					}
					flows.Add(overflow);
				}
				if (statusKeeper.HasUnderflowToSend(out underflow)) {
					List <StatusFlow> flows = null;
					if (!mGatheredFlows.TryGetValue(underflow.TargetName, out flows)) {
						flows = new List <StatusFlow>();
						mGatheredFlows.Add(underflow.TargetName, flows);
					}
					flows.Add(underflow);
				}
			}
			//now that we've gathered all the flows
			//send them to the status keepers
			for (int i = 0; i < StatusKeepers.Count; i++) {
				StatusKeeper statusKeeper = StatusKeepers[i];
				List <StatusFlow> flows = null;
				if (mGatheredFlows.TryGetValue(statusKeeper.Name, out flows)) {
					statusKeeper.ReceiveFlows(flows);
				}
			}
		}

		protected Dictionary <string, List <StatusFlow>> mGatheredFlows = new Dictionary<string, List<StatusFlow>>();

		public void OnExposureDecrease()
		{

		}

		public void OnExposureIncrease()
		{

		}

		public void OnHeatIncrease()
		{ 
			//TODO - notification?
		}

		public void OnHeatDecrease()
		{
			//TODO - notification?
		}

		#region AvtarActions

		public bool ItemCraft(double timeStamp)
		{
			//StrengthStatusKeeper.CurrentValue -= StatusCraftStrengthReducePerMinute * WorldClock.DeltaTimeMinutes;
			//ThirstStatusKeeper.CurrentValue -= StatusCraftThirstIncreasePerMinute * WorldClock.DeltaTimeMinutes;
			return true;
		}

		public bool ItemCarry(double timeStamp)
		{
			//StrengthStatusKeeper.CurrentValue -= StatusCarryStrengthReducePerMinute * WorldClock.DeltaTimeMinutes;
			return true;
		}

		#endregion

		protected Bed mLastBed = null;
		protected GameObject mChildEditor = null;
		protected bool mInCivilizationLastFrame = false;
		protected bool mCheckingStatusKeepers = false;
		protected bool mCheckingNearbyFires = false;
		protected bool mCheckingActiveConditions = false;
		protected bool mCheckingActiveStateList = false;
		protected bool mCheckingEnvironment = false;
		protected bool mCheckingFlows = false;
		protected double mLastConditionCheckTime = 0.15f;
		protected double mLastStatusKeepersCheckTime = 0.0f;
		protected double mCheckConditionsTime = 0.0f;
		protected double mStartSleepTime = 0f;
		protected Dictionary <string, StatusKeeper> mStatusKeeperLookup = new Dictionary <string, StatusKeeper>();
	}

	[Serializable]
	public class PlayerStatusState
	{
		public double SleepTarget = 0.0f;
		public double StartSleepTime = 0.0f;
		public bool IsSleeping = false;
		public bool IsTraveling = false;
		public bool IsInteractingWithCharacter = false;
		public string LastSleepInterruption = string.Empty;
		public MobileReference LastBed;
		public MobileReference LastRespawnPoint;
		public MobileReference LastHouseOfHealing;

		public string LatestCauseOfDeath {
			get {
				if (CausesOfDeath.Count > 0) {
					return CausesOfDeath[CausesOfDeath.Count - 1];
				}
				return string.Empty;
			}
		}

		public List <string> CausesOfDeath = new List <string>();
		public List <Condition> ActiveConditions = new List <Condition>();
	}
}