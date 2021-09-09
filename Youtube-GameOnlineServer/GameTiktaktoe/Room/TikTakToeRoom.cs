using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants.Match;
using Youtube_GameOnlineServer.GameTiktaktoe.Constants;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Handlers;

namespace Youtube_GameOnlineServer.GameTiktaktoe.Room
{
    public class TiktakToeRoom : BaseRoom
    {
        private readonly int _time;
        private List<List<int>> Board { get; set; }
        private MatchStatus MatchStatus { get; set; }
        private string CurrentTurn { get; set; }
        private Timer TurnTimer { get; set; }
        private string TurnId { get; set; }

        public TiktakToeRoom(int time = 10) : base(RoomType.Battle)
        {
            _time = time;
            this.Board = new List<List<int>>();
            MatchStatus = MatchStatus.Init;
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
            if (this.CurrentTurn != string.Empty)
            {
                var nextPlayer = this.Players.FirstOrDefault(p => p.Key != CurrentTurn).Value;
                if (nextPlayer != null)
                {
                    this.CurrentTurn = nextPlayer.SessionId;
                    this.TurnId = GameHelper.RandomString(20);
                    this.OnProcessTurn();
                    return;
                }
            }

            var rd = new Random();
            var nextPlayerRnd = rd.Next(0, 1000);
            if (nextPlayerRnd % 2 == 0)
            {
                var nextPlayer = this.Players.Values.ToList()[0];
                this.CurrentTurn = nextPlayer.SessionId;
            }
            else
            {
                var nextPlayer = this.Players.Values.ToList()[1];
                this.CurrentTurn = nextPlayer.SessionId;
            }
            this.TurnId = GameHelper.RandomString(20);
            var messageTurn = new WsMessage<TurnData>(WsTags.Turn, new TurnData
            {
                Id = this.TurnId,
                PlayerId = this.CurrentTurn,
                TimerCount = _time
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
                this.SetTurn();
            };
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

            MatchStatus = MatchStatus.Start;
            this.SetTurn();
        }

        public override bool ExitRoom(IPlayer player)
        {
            base.ExitRoom(player);

            this.RoomInfo();
            return true;
        }
    }
}