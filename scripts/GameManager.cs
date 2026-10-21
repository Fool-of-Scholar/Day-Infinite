using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    // --- Signals ---
    [Signal] public delegate void StoredScrapsChangedEventHandler(int newTotal);

    // Persistent City Resources
    [Export] public int StoredScraps { get; set; } = 0;

    // Facility Unlock Flags (Unlocked via Boss Blueprints)
    public bool FacilityUpgradeUnlocked { get; set; } = false;
    public bool FacilityEncyclopediaUnlocked { get; set; } = false;
    public bool FacilityResourceUnlocked { get; set; } = false;

    // Design Architect Selection ("Damage", "Dodge", "Armor")
    public string ActiveArchitect { get; set; } = "None";

    // Encyclopedia Data (Tracks enemy names encountered)
    public HashSet<string> DiscoveredEnemies { get; set; } = new HashSet<string>();

    // Weapon Unlock System (AR, Mine, Force Field unlocked by default; Rocket & Tesla locked)
    public HashSet<string> UnlockedWeapons { get; set; } = new HashSet<string>()
    {
        "ar", "mine", "force_field"
    };

    public override void _Ready()
    {
        Instance = this;
    }

    // --- Currency Methods ---
    public void AddScraps(int amount)
    {
        StoredScraps += amount;
        EmitSignal(SignalName.StoredScrapsChanged, StoredScraps);
        GD.Print($"[GameManager] Stored Scraps Updated: {StoredScraps}");
    }

    public bool SpendScraps(int amount)
    {
        if (StoredScraps >= amount)
        {
            StoredScraps -= amount;
            EmitSignal(SignalName.StoredScrapsChanged, StoredScraps);
            GD.Print($"[GameManager] Spent {amount} scraps. Remaining: {StoredScraps}");
            return true;
        }
        
        GD.PrintErr("[GameManager] Not enough scraps to make this purchase!");
        return false;
    }

    // --- Weapon Unlock Methods ---
    public bool IsWeaponUnlocked(string weaponId)
    {
        return UnlockedWeapons.Contains(weaponId);
    }

    public void UnlockWeapon(string weaponId)
    {
        if (!UnlockedWeapons.Contains(weaponId))
        {
            UnlockedWeapons.Add(weaponId);
            GD.Print($"[GameManager] Weapon Unlocked: {weaponId}");
        }
    }

    // --- Encyclopedia Methods ---
    public void RegisterEnemyEncountered(string enemyName)
    {
        if (!DiscoveredEnemies.Contains(enemyName))
        {
            DiscoveredEnemies.Add(enemyName);
            GD.Print($"[Encyclopedia] New enemy logged: {enemyName}");
        }
    }
}