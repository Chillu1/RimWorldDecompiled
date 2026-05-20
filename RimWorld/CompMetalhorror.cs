using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompMetalhorror : ThingComp
{
	public ImplantSource implantSource;

	public Pawn emergedFrom;

	private Pawn lastDamagedBy;

	private int filthTimer;

	private int speedBoostTimer;

	private CompCanBeDormant intDormantComp;

	private LordJob_Metalhorror intLordJob;

	private static readonly IntRange DropFilthTicksRange = new IntRange(600, 1800);

	private static readonly IntRange SpeedBoostTicksRange = new IntRange(1200, 2400);

	private const int FilthSearchRadius = 4;

	private const int MaxFilth = 12;

	private static readonly List<IntVec3> filthOptions = new List<IntVec3>();

	public CompCanBeDormant DormantComp => intDormantComp ?? (intDormantComp = parent.GetComp<CompCanBeDormant>());

	public LordJob_Metalhorror LordJob => intLordJob ?? (intLordJob = (LordJob_Metalhorror)Pawn.GetLord().LordJob);

	public bool Hibernating => DormantComp.Awake;

	public int Biosignature => implantSource?.Biosignature ?? 0;

	public string BiosignatureName => implantSource?.BiosignatureName ?? "INVALID";

	public CompProperties_Metalhorror Props => (CompProperties_Metalhorror)props;

	public Pawn Pawn => (Pawn)parent;

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (!(totalDamageDealt <= 0f) && dinfo.Instigator is Pawn pawn)
		{
			lastDamagedBy = pawn;
		}
	}

	public override void PostPostMake()
	{
		speedBoostTimer = SpeedBoostTicksRange.RandomInRange;
	}

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		FilthMaker.TryMakeFilth(Pawn.PositionHeld, prevMap, ThingDefOf.Filth_MetalhorrorDebris);
	}

	public void FindOrCreateEmergedLord()
	{
		foreach (Lord lord2 in Pawn.MapHeld.lordManager.lords)
		{
			if (lord2.LordJob is LordJob_Metalhorror lordJob_Metalhorror && lordJob_Metalhorror.biosignature == implantSource.Biosignature)
			{
				lord2.AddPawn(Pawn);
				intLordJob = (LordJob_Metalhorror)lord2.LordJob;
				return;
			}
		}
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_Metalhorror(), Pawn.MapHeld);
		intLordJob = (LordJob_Metalhorror)lord.LordJob;
		intLordJob.biosignature = implantSource.Biosignature;
		lord.AddPawn(Pawn);
	}

	public override void CompTick()
	{
		if (!Hibernating)
		{
			FilthTick();
			SpeedBoostTick();
		}
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
		float amount = 0.2f * dinfo.Amount;
		if (dinfo.Def == DamageDefOf.Burn)
		{
			amount = 2f * dinfo.Amount;
		}
		dinfo.SetAmount(amount);
	}

	public override string CompInspectStringExtra()
	{
		if (implantSource == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (emergedFrom != null)
		{
			stringBuilder.AppendLine(string.Format("{0}: {1}", "EmergedFrom".Translate(), emergedFrom.LabelShort));
		}
		stringBuilder.Append(implantSource.GetSourceDesc().CapitalizeFirst() ?? "");
		if (Prefs.DevMode && DebugSettings.godMode && Pawn.GetLord() != null)
		{
			stringBuilder.Append($"\nDEV: Biosignature: {BiosignatureName} ({Pawn.GetLord().ownedPawns.Count - 1} siblings)");
		}
		return stringBuilder.ToString();
	}

	private void SpeedBoostTick()
	{
		speedBoostTimer--;
		if (speedBoostTimer <= 0)
		{
			speedBoostTimer = SpeedBoostTicksRange.RandomInRange;
			if (!Pawn.health.hediffSet.HasHediff(HediffDefOf.MetalhorrorSpeedBoost))
			{
				Pawn.health.AddHediff(HediffDefOf.MetalhorrorSpeedBoost);
			}
		}
	}

	private void FilthTick()
	{
		if (!Pawn.Spawned)
		{
			return;
		}
		filthTimer--;
		if (filthTimer > 0)
		{
			return;
		}
		filthOptions.Clear();
		filthTimer = DropFilthTicksRange.RandomInRange;
		int found = 0;
		Pawn.Map.floodFiller.FloodFill(Pawn.Position, delegate(IntVec3 x)
		{
			if (!x.WalkableBy(Pawn.Map, Pawn))
			{
				return false;
			}
			if (x.InHorDistOf(Pawn.Position, 4f))
			{
				return false;
			}
			return !PawnUtility.AnyPawnBlockingPathAt(x, Pawn, actAsIfHadCollideWithPawnsJob: true);
		}, delegate(IntVec3 cell)
		{
			List<Thing> thingList = cell.GetThingList(Pawn.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == ThingDefOf.Filth_GrayFleshNoticeable || thingList[i].def == ThingDefOf.Filth_GrayFlesh)
				{
					found++;
					if (found >= 12)
					{
						return true;
					}
				}
				else
				{
					filthOptions.Add(cell);
				}
			}
			return false;
		});
		if (found < 12 && filthOptions.Any())
		{
			FilthMaker.TryMakeFilth(filthOptions.RandomElement(), Pawn.Map, ThingDefOf.Filth_GrayFlesh);
		}
		filthOptions.Clear();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref filthTimer, "filthTimer", 0);
		Scribe_Values.Look(ref speedBoostTimer, "speedBoostTimer", 0);
		Scribe_References.Look(ref lastDamagedBy, "lastDamagedBy");
		Scribe_References.Look(ref emergedFrom, "emergedFrom", saveDestroyedThings: true);
		Scribe_Deep.Look(ref implantSource, "implantSource");
	}
}
