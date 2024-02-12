namespace TomoClub.Core
{
	public enum PlayerType { Master_Spectator, Master_Player, Spectator, Player }
	public enum RoomState { Null, Unassigned_Arenas, Assigned_Arenas };
	public enum TeamName { Red, Purple, None };

	public enum BuildType { Moderator, Player, Common, Spectator }

	public enum Regions { Best, US_West, US_East, India, Europe, Turkey, Singapore, Australia, Japan, South_Korea, Canada, South_Africa, Brazil }

	public enum GameStates { Null, MainMenu, RoomLobby, TeamAssign, InGame }

	public enum GameType { InterArena, IntraArena };

	public enum AuthenticationType { Device_Based, Name_Based }



}
