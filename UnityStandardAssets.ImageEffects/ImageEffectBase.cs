using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("")]
	public class ImageEffectBase : MonoBehaviour
	{
		public Shader shader;

		private Material m_Material;

		protected Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = new Material(shader);
					m_Material.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_Material;
			}
		}

		protected virtual void Start()
		{
			if (!shader || !shader.isSupported)
			{
				base.enabled = false;
			}
		}

		protected virtual void OnDisable()
		{
			if ((bool)m_Material)
			{
				Object.DestroyImmediate(m_Material);
			}
		}
	}
}
