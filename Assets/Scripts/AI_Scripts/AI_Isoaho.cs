using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Isoaho : PlayerControllerInterface
{
    // TÄMÄ TULEE TEHTÄVÄSSÄ TÄYDENTÄÄ
    // Käytä vain PlayerControllerInterfacessa olevia metodeja TIMissä olevan ohjeistuksen mukaan

    // Pakota eteen tarkoittaa, että tekoäly vaatii itseltään, että seuraava siirto on eteen, ei väliä mikä on
    private bool pakota_eteen = false;
    /* edellinen liike
     * 0 = lyö
     * 1 = eteen
     * 2 = vasempaan
     * 3 = oikeaan
     */
    private int edellinen = 0;
    private int seuraava = 0;

    public override void DecideNextMove()
    {
        // Jos edessä on vihollinen, lyödään sitä
        if (GetForwardTileStatus() == 2)
        {
            seuraava = 0;
        }
        // Jos tekoäly on käskenyt itseään liikkumaan eteen ja se on mahdollista
        else if (pakota_eteen && GetForwardTileStatus() == 0)
        {
            seuraava = 1;
        }
        // Muutoin keksitään muuta tähteellisempää tekemistä
        else
        {
            suunnistaUhriin(etsiUhri());
        }

        // Lopuksi katsotaan tilakoneesta mitä tekoäly tahtoikaan tehdä
        switch (seuraava)
        {
            // 0: Tahdon lyödä!
            case 0:
                Hit();
                break;
            // Tahdon liikkua eteenpäin!
            case 1:
                pakota_eteen = false;
                MoveForward();
                break;
            // Tahdon kääntyä vasempaan!
            case 2:
                // Kunhan en kääntynyt äsken oikeaan, niin voin kääntyä vasempaan (Loopin esto)
                if (edellinen != 3)
                    TurnLeft();
                else
                    // Koska tahdoin kääntyä, niin käännytään sitten vielä oikeaan, kun kerta viimeksikin käännyin
                    TurnRight();
                break;
            // Muut tarkoittaa, että tahdoin kääntyä oikeaan
            default:
                // Käännyinkö vasempaan jo? Jos en, voin kääntyä oikeaan.
                if (edellinen != 2)
                    TurnRight();
                // Käännyin äsken vasempaan, joten käännyn uudestaan.
                else
                    TurnLeft();
                break;

        }
        // Asetetaan edellinen liike ja lasketaan askelia lisää.
        edellinen = seuraava;
    }

    /// <summary>
    /// Funktio, jossa päätetään mihin suuntaan yritetään päästä.
    /// </summary>
    /// <param name="uhri">Kohteeksi valittu piste.</param>
    private void suunnistaUhriin(Vector2 uhri)
    {
        Vector2 mina = GetPosition();

        // Suunnistetaan kohti uhria
        Vector2 suunta = (uhri - mina).normalized;
        float posX = Mathf.Abs(suunta.x);
        float posY = Mathf.Abs(suunta.y);
        // Jos normalisoidun vektorin X on suurempi kuin Y, 
        // kannattaa liikkua Vas.-Oik. suuntiin
        if (posX > posY)
        {
            // Positiivinen: suunnistettava oikealle
            if (suunta.x > 0)
                suunnistaOikealle(uhri);
            // Muutoin liikuttava vas.
            else
                suunnistaVasemmalle(uhri);
        }
        else
        {
            if (suunta.y > 0)
                suunnistaYlos(uhri);
            else
                suunnistaAlas(uhri);
        }

    }
    /// <summary>
    /// Kohde on oikealla päin. Yritetään suunnistaa sinne.
    /// </summary>
    /// <param name="uhri">Kohteeksi valittu piste</param>
    private void suunnistaOikealle(Vector2 uhri)
    {
        if (GetRotation().x == 1)
        {
            // Oikea
            if (GetForwardTileStatus() == 0)
            {
                seuraava = 1;
            }
            else
            {
                // Mitä jos seinä?
                pakota_eteen = true;
                kaannyRandomSuuntaan();
            }
        }
        else if (GetRotation().x == -1)
        {
            // Vasen
            kaannyRandomSuuntaan();
        }
        else if (GetRotation().y == 1)
        {
            // Ylös
            seuraava = 3;
        }
        else
        {
            // Alas
            seuraava = 2;
        }
    }
    private void suunnistaVasemmalle(Vector2 uhri)
    {
        if (GetRotation().x == 1)
        {
            // Oikea
            kaannyRandomSuuntaan();
        }
        else if (GetRotation().x == -1)
        {
            // Vasen
            if (GetForwardTileStatus() == 0)
            {
                seuraava = 1;
            }
            else
            {
                // Mitä jos seinä?
                pakota_eteen = true;
                kaannyRandomSuuntaan();
            }
        }
        else if (GetRotation().y == 1)
        {
            // Ylös
            seuraava = 2;
        }
        else
        {
            // Alas
            seuraava = 3;
        }
    }

    private void suunnistaYlos(Vector2 uhri)
    {
        if (GetRotation().x == 1)
        {
            // Oikea
            seuraava = 2;
        }
        else if (GetRotation().x == -1)
        {
            // Vasen
            seuraava = 3;
        }
        else
        {
            // Ylös
            if (GetForwardTileStatus() == 0)
            {
                seuraava = 1;
            }
            else
            {
                // Mitä jos seinä?
                pakota_eteen = true;
                kaannyRandomSuuntaan();
            }
        }
    }

    private void suunnistaAlas(Vector2 uhri)
    {
        if (GetRotation().x == 1)
        {
            // Oikea
            seuraava = 3;
        }
        else if (GetRotation().x == -1)
        {
            // Vasen
            seuraava = 2;
        }
        else
        {
            // Alas
            if (GetForwardTileStatus() == 0)
            {
                seuraava = 1;
            }
            else
            {
                // Mitä jos seinä?
                pakota_eteen = true;
                kaannyRandomSuuntaan();
            }
        }
    }

    private void kaannyRandomSuuntaan()
    {
        if (Random.value <= 0.5)
        {
            seuraava = 2;
        }
        else
        {
            seuraava = 3;
        }
    }
    /// <summary>
    ///     etsiUhri() etsii matalimmalla elämäpistetasolla olevista vihollisista lähimmän ja palauttaa sen sijainnin.
    /// </summary>
    /// <returns>Palauttaa sijainnin, jossa on matalimmalla elämäpistetasolla oleva vihollinen, joka on lähimpänä.</returns>
    private Vector2 etsiUhri()
    {
        // Ensin listataan kaikki viholliset, joista etsitään matalimman elämän viholliset
        Vector2[] viholliset = GetEnemyPositions();
        // Botin maksimielämät ovat 3, joten siihen on hyvä verrata
        int matalinElama = 3;
        // Käydään kaikki viholliset läpi
        for (int i = 0; i < viholliset.Length; i++)
        {
            if (GetEnemyHP(viholliset[i]) < matalinElama)
                matalinElama = GetEnemyHP(viholliset[i]);
        }
        // Haetaan vihollisista matalimman elämän viholliset
        List<Vector2> potentiaalisetUhrit = new List<Vector2>();
        for (int i = 0; i < viholliset.Length; i++)
        {
            if (GetEnemyHP(viholliset[i]) == matalinElama)
                potentiaalisetUhrit.Add(viholliset[i]);
        }
        // Otetaan talteen oma sijainti, sekä etäisyys ensimmäiseen listallaolijaan
        Vector2 mina = GetPosition();
        double matalinEtaisyys = Vector2.Distance(potentiaalisetUhrit[0], mina);
        Vector2 uhri = potentiaalisetUhrit[0];
        // Etsitään, onko kukaan lähempänä kuin ensimmäinen.
        potentiaalisetUhrit.ForEach(kohde =>
        {
            // Jos etäisyys on pienempi, niin ollaan lähimpänä tähän asti
            if (Vector2.Distance(kohde, mina) < matalinEtaisyys)
            {
                uhri = kohde;
                matalinEtaisyys = Vector2.Distance(kohde, mina);
            }
        });
        return uhri;
    }

}