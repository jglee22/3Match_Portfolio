using UnityEngine;

public enum BlockType
{
    Apple,
    Banana,
    Orange,
    Grape,

    RowClear,
    ColClear,
    Bomb,
    Lightning,
}

public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public BlockType blockType;
    public SpriteRenderer spriteRenderer;

    public bool isSpecial;
    public bool isRowClear;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    public void SetType(BlockType type, Sprite sprite)
    {
        blockType = type;
        spriteRenderer.sprite = sprite;

        if (isSpecial)
            spriteRenderer.color = isRowClear ? Color.red : Color.cyan;
        else
            spriteRenderer.color = Color.white;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        gridManager.SelectBlock(this);
    }
}
