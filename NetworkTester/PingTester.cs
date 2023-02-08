using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

class PingTester {
    public PingResult Check(Host host, PingOptions o, int timeout = 1000, int attemps = 4) {
        if ( attemps == 0 )
            throw new ArgumentOutOfRangeException(nameof(attemps));

        string message = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaq";
        var buf = Encoding.ASCII.GetBytes(message);
        bool wasSuccess = false;
        List<TimeSpan> spans = new List<TimeSpan>();
        List<PingReply> replies = new List<PingReply>();
        Ping ping = new Ping();
        Stopwatch sw = new Stopwatch();
        for ( int i = 0; i < attemps; i++ ) {
            sw.Start();
            var reply = ping.Send(host.Ip, timeout, buf, o);
            sw.Stop();
            spans.Add(sw.Elapsed);
            replies.Add(reply);
            if ( reply.Status == IPStatus.Success ) {
                wasSuccess = true;
                break;
            }
        }

        return new PingResult() {
            AllReplies = replies,
            AtLeastOneSuccess = wasSuccess,
            // replies.Count not zero
            AverageTime = TimeSpan.FromTicks(spans.Sum(s => s.Ticks) / replies.Count),
            CountSuccess = replies.Where(r => r.Status == IPStatus.Success).Count(),
            CountFailed = replies.Where(r => r.Status != IPStatus.Success).Count(),
        };
    }
}