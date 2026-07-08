namespace NuzlockeTrackerWeb.Components.GameData;

public enum CharacterTier
{
    TakeOver,
    Z,
    S,
    Null
}
public class Character
{
    public string Name { get; set; } 
    public string Series { get; set; } 
    public bool IsLost { get; set; } 
    public bool IsBanned { get; set; } 
    public bool IsSelected { get; set; } 
    public int ID { get; set; } 
    
    public CharacterTier Tier { get; set; } 
    public Character(string name, string series, int id,  CharacterTier tier)
    {
        Name = name;
        Series = series;
        ID = id; 
        Tier = tier;
        IsLost = false;
        IsBanned = false;
        IsSelected = false;
    }
    
    public override bool Equals(object? obj) => obj is Character other && other.ID == ID;
    public override int GetHashCode() => ID.GetHashCode();
    public override string ToString() => $"{Name} ({Series})";
}