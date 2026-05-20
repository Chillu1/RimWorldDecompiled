using System;

namespace Verse.AI;

public class PathingContext : IDisposable
{
	public readonly Map map;

	public readonly PathGrid pathGrid;

	public PathingContext(Map map, PathGrid pathGrid)
	{
		this.map = map;
		this.pathGrid = pathGrid;
	}

	public void Dispose()
	{
		pathGrid.Dispose();
	}
}
