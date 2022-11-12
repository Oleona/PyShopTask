using System;
using Xunit;
using Task1;

namespace Task1.Tests
{
    public class GetScoreTests
    {
        private readonly Game testGame;


        public GetScoreTests()
        {
            testGame = generateTestGame(needDuplicates: false);
        }

        [Fact]
        public void TestGetScoreWhenOffsetExists()
        {
            // arrange
            Score expectedScore = new(1, 0);
            // act
            Score receivedScore = testGame.getScore(1);
            // assert
            Assert.Equal(expectedScore, receivedScore);
        }

        [Fact]
        public void TestGetScoreByNearestOffset()
        {
            // arrange
            Score expectedScore = new(1, 0);
            // act
            Score receivedScore = testGame.getScore(3);
            // assert
            Assert.Equal(expectedScore, receivedScore);
        }


        [Fact]
        public void TestWhenNearestOffsetAndScoreChanged()
        {
            // arrange
            Score expectedScore = new(-1, -1);
            // act
            Score receivedScore = testGame.getScore(4);
            // assert
            Assert.Equal(expectedScore, receivedScore);
        }

        [Fact]
        public void TestOnNegativeOffset()
        {
            // act&assert
            Assert.Throws<ArgumentOutOfRangeException>(() => testGame.getScore(-1));
        }

        [Fact]
        public void TestOnOffsetBiggerThanInStampsArray()
        {
            // act&assert
            Assert.Throws<ArgumentOutOfRangeException>(() => testGame.getScore(100000));
        }

        [Fact]
        public void TestOnStampsArrayWithDuplicates()
        {
            Game newTestGame = generateTestGame(needDuplicates: true);
            // act&assert
            Assert.Throws<NotSupportedException>(() => newTestGame.getScore(5));
        }


        private Game generateTestGame(bool needDuplicates)
        {
            GameStamp[] gameStamp = new GameStamp[Game.TIMESTAMPS_COUNT];

            gameStamp[0] = needDuplicates ? new(1, 0, 0) : new(0, 0, 0);
            gameStamp[1] = new(1, 1, 0);
            gameStamp[2] = new(3, 1, 0);
            gameStamp[3] = new(5, 1, 1);

            for (int i = 4; i < gameStamp.Length; i++)
            {
                gameStamp[i].offset = i + 2;
                gameStamp[i].score = new(2, 2);
            }

            return new Game(gameStamp);
        }
    }
}
