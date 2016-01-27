using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Frontiers;
using Frontiers.World;
using ExtensionMethods;

namespace Frontiers.GUI
{
		public class GUIMapTile : MonoBehaviour
		{
				public static float HeightMeshSize = 1f;
				public WorldMapChunk ChunkToDisplay;
				public Transform tr;
				public Transform TileBackground;
				public MeshFilter HeightMeshFilter;
				public UIDragObject DragObject;
				public Queue <WorldMapLocation> LocationsToDisplay = new Queue <WorldMapLocation>();
				public List <WorldMapLocation> SmallLocations = new List <WorldMapLocation>();
				public List <WorldMapLocation> MediumLocations = new List <WorldMapLocation>();
				public List <WorldMapLocation> LargeLocations = new List <WorldMapLocation>();
				public List <WorldMapLocation> ConstantLocations = new List <WorldMapLocation>();
				public List <int> BlendedWith = new List<int>();
				public GameObject MapTextureObject;
				public GameObject WMIconPrefab;
				public GameObject WMLabelPrefab;
				public GameObject WMMarkPrefab;
				public bool IsArbitrary = false;
				public static float gLocationRadiusMult = 0.025f;
				public List <int> TopVertices = new List<int>();
				public List <int> BotVertices = new List<int>();
				public List <int> LeftVertices = new List<int>();
				public List <int> RightVertices = new List<int>();
				public BoxCollider TileCollider;

				public void Awake()
				{
						tr = transform;
				}

				public void InitializeAsChunk(WorldMapChunk chunkToDisplay)
				{
						ChunkToDisplay = chunkToDisplay;
						name = chunkToDisplay.Name.ToString();
						transform.localPosition = new Vector3(ChunkToDisplay.TileOffset.x, ChunkToDisplay.TileOffset.z, 0f);

						RefreshTexture();
						RefreshGeometry();
				}

				public void InitializeAsArbitraryChunk(WorldMapChunk chunkToDisplay, Transform dragTarget)
				{
						ChunkToDisplay = chunkToDisplay;
						name = chunkToDisplay.Name.ToString();

						transform.localPosition = new Vector3(ChunkToDisplay.TileOffset.x, ChunkToDisplay.TileOffset.z, 0f);
						MapTextureObject.renderer.enabled = true;
						TileBackground.localScale = new Vector3(Globals.WorldChunkSize, Globals.WorldChunkSize, Globals.WorldChunkTerrainHeight);
						DragObject.target = dragTarget;
						IsArbitrary = true;

						RefreshTexture();
						RefreshGeometry();
				}

				public void SetEmptyTileOffset(int xTileOffset, int zTileOffset, Transform dragTarget)
				{
						Vector3 offset = new Vector3((xTileOffset * Globals.WorldChunkSize) + Globals.WorldChunkOffsetX, 0f, (zTileOffset * Globals.WorldChunkSize) + Globals.WorldChunkOffsetZ);
						transform.localPosition = new Vector3(offset.x, offset.z, 0f);
						MapTextureObject.renderer.enabled = true;
						TileBackground.localScale = new Vector3(Globals.WorldChunkSize, Globals.WorldChunkSize, Globals.WorldChunkTerrainHeight);
						DragObject.target = dragTarget;
						ChunkToDisplay = WorldMapChunk.Empty;

						RefreshTexture();
				}

				public void RefreshTexture()
				{
						Texture2D mapTexture = null;
						Texture2D alphaTexture = null;
						if (!IsArbitrary && Mods.Get.Runtime.ChunkMap(ref mapTexture, ChunkToDisplay.Name, "ColorOverlay")) {// "MiniHeightMap")) {
								MapTextureObject.renderer.enabled = true;
								MapTextureObject.renderer.material.SetTexture("_MainTex", mapTexture);
								MapTextureObject.renderer.material.SetTexture("_Mask", mapTexture);
						} else {
								MapTextureObject.renderer.enabled = false;
						}
						//TODO set failsafe water texture
						MapTextureObject.layer = Globals.LayerNumGUIMap;
				}

				public void RefreshGeometry()
				{
						if (ChunkToDisplay.MiniHeightmap == null) {
								Debug.Log("Mini heightmap for " + ChunkToDisplay.Name + " was null, not generating");
								return;
						}

						//create a local copy of our mesh
						Mesh mesh = HeightMeshFilter.mesh;
						//use the mini heightmap to generate vertices
						Vector3[] vertices = mesh.vertices;

						Vector2 uv = new Vector2();
						Color c;
						for (int i = 0; i < vertices.Length; i++) {
								uv.x = Mathf.InverseLerp(0f, HeightMeshSize, vertices[i].x);
								uv.y = Mathf.InverseLerp(HeightMeshSize, 0f, -vertices[i].z);
								//c = ChunkToDisplay.MiniHeightmap.GetPixel(Mathf.FloorToInt(uv.x * ChunkToDisplay.MiniHeightmap.width), Mathf.FloorToInt(uv.y * ChunkToDisplay.MiniHeightmap.height));
								c = ChunkToDisplay.MiniHeightmap.GetPixelBilinear(uv.x, uv.y);
								vertices[i].y = c.r * NGUIWorldMap.ChunkMeshMultiplier;
						}
						mesh.vertices = vertices;

						if (IsArbitrary) {
								TileCollider.enabled = false;
						} else {
								TileCollider.enabled = true;
						}
				}

				public void RefreshOffset() {

				}

