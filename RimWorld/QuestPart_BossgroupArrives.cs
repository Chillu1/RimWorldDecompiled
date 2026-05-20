using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_BossgroupArrives : QuestPartActivable
{
	private const int CheckInterval = 2500;

	public int minDelay;

	public int maxDelay;

	public MapParent mapParent;

	public BossgroupDef bossgroupDef;

	public override void QuestPartTick()
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (enableTick + minDelay <= ticksGame)
		{
			if (enableTick + maxDelay <= ticksGame)
			{
				Complete();
			}
			else if (mapParent.IsHashIntervalTick(2500) && (bool)bossgroupDef.Worker.ShouldSummonNow(mapParent.Map))
			{
				Complete();
			}
		}
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		if (base.State == QuestPartState.Enabled)
		{
			int num = enableTick + minDelay - Find.TickManager.TicksGame;
			int numTicks = enableTick + maxDelay - Find.TickManager.TicksGame;
			string text = "";
			if (num >= 0)
			{
				text = text + "\nTicks until available: " + num + " (" + num.ToStringTicksToPeriodVerbose() + ")";
			}
			text = text + "\nTicks until forced arrival: " + numTicks + " (" + numTicks.ToStringTicksToPeriodVerbose() + ")";
			text = text + "\nShould summon now (blocked by visitors etc.): " + (bool)bossgroupDef.Worker.ShouldSummonNow(mapParent.Map);
			Vector2 vector = Text.CalcSize(text);
			Rect rect = new Rect(innerRect.x, curY, innerRect.width, vector.y);
			Widgets.Label(rect, text);
			curY += rect.height + 4f;
			Rect rect2 = new Rect(innerRect.x, curY, 500f, 25f);
			if (Widgets.ButtonText(rect2, "End " + this))
			{
				Complete();
			}
			curY += rect2.height;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref minDelay, "minDelay", 0);
		Scribe_Values.Look(ref maxDelay, "maxDelay", 0);
		Scribe_Defs.Look(ref bossgroupDef, "bossgroupDef");
	}
}
