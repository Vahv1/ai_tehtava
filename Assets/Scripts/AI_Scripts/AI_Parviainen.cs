using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// @author Jussi Parviainen
/// @created 03.03.2021
/// Sykli4 AI hahmo.
/// https://tim.jyu.fi/view/kurssit/tie/peliteknologia/syklit-2021/sykli-4-pelitekoaly/tehtava#rajoitukset
/// Sallitut metodit:
/// - MoveForward(), liikkuu omalla vuorolla eteenp‰in yhden ruudun
/// - TurnLeft(), k‰‰ntyy omalla vuorolla vasempaan p‰in
/// - TurnRight(), k‰‰ntyy omalla vuorolla oikeaan p‰in
/// - Hit(), lyˆ edess‰ olevaa vastustajaa omalla vuorolla, jos vastustaja on suoraan kohti = 1 damage, jos lyˆnti tapahtuu vastustajan sivuilta tai takaa = insta kill
/// - Pass(), ei tee omalla vuorolla mit‰‰n
/// - GetForwardTileStatus(), 0 = tyhj‰, 1 = sein‰, 2 = pelaaja
/// - GetPosition(), oma sijainti
/// - GetRotation(), oma rotaatio:  (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = ylˆs | (0,-1) = alas
/// - GetHP(), oma el‰mien m‰‰r‰ (Max Health = 3)
/// - GetEnemyPositions(), Palauttaa kaikki vastustajien sijainnit
/// - GetEnemyRotation(Vector2 enemyPos), Palauttaa vastustajan rotaation: (1,0) = oikealle | (-1,0) = vasemmalle | (0,1) = ylˆs | (0,-1) = alas, (0,0) --> ei vastustajaa
/// - GetEnemyHP(Vector2 pos), Palauttaa vastustajan el‰mien m‰‰r‰n (Max Health = 3), palauttaa -1, jos vastustajaa ei ole kysytt‰v‰ss‰ sijainnissa
/// </summary>
public class AI_Parviainen : PlayerControllerInterface
{
    /// <summary>
    /// Luokka karttapisteelle, joka sis‰lt‰‰ osoittimen karttapalaan (chunk) ja siin‰ olevaan pisteeseen (point)
    /// Agentti pystyy k‰yt‰nnˆss‰ toimimaan miss‰ vain, kun A* algoritmiss‰ hyˆdynnett‰v‰ data tallentuu dynaamisesti lohkoihin.
    /// </summary>
    public class PathPoint
    {
        public Vector2Int chunk;
        public Vector2Int point;

        /// <summary>
        /// Olion muodostaja
        /// </summary>
        /// <param name="chunkIndex">Karttapalan indeksi</param>
        /// <param name="pointIndex">Karttapalassa olevan pisteen indeksi</param>
        public PathPoint(Vector2Int chunkIndex, Vector2Int pointIndex)
        {
            chunk = chunkIndex;
            point = pointIndex;
        }

        /// <summary>
        /// Vertailee kahta karttapistett‰ toisiinsa
        /// </summary>
        /// <param name="pathPoint">karttapiste</param>
        /// <returns>Palauttaa true tai false riippuen ovatko pisteet samat</returns>
        public bool Compare(PathPoint pathPoint)
        {
            if (chunk.Equals(pathPoint.chunk) && point.Equals(pathPoint.point)) return true;
            return false;
        }
    }


    /// <summary>
    /// Luokka A* algorimin Nodelle, joka tallettaa tietoa reitinetsint‰‰ varten
    /// </summary>
    private class A_Star_Node
    {
        public PathPoint pathPoint;
        public float g_score = float.MaxValue;
        public float f_score = float.MaxValue;
        public A_Star_Node came_from = null;
        public int came_from_dir = -1; // <-- osoittaa suoraan DIRECTIONS taulukkoon, josta saadaan tieto mist‰ suunnasta Nodeen tultiin

        /// <summary>
        /// Olion muodostaja, tarvitsee alkuun vain pathPointin
        /// </summary>
        /// <param name="pathPoint">sijainti, miss‰ Node sijaitsee</param>
        public A_Star_Node(PathPoint pathPoint)
        {
            this.pathPoint = pathPoint;
        }

        /// <summary>
        /// Muuntaa Noden Dictionaryssa k‰ytett‰v‰ksi avaimeksi ja palauttaa sen
        /// </summary>
        /// <returns>Muuntaa Noden Dictionaryssa k‰ytett‰v‰ksi avaimeksi ja palauttaa sen</returns>
        public string GetKey()
        {
            return pathPoint.chunk.x + "_" + pathPoint.chunk.y + "|" + "_" + pathPoint.point.x + "_" + pathPoint.point.y;
        }
    }

    // Debuggausta varten vapaasti s‰‰dett‰viss‰ olevat muuttujat:
    [Header("Debug Draw Gizmos:")]
    [SerializeField] bool drawMapChunks = false;
    [SerializeField] bool drawFoundMapPoints = false;
    [SerializeField] bool drawPlannedPath = false;
    [SerializeField] bool drawCurrentTarget = false;
    [SerializeField] bool drawFleeDirection = false;

