using System.Text;
using Verse;

namespace RimWorld
{
	public class ExternalHistory : IExposable
	{
		public string gameVersion = "?";

		public string gameplayID = "?";

		public string userName = "?";

		public string storytellerName = "?";

		public string realWorldDate = "?";

		public string firstUploadDate = "?";

		public int firstUploadTime;

		public bool devMode;

		public History history = new History();

		public static string defaultUserName = "Anonymous";

		public string AllInformation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("storyteller: ");
				stringBuilder.Append(storytellerName);
				stringBuilder.Append("   userName: ");
				stringBuilder.Append(userName);
				stringBuilder.Append("   realWorldDate(UTC): ");
				stringBuilder.Append(realWorldDate);
				return stringBuilder.ToString();
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref gameVersion, "gameVersion");
			Scribe_Values.Look(ref gameplayID, "gameplayID");
			Scribe_Values.Look(ref userName, "userName");
			Scribe_Values.Look(ref storytellerName, "storytellerName");
			Scribe_Values.Look(ref realWorldDate, "realWorldDate");
			Scribe_Values.Look(ref firstUploadDate, "firstUploadDate");
			Scribe_Values.Look(ref firstUploadTime, "firstUploadTime", 0);
			Scribe_Values.Look(ref devMode, "devMode", defaultValue: false);
			Scribe_Deep.Look(ref history, "history");
		}
	}
}