				public void Update()
				{
						if (LocationsToDisplay.Count > 0) {
								WorldMapLocation wml = LocationsToDisplay.Dequeue();
								WorldMap.CreateLocationLabel(this, wml);
						}
				}

				public void OnDrawGizmos()
				{
						Gizmos.color = Colors.Alpha(Color.white, 0.1f);
						if (!ChunkToDisplay.IsEmpty) {
								Gizmos.DrawWireCube(TileCollider.bounds.center, TileCollider.bounds.size);
						}

				}

				public void BlendEdges(List<GUIMapTile> chunksDisplayed)
				{
						if (ChunkToDisplay.IsEmpty) {
								//Debug.Log("We're empty");
								return;
						}

						//Debug.Log("Blending " + ChunkToDisplay.ChunkID.ToString () + " with " + ChunkToDisplay.BlendIDTop.ToString() + ", " + ChunkToDisplay.BlendIDBottom.ToString() + ", " + ChunkToDisplay.BlendIDLeft.ToString() + ", " + ChunkToDisplay.BlendIDRight.ToString());
						for (int i = 0; i < chunksDisplayed.Count; i++) {
								GUIMapTile chunkDisplayed = chunksDisplayed[i];

								if (chunkDisplayed == this) {
										//Debug.Log("Whoops, not blending with this");
										continue;
								}

								if (BlendedWith.Contains(chunkDisplayed.ChunkToDisplay.ChunkID)) {
										//Debug.Log("Already blended " + ChunkToDisplay.ChunkID.ToString() + " with " + chunkDisplayed.ChunkToDisplay.ChunkID.ToString());
										continue;
								}

								if ((ChunkToDisplay.BlendIDTop > 0 && chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDTop)
								|| (ChunkToDisplay.BlendIDBottom > 0 && chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDBottom)
								|| (ChunkToDisplay.BlendIDLeft > 0 && chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDLeft)
								|| (ChunkToDisplay.BlendIDRight > 0 && chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDRight)) {

										//Debug.Log("Blending " + chunkDisplayed.ChunkToDisplay.ChunkID.ToString() + " with " + ChunkToDisplay.ChunkID.ToString());
										chunkDisplayed.BlendedWith.Add(ChunkToDisplay.ChunkID);

										Mesh mesh1 = HeightMeshFilter.sharedMesh;
										Mesh mesh2 = chunkDisplayed.HeightMeshFilter.sharedMesh;

										Vector3[] verts1 = mesh1.vertices;
										Vector3[] verts2 = mesh2.vertices;
										Vector3 vert1;
										Vector3 vert2;
										float blendedHeight;
										List<int> indices1 = null;
										List<int> indices2 = null;
										int index1 = 0;
										int index2 = 0;

										if (chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDTop) {
												indices1 = TopVertices;
												indices2 = BotVertices;
										} else if (chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDBottom) {
												indices1 = BotVertices;
												indices2 = TopVertices;
										} else if (chunkDisplayed.ChunkToDisplay.ChunkID == ChunkToDisplay.BlendIDLeft) {
												indices1 = LeftVertices;
												indices2 = RightVertices;
										} else {
												indices1 = RightVertices;
												indices2 = LeftVertices;
										}

										for (int v = 0; v < indices1.Count; v++) {
												index1 = indices1[v];
												index2 = indices2[v];
												vert1 = verts1[index1];
												vert2 = verts2[index2];
												blendedHeight = (vert1.y + vert2.y) / 2;
												vert1.y = blendedHeight;
												vert2.y = blendedHeight;
												verts1[index1] = vert1;
												verts2[index2] = vert2;
										}

										mesh1.vertices = verts1;
										mesh2.vertices = verts2;

										mesh1.RecalculateBounds();
										mesh2.RecalculateBounds();

										System.Array.Clear(verts1, 0, verts1.Length);
										System.Array.Clear(verts2, 0, verts2.Length);

										verts1 = null;
										verts2 = null;
								}
						}
				}

				public bool NearlyEqual(float a, float b, float epsilon)
				{
						float absA = Mathf.Abs(a);
						float absB = Mathf.Abs(b);
						float diff = Mathf.Abs(a - b);

						if (a == b) { // shortcut, handles infinities
								return true;
						} else if (a == 0 || b == 0 || diff < float.MinValue) {
								// a or b is zero or both are extremely close to it
								// relative error is less meaningful here
								return diff < (epsilon * float.MinValue);
						} else { // use relative error
								return diff / (absA + absB) < epsilon;
						}
				}

				protected bool mKeepLoadingLocations = false;

				public void OnDestroy()
				{
						DestroyLocations(SmallLocations);
						DestroyLocations(MediumLocations);
						DestroyLocations(LargeLocations);
						DestroyLocations(ConstantLocations);

						SmallLocations.Clear();
						MediumLocations.Clear();
						LargeLocations.Clear();
						ConstantLocations.Clear();
						LocationsToDisplay.Clear();

						GameObject.Destroy(HeightMeshFilter.mesh);
				}

				protected void DestroyLocations(List <WorldMapLocation> locations)
				{
						foreach (WorldMapLocation wml in locations) {
								if (wml.Icon != null) {
										GameObject.Destroy(wml.Icon.gameObject);
								}
								if (wml.Attention != null) {
										GameObject.Destroy(wml.Attention.gameObject);
								}
								if (wml.Label != null) {
										GameObject.Destroy(wml.Label.gameObject);
								}
						}
				}

				public void OnMinimize()
				{
						mKeepLoadingLocations = false;
				}
		}
}