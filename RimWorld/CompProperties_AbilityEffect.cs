using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityEffect : AbilityCompProperties
	{
		public int goodwillImpact;

		public bool psychic;

		public bool applicableToMechs = true;

		public bool applyGoodwillImpactToLodgers = true;

		public ClamorDef clamorType;

		public int clamorRadius;

		public float screenShakeIntensity;

		public SoundDef sound;

		public string customLetterLabel;

		public string customLetterText;

		public bool sendLetter = true;

		public string message;

		public MessageTypeDef messageType;

		public float weight = 1f;

		public bool availableWhenTargetIsWounded = true;
	}
}
