using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZBase.Foundation.Singletons;
using DG.Tweening;
using System.Threading;
using System;
public class LevelManager : MonoBehaviour, IManager
{
    private int _levelIndex;
    private int _width;
    private int _height;
    private LevelDataItem _data;
    private int _numberOfBlock;
    [SerializeField] private List<Puzzle> _puzzlePieces;
    [SerializeField] private List<Block> _blocks;
    [SerializeField] private List<Node> _levelNodes;

    private float _levelTime;
    private float _oneStarTime, _twoStarTime;

    [SerializeField] private float _inputDelay;
    private bool _canMove = true;
    private CancellationTokenSource _cts;

    private GameObject _board;

    private List<GameObject> _disposeList;

    public async UniTask OnApplicationStart()
    {
        Pubsub.Subscriber.Scope<GameplayScope>().Subscribe<OnEnterLevel>(OnEnterLevel);
        _levelNodes = new List<Node>();
        _blocks = new List<Block>();
        _puzzlePieces = new List<Puzzle>();
        _disposeList = new List<GameObject>();
        await UniTask.CompletedTask;
    }

    public async UniTask OnEnterLevel(OnEnterLevel e)
    {
        UnloadLevelObjects();
        _data = e.Data;
        _levelIndex = e.Data.index;
        _cts = new CancellationTokenSource();

        string savedLevel = PlayerPrefs.GetString($"level-{_levelIndex}");

        if (!string.IsNullOrEmpty(savedLevel))
        {
            LevelBoardData savedData = JsonUtility.FromJson<LevelBoardData>(savedLevel);
            await CreateBoard(savedData, e.Data);
        }
        else
        {
            await CreateBoard(e.Data);
        }
        StartCoundownLevel().Forget();
        await UniTask.CompletedTask;
    }

