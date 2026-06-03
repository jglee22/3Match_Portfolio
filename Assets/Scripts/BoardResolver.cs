using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>제거 → 낙하·리필 대기 → 연쇄 재검사 루프 (코루틴)</summary>
public class BoardResolver : MonoBehaviour
{
    [SerializeField] float removeDelay = 0.25f;
    [SerializeField] float settleDelay = 0.35f;

    GridManager grid;
    MatchFinder matchFinder;
    SpecialBlockHandler specialHandler;

    public void Initialize(GridManager gridManager, MatchFinder finder, SpecialBlockHandler special)
    {
        grid = gridManager;
        matchFinder = finder;
        specialHandler = special;
    }

    public void StartCascade()
    {
        StopAllCoroutines();
        StartCoroutine(CascadeRoutine());
    }

    IEnumerator CascadeRoutine()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            {
                grid.SetProcessing(false);
                yield break;
            }

            List<Block> matches = matchFinder.FindAllMatches();
            if (matches.Count == 0)
            {
                grid.ResetChainCount();
                grid.SetProcessing(false);
                grid.CheckDeadlockAndShuffle();
                yield break;
            }

            if (matches.Count < 3)
            {
                Debug.LogWarning($"[BoardResolver] 매치 수 부족({matches.Count}) — 연쇄 중단");
                grid.SetProcessing(false);
                yield break;
            }

            grid.IncrementChainCount();

            SpecialSpawnPlan specialPlan = specialHandler.PlanSpecialSpawn(
                matches,
                grid.ChainCount,
                grid.LastMovedBlock,
                matchFinder);

            ScoreManager.Instance.AddScore(matches.Count * 10);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.matchSFX);

            foreach (Block block in matches)
                grid.ClearBlockAt(block.x, block.y);

            yield return new WaitForSeconds(removeDelay);

            grid.FillEmptySpaces();

            yield return new WaitForSeconds(settleDelay + grid.RefillFallDuration);

            if (specialPlan.hasSpecial)
                yield return grid.PlaySpecialSpawnTransition(specialPlan);

            yield return new WaitForSeconds(settleDelay * 0.3f);
        }
    }

    public void ContinueAfterSpecial()
    {
        StopAllCoroutines();
        StartCoroutine(AfterSpecialRoutine());
    }

    IEnumerator AfterSpecialRoutine()
    {
        yield return new WaitForSeconds(settleDelay);
        StartCascade();
    }
}
