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
    public int world = 0;
    public int MinPlayers = 2;
    public int MaxPlayers = 2;

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
        switch(world)
        {
            case 0:
            // 8x5 with columns
            Map.Add("_______W");
            Map.Add("____W___");
            Map.Add("___RB___");
            Map.Add("___W____");
            Map.Add("W_______");
            MapAbove.Add("________");
            MapAbove.Add("_1____3_");
            MapAbove.Add("________");
            MapAbove.Add("_4____2_");
            MapAbove.Add("________");
            break;
            case 1:
            // 10x8 with columns
            Map.Add("_______W");
            Map.Add("_W_____W");
            Map.Add("_______W");
            Map.Add("____W___");
            Map.Add("___W____");
            Map.Add("W_______");
            Map.Add("W____W__");
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
        switch(levelState)
        {
            case LevelState.None:
                MakeLevel(Map);
                levelState = LevelState.WaitingToSpawn;
                break;
            case LevelState.WaitingToSpawn:
                ////if(SpawnTimer.Tick(Time.deltaTime))
                ////{
                ////    int spawnSlot = PlayerCount();
                ////    for(int i=spawnSlot; i<MinPlayers; ++i)
                ////    {
                ////        SpawnPlayer(i, i);
                ////    }
                ////    levelState = LevelState.Playing;
                ////}
                for(int i=0; i<6; ++i)
                {
                    if(controllers[i].inputData.PrimaryIsDown)
                    {
                        if(!controllers[i].Pawn)
                        {
                            int spawnSlot = PlayerCount();
                            SpawnPlayer(spawnSlot, i);
                            if(PlayerCount() >= MaxPlayers)
                            {
                                levelState = LevelState.Playing;
                            }
                        }
                    }
                }
                break;
            case LevelState.Playing:
                if(PlayerCount() < 2)
                {
                    levelState = LevelState.GameOver;
                    this.StartCoroutine(GameOverSequence());
                }
                break;
        }
        

    }

    private int PlayerCount()
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

    public void ExplodeAt(Tile tile, int radius)
    {
        foreach (var neighbor in this.tiles)
        {
            if (neighbor == null)
            {
                continue;
            }

            var distance = Mathf.Abs(tile.Column - neighbor.Column) + Mathf.Abs(tile.Row - neighbor.Row);
            if (distance <= radius)
            {
                this.StartCoroutine(neighbor.HandleExplode(distance * 0.4f));
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
            bool bUseAltMat = spawnSlot >= 2;
            controllers[controller].Pawn = playerView;

            GameObject cursor = Instantiate(CursorProto, Vector3.zero, Quaternion.identity);
            cursor.transform.SetParent(player.transform, false);
            var cursorView = cursor.GetComponent<Cursor>();
            cursorView.Tint = Tint;

            playerView.Cursor = cursorView;
            playerView.flyingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;
            playerView.sittingModel.GetComponent<Renderer>().material = bUseAltMat ? CommonAlt : CommonMain;

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

        if (numberOfColumns > 0)
        {
            var containerPosition = Vector3.zero;
            containerPosition.x = -((numberOfColumns - 1) / 2f);
            this.Container.transform.localPosition = containerPosition;
        }
    }
}
