namespace MinecraftLaunch.Classes.Models.Event {
    public class LogReceivedEventArgs(string log) : EventArgs {
        public string Text => log;
    }
}
