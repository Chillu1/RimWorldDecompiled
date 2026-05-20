using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StunHandler : IExposable
{
	public Thing parent;

	private Pawn parentPawn;

	private int stunTicksLeft;

	private Mote moteStun;

	private bool showStunMote = true;

	private Effecter empEffecter;

	private bool stunFromEMP;

	private Dictionary<DamageDef, int> adaptationTicksLeft;

	private bool disableRotation;

	private CompStunnable stunnableComp;

	public const float StunDurationTicksPerDamage = 30f;

	private static readonly List<KeyValuePair<DamageDef, int>> tmpAdaptationTicksLeft = new List<KeyValuePair<DamageDef, int>>();

	public bool Stunned
	{
		get
		{
			if (stunTicksLeft <= 0)
			{
				return Hypnotized;
			}
			return true;
		}
	}

	public bool StunFromEMP => stunFromEMP;

	public int StunTicksLeft => stunTicksLeft;

	public bool DisableRotation
	{
		get
		{
			if (!disableRotation)
			{
				return Hypnotized;
			}
			return true;
		}
	}

	public bool Hypnotized
	{
		get
		{
			if (ModsConfig.AnomalyActive && parentPawn != null)
			{
				return Find.Anomaly.IsPawnHypnotized(parentPawn);
			}
			return false;
		}
	}

	public StunHandler(Thing parent)
	{
		this.parent = parent;
		parentPawn = parent as Pawn;
		stunnableComp = parent.TryGetComp<CompStunnable>();
		adaptationTicksLeft = new Dictionary<DamageDef, int>();
	}

	public void StunHandlerTick()
	{
		if (adaptationTicksLeft.Count > 0)
		{
			tmpAdaptationTicksLeft.Clear();
			tmpAdaptationTicksLeft.AddRange(adaptationTicksLeft);
			foreach (KeyValuePair<DamageDef, int> item in tmpAdaptationTicksLeft)
			{
				item.Deconstruct(out var key, out var value);
				DamageDef damageDef = key;
				if (value > 0)
				{
					Dictionary<DamageDef, int> dictionary = adaptationTicksLeft;
					key = damageDef;
					value = dictionary[key]--;
				}
			}
		}
		if (stunTicksLeft > 0)
		{
			stunTicksLeft--;
			if (showStunMote && (moteStun == null || moteStun.Destroyed) && !Hypnotized)
			{
				moteStun = MoteMaker.MakeStunOverlay(parent);
			}
			if (parent is Pawn { Downed: not false })
			{
				stunTicksLeft = 0;
			}
			moteStun?.Maintain();
			if (!stunFromEMP)
			{
				return;
			}
			if (empEffecter == null)
			{
				if (stunnableComp != null && stunnableComp.UseLargeEMPEffecter)
				{
					empEffecter = CreateLargeEffecter();
				}
				else
				{
					empEffecter = EffecterDefOf.DisabledByEMP.Spawn();
				}
			}
			empEffecter.EffectTick(parent, parent);
		}
		else
		{
			if (empEffecter != null)
			{
				empEffecter.Cleanup();
				empEffecter = null;
				stunFromEMP = false;
			}
			disableRotation = false;
		}
	}

	public void Notify_DamageApplied(DamageInfo dinfo)
	{
		if (!CanBeStunnedByDamage(dinfo.Def))
		{
			return;
		}
		float num = ((float?)dinfo.Def.constantStunDurationTicks) ?? (dinfo.Amount * 30f);
		if (CanAdaptToDamage(dinfo.Def))
		{
			if (((adaptationTicksLeft.TryGetValue(dinfo.Def, out var value) && value != 0) ? 1 : 0) <= (false ? 1 : 0))
			{
				adaptationTicksLeft.SetOrAdd(dinfo.Def, dinfo.Def.stunAdaptationTicks);
			}
			else
			{
				if (dinfo.Def.displayAdaptedTextMote)
				{
					MoteMaker.ThrowText(new Vector3((float)parent.Position.x + 1f, parent.Position.y, (float)parent.Position.z + 1f), text: dinfo.Def.adaptedText ?? ((string)"Adapted".Translate()), map: parent.Map, color: Color.white);
				}
				num = 0f;
			}
		}
		if (num > 0f)
		{
			if (dinfo.Def.stunResistStat != null && !dinfo.Def.stunResistStat.Worker.IsDisabledFor(parent))
			{
				num *= Mathf.Clamp01(1f - parent.GetStatValue(dinfo.Def.stunResistStat));
			}
			if (dinfo.Def == DamageDefOf.EMP)
			{
				stunFromEMP = true;
			}
			StunFor(Mathf.RoundToInt(num), dinfo.Instigator);
		}
	}

	public void StunFor(int ticks, Thing instigator, bool addBattleLog = true, bool showMote = true, bool disableRotation = false)
	{
		stunTicksLeft = Mathf.Max(stunTicksLeft, ticks);
		showStunMote = showMote;
		this.disableRotation = disableRotation;
		if (addBattleLog)
		{
			Find.BattleLog.Add(new BattleLogEntry_Event(parent, RulePackDefOf.Event_Stun, instigator));
		}
	}

	public void StopStun()
	{
		stunTicksLeft = 0;
	}

	private Effecter CreateLargeEffecter()
	{
		Effecter effecter = EffecterDefOf.DisabledByEMPLarge.Spawn();
		for (int i = 0; i < effecter.children.Count; i++)
		{
			if (effecter.children[i] is SubEffecter_SprayerChance subEffecter_SprayerChance)
			{
				if (stunnableComp.EMPEffecterDimensions.HasValue || stunnableComp.EMPEffecterOffset.HasValue)
				{
					subEffecter_SprayerChance.spawnLocOverride = MoteSpawnLocType.OnSource;
				}
				subEffecter_SprayerChance.dimensionsOverride = stunnableComp.EMPEffecterDimensions;
				subEffecter_SprayerChance.offsetOverride = stunnableComp.EMPEffecterOffset;
				subEffecter_SprayerChance.chanceOverride = stunnableComp.EMPChancePerTick;
			}
		}
		return effecter;
	}

	private bool CanBeStunnedByDamage(DamageDef def)
	{
		if (!def.causeStun)
		{
			return false;
		}
		if (stunnableComp != null && !stunnableComp.CanBeStunnedByDamage(def))
		{
			return false;
		}
		if (parent is Pawn pawn)
		{
			if (pawn.Downed || pawn.Dead)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && pawn.health.hediffSet.HasHediff(HediffDefOf.AwokenCorpse))
			{
				return false;
			}
			if (def == DamageDefOf.Stun)
			{
				return true;
			}
			if (def == DamageDefOf.EMP && !pawn.RaceProps.IsFlesh)
			{
				return true;
			}
			if (ModsConfig.BiotechActive && def == DamageDefOf.MechBandShockwave && pawn.RaceProps.IsMechanoid)
			{
				return true;
			}
			if (def == DamageDefOf.NerveStun && !pawn.RaceProps.IsMechanoid)
			{
				return true;
			}
			return false;
		}
		if (def == DamageDefOf.NerveStun)
		{
			return false;
		}
		return true;
	}

	private bool CanAdaptToDamage(DamageDef def)
	{
		if (def.stunAdaptationTicks <= 0)
		{
			return false;
		}
		if (stunnableComp != null && stunnableComp.CanAdaptToDamage(def))
		{
			return true;
		}
		if (parent is Pawn pawn)
		{
			if (def == DamageDefOf.EMP)
			{
				if (ModsConfig.AnomalyActive && (pawn.kindDef == PawnKindDefOf.Revenant || pawn.kindDef == PawnKindDefOf.Nociosphere))
				{
					return true;
				}
				if (!pawn.RaceProps.IsMechanoid)
				{
					return pawn.RaceProps.IsDrone;
				}
				return true;
			}
			if (ModsConfig.AnomalyActive && def == DamageDefOf.NerveStun)
			{
				if (!pawn.RaceProps.IsMechanoid)
				{
					return !pawn.RaceProps.IsDrone;
				}
				return false;
			}
		}
		return false;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref stunTicksLeft, "stunTicksLeft", 0);
		Scribe_Values.Look(ref showStunMote, "showStunMote", defaultValue: false);
		Scribe_Values.Look(ref stunFromEMP, "stunFromEMP", defaultValue: false);
		Scribe_Values.Look(ref disableRotation, "disableRotation", defaultValue: false);
		Scribe_Collections.Look(ref adaptationTicksLeft, "adaptationTicksLeft", LookMode.Def, LookMode.Value);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (adaptationTicksLeft == null)
			{
				adaptationTicksLeft = new Dictionary<DamageDef, int>();
			}
			int value = 0;
			Scribe_Values.Look(ref value, "EMPAdaptedTicksLeft", 0);
			if (value != 0)
			{
				adaptationTicksLeft.SetOrAdd(DamageDefOf.EMP, value);
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			adaptationTicksLeft.RemoveAll((KeyValuePair<DamageDef, int> x) => x.Value <= 0);
		}
	}
}
