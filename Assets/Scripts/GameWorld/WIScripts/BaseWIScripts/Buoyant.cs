using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using System;

namespace Frontiers.World.WIScripts
{
	public class Buoyant : WIScript
	{
		public IBodyOfWater Water = null;
		public BuoyantState State = new BuoyantState();
		public float MinorVariation;
		public float WaterLevel;
		public Vector3 Force;
		public float ForceOffset = -0.1f;
		public float ForceMultiplier = 0.5f;

		public override void OnInitialized()
		{
			worlditem.OnEnterBodyOfWater += OnEnterBodyOfWater;
			worlditem.OnExitBodyOfWater += OnExitBodyOfWater;
			worlditem.OnAddedToGroup += OnAddedToGroup;
			worlditem.OnAddedToPlayerInventory += OnAddedToPlayerInventory;
		}

		public void OnAddedToGroup()
		{
			if (State.IsSubmerged) {
				OnEnterBodyOfWater();
			}
		}

		public void OnAddedToPlayerInventory()
		{
			OnExitBodyOfWater();
		}

		public void OnEnterBodyOfWater()
		{
			mTerrainHit.feetPosition = worlditem.Position;
			mTerrainHit.groundedHeight = 100f;
			mTerrainHit.ignoreWorldItems = true;
			mTerrainHit.overhangHeight = 10f;
			mTerrainHit.ignoreWater = true;
			mWorldHeightOnStart = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);
			mStartTimeOffset = UnityEngine.Random.value * 10;
			State.IsSubmerged = true;
			worlditem.SetMode(WIMode.World);
			worlditem.rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
			worlditem.rb.useGravity = false;
			enabled = true;
		}

		public void OnExitBodyOfWater()
		{
			State.IsSubmerged = false;
			worlditem.SetMode(WIMode.World);
			worlditem.rb.constraints = RigidbodyConstraints.None;
			worlditem.rb.useGravity = true;
			enabled = false;
		}

		public void Update()
		{
			if (Water == null) {
				FindWater();
			} else {
				mTerrainHit.feetPosition = worlditem.Position;
				Vector3 targetPosition = mTerrainHit.feetPosition;
				MinorVariation = (float)Math.Abs(Math.Sin((WorldClock.AdjustedRealTime * GameWorld.Get.CurrentBiome.WaveSpeed * Globals.WaveSpeed) + mStartTimeOffset)) * GameWorld.Get.CurrentBiome.WaveIntensity;
				WaterLevel = Water.WaterHeightAtPosition(mTerrainHit.feetPosition);
				targetPosition.y = WaterLevel - MinorVariation + ForceOffset;
				Force = (targetPosition - mTerrainHit.feetPosition) * ForceMultiplier;
				worlditem.rb.AddForce(Force, ForceMode.VelocityChange);
			}
		}

		protected void FindWater()
		{
			if (Physics.Raycast(worlditem.Position + Vector3.up * 10, Vector3.down, out mWaterHit, 100f, Globals.LayerFluidTerrain)) {
				WorldItem bodyOfWaterWorlditem = null;
				BodyOfWater bodyOfWater = null;
				Ocean ocean = null;
				if (mWaterHit.collider.attachedRigidbody.gameObject.HasComponent <WorldItem>(out bodyOfWaterWorlditem) && bodyOfWaterWorlditem.Is <BodyOfWater>(out bodyOfWater)) {
					Water = bodyOfWater;
				} else if (mWaterHit.collider.attachedRigidbody.gameObject.HasComponent <Ocean>(out ocean)) {
					Water = ocean;
				} else {
					State.IsSubmerged = false;
					enabled = false;
					return;
				}
			}
		}

		protected float mStartTimeOffset = 0f;
		protected float mWorldHeightOnStart = 0f;
		protected GameWorld.TerrainHeightSearch mTerrainHit;
		protected static RaycastHit mWaterHit;
	}

	[Serializable]
	public class BuoyantState
	{
		public bool IsSubmerged = false;
	}
}