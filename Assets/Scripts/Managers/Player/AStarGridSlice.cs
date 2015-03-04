using UnityEngine;
using System.Collections;
//using Pathfinding;

public class AStarGridSlice : MonoBehaviour
{
	public bool CellIsDirty = false;
//	public GridGraph	GridSlice;
	/// <summary>
	/// Global variable representing cell size.
	/// </summary>
	public static float CellSize = Globals.PathfindingGridSliceSize;
	/// <summary>
	/// The nine cells in the game world.
	/// </summary>
	private AStarGridSlice[] Cells;
	/// <summary>
	/// Is this the center cell?
	/// </summary>
	/// <remarks>
	/// Set this on the center cell during object creation.
	/// </remarks>
	public bool isCenterCell;
	/// <summary>
	/// This X position of this grid cell
	/// </summary>
	private int mCellX;
	/// <summary>
	/// This Z position of this grid cell
	/// </summary>
	private int mCellZ;

	/// <summary>
	/// Initialize the cell.
	/// </summary>
	void Start ()
	{
		LocalToCell (transform.localPosition, out mCellX, out mCellZ);
		Cells = FindObjectsOfType<AStarGridSlice> () as AStarGridSlice[];
	}

	/// <summary>
	/// Moves a cell to a new location
	/// </summary>
	/// <param name="cellX">The X position of the grid cell.</param>
	/// <param name="cellZ">The Z position of the grid cell.</param>
	/// <param name="isCenter">Is this the new center cell?</param>
	public void MoveCell (int cellX, int cellZ, bool isCenter)
	{
		// Update the cell transform
		transform.localPosition = new Vector3 (CellSize * cellX,
			transform.localPosition.y,
			CellSize * cellZ);
		// Update script state
		mCellX = cellX;
		mCellZ = cellZ;
		isCenterCell = isCenter;

//		GridSlice.center 	= transform.position;
		CellIsDirty = true;
	}

	/// <summary>
	/// Determine which cell corresponds to a
	/// </summary>
	/// <param name="position"></param>
	/// <param name="cellX"></param>
	/// <param name="cellZ"></param>
	private void LocalToCell (Vector3 position, out int cellX, out int cellZ)
	{
		cellX = Mathf.RoundToInt (position.x / CellSize);
		cellZ = Mathf.RoundToInt (position.z / CellSize);
	}

	/// <summary>
	/// Refresh the last player position (every two seconds)
	/// </summary>
	/// <param name="playerPosition">The new position of the player</param>
	public bool Refresh (Vector3 playerPosition)
	{
		// If this is not the center cell, we are done.
		if (!isCenterCell)
			return false;

		// Get the current cell location of the player
		int currCellX, currCellZ;
		LocalToCell (playerPosition, out currCellX, out currCellZ);

		// If the player has not crossed a cell boundary, do nothing.
		if (mCellX == currCellX && mCellZ == currCellZ)
			return false;

		// Store temporary copies of previous cell position.
		int lastCellX = mCellX;
		int lastCellZ = mCellZ;

		// Move the out-of-range cells to their new position
//		foreach (AStarGridSlice cell in Cells) {
//			// Get current offset
//			int currDistX = cell.mCellX - currCellX;
//			int currDistZ = cell.mCellZ - currCellZ;
//
//			// Determine the previous offset
//			int lastDistX = cell.mCellX - lastCellX;
//			int lastDistZ = cell.mCellZ - lastCellZ;
//
//			// Determine the new position of the cell
//			int nextCellX = currCellX;
//			int nextCellZ = currCellZ;
//
//			// Determine new cell X position
//			if (-1 <= currDistX && currDistX <= 1)
//				nextCellX = cell.mCellX;
//			else
//				nextCellX -= lastDistX;
//
//			// Determine new cell Z position
//			if (-1 <= currDistZ && currDistZ <= 1)
//				nextCellZ = cell.mCellZ;
//			else
//				nextCellZ -= lastDistZ;
//
//			// Move the cell to its new position
//			cell.MoveCell (nextCellX, nextCellZ, nextCellX == currCellX && nextCellZ == currCellZ);
//		}

		//refresh this grid
//		GridSlice.center 	= transform.position;
		CellIsDirty = true;

		return true;
	}

	private void OnDrawGizmos ()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawWireCube (transform.position, Vector3.one * Globals.PathfindingGridSliceSize);
	}
}
