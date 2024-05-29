using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float walkRadius = 5f; // Radius within which the enemy can walk
    public float walkSpeed = 2f; // Speed of walking
    public LayerMask groundLayer; // LayerMask for the ground
    public GameObject weaponPrefab; // Prefab of the weapon to attack with
    public Transform attackPoint; // Point from which to attack/shoot
    public bool isMelee = true; // Determines if the enemy uses melee attacks

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isWalking = false;
    private bool isAttacking = false;
    private Transform player;
    private WeaponController weaponController;

    private void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        weaponController = GetComponent<WeaponController>();

        if (weaponController == null && weaponPrefab != null)
        {
            GameObject weaponInstance = Instantiate(weaponPrefab, attackPoint.position, Quaternion.identity);
            weaponController = weaponInstance.GetComponent<WeaponController>();
        }

        StartCoroutine(WalkRoutine());
    }

    private void Update()
    {
        if (!isAttacking)
        {
            Walk();
        }

        if (Vector3.Distance(transform.position, player.position) <= weaponController.currentWeapon.range)
        {
            isAttacking = true;
            Attack();
        }
        else
        {
            isAttacking = false;
        }
    }

    private IEnumerator WalkRoutine()
    {
        while (true)
        {
            if (!isWalking)
            {
                targetPosition = startPosition + new Vector3(Random.Range(-walkRadius, walkRadius), 0, 0);
                isWalking = true;
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private void Walk()
    {
        if (isWalking)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isWalking = false;
            }
        }
    }

    private void Attack()
    {
        if (isMelee)
        {
            //weaponController.PerformMeleeAttack();
        }
        else
        {
            //weaponController.PerformRangedAttack(player.position);
        }
    }
}
