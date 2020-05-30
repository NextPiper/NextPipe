namespace NextPipe.Core.Queries.Models
{
    public class RabbitMQConfig
    {
        public string Hostname { get; }
        public string Username { get; }
        public string Password { get; }
        public int Port { get; }

        public RabbitMQConfig(string hostname, string username, string password, int port)
        {
            Hostname = hostname;
            Username = username;
            Password = password;
            Port = port;
        }
    }
}