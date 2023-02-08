class PingTesterEventArgs : EventArgs {
    public Host Host { get; set; }
    public int CountMissed { get; set; }
}
