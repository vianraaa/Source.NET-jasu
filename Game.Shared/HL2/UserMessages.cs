namespace Game.Shared;

public partial class UserMessages {
	public UserMessages() {
		RegisterUsermessages();
	}
	public void RegisterUsermessages() {
		Register("Geiger", 1);
		Register("Train", 1);
		Register("HudText", -1);
		Register("SayText", -1);
		Register("SayText2", -1);
		Register("TextMsg", -1);
		Register("HudMsg", -1);
		Register("ResetHUD", 1);
		Register("GameTitle", 0);
		Register("ItemPickup", -1);
		Register("ShowMenu", -1);
		Register("Shake", 0xd);
		Register("Fade", 10);
		Register("VGUIMenu", -1);
		Register("Rumble", 3);
		Register("Battery", 2);
		Register("Damage", 0x12);
		Register("VoiceMask", 0x21);
		Register("RequestState", 0);
		Register("CloseCaption", -1);
		Register("HintText", -1);
		Register("KeyHintText", -1);
		Register("SquadMemberDied", 0);
		Register("AmmoDenied", 2);
		Register("CreditsMsg", 1);
		Register("LogoTimeMsg", 4);
		Register("AchievementEvent", -1);
		Register("UpdateJalopyRadar", -1);
		Register("CallVoteFailed", -1);
		Register("VoteStart", -1);
		Register("VotePass", -1);
		Register("VoteFailed", 2);
		Register("VoteSetup", -1);
		Register("SPHapWeapEvent", 4);
		Register("HapDmg", -1);
		Register("HapPunch", -1);
		Register("HapSetDrag", -1);
		Register("HapSetConst", -1);
		Register("HapMeleeContact", 0);
		Register("LuaUserMessage", -1);
		Register("LuaCmd", -1);
		Register("SWEPCmd", -1);
		Register("AmmoPickup", -1);
		Register("WeaponPickup", -1);
		Register("NetworkedVar", -1);
		Register("BreakModel", -1);
		Register("CheapBreakModel", -1);
	}
}