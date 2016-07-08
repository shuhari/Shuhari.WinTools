namespace Shuhari.WinTools.Core.Features.ImageFinder.Args
{
    public class ChangeStateArgs : NotifyArgs
    {
        private readonly State _state;

        public ChangeStateArgs(State state)
        {
            _state = state;
        }

        public override void Apply(IImageFinderUI win)
        {
            win.EnterState(_state);
        }
    }

}
