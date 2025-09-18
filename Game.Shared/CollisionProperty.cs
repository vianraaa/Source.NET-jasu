#if CLIENT_DLL
using Game.Client;
#endif

#if GAME_DLL
using Game.Server;
#endif

using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Game.Shared;

public class CollisionProperty
{
#if CLIENT_DLL
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_BaseEntity ret = new C_BaseEntity();
		ret.Init(entNum, serialNum);
		return ret;
	}

	public static RecvTable DT_CollisionProperty = new([]);
	public static readonly ClientClass CC_CollisionProperty = new ClientClass("CollisionProperty", CreateObject, null, DT_CollisionProperty);
#else
	public static SendTable DT_CollisionProperty = new([]);
	public static readonly ServerClass CC_CollisionProperty = new ServerClass("CollisionProperty", DT_CollisionProperty)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);
#endif
}
