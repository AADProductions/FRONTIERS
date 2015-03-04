using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World
{
		public class Painting : WIScript
		{
				public PaintingState State = new PaintingState();
				public Renderer PaintingRenderer;

				public override void OnInitialized()
				{
						if (!string.IsNullOrEmpty(State.PaintingMapName)) {
								Texture2D painting = null;
								IEnumerator loader = null;
								if (Mods.Get.Runtime.GenericTexture(ref painting, ref loader, State.PaintingMapName, "Painting", false)) {
										if (loader != null) {
												StartCoroutine(loader);
										}
										//it will look correct once the loader is done
										PaintingRenderer.material.mainTexture = painting;
								}
						}
				}
		}

		[Serializable]
		public class PaintingState
		{
				public string PaintingMapName = string.Empty;
		}
}
