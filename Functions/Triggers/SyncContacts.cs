using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TwizzitSync.Config;
using TwizzitSync.Managers;
using TwizzitSync.Models.SharePoint;
using TwizzitSync.Models.Twizzit;
using TwizzitSync.Services;
using TwizzitSync.Utils;

namespace TwizzitSync.Functions.Triggers;

public class SyncContacts(ILogger<SyncContacts> log)
{
    private static readonly HashSet<int> KnownMembershipTypes =
    [
        MembershipTypeConfig.youth,
        MembershipTypeConfig.recreational,
        MembershipTypeConfig.competitive
    ];

    [Function("SyncContacts")]
    public async System.Threading.Tasks.Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var twizzitService = await TwizzitService.CreateAsync();
        var contacts = await twizzitService.GetContactsWithMembershipAsync();

        log.LogInformation($"Contacts: {contacts.Length}");

        // Phase 1: Filter to contacts with active memberships only
        var activeContacts = contacts.Where(c => HasActiveMembership(c.Membership)).ToArray();

        log.LogInformation($"Active contacts: {activeContacts.Length}");

        // Phase 2: Expand all emails per contact and determine target
        var emailEntries = new List<(int TwizzitId, string Email, Models.Enum.Target Target)>();
        foreach (var contact in activeContacts)
        {
            var target = contact.Membership.MembershipTypeId switch
            {
                MembershipTypeConfig.youth => Models.Enum.Target.Youth,
                MembershipTypeConfig.recreational or MembershipTypeConfig.competitive => Models.Enum.Target.Adult,
                _ => Models.Enum.Target.None
            };

            foreach (var email in GetEmailAddresses(contact))
            {
                emailEntries.Add((contact.Id, email, target));
            }
        }

        // Phase 3: Group by (Email, Target) to deduplicate, merge TwizzitIds
        var desiredContacts = emailEntries
            .GroupBy(e => (e.Email.ToLowerInvariant(), e.Target))
            .Select(g => new SPContact
            {
                TwizzitId = string.Join(",", g.Select(e => e.TwizzitId).Distinct().OrderBy(id => id)),
                Address = g.First().Email,
                Target = g.Key.Target
            })
            .ToList();

        log.LogInformation($"Desired SP entries: {desiredContacts.Count}");

        // Phase 4: Compare with existing SharePoint contacts
        using var ctx = await AuthenticationUtil.GetClientContextAsync(EnvironmentConfig.SharePointDataSite);
        var contactManager = new SPContactManager(ctx);
        var spContacts = contactManager.GetContactsAsync();

        log.LogInformation($"SP Contacts: {spContacts.Length}");

        // Delete: SP contacts that no longer match any desired (Address, Target)
        var spRemovalIds = spContacts
            .Where(sp => !desiredContacts.Any(d =>
                string.Equals(d.Address, sp.Address, StringComparison.OrdinalIgnoreCase) && d.Target == sp.Target))
            .Select(sp => sp.Id)
            .ToList();

        log.LogInformation($"SP Removals: {spRemovalIds.Count}");

        if (spRemovalIds.Count != 0)
        {
            contactManager.DeleteContactsAsync([.. spRemovalIds]);
        }

        // Create & Update
        var newSPContacts = new List<SPContact>();
        var updateSPContacts = new List<SPContact>();
        foreach (var desired in desiredContacts)
        {
            var spContact = spContacts.FirstOrDefault(sp =>
                string.Equals(sp.Address, desired.Address, StringComparison.OrdinalIgnoreCase) && sp.Target == desired.Target);

            if (spContact is null)
            {
                newSPContacts.Add(desired);
            }
            else if (spContact.TwizzitId != desired.TwizzitId)
            {
                spContact.TwizzitId = desired.TwizzitId;
                updateSPContacts.Add(spContact);
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

    private static bool HasActiveMembership(Membership membership)
    {
        if (membership is null)
            return false;

        if (!KnownMembershipTypes.Contains(membership.MembershipTypeId))
            return false;

        if (string.IsNullOrWhiteSpace(membership.EndDate))
            return true;

        return DateTime.TryParse(membership.EndDate, out var endDate) && endDate >= DateTime.Today;
    }

    private static IEnumerable<string> GetEmailAddresses(Contact contact)
    {
        if (!string.IsNullOrWhiteSpace(contact.Email1?.EmailAddress))
            yield return contact.Email1.EmailAddress;

        if (!string.IsNullOrWhiteSpace(contact.Email2?.EmailAddress))
            yield return contact.Email2.EmailAddress;

        if (!string.IsNullOrWhiteSpace(contact.Email3?.EmailAddress))
            yield return contact.Email3.EmailAddress;
    }
}
