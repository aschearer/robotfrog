using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class Level : MonoBehaviour {

    public static bool IsGameOver;

    public GameObject TileBarrier;
    public GameObject TileFloating;
    public GameObject TileWater;
    public GameObject TileBox;
    public GameObject TileRock;
    public GameObject Player;
    public GameObject CursorProto;

    public Transform Container;

    public List<string> Map;

    private List<Tile> tiles = new List<Tile>();

    public void Start()
    {
        if(Map.Count == 0)
        {
            // 12x12 including walls
            ////Map.Add("XXXXXXXXXXXX");
            ////Map.Add("X__________X");
            ////Map.Add("X_1________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X________2_X");
            ////Map.Add("X__________X");
            ////Map.Add("XXXXXXXXXXXX");

            // 7x7
            ////Map.Add("XXXXXXX");
            ////Map.Add("X_____X");
            ////Map.Add("X_____X");
            ////Map.Add("X_____X");
            ////Map.Add("X_____X");
            ////Map.Add("X_____X");
            ////Map.Add("XXXXXXX");

            // 12x7
            ////Map.Add("XXXXXXXXXXXX");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("X__________X");
            ////Map.Add("XXXXXXXXXXXX");

            // 10x7
            ////Map.Add("XXXXXXXXXX");
            ////Map.Add("X________X");
            ////Map.Add("X_1______X");
            ////Map.Add("X________X");
            ////Map.Add("X______2_X");
            ////Map.Add("X________X");
            ////Map.Add("XXXXXXXXXX");

            // 10x7 with columns
            Map.Add("XXXXXXXXXX");
            Map.Add("X_______WX");
            Map.Add("X_1__W___X");
            Map.Add("X___RB___X");
            Map.Add("X___W__2_X");
            Map.Add("XW_______X");
            Map.Add("XXXXXXXXXX");
        }
        MakeLevel(Map);
    }

    public void Update()
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

            if (playerCount == 2)
            {
                break;
            }
        }

        if (playerCount < 2 && !Level.IsGameOver)
        {
            // Game is over
            Level.IsGameOver = true;
            this.StartCoroutine(this.GameOverSequence());
        }

        // Test code to trigger explosions via mouse
        ////if (Input.GetMouseButtonDown(0))
        ////{
        ////    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ////    RaycastHit hit;
        ////    if (Physics.Raycast(ray, out hit))
        ////    {
        ////        var tile = hit.transform.gameObject.GetComponent<Tile>();
        ////        if (tile != null)
        ////        {
        ////            this.ExplodeAt(tile, 1);
        ////        }
        ////    }
        ////}
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

        yield return new WaitForSeconds(1f);

        // Step 1 destroy all things
        for (int i = 0; i < this.Container.childCount; i++)
        {
            var child = this.Container.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }

        // Step 2 create all things
        this.MakeLevel(this.Map);

        Level.IsGameOver = false;
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

    public void MakeLevel(List<string> Map)
    {
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
                Color Tint = Color.white;
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
                    if (Prefab == TileFloating || Prefab == TileRock)
                    {
                        this.tiles.Add(Tile.GetComponent<Tile>());
                    }
                    else
                    {
                        this.tiles.Add(null);
                    }
                }
                else
                {
                    // Create an ordinary ground tile
                    GameObject Tile = Instantiate(TileFloating, Position, Rotation, this.Container);
                    Tile.name = "Tile" + row + "," + column;
                    this.tiles.Add(Tile.GetComponent<Tile>());

                    Prefab = Player;
                    ControllerId controllerId = ControllerId.KeyboardLeft;
                    switch (tiles[column])
                    {
                        case '1':
                            name = "Player1";
                            Tint = Color.red;
                            break;
                        case '2':
                            name = "Player2";
                            controllerId = ControllerId.KeyboardRight;
                            Tint = Color.blue;
                            break;
                    }

                    Position.y += 1;
                    GameObject player = Instantiate(Prefab, Position, Rotation, this.Container);
                    player.name = name;
                    var playerView = player.GetComponent<Player>();
                    playerView.playerId = controllerId;
                    playerView.Level = this;
                    playerView.Tint = Tint;


                    GameObject cursor = Instantiate(CursorProto, Vector3.zero, Quaternion.identity);
                    cursor.transform.SetParent(player.transform, false);
                    var cursorView = cursor.GetComponent<Cursor>();
                    cursorView.Tint = Tint;

                    playerView.Cursor = cursorView;
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
