using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

class PingTester {
    private readonly Host host;
    private readonly Configs configs;

    public event EventHandler<PingTesterEventArgs>? OnPingLost;
    public PingTester(Host host, Configs configs) {
        this.host = host;
        this.configs = configs;
    }
    private bool prevOper = true;
    public PingResult Check() {
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
                prevOper = true;
                break;
            }
            else {
                if ( prevOper is true ) {
                    this.OnPingLost?.Invoke(this, new PingTesterEventArgs() {
                        Host = this.host,
                    });
                    prevOper = false;
                }

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