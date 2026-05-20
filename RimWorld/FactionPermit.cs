using Verse;

namespace RimWorld;

public class FactionPermit : IExposable
{
	private RoyalTitleDef title;

	private RoyalTitlePermitDef permit;

	private Faction faction;

	private int lastUsedTick = -1;

	public RoyalTitleDef Title => title;

	public RoyalTitlePermitDef Permit => permit;

	public Faction Faction => faction;

	public int LastUsedTick => lastUsedTick;

	public bool OnCooldown
	{
		get
		{
			if (LastUsedTick > 0)
			{
				return Find.TickManager.TicksGame < LastUsedTick + permit.CooldownTicks;
			}
			return false;
		}
	}

	public FactionPermit()
	{
	}

	public FactionPermit(Faction faction, RoyalTitleDef title, RoyalTitlePermitDef permit)
	{
		this.title = title;
		this.faction = faction;
		this.permit = permit;
	}

	public void Notify_Used()
	{
		lastUsedTick = Find.TickManager.TicksGame;
	}

	public void ResetCooldown()
	{
		lastUsedTick = -1;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_Defs.Look(ref title, "title");
		Scribe_Defs.Look(ref permit, "permit");
		Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -1);
	}
}
