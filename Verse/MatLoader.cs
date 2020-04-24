using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MatLoader
	{
		private struct Request
		{
			public string path;

			public int renderQueue;

			public override int GetHashCode()
			{
				return Gen.HashCombineInt(Gen.HashCombine(0, path), renderQueue);
			}

			public override bool Equals(object obj)
			{
				if (!(obj is Request))
				{
					return false;
				}
				return Equals((Request)obj);
			}

			public bool Equals(Request other)
			{
				if (other.path == path)
				{
					return other.renderQueue == renderQueue;
				}
				return false;
			}

			public static bool operator ==(Request lhs, Request rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(Request lhs, Request rhs)
			{
				return !(lhs == rhs);
			}

			public override string ToString()
			{
				return "MatLoader.Request(" + path + ", " + renderQueue + ")";
			}
		}

		private static Dictionary<Request, Material> dict = new Dictionary<Request, Material>();

		public static Material LoadMat(string matPath, int renderQueue = -1)
		{
			Material material = (Material)Resources.Load("Materials/" + matPath, typeof(Material));
			if (material == null)
			{
				Log.Warning("Could not load material " + matPath);
			}
			Request request = default(Request);
			request.path = matPath;
			request.renderQueue = renderQueue;
			Request key = request;
			if (!dict.TryGetValue(key, out Material value))
			{
				value = MaterialAllocator.Create(material);
				if (renderQueue != -1)
				{
					value.renderQueue = renderQueue;
				}
				dict.Add(key, value);
			}
			return value;
		}
	}
}
