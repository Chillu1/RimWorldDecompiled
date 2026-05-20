using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public class PathGridDef : Def
{
	public Type workerType = typeof(PathGrid);

	public bool fencePassable;

	public bool flying;

	public override IEnumerable<string> ConfigErrors()
	{
		if (workerType == null)
		{
			yield return "Path grid def worker type is not present.";
		}
		else if (!typeof(PathGrid).IsAssignableFrom(workerType))
		{
			yield return "Path grid def worker type is not a subclass of PathGrid, type was: " + workerType.FullName;
		}
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
	}
}
