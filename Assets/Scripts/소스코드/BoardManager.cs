using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject backgroundBlocks;
    public GameObject[] blocks;
    public GameObject crown;

    private Block[,] board;

    public bool isShaking;

    private int[,] map = new int[,]
    {
        {1, 0, 1, 0 ,1, 0, 1},
        {0, 1, 0, 1, 0, 1, 0},
        {1, 0, 1, 0, 1, 0, 1},
        {0, 1, 0, 1, 0, 1, 0},
        {1, 0, 1, 0, 1, 0, 1},
        {0, 1, 0, 1, 0, 1, 0},
        {1, 0, 1, 0, 1, 0, 1},
        {0, 2, 0, 2, 0, 2, 0},
        {1, 0, 1, 0, 1, 0, 1}
    };

    public int rows => map.GetLength(0); // 세로
    public int cols => map.GetLength(1); // 가로

    private float cellWidth = 0.76f;
    private float cellHeight = 0.46f;

    private void Start()
    {
        board = new Block[rows, cols];
        InitializeBoard();
    }

    private void ClearBoard()
    {
        if (board == null)
            board = new Block[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col] != null)
                {
                    DestroyImmediate(board[row, col].gameObject);
                    board[row, col] = null;
                }
            }
        }
    }

    private void InitializeBoard()
    {
        do
        {
            // 🔹 1️⃣ 기존 보드 초기화
            ClearBoard();

            float boardWidth = (cols - 1) * cellWidth;
            float boardHeight = (rows - 1) * cellHeight;
            Vector3 offset = new Vector3(boardWidth / 2f, boardHeight / 2f, 0);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (map[row, col] != 0)
                    {
                        Vector3 pos = new Vector3(col * cellWidth, (rows - 1 - row) * cellHeight, 0);
                        pos -= offset;

                        Instantiate(backgroundBlocks, pos, Quaternion.identity);

                        if (map[row, col] == 1)
                        {
                            int randIndex = Random.Range(0, blocks.Length);
                            var obj = Instantiate(blocks[randIndex], pos, Quaternion.identity);
                            var block = obj.GetComponent<Block>();
                            block.SetPosition(row, col);
                            block.type = randIndex;
                            board[row, col] = block;
                        }
                        else if (map[row, col] == 2)
                        {
                            var obj = Instantiate(crown, pos, Quaternion.identity);
                            var block = obj.GetComponent<Block>();
                            block.SetPosition(row, col);
                            block.type = 6;
                            board[row, col] = block;
                        }
                    }
                }
            }

            // 🔹 2️⃣ 매치 검사
            var matches = FindMatches();

            // 🔹 3️⃣ 매치 있으면 다시 배치
            if (matches.Count > 0)
            {
                Debug.Log("⚠️ Initial match found — regenerating board...");
                foreach (var b in matches)
                {
                    if (b != null) DestroyImmediate(b.gameObject);
                }
            }
            else
            {
                Debug.Log("✅ Board initialized without matches!");
                break; // 정상 종료
            }

        } while (true);
    }

    // --------------------------
    // 🔹 매치 검사
    // --------------------------
    public List<Block> FindMatches()
    {
        List<Block> matched = new List<Block>();

        // 대각선 ↘ 검사
        for (int row = 0; row < rows - 2; row++)
        {
            for (int col = 0; col < cols - 2; col++)
            {
                Block a = board[row, col];
                Block b = board[row + 1, col + 1];
                Block c = board[row + 2, col + 2];
                if (a != null && b != null && c != null && a.type == b.type && b.type == c.type)
                {
                    matched.AddUnique(a, b, c);
                    Debug.Log($"[Match ↘] type {a.type} at ({a.row},{a.col}), ({b.row},{b.col}), ({c.row},{c.col})");
                }
            }
        }

        // 대각선 ↙ 검사
        for (int row = 0; row < rows - 2; row++)
        {
            for (int col = 2; col < cols; col++)
            {
                Block a = board[row, col];
                Block b = board[row + 1, col - 1];
                Block c = board[row + 2, col - 2];
                if (a != null && b != null && c != null && a.type == b.type && b.type == c.type)
                {
                    matched.AddUnique(a, b, c);
                    Debug.Log($"[Match ↙] type {a.type} at ({a.row},{a.col}), ({b.row},{b.col}), ({c.row},{c.col})");
                }
            }
        }

        // 세로 검사
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows - 4; row++)
            {
                Block a = board[row, col];
                Block b = board[row + 2, col];
                Block c = board[row + 4, col];
                if (a != null && b != null && c != null && a.type == b.type && b.type == c.type)
                {
                    matched.AddUnique(a, b, c);
                    Debug.Log($"[Match ↓] type {a.type} at ({a.row},{a.col}), ({b.row},{b.col}), ({c.row},{c.col})");
                }
            }
        }

        if (matched.Count > 0)
            Debug.Log($"✅ Total matched blocks: {matched.Count}");
        else
            Debug.Log("❌ No matches found.");

        return matched;
    }

    // --------------------------
    // 🔹 매치 블록 제거
    // --------------------------
    public void RemoveBlocks(List<Block> matched)
    {
        List<Block> crownList = new List<Block>();

        foreach (var b in matched)
        {
            // 주변 방향 정의 (위, 아래, 좌우, 대각선)
            int[,] dirs = new int[,]
            {
            {-2,  0}, // 위
            {+2,  0}, // 아래
            {-1, -1}, // ↖
            {-1, +1}, // ↗
            {+1, -1}, // ↙
            {+1, +1}, // ↘
            };

            for (int i = 0; i < dirs.GetLength(0); i++)
            {
                int nRow = b.row + dirs[i, 0];
                int nCol = b.col + dirs[i, 1];

                if (nRow >= 0 && nRow < rows && nCol >= 0 && nCol < cols)
                {
                    Block neighbor = board[nRow, nCol];
                    if (neighbor != null && neighbor.type == 6)
                    {
                        if (!crownList.Contains(neighbor)) crownList.Add(neighbor);
                        // 🔹 여기에 추가 행동 가능:
                        // e.g. neighbor 터뜨리기, 강화 효과, 폭발 등
                    }
                }
            }

            // 블록 제거
            board[b.row, b.col] = null;
            Destroy(b.gameObject);
        }

        if (crownList.Count != 0)
        {
           foreach (var b in crownList)
            {
                b.GetComponent<Block>().OpenCrown();
            }
            print("Found : " + crownList.Count);
            GameManager.Instance.AddScore(crownList.Count);
            crownList.Clear();
        }
    }



    // --------------------------
    // 🔹 블록 떨어뜨리기
    // --------------------------
    public void DropBlocks()
    {
        for (int col = 0; col < cols; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (board[row, col] == null && map[row, col] != 0)
                {
                    for (int upper = row - 1; upper >= 0; upper--)
                    {
                        if (board[upper, col] != null)
                        {
                            Block falling = board[upper, col];
                            board[row, col] = falling;
                            board[upper, col] = null;
                            falling.SetPosition(row, col);
                            StartCoroutine(MoveTo(falling.transform, GridToWorld(row, col), 0.15f));
                            break;
                        }
                    }
                }
            }
        }
    }

    // --------------------------
    // 🔹 빈칸 채우기
    // --------------------------
    public void FillEmptySlots()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col] == null && map[row, col] != 0)
                {
                    int randIndex = Random.Range(0, blocks.Length);
                    Vector3 spawnPos = GridToWorld(-1, col) + Vector3.up * 2f;
                    var obj = Instantiate(blocks[randIndex], spawnPos, Quaternion.identity);
                    var block = obj.GetComponent<Block>();
                    block.SetPosition(row, col);
                    block.type = randIndex;
                    board[row, col] = block;
                    StartCoroutine(MoveTo(block.transform, GridToWorld(row, col), 0.25f));
                }
            }
        }
    }

    // --------------------------
    // 🧭 좌표 변환
    // --------------------------
    private Vector3 GridToWorld(int row, int col)
    {
        float offsetX = -(cols - 1) * cellWidth / 2f;
        float offsetY = -(rows - 1) * cellHeight / 2f;
        return new Vector3(col * cellWidth + offsetX, (rows - 1 - row) * cellHeight + offsetY, 0);
    }

    // --------------------------
    // 🪄 부드러운 이동
    // --------------------------
    private IEnumerator MoveTo(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.position;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            t.position = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }
        t.position = target;
    }

    public bool HasPossibleMove()
    {
        // 상하좌우, 대각선 방향 등 실제 이동 가능한 방향 정의
        int[,] dirs = new int[,]
        {
        { -1, 0 }, { +1, 0 }, { 0, -1 }, { 0, +1 },
        { -1, -1 }, { -1, +1 }, { +1, -1 }, { +1, +1 }
        };

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Block a = board[row, col];
                if (a == null || a.type == 6) continue; // 빈칸, 왕관 제외

                for (int i = 0; i < dirs.GetLength(0); i++)
                {
                    int nRow = row + dirs[i, 0];
                    int nCol = col + dirs[i, 1];

                    if (nRow < 0 || nRow >= rows || nCol < 0 || nCol >= cols)
                        continue;

                    Block b = board[nRow, nCol];
                    if (b == null || b.type == 6) continue;

                    // 임시 스왑
                    board[row, col] = b;
                    board[nRow, nCol] = a;

                    bool hasMatch = CheckMatchAt(row, col) || CheckMatchAt(nRow, nCol);

                    // 원복
                    board[row, col] = a;
                    board[nRow, nCol] = b;

                    if (hasMatch)
                        return true; // 하나라도 가능한 이동이 있으면 OK
                }
            }
        }

        return false; // 아무 이동으로도 매치 안 생김
    }
    public void ShuffleBoard()
    {
        StartCoroutine(ShuffleUntilValid());
    }

    private IEnumerator ShuffleUntilValid()
    {
        int shuffleCount = 0;
        isShaking = true;
        do
        {
            shuffleCount++;

            List<Block> allBlocks = new List<Block>();

            // 현재 블록 수집
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (board[row, col] != null && map[row, col] != 2)
                        allBlocks.Add(board[row, col]);
                }
            }

            // 랜덤 섞기
            for (int i = 0; i < allBlocks.Count; i++)
            {
                Block temp = allBlocks[i];
                int rand = Random.Range(i, allBlocks.Count);

                if (allBlocks[rand].type == 6 || allBlocks[i].type == 6)
                    continue; // crown(6)은 건드리지 않음

                allBlocks[i] = allBlocks[rand];
                allBlocks[rand] = temp;
            }

            // 재배치
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (map[row, col] != 0 && map[row, col] != 2)
                    {
                        var block = allBlocks[index++];
                        block.SetPosition(row, col);
                        board[row, col] = block;
                    }
                }
            }

            yield return new WaitForSeconds(0.25f); // 이동이 끝나기를 약간 기다림

            // 🔍 매치 검사
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                Debug.Log($"✅ Shuffle complete ({shuffleCount} attempt(s)) — no immediate matches.");

                // 재배치
                int index2 = 0;
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        if (map[row, col] != 0 && map[row, col] != 2)
                        {
                            var block = allBlocks[index2++];
                            block.SetPosition(row, col);
                            board[row, col] = block;
                            StartCoroutine(MoveTo(block.transform, GridToWorld(row, col), 0.15f));
                        }
                    }
                }
                break; // 정상 종료
            }

            // 🔄 매치 있으면 다시 시도
            Debug.Log($"⚠️ Found {matches.Count} matches after shuffle — reshuffling...");
            yield return null; // 프레임 쉬고 다시 반복

        } while (true);
        isShaking = false;
    }



    private bool CheckMatchAt(int row, int col)
    {
        Block center = board[row, col];
        if (center == null) return false;

        int t = center.type;

        // ↘ 대각선
        if (row + 2 < rows && col + 2 < cols)
        {
            if (board[row + 1, col + 1]?.type == t &&
                board[row + 2, col + 2]?.type == t)
                return true;
        }

        // ↙ 대각선
        if (row + 2 < rows && col - 2 >= 0)
        {
            if (board[row + 1, col - 1]?.type == t &&
                board[row + 2, col - 2]?.type == t)
                return true;
        }

        // 세로 ↓
        if (row + 4 < rows)
        {
            if (board[row + 2, col]?.type == t &&
                board[row + 4, col]?.type == t)
                return true;
        }

        return false;
    }

    public Block[,] GetBoard() => board;
}

// 🔸 리스트 중복 방지용 확장 메서드
public static class ListExtensions
{
    public static void AddUnique(this List<Block> list, params Block[] blocks)
    {
        foreach (var b in blocks)
            if (!list.Contains(b)) list.Add(b);
    }
}
