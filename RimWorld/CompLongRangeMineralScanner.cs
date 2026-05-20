using System.Collections.Generic;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompLongRangeMineralScanner : CompScanner
{
	private ThingDef targetMineable;

	public new CompProperties_LongRangeMineralScanner Props => props as CompProperties_LongRangeMineralScanner;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Defs.Look(ref targetMineable, "targetMineable");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && targetMineable == null)
		{
			SetDefaultTargetMineral();
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		SetDefaultTargetMineral();
	}

	private void SetDefaultTargetMineral()
	{
		targetMineable = ThingDefOf.MineableGold;
	}

	protected override void DoFind(Pawn worker)
	{
		Slate slate = new Slate();
		slate.Set("map", parent.Map);
		slate.Set("targetMineable", targetMineable);
		slate.Set("targetMineableThing", targetMineable.building.mineableThing);
		slate.Set("worker", worker);
		if (QuestScriptDefOf.LongRangeMineralScannerLump.CanRun(slate, parent.Map))
		{
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.LongRangeMineralScannerLump, slate);
			Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item2 in base.CompGetGizmosExtra())
		{
			yield return item2;
		}
		if (parent.Faction != Faction.OfPlayer)
		{
			yield break;
		}
		ThingDef mineableThing = targetMineable.building.mineableThing;
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + mineableThing.LabelCap;
		command_Action.defaultDesc = "CommandSelectMineralToScanForDesc".Translate();
		command_Action.icon = mineableThing.uiIcon;
		command_Action.iconAngle = mineableThing.uiIconAngle;
		command_Action.iconOffset = mineableThing.uiIconOffset;
		command_Action.action = delegate
		{
			List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingDef item3 in mineables)
			{
				ThingDef localD = item3;
				FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate
				{
					foreach (object selectedObject in Find.Selector.SelectedObjects)
					{
						if (selectedObject is Thing thing)
						{
							CompLongRangeMineralScanner compLongRangeMineralScanner = thing.TryGetComp<CompLongRangeMineralScanner>();
							if (compLongRangeMineralScanner != null)
							{
								compLongRangeMineralScanner.targetMineable = localD;
							}
						}
					}
				}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing));
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		};
		yield return command_Action;
	}
}
