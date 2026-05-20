using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class RitualObligation : IExposable, ILoadReferenceable
{
	public Precept_Ritual precept;

	private int triggeredTick;

	public bool sendLetter = true;

	public bool expires = true;

	public TargetInfo targetA;

	public TargetInfo targetB;

	public TargetInfo targetC;

	public List<Pawn> onlyForPawns;

	public int ID;

	public static readonly int[] StageDays = new int[2] { 3, 9 };

	private StringBuilder sbTemp = new StringBuilder();

	private List<string> unfilledRolesTemp = new List<string>();

	public bool StillValid
	{
		get
		{
			if (expires)
			{
				return TicksUntilExpiration >= 0;
			}
			return true;
		}
	}

	public static int DaysToExpire => StageDays[StageDays.Length - 1];

	public int ActiveForTicks => Find.TickManager.TicksGame - triggeredTick;

	public int TicksUntilExpiration => triggeredTick + DaysToExpire * 60000 - Find.TickManager.TicksGame;

	public TargetInfo FirstValidTarget
	{
		get
		{
			if (targetA.IsValid)
			{
				return targetA;
			}
			if (targetB.IsValid)
			{
				return targetB;
			}
			if (targetC.IsValid)
			{
				return targetC;
			}
			return TargetInfo.Invalid;
		}
	}

	public string LetterLabel
	{
		get
		{
			string text = precept.obligationTargetFilter.LabelExtraPart(this);
			if (text.NullOrEmpty())
			{
				return "RitualOpportunity".Translate(precept.LabelCap);
			}
			return "RitualOpportunityFor".Translate(precept.LabelCap, text);
		}
	}

	public TaggedString LetterText
	{
		get
		{
			Precept_Ritual precept_Ritual = precept;
			sbTemp.Clear();
			Pawn arg = targetA.Thing as Pawn;
			if (targetA.Thing is Corpse corpse)
			{
				arg = corpse.InnerPawn;
			}
			RitualObligationTrigger_Date ritualObligationTrigger_Date = precept_Ritual.obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault();
			TaggedString taggedString = ((precept_Ritual.ideo.adjective.NullOrEmpty() && !precept_Ritual.ritualExpectedDescNoAdjective.NullOrEmpty()) ? precept_Ritual.ritualExpectedDescNoAdjective : precept_Ritual.ritualExpectedDesc).Formatted(precept_Ritual.ideo.Named("IDEOLIGION"), precept_Ritual.Named("RITUAL"), arg.Named("PAWN"), precept_Ritual.ideo.memberName.Colorize(precept_Ritual.ideo.TextColor).Named("MEMBER"), precept_Ritual.ideo.MemberNamePlural.Colorize(precept_Ritual.ideo.TextColor).Named("MEMBERS"), (precept_Ritual.ideo.classicMode ? Faction.OfPlayer.Name : precept_Ritual.ideo.name).Colorize(precept_Ritual.ideo.TextColor).Named("IDEO"), ritualObligationTrigger_Date?.DateString.Named("DATE") ?? ((NamedArgument)""));
			sbTemp.AppendLineTagged(taggedString);
			sbTemp.AppendLine();
			IEnumerable<string> targetInfos = precept_Ritual.obligationTargetFilter.GetTargetInfos(this);
			if (targetInfos.Count() > 1)
			{
				if ("RitualMultiTargetDesc".CanTranslate())
				{
					sbTemp.AppendLine("RitualMultiTargetDesc".Translate(precept_Ritual.Named("RITUAL")) + ":\n" + targetInfos.ToLineList("  - ", capitalizeItems: true));
				}
				else
				{
					sbTemp.AppendLine("RitualTargetsExplanation".Translate(precept_Ritual.Named("RITUAL"), targetInfos.ToLineList("  - ", capitalizeItems: true)).CapitalizeFirst());
				}
			}
			else if ("RitualSingleTargetDesc".CanTranslate())
			{
				sbTemp.AppendLine("RitualSingleTargetDesc".Translate(precept_Ritual.Named("RITUAL"), targetInfos.First().UncapitalizeFirst().Named("FOCUS")));
			}
			else
			{
				sbTemp.AppendLine("RitualTargetsExplanation".Translate(precept_Ritual.Named("RITUAL"), targetInfos.ToLineList("  - ", capitalizeItems: true)).CapitalizeFirst());
			}
			if (expires)
			{
				Map map = ((Current.ProgramState == ProgramState.Playing) ? Find.AnyPlayerHomeMap : null);
				string text = GenDate.QuadrumDateStringAt(Find.TickManager.TicksAbs + StageDays[StageDays.Length - 1] * 60000, (map != null) ? Find.WorldGrid.LongLatOf(map.Tile).x : 0f);
				sbTemp.AppendLine();
				sbTemp.AppendLineTagged("RitualExpiresIn".Translate((StageDays[StageDays.Length - 1] * 60000).ToStringTicksToPeriodVague(), text));
			}
			unfilledRolesTemp.Clear();
			foreach (RitualRole item in precept_Ritual.behavior.def.RequiredRoles())
			{
				Precept_Role precept_Role = item.FindInstance(precept_Ritual.ideo);
				if (!item.substitutable && precept_Role != null && precept_Role.Active && precept_Role.ChosenPawnSingle() == null)
				{
					unfilledRolesTemp.Add(precept_Role.LabelCap);
				}
			}
			if (unfilledRolesTemp.Any())
			{
				sbTemp.AppendLine();
				sbTemp.AppendLine();
				sbTemp.AppendLine("RitualUnfilledRoles".Translate(precept_Ritual.LabelCap) + ":");
				sbTemp.Append(unfilledRolesTemp.ToLineList("  - "));
			}
			return sbTemp.ToString().TrimEndNewlines();
		}
	}

	public RitualObligation()
	{
	}

	public RitualObligation(Precept_Ritual precept, TargetInfo targetA, TargetInfo targetB, TargetInfo targetC)
	{
		this.precept = precept;
		this.targetA = targetA;
		this.targetB = targetB;
		this.targetC = targetC;
		triggeredTick = Find.TickManager.TicksGame;
		ID = Find.UniqueIDsManager.GetNextRitualObligationID();
	}

	public RitualObligation(Precept_Ritual precept, TargetInfo targetA, TargetInfo targetB, bool expires = true)
		: this(precept, targetA, targetB, TargetInfo.Invalid)
	{
		this.expires = expires;
	}

	public RitualObligation(Precept_Ritual precept, TargetInfo targetA, bool expires = true)
		: this(precept, targetA, TargetInfo.Invalid, TargetInfo.Invalid)
	{
		this.expires = expires;
	}

	public RitualObligation(Precept_Ritual precept, bool expires = true)
		: this(precept, TargetInfo.Invalid, TargetInfo.Invalid, TargetInfo.Invalid)
	{
		this.expires = expires;
	}

	public void DebugOffsetTriggeredTick(int ticks)
	{
		triggeredTick += ticks;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref triggeredTick, "triggeredTick", 0);
		Scribe_Values.Look(ref sendLetter, "sendLetter", defaultValue: false);
		Scribe_Values.Look(ref expires, "expires", defaultValue: false);
		Scribe_TargetInfo.Look(ref targetA, saveDestroyedThings: true, "targetA");
		Scribe_TargetInfo.Look(ref targetB, saveDestroyedThings: true, "targetB");
		Scribe_TargetInfo.Look(ref targetC, saveDestroyedThings: true, "targetC");
		Scribe_Collections.Look(ref onlyForPawns, "onlyForPawns", LookMode.Reference);
		Scribe_Values.Look(ref ID, "ID", -1);
	}

	public virtual void CopyTo(RitualObligation other)
	{
		other.ID = ID;
		other.precept = precept;
		other.triggeredTick = triggeredTick;
		other.sendLetter = sendLetter;
		other.expires = expires;
		other.targetA = targetA;
		other.targetB = targetB;
		other.targetC = targetC;
		other.onlyForPawns = onlyForPawns;
	}

	public string GetUniqueLoadID()
	{
		return "RitualObligation_" + ID;
	}
}
