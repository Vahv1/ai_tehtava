using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Rasmus : PlayerControllerInterface
{
    // Muuttuja joka kertoo mihin pelaaja katsoi viime kierroksella
    private Vector2 lastRotation;
    // Lippu joka kertoo kutsutaanko funktiota ensimm�ist� kertaa.
    private bool flgFirstTime = true;
    // Muuttuja joka kertoo montako kertaa per�kk�in pelaaja on t�rm�nnyt sein��n.
    private int wallCollosionCount = 0;
    // Muuttuja johon rakennellaan karttaa vihollisen sek� omien liikkeiden perusteella.
    private Dictionary<Vector2, int> mapNodes;
    public override void DecideNextMove()
    {
        if (this.flgFirstTime)
        {
            //Ensimm�isell� kerralla alustetaan joitain muuttujia.
            this.flgFirstTime = false;
            this.lastRotation = GetRotation();
            this.mapNodes = new Dictionary<Vector2, int>();
        }
        // Pelaajan edess� olevan "tiilen" tyypin.
        int status = GetForwardTileStatus();
        // Oman pelaajan suuntaus.
        Vector2 currentRotation = GetRotation();
        // Oman pelaajan sijainti.
        Vector2 currentPosition = GetPosition();
        currentPosition = new Vector2
        (
            // Pakko tehd� t�mm�nen py�ristely, kun tuo Vector2 mik� saadaa GetPosition metodista,
            // saattaa palauttaa esim 0,5 sijasta esim 0,50000004, jonka vuoksi tarkistelut ei onnistu.
            (float)decimal.Round((decimal)currentPosition.x, 1),
            (float)decimal.Round((decimal)currentPosition.y, 1)
        );
        // T�nne kasataan vihollisten sijainnit avain-arvo muodossa, jotta s��styt��n ylim��r�isilt� luupeilta. Sijainti toimii avaimena t�ss� tapauksessa.
        Dictionary<Vector2, int> enemyPositions = new Dictionary<Vector2, int>();
        // Vihollisten sijainnit. 
        Vector2[] rows = GetEnemyPositions();
        // K�yd��n vihollisten sijainnit l�pi
        for (int i = 0; rows.Length > i; i++)
        {
            Vector2 row = new Vector2
            (
               //Pakko tehd� t�mm�nen py�ristely, kun tuo Vector2 mik� saadaa GetEnemyPositions metodista
               //saattaa palauttaa esim 0,5 sijasta esim 0,50000004, jonka vuoksi tarkistelut ei onnistu.
               (float)decimal.Round((decimal)rows[i].x, 1),
               (float)decimal.Round((decimal)rows[i].y, 1)
            );
            // Asetetaan vihollisen sijainti avaimeksi. 1 kertoo ett� arvo l�ytyy.
            enemyPositions[row] = 1;
            // Kartoitetaan tyhji� alueita kartalla, vastustajien liikkeiden avulla.
            this.mapNodes[row] = 0;
        }
        //My�s pelaajan liikkeill� kartoitetaan tyji� alueita kartalla.
        this.mapNodes[currentPosition] = 0;
        // Tarkastellaan l�ytyyk� l�hist�lt� vihollisia.
        ArrayList EnemyFound = this.CheckSurroundings(currentPosition, enemyPositions);
        // jos edess� oleva kentt� ei ole sein�, niin tyhjennet��n sein��nt�rm�yslaskuri
        if (status != 1)
            this.wallCollosionCount = 0;

        switch (status)
        {
            // 2 = Vihollinen
            case 2:
                //Jos vihollinen on suoraan edess�, niin ly�d��n sit� aina.
                nextMove = Hit;
                break;
            // 1 = Sein� 
            case 1:
                //nTrun:  1 = k��nn�s oikealle, 2 = k��nn�s vasemmalle
                int nTurn1 = 0;
                int eFound1 = (int)EnemyFound[0];
                if (eFound1 != 0)
                {
                    // Ensisijaisesti k�nnyt��n kohti vihollista, jos se on vieress�.
                    nTurn1 = this.MoveTowardsEnemy(eFound1);
                }
                else
                {
                    // Jos vihollista ei l�ydy vierest�, niin... 
                    if (this.wallCollosionCount == 0)
                    {
                        // Ensimm�isell� kerralla arvotaan k��ntymissuunta.
                        nTurn1 = Random.Range(1, 3);
                    }
                    else
                    {
                        // Jos sein��n on t�rm�tty jo kertaalleen, niin ei en�� arvota k��ntymissuuntaa. 
                        // K��nnyt��n t�ll�in samaan suuntaan mihin viime kierroksellakin k��nnyttiin.
                        // (x saattaa palauttaa arvon infinity, jonka vuoksi pit�� muuttaa x,y -arvot muotoon int, jolloin infinity muutetaan arvoksi 0.)
                        int cx = (int)currentRotation.x;
                        int cy = (int)currentRotation.y;
                        int lx = (int)this.lastRotation.x;
                        int ly = (int)this.lastRotation.y;
                        if (lx == 0 && ly == 1) // Pelaajan suunta on yl�s.
                            nTurn1 = ((cx == 1 && cy == 0) ? 1 : 2);
                        else if (lx == 0 && ly == -1) // Pelaajan suunta on alas
                            nTurn1 = ((cx == -1 && cy == 0) ? 1 : 2);
                        else if (lx == 1 && ly == 0) // Pelaajan suunta on oikealle
                            nTurn1 = ((cx == 0 && cy == -1) ? 1 : 2);
                        else // Pelaajan suunta on vasemmalle
                            nTurn1 = ((cx == 0 && cy == 1) ? 1 : 2);
                    }
                }
                if (nTurn1 == 1)
                    nextMove = TurnRight;
                else
                    nextMove = TurnLeft;

                this.wallCollosionCount++;
                break;

            // 0 = Tyhj�
            default:
                int nTurnD = 0;
                int eFoundD = (int)EnemyFound[0];
                if (eFoundD != 0)
                {
                    // Vihollinen on vieress�. Tarkastetaan mihin suuntaa pit�� k��nty�, jotta voidaan ly�d� sit�.
                    nTurnD = this.MoveTowardsEnemy(eFoundD);
                }
                else
                {
                    // Periaatteessa pelaajan ei pit�isi j��d� ikuisiin luuppeihin py�rim��n tuon case 1 ansiosta,
                    // mutta pelataan viel� varman p��lle lis��m�ll� t�nnekkin satunnaisuutta.
                    if (Random.Range(0, 60) == 30)
                        nTurnD = Random.Range(1, 3);
                }

                if (nTurnD == 1)
                    nextMove = TurnRight;
                else if (nTurnD == 2)
                    nextMove = TurnLeft;
                else
                    nextMove = MoveForward;
                break;
        }
        this.lastRotation = currentRotation;
    }
    /*
    * Tarkastellaan ymp�rist��, eli l�ytyyk� l�hell� olevista ruuduista vihollisia.
    * palautetaan array list, jossa:
    *   avaimella 0 l�ytyy vihollisen sijainti pelaajaan n�hden: 0 = vihollisia ei ole l�hist�ll�, 1 = oikealla, 2 = ylh��ll�, 3 = alapuolella, 4 = vasemmalla
    *   avaimella 1 l�ytyy vihollisen sijainti x,y-aksellilla Vector2-muodossa.
    **/
    private ArrayList CheckSurroundings(Vector2 playerPosition, Dictionary<Vector2, int> enemyPositions)
    {
        ArrayList ret = new ArrayList();

        int val = 0;
        float px = playerPosition.x;
        float py = playerPosition.y;
        // L�ytyyk� oikealta vihollista?
        Vector2 Pos1 = new Vector2(px + 1, py);
        if (enemyPositions.TryGetValue(Pos1, out val))
        {
            ret.Add(1);
            ret.Add(Pos1);
            return ret;
        }
        // L�ytyyk� ylh��lt� vihollista?
        Vector2 Pos2 = new Vector2(px, py + 1);
        if (enemyPositions.TryGetValue(Pos2, out val))
        {
            ret.Add(2);
            ret.Add(Pos2);
            return ret;
        }
        // L�ytyyk� alapuolelta vihollista?
        Vector2 Pos3 = new Vector2(px, py - 1);
        if (enemyPositions.TryGetValue(Pos3, out val))
        {
            ret.Add(3);
            ret.Add(Pos3);
            return ret;
        }
        // L�ytyyk� vasemmalta vihollista?
        Vector2 Pos4 = new Vector2(px - 1, py);
        if (enemyPositions.TryGetValue(Pos4, out val))
        {
            ret.Add(4);
            ret.Add(Pos4);
            return ret;
        }
        //Vihollisia ei l�ytynyt.
        ret.Add(0);
        ret.Add(new Vector2());
        return ret;
    }
    /**
     * Lasketaan ett� mihin suuntaan pelaajan pit�� k��nty�, ett� se kohtaa vihollisen.
     * 0 : Ei tarvitse k��nty�
     * 1 : Oikealle
     * 2 : Vasemmalle
     **/
    private int MoveTowardsEnemy(int enemyPosition)
    {
        int ret = 0;
        Vector2 playerRotation = GetRotation();
        int prx = (int)playerRotation.x;
        int pry = (int)playerRotation.y;
        if (prx == 0 && pry == 1) // Pelaajan suunta on yl�s.
        {
            if (enemyPosition != 2)
                ret = (enemyPosition == 1 ? 1 : 2);
        }
        else if (prx == 0 && pry == -1) // Pelaajan suunta on alas
        {
            if (enemyPosition != 3)
                ret = (enemyPosition == 4 ? 1 : 2);
        }
        else if (prx == 1 && pry == 0) // Pelaajan suunta on oikealle
        {
            if (enemyPosition != 1)
                ret = (enemyPosition == 3 ? 1 : 2);
        }
        else // Pelaajan suunta on vasemmalle
        {
            if (enemyPosition != 4)
                ret = (enemyPosition == 2 ? 1 : 2);
        }
        return ret;
    }
}