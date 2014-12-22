using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

//it's the switchiest class you ever did see
//ChunkModeChanger is responsible for loading and unloading chunk content
//based on its current mode and its target mode
//it used to me more important back when we had more chunks
//now it could probably be folded into WorldChunk
//but i still keep it around in case i want to use it as a
//general-purpose optimizer
public class ChunkModeChanger : MonoBehaviour
{
		WorldChunk Chunk;
		//this is what we use to determine loading steps
		public ChunkMode StartTargetMode = ChunkMode.Unloaded;
		public static List <int> DebugChunkIDs = new List<int>() { 3, 4 };

		public void RefreshTargetMode()
		{	//if we haven't started yet
				if (Chunk == null)
						return;
		}

		public void Awake()
		{
				Chunk = gameObject.GetComponent <WorldChunk>();
				if (Chunk.TargetMode == Chunk.CurrentMode) {	//wrap it up, we're done here!
						Finish();
				} else if ((Chunk.TargetMode == ChunkMode.Primary && Chunk.CurrentMode == ChunkMode.Immediate)
				         || (Chunk.TargetMode == ChunkMode.Immediate && Chunk.CurrentMode == ChunkMode.Primary)) {	//immediate and primary are identical
						//primary just means the player is >rightontopofit<
						//so wrap this up too
						Finish();
				}
				StartTargetMode = Chunk.TargetMode;
				//if the target mode does not equal the current mode
				//start the update chunk process in Start
				//do not start enumerators in Awake
		}

		public void Start()
		{
				if (mFinished)
						return;

				StartCoroutine(RunSequence());
		}

		protected LoadStep GetNextStep()
		{
				ChunkMode targetMode = Chunk.TargetMode;
				ChunkMode currentMode = Chunk.CurrentMode;
				StartTargetMode = targetMode;
				LoadStep nextLoadStep = LoadStep.None;

				switch (targetMode) {
						case ChunkMode.Unloaded:
						default:
								switch (currentMode) {
										case ChunkMode.Unloaded:
										default:
												//nothing to do here
												break;

										case ChunkMode.Distant:
												nextLoadStep = LoadStep.UnloadDistant;
												break;

										case ChunkMode.Adjascent:
												nextLoadStep = LoadStep.UnloadAdjascent;
												break;

										case ChunkMode.Immediate:
										case ChunkMode.Primary:
												nextLoadStep = LoadStep.UnloadImmediate;
												break;
								}
								break;

						case ChunkMode.Distant:
								switch (currentMode) {
										case ChunkMode.Unloaded:
										default:
												nextLoadStep = LoadStep.LoadDistant;
												break;

										case ChunkMode.Distant:
												//nothing to do here
												break;

										case ChunkMode.Adjascent:
												nextLoadStep = LoadStep.UnloadAdjascent;
												break;

										case ChunkMode.Immediate:
										case ChunkMode.Primary:
												nextLoadStep = LoadStep.UnloadImmediate;
												break;
								}
								break;

						case ChunkMode.Adjascent:
								switch (currentMode) {
										case ChunkMode.Unloaded:
										default:
												nextLoadStep = LoadStep.LoadDistant;
												break;

										case ChunkMode.Distant:
												nextLoadStep = LoadStep.LoadAdjascent;
												break;

										case ChunkMode.Adjascent:
												//nothing to do here
												break;

										case ChunkMode.Immediate:
										case ChunkMode.Primary:
												nextLoadStep = LoadStep.UnloadImmediate;
												break;
								}
								break;

						case ChunkMode.Immediate:
						case ChunkMode.Primary:
								switch (currentMode) {
										case ChunkMode.Unloaded:
										default:
												nextLoadStep = LoadStep.LoadDistant;
												break;

										case ChunkMode.Distant:
												nextLoadStep = LoadStep.LoadAdjascent;
												break;

										case ChunkMode.Adjascent:
												nextLoadStep = LoadStep.LoadImmediate;
												break;

										case ChunkMode.Immediate:
										case ChunkMode.Primary:
												//nothing to do here
												break;
								}
								break;
				}
				return nextLoadStep;
		}

