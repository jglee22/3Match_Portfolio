using UnityEngine;
public enum BlockType
{
    Apple,
    Banana,
    Orange,
    Grape,
    // 필요시 추가
}
public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public BlockType blockType;
    public SpriteRenderer spriteRenderer;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    public void SetType(BlockType type, Sprite sprite)
    {
        blockType = type;
        spriteRenderer.sprite = sprite;
    }

    void OnMouseDown()
    {
        gridManager.SelectBlock(this);
    }
}
