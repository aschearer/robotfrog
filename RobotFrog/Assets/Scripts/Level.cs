using UnityEngine;
using System.Collections.Generic;
using System;

public class Level : MonoBehaviour {
    
    public GameObject TileRock;
    public GameObject TileFloating;
    public GameObject TileWater;
    public GameObject Player;

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
            Map.Add("X________X");
            Map.Add("X___W__2_X");
            Map.Add("XW_______X");
            Map.Add("XXXXXXXXXX");
        }
        MakeLevel(Map);
    }

    public void Update()
    {
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
                    case 'X': Prefab = TileRock; name = "Rock"; break;
                    case 'W': Prefab = TileWater; name = "Water"; break;
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
                            break;
                        case '2':
                            name = "Player2";
                            controllerId = ControllerId.KeyboardRight;
                            break;
                    }

                    Position.y += 1;
                    GameObject player = Instantiate(Prefab, Position, Rotation, this.Container);
                    player.name = name;
                    var playerView = player.GetComponent<Player>();
                    playerView.playerId = controllerId;
                    playerView.Level = this;
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
            var containerPosition = this.Container.transform.localPosition;
            containerPosition.x = -((numberOfColumns - 1) / 2f);
            this.Container.transform.localPosition = containerPosition;
        }
    }
}
