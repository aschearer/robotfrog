using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public enum LevelState
{
    None,
    WaitingToSpawn,
    Playing,
    GameOver,
}

public class Level : MonoBehaviour {


    public static LevelState levelState = LevelState.None;
    public int PlayerCount = 2;

    public GameObject SplashScreenOne;
    private float SplashTime = 3.0f;
    private float SplashTimeRemaining;

    public GameObject TileBarrier;
    public GameObject TileFloating;
    public GameObject TileWater;
    public GameObject TileBox;
    public GameObject TileRock;
    public GameObject PlayerMainProto;
    public GameObject PlayerAltProto;
    public GameObject CursorProto;
    public GameObject ZapExplodeProto;
    public GameObject SplashExplodeProto;
    public GameObject DecoWallProto;
    public GameObject DecoOneProto;
    public GameObject DecoTwoProto;
    public GameObject Player1SpotProto;
    public GameObject Player2SpotProto;
    public GameObject HyperStoneProto;
    public Material CommonMain;
    public Material CommonAlt;

    public Transform Container;

    public List<string> Map;
    public List<string> MapAbove;

    private int NumberOfColumns;
    private int roundCount = 1;

    private List<Tile> tiles = new List<Tile>();

    private List<Controller> controllers = new List<Controller>();

    private List<Player> players = new List<Player>();
    private List<HyperStone> hyperstones = new List<HyperStone>();

    private float nextHyperStoneTimeStamp = 0.0f;
    public float hyperStoneFirstDelay = 8.0f;
    public int hyperStoneCount = 4;
    
    private ManualTimer SpawnTimer = new ManualTimer();
    private float targetCameraSize;
    private Vector3 targetCameraPosition;
    private bool hasStarted;

    public void Start()
    {
        this.StartCoroutine(this.PlayMusic());
        SplashTimeRemaining = SplashTime;
        SpawnTimer.SetTime(Globals.WarmupTime);
        for(int i=0; i<6; ++i)
        {
            GameObject controllerGO = new GameObject();
            controllerGO.name = "Controller"+i;
            ControllerHuman controller = controllerGO.AddComponent<ControllerHuman>();
            controller.AssignDeviceId(i);
            controllers.Add(controller);

        }
        switch(PlayerCount)
        {
            case 0:
            case 1:
            case 2:
                GetTwoPlayer();
                break;
            case 3:
            case 4:
                //four players will lock up
                //in the gameover section i added a check for the round ending that only uses getplayertwo to switch the maps 
                //no player number check is done
                GetFourPlayer();
                break;
        }

        this.targetCameraSize = Camera.main.orthographicSize;
        Camera.main.orthographicSize = this.targetCameraSize * 2;
        this.targetCameraPosition = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition += new Vector3(0, 4);

        this.hasStarted = true;
    }

    private IEnumerator PlayMusic()
    {
        while (AudioManager.Instance == null)
        {
            yield return null;
        }

        AudioManager.Instance.SetMusic(20);
    }

    void GetTwoPlayer()
    {
        Map.Clear();
        MapAbove.Clear();
        int rInt = UnityEngine.Random.Range(0, 4);
        switch (rInt)
        {
            case 0:
                // 8x5 with columns
                Map.Add("_R____W_");
                Map.Add("_R_BWB__");
                Map.Add("________");
                Map.Add("__BWB_R_");
                Map.Add("_W____R_");
                MapAbove.Add("________");
                MapAbove.Add("_1____3_");
                MapAbove.Add("________");
                MapAbove.Add("_4____2_");
                MapAbove.Add("________");
                break;
            case 1:
                //8x6 
                Map.Add("W_R_____");
                Map.Add("___WW___");
                Map.Add("_R_BB___");
                Map.Add("___BB_R_");
                Map.Add("___WW___");
                Map.Add("_____R_W");
                MapAbove.Add("________");
                MapAbove.Add("________");
                MapAbove.Add("_1____3_");
                MapAbove.Add("_4____2_");
                MapAbove.Add("________");
                MapAbove.Add("________");
                break;
            case 2:
                // 8x5 with columns
                Map.Add("_B______R");
                Map.Add("W__BBB__W");
                Map.Add("W___B___W");
                Map.Add("W__BBB__W");
                Map.Add("R______B_");

                MapAbove.Add("_________");
                MapAbove.Add("_1_____3_");
                MapAbove.Add("_________");
                MapAbove.Add("_4_____2_");
                MapAbove.Add("_________");
                break;
            case 3:
                // 8x5 with columns
                Map.Add("_B_B_B_B");
                Map.Add("B_BWB_BW");
                Map.Add("___RR___");
                Map.Add("WB_BWB_B");
                Map.Add("B_B_B_B_");

                MapAbove.Add("1______3");
                MapAbove.Add("________");
                MapAbove.Add("________");
                MapAbove.Add("________");
                MapAbove.Add("4______2");
                break;

        }

    }

