using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldFeatureTextMesh_Legacy : WorldFeatureTextMesh
	{
		private TextMesh textMesh;

		private const float TextScale = 0.23f;

		private const int MinFontSize = 13;

		private const int MaxFontSize = 40;

		[TweakValue("Interface.World", 0f, 10f)]
		private static float TextScaleFactor = 7.5f;

		public override bool Active => textMesh.gameObject.activeInHierarchy;

		public override Vector3 Position => textMesh.transform.position;

		public override Color Color
		{
			get
			{
				return textMesh.color;
			}
			set
			{
				textMesh.color = value;
			}
		}

		public override string Text
		{
			get
			{
				return textMesh.text;
			}
			set
			{
				textMesh.text = value;
			}
		}

		public override float Size
		{
			set
			{
				textMesh.fontSize = Mathf.RoundToInt(value * TextScaleFactor);
			}
		}

		public override Quaternion Rotation
		{
			get
			{
				return textMesh.transform.rotation;
			}
			set
			{
				textMesh.transform.rotation = value;
			}
		}

		public override Vector3 LocalPosition
		{
			get
			{
				return textMesh.transform.localPosition;
			}
			set
			{
				textMesh.transform.localPosition = value;
			}
		}

		private static void TextScaleFactor_Changed()
		{
			Find.WorldFeatures.textsCreated = false;
		}

		public override void SetActive(bool active)
		{
			textMesh.gameObject.SetActive(active);
		}

		public override void Destroy()
		{
			Object.Destroy(textMesh.gameObject);
		}

		public override void Init()
		{
			GameObject gameObject = new GameObject("World feature name (legacy)");
			gameObject.layer = WorldCameraManager.WorldLayer;
			Object.DontDestroyOnLoad(gameObject);
			textMesh = gameObject.AddComponent<TextMesh>();
			textMesh.color = new Color(1f, 1f, 1f, 0f);
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;
			textMesh.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = WorldMaterials.FeatureNameRenderQueue;
			Color = new Color(1f, 1f, 1f, 0f);
			textMesh.transform.localScale = new Vector3(0.23f, 0.23f, 0.23f);
		}

		public override void WrapAroundPlanetSurface()
		{
		}
	}
}
