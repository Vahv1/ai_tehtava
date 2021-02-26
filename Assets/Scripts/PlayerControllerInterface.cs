using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerInterface : MonoBehaviour
{
    private PlayerController playerController;

    // nextMoveen tallennetaan metodi joka halutaan suorittaa seuraavalla vuorolla
    public delegate void NextMove();
    public NextMove nextMove;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Tämä metodi ylikirjoitetaan omassa skriptissä
    public virtual void DecideNextMove()
    {

    }

    // --- NÄITÄ METODEJA SAA KÄYTTÄÄ OMASSA SKRIPTISSÄ ---
    // Liikkuu yhden ruudun eteenpäin
    public void MoveForward()
    {
        playerController.MoveForward();
    }

    // Kääntyy 90 astetta vastapäivään
    public void TurnLeft()
    {
        playerController.TurnLeft();
    }

    // Kääntyy 90 astetta myötäpäivään
    public void TurnRight()
    {
        playerController.TurnRight();
    }

    // Lyö edessä olevaa kohdetta
    public void Hit()
    {
        playerController.Hit();
    }

    // Skippaa vuoron eli ei tee mitään
    public void Pass()
    {
        playerController.Pass();
    }

    // Antaa tiedon onko pelaajan edessä olevassa ruudussa tyhjä, seinä vai pelaaja
    // 0 = tyhjä, 1 = seinä, 2 = pelaaja
    public int GetForwardTileStatus()
    {
        return playerController.GetForwardTileStatus();
    }

    // Palauttaa oman sijainnin pelimaailmassa 2D-vektorina
    public Vector2 GetPosition()
    {
        return playerController.GetPosition();
    }

    // Palauttaa oman rotaation vektorina
    // (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = ylös | (0,-1) = alas
    public Vector2 GetRotation()
    {
        return playerController.GetRotation();
    }

    // Palauttaa oman HP-määrän
    public int GetHP()
    {
        return playerController.GetHP();
    }

    // Palauttaa listan, jossa on kaikkien vihollisten sijainti pelimaailmassa
    public Vector2[] GetEnemyPositions()
    {
        return playerController.GetEnemyPositions();
    }

    // Palauttaa annetussa sijainnissa olevan vihollisen rotaation vektorina
    // (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = ylös | (0,-1) = alas
    // Palauttaa (0, 0) jos sijainnissa ei ole vihollista
    public Vector2 GetEnemyRotation(Vector2 enemyPos)
    {
        return playerController.GetEnemyRotation(enemyPos);
    }

    // Palauttaa annetussa sijainnissa olevan vihollisen HP-määrän
    // Palauttaa -1 jos sijainnissa ei ole vihollista
    public int GetEnemyHP(Vector2 pos)
    {
        return playerController.GetEnemyHP(pos);
    }
}
