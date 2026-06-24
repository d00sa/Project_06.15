namespace Space
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
        StartGame,
        WaitUntilStartGame,
        LoadData,
        GameJudge,
        WaitStage, //스테이지 대기
        StartStage, //스테이지 진행
        GameClear, //게임 클리어
        GameLose, //게임패배
        WaitForUser //유저 대기
    }

    public enum Commands
    {
        Idle,Prepare,Casting,OnAction,Finish
    }

    //동작 종류들
    public enum StateType
    {
        Idle, Move, Dead, EOF
    }

    public enum Sounds
    {
        Master, BGM, SFX
    }

    public class Constant
    {
        public const int REMAIN_SKIP_TIME = 5;
    }
}