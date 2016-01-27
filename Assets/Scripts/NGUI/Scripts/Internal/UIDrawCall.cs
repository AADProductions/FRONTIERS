//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	public enum Clipping
	{
		None,
		HardClip,
		// Uses the hardware clip() function -- may be slow on some mobile devices
		AlphaClip,
		// Adjust the alpha, compatible with all devices
		SoftClip,
		// Alpha-based clipping with a softened edge
	}

	Transform mTrans;
	// Cached transform
	Material mSharedMat;
	// Material used by this screen
	Mesh mMesh;
	// Generated mesh
	MeshFilter mFilter;
	// Mesh filter for this draw call
	MeshRenderer	mRen;
	// Mesh renderer for this screen
	Clipping mClipping;
	// Clipping mode
	Vector4 mClipRange;
	// Clipping, if used
	Vector2 mClipSoft;
	// Clipping softness
	Material mClippedMat;
	// Instantiated material, if necessary
	Material mDepthMat;
	// Depth-writing material, created if necessary
	int[] mIndices;
	// Cached indices
	bool mDepthPass = false;
	bool mReset = true;

	/// <summary>
	/// Whether an additional pass will be created to render the geometry to the depth buffer first.
	/// </summary>

	public bool depthPass {
		get { return mDepthPass; }
		set {
			if (mDepthPass != value) {
				mDepthPass = value;
				mReset = true;
			}
		}
	}

	/// <summary>
	/// Transform is cached for speed and efficiency.
	/// </summary>

	public Transform cachedTransform {
		get {
			if (mTrans == null)
				mTrans = transform;
			return mTrans;
		}
	}

	/// <summary>
	/// Material used by this screen.
	/// </summary>

	public Material material { get { return mSharedMat; } set { mSharedMat = value; } }

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return mMesh.vertexCount >> 1; } }

	/// <summary>
	/// Clipping used by the draw call
	/// </summary>

	public Clipping clipping {
		get { return mClipping; }
		set {
			if (mClipping != value) {
				mClipping = value;
				mReset = true;
			}
		}
	}

	/// <summary>
	/// Clip range set by the panel -- used with a shader that has the "_ClipRange" property.
	/// </summary>

	public Vector4 clipRange { get { return mClipRange; } set { mClipRange = value; } }

	/// <summary>
	/// Clipping softness factor, if soft clipping is used.
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoft; } set { mClipSoft = value; } }

	/// <summary>
	/// Update the renderer's materials.
	/// </summary>

	void UpdateMaterials()
	{
		bool useClipping = (mClipping != Clipping.None);

		// If clipping should be used, create the clipped material
		if (useClipping) {
			Shader shader = null;

			if (mClipping != Clipping.None) {

				const string hard	= " (HardClip)";
				const string alpha	= " (AlphaClip)";
				const string soft	= " (SoftClip)";

				const string hardVR	= " (HardClip VR)";
				const string alphaVR 	= " (AlphaClip VR)";
				const string softVR	= " (SoftClip VR)";

				const string vrStandard	= " (Oculus A)";

				// Figure out the normal shader's name
				string shaderName = mSharedMat.shader.name;

				#if UNITY_EDITOR
				if (Frontiers.VRManager.VRMode | Frontiers.VRManager.VRTestingMode) {
				#else
				if (Frontiers.VRManager.VRMode) {
				#endif
					shaderName = shaderName.Replace(hard, "");
					shaderName = shaderName.Replace(alpha, "");
					shaderName = shaderName.Replace(soft, "");
					shaderName = shaderName.Replace(vrStandard, "");
					// Try to find the new shader
					if (mClipping == Clipping.HardClip)
						shader = Shader.Find(shaderName + hardVR);
					else if (mClipping == Clipping.AlphaClip)
						shader = Shader.Find(shaderName + alphaVR);
					else if (mClipping == Clipping.SoftClip)
						shader = Shader.Find(shaderName + softVR);
				} else {
					shaderName = shaderName.Replace(hard, "");
					shaderName = shaderName.Replace(alpha, "");
					shaderName = shaderName.Replace(soft, "");
					// Try to find the new shader
					if (mClipping == Clipping.HardClip)
						shader = Shader.Find(shaderName + hard);
					else if (mClipping == Clipping.AlphaClip)
						shader = Shader.Find(shaderName + alpha);
					else if (mClipping == Clipping.SoftClip)
						shader = Shader.Find(shaderName + soft);
				}

				// If there is a valid shader, assign it to the custom material
				if (shader == null)
					mClipping = Clipping.None;
			}

			// If we found the shader, create a new material
			if (shader != null) {
				mClippedMat = new Material(mSharedMat);
				mClippedMat.shader = shader;
			}
		} else if (mClippedMat != null) {
			NGUITools.Destroy(mClippedMat);
			mClippedMat = null;
		}

		// If depth pass should be used, create the depth material
		if (mDepthPass) {
			if (mDepthMat == null) {
				Shader shader = Shader.Find("Depth");
				mDepthMat = new Material(shader);
				mDepthMat.mainTexture = mSharedMat.mainTexture;
			}
		} else if (mDepthMat != null) {
			NGUITools.Destroy(mDepthMat);
			mDepthMat = null;
		}

		// Determine which material should be used
		Material mat = (mClippedMat != null) ? mClippedMat : mSharedMat;

		if (mDepthMat != null) {
			// If we're already using this material, do nothing
			if (mRen.sharedMaterials != null && mRen.sharedMaterials.Length == 2 && mRen.sharedMaterials[1] == mat)
				return;

			// Set the double material
			mRen.sharedMaterials = new Material[] { mDepthMat, mat };
		} else if (mRen.sharedMaterial != mat) {
			mRen.sharedMaterials = new Material[] { mat };
		}
	}

	/// <summary>
	/// Set the draw call's geometry.
	/// </summary>

	public void Set(BetterList<Vector3> verts, BetterList<Vector3> norms, BetterList<Vector4> tans, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		int count = verts.size;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == uvs.size && count == cols.size) && (count % 4) == 0) {
			int index = 0;

			// It takes 6 indices to draw a quad of 4 vertices
			int indexCount = (count >> 1) * 3;

			// Populate the index buffer
			if (mIndices == null || mIndices.Length != indexCount) {
				mIndices = new int[indexCount];

				for (int i = 0; i < count; i += 4) {
					mIndices[index++] = i;
					mIndices[index++] = i + 1;
					mIndices[index++] = i + 2;

					mIndices[index++] = i + 2;
					mIndices[index++] = i + 3;
					mIndices[index++] = i;
				}
			}

			// Cache all components
			if (mFilter == null)
				mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null)
				mFilter = gameObject.AddComponent<MeshFilter>();
			if (mRen == null)
				mRen = gameObject.GetComponent<MeshRenderer>();

			if (mRen == null) {
				mRen = gameObject.AddComponent<MeshRenderer>();
				UpdateMaterials();
			}

			if (verts.size < 65000) {
				if (mMesh == null) {
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = "UIDrawCall for " + mSharedMat.name;
				} else {
					mMesh.Clear();
				}

				// Set the mesh values
				mMesh.vertices = verts.ToArray();
				if (norms != null)
					mMesh.normals = norms.ToArray();
				if (tans != null)
					mMesh.tangents = tans.ToArray();
				mMesh.uv = uvs.ToArray();
				mMesh.colors = cols.ToArray();
				mMesh.triangles = mIndices;
				mMesh.RecalculateBounds();
				mFilter.mesh = mMesh;
			} else {
				if (mMesh != null)
					mMesh.Clear();
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}
		} else {
			if (mMesh != null)
				mMesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}
	}

	/// <summary>
	/// This function is called when it's clear that the object will be rendered.
	/// We want to set the shader used by the material, creating a copy of the material in the process.
	/// We also want to update the material's properties before it's actually used.
	/// </summary>

	void OnWillRenderObject()
	{
		if (mReset) {
			mReset = false;
			UpdateMaterials();
		}

		if (mClippedMat != null) {
			mClippedMat.mainTextureOffset = new Vector2(-mClipRange.x / mClipRange.z, -mClipRange.y / mClipRange.w);
			mClippedMat.mainTextureScale = new Vector2(1f / mClipRange.z, 1f / mClipRange.w);

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (mClipSoft.x > 0f)
				sharpness.x = mClipRange.z / mClipSoft.x;
			if (mClipSoft.y > 0f)
				sharpness.y = mClipRange.w / mClipSoft.y;
			mClippedMat.SetVector("_ClipSharpness", sharpness);
		}
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy()
	{
		NGUITools.DestroyImmediate(mMesh);
		NGUITools.DestroyImmediate(mClippedMat);
		NGUITools.DestroyImmediate(mDepthMat);
	}
}