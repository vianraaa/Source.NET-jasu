
#if CLIENT_DLL
namespace Game.Client;
#else
namespace Game.Server;
#endif

public partial class
#if CLIENT_DLL
	C_BaseEntity
#else
	BaseEntity
#endif
{

}

