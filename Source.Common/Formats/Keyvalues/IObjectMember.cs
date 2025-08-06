using System;

namespace Source.Common.Formats.Keyvalues
{
	interface IObjectMember
	{
		bool IsExplicitName { get; }

		string Name { get; }

		Type MemberType { get; }

		object Value { get; set; }
	}
}
