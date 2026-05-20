using Unity.Burst;
using UnityEngine;

namespace Verse.Glow;

[BurstCompile]
public struct GlowLight
{
	public readonly float overlightRadius;

	public readonly float glowRadius;

	public readonly ColorInt glowColor;

	public readonly bool isCavePlant;

	public readonly bool isTerrain;

	public readonly IntVec3 position;

	public readonly IntVec3 localGlowGridStartPos;

	public readonly int radius;

	public readonly int diameter;

	public readonly int id;

	public int localGlowPoolIndex;

	public bool dirty;

	public CellRect AffectedRect => new CellRect(localGlowGridStartPos.x, localGlowGridStartPos.z, diameter, diameter);

	public GlowLight(CompGlower glower, int glowPoolIndex)
	{
		overlightRadius = glower.Props.overlightRadius;
		glowRadius = glower.GlowRadius;
		glowColor = glower.GlowColor;
		isCavePlant = glower.Props.overrideIsCavePlant || (glower.parent.def.plant?.cavePlant ?? false);
		id = glower.parent.thingIDNumber;
		localGlowPoolIndex = glowPoolIndex;
		dirty = true;
		isTerrain = false;
		position = glower.parent.Position;
		diameter = Mathf.CeilToInt(glower.GlowRadius * 2f + 1f);
		radius = Mathf.CeilToInt(glower.GlowRadius);
		localGlowGridStartPos = position - new IntVec3(radius, 0, radius);
	}

	public GlowLight(IntVec3 cell, Map map, int glowPoolIndex)
	{
		id = map.cellIndices.CellToIndex(cell);
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(id);
		overlightRadius = 0f;
		glowRadius = terrainDef.glowRadius;
		glowColor = terrainDef.glowColor;
		isCavePlant = false;
		isTerrain = true;
		localGlowPoolIndex = glowPoolIndex;
		dirty = true;
		position = cell;
		diameter = Mathf.CeilToInt(glowRadius * 2f + 1f);
		radius = Mathf.CeilToInt(glowRadius);
		localGlowGridStartPos = position - new IntVec3(radius, 0, radius);
	}

	[BurstCompile]
	public int WorldToLocalIndex(in IntVec3 world)
	{
		return DeltaToLocalIndex(world - position);
	}

	[BurstCompile]
	public int DeltaToLocalIndex(in IntVec3 delta)
	{
		return CellIndicesUtility.CellToIndex(new IntVec3(radius, 0, radius) + delta, diameter);
	}

	[BurstCompile]
	public IntVec3 IndexToLocalDelta(int index)
	{
		return CellIndicesUtility.IndexToCell(index, diameter) - new IntVec3(radius, 0, radius);
	}

	public override string ToString()
	{
		return $"{position} with radius {glowRadius} [ID: {id}, Rect: {AffectedRect}], rgba: {glowColor}, isCavePlant: {isCavePlant}, isTerrain: {isTerrain}, localGlowPoolIndex: {localGlowPoolIndex}";
	}
}
