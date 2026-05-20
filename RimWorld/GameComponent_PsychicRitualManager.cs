using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GameComponent_PsychicRitualManager : GameComponent
{
	private Dictionary<PsychicRitualDef, int> ritualCooldowns = new Dictionary<PsychicRitualDef, int>();

	private List<KeyValuePair<PsychicRitualDef, int>> availableRitualDefs = new List<KeyValuePair<PsychicRitualDef, int>>();

	private List<PsychicRitualDef> tmpRitualDefs;

	private List<int> tmpCooldowns;

	public GameComponent_PsychicRitualManager(Game game)
	{
	}

	public override void GameComponentTick()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		availableRitualDefs = ritualCooldowns.Where((KeyValuePair<PsychicRitualDef, int> cd) => cd.Value <= Find.TickManager.TicksGame).ToList();
		foreach (KeyValuePair<PsychicRitualDef, int> availableRitualDef in availableRitualDefs)
		{
			ritualCooldowns.Remove(availableRitualDef.Key);
			if (availableRitualDef.Key.researchPrerequisite != null && availableRitualDef.Key.researchPrerequisite.IsFinished)
			{
				Messages.Message("PsychicRitualCanBeCastAgain".Translate(availableRitualDef.Key.label), MessageTypeDefOf.PositiveEvent);
			}
		}
	}

	public void RegisterCooldown(PsychicRitualDef psychicRitualDef)
	{
		ritualCooldowns[psychicRitualDef] = Find.TickManager.TicksGame + psychicRitualDef.cooldownHours * 2500;
	}

	public void ClearCooldown(PsychicRitualDef psychicRitualDef)
	{
		ritualCooldowns.Remove(psychicRitualDef);
	}

	public int GetAvailableTick(PsychicRitualDef psychicRitualDef)
	{
		if (!ritualCooldowns.ContainsKey(psychicRitualDef))
		{
			return Find.TickManager.TicksGame;
		}
		return ritualCooldowns[psychicRitualDef];
	}

	public AcceptanceReport CanInvoke(PsychicRitualDef psychicRitualDef, Map map)
	{
		if (!psychicRitualDef.castableOnPocketMaps && map.IsPocketMap)
		{
			return new AcceptanceReport("CantCastOnPocketMap".Translate(psychicRitualDef.label).Resolve());
		}
		if (map.Tile.Valid && !psychicRitualDef.layerWhitelist.NullOrEmpty() && !psychicRitualDef.layerWhitelist.Contains(map.Tile.LayerDef))
		{
			return new AcceptanceReport("CannotPerformPlanetLayer".Translate(map.Tile.LayerDef.gerundLabel.Named("GERUND"), map.Tile.LayerDef.label.Named("LAYER")).EndWithPeriod().Resolve());
		}
		if (ritualCooldowns.TryGetValue(psychicRitualDef, out var value))
		{
			if (Find.TickManager.TicksGame >= value)
			{
				return true;
			}
			return new AcceptanceReport("PsychicRitualOnCooldown".Translate((GetAvailableTick(psychicRitualDef) - Find.TickManager.TicksGame).ToStringTicksToPeriod()).Resolve());
		}
		return true;
	}

	public void ClearAllCooldowns()
	{
		ritualCooldowns.Clear();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref ritualCooldowns, "ritualCooldowns", LookMode.Def, LookMode.Value, ref tmpRitualDefs, ref tmpCooldowns);
	}
}
