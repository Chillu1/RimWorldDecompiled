using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class FocusStrengthOffset_BuildingDefs : FocusStrengthOffset
{
	public List<MeditationFocusOffsetPerBuilding> defs = new List<MeditationFocusOffsetPerBuilding>();

	public float radius = 10f;

	public int maxBuildings = int.MaxValue;

	public float offsetPerBuilding;

	public bool drawRingRadius = true;

	[NoTranslate]
	public string explanationKey;

	[NoTranslate]
	public string explanationKeyAbstract;

	protected float minOffsetPerBuilding = float.MaxValue;

	protected float maxOffsetPerBuilding = float.MinValue;

	public float MaxOffsetPerBuilding
	{
		get
		{
			if (maxOffsetPerBuilding == float.MinValue)
			{
				for (int i = 0; i < defs.Count; i++)
				{
					MeditationFocusOffsetPerBuilding meditationFocusOffsetPerBuilding = defs[i];
					if (meditationFocusOffsetPerBuilding.offset > maxOffsetPerBuilding)
					{
						maxOffsetPerBuilding = meditationFocusOffsetPerBuilding.offset;
					}
				}
			}
			return maxOffsetPerBuilding;
		}
	}

	public float MinOffsetPerBuilding
	{
		get
		{
			if (minOffsetPerBuilding == float.MaxValue)
			{
				for (int i = 0; i < defs.Count; i++)
				{
					MeditationFocusOffsetPerBuilding meditationFocusOffsetPerBuilding = defs[i];
					if (meditationFocusOffsetPerBuilding.offset < minOffsetPerBuilding)
					{
						minOffsetPerBuilding = meditationFocusOffsetPerBuilding.offset;
					}
				}
			}
			return minOffsetPerBuilding;
		}
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		if (parent.Spawned)
		{
			float num = 0f;
			List<Thing> forCell = parent.Map.listerBuldingOfDefInProximity.GetForCell(parent.Position, radius, defs, parent);
			for (int i = 0; i < forCell.Count && i < maxBuildings; i++)
			{
				num += OffsetForBuilding(forCell[i]);
			}
			return num;
		}
		return 0f;
	}

	protected virtual int BuildingCount(Thing parent)
	{
		if (parent == null || !parent.Spawned)
		{
			return 0;
		}
		return Math.Min(parent.Map.listerBuldingOfDefInProximity.GetForCell(parent.Position, radius, defs, parent).Count, maxBuildings);
	}

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		if (parent.Spawned)
		{
			return BuildingCount(parent) > 0;
		}
		return false;
	}

	protected virtual float OffsetForBuilding(Thing b)
	{
		return OffsetFor(b.def);
	}

	public override void PostDrawExtraSelectionOverlays(Thing parent, Pawn user = null)
	{
		base.PostDrawExtraSelectionOverlays(parent, user);
		if (drawRingRadius)
		{
			GenDraw.DrawRadiusRing(parent.Position, radius, PlaceWorker_MeditationOffsetBuildingsNear.RingColor);
		}
		List<Thing> forCell = parent.Map.listerBuldingOfDefInProximity.GetForCell(parent.Position, radius, defs, parent);
		for (int i = 0; i < forCell.Count && i < maxBuildings; i++)
		{
			GenDraw.DrawLineBetween(parent.TrueCenter(), forCell[i].TrueCenter(), SimpleColor.Green);
		}
	}

	public override string GetExplanation(Thing parent)
	{
		if (!parent.Spawned)
		{
			return GetExplanationAbstract(parent.def);
		}
		int num = BuildingCount(parent);
		return explanationKey.Translate(num, maxBuildings, MinOffsetPerBuilding.ToString("0%"), MaxOffsetPerBuilding.ToString("0%"), NamedArgumentUtility.Named(defs[0].building, "BUILDING")) + ": " + GetOffset(parent).ToStringWithSign("0%");
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return explanationKeyAbstract.Translate(maxBuildings, MinOffsetPerBuilding.ToString("0%"), MaxOffsetPerBuilding.ToString("0%"), NamedArgumentUtility.Named(defs[0].building, "BUILDING")) + ": +0-" + MaxOffset().ToString("0%");
	}

	public override float MaxOffset(Thing parent = null)
	{
		return (float)maxBuildings * MaxOffsetPerBuilding;
	}

	protected float OffsetFor(ThingDef def)
	{
		return defs.FirstOrDefault((MeditationFocusOffsetPerBuilding d) => d.building == def)?.offset ?? 0f;
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		foreach (MeditationFocusOffsetPerBuilding def in defs)
		{
			if (def.offset == float.MinValue)
			{
				def.offset = offsetPerBuilding;
			}
		}
	}
}
