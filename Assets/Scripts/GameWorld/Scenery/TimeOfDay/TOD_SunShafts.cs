using UnityEngine;

/// Sun shaft class.
///
/// Component of the main camera of the scene to render sun shafts.
/// Based on the sun shafts from the default Unity image effects.

[ExecuteInEditMode]
//[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Sun Shafts")]
class TOD_SunShafts : TOD_PostEffectsBase
{
    private const int PASS_DEPTH   = 2;
    private const int PASS_NODEPTH = 3;
    private const int PASS_RADIAL  = 1;
    private const int PASS_SCREEN  = 0;
    private const int PASS_ADD     = 4;

    /// Available resolutions for the sun shafts.
    /// High is full, Normal is half and Low is quarter the screen resolution
    public enum SunShaftsResolution
    {
        Low,
        Normal,
        High,
    }

    /// Available methods to blend the sun shafts with the image.
    public enum SunShaftsBlendMode
    {
        Screen,
        Add,
    }

    /// Sky dome reference inspector variable.
    /// Has to be manually set to the sky dome instance.
    public TOD_Sky sky = null;

    /// Inspector variable to define the sun shaft rendering resolution.
    public SunShaftsResolution Resolution = SunShaftsResolution.Normal;

    /// Inspector variable to define the sun shaft rendering blend mode.
    public SunShaftsBlendMode BlendMode = SunShaftsBlendMode.Screen;

    /// Inspector variable to define the number of blur iterations to be performaed.
    public int RadialBlurIterations = 2;

    /// Inspector variable to define the radius to blur filter applied to the sun shafts.
    public float SunShaftBlurRadius = 2;

    /// Inspector variable to define the intensity of the sun shafts.
    public float SunShaftIntensity = 1;

    /// Inspector variable to define the maximum radius of the sun shafts.
    public float MaxRadius = 1;

    /// Inspector variable to define whether or not to use the depth buffer.
    /// If enabled, requires the target platform to allow the camera to create a depth texture.
    /// Unity always creates this depth texture if deferred lighting is enabled.
    /// Otherwise this script will enable it for the camera it is attached to.
    /// If disabled, requires all shaders writing to the depth buffer to also write to the frame buffer alpha channel.
    /// Only the frame buffer alpha channel will then be used to check for shaft blockers in the image effect.
    /// This is being done correctly in the built-in Unity opaque shaders as they always write 1 to the alpha channel.
    /// However, this is not being done in most of the built-in Unity cutout transparent and terrain shaders.
    public bool UseDepthTexture = true;

    /// Inspector variable pointing to the sun shaft rendering shader.
    public Shader SunShaftsShader = null;

    /// Inspector variable pointing to the clear rendering shader.
    public Shader ScreenClearShader = null;

    private Material sunShaftsMaterial   = null;
    private Material screenClearMaterial = null;

    #if UNITY_EDITOR
    protected void Update()
    {
        RadialBlurIterations = Mathf.Clamp(RadialBlurIterations, 1, 4);
        SunShaftBlurRadius   = Mathf.Max(SunShaftBlurRadius, 0);
        SunShaftIntensity    = Mathf.Max(SunShaftIntensity, 0);
        MaxRadius            = Mathf.Max(MaxRadius, 0);
    }
    #endif

    protected void OnDisable()
    {
        if (sunShaftsMaterial)
        {
            DestroyImmediate(sunShaftsMaterial);
        }
        if (screenClearMaterial)
        {
            DestroyImmediate(screenClearMaterial);
        }
    }

    protected override bool CheckResources()
    {
        CheckSupport(UseDepthTexture);

        sunShaftsMaterial = CheckShaderAndCreateMaterial(SunShaftsShader, sunShaftsMaterial);
        screenClearMaterial = CheckShaderAndCreateMaterial(ScreenClearShader, screenClearMaterial);

        if (!isSupported) ReportAutoDisable();
        return isSupported;
    }

    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false || !sky)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Let the sky dome know we are here
        sky.Components.SunShafts = this;

        // Enable depth texture on camera
        // This actually has to be checked every frame
        if (UseDepthTexture)
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        // Selected resolution
        int width, height;
        if (Resolution == SunShaftsResolution.High)
        {
            width = source.width;
            height = source.height;
        }
        else if (Resolution == SunShaftsResolution.Normal)
        {
            width = source.width / 2;
            height = source.height / 2;
        }
        else
        {
            width = source.width / 4;
            height = source.height / 4;
        }

        // Sun position
        Vector3 v = GetComponent<Camera>().WorldToViewportPoint(sky.Components.SunTransform.position);

        sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * SunShaftBlurRadius);
        sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, MaxRadius));

        RenderTexture buffer1 = RenderTexture.GetTemporary(width, height, 0);
        RenderTexture buffer2 = RenderTexture.GetTemporary(width, height, 0);

        // Create blocker mask
        if (UseDepthTexture)
        {
            Graphics.Blit(source, buffer1, sunShaftsMaterial, PASS_DEPTH);
        }
        else
        {
            Graphics.Blit(source, buffer1, sunShaftsMaterial, PASS_NODEPTH);
        }

        // Paint a small black small border to get rid of clamping problems
        DrawBorder(buffer1, screenClearMaterial);

        // Radial blur
        {
            float ofs = SunShaftBlurRadius * (1.0f / 768.0f);

            sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4 (ofs, ofs, 0.0f, 0.0f));
            sunShaftsMaterial.SetVector("_SunPosition", new Vector4 (v.x, v.y, v.z, MaxRadius));

            for (int it2 = 0; it2 < RadialBlurIterations; it2++ )
            {
                // Each iteration takes 2 * 6 samples
                // We update _BlurRadius each time to cheaply get a very smooth look
                Graphics.Blit(buffer1, buffer2, sunShaftsMaterial, PASS_RADIAL);
                ofs = SunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4 (ofs, ofs, 0.0f, 0.0f) );

                Graphics.Blit(buffer2, buffer1, sunShaftsMaterial, PASS_RADIAL);
                ofs = SunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4 (ofs, ofs, 0.0f, 0.0f) );
            }
        }

        // Blend together
        {
            Vector4 color = (v.z >= 0.0)
                          ? (1-sky.Atmosphere.Fogginess) * SunShaftIntensity * (Vector4)sky.SunShaftColor
                          : Vector4.zero; // No back projection!

            sunShaftsMaterial.SetVector("_SunColor", color);
            sunShaftsMaterial.SetTexture("_ColorBuffer", buffer1);

            if (BlendMode == SunShaftsBlendMode.Screen)
            {
                Graphics.Blit(source, destination, sunShaftsMaterial, PASS_SCREEN);
            }
            else
            {
                Graphics.Blit(source, destination, sunShaftsMaterial, PASS_ADD);
            }

            RenderTexture.ReleaseTemporary(buffer1);
            RenderTexture.ReleaseTemporary(buffer2);
        }
    }
}
