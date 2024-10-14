using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorManager : MonoBehaviour
{
    public static FloorManager instance;

    public bool doorsAreOpen;
    public List<Room> rooms;

    public FloorResources floorResources;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Okay then init
        instance = this;
        rooms = new List<Room>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleDoors();
        }
    }

    public void CloseDoors()
    {
        doorsAreOpen = false;
        Tile doorTileClosed = floorResources.doorTileClosed;
        ChangeDoorState(doorTileClosed);
    }

    public void OpenDoors()
    {
        doorsAreOpen = true;
        Tile doorTileOpen = floorResources.doorTileOpen;
        ChangeDoorState(doorTileOpen);
    }

    private void ChangeDoorState(Tile doorTile)
    {
        foreach (Room room in rooms)
        {
            BoundsInt bounds = room.doorTilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    Debug.Log("Checking tile: " + x + "," + y);
                    // Check if there's an existing tile at this position
                    if (room.doorTilemap.GetTile(tilePosition) != null)
                    {
                        // Replace the existing tile with the new tile
                        Debug.Log("Changed tile");
                        room.doorTilemap.SetTile(tilePosition, doorTile);
                    }
                }
            }
        }
    }

    private void ToggleDoors()
    {
        if(doorsAreOpen)
        {
            CloseDoors();
        }
        else
        {
            OpenDoors();
        }
    }


}