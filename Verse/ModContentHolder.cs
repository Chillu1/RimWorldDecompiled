using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Verse
{
	public class ModContentHolder<T> where T : class
	{
		private ModContentPack mod;

		public Dictionary<string, T> contentList = new Dictionary<string, T>();

		public List<IDisposable> extraDisposables = new List<IDisposable>();

		public ModContentHolder(ModContentPack mod)
		{
			this.mod = mod;
		}

		public void ClearDestroy()
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
			{
				foreach (T value in contentList.Values)
				{
					T localObj = value;
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)localObj);
					});
				}
			}
			for (int i = 0; i < extraDisposables.Count; i++)
			{
				extraDisposables[i].Dispose();
			}
			extraDisposables.Clear();
			contentList.Clear();
		}

		public void ReloadAll()
		{
			foreach (Pair<string, LoadedContentItem<T>> item in ModContentLoader<T>.LoadAllForMod(mod))
			{
				string first = item.First;
				first = first.Replace('\\', '/');
				string text = GenFilePaths.ContentPath<T>();
				if (first.StartsWith(text))
				{
					first = first.Substring(text.Length);
				}
				if (first.EndsWith(Path.GetExtension(first)))
				{
					first = first.Substring(0, first.Length - Path.GetExtension(first).Length);
				}
				if (contentList.ContainsKey(first))
				{
					Log.Warning("Tried to load duplicate " + typeof(T) + " with path: " + item.Second.internalFile + " and internal path: " + first);
				}
				else
				{
					contentList.Add(first, item.Second.contentItem);
					if (item.Second.extraDisposable != null)
					{
						extraDisposables.Add(item.Second.extraDisposable);
					}
				}
			}
		}

		public T Get(string path)
		{
			if (contentList.TryGetValue(path, out T value))
			{
				return value;
			}
			return null;
		}

		public IEnumerable<T> GetAllUnderPath(string pathRoot)
		{
			foreach (KeyValuePair<string, T> content in contentList)
			{
				if (content.Key.StartsWith(pathRoot))
				{
					yield return content.Value;
				}
			}
		}
	}
}
