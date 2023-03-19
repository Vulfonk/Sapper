using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var scene = new Scene(40, 15, 10);
            var gameController = new GameController(scene);
            var inputParser = new InputParser();
            bool inGame = true;
            GameStatus gameStatus = GameStatus.InGame;
            string strScene;

            while (GameStatus.InGame == gameStatus)
            {
                Console.Clear();
                strScene = scene.ToString();
                ColoredConsole.WriteLine(strScene);
                string input = Console.ReadLine();
                var userActionInfo = inputParser.ParseInput(input);
                gameStatus = gameController.ExecuteUserAction(userActionInfo);
            }

            Console.Clear();
            scene.OpenAllFields();
            strScene = scene.ToString();
            ColoredConsole.WriteLine(strScene);

            if (gameStatus == GameStatus.Lose)
            {
                Console.WriteLine("YOU LOOOOOOOOOOOUSE");
            }
            else
            {
                Console.WriteLine("YOU WON!");
            }

            Console.ReadLine();
        }
    }

    struct UserActionInfo
    {
        public UserActionType Action;
        public int X;
        public int Y;
    }

    class InputParser
    {
        Dictionary<string, UserActionType> userActionDict = new Dictionary<string, UserActionType>()
        {
            ["o"] = UserActionType.Open,
            ["f"] = UserActionType.Flag,
            ["q"] = UserActionType.Question,
        };
        
        private UserActionType ParseActionPart(string input)
        {
            return userActionDict[input];
        }
        
        public UserActionInfo ParseInput(string input)
        {
            string[] splitedInput = input.Split();
            if (input.Split().Length != 3)
            {
                throw new Exception();
            }

            UserActionType action = ParseActionPart(splitedInput[0]);

            int x = Int32.Parse(splitedInput[1]);
            int y = Int32.Parse(splitedInput[2]);

            return new UserActionInfo { Action = action, X = x, Y = y };
        }
    }

    class GameController
    {
        Scene scene;

        public GameStatus ExecuteUserAction(UserActionInfo input)
        {
            int x = input.X;
            int y = input.Y;
            UserActionType action = input.Action;

            if (x >= scene.Width || x < 0)
            {
                throw new Exception();
            }
            if (y >= scene.Height || y < 0)
            {
                throw new Exception();
            }

            if (action == UserActionType.Open)
            {
                bool isBomb = scene.Open(x, y);
                if (isBomb)
                {
                    return GameStatus.Lose;
                }
            }
            if (scene.AllEmptyFieldsOpen)
            {
                return GameStatus.Win;
            }
            else
            {
                return GameStatus.InGame;
            }
        }

        public GameController(Scene scene)
        {
            this.scene = scene;
        }

    }

    enum GameStatus
    {
        InGame,
        Lose,
        Win
    }

    enum UserActionType
    {
        Open,
        Flag,
        Question
    }

    static class ColoredConsole
    {
        static ConsoleColor defaultColor = ConsoleColor.White;

        static Dictionary<char, ConsoleColor> charColorDict = new Dictionary<char, ConsoleColor>()
        {
            ['b'] = ConsoleColor.DarkRed,
            ['0'] = ConsoleColor.Gray,
            ['1'] = ConsoleColor.Yellow,
            ['2'] = ConsoleColor.DarkYellow,
            ['3'] = ConsoleColor.Red,
            ['4'] = ConsoleColor.Red,
            ['5'] = ConsoleColor.Red,
            ['6'] = ConsoleColor.Red,
            ['7'] = ConsoleColor.Red,
            ['8'] = ConsoleColor.Red,
            ['9'] = ConsoleColor.Red,
        };

        static void WriteChar(char c)
        {
            if (charColorDict.ContainsKey(c))
            {
                Console.ForegroundColor = charColorDict[c];
                Console.Write(c);
                Console.ForegroundColor = defaultColor;
            }
            else
            {
                Console.Write(c);
            }
        }

        static public void WriteLine(string str)
        {
            foreach (var c in str)
            {
                WriteChar(c);
            }
        }
    }

    class Scene
    {
        private int width;

        public int Width => width;

        private int height;

        public int Height => height;

        private Field[,] fields;
        private int bombCount;

        public Field[,] Fields => fields;

        private int openFieldsCount = 0;

        public bool AllEmptyFieldsOpen => width * height - bombCount == openFieldsCount;

        private void GenerateEmptyFields()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    this.Fields[i, j] = new Field(false);
                    this.Fields[i, j].NumberNeighborBombs = 0;
                }
            }
        }

        private void GenerateFields(int bombCount)
        {
            var width = this.width;
            var height = this.height;

            var rand = new Random();

            this.GenerateEmptyFields();

            for (int i = 0; i < bombCount; i++)
            {
                int x = rand.Next(0, width);
                int y = rand.Next(0, height);
                this.AddBomb(x, y);
            }
        }

        private void IncrementFieldBombCount(int x, int y)
        {
            var isFieldInScene = x >= 0 && x < width && y >= 0 && y < height;
            if (isFieldInScene)
            {
                this.Fields[x, y].NumberNeighborBombs++;
            }
        }

        private bool AddBomb(int x, int y)
        {
            bool xyIsBomb = this.Fields[x, y].IsBomb;
            if (!xyIsBomb)
            {
                this.Fields[x, y] = new Field(true);

                //   +1 +1 +1
                //   +1  b +1
                //   +1 +1 +1

                this.IncrementFieldBombCount(x - 1, y - 1);
                this.IncrementFieldBombCount(x - 1, y);
                this.IncrementFieldBombCount(x - 1, y + 1);
                this.IncrementFieldBombCount(x, y - 1);
                //this.IncrementFieldBombCount(x, y);
                this.IncrementFieldBombCount(x, y + 1);
                this.IncrementFieldBombCount(x + 1, y - 1);
                this.IncrementFieldBombCount(x + 1, y);
                this.IncrementFieldBombCount(x + 1, y + 1);
            }

            return xyIsBomb;

        }

        public string ToString()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    char fieldChar;
                    if (this.Fields[j, i].IsOpen)
                    {
                        fieldChar = this.Fields[j, i].ToChar();
                    }
                    else
                    {
                        fieldChar = '#';
                    }
                    stringBuilder.Append(fieldChar);
                }
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }

        public bool Open(int x, int y)
        {
            if (this.fields[x, y].IsBomb)
            {
                return true;
            }
            if (this.fields[x, y].IsOpen)
            {
                return false;
            }
            this.fields[x, y].IsOpen = true;

            this.openFieldsCount++;

            if (this.fields[x, y].NumberNeighborBombs == 0)
            {
                if (this.IsCanOpened(x + 1, y))
                    Open(x + 1, y);
                if (this.IsCanOpened(x - 1, y))
                    Open(x - 1, y);
                if (this.IsCanOpened(x, y + 1))
                    Open(x, y + 1);
                if (this.IsCanOpened(x, y - 1))
                    Open(x, y - 1);
            }
            return false;
        }

        private bool IsCanOpened(int x, int y)
        {
            return 
                x >= 0 && x < this.width &&
                y >= 0 && y < this.height &&
                !this.fields[x, y].IsOpen;
        }

        internal void OpenAllFields()
        {
            foreach(var field in fields)
            {
                field.IsOpen = true;
            }
        }

        public Scene(int width, int height, int bombCount)
        {
            this.width = width;
            this.height = height;
            this.fields = new Field[width, height];
            this.bombCount = bombCount;
            this.GenerateFields(bombCount);
        }
    }

    class Field
    {
        private ClosedFieldState fieldState;

        private bool isBomb;

        public bool IsBomb => isBomb;

        private bool isOpen;

        public bool IsOpen { get => isOpen; set => isOpen = value; }

        public int NumberNeighborBombs;

        public ClosedFieldState ClosedFieldState => fieldState;

        public char ToChar()
        {
            if (this.isBomb == true)
            {
                return 'b';
            }
            else
            {
                var strNumBombs = this.NumberNeighborBombs.ToString();
                if (strNumBombs.Length == 1)
                {
                    return strNumBombs.First();
                }
                else
                {
                    throw new Exception();
                }
            }
        }


        public Field(bool isBomb)
        {
            this.isBomb = isBomb;
        }
    }

    enum FieldState
    {
        Close,
        Open,

    }

    enum ClosedFieldState
    {
        Empty,
        Flag,
        Question,
    }

}
