namespace Verse
{
	public class ManeuverDef : Def
	{
		public ToolCapacityDef requiredCapacity;

		public VerbProperties verb;

		public RulePackDef combatLogRulesHit;

		public RulePackDef combatLogRulesDeflect;

		public RulePackDef combatLogRulesMiss;

		public RulePackDef combatLogRulesDodge;

		public LogEntryDef logEntryDef;
	}
}
