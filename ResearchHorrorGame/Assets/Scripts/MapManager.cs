using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject piecePrefab;

    /// <summary>
    /// The piece that moves to cover the back part of the map
    /// </summary>
    public Transform backCap;
    private Vector3 backCapPos;

    public GameObject doors;
    public Vector3 doorsPos;

    /// <summary>
    /// The piece that moves to cover the front part of the map
    /// </summary>
    public Transform frontCap;
    private Vector3 frontCapPos;

    public List<Transform> pieces = new List<Transform>();

    private Vector3 PlayerPos { get => Player.player.transform.position; }
    private float SpawnZThreshold { get => ptr * pieceLength - (pieceMargin * pieceLength); }
    private int ptr = 0;

    /// <summary>
    /// The length of the map piece in the forward direction in meters.
    /// </summary>
    public float pieceLength = 10;

    /// <summary>
    /// The space (in number of map pieces) before the end of a given piece that the player must reach before making the MapManager spawn a new piece
    /// The pieceMargin + blockSize should not exeed maxMapSize or else player will be left outside of the map every time it spawns new pieces
    /// </summary>
    public readonly int pieceMargin = 4;

    /// <summary>
    /// How many pieces will be spawned in each increment of the map?
    /// </summary>
    public readonly int blockSize = 2;

    /// <summary>
    /// The max size of the map (in number of map pieces)
    /// </summary>
    public readonly int maxMapSize = 10;

    /*
     * TODO: Implement a hiding feature so that the player doesn't see the map pieces spawning:
     * 
     * Make the system automatically spawn doors that are positioned after the next threshold (so that the player passes the threshold, then it spawns more, then the player 
     * passes the door so they never see the pieces spawning)
     * 
     * This requires the threshold to be a high number so that the player isn't always breaking down doors and it becomes annoying
     * AND/OR
     * Randomize the threshold by a small amount so that doors are not always the same exact distance apart
     */


    private void Awake()
    {
        backCapPos = backCap.position;
        frontCapPos = frontCap.position;
        doorsPos = doors.transform.position;

        SpawnMapPieces();
    }

    private void Update()
    {
        if(PlayerPos.z >= SpawnZThreshold)
        {
            SpawnMapPieces();
        }

        if(pieces.Count > maxMapSize)
            DeallocateMapPieces();
    }

    private void SpawnMapPieces()
    {
        Transform t = null;
        for(int i = 0; i < blockSize; i++)
        {
            t = Instantiate(piecePrefab).transform;
            t.parent = transform;

            t.position = Vector3.forward * (ptr * pieceLength + pieceLength / 2);

            pieces.Add(t);
            ptr++;
        }

        HideFront(t);

        //If the SpawnZThreshold is greater than zero (plus a small margin), then spawn doors.
        //(this check prevents doors from being spawned in negative areas as well as too close to the start of the map)
        if(SpawnZThreshold > 0.5f)
            SpawnDoor();
    }

    private void DeallocateMapPieces()
    {
        int deallocate = pieces.Count - maxMapSize;

        for(int i = 0; i < deallocate; i++)
        {
            Destroy(pieces[0].gameObject);
            pieces.RemoveAt(0);
        }

        HideBack();
    }

    /// <summary>
    /// Used to set the z position of the backCap Transform so that it hides the opening on the back of the map after the system deletes deallocates pieces
    /// </summary>
    private void HideBack()
    {
        //Set the position of the backCap to the end of the first piece of the map
        backCapPos.z = pieces[0].position.z - pieceLength / 2;
        backCap.position = backCapPos;
    }

    /// <summary>
    /// Does the same as HideBack except for the frontCap as well as except for accepting in the Transform of the lastMapPiece spawned to prevent the system from having to
    /// access the last List item in linear time every frame. Instead just pass in the Transform reference after spawning the last piece in SpawnMapPieces.
    /// </summary>
    private void HideFront(Transform lastMapPiece)
    {
        //Set the position of the frontCap to the end of the last piece of the map
        frontCapPos.z = lastMapPiece.position.z + pieceLength / 2;
        frontCap.position = frontCapPos;
    }

    private void SpawnDoor()
    {
        Transform t = Instantiate(doors).transform;
        //Spawn a door slightly after the next threshold so player passes threshold while view is still blocked by door
        doorsPos.z = SpawnZThreshold + 8f; //The door will be placed 8m after the point where the player crosses to spawn new pieces
        t.position = doorsPos;
    }
}