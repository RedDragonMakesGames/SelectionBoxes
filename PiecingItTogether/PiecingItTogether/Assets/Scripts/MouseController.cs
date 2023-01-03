using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseController : MonoBehaviour
{
    public int boxDepth = 30;
    //Match these to the size of the floor
    public int xSize = 100;
    public int zSize = 100;

    public TMPro.TextMeshProUGUI leftText;
    public TMPro.TextMeshProUGUI rightText;
    public int maxMoves = 1;

    private int movesRemaining;

    private List<GameObject> selectedObjects = new();
    private LineRenderer box = null;

    private Vector3 boxStart;
    private Vector3 boxEnd;
    private Vector3 lastMousePos;
    private bool bIsSelecting = false;
    private bool wasSelected = false;

    private bool levelComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        box = GetComponent<LineRenderer>();
        movesRemaining = maxMoves;
        rightText.text = "Level " + SceneManager.GetActiveScene().buildIndex.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        //Restart if needed
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //Go to the next level if beaten
        if (levelComplete && Input.GetKeyDown(KeyCode.Space))
        {
            try
            {
                if (!SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1).IsValid())
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
            catch (System.Exception)
            {
                rightText.text = "That was the last level, you win! Well done, and thanks for playing!";
            }

        }

        //Handle text
        if (levelComplete)
        {
            leftText.text = "Level complete! Press Space to continue!";
        }
        else
        {
            if (movesRemaining > 0)
            {
                leftText.text = "Moves remaining: " + movesRemaining.ToString();
            }
            else
            {
                leftText.text = "Out of moves! Press R to retry";
            }
        }

        if (movesRemaining > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                boxStart = Input.mousePosition;
                bIsSelecting = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                boxEnd = Input.mousePosition;
                bIsSelecting = false;
                selectedObjects.Clear();
                SelectContents();
            }
        }

        if (wasSelected && selectedObjects.Count == 0)
        {
            wasSelected = false;
            movesRemaining -= 1;
            if (CheckIfLevelComplete())
            {
                levelComplete = true;
            }
        }

        if (selectedObjects.Count > 0)
        {
            wasSelected = true;
        }


        if (bIsSelecting) 
        {
            //Draw selection box
            Vector3[] points = { boxStart, new Vector3(boxStart.x, Input.mousePosition.y), Input.mousePosition, new Vector3(Input.mousePosition.x, boxStart.y), boxStart };
            for (int i = 0; i < 5; i++)
            {
                points[i] = Camera.main.ScreenToWorldPoint(new Vector3(points[i].x, points[i].y, boxDepth));
            }
            box.SetPositions(points);
            box.enabled = true;
        }
        else
        {
            box.enabled = false;
        }

        MoveSelectedObjects();
        lastMousePos = Input.mousePosition;
    }

    void SelectContents()
    {
        //Select all selectable objects in the area
        //Probably the best way to do this is to get all selectable objects, and then select the ones which fall inside the area
        SelectableCompoment[] blocks = FindObjectsOfType<SelectableCompoment>();
        for (int i = 0; i < blocks.Length;  i++)
        {
            float x1 = Camera.main.ScreenToViewportPoint(boxStart).x;
            float x2 = Camera.main.ScreenToViewportPoint(boxEnd).x;
            float xPos = Camera.main.WorldToViewportPoint(blocks[i].gameObject.transform.position).x;
            if ((x1 < xPos && xPos < x2) || (x2 < xPos && xPos < x1)) 
            {
                //If the block's X pos is within the box
                float y1 = Camera.main.ScreenToViewportPoint(boxStart).y;
                float y2 = Camera.main.ScreenToViewportPoint(boxEnd).y;
                float yPos = Camera.main.WorldToViewportPoint(blocks[i].gameObject.transform.position).y;

                if ((y1 < yPos && yPos < y2) || (y2 < yPos && yPos < y1))
                {
                    //If the position of the blocks is within the selection box
                    selectedObjects.Add(blocks[i].gameObject);
                }
            }
        }
    }

    void MoveSelectedObjects()
    {
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            //Vector3 movement = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(lastMousePos);
            RaycastHit beforeHit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(lastMousePos), out beforeHit))
            {
                break;
            }
            RaycastHit afterHit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out afterHit))
            {
                break;
            }
            Vector3 movement = afterHit.point - beforeHit.point;
            selectedObjects[i].transform.position += movement;

            //Make sure the objects stay on the floor
            if (selectedObjects[i].transform.position.x < -xSize/2)
            {
                selectedObjects[i].transform.position = new Vector3(-xSize/2, selectedObjects[i].transform.position.y, selectedObjects[i].transform.position.z);
            }
            else if ((selectedObjects[i].transform.position.x > xSize / 2))
            {
                selectedObjects[i].transform.position = new Vector3(xSize / 2, selectedObjects[i].transform.position.y, selectedObjects[i].transform.position.z);
            }

            if (selectedObjects[i].transform.position.z < -zSize / 2)
            {
                selectedObjects[i].transform.position = new Vector3(selectedObjects[i].transform.position.x, selectedObjects[i].transform.position.y, -zSize/2);
            }
            else if ((selectedObjects[i].transform.position.z > zSize / 2))
            {
                selectedObjects[i].transform.position = new Vector3(selectedObjects[i].transform.position.x, selectedObjects[i].transform.position.y, zSize/2);
            }
        }
    }

    bool CheckIfLevelComplete()
    {
        CheckContents[] boxes = FindObjectsOfType<CheckContents>();
        bool complete = true;
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i].correct != true)
            {
                complete = false;
            }
        }
        return complete;
    }

}
