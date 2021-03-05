using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pf = NesScripts.Controls.PathFind;

// Generoi pelikent‰n ja asettaa pelaajien l‰htˆpaikat
// Pit‰‰ tallessa tietoa pelikent‰n ruutujen tilasta
public class GridManager : MonoBehaviour
{
    public float[,] grid; // Pelikent‰n ruudukko
    public GameObject playerParent; // Objekti jonka lapsia pelaajat on
    public int size, columns, rows;
    public GameObject wall; // Sein‰-prefab joita kent‰lle asetetaan

    // Luodaan lista trap pointeista, joihin ei haluta spawnaa pelaajia
    List<(int, int)> trapPointList = new List<(int, int)>();

    public List<pf.Point> polku; // tallentaa polun kahden ruudun v‰lill‰
    pf.Point alwaysEmpty = new pf.Point(2, 2); // Piste joka j‰tet‰‰n tyhj‰ksi aina

    void Awake()
    {
        size = (int)Camera.main.orthographicSize;
        rows = size * 2;
        columns = size * 2;
        grid = new float[columns, rows];

        // Generoidaan pelikentt‰
        GenerateGrid();
        // K‰‰nnet‰‰n pelikentt‰ gridin 0-arvot ykkˆsiksi ja toisin p‰in pathfindingia varten
        pf.Grid pfGrid = ReverseGrid(grid);
        // T‰ytet‰‰n trapPointList pisteill‰ joihin ei ole pathfindingin mukaan p‰‰sy‰
        GenerateTrapPointList(pfGrid);

        // Pelaajien spawnaus
        foreach (Transform child in playerParent.transform)
        {
            SpawnPlayer(child);
        }
    }

    // Generoidaan pelikent‰n
    void GenerateGrid()
    {
        // Gridin arvo 0 = tyhj‰, 1 = sein‰, 2 = pelaaja
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = 0;

                // sein‰t kenttien laidoille
                if (i == columns - 1 || i == 0 || j == rows - 1 || j == 0)
                {
                    grid[i, j] = 1;
                }
                // muualle randomilla
                else
                {
                    int rnd = UnityEngine.Random.Range(0, 7); // mill‰ mahiksella tulee sein‰
                    grid[i, j] = rnd == 1 ? 1 : 0;
                }

                // Teippiviritelm‰, j‰tet‰‰n 2,2 tyhj‰ks ja katotaan sen kautta ettei oo 'trappeja' kent‰ll‰
                // T‰‰ luottaa siihen, ett‰ ei tuu trappialuetta, jonka sis‰ll‰ ois (2,2) ja (2,3)
                grid[2, 2] = 0;
                grid[2, 3] = 0;

                if (grid[i, j] == 1)
                {
                    SpawnWall(i, j);
                }
            }
        }
    }

    // T‰ytt‰‰ trapPointListin pisteill‰ joihin ei ole p‰‰sy‰ alwaysEmpty-pisteest‰
    void GenerateTrapPointList(pf.Grid pfGrid)
    {
        // K‰yd‰‰n kaikki tyhj‰t ruudut l‰vite
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Etsit‰‰n polku jokaisesta tyhj‰st‰ ruudusta alwaysEmpty ruutuun
                // --> saadaan tieto onko paikkoja jotka eiv‰t ole yhteydess‰ alwaysEmptyyn
                // huom. teippiviritelm‰: jos alwaysEmpty on trappialueella niin kaikki pelaajat spawnataan sinne
                if (grid[i, j] == 0)
                {
                    pf.Point pt = new pf.Point(i, j);
                    polku = pf.Pathfinding.FindPath(pfGrid, alwaysEmpty, pt);
                    if (polku.Count < 1)
                    {
                        trapPointList.Add((i, j));
                    }
                }
            }
        }
    }

    // Vaihtaa griding kakkoset ja nollat ykkˆsiksi sek‰ ykkˆset nolliksi
    // Pathfindingille 0 = blokattu ruutu
    pf.Grid ReverseGrid(float[,] oldGrid)
    {
        // K‰‰nnet‰‰n gridin arvot toisin p‰in pathfindia varten (0 = blokattu ruutu)
        // ja tehd‰‰n niist‰ Pathfind-Grid
        float[,] reverseGrid = new float[columns, rows];
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (oldGrid[i, j] == 2) { reverseGrid[i, j] = 1; }
                else { reverseGrid[i, j] = 1 - oldGrid[i, j]; }
            }
        }
        return new pf.Grid(reverseGrid);
    }

    // Spawnaa sein‰n annettuun grid sijaintiin
    void SpawnWall(int x, int y)
    {
        Vector2 pos = new Vector2(x - (size - 0.5f), y - (size - 0.5f)); // sijainti keskelle oikeaa ruutua
        Instantiate(wall, pos, Quaternion.identity);
    }

    // Spawnaa pelaajan randomiin tyhj‰‰n ruutuun
    public void SpawnPlayer(Transform player)
    {
        int rndX = 0;
        int rndY = 0;
        // Arvotaan uudestaan jos ruutu on muu kuin tyhj‰ tai jos ruutu on trapPoint-listalla
        while (grid[rndX, rndY] != 0 || trapPointList.Contains((rndX, rndY)))
        {
            rndX = UnityEngine.Random.Range(1, columns - 1);
            rndY = UnityEngine.Random.Range(1, rows - 1);
        }
        grid[rndX, rndY] = 2;
        Vector2 pos = new Vector2(rndX - (size - 0.5f), rndY - (size - 0.5f)); // sijainti keskelle oikeaa ruutua
        player.position = pos;
    }
}
