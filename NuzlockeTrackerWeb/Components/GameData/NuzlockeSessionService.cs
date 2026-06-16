
namespace NuzlockeTrackerWeb.Components.GameData
{
    public enum MatchStage
    {
        Home,
        Setup,
        CharacterSelect,
        Game,
    }
    //  MATCH DATA 
    public class MatchResult {
        public int Id { get; init; } 
        public DateTime MatchDate { get; init; } = DateTime.UtcNow; 
        public string Team1Key { get; init; } = "";
        public string Team2Key { get; init; } = "";
        public List<string> Team1Names { get; init; } = new();
        public List<string> Team2Names { get; init; } = new();
        public List<string> Team1Roster { get; init; } = new();
        public List<string> Team2Roster { get; init; } = new();
        public List<string> BanList { get; init; } = new();
        public int Team1Rounds { get; init; }
        public int Team2Rounds { get; init; }
        public int WinningTeamSide { get; init; }
    }
    
    //  Live Match Data (Used While Playing Actively) 
    public class GameSession {
        public string Id { get; set; } = Guid.NewGuid().ToString()[..6].ToUpper();
        public MatchStage Stage { get; set; } = MatchStage.Home;
        public List<Player> Team1 { get; set; } = new();
        public List<Player> Team2 { get; set; } = new();
        public List<Player> PickingOrder { get; set; } = new();
        public List<string> CurrentBans { get; set; } = new();
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<MatchResult> MatchHistory { get; set; } = new();
        public int CurrentPickerIndex { get; set; }
        public int WinningTeamNumber { get; set; }
        public bool HistoryRecorded { get; set; }
        public string RivalryText { get; set; } = "";
        public List<Player> UnassignedPlayers { get; set; } = new();
        public event Action? OnChange;
        public string HostId { get; set; } = ""; 
        public void Notify() => OnChange?.Invoke();
    }

    public class NuzlockeSessionService {
        public Dictionary<string, GameSession> Sessions { get; } = new();
        public GameSession Create() {
            GameSession session = new GameSession();
            Sessions[session.Id] = session;
            return session;
        }
    }
    
    
}