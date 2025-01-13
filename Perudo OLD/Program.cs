using System;

namespace Перудо
{
    public const int PLAYER_NUMBER 2;
    public const int DICE_NUMBER 2;
    public const int BETS_NUMBER 12;
    public const int DICE_SIZE 6;
    internal class Program
    {
        static void Main(string[] args)
        {
            for(int i = 1; i <= BET_NUMBER, i++){
                Bet bet = Bet(i);
                Console.Write("{");
                Console.Write(bet.Amount);
                Console.Write(",");
                Console.Write(bet.Value);
                Console.Write("}");
            }
            /*int[] startingRestarts = { 1, 1 };
            GameState startState = new GameState(1, Player.Red, startingRestarts);
            TreeNode root = new TreeNode(null, startState, 0);
            GameTree tree = new GameTree(root);
            root.FullyGrow();
            root.Display();*/
            Console.ReadKey();
        }
    }

    public class GameTree
    {
        private TreeNode _root;

        public GameTree(TreeNode root)
        {
            _root = root;
        }



    }

    public class TreeNode
    {
        public bool IsLeaf { get; private set; } = false;
        public bool IsFullyGrown { get; private set; } = false;
        public bool IsGrown { get; private set; } = false;

        public int PreviousBetIndex { get; private set; }  
        public Bet PreviousBet { get; private set; }
        public TreeNode Parent { get; private set; }
        public TreeNode[] Children { get; private set; }
        public Strategy Strategy { get; private set; }
        public int Height { get; private set; }
        public Player CurrentPlayer { get; private set; }

        public TreeNode(TreeNode parent, int previousBetIndex, int height)
        {
            Parent = parent;
            Height = height;
            PreviousBetIndex = previousBetIndex;
        }

        public void GrowOneStep()
        {
            //int numberOfPossibleMoves = _state.GetNumberOfPossibleMoves();    //state is not needed, because in this game state = PreviousBetIndex
            IsLeaf =  (PreviousBetIndex == 0 && Height != 0);
            if (IsLeaf)   //
            {
                IsGrown = true;
                IsFullyGrown = true;
                Children = null;
                Strategy = null;
                return;
            }
            int numberOfChildren = (BETS_NUMBER - PreviousBetIndex) + 1;    //
            if (numberOfChildren == 1)
            {
                IsGrown = true;
                IsLeaf = true;
                IsFullyGrown = true;
                Children = null;
                Strategy = null;
                return;
            }
            Children = new TreeNode[numberOfChildren];
            Strategy = new float[numberOfChildren][DICE_SIZE];
            for (int i = 1; i < numberOfChildren; i++)
            {
                for(int j = 1; j < DICE_SIZE; j++)
                {
                    Strategy[i][j] = 1 / numberOfChildren;
                }
            }

            IsGrown = true;


            for (int i = 1; i < numberOfChildren; i++)
            {
                Children[i] = new TreeNode(this, PreviousBetIndex + i, Height + 1);
            }

            Children[0] = new TreeNode(this, 0, Height + 1);
        }
        public void FullyGrow()
        {
            GrowOneStep();
            if (IsLeaf)
            {
                IsFullyGrown = true;
                return;
            }

            foreach (TreeNode child in Children)
            {
                child.FullyGrow();
            }
            IsFullyGrown = true;
            return;
        }
        public void Display()
        {
            if (IsLeaf)
            {
                Console.Write("Green,");
                return;
            }
            Console.Write(_state.CurrentPlayer);
            Console.Write("-> <|");
            for (int i = 0; i < Children.Length; i++)
            {
                Console.Write(@"""" + ((MoveType)i) + @"""" + "->");
                Children[i].Display();
            }
            Console.Write("\b");
            Console.Write("|>,");
            return;
        }
    }

    public class Strategy
    {
        private int _numberOfPossibleMoves;    
        private float[] _movesEV;
        private Range[PLAYER_NUMBER] _ranges;
    }
    
    public class Range
    {
        private int[] _diceProbs;
    }

    public enum Player
    {
        Red = 0,
        Blue = 1
    }
    public class Bet
    {
        public int Amount { get; private set; }
        public int Value { get; private set; }
        public int Index { get; private set; }
        public Bet(int amount, int value){
            Amount = amount;    //test for amount > 0 here?
            Value = value;      //test for 0 < value < 7 here? 
            if (value == 1){
                //Index = amount * 11;
                //if (Index > BETS_NUMBER)    { Index = (BETS_NUMBER / 6) * 5 + amount; }
                Index = min(amount * 11, (BETS_NUMBER / 6) * 5 + amount);
            }
            else{
                Index = (amount - 1) * 5 + value + amount / 2;  //TEST THIS FORMULA, /2 because of the ones
            }
            
        }
        public Bet(int index){
            Index = index;
            //One is short for 1-Value bet
            //Regular one is a one, which is at the same rank as if there were infinite dice
            //Tail one is the opposite
            int _numberOfRegularOnes = BET_NUMBER / 11;
            int _numberOfNotOnes = DICE_NUMBER * (DICE_SIZE - 1);
            int _numberOfRegularBets = _numberOfNotOnes + _numberOfRegularOnes
            bool _isTailOne = index > _numberOfRegularBets;
            if (isTailOne) {
                Value = 1; 
                Amount = _numberOfRegularOnes + (index - _numberOfRegularBets);
                return;
            }
            int _onesBefore = index / 11;   //Before stands for amount of ones 
            bool _isOne = (index % 11 == 0);
            if (_isOne){
                Value = 1;
                Amount = _onesBefore;
            }

            int _betIndexExceptOnes = index - _onesBefore;
            Amount = ((_betIndexExceptOnes - 1) / 5) + 1;
            Value = (_betIndexExceptOnes - 1) % 5 + 1;
        }
    }
    public const Bet Check {Amount = 0; Value = 0; Index = 0 }
}
