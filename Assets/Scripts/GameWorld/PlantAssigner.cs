using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Frontiers.World;

namespace Frontiers
{
	/// <summary>
	/// Provides static methods for the assignment of plants to plant instance tempates.
	/// </summary>
	public class PlantAssigner
	{
		//======================================================
		//      Public methods
		//======================================================
		/// <summary>
		/// Finds the closest tree to the player within the collider radius.
		/// </summary>
		/// <param name="player">The player, defining the center and radius of the search operation.</param>
		/// <param name="world">The world which is searched.</param>
		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>

		public static List <QuadTree <PlantInstanceTemplate>> chunkQuads = new List<QuadTree<PlantInstanceTemplate>> ( );
		public static List <QuadNode <PlantInstanceTemplate>> quadTreeCells = new List<QuadNode<PlantInstanceTemplate>> ( );
		public static List <PlantInstanceTemplate> plantList = new List<PlantInstanceTemplate> ( );

		public static PlantInstanceTemplate FindClosestPlantRequiringInstance (LocalPlayer player, GameWorld world)
		{
			chunkQuads.Clear ();
			quadTreeCells.Clear ();
			plantList.Clear ();

			for (int i = 0; i < GameWorld.Get.ImmediateChunks.Count; i++) {
				chunkQuads.Add (GameWorld.Get.ImmediateChunks [i].PlantInstanceQuad);
			}
			// Find all individual quad tree cells which may be relevant
			quadTreeCells.AddRange (chunkQuads.SelectMany (cq => cq.FindNodesIntersecting (player.ColliderBounds)));

			// Retrieve the trees from the relevant cells.
			plantList.AddRange (quadTreeCells.SelectMany (c => c.Content ?? Enumerable.Empty<PlantInstanceTemplate> ()));

			// From here, the approach is just regular brute force on the reduced list.
			return FindClosestPlantRequiringInstance (player, plantList);

//			Bounds radiusBounds = new Bounds (player.Position, new Vector3 (player.ColliderRadius, player.ColliderRadius, player.ColliderRadius) * 2);
//
//			// Sort out all irrelevant chunks, i.e. all which cannot contain a tree within collider radius.
//			// Relevant are all where a) the player currently is in or b) the collider radius intersects with the chunk bounds.
//			float squaredRadius = player.ColliderRadius * player.ColliderRadius;
//			var relevantChunks = world.WorldChunks.Where (wc =>
//				wc.CurrentMode == ChunkMode.Immediate || wc.CurrentMode == ChunkMode.Primary
//									//&& wc.ChunkBounds.Intersects (radiusBounds)//wc.ChunkBounds.Contains (player.Position) || wc.ChunkBounds.Intersects (radiusBounds)
//			                     );
//
//			// Retrieve quadtrees of relevant chunks (generate if not yet available)
//			var chunkQuads = relevantChunks.Select (c => c.PlantInstanceQuad);
//
//			// Find all individual quad tree cells which may be relevant
//			var quadtreeCells = chunkQuads.SelectMany (cq => cq.FindNodesIntersecting (radiusBounds));
//
//			// Retrieve the trees from the relevant cells.
//			var plantList = quadtreeCells.SelectMany (c => c.Content ?? Enumerable.Empty <PlantInstanceTemplate> ());
//
//			// From here, the approach is just regular brute force on the reduced list.
//			return FindClosestPlantRequiringInstance (player, plantList);
		}

		/// <summary>
		/// Finds the closest tree to the player within the collider radius.
		/// </summary>
		/// <param name="player">The player, defining the center and radius of the search operation.</param>
		/// <param name="world">The world which is searched.</param>
		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
		public static PlantInstanceTemplate FindClosestPlantRequiringInstanceReallyBruteForce (LocalPlayer player, GameWorld world)
		{
			var relevantChunks = world.WorldChunks.Where (wc => wc.CurrentMode == ChunkMode.Immediate);
			return FindClosestPlantRequiringInstance (player, relevantChunks.SelectMany (c => c.PlantInstances));
		}

