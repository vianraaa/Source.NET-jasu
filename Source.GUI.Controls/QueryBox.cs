using Source.Common.Formats.Keyvalues;

namespace Source.GUI.Controls;

public class QueryBox : MessageBox
{
	public QueryBox(string title, string queryText, Panel? parent = null) : base(title, queryText, parent) {
	}
}
