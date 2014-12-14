using System;
using UnityEngine;
using Frontiers;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;

public class MagicEffectSphere : EffectSphere
{
		public Transform SphereTransform;
		public Projector EdgeProjector;
		public MeshRenderer SphereRenderer;
		public MeshFilter SphereFilter;
		public Color SphereTargetColor;
		public Color ProjectorTargetColor;
		public float Intensity;

		public override void Start()
		{
				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//can we do something about this?
				sphere.collider.enabled = false;
				SphereTransform = sphere.transform;
				SphereTransform.parent = transform;
				SphereTransform.ResetLocal();

				SphereFilter = sphere.GetComponent <MeshFilter>();
				SphereRenderer = sphere.GetComponent <MeshRenderer>();
				SphereRenderer.material = Mats.Get.MagicEffectMaterial;

				sphere.layer = Globals.LayerNumScenery;

				GameObject edgeProjectorObject = gameObject.FindOrCreateChild("EdgeProjector").gameObject;
				EdgeProjector = edgeProjectorObject.GetOrAdd <Projector>();
				EdgeProjector.material = Mats.Get.SpellEffectProjectorMaterial;
				EdgeProjector.nearClipPlane = 1.0f;
				EdgeProjector.farClipPlane = 1.01f;
				EdgeProjector.fieldOfView = 1.0f;
				EdgeProjector.aspectRatio = 1.0f;
				EdgeProjector.isOrthoGraphic = true;
				EdgeProjector.orthographicSize = 1.0f;
				EdgeProjector.ignoreLayers = Globals.LayerScenery;
				EdgeProjector.transform.Rotate(90f, 0f, 0f);
				edgeProjectorObject.layer = Globals.LayerNumScenery;

				Intensity = 0f;
				SphereTargetColor = SphereRenderer.material.color;
				ProjectorTargetColor = SphereRenderer.material.color;

				base.Start();
		}

		protected override void OnUpdateRadius()
		{
				Intensity = Mathf.Lerp(Intensity, 1f, Collider.radius / TargetRadius);
				SphereRenderer.material.color = Colors.Alpha(SphereTargetColor, Intensity);
				EdgeProjector.material.color = Color.Lerp(Color.black, ProjectorTargetColor, Intensity);

				EdgeProjector.orthographicSize = Collider.radius * 1.075f + (Mathf.Sin((float)WorldClock.DayCycleCurrentNormalized) * 0.1f);
				EdgeProjector.nearClipPlane = -Collider.radius;
				EdgeProjector.farClipPlane = Collider.radius;

				SphereTransform.localScale = Vector3.one * (EdgeProjector.orthographicSize * 2f);
				SphereTransform.Rotate(0f, (float)(WorldClock.RTDeltaTime * 0.1), 0f);
		}

		protected override void OnCooldown()
		{
				float normalizedCooldownLeft = (float)((mCooldownStartTime - mCooldownEndTime) / RTDuration);
				Intensity = Mathf.Lerp(Intensity, 0f, normalizedCooldownLeft);
				SphereRenderer.material.color = Colors.Alpha(SphereTargetColor, Intensity);
				EdgeProjector.material.color = Color.Lerp(Color.black, ProjectorTargetColor, Intensity);
		}
}
