using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckContents : MonoBehaviour
{
    //Input
    public blockColour colour = blockColour.None;
    public int boxesRequired = 1;
    //Outputs
    public int correctBoxes = 0;
    public int wrongBoxes = 0;
    public bool correct = false;

    public TextMeshProUGUI targetBlocksText;

    // Start is called before the first frame update
    void Start()
    {
        targetBlocksText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);
        int rightBoxes = 0;
        int incorrectBoxes = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != this.gameObject)
            {
                SelectableCompoment box = colliders[i].GetComponent<SelectableCompoment>();
                if (box != null)
                {
                    if (box.colour == colour)
                    {
                        rightBoxes++;
                    }
                    else
                    {
                        incorrectBoxes++;
                    }
                }
            }
        }
        correctBoxes = rightBoxes;
        wrongBoxes = incorrectBoxes;

        if (correctBoxes == boxesRequired && incorrectBoxes == 0)
            correct = true;
        else
            correct = false;

        targetBlocksText.text = (boxesRequired - correctBoxes).ToString();
    }
}
