using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class JobDef : Def
	{
		public Type driverClass;

		[MustTranslate]
		public string reportString = "Doing something.";

		public bool playerInterruptible = true;

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

		public bool makeTargetPrisoner;

		public int waitAfterArriving;

		public int joyDuration = 4000;

		public int joyMaxParticipants = 1;

		public float joyGainRate = 1f;

		public SkillDef joySkill;

		public float joyXpPerTick;

		public JoyKindDef joyKind;

		public Rot4 faceDir = Rot4.Invalid;

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
}
