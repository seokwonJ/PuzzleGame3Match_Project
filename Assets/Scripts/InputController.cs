using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Camera cam;
    private Block selectedBlock;
    private bool hasSwapped = false;
    private BoardManager boardManager;
    public bool isCanClick = true;

    private void Awake()
    {
        cam = Camera.main;
        boardManager = FindObjectOfType<BoardManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isCanClick && !boardManager.isShaking && !GameManager.Instance.isPaused)
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                selectedBlock = hit.GetComponent<Block>();
                if (selectedBlock.type == 6)
                {
                    selectedBlock = null;
                    hasSwapped = false;
                    return;
                }
                hasSwapped = false;
            }
        }

        if (Input.GetMouseButton(0) && selectedBlock != null && !hasSwapped)
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                Block hoverBlock = hit.GetComponent<Block>();
                if (hoverBlock.type == 6)
                {
                    selectedBlock = null;
                    hasSwapped = false;
                    return;
                }
                if (hoverBlock != null && hoverBlock != selectedBlock)
                {
                    hasSwapped = true;
                    StartCoroutine(HandleSwap(selectedBlock, hoverBlock));
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedBlock = null;
            hasSwapped = false;
        }
    }

    private IEnumerator HandleSwap(Block a, Block b)
    {
        SwapBlocks(a, b);

        isCanClick = false;

        yield return new WaitForSeconds(0.2f);

        List<Block> matched = boardManager.FindMatches();

        if (matched.Count > 0)
        {
            boardManager.RemoveBlocks(matched);
            GameManager.Instance.AddMoveCount(1);

            yield return new WaitForSeconds(0.3f);

            boardManager.DropBlocks();
            boardManager.FillEmptySlots();

            yield return new WaitForSeconds(0.3f);

            // 연쇄 매치 반복
            List<Block> chain = boardManager.FindMatches();
            while (chain.Count > 0)
            {
                boardManager.RemoveBlocks(chain);
                yield return new WaitForSeconds(0.3f);
                boardManager.DropBlocks();
                boardManager.FillEmptySlots();
                chain = boardManager.FindMatches();
                yield return new WaitForSeconds(0.3f);
            }
            if (!boardManager.HasPossibleMove())
            {
                boardManager.ShuffleBoard();
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            // 매치 없으면 원위치로 복귀
            SwapBlocks(a, b);
        }

        isCanClick = true;
    }

    private void SwapBlocks(Block a, Block b)
    {
        var board = boardManager.GetBoard();


        // 1️⃣ 실제 위치 교환
        Vector3 tempPos = a.transform.position;
        a.transform.position = b.transform.position;
        b.transform.position = tempPos;

        // 2️⃣ 논리적 좌표 교환 (row, col)
        int tempRow = a.row;
        int tempCol = a.col;

        a.SetPosition(b.row, b.col);
        b.SetPosition(tempRow, tempCol);

        // 3️⃣ 보드 배열 갱신
        board[a.row, a.col] = a;
        board[b.row, b.col] = b;

        Debug.Log($"Swapped ({a.row},{a.col}) <-> ({b.row},{b.col})");
    }
}