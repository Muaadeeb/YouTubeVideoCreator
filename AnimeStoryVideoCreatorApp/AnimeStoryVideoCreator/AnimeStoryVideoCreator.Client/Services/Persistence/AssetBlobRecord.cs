namespace AnimeStoryVideoCreator.Client.Services.Persistence;

public class AssetBlobRecord
{
    public string Id { get; set; } = "";
    public string MimeType { get; set; } = "application/octet-stream";
    public string DataBase64 { get; set; } = "";
}

public class LocalStorageWriteResult
{
    public bool Ok { get; set; }
    public string? Name { get; set; }
    public string? Message { get; set; }
}
