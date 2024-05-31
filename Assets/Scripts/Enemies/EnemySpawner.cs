using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    private Queue<Enemy> enemyPool = new Queue<Enemy>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize enemy pool from inactive children
        InitializePoolFromChildren();
    }

    private void InitializePoolFromChildren()
    {
        // Add all inactive child enemies to the pool
        foreach (Transform child in transform)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null && !child.gameObject.activeInHierarchy)
            {
                enemyPool.Enqueue(enemy);
            }
        }
    }

    public Enemy GetEnemy(Vector3 position)
    {
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("No more enemies in the pool. Consider increasing initial pool size.");
            return null;
        }

        Enemy enemy = enemyPool.Dequeue();
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemyPool.Enqueue(enemy);

    }

    public void RespawnEnemy(Enemy enemy, Vector3 position, float delay)
    {
        // Ensure the enemy's health is reset and the animator is reset
        HealthController healthController = enemy.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.ResetHealth(); // Add this method in HealthController
            healthController.ResetAnimator(); // Add this method in HealthController
        }

        StartCoroutine(RespawnCoroutine(enemy, position, delay));
    }

    private IEnumerator RespawnCoroutine(Enemy enemy, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnEnemy(enemy);
        GetEnemy(position);
    }
}
