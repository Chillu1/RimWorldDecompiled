using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Color Adjustments/Tonemapping")]
	public class Tonemapping : PostEffectsBase
	{
		public enum TonemapperType
		{
			SimpleReinhard,
			UserCurve,
			Hable,
			Photographic,
			OptimizedHejiDawson,
			AdaptiveReinhard,
			AdaptiveReinhardAutoWhite
		}

		public enum AdaptiveTexSize
		{
			Square16 = 0x10,
			Square32 = 0x20,
			Square64 = 0x40,
			Square128 = 0x80,
			Square256 = 0x100,
			Square512 = 0x200,
			Square1024 = 0x400
		}

		public TonemapperType type = TonemapperType.Photographic;

		public AdaptiveTexSize adaptiveTextureSize = AdaptiveTexSize.Square256;

		public AnimationCurve remapCurve;

		private Texture2D curveTex;

		public float exposureAdjustment = 1.5f;

		public float middleGrey = 0.4f;

		public float white = 2f;

		public float adaptionSpeed = 1.5f;

		public Shader tonemapper;

		public bool validRenderTextureFormat = true;

		private Material tonemapMaterial;

		private RenderTexture rt;

		private RenderTextureFormat rtFormat = RenderTextureFormat.ARGBHalf;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: false, needHdr: true);
			tonemapMaterial = CheckShaderAndCreateMaterial(tonemapper, tonemapMaterial);
			if (!curveTex && type == TonemapperType.UserCurve)
			{
				curveTex = new Texture2D(256, 1, TextureFormat.ARGB32, mipChain: false, linear: true);
				curveTex.filterMode = FilterMode.Bilinear;
				curveTex.wrapMode = TextureWrapMode.Clamp;
				curveTex.hideFlags = HideFlags.DontSave;
			}
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		public float UpdateCurve()
		{
			float num = 1f;
			if (remapCurve.keys.Length < 1)
			{
				remapCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(2f, 1f));
			}
			if (remapCurve != null)
			{
				if (remapCurve.length > 0)
				{
					num = remapCurve[remapCurve.length - 1].time;
				}
				for (float num2 = 0f; num2 <= 1f; num2 += 0.003921569f)
				{
					float num3 = remapCurve.Evaluate(num2 * 1f * num);
					curveTex.SetPixel((int)Mathf.Floor(num2 * 255f), 0, new Color(num3, num3, num3));
				}
				curveTex.Apply();
			}
			return 1f / num;
		}

		private void OnDisable()
		{
			if ((bool)rt)
			{
				Object.DestroyImmediate(rt);
				rt = null;
			}
			if ((bool)tonemapMaterial)
			{
				Object.DestroyImmediate(tonemapMaterial);
				tonemapMaterial = null;
			}
			if ((bool)curveTex)
			{
				Object.DestroyImmediate(curveTex);
				curveTex = null;
			}
		}

		private bool CreateInternalRenderTexture()
		{
			if ((bool)rt)
			{
				return false;
			}
			rtFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGBHalf);
			rt = new RenderTexture(1, 1, 0, rtFormat);
			rt.hideFlags = HideFlags.DontSave;
			return true;
		}

		[ImageEffectTransformsToLDR]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!CheckResources())
			{
				Graphics.Blit(source, destination);
				return;
			}
			exposureAdjustment = ((exposureAdjustment < 0.001f) ? 0.001f : exposureAdjustment);
			if (type == TonemapperType.UserCurve)
			{
				float value = UpdateCurve();
				tonemapMaterial.SetFloat("_RangeScale", value);
				tonemapMaterial.SetTexture("_Curve", curveTex);
				Graphics.Blit(source, destination, tonemapMaterial, 4);
				return;
			}
			if (type == TonemapperType.SimpleReinhard)
			{
				tonemapMaterial.SetFloat("_ExposureAdjustment", exposureAdjustment);
				Graphics.Blit(source, destination, tonemapMaterial, 6);
				return;
			}
			if (type == TonemapperType.Hable)
			{
				tonemapMaterial.SetFloat("_ExposureAdjustment", exposureAdjustment);
				Graphics.Blit(source, destination, tonemapMaterial, 5);
				return;
			}
			if (type == TonemapperType.Photographic)
			{
				tonemapMaterial.SetFloat("_ExposureAdjustment", exposureAdjustment);
				Graphics.Blit(source, destination, tonemapMaterial, 8);
				return;
			}
			if (type == TonemapperType.OptimizedHejiDawson)
			{
				tonemapMaterial.SetFloat("_ExposureAdjustment", 0.5f * exposureAdjustment);
				Graphics.Blit(source, destination, tonemapMaterial, 7);
				return;
			}
			bool flag = CreateInternalRenderTexture();
			RenderTexture temporary = RenderTexture.GetTemporary((int)adaptiveTextureSize, (int)adaptiveTextureSize, 0, rtFormat);
			Graphics.Blit(source, temporary);
			int num = (int)Mathf.Log((float)temporary.width * 1f, 2f);
			int num2 = 2;
			RenderTexture[] array = new RenderTexture[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = RenderTexture.GetTemporary(temporary.width / num2, temporary.width / num2, 0, rtFormat);
				num2 *= 2;
			}
			RenderTexture source2 = array[num - 1];
			Graphics.Blit(temporary, array[0], tonemapMaterial, 1);
			if (type == TonemapperType.AdaptiveReinhardAutoWhite)
			{
				for (int j = 0; j < num - 1; j++)
				{
					Graphics.Blit(array[j], array[j + 1], tonemapMaterial, 9);
					source2 = array[j + 1];
				}
			}
			else if (type == TonemapperType.AdaptiveReinhard)
			{
				for (int k = 0; k < num - 1; k++)
				{
					Graphics.Blit(array[k], array[k + 1]);
					source2 = array[k + 1];
				}
			}
			adaptionSpeed = ((adaptionSpeed < 0.001f) ? 0.001f : adaptionSpeed);
			tonemapMaterial.SetFloat("_AdaptionSpeed", adaptionSpeed);
			Graphics.Blit(source2, rt, tonemapMaterial, flag ? 3 : 2);
			middleGrey = ((middleGrey < 0.001f) ? 0.001f : middleGrey);
			tonemapMaterial.SetVector("_HdrParams", new Vector4(middleGrey, middleGrey, middleGrey, white * white));
			tonemapMaterial.SetTexture("_SmallTex", rt);
			if (type == TonemapperType.AdaptiveReinhard)
			{
				Graphics.Blit(source, destination, tonemapMaterial, 0);
			}
			else if (type == TonemapperType.AdaptiveReinhardAutoWhite)
			{
				Graphics.Blit(source, destination, tonemapMaterial, 10);
			}
			else
			{
				Debug.LogError("No valid adaptive tonemapper type found!");
				Graphics.Blit(source, destination);
			}
			for (int l = 0; l < num; l++)
			{
				RenderTexture.ReleaseTemporary(array[l]);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
