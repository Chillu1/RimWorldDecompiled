using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Camera/Depth of Field (Lens Blur, Scatter, DX11)")]
	public class DepthOfField : PostEffectsBase
	{
		public enum BlurType
		{
			DiscBlur,
			DX11
		}

		public enum BlurSampleCount
		{
			Low,
			Medium,
			High
		}

		public bool visualizeFocus;

		public float focalLength = 10f;

		public float focalSize = 0.05f;

		public float aperture = 0.5f;

		public Transform focalTransform;

		public float maxBlurSize = 2f;

		public bool highResolution;

		public BlurType blurType;

		public BlurSampleCount blurSampleCount = BlurSampleCount.High;

		public bool nearBlur;

		public float foregroundOverlap = 1f;

		public Shader dofHdrShader;

		private Material dofHdrMaterial;

		public Shader dx11BokehShader;

		private Material dx11bokehMaterial;

		public float dx11BokehThreshold = 0.5f;

		public float dx11SpawnHeuristic = 0.0875f;

		public Texture2D dx11BokehTexture;

		public float dx11BokehScale = 1.2f;

		public float dx11BokehIntensity = 2.5f;

		private float focalDistance01 = 10f;

		private ComputeBuffer cbDrawArgs;

		private ComputeBuffer cbPoints;

		private float internalBlurWidth = 1f;

		private Camera cachedCamera;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: true);
			dofHdrMaterial = CheckShaderAndCreateMaterial(dofHdrShader, dofHdrMaterial);
			if (supportDX11 && blurType == BlurType.DX11)
			{
				dx11bokehMaterial = CheckShaderAndCreateMaterial(dx11BokehShader, dx11bokehMaterial);
				CreateComputeResources();
			}
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		private void OnEnable()
		{
			cachedCamera = GetComponent<Camera>();
			cachedCamera.depthTextureMode |= DepthTextureMode.Depth;
		}

		private void OnDisable()
		{
			ReleaseComputeResources();
			if ((bool)dofHdrMaterial)
			{
				Object.DestroyImmediate(dofHdrMaterial);
			}
			dofHdrMaterial = null;
			if ((bool)dx11bokehMaterial)
			{
				Object.DestroyImmediate(dx11bokehMaterial);
			}
			dx11bokehMaterial = null;
		}

		private void ReleaseComputeResources()
		{
			if (cbDrawArgs != null)
			{
				cbDrawArgs.Release();
			}
			cbDrawArgs = null;
			if (cbPoints != null)
			{
				cbPoints.Release();
			}
			cbPoints = null;
		}

		private void CreateComputeResources()
		{
			if (cbDrawArgs == null)
			{
				cbDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.DrawIndirect);
				int[] data = new int[4] { 0, 1, 0, 0 };
				cbDrawArgs.SetData(data);
			}
			if (cbPoints == null)
			{
				cbPoints = new ComputeBuffer(90000, 28, ComputeBufferType.Append);
			}
		}

		private float FocalDistance01(float worldDist)
		{
			return cachedCamera.WorldToViewportPoint((worldDist - cachedCamera.nearClipPlane) * cachedCamera.transform.forward + cachedCamera.transform.position).z / (cachedCamera.farClipPlane - cachedCamera.nearClipPlane);
		}

		private void WriteCoc(RenderTexture fromTo, bool fgDilate)
		{
			dofHdrMaterial.SetTexture("_FgOverlap", null);
			if (nearBlur && fgDilate)
			{
				int width = fromTo.width / 2;
				int height = fromTo.height / 2;
				RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, fromTo.format);
				Graphics.Blit(fromTo, temporary, dofHdrMaterial, 4);
				float num = internalBlurWidth * foregroundOverlap;
				dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, num, 0f, num));
				RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, fromTo.format);
				Graphics.Blit(temporary, temporary2, dofHdrMaterial, 2);
				RenderTexture.ReleaseTemporary(temporary);
				dofHdrMaterial.SetVector("_Offsets", new Vector4(num, 0f, 0f, num));
				temporary = RenderTexture.GetTemporary(width, height, 0, fromTo.format);
				Graphics.Blit(temporary2, temporary, dofHdrMaterial, 2);
				RenderTexture.ReleaseTemporary(temporary2);
				dofHdrMaterial.SetTexture("_FgOverlap", temporary);
				Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 13);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else
			{
				Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 0);
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!CheckResources())
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (aperture < 0f)
			{
				aperture = 0f;
			}
			if (maxBlurSize < 0.1f)
			{
				maxBlurSize = 0.1f;
			}
			focalSize = Mathf.Clamp(focalSize, 0f, 2f);
			internalBlurWidth = Mathf.Max(maxBlurSize, 0f);
			focalDistance01 = (focalTransform ? (cachedCamera.WorldToViewportPoint(focalTransform.position).z / cachedCamera.farClipPlane) : FocalDistance01(focalLength));
			dofHdrMaterial.SetVector("_CurveParams", new Vector4(1f, focalSize, 1f / (1f - aperture) - 1f, focalDistance01));
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			RenderTexture renderTexture3 = null;
			RenderTexture renderTexture4 = null;
			float num = internalBlurWidth * foregroundOverlap;
			if (visualizeFocus)
			{
				WriteCoc(source, fgDilate: true);
				Graphics.Blit(source, destination, dofHdrMaterial, 16);
			}
			else if (blurType == BlurType.DX11 && (bool)dx11bokehMaterial)
			{
				if (highResolution)
				{
					internalBlurWidth = ((internalBlurWidth < 0.1f) ? 0.1f : internalBlurWidth);
					num = internalBlurWidth * foregroundOverlap;
					renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
					RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
					WriteCoc(source, fgDilate: false);
					renderTexture3 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
					renderTexture4 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
					Graphics.Blit(source, renderTexture3, dofHdrMaterial, 15);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, 1.5f, 0f, 1.5f));
					Graphics.Blit(renderTexture3, renderTexture4, dofHdrMaterial, 19);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(1.5f, 0f, 0f, 1.5f));
					Graphics.Blit(renderTexture4, renderTexture3, dofHdrMaterial, 19);
					if (nearBlur)
					{
						Graphics.Blit(source, renderTexture4, dofHdrMaterial, 4);
					}
					dx11bokehMaterial.SetTexture("_BlurredColor", renderTexture3);
					dx11bokehMaterial.SetFloat("_SpawnHeuristic", dx11SpawnHeuristic);
					dx11bokehMaterial.SetVector("_BokehParams", new Vector4(dx11BokehScale, dx11BokehIntensity, Mathf.Clamp(dx11BokehThreshold, 0.005f, 4f), internalBlurWidth));
					dx11bokehMaterial.SetTexture("_FgCocMask", nearBlur ? renderTexture4 : null);
					Graphics.SetRandomWriteTarget(1, cbPoints);
					Graphics.Blit(source, renderTexture, dx11bokehMaterial, 0);
					Graphics.ClearRandomWriteTargets();
					if (nearBlur)
					{
						dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, num, 0f, num));
						Graphics.Blit(renderTexture4, renderTexture3, dofHdrMaterial, 2);
						dofHdrMaterial.SetVector("_Offsets", new Vector4(num, 0f, 0f, num));
						Graphics.Blit(renderTexture3, renderTexture4, dofHdrMaterial, 2);
						Graphics.Blit(renderTexture4, renderTexture, dofHdrMaterial, 3);
					}
					Graphics.Blit(renderTexture, temporary, dofHdrMaterial, 20);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(internalBlurWidth, 0f, 0f, internalBlurWidth));
					Graphics.Blit(renderTexture, source, dofHdrMaterial, 5);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, internalBlurWidth, 0f, internalBlurWidth));
					Graphics.Blit(source, temporary, dofHdrMaterial, 21);
					Graphics.SetRenderTarget(temporary);
					ComputeBuffer.CopyCount(cbPoints, cbDrawArgs, 0);
					dx11bokehMaterial.SetBuffer("pointBuffer", cbPoints);
					dx11bokehMaterial.SetTexture("_MainTex", dx11BokehTexture);
					dx11bokehMaterial.SetVector("_Screen", new Vector3(1f / (1f * (float)source.width), 1f / (1f * (float)source.height), internalBlurWidth));
					dx11bokehMaterial.SetPass(2);
					Graphics.DrawProceduralIndirectNow(MeshTopology.Points, cbDrawArgs);
					Graphics.Blit(temporary, destination);
					RenderTexture.ReleaseTemporary(temporary);
					RenderTexture.ReleaseTemporary(renderTexture3);
					RenderTexture.ReleaseTemporary(renderTexture4);
				}
				else
				{
					renderTexture = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
					renderTexture2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
					num = internalBlurWidth * foregroundOverlap;
					WriteCoc(source, fgDilate: false);
					source.filterMode = FilterMode.Bilinear;
					Graphics.Blit(source, renderTexture, dofHdrMaterial, 6);
					renderTexture3 = RenderTexture.GetTemporary(renderTexture.width >> 1, renderTexture.height >> 1, 0, renderTexture.format);
					renderTexture4 = RenderTexture.GetTemporary(renderTexture.width >> 1, renderTexture.height >> 1, 0, renderTexture.format);
					Graphics.Blit(renderTexture, renderTexture3, dofHdrMaterial, 15);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, 1.5f, 0f, 1.5f));
					Graphics.Blit(renderTexture3, renderTexture4, dofHdrMaterial, 19);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(1.5f, 0f, 0f, 1.5f));
					Graphics.Blit(renderTexture4, renderTexture3, dofHdrMaterial, 19);
					RenderTexture renderTexture5 = null;
					if (nearBlur)
					{
						renderTexture5 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
						Graphics.Blit(source, renderTexture5, dofHdrMaterial, 4);
					}
					dx11bokehMaterial.SetTexture("_BlurredColor", renderTexture3);
					dx11bokehMaterial.SetFloat("_SpawnHeuristic", dx11SpawnHeuristic);
					dx11bokehMaterial.SetVector("_BokehParams", new Vector4(dx11BokehScale, dx11BokehIntensity, Mathf.Clamp(dx11BokehThreshold, 0.005f, 4f), internalBlurWidth));
					dx11bokehMaterial.SetTexture("_FgCocMask", renderTexture5);
					Graphics.SetRandomWriteTarget(1, cbPoints);
					Graphics.Blit(renderTexture, renderTexture2, dx11bokehMaterial, 0);
					Graphics.ClearRandomWriteTargets();
					RenderTexture.ReleaseTemporary(renderTexture3);
					RenderTexture.ReleaseTemporary(renderTexture4);
					if (nearBlur)
					{
						dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, num, 0f, num));
						Graphics.Blit(renderTexture5, renderTexture, dofHdrMaterial, 2);
						dofHdrMaterial.SetVector("_Offsets", new Vector4(num, 0f, 0f, num));
						Graphics.Blit(renderTexture, renderTexture5, dofHdrMaterial, 2);
						Graphics.Blit(renderTexture5, renderTexture2, dofHdrMaterial, 3);
					}
					dofHdrMaterial.SetVector("_Offsets", new Vector4(internalBlurWidth, 0f, 0f, internalBlurWidth));
					Graphics.Blit(renderTexture2, renderTexture, dofHdrMaterial, 5);
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, internalBlurWidth, 0f, internalBlurWidth));
					Graphics.Blit(renderTexture, renderTexture2, dofHdrMaterial, 5);
					Graphics.SetRenderTarget(renderTexture2);
					ComputeBuffer.CopyCount(cbPoints, cbDrawArgs, 0);
					dx11bokehMaterial.SetBuffer("pointBuffer", cbPoints);
					dx11bokehMaterial.SetTexture("_MainTex", dx11BokehTexture);
					dx11bokehMaterial.SetVector("_Screen", new Vector3(1f / (1f * (float)renderTexture2.width), 1f / (1f * (float)renderTexture2.height), internalBlurWidth));
					dx11bokehMaterial.SetPass(1);
					Graphics.DrawProceduralIndirectNow(MeshTopology.Points, cbDrawArgs);
					dofHdrMaterial.SetTexture("_LowRez", renderTexture2);
					dofHdrMaterial.SetTexture("_FgOverlap", renderTexture5);
					dofHdrMaterial.SetVector("_Offsets", 1f * (float)source.width / (1f * (float)renderTexture2.width) * internalBlurWidth * Vector4.one);
					Graphics.Blit(source, destination, dofHdrMaterial, 9);
					if ((bool)renderTexture5)
					{
						RenderTexture.ReleaseTemporary(renderTexture5);
					}
				}
			}
			else
			{
				source.filterMode = FilterMode.Bilinear;
				if (highResolution)
				{
					internalBlurWidth *= 2f;
				}
				WriteCoc(source, fgDilate: true);
				renderTexture = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
				renderTexture2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
				int pass = ((blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 17 : 11);
				if (highResolution)
				{
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, internalBlurWidth, 0.025f, internalBlurWidth));
					Graphics.Blit(source, destination, dofHdrMaterial, pass);
				}
				else
				{
					dofHdrMaterial.SetVector("_Offsets", new Vector4(0f, internalBlurWidth, 0.1f, internalBlurWidth));
					Graphics.Blit(source, renderTexture, dofHdrMaterial, 6);
					Graphics.Blit(renderTexture, renderTexture2, dofHdrMaterial, pass);
					dofHdrMaterial.SetTexture("_LowRez", renderTexture2);
					dofHdrMaterial.SetTexture("_FgOverlap", null);
					dofHdrMaterial.SetVector("_Offsets", Vector4.one * (1f * (float)source.width / (1f * (float)renderTexture2.width)) * internalBlurWidth);
					Graphics.Blit(source, destination, dofHdrMaterial, (blurSampleCount == BlurSampleCount.High) ? 18 : 12);
				}
			}
			if ((bool)renderTexture)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			if ((bool)renderTexture2)
			{
				RenderTexture.ReleaseTemporary(renderTexture2);
			}
		}
	}
}