    void GetFourPlayer()
    {

        Map.Clear();
        MapAbove.Clear();
        
        // 10x8 with columns
        Map.Add("_______W");
        Map.Add("_WR____W");
        Map.Add("_R_____W");
        Map.Add("____WBB_");
        Map.Add("_BBW____");
        Map.Add("W_____R_");
        Map.Add("W____RW_");
        Map.Add("W_______");
        MapAbove.Add("_1______");
        MapAbove.Add("______3_");
        MapAbove.Add("________");
        MapAbove.Add("________");
        MapAbove.Add("________");
        MapAbove.Add("________");
        MapAbove.Add("_4______");
        MapAbove.Add("______2_");
    }
    public void Update()
    {
        if (!this.hasStarted)
        {
            return;
        }
        if(SplashTimeRemaining > 0)
        {
            SplashTimeRemaining -= Time.deltaTime;
            if(SplashTimeRemaining <= 0)
            {
                SplashScreenOne.SetActive(false);
            }
        }

        if (Level.levelState == LevelState.WaitingToSpawn && this.players.Count == 0)
        {
            var angles = this.transform.localEulerAngles;
            angles.y += Time.deltaTime * Mathf.PI * 8;
            this.transform.localEulerAngles = angles;
        }

        if(Input.GetKey(KeyCode.PageDown))
        {
            Time.timeScale = 0.2f;
        }
        if(Input.GetKey(KeyCode.PageUp))
        {
            Time.timeScale = 1.0f;
        }
        switch(levelState)
        {
            case LevelState.None:
                MakeLevel(Map);
                nextHyperStoneTimeStamp = Time.time + hyperStoneFirstDelay;
                levelState = LevelState.WaitingToSpawn;
                break;
            case LevelState.WaitingToSpawn:
                if(PlayerCount > 2 && SpawnTimer.Tick(Time.deltaTime))
                {
                    int spawnSlot = CountPlayers();
                    for(int i=spawnSlot; i<PlayerCount; ++i)
                    {
                        SpawnPlayer(i, i);
                    }
                    AudioManager.Instance.SetMusic(21);
                    levelState = LevelState.Playing;
                }
                for(int i=0; i<6; ++i)
                {
                    if(controllers[i].slotInTheGame >= 0 && !controllers[i].Pawn)
                    {
                        SpawnPlayer(controllers[i].slotInTheGame, i);
                        AfterPlayerJoin();
                    }
                }
                for(int i=0; i<6; ++i)
                {
                    if(controllers[i].inputData.PrimaryIsDown && !controllers[i].Pawn)
                    {
                        int spawnSlot = CountPlayers();
                        SpawnPlayer(spawnSlot, i);
                        AfterPlayerJoin();
                    }
                }
                break;
            case LevelState.Playing:
                if(CountPlayers() < 2)
                {
                    //what i did here was stupid
                    //remove gettwoplayer as it will only cycle the twoplayer maps
                    levelState = LevelState.GameOver;
                    
                    this.StartCoroutine(GameOverSequence());
                    if(roundCount == 3)
                    {
                        GetTwoPlayer();
                        roundCount = 0;
                    }
                    else
                    {
                        ++roundCount;
                    }
                        
                }
                if(hyperstones.Count < hyperStoneCount && nextHyperStoneTimeStamp < Time.time )
                {
                    nextHyperStoneTimeStamp = Time.time + UnityEngine.Random.Range(5.0f,15.0f);

                    SpawnHyperstone();
                }
                break;
        }
        

    }

    private void AfterPlayerJoin()
    {
        AudioManager.Instance.PlaySound(18);
        if(CountPlayers() >= PlayerCount)
        {
            AudioManager.Instance.SetMusic(21);
            levelState = LevelState.Playing;
        }
    }

    private int CountPlayers()
    {
        // Test for end-game condition
        int playerCount = 0;
        for (int i = 0; i < this.Container.childCount; i++)
        {
            var child = this.Container.GetChild(i);
            var player = child.GetComponent<Player>();
            if (player)
            {
                playerCount++;
            }
        }

        return playerCount;
    }

