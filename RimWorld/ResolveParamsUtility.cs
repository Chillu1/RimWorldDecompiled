using System.Collections.Generic;

namespace RimWorld;

public static class ResolveParamsUtility
{
	public static void SetCustom<T>(ref Dictionary<string, object> custom, string name, T obj, bool inherit = false)
	{
		if (custom == null)
		{
			custom = new Dictionary<string, object>();
		}
		else
		{
			custom = new Dictionary<string, object>(custom);
		}
		if (!custom.ContainsKey(name))
		{
			custom.Add(name, obj);
		}
		else if (!inherit)
		{
			custom[name] = obj;
		}
	}

	public static void RemoveCustom(ref Dictionary<string, object> custom, string name)
	{
		if (custom != null)
		{
			custom = new Dictionary<string, object>(custom);
			custom.Remove(name);
		}
	}

	public static bool TryGetCustom<T>(Dictionary<string, object> custom, string name, out T obj)
	{
		if (custom == null || !custom.TryGetValue(name, out var value))
		{
			obj = default(T);
			return false;
		}
		obj = (T)value;
		return true;
	}

	public static T GetCustom<T>(Dictionary<string, object> custom, string name)
	{
		if (custom == null || !custom.TryGetValue(name, out var value))
		{
			return default(T);
		}
		return (T)value;
	}
}
