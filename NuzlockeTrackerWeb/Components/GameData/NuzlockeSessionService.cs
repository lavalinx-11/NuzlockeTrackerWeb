using System;
using System.Collections.Generic;
using System.Linq;

namespace NuzlockeTrackerWeb.Components.GameData
{
    // --- SECTION: DATA MODELS ---
    public class MatchResult {
        public string Team1Key { get; set; } = "";
        public string Team2Key { get; set; } = "";
        public List<string> Team1Names { get; set; } = new();
        public List<string> Team2Names { get; set; } = new();
        public List<string> Team1Roster { get; set; } = new();
        public List<string> Team2Roster { get; set; } = new();
        public List<string> BanList { get; set; } = new();
        public int Team1Rounds { get; set; }
        public int Team2Rounds { get; set; }
        public int WinningTeamSide { get; set; }
    }

    public class PlayerStat {
        public string Name { get; set; } = "";
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int CurrentStreak { get; set; }
        public List<bool> RecentForm { get; set; } = new();
        public int Ratio => (Wins + Losses) == 0 ? 0 : (int)((double)Wins / (Wins + Losses) * 100);
    }

    public class TeamStat {
        public string TeamComposition { get; set; } = "";
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ratio => (Wins + Losses) == 0 ? 0 : (int)((double)Wins / (Wins + Losses) * 100);
    }

    // --- SECTION: SHARED SESSION ---
    public class GameSession {
        public string Id { get; set; } = Guid.NewGuid().ToString()[..6].ToUpper();
        public string View { get; set; } = "menu";
        public List<Player> Team1 { get; set; } = new();
        public List<Player> Team2 { get; set; } = new();
        public List<Player> PickingOrder { get; set; } = new();
        public List<string> CurrentBans { get; set; } = new();
        public List<MatchResult> MatchHistory { get; set; } = new();
        public int CurrentPickerIndex { get; set; } = 0;
        public int WinningTeamNumber { get; set; } = 0;
        public bool HistoryRecorded { get; set; } = false;
        public string RivalryText { get; set; } = "";
        public List<Player> UnassignedPlayers { get; set; } = new();
        public event Action OnChange;
        public string HostId { get; set; } = ""; 
        public void Notify() => OnChange?.Invoke();
    }

    public class NuzlockeSessionService {
        public Dictionary<string, GameSession> Sessions { get; } = new();
        public GameSession Create() {
            var s = new GameSession();
            Sessions[s.Id] = s;
            return s;
        }
    }
}