using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class AncientHatch : MapPortal
{
	public TileMutatorWorker_Stockpile.StockpileType stockpileType;

	public LayoutDef layout;

	private CompHackable hackableInt;

	private GraphicData openGraphicData;

	private const string OpenTexturePath = "Things/Building/AncientHatch/AncientHatch_Open";

	private CompHackable Hackable => hackableInt ?? (hackableInt = GetComp<CompHackable>());

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref stockpileType, "stockpileType", TileMutatorWorker_Stockpile.StockpileType.Medicine);
		Scribe_Defs.Look(ref layout, "layout");
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		openGraphicData = new GraphicData();
		openGraphicData.CopyFrom(def.graphicData);
		openGraphicData.texPath = "Things/Building/AncientHatch/AncientHatch_Open";
	}

	public override void Print(SectionLayer layer)
	{
		if (IsEnterable(out var _))
		{
			openGraphicData.Graphic.Print(layer, this, 0f);
		}
		else
		{
			Graphic.Print(layer, this, 0f);
		}
	}

	protected override IEnumerable<GenStepWithParams> GetExtraGenSteps()
	{
		if (layout != null)
		{
			yield return new GenStepWithParams(GenStepDefOf.AncientStockpile, new GenStepParams
			{
				layout = layout
			});
		}
		else
		{
			yield return new GenStepWithParams(GenStepDefOf.AncientStockpile, default(GenStepParams));
		}
	}

	public override bool IsEnterable(out string reason)
	{
		if (!Hackable.IsHacked)
		{
			reason = "Locked".Translate();
			return false;
		}
		return base.IsEnterable(out reason);
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
		if (Hackable.IsHacked)
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.Append("HatchUnlocked".Translate());
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
	}
}
