using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string LobbySceneName = "Lobby";
    public const string GameSceneName = "Main";

    public static void LoadLobby()
    {
        SceneManager.LoadScene(LobbySceneName);
    }

    public static void LoadGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }
}
