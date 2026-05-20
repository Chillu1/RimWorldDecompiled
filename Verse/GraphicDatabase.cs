using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using LudeonTK;
using UnityEngine;

namespace Verse;

public static class GraphicDatabase
{
	private static Dictionary<GraphicRequest, Graphic> allGraphics = new Dictionary<GraphicRequest, Graphic>();

	private static Dictionary<Type, Func<GraphicRequest, Graphic>> cachedGraphicGetters = new Dictionary<Type, Func<GraphicRequest, Graphic>>();

	public static Graphic Get<T>(string path) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, null, 0, null, null));
	}

	public static Graphic Get<T>(string path, Shader shader) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, null, 0, null, null));
	}

	public static Graphic Get<T>(string path, Shader shader, string mask) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, null, 0, null, mask));
	}

	public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, 0, null, null));
	}

	public static Graphic Get<T>(string path, Shader shader, string mask, int renderQueue) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, null, renderQueue, null, mask));
	}

	public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, int renderQueue) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, renderQueue, null, null));
	}

	public static Graphic Get<T>(Texture2D texture, Shader shader, Vector2 drawSize, Color color, int renderQueue) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), texture, shader, drawSize, color, Color.white, null, renderQueue, null, null));
	}

	public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, null, 0, null, null));
	}

	public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, string maskPath = null) where T : Graphic, new()
	{
		return GetInner<T>(new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, data, 0, null, maskPath));
	}

	public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, string maskPath = null)
	{
		return Get(graphicClass, path, shader, drawSize, color, colorTwo, null, null, maskPath);
	}

	public static Graphic Get(Type graphicClass, Texture2D texture, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters, string maskPath = null)
	{
		return Get(new GraphicRequest(graphicClass, texture, shader, drawSize, color, colorTwo, data, 0, shaderParameters, maskPath));
	}

	public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters, string maskPath = null)
	{
		return Get(new GraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, data, 0, shaderParameters, maskPath));
	}

	private static Graphic Get(GraphicRequest req)
	{
		try
		{
			if (!cachedGraphicGetters.TryGetValue(req.graphicClass, out var value))
			{
				MethodInfo method = typeof(GraphicDatabase).GetMethod("GetInner", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).MakeGenericMethod(req.graphicClass);
				value = (Func<GraphicRequest, Graphic>)Delegate.CreateDelegate(typeof(Func<GraphicRequest, Graphic>), method);
				cachedGraphicGetters.Add(req.graphicClass, value);
			}
			return value(req);
		}
		catch (Exception ex)
		{
			Log.Error("Exception getting " + req.graphicClass?.ToString() + " at " + req.path + ": " + ex.ToString());
		}
		return BaseContent.BadGraphic;
	}

	private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
	{
		req.color = (Color32)req.color;
		req.colorTwo = (Color32)req.colorTwo;
		req.renderQueue = ((req.renderQueue == 0 && req.graphicData != null) ? req.graphicData.renderQueue : req.renderQueue);
		if (!allGraphics.TryGetValue(req, out var value))
		{
			if (!UnityData.IsInMainThread)
			{
				Log.ErrorOnce($"Attempted to load a graphic off the main thread: {req}", req.GetHashCode());
				return null;
			}
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
