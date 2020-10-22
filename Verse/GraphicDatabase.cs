using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class GraphicDatabase
	{
		private static Dictionary<GraphicRequest, Graphic> allGraphics = new Dictionary<GraphicRequest, Graphic>();

		public static Graphic Get<T>(string path) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, null, 0, null));
		}

		public static Graphic Get<T>(string path, Shader shader) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, null, 0, null));
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, 0, null));
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, int renderQueue) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, renderQueue, null));
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, null, 0, null));
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data) where T : Graphic, new()
		{
			return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, data, 0, null));
		}

		public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo)
		{
			return Get(graphicClass, path, shader, drawSize, color, colorTwo, null, null);
		}

		public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters)
		{
			GraphicRequest graphicRequest = new GraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, data, 0, shaderParameters);
			if (graphicRequest.graphicClass == typeof(Graphic_Single))
			{
				return GetInner<Graphic_Single>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Terrain))
			{
				return GetInner<Graphic_Terrain>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Multi))
			{
				return GetInner<Graphic_Multi>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Mote))
			{
				return GetInner<Graphic_Mote>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Random))
			{
				return GetInner<Graphic_Random>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Flicker))
			{
				return GetInner<Graphic_Flicker>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Appearances))
			{
				return GetInner<Graphic_Appearances>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_StackCount))
			{
				return GetInner<Graphic_StackCount>(graphicRequest);
			}
			try
			{
				return (Graphic)GenGeneric.InvokeStaticGenericMethod(typeof(GraphicDatabase), graphicRequest.graphicClass, "GetInner", graphicRequest);
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat("Exception getting ", graphicClass, " at ", path, ": ", ex.ToString()));
			}
			return BaseContent.BadGraphic;
		}

		private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
		{
			req.color = (Color32)req.color;
			req.colorTwo = (Color32)req.colorTwo;
			if (!allGraphics.TryGetValue(req, out var value))
			{
				value = new T();
				value.Init(req);
				allGraphics.Add(req, value);
			}
			return (T)value;
		}

		public static void Clear()
		{
			allGraphics.Clear();
		}

		[DebugOutput("System", false)]
		public static void AllGraphicsLoaded()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("There are " + allGraphics.Count + " graphics loaded.");
			int num = 0;
			foreach (Graphic value in allGraphics.Values)
			{
				stringBuilder.AppendLine(num + " - " + value.ToString());
				if (num % 50 == 49)
				{
					Log.Message(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
				num++;
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
