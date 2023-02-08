using System.Net.NetworkInformation;
using System.Text;

Console.WriteLine("Network Tester v1.0");

//var file = args[0];
var file = "hosts.txt";
if ( string.IsNullOrEmpty(file) || File.Exists(file) is false ) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"No {file} file present. Halt.");
    Console.ResetColor();
    Console.ReadKey();
    return;
}

List<Host> hosts = new();
var hostsRaw = await File.ReadAllLinesAsync(file);
foreach ( var raw in hostsRaw ) {
    hosts.Add(Host.Parse(raw));
}
Console.WriteLine($"Readed {hosts.Count} hosts from file");

PingOptions o = new PingOptions();
o.DontFragment = true;

Console.WriteLine("Begin ping");
Console.OutputEncoding = Encoding.Unicode;
Console.WindowWidth = 200;
Console.BufferWidth = 200;
var tasks = new List<Task>();
while ( true ) {
    Console.Clear();
    Console.CursorVisible = false;
    tasks.Clear();
    foreach ( var host in hosts ) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        tasks.Add(Task.Run(() => {
            PingTester tester = new PingTester();
            lock ( tasks ) {
                Console.SetCursorPosition(0, host.Position);
                Console.Write($"{host.Ip}\t{host.Name}");
            }
            var res = tester.Check(host, o);
            lock ( tasks ) {
                Console.SetCursorPosition(60, host.Position);
                if ( res.AtLeastOneSuccess is true ) {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($"ONLINE ");
                    Console.ResetColor();

                    Console.Write($"\tSUCCES:{res.CountSuccess}\tFAILS:{res.CountFailed}\tAVG: {res.AverageTime}");

                }
                else {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write($"OFFLINE");
                    Console.ResetColor();
                    Console.Write($"\tFAILS:{res.CountFailed} ({string.Join(' ', res.AllReplies.Select(r => r.Status.ToString()))})");
                }
            }
        }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
    // wait for each ping finish
    tasks.ForEach(t => t.Wait());
    Console.SetCursorPosition(0, tasks.Count + 3);
    Console.WriteLine("Press any key to restart");
    Console.ReadKey();
}

