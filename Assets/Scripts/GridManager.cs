using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int[,] grid;
    public GameObject playerParent;
    int vertical, horizontal, columns, rows;
    public GameObject wall;
    public GameObject player;

    void Start()
    {
        vertical = (int)Camera.main.orthographicSize;
        horizontal = vertical * (Screen.width / Screen.height);
        rows = vertical * 2;
        columns = horizontal * 2;
        grid = new int[columns, rows];

        // Pelikentän generointi. Gridin arvo 0 = tyhjä, 1 = seinä, 2 = pelaaja.
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = 0;

                // seinät kenttien laidoille
                if (i == columns-1 || i == 0 || j == rows - 1 || j == 0)
                {
                    grid[i, j] = 1;
                }
                // muualle randomilla
                else
                {
                    int rnd = UnityEngine.Random.Range(0, 5); // 20% mahis tulla seinä
                    grid[i, j] = rnd == 1 ? 1 : 0;
                }

                if (grid[i, j] == 1)
                {
                    SpawnWall(i, j);
                }
            }
        }

        // Pelaajien spawnaus
        foreach (Transform child in playerParent.transform)
        {
            int rndX = 0;
            int rndY = 0;
            while (grid[rndX, rndY] != 0)
            {
                rndX = UnityEngine.Random.Range(1, columns - 1);
                rndY = UnityEngine.Random.Range(1, rows - 1);
            }
            grid[rndX, rndY] = 2;
            SpawnPlayer(child, rndX, rndY);
        }
    }

    // Spawnaa seinän annettuun grid sijaintiin
    void SpawnWall(int x, int y)
    {
        Vector2 pos = new Vector2(x - (horizontal - 0.5f), y - (vertical - 0.5f)); // sijainti keskelle oikeaa ruutua
        Instantiate(wall, pos, Quaternion.identity);
    }

    // Spawnaa pelaajan annettuun grid sijaintiin
    void SpawnPlayer(Transform player, int x, int y)
    {
        Vector2 pos = new Vector2(x - (horizontal - 0.5f), y - (vertical - 0.5f)); // sijainti keskelle oikeaa ruutua
        player.position = pos;
    }
}
