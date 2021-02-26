using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

// Hoitaa pelaajien siirtojen toteutuksen oikeaan aikaan ja ilmoittaa pelaajille kun vuoro on p‰‰ttynyt
// Miten pit‰isi p‰‰tt‰‰ kuka pelaaja toteuttaa siirron ekana?
public class TurnManager : MonoBehaviour
{
    // Delegaatti, jolla pelaajascriptit saa tiet‰‰ millon vuoro on p‰‰ttynyt
    public delegate void TurnEndDelegate();
    public static TurnEndDelegate turnEndDelegate;

    public float turnDuration = 3f;
    public float nextMoveTime;

    // Taulukko johon tallennetaan kaikki pelaajaobjektit
    //public GameObject[] players;
    public List<GameObject> players = new List<GameObject>();

    private void Start()
    {
        nextMoveTime = turnDuration; // Aloitetaan eka vuoro vasta yhden vuoron keston j‰lkeen
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
    }

    void Update()
    {
        // Aloitetaan vuoro aina kun nextMoveTime ylitetty
        if (nextMoveTime <= Time.time)
        {
            ExecuteTurn();
        }
    }

    // Suoritetaan vuoro eli toteutetaan kaikkien pelaajien sirrot ja ilmoitetaan vuoron p‰‰ttymisest‰
    private void ExecuteTurn()
    {
        nextMoveTime = Time.time + turnDuration;

        // Suorita kaikkien pelaajien p‰‰tt‰m‰ seuraava siirto
        foreach (GameObject player in players)
        {
            try
            {
                player.GetComponent<PlayerControllerInterface>().nextMove();
            }
            catch
            {
                Debug.Log("Seuraavaa siirtoa ei ole asetettu " + player.gameObject.name);
            }
        }

        // Vaihdetaan randomilla pelaajalistan j‰rjestyst‰ (vuoroj‰rjestys)
        System.Random rnd = new System.Random();
        players = players.OrderBy(a => rnd.Next()).ToList();

        turnEndDelegate();
    }
}
