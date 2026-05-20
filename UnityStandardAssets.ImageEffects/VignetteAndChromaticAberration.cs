using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Camera/Vignette and Chromatic Aberration")]
	public class VignetteAndChromaticAberration : PostEffectsBase
	{
		public enum AberrationMode
		{
			Simple,
			Advanced
		}

		public AberrationMode mode;

		public float intensity = 0.036f;

		public float chromaticAberration = 0.2f;

		public float axialAberration = 0.5f;

		public float blur;

		public float blurSpread = 0.75f;

		public float luminanceDependency = 0.25f;

		public float blurDistance = 2.5f;

		public Shader vignetteShader;

		public Shader separableBlurShader;

		public Shader chromAberrationShader;

		private Material m_VignetteMaterial;

		private Material m_SeparableBlurMaterial;

		private Material m_ChromAberrationMaterial;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: false);
			m_VignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, m_VignetteMaterial);
			m_SeparableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, m_SeparableBlurMaterial);
			m_ChromAberrationMaterial = CheckShaderAndCreateMaterial(chromAberrationShader, m_ChromAberrationMaterial);
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!CheckResources())
			{
				Graphics.Blit(source, destination);
				return;
			}
			int width = source.width;
			int height = source.height;
			bool flag = Mathf.Abs(blur) > 0f || Mathf.Abs(intensity) > 0f;
			float num = 1f * (float)width / (1f * (float)height);
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			if (flag)
			{
				renderTexture = RenderTexture.GetTemporary(width, height, 0, source.format);
				if (Mathf.Abs(blur) > 0f)
				{
					renderTexture2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);
					Graphics.Blit(source, renderTexture2, m_ChromAberrationMaterial, 0);
					for (int i = 0; i < 2; i++)
					{
						m_SeparableBlurMaterial.SetVector("offsets", new Vector4(0f, blurSpread * 0.001953125f, 0f, 0f));
						RenderTexture temporary = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);
						Graphics.Blit(renderTexture2, temporary, m_SeparableBlurMaterial);
						RenderTexture.ReleaseTemporary(renderTexture2);
						m_SeparableBlurMaterial.SetVector("offsets", new Vector4(blurSpread * 0.001953125f / num, 0f, 0f, 0f));
						renderTexture2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);
						Graphics.Blit(temporary, renderTexture2, m_SeparableBlurMaterial);
						RenderTexture.ReleaseTemporary(temporary);
					}
				}
				m_VignetteMaterial.SetFloat("_Intensity", 1f / (1f - intensity) - 1f);
				m_VignetteMaterial.SetFloat("_Blur", 1f / (1f - blur) - 1f);
				m_VignetteMaterial.SetTexture("_VignetteTex", renderTexture2);
				Graphics.Blit(source, renderTexture, m_VignetteMaterial, 0);
			}
			m_ChromAberrationMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
			m_ChromAberrationMaterial.SetFloat("_AxialAberration", axialAberration);
			m_ChromAberrationMaterial.SetVector("_BlurDistance", new Vector2(0f - blurDistance, blurDistance));
			m_ChromAberrationMaterial.SetFloat("_Luminance", 1f / Mathf.Max(Mathf.Epsilon, luminanceDependency));
			if (flag)
			{
				renderTexture.wrapMode = TextureWrapMode.Clamp;
			}
			else
			{
				source.wrapMode = TextureWrapMode.Clamp;
			}
			Graphics.Blit(flag ? renderTexture : source, destination, m_ChromAberrationMaterial, (mode != AberrationMode.Advanced) ? 1 : 2);
			RenderTexture.ReleaseTemporary(renderTexture);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
	}
}
