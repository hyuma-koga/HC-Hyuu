﻿using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnRoot;
    public SnakeFollowMouse snake;
    public BlockDestroyManager destroyManager;

    public float previewOffset = 10f;
    public float spawnYOffset = 8f;
    public float spawnSpacingY = 4f;

    [Header("スター無敵用設定")]
    public float starBlockChance = 0.1f;
    public GameObject starIconPrefab;
    public InvincibleManager invincibleManager;

    private float lastSpawnY;
    private readonly float[] xPositions = new float[] { -2.248f, -1.124f, 0f, 1.124f, 2.248f };

    private void Start()
    {
        //初回のブロック生成
        if (snake != null && snake.head != null)
        {
            lastSpawnY = snake.transform.position.y + spawnSpacingY;
            float spawnY = lastSpawnY + spawnYOffset;
            SpawnBlockRow(spawnY);
        }
        else
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (snake == null || snake.head == null)
        {
            return;
        }

        float snakeY = snake.head.position.y;

        if ((snakeY + previewOffset) - lastSpawnY >= spawnSpacingY)
        {
            SpawnBlockRow(snakeY + spawnYOffset);
            lastSpawnY = snakeY;
        }
    }

    private void SpawnBlockRow(float spawnY)
    {
        if (spawnY >= 100f)
        {
            return;
        }

        int blockCount = Random.Range(2, 6);
        List<int> availableIndices = new List<int>() { 0, 1, 2, 3, 4 };
        List<int> selectedIndices = new List<int>();

        for (int i = 0; i < blockCount; i++)
        {
            int randIdx = Random.Range(0, availableIndices.Count);
            selectedIndices.Add(availableIndices[randIdx]);
            availableIndices.RemoveAt(randIdx);
        }

        int snakeCount = snake.GetSegmentCount();
        int passableIdx = selectedIndices[Random.Range(0, selectedIndices.Count)];

        foreach (int idx in selectedIndices)
        {
            Vector3 pos = new Vector3(xPositions[idx], spawnY, 0f);
            GameObject block = Instantiate(blockPrefab, pos, Quaternion.identity, spawnRoot);

            //スター付きブロック生成
            bool isStarBlock = Random.value < 0.1f;
            if (isStarBlock)
            {
                GameObject starIcon = Instantiate(starIconPrefab, block.transform);
                starIcon.transform.localPosition = new Vector3(0f, -0.27f, -0.1f);
            }

            if (block.TryGetComponent(out BlockStarTrigger starTrigger))
            {
                starTrigger.isStarBlock = isStarBlock;
                starTrigger.invincibleManager = invincibleManager;
            }

            int hp = (idx == passableIdx)
                ? Random.Range(1, Mathf.Max(1, snakeCount))
                : Random.Range(1, 32);

            if (block.TryGetComponent(out BlockHP blockHP))
            {
                blockHP.hp = hp;
            }

            if (block.TryGetComponent(out BlockCollision blockCollision))
            {
                blockCollision.SetInitialHP(hp);
                blockCollision.destroyManager = destroyManager;
            }

            if (block.TryGetComponent(out BlockController controller))
            {
                controller.UpdateHPDisplay();
            }

            if (block.TryGetComponent(out BlockAutoDestroy autoDestroy))
            {
                autoDestroy.isAutoSpawned = true;
            }
        }
    }

    public void ResetSpawner()
    {
        if (snake == null || snake.head == null)
        {
            return;
        }

        //一度すべてのブロックを削除
        ClearAllBlocks();

        //新しい Snake の位置を基準に再設定
        lastSpawnY = snake.head.position.y + spawnSpacingY;
        float spawnY = lastSpawnY + spawnYOffset;

        //最初のブロック列を再生成
        SpawnBlockRow(spawnY);
    }

    public void ClearAllBlocks()
    {
        if (spawnRoot == null)
        {
            return;
        }

        foreach (Transform child in spawnRoot)
        {
            Destroy(child.gameObject);
        }
    }
}
