using System.Net;
using System.Text.Json.Serialization;

struct Host {
    private static int _position = 0;
    public int Position { get; set; }
    public string Name { get; set; }
    public IPAddress Ip { get; set; }
    public static Host Parse(string? row) {
        var temp = row?.Split('\t');
        if ( temp?.Length != 2 )
            throw new FormatException(nameof(row));

        Host host = new Host();
        host.Ip = IPAddress.Parse(temp[0]);
        host.Name = temp[1];
        host.Position = Interlocked.Increment(ref _position);
        return host;
    }
}
