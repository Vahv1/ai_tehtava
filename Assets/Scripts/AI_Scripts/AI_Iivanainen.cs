using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Iivanainen : PlayerControllerInterface
{
    //[Range(0, 1)] [SerializeField] float chaosInBehavior = 0.1f;  // M��rittelee RNG k�yt�ksen
    protected Dictionary<string, PositionInformation> visitedLocations = new Dictionary<string, PositionInformation>();

    protected string directionToMoveTo = "";

    protected float chaosFactorInMovement = 0.4f;

    public override void DecideNextMove()
    {
        AddPosition();  //Lis�t��n nykyinen paikka k�ytyihin lokaatioihin (jos ei viel� lis�tty)


        int status = GetForwardTileStatus();

        // Ensisijaisesti p��tet��n hy�kk�yksest�:

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


        // Jos ollaan k��ntym�ss� uuteen suuntaan, jatketaan k��ntymist�

        if (!directionToMoveTo.Equals("") && !GetRotation().ToString().Equals(directionToMoveTo))
        {
            nextMove = TurnRight;
            return;
        }

        // Jos ollaan k��nnytty oikeaan suuntaan nollataan k��ntyv� suunta

        if (GetRotation().ToString().Equals(directionToMoveTo))
        {
            directionToMoveTo = "";
        }


        // Jos liikutaan, halutaan liikkua eteenp�in uuteen ruutuun (gotta go fast -prinsiippi)

        if (status == 0 && !IsLocationVisited(GetForwardPosition()))  //Jos voidaan menn� eteenp�in, eik� olla viel� oltu t��ll�
        {
            nextMove = MoveForward;
            visitedLocations[GetPosition().ToString()].RegisterDirectionTravelled(GetRotation());  //rekister�id��n kuljettu suunta
            return;
        }

        // Jos vastaan tulee sein�, rekister�id��n se kyseiseen ruutuun

        if (status == 1)
        {
            visitedLocations[GetPosition().ToString()].RegisterUnableToMove(GetRotation());
        }

        // Jos ollaan jo oltu t��ll� selvitet��n voidaanko liikkua uusiin alueisiin

        if (status == 0 && IsLocationVisited(GetForwardPosition()) && visitedLocations[GetForwardPosition().ToString()].AreThereUnexploredDirections())
        {
            nextMove = MoveForward;
            return;
        }



        // Selvitet��n onko viel� suuntia joihin liikkua, jos on niin asetetaan se tavoitteeksi

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


// Apuluokka joka pit�� yll� tietoa siit�, mihin suuntiin lokaatiossa on yritetty liikkua

public class PositionInformation
{
    int[] visitedDirections;

    public PositionInformation()
    {
        visitedDirections = new int[] { 0, 0, 0, 0 };  // RIGHT, DOWN, LEFT, UP. Negative value means impossible to visit
    }

    // Rekister�i nykyiseen ruutuun suunnan, johon liikuttiin
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

    //Rekister�i tiedon, ett� ei voida liikkua t�st� ruudusta t�h�n suuntaan
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


    // Kertoo ollaanko ruudusta yritetty viel� liikkua eri suuntiin
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

    // Kertoo mihin suuntaan voidaan viel� yritt�� liikkua (hirveet� spagettikoodia)
    public string UnexploredDirection()
    {
        if (visitedDirections[0] == 0) return "(1.0, 0.0)";
        if (visitedDirections[1] == 0) return "(0.0, -1.0)";
        if (visitedDirections[2] == 0) return "(-1.0, 0.0)";
        if (visitedDirections[3] == 0) return "(0.0, 1.0)";

        return "";
    }

}