namespace OneSyncATD
{
    public delegate void UpdateHandler(object sender, UpdateEventArgs e);

    public class UpdateEventArgs : System.EventArgs
    {
        // Private fields
        private int _progress;
        private string _message;

        // Public Properties
        public int Progress { get {return _progress;} }
        public string Message { get { return _message; } }
        
        public UpdateEventArgs(string msg, int progress)
        {
            _message = msg;
            _progress = progress;
        }
    }
}
