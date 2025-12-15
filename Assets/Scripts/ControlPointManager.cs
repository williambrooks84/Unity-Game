using UnityEngine;
using System.Linq;

using TMPro;

public class ControlPointManager : MonoBehaviour
{
    private ControlPoint[] controlPoints;
    private bool victoryTriggered = false;

    void Start()
    {
        controlPoints = FindObjectsOfType<ControlPoint>();
    }

    void Update()
    {
        if (!victoryTriggered && controlPoints.All(cp => cp.IsCaptured()))
        {
            TriggerVictory();
        }
    }

    void TriggerVictory()
    {
        victoryTriggered = true;
        Menu menu = FindObjectOfType<Menu>();
        if (menu != null)
        {
            menu.ShowVictoryScreen();
        }
    }
}
