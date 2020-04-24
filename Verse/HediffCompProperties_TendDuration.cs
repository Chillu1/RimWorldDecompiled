using UnityEngine;

namespace Verse
{
	public class HediffCompProperties_TendDuration : HediffCompProperties
	{
		private float baseTendDurationHours = -1f;

		private float tendOverlapHours = 3f;

		public bool tendAllAtOnce;

		public int disappearsAtTotalTendQuality = -1;

		public float severityPerDayTended;

		public bool showTendQuality = true;

		[LoadAlias("labelTreatedWell")]
		public string labelTendedWell;

		[LoadAlias("labelTreatedWellInner")]
		public string labelTendedWellInner;

		[LoadAlias("labelSolidTreatedWell")]
		public string labelSolidTendedWell;

		public bool TendIsPermanent => baseTendDurationHours < 0f;

		public int TendTicksFull
		{
			get
			{
				if (TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksFull on permanent-tend Hediff.", 6163263);
				}
				return Mathf.RoundToInt((baseTendDurationHours + tendOverlapHours) * 2500f);
			}
		}

		public int TendTicksBase
		{
			get
			{
				if (TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksBase on permanent-tend Hediff.", 61621263);
				}
				return Mathf.RoundToInt(baseTendDurationHours * 2500f);
			}
		}

		public int TendTicksOverlap
		{
			get
			{
				if (TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksOverlap on permanent-tend Hediff.", 1963263);
				}
				return Mathf.RoundToInt(tendOverlapHours * 2500f);
			}
		}

		public HediffCompProperties_TendDuration()
		{
			compClass = typeof(HediffComp_TendDuration);
		}
	}
}
