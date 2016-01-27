using UnityEngine;

/// Main sky dome management class.
///
/// Component of the sky dome parent game object.

[ExecuteInEditMode]
public partial class TOD_Sky : MonoBehaviour
{
	/// Convert spherical coordinates to cartesian coordinates.
    /// \param radius Spherical coordinates radius.
    /// \param theta Spherical coordinates theta.
    /// \param phi Spherical coordinates phi.
    /// \return Unity position in world space.
    internal Vector3 OrbitalToUnity(float radius, float theta, float phi)
    {
        Vector3 res;

        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);
        float sinPhi   = Mathf.Sin(phi);
        float cosPhi   = Mathf.Cos(phi);

        res.z = radius * sinTheta * cosPhi;
        res.y = radius * cosTheta;
        res.x = radius * sinTheta * sinPhi;

        return res;
    }

    /// Convert spherical coordinates to cartesian coordinates.
    /// \param theta Spherical coordinates theta.
    /// \param phi Spherical coordinates phi.
    /// \return Unity position in local space.
    internal Vector3 OrbitalToLocal(float theta, float phi)
    {
        Vector3 res;

        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);
        float sinPhi   = Mathf.Sin(phi);
        float cosPhi   = Mathf.Cos(phi);

        res.z = sinTheta * cosPhi;
        res.y = cosTheta;
        res.x = sinTheta * sinPhi;

        return res;
    }

    /// Sample atmosphere colors from the sky dome.
    /// \param direction View direction in world space.
    /// \return Color of the atmosphere in the specified direction.
    internal Color SampleAtmosphere(Vector3 direction, bool clampAlpha = true)
    {
        direction = Components.DomeTransform.InverseTransformDirection(direction);

        const float _Gamma        = 2.2f;
        const float _OneOverGamma = 1.0f / _Gamma;

        float _Height    = World.HorizonOffset;
		float _Contrast  = (SpaceMode ? 1.5f : Atmosphere.Contrast) * _OneOverGamma;
		float _Haziness  = (SpaceMode ? 0f : Atmosphere.Haziness);
		float _Fogginess = (SpaceMode ? 0f : Atmosphere.Fogginess);

        Color   TOD_SunColor      = SunColor;
        Color   TOD_MoonColor     = MoonColor;
        Color   TOD_MoonHaloColor = MoonHaloColor;
        Color   TOD_CloudColor    = CloudColor;
        Color   TOD_AdditiveColor = AdditiveColor;
        Vector3 TOD_SunDirection  = Components.DomeTransform.InverseTransformDirection(SunDirection);
        Vector3 TOD_MoonDirection = Components.DomeTransform.InverseTransformDirection(MoonDirection);

        Vector3 _OpticalDepth      = this.opticalDepth;
        Vector3 _OneOverBeta       = this.oneOverBeta;
        Vector3 _BetaRayleigh      = this.betaRayleigh;
        Vector3 _BetaRayleighTheta = this.betaRayleighTheta;
        Vector3 _BetaMie           = this.betaMie;
        Vector3 _BetaMieTheta      = this.betaMieTheta;
        Vector3 _BetaMiePhase      = this.betaMiePhase;
        Vector3 _BetaNight         = this.betaNight;

        Color color = Color.black;

        // Angle between sun and normal
        float cosTheta = Mathf.Max(0, Vector3.Dot(-direction, TOD_SunDirection));

        // Parameter value
        // See [7] page 70 equation (5.7)
        float h = Mathf.Clamp(direction.y + _Height, 0.001f, 1);
        float f = Mathf.Pow(h, _Haziness);

        // Optical depth integral approximation
        // See [7] page 71 equation (5.8)
        // See [7] page 71 equation (5.10)
        // See [7] page 76 equation (6.1)
        float sh = (1 - f) * 190000;
        float sr = sh + f * (_OpticalDepth.x - sh);
        float sm = sh + f * (_OpticalDepth.y - sh);

        // Angular dependency
        // See [3] page 2 equation (2) and (4)
        float angular = (1 + cosTheta*cosTheta);

        // Rayleigh and mie scattering factors
        // See [3] page 2 equation (1) and (2)
        // See [3] page 2 equation (3) and (4)
        Vector3 beta = _BetaRayleigh * sr
                     + _BetaMie * sm;
        Vector3 betaTheta = _BetaRayleighTheta
                          + _BetaMieTheta / Mathf.Pow(_BetaMiePhase.x - _BetaMiePhase.y * cosTheta, 1.5f);

        // Scattering solution
        // See [5] page 11
        float E_sun_r  = TOD_SunColor.r;
        float E_sun_g  = TOD_SunColor.g;
        float E_sun_b  = TOD_SunColor.b;
        float E_moon_r = TOD_MoonColor.r;
        float E_moon_g = TOD_MoonColor.g;
        float E_moon_b = TOD_MoonColor.b;
        float T_val_r  = Mathf.Exp(-beta.x);
        float T_val_g  = Mathf.Exp(-beta.y);
        float T_val_b  = Mathf.Exp(-beta.z);
        float L_sun_r  = angular * betaTheta.x * _OneOverBeta.x;
        float L_sun_g  = angular * betaTheta.y * _OneOverBeta.y;
        float L_sun_b  = angular * betaTheta.z * _OneOverBeta.z;
        float L_moon_r = _BetaNight.x;
        float L_moon_g = _BetaNight.y;
        float L_moon_b = _BetaNight.z;

        // Add scattering color
        color.r = (1-T_val_r) * (E_sun_r*L_sun_r + E_moon_r*L_moon_r);
        color.g = (1-T_val_g) * (E_sun_g*L_sun_g + E_moon_g*L_moon_g);
        color.b = (1-T_val_b) * (E_sun_b*L_sun_b + E_moon_b*L_moon_b);
        color.a = 10 * Max3(color.r, color.g, color.b);

        // Add simple moon halo
        color += TOD_MoonHaloColor * Mathf.Pow(Mathf.Max(0, Vector3.Dot(TOD_MoonDirection, -direction)), 10);

        // Add additive color
        color += TOD_AdditiveColor;

        // Add fog color
        color.r = Mathf.Lerp(color.r, TOD_CloudColor.r, _Fogginess);
        color.g = Mathf.Lerp(color.g, TOD_CloudColor.g, _Fogginess);
        color.b = Mathf.Lerp(color.b, TOD_CloudColor.b, _Fogginess);
        color.a += _Fogginess;

        // Clamp alpha by default
        if (clampAlpha) color.a = Mathf.Clamp01(color.a);

        // Adjust output color according to gamma value
        color = PowRGBA(color, _Contrast);

        return color;
    }

    /// Setup rayleigh and mie scattering by precalculating as much of it as possible.
    // See [2] page 2
    // See [3] page 2
    private void SetupScattering()
    {
        // Scale
        const float ray_scale_const = 1.0f;
        const float ray_scale_theta = 20.0f;
        const float mie_scale_const = 0.1f;
        const float mie_scale_theta = 2.0f;

        // Rayleigh
        {
			if (SpaceMode) {
				// Artistic color multiplier
				float mult_r = 0.001f + 10 * 0.05f;
				float mult_g = 0.001f + 10 * 0.05f;
				float mult_b = 0.001f + 10 * 0.05f;

				// Scattering coefficient
				const float beta_r = 5.8e-6f;
				const float beta_g = 13.5e-6f;
				const float beta_b = 33.1e-6f;

				// Phase function
				const float phase = (3) / (16 * pi);

				// Shader paramters
				betaRayleigh.x = ray_scale_const * beta_r * mult_r;
				betaRayleigh.y = ray_scale_const * beta_g * mult_g;
				betaRayleigh.z = ray_scale_const * beta_b * mult_b;
				betaRayleighTheta.x = ray_scale_theta * beta_r * mult_r * phase;
				betaRayleighTheta.y = ray_scale_theta * beta_g * mult_g * phase;
				betaRayleighTheta.z = ray_scale_theta * beta_b * mult_b * phase;
				opticalDepth.x = 8000 * Mathf.Exp (-World.ViewerHeight * 50000 / 8000);
			} else {
				// Artistic color multiplier
				float mult_r = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.r;
				float mult_g = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.g;
				float mult_b = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.b;

				// Scattering coefficient
				const float beta_r = 5.8e-6f;
				const float beta_g = 13.5e-6f;
				const float beta_b = 33.1e-6f;

				// Phase function
				const float phase = (3) / (16 * pi);

				// Shader paramters
				betaRayleigh.x = ray_scale_const * beta_r * mult_r;
				betaRayleigh.y = ray_scale_const * beta_g * mult_g;
				betaRayleigh.z = ray_scale_const * beta_b * mult_b;
				betaRayleighTheta.x = ray_scale_theta * beta_r * mult_r * phase;
				betaRayleighTheta.y = ray_scale_theta * beta_g * mult_g * phase;
				betaRayleighTheta.z = ray_scale_theta * beta_b * mult_b * phase;
				opticalDepth.x = 8000 * Mathf.Exp (-World.ViewerHeight * 50000 / 8000);
			}
        }

        // Mie
        {
			if (SpaceMode) {
				// Artistic color multiplier
				float mult_r = 0.001f + 5 * 0.05f;
				float mult_g = 0.001f + 5 * 0.05f;
				float mult_b = 0.001f + 5 * 0.05f;

				// Scattering coefficient
				const float beta = 2e-5f;

				// Phase function
				float g = 0.91f;
				float phase = (3) / (4 * pi) * (1 - g * g) / (2 + g * g);

				// Shader paramters
				betaMie.x = mie_scale_const * beta * mult_r;
				betaMie.y = mie_scale_const * beta * mult_g;
				betaMie.z = mie_scale_const * beta * mult_b;
				betaMieTheta.x = mie_scale_theta * beta * mult_r * phase;
				betaMieTheta.y = mie_scale_theta * beta * mult_g * phase;
				betaMieTheta.z = mie_scale_theta * beta * mult_b * phase;
				betaMiePhase.x = 1 + g * g;
				betaMiePhase.y = 2 * g;
				opticalDepth.y = 1200 * Mathf.Exp (-World.ViewerHeight * 50000 / 1200);
			} else {
				// Artistic color multiplier
				float mult_r = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.r;
				float mult_g = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.g;
				float mult_b = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.b;

				// Scattering coefficient
				const float beta = 2e-5f;

				// Phase function
				float g = Atmosphere.Directionality;
				float phase = (3) / (4 * pi) * (1 - g * g) / (2 + g * g);

				// Shader paramters
				betaMie.x = mie_scale_const * beta * mult_r;
				betaMie.y = mie_scale_const * beta * mult_g;
				betaMie.z = mie_scale_const * beta * mult_b;
				betaMieTheta.x = mie_scale_theta * beta * mult_r * phase;
				betaMieTheta.y = mie_scale_theta * beta * mult_g * phase;
				betaMieTheta.z = mie_scale_theta * beta * mult_b * phase;
				betaMiePhase.x = 1 + g * g;
				betaMiePhase.y = 2 * g;
				opticalDepth.y = 1200 * Mathf.Exp (-World.ViewerHeight * 50000 / 1200);
			}
        }

        oneOverBeta = Inverse(betaMie + betaRayleigh);
        betaNight   = Vector3.Scale(betaRayleighTheta + betaMieTheta / Mathf.Pow(betaMiePhase.x, 1.5f), oneOverBeta);
    }

    /// Calculate sun and moon position.
    private void SetupSunAndMoon()
    {
        // Local latitude
        float lat_rad = Mathf.Deg2Rad * Cycle.Latitude;
        float lat_sin = Mathf.Sin(lat_rad);
        float lat_cos = Mathf.Cos(lat_rad);

        // Local longitude
        float lon_deg = Cycle.Longitude;

        // Local time
        float d = 367*Cycle.Year - 7*(Cycle.Year + (Cycle.Month+9)/12)/4 + 275*Cycle.Month/9 + Cycle.Day - 730530;
        float t = Cycle.Hour - Cycle.UTC;
        float ecl = 23.4393f - 3.563E-7f * d;
        float ecl_rad = Mathf.Deg2Rad * ecl;
        float ecl_sin = Mathf.Sin(ecl_rad);
        float ecl_cos = Mathf.Cos(ecl_rad);

        // Sun
        float altitude, azimuth, theta, phi;
        {
            // See http://www.stjarnhimlen.se/comp/ppcomp.html#4

            float w = 282.9404f + 4.70935E-5f * d;
            float e = 0.016709f - 1.151E-9f * d;
            float M = 356.0470f + 0.9856002585f * d;

            float M_rad = Mathf.Deg2Rad * M;
            float M_sin = Mathf.Sin(M_rad);
            float M_cos = Mathf.Cos(M_rad);

            // See http://www.stjarnhimlen.se/comp/ppcomp.html#5

            float E_deg = M + e * Mathf.Rad2Deg * M_sin * (1 + e * M_cos);
            float E_rad = Mathf.Deg2Rad * E_deg;
            float E_sin = Mathf.Sin(E_rad);
            float E_cos = Mathf.Cos(E_rad);

            float xv = E_cos - e;
            float yv = E_sin * Mathf.Sqrt(1 - e*e);

            float v = Mathf.Rad2Deg * Mathf.Atan2(yv, xv);
            float r = Mathf.Sqrt(xv*xv + yv*yv);

            float l = v + w;
            float l_rad = Mathf.Deg2Rad * l;
            float l_sin = Mathf.Sin(l_rad);
            float l_cos = Mathf.Cos(l_rad);

            float xs = r * l_cos;
            float ys = r * l_sin;

            float xe = xs;
            float ye = ys * ecl_cos;
            float ze = ys * ecl_sin;

            float rasc_rad = Mathf.Atan2(ye, xe);
            float rasc_deg = Mathf.Rad2Deg * rasc_rad;
            float decl_rad = Mathf.Atan2(ze, Mathf.Sqrt(xe*xe + ye*ye));
            float decl_sin = Mathf.Sin(decl_rad);
            float decl_cos = Mathf.Cos(decl_rad);

            // See http://www.stjarnhimlen.se/comp/ppcomp.html#5b

            float GMST0_deg = v + w + 180;
            float GMST_deg  = GMST0_deg + t*15;
            float LST_deg   = GMST_deg + lon_deg;

            // See http://www.stjarnhimlen.se/comp/ppcomp.html#12b

            float HA_deg = LST_deg - rasc_deg;
            float HA_rad = Mathf.Deg2Rad * HA_deg;
            float HA_sin = Mathf.Sin(HA_rad);
            float HA_cos = Mathf.Cos(HA_rad);

            float x = HA_cos * decl_cos;
            float y = HA_sin * decl_cos;
            float z = decl_sin;

            float xhor = x * lat_sin - z * lat_cos;
            float yhor = y;
            float zhor = x * lat_cos + z * lat_sin;

            azimuth  = Mathf.Atan2(yhor, xhor) + Mathf.Deg2Rad * 180;
            altitude = Mathf.Atan2(zhor, Mathf.Sqrt(xhor*xhor + yhor*yhor));

            theta = pi/2 - altitude;
            phi   = azimuth;
        }

        // Update sun position
        var sunPos = OrbitalToLocal(theta, phi);
        #if UNITY_EDITOR
        if (Components.SunTransform.localPosition != sunPos)
        #endif
        {
            Components.SunTransform.localPosition = sunPos;
            Components.SunTransform.LookAt(Components.DomeTransform.position, Components.SunTransform.up);

            // Add camera-based sun animation rotation
            if (Components.CameraTransform != null)
            {
                Vector3 camRot = Components.CameraTransform.rotation.eulerAngles;
                Vector3 sunRot = Components.SunTransform.localEulerAngles;
				sunRot.z = 2 * (float) (Frontiers.WorldClock.AdjustedRealTime + Mathf.Abs(camRot.x) + Mathf.Abs(camRot.y) + Mathf.Abs(camRot.z));
                Components.SunTransform.localEulerAngles = sunRot;
            }
        }

        // Update moon position
        var moonPos = OrbitalToLocal(theta+pi, phi);
        #if UNITY_EDITOR
        if (Components.MoonTransform.localPosition != moonPos)
        #endif
        {
            Components.MoonTransform.localPosition = moonPos;
            Components.MoonTransform.LookAt(Components.DomeTransform.position, Components.MoonTransform.up);
        }

        // Setup sun size - additional factor of two because it is a quad
        float sun_r = 4 * Mathf.Tan(Mathf.Deg2Rad / 2 * Day.SunMeshSize);
        float sun_d = 2 * sun_r;
        var sunScale = new Vector3(sun_d, sun_d, sun_d);
        #if UNITY_EDITOR
        if (Components.SunTransform.localScale != sunScale)
        #endif
        {
            Components.SunTransform.localScale = sunScale;
        }

        // Setup moon size
        float moon_r = 2 * Mathf.Tan(Mathf.Deg2Rad / 2 * Night.MoonMeshSize);
        float moon_d = 2 * moon_r;
        var moonScale = new Vector3(moon_d, moon_d, moon_d);
        #if UNITY_EDITOR
        if (Components.MoonTransform.localScale != moonScale)
        #endif
        {
            Components.MoonTransform.localScale = moonScale;
        }

        // Update properties
        SunZenith  = Mathf.Rad2Deg * theta;
        MoonZenith = Mathf.PingPong(SunZenith + 180, 180);

        // Update renderer states
        var sunEnabled   = (Components.SunTransform.localPosition.y  > -0.5f);
        var moonEnabled  = (Components.MoonTransform.localPosition.y > -0.1f);
        var spaceEnabled = (SampleAtmosphere(Vector3.up, false).a < 1.1f);
		var cloudEnabled = (Clouds.Density > 0) && !SpaceMode;
        #if UNITY_EDITOR
        if (Components.SunRenderer.enabled != sunEnabled)
        #endif
        {
            Components.SunRenderer.enabled = sunEnabled;
        }
        #if UNITY_EDITOR
        if (Components.MoonRenderer.enabled != moonEnabled)
        #endif
        {
            Components.MoonRenderer.enabled  = moonEnabled;
        }
        #if UNITY_EDITOR
        if (Components.SpaceRenderer.enabled != spaceEnabled)
        #endif
        {
            Components.SpaceRenderer.enabled = spaceEnabled;
        }
        #if UNITY_EDITOR
        if (Components.CloudRenderer.enabled != cloudEnabled)
        #endif
        {
            Components.CloudRenderer.enabled = cloudEnabled;
        }

        // Update light source
        SetupLightSource(theta, phi);
    }

    /// Update light source color, intensity and position.
    private void SetupLightSource(float theta, float phi)
    {
        // Relative optical mass (air mass coefficient approximated by a spherical shell)
        // See http://en.wikipedia.org/wiki/Air_mass_(solar_energy)
        float c = Mathf.Cos(Mathf.Pow(theta/(2*pi), 2f - Light.Falloff) * 2*pi);
        float m = Mathf.Sqrt(708f*708f*c*c + 2*708f + 1) - 708f*c;

        //float m = 1 / (Mathf.Cos(theta) + 0.15f * Mathf.Pow(93.885f - Mathf.Rad2Deg * theta, -1.253f));

        // Wavelengths in micrometers
        // See [3] page 2
        const float lambda_r = 680.0e-3f; // [um]
        const float lambda_g = 550.0e-3f; // [um]
        const float lambda_b = 440.0e-3f; // [um]

        // Transmitted sun color
        float r = Day.SunLightColor.r;
        float g = Day.SunLightColor.g;
        float b = Day.SunLightColor.b;
		float a = Components.LightSource.intensity / Mathf.Max (Day.SunLightIntensity * Day.LightIntensityMultiplier, Night.MoonLightIntensity);

        // Transmittance due to Rayleigh scattering of air molecules
        // See [1] page 21
        const float rayleigh_beta  = 0.008735f;
        const float rayleigh_alpha = 4.08f;
        r *= Mathf.Exp(-rayleigh_beta * Mathf.Pow(lambda_r, -rayleigh_alpha * m));
        g *= Mathf.Exp(-rayleigh_beta * Mathf.Pow(lambda_g, -rayleigh_alpha * m));
        b *= Mathf.Exp(-rayleigh_beta * Mathf.Pow(lambda_b, -rayleigh_alpha * m));

        // Angstrom's turbididty formula for aerosal (does not improve anything visually)
        // See [1] page 21
        //const float aerosol_turbidity = 1.0f;
        //const float aerosal_beta = 0.04608f * aerosol_turbidity - 0.04586f;
        //const float aerosal_alpha = 1.3f;
        //r *= Mathf.Exp(-aerosal_beta * Mathf.Pow(lambda_r, -aerosal_alpha * m));
        //g *= Mathf.Exp(-aerosal_beta * Mathf.Pow(lambda_g, -aerosal_alpha * m));
        //b *= Mathf.Exp(-aerosal_beta * Mathf.Pow(lambda_b, -aerosal_alpha * m));

        // Transmittance due to ozone absorption (does not improve anything visually)
        // See [1] page 21
        //const float ozone_l  = 0.350f; // [cm]
        //const float ozone_kr = 0.067f; // [1/cm]
        //const float ozone_kg = 0.040f; // [1/cm]
        //const float ozone_kb = 0.009f; // [1/cm]
        //r *= Mathf.Exp(-ozone_kr * Mathf.Pow(lambda_r, -ozone_l * m));
        //g *= Mathf.Exp(-ozone_kg * Mathf.Pow(lambda_g, -ozone_l * m));
        //b *= Mathf.Exp(-ozone_kb * Mathf.Pow(lambda_b, -ozone_l * m));

        // Update lerp value
        LerpValue = Mathf.Clamp01(1.1f * Max3(r, g, b));

        // Light components
        float light_moon_r  = Night.MoonLightColor.r;
        float light_moon_g  = Night.MoonLightColor.g;
        float light_moon_b  = Night.MoonLightColor.b;
        float light_sun_r   = Day.SunLightColor.r * Mathf.Lerp(1, r, Light.Coloring);
        float light_sun_g   = Day.SunLightColor.g * Mathf.Lerp(1, g, Light.Coloring);
        float light_sun_b   = Day.SunLightColor.b * Mathf.Lerp(1, b, Light.Coloring);
        float light_shaft_r = Day.SunShaftColor.r * Mathf.Lerp(1, r, Light.ShaftColoring);
        float light_shaft_g = Day.SunShaftColor.g * Mathf.Lerp(1, g, Light.ShaftColoring);
        float light_shaft_b = Day.SunShaftColor.b * Mathf.Lerp(1, b, Light.ShaftColoring);

        // Some precalculations
        Color light_moon = new Color(light_moon_r, light_moon_g, light_moon_b, a);
        Color light_sun  = new Color(light_sun_r,  light_sun_g,  light_sun_b,  a);
        Color light_lerp = Color.Lerp(light_moon, light_sun, Max3(light_sun.r, light_sun.g, light_sun.b));
		//NEW
		//add a 'base' value for times when the sun and moon brightness are both very dark (eg, dawn & dusk)
		if (!SpaceMode) {
			light_lerp = Frontiers.Colors.Min (light_lerp, Atmosphere.DuskAmbientColor);
		}

        // Update sun shaft color
        SunShaftColor = new Color(light_shaft_r,  light_shaft_g,  light_shaft_b, a);

        // Update ambient color
		float ambMult = Mathf.Lerp(Night.AmbientIntensity, Day.AmbientIntensity, LerpValue);
        AmbientColor = new Color(light_lerp.r * ambMult, light_lerp.g * ambMult, light_lerp.b * ambMult, 1);
		AmbientColor = Color.Lerp (Color.black, AmbientColor, Atmosphere.AmbientIntensityMultiplier);

		if (SpaceMode) {
			SunColor = 0.25f * Day.SkyMultiplier * Day.SunLightColor;
		} else {
			// Update sun color
			SunColor = Atmosphere.Brightness
			* Day.SkyMultiplier
			* Mathf.Lerp (1.0f, 0.1f, Mathf.Sqrt (SunZenith / 90) - 0.25f)
			* Color.Lerp (Day.SunLightColor * LerpValue, new Color (r, g, b, a), Light.SkyColoring);
			SunColor = new Color (SunColor.r, SunColor.g, SunColor.b, LerpValue);
		}

        // Update moon color
        MoonColor = (1-LerpValue) * 0.5f
                  * Atmosphere.Brightness
                  * Night.SkyMultiplier
                  * Night.MoonLightColor;
        MoonColor = new Color(MoonColor.r, MoonColor.g, MoonColor.b, 1-LerpValue);

        // Update moon halo color
        Color haloColor = (1-LerpValue) * (1-Mathf.Abs(Cycle.MoonPhase))
                        * Atmosphere.Brightness
                        * Night.MoonHaloColor;
        haloColor.r *= haloColor.a;
        haloColor.g *= haloColor.a;
        haloColor.b *= haloColor.a;
        haloColor.a = Max3(haloColor.r, haloColor.g, haloColor.b);
        MoonHaloColor = haloColor;

        // Update cloud color
        Color cloudColor = Color.Lerp(MoonColor, SunColor, LerpValue);
        float cloud_a = Mathf.Lerp(Night.CloudMultiplier, Day.CloudMultiplier, LerpValue);
        float cloud_avg = (cloudColor.r + cloudColor.g + cloudColor.b) / 3;
        CloudColor = cloud_a * 1.25f
                   * Clouds.Brightness
                   * Color.Lerp(new Color(cloud_avg, cloud_avg, cloud_avg), cloudColor, Light.CloudColoring);
        CloudColor = new Color(CloudColor.r, CloudColor.g, CloudColor.b, cloud_a);

		if (!SpaceMode) {
			//NEW - desaturate sun and moon colors
			SunColor = Color.Lerp (SunColor, CloudColor, Clouds.Density);
			MoonColor = Color.Lerp (MoonColor, CloudColor, Clouds.Density);
		}

        // Update additive color
        Color additiveColor = Color.Lerp(Night.AdditiveColor, Day.AdditiveColor, LerpValue);
        additiveColor.r *= additiveColor.a;
        additiveColor.g *= additiveColor.a;
        additiveColor.b *= additiveColor.a;
        additiveColor.a = Max3(additiveColor.r, additiveColor.g, additiveColor.b);
        AdditiveColor = additiveColor;

        // Intensity
        const float threshold = 0.2f;

        float intensity, shadowStrength;
        Vector3 position;

		if (SpaceMode) {
			intensity = Day.SunLightIntensity;
			position = OrbitalToLocal (theta, phi);
			shadowStrength = Mathf.Clamp01 (Day.ShadowStrength + Day.ShadowStrengthBooster);
			Components.LightSource.color = light_sun;
		} else {
			if (LerpValue > threshold) {
				float lerp = (LerpValue - threshold) / (1 - threshold);

				intensity = Mathf.Lerp (0, Day.SunLightIntensity * Day.LightIntensityMultiplier, lerp);
				shadowStrength = Day.ShadowStrength + Day.ShadowStrengthBooster;
				position = OrbitalToLocal (Mathf.Min (theta, (1 - Light.MinimumHeight) * pi / 2), phi);

				#if UNITY_EDITOR
				if (Components.LightSource.color != light_sun)
            #endif
            {
					Components.LightSource.color = light_sun;
				}
			} else {
				float lerp = (threshold - LerpValue) / threshold;
				float phase = 1 - Mathf.Abs (Cycle.MoonPhase);

				intensity = Mathf.Lerp (0, Night.MoonLightIntensity, lerp);//Mathf.Lerp(0, Night.MoonLightIntensity*phase, lerp);
				shadowStrength = Night.ShadowStrength + Day.ShadowStrengthBooster;
				position = OrbitalToLocal (Mathf.Max (theta + pi, (1 + Light.MinimumHeight) * pi / 2 + pi), phi);

				#if UNITY_EDITOR
				if (Components.LightSource.color != light_moon)
            #endif
            {
					Components.LightSource.color = light_moon;
				}
			}
		}

        #if UNITY_EDITOR
        if (Components.LightSource.intensity != intensity)
        #endif
        {
            Components.LightSource.intensity = intensity;
        }
        #if UNITY_EDITOR
        if (Components.LightSource.shadowStrength != shadowStrength)
        #endif
        {
            Components.LightSource.shadowStrength = shadowStrength;
        }
        #if UNITY_EDITOR
        if (Components.LightTransform.localPosition != position)
        #endif
        {
            Components.LightTransform.localPosition = position;
			Components.LightTransform.LookAt (Components.DomeTransform.position);
			Components.Light.transform.rotation = Components.LightTransform.rotation;
        }

		var shadows = (Components.LightSource.shadowStrength == 0)
		                                      ? LightShadows.None
				              : Frontiers.PlayerPreferences.DefaultShadowSetting;

        #if UNITY_EDITOR
        if (Components.LightSource.shadows != shadows)
        #endif
        {
            Components.LightSource.shadows = shadows;
        }
    }

    /// Calculate the fog color.
    private Color SampleFogColor()
    {
        Vector3 camera = Components.CameraTransform != null ? Components.CameraTransform.forward : Vector3.forward;
        Vector3 sample = Vector3.Lerp(new Vector3(camera.x, 0, camera.z), Vector3.up, World.FogColorBias);
        Color color = SampleAtmosphere(sample);
        return new Color(color.a * color.r, color.a * color.g, color.a * color.b, 1);
    }

	private Color SampleLocalFogColor()
	{
//		if (Frontiers.Player.Local != null) {
//			return SampleAtmosphere (Frontiers.Player.Local.ForwardVector);
//		}
		return SampleAtmosphere (Vector3.up);//Vector3.zero);
		//return new Color(color.a * color.r, color.a * color.g, color.a * color.b, 1);
	}

    /// Power of the RGB components of a color.
    private Color PowRGB(Color c, float p)
    {
        return new Color(Mathf.Pow(c.r, p), Mathf.Pow(c.g, p), Mathf.Pow(c.b, p), c.a);
    }

    /// Power of the RGBA components of a color.
    private Color PowRGBA(Color c, float p)
    {
        return new Color(Mathf.Pow(c.r, p), Mathf.Pow(c.g, p), Mathf.Pow(c.b, p), Mathf.Pow(c.a, p));
    }

    /// Max of three numbers.
    private float Max3(float a, float b, float c)
    {
        return (a >= b && a >= c) ? a : (b >= c) ? b : c;
    }

    /// Inverse of a vector.
    private Vector3 Inverse(Vector3 v)
    {
        return new Vector3(1f/v.x, 1f/v.y, 1f/v.z);
    }
}
