using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreScript : MonoBehaviour
{
    public PlayerController AI;
    public Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        // P‰ivitet‰‰n tekstiin pelaajan nimi ja tapot/kuolemat
        if (AI != null)
        {
            text.text = AI.gameObject.name + " " + AI.killCount + "/" + AI.deathCount;
        }
    }
}
