//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Frontiers.World;
//
//
//namespace Frontiers
//{
//	/// <summary>
//	/// Provides static methods for the assignment of colliders to trees.
//	/// </summary>
//	public class PathMarkerAssigner
//	{
//		//======================================================
//		//      Public methods
//		//======================================================
//		/// <summary>
//		/// Finds the closest tree to the player within the collider radius.
//		/// </summary>
//		/// <param name="player">The player, defining the center and radius of the search operation.</param>
//		/// <param name="world">The world which is searched.</param>
//		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
//		public static PathMarkerInstanceTemplate FindClosestPathMarkerRequiringInstance (LocalPlayer player, GameWorld world)
//		{
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
//			var chunkQuads = relevantChunks.Select (c => c.PathMarkerInstanceQuad);
//
//			// Find all individual quad tree cells which may be relevant
//			var quadtreeCells = chunkQuads.SelectMany (cq => cq.FindNodesIntersecting (radiusBounds));
//
//			// Retrieve the trees from the relevant cells.
//			var treeList = quadtreeCells.SelectMany (c => c.Content ?? Enumerable.Empty <PathMarkerInstanceTemplate> ());
//
//			// From here, the approach is just regular brute force on the reduced list.
//			return FindClosestPathMarkerRequiringInstance (player, treeList);
//		}
//
//		/// <summary>
//		/// Finds the closest tree to the player within the collider radius.
//		/// </summary>
//		/// <param name="player">The player, defining the center and radius of the search operation.</param>
//		/// <param name="world">The world which is searched.</param>
//		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
//		public static PathMarkerInstanceTemplate FindClosestPathMarkerRequiringInstanceReallyBruteForce (LocalPlayer player, GameWorld world)
//		{
//			var relevantChunks = world.WorldChunks.Where (wc => wc.CurrentMode == ChunkMode.Immediate);
//			return FindClosestPathMarkerRequiringInstance (player, relevantChunks.SelectMany (c => c.PathMarkerInstances));
//		}
//
//		/// <summary>
//		/// Finds the closest tree to the player within the collider radius.
//		/// </summary>
//		/// <param name="player">The player, defining the center and radius of the search operation.</param>
//		/// <param name="world">The world which is searched.</param>
//		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
//		public static PathMarkerInstanceTemplate FindClosestPathMarkerRequiringInstanceBruteForce (LocalPlayer player, GameWorld world)
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
//			return FindClosestPathMarkerRequiringInstance (player, relevantChunks.SelectMany (c => c.PathMarkerInstances));
//		}
//
//		/// <summary>
//		/// Finds all active colliders which are already outside of the collider radius and not locked.
//		/// </summary>
//		/// <param name="player">The player object, which defines the center and radius of the search.</param>
//		/// <param name="world">The world with the colliders.</param>
//		/// <returns>An enumerable list of tree colliders which can be reassigned.</returns>
//		public static IEnumerable<PathMarker> FindIrrelevantInstances (LocalPlayer player, Paths paths)
//		{
//			//float squaredRadius = player.ColliderRadius * player.ColliderRadius;
//			Bounds colliderBounds = player.ColliderBounds;
//			foreach (var pathMarker in paths.ActivePathMarkers) {
//				if (!colliderBounds.Contains (pathMarker.Position)) {
//					yield return pathMarker;
//				}
//				//Vector3 dst = collider.Position - player.Position;
//				//float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;
//
//				// What if it is locked? TEMP
//
//				//if (distanceSquared < squaredRadius) {
//					//PathMarkerInstanceTemplate instance;
//					//if (world.ColliderMappings.TryGetValue (collider, out instance) && (!instance.LockCollider)) {
//					//yield return collider;
//					//}
//				//}
//			}
//		}
//		//======================================================
//		//      Protected methods
//		//======================================================
//		/// <summary>
//		/// Finds the closest tree to the player within the collider radius.
//		/// </summary>
//		/// <param name="player">The player, defining the center and radius of the search operation.</param>
//		/// <param name="trees">An enumerable list of trees which is searched.</param>
//		/// <returns>The closest tree instance to the player, or null if no tree was found within the radius.</returns>
//		protected static PathMarkerInstanceTemplate FindClosestPathMarkerRequiringInstance (LocalPlayer player, IEnumerable<PathMarkerInstanceTemplate> pathMarkers)
//		{
//			PathMarkerInstanceTemplate result = PathMarkerInstanceTemplate.Empty;
//			float minDistance = float.MaxValue;
//
//			foreach (var pathMarker in pathMarkers) {
//				if ((!pathMarker.HasInstance) && pathMarker.RequiresInstance) {
//					Vector3 dst = pathMarker.Position - player.Position;
//					float distanceSquared = dst.x * dst.x + dst.y * dst.y + dst.z * dst.z;
//
//					if (distanceSquared < minDistance) {
//						minDistance = distanceSquared;
//						result = pathMarker;
//					}
//				}
//			}
//			return result;
//		}
//	}
//}
//// End of namespace Frontiers
