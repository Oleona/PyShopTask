using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Task1.Tests")]
namespace Task1
{

    class App
    {
        static void Main(string[] args)
        {
            Game.task1();
        }
    }
    public struct Score
    {
        public int home;
        public int away;

        public Score(int home, int away)
        {
            this.home = home;
            this.away = away;
        }
    }

    public struct GameStamp
    {
        public int offset;
        public Score score;
        public GameStamp(int offset, int home, int away)
        {
            this.offset = offset;
            this.score = new Score(home, away);
        }
    }

    public class Game
    {
        public const int TIMESTAMPS_COUNT = 50000;

        const double PROBABILITY_SCORE_CHANGED = 0.0001;

        const double PROBABILITY_HOME_SCORE = 0.45;

        const int OFFSET_MAX_STEP = 3;

        GameStamp[] gameStamps;

        public Game()
        {
            this.gameStamps = new GameStamp[] { };
        }

        public Game(GameStamp[] gameStamps)
        {
            this.gameStamps = gameStamps;
        }

        GameStamp generateGameStamp(GameStamp previousValue)
        {
            Random rand = new Random();

            bool scoreChanged = rand.NextDouble() > 1 - PROBABILITY_SCORE_CHANGED;
            int homeScoreChange = scoreChanged && rand.NextDouble() > 1 - PROBABILITY_HOME_SCORE ? 1 : 0;
            int awayScoreChange = scoreChanged && homeScoreChange == 0 ? 1 : 0;
            int offsetChange = (int)(Math.Floor(rand.NextDouble() * OFFSET_MAX_STEP)) + 1;

            return new GameStamp(
                previousValue.offset + offsetChange,
                previousValue.score.home + homeScoreChange,
                previousValue.score.away + awayScoreChange
                );
        }

        static Game generateGame()
        {
            Game game = new Game();
            game.gameStamps = new GameStamp[TIMESTAMPS_COUNT];

            GameStamp currentStamp = new GameStamp(0, 0, 0);
            for (int i = 0; i < TIMESTAMPS_COUNT; i++)
            {
                game.gameStamps[i] = currentStamp;
                currentStamp = game.generateGameStamp(currentStamp);
            }

            return game;
        }

        public static void task1()
        {
            Game game = generateGame();
            game.printGameStamps();

        }

        void printGameStamps()
        {
            foreach (GameStamp stamp in this.gameStamps)
            {
                Console.WriteLine($"{stamp.offset}: {stamp.score.home}-{stamp.score.away}");
            }
        }

        public Score getScore(int offset)
        {
            if (offset < 0 || offset > gameStamps.Last().offset)
            {
                throw new ArgumentOutOfRangeException($"Offset {offset} is negative or greater than the maximum offset in the stamps array");
            }

            var distinctTimeStamps = gameStamps.Select(x => x.offset).Distinct().Count();
            if (distinctTimeStamps != TIMESTAMPS_COUNT)
            {
                throw new NotSupportedException("All offsets in stamps array must be different");
            }


            var nearestLeastOffset = gameStamps.Where(x => x.offset <= offset).Max(x => x.offset);
            if (nearestLeastOffset == offset)
            {
                return gameStamps.Single(x => x.offset == offset).score;
            }

            var nearestLeastOffsetScore = gameStamps.Single(x => x.offset == nearestLeastOffset).score;

            var nearestMostOffset = gameStamps.Where(x => x.offset >= offset).First().offset;
            var nearestMostOffsetScore = gameStamps.Single(x => x.offset == nearestMostOffset).score;

            if (nearestLeastOffsetScore.away == nearestMostOffsetScore.away
                && nearestLeastOffsetScore.home == nearestMostOffsetScore.home)
            {
                return nearestLeastOffsetScore;
            }

            Console.WriteLine(
                $"Cannot get score: offset {offset} does not exist in the stamp array " +
                $"and the score changed between nearest ones");
            return new Score(-1, -1);
        }
    }
}
