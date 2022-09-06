using DataAccess.DataModels;

namespace DataAccess.Interfaces
{
    public interface ITwilioCredentialsRepository
    {
        TwilioCredentials GetTwilioCredentials(int? OrganizationId);
        void AddTwilioCredentials(TwilioCredentials objTwilioCredentials);
        TwilioCredentials GetTwilioCredentialsById(int twilioaccountId);
        TwilioCredentials CheckIfAccountSidExists(string accountSid);
        void DeleteTwilioRecord(int id);
    }
}