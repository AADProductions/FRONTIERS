using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Frontiers.World;

namespace Frontiers
{
		/// <summary>
		/// Provides static methods for the assignment of colliders to trees.
		/// </summary>
		public class ColliderAssigner
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
				public static List <QuadNode <TreeInstanceTemplate>> quadTreeCells = new List<QuadNode<TreeInstanceTemplate>>();
				public static List <TreeInstanceTemplate> treeList = new List<TreeInstanceTemplate>();

				public static TreeInstanceTemplate FindClosestTreeRequiringCollider(LocalPlayer player, GameWorld world)
				{
						quadTreeCells.Clear();
						treeList.Clear();

						for (int i = 0; i < GameWorld.Get.ImmediateChunks.Count; i++) {
								//find all the nodes that intersect the player bounds - this is recursive so it should cover everything
								GameWorld.Get.ImmediateChunks[i].TreeInstanceQuad.FindNodesIntersecting(player.ColliderBounds, quadTreeCells);
						}

						for (int i = 0; i < quadTreeCells.Count; i++) {
								// Retrieve the trees from the relevant cells.
								if (quadTreeCells[i].Content != null) {
										treeList.AddRange(quadTreeCells[i].Content);
								}
						}

						// From here, the approach is just regular brute force on the reduced list.
						return FindClosestTreeRequiringCollider(player, treeList);
				}

				/// <summary>
				/// Finds the closest tree to the player within the collider radius.
				/// </summary>
				/// <param name="player">The player, defining the center and radius of the search operation.</param>
				/// <param name="world">The world which is searched.</param>
				/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
				//		public static TreeInstanceTemplate FindClosestTreeRequiringColliderReallyBruteForce (LocalPlayer player, GameWorld world)
				//		{
				//			var relevantChunks = world.WorldChunks.Where (wc => wc.CurrentMode == ChunkMode.Immediate);
				//			return FindClosestTreeRequiringCollider (player, relevantChunks.SelectMany (c => c.TreeInstances));
				//		}

				/// <summary>
				/// Finds the closest tree to the player within the collider radius.
				/// </summary>
				/// <param name="player">The player, defining the center and radius of the search operation.</param>
				/// <param name="world">The world which is searched.</param>
				/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
				//		public static TreeInstanceTemplate FindClosestTreeRequiringColliderBruteForce (LocalPlayer player, GameWorld world)
				//		{
				//			Bounds radiusBounds = new Bounds (player.Position, new Vector3 (player.ColliderRadius, player.ColliderRadius, player.ColliderRadius) * 2);
				//
				//			// Sort out all irrelevant chunks, i.e. all which cannot contain a tree within collider radius.
				//			// Relevant are all where a) the player currently is in or b) the collider radius intersects with the chunk bounds.
				//			float squaredRadius = player.ColliderRadius * player.ColliderRadius;
				//			var relevantChunks = world.WorldChunks.Where (wc =>
				//                				wc.CurrentMode == ChunkMode.Immediate &&
				//						wc.ChunkBounds.Contains (player.Position) || wc.ChunkBounds.Intersects (radiusBounds)
				//			                     );
				//
				//			return FindClosestTreeRequiringCollider (player, relevantChunks.SelectMany (c => c.TreeInstances));
				//		}

				/// <summary>
				/// Finds all active colliders which are already outside of the collider radius and not locked.
				/// </summary>
				/// <param name="player">The player object, which defines the center and radius of the search.</param>
				/// <param name="world">The world with the colliders.</param>
				/// <returns>An enumerable list of tree colliders which can be reassigned.</returns>
				public static IEnumerable<TreeCollider> FindIrrelevantColliders(LocalPlayer player, GameWorld world)
				{
						//float squaredRadius = player.ColliderRadius * player.ColliderRadius;
						for (int i = 0; i < world.ActiveColliders.Count; i++) {
								if (!player.ColliderBounds.Contains(world.ActiveColliders[i].Position)) {
										yield return world.ActiveColliders[i];
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
				}
				//======================================================
				//      Protected methods
				//======================================================
				/// <summary>
				/// Finds the closest tree to the player within the collider radius.
				/// </summary>
				/// <param name="player">The player, defining the center and radius of the search operation.</param>
				/// <param name="trees">An enumerable list of trees which is searched.</param>
				/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
				protected static TreeInstanceTemplate FindClosestTreeRequiringCollider(LocalPlayer player, List <TreeInstanceTemplate> trees)
				{
						TreeInstanceTemplate result = TreeInstanceTemplate.Empty;
						TreeInstanceTemplate tree = null;
						float minDistance = float.MaxValue;

						for (int i = 0; i < trees.Count; i++) {
								tree = trees[i];
								if ((!tree.HasInstance) && tree.RequiresInstance) {
										Vector3 dst = tree.Position - player.Position;
										float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;

										if (distanceSquared < minDistance) {
												minDistance = distanceSquared;
												result = tree;
										}
								}
						}
						return result;
				}
		}
}
// End of namespace Frontiers
