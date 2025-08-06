namespace Source.Common.Formats.Keyvalues.Abstraction
{
	interface IParsingVisitationListener : IVisitationListener
	{
		void DiscardCurrentObject();

		IParsingVisitationListener GetMergeListener();

		IParsingVisitationListener GetAppendListener();
	}
}
