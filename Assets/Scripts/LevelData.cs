using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "3Match/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level 1";
    [Tooltip("Resources/Masks/ 경로 (확장자 제외)")]
    public string maskResourcePath = "Masks/Mask_Level01";
    public int goalScore = 1000;
    public float timeLimit = 60f;
}