    // Scriptiss‰ usein k‰ytetyt suunnat kovakoodattuna: 
    private enum Directions { Up, Right, Down, Left };
    private static readonly Vector2Int UP = new Vector2Int(0, 1);
    private static readonly Vector2Int RIGHT = new Vector2Int(1, 0);
    private static readonly Vector2Int DOWN = new Vector2Int(0, -1);
    private static readonly Vector2Int LEFT = new Vector2Int(-1, 0);
    private static readonly Vector2Int ZERO = new Vector2Int(0, 0);
    private static readonly Vector2Int[] DIRECTIONS = new Vector2Int[]
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    };

    // Levelin alue on tiedossa, joten hyˆdynnet‰‰n sit‰:
    static readonly Vector2 LEVEL_TOP_LEFT = new Vector2(-19.5f, 19.5f);
    static readonly Vector2 LEVEL_BOTTOM_RIGHT = new Vector2(19.5f, -19.5f);

    // Karttalohkojen koko ja k‰ytetyn gridin koko:
    readonly Vector2Int chunkSize = new Vector2Int(20, 20); // karttalohkon koko
    const float GRID_SIZE = 1f; // Gridin koko peliss‰.

    // Kutsutaan omaa Init metodia ensimm‰isen vuoron alussa: <-- alustetaan esimerkiksi levelin rajat esteiksi A* algorimi‰ varten
    bool initCalled = false;

    // Agentin toiminnot (vaellus, pakeneminen, jahtaaminen ja vihollisen lyˆminen):
    private enum Action { Wander, Flee, Chase, HitEnemy };
    private Action currentAction = Action.Wander;

    // Reitin haussa sek‰ p‰‰tˆksen teossa k‰ytett‰vi‰ muuttujia:
    public enum MapPoint { Unknown, Movable, Blocked }; // karttapiste on joko tuntematon, liikuttava tai siin‰ on este
    Dictionary<Vector2Int, MapPoint[,]> movementMap = new Dictionary<Vector2Int, MapPoint[,]>(); // Tallentaa karttapisteiden tietoja reitinsuunnittelua varten
    Dictionary<Vector2Int, float[,]> enemyAvoidanceMap = new Dictionary<Vector2Int, float[,]>(); // Dynaaminen kartta, joka kasvattaa painoarvoa reitinsuunnitteluun vihollisten ymp‰rille
    int pathPlanningEnemyAvoidRadius = 2; // <-- kasvattaa vihollisten painoarvoa reitinsuunnittelua varten t‰lt‰ et‰isyydelt‰
    Dictionary<Vector2Int, bool[,]> dynamicObstacleMap = new Dictionary<Vector2Int, bool[,]>(); // Dynaaminen estekartta, johon p‰ivitet‰‰n joka vuoron alussa vihollisten sijainnit.

    List<Vector2> enemiesToEvade = new List<Vector2>(); // ker‰t‰‰n vaaralliset viholliset talteen, joiden perusteella menn‰‰n karkuun
    bool isEnemyFront = false; // Lyˆnti m‰‰r‰ytyy trueksi, jos pelaajan edess‰ on vihollinen. Lyˆnti Overridaa kaikki pelaajan toiminnot ja pelaaja taisetelee kuolemaan asti.

    // Jos vihollinen on kahden ruudun p‰‰ss‰ pelaajasta, pakenemis toiminto triggeroituu, jos vastustajan suunta t‰sm‰‰ johonkin
    // matriisin indeksiss‰ X,Y osoitettuihin suuntiin. ZERO:n tapauksessa vertailu lopetetaan. Vastustajan sijainti
    // tulee peilata maskiin vertailua varten, jossa indeksi (2,2) on pelaajan oma sijainti ja sijainti (0,0) on matriisin vasen alanurkka.
    // Huom! Jos vastustaja on suoraan pelaajan edess‰, pelaaja taistelee kuolemaan asti ja Flee toiminto ei voi aktivoitua
    private readonly Vector2Int[,,] EvadeMask = new Vector2Int[5, 5, 3]
    {
        { {UP, RIGHT, ZERO}, {UP, RIGHT, ZERO}, {UP, RIGHT, DOWN}, {DOWN, RIGHT, ZERO}, {DOWN, RIGHT, ZERO} },
        { {UP, RIGHT, ZERO}, {UP, RIGHT, ZERO}, {UP, RIGHT, DOWN}, {DOWN, RIGHT, ZERO}, {DOWN, RIGHT, ZERO} },
        { {UP, LEFT, RIGHT}, {UP, LEFT, RIGHT}, {ZERO, ZERO, ZERO}, {DOWN, LEFT, RIGHT}, {DOWN, LEFT, RIGHT} },
        { {UP, LEFT, ZERO}, {UP, LEFT, ZERO}, {UP, LEFT, DOWN}, {DOWN, LEFT, ZERO}, {DOWN, LEFT, ZERO} },
        { {UP, LEFT, ZERO}, {UP, LEFT, ZERO}, {UP, LEFT, DOWN}, {DOWN, LEFT, ZERO}, {DOWN, LEFT, ZERO} }
    };

    // Jahtaamisessa k‰ytett‰v‰t parametrit:
    const int MAX_CHASE_TURNS = 50; // Rajoitetaan jahtaamista, jotta agentti ei yrit‰ jahdata esim. samaa vihollista loputtomasti
    int chase_action_counter = 0;
    int chase_coolDown_counter = 0;
    const int MIN_CHASE_COOLDOWN_TURNS = 10; // chase laskurin t‰yttyessa, pistet‰‰n agentti j‰‰hylle hetkellisesti.
    const int MAX_CHASE_COOLDOWN_TURNS = 50;
    PathPoint targetEnemyBehind = null; // Agentti pyrkii aina liikkumaan kohteen taakse
    PathPoint targetEnemy = null; // Kohteen oikea sijainti

    // Agentin nykyinen suunniteltu reitti:
    List<PathPoint> currentMovePath = new List<PathPoint>();


    Vector2 DebugDrawGizmosFleePos = new Vector2(0f, 0f);

    /// <summary>
    /// P‰‰tt‰‰ seuraavan toiminnon, jonka AI tekee
    /// </summary>
    public override void DecideNextMove()
    {
        // Kutsutaan initti‰, joka suoritetaan pelk‰st‰‰n ensimm‰isell‰ kierroksella:
        Init();

        // Toteutetaan karttojen p‰ivitys, josta saatavaa dataa k‰yetet‰‰n hyv‰ksi esim. reitinsuunnittelussa ja p‰‰tˆksen teossa:
        UpdateMaps();

        // P‰‰tet‰‰n strategia mit‰ agentti tulee k‰ytt‰m‰‰n:
        DecideAction();

        // Toteutetaan p‰‰tetty strategia:
        ExecuteAction();
    }


    /// <summary>
    /// Initialisoidaan aluksi agentin k‰ytt‰m‰ strategia vihollisten perusteella:
    /// </summary>
    void Init()
    {
        if (initCalled) return;
        initCalled = true;

        // Tied‰mme levelin rajat, joten p‰ivitet‰‰n tieto levelin liikkumiskarttaan
        for (int i = 0; i < 39; i++)
        {
            SetMapPointStatus(LEVEL_TOP_LEFT + new Vector2(i * GRID_SIZE, 0f), MapPoint.Blocked);
            SetMapPointStatus(new Vector2(LEVEL_BOTTOM_RIGHT.x, LEVEL_TOP_LEFT.y) + new Vector2(0f, -i * GRID_SIZE), MapPoint.Blocked);
            SetMapPointStatus(LEVEL_BOTTOM_RIGHT + new Vector2(-i * GRID_SIZE, 0f), MapPoint.Blocked);
            SetMapPointStatus(new Vector2(LEVEL_TOP_LEFT.x, LEVEL_BOTTOM_RIGHT.y) + new Vector2(0f, i * GRID_SIZE), MapPoint.Blocked);
        }

        chase_coolDown_counter = Random.Range(MIN_CHASE_COOLDOWN_TURNS, MAX_CHASE_COOLDOWN_TURNS + 1);
    }


    /// <summary>
    /// P‰ivitt‰‰ kaikki agentin hyˆdynt‰m‰t kartat
    /// </summary>
    void UpdateMaps()
    {
        // OMAN SIJAINNIN JA EDESSƒ OLEVAN TIEDON ANALYSOINTI -------------------------
        // - p‰ivitet‰‰n oma sijainti kartassa liikuttavaksi
        // - Otetaan tieto mit‰ agentin edess‰ on

        isEnemyFront = false;
        // Lis‰t‰‰n oma sijainti liikuttavaksi:
        SetMapPointStatus(GetPosition(), MapPoint.Movable);

        // Lis‰t‰‰n tieto edest‰ saatavan datan perusteella:
        if (GetForwardTileStatus() == 0 || GetForwardTileStatus() == 2)
        {
            SetMapPointStatus(GetPosition() + GetRotation(), MapPoint.Movable);
            if (GetForwardTileStatus() == 2) isEnemyFront = true;
        }
        else
        {
            SetMapPointStatus(GetPosition() + GetRotation() * GRID_SIZE, MapPoint.Blocked);
        }

        // VIHOLLISTEN LƒPIKƒYNTI ------------------------------
        //  - P‰ivitet‰‰n vihollisten sijainnit liikuttavaksi (reitinhaku paranee, jokaisella kierroksella)
        //  - P‰ivitet‰‰n vihollisten sijainnit dynaamisiksi esteiksi (niihin ei saa liikkua reitti‰ etsiess‰)
        //  - Katsotaan onko vastustaja mahdollinen uhka pelaajalle
        //  - Jos jahtaaminen on sallittua, etsit‰‰n l‰hin vihollinen, jonka kimppuun voi hyˆk‰t‰ takaa p‰in
        //  - Asetetaan vihollisten v‰lttelyn painoarvot reitinetsint‰‰n (pyrit‰‰n v‰ltt‰m‰‰n vihollisia reitti‰ suunnitellessa

        // v‰hennet‰‰n laskuria, joka tauottaa jahtaamista:
        if (chase_action_counter >= MAX_CHASE_TURNS)
        {
            chase_coolDown_counter--;
            if (chase_coolDown_counter <= 0)
            {
                chase_action_counter = 0;
                chase_coolDown_counter = Random.Range(MIN_CHASE_COOLDOWN_TURNS, MAX_CHASE_COOLDOWN_TURNS + 1);
            }
        }

        // resetoidaan agentin jahtaamisen kohde:
        targetEnemy = null;
        targetEnemyBehind = null;
        float closestTarget = float.MaxValue;

        // tyhj‰t‰‰n dynaamiset kartat/listat p‰ivityst‰ varten:
        enemyAvoidanceMap.Clear();
        dynamicObstacleMap.Clear();
        enemiesToEvade.Clear();

        // Otetaan vihollisen sijainnit ja k‰yd‰‰n ne l‰pi:
        Vector2[] enemyPositions = GetEnemyPositions();
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            // Varmuuden vuoksi tehd‰‰n varmistus, ett‰ ei varmasti ole agentti itse kyseess‰:
            if (CompareVector2(enemyPositions[i], GetPosition(), 0.1f)) continue;

            // Asetetaan karttapiste liikuttavaksi sek‰ asetetaan samalla vihollisen piste dynaamiseksi esteeksi:
            SetMapPointStatus(enemyPositions[i], MapPoint.Movable);
            SetDynamicObstacle(enemyPositions[i], true);

            // Katsotaan onko k‰sitelt‰v‰ vihollinen uhka: Jos on --> lis‰t‰‰n enemiesToEvade listaan
            Vector2 enemyDir = GetEnemyRotation(enemyPositions[i]);
            if (IsEnemyThreat(enemyPositions[i], enemyDir)) enemiesToEvade.Add(enemyPositions[i]);

            // Etsit‰‰n l‰hin potentiaalinen vihollinen, Jos maksimivuorot jahtaamiseen ei ole t‰yttyneet:
            if (chase_action_counter < MAX_CHASE_TURNS)
            {
                float dst = (enemyPositions[i] - GetPosition()).magnitude;
                if (dst < closestTarget)
                {
                    PathPoint enemyBehind1 = CalculatePathPoint(enemyPositions[i] - enemyDir * GRID_SIZE);
                    PathPoint enemyBehind2 = CalculatePathPoint(enemyPositions[i] - enemyDir * 2f * GRID_SIZE);
                    if (GetMapPoint(enemyBehind1) != MapPoint.Blocked && !GetDynamicObstacle(enemyBehind1) &&
                        GetMapPoint(enemyBehind2) != MapPoint.Blocked && !GetDynamicObstacle(enemyBehind2))
                    {
                        targetEnemy = CalculatePathPoint(enemyPositions[i]);
                        targetEnemyBehind = enemyBehind1;
                        closestTarget = dst;
                    }
                }
            }

            // Asetetaan reittien painoarvoja isommaksi vihollisten ymp‰rilt‰: halutaan v‰ltt‰‰ vihollisia liikkuessa:
            for (int x = -pathPlanningEnemyAvoidRadius; x <= pathPlanningEnemyAvoidRadius; x++)
            {
                for (int y = -pathPlanningEnemyAvoidRadius; y <= pathPlanningEnemyAvoidRadius; y++)
                {
                    // Jos k‰sitelt‰v‰ piste on vihollisen takana --> ei kasvateta painoarvoa.
                    // T‰ll‰ saadaan aikaiseksi, ett‰ agentti pyrkii reitinsuunnittelussa hyˆkk‰‰m‰‰n takaap‰in.
                    Vector2 p = (enemyPositions[i] + new Vector2(x * GRID_SIZE, y * GRID_SIZE));
                    if ((p - enemyPositions[i]).normalized == -enemyDir) continue;
                    // Muissa tapauksissa kasvatetaan reitin pisteen painoarvoa suuremmaksi. (asetetaan pisteet l‰hell‰ vihollista suuremmalla arvolla ja kauemmat pienemm‰ll‰)
                    float dangerMultiplier = 1f - (((new Vector2(x, y).magnitude * GRID_SIZE - GRID_SIZE) / (pathPlanningEnemyAvoidRadius * GRID_SIZE)));
                    if (dangerMultiplier < 0f) dangerMultiplier = 0f;
                    if (dangerMultiplier > 1f) dangerMultiplier = 1f;
                    IncreaseEnemyAvoidaceMapValue(enemyPositions[i] + new Vector2(x * GRID_SIZE, y * GRID_SIZE), dangerMultiplier * 100f);
                }
            }
        }
    }


    /// <summary>
    /// Agentin p‰‰tˆkenteko. Tulee kutsua aina ennen Execute Actionia
    /// </summary>
    void DecideAction()
    {
        // Jos agentin edess‰ on vihollinen --> lyˆd‰‰n
        if (isEnemyFront)
        {
            currentAction = Action.HitEnemy;
            return;
        }
        // Jos karttojen p‰ivityksess‰ lˆytyi uhkaavia vihollisia --> paetaan
        if (enemiesToEvade.Count > 0)
        {
            currentAction = Action.Flee;
            return;
        }
        // Jos jahdattava vihollinen lˆytyi --> jahdataan
        if (targetEnemy != null)
        {
            currentAction = Action.Chase;
            chase_action_counter++;
            return;
        }

        // Oletuksena agentti liikkuu satunnaisesti, jos mik‰‰n muu toimenpide ei aktivoitunut:
        if (currentAction != Action.Wander) currentMovePath.Clear(); // Jos aikaisempi actioni ei ollut vaeltaminen --> clearataan olemassa oleva reitti
        currentAction = Action.Wander;
    }


    /// <summary>
    /// Toteuttaa agentille m‰‰ritetyn toimenpiteen
    /// </summary>
    void ExecuteAction()
    {
        // Satunnainen liikkuminen:
        if (currentAction == Action.Wander)
        {
            if (currentMovePath.Count < 1) currentMovePath = Find_Path_A_Star(CalculateRandomPointInsideLevel());
            else currentMovePath = Find_Path_A_Star(currentMovePath[currentMovePath.Count - 1]);
            MoveOnPath();
            return;
        }
        // Pakeneminen:
        if (currentAction == Action.Flee && enemiesToEvade.Count > 0)
        {
            Vector2 evasionDir = new Vector2(0f, 0f);
            int sumCount = 0;
            for (int i = 0; i < enemiesToEvade.Count; i++)
            {
                Vector2 dif = GetPosition() - enemiesToEvade[i];
                float threatLevel = 1f - ((dif.magnitude - GRID_SIZE) / (2f * GRID_SIZE));
                if (threatLevel > 1f) threatLevel = 1f;
                if (threatLevel < 0.01f) threatLevel = 0.01f;
                evasionDir += dif.normalized * threatLevel;
                sumCount++;

            }
            Vector2 fleePos = GetPosition() + (evasionDir /= sumCount).normalized * 5f * GRID_SIZE;
            DebugDrawGizmosFleePos = fleePos;
            currentMovePath = Find_Path_A_Star(fleePos);
            MoveOnPath();

            // Jos paettiin pistet‰‰n jahtaaminen tauolle:
            chase_action_counter = MAX_CHASE_TURNS;
            chase_coolDown_counter = Random.Range(MIN_CHASE_COOLDOWN_TURNS, MAX_CHASE_COOLDOWN_TURNS + 1);

            return;
        }
        // Vihollisen jahtaaminen:
        if (currentAction == Action.Chase && targetEnemy != null)
        {
            // Etsit‰‰n reitti vihollisen taakse:
            currentMovePath = Find_Path_A_Star(targetEnemyBehind);
            // Lis‰t‰‰n reittiin viel‰ vihollisen oikea sijainti, jotta agentti k‰‰ntyy katsomaan vihollista varmasti lyˆnti‰ varten.
            currentMovePath.Add(targetEnemy);
            // Liikutaan suunnitellulla polulla: 
            MoveOnPath();
            return;
        }
        // Vihollisen lyˆminen:
        if (currentAction == Action.HitEnemy)
        {
            nextMove = Hit;
            // nollataan jahtaamisen countteri, jos tehtiin lyˆnti (toimii samalla palkintona agentille)
            chase_action_counter = 0;
            return;
        }

        // Oletuksena Pass, jos mit‰‰n seuraavista toimenpiteist‰ ei tehty:
        nextMove = Pass;
    }


    /// <summary>
    /// Liikuttaa agenttia polulla
    /// </summary>
    /// <returns>Palauttaa true, jos toimenpide tehtiin</returns>
    bool MoveOnPath()
    {
        if (currentMovePath.Count < 1)
        {
            //Debug.Log("Yritettiin liikkua polulla, mutta polun pituus oli 0! " + currentAction);
            nextMove = Pass;
            return false;
        }

        // Jos pelaaja on edennyt maaliin:
        PathPoint playerPos = CalculatePathPoint(GetPosition());
        if (playerPos.Compare(currentMovePath[0]))
        {
            currentMovePath.RemoveAt(0);
            if (currentMovePath.Count < 1)
            {
                //Debug.Log("Pelaaja oli jo tavoitellussa kohteessa: " + currentAction);
                nextMove = Pass;
                return false;
            }
        }
        Vector2 playerRot = GetRotation();
        Vector2 targetDir = (CalculatePathPointWorldPos(currentMovePath[0]) - CalculatePathPointWorldPos(playerPos)).normalized;
        float angle = Vector2.SignedAngle(playerRot, targetDir.normalized);
        // Eteenp‰in liikkuminen, jos agentti on kohti liikuttavaa pistett‰:
        float eps = 0.1f;
        if (CompareFloat(angle, 0f, eps))
        {
            nextMove = MoveForward;
            return true;
        }
        // Oikealle p‰in k‰‰ntyminen, jos liikuttava piste on oikealla:
        if (CompareFloat(angle, -90f, eps))
        {
            nextMove = TurnRight;
            return true;
        }
        // Vasemmalle p‰in k‰‰ntyminen, jos liikuttava piste on vasemmalla:
        if (CompareFloat(angle, 90f, eps))
        {
            nextMove = TurnLeft;
            return true;
        }
        // Valitaan satunnainen suunta, jos liikuttava piste on takana:
        if (Random.Range(0, 2) == 1)
        {
            nextMove = TurnRight;
        }
        else
        {
            nextMove = TurnLeft;
        }
        return true;
    }


    /// <summary>
    /// M‰‰ritt‰‰ onko vihollinen uhka vai ei k‰ytt‰en scriptiss‰ olevaa EvadeMask:ia
    /// </summary>
    /// <param name="enemy">vihollisen sijainti</param>
    /// <param name="enemyDir">vihollisen suunta</param>
    /// <returns>Palauttaa onko vihollinen uhka vai ei</returns>
    bool IsEnemyThreat(Vector2 enemy, Vector2 enemyDir)
    {
        // Lasketaan v‰lttely maskin vasen alanurkan sijainti:
        Vector2 evadeMaskWorldAnchor = GetPosition() - 0.5f * GRID_SIZE * new Vector2(EvadeMask.GetLength(0), EvadeMask.GetLength(1));
        // Jos vihollinen on maskin kattaman alueen sis‰ll‰, katsotaan onko vihollinen uhka:
        if (enemy.x >= evadeMaskWorldAnchor.x && enemy.x <= evadeMaskWorldAnchor.x + EvadeMask.GetLength(0) * GRID_SIZE &&
            enemy.y >= evadeMaskWorldAnchor.y && enemy.y <= evadeMaskWorldAnchor.y + EvadeMask.GetLength(1) * GRID_SIZE)
        {
            // Lasketaan indeksit matriisiin:
            int index_X = Mathf.FloorToInt((enemy.x - evadeMaskWorldAnchor.x) / GRID_SIZE);
            int index_Y = Mathf.FloorToInt((enemy.y - evadeMaskWorldAnchor.y) / GRID_SIZE);
            // Varmistetaan, ett‰ indeksit ovat varmasti matriisin sis‰ll‰:
            if (index_X < 0) index_X = 0;
            if (index_X > EvadeMask.GetLength(0) - 1) index_X = EvadeMask.GetLength(0) - 1;
            if (index_Y < 0) index_Y = 0;
            if (index_Y > EvadeMask.GetLength(1) - 1) index_Y = EvadeMask.GetLength(1) - 1;
            // K‰yd‰‰n l‰pi maskissa m‰‰r‰tyt suunnat, joiden perusteella vihollinen luokitellaan vaaralliseksi:
            for (int i = 0; i < EvadeMask.GetLength(2); i++)
            {
                if (EvadeMask[index_X, index_Y, i].Equals(ZERO)) return false; // ZERO pys‰ytt‰‰ tarkistuksen
                if (CompareVector2(enemyDir, EvadeMask[index_X, index_Y, i], 0.1f))
                {
                    return true; // Jos vihollisen suunta t‰sm‰‰ --> vihollinen on uhka
                }
            }
        }
        // Palautetaan false, jos vihollinen ei ollut maskin kattaman alueen sis‰ll‰:
        return false;
    }


    /// <summary>
    /// Asettaa kartassa olevan pisteen statuksen
    /// </summary>
    /// <param name="pos">Asetettavan pisteen sijainti maailmassa</param>
    /// <param name="mapPointStatus">Karttapisteen tila, joka asetetaan</param>
    void SetMapPointStatus(Vector2 pos, MapPoint mapPointStatus)
    {
        // Lasketaan PathPoint, joka osoittaa chunkin ja pisteen indeksin
        PathPoint path = CalculatePathPoint(pos);
        // Jos karttalohkoa ei ole olemassa --> luodaan karttalohko
        if (!movementMap.ContainsKey(path.chunk))
        {
            MapPoint[,] mapPoints = new MapPoint[chunkSize.x, chunkSize.y];
            for (int x = 0; x < mapPoints.GetLength(0); x++)
            {
                for (int y = 0; y < mapPoints.GetLength(1); y++) mapPoints[x, y] = MapPoint.Unknown;
            }
            movementMap.Add(path.chunk, mapPoints);
        }
        // Asetetaan karttapalan pisteen arvo:
        movementMap[path.chunk][path.point.x, path.point.y] = mapPointStatus;
    }


    /// <summary>
    /// Palauttaa kartassa olevan pisteen informaation
    /// </summary>
    /// <param name="chunkIndex">Lohkon indeksi</param>
    /// <param name="pointIndex">Lohkon sis‰ll‰ olevan pisteen indeksi</param>
    /// <returns>Palauttaa karttapisteen informaation</returns>
    MapPoint GetMapPoint(Vector2Int chunkIndex, Vector2Int pointIndex)
    {
        if (movementMap.ContainsKey(chunkIndex)) return movementMap[chunkIndex][pointIndex.x, pointIndex.y];
        return MapPoint.Unknown;
    }


    /// <summary>
    /// Palauttaa kartassa olevan pisteen informaation
    /// </summary>
    /// <param name="pathPoint">Osoitin karttapisteeseen</param>
    /// <returns>Palauttaa karttapisteen informaation</returns>
    MapPoint GetMapPoint(PathPoint pathPoint)
    {
        return GetMapPoint(pathPoint.chunk, pathPoint.point);
    }


    /// <summary>
    /// Kasvattaa v‰lttelyn painoarvoa enemyAvoidance -karttaan.
    /// </summary>
    /// <param name="pos">Sijainti maailmassa, johon halutaan asettaa arvo</param>
    /// <param name="weight">arvo, jolla aikaisempaa painoarvoa kasvatetaan</param>
    void IncreaseEnemyAvoidaceMapValue(Vector2 pos, float weight)
    {
        PathPoint path = CalculatePathPoint(pos);
        if (!enemyAvoidanceMap.ContainsKey(path.chunk)) enemyAvoidanceMap.Add(path.chunk, new float[chunkSize.x, chunkSize.y]);
        enemyAvoidanceMap[path.chunk][path.point.x, path.point.y] += weight;
    }


    /// <summary>
    /// Palautta vihollisen v‰lttelyn painon kartassa
    /// Palauttaa oletuksena 0f, jos vihollinen ei ole kyseisess‰ karttapalan kohdassa tai
    /// kartan palaa ei ole olemassa.
    /// </summary>
    /// <param name="chunkIndex">Kartan palan indeksi</param>
    /// <param name="pointIndex">Kartan palan pisteen indeksi</param>
    /// <returns>Palautta vihollisen v‰lttelyn painon kartassa</returns>
    float GetEnemyAvoidanceValue(Vector2Int chunkIndex, Vector2Int pointIndex)
    {
        if (enemyAvoidanceMap.ContainsKey(chunkIndex)) return enemyAvoidanceMap[chunkIndex][pointIndex.x, pointIndex.y];
        return 0f;
    }


    /// <summary>
    /// Palauttaa vihollisen v‰lttelyn painoarvon kartan pisteess‰.
    /// </summary>
    /// <param name="pathPoint">Karttapiste</param>
    /// <returns>Palauttaa vihollisen v‰lttelyn painoarvon kartan pisteess‰</returns>
    float GetEnemyAvoidanceValue(PathPoint pathPoint)
    {
        return GetEnemyAvoidanceValue(pathPoint.chunk, pathPoint.point);
    }


    /// <summary>
    /// Asettaa dynaamisen esteen karttapisteeseen.
    /// </summary>
    /// <param name="pos">Mailman sijainti</param>
    /// <param name="isPointObstacle">Onko pisteess‰ este vai ei</param>
    void SetDynamicObstacle(Vector2 pos, bool isPointObstacle)
    {
        PathPoint path = CalculatePathPoint(pos);
        if (!dynamicObstacleMap.ContainsKey(path.chunk)) dynamicObstacleMap.Add(path.chunk, new bool[chunkSize.x, chunkSize.y]);
        dynamicObstacleMap[path.chunk][path.point.x, path.point.y] = isPointObstacle;
    }


    /// <summary>
    /// Palauttaa onko karttapisteess‰ dynaamista estett‰ vai ei
    /// </summary>
    /// <param name="chunkIndex">Karttalohkon indeksi</param>
    /// <param name="pointIndex">Pisteen indeksi</param>
    /// <returns>Palauttaa onko karttapisteess‰ dynaamista estett‰ vai ei</returns>
    bool GetDynamicObstacle(Vector2Int chunkIndex, Vector2Int pointIndex)
    {
        if (dynamicObstacleMap.ContainsKey(chunkIndex)) return dynamicObstacleMap[chunkIndex][pointIndex.x, pointIndex.y];
        return false;
    }


    /// <summary>
    /// Palauttaa onko karttapisteess‰ dynaamista estett‰ vai ei
    /// </summary>
    /// <param name="pathPoint">karttapiste</param>
    /// <returns>Palauttaa onko karttapisteess‰ dynaamista estett‰ vai ei</returns>
    bool GetDynamicObstacle(PathPoint pathPoint)
    {
        return GetDynamicObstacle(pathPoint.chunk, pathPoint.point);
    }


    /// <summary>
    /// Etsii polun k‰ytt‰en A*-algoritmia
    /// Validoi annetun kohteen, ett‰ reitti‰ ei yritet‰ etsi‰ pisteeseen, jossa on este.
    /// Palauttaa pelaajan oman sijainnin, jos reitti‰ ei lˆytynyt
    /// </summary>
    /// <param name="targetWorldPos">Sijainti maailmassa, johon halutaan etsi‰ reitti</param>
    /// <returns>Palauttaa listan, joka sis‰lt‰‰ reitin kohteeseen</returns>
    List<PathPoint> Find_Path_A_Star(Vector2 targetWorldPos)
    {
        return Find_Path_A_Star(CalculatePathPoint(targetWorldPos));
    }


    /// <summary>
    /// Etsii polun k‰ytt‰en A*-algoritmia.
    /// Validoi annetun kohteen, ett‰ reitti‰ ei yritet‰ etsi‰ pisteeseen, jossa on este.
    /// Palauttaa pelaajan oman sijainnin, jos reitti‰ ei lˆytynyt
    /// </summary>
    /// <param name="target">Karttapiste, johon reitti halutaan etsi‰</param>
    /// <returns>Palauttaa listan, joka sis‰lt‰‰ reitin kohteeseen</returns>
    List<PathPoint> Find_Path_A_Star(PathPoint target)
    {
        // Validoidaan annettu piste varmuuden vuoksi: (t‰m‰n j‰lkeen piste, johon yritet‰‰n liikkua ei ole este eik‰ pelaajan oma sijainti)
        target = ValidatePathPoint(target);
        // Targetin sijainti maailmassa:
        Vector2 target_world = CalculatePathPointWorldPos(target);

        // Luodaan l‰htˆpisteelle A_Star_Node -olio:
        A_Star_Node start = new A_Star_Node(CalculatePathPoint(GetPosition()));

        // g_score mittaa kuljettua matkaa
        start.g_score = 0f;
        // f_score lasketaan: nykyinen kuljettu matka + arvioitu et‰isyys kohteeseen
        start.f_score = start.g_score + (target_world - CalculatePathPointWorldPos(start.pathPoint)).magnitude;

        // Luodaan open_set lista ja lis‰t‰‰n siihen aloitus node:
        Dictionary<string, A_Star_Node> open_set = new Dictionary<string, A_Star_Node>();
        open_set.Add(start.GetKey(), start);

        // kaikki open_setin kasitellyt nodet siirtyy closed_settiin_
        Dictionary<string, A_Star_Node> closed_set = new Dictionary<string, A_Star_Node>();

        // REITIN ETSINTƒ:
        while (open_set.Count > 0)
        {
            // 1. valitaan open_set:ist‰ piste, jolla on pienin f_score
            A_Star_Node current = null;
            foreach (KeyValuePair<string, A_Star_Node> node in open_set)
            {
                if (current == null) { current = node.Value; continue; };
                if (node.Value.f_score < current.f_score) current = node.Value;
            }

            // 2. Jos nykyinen piste on tavoiteltu piste:
            if (current.pathPoint.Compare(target))
            {

                // Palautetaan muodostettu reitti
                List<PathPoint> path = new List<PathPoint>();
                path.Add(current.pathPoint);
                while (current.came_from != null)
                {
                    current = current.came_from;
                    path.Insert(0, current.pathPoint);
                }
                return path;
            }

            // 3. Poistetaan nykyinen k‰sitelt‰v‰ piste open_set listasta ja siirret‰‰n se closed_set listaan.
            open_set.Remove(current.GetKey());
            closed_set.Add(current.GetKey(), current);

            // 4. K‰yd‰‰n nykyisen k‰sitelt‰v‰n pisteen naapurit l‰pi 4kpl (yl‰, oikea, ala, vasen)
            for (int i = 0; i < 4; i++)
            {
                // Muodostetaan naapurille olio:
                A_Star_Node neighbour = new A_Star_Node(CalculateNeighbourPathPoint(current.pathPoint, (Directions)i));

                // Jos naapuri on olemassa closed_set listassa --> jatketaan seuraavaan naapuriin
                if (closed_set.ContainsKey(neighbour.GetKey())) continue;

                // Haetaan k‰sitelt‰v‰n pisteen MapPoint ja jos pisteen kohdalla on este --> jatketaan seuraavaan naapuriin
                MapPoint mapPoint = GetMapPoint(neighbour.pathPoint);
                if (mapPoint == MapPoint.Blocked) continue;
                if (GetDynamicObstacle(neighbour.pathPoint)) continue;

                // Jos MapPoint on merkattu tuntemattomaksi, kasvatetaan naapurin painoarvoa:
                float weight = 0f;
                if (mapPoint == MapPoint.Unknown) weight += 1f;

                // Jos pelaajan tulee tehd‰ k‰‰nnˆs, kasvatetaan naapurin painoarvoa
                Vector2 previousMoveDir;
                if (current.came_from_dir < 0) previousMoveDir = GetRotation();
                else previousMoveDir = DIRECTIONS[current.came_from_dir];
                weight += Vector2.Angle(previousMoveDir, DIRECTIONS[i]) / 90f;

                // Jos naapurissa on vihollinen, kasvatetaan pisteen liikkumisen painoarvoa:
                weight += GetEnemyAvoidanceValue(neighbour.pathPoint);

                // Lasketaan k‰sitelt‰v‰lle naapurille g_score ja f_score:
                neighbour.g_score = current.g_score + (1f + weight);
                neighbour.f_score = neighbour.g_score + (target_world - CalculatePathPointWorldPos(neighbour.pathPoint)).magnitude;
                neighbour.came_from = current;
                neighbour.came_from_dir = i;

                // Jos naapuri on open_setiss‰:
                if (open_set.TryGetValue(neighbour.GetKey(), out A_Star_Node old))
                {
                    if (neighbour.g_score < old.g_score)
                    {
                        old.f_score = neighbour.f_score;
                        old.g_score = neighbour.g_score;
                        old.came_from = current;
                        old.came_from_dir = i;
                    }
                }
                // Muuten lis‰t‰‰n naapuri open_settiin
                else
                {
                    open_set.Add(neighbour.GetKey(), neighbour);
                }
            }
        }
        // Palautetaan pelaajan oma sijainti, jos reitti‰ ei lˆytynyt
        return new List<PathPoint> { start.pathPoint };
    }


    /// <summary>
    /// Validoi annetun pisteen. Jos piste on jo vartattu (este tai oma sijainti). Palauttaa ensimm‰isen lˆytyv‰n
    /// pisteen parametrina annetun pisteen ymp‰rilt‰, joka on kuljettavissa tai sen status on tuntematon.
    /// Validointi ottaa huomioon, ett‰ validin pisteen etsint‰ tapahtuu levelin sis‰ll‰.
    /// </summary>
    /// <param name="pathPoint">Reittipiste, joka validoidaan</param>
    /// <returns>Palauttaa annetun pisteen sijainnin tai l‰himm‰n ensimm‰isen‰ lˆytyv‰n vapaana olevan pisteen</returns>
    public PathPoint ValidatePathPoint(PathPoint pathPoint)
    {
        // Varmistetaan, ett‰ piste on levelin sis‰ll‰:
        Vector2 origin = CalculatePathPointWorldPos(pathPoint);
        bool originMoved = false;
        if (origin.x < LEVEL_TOP_LEFT.x + GRID_SIZE) { origin.x = LEVEL_TOP_LEFT.x + GRID_SIZE; originMoved = true; }
        if (origin.x > LEVEL_BOTTOM_RIGHT.x - GRID_SIZE) { origin.x = LEVEL_BOTTOM_RIGHT.x - GRID_SIZE; originMoved = true; }
        if (origin.y < LEVEL_BOTTOM_RIGHT.y + GRID_SIZE) { origin.y = LEVEL_BOTTOM_RIGHT.y + GRID_SIZE; originMoved = true; }
        if (origin.y > LEVEL_TOP_LEFT.y - GRID_SIZE) { origin.y = LEVEL_TOP_LEFT.y - GRID_SIZE; originMoved = true; }

        // Korjataan annetun pathpointin sijainti, jos annetun pisteen sijaintia muutettiin
        if (originMoved) pathPoint = CalculatePathPoint(origin);

        // Jos pisteess‰ ei ole estett‰ --> palautetaan se suoraan
        PathPoint ownPos = CalculatePathPoint(GetPosition());
        if (!GetDynamicObstacle(pathPoint) && GetMapPoint(pathPoint) != MapPoint.Blocked && !pathPoint.Compare(ownPos)) return pathPoint;

        // Muuten etsit‰‰n validia pistett‰ annetun pisteen ymp‰rilt‰ ja palautetaan heti ensimm‰inen validi piste:
        // Etsint‰ tapahtuu kasvattamalla kierroksittain et‰isyystt‰ annetusta pisteest‰ ja k‰yden l‰pi kaikki pisteet silt‰ et‰isyydelt‰:
        int currentSearchRadius = 1;
        int maxSearchRadius = 1000;
        while (currentSearchRadius < maxSearchRadius)
        {
            float x_min = origin.x - currentSearchRadius * GRID_SIZE;
            if (x_min < LEVEL_TOP_LEFT.x + GRID_SIZE) x_min = LEVEL_TOP_LEFT.x + GRID_SIZE;
            if (x_min > LEVEL_BOTTOM_RIGHT.x - GRID_SIZE) x_min = LEVEL_BOTTOM_RIGHT.x - GRID_SIZE;
            float x_max = origin.x + currentSearchRadius * GRID_SIZE;
            if (x_max < LEVEL_TOP_LEFT.x + GRID_SIZE) x_max = LEVEL_TOP_LEFT.x + GRID_SIZE;
            if (x_max > LEVEL_BOTTOM_RIGHT.x - GRID_SIZE) x_max = LEVEL_BOTTOM_RIGHT.x - GRID_SIZE;

            float y_min = origin.y - currentSearchRadius * GRID_SIZE;
            if (y_min < LEVEL_BOTTOM_RIGHT.y + GRID_SIZE) y_min = LEVEL_BOTTOM_RIGHT.y + GRID_SIZE;
            if (y_min > LEVEL_TOP_LEFT.y - GRID_SIZE) y_min = LEVEL_TOP_LEFT.y - GRID_SIZE;
            float y_max = origin.y + currentSearchRadius * GRID_SIZE;
            if (y_max < LEVEL_BOTTOM_RIGHT.y + GRID_SIZE) y_max = LEVEL_BOTTOM_RIGHT.y + GRID_SIZE;
            if (y_max > LEVEL_TOP_LEFT.y - GRID_SIZE) y_max = LEVEL_TOP_LEFT.y - GRID_SIZE;

            float x = x_min;
            while (x <= x_max)
            {
                PathPoint point1 = CalculatePathPoint(new Vector2(x, y_max));
                if (!GetDynamicObstacle(point1) && GetMapPoint(point1) != MapPoint.Blocked && !point1.Compare(ownPos)) return point1;
                PathPoint point2 = CalculatePathPoint(new Vector2(x, y_min));
                if (!GetDynamicObstacle(point2) && GetMapPoint(point2) != MapPoint.Blocked && !point2.Compare(ownPos)) return point2;
                x += GRID_SIZE;
            }
            float y = y_min + GRID_SIZE;
            while (y <= y_max - GRID_SIZE)
            {
                PathPoint point1 = CalculatePathPoint(new Vector2(x_max, y));
                if (!GetDynamicObstacle(point1) && GetMapPoint(point1) != MapPoint.Blocked && !point1.Compare(ownPos)) return point1;
                PathPoint point2 = CalculatePathPoint(new Vector2(x_min, y));
                if (!GetDynamicObstacle(point2) && GetMapPoint(point2) != MapPoint.Blocked && !point2.Compare(ownPos)) return point2;
                y += GRID_SIZE;
            }

            currentSearchRadius++;
        }

        return null;
    }


    /// <summary>
    /// Laskee satunnaisen pisteen levelin sis‰ll‰ ja validoi sen eli satunnainen piste on vapaana
    /// </summary>
    /// <returns>Laskee satunnaisen pisteen levelin sis‰ll‰ ja validoi sen eli satunnainen piste on vapaana</returns>
    public PathPoint CalculateRandomPointInsideLevel()
    {
        Vector2 rnd = new Vector2(
            Random.Range(LEVEL_TOP_LEFT.x + GRID_SIZE, LEVEL_BOTTOM_RIGHT.x - GRID_SIZE),
            Random.Range(LEVEL_BOTTOM_RIGHT.y + GRID_SIZE, LEVEL_TOP_LEFT.y - GRID_SIZE));
        return ValidatePathPoint(CalculatePathPoint(rnd));
    }


    /// <summary>
    /// Laskee karttapalan indeksin maailman sijainnin perusteella
    /// </summary>
    /// <param name="pos">Sijainti maailmassa</param>
    /// <returns>Palauttaa karttapalan indeksin</returns>
    Vector2Int CalculateChunkIndex(Vector2 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / (chunkSize.x * GRID_SIZE)), Mathf.FloorToInt(pos.y / (chunkSize.y * GRID_SIZE)));
    }


    /// <summary>
    /// Laskee karttapalan vasemman alanurkan koordinaatin sijainnin maailmassa
    /// </summary>
    /// <param name="chunkIndex">Karttapalan indeksi</param>
    /// <returns>Laskee karttapalan vasemman alanurkan koordinaatin sijainnin maailmassa</returns>
    Vector2 CalculateChunkWorldAnchor(Vector2Int chunkIndex)
    {
        return new Vector2(chunkIndex.x * chunkSize.x * GRID_SIZE, chunkIndex.y * chunkSize.y * GRID_SIZE);
    }


    /// <summary>
    /// Laskee karttapisteen sijainnin
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    PathPoint CalculatePathPoint(Vector2 pos)
    {
        // Lasketaan chunkin indeksi sek‰ vasemman alanurkan sijainti maailmassa:
        Vector2Int chunkIndex = CalculateChunkIndex(pos);
        Vector2 chunkWorldAnchor = CalculateChunkWorldAnchor(chunkIndex);
        // Lasketaan pisteen indeksi:
        Vector2Int pointIndex = new Vector2Int(
            Mathf.FloorToInt((pos.x - chunkWorldAnchor.x) / GRID_SIZE),
            Mathf.FloorToInt((pos.y - chunkWorldAnchor.y) / GRID_SIZE));
        // Tehd‰‰n varmistus, ett‰ indeksi on varmasti chunkin sis‰ll‰, koska floatteihin ei voi koskaan luottaa 100%:sti:
        if (pointIndex.x > chunkSize.x - 1) pointIndex.x = chunkSize.x - 1;
        if (pointIndex.x < 0) pointIndex.x = 0;
        if (pointIndex.y > chunkSize.y - 1) pointIndex.y = chunkSize.y - 1;
        if (pointIndex.y < 0) pointIndex.y = 0;
        // Palautetaan polku pisteeseen:
        return new PathPoint(chunkIndex, pointIndex);
    }


    /// <summary>
    /// Laskee karttapalassa olevan pisteen ymp‰rille muodostuvan vokselin vasemman alanurkan sijainnin maailmassa.
    /// </summary>
    /// <param name="chunkIndex">Karttapalan indeksi</param>
    /// <param name="pointIndex">Karttapalassa olevan pisteen indeksi</param>
    /// <returns>Palauttaa karttapalassa olevan pisteen vasemman alanurkan sijainnin maailmassa</returns>
    Vector2 CalculatePointVoxelWorldAnchor(Vector2Int chunkIndex, Vector2Int pointIndex)
    {
        Vector2 chunkWorldAnchor = CalculateChunkWorldAnchor(chunkIndex);
        return new Vector2(chunkWorldAnchor.x + pointIndex.x * GRID_SIZE, chunkWorldAnchor.y + pointIndex.y * GRID_SIZE);
    }


    /// <summary>
    /// Laskee karttapalassa olevan pisteen ymp‰rille muodostuvan vokselin vasemman alanurkan sijainnin maailmassa.
    /// </summary>
    /// <param name="pathPoint">Karttapiste</param>
    /// <returns></returns>
    Vector2 CalculatePathPointVoxelWorldAnchor(PathPoint pathPoint)
    {
        return CalculatePointVoxelWorldAnchor(pathPoint.chunk, pathPoint.point);
    }


    /// <summary>
    /// Laskee karttapalassa olevan pisteen sijainnin maailmassa (gridin muodostaman vokselin keskipiste).
    /// </summary>
    /// <param name="chunkIndex">Karttapalan indeksi</param>
    /// <param name="pointIndex">Karttapalassa olevan pisteen indeksi</param>
    /// <returns> Laskee karttapalassa olevan pisteen sijainnin maailmassa</returns>
    Vector2 CalculatePathPointWorldPos(Vector2Int chunkIndex, Vector2Int pointIndex)
    {
        return CalculatePointVoxelWorldAnchor(chunkIndex, pointIndex) + new Vector2(0.5f * GRID_SIZE, 0.5f * GRID_SIZE);
    }


    /// <summary>
    /// Laskee karttapisteen sijainnin maailmassa
    /// </summary>
    /// <param name="pathPoint">Karttapiste</param>
    /// <returns></returns>
    Vector2 CalculatePathPointWorldPos(PathPoint pathPoint)
    {
        return CalculatePathPointWorldPos(pathPoint.chunk, pathPoint.point);
    }


    /// <summary>
    /// Laskee polun pisteen naapuriin
    /// </summary>
    /// <param name="chunkIndex">Karttapalan indeksi, jossa piste sijaitsee</param>
    /// <param name="pointIndex">Pisteen indeksi, jonka naapuri halutaan saada</param>
    /// <param name="neighbour">Naapuri, joka halutaan saada</param>
    /// <returns>Laskee polun pisteen naapuriin</returns>
    PathPoint CalculateNeighbourPathPoint(PathPoint pathPoint, Directions neighbour)
    {
        PathPoint tmp = new PathPoint(pathPoint.chunk, pathPoint.point);
        if (neighbour == Directions.Up)
        {
            tmp.point.y++;
            if (tmp.point.y > chunkSize.y - 1) { tmp.point.y = 0; tmp.chunk.y++; }
        }
        else if (neighbour == Directions.Right)
        {
            tmp.point.x++;
            if (tmp.point.x > chunkSize.x - 1) { tmp.point.x = 0; tmp.chunk.x++; }
        }
        else if (neighbour == Directions.Down)
        {
            tmp.point.y--;
            if (tmp.point.y < 0) { tmp.point.y = chunkSize.y - 1; tmp.chunk.y--; }
        }
        else if (neighbour == Directions.Left)
        {
            tmp.point.x--;
            if (tmp.point.x < 0) { tmp.point.x = chunkSize.x - 1; tmp.chunk.x--; }
        }

        return tmp;
    }


    /// <summary>
    /// Koska liukulukujen suoraan vertailuun ei voi luottaa, niin t‰m‰ on oma vertailu, jolla voi s‰‰t‰‰ halutun epsilonin
    /// </summary>
    /// <param name="value1">vertailtava arvo 1</param>
    /// <param name="value2">vertailtava arvo 2</param>
    /// <param name="eps">epsiloni >= 0, jonka p‰‰ss‰ vertailtavien lukujen tulee olla</param>
    /// <returns></returns>
    bool CompareFloat(float value1, float value2, float eps)
    {
        if (Mathf.Abs(value2 - value1) <= eps) return true;
        return false;
    }


    /// <summary>
    /// Koska liukulujen suoraan vertailuun ei voi luottaa, niin t‰m‰ on oma vertailu, jolla voi s‰‰t‰‰ halutun epsilonin
    /// </summary>
    /// <param name="vec1">vertailtava vektori 1</param>
    /// <param name="vec2">vertailtava vektori 2</param>
    /// <param name="eps">epsiloni, jonka p‰‰ss‰ vertailtavien vektorien x ja y saa maksimissaan olla toisistaan</param>
    /// <returns></returns>
    bool CompareVector2(Vector2 vec1, Vector2 vec2, float eps)
    {
        if (CompareFloat(vec1.x, vec2.x, eps) && CompareFloat(vec1.y, vec2.y, eps)) return true;
        return false;
    }


    /// <summary>
    /// Debuggauksessa teht‰v‰ piirto, joka helpottaa agentin toimintojen seuraamista
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Chunkkien ja karkoitettujen pisteiden piirto:
        if (drawMapChunks || drawFoundMapPoints)
        {
            foreach (KeyValuePair<Vector2Int, MapPoint[,]> chunk in movementMap)
            {
                if (drawMapChunks)
                {
                    Vector2 bottomLeft = CalculateChunkWorldAnchor(chunk.Key);
                    Vector2 topLeft = bottomLeft + new Vector2(0f, chunkSize.y * GRID_SIZE);
                    Vector2 topRight = topLeft + new Vector2(chunkSize.x * GRID_SIZE, 0f);
                    Vector2 bottomRight = topRight + new Vector2(0f, -chunkSize.y * GRID_SIZE);
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(bottomLeft, topLeft);
                    Gizmos.DrawLine(topLeft, topRight);
                    Gizmos.DrawLine(topRight, bottomRight);
                    Gizmos.DrawLine(bottomRight, bottomLeft);
                }

                if (drawFoundMapPoints)
                {
                    float drawRadius = 0.05f;
                    Vector2 chunkWorldAnchor = CalculateChunkWorldAnchor(chunk.Key);
                    for (int x = 0; x < chunk.Value.GetLength(0); x++)
                    {
                        for (int y = 0; y < chunk.Value.GetLength(1); y++)
                        {
                            if (chunk.Value[x, y] == MapPoint.Movable) Gizmos.color = Color.green;
                            if (chunk.Value[x, y] == MapPoint.Blocked || GetDynamicObstacle(chunk.Key, new Vector2Int(x, y))) Gizmos.color = Color.red;
                            else if (chunk.Value[x, y] == MapPoint.Unknown) continue;

                            Vector2 point = chunkWorldAnchor + new Vector2(x * GRID_SIZE + 0.5f * GRID_SIZE, y * GRID_SIZE + 0.5f * GRID_SIZE);
                            Gizmos.DrawLine(point + new Vector2(-drawRadius, 0f), point + new Vector2(drawRadius, 0f));
                            Gizmos.DrawLine(point + new Vector2(0f, -drawRadius), point + new Vector2(0f, drawRadius));
                        }
                    }
                }
            }
        }

        // Piirret‰‰n kuljettava reitti:
        if (drawPlannedPath)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i + 1 < currentMovePath.Count; i++)
            {
                Gizmos.DrawLine(CalculatePathPointWorldPos(currentMovePath[i]), CalculatePathPointWorldPos(currentMovePath[i + 1]));
            }
        }

        // Viholliskohteen piirto:
        if (drawCurrentTarget && targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(GetPosition(), CalculatePathPointWorldPos(targetEnemy));
        }

        // Pakenemissuunnan piirto:
        if (drawFleeDirection && currentAction == Action.Flee)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(GetPosition(), DebugDrawGizmosFleePos);
        }
    }
}