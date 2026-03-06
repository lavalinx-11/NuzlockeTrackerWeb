namespace NuzlockeTrackerWeb.Components.GameData;

public class Player
{
    public string Name { get; set; } = "";
    public int TeamNumber { get; set; } 
    public bool IsLocked { get; set; } = false; // Add this
    public List<Character> Roster { get; set; } = new();
    public bool IsEliminated => Roster.Count > 0 && Roster.All(c => c.IsLost);
    public bool IsRosterFull => Roster.Count >= 5;
    public string ClaimedBySessionId { get; set; } = "";

    // Fixed: Finds the character by name and marks them as lost
    public void LoseCharacter(string charName)
    {
        var character = Roster.FirstOrDefault(c => c.Name.Equals(charName, StringComparison.OrdinalIgnoreCase));
        if (character != null)
        {
            character.IsLost = true;
        }
    }

    // Helper for the "Max 2 per Series" rule
    public int GetSeriesCount(string seriesName)
    {
        return Roster.Count(c => c.Series.Equals(seriesName, StringComparison.OrdinalIgnoreCase));
    }
}