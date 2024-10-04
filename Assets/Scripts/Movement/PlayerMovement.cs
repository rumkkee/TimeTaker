using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast a player moves")]
    public float moveSpeed = 5f;
    // Tilemap stuff.. 
    [Header("Tilemap collisions")]
    [SerializeField]
    [Tooltip("The tile map that the ground is on")]
    public Tilemap groundTileMap;
    [SerializeField]
    [Tooltip("The tile map that the collisions are determined is on")]
    public Tilemap collisionTileMap;
    // is the player moving?
    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public Vector3 targetPos;
    [HideInInspector]
    public Vector3Int currentGridPos;
    public EnemyEntity testEnemy; 
    private TimeManager _timeManInstance;
    private void Start()
    {
        // Get the time manger 
        _timeManInstance = TimeManager.instance;
    }
    private void Update()
    {
        // Check if we moved
        if (currentGridPos != groundTileMap.WorldToCell(transform.position))
        {
            Debug.Log($"We moved cells: {currentGridPos}");
            Action movement = new Action(Action.TypeOfAction.Movement, currentGridPos, MoveToPosition);
            testEnemy.pathFind(transform);
            currentGridPos = groundTileMap.WorldToCell(transform.position);
            _timeManInstance.IncrementIndex();
            _timeManInstance.addAction(movement);
            
        }
        // Not moving don't care lol.. 
        if (!isMoving)
        {
            return;
        }
        // We are movin and grovin
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPos) < float.Epsilon)
        {
            isMoving = false;

        }
    }
    public static Vector2 SnapCardinal(Vector2 inputDir)
    {
        if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
        {
            return new Vector2(Mathf.Sign(inputDir.x), 0);
        }
        return new Vector2(0, MathF.Sign(inputDir.y));
    }
    public void Move(Vector2 direction)
    {
        // No point and taking direction while moving.. 
        if (isMoving)
        {
            // Debug.Log("Player is already moving");
            return;
        }
        // Lets check if we can move.. 
        if (CanMove(direction))
        {
            
            targetPos = transform.position + (Vector3)direction;
            isMoving = true;
            // Debug.Log($"Lets move?: isMoving? {isMoving}");
        }
    }
    public bool CanMove(Vector2 direction)
    {
        // Lets check if we can move.. 
        Vector3Int gridPos = groundTileMap.WorldToCell(transform.position + (Vector3)direction);
        if (!groundTileMap.HasTile(gridPos) || collisionTileMap.HasTile(gridPos))
        {
            return false;
        }
        return true;
    }
    public void MoveToPosition(Vector3 pos)
    {
        // take a direction!
        Vector2 direction = new Vector2(pos.x - transform.position.x, pos.y - transform.position.y);
        if (CanMove(direction))
        {
            transform.position = pos;
            currentGridPos = groundTileMap.WorldToCell(transform.position);
        }
    }
}