		protected IEnumerator RunSequence()
		{
				yield return null;
				while (Chunk.CurrentMode != Chunk.TargetMode) {
						while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
								yield return null;
						}
						//run the sequence
						switch (GetNextStep()) {
								case LoadStep.LoadImmediate:
										GameWorld.IsLoadingChunk = true;
										yield return StartCoroutine(LoadImmediate());
										RefreshTerrainSettings(Chunk.CurrentMode);
										GameWorld.IsLoadingChunk = false;
										break;

								case LoadStep.LoadAdjascent:
										GameWorld.IsLoadingChunk = true;
										yield return StartCoroutine(LoadAdjascent());
										RefreshTerrainSettings(Chunk.CurrentMode);
										GameWorld.IsLoadingChunk = false;
										break;

								case LoadStep.LoadDistant:
								default:
										GameWorld.IsLoadingChunk = true;
										yield return StartCoroutine(LoadChunk());
										yield return StartCoroutine(LoadDistant());
										RefreshTerrainSettings(Chunk.CurrentMode);
										GameWorld.IsLoadingChunk = false;
										break;

								case LoadStep.UnloadDistant:
										yield return StartCoroutine(UnloadDistant());
										yield return StartCoroutine(UnloadChunk());
										RefreshTerrainSettings(Chunk.CurrentMode);
										break;

								case LoadStep.UnloadAdjascent:
										yield return StartCoroutine(UnloadAdjascent());
										RefreshTerrainSettings(Chunk.CurrentMode);
										break;

								case LoadStep.UnloadImmediate:
										yield return StartCoroutine(UnloadImmediate());
										RefreshTerrainSettings(Chunk.CurrentMode);
										break;
						}
						yield return null;
				}
				Finish();
				yield break;
		}

		protected void RefreshTerrainSettings(ChunkMode mode)
		{
				if (!Chunk.HasPrimaryTerrain) {
						return;
				}
		}

		protected IEnumerator LoadChunk()
		{
				Terrain newTerrain = null;
				while (!GameWorld.Get.ClaimTerrain(out newTerrain)) {
						if (Chunk.TargetMode != StartTargetMode) {
								//whoops! something changed while we were waiting
								yield break;
						} else if (GameManager.Is(FGameState.InGame)) {
								yield return new WaitForSeconds(0.5f);
						} else {
								yield return null;
						}
				}
				//un-set the terrain neighbors while we move the terrain
				GameWorld.Get.DetatchChunkNeighbors();
				yield return StartCoroutine(Chunk.GenerateTerrain(newTerrain, !Player.Local.Surroundings.IsUnderground));
				//now re-set the neighbors
				GameWorld.Get.ReattachChunkNeighbors();

				yield return StartCoroutine(Chunk.RefreshTerrainTextures());
				yield return Chunk.StartCoroutine(Chunk.RefreshTerrainObjects());
				yield return Chunk.StartCoroutine(Chunk.AddTerrainTrees());
				yield return Chunk.StartCoroutine(Chunk.AddTerrainDetails());
				//Chunk.PrimaryTerrain.castShadows = false;
				yield break;
		}

		protected IEnumerator LoadImmediate()
		{
				while (GameManager.Is(FGameState.Cutscene)) {
						yield return null;
				}
				yield return Chunk.StartCoroutine(Chunk.AddTerainFX(ChunkMode.Immediate));

				Chunk.PrimaryTerrain.Flush();

				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
						Structures.LoadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabs[i], Chunk, ChunkMode.Immediate);
				}
				for (int i = 0; i < Chunk.SceneryData.BelowGround.SolidTerrainPrefabs.Count; i++) {
						Structures.LoadChunkPrefab(Chunk.SceneryData.BelowGround.SolidTerrainPrefabs[i], Chunk, ChunkMode.Immediate);
				}
				if (Chunk.TargetMode == ChunkMode.Primary) {
						Chunk.CurrentMode = ChunkMode.Primary;
				} else {
						Chunk.CurrentMode = ChunkMode.Immediate;
				}
				yield break;
		}

		protected IEnumerator LoadAdjascent()
		{
				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabsAdjascent.Count; i++) {
						Structures.LoadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabsAdjascent[i], Chunk, ChunkMode.Adjascent);
						switch (Chunk.TargetMode) {
								case ChunkMode.Immediate:
								case ChunkMode.Primary:
										break;

								case ChunkMode.Adjascent:
										yield return WorldClock.WaitForRTSeconds(0.05f);
										break;

								default:
										//if we don't need this >rightaway< give the world some time to breathe
										yield return WorldClock.WaitForRTSeconds(0.1f);
										break;
						}
				}
				Chunk.CurrentMode = ChunkMode.Adjascent;
				yield break;
		}

		protected IEnumerator LoadDistant()
		{
				yield return Chunk.StartCoroutine(Chunk.AddRivers(ChunkMode.Immediate));

				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabsDistant.Count; i++) {
						Structures.LoadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabsDistant[i], Chunk, ChunkMode.Distant);
						switch (Chunk.TargetMode) {
								case ChunkMode.Immediate:
								case ChunkMode.Primary:
										break;

								case ChunkMode.Adjascent:
										yield return WorldClock.WaitForRTSeconds(0.05f);
										break;

								default:
										//if we don't need this >rightaway< give the world some time to breathe
										yield return WorldClock.WaitForRTSeconds(0.1f);
										break;
						}
				}
				Chunk.CurrentMode = ChunkMode.Distant;
				yield break;
		}

		protected IEnumerator UnloadImmediate()
		{
				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
						Structures.UnloadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabs[i]);//, Chunk, ChunkMode.Immediate);
				}
				Chunk.CurrentMode = ChunkMode.Adjascent;
				yield break;
		}

		protected IEnumerator UnloadAdjascent()
		{
				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabsAdjascent.Count; i++) {
						Structures.UnloadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabsAdjascent[i]);//, Chunk, ChunkMode.Adjascent);
				}
				Chunk.CurrentMode = ChunkMode.Distant;
				yield break;
		}

		protected IEnumerator UnloadDistant()
		{
				for (int i = 0; i < Chunk.SceneryData.AboveGround.SolidTerrainPrefabsDistant.Count; i++) {
						Structures.UnloadChunkPrefab(Chunk.SceneryData.AboveGround.SolidTerrainPrefabsDistant[i]);//, Chunk, ChunkMode.Distant);
				}
				Chunk.CurrentMode = ChunkMode.Unloaded;
				yield break;
		}

		protected IEnumerator UnloadChunk()
		{
				Chunk.UnloadTerrain();
				Chunk.CurrentMode = ChunkMode.Unloaded;
				yield break;
		}

		protected void Finish()
		{
				Chunk.CurrentMode = Chunk.TargetMode;
				mFinished = true;
				//System.GC.Collect();//TODO does this actually help?
				GameObject.Destroy(this);
				GameWorld.Get.RefreshTerrainDetailSettings();
		}

		protected bool mFinished = false;

		public enum LoadStep
		{
				None,
				LoadImmediate,
				LoadAdjascent,
				LoadDistant,
				UnloadImmediate,
				UnloadAdjascent,
				UnloadDistant,
		}
}
