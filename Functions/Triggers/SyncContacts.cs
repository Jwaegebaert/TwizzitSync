using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TwizzitSync.Config;
using TwizzitSync.Managers;
using TwizzitSync.Models.SharePoint;
using TwizzitSync.Services;
using TwizzitSync.Utils;

namespace TwizzitSync.Functions.Triggers;

public class SyncContacts(ILogger<SyncContacts> log)
{
    [Function("SyncContacts")]
    public async System.Threading.Tasks.Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var twizzitService = await TwizzitService.CreateAsync();
        var contacts = await twizzitService.GetContactsWithMembershipAsync();

        log.LogInformation($"Contacts: {contacts.Length}");

        using var ctx = await AuthenticationUtil.GetClientContextAsync(EnvironmentConfig.SharePointDataSite);
        var contactManager = new SPContactManager(ctx);
        var spContacts = contactManager.GetContactsAsync();

        log.LogInformation($"SP Contacts: {spContacts.Length}");

        var spRemovalIds = new List<int>();
        foreach (var spContact in spContacts)
        {
            var twizzitContact = contacts.FirstOrDefault(c => c.Id == spContact.TwizzitId);

            if (twizzitContact is null)
            {   
                spRemovalIds.Add(spContact.Id);
            }
        }

        log.LogInformation($"SP Removals: {spRemovalIds.Count}");

        if (spRemovalIds.Count != 0)
        {
            contactManager.DeleteContactsAsync([.. spRemovalIds]);
        }

        var newSPContacts = new List<SPContact>();
        var updateSPContacts = new List<SPContact>();
        foreach (var contact in contacts)
        {
            var spContact = spContacts.FirstOrDefault(c => c.TwizzitId == contact.Id);
            var target = contact.Membership.MembershipTypeId switch
            {
                MembershipTypeConfig.youth => Models.Enum.Target.Youth,
                MembershipTypeConfig.recreational or MembershipTypeConfig.competitive => Models.Enum.Target.Adult,
                _ => Models.Enum.Target.None
            };

            if (spContact is null)
            {
                newSPContacts.Add(new SPContact
                {
                    TwizzitId = contact.Id,
                    Address = contact.Email1.EmailAddress,
                    Target = target
                });
            }
            else
            {
                if (spContact.Address != contact.Email1.EmailAddress || spContact.Target != target)
                {
                    spContact.Address = contact.Email1.EmailAddress;
                    spContact.Target = target;
                    updateSPContacts.Add(spContact);
                }
            }
        }

        log.LogInformation($"New SP Contacts: {newSPContacts.Count}");

        if (newSPContacts.Count != 0)
        {
            contactManager.CreateContactsAsync([.. newSPContacts]);
        }

        log.LogInformation($"Update SP Contacts: {updateSPContacts.Count}");

        if (updateSPContacts.Count != 0)
        {
            contactManager.UpdateContactsAsync([.. updateSPContacts]);
        }

        log.LogInformation("Sync completed.");
    }
}
