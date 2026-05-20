using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class SimpleSurface : IEnumerable<SurfaceColumn>, IEnumerable
{
	private List<SurfaceColumn> columns = new List<SurfaceColumn>();

	public float Evaluate(float x, float y)
	{
		if (columns.Count == 0)
		{
			Log.Error("Evaluating a SimpleCurve2D with no columns.");
			return 0f;
		}
		if (x <= columns[0].x)
		{
			return columns[0].y.Evaluate(y);
		}
		if (x >= columns[columns.Count - 1].x)
		{
			return columns[columns.Count - 1].y.Evaluate(y);
		}
		SurfaceColumn surfaceColumn = columns[0];
		SurfaceColumn surfaceColumn2 = columns[columns.Count - 1];
		for (int i = 0; i < columns.Count; i++)
		{
			if (x <= columns[i].x)
			{
				surfaceColumn2 = columns[i];
				if (i > 0)
				{
					surfaceColumn = columns[i - 1];
				}
				break;
			}
		}
		float t = (x - surfaceColumn.x) / (surfaceColumn2.x - surfaceColumn.x);
		return Mathf.Lerp(surfaceColumn.y.Evaluate(y), surfaceColumn2.y.Evaluate(y), t);
	}

	public void Add(SurfaceColumn newColumn)
	{
		columns.Add(newColumn);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<SurfaceColumn> GetEnumerator()
	{
		foreach (SurfaceColumn column in columns)
		{
			yield return column;
		}
	}

	public IEnumerable<string> ConfigErrors(string prefix)
	{
		for (int i = 0; i < columns.Count - 1; i++)
		{
			if (columns[i + 1].x < columns[i].x)
			{
				yield return prefix + ": columns are out of order";
				break;
			}
		}
	}
}
