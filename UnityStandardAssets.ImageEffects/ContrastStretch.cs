using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Color Adjustments/Contrast Stretch")]
	public class ContrastStretch : MonoBehaviour
	{
		[Range(0.0001f, 1f)]
		public float adaptationSpeed = 0.02f;

		[Range(0f, 1f)]
		public float limitMinimum = 0.2f;

		[Range(0f, 1f)]
		public float limitMaximum = 0.6f;

		private RenderTexture[] adaptRenderTex = new RenderTexture[2];

		private int curAdaptIndex;

		public Shader shaderLum;

		private Material m_materialLum;

		public Shader shaderReduce;

		private Material m_materialReduce;

		public Shader shaderAdapt;

		private Material m_materialAdapt;

		public Shader shaderApply;

		private Material m_materialApply;

		protected Material materialLum
		{
			get
			{
				if (m_materialLum == null)
				{
					m_materialLum = new Material(shaderLum);
					m_materialLum.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_materialLum;
			}
		}

		protected Material materialReduce
		{
			get
			{
				if (m_materialReduce == null)
				{
					m_materialReduce = new Material(shaderReduce);
					m_materialReduce.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_materialReduce;
			}
		}

		protected Material materialAdapt
		{
			get
			{
				if (m_materialAdapt == null)
				{
					m_materialAdapt = new Material(shaderAdapt);
					m_materialAdapt.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_materialAdapt;
			}
		}

		protected Material materialApply
		{
			get
			{
				if (m_materialApply == null)
				{
					m_materialApply = new Material(shaderApply);
					m_materialApply.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_materialApply;
			}
		}

		private void Start()
		{
			if (!shaderAdapt.isSupported || !shaderApply.isSupported || !shaderLum.isSupported || !shaderReduce.isSupported)
			{
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			for (int i = 0; i < 2; i++)
			{
				if (!adaptRenderTex[i])
				{
					adaptRenderTex[i] = new RenderTexture(1, 1, 0);
					adaptRenderTex[i].hideFlags = HideFlags.HideAndDontSave;
				}
			}
		}

		private void OnDisable()
		{
			for (int i = 0; i < 2; i++)
			{
				Object.DestroyImmediate(adaptRenderTex[i]);
				adaptRenderTex[i] = null;
			}
			if ((bool)m_materialLum)
			{
				Object.DestroyImmediate(m_materialLum);
			}
			if ((bool)m_materialReduce)
			{
				Object.DestroyImmediate(m_materialReduce);
			}
			if ((bool)m_materialAdapt)
			{
				Object.DestroyImmediate(m_materialAdapt);
			}
			if ((bool)m_materialApply)
			{
				Object.DestroyImmediate(m_materialApply);
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(source.width / 1, source.height / 1);
			Graphics.Blit(source, renderTexture, materialLum);
			while (renderTexture.width > 1 || renderTexture.height > 1)
			{
				int num = renderTexture.width / 2;
				if (num < 1)
				{
					num = 1;
				}
				int num2 = renderTexture.height / 2;
				if (num2 < 1)
				{
					num2 = 1;
				}
				RenderTexture temporary = RenderTexture.GetTemporary(num, num2);
				Graphics.Blit(renderTexture, temporary, materialReduce);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary;
			}
			CalculateAdaptation(renderTexture);
			materialApply.SetTexture("_AdaptTex", adaptRenderTex[curAdaptIndex]);
			Graphics.Blit(source, destination, materialApply);
			RenderTexture.ReleaseTemporary(renderTexture);
		}

		private void CalculateAdaptation(Texture curTexture)
		{
			int num = curAdaptIndex;
			curAdaptIndex = (curAdaptIndex + 1) % 2;
			float value = 1f - Mathf.Pow(1f - adaptationSpeed, 30f * Time.deltaTime);
			value = Mathf.Clamp(value, 0.01f, 1f);
			materialAdapt.SetTexture("_CurTex", curTexture);
			materialAdapt.SetVector("_AdaptParams", new Vector4(value, limitMinimum, limitMaximum, 0f));
			Graphics.SetRenderTarget(adaptRenderTex[curAdaptIndex]);
			GL.Clear(clearDepth: false, clearColor: true, Color.black);
			Graphics.Blit(adaptRenderTex[num], adaptRenderTex[curAdaptIndex], materialAdapt);
		}
	}
}
