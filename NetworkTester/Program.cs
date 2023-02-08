using System.Net.NetworkInformation;
using System.Text;

Console.WriteLine("Network Tester v1.0");

//var file = args[0];
var hostsFile = "hosts.txt";
var configsFile = "config.json";
List<Host> hosts = new();
if ( string.IsNullOrEmpty(hostsFile) || File.Exists(hostsFile) is false ) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"No {hostsFile} file present. Halt.");
    Console.ResetColor();
    Console.ReadKey();
    return;
}
else {
    var hostsRaw = await File.ReadAllLinesAsync(hostsFile);
    foreach ( var raw in hostsRaw ) {
        hosts.Add(Host.Parse(raw));
    }
    if ( hosts.Count < 1 ) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"No hosts from {hostsFile} present. Halt.");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }
    Console.WriteLine($"Readed {hosts.Count} hosts from file");
}

var configs = await Configs.GetInstanceAsync(configsFile);

PingOptions o = new PingOptions();
o.DontFragment = true;

Console.WriteLine("Begin ping");
Console.OutputEncoding = Encoding.Unicode;
Console.WindowWidth = configs.WindowWidth;
Console.BufferWidth = configs.WindowWidth;
var tasks = new List<Task>();
Console.Clear();
Console.SetCursorPosition(0, 0);
Console.CursorVisible = false;
tasks.Clear();
Console.WriteLine($"IP\t\tName\t\t\t\t\t    Status\tTotal success\tTotal fails\tRefresh\tAverage time");
foreach ( var host in hosts ) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    tasks.Add(Task.Run(async () => {
        int count = 0;
        int countSuccess = 0;
        int countFails = 0;

        lock ( tasks ) {
            Console.SetCursorPosition(0, host.Position);
            Console.Write($"{host.Ip}\t{host.Name}");
        }
        PingTester tester = new PingTester(host, configs);
        tester.OnPingLost += (sender, e) => {
            lock ( tasks ) {
                Console.SetCursorPosition(60, e.Host.Position);
                for ( int i = 0; i < 50; i++ ) {
                    Console.Write(' ');
                }
                Console.SetCursorPosition(60, host.Position);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.Write($"MISSED");
                Console.ResetColor();
            }

        };
        while ( true ) {
            var res = tester.Check();
            lock ( tasks ) {
                Console.SetCursorPosition(60, host.Position);
                for ( int i = 0; i < 50; i++ ) {
                    Console.Write(' ');
                }
                Console.SetCursorPosition(60, host.Position);

                if ( res.AtLeastOneSuccess is true ) {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($"ONLINE ");
                    Console.ResetColor();

                    Console.Write($"\tSUCCES:{countSuccess += res.CountSuccess}\tFAILED:{countFails += res.CountFailed}\t{++count}\tAVG: {res.AverageTime}             ");
                }
                else {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write($"OFFLINE");
                    countFails += res.CountFailed;
                    Console.Write($"\t{++count} {string.Join(' ', res.AllReplies.Select(r => r.Status.ToString()))}");
                    Console.ResetColor();

                }
            }
            await Task.Delay(configs.RefreshDelay);
        }
    }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
}
// wait for each ping finish
tasks.ForEach(t => t.Wait());
