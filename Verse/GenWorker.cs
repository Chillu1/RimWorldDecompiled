using System;
using System.Collections.Generic;

namespace Verse;

public static class GenWorker<T>
{
	private static readonly Dictionary<Type, T> Workers = new Dictionary<Type, T>();

	private static T Get<D>() where D : T
	{
		return Get(typeof(D));
	}

	public static T Get(Type type)
	{
		if (!Workers.TryGetValue(type, out var value))
		{
			value = (Workers[type] = (T)Activator.CreateInstance(type));
		}
		return value;
	}

	public static T Get(Type type, params object[] args)
	{
		if (!Workers.TryGetValue(type, out var value))
		{
			value = (Workers[type] = (T)Activator.CreateInstance(type, args));
		}
		return value;
	}
}
