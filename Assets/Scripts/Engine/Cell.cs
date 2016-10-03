using UnityEngine;

/// <summary>
/// The samllest Unit of a game world...
/// </summary>
public class Cell : CellBehaviour
{
    protected Grid grid;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public bool walkable;
    public bool colored;

    /// <summary>
    /// Can be used for graph traversals
    /// </summary>
    public Cell parent;

    public Cell(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, Grid _grid)
    {
        grid = _grid;
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public bool Done { get { return walkable && colored; } }

    public override string ToString()
    {
        return gridX + ", " + gridY;
    }

    public virtual void InitBehavoiur(string character, GameObject g)
    {
        #region custom code
        switch (character)
        {
            case "0":
                //tile
                walkable = true;

                //ground
                g.transform.parent = grid.TileHolder.transform;

                break;
            case "1":
                //immobile wall
                walkable = false;

                //wall
                g = Object.Instantiate(grid.wall, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.WallHolder.transform;

                break;
            case "P":
                //tile
                walkable = true;

                //ground
                g.transform.parent = grid.TileHolder.transform;

                //player
                g = Object.Instantiate(grid.playerPrefab, worldPosition, Quaternion.identity) as GameObject;

                break;
            case "D":
                walkable = false;

                //tile
                g.transform.parent = grid.TileHolder.transform;

                //mobile wall
                g = Object.Instantiate(grid.wall, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.WallHolder.transform;
                g.GetComponent<MeshRenderer>().materials[0].mainTexture = grid.movableBoxTexture;
                g.AddComponent<WallMobility>();
                g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.DOWN;
                g.GetComponent<WallMobility>().occurRate = .1f;
                g.GetComponent<WallMobility>().moveSpeed = 3f;
                g.GetComponent<WallMobility>().whatIsWall = grid.whatIsWall;
                g.GetComponent<WallMobility>().node = this;
                g.GetComponent<WallMobility>().grid = grid.grid;

                break;
            case "U":
                walkable = false;

                //tile
                g.transform.parent = grid.TileHolder.transform;

                //mobile wall 
                g = Object.Instantiate(grid.wall, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.WallHolder.transform;
                g.GetComponent<MeshRenderer>().materials[0].mainTexture = grid.movableBoxTexture;
                g.AddComponent<WallMobility>();
                g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.UP;
                g.GetComponent<WallMobility>().occurRate = .1f;
                g.GetComponent<WallMobility>().moveSpeed = 3f;
                g.GetComponent<WallMobility>().whatIsWall = grid.whatIsWall;
                g.GetComponent<WallMobility>().node = this;
                g.GetComponent<WallMobility>().grid = grid.grid;

                break;
            case "L":
                walkable = false;

                //tile
                g.transform.parent = grid.TileHolder.transform;

                //mobile wall 
                g = Object.Instantiate(grid.wall, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.WallHolder.transform;
                g.GetComponent<MeshRenderer>().materials[0].mainTexture = grid.movableBoxTexture;
                g.AddComponent<WallMobility>();
                g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.LEFT;
                g.GetComponent<WallMobility>().occurRate = .1f;
                g.GetComponent<WallMobility>().moveSpeed = 3f;
                g.GetComponent<WallMobility>().whatIsWall = grid.whatIsWall;
                g.GetComponent<WallMobility>().node = this;
                g.GetComponent<WallMobility>().grid = grid.grid;

                break;
            case "R":
                walkable = false;

                //tile
                g.transform.parent = grid.TileHolder.transform;

                //mobile wall 
                g = Object.Instantiate(grid.wall, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.WallHolder.transform;
                g.GetComponent<MeshRenderer>().materials[0].mainTexture = grid.movableBoxTexture;
                g.AddComponent<WallMobility>();
                g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.RIGHT;
                g.GetComponent<WallMobility>().occurRate = .1f;
                g.GetComponent<WallMobility>().moveSpeed = 3f;
                g.GetComponent<WallMobility>().whatIsWall = grid.whatIsWall;
                g.GetComponent<WallMobility>().node = this;
                g.GetComponent<WallMobility>().grid = grid.grid;

                break;
            case "X":
                walkable = true;
                colored = true;
                grid.objectiveNodes.Add(this);

                //tile
                g.transform.parent = grid.TileHolder.transform;

                //objective
                g = Object.Instantiate(grid.objectiveTile, worldPosition, Quaternion.identity) as GameObject;
                g.transform.parent = grid.ObjectiveHolder.transform;
                grid.objectiveNodeGameobjects.Add(g);

                break;
        }
        #endregion
    }
}

public interface CellBehaviour
{
    void InitBehavoiur(string character, GameObject g);
}

