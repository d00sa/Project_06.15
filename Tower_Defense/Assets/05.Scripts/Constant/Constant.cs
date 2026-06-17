namespace ConstantSpace
{
    public enum Degree
    {
        Easy,
        Normal,
        Hard
    }

    public enum GameState
    {
        Idle,
        LoadDifficultData,
        WaitUntilDifficultDataLoaded,
        StartGame,
        LoadData,
        GameSetting,
        WaitStage, //스테이지 대기
        StartStage, //스테이지 진행
        GameClear, //게임 클리어
        GameLose, //게임패배
        WaitForUser
    }
    
    public class Constant
    {
        public const int REMAIN_SKIP_TIME = 5;
    }
}