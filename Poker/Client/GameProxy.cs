﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractStorage;
using GameLogic;

namespace Client
{
    public class GameProxy
    {
        private static Dictionary<char, GameState> charTurn = new Dictionary<char, GameState>() {
            { 'p', GameState.PreFlop},
            { 'f', GameState.Flop},
            { 't', GameState.Turn},
            { 'r', GameState.River },
            { 's', GameState.ShowTime}
        };

        private static Dictionary<char, TurnRole> charRole = new Dictionary<char, TurnRole>() {
            { 'd', TurnRole.Dealer},
            { 's', TurnRole.SmallBlind},
            { 'b', TurnRole.BigBlind}
        };
        public IContract Proxy { get; private set; }
        public int CurrentGameId { get; private set; }
        public Dictionary<int, int> GamePlayerCountDict;
        public string PlayerName = "Unknown";
        public int PlayerId;
        public int PlayerPosition;
        public GameState CurrentTurn;
        public List<string> PlayerNames;
        public List<Card> TableCards;
        public Dictionary<TurnRole, int> RoleSeat;
        public State CurrentState;
        public GameProxy(IContract proxy) {
            Proxy = proxy;
            GamePlayerCountDict = new Dictionary<int, int>();
            PlayerNames = new List<string>();
            TableCards = new List<Card>();
            RoleSeat = new Dictionary<TurnRole, int>() {
                { TurnRole.Dealer, -1 },
                { TurnRole.SmallBlind, -1 },
                { TurnRole.BigBlind, -1 }
            };
            CurrentState = null;
        }

        public bool GetExistingGamesWithPlayerCount() {
            var gamePlayerCountDict = Proxy.ShowExistingGames();
            if (gamePlayerCountDict is null)
                return false;
            GamePlayerCountDict = gamePlayerCountDict;
            return true;
        }

        public bool CreateNewGame(int gameId) {
            var successed = Proxy.CreateNewGame(gameId);
            return successed;
        }

        public void SetName(string name) {
            PlayerName = name;
        }

        public bool SetState(int gameId) {
            var playerNames = Proxy.CheckPlayerNames(gameId);
            if (playerNames is null) {
                return false;
            }
            PlayerNames = playerNames;
            return true;
        }

        public bool TryJoinTheGame(int gameId) {
            var position = -1;
            for (int i = 0; i < 10; i++) {
                if (PlayerNames[i] != null) {
                    position = i;
                    break;
                }       
            }
            if (position == -1)
                return false;

            var playerId = Proxy.Join(gameId, PlayerName, position);
            PlayerPosition = position;
            if (playerId == -1)
                return false;
            PlayerId = playerId;
            CurrentGameId = gameId;
            return true;
        }

        public bool LeaveTheGame() {
            var successed = Proxy.LeaveTheGame(CurrentGameId, PlayerId);
            return successed;
        }

        public bool MakeBet(int bet)
        {
            if (CurrentState.CurrentPlayer != PlayerPosition)
                return false;
            var successed = Proxy.DoBet(CurrentGameId, PlayerId, bet);
            return successed;
        }

        public bool MakeCall()
        {
            if (CurrentState.CurrentPlayer != PlayerPosition)
                return false;
            var successed = Proxy.DoCall(CurrentGameId, PlayerId);
            return successed;
        }

        public bool MakeRaise(int bet)
        {
            if (CurrentState.CurrentPlayer != PlayerPosition)
                return false;
            var successed = Proxy.DoRaise(CurrentGameId, PlayerId, bet);
            return successed;
        }

        public bool MakeFold()
        {
            if (CurrentState.CurrentPlayer != PlayerPosition)
                return false;
            var successed = Proxy.DoFold(CurrentGameId, PlayerId);
            return successed;
        }

        public bool SendMessage(string message) {
            var successed = Proxy.SendMessage(CurrentGameId, PlayerId, message);
            return successed;
        }

        public bool UpdateTableCards() {
            var tableCards = Proxy.CheckTableCards(CurrentGameId);
            if (tableCards is null)
                return false;

            TableCards = Card.ParseCards(tableCards);
            return true;
        }

        public bool UpdatePlayerRoles()
        {
            var roles = Proxy.CheckRoles(CurrentGameId);
            if (roles is null)
                return false;
            foreach (var role in roles.Keys) {
                RoleSeat[charRole[role]] = roles[role];
            }

            return true;
        }

        public bool UpdateRegularData()
        {
            var state = Proxy.GetState(CurrentGameId);
            if (state is null)
                return false;
            CurrentState = state;
            return true;
        }
    }
}
