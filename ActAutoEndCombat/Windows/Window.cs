using Dalamud.Plugin;

namespace ActAutoEndCombat.Windows
{
    public abstract class Window<T> where T : IDalamudPlugin
    {

        protected T Plugin { get; private set; }

        protected bool _WindowVisible;
        public bool Visible
        {
            get => _WindowVisible;
            set
            {
                if (value == _WindowVisible) return;
                
                _WindowVisible = value;
                if (_WindowVisible)
                {
                    OnOpen();
                }
                else
                {
                    OnClose();
                }

            }
        }

        protected Window(T plugin)
        {
            Plugin = plugin;
        }

        public void Draw()
        {
            if (Visible)
                DrawUi();
        }

        protected abstract void DrawUi();

        protected virtual void OnOpen() { }

        protected virtual void OnClose() { }

        public void Toggle()
        {
            Visible = !Visible;
        }

    }

}
