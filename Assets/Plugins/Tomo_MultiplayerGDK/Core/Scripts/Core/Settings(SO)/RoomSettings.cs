using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Room Settings")]
public class RoomSettings : ScriptableObject
{
    [Header("Create Room Settings")]
	[Tooltip("Min to Maximum amount of players that the room can hold for this game")]
	public Vector2Int totalPlayersRange;
	[Tooltip("Min to Maximum amount of arenas that the room can hold for this game")]
	public Vector2Int totalArenaRange;
	[Tooltip("Min to Maximum amount of players that each arena can hold for this game")]
	public Vector2Int perArenaPlayersRange;

}
