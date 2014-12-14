using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World
{
		public class Placeable : WIScript
		{
				public List <SurfaceOrientation> PermittedSurfaceOrientations	= new List <SurfaceOrientation>();
				public LayerMask PermittedLayers = Globals.LayersActive;
				public PlacementOrientation Orientation = PlacementOrientation.Gravity;
				public Receptacle Receptacle;
				public List <string> PermittedTargetScripts = new List <string>();

				public override bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						bool isPermitted = true;
						if (PermittedTargetScripts.Count > 0) {
								isPermitted &= targetObject.HasAtLeastOne(PermittedTargetScripts);
						}
						return isPermitted;
				}
		}

		public enum PlacementOrientation
		{
				Surface,
				InvertedSurface,
				Gravity,
				InvertedGravity,
				Random
		}
}