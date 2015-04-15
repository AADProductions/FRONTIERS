using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers
{
		namespace World
		{
				public interface IBodyOfWater {

						float WaterHeightAtPosition(Vector3 position);
				}

				public interface IMeleeWeapon
				{
						float ImpactTime { get; }

						float UseSpeed { get; }

						float SwingDelay { get; }

						float WindupDelay { get; }

						float SwingRate { get; }

						float SwingDuration { get; }

						float SwingImpactForce { get; }

						float StrengthDrain { get; }

						float RecoilIntensity { get; }

						bool RandomSwingDirection { get; }

						string AttackState { get; }
				}

				public interface IEquippableAction
				{
						bool IsCycling { get; }

						bool CanCycle { get; }

						bool IsActive { get; }
				}

				public interface IStackOwner //TODO have IStackOwner implement IItemOfInterest
				{
						WorldItem worlditem { get; }

						bool IsWorldItem { get; }

						string DisplayName { get; }

						string StackName { get; }

						string FileName { get; }

						string QuestName { get; }

						bool UseRemoveItemSkill(HashSet <string> removeItemSkillNames, ref IStackOwner useTarget);

						List <string> RemoveItemSkills { get; }

						WISize Size { get; }

						void Refresh();
				}

				public interface ITrap
				{
						double TimeLastChecked { get; set; }

						double TimeSet { get; }

						float SkillOnSet { get; set; }

						WorldItem Owner { get; }

						bool IsFinished { get; }

						string TrappingSkillName { get; }

						TrapMode Mode { get; set; }

						bool SkillUpdating { get; set; }

						bool LastTriggerWasSuccessful { get; set; }

						bool RequiresMinimumPlayerDistance { get; }

						List <string> CanCatch { get; }

						List <string> Exceptions { get; }

						void OnCatchTarget(float skillRoll);

						List <ICreatureDen> IntersectingDens { get; }
				}

				public interface ICreatureDen : ITerritoryBase
				{
						IItemOfInterest IOI { get; }

						bool IsFinished { get; }

						string NameOfCreature { get; }

						void AddCreature(IWIBase creature);

						void SpawnCreatureCorpse(Vector3 position, string causeOfDeath, float timeSinceDeath);

						bool BelongsToPack(WorldItem worlditem);

						bool TrapsSpawnCorpse { get; }

						void CallForHelp(WorldItem creatureInNeed, IItemOfInterest sourceOfTrouble);

						GameObject gameObject { get; }
				}

				public interface ITerritoryBase
				{
						float Radius { get; }

						float InnerRadius { get; }

						Vector3 Position { get; }

						Transform transform { get; }
				}

				public interface IBank
				{
						Action RefreshAction { get; set; }

						void AddBaseCurrencyOfType(float numBaseCurrency, WICurrencyType type);

						void AddBaseCurrencyOfType(int numBaseCurrency, WICurrencyType type);

						void Add(int numCurrency, WICurrencyType type);

						bool TryToRemove(int numToRemove, WICurrencyType type, bool makeChange);

						bool TryToRemove(int numBaseCurrency);

						void Absorb(IBank otherBank);

						void Clear();

						int BaseCurrencyValue { get; }

						int Bronze { get; set; }

						int Silver { get; set; }

						int Gold { get; set; }

						int Lumen { get; set; }

						int Warlock { get; set; }
				}

				public interface IHostile
				{
						IItemOfInterest hostile { get; }

						string DisplayName { get; }

						IItemOfInterest PrimaryTarget { get; }

						bool HasPrimaryTarget { get; }

						bool CanSeePrimaryTarget { get; }

						HostileMode Mode { get; }

						void CoolOff ( );
				}

				public interface IVisible : IItemOfInterest
				{
						bool IsVisible { get; }

						float AwarenessDistanceMultiplier { get; }

						float FieldOfViewMultiplier { get; }

						void LookerFailToSee();
				}

				public interface IListener
				{
						void HearSound(IAudible source, MasterAudio.SoundType type, string sound);
				}

				public interface IAwarenessBubbleUser
				{
						Transform transform { get; }
				}

				public interface IItemOfInterest
				{
						ItemOfInterestType IOIType { get; }

						Vector3 Position { get; }

						bool Has(string scriptName);

						bool HasAtLeastOne(List <string> scriptNames);

						WorldItem worlditem { get; }

						PlayerBase player { get; }

						ActionNode node { get; }

						WorldLight worldlight { get; }

						Fire fire { get; }

						GameObject gameObject { get; }

						bool Destroyed { get; }

						bool HasPlayerFocus { get; set; }
				}

				public interface IAudible : IItemOfInterest, IAwarenessBubbleUser
				{
						bool IsAudible { get; }

						float AudibleRange { get; }

						float AudibleVolume { get; }

						MasterAudio.SoundType LastSoundType { get; set; }

						string LastSoundName { get; set; }

						void ListenerFailToHear();
				}

				public interface IDamageable : IKillable
				{
						float NormalizedDamage	{ get; }

						bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string sourceName, out float actualDamage, out bool isDead);

						WIMaterialType BaseMaterialType	{ get; }

						WIMaterialType ArmorMaterialTypes { get; }

						IItemOfInterest LastDamageSource { get; set; }

						BodyPart LastBodyPartHit { get; set; }

						int ArmorLevel(WIMaterialType materialType);

						void InstantKill(IItemOfInterest source);
				}


				public interface IBodyOwner
				{
						Vector3 Position { get; set; }

						Quaternion Rotation { get; }

						WorldBody Body { get; set; }

						bool Initialized { get; }

						bool IsImmobilized { get; }

						bool IsGrounded { get; }

						bool IsRagdoll { get; }

						bool IsDestroyed { get; }

						double CurrentMovementSpeed { get; set; }

						double CurrentRotationSpeed { get; set; }

						int CurrentIdleAnimation { get; set; }
				}

				public interface IPhotosensitive
				{
						float Radius { get; }

						Vector3 Position { get; }

						float LightExposure { get; set; }

						float HeatExposure { get; set; }

						Action OnExposureIncrease { get; set; }

						Action OnExposureDecrease { get; set; }

						Action OnHeatIncrease { get; set; }

						Action OnHeatDecrease { get; set; }

						List <WorldLight> LightSources { get; set; }

						List <Fire> FireSources { get; set; }
				}

				public interface IUnloadable
				{
						bool PrepareToUnload();

						bool ReadyToUnload { get; }

						bool TryToCancelUnload();

						void BeginUnload();

						bool FinishedUnloading { get; }
				}

				public interface IUnloadableParent
				{
						void GetChildItems(List <IUnloadableChild> unloadableChildItems);
				}

				public interface ILoadable
				{
						bool PrepareToLoad();

						bool ReadyToLoad { get; }

						bool TryToCancelLoad();

						void BeginLoad();

						bool FinishedLoading { get; }
				}

				public interface IUnloadableChild
				{
						int Depth { get; }

						bool Terminal { get; }

						bool HasUnloadingParent { get; }

						IUnloadableChild ShallowestUnloadingParent { get; }
				}

				public interface IWIBase : IStackOwner
				{
						string PackName { get; }

						string PrefabName { get; }

						string Subcategory { get; }

						bool HasStates { get; }

						string State { get; set; }

						bool IsQuestItem { get; }

						float BaseCurrencyValue { get; }

						WICurrencyType CurrencyType { get; }

						bool CanEnterInventory { get; }

						bool CanBeCarried { get; }

						bool UnloadWhenStacked { get; }

						bool IsStackContainer { get; }

						bool UseAsContainerInInventory { get; }

						WIStackContainer StackContainer { get; }

						WIStackMode StackMode { get; }

						int NumItems { get; }

						WIMode Mode { get; }

						Action OnRemoveFromStack { get; set; }

						Action OnRemoveFromGroup { get; set; }

						SVector3 ChunkPosition { get; set; }

						bool Is(string scriptName);

						bool Is(WIMode mode);

						bool Is<T>() where T : WIScript;

						bool Has<T>() where T : WIScript;

						bool HasAtLeastOne(List <string> scriptNames);

						void Add(string scriptName);

						bool GetStateOf<T>(out object stateObject) where T : WIScript;

						bool SetStateOf<T>(object stateData) where T : WIScript;

						void RemoveFromGame();

						void Clear();

						WIGroup Group { get; set; }

						StackItem GetStackItem(WIMode stackItemMode);
				}
		}
		public interface IProgressDialogObject
		{
				float ProgressValue { get; }

				string ProgressMessage { get; }

				string ProgressObjectName { get; }

				string ProgressIconName { get; }

				bool ProgressFinished { get; }

				bool ProgressCanceled { get; set; }
		}

		public interface ILayerMask
		{
				float GetValue(Vector3 samplePoint, Vector3 sampleNormal);

				ILayerMask AddChildLayer(ILayerMask newChildLayer);
		}

		public interface IGUIChildEditor
		{
				GameObject NGUIObject { get; set; }

				GameObject NGUIParentObject	{ get; set; }

				Camera NGUICamera { get; }

				GameObject gameObject { get; }
		}

		public interface IGUIChildEditor<R> : IGUIChildEditor
		{
				void ReceiveFromParentEditor(R editObject, ChildEditorCallback<R> callBack);

				void Finish();
		}

		public interface IGUIParentEditor
		{
				GameObject NGUIObject { get; set; }

				GameObject gameObject { get; }
		}

		public interface IGUIParentEditor<R> : IGUIParentEditor
		{
				void ReceiveFromChildEditor(R editObject, IGUIChildEditor<R> childEditor);
		}

		public interface IKillable
		{
				bool IsDead { get; }
		}

		public interface IThermal
		{
				float Insulation { get; }

				float Temperature { get; set; }

				GooThermalState ThermalState { get; set; }

				bool TryToIgnite(Vector3 ignitionPoint, float newTemperature, out IBurnable fuelSource);

				bool TryToChangeTemperature(Vector3 temperaturePoint, float newTemperature, out float actualTemperature);
		}

		public interface IBurnable : IKillable, IThermal
		{
				Vector3 Position { get; }

				Vector3 LocalPosition { get; }

				float Radius { get; }

				float FuelBurned { get; }

				float NormalizedBurnDamage { get; }

				float TotalFuel { get; }

				float FuelAvailable { get; }

				bool IsDepleted { get; }

				bool BurnFuel(float attemptedFuel, out float actualFuel);

				bool TryToSpread(float spreadRange, float newTemperature, out IBurnable newFuelSource);
		}
}