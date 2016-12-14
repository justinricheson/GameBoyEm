namespace GameBoyEm.Api
{
    public interface IController
    {
        bool APressed { get; set; }
        bool BPressed { get; set; }
        bool StartPressed { get; set; }
        bool SelectPressed { get; set; }
        bool UpPressed { get; set; }
        bool DownPressed { get; set; }
        bool LeftPressed { get; set; }
        bool RightPressed { get; set; }
        bool FastPressed { get; set; }
        void Step();
    }
}
