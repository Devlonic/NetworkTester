using System.Text;
using System.Text.Json;

class Configs {
    public int RefreshDelay { get; set; } = 1000;
    public int WindowWidth { get; set; } = 200;
    public int PingTimeOut { get; set; } = 2000;
    public int PingAttempsCount { get; set; } = 4;
    public string DefaultMessage { get; set; } = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaq";

    public static async Task<Configs> GetInstanceAsync(string file) {
        Configs? configs;
        if ( File.Exists(file) is false ) {
            configs = new Configs();
            var arr = JsonSerializer.SerializeToUtf8Bytes(configs);
            var str = Encoding.UTF8.GetString(arr);
            await File.WriteAllTextAsync(file, str);
        }
        else {
            var str = await File.ReadAllTextAsync(file);
            configs = JsonSerializer.Deserialize<Configs>(str);
            if ( configs is null )
                throw new JsonException(nameof(file) + " has invalid json structure");
        }
        return configs;
    }
}