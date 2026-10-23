namespace Anemoi.Centralize.Application.Configurations;

public sealed class DevEnvironmentsSetting
{
    public string LocalEnvDir { get; set; } = "local_env";
    public string DockerSocketPath { get; set; } = "/var/run/docker.sock";
    
    // Mail
    public string MailContainerName { get; set; } = "smtp4dev_server";
    public string MailDisplayName { get; set; } = "Mail Test";
    public int MailSmtpPort { get; set; } = 25;
    public string MailWebUiUrl { get; set; } = "/mail/";
    public string MailApiUrl { get; set; } = "http://localhost:5555/mail/api/messages";
    public string MailDescription { get; set; } = "SMTP server for receiving and debugging emails locally";

    // Mock API
    public string MockApiContainerName { get; set; } = "api-test";
    public string MockApiDisplayName { get; set; } = "API Mock Server";
    public int MockApiPort { get; set; } = 8080;
    public string MockApiBasePath { get; set; } = "/api/mock";
    public string MockApiDescription { get; set; } = "Dynamic REST API endpoint with custom mock response data";
}
