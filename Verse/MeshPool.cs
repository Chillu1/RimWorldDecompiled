using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class MeshPool
	{
		private const int MaxGridMeshSize = 15;

		private const float HumanlikeBodyWidth = 1.5f;

		private const float HumanlikeHeadAverageWidth = 1.5f;

		private const float HumanlikeHeadNarrowWidth = 1.3f;

		public static readonly GraphicMeshSet humanlikeBodySet;

		public static readonly GraphicMeshSet humanlikeHeadSet;

		public static readonly GraphicMeshSet humanlikeHairSetAverage;

		public static readonly GraphicMeshSet humanlikeHairSetNarrow;

		public static readonly Mesh plane025;

		public static readonly Mesh plane03;

		public static readonly Mesh plane05;

		public static readonly Mesh plane08;

		public static readonly Mesh plane10;

		public static readonly Mesh plane10Back;

		public static readonly Mesh plane10Flip;

		public static readonly Mesh plane14;

		public static readonly Mesh plane20;

		public static readonly Mesh wholeMapPlane;

		private static Dictionary<Vector2, Mesh> planes;

		private static Dictionary<Vector2, Mesh> planesFlip;

		public static readonly Mesh circle;

		public static readonly Mesh[] pies;

		static MeshPool()
		{
			humanlikeBodySet = new GraphicMeshSet(1.5f);
			humanlikeHeadSet = new GraphicMeshSet(1.5f);
			humanlikeHairSetAverage = new GraphicMeshSet(1.5f);
			humanlikeHairSetNarrow = new GraphicMeshSet(1.3f, 1.5f);
			plane025 = MeshMakerPlanes.NewPlaneMesh(0.25f);
			plane03 = MeshMakerPlanes.NewPlaneMesh(0.3f);
			plane05 = MeshMakerPlanes.NewPlaneMesh(0.5f);
			plane08 = MeshMakerPlanes.NewPlaneMesh(0.8f);
			plane10 = MeshMakerPlanes.NewPlaneMesh(1f);
			plane10Back = MeshMakerPlanes.NewPlaneMesh(1f, flipped: false, backLift: true);
			plane10Flip = MeshMakerPlanes.NewPlaneMesh(1f, flipped: true);
			plane14 = MeshMakerPlanes.NewPlaneMesh(1.4f);
			plane20 = MeshMakerPlanes.NewPlaneMesh(2f);
			planes = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);
			planesFlip = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);
			circle = MeshMakerCircles.MakeCircleMesh(1f);
			pies = new Mesh[361];
			for (int i = 0; i < 361; i++)
			{
				pies[i] = MeshMakerCircles.MakePieMesh(i);
			}
			wholeMapPlane = MeshMakerPlanes.NewWholeMapPlane();
		}

		public static Mesh GridPlane(Vector2 size)
		{
			if (!planes.TryGetValue(size, out var value))
			{
				value = MeshMakerPlanes.NewPlaneMesh(size, flipped: false, backLift: false, twist: false);
				planes.Add(size, value);
			}
			return value;
		}

		public static Mesh GridPlaneFlip(Vector2 size)
		{
			if (!planesFlip.TryGetValue(size, out var value))
			{
				value = MeshMakerPlanes.NewPlaneMesh(size, flipped: true, backLift: false, twist: false);
				planesFlip.Add(size, value);
			}
			return value;
		}

		private static Vector2 RoundedToHundredths(this Vector2 v)
		{
			return new Vector2((float)(int)(v.x * 100f) / 100f, (float)(int)(v.y * 100f) / 100f);
		}

		[DebugOutput("System", false)]
		public static void MeshPoolStats()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MeshPool stats:");
			stringBuilder.AppendLine("Planes: " + planes.Count);
			stringBuilder.AppendLine("PlanesFlip: " + planesFlip.Count);
			Log.Message(stringBuilder.ToString());
		}
	}
}
