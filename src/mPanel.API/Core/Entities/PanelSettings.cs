using System.ComponentModel.DataAnnotations.Schema;

namespace mPanel.API.Core.Entities;

public class PanelSettings
{
    public int Id { get; private set; } = 1;

    public string Name { get; set; } = "mPanel";
    public string? Url { get; set; }

    public bool AllowRegistration { get; set; } = true;
    public bool AllowAccountSelfDeletion { get; set; } = true;

    public Smtp Smtp { get; set; } = new();
}

[ComplexType]
public class Smtp
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }
}