    void Update()
    {
        if (_canMove)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2.right);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Move(Vector2.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Move(Vector2.down);
        }
    }

    private async UniTask CreateBoard(LevelDataItem data)
    {
        _width = data.boardWidth;
        _height = data.boardHeight;
        _numberOfBlock = data.numberOfBlock;

        _levelTime = data.levelTime;
        _oneStarTime = data.oneStarTime;
        _twoStarTime = data.twoStarTime;

        _board = new GameObject("board-holder");
        Vector2 offset = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector2 spawnPosition = new Vector2(x, y) - offset;
                var node = await LoadLevelObject("node-prefab");
                node.transform.position = spawnPosition;
                node.transform.SetParent(_board.transform);
                _levelNodes.Add(node.GetComponent<Node>());
            }
        }

        await LoadLevelBackground();
        await LoadBoardVisual(offset);
        await LoadBlock();
        await PlacePuzzle();
        Singleton.Of<DataManager>().SaveLevelBoardData(_levelIndex, _puzzlePieces, _blocks);
    }
    private async UniTask CreateBoard(LevelBoardData savedData, LevelDataItem data)
    {
        _levelNodes = new List<Node>();

        _width = data.boardWidth;
        _height = data.boardHeight;
        _numberOfBlock = data.numberOfBlock;

        _levelTime = data.levelTime;
        _oneStarTime = data.oneStarTime;
        _twoStarTime = data.twoStarTime;

        _board = new GameObject("board-holder");
        Vector2 offset = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        await LoadLevelBackground();
        await LoadBoardVisual(offset);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector2 spawnPosition = new Vector2(x, y) - offset;
                var node = await LoadLevelObject("node-prefab");
                node.transform.position = spawnPosition;
                node.transform.SetParent(_board.transform);
                _levelNodes.Add(node.GetComponent<Node>());
            }
        }

        for (int i = 0; i < savedData.puzzlePositions.Count; i++)
        {
            Vector2 position = savedData.puzzlePositions[i];
            var freeNode = _levelNodes.FirstOrDefault(n => n.Position == position);
            if (freeNode != null)
            {
                var puzzle = await LoadLevelObject($"puzzle-piece-{i}-prefab");
                puzzle.transform.position = position;
                _puzzlePieces.Add(puzzle.GetComponent<Puzzle>());
                freeNode.OccupiedObject = puzzle.GetComponent<Puzzle>();
                puzzle.GetComponent<Puzzle>().Initialize(freeNode);
                puzzle.transform.SetParent(_board.transform);
            }
        }

        for (int i = 0; i < savedData.blockPositions.Count; i++)
        {
            Vector2 position = savedData.blockPositions[i];
            var freeNode = _levelNodes.FirstOrDefault(n => n.Position == position);
            if (freeNode != null)
            {
                var block = await LoadLevelObject("block-prefab");
                block.transform.position = position;
                _blocks.Add(block.GetComponent<Block>());
                freeNode.OccupiedObject = block.GetComponent<Block>();
                block.GetComponent<Block>().InitializeBlock(freeNode);
                block.transform.SetParent(_board.transform);
            }
        }
    }

    private async UniTask StartCoundownLevel()
    {
        while (_levelTime > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _cts.Token);
            _levelTime--;
        }
        FinishLevel(false);
    }

    private void Move(Vector2 moveDir)
    {
        _canMove = false;
        InputCooldown().Forget();

        var objects = _puzzlePieces.OrderBy(b => b.Position.x).ThenBy(b => b.Position.y).ToList();
        if (moveDir == Vector2.right || moveDir == Vector2.up)
            objects.Reverse();

        foreach (var puzzle in objects)
        {
            var currentNode = puzzle.Node;
            var nextNode = GetNodePosition(currentNode.Position + moveDir);
            if (nextNode != null && nextNode.OccupiedObject == null)
            {
                currentNode.OccupiedObject = null;
                puzzle.Node = nextNode;
                nextNode.OccupiedObject = puzzle;
                puzzle.transform.DOMove(nextNode.transform.position, 0.3f);
            }
        }
        if (CheckWin())
        {
            FinishLevel(true);
        }
    }

    private bool CheckAnchor(Puzzle anchor, Vector2 dir, Puzzle expectedNeighbor)
    {
        var neighborNode = GetNodePosition(anchor.Node.Position + dir);
        return neighborNode != null && neighborNode.OccupiedObject == expectedNeighbor;
    }

    private bool CheckWin()
    {
        var piece1 = _puzzlePieces[0];
        var piece2 = _puzzlePieces[1];
        var piece3 = _puzzlePieces[2];
        var piece4 = _puzzlePieces[3];

        return CheckAnchor(piece1, Vector2.right, piece2) &&
            CheckAnchor(piece1, Vector2.up, piece3) &&
            CheckAnchor(piece4, Vector2.left, piece3) &&
            CheckAnchor(piece4, Vector2.down, piece2);
    }

    private async UniTask InputCooldown()
    {
        await UniTask.WaitForSeconds(_inputDelay);
        _canMove = true;
    }

    private Node GetNodePosition(Vector2 pos)
    {
        return _levelNodes.FirstOrDefault(n => n.Position == pos);
    }

    private async UniTask LoadBoardVisual(Vector2 offset)
    {
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        //var board = Instantiate(_boardPrefab, center - offset, Quaternion.identity);
        var board = await LoadLevelObject("board-prefab");
        board.GetComponent<SpriteRenderer>().size = new Vector2(_width + 0.5f, _height + 0.85f);
        _disposeList.Add(board);
        board.transform.SetParent(_board.transform);
    }

    private async UniTask LoadBlock()
    {
        for (int i = 0; i < _numberOfBlock; i++)
        {
            var freeNode = _levelNodes.Where(n => n.OccupiedObject == null).ToList();
            var block = await LoadLevelObject("block-prefab");
            var targetNode = freeNode[UnityEngine.Random.Range(0, freeNode.Count)];
            block.transform.position = targetNode.Position;
            var blockPiece = block.GetComponent<Block>();
            targetNode.OccupiedObject = blockPiece;
            blockPiece.InitializeBlock(targetNode);
            blockPiece.transform.SetParent(_board.transform);
            _blocks.Add(blockPiece);
        }
    }

    private async UniTask PlacePuzzle()
    {
        for (int i = 0; i < 4; i++)
        {
            var freeNode = _levelNodes.Where(n => n.OccupiedObject == null).ToList();
            var piece = await LoadLevelObject($"puzzle-piece-{i}-prefab");
            var targetNode = freeNode[UnityEngine.Random.Range(0, freeNode.Count)];

            piece.transform.position = targetNode.Position;

            var puzzlePiece = piece.GetComponent<Puzzle>();
            targetNode.OccupiedObject = puzzlePiece;
            _puzzlePieces.Add(piece.GetComponent<Puzzle>());
            puzzlePiece.Initialize(targetNode);
            piece.transform.SetParent(_board.transform);
        }
    }

    private async UniTask LoadLevelBackground()
    {
        var background = await LoadLevelObject("level-background-prefab");
        var canvas = background.GetComponentInChildren<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        _disposeList.Add(background);
        background.transform.SetParent(_board.transform);
    }

    private async UniTask<GameObject> LoadLevelObject(string objectToLoad)
    {
        var gameObject = await SingleBehaviour.Of<PoolingManager>().Rent(objectToLoad);
        return gameObject;
    }

    private void FinishLevel(bool isWin)
    {
        _cts?.Cancel();
        int stars = 1;
        if (isWin)
        {
            if (_levelTime >= _twoStarTime)
            {
                stars = 3;
            }
            else if (_levelTime >= _oneStarTime)
            {
                stars = 2;
            }
            var args = new object[] { stars, isWin };
            Pubsub.Publisher.Scope<GameplayScope>().Publish(new OnFinishLevel(_levelIndex, stars));
            Pubsub.Publisher.Scope<UIScope>().Publish(new ShowModalEvent("ui-finish-level-win-modal", false, args));
        }
        else
        {
            stars = 0;
            Pubsub.Publisher.Scope<UIScope>().Publish(new ShowModalEvent("ui-finish-level-lose-modal", false));
        }
    }

    public float GetLevelTime()
    {
        return _levelTime;
    }

    public int GetCurrentLevelIndex()
    {
        return _levelIndex;
    }

    public LevelDataItem GetCurrentLevelData()
    {
        return _data;
    }

    public void UnloadLevelObjects()
    {
        _cts?.Cancel();
        foreach (var n in _levelNodes)
        {
            n.OccupiedObject = null;
            _disposeList.Add(n.gameObject);
        }
        foreach (var b in _blocks)
        {
            b.Node = null;
            _disposeList.Add(b.gameObject);
        }
        foreach (var p in _puzzlePieces)
        {
            p.Node = null;
            _disposeList.Add(p.gameObject);
        }
        foreach (var d in _disposeList)
        {
            SingleBehaviour.Of<PoolingManager>().Return(d);
        }
        _levelNodes.Clear();
        _blocks.Clear();
        _puzzlePieces.Clear();
        Destroy(_board);
    }
}
