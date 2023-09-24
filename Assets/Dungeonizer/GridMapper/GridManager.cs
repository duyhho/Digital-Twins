using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridCell
{
    public int x = 0;
    public int y = 0;
    public bool isVisited = false;
    public bool hasWall = false;
    public bool hasDoor = false;
    public bool hasGround = false;
    public bool hasFire = false;
}
public class GridManager : MonoBehaviour
{
    public GameObject agent;
    public Vector3 gridOrigin = Vector3.zero;  // Origin of the grid in world space
    public float cellSize = 1.0f;  // Size of each grid cell
    public Vector2 environmentSize = new Vector2(150, 150); // example 20x20 meters environment
    public float resizeBuffer = 10f; // The buffer zone near the edges of the grid
    public int gridExpansionAmount = 50; // The amount to expand the grid by

    GridCell[,] grid;
    float lastResizeTime = 0f;
    float resizeCooldown = 1f; // Set a cooldown time of 1 second
    public bool drawGizmos = false;
    public float parentOffsetHeight = 0f;
    void Start()
    {
        Vector3 agentInitialPos = agent.transform.position;
        gridOrigin = new Vector3(agentInitialPos.x - (environmentSize.x / 2), agentInitialPos.y, agentInitialPos.z - (environmentSize.y / 2));
        grid = new GridCell[Mathf.FloorToInt(environmentSize.x / cellSize), Mathf.FloorToInt(environmentSize.y / cellSize)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new GridCell
                {
                    x = i,
                    y = j
                };

            }
        }
    }
    void Update()
    {


        // Cooldown to prevent frequent resizing
        if (Time.time - lastResizeTime < resizeCooldown)
        {
            return;
        }


        ResizeGrid();

    }

    void ResizeGrid()
    {
        // Save the old grid data
        GridCell[,] oldGrid = grid;
        int xMax = grid.GetLength(0) - 1;
        int yMax = grid.GetLength(1) - 1;

        Vector2Int agentGridPos = WorldToGrid(agent.transform.position);

        // Determine the amount to expand on each edge
        int expandLeft = agentGridPos.x < resizeBuffer ? gridExpansionAmount : 0;
        int expandRight = agentGridPos.x > xMax - resizeBuffer ? gridExpansionAmount : 0;
        int expandBottom = agentGridPos.y < resizeBuffer ? gridExpansionAmount : 0;
        int expandTop = agentGridPos.y > yMax - resizeBuffer ? gridExpansionAmount : 0;

        // Adjust the gridOrigin to keep the 'world' positions of the cells consistent
        gridOrigin.x -= expandLeft * cellSize;
        gridOrigin.z -= expandBottom * cellSize;

        if (expandLeft > 0 || expandRight > 0 || expandBottom > 0 || expandTop > 0)
        {
            // Calculating the new environment size based on the expansions
            environmentSize = new Vector2(environmentSize.x + expandLeft + expandRight, environmentSize.y + expandBottom + expandTop);

            // Create and initialize a new grid with the new dimensions
            grid = new GridCell[Mathf.FloorToInt(environmentSize.x / cellSize), Mathf.FloorToInt(environmentSize.y / cellSize)];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j] = new GridCell
                    {
                        x = i,
                        y = j
                    };

                    // Calculate the corresponding index in the old grid
                    int oldGridX = i - expandLeft;
                    int oldGridY = j - expandBottom;

                    // If the old grid has data at this index, copy it to the new grid
                    if (oldGridX >= 0 && oldGridY >= 0 && oldGridX < oldGrid.GetLength(0) && oldGridY < oldGrid.GetLength(1))
                    {
                        grid[i, j] = oldGrid[oldGridX, oldGridY];
                    }
                }
            }

            // Update last resize time to implement a cooldown
            lastResizeTime = Time.time;
        }
    }


    // Convert a world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z - gridOrigin.z) / cellSize);  // Use z because Unity is Y-up

        x = Mathf.Clamp(x, 0, grid.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, grid.GetLength(1) - 1);

        return new Vector2Int(x, y);
    }
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = gridPosition.x * cellSize + gridOrigin.x + cellSize / 2.0f;
        float z = gridPosition.y * cellSize + gridOrigin.z + cellSize / 2.0f;

        // Assuming the y-coordinate (height) is 0; adjust if necessary
        return new Vector3(x, 0, z);
    }


    public void SetVisited(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (!IsValidGridPosition(gridPosition))
        {
            return;
        }

        var agentComponent = agent.GetComponent<DungeonAgentFire>();

        if (grid[gridPosition.x, gridPosition.y].isVisited != state)
        {
            grid[gridPosition.x, gridPosition.y].isVisited = state;

            // Reward or penalty based on the new state
            if (state)
            {
                // agentComponent.AddReward(0.01f); // Reward for visiting a new cell
            }
        }
        else
        {
            // agentComponent.AddReward(-0.01f); // Penalty for redundant action (revisiting or unvisiting)
        }
    }




    public void SetWall(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (grid[gridPosition.x, gridPosition.y].hasWall != state)
        {
            grid[gridPosition.x, gridPosition.y].hasWall = state;
            // Optionally: Trigger any events or notifications about the state change
        }
    }

    public void SetDoor(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (grid[gridPosition.x, gridPosition.y].hasDoor != state)
        {
            grid[gridPosition.x, gridPosition.y].hasDoor = state;
            // Optionally: Trigger any events or notifications about the state change
        }
    }

    public void SetGround(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (grid[gridPosition.x, gridPosition.y].hasGround != state)
        {
            grid[gridPosition.x, gridPosition.y].hasGround = state;
            // Optionally: Trigger any events or notifications about the state change
        }
    }

    public void SetFire(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (!IsValidGridPosition(gridPosition))
        {
            return;
        }

        var agentComponent = agent.GetComponent<DungeonAgentFire>();

        if (grid[gridPosition.x, gridPosition.y].hasFire != state)
        {
            grid[gridPosition.x, gridPosition.y].hasFire = state;

            // Give a reward for discovering fire for the first time
            if (state)
            {
                // Debug.Log("Reward discover fire!");
                // agentComponent.AddReward(0.5f);
            }
            // Optionally: Trigger any events or notifications about the state change
        }
    }




    public void ResetGrid()
    {
        // Calculating the new environment size based on the expansions
        environmentSize = new Vector2(150, 150);
        // Create and initialize a new grid with the new dimensions

        Vector3 agentInitialPos = agent.transform.position;
        gridOrigin = new Vector3(agentInitialPos.x - (environmentSize.x / 2), agentInitialPos.y, agentInitialPos.z - (environmentSize.y / 2));
        grid = new GridCell[Mathf.FloorToInt(environmentSize.x / cellSize), Mathf.FloorToInt(environmentSize.y / cellSize)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new GridCell
                {
                    x = i,
                    y = j
                };
            }
        }
    }
    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;
        if (grid == null)
        {
            ResetGrid();
            Debug.Log("grid:" + grid.GetLength(0));

        };
        float offset = 150f;
        float opacity = 0.5f;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Vector3 cellWorldPosition = new Vector3(gridOrigin.x + x * cellSize - offset, gridOrigin.y - 1f + parentOffsetHeight, gridOrigin.z + y * cellSize);

                // None of the above conditions were true, so set the color to white
                Gizmos.color = Color.white;
                // Gizmos.color = new Color(0.53f, 0.81f, 0.98f, opacity);
                if (grid[x, y].hasGround)
                {
                    Gizmos.color = new Color(0.53f, 0.81f, 0.98f, opacity);
                    if (grid[x, y].isVisited)
                        Gizmos.color = new Color(0.0f, 0.0f, 1.0f);
                }
                if (grid[x, y].hasDoor)
                    Gizmos.color = new Color(1.0f, 1.0f, 0.0f, opacity);
                if (grid[x, y].hasWall)
                    Gizmos.color = new Color(0, 0, 0, opacity);
                if (grid[x, y].hasFire)
                    Gizmos.color = new Color(1.0f, 0.0f, 1.0f, opacity);





                // If visited, override with full opacity
                if (grid[x, y].isVisited) { }
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1.0f);


                Gizmos.DrawCube(cellWorldPosition, new Vector3(cellSize, 0.01f, cellSize));
            }
        }

        // Drawing an arrow to indicate the agent's facing direction
        Vector2Int agentGridPosition = WorldToGrid(agent.transform.position);
        Vector3 agentPosition = new Vector3(agentGridPosition.x * cellSize + gridOrigin.x - offset, gridOrigin.y - 1f + parentOffsetHeight, agentGridPosition.y * cellSize + gridOrigin.z);
        Vector3 agentForward = agent.transform.forward;
        float arrowLength = 1f;
        float arrowHeadSize = 2.5f;

        // Calculate the positions of the arrow
        Vector3 arrowTail = agentPosition;
        Vector3 arrowHead = agentPosition + agentForward * arrowLength;
        Vector3 arrowLeft = arrowHead - (agentForward + agent.transform.right).normalized * arrowHeadSize;
        Vector3 arrowRight = arrowHead - (agentForward - agent.transform.right).normalized * arrowHeadSize;

        // Drawing the main shaft of the arrow with a "cylinder" to make it thicker
        Gizmos.color = Color.cyan;

        // Drawing a series of parallel lines to simulate a thicker line for the arrow shaft
        float thickness = 0.1f; // Adjust the thickness value as needed
        for (float i = -thickness; i <= thickness; i += 0.02f)
        {
            Gizmos.DrawLine(arrowTail + agent.transform.right * i, arrowHead + agent.transform.right * i);
        }

        // Drawing the head of the arrow with lines
        Gizmos.DrawLine(arrowHead, arrowLeft);
        Gizmos.DrawLine(arrowHead, arrowRight);

        // Drawing spheres at the ends to simulate thickness at the ends of the lines forming the arrowhead
        Gizmos.DrawSphere(arrowLeft, thickness);
        Gizmos.DrawSphere(arrowRight, thickness);
        // Get the 3x3 grid in front of the agent
        Vector2Int[] frontGrids = GetFrontGrid(agentGridPosition, 3);

        foreach (var gridPosition in frontGrids)
        {
            if (IsValidGridPosition(gridPosition))
            {
                // Set a default semi-transparent color for the front grid cells
                Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.35f); // Semi-transparent green

                Vector3 cellWorldPosition = new Vector3(gridOrigin.x + gridPosition.x * cellSize - offset, gridOrigin.y - 1f + parentOffsetHeight, gridOrigin.z + gridPosition.y * cellSize);
                Gizmos.DrawCube(cellWorldPosition, new Vector3(cellSize, 0.01f, cellSize));
            }
        }
    }

    public Vector2Int[] GetFrontGrid(Vector2Int agentGridPosition, int gridSize)
    {
        // Getting agent's forward direction in grid coordinates
        Vector2Int agentForwardGrid = new Vector2Int(Mathf.RoundToInt(agent.transform.forward.x), Mathf.RoundToInt(agent.transform.forward.z));

        // The middle cell of the grid (not necessarily in the center, depending on the forward direction)
        Vector2Int middleCell = agentGridPosition + agentForwardGrid * (gridSize / 2);

        // Getting the other cells in the grid
        Vector2Int[] frontGrids = new Vector2Int[gridSize * gridSize];
        int index = 0;
        int halfGridSize = gridSize / 2;
        for (int i = -halfGridSize; i <= halfGridSize; i++)
        {
            for (int j = -halfGridSize; j <= halfGridSize; j++)
            {
                frontGrids[index++] = new Vector2Int(middleCell.x + i, middleCell.y + j);
            }
        }

        return frontGrids;
    }


    bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < grid.GetLength(0) && position.y < grid.GetLength(1);
    }
    public GridCell GetGridCell(int x, int y)
    {
        Vector2Int gridPosition = new Vector2Int(x, y);
        if (IsValidGridPosition(gridPosition))
        {
            return grid[x, y];
        }
        else
        {
            // If the coordinates are out of bounds, return null or throw an exception
            return null;
        }
    }
}
