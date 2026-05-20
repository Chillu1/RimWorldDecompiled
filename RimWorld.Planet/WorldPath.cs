using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldPath : IDisposable
{
	private readonly List<PlanetTile> nodes = new List<PlanetTile>(128);

	private float totalCostInt;

	private int curNodeIndex;

	private PlanetLayer layer;

	public bool inUse;

	public bool Found => totalCostInt >= 0f;

	public float TotalCost => totalCostInt;

	public int NodesLeftCount => curNodeIndex + 1;

	public List<PlanetTile> NodesReversed => nodes;

	public PlanetTile FirstNode
	{
		get
		{
			List<PlanetTile> list = nodes;
			return list[list.Count - 1];
		}
	}

	public PlanetTile LastNode => nodes[0];

	public PlanetLayer Layer => layer;

	public int NodeCount => nodes.Count;

	public static WorldPath NotFound => WorldPathPool.NotFoundPath;

	public void AddNodeAtStart(PlanetTile tile)
	{
		nodes.Add(tile);
	}

	public void SetupFound(float totalCost, PlanetLayer planetLayer)
	{
		if (this == NotFound)
		{
			Log.Warning($"Calling SetupFound with totalCost={totalCost} on WorldPath.NotFound");
			return;
		}
		layer = planetLayer;
		totalCostInt = totalCost;
		curNodeIndex = nodes.Count - 1;
	}

	public void Dispose()
	{
		ReleaseToPool();
	}

	public void ReleaseToPool()
	{
		if (this != NotFound)
		{
			totalCostInt = 0f;
			nodes.Clear();
			inUse = false;
			layer = null;
		}
	}

	public static WorldPath NewNotFound()
	{
		return new WorldPath
		{
			totalCostInt = -1f
		};
	}

	public PlanetTile ConsumeNextNode()
	{
		PlanetTile result = Peek(1);
		curNodeIndex--;
		return result;
	}

	public PlanetTile Peek(int nodesAhead)
	{
		return nodes[curNodeIndex - nodesAhead];
	}

	public override string ToString()
	{
		if (!Found)
		{
			return "WorldPath(not found)";
		}
		if (!inUse)
		{
			return "WorldPath(not in use)";
		}
		return "WorldPath(nodeCount= " + nodes.Count + ((nodes.Count > 0) ? (" first=" + FirstNode.ToString() + " last=" + LastNode) : "") + " cost=" + totalCostInt + " )";
	}

	public void DrawPath(Caravan pathingCaravan)
	{
		if (!Found || NodesLeftCount <= 0)
		{
			return;
		}
		WorldGrid worldGrid = Find.WorldGrid;
		float num = 0.08f;
		for (int i = 0; i < NodesLeftCount - 1; i++)
		{
			Vector3 tileCenter = worldGrid.GetTileCenter(Peek(i));
			Vector3 tileCenter2 = worldGrid.GetTileCenter(Peek(i + 1));
			tileCenter += tileCenter.normalized * num;
			tileCenter2 += tileCenter2.normalized * num;
			GenDraw.DrawWorldLineBetween(tileCenter, tileCenter2);
		}
		if (pathingCaravan != null)
		{
			Vector3 drawPos = pathingCaravan.DrawPos;
			Vector3 tileCenter3 = worldGrid.GetTileCenter(Peek(0));
			drawPos += drawPos.normalized * num;
			tileCenter3 += tileCenter3.normalized * num;
			if ((drawPos - tileCenter3).sqrMagnitude > 0.005f)
			{
				GenDraw.DrawWorldLineBetween(drawPos, tileCenter3);
			}
		}
	}
}
