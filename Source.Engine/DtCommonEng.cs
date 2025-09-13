
using Source.Common;
using Source.Common.Server;
using Source.Engine.Client;

namespace Source.Engine;

public class DtCommonEng(Host Host, IServerGameDLL serverGameDLL)
{
	internal void CreateClientTablesFromServerTables() {

	}

	internal void CreateClientClassInfosFromServerClasses(BaseClientState state) {
		ServerClass? classes = serverGameDLL.GetAllServerClasses();

		state.NumServerClasses = Constants.TEMP_TOTAL_SERVER_CLASSES;
		state.ServerClasses = new C_ServerClassInfo[state.NumServerClasses];

		for (ServerClass? svclass = classes; svclass != null; svclass = svclass.Next) {
			state.ServerClasses[svclass.ClassID].ClassName = svclass.NetworkName;
			state.ServerClasses[svclass.ClassID].DatatableName = new(svclass.Table.GetName());
		}
	}
}