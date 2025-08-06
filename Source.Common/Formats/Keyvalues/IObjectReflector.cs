using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues
{
	interface IObjectReflector
	{
		IEnumerable<IObjectMember> GetMembers(object @object);
	}
}
