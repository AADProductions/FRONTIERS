using UnityEngine;

public partial class TOD_Sky : MonoBehaviour
{
    /// Adjust shaders and meshes according to the quality settings.
    private void SetupQualitySettings()
    {
        TOD_Resources resources = Components.Resources;

        Material cloudMaterial  = null;
        Material shadowMaterial = null;

        switch (CloudQuality)
        {
            case TOD_Sky.CloudQualityType.Fastest:
                cloudMaterial  = resources.CloudMaterialFastest;
                shadowMaterial = resources.ShadowMaterialFastest;
                break;
            case TOD_Sky.CloudQualityType.Density:
                cloudMaterial  = resources.CloudMaterialDensity;
                shadowMaterial = resources.ShadowMaterialDensity;
                break;
            case TOD_Sky.CloudQualityType.Bumped:
                cloudMaterial  = resources.CloudMaterialBumped;
                shadowMaterial = resources.ShadowMaterialBumped;
                break;
            default:
                Debug.LogError("Unknown cloud quality.");
                break;
        }

        Mesh spaceMesh      = null;
        Mesh atmosphereMesh = null;
        Mesh clearMesh      = null;
        Mesh cloudMesh      = null;
        Mesh sunMesh        = null;
        Mesh moonMesh       = null;

        switch (MeshQuality)
        {
            case TOD_Sky.MeshQualityType.Low:
                spaceMesh      = resources.IcosphereLow;
                atmosphereMesh = resources.IcosphereLow;
                clearMesh      = resources.IcosphereLow;
                cloudMesh      = resources.HalfIcosphereLow;
                sunMesh        = resources.Quad;
                moonMesh       = resources.SphereLow;
                break;
            case TOD_Sky.MeshQualityType.Medium:
                spaceMesh      = resources.IcosphereMedium;
                atmosphereMesh = resources.IcosphereMedium;
                clearMesh      = resources.IcosphereLow;
                cloudMesh      = resources.HalfIcosphereMedium;
                sunMesh        = resources.Quad;
                moonMesh       = resources.SphereMedium;
                break;
            case TOD_Sky.MeshQualityType.High:
                spaceMesh      = resources.IcosphereHigh;
                atmosphereMesh = resources.IcosphereHigh;
                clearMesh      = resources.IcosphereLow;
                cloudMesh      = resources.HalfIcosphereHigh;
                sunMesh        = resources.Quad;
                moonMesh       = resources.SphereHigh;
                break;
            default:
                Debug.LogError("Unknown mesh quality.");
                break;
        }

		if (!mUpdatedComponents) {
			if (!Components.SpaceShader || Components.SpaceShader.name != resources.SpaceMaterial.name) {
				Components.SpaceShader = Components.SpaceRenderer.sharedMaterial = resources.SpaceMaterial;
			}

			if (!Components.AtmosphereShader || Components.AtmosphereShader.name != resources.AtmosphereMaterial.name) {
				Components.AtmosphereShader = Components.AtmosphereRenderer.sharedMaterial = resources.AtmosphereMaterial;
			}

			if (!Components.ClearShader || Components.ClearShader.name != resources.ClearMaterial.name) {
				Components.ClearShader = Components.ClearRenderer.sharedMaterial = resources.ClearMaterial;
			}

			if (!Components.CloudShader || Components.CloudShader.name != cloudMaterial.name) {
				Components.CloudShader = Components.CloudRenderer.sharedMaterial = cloudMaterial;
			}

			if (!Components.ShadowShader || Components.ShadowShader.name != shadowMaterial.name) {
				Components.ShadowShader = Components.ShadowProjector.material = shadowMaterial;
			}

			if (!Components.SunShader || Components.SunShader.name != resources.SunMaterial.name) {
				Components.SunShader = Components.SunRenderer.sharedMaterial = resources.SunMaterial;
			}

			if (!Components.MoonShader || Components.MoonShader.name != resources.MoonMaterial.name) {
				Components.MoonShader = Components.MoonRenderer.sharedMaterial = resources.MoonMaterial;
			}

			if (Components.SpaceMeshFilter.sharedMesh != spaceMesh) {
				Components.SpaceMeshFilter.mesh = spaceMesh;
			}

			if (Components.AtmosphereMeshFilter.sharedMesh != atmosphereMesh) {
				Components.AtmosphereMeshFilter.mesh = atmosphereMesh;
			}

			if (Components.ClearMeshFilter.sharedMesh != clearMesh) {
				Components.ClearMeshFilter.mesh = clearMesh;
			}

			if (Components.CloudMeshFilter.sharedMesh != cloudMesh) {
				Components.CloudMeshFilter.mesh = cloudMesh;
			}

			if (Components.SunMeshFilter.sharedMesh != sunMesh) {
				Components.SunMeshFilter.mesh = sunMesh;
			}

			if (Components.MoonMeshFilter.sharedMesh != moonMesh) {
				Components.MoonMeshFilter.mesh = moonMesh;
			}
			mUpdatedComponents = true;
		}
    }
	protected bool mUpdatedComponents = false;
}
