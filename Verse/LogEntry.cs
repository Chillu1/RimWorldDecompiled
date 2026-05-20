using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

[StaticConstructorOnStartup]
public abstract class LogEntry : IExposable, ILoadReferenceable
{
	protected int logID;

	protected int ticksAbs = -1;

	public LogEntryDef def;

	private WeakReference<Thing> cachedStringPov;

	private string cachedString;

	private float cachedHeightWidth;

	private float cachedHeight;

	public static readonly Texture2D Blood = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Blood");

	public static readonly Texture2D BloodTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/BloodTarget");

	public static readonly Texture2D Downed = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Downed");

	public static readonly Texture2D DownedTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/DownedTarget");

	public static readonly Texture2D Skull = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Skull");

	public static readonly Texture2D SkullTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/SkullTarget");

	public int Age => Find.TickManager.TicksAbs - ticksAbs;

	public int Tick => ticksAbs;

	public int LogID => logID;

	public int Timestamp => ticksAbs;

	public LogEntry(LogEntryDef def = null)
	{
		ticksAbs = Find.TickManager.TicksAbs;
		this.def = def;
		if (Scribe.mode == LoadSaveMode.Inactive)
		{
			logID = Find.UniqueIDsManager.GetNextLogID();
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref ticksAbs, "ticksAbs", 0);
		Scribe_Values.Look(ref logID, "logID", 0);
		Scribe_Defs.Look(ref def, "def");
	}

	public string ToGameStringFromPOV(Thing pov, bool forceLog = false)
	{
		if (cachedString == null || pov == null != (cachedStringPov == null) || (cachedStringPov != null && pov != cachedStringPov.Target) || DebugViewSettings.logGrammarResolution || forceLog)
		{
			Rand.PushState();
			try
			{
				Rand.Seed = logID;
				cachedStringPov = ((pov != null) ? new WeakReference<Thing>(pov) : null);
				cachedString = ToGameStringFromPOV_Worker(pov, forceLog);
				cachedHeightWidth = 0f;
				cachedHeight = 0f;
			}
			finally
			{
				Rand.PopState();
			}
		}
		return cachedString;
	}

	protected virtual string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
	{
		return GrammarResolver.Resolve("r_logentry", GenerateGrammarRequest(), null, forceLog);
	}

	protected virtual GrammarRequest GenerateGrammarRequest()
	{
		return default(GrammarRequest);
	}

	public float GetTextHeight(Thing pov, float width)
	{
		string text = ToGameStringFromPOV(pov);
		if (cachedHeightWidth != width)
		{
			cachedHeightWidth = width;
			cachedHeight = Text.CalcHeight(text, width);
		}
		return cachedHeight;
	}

	protected void ResetCache()
	{
		cachedStringPov = null;
		cachedString = null;
		cachedHeightWidth = 0f;
		cachedHeight = 0f;
	}

	public abstract bool Concerns(Thing t);

	public abstract IEnumerable<Thing> GetConcerns();

	public virtual bool CanBeClickedFromPOV(Thing pov)
	{
		return false;
	}

	public virtual void ClickedFromPOV(Thing pov)
	{
	}

	public virtual Texture2D IconFromPOV(Thing pov)
	{
		return null;
	}

	public virtual Color? IconColorFromPOV(Thing pov)
	{
		return null;
	}

	public virtual void Notify_FactionRemoved(Faction faction)
	{
	}

	public virtual void Notify_IdeoRemoved(Ideo ideo)
	{
	}

	public virtual string GetTipString()
	{
		return "OccurredTimeAgo".Translate(Age.ToStringTicksToPeriod()).CapitalizeFirst() + ".";
	}

	public virtual bool ShowInCompactView()
	{
		return true;
	}

	public void Debug_OverrideTicks(int newTicks)
	{
		ticksAbs = newTicks;
	}

	public string GetUniqueLoadID()
	{
		return $"LogEntry_{ticksAbs}_{logID}";
	}
}
