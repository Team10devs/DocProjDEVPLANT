namespace DocProjDEVPLANT.Services.Mail;

public class EmailConfig
{
    public string From { get; set; }
    
    public string To { get; set; }
    public string EmailHost { get; set; }
    
    public int Port { get; set; }
    
    public string EmailUserName { get; set; }
    
    public string EmailPassword { get; set; }
}