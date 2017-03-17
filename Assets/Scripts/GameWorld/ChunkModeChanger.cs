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

	public void RefreshTargetMode ()
	{	//if we haven't started yet
		if (Chunk == null)
			return;
	}

	public void Awake ()
	{
		Chunk = gameObject.GetComponent <WorldChunk> ();
		if (Chunk.TargetMode == Chunk.CurrentMode) {	//wrap it up, we're done here!
			Finish ();
		} else if ((Chunk.TargetMode == ChunkMode.Primary && Chunk.CurrentMode == ChunkMode.Immediate)
		           || (Chunk.TargetMode == ChunkMode.Immediate && Chunk.CurrentMode == ChunkMode.Primary)) {	//immediate and primary are identical
			//primary just means the player is >rightontopofit<
			//so wrap this up too
			Finish ();
		}
		StartTargetMode = Chunk.TargetMode;
		//if the target mode does not equal the current mode
		//start the update chunk process in Start
		//do not start enumerators in Awake
	}

	public void Start ()
	{
		if (mFinished)
			return;

		StartCoroutine (RunSequence ());
	}

	protected IEnumerator RunSequence ()
	{
		yield return null;
        while (!Chunk.Initialized) {
            yield return null;
        }

        bool showAboveGround = !Player.Local.Surroundings.IsUnderground;

        switch (Chunk.TargetMode) {

            case ChunkMode.Immediate:
            case ChunkMode.Primary:
                Chunk.Transforms.AboveGroundStaticImmediate.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.AboveGroundStaticAdjascent.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.AboveGroundStaticDistant.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.BelowGroundStatic.gameObject.SetActive(!showAboveGround);
                break;

            case ChunkMode.Adjascent:
                Chunk.Transforms.AboveGroundStaticImmediate.gameObject.SetActive(false);
                Chunk.Transforms.AboveGroundStaticAdjascent.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.AboveGroundStaticDistant.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.BelowGroundStatic.gameObject.SetActive(!showAboveGround);
                break;

            case ChunkMode.Distant:
                Chunk.Transforms.AboveGroundStaticImmediate.gameObject.SetActive(false);
                Chunk.Transforms.AboveGroundStaticAdjascent.gameObject.SetActive(false);
                Chunk.Transforms.AboveGroundStaticDistant.gameObject.SetActive(true & showAboveGround);
                Chunk.Transforms.BelowGroundStatic.gameObject.SetActive(!showAboveGround);
                break;

            case ChunkMode.Unloaded:
                Chunk.Transforms.AboveGroundStaticImmediate.gameObject.SetActive(false);
                Chunk.Transforms.AboveGroundStaticAdjascent.gameObject.SetActive(false);
                Chunk.Transforms.AboveGroundStaticDistant.gameObject.SetActive (false);
                Chunk.Transforms.BelowGroundStatic.gameObject.SetActive(false);
                break;
        }

		Finish ();
		yield break;
	}

	protected void RefreshTerrainSettings (ChunkMode mode)
	{
		if (!Chunk.HasPrimaryTerrain) {
			return;
		}
	}

	protected IEnumerator LoadChunk ()
	{
		/*Terrain newTerrain = null;
		while (!GameWorld.Get.ClaimTerrain (out newTerrain)) {
			if (Chunk.TargetMode == ChunkMode.Unloaded) {
				yield break;
			} else {
				yield return null;
			}
		}
		//un-set the terrain neighbors while we move the terrain
		GameWorld.Get.DetatchChunkNeighbors ();
		yield return StartCoroutine (Chunk.GenerateTerrain (newTerrain, !Player.Local.Surroundings.IsUnderground));
		//now re-set the neighbors
		GameWorld.Get.ReattachChunkNeighbors ();
		var refreshTerrainTextures = Chunk.RefreshTerrainTextures ();
		while (refreshTerrainTextures.MoveNext ()) {
			yield return refreshTerrainTextures.Current;
		}*/

		if (Chunk.TargetMode == ChunkMode.Unloaded) {
			yield break;
		}

		var addRivers = Chunk.AddRivers (ChunkMode.Immediate);
		while (addRivers.MoveNext ()) {
			yield return addRivers.Current;
		}

		yield break;
	}

	protected IEnumerator UnloadChunk ()
	{
		//Chunk.UnloadTerrain ();
		Chunk.CurrentMode = ChunkMode.Unloaded;
		yield break;
	}

	protected void Finish ()
	{
		Chunk.CurrentMode = Chunk.TargetMode;
		//Chunk.ShowAboveGround (!Player.Local.Surroundings.IsUnderground);
		mFinished = true;
		GameObject.Destroy (this);
		GameWorld.Get.RefreshTerrainDetailSettings ();
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
