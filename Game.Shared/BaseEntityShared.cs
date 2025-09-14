
#if CLIENT_DLL
namespace Game.Client;
#else
namespace Game.Server;
#endif

[Flags]
public enum EntityCapabilities : uint
{
	MustSpawn = 0x00000001,
	AcrossTransition = 0x00000002,
	ForceTransition = 0x00000004,
	NotifyOnTransition = 0x00000008,
	ImpulseUse = 0x00000010,
	ContinuousUse = 0x00000020,
	OnOffUse = 0x00000040,
	DirectionalUse = 0x00000080,
	UseOnGround = 0x00000100,
	UseInRadius = 0x00000200,
	SaveNonNetworkable = 0x00000400,
	Master = 0x10000000,
	WCEditPosition = 0x40000000,
	DontSave = 0x80000000
}

public enum InvalidatePhysicsBits
{
	PositionChanged = 0x1,
	AnglesChanged = 0x2,
	VelocityChanged = 0x4,
	AnimationChanged = 0x8,
}

public partial class
#if CLIENT_DLL
	C_BaseEntity
#else
	BaseEntity
#endif
{

}

