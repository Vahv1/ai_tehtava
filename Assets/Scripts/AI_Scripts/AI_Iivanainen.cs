using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Iivanainen : PlayerControllerInterface
{
    //[Range(0, 1)] [SerializeField] float chaosInBehavior = 0.1f;  // Määrittelee RNG käytöksen
    protected Dictionary<string, PositionInformation> visitedLocations = new Dictionary<string, PositionInformation>();

    protected string directionToMoveTo = "";

    protected float chaosFactorInMovement = 0.4f;

    public override void DecideNextMove()
    {
        AddPosition();  //Lisätään nykyinen paikka käytyihin lokaatioihin (jos ei vielä lisätty)


        int status = GetForwardTileStatus();

        // Ensisijaisesti päätetään hyökkäyksestä:

        if (GetHP() == 1 || IsForwardEnemyFacingMe() || GetEnemyHP(GetForwardPosition()) == 1)
        {
            nextMove = Hit;
            return;
        }

        // Alussa on mahdollisuus liikkua kaaoottisesti

        float random = Random.Range(0, 100);

        if (GetForwardTileStatus() == 0 && random < chaosFactorInMovement * 100)
        {
            nextMove = MoveForward;
            return;
        }
        if (random < chaosFactorInMovement * 100)
        {
            nextMove = TurnLeft;
            return;
        }


        // Jos ollaan kääntymässä uuteen suuntaan, jatketaan kääntymistä

        if (!directionToMoveTo.Equals("") && !GetRotation().ToString().Equals(directionToMoveTo))
        {
            nextMove = TurnRight;
            return;
        }

        // Jos ollaan käännytty oikeaan suuntaan nollataan kääntyvä suunta

        if (GetRotation().ToString().Equals(directionToMoveTo))
        {
            directionToMoveTo = "";
        }


        // Jos liikutaan, halutaan liikkua eteenpäin uuteen ruutuun (gotta go fast -prinsiippi)

        if (status == 0 && !IsLocationVisited(GetForwardPosition()))  //Jos voidaan mennä eteenpäin, eikä olla vielä oltu täällä
        {
            nextMove = MoveForward;
            visitedLocations[GetPosition().ToString()].RegisterDirectionTravelled(GetRotation());  //rekisteröidään kuljettu suunta
            return;
        }

        // Jos vastaan tulee seinä, rekisteröidään se kyseiseen ruutuun

        if (status == 1)
        {
            visitedLocations[GetPosition().ToString()].RegisterUnableToMove(GetRotation());
        }

        // Jos ollaan jo oltu täällä selvitetään voidaanko liikkua uusiin alueisiin

        if (status == 0 && IsLocationVisited(GetForwardPosition()) && visitedLocations[GetForwardPosition().ToString()].AreThereUnexploredDirections())
        {
            nextMove = MoveForward;
            return;
        }



        // Selvitetään onko vielä suuntia joihin liikkua, jos on niin asetetaan se tavoitteeksi

        string direction = visitedLocations[GetPosition().ToString()].UnexploredDirection();


        if (!direction.Equals(""))
        {
            directionToMoveTo = direction;
            nextMove = TurnRight;
            return;
        }







    }

    // Apumetodeja

    private void AddPosition()
    {
        string key = GetPosition().ToString();

        if (visitedLocations.ContainsKey(key)) return;
        visitedLocations.Add(key, new PositionInformation());
    }


    private bool IsLocationVisited(Vector2 position)
    {
        return (visitedLocations.ContainsKey(position.ToString()));
    }


    private Vector2 GetForwardPosition()
    {
        return GetPosition() + GetRotation();
    }


    private bool IsForwardEnemyFacingMe()
    {
        Vector2 enemy = GetEnemyRotation(GetForwardPosition());
        Vector2 player = GetRotation();

        if (enemy + player == Vector2.zero) return true;
        return false;
    }

}


// Apuluokka joka pitää yllä tietoa siitä, mihin suuntiin lokaatiossa on yritetty liikkua

public class PositionInformation
{
    int[] visitedDirections;

    public PositionInformation()
    {
        visitedDirections = new int[] { 0, 0, 0, 0 };  // RIGHT, DOWN, LEFT, UP. Negative value means impossible to visit
    }

    // Rekisteröi nykyiseen ruutuun suunnan, johon liikuttiin
    public void RegisterDirectionTravelled(Vector2 vector)
    {
        string vString = vector.ToString();

        switch (vString)
        {
            case "(1.0, 0.0)":
                visitedDirections[0] = 1;   // Visited right
                break;
            case "(0.0, -1.0)":
                visitedDirections[1] = 1;   // Visited down
                break;
            case "(-1.0, 0.0)":
                visitedDirections[2] = 1;   // Visited left
                break;
            case "(0.0, 1.0)":
                visitedDirections[3] = 1;   //Visited up
                break;
        }
        return;
    }

    //Rekisteröi tiedon, että ei voida liikkua tästä ruudusta tähän suuntaan
    public void RegisterUnableToMove(Vector2 vector)
    {
        string vString = vector.ToString();

        switch (vString)
        {
            case "(1.0, 0.0)":
                visitedDirections[0] = -1;   // Can't visit right
                break;
            case "(0.0, -1.0)":
                visitedDirections[1] = -1;   // Can't visit down
                break;
            case "(-1.0, 0.0)":
                visitedDirections[2] = -1;   // Can't visit left
                break;
            case "(0.0, 1.0)":
                visitedDirections[3] = -1;   //Can't visit up
                break;
        }
        return;
    }


    // Kertoo ollaanko ruudusta yritetty vielä liikkua eri suuntiin
    public bool AreThereUnexploredDirections()
    {
        foreach (int explored in visitedDirections)
        {
            if (explored == 0)
            {
                return true;
            }
        }
        return false;
    }

    // Kertoo mihin suuntaan voidaan vielä yrittää liikkua (hirveetä spagettikoodia)
    public string UnexploredDirection()
    {
        if (visitedDirections[0] == 0) return "(1.0, 0.0)";
        if (visitedDirections[1] == 0) return "(0.0, -1.0)";
        if (visitedDirections[2] == 0) return "(-1.0, 0.0)";
        if (visitedDirections[3] == 0) return "(0.0, 1.0)";

        return "";
    }

}