namespace SocialCookies.Bwasm.Modelos
{
    /// <summary>
    /// Usa mismas propiedades do modelo Usuario da parte API do proxecto
    /// This class uses the same properties as the User model from the API project
    /// </summary>
    public class UsuarioPerfilModel
    {
        public int UsuarioId { get; set; }
        public string Email { get; set; }
        public string Apelidos { get; set; }
        public string Nome { get; set; }

    }
}
