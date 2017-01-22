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
    public Material CommonMain;
    public Material CommonAlt;

    public Transform Container;

    public List<string> Map;
    public List<string> MapAbove;

    private List<Tile> tiles = new List<Tile>();

    private List<Controller> controllers = new List<Controller>();

    private List<Player> players = new List<Player>();
    
    private ManualTimer SpawnTimer = new ManualTimer();

    public void Start()
    {
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
            case 3:
            case 4:
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
            break;
        }
    }

    public void Update()
    {
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
                    levelState = LevelState.Playing;
                }
                for(int i=0; i<6; ++i)
                {
                    if(controllers[i].inputData.PrimaryIsDown || controllers[i].isInTheGame)
                    {
                        if(!controllers[i].Pawn)
                        {
                            int spawnSlot = CountPlayers();
                            SpawnPlayer(spawnSlot, i);
                            if(CountPlayers() >= PlayerCount)
                            {
                                levelState = LevelState.Playing;
                            }
                        }
                    }
                }
                break;
            case LevelState.Playing:
                if(CountPlayers() < 2)
                {
                    levelState = LevelState.GameOver;
                    this.StartCoroutine(GameOverSequence());
                }
                break;
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
        this.MakeLevel(this.Map);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            twirl.angle = 180 + 180 * t;
            yield return null;
        }

        twirl.angle = 0;
        twirl.enabled = false;

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

            var distance = Mathf.Abs(origin.Column - neighbor.Column) + Mathf.Abs(origin.Row - neighbor.Row);
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

    public void SpawnPlayer(int spawnSlot, int controller)
    {
        bool bDidFind = false;
        int spawnRow = 0;
        int spawnCol = 0;
        GameObject Prefab = null;

        int TileRadius = 1;
        Quaternion Rotation = Quaternion.identity;
        Color Tint = Color.white;
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
            player.name = "Player"+spawnSlot;
            var playerView = player.GetComponent<Player>();
            playerView.Level = this;
            playerView.Tint = Tint;
            playerView.Column = (int)Mathf.Round(Position.x);
            playerView.Row = (int)Mathf.Round(-Position.z);
            bool bUseAltMat = spawnSlot >= 2;
            controllers[controller].isInTheGame = true;
            controllers[controller].Pawn = playerView;

            GameObject cursor = Instantiate(CursorProto, Vector3.zero, Quaternion.identity);
            cursor.transform.SetParent(player.transform, false);
            var cursorView = cursor.GetComponent<Cursor>();
            cursorView.Tint = Tint;

            playerView.Cursor = cursorView;
            playerView.flyingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;
            playerView.sittingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;
            this.players.Add(playerView);

            var objectName = spawnSlot == 0 ? "RedPlayer" : "BluePlayer";
            var promptGameObject = GameObject.Find(objectName);
            if (promptGameObject != null)
            {
                var prompt = promptGameObject.GetComponent<CanvasGroup>();
                this.StartCoroutine(this.Fade(prompt));
            }
        }
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
        
        this.Container.transform.localPosition = Vector3.zero;
        int TileRadius = 1;
        int numberOfColumns = 0;
        for(int row=0; row<Map.Count; ++row)
        {
            string tiles = Map[row];
            numberOfColumns = tiles.Length;
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
            }
        }

        for(int row=-1; row<=Map.Count; ++row)
        {
            for(int column=-1; column<=numberOfColumns; ++column)
            {
                Vector3 Position = this.Container.localPosition + new Vector3(0.25f + column*TileRadius, 0.5f, -row*TileRadius);
                Quaternion Rotation = Quaternion.identity;
                Position.x -= 2;
                if(row == -1 || row == Map.Count ||
                    column == -1 || column == numberOfColumns)
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

        if (numberOfColumns > 0)
        {
            var containerPosition = Vector3.zero;
            containerPosition.x = -((numberOfColumns - 1) / 2f);
            containerPosition.z = (Map.Count -1) / 2f;
            this.Container.transform.localPosition = containerPosition;
        }
    }
}
