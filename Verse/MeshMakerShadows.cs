using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MeshMakerShadows
	{
		private static List<Vector3> vertsList = new List<Vector3>();

		private static List<Color32> colorsList = new List<Color32>();

		private static List<int> trianglesList = new List<int>();

		private static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

		public static Mesh NewShadowMesh(float baseWidth, float baseHeight, float tallness)
		{
			Color32 item = new Color32(byte.MaxValue, 0, 0, (byte)(255f * tallness));
			float num = baseWidth / 2f;
			float num2 = baseHeight / 2f;
			vertsList.Clear();
			colorsList.Clear();
			trianglesList.Clear();
			vertsList.Add(new Vector3(0f - num, 0f, 0f - num2));
			vertsList.Add(new Vector3(0f - num, 0f, num2));
			vertsList.Add(new Vector3(num, 0f, num2));
			vertsList.Add(new Vector3(num, 0f, 0f - num2));
			colorsList.Add(LowVertexColor);
			colorsList.Add(LowVertexColor);
			colorsList.Add(LowVertexColor);
			colorsList.Add(LowVertexColor);
			trianglesList.Add(0);
			trianglesList.Add(1);
			trianglesList.Add(2);
			trianglesList.Add(0);
			trianglesList.Add(2);
			trianglesList.Add(3);
			int count = vertsList.Count;
			vertsList.Add(new Vector3(0f - num, 0f, 0f - num2));
			colorsList.Add(item);
			vertsList.Add(new Vector3(0f - num, 0f, num2));
			colorsList.Add(item);
			trianglesList.Add(0);
			trianglesList.Add(count);
			trianglesList.Add(count + 1);
			trianglesList.Add(0);
			trianglesList.Add(count + 1);
			trianglesList.Add(1);
			int count2 = vertsList.Count;
			vertsList.Add(new Vector3(num, 0f, num2));
			colorsList.Add(item);
			vertsList.Add(new Vector3(num, 0f, 0f - num2));
			colorsList.Add(item);
			trianglesList.Add(2);
			trianglesList.Add(count2);
			trianglesList.Add(count2 + 1);
			trianglesList.Add(count2 + 1);
			trianglesList.Add(3);
			trianglesList.Add(2);
			int count3 = vertsList.Count;
			vertsList.Add(new Vector3(0f - num, 0f, 0f - num2));
			colorsList.Add(item);
			vertsList.Add(new Vector3(num, 0f, 0f - num2));
			colorsList.Add(item);
			trianglesList.Add(0);
			trianglesList.Add(3);
			trianglesList.Add(count3);
			trianglesList.Add(3);
			trianglesList.Add(count3 + 1);
			trianglesList.Add(count3);
			return new Mesh
			{
				name = "NewShadowMesh()",
				vertices = vertsList.ToArray(),
				colors32 = colorsList.ToArray(),
				triangles = trianglesList.ToArray()
			};
		}
	}
}
