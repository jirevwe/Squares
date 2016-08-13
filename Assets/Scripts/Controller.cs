using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class Controller : MonoBehaviour {

    public static Controller instance;

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;
    public int[,] level;

    public GameObject ground;
    public GameObject wall;
    public GameObject playerPrefab;

    GameObject player;

    public TextAsset levelTextFile;

    float nodeDiameter;
	int gridSizeX, gridSizeY;

    private RaycastHit hitInfo;
    public LayerMask whatIsPlayer;
    private GameObject objectSelected = null;
    private bool selected;

    void Start()
    {
        instance = this;
    }

	void Awake()
	{
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

    void GetInput()
    {
        Vector3 positionToMoveTo = Vector3.zero;

        if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            Node node = NodeFromWorldPoint(player.transform.position);
            int currentY = node.gridY;
            for(; currentY < gridSizeY; currentY++)
		    {
                if (grid[node.gridX, currentY].walkable)
                    positionToMoveTo = grid[node.gridX, currentY].worldPosition;
                else
                    break;
            }
            player.transform.position = positionToMoveTo;
            Debug.Log(positionToMoveTo + " " + NodeFromWorldPoint(positionToMoveTo).ToString());
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            Node node = NodeFromWorldPoint(player.transform.position);
            int currentY = node.gridY;
            for (; currentY > 0; currentY--)
            {
                if (grid[node.gridX, currentY].walkable)
                    positionToMoveTo = grid[node.gridX, currentY].worldPosition;
                else
                    break;
            }
            player.transform.position = positionToMoveTo;
            Debug.Log(positionToMoveTo + " " + NodeFromWorldPoint(positionToMoveTo).ToString());
        }else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Node node = NodeFromWorldPoint(player.transform.position);
            int currentX = node.gridX;
            for (; currentX > 0; currentX--)
            {
                if (grid[currentX, node.gridY].walkable)
                    positionToMoveTo = grid[currentX, node.gridY].worldPosition;
                else
                    break;
            }
            player.transform.position = positionToMoveTo;
            Debug.Log(positionToMoveTo + " " + NodeFromWorldPoint(positionToMoveTo).ToString());
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Node node = NodeFromWorldPoint(player.transform.position);
            int currentX = node.gridX;
            for (; currentX < gridSizeX; currentX++)
            {
                if (grid[currentX, node.gridY].walkable)
                    positionToMoveTo = grid[currentX, node.gridY].worldPosition;
                else
                    break;
            }
            player.transform.position = positionToMoveTo;
            Debug.Log(positionToMoveTo + " " + NodeFromWorldPoint(positionToMoveTo).ToString());
        }
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
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
				grid[x, y] = new Node(walkable, worldPoint, x, y);

                Node node; GameObject g;
                switch (level[x, y])
                {
                    case 0:
                        //wall
                        node = grid[x, y];
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        node.gameObject = g;

                        break;
                    case 1:
                        //cell
                        node = grid[x, y];
                        node.walkable = false;
                        g = Instantiate(wall, node.worldPosition, Quaternion.identity) as GameObject;
                        node.gameObject = g;

                        break;
                    case 2:
                        //cell
                        node = grid[x, y];
                        g = Instantiate(ground, node.worldPosition, Quaternion.identity) as GameObject;
                        node.gameObject = g;

                        //player
                        player = Instantiate(playerPrefab, node.worldPosition, Quaternion.identity) as GameObject;

                        break;
                }
            }
		}
	}

    private int[,] Load(TextAsset fileName)
    {
        int[,] levelFile = null;

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
                        levelFile = new int[lines.Length, line.Length];
                    for (int j = 0; j < line.Length; j++)
                    {
                        levelFile[i, j] = Convert.ToInt32(line[j].ToString());
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
