namespace BeatTogether.DedicatedServer.Interface.Enums
{
    public enum GameplayState : byte
    {
        None = 0,
        SceneLoad = 1,
        SongLoad = 2,
        Gameplay = 3,
        Results = 4
    }
}
