using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
	namespace World
	{
		public interface IDamageable : IKillable
		{
			float NormalizedDamage	{ get; }

			bool TakeDamage (WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string sourceName, out float actualDamage, out bool isDead);

			WIMaterialType BaseMaterialType	{ get; }

			WIMaterialType ArmorMaterialTypes	{ get; }

			IItemOfInterest LastDamageSource	{ get; set; }

			int ArmorLevel (WIMaterialType materialType);
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

			WIStackContainer StackContainer { get; }
			 
			WIStackMode StackMode { get; }

			int NumItems { get; }

			WIMode Mode { get; }

			Action OnRemoveFromStack { get; set; }

			Action OnRemoveFromGroup { get; set; }

			SVector3 ChunkPosition { get; set; }

			bool Is (string scriptName);

			bool Is (WIMode mode);

			bool Is<T> () where T : WIScript;

			bool Has<T> () where T : WIScript;

			void Add (string scriptName);

			bool GetStateOf<T> (out object stateObject) where T : WIScript;

			bool SetStateOf<T> (object stateData) where T : WIScript;

			void RemoveFromGame ();

			void Clear ();

			WIGroup Group { get; set; }

			StackItem GetStackItem (WIMode stackItemMode);
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
		float GetValue (Vector3 samplePoint, Vector3 sampleNormal);

		ILayerMask AddChildLayer (ILayerMask newChildLayer);
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
		void ReceiveFromParentEditor (R editObject, ChildEditorCallback<R> callBack);
	}

	public interface IGUIParentEditor
	{
		GameObject NGUIObject { get; set; }

		GameObject gameObject { get; }
	}

	public interface IGUIParentEditor<R> : IGUIParentEditor
	{
		void ReceiveFromChildEditor	(R editObject, IGUIChildEditor<R> childEditor);
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

		bool TryToIgnite (Vector3 ignitionPoint, float newTemperature, out IBurnable fuelSource);

		bool TryToChangeTemperature (Vector3 temperaturePoint, float newTemperature, out float actualTemperature);
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

		bool BurnFuel (float attemptedFuel, out float actualFuel);

		bool TryToSpread (float spreadRange, float newTemperature, out IBurnable newFuelSource);
	}
}