		/// <summary>
		/// Finds the closest tree to the player within the collider radius.
		/// </summary>
		/// <param name="player">The player, defining the center and radius of the search operation.</param>
		/// <param name="world">The world which is searched.</param>
		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
		public static PlantInstanceTemplate FindClosestPlantRequiringInstanceBruteForce (LocalPlayer player, GameWorld world)
		{
			Bounds radiusBounds = new Bounds (player.Position, new Vector3 (player.ColliderRadius, player.ColliderRadius, player.ColliderRadius) * 2);

			// Sort out all irrelevant chunks, i.e. all which cannot contain a tree within collider radius.
			// Relevant are all where a) the player currently is in or b) the collider radius intersects with the chunk bounds.
			float squaredRadius = player.ColliderRadius * player.ColliderRadius;
			var relevantChunks = world.WorldChunks.Where (wc =>
                				wc.CurrentMode == ChunkMode.Immediate &&
						wc.ChunkBounds.Contains (player.Position) || wc.ChunkBounds.Intersects (radiusBounds)
			                     );

			return FindClosestPlantRequiringInstance (player, relevantChunks.SelectMany (c => c.PlantInstances));
		}

		/// <summary>
		/// Finds all active colliders which are already outside of the collider radius and not locked.
		/// </summary>
		/// <param name="player">The player object, which defines the center and radius of the search.</param>
		/// <param name="world">The world with the colliders.</param>
		/// <returns>An enumerable list of Plant colliders which can be reassigned.</returns>
		public static IEnumerable <WorldPlant> FindIrrelevantInstances (LocalPlayer player, Plants plants)
		{
			//float squaredRadius = player.ColliderRadius * player.ColliderRadius;
			for (int i = 0; i < plants.ActivePlants.Count; i++) {
				if (!player.ColliderBounds.Contains (plants.ActivePlants [i].Position)) {
					yield return plants.ActivePlants [i];
				}
				//Vector3 dst = collider.Position - player.Position;
				//float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;

				// What if it is locked? TEMP

				//if (distanceSquared < squaredRadius) {
				//TreeInstanceTemplate instance;
				//if (world.ColliderMappings.TryGetValue (collider, out instance) && (!instance.LockCollider)) {
				//yield return collider;
				//}
				//}
			}
//			//float squaredRadius = player.ColliderRadius * player.ColliderRadius;
//			Bounds colliderBounds = player.ColliderBounds;
//			foreach (var worldPlant in plants.ActivePlants) {
//				if (!colliderBounds.Contains (worldPlant.Position)) {
//					yield return worldPlant;
//				}
//				//Vector3 dst = collider.Position - player.Position;
//				//float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;
//
//				// What if it is locked? TEMP
//
//				//if (distanceSquared < squaredRadius) {
//					//PlantInstanceTemplate instance;
//					//if (world.ColliderMappings.TryGetValue (collider, out instance) && (!instance.LockCollider)) {
//					//yield return collider;
//					//}
//				//}
//			}
		}
		//======================================================
		//      Protected methods
		//======================================================
		/// <summary>
		/// Finds the closest Plant to the player within the collider radius.
		/// </summary>
		/// <param name="player">The player, defining the center and radius of the search operation.</param>
		/// <param name="Plants">An enumerable list of Plants which is searched.</param>
		/// <returns>The closest Plant instance to the player, or null if no Plant was found within the radius.</returns>
		protected static PlantInstanceTemplate FindClosestPlantRequiringInstance (LocalPlayer player, IEnumerable <PlantInstanceTemplate> plants)
		{
			PlantInstanceTemplate result = PlantInstanceTemplate.Empty;
			float minDistance = float.MaxValue;

			foreach (var plant in plants) {
				if (!plant.HasInstance && plant.ReadyToBePlanted) {
					Vector3 dst = plant.Position - player.Position;
					float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;

					if (distanceSquared < minDistance) {
						minDistance = distanceSquared;
						result = plant;
					}
				}
			}
			return result;
		}
	}
}
// End of namespace Frontiers
