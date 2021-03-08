using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Pollari : PlayerControllerInterface
{   // Pollaribotin apumuuttujat
    private Vector2 omasijainti;
    private Vector2 omasuunta;

    private Vector2[] vihut;
    private Vector2[] seinat;

    private bool seinaeessa;
    private bool vihuTulossaEteen;
    private Vector2 jahtipaikka;

    private int vaaraTaso; // Pienempi pahempi

    private bool lyotiinTyhjaa = false;
    private int tyhjalyonnit = 0;
    private int passiivisiirrot = 0;

    private Vector2 vihuOikealla = new Vector2(-100, 0);
    private Vector2 vihuVasemmalla = new Vector2(-100, 0);
    private Vector2 vihuTakana = new Vector2(-100, 0);
    private Vector2 vihuEdessa = new Vector2(-100, 0);

    // P‰‰tt‰‰ seuraavan siirron
    public override void DecideNextMove()
    {
        Analysoi(); // Ker‰t‰‰n tarvittavat tiedot ymp‰ristˆst‰
        lyotiinTyhjaa = !lyotiinTyhjaa; // Asetetaan tyhj‰lyˆnti = false

        if (vaaraTaso == 0) Vaara0(); // Suoritetaan vaaratason 0 mukainen toiminto
        else if (vaaraTaso == 1) Vaara1(); // Suoritetaan vaaratason 1 mukainen toiminto
        else if (vaaraTaso == 2) Vaara2(); // Suoritetaan vaaratason 2 mukainen toiminto
        else if (vaaraTaso == 3) Vaara3(); // Suoritetaan vaaratason 3 mukainen toiminto

        if (lyotiinTyhjaa == false) tyhjalyonnit = 0; // Jos t‰ll‰ vuorolla ei lyˆty tyhj‰‰, resetoidaan tyhj‰lyˆnnit

    }

    // ======================== Toimintometodit ========================

    // Vaaratason 0 toiminnot. T‰m‰ on paha, kuolema tulossa, ja jotain on teht‰v‰
    private void Vaara0()
    {
        // V‰istet‰‰n tappavaa iskua, jos edess‰ tyhj‰‰, eik‰ kukaan ole liikkumassa tielle
        if (GetForwardTileStatus() == 0 && vihuTulossaEteen == false) nextMove = MoveForward;

        // Jos uhataan sek‰ sivusta, ett‰ edest‰, lyˆd‰‰n edess‰ olevaa vihua
        else if (GetForwardTileStatus() == 2 && UhkaVuorot(omasijainti + omasuunta) == 0) nextMove = Hit;

        // Jos uhataan molemmista sivuista, ja eteen liikkumassa vihu, yritet‰‰n liikkua eteen p‰in
        else if (UhkaVuorot(Oikearuutu()) == 0 && UhkaVuorot(Vasenruutu()) == 0) nextMove = MoveForward;

        // Jos uhataan takaa, ja edess‰ vihu, lyˆd‰‰n vihua
        else if (GetForwardTileStatus() == 2 && UhkaVuorot(omasijainti - omasuunta) == 0) nextMove = Hit;

        // Jos uhataan sivusta, muutei takaa, eteen liikkumassa vihu, ja sivusta uhkaavalla liikaa hp:ta, yritet‰‰n livahtaa eteen
        else if (UhkaVuorot(omasijainti - omasuunta) >= 0 && GetForwardTileStatus() == 0 && vihuTulossaEteen == true && SivuillaEnemm‰nHPta()) nextMove = MoveForward;

        // Jos uhataan sivusta, muutei takaa, ja eteen liikkumassa vihu, k‰‰nnyt‰‰n sivusta uhkaavan suuntaan
        else if (UhkaVuorot(omasijainti - omasuunta) >= 0 && GetForwardTileStatus() == 0 && vihuTulossaEteen == true) KaannyVihunSuuntaan();

        // Jos uhataan takaa, ja eteen liikkumassa vihu, yritet‰‰n liikkua eteen p‰in
        else if (UhkaVuorot(omasijainti - omasuunta) == 0 && GetForwardTileStatus() == 0 && vihuTulossaEteen == true) nextMove = MoveForward;

        // Jos uhataan sivusta / takaa, ja sein‰ tai vihu edess‰, k‰‰nnyt‰‰n sivusta/takaa uhkaavan vihollisen suuntaan.
        else if (GetForwardTileStatus() >= 1) KaannyVihunSuuntaan();

        // Muutoin siirryt‰‰n seuraavan vaaratason toimintaj‰rjestykseen
        else Vaara1();
    }

    // Vaaratason 1 toiminnot. T‰ss‰ ehtii viel‰ tehd‰ jotain, mutta tilanne ei ole kovin mieluisa
    private void Vaara1()
    {
        // Jos 1 iskulla tuhottava vihollinen edess‰, tuhotaan se.
        if (vaaraTaso >= 1 && GetForwardTileStatus() == 2 && (UhkaVuorot(omasijainti + omasuunta) > 0 || GetEnemyHP(omasijainti + omasuunta) == 1)) nextMove = Hit;

        // Liikutaan suoraan, jos edess‰ tyhj‰‰, kukaan ei ole liikkumassa tielle, tai tulossa viistosta kylkeen
        else if (GetForwardTileStatus() == 0 && vihuTulossaEteen == false && !Viistouhka()) nextMove = MoveForward;

        // Muutoin siirryt‰‰n seuraavan vaaratason toimintaj‰rjestykseen
        else Vaara2();
    }

    // Vaaratason 2 toiminnot. Nimi saattaa olla h‰m‰‰v‰, sill‰ t‰m‰ ei ole oikeasti vaara Pollaribotille, vaan sille onnettomalle, joka on k‰‰nt‰nyt selk‰ns‰ Pollaribotille
    private void Vaara2()
    {
        // Jos edess‰ vihu, niin painitaan
        if (GetForwardTileStatus() == 2) nextMove = Hit;

        // Jos vihu pakenemassa jahtipaikalla, kukaan ei tulossa tielle tai l‰hestym‰ss‰ viistosta, jahdataan
        else if (Onkovihu(jahtipaikka) && GetEnemyRotation(jahtipaikka) == omasuunta && vihuTulossaEteen == false && !Viistouhka()) nextMove = MoveForward;

        // Jos onneton botti on oikealla, k‰‰nnyt‰‰n oikealle
        else if (UhkaVuorot(Oikearuutu()) <= 2) nextMove = TurnRight;

        // Jos onneton botti on vasemmalla, k‰‰nnyt‰‰n vasempaan
        else if (UhkaVuorot(Vasenruutu()) <= 2) nextMove = TurnLeft;

        // Muutoin siirryt‰‰n seuraavan vaaratason toimintaj‰rjestykseen
        else Vaara3();
    }

    // Vaaratason 3 toiminnot. T‰ss‰h‰n ei ole oikeasti vaaraa, navidoidaan kartalla
    private void Vaara3()
    {
        // Jos edess‰ vihu, niin painitaan
        if (GetForwardTileStatus() == 2) nextMove = Hit;

        // Jos edess‰ tyhj‰‰, ja joku tulossa eteen, lyˆd‰‰n sit‰. Kolmen tyhj‰lyˆnnin j‰lkeen keksit‰‰n jotain muuta 
        else if (GetForwardTileStatus() == 0 && vihuTulossaEteen == true && tyhjalyonnit < 3)
        {
            lyotiinTyhjaa = true;
            tyhjalyonnit += 1;
            nextMove = Hit;
        }

        // K‰‰nnyt‰‰n ennakoimaan vierelle tuleva vihu
        else if (tulossaViereen() == 1) nextMove = TurnRight;
        else if (tulossaViereen() == 2) nextMove = TurnLeft;

        // Kolmen tyhj‰lyˆnnin j‰lkeen k‰‰nnyt‰‰n
        else if (GetForwardTileStatus() == 0 && vihuTulossaEteen == true) Turn();

        // Jos vihu pakenemassa jahtipaikalla, kukaan ei tulossa tielle tai l‰hestym‰ss‰ viistosta, jahdataan
        else if (Onkovihu(jahtipaikka) && GetEnemyRotation(jahtipaikka) == omasuunta && vihuTulossaEteen == false && !Viistouhka()) nextMove = MoveForward;

        // Jos passiivisiirtoja enemm‰n kuin vihollisia, k‰‰nnyt‰‰n sinne, miss‰ niit‰ on eniten, ellei edess‰ ole sein‰‰
        else if (passiivisiirrot > vihut.Length)
        {
            passiivisiirrot = 0;
            Turn();
        }

        // Jos edess‰ sein‰, k‰‰nnyt‰‰n. Lasketaan passiivisiirroksi
        else if (GetForwardTileStatus() == 1)
        {
            passiivisiirrot++;
            Turn();
        }

        // Jos edess‰ tyhj‰‰, eik‰ kukaan tulossa eteen tai l‰hestym‰ss‰ viistosta, liikutaan eteen. Lasketaan passiivisiirroksi
        else if (GetForwardTileStatus() == 0 && vihuTulossaEteen == false && !Viistouhka())
        {
            passiivisiirrot++;
            nextMove = MoveForward;
        }

        else if (!Kannoilla()) nextMove = Pass; // Muuten passataan, jos vihua ei oo per‰ss‰

        else Turn(); // Muuten k‰‰nnyt‰‰n
    }


    // ======================== Toimintojen Apumetodit ========================

    // Tallentaa tiedot ymp‰ristˆst‰
    private void Analysoi()
    {
        omasijainti = GetPosition();    // Tallennetaan muutama muuttuja yms myˆhemp‰‰ k‰yttˆ‰ varten
        omasuunta = GetRotation();
        vihut = GetEnemyPositions();
        vihuTulossaEteen = JokuTulossaEteen();
        // Tallennetaan jahtipaikka
        jahtipaikka = new Vector2((Mathf.Round((omasijainti.x + omasuunta.x + omasuunta.x + omasuunta.x) * 100f) / 100f), (Mathf.Round((omasijainti.y + omasuunta.y + omasuunta.y + omasuunta.y) * 100f) / 100f));

        if (GetForwardTileStatus() == 1) Tallennaseina(); // Jos edess‰ on sein‰, tallennetaan se muistiin

        vaaraTaso = 3; // Asettaa vaaratasoksi 3 ja myˆhemmin pienemm‰ksi sen mukaan, kuinka vihuja lˆytyy sivuilta ja takaa

        // Jos edess‰ vihollinen, kirjataan ylˆs. Muuten asetetaan vihuEdess‰ arvoksi (-100, 0)
        if (Onkovihu(omasijainti + omasuunta)) vihuEdessa = new Vector2(omasijainti.x + omasuunta.x, omasijainti.y + omasuunta.y);
        else vihuEdessa = new Vector2(-100, 0);

        // Seuraavissa kohdissa tarkistetaan ymp‰rˆiv‰t viholliset, ja m‰‰ritell‰‰n "vaarataso" niiden suutien mukaan
        if (Onkovihu(omasijainti - omasuunta)) // Tarkistetaan takaa
        {
            vihuTakana = new Vector2(omasijainti.x - omasuunta.x, omasijainti.y - omasuunta.y);
            if (vaaraTaso > UhkaVuorot(vihuTakana)) vaaraTaso = UhkaVuorot(vihuTakana);
        }
        else vihuTakana = new Vector2(-100, 0);

        if (Onkovihu(Vasenruutu())) // Tarkistetaan vasemmalta
        {
            vihuVasemmalla = Vasenruutu();
            if (vaaraTaso > UhkaVuorot(vihuVasemmalla)) vaaraTaso = UhkaVuorot(vihuVasemmalla);
        }
        else vihuVasemmalla = new Vector2(-100, 0);

        if (Onkovihu(Oikearuutu())) // Tarkistetaan oikealta
        {
            vihuOikealla = Oikearuutu();
            if (vaaraTaso > UhkaVuorot(vihuOikealla)) vaaraTaso = UhkaVuorot(vihuOikealla);
        }
        else vihuOikealla = new Vector2(-100, 0);
    }

    // Palauttaa true, jos vasemmalla ja oikealla olevien vihollisten hp:t ovat suuremmat tai yht‰ suuret, kuin omat hp:t
    private bool SivuillaEnemm‰nHPta()
    {
        int vihuHPt = 0;
        if (GetEnemyHP(Vasenruutu()) > 0) vihuHPt += GetEnemyHP(Vasenruutu());
        if (GetEnemyHP(Oikearuutu()) > 0) vihuHPt += GetEnemyHP(Oikearuutu());
        if (vihuHPt >= GetHP()) return true;
        else return false;

    }

    // M‰‰ritt‰‰ seuraavaksi siirroksi vierest‰ tappavasti uhkaavan vihollisen suuntaan k‰‰ntymisen, jos sellaisen tiedet‰‰n olevan olemassa
    private void KaannyVihunSuuntaan()
    {
        if (UhkaVuorot(Oikearuutu()) == 0) nextMove = TurnRight; // Jos tappavasti uhkaava vihu on oikealla, k‰‰nnyt‰‰n sinne
        else nextMove = TurnLeft; // Muuten k‰‰nnyt‰‰n vasempaan
    }

    // K‰‰nnyt‰‰n sinne, miss‰ vihuja on enemm‰n, mutta ensisijaisesti erisuuntaan, kuin vierell‰ sijaitseva sein‰, jos sellainen on muistissa
    private void Turn()
    {
        if (Seinapaikalla(Oikearuutu())) nextMove = TurnLeft; // Jos sein‰ on oikealla, k‰‰nnyt‰‰n vasempaan
        else if (Seinapaikalla(Vasenruutu())) nextMove = TurnRight; // Jos sein‰ vasemmalla, k‰‰nnyt‰‰n oikealle
        else TurnVihujenMukaan(); // Jos ei seini‰, k‰‰nnyt‰‰n sinne, miss‰ vihuja eniten
    }

    // K‰‰nnyt‰‰n sinne, miss‰ vihuja on eniten
    private void TurnVihujenMukaan()
    {
        float vasemmalle = 0; // N‰it‰ t‰ydennell‰‰n alla myˆhemp‰‰ vertailua varten
        float oikealle = 0;

        if (Mathf.Abs(omasuunta.x) < 0.1f) // Jos liikutaan y-akselin suuntaan
        {
            for (int i = 0; i < vihut.Length; i++)
            {   // Verrataan jokaisen vihollisen sijaintia x-akselilla, ja kirjataan tulokset
                if (vihut[i].x < omasijainti.x) vasemmalle = vasemmalle + omasuunta.y;
                if (vihut[i].x > omasijainti.x) oikealle = oikealle + omasuunta.y;
            }
        }

        else if (Mathf.Abs(omasuunta.y) < 0.1f) // Jos liikutaan x-akselin suuntaan
        {
            for (int i = 0; i < vihut.Length; i++)
            {   // Verrataan jokaisen vihollisen sijaintia y-akselilla, ja kirjataan tulokset
                if (vihut[i].y > omasijainti.y) vasemmalle = vasemmalle + omasuunta.x;
                if (vihut[i].y < omasijainti.y) oikealle = oikealle + omasuunta.x;
            }
        }

        // Valitaan suunta kirjattujen tulosten mukaan
        if (vasemmalle > oikealle) nextMove = TurnLeft;
        else nextMove = TurnRight;

    }

    // ======================== Analysoinnin Apumetodit ========================


    // Palauttaa true, jos annetulla paikalla on muistiin merkitty sein‰
    private bool Seinapaikalla(Vector2 paikka)
    {
        if (seinat == null) return false;
        for (int i = 0; i < seinat.Length; i++) // K‰y l‰pi kaikki muistiin merkityt sein‰t
        {
            if (paikka == seinat[i]) return true; // Jos matchi lˆytyy, palauttaa true
        }
        return false; // Muuten palautetaan false
    }

    // Tallentaa edess‰ olevan sein‰n muistiin
    private void Tallennaseina()
    {
        if (!Seinapaikalla(omasijainti + omasuunta)) // Skipataan koko sein‰n tallennus, jos edess‰ oleva sein‰ on jo muistissa
        {
            if (seinat == null)
            {
                seinat = new Vector2[] // Jos seini‰ ei ole ennest‰‰n kirjattuna, luodaan uusi taulukko niille, ja kirjataan ensimm‰inen ylˆs
                {
                    new Vector2(omasijainti.x + omasuunta.x, omasijainti.y + omasuunta.y)
                };
            }
            else
            {   // Muuten lis‰t‰‰n uusi sein‰ listaan pienell‰ listakikkailulla
                Vector2[] uudetSeinat = new Vector2[seinat.Length + 1]; // Luodaan uus lista
                for (int i = 0; i < seinat.Length; i++)
                {
                    uudetSeinat[i] = seinat[i]; // T‰ydennet‰‰n uus lista vanhan alkioilla
                }
                uudetSeinat[seinat.Length] = new Vector2(omasijainti.x + omasuunta.x, omasijainti.y + omasuunta.y); // Lis‰t‰‰n uuteen listaan uus sein‰
                seinat = uudetSeinat; // Vanha lista onki nyt uus lista
            }
        }

    }

    // Palauttaa, montako vuoroa viereisell‰ vihollisella kest‰‰ k‰‰nty‰ kohti pelaajaa, ja 3, jos ruudussa ei ole vihollista
    private int UhkaVuorot(Vector2 vihusijainti)
    {
        if (Onkovihu(vihusijainti) == false) return 3; // Palauttaa 3, jos annetussa ruudussa ei ole vihollista

        // Hirveet Mathf.Abs-kikkailut, koska floateista ei saa vertailtua tarkkoja arvoja. Katotaan, t‰ht‰‰kˆ vihu Pollaribottia
        else if ((Mathf.Abs((vihusijainti.x + GetEnemyRotation(vihusijainti).x) - omasijainti.x) < 0.1f)
                && Mathf.Abs((vihusijainti.y + GetEnemyRotation(vihusijainti).y) - omasijainti.y) < 0.1f)
        {
            return 0; //  Pelaaja vihun edess‰ -> 0
        }

        // Hirveet Mathf.Abs-kikkailut, koska floateista ei saa vertailtua tarkkoja arvoja. Katotaan, onko vihun selk‰ kohti Pollaribottia
        else if ((Mathf.Abs((vihusijainti.x - GetEnemyRotation(vihusijainti).x) - omasijainti.x) < 0.1f)
                && Mathf.Abs((vihusijainti.y - GetEnemyRotation(vihusijainti).y) - omasijainti.y) < 0.1f)
        {
            return 2; //  Pelaaja vihun takana -> 2 
        }

        else return 1; // Muuten palautetaan 1. T‰llˆin vihun kyljen pit‰isi olla kohti Pollaribottia
    }

    // Palauttaa true, jos toinen vihu katsoo Pollaribotin edess‰ olevaa ruutua
    private bool JokuTulossaEteen()
    {
        for (int i = 0; i < vihut.Length; i++)
        {
            if (vihut[i] + GetEnemyRotation(vihut[i]) == omasijainti + omasuunta) return true;
        }
        return false;
    }

    // Tarkistaa, onko annetussa paikassa vihua
    private bool Onkovihu(Vector2 paikka)
    {
        for (int i = 0; i < vihut.Length; i++) // Selataan vihut l‰pi
        {
            if (paikka == vihut[i]) return true; // True, jos matchi lˆytyy
        }
        return false; // Muuten false
    }

    // Palauttaa vasemman puoleisen ruudun koordinaatin
    // N‰iss‰ hirveet Mathf.Roundailut, jotta palautetut vektorit ois vertailukelposia
    private Vector2 Vasenruutu()
    {
        // Jos liikkuu y-akselin suunnassa
        if (Mathf.Abs(omasuunta.x) < 0.1f) return new Vector2((Mathf.Round((omasijainti.x - omasuunta.y) * 100f) / 100f), (Mathf.Round(omasijainti.y * 100f) / 100f));
        // Jos liikku x-akselin suunnassa
        else return new Vector2((Mathf.Round(omasijainti.x * 100f) / 100f), (Mathf.Round((omasijainti.y + omasuunta.x) * 100f) / 100f));
    }

    // Palauttaa oikean puoleisen ruudun koordinaatin
    private Vector2 Oikearuutu()
    {
        // Jos liikkuu y-akselin suunnassa
        if (Mathf.Abs(omasuunta.x) < 0.1f) return new Vector2((Mathf.Round((omasijainti.x + omasuunta.y) * 100f) / 100f), (Mathf.Round(omasijainti.y * 100f) / 100f));
        // Jos liikku x-akselin suunnassa
        else return new Vector2((Mathf.Round(omasijainti.x * 100f) / 100f), (Mathf.Round((omasijainti.y - omasuunta.x) * 100f) / 100f));
    }

    // Palauttaa true, jos vihollinen on liikkumassa viistosta tappavaan asemaan
    private bool Viistouhka()
    {
        bool xSuunnassa;
        Vector2 vasenUhka;
        Vector2 oikeaUhka;

        // Katsotaan, liikutaanko x-akselin suunnassa
        if (Mathf.Abs(omasuunta.x) < 0.1f) xSuunnassa = true;
        else xSuunnassa = false;

        // Etsit‰‰n vasenUhka-ruutu
        if (xSuunnassa) vasenUhka = new Vector2((Mathf.Round((omasijainti.x - omasuunta.y - omasuunta.y) * 100f) / 100f),
                                                                    (Mathf.Round((omasijainti.y + omasuunta.y) * 100f) / 100f));
        // Jos liikutaan y-akselin suuntaan, yll‰ oleva koodirivi lˆyt‰‰ vasenUhka-ruudun, jos ei, alla oleva koodirivi lˆyt‰‰ vasenUhka-ruudun
        else vasenUhka = new Vector2((Mathf.Round((omasijainti.x + omasuunta.x) * 100f) / 100f),
                                    (Mathf.Round((omasijainti.y + omasuunta.x + omasuunta.x) * 100f) / 100f));

        // Etsit‰‰n oikeaUhka-ruutu
        if (xSuunnassa) oikeaUhka = new Vector2((Mathf.Round((omasijainti.x + omasuunta.y + omasuunta.y) * 100f) / 100f),
                                                                    (Mathf.Round((omasijainti.y + omasuunta.y) * 100f) / 100f));
        // Jos liikutaan y-akselin suuntaan, yll‰ oleva koodirivi lˆyt‰‰ vasenUhka-ruudun, jos ei, alla oleva koodirivi lˆyt‰‰ vasenUhka-ruudun
        else oikeaUhka = new Vector2((Mathf.Round((omasijainti.x + omasuunta.x) * 100f) / 100f),
                                    (Mathf.Round((omasijainti.y - omasuunta.x - omasuunta.x) * 100f) / 100f));

        // Tarkistaa, jos vasenUhka-ruudun vihu l‰hestyy
        if (Onkovihu(vasenUhka) && (vasenUhka + GetEnemyRotation(vasenUhka) + GetEnemyRotation(vasenUhka)) == (omasijainti + omasuunta))
        {
            if (!Seinapaikalla(vasenUhka + GetEnemyRotation(vasenUhka))) return true; // Jos vasenUhka-ruudun vihun edess‰ ei ole muistiin merkitty sein‰, palautetaan true
        }

        // Tarkistaa, jos oikeaUhka-ruudun vihu l‰hestyy
        else if (Onkovihu(oikeaUhka) && (oikeaUhka + GetEnemyRotation(oikeaUhka) + GetEnemyRotation(oikeaUhka)) == (omasijainti + omasuunta))
        {
            if (!Seinapaikalla(oikeaUhka + GetEnemyRotation(oikeaUhka))) return true; // Jos vasenUhka-ruudun vihun edess‰ ei ole muistiin merkitty sein‰, palautetaan true
        }

        return false; // Muuten ei uhkaa
    }

    // Palauttaa true, jos vihu kannoilla
    private bool Kannoilla()
    {
        // Tallennetaan ylˆs tarkasteltavat ruudut
        Vector2 kahenPaassa = new Vector2((Mathf.Round((omasijainti.x - omasuunta.x - omasuunta.x) * 100f) / 100f),
                                            (Mathf.Round((omasijainti.y - omasuunta.y - omasuunta.y) * 100f) / 100f));
        Vector2 kolmenPaassa = new Vector2((Mathf.Round((omasijainti.x - omasuunta.x - omasuunta.x - omasuunta.x) * 100f) / 100f),
                                            (Mathf.Round((omasijainti.y - omasuunta.y - omasuunta.y - omasuunta.y) * 100f) / 100f));

        // Jos vihu on kahen tai kolmen ruudun p‰‰ss‰ takana, matkalla samaan suuntaan, eik‰ v‰liss‰ ole sein‰‰, palauttaa true
        if (Onkovihu(kahenPaassa) && GetEnemyRotation(kahenPaassa) == omasuunta && !Seinapaikalla(omasijainti - omasuunta)) return true;
        if (Onkovihu(kolmenPaassa) && GetEnemyRotation(kolmenPaassa) == omasuunta && !Seinapaikalla(omasijainti - omasuunta) && !Seinapaikalla(kahenPaassa)) return true;
        return false; // Muuten palauttaa false
    }

    // Palauttaa 1, jos vihu on tulossa oikealle, 2 jos vasemmalle, ja 0, jos vihollista ei ole tulossa viereen
    private int tulossaViereen()
    {
        // P‰invastaista suuntaa kuvaava vektori
        Vector2 vastaSuunta = new Vector2((Mathf.Round((-omasuunta.x) * 100f) / 100f), (Mathf.Round((-omasuunta.y) * 100f) / 100f));

        Vector2[] viistoRuudut = new Vector2[4] // Taulukko viistoruuduille
                {
                    new Vector2(omasijainti.x + 1, omasijainti.y + 1),
                    new Vector2(omasijainti.x - 1, omasijainti.y + 1),
                    new Vector2(omasijainti.x + 1, omasijainti.y - 1),
                    new Vector2(omasijainti.x - 1, omasijainti.y - 1)
                };

        for (int i = 0; i < viistoRuudut.Length; i++) // Tarkistetaan kaikki viistoruudut
        {
            if (!Seinapaikalla(Vasenruutu()) && Onkovihu(viistoRuudut[i]) && (viistoRuudut[i] + GetEnemyRotation(viistoRuudut[i]) == Vasenruutu())) return 2;
            if (!Seinapaikalla(Oikearuutu()) && Onkovihu(viistoRuudut[i]) && (viistoRuudut[i] + GetEnemyRotation(viistoRuudut[i]) == Oikearuutu())) return 1;
        }

        return 0; // Muuten ei uhkaa
    }
}