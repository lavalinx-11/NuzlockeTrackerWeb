namespace NuzlockeTrackerWeb.Components.GameData;

public class Character
{
    public string Name { get; set; }
    public string Series { get; set; } 
    public bool IsLost { get; set; }
    public bool IsBanned { get; set; }
    public bool IsSelected { get; set; }
    public int ID { get; set; }
    
    public Character(string name, string series, int id)
    {
        Name = name;
        Series = series;
        ID = id; 
        IsLost = false;
        IsBanned = false;
        IsSelected = false;
    }

    public override bool Equals(object? obj) => obj is Character other && other.ID == ID;
    public override int GetHashCode() => ID.GetHashCode();
    public override string ToString() => $"{Name} ({Series})";
}