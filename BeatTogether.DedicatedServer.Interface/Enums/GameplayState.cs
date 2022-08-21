namespace BeatTogether.DedicatedServer.Interface.Enums
{
    public enum GameplayState : byte
    {
        None = 0,
        SceneLoad = 1,
        SongLoad = 1,
        Gameplay = 2,
        Results = 3
    }
}
