using System.Net.NetworkInformation;

struct PingResult {
    public List<PingReply> AllReplies { get; set; }
    public bool AtLeastOneSuccess { get; set; }
    public TimeSpan AverageTime { get; set; }
    public int CountSuccess { get; set; }
    public int CountFailed { get; set; }
}
