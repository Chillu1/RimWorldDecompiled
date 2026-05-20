using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class JobDef : Def
{
	public Type driverClass;

	[MustTranslate]
	public string reportString = "Doing something.";

	public bool playerInterruptible = true;

	public bool forceCompleteBeforeNextJob;

	public CheckJobOverrideOnDamageMode checkOverrideOnDamage = CheckJobOverrideOnDamageMode.Always;

	public bool alwaysShowWeapon;

	public bool neverShowWeapon;

	public bool suspendable = true;

	public bool casualInterruptible = true;

	public bool allowOpportunisticPrefix;

	public bool collideWithPawns;

	public bool isIdle;

	public TaleDef taleOnCompletion;

	public bool neverFleeFromEnemies;

	public bool sleepCanInterrupt = true;

	public bool makeTargetPrisoner;

	public int waitAfterArriving;

	public bool carryThingAfterJob;

	public bool dropThingBeforeJob = true;

	public bool isCrawlingIfDowned = true;

	public bool alwaysShowReport;

	public bool abilityCasting;

	public bool tryStartFlying;

	public bool ifFlyingKeepFlying;

	public float overrideFlyChance = -1f;

	public bool displayAsAreaInFloatMenu = true;

	public bool ignoreFenceBlocked;

	public int joyDuration = 4000;

	public int joyMaxParticipants = 1;

	public float joyGainRate = 1f;

	public SkillDef joySkill;

	public float joyXpPerTick;

	public JoyKindDef joyKind;

	public Rot4 faceDir = Rot4.Invalid;

	public int learningDuration = 20000;

	public ReservationLayerDef containerReservationLayer;

	public bool boardingGravship;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (joySkill != null && joyXpPerTick == 0f)
		{
			yield return "funSkill is not null but funXpPerTick is zero";
		}
	}
}
