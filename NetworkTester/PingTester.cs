using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

class PingTester {
    public PingResult Check(Host host, Configs configs) {
        if ( configs.PingAttempsCount == 0 )
            throw new ArgumentOutOfRangeException(nameof(configs.PingAttempsCount));

        string message = configs.DefaultMessage;
        var buf = Encoding.ASCII.GetBytes(message);
        bool wasSuccess = false;
        List<TimeSpan> spans = new List<TimeSpan>();
        List<PingReply> replies = new List<PingReply>();
        Ping ping = new Ping();
        PingOptions options = new PingOptions() {
            DontFragment = true,
        };
        Stopwatch sw = new Stopwatch();
        for ( int i = 0; i < configs.PingAttempsCount; i++ ) {
            sw.Start();
            var reply = ping.Send(host.Ip, configs.PingTimeOut, buf, options);
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