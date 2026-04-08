using TwizzitSync.Models.Enum;

namespace TwizzitSync.Models.SharePoint;

internal class SPContact
{
    public int Id { get; set; }
    public string TwizzitId { get; set; }
    public string Address { get; set; }
    public Target Target { get; set; }
}
