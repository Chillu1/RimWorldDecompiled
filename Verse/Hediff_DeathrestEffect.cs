using UnityEngine;

namespace Verse
{
	public class Hediff_DeathrestEffect : Hediff
	{
		public override string LabelBase => base.LabelBase + " x" + Mathf.FloorToInt(Severity);
	}
}
