
using System.Linq;
using Microsoft.SharePoint.Client;
using TwizzitSync.Config;
using TwizzitSync.Extensions;
using TwizzitSync.Models.SharePoint;

namespace TwizzitSync.Managers;

internal class SPContactManager(ClientContext ctx)
{
    private readonly ClientContext _ctx = ctx;

    public SPContact[] GetContactsAsync()
    {
        var list = _ctx.Web.Lists.GetByTitle(SharePointConfig.Lists.Contact);
        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());
        _ctx.Load(items);
        _ctx.ExecuteQuery();

        return items.Select(ListItemToContact).ToArray();
    }

    public void CreateContactsAsync(SPContact[] contacts)
    {
        var list = _ctx.Web.Lists.GetByTitle(SharePointConfig.Lists.Contact);
        foreach (var contact in contacts)
        {
            var item = list.AddItem(new ListItemCreationInformation());
            item[FieldConfig.Contact.TwizzitId] = contact.TwizzitId;
            item[FieldConfig.Contact.Address] = contact.Address;
            item[FieldConfig.Contact.Target] = contact.Target.ToString();
            item.Update();
        }

        _ctx.ExecuteQuery();
    }

    public void UpdateContactsAsync(SPContact[] contacts)
    {
        var list = _ctx.Web.Lists.GetByTitle(SharePointConfig.Lists.Contact);
        foreach (var contact in contacts)
        {
            var item = list.GetItemById(contact.Id);
            item[FieldConfig.Contact.TwizzitId] = contact.TwizzitId;
            item[FieldConfig.Contact.Address] = contact.Address;
            item[FieldConfig.Contact.Target] = contact.Target.ToString();
            item.Update();
        }

        _ctx.ExecuteQuery();
    }

    public void DeleteContactsAsync(int[] ids)
    {
        var list = _ctx.Web.Lists.GetByTitle(SharePointConfig.Lists.Contact);
        foreach (var id in ids)
        {
            var item = list.GetItemById(id);
            item.DeleteObject();
        }

        _ctx.ExecuteQuery();
    }

    private static SPContact ListItemToContact(ListItem item)
    {
        return new()
        {
            Id = item.Id,
            TwizzitId = item.FieldValues[FieldConfig.Contact.TwizzitId]?.ToString(),
            Address = (string)item.FieldValues[FieldConfig.Contact.Address],
            Target = EnumExtensions.GetEnumValue<Models.Enum.Target>((string)item.FieldValues[FieldConfig.Contact.Target]),
        };
    }
}