    private IEnumerator GameOverSequence()
    {
        foreach (var player in this.players)
        {
            if (player)
            {
                player.GameOver();
            }
        }

        var position = this.transform.localPosition;
        for (float t = 0; t < 1; t += Time.deltaTime * 5)
        {
            var shakenPosition = position;
            shakenPosition.x = UnityEngine.Random.Range(-0.1f, 0.1f);
            shakenPosition.y = UnityEngine.Random.Range(-0.1f, 0.1f);
            this.transform.localPosition = shakenPosition;
            yield return null;
        }

        this.transform.localPosition = position;

        yield return new WaitForSeconds(0.5f);

        var twirl = Camera.main.GetComponent<Twirl>();
        twirl.angle = 0;
        twirl.enabled = true;
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            twirl.angle = 180 * t;
            yield return null;
        }

        // Step 1 destroy all things
        for (int i = 0; i < this.Container.childCount; i++)
        {
            var child = this.Container.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
        for(int i=0; i<6; ++i)
        {
            controllers[i].Pawn = null;
        }

        // Step 2 create all things
        GetTwoPlayer();
        this.MakeLevel(this.Map);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            twirl.angle = 180 + 180 * t;
            yield return null;
        }

        twirl.angle = 0;
        twirl.enabled = false;

        nextHyperStoneTimeStamp = Time.time + hyperStoneFirstDelay;
        levelState = LevelState.WaitingToSpawn;

    }

    public void ExplodeAt(Tile origin, int radius, int magnitude = 2, bool includeSelf = true)
    {
        foreach (var neighbor in this.tiles)
        {
            if (neighbor == null)
            {
                continue;
            }
            if(!includeSelf && neighbor == origin)
            {
                continue;
            }

            int distance = 1000;
            int distanceColumn = Mathf.Abs(origin.Column - neighbor.Column);
            int distanceRow = Mathf.Abs(origin.Row - neighbor.Row);
            if(distanceColumn==0 && distanceRow==0)
            {
                distance = 0;
            }
            else if(distanceColumn==1 && distanceRow==0 || distanceColumn==0 && distanceRow==1)
            {
                distance = 1;
            }
            else if(distanceColumn==1 && distanceRow==1)
            {
                distance = 2;
            }
            else if(distanceColumn==2 && distanceRow==0 || distanceColumn==0 && distanceRow==2)
            {
                distance = 3;
            }
            else if(distanceColumn==2 && distanceRow==1 || distanceColumn==1 && distanceRow==2)
            {
                distance = 4;
            }
            else if(distanceColumn==2 && distanceRow==2)
            {
                distance = 5;
            }
            else if(distanceColumn<=3 && distanceRow<=3)
            {
                distance = 6;
            }
            else if(distanceColumn<=4 && distanceRow<=4)
            {
                distance = 9;
            }


            if (distance <= radius)
            {
                this.StartCoroutine(neighbor.HandleExplode(magnitude, distance * 0.4f));
            }
        }
    }

    internal Tile GetTileAt(int column, int row)
    {
        foreach (var tile in this.tiles)
        {
            if (tile != null && tile.Column == column && tile.Row == row)
            {
                return tile;
            }
        }

        return null;
    }

    public void SpawnHyperstone()
    {
        Tile goodTile = null;
        int attempts = 0;
        float mid = (float)(NumberOfColumns-1)/2.0f;
        int minColumn = (int)Mathf.Floor(mid);
        int maxColumn = (int)Mathf.Ceil(mid);
        if(minColumn == maxColumn)
        {
            minColumn--;
            maxColumn++;
        }
        while(!goodTile && ++attempts < 20)
        {
            int randomTileInt = UnityEngine.Random.Range(0,this.tiles.Count);
            Tile tile = this.tiles[randomTileInt];
            if(tile.Column >= minColumn && tile.Column <= maxColumn)
            {
                if(tile.State != TileState.Water && tile.State != TileState.Barrier)
                {
                    goodTile = tile;
                }
            }
        }
        if(!goodTile)
        {
            return;
        }


        int TileRadius = 1;
        Vector3 Position = new Vector3(goodTile.Column*TileRadius, 0, -goodTile.Row*TileRadius);
        Position.y += 4;
        GameObject stoneGO = Instantiate(HyperStoneProto, Vector3.zero, Quaternion.identity, this.Container);
        stoneGO.transform.localPosition = Position;
        stoneGO.transform.localEulerAngles = Vector3.zero;
        stoneGO.name = "HyperStone"+hyperstones.Count;
        var hyperstone = stoneGO.GetComponent<HyperStone>();
        hyperstone.OwnerTile = goodTile;
        hyperstone.OwnerPlayer = null;
        hyperstones.Add(hyperstone);
    }

    public void SpawnPlayer(int spawnSlot, int controller)
    {
        bool bDidFind = false;
        int spawnRow = 0;
        int spawnCol = 0;
        GameObject Prefab = null;

        int TileRadius = 1;
        Quaternion Rotation = Quaternion.identity;
        Color Tint = Color.white;
        int JumpSound = 13;
        for(int row=0; row<MapAbove.Count && !bDidFind; ++row)
        {
            string tiles = MapAbove[row];
            for(int column=0; column<tiles.Length && !bDidFind; ++column)
            {
                switch(spawnSlot)
                {
                    default:
                    case 0:
                    if(tiles[column] == '1')
                    {
                        Tint = Color.red;
                        JumpSound = 13;
                        Prefab = PlayerMainProto;
                        spawnRow = row;
                        spawnCol = column;
                        bDidFind = true;
                    }
                    break;
                    case 1:
                    if(tiles[column] == '2')
                    {
                        Tint = Color.blue;
                        JumpSound = 14;
                        Prefab = PlayerAltProto;
                        spawnRow = row;
                        spawnCol = column;
                        bDidFind = true;
                    }
                    break;
                    case 2:
                    if(tiles[column] == '3')
                    {
                        Tint = Color.yellow;
                        JumpSound = 13;
                        Prefab = PlayerMainProto;
                        spawnRow = row;
                        spawnCol = column;
                        bDidFind = true;
                    }
                    break;
                    case 3:
                    if(tiles[column] == '4')
                    {
                        Tint = Color.magenta;
                        JumpSound = 14;
                        Prefab = PlayerAltProto;
                        spawnRow = row;
                        spawnCol = column;
                        bDidFind = true;
                    }
                    break;
                }
            }
        }
                   
        if(bDidFind)
        {
            Vector3 Position = new Vector3(spawnCol*TileRadius, 0, -spawnRow*TileRadius);
            Position.y += 1;
            GameObject player = Instantiate(Prefab, Vector3.zero, Rotation, this.Container);
            player.transform.localPosition = Position;
            player.transform.localEulerAngles = Vector3.zero;
            player.name = "Player"+spawnSlot;
            var playerView = player.GetComponent<Player>();
            playerView.Level = this;
            playerView.Tint = Tint;
            playerView.JumpSound = JumpSound;
            playerView.Strength = 0;
            playerView.Column = (int)Mathf.Round(Position.x);
            playerView.Row = (int)Mathf.Round(-Position.z);
            bool bUseAltMat = spawnSlot >= 2;
            controllers[controller].slotInTheGame = spawnSlot;
            controllers[controller].Pawn = playerView;

            GameObject cursor = Instantiate(CursorProto, Vector3.zero, Quaternion.identity);
            cursor.transform.SetParent(player.transform, false);
            var cursorView = cursor.GetComponent<Cursor>();
            cursorView.Tint = Tint;

            playerView.Cursor = cursorView;
            playerView.flyingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;
            playerView.sittingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;

            var tile = this.GetTileAt(playerView.Column, playerView.Row);
            if (tile)
            {
                tile.TouchingPlayers.Add(playerView);
            }

            this.players.Add(playerView);
            if (players.Count == 2)
            {
                AudioManager.Instance.PlaySound(18);
            }

            var objectName = spawnSlot == 0 ? "RedPlayer" : "BluePlayer";
            var promptGameObject = GameObject.Find(objectName);
            if (promptGameObject != null)
            {
                var prompt = promptGameObject.GetComponent<CanvasGroup>();
                this.StartCoroutine(this.Fade(prompt));
            }

            var spot = GameObject.Find("Player" + this.players.Count + "Spot");
            if (spot)
            {
                GameObject.Destroy(spot.gameObject);
            }

            if (players.Count == 1)
            {
                this.StartCoroutine(this.AnimateToPlaying());
            }
        }
    }

    private IEnumerator AnimateToPlaying()
    {
        var title = GameObject.Find("Title");
        if (title)
        {
            GameObject.Destroy(title.gameObject);
        }

        var runTime = 0.5f;
        var angles = this.transform.localEulerAngles;
        var angleDistance = angles.y;
        var orthoDistance = Camera.main.orthographicSize;
        var startPosition = Camera.main.transform.localPosition;
        for (float t = 0; t < runTime; t += Time.deltaTime)
        {
            var p = t / runTime;

            angles.y = Mathf.Lerp(angleDistance, 0, p);
            this.transform.localEulerAngles = angles;

            Camera.main.orthographicSize = Mathf.Lerp(orthoDistance, this.targetCameraSize, p);
            Camera.main.transform.localPosition = Vector3.Lerp(startPosition, this.targetCameraPosition, p);

            yield return null;
        }

        this.transform.localEulerAngles = Vector3.zero;
    }

    private IEnumerator Fade(CanvasGroup prompt)
    {
        for (float t = 1; t >= 0; t -= Time.deltaTime * 4)
        {
            prompt.alpha = t;

            yield return null;
        }

        GameObject.Destroy(prompt.gameObject);
    }

    internal Player GetPlayerAt(int column, int row)
    {
        foreach (var player in this.players)
        {
            if (player != null && player.Column == column && player.Row == row)
            {
                return player;
            }
        }

        return null;
    }

    public void MakeElectricity(Vector3 pos)
    {
        Instantiate(ZapExplodeProto, pos - Vector3.up*0.5f, Quaternion.identity);
        
    }

    public void MakeSplash(Vector3 pos)
    {
        Instantiate(SplashExplodeProto, pos - Vector3.up*1.5f, Quaternion.identity);
        
    }

    public void MakeLevel(List<string> Map)
    {
    	this.players.Clear();
        this.tiles.Clear();
        this.hyperstones.Clear();
        
        this.Container.transform.localPosition = Vector3.zero;
        int TileRadius = 1;
        NumberOfColumns = 0;
        for(int row=0; row<Map.Count; ++row)
        {
            string tiles = Map[row];
            string aboveTiles = MapAbove[row];
            NumberOfColumns = tiles.Length;
            for(int column=0; column<tiles.Length; ++column)
            {
                Vector3 Position = this.transform.localPosition + new Vector3(column*TileRadius, 0, -row*TileRadius);
                Quaternion Rotation = Quaternion.identity;
                GameObject Prefab = null;
                string name = string.Empty;
                switch(tiles[column])
                {
                    default: break;
                    case '_': Prefab = TileFloating; name = "Floating"; break;
                    case 'X': Prefab = TileBarrier; name = "Barrier"; break;
                    case 'W': Prefab = TileWater; name = "Water"; break;
                    case 'B': Prefab = TileBox; name = "Box"; break;
                    case 'R': Prefab = TileRock; name = "Rock"; break;
                }

                if (Prefab != null)
                {
                    GameObject Tile = Instantiate(Prefab, Position, Rotation, this.Container);
                    Tile.name = name + row + "," + column;
                    this.tiles.Add(Tile.GetComponent<Tile>());
                }
                else
                {
                    
                }

                if (this.tiles[this.tiles.Count - 1] != null)
                {
                    var tile = this.tiles[this.tiles.Count - 1];
                    tile.Column = column;
                    tile.Row = row;
                }

                if (Level.levelState == LevelState.None)
                {
                    if (aboveTiles[column] == '1')
                    {
                        Position.y += 0.7f;
                        GameObject Tile = Instantiate(this.Player1SpotProto, Position, Rotation, this.Container);
                        Tile.name = "Player1Spot";
                    }
                    else if (aboveTiles[column] == '2')
                    {
                        Position.y += 0.6f;
                        GameObject Tile = Instantiate(this.Player2SpotProto, Position, Rotation, this.Container);
                        Tile.name = "Player2Spot";
                    }
                }
            }
        }

        for(int row=-1; row<=Map.Count; ++row)
        {
            for(int column=-1; column<=NumberOfColumns; ++column)
            {
                Vector3 Position = this.Container.localPosition + new Vector3(0.25f + column*TileRadius, 0.5f, -row*TileRadius);
                Quaternion Rotation = Quaternion.identity;
                Position.x -= 2;
                if(row == -1 || row == Map.Count ||
                    column == -1 || column == NumberOfColumns)
                {
                    Position.y += 0.25f;
                    GameObject DecoWall = Instantiate(DecoWallProto, Position, Rotation, this.Container);
                    DecoWall.name = "DecoWall" + row + "," + column;
                }
                Position.y -= 1;
                if(UnityEngine.Random.value > 0.5f)
                {
                    GameObject DecoOne = Instantiate(DecoOneProto, Position, Rotation, this.Container);
                    DecoOne.name = "DecoOne" + row + "," + column;

                    Position.y -= 1;
                }
                GameObject DecoTwo = Instantiate(DecoTwoProto, Position, Rotation, this.Container);
                DecoTwo.name = "DecoTwo" + row + "," + column;
            }
        }

        if (NumberOfColumns > 0)
        {
            var containerPosition = Vector3.zero;
            containerPosition.x = -((NumberOfColumns - 1) / 2f);
            containerPosition.z = (Map.Count -1) / 2f;
            this.Container.transform.localPosition = containerPosition;
        }
    }
}
