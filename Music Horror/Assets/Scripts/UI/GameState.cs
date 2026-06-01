public static class GameState
{
    // Global states
    public static bool IsPaused = false;
    public static bool InputLocked = false;
    public static bool IsGameOver = false;
    public static bool IsInCutscene = false;
    public static bool InventoryOpen = false;

    // Helper property
    public static bool GameplayBlocked =>
        IsPaused ||
        InputLocked ||
        IsGameOver ||
        IsInCutscene;
}