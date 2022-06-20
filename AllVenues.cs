using FFXIVVenues.VenueTests.Discord;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VenueModels.V2021;

namespace FFXIVVenues.VenueTests
{
    public class AllVenues
    {

        private static Regex _discordPattern = new Regex(@"(https?:\/\/)?(www\.)?((discord(app)?(\.com|\.io)(\/invite)?)|(discord\.gg))\/(\w+)");
        private static Venue[] _venues;
        private HttpClient _client;

        static AllVenues()
        {
            var request = new HttpClient().GetAsync("https://raw.githubusercontent.com/FFXIVVenues/ffxiv-venues-web/master/src/venues.json").Result;
            _venues = JsonSerializer.Deserialize<Venue[]>(request.Content.ReadAsStream());
        }

        public static IEnumerable GetData() =>
            _venues.Where(v => v != null).Select(v => new TestCaseData(v).SetArgDisplayNames(v.name));

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this._client = new HttpClient();
        }

        [Test]
        [TestCaseSource("GetData")]
        public async Task HaveValidWebsites(Venue venue)
        {
            if (venue.website == null)
                Assert.Ignore($"No website link for {venue.name}.");
            var response = await this._client.GetAsync(venue.website);
            Assert.IsTrue(response.IsSuccessStatusCode, $"The website link for {venue.name} is giving a {response.StatusCode} ({venue.website}).");
        }

        [Test]
        [TestCaseSource("GetData")]
        public async Task HaveValidDiscord(Venue venue)
        {
            if (venue.discord == null)
                Assert.Ignore($"No discord link for {venue.name}.");

            await Task.Delay(5000);

            var match = _discordPattern.Match(venue.discord);
            if (!match.Success)
                Assert.Ignore($"The discord link for {venue.name} is not a standard link, skipped.");

            var inviteCode = match.Groups[9].ToString();
            var responseMessage = await this._client.GetAsync($"https://discordapp.com/api/invite/{inviteCode}");

            Assert.IsTrue(responseMessage.StatusCode != HttpStatusCode.NotFound, $"The discord link for {venue.name} is invalid ({venue.discord}).");
            if (!responseMessage.IsSuccessStatusCode)
            {
                Assert.Warn($"Could not query validity of discord link for {venue.name}.");
                return;
            }

            var response = await responseMessage.Content.ReadAsStreamAsync();
            var invite = await JsonSerializer.DeserializeAsync<Invite>(response);

            Assert.IsTrue(invite.Expires_at == null, $"The discord link for {venue.name} expires at {invite.Expires_at}.");
        }

    }
}