using UnityEditor;
using UnityEngine;
public enum BlockType
{
    // 일반 블록
    Apple,
    Banana,
    Orange,
    Grape,
    // 필요시 추가

    // 특수 블록
    RowClear, // 가로줄 제거 특수 블록
    ColClear,  // 세로줄 제거 특수 블록
    Bomb,     //3x3 제거
    Lightning, //가로,세로줄 전체 제거
}
public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public BlockType blockType;
    public SpriteRenderer spriteRenderer;

    public bool isSpecial = false;  // 특수 블록 체크
    public bool isRowClear = false; // true : 가로줄 , false : 세로줄

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
        {
            spriteRenderer.color = isRowClear ? Color.red : Color.cyan;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    void OnMouseDown()
    {
        gridManager.SelectBlock(this);
    }
}
