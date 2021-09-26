using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants.Match;
using Youtube_GameOnlineServer.GameModels;
using Youtube_GameOnlineServer.GameModels.Handlers;
using Youtube_GameOnlineServer.GameTiktaktoe.Constants;
using Youtube_GameOnlineServer.Logging;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Handlers;

namespace Youtube_GameOnlineServer.GameTiktaktoe.Room
{
    public class TiktakToeRoom : BaseRoom
    {
        private readonly int _time;
        private List<List<PixelType>> Board { get; set; }
        private MatchStatus MatchStatus { get; set; }
        private string CurrentTurn { get; set; }
        private Timer TurnTimer { get; set; }
        private string TurnId { get; set; }
        private int Turn { get; set; }
        private readonly IGameLogger _logger;
        private MatchModel CurrentMatch { get; set; }
        private RoomModel RoomModel { get; set; }
        private readonly RoomHandler _roomHandler;
        private int GameTurn { get; set; }

        public TiktakToeRoom(RoomHandler roomHandler, int time = 10) : base(RoomType.Battle)
        {
            _logger = new GameLogger();
            _roomHandler = roomHandler;
            _time = time;
            this.Board = new List<List<PixelType>>();
            MatchStatus = MatchStatus.Init;
            this.Turn = 0;
            this.GameTurn = 0;

            this.CreateRoomDb();
        }

        private void CreateRoomDb()
        {
            var roomDb = new RoomModel
            {
                RoomId = this.Id,
                Players = new List<string>(),
                Match = new List<MatchModel>(),
            };
            this.RoomModel = this._roomHandler.Create(roomDb);
        }

        private void InitBoard()
        {
            this.Board = new List<List<PixelType>>();
            for (var i = 0; i < 10; i++)
            {
                var rows = new List<PixelType>();
                for (var col = 0; col < 10; col++)
                {
                    rows.Add(PixelType.None);
                }

                this.Board.Add(rows);
            }
        }

        private void ResetMatch()
        {
            this.GameTurn += 1;
            this.CurrentMatch = new MatchModel
            {
                Winner = "",
                Loser = "",
                Point = 0,
                GameActions = new List<GameAction>()
            };
            this.InitBoard();
            this.CurrentTurn = "";
            this.Turn = 0;
            this.MatchStatus = MatchStatus.Init;
            this.TurnTimer = null;
            this.TurnId = "";
        }

        public override bool JoinRoom(IPlayer player)
        {
            if (!base.JoinRoom(player)) return false;
            player.SetPixelType(player.GetUserInfo().Id == this.OwnerId ? PixelType.X : PixelType.O);
            this.UpdateRoomDb();
            this.RoomInfo();
            return true;
        }

        private void SetTurn()
        {
            this.Turn += 1;
            this.TurnTimer?.Dispose();
            if (this.CurrentTurn != string.Empty)
            {
                var nextPlayer = this.Players.FirstOrDefault(p => p.Key != CurrentTurn).Value;
                if (nextPlayer != null)
                {
                    this.CurrentTurn = nextPlayer.GetUserInfo().Id;
                    this.TurnId = GameHelper.RandomString(20);
                    var message = new WsMessage<TurnData>(WsTags.Turn, new TurnData
                    {
                        Id = this.TurnId,
                        PlayerId = this.CurrentTurn,
                        TimerCount = _time,
                        Turn = Turn,
                        Game = this.GameTurn
                    });
                    this.SendMessage(message);
                    this.OnProcessTurn();
                    return;
                }
            }

            var rd = new Random();
            var nextPlayerRnd = rd.Next(0, 1000);
            var nextTurn = nextPlayerRnd % 2 == 0 ? this.Players.Values.ToList()[0] : this.Players.Values.ToList()[1];
            this.CurrentTurn = nextTurn.GetUserInfo().Id;
            this.TurnId = GameHelper.RandomString(20);
            var messageTurn = new WsMessage<TurnData>(WsTags.Turn, new TurnData
            {
                Id = this.TurnId,
                PlayerId = this.CurrentTurn,
                TimerCount = _time,
                Turn = Turn,
                Game = this.GameTurn
            });
            this.SendMessage(messageTurn);
            this.OnProcessTurn();
        }

