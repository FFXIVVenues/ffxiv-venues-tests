using System;

namespace FFXIVVenues.VenueTests.Discord
{
    public class Invite
    {
        public string Code { get; set; }
        public DateTime? Expires_at { get; set; }
        public Guild Guild { get; set; }
        public Channel Channel { get; set; }
        public User Inviter { get; set; }
        public int Target_type { get; set; }
        public User Target_user { get; set; }
    }
}
