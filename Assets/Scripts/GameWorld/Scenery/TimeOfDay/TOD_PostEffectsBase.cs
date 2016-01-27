using UnityEngine;

/// Post effects base class.
///
/// Based on the post effects base from the default Unity image effects.

[ExecuteInEditMode]
//[RequireComponent(typeof(Camera))]
public abstract class TOD_PostEffectsBase : MonoBehaviour
{
    protected bool isSupported = true;

    protected abstract bool CheckResources();

    protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
    {
        if (!shader)
        {
            //Debug.Log("Missing shader in " + this.ToString());
            enabled = false;
            return null;
        }

        if (shader.isSupported && material && material.shader == shader)
        {
            return material;
        }

        if (!shader.isSupported)
        {
            NotSupported();
            Debug.LogError("The shader " + shader.ToString() + " on effect " + this.ToString() + " is not supported on this platform!");
            return null;
        }
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material ? material : null;
        }
    }

    protected Material CreateMaterial(Shader shader, Material material)
    {
        if (!shader)
        {
            //Debug.Log("Missing shader in " + this.ToString());
            return null;
        }

        if (material && material.shader == shader && shader.isSupported)
        {
            return material;
        }

        if (!shader.isSupported)
        {
            return null;
        }
        else
        {
            material = new Material (shader);
            material.hideFlags = HideFlags.DontSave;
            return material ? material : null;
        }
    }

    protected void OnEnable()
    {
        isSupported = true;
    }

    protected void Start()
    {
         CheckResources();
    }

    protected bool CheckSupport(bool needDepth)
    {
        isSupported = true;

        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            NotSupported();
            return false;
        }

        if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            NotSupported();
            return false;
        }

        if (needDepth)
        {
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }

        return true;
    }

    protected bool CheckSupport(bool needDepth, bool needHdr)
    {
        if (!CheckSupport(needDepth))
        {
            return false;
        }

        if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
        {
            NotSupported();
            return false;
        }

        return true;
    }

    protected void ReportAutoDisable()
    {
        Debug.LogWarning("The image effect " + this.ToString() + " has been disabled as it's not supported on the current platform.");
    }

    protected void NotSupported()
    {
        enabled = false;
        isSupported = false;
        return;
    }

    protected void DrawBorder(RenderTexture dest, Material material)
    {
        float x1;
        float x2;
        float y1;
        float y2;

        RenderTexture.active = dest;
        bool invertY = true; // source.texelSize.y < 0.0f;

        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();

        for (int i = 0; i < material.passCount; i++)
        {
            material.SetPass(i);

            float y1_;
            float y2_;
            if (invertY)
            {
                y1_ = 1.0f; y2_ = 0.0f;
            }
            else
            {
                y1_ = 0.0f; y2_ = 1.0f;
            }

            // Left
            x1 = 0.0f;
            x2 = 0.0f + 1.0f/(dest.width*1.0f);
            y1 = 0.0f;
            y2 = 1.0f;
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // Right
            x1 = 1.0f - 1.0f/(dest.width*1.0f);
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // Top
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 0.0f + 1.0f/(dest.height*1.0f);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // Bottom
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 1.0f - 1.0f/(dest.height*1.0f);
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            GL.End();
        }

        GL.PopMatrix();
    }
}
