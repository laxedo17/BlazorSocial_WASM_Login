namespace RedesCookies_API.Db.Entity;

public class Usuario
{
    //Entity model to use with EF Core, code first approach
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Nome { get; set; }
    public string? Apelidos { get; set; }
    public string? Password { get; set; } //password

    //Representa nome login de rede social externa (twitter, microsoft, etc)
    //Represents the external login name of a social network -twitter, ms, etc-
    public string? ExternalLoginNome { get; set; }

}