using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;

public class Controller : MonoBehaviour {

    public static Controller instance;

	public bool displayGridGizmos;
    public float moveRate;
	public float nodeRadius;
	public Node[,] grid;
    public string[,] level;
    public TextAsset levelTextFile;
    public GameObject ground;
    public GameObject wall;
    public GameObject playerPrefab;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsWall;
    public LayerMask whatIsTile;
    public bool debugMode;

    bool move = false; 
    GameObject player;
    Vector2 gridWorldSize;
    float nodeDiameter;
	int gridSizeX, gridSizeY;
    RaycastHit hitInfo;
    GameObject objectSelected = null;
    GameObject TileHolder;
    GameObject WallHolder;
    bool selected;
    Vector3 positionToMoveTo = Vector3.zero;

    void Start()
    {
        instance = this;
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
    }

	void Awake()
	{
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");

        level = Load(levelTextFile);
        instance = this;
        nodeDiameter = nodeRadius * 2;

        gridWorldSize.x = level.GetUpperBound(0) + 1;
        gridWorldSize.y = level.GetUpperBound(1) + 1;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid();
	}

    void Update()
    {
        if (instance == null)
            instance = this;

        GetInput();
        PlayerMove();
    }

    void RefreshGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y].walkable = Physics.CheckSphere(grid[x, y].worldPosition, 1, whatIsTile);
                grid[x, y].walkable = Physics.CheckSphere(grid[x, y].worldPosition, 1, whatIsPlayer);
                grid[x, y].walkable = !Physics.CheckSphere(grid[x, y].worldPosition, 1, whatIsWall);
            }
        }
    }

    void LateUpdate()
    {
        if (!debugMode)
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hitInfo, 10, whatIsPlayer))
        {
            selected = true;
            objectSelected = hitInfo.collider.gameObject;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selected = true;
            objectSelected = null;
        }

        if (selected && objectSelected != null)
        {
            var vect = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vect.y = 0;
            hitInfo.collider.transform.position = vect;
        }
    }

    void PlayerMove()
    {
        if (player.transform.position != positionToMoveTo)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, positionToMoveTo, Time.deltaTime * moveRate);
            NodeFromWorldPoint(player.transform.position).walkable = true;
        }
        if (player.transform.position == positionToMoveTo)
        {
            move = false;
        }
    }

    void GetInput()
    {
        if (move == true)
            return;

        Node node = NodeFromWorldPoint(player.transform.position);
        NodeFromWorldPoint(positionToMoveTo).walkable = true;

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            
            int currentY = node.gridY;
            for(; currentY < gridSizeY; currentY++)
		    {
                if (grid[node.gridX, currentY].walkable)
                    positionToMoveTo = grid[node.gridX, currentY].worldPosition;
                else
                    break;
            }
            move = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            int currentY = node.gridY;
            for (; currentY > 0; currentY--)
            {
                if (grid[node.gridX, currentY].walkable)
                    positionToMoveTo = grid[node.gridX, currentY].worldPosition;
                else
                    break;
            }
            move = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            int currentX = node.gridX;
            for (; currentX > 0; currentX--)
            {
                if (grid[currentX, node.gridY].walkable)
                    positionToMoveTo = grid[currentX, node.gridY].worldPosition;
                else
                    break;
            }
            move = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            int currentX = node.gridX;
            for (; currentX < gridSizeX; currentX++)
            {
                if (grid[currentX, node.gridY].walkable)
                    positionToMoveTo = grid[currentX, node.gridY].worldPosition;
                else
                    break;
            }
            move = true;
        }
        NodeFromWorldPoint(positionToMoveTo).walkable = false;
    }

    public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				grid[x, y] = new Node(false, worldPoint, x, y);

                Node node; GameObject g;
                switch (level[x, y])
                {
                    case "0":
                        //tile
                        node = grid[x, y];
                        node.walkable = true;
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        break;
                    case "1":
                        //immobile wall
                        node = grid[x, y];
                        node.walkable = false;
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;

                        break;
                    case "2":
                        //tile
                        node = grid[x, y];
                        node.walkable = true;
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //player
                        player = Instantiate(playerPrefab, node.worldPosition, Quaternion.identity) as GameObject;

                        break;
                    case "D":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.DOWN;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "U":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.UP;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "L":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.LEFT;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                    case "R":
                        node = grid[x, y];
                        node.walkable = false;

                        //tile
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = TileHolder.transform;

                        //mobile wall 
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        g.transform.parent = WallHolder.transform;
                        g.AddComponent<WallMobility>();
                        g.GetComponent<WallMobility>().direction = WallMobility.MovementDirection.RIGHT;
                        g.GetComponent<WallMobility>().occurRate = .1f;
                        g.GetComponent<WallMobility>().moveSpeed = 3f;
                        g.GetComponent<WallMobility>().whatIsWall = whatIsWall;
                        g.GetComponent<WallMobility>().node = node;
                        g.GetComponent<WallMobility>().grid = grid;

                        break;
                }
            }
		}
	}

    private string[,] Load(TextAsset fileName)
    {
        string[,] levelFile = null;

        try
        {
            string allLines = fileName.text;
            string[] lines = allLines.Split(new char[] { '#' });

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim(new char[] { '\n', '\r', '\t' });
                if (line != null)
                {
                    if (levelFile == null)
                        levelFile = new string[lines.Length, line.Length];
                    for (int j = 0; j < line.Length; j++)
                    {
                        levelFile[i, j] = line[j].ToString();
                    }
                }
            }
            return levelFile;
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (IOException e)
        {
            Debug.LogError(string.Format("{0}\n", e.Message));
            return levelFile;
        }
    }

    public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}
}
