using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers
{
		public class Meshes : Manager
		{
				public static Meshes Get;
				public List <Mesh> MeshesToBlacken = new List<Mesh>();

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						//sets all vertex colors to black
						//used to prevent rocks from being wiggly
						for (int i = 0; i < MeshesToBlacken.Count; i++) {
								Mesh mesh = MeshesToBlacken[i];
								mesh.colors = new Color [mesh.vertices.Length];//black by default
						}
				}

				public Mesh WorldMapGatewayEntranceLeftIcon;
				public Mesh WorldMapGatewayEntranceRightIcon;
				public Mesh WorldMapIcon;
				public Mesh WorldMapIconHighlight;
				public Mesh WorldMapIconSelect;
				public Mesh GroundPathPlane;
				public Mesh EffectSphereMesh;

				public Mesh GetMeshByType(string type)
				{
						Mesh mesh = null;
						if (!mMeshesByType.TryGetValue(type, out mesh)) {
								mesh = new Mesh();
								mesh.vertices	= WorldMapIcon.vertices;
								mesh.triangles	= WorldMapIcon.triangles;
								mesh.normals	= WorldMapIcon.normals;
								mesh.uv = Mats.Get.Icons.MapIconUVsByType(type);
								mMeshesByType.Add(type, mesh);
						}
						return mesh;
				}

				public Dictionary <string, Mesh> mMeshesByType = new Dictionary <string, Mesh>();
		}
}