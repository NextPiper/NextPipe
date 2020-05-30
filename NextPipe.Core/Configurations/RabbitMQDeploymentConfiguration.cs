namespace NextPipe.Core.Configurations
{
    public class RabbitMQDeploymentConfiguration
    {
        public bool IsRabbitServiceEnabled { get; set; }
        public string RabbitServiceUsername { get; set; }
        public string RabbitServicePassword { get; set; }
    }
}