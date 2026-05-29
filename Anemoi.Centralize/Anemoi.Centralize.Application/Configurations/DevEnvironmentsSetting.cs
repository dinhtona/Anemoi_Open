namespace Anemoi.Centralize.Application.Configurations;

public sealed class DevEnvironmentsSetting
{
    public string LocalEnvDir { get; set; } = "local_env";
    public string DockerSocketPath { get; set; } = "/var/run/docker.sock";
    
    // Mail
    public string MailContainerName { get; set; } = "smtp4dev_server";
    public string MailDisplayName { get; set; } = "Mail Test";
    public int MailSmtpPort { get; set; } = 25;
    public string MailWebUiUrl { get; set; } = "http://localhost:5555";
    public string MailDescription { get; set; } = "SMTP server for receiving and debugging emails locally";

    // SFTP
    public string SftpContainerName { get; set; } = "sftp_server";
    public string SftpDisplayName { get; set; } = "SFTP for Dev";
    public int SftpPort { get; set; } = 2222;
    public string SftpUsername { get; set; } = "user1";
    public string SftpPassword { get; set; } = "123456";
    public string SftpDescription { get; set; } = "SFTP server for transferring and sharing files locally";

    // Mock API
    public string MockApiContainerName { get; set; } = "api-test";
    public string MockApiDisplayName { get; set; } = "API Mock Server";
    public int MockApiPort { get; set; } = 8080;
    public string MockApiBasePath { get; set; } = "/api/mock";
    public string MockApiDescription { get; set; } = "Dynamic REST API endpoint with custom mock response data";
}
