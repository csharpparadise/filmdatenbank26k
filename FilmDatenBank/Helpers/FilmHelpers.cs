// FilmDatenBank/Helpers/FilmHelpers.cs
namespace FilmDatenBank.Helpers;

public static class FilmHelpers
{
    public static string MimeType(byte[] b) =>
        b.Length >= 4 && b[0] == 0x89 && b[1] == 0x50 ? "image/png"  :
        b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 ? "image/jpeg" :
        b.Length >= 3 && b[0] == 0x47 && b[1] == 0x49 ? "image/gif"  :
        "image/jpeg";

    public static string DiscTypeClass(string discType) => discType switch
    {
        "BD" => "bluray",
        "4K" => "4k",
        _    => "dvd"
    };

    public static string DiscTypeLabel(string discType) => discType switch
    {
        "BD" => "Blu-ray",
        "4K" => "4K UHD",
        _    => discType
    };
}
