using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    // nextMoveen tallennetaan metodi joka halutaan suorittaa seuraavalla vuorolla
    public delegate void NextMove();
    public NextMove nextMove;

    public GridManager gridManager;

    void Start()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();

        // P��tet��n seuraava siirto aina kun TurnManager ilmoittaa vuoron p��ttyneen
        TurnManager.turnEndDelegate += DecideNextMove;
        // P��tet��n ensimm�inen siirto
        DecideNextMove();
    }

    // T�m� on funktio, joka pit�� teht�v�ss� kehitt�� itse
    private void DecideNextMove()
    {
        Debug.Log("Mietit��n seuraavaa siirtoa...");

        // Testi� varten ottaa nyt vaan random siirron
        int rnd = UnityEngine.Random.Range(1, 5);
        if (rnd == 5) { nextMove = TurnRight; }
        else if (rnd == 6) { nextMove = TurnLeft; }
        else { nextMove = MoveForward; }

        Debug.Log("Siirto mietitty");
    }

    // -------- FUNKTIOT JOITA TEHT�V�SS� SAA K�YTT�� ------- //
    // TODO joku getTileStatus joka palauttaa tiedon onko halutussa ruudussa tyhj�, sein� vai pelaaja
    // Pit�sk� viel� piilottaa n�� varsinaiset toiminnat niin ois selkeempi tekij�ille eli t�ss�
    // vaan kutsuttas samannimist� funktioo jostain toisesta skriptist� jossa ois oikee toiminta
    private void MoveForward()
    { 
        if (GetPositionStatus(transform.position + transform.right) != 0) 
        {
            Debug.Log("EI VOI LIIKKUA KOSKA RUUTU T�YNN�");
        }
        else
        {
            Vector2 oldPos = transform.position;
            gameObject.transform.position += transform.right;
            UpdatePlayerPosition(oldPos);
        }
    }

    private void TurnLeft()
    {
        transform.Rotate(0, 0, 90, Space.World);
    }

    private void TurnRight()
    {
        transform.Rotate(0, 0, 90, Space.World);
    }


    // ------ MUUT FUNKTIOT ------ //
    // N�� varmaan pit�s olla lopullisessa sit jossain muualla
    
    //Antaa maailmasijainnin gridisijaintina
    private int[] WorldPosToGridPos(Vector2 worldPos)
    {
        int[] gridPos = { 0, 0 };
        gridPos[0] = Convert.ToInt32(24.5f + worldPos.x); //todo 24.5 ei pit�s olla kovakoodattu
        gridPos[1] = Convert.ToInt32(24.5f + worldPos.y);
        return gridPos;
    }

    // Antaa maailmasijainnista tiedon onko ruudussa tyhj�, sein� vai pelaaja
    private int GetPositionStatus(Vector2 worldPos)
    {
        Vector2 forwardTilePos = transform.position + transform.right;
        int[] forwardTileGridPos = WorldPosToGridPos(forwardTilePos);
        int positionStatus = gridManager.grid[forwardTileGridPos[0], forwardTileGridPos[1]];
        return positionStatus;
    }

    //P�ivitt�� pelaajan sijainnin gridiin, parametri on pelaajan sijainti aiemmin
    private void UpdatePlayerPosition(Vector2 oldPosition)
    {
        // Tyjennet��n vanhan sijainnin ruutu
        int[] oldGridPos = WorldPosToGridPos(oldPosition);
        gridManager.grid[oldGridPos[0], oldGridPos[1]] = 0;

        // T�ytet��n uuden sijainnin ruutu
        int[] newGridPos = WorldPosToGridPos(transform.position);
        gridManager.grid[newGridPos[0], newGridPos[1]] = 2;
    }
}
