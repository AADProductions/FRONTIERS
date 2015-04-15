using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
		[ExecuteInEditMode]
		public class TheRift : WIScript
		{
				public TheRiftState State = new TheRiftState();
				public Mesh RiftSegment;
				public Mesh RiftSegmentLOD1;
				public Mesh RiftSegmentLOD2;
				public Material RiftSegmentMaterial;
				public Material RiftSegmentMaterialLOD1;
				public Material RiftSegmentMaterialLOD2;
				public LavaRiver RiftLavaPrefab;
				public GameObject HeatDistortionPrefab;
				public ParticleEmitter RiftSmokePrefab;
				public Light RiftLightPrefab;
				public bool HasBeenGeneratedThisSession = false;
				public Transform FXPivot;
				public Transform SegmentPivot;
				public Transform SegmentPivotOuter;
				public Transform SegmentPivotInner;
				public LavaRiver RiftLava;
				public Spline HeatDistortion;
				public List <Light> PointLights = new List <Light>();
				public List <ParticleEmitter> SmokeSources = new List <ParticleEmitter>();
				public List <Rigidbody> OuterSegments = new List <Rigidbody>();
				public List <MeshRenderer> OuterSegmentRenderers = new List <MeshRenderer>();
				public List <Rigidbody> InnerSegments = new List <Rigidbody>();
				public List <MeshRenderer> InnerSegmentRenderers = new List <MeshRenderer>();
				public List <Vector3> LightTargetPositions = new List<Vector3>();
				/*public float OuterRadius = 3075f;
				public float InnerRadius = 2665f;*/
				public float SmokeHeight = 50f;
				/*public int NumOuterRiftSegments = 204;
				public int NumInnerRiftSegments = 192;
				public float DegreesPerOuterSegment = 1.7647f;
				public float DegreesPerInnerSegment = 1.875f;*/
				public int MaximumYRotationVariation = 10;
				public int MinimumScale = 95;
				public int MaximumScale = 110;
				public int NumLightSources = 10;
				public int NumSmokeSources = 10;
				public float SmokeParticlesRange = 90f;
				public int NumRiftColliders = 3;
				public float ColliderDistance = 100f;
				public float MagmaEffectSpawnValue = 0.99f;
				public string MagmaEffectName = "RiftMagmaEffect";
				public Vector3 MagmaEffectOffset = new Vector3(0f, 125f, 0f);
				public float TargetLightIntensity;
				public float TargetSwapValue = 0.9f;
				public Material RiftSmokeMaterial;
				public Color RiftFogColor;

				public bool HasGeneratedFXThisSession {
						get {
								return FXPivot != null; 
						}
				}

				public override bool EnableAutomatically {
						get {
								return true;
						}
				}

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisibile;
						worlditem.OnInvisible += OnInvisible;
						worlditem.OnAddedToGroup += OnAddedToGroup;
						TargetLightIntensity = RiftLightPrefab.intensity;
				}

				public void OnAddedToGroup()
				{
						if (!HasBeenGeneratedThisSession) {
								BuildRift();
								Cutscene.OnCutsceneStart += OnCutsceneStart;
						}
						if (!HasGeneratedFXThisSession && !mBuildingFX) {
								mBuildingFX = true;
								StartCoroutine(BuildRiftFX());
						}
				}

				public void OnVisibile()
				{
						//sets all the segments & fx & stuff to visible
				}

				public void OnInvisible()
				{
						//sets all the segments to hidden
				}

				protected void BuildRift()
				{
						if (!mBuildingRift) {
								mBuildingRift = true;
								if (State.HasBeenGeneratedOnce) {
										StartCoroutine(BuildRiftSegments());
								} else {
										//if we haven't built it once we have to place each segment
										StartCoroutine(GenerateRiftSegments());
								}
						}
				}

				protected IEnumerator BuildRiftSegments()
				{
						SegmentPivot = gameObject.FindOrCreateChild("SegmentPivot");
						SegmentPivotOuter = SegmentPivot.gameObject.FindOrCreateChild("SegmentPivotOuter");
						SegmentPivotInner = SegmentPivot.gameObject.FindOrCreateChild("SegmentPivotInner");

						if (State.BuildOuterRing) {
								for (int i = 0; i < State.RiftOuterSegments.Count; i++) {
										Transform riftSegment = BuildRiftSegment(i, OuterSegments, OuterSegmentRenderers).transform;
										riftSegment.parent = SegmentPivotOuter;
										State.RiftOuterSegments[i].ApplyTo(riftSegment, true);
								}
						}
						if (State.BuildInnerRing) {
								for (int i = 0; i < State.RiftInnerSegments.Count; i++) {
										Transform riftSegment = BuildRiftSegment(i, InnerSegments, InnerSegmentRenderers).transform;
										riftSegment.parent = SegmentPivotInner;
										State.RiftInnerSegments[i].ApplyTo(riftSegment, true);
								}
						}
						HasBeenGeneratedThisSession = true;
						mBuildingRift = false;
						yield break;
				}

				protected IEnumerator GenerateRiftSegments()
				{
						SegmentPivot = gameObject.FindOrCreateChild("SegmentPivot");
						SegmentPivotOuter = SegmentPivot.gameObject.FindOrCreateChild("SegmentPivotOuter");
						SegmentPivotInner = SegmentPivot.gameObject.FindOrCreateChild("SegmentPivotInner");
						Transform segmentRigidBodyOuter = SegmentPivot.gameObject.FindOrCreateChild("SegmentRigidBodyOuter");
						Transform segmentRigidBodyInner = SegmentPivot.gameObject.FindOrCreateChild("SegmentRigidBodyInner");
						int yieldCounter = 0;
						float nextRotation;
						float nextScale;
						//get a randomizer based on the world seed
						#if UNITY_EDITOR
						System.Random random = new System.Random(0);
						#else
						System.Random random = new System.Random(Profile.Get.CurrentGame.Seed);
						#endif

						if (State.BuildOuterRing) {
								for (int i = 0; i < State.NumOuterRiftSegments; i++) {
										GameObject riftSegment = BuildRiftSegment(i, OuterSegments, OuterSegmentRenderers);
										Transform riftSegmentTransform = riftSegment.transform;
										riftSegmentTransform.parent = SegmentPivot;
										riftSegmentTransform.ResetLocal();
										riftSegmentTransform.Translate(0f, 0f, State.OuterRadius);
										nextScale = (((float)random.Next(MinimumScale, MaximumScale)) / 100);
										riftSegmentTransform.localScale = Vector3.one * nextScale;
										riftSegmentTransform.parent = SegmentPivotOuter;
										SegmentPivotOuter.Rotate(0f, State.DegreesPerOuterSegment, 0f);
										nextRotation = ((float)random.Next(-MaximumYRotationVariation * 100, MaximumYRotationVariation * 100)) / 100;
										riftSegmentTransform.Rotate(0f, nextRotation, 0f);
										yieldCounter++;
										if (!GameManager.Is(FGameState.InGame)) {
												yieldCounter++;
												if (yieldCounter > 7) {
														yieldCounter = 0;
														yield return null;
												}
										}
								}
						}

						if (State.BuildInnerRing) {
								for (int i = 0; i < State.NumInnerRiftSegments; i++) {
										GameObject riftSegment = BuildRiftSegment(i, InnerSegments, InnerSegmentRenderers);
										Transform riftSegmentTransform = riftSegment.transform;
										riftSegmentTransform.parent = SegmentPivot;
										riftSegmentTransform.ResetLocal();
										riftSegmentTransform.Translate(0f, 0f, State.InnerRadius);
										nextScale = (((float)random.Next(MinimumScale, MaximumScale)) / 100);
										riftSegmentTransform.localScale = Vector3.one * nextScale;
										riftSegmentTransform.parent = SegmentPivotInner;
										SegmentPivotInner.Rotate(0f, State.DegreesPerInnerSegment, 0f);
										nextRotation = ((float)random.Next(-MaximumYRotationVariation * 100, MaximumYRotationVariation * 100)) / 100;
										riftSegmentTransform.Rotate(0f, 180f + nextRotation, 0f);
										yieldCounter++;
										if (!GameManager.Is(FGameState.InGame)) {
												yieldCounter++;
												if (yieldCounter > 7) {
														yieldCounter = 0;
														yield return null;
												}
										}
								}
						}

						yield return null;

						#if UNITY_EDITOR
						if (Application.isPlaying) {
						//should i save rift data...?
								//now that we've generated it, save the positions so we don't have to do it again
								for (int i = 0; i < OuterSegmentRenderers.Count; i++) {
										State.RiftOuterSegments.Add(new STransform(OuterSegmentRenderers[i].transform, true));
								}
								for (int i = 0; i < InnerSegmentRenderers.Count; i++) {
										State.RiftInnerSegments.Add(new STransform(InnerSegmentRenderers[i].transform, true));
								}
						}
						HasBeenGeneratedThisSession = true;
						State.HasBeenGeneratedOnce = true;
						#else
						//now that we've generated it, save the positions so we don't have to do it again
						for (int i = 0; i < OuterSegmentRenderers.Count; i++) {
								State.RiftOuterSegments.Add(new STransform(OuterSegmentRenderers[i].transform, true));
						}
						for (int i = 0; i < InnerSegmentRenderers.Count; i++) {
								State.RiftInnerSegments.Add(new STransform(InnerSegmentRenderers[i].transform, true));
						}
						HasBeenGeneratedThisSession = true;
						State.HasBeenGeneratedOnce = true;
						#endif

						mBuildingRift = false;

						yield break;
				}

				protected IEnumerator BuildRiftFX()
				{
						Debug.Log("Generating FX");
						FXPivot = gameObject.FindOrCreateChild("FXPivot");
						FXPivot.Rotate(0f, -SmokeParticlesRange / 2, 0f);//so our Z points to the middle with particles on either side
						for (int i = 0; i < NumSmokeSources; i++) {
								GameObject smokeParticlesGameObject = GameObject.Instantiate(RiftSmokePrefab.gameObject) as GameObject;
								ParticleEmitter smokeParticles = smokeParticlesGameObject.GetComponent <ParticleEmitter>();
								Transform smokeParticlesTransform = smokeParticles.transform;
								smokeParticlesTransform.parent = worlditem.tr;
								smokeParticlesTransform.ResetLocal();
								smokeParticlesTransform.Translate(0f, SmokeHeight, (State.OuterRadius + State.InnerRadius) / 2);
								smokeParticlesTransform.parent = FXPivot;
								smokeParticles.Simulate(1.0f);
								SmokeSources.Add(smokeParticles);

								GameObject lightGameObject = null;
								if (State.BuildPointLights) {
										lightGameObject = GameObject.Instantiate(RiftLightPrefab.gameObject) as GameObject;
										lightGameObject.transform.parent = FXPivot;
										lightGameObject.transform.localPosition = smokeParticlesTransform.localPosition;
										smokeParticlesTransform.parent = lightGameObject.transform;
										PointLights.Add(lightGameObject.light);
								}

								FXPivot.Rotate(0f, SmokeParticlesRange / NumSmokeSources, 0f);

								if (State.BuildPointLights) {
										LightTargetPositions.Add(lightGameObject.transform.localPosition);
								}
						}

						mBuildingFX = false;
						yield break;
				}

				protected GameObject BuildRiftSegment(int segmentNumber, List <Rigidbody> rigidBodies, List <MeshRenderer> meshRenderers)
				{
						GameObject riftSegment = new GameObject(segmentNumber.ToString());
						GameObject riftSegmentLOD1 = riftSegment.CreateChild("LOD1").gameObject;
						GameObject riftSegmentLOD2 = riftSegment.CreateChild("LOD2").gameObject;

						riftSegment.layer = Globals.LayerNumSolidTerrain;
						riftSegmentLOD1.layer = Globals.LayerNumScenery;
						riftSegmentLOD1.layer = Globals.LayerNumScenery;
						riftSegment.tag = Globals.TagGroundStone;

						MeshFilter mf = riftSegment.AddComponent <MeshFilter>();
						MeshRenderer pr = riftSegment.AddComponent <MeshRenderer>();
						pr.sharedMaterial = RiftSegmentMaterial;
						mf.sharedMesh = RiftSegment;
						Rigidbody rb = riftSegment.AddComponent <Rigidbody>();
						rb.isKinematic = true;
						rb.detectCollisions = true;
						#if UNITY_EDITOR
						//should i create colliders?
						#else
						MeshCollider mc = riftSegment.AddComponent <MeshCollider>();
						mc.sharedMesh = RiftSegment;
						#endif

						rigidBodies.Add(rb);
						meshRenderers.Add(pr);

						#if UNITY_EDITOR
						MeshRenderer lod1Mr = riftSegmentLOD1.AddComponent <MeshRenderer>();
						lod1Mr.sharedMaterial = RiftSegmentMaterialLOD1;
						mf = riftSegmentLOD1.AddComponent <MeshFilter>();
						mf.sharedMesh = RiftSegmentLOD1;
						#else
						MeshRenderer lod1Mr = riftSegmentLOD1.AddComponent <MeshRenderer>();
						lod1Mr.sharedMaterial = RiftSegmentMaterialLOD1;
						mf = riftSegmentLOD1.AddComponent <MeshFilter>();
						mf.sharedMesh = RiftSegmentLOD1;

						MeshRenderer lod2Mr = riftSegmentLOD2.AddComponent <MeshRenderer>();
						lod2Mr.sharedMaterial = RiftSegmentMaterialLOD2;
						mf = riftSegmentLOD2.AddComponent <MeshFilter>();
						mf.sharedMesh = RiftSegmentLOD2;

						LODGroup lodGroup = riftSegment.AddComponent <LODGroup>();

						Renderer[] primaryRenderer = new Renderer [] { pr };
						Renderer[] lod1Renderer = new Renderer [] { lod1Mr };
						Renderer[] lod2Renderer = new Renderer [] { lod2Mr };

						LOD primary = new LOD(Globals.SceneryLODRatioPrimary, new Renderer [] { pr });
						LOD lod1 = new LOD(Globals.SceneryLODRatioSecondary, new Renderer [] { lod1Mr });
						LOD lod2 = new LOD(Globals.SceneryLODRatioOff, new Renderer [] { lod2Mr });
						lodGroup.SetLODS(new LOD [] { primary, lod1, lod2 });
						#endif
		
						return riftSegment;
				}

				public void OnCutsceneStart()
				{		//if we haven't been generated yet interrupt the cutscene
						if (!HasBeenGeneratedThisSession) {
								if (Cutscene.SuspendCutsceneStart ()) {
										StartCoroutine(LoadRiftBeforeCutscene());
								}
						}
				}

				public void Update()
				{
						if (!HasGeneratedFXThisSession || mBuildingFX || (worlditem.Is(WIActiveState.Visible | WIActiveState.Active) && !Cutscene.IsActive)) {
								return;
						}
						//this takes care of lava, fx and whatnot
						gameCameraPosition = GameManager.Get.GameCamera.transform.position;
						gameCameraPosition.y = worlditem.tr.position.y;
						FXPivot.LookAt(gameCameraPosition);

						if (GameManager.Is (FGameState.Cutscene | FGameState.InGame)) {
							if (UnityEngine.Random.value > MagmaEffectSpawnValue) {
									//select a random light source
									Light lightSource = PointLights[UnityEngine.Random.Range(0, PointLights.Count)];
									FXManager.Get.SpawnFX(lightSource.transform.position + MagmaEffectOffset, MagmaEffectName);
									lightSource.intensity = TargetLightIntensity * 2;
							}

							float swapValue = UnityEngine.Random.value;
							if (swapValue > TargetSwapValue) {
									//swap 2 random target points
									//this will keep the lights & smoke moving about
									int index1 = UnityEngine.Random.Range(0, LightTargetPositions.Count);
									int index2 = index1++;
									if (swapValue > 0.5f) {
											index2 = index1--;
									}
									if (index2 < LightTargetPositions.Count && index2 > 0) {
											Vector3 position1 = LightTargetPositions[index1];
											LightTargetPositions[index1] = LightTargetPositions[index2];
											LightTargetPositions[index2] = position1;
									}
							}
						}

						for (int i = 0; i < PointLights.Count; i++) {
								PointLights[i].intensity = Mathf.Lerp(PointLights[i].intensity, TargetLightIntensity, 0.15f);
								PointLights[i].transform.localPosition = Vector3.Lerp(PointLights[i].transform.localPosition, LightTargetPositions[i], 0.05f);
						}

						if (WorldClock.IsDay) {
								//during the day fog is relatively dense
								RiftSmokeMaterial.SetColor("_FogColor", RenderSettings.fogColor);
								RiftSmokeMaterial.SetFloat("_FogStart", RenderSettings.fogStartDistance);
								RiftSmokeMaterial.SetFloat("_FogEnd", RenderSettings.fogEndDistance);
						} else {
								//during the night it's not so dense
								RiftSmokeMaterial.SetColor("_FogColor", RenderSettings.fogColor);
								RiftSmokeMaterial.SetFloat("_FogStart", RenderSettings.fogStartDistance);
								RiftSmokeMaterial.SetFloat("_FogEnd", (RenderSettings.fogEndDistance + GameManager.Get.GameCamera.farClipPlane));
						}
				}

				protected bool mLoadingRiftBeforeCutscene = false;

				protected IEnumerator LoadRiftBeforeCutscene()
				{		//this tells the cutscene what we're up to while we build the rest of the Rift
						StartCoroutine(GUI.GUILoading.LoadStart(Frontiers.GUI.GUILoading.Mode.FullScreenBlack));
						while (!HasBeenGeneratedThisSession) {
								int percent = Mathf.FloorToInt(((float)(OuterSegments.Count + InnerSegments.Count) / (float)(State.NumInnerRiftSegments + State.NumOuterRiftSegments)) * 100);
								GUI.GUILoading.ActivityInfo = "Generating the Rift";
								GUI.GUILoading.DetailsInfo = "Creating Rift Segments for first time (" + percent.ToString() + "%)";
								yield return null;
						}
						int numGenerated = OuterSegments.Count + InnerSegments.Count;
						int numRequired = 0;
						if (State.BuildInnerRing) {
								numRequired += State.NumInnerRiftSegments;
						}
						if (State.BuildOuterRing) {
								numRequired += State.NumOuterRiftSegments;
						}
						numRequired -= 1;
						if (numGenerated >= numRequired) {
								Cutscene.Unsuspend ();
						}
						Cutscene.Unsuspend ();
						GUI.GUILoading.DetailsInfo = "Finished generating the Rift";
						StartCoroutine(GUI.GUILoading.LoadFinish());
						yield break;
				}

				protected void UpdateColliders()
				{
//			//if the primary LOD is active set the rigidbody active as well
//			for (int i = 0; i < OuterSegments.Count; i++) {
//				OuterSegments [i].detectCollisions = OuterSegmentRenderers [i].enabled;
//			}
//
//			for (int i = 0; i < InnerSegments.Count; i++) {
//				InnerSegments [i].detectCollisions = InnerSegmentRenderers [i].enabled;
//			}
				}

				protected Vector3 gameCameraPosition;
				protected bool mBuildingRift = false;
				protected bool mBuildingFX = false;
				#if UNITY_EDITOR
				public void BuildTempRift()
				{
						var buildRiftSegments = BuildRiftSegments();
						while (buildRiftSegments.MoveNext()) {
								//building rift
						}
						var generateRiftSegments = GenerateRiftSegments();
						while (generateRiftSegments.MoveNext()) {
								//building rift
						}
				}
				#endif
		}

		[Serializable]
		public class TheRiftState
		{
				public float OuterRadius = 3075f;
				public float InnerRadius = 2665f;
				public int NumOuterRiftSegments = 204;
				public int NumInnerRiftSegments = 192;
				public float DegreesPerOuterSegment = 1.7647f;
				public float DegreesPerInnerSegment = 1.875f;
				public bool BuildOuterRing = true;
				public bool BuildInnerRing = true;
				public bool BuildPointLights = true;
				public bool HasBeenGeneratedOnce = false;
				public List <STransform> RiftInnerSegments = new List <STransform>();
				public List <STransform> RiftOuterSegments = new List <STransform>();
		}
}
