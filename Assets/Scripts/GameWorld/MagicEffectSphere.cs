using System;
using UnityEngine;
using Frontiers;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;

namespace Frontiers.World.Gameplay {
	public class MagicEffectSphere : EffectSphere
	{
		public Transform SphereTransform;
		public Projector EdgeProjector;
		public MeshRenderer SphereRenderer;
		public MeshFilter SphereFilter;
		public Color SphereTargetColor;
		public Color ProjectorTargetColor;
		public float Intensity;

		public override void Start ()
		{
			if (SphereRenderer == null) {
				GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				//can we do something about this?
				sphere.GetComponent<Collider>().enabled = false;
				SphereTransform = sphere.transform;
				SphereTransform.parent = transform;
				SphereTransform.ResetLocal ();

				SphereFilter = sphere.GetComponent <MeshFilter> ();
				SphereRenderer = sphere.GetComponent <MeshRenderer> ();
				SphereRenderer.castShadows = false;
				SphereRenderer.receiveShadows = false;
				//SphereRenderer.material = Mats.Get.MagicEffectMaterial;
				sphere.layer = Globals.LayerNumScenery;
			}

			GameObject edgeProjectorObject = gameObject.FindOrCreateChild ("EdgeProjector").gameObject;
			EdgeProjector = edgeProjectorObject.GetOrAdd <Projector> ();
			//EdgeProjector.material = Mats.Get.SpellEffectProjectorMaterial;
			EdgeProjector.nearClipPlane = 1.0f;
			EdgeProjector.farClipPlane = 1.01f;
			EdgeProjector.fieldOfView = 1.0f;
			EdgeProjector.aspectRatio = 1.0f;
			EdgeProjector.orthographic = true;
			EdgeProjector.orthographicSize = 1.0f;
			EdgeProjector.ignoreLayers = Globals.LayerScenery;
			EdgeProjector.transform.localRotation = Quaternion.Euler (90f, 0f, 0f);
			edgeProjectorObject.layer = Globals.LayerNumScenery;

			Intensity = 0f;
			SphereTargetColor = SphereRenderer.material.color;
			ProjectorTargetColor = SphereRenderer.material.color;

			base.Start ();
		}

		protected override void OnUpdateRadius ()
		{
			Intensity = Mathf.Lerp (Intensity, 1f, Collider.radius / TargetRadius);
			SphereRenderer.material.color = Colors.Alpha (SphereTargetColor, Intensity);
			EdgeProjector.material.color = Color.Lerp (Color.black, ProjectorTargetColor, Intensity);

			EdgeProjector.orthographicSize = Collider.radius * 1.075f + (Mathf.Sin ((float)WorldClock.DayCycleCurrentNormalized) * 0.1f);
			EdgeProjector.nearClipPlane = -Collider.radius;
			EdgeProjector.farClipPlane = Collider.radius;
			EdgeProjector.transform.localPosition = UnityEngine.Random.onUnitSphere * 0.05f;

			SphereTransform.localScale = Vector3.one * (EdgeProjector.orthographicSize);
			SphereTransform.Rotate (0f, (float)(WorldClock.RTDeltaTime * 0.1), 0f);
			SphereTransform.localPosition = UnityEngine.Random.onUnitSphere * 0.05f;
		}

		protected override void OnCooldown ()
		{
			float normalizedCooldownLeft = (float)((mCooldownStartTime - mCooldownEndTime) / RTDuration);
			Intensity = Mathf.Lerp (Intensity, 0f, normalizedCooldownLeft);
			SphereRenderer.material.color = Colors.Alpha (SphereTargetColor, Intensity);
			EdgeProjector.material.color = Color.Lerp (Color.black, ProjectorTargetColor, Intensity);
		}
	}
}