using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace Perudo
{
    public class Settings
    {
        private Settings() { }

        public static int PLAYER_NUMBER = 2;
        public static int DICE_NUMBER = 2;
        public static int DICE_SIZE = 6;
        public static readonly int BETS_NUMBER = DICE_NUMBER * DICE_SIZE;
        public static float STEPSIZE = 0.1f; //a number in a range (0,1), dictates the accuracy vs speed
        public static int DEFAULT_ITERATIONS = 1000;
        private static List<int> _PLAYER_HAND_SIZES = new List<int> { 1, 1 };


        public static int PLAYER_HAND_SIZES(int i)
        {
            return _PLAYER_HAND_SIZES[i];
        }
        public const string GREETINGS = "Здравствуйте! Вас приветствует солвер перудо! Для лучшей работы приложения рекомендуем развернуть консоль на весь экран.\n" +
            "На данный момент солвер умеет решать только игру 1 на 1, когда у каждого игрока по одному кубику.\n" +
            "Для начала работы введите количество итераций решения. " +
            "Чтобы оставить количество итераций по умолчанию (1000), введите 0.";
        public const string DESCRIPTION = "Решение уже рассчитывается.\n" +
            "Решение выводится в формате таблицы. Первый столбец таблицы - варианты руки, то есть кубика игрока.\n" +
            "Второй столбец - вероятность для игрока иметь данную руку в данный момент игры. Через точку с запятой приводятся эти вероятности для всех игроков.\n" +
            "Остальные столбцы отвечают за доступные действия для текущего игрока. В заголовке столбца обозначено действие: check или Bet(amount,value).\n" +
            "В ячейках таблицы указана стратегия для данного действия при условии, что у вас данная рука.\n" +
            "Стратегия указана в формате (P;EV), где P - вероятность сделать действие, EV - математическое ожидание выигрыша от этого действия.";
    }

    public class Table
	{
		private static List<Player> Players;

		private Table() { }
		static Table()
		{
			SetPlayers();
		}
		private static void SetPlayers()
		{
			Players = new List<Player>();
			for (int playerIndex = 0; playerIndex < Settings.PLAYER_NUMBER; playerIndex++)
			{
				Players.Add(new Player(playerIndex));
			}
		}
		public static Player GetPlayer(int i)
		{
			return Players[i];
		}
	}
	public class Visualizer
	{
		public void Play(TreeNode treeNode)
		{
			int amount, value;
			TreeNode nextNode;
			Display(treeNode);
			Console.WriteLine();
			Console.Write("Введите количество следующей ставки. Для других действий введите 0: ");
			amount = Convert.ToInt32(Console.ReadLine());
			if (amount == 0) nextNode = SetupGame(treeNode);
			else
			{
				Console.Write("Введите номинал следующей ставки: ");
				value = Convert.ToInt32(Console.ReadLine());
				Bet bet = new Bet(amount, value);
				nextNode = treeNode;
				foreach (TreeNode child in treeNode.Children)
				{
					if (child.PreviousBet.Index == bet.Index && !(child is Leaf)) nextNode = child;
				}
                if (nextNode == treeNode) Console.WriteLine("Выбранная ставка невозможна, попробуйте ещё раз");
            }
			Play(nextNode);
		}
		public void Display(TreeNode treeNode)
		{
			if (treeNode is Leaf) DisplayLeaf(treeNode);
			else DisplayNotLeaf(treeNode);
		}
		private void DisplayNotLeaf(TreeNode treeNode)
		{
			Console.WriteLine();
			if (treeNode.Height == 0) Console.WriteLine("\n\n\n");
			Console.WriteLine($"Идёт {treeNode.Height + 1}-й ход. Сейчас ходит игрок {treeNode.ActivePlayer.Index + 1}. Предыдущая ставка: Bet({treeNode.PreviousBet.Amount};{treeNode.PreviousBet.Value})");
			Console.WriteLine();
			Console.Write("dice\t");
			Console.Write("P\t");
			int move = 0;
			if (treeNode.Children[0] is Leaf) { Console.Write("Check\t\t"); move = 1; }
			for (; move < treeNode.Children.Count(); move++)
			{
				Console.Write($"Bet ({treeNode.Children[move].PreviousBet.Amount}; {treeNode.Children[move].PreviousBet.Value})\t");
			}
			for (int roll = 0; roll < treeNode.ActivePlayer.DiceCup.Rolls.Count(); roll++)
			{
				Console.WriteLine();
				treeNode.ActivePlayer.DiceCup.Rolls[roll].Display();
                Console.Write($"{treeNode.Strategy.GetRollProbability(treeNode.ActivePlayer.Index, roll):F3}\t");
				for (move = 0; move < treeNode.Children.Count(); move++)
				{
					Console.Write($"({treeNode.Strategy.GetMoveProbability(roll, move):F3}; {treeNode.Children[move].Strategy.GetNodeEV(treeNode.ActivePlayer.Index, roll):F3})\t");
				}
			}
			Console.WriteLine();
			return;
		}
		private void DisplayLeaf(TreeNode treeNode)
		{
			Console.WriteLine($"Игрок номер {treeNode.ActivePlayer.Index + 1} начал проверку. Далее указаны ожидаемые выигрыши обоих игроков в зависимости от руки, а также вероятности иметь данную рукую.\n");
			for (int player = 0; player < Settings.PLAYER_NUMBER; player++)
			{
				int rollCount = Table.GetPlayer(player).DiceCup.Rolls.Count();
                Console.WriteLine($"Игрок номер {player + 1}.");
				Console.Write("Рука:\t\t\t");
				for (int roll = 0; roll < rollCount; roll++)
				{
					Console.Write($"{roll + 1}\t");
				}
				Console.Write("\nВероятность:\t\t");
				for (int roll = 0; roll < rollCount; roll++)
				{
					Console.Write($"{treeNode.Strategy.GetRollProbability(player,roll):F3}\t");
				}
				Console.Write("\nОжидаемый выигрыш:\t");
				for (int roll = 0; roll < rollCount; roll++)
				{
					Console.Write($"{treeNode.Strategy.GetNodeEV(player, roll):F3}\t");
				}
				Console.WriteLine("\n");
			}
		}
		private TreeNode SetupGame(TreeNode treeNode)
		{
			Console.Write("Для проверки предыдущей ставки введите 0, для отката на N шагов назад введите N: ");
			int value = Convert.ToInt32(Console.ReadLine());
			if (value == 0 && treeNode is Root) { Console.WriteLine("Ошибка, нет ставки для проверки"); return treeNode; }
			if (value == 0) return treeNode.Children[0];
			if (value > treeNode.Height) { Console.WriteLine("Ошибка, слишком глубоко"); return treeNode; }
			TreeNode nextNode = treeNode;
			for (int i = 0; i < value; i++)
			{
				nextNode = nextNode.Parent;
			}
			return nextNode;
		}
	}

	internal class Program
	{
		static void Main(string[] args)
		{
			Visualizer visualizer = new Visualizer();
			Root root = new Root();
			root.FullyGrow();
			Console.WriteLine(Settings.GREETINGS);
			int numberOfIterations = Convert.ToInt32(Console.ReadLine());
			if (numberOfIterations == 0) numberOfIterations = Settings.DEFAULT_ITERATIONS;
			Console.WriteLine(Settings.DESCRIPTION);
			for (int i = 0; i < numberOfIterations; i++)
			{
				root.CycleStrategy();
			}
			visualizer.Play(root);
			Console.ReadKey();
		}
	}

	public class TreeNode
	{
		public int ChildIndex { get; private set; }     //index of this child among all of the parent's children
		public Bet PreviousBet { get; private set; }
		public TreeNode Parent { get; private set; }
        public TreeNode[] Children;			//this better stay public?..
		public Strategy Strategy { get; private set; }
		public int Height { get; private set; }
		public Player ActivePlayer { get; private set; }

		public TreeNode(TreeNode parent, Bet previousBet, int childIndex)  //need separate constructor for leaves?
		{
			Parent = parent;
			Height = DetermineHeight();
			ActivePlayer = DeterminePlayer();
			PreviousBet = previousBet;
			ChildIndex = childIndex;
		}
		
		public virtual int DetermineHeight() {
			int height = Parent.Height + 1;
			return height;
		}
		public virtual Player DeterminePlayer() {
			return Parent.ActivePlayer.GetNextPlayer();
		}

		public void GrowOneStep()   //creates Children array and Strategy object
		{
			GrowOneStep(out TreeNode[] children, out Strategy strategy);
			Strategy = strategy;
			Children = children;
		}
		public virtual void GrowOneStep(out TreeNode[] children, out Strategy strategy)
		{
			int numberOfChildren = (Settings.BETS_NUMBER - PreviousBet.Index) + 1;    //+1 comes from there always being a check option
			children = new TreeNode[numberOfChildren];
			strategy = new Strategy(numberOfChildren, this);
			Bet nextBet = PreviousBet;
			for (int i = 1; i < numberOfChildren; i++)
			{
				nextBet = nextBet.NextBet();
				children[i] = new TreeNode(this, nextBet, i);
			}

			children[0] = new Leaf(this);
		}
		public void FullyGrow()
		{
			
			GrowOneStep();
			if (this is Leaf) return;
			foreach (TreeNode child in Children)
			{
				child.FullyGrow();
			}
			return;
		}
		public void CycleStrategy()
		{
			Strategy.PropagateDown();
			if (this is Leaf) { Strategy.PropagateUp(); return; };
			foreach (TreeNode child in Children)
			{
				child.CycleStrategy();
			}
			Strategy.PropagateUp();
			return;
		}
	}
	public class Leaf : TreeNode 
	{ 
		public Leaf(TreeNode parent) : base(parent, parent.PreviousBet, 0) { }
		public override Player DeterminePlayer()
		{
			return Parent.ActivePlayer;
		}
		public override void GrowOneStep(out TreeNode[] children, out Strategy strategy)
		{
			children = null;
			strategy = new LeafStrategy(this);
		}

	}
	public class Root : TreeNode
	{
		public Root() : base(null, new Bet(0), 0) { }
		public override int DetermineHeight()
		{
			return 0;
		}
		public override Player DeterminePlayer()
		{
			return new Player(0);
		}
		public override void GrowOneStep(out TreeNode[] children, out Strategy strategy)
		{
			int numberOfChildren = Settings.BETS_NUMBER - Settings.DICE_NUMBER;    //minus dicenumber comes from it being impossible to start with a 1-valued bet
			children = new TreeNode[numberOfChildren];
			strategy = new Strategy(numberOfChildren, this);

			Bet nextBet = new Bet(1);
			for (int childIndex = 0; childIndex < numberOfChildren; childIndex++)
			{
				if (nextBet.Value == 1) nextBet = nextBet.NextBet();   //it is impossible to start with a 1-valued bet
				children[childIndex] = new TreeNode(this, nextBet, childIndex);
                nextBet = nextBet.NextBet();
            }
            Console.WriteLine("");
			return;
		}
	}
	//СДЕЛАТЬ ВСЕ ДВУМЕРНЫЕ МАТРИЦЫ СЛОВАРЯМИ
	//СДЕЛАТЬ ПОЛЕ _activePlayer в стратегии
	public class Strategy
	{
		public Strategy(int numberOfPossibleMoves, TreeNode treeNode)
		{
			TreeNode = treeNode;
			_numberOfPossibleMoves = numberOfPossibleMoves;
			InputRanges = new List<List<float>>();
			NodeEV = new List<List<float>>();
			MoveProbabilities = new List<List<float>>();

			for(int player = 0; player < Settings.PLAYER_NUMBER; player++)
			{
				InputRanges.Add(new List<float>());
				NodeEV.Add(new List<float>());
				Cup cup = Table.GetPlayer(player).DiceCup;
				foreach(Roll roll in cup.Rolls)
				{
					InputRanges[player].Add(roll.GetProbability());
					NodeEV[player].Add(0f);
				}
			}
			for (int roll = 0; roll < TreeNode.ActivePlayer.DiceCup.Rolls.Count; roll++)
			{
				MoveProbabilities.Add(new List<float>());

                for (int move = 0; move < numberOfPossibleMoves; move++)
                {
					MoveProbabilities[roll].Add(1f / numberOfPossibleMoves);
				}
			}
		}
		public TreeNode TreeNode { get; private set; }
        //private Dictionary<Player, Dictionary<Roll, float>> NodeEV;
        private List<List<float>> NodeEV;               //NodeEV[player][roll] is the expected value of a certain player, assuming they hold a certain roll 
        //private Dictionary<Player, Dictionary<Roll, float>> InputRanges;
        private List<List<float>> InputRanges;          //InputRanges[player][roll] is the probability for a certain player to hold a certain roll
        //private Dictionary<Roll, Dictionary<Bet, float>>
        private List<List<float>> MoveProbabilities;    //MoveProbabilities[roll][move] is the probability for current player to make a certain move, assuming they hold a certain dice 

        public float GetNodeEV(int player, int roll)
		{
			return NodeEV[player][roll];
		}
		public float GetRollProbability(int player, int roll)
		{
			return InputRanges[player][roll];
		}
		public float GetMoveProbability(int roll, int move)
		{
			return MoveProbabilities[roll][move];
		}

		public void PropagateUp()
		{
			PropagateUp(out List<List<float>> nodeEV);
			NodeEV = nodeEV;
		}
		public virtual void PropagateUp(out List<List<float>> nodeEV)   //update EVs according to children
		{
            nodeEV = new List<List<float>>();
            //if (TreeNode is Leaf) return;
            
            for (int player = 0; player < Settings.PLAYER_NUMBER; player++)
			{
				nodeEV.Add(new List<float>());
				int rollCount = Table.GetPlayer(player).DiceCup.Rolls.Count();
				for (int roll = 0; roll < rollCount; roll++)
				{
					float rollEV = 0f;
					for (int move = 0; move < _numberOfPossibleMoves; move++)
					{
						float totalMoveProb = 0f;
                        //for current player move probabilities depend on his dice, for others we need to sum up over current player's dice
                        if (player == TreeNode.ActivePlayer.Index) totalMoveProb = MoveProbabilities[roll][move];    
						else
						{
							for (int runningRoll = 0; runningRoll < TreeNode.ActivePlayer.DiceCup.Rolls.Count(); runningRoll++)  //active player's dice
							{
								totalMoveProb += MoveProbabilities[runningRoll][move] * InputRanges[TreeNode.ActivePlayer.Index][runningRoll];
							}
						}
						rollEV += totalMoveProb * TreeNode.Children[move].Strategy.NodeEV[player][roll];
                    }
					nodeEV[player].Add(rollEV);

                }
			}
		}
		public void PropagateDown() //update InputRanges according to parent and then update strategy according to EV 
		{
			PropagateRanges();
			UpdateStrategy();
		}
		private void PropagateRanges()
		{
			if(TreeNode.Height == 0) return;
			int previousPlayer = TreeNode.Parent.ActivePlayer.Index;
			for (int player = 0; player < Settings.PLAYER_NUMBER; player++)
			{
                int rollCount = Table.GetPlayer(player).DiceCup.Rolls.Count();
                if (player != previousPlayer)
				{
					for (int roll = 0; roll < rollCount; roll++)
					{
						InputRanges[player][roll] = TreeNode.Parent.Strategy.InputRanges[player][roll]; //range is not changed if player made no action
                    }
					continue;
				}
				
				float totalProbability = 0f;

                for (int roll = 0; roll < rollCount; roll++)
				{
                    float previousRange = TreeNode.Parent.Strategy.InputRanges[player][roll];
                    float previousMoveProbability = TreeNode.Parent.Strategy.MoveProbabilities[roll][TreeNode.ChildIndex];
                    float newRange = previousRange * previousMoveProbability;
                    //seems like it is the only place in code, where low numbers can accidently becom zeros. If NaN emerge anywhere, redo setters everywhere to include this check
                    if (newRange == 0) newRange = float.Epsilon;		
                    InputRanges[player][roll] = newRange;
					totalProbability += InputRanges[player][roll];
				}
				for (int roll = 0; roll < rollCount; roll++)   //renormalization
				{
                    InputRanges[player][roll] /= totalProbability;
                    if (InputRanges[player][roll] == 0) InputRanges[player][roll] = float.Epsilon;
                }
			}
			
		}
		private void UpdateStrategy()
		{
			if (this is LeafStrategy) return;
            int rollCount = TreeNode.ActivePlayer.DiceCup.Rolls.Count();
			
            for (int roll = 0; roll < rollCount; roll++)
			{
				float totalProbability = 0f;    //needed for renormalization
				for (int move = 0; move < _numberOfPossibleMoves; move++)
				{
					MoveProbabilities[roll][move] *= (1f + Settings.STEPSIZE * TreeNode.Children[move].Strategy.NodeEV[TreeNode.ActivePlayer.Index][roll]);  //not normalized, can be larger than 1
					totalProbability += MoveProbabilities[roll][move];
				}
				
				float normalizingQuotent = 1f / totalProbability;
				for (int move = 0; move < _numberOfPossibleMoves; move++)
				{
					MoveProbabilities[roll][move] *= normalizingQuotent;  //normalization
				}
			}
		}

		private int _numberOfPossibleMoves;
	}
	public class LeafStrategy : Strategy
	{
        // Value of some exact combination of rolls. First index is the better's roll, second index is the checker's roll. Positive if checker wins
        public List<List<float>> CellValue { get; private set; }
		public LeafStrategy(TreeNode treeNode): base(0, treeNode)
		{
			CalculateCellValue(treeNode.PreviousBet);
		}
		private void CalculateCellValue(Bet bet)
		{
			int n; //counter of dice of bet's value
			Cup checkerCup = TreeNode.ActivePlayer.DiceCup;
            Cup betterCup = TreeNode.ActivePlayer.GetPreviousPlayer().DiceCup;
            CellValue = new List<List<float>>();
			foreach (Roll betterRoll in betterCup.Rolls) 
			{
				CellValue.Add(new List<float>());
				foreach(Roll checkerRoll in checkerCup.Rolls)
				{
					n = betterRoll.GetAmountOfValue(bet.Value - 1) + checkerRoll.GetAmountOfValue(bet.Value - 1);	//"-1", because bet.Value is in range (1;;6)
					if (bet.Value != 1) n += betterRoll.GetAmountOfValue(0) + checkerRoll.GetAmountOfValue(0);
					float value = 1f;
                    if (n >= bet.Amount) value = -1f;
					CellValue[betterRoll.Index].Add(value);
                }
			}
		}
		public override void PropagateUp(out List<List<float>> nodeEV)
		{
			nodeEV = new List<List<float>>();
			int better = TreeNode.ActivePlayer.GetPreviousPlayer().Index;
            int betterRollCount = TreeNode.ActivePlayer.GetPreviousPlayer().DiceCup.Rolls.Count();
            int checker = TreeNode.ActivePlayer.Index;
            int checkerRollCount = TreeNode.ActivePlayer.DiceCup.Rolls.Count();

            for (int player = 0; player < Settings.PLAYER_NUMBER; player++)
			{
                int rollCount = Table.GetPlayer(player).DiceCup.Rolls.Count();
                nodeEV.Add(new List<float>());
				for (int roll = 0;  roll < rollCount; roll++)
				{
					nodeEV[player].Add(0f);
				}
			}
			for (int betterRoll = 0; betterRoll < betterRollCount; betterRoll++)
			{
				for (int checkerRoll = 0; checkerRoll < checkerRollCount; checkerRoll++)
				{
					nodeEV[better][betterRoll] -= CellValue[betterRoll][checkerRoll] * GetRollProbability(checker, checkerRoll); //minus comes from better losing when checker wins
					nodeEV[checker][checkerRoll] += CellValue[betterRoll][checkerRoll] * GetRollProbability(better, betterRoll);
				}
			}
		}
	}

	public class Player
	{
		public int Index { get; private set; }
		public Cup DiceCup;
		private int _handSize;


		public Player(int index) { Index = index; _handSize = Settings.PLAYER_HAND_SIZES(index); DiceCup = new Cup(_handSize); }
		public Player GetNextPlayer()
		{
			if (this is null) return new Player(0);
			int index = Index + 1;
			if (index < Settings.PLAYER_NUMBER) return new Player(index);
			return new Player(0);
		}
		public Player GetPreviousPlayer()
		{
			int lastIndex = Settings.PLAYER_NUMBER - 1;
			if (this is null) return new Player(lastIndex);
			int index = Index - 1;
			if (index >= 0) return new Player(index);
			return new Player(lastIndex);
		}
		public int GetHandSize()
		{
			return _handSize;
		}
	}
	public class Cup    // Player's cup, containing all the possible rolls
	{
		public int DiceAmount { get; private set; }

		public List<Roll> Rolls = new List<Roll>();
		public Cup(int diceAmount)
		{
			DiceAmount = diceAmount;
			FillRolls();
		}
		public Cup(Player player)
		{
			DiceAmount = player.GetHandSize();
			FillRolls();
		}
		private void FillRolls()
		{
			Rolls.Clear();
			Rolls.Add(new Roll(DiceAmount));
			while(true)
			{
				Roll nextRoll = Rolls.Last().NextRoll();
				if (nextRoll == null) break;
				Rolls.Add(nextRoll);
			}
		}
	}
	public class Roll
	{
		private List<int> Values;           // This is a roll in Value-form, like {1,1,3,3,3,5}
		private List<int> AmountsOfValue;   // This form of a roll is a 6-element array like {2,0,3,0,1,0}
		private float _probability;         // Initial probability of rolling this roll

		public int Index { get; private set; }
		
		public Roll(List<int> values, int index)
		{
			Values = values;
            Index = index;
            FillAmountOfValue();
            _probability = 0f;
            CalculateProbability();
		}

		public Roll(int diceAmount)	//Constructor for the first Roll
		{
			Index = 0;
			Values = new List<int>();
			for(int i = 0; i < diceAmount; i++)
			{
				Values.Add(1);
			}
			AmountsOfValue = new List<int>() {diceAmount};
			for (int i = 1; i < Settings.DICE_SIZE; i++)
			{
				AmountsOfValue.Add(0);
			}
			_probability = 1f / IntPow(Settings.DICE_SIZE, diceAmount);
		}
		public Roll NextRoll()  
		{
			List<int> nextValues = new List<int>();
			int diceAmount = Values.Count();

			int i = diceAmount - 1; //filling Values
			for (; ; i--)
			{
				if (i < 0) return null;
				if (Values[i] == Settings.DICE_SIZE) continue;
				break;
			}
			for (int j = 0; j < i; j++)
			{
				nextValues.Add(Values[j]);
			}
			for (int j = i; j < diceAmount; j++)
			{
				nextValues.Add(Values[i] + 1);
			}

			return new Roll(nextValues, Index + 1);
		}
		private int IntPow(int x, int power)
		{
			int result = 1;
			for(int i = 0; i < power; i++)
			{
				result *= x;
			}
			return result;
		}
		private int Factorial(int n)
		{
			int result = 1;
			for (; n > 0; n--) { result *= n; }
			return result;
		}

        public int GetAmountOfValue(int value)
        {
            return AmountsOfValue[value];
        }
        public float GetProbability()
		{
			if (_probability == 0f) CalculateProbability();
            return _probability;
		}

		private void CalculateProbability()
		{
            _probability = (float)Factorial(Values.Count()) / (float)IntPow(Settings.DICE_SIZE, Values.Count());
            for (int i = 0; i < Settings.DICE_SIZE; i++)
            {
                _probability = _probability / Factorial(GetAmountOfValue(i));
                //Console.WriteLine("");
            }
        }
		private void FillAmountOfValue()
		{
            AmountsOfValue = new List<int>();
            for (int j = 0; j < Settings.DICE_SIZE; j++)
            {
                AmountsOfValue.Add(0);
            }
            for (int j = 0; j < Values.Count; j++)
            {
                AmountsOfValue[Values[j] - 1] += 1;
            }
        }

		public void Display()
		{
			string output = null;
			for (int i = 0; i < Values.Count; i++)
			{
				output += Values[i];
			}
			Console.Write(output + "\t");
		}
	}
	public class Bet
	{
		public int Amount { get; private set; }
		public int Value { get; private set; }
		public int Index { get; private set; }
		public Bet(int amount, int value)
		{
			Amount = amount;    //test for amount > 0 here?
			Value = value;      //test for 0 < value < 7 here? 
			if (value == 1)
			{
				Index = Math.Min(amount * (2 * Settings.DICE_SIZE - 1), (Settings.DICE_NUMBER) * (Settings.DICE_SIZE - 1) + amount);
			}
			else
			{
				Index = (amount - 1) * (Settings.DICE_SIZE - 1) + (value - 1) + (amount - 1) / 2;  //Works, so fuck it
			}

		}
		public Bet(int index)
		{
			Index = index;
			if (index == 0)
			{
				Amount = 0;
				Value = 0;
				return;
			}
			//One is short for 1-Value bet
			//Regular one is a one, which is at the same rank as if there were infinite dice
			//Tail one is the opposite
			int _numberOfRegularOnes = Settings.BETS_NUMBER / (2 * Settings.DICE_SIZE - 1);
			int _numberOfNotOnes = Settings.DICE_NUMBER * (Settings.DICE_SIZE - 1);
			int _numberOfRegularBets = _numberOfNotOnes + _numberOfRegularOnes;
			bool _isTailOne = index > _numberOfRegularBets;
			if (_isTailOne)
			{
				Value = 1;
				Amount = _numberOfRegularOnes + (index - _numberOfRegularBets);
				return;
			}
			int _onesBefore = index / (2 * Settings.DICE_SIZE - 1);
			bool _isOne = (index % 11 == 0);
			if (_isOne)
			{
				Value = 1;
				Amount = _onesBefore;
				return;
			}
			int _betIndexExceptOnes = index - _onesBefore;
			Amount = ((_betIndexExceptOnes - 1) / (Settings.DICE_SIZE - 1) + 1);
			Value = (_betIndexExceptOnes - 1) % (Settings.DICE_SIZE - 1) + 2;
		}
		public Bet NextBet()
		{
			return new Bet(Index + 1);
		}
	}
}