using RimWorld;
using UnityEngine;

namespace Verse;

public struct CellTerrain
{
	public TerrainDef def;

	public bool polluted;

	public float snowCoverage;

	public float sandCoverage;

	public ColorDef color;

	public CellTerrain(TerrainDef def, bool polluted, float snowCoverage, float sandCoverage, ColorDef color)
	{
		this.def = def;
		this.polluted = polluted;
		this.snowCoverage = snowCoverage;
		this.sandCoverage = sandCoverage;
		this.color = color;
	}

	public override bool Equals(object obj)
	{
		if (obj is CellTerrain terrain)
		{
			return Equals(terrain);
		}
		return false;
	}

	public bool Equals(CellTerrain terrain)
	{
		if (terrain.def == def && terrain.color == color && terrain.polluted == polluted && Mathf.Abs(terrain.snowCoverage - snowCoverage) < float.Epsilon)
		{
			return Mathf.Abs(terrain.sandCoverage - sandCoverage) < float.Epsilon;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(0, def), polluted), snowCoverage), sandCoverage), color);
	}
}
