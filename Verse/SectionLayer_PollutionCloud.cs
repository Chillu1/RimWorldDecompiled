using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class SectionLayer_PollutionCloud : SectionLayer_Gas
{
	private static Material matCached;

	private float MinCellDistance = 5f;

	private int AttemptsPerCell = 15;

	private float MinPercentage = 0.35f;

	private static readonly SimpleCurve NumCellsPerSectionPollutionLevel = new SimpleCurve
	{
		new CurvePoint(10f, 3f),
		new CurvePoint(20f, 6f)
	};

	private static readonly Vector2 FadeTexScrollSpeed = new Vector2(0.035f, 0.0125f);

	private static readonly Vector2 FadeTexScale = new Vector2(0.15f, 0.15f);

	private static readonly CachedTexture PollutionTex = new CachedTexture("Other/PollutionCloud");

	private static readonly CachedTexture PollutionFadeTex = new CachedTexture("Other/PollutionCloudFade");

	private static readonly Color SnowPollutionCloudColor = new Color(1f, 1f, 1f, 0.66f);

	private const float SnowPollutionColorThreshold = 0.4f;

	private List<IntVec3> cellsTmp = new List<IntVec3>();

	protected override FloatRange VertexScaleOffsetRange => new FloatRange(7f, 11f);

	protected override FloatRange VertexPositionOffsetRange => new FloatRange(-1f, 1f);

	public override Material Mat
	{
		get
		{
			if (matCached == null)
			{
				matCached = MaterialPool.MatFrom(PollutionTex.Texture, ShaderDatabase.PollutionCloud, Color.white, 3000);
				matCached.SetTexture(ShaderPropertyIDs.FadeTex, PollutionFadeTex.Texture);
				matCached.SetVector(ShaderPropertyIDs.TexScrollSpeed, FadeTexScrollSpeed);
				matCached.SetVector(ShaderPropertyIDs.TexScale, FadeTexScale);
			}
			return matCached;
		}
	}

	public override bool Visible
	{
		get
		{
			if (base.Visible)
			{
				return ModsConfig.BiotechActive;
			}
			return false;
		}
	}

	public SectionLayer_PollutionCloud(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Pollution;
	}

	public override Color ColorAt(IntVec3 cell)
	{
		float depth = base.Map.snowGrid.GetDepth(cell);
		cell.GetSandDepth(base.Map);
		TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(cell);
		if (!(depth < 0.4f))
		{
			return SnowPollutionCloudColor;
		}
		return terrainDef.pollutionCloudColor;
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		LayerSubMesh subMesh = GetSubMesh(Mat);
		float altitude = AltitudeLayer.Gas.AltitudeFor();
		int num = section.botLeft.x;
		foreach (IntVec3 item in AffectedCells(section.CellRect))
		{
			int count = subMesh.verts.Count;
			AddCell(item, num, count, subMesh, altitude);
			num++;
		}
		if (subMesh.verts.Count > 0)
		{
			subMesh.FinalizeMesh(MeshParts.All);
		}
	}

	private IEnumerable<IntVec3> AffectedCells(CellRect rect)
	{
		cellsTmp.Clear();
		int num = 0;
		foreach (IntVec3 cell in rect.Cells)
		{
			if (base.Map.pollutionGrid.IsPolluted(cell))
			{
				num++;
			}
		}
		if ((float)num / (float)rect.Area < MinPercentage)
		{
			yield break;
		}
		float numCellsToCover = NumCellsPerSectionPollutionLevel.Evaluate(num);
		for (int i = 0; (float)i < numCellsToCover; i++)
		{
			for (int j = 0; j < AttemptsPerCell; j++)
			{
				IntVec3 randomCell = rect.RandomCell;
				if (!base.Map.pollutionGrid.IsPolluted(randomCell) || base.Map.terrainGrid.TerrainAt(randomCell).IsWater)
				{
					continue;
				}
				bool flag = !cellsTmp.Contains(randomCell);
				if (flag)
				{
					foreach (IntVec3 item in cellsTmp)
					{
						if (!item.InHorDistOf(randomCell, MinCellDistance))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					cellsTmp.Add(randomCell);
					yield return randomCell;
					break;
				}
			}
		}
	}
}