        private void OnProcessTurn()
        {
            if (this.TurnTimer != null)
            {
                this.TurnTimer.Stop();
                this.TurnTimer.Dispose();
            }

            this.TurnTimer = new Timer(TimeSpan.FromSeconds(this._time).TotalMilliseconds)
            {
                AutoReset = false,
            };
            this.TurnTimer.Elapsed += (sender, args) =>
            {
                var rdnPlace = new List<List<int>>();
                for (var i = 0; i < Board.Count; i++)
                {
                    var row = Board[i];
                    for (var col = 0; col < row.Count; col++)
                    {
                        if (row[col] == PixelType.None)
                        {
                            rdnPlace.Add(new List<int>
                            {
                                i,
                                col
                            });
                        }
                    }
                }

                if (rdnPlace.Count > 0)
                {
                    rdnPlace.Shuffle();
                    var place = rdnPlace[0];
                    var player = this.Players.FirstOrDefault(p => p.Key == this.CurrentTurn).Value;
                    var placeData = new PlaceData
                    {
                        Col = place[1],
                        Row = place[0],
                        PixelType = player.GetPixelType()
                    };
                    this.SetPlace(player, placeData);
                }
                else
                {
                    var winner = this.Players.FirstOrDefault(p => p.Key != this.CurrentTurn).Value;
                    this.GameOver(winner);
                }
            };
            this.TurnTimer.Start();
        }

        public void StartGame(IPlayer player)
        {
            var invalidMess = new WsMessage<string>(WsTags.Invalid, "Bạn không phải là chủ phòng");
            if (player.GetUserInfo().Id != OwnerId)
            {
                player.SendMessage(invalidMess);
                return;
            }

            if (this.MatchStatus == MatchStatus.Start)
            {
                invalidMess.Data = "Phòng này đang chơi";
                player.SendMessage(invalidMess);
                return;
            }

            if (this.Players.Count < 2)
            {
                invalidMess.Data = "Phòng phải có 2 người";
                player.SendMessage(invalidMess);
                return;
            }

            this.ResetMatch();
            MatchStatus = MatchStatus.Start;
            var message = new WsMessage<GameInfoData>(WsTags.GameInfo, new GameInfoData
            {
                Status = MatchStatus,
                TimeCount = _time
            });
            this.SendMessage(message);
            this.SetTurn();
        }

        private void UpdateRoomDb()
        {
            this.RoomModel.Players = this.Players.Keys.ToList();
            this._roomHandler.Update(this.RoomModel.Id, RoomModel);
        }

        public override bool ExitRoom(IPlayer player)
        {
            if (this.MatchStatus == MatchStatus.Start)
            {
                var winner = this.Players.FirstOrDefault(p => p.Key != player.GetUserInfo().Id).Value;
                this.GameOver(winner);
            }

            base.ExitRoom(player);
            this.RoomInfo();

            return true;
        }

        public void SetPlace(IPlayer player, PlaceData data)
        {
            //check Player in match

            if (FindPlayer(player.GetUserInfo().Id) == null)
            {
                return;
            }

            var invalidMess = new WsMessage<string>(WsTags.Invalid, "Bạn không phải là chủ phòng");
            //validate turn owner
            if (CurrentTurn != player.GetUserInfo().Id)
            {
                invalidMess.Data = "Bạn chưa đến lượt";
                player.SendMessage(invalidMess);
                return;
            }

            if (this.MatchStatus == MatchStatus.GameOver)
            {
                invalidMess.Data = "Trận đấu đã kết thúc";
                player.SendMessage(invalidMess);
                return;
            }

            var place = this.Board[data.Row][data.Col];
            if (place != PixelType.None)
            {
                invalidMess.Data = "Lỗi vị trí.";
                player.SendMessage(invalidMess);
            }

            var gameAction = new GameAction
            {
                PlaceData = data,
                UserId = player.GetUserInfo().Id,
                CreatedAt = DateTime.Now
            };
            this.CurrentMatch.GameActions.Add(gameAction);
            this.Board[data.Row][data.Col] = data.PixelType;
            this.PrintBoard();
            var mess = new WsMessage<PlaceData>(WsTags.SetPlace, data);
            this.SendMessage(mess);
            TurnTimer.Stop();
            TurnTimer.Dispose();
            var gameOver = this.CheckGameOver(data);
            if (gameOver)
            {
                this.GameOver(player);

                return;
            }

            if (this.BoardFull())
            {
                this.GameOver(null);
                return;
            }

            SetTurn();
        }

