using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants.Match;
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
        private readonly IGameLogger Logger;

        public TiktakToeRoom(int time = 10) : base(RoomType.Battle)
        {
            Logger = new GameLogger();
            _time = time;
            this.Board = new List<List<PixelType>>();
            MatchStatus = MatchStatus.Init;
            this.Turn = 0;
            this.InitBoard();
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

        public override bool JoinRoom(IPlayer player)
        {
            if (!base.JoinRoom(player)) return false;
            player.SetPixelType(player.GetUserInfo().Id == this.OwnerId ? PixelType.X : PixelType.O);
            this.RoomInfo();
            return true;
        }

        private void SetTurn()
        {
            this.Turn += 1;
            this.TurnTimer?.Dispose();
            if (this.CurrentTurn != null)
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
                        Turn = Turn
                    });
                    this.SendMessage(message);
                    this.OnProcessTurn();
                    return;
                }
            }

            var rd = new Random();
            var nextPlayerRnd = rd.Next(0, 1000);
            if (nextPlayerRnd % 2 == 0)
            {
                var nextPlayer = this.Players.Values.ToList()[0];
                this.CurrentTurn = nextPlayer.GetUserInfo().Id;
            }
            else
            {
                var nextPlayer = this.Players.Values.ToList()[1];
                this.CurrentTurn = nextPlayer.GetUserInfo().Id;
            }

            this.TurnId = GameHelper.RandomString(20);
            var messageTurn = new WsMessage<TurnData>(WsTags.Turn, new TurnData
            {
                Id = this.TurnId,
                PlayerId = this.CurrentTurn,
                TimerCount = _time,
                Turn = Turn
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
            this.TurnTimer.Elapsed += (sender, args) => { this.SetTurn(); };
            this.TurnTimer.Start();
        }

        public void OnPlayerSetLock(LockData data, IPlayer player)
        {
            var invalidMess = new WsMessage<string>(WsTags.Invalid, "Không phải lượt của bạn");
            if (data.Id != TurnId && this.CurrentTurn != player.SessionId)
            {
                player.SendMessage(invalidMess);
                return;
            }

            if (this.MatchStatus != MatchStatus.Start)
            {
                invalidMess.Data = "Trận đấu chưa bắt đầu hoặc đã kết thúc";
                player.SendMessage(invalidMess);
                return;
            }

            //todo check position block row, coll


            //todo check win
        }

        public void StartGame(IPlayer player)
        {
            var invalidMess = new WsMessage<string>(WsTags.Invalid, "Bạn không phải là chủ phòng");
            if (player.GetUserInfo().Id != OwnerId)
            {
                player.SendMessage(invalidMess);
                return;
            }

            if (this.MatchStatus != MatchStatus.Init)
            {
                invalidMess.Data = "Phòng này đang chơi hoặc đã kết thúc";
                player.SendMessage(invalidMess);
                return;
            }

            if (this.Players.Count < 2)
            {
                invalidMess.Data = "Phòng phải có 2 người";
                player.SendMessage(invalidMess);
                return;
            }

            this.InitBoard();
            MatchStatus = MatchStatus.Start;
            var message = new WsMessage<GameInfoData>(WsTags.GameInfo, new GameInfoData()
            {
                Status = MatchStatus,
                TimeCount = _time
            });
            this.SendMessage(message);
            this.SetTurn();
        }

        public override bool ExitRoom(IPlayer player)
        {
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

            var place = this.Board[data.Row][data.Col];
            if (place != PixelType.None)
            {
                invalidMess.Data = "Lỗi vị trí.";
                player.SendMessage(invalidMess);
            }

            this.Board[data.Row][data.Col] = data.PixelType;
            var mess = new WsMessage<PlaceData>(WsTags.SetPlace, data);
            this.SendMessage(mess);
            var gameOver = this.CheckGameOver(data);
            if (gameOver)
            {
                Logger.Print("Game Over");
                invalidMess.Data = "Game Over";
                this.SendMessage(invalidMess);
                //todo end match
                return;
            }

            SetTurn();
        }

        private bool CheckGameOver(PlaceData data)
        {
            //Check row
            var pixelCount = 0;
            for (var row = data.Row - 1; row >= 0; row--)
            {
                if (this.Board[row][data.Col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 4)
            {
                return true;
            }

            //Check col
            pixelCount = 0;
            for (var col = data.Col - 1; col >= 0; col--)
            {
                if (this.Board[data.Row][col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 4)
            {
                return true;
            }

            //check all row
            pixelCount = 1;
            for (var row = 0; row < 10; row++)
            {
                if (this.Board[row][data.Col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 5)
            {
                return true;
            }

            //check all col
            pixelCount = 1;
            for (var col = 0; col < 10; col++)
            {
                if (this.Board[data.Row][col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 5)
            {
                return true;
            }

            //check diagonal right to left
            pixelCount = 1;
            for (int col = data.Col - 1, row = data.Row + 1; col is >= 0 and < 10; col--, row++)
            {
                if (this.Board[row][col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 5)
            {
                return true;
            }

            //check diagonal right to left
            pixelCount = 1;
            for (int col = data.Col + 1, row = data.Row - 1; col is >= 0 and < 10; col++, row--)
            {
                if (this.Board[row][col] != data.PixelType)
                {
                    break;
                }

                pixelCount++;
            }

            if (pixelCount >= 5)
            {
                return true;
            }

            return false;
        }
    }
}