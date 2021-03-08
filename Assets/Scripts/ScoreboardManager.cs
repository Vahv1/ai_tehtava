using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardManager : MonoBehaviour
{
    public Font font;
    public TurnManager turnManager;

    void Start()
    {
        turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        int i = 1;
        foreach (Transform playerScore in transform)
        {
            Vector2 pos = playerScore.transform.position;
            pos.y -= 45 * i;
            playerScore.transform.position = pos;
            playerScore.GetComponent<PlayerScoreScript>().text.color = turnManager.players[i - 1].GetComponent<SpriteRenderer>().color;

            // Asetetaan jokaiselle tekstille player script seurattavaksi
            playerScore.GetComponent<PlayerScoreScript>().AI = turnManager.players[i - 1].GetComponent<PlayerController>();
            i++;
        }

    }

    void Update()
    {
        
    }
}
