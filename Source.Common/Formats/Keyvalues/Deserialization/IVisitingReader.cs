using System;

namespace Source.Common.Formats.Keyvalues.Deserialization
{
	interface IVisitingReader : IDisposable
	{
		void ReadObject();
	}
}
