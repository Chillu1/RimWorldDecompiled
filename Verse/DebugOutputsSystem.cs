using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Verse
{
	public static class DebugOutputsSystem
	{
		[DebugOutput("System", false)]
		public static void LoadedAssets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			stringBuilder.AppendLine("Meshes: " + array.Length + " (" + TotalBytes(array).ToStringBytes() + ")");
			UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(Material));
			stringBuilder.AppendLine("Materials: " + array2.Length + " (" + TotalBytes(array2).ToStringBytes() + ")");
			stringBuilder.AppendLine("   Damaged: " + DamagedMatPool.MatCount);
			stringBuilder.AppendLine("   Faded: " + FadedMaterialPool.TotalMaterialCount + " (" + FadedMaterialPool.TotalMaterialBytes.ToStringBytes() + ")");
			stringBuilder.AppendLine("   SolidColorsSimple: " + SolidColorMaterials.SimpleColorMatCount);
			UnityEngine.Object[] array3 = Resources.FindObjectsOfTypeAll(typeof(Texture));
			stringBuilder.AppendLine("Textures: " + array3.Length + " (" + TotalBytes(array3).ToStringBytes() + ")");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Texture list:");
			UnityEngine.Object[] array4 = array3;
			for (int i = 0; i < array4.Length; i++)
			{
				string text = ((Texture)array4[i]).name;
				if (text.NullOrEmpty())
				{
					text = "-";
				}
				stringBuilder.AppendLine(text);
			}
			Log.Message(stringBuilder.ToString());
		}

		private static long TotalBytes(UnityEngine.Object[] arr)
		{
			long num = 0L;
			foreach (UnityEngine.Object o in arr)
			{
				num += Profiler.GetRuntimeMemorySizeLong(o);
			}
			return num;
		}

		[DebugOutput("System", true)]
		public static void DynamicDrawThingsList()
		{
			Find.CurrentMap.dynamicDrawManager.LogDynamicDrawThings();
		}

		[DebugOutput("System", false)]
		public static void RandByCurveTests()
		{
			DebugHistogram debugHistogram = new DebugHistogram(Enumerable.Range(0, 30).Select((Func<int, float>)((int x) => x)).ToArray());
			SimpleCurve curve = new SimpleCurve
			{
				new CurvePoint(0f, 0f),
				new CurvePoint(10f, 1f),
				new CurvePoint(15f, 2f),
				new CurvePoint(20f, 2f),
				new CurvePoint(21f, 0.5f),
				new CurvePoint(30f, 0f)
			};
			float num = 0f;
			for (int i = 0; i < 1000000; i++)
			{
				float num2 = Rand.ByCurve(curve);
				num += num2;
				debugHistogram.Add(num2);
			}
			debugHistogram.Display();
			Log.Message($"Average {num / 1000000f}, calculated as {Rand.ByCurveAverage(curve)}");
		}
	}
}
