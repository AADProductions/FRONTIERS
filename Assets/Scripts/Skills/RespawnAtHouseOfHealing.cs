using UnityEngine;
using System.Collections;

namespace Frontiers.World.Gameplay
{
	public class RespawnAtHouseOfHealing : RespawnSkill
	{
		public override bool RequirementsMet {
			get {
				return true;
			}
		}

		public override bool RequiresAtLeastOneEquippedWorldItem {
			get {
				return false;
			}
		}

		public override bool ExtensionRequirementsMet {
			get {
				return true;
			}
		}

		protected override void RespawnPlayer(LocalPlayer player)
		{
			if (!mSpawningPlayerOverTime) {
				mSpawningPlayerOverTime = true;
				StartCoroutine(SpawnPlayerOverTime(player));
			}
		}

		public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
		{
			return targetObject.IOIType == ItemOfInterestType.Player && targetObject.player.IsDead;
		}

		protected IEnumerator SpawnPlayerOverTime(LocalPlayer player)
		{
			mWaitingForFade = true;
			//add a request to spawn a healer character near the player
			Frontiers.GUI.CameraFade.StartAlphaFade(Color.red, false, 3.0f, 0f, () => {
				mWaitingForFade = false;
			});
			while (mWaitingForFade) {
				yield return null;
			}
			//now fade out red
			Frontiers.GUI.CameraFade.StartAlphaFade(Color.red, true, 3.0f);
			//now that the screen is covered
			//move the player
			GameWorld.TerrainHeightSearch terrainHit = new GameWorld.TerrainHeightSearch();
			//search for an appropriate spot within a radius around the player
			bool foundGoodSpot = false;
			int maxTries = 50;
			int numTriesSoFar = 0;
			while (!foundGoodSpot) {
				Vector3 randomSpot = (UnityEngine.Random.onUnitSphere * 25f) + player.Surroundings.LastPositionOnland;
				randomSpot.y += 150;
				terrainHit.feetPosition = randomSpot;
				terrainHit.overhangHeight = Globals.DefaultCharacterHeight;
				terrainHit.groundedHeight = 200f;//we don't care about being grounded, just find the floor
				terrainHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref terrainHit);
				if (!terrainHit.hitWater && !terrainHit.hitTerrainMesh && terrainHit.hitTerrain) {
					Debug.Log("Found spot to spawn: " + terrainHit.feetPosition.ToString());
					foundGoodSpot = true;
					break;
				} else {
					numTriesSoFar++;
					if (numTriesSoFar > maxTries) {
						Debug.Log("Couldn't find good place to spawn, going with last position on land");
						terrainHit.feetPosition = player.Surroundings.LastPositionOnland;
						break;
					}
				}
			}
			terrainHit.feetPosition.y += 0.25f;
			player.Position = terrainHit.feetPosition;
			//spawn the player
			player.Spawn();
			//finally spawn the healer nearby
			CharacterSpawnRequest spawnRequest = new CharacterSpawnRequest();
			spawnRequest.ActionNodeName = "HealerActionNode";
			spawnRequest.CharacterName = "Healer";
			spawnRequest.UseGenericTemplate = true;
			spawnRequest.CustomConversation = Globals.HouseOfHealingExteriorConversation;
			spawnRequest.FinishOnSpawn = true;
			spawnRequest.SpawnBehindPlayer = true;
			spawnRequest.MinimumDistanceFromPlayer = 1f;
			player.CharacterSpawner.AddSpawnRequest(spawnRequest);
			//take the player's money
			//take 1/2 (or whatever global value is) reduced by skill value
			int moneyToTake = Mathf.FloorToInt(player.Inventory.InventoryBank.BaseCurrencyValue * (Globals.HouseOfHealingRevivalCost * (1f - State.NormalizedUsageLevel)));
			player.Inventory.InventoryBank.TryToRemove(moneyToTake);
			//and we're done!
			mSpawningPlayerOverTime = false;
			yield break;
		}

		protected bool mWaitingForFade = false;
		protected bool mSpawningPlayerOverTime = false;
	}
}