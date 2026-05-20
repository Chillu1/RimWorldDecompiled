using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Displacement/Fisheye")]
	public class Fisheye : PostEffectsBase
	{
		[Range(0f, 1.5f)]
		public float strengthX = 0.05f;

		[Range(0f, 1.5f)]
		public float strengthY = 0.05f;

		public Shader fishEyeShader;

		private Material fisheyeMaterial;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: false);
			fisheyeMaterial = CheckShaderAndCreateMaterial(fishEyeShader, fisheyeMaterial);
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
			float num = 5f / 32f;
			float num2 = (float)source.width * 1f / ((float)source.height * 1f);
			fisheyeMaterial.SetVector("intensity", new Vector4(strengthX * num2 * num, strengthY * num, strengthX * num2 * num, strengthY * num));
			Graphics.Blit(source, destination, fisheyeMaterial);
		}
	}
}