        private bool BoardFull()
        {
            var countBlock = this.Board.SelectMany(t => t).Count(t1 => t1 != PixelType.None);
            return countBlock == 100;
        }

        private void PrintBoard()
        {
            foreach (var t in this.Board)
            {
                foreach (var t1 in t)
                {
                    switch (t1)
                    {
                        case PixelType.None:
                            Console.Write("-");
                            break;
                        case PixelType.O:
                            Console.Write("0");
                            break;
                        case PixelType.X:
                            Console.Write("X");
                            break;
                    }
                }

                Console.WriteLine();
            }

            this._logger.Print("Next turn");
        }

        private void GameOver(IPlayer winner)
        {
            try
            {
                _logger.Print("Game Over");
                MatchStatus = MatchStatus.GameOver;
                this.TurnTimer?.Dispose();
                if (winner == null)
                {
                    var endMatchData = new EndMatchData
                    {
                        WinnerId = "",
                        Point = 0
                    };
                    var mes = new WsMessage<EndMatchData>(WsTags.GameOver, endMatchData);
                    this.SendMessage(mes);
                    return;
                }
                else
                {
                    this.CurrentMatch.Winner = winner.GetUserInfo().Id;
                    this.CurrentMatch.Point = 10;
                    this.CurrentMatch.Loser = this.Players.FirstOrDefault(p => p.Key != winner.GetUserInfo().Id).Key;
                    this.RoomModel.Match.Add(this.CurrentMatch);
                    this._roomHandler.Update(RoomModel.Id, RoomModel);
                    winner.UpdatePoint(10);
                    var endMatchData = new EndMatchData
                    {
                        WinnerId = winner.GetUserInfo().Id,
                        Point = 10
                    };
                    var mes = new WsMessage<EndMatchData>(WsTags.GameOver, endMatchData);
                    this.SendMessage(mes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool CheckGameOver(PlaceData data)
        {
            try
            {
                //Check row
                var pixelCount = 1;
                for (var row = data.Row - 1; row >= 0; row--)
                {
                    var boardPlace = this.Board[row][data.Col];
                    if (boardPlace != data.PixelType)
                    {
                        break;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                //Check col
                pixelCount = 1;
                for (var col = data.Col - 1; col >= 0; col--)
                {
                    var boardPlace = this.Board[data.Row][col];
                    if (boardPlace != data.PixelType)
                    {
                        break;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                //check all row
                pixelCount = 0;
                for (var row = 0; row < 10; row++)
                {
                    var boardPlace = this.Board[row][data.Col];
                    if (boardPlace != data.PixelType)
                    {
                        pixelCount = 0;
                        continue;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }


                //check all col
                pixelCount = 0;
                for (var col = 0; col < 10; col++)
                {
                    if (this.Board[data.Row][col] != data.PixelType)
                    {
                        pixelCount = 0;
                        continue;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                //check diagonal right to left
                pixelCount = 1;
                for (int col = data.Col - 1, row = data.Row + 1; col is >= 0 and < 10; col--, row++)
                {
                    if (row > 9 || this.Board[row][col] != data.PixelType)
                    {
                        break;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                //check diagonal left to right
                pixelCount = 1;
                for (int col = data.Col + 1, row = data.Row - 1; col is >= 0 and < 10; col++, row--)
                {
                    if (row < 0 || this.Board[row][col] != data.PixelType)
                    {
                        break;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                pixelCount = 1;
                for (int col = data.Col - 1, row = data.Row - 1; col >= 0 && row >= 0; col--, row--)
                {
                    if (this.Board[row][col] != data.PixelType)
                    {
                        pixelCount = 0;
                        continue;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                pixelCount = 1;
                for (int col = data.Col + 1, row = data.Row + 1; col < 10 && row < 10; col++, row++)
                {
                    if (this.Board[row][col] != data.PixelType)
                    {
                        pixelCount = 0;
                        continue;
                    }

                    pixelCount++;
                    if (pixelCount >= 5)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return false;
            }
        }
    }
}