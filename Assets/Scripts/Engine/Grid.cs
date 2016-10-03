using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This represenst the game world or part of it...
/// It consists of cells
/// </summary>
public class Grid : MonoBehaviour {

    public static Grid instance;

	public bool displayGridGizmos;
    public float moveRate;
	public float nodeRadius;
	public Cell[,] grid;
    public string[,] level;
    public GameObject ground;
    public GameObject wall;
    public GameObject playerPrefab;
    public GameObject objectiveTile;
    public LayerMask whatIsWall;
    public LayerMask whatIsTile;
    public bool debugMode;
    public List<Cell> objectiveNodes;
    public Text gameTime;
    public Text movesText;
    public Texture movableBoxTexture;

    Vector2 gridWorldSize;
    float nodeDiameter;
    [HideInInspector]
	public int gridSizeX, gridSizeY;
    public GameObject TileHolder;
    public GameObject WallHolder;
    public GameObject ObjectiveHolder;
    public List<GameObject> objectiveNodeGameobjects;
    float time;
    public float movesMade;
    TextAsset levelTextFile;

    public EasyTouch.SwipeDirection swipe;

    void Start()
    {
        instance = this;
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");
    }

	void Awake()
	{
        //init
        WallHolder = GameObject.FindGameObjectWithTag("WallHolder");
        TileHolder = GameObject.FindGameObjectWithTag("TileHolder");
        ObjectiveHolder = GameObject.FindGameObjectWithTag("ObjectiveHolder");

        //load file
        levelTextFile = Resources.Load(PlayerPrefs.GetString("LEVEL_TO_LOAD")) as TextAsset;

        level = Load(levelTextFile);
        instance = this;
        nodeDiameter = nodeRadius * 2;

        //create level grid
        gridWorldSize.x = level.GetUpperBound(0) + 1;
        gridWorldSize.y = level.GetUpperBound(1) + 1;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        //this is custom stuff will be made generic later
        objectiveNodes = new List<Cell>();
        objectiveNodeGameobjects = new List<GameObject>();

        //Create Grid World
        grid = new Cell[gridSizeX, gridSizeY];
        CreateGrid(grid);
    }

    void Update()
    {
        if (instance == null)
            instance = this;
        
        //more custom stuff
        UpdateGameTime();
        movesText.text = movesMade.ToString();

        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene(0);
    }

    void UpdateGameTime()
    {
        time += Time.deltaTime;

        var minutes = time / 60;            //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;            //Use the euclidean division for the seconds.
        //var fraction = (time * 100) % 100;

        //update the label value
        //gameTime.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
        gameTime.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

	void CreateGrid(Cell[,] _grid)
	{
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				_grid[x, y] = new Cell(false, worldPoint, x, y, this);

                Cell cell; GameObject g;

                cell = _grid[x, y];
                g = Instantiate(ground, cell.worldPosition, Quaternion.identity) as GameObject;

                cell.InitBehavoiur(level[x, y], g);
            }
        }
	}

    /// <summary>
    /// Load the level text file and returns it as a tow dimensional array
    /// </summary>
    /// <param name="fileName">The index of the level file</param>
    /// <returns>the 2 dimesional array representation of the level file</returns>
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
        catch (IOException e)
        {
            Debug.LogError(string.Format("{0}\n", e.Message));
            return levelFile;
        }
    }

    /// <summary>
    /// returns the immediate neigbhors of a cell
    /// </summary>
    /// <param name="Cell"></param>
    /// <returns></returns>
    public List<Cell> GetNeighbours(Cell Cell)
	{
		List<Cell> neighbours = new List<Cell>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = Cell.gridX + x;
				int checkY = Cell.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add((Cell)grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Cell NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

    #region SwipeInput

    void OnEnable()
    {
        EasyTouch.On_SwipeStart += On_SwipeStart;
        EasyTouch.On_Swipe += On_Swipe;
        EasyTouch.On_SwipeEnd += On_SwipeEnd;
    }

    void OnDisable()
    {
        UnsubscribeEvent();

    }

    void OnDestroy()
    {
        UnsubscribeEvent();
    }

    void UnsubscribeEvent()
    {
        EasyTouch.On_SwipeStart -= On_SwipeStart;
        EasyTouch.On_Swipe -= On_Swipe;
        EasyTouch.On_SwipeEnd -= On_SwipeEnd;
    }


    // At the swipe beginning 
    private void On_SwipeStart(Gesture gesture)
    {

    }

    // During the swipe
    private void On_Swipe(Gesture gesture)
    {

    }

    // At the swipe end 
    private void On_SwipeEnd(Gesture gesture)
    {
        swipe = gesture.swipe;
    }

    #endregion
}
