namespace Source.Common;

public enum NetChannelGroup
{
	Generic = 0,    // must be first and is default group
	LocalPlayer,    // bytes for local player entity update
	OtherPlayers,   // bytes for other players update
	Entities,       // all other entity bytes
	Sounds,         // game sounds
	Events,         // event messages
	UserMessage,   // user messages
	EntMessage,    // entity messages
	Voice,          // voice data
	StringTable,    // a stringtable update
	Move,           // client move cmds
	StringCmd,      // string command
	SignOn,         // various signondata
	Total,          // must be last and is not a real group
};
