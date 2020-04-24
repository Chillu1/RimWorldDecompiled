using System;
using TMPro;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldFeatureTextMesh_TextMeshPro : WorldFeatureTextMesh
	{
		private TextMeshPro textMesh;

		public static readonly GameObject WorldTextPrefab = Resources.Load<GameObject>("Prefabs/WorldText");

		[TweakValue("Interface.World", 0f, 5f)]
		private static float TextScale = 1f;

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
				textMesh.fontSize = value * TextScale;
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

		private static void TextScale_Changed()
		{
			Find.WorldFeatures.textsCreated = false;
		}

		public override void SetActive(bool active)
		{
			textMesh.gameObject.SetActive(active);
		}

		public override void Destroy()
		{
			UnityEngine.Object.Destroy(textMesh.gameObject);
		}

		public override void Init()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(WorldTextPrefab);
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			textMesh = gameObject.GetComponent<TextMeshPro>();
			Color = new Color(1f, 1f, 1f, 0f);
			Material[] sharedMaterials = textMesh.GetComponent<MeshRenderer>().sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				sharedMaterials[i].renderQueue = WorldMaterials.FeatureNameRenderQueue;
			}
		}

		public override void WrapAroundPlanetSurface()
		{
			textMesh.ForceMeshUpdate();
			TMP_TextInfo textInfo = textMesh.textInfo;
			int characterCount = textInfo.characterCount;
			if (characterCount == 0)
			{
				return;
			}
			float num = textMesh.bounds.extents.x * 2f;
			float num2 = Find.WorldGrid.DistOnSurfaceToAngle(num);
			Matrix4x4 localToWorldMatrix = textMesh.transform.localToWorldMatrix;
			Matrix4x4 worldToLocalMatrix = textMesh.transform.worldToLocalMatrix;
			for (int i = 0; i < characterCount; i++)
			{
				TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
				if (tMP_CharacterInfo.isVisible)
				{
					int materialReferenceIndex = textMesh.textInfo.characterInfo[i].materialReferenceIndex;
					int vertexIndex = tMP_CharacterInfo.vertexIndex;
					Vector3 b = textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex] + textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 1] + textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 2] + textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 3];
					b /= 4f;
					float num3 = b.x / (num / 2f);
					bool flag = num3 >= 0f;
					num3 = Mathf.Abs(num3);
					float num4 = num2 / 2f * num3;
					float num5 = (180f - num4) / 2f;
					float num6 = 200f * Mathf.Tan(num4 / 2f * ((float)Math.PI / 180f));
					Vector3 vector = new Vector3(Mathf.Sin(num5 * ((float)Math.PI / 180f)) * num6 * (flag ? 1f : (-1f)), b.y, Mathf.Cos(num5 * ((float)Math.PI / 180f)) * num6);
					Vector3 b2 = vector - b;
					Vector3 a = textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex] + b2;
					Vector3 a2 = textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 1] + b2;
					Vector3 a3 = textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 2] + b2;
					Vector3 a4 = textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 3] + b2;
					Quaternion rotation = Quaternion.Euler(0f, num4 * (flag ? (-1f) : 1f), 0f);
					a = rotation * (a - vector) + vector;
					a2 = rotation * (a2 - vector) + vector;
					a3 = rotation * (a3 - vector) + vector;
					a4 = rotation * (a4 - vector) + vector;
					a = worldToLocalMatrix.MultiplyPoint(localToWorldMatrix.MultiplyPoint(a).normalized * (100f + WorldAltitudeOffsets.WorldText));
					a2 = worldToLocalMatrix.MultiplyPoint(localToWorldMatrix.MultiplyPoint(a2).normalized * (100f + WorldAltitudeOffsets.WorldText));
					a3 = worldToLocalMatrix.MultiplyPoint(localToWorldMatrix.MultiplyPoint(a3).normalized * (100f + WorldAltitudeOffsets.WorldText));
					a4 = worldToLocalMatrix.MultiplyPoint(localToWorldMatrix.MultiplyPoint(a4).normalized * (100f + WorldAltitudeOffsets.WorldText));
					textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex] = a;
					textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 1] = a2;
					textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 2] = a3;
					textMesh.textInfo.meshInfo[materialReferenceIndex].vertices[vertexIndex + 3] = a4;
				}
			}
			textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		}
	}
}
