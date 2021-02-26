using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generoi pelikentän ja asettaa pelaajien lähtöpaikat
// Pitää tallessa tietoa pelikentän ruutujen tilasta
public class GridManager : MonoBehaviour
{
    public int[,] grid; // Pelikentän ruudukko
    public GameObject playerParent; // Objekti jonka lapsia pelaajat on
    public int size, columns, rows;

    // Prefabit joita kentälle laitetaan
    public GameObject wall;
    void Awake()
    {
        size = (int)Camera.main.orthographicSize;
        rows = size * 2;
        columns = size * 2;
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
                    int rnd = UnityEngine.Random.Range(0, 7); // millä mahiksella tulee seinä
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
            SpawnPlayer(child);
        }
    }

    // Spawnaa seinän annettuun grid sijaintiin
    void SpawnWall(int x, int y)
    {
        Vector2 pos = new Vector2(x - (size - 0.5f), y - (size - 0.5f)); // sijainti keskelle oikeaa ruutua
        Instantiate(wall, pos, Quaternion.identity);
    }

    // Spawnaa pelaajan randomiin tyhjään ruutuun
    public void SpawnPlayer(Transform player)
    {
        // OIKEE MEKANIIKKA
        int rndX = 0;
        int rndY = 0;
        while (grid[rndX, rndY] != 0)
        {
            rndX = UnityEngine.Random.Range(1, columns - 1);
            rndY = UnityEngine.Random.Range(1, rows - 1);
        }
        grid[rndX, rndY] = 2;

        Vector2 pos = new Vector2(rndX - (size - 0.5f), rndY - (size - 0.5f)); // sijainti keskelle oikeaa ruutua
        player.position = pos;
    